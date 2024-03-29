﻿//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Access to SteamVR system (hmd) and compositor (distort) interfaces.
//
//=============================================================================

#define BACKGROUND_STEAMVR

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Numerics;
using Valve.VR;
using System;

public class SteamVR : System.IDisposable
{
	// Use this to check if SteamVR is currently active without attempting
	// to activate it in the process.
	public static bool active { get { return _instance != null; } }

	// Set this to false to keep from auto-initializing when calling SteamVR.instance.
	private static bool _enabled = true;
	public static bool enabled
	{
		get { return _enabled; }
		set
		{
			_enabled = value;
			if (!_enabled)
				SafeDispose();
		}
	}

	private static SteamVR _instance;
	public static SteamVR instance
	{
		get
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return null;
#endif
			if (!enabled)
				return null;

			if (_instance == null)
			{
#if BACKGROUND_STEAMVR
                _instance = CreateInstanceBackground();
#else
                _instance = CreateInstanceBackground();

                // If init failed, then auto-disable so scripts don't continue trying to re-initialize things.
                if (_instance == null)
					_enabled = false;
#endif
			}

			return _instance;
		}
	}

	public static bool usingNativeSupport
	{
		get
		{
			return false;
		}
	}

	static SteamVR CreateInstance()
	{
		try
		{
			var error = EVRInitError.None;
			if (!SteamVR.usingNativeSupport)
			{
				OpenVR.Init(ref error);
				if (error != EVRInitError.None)
				{
					ReportError(error);
					ShutdownSystems();
					return null;
				}
			}

			// Verify common interfaces are valid.

			OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
			if (error != EVRInitError.None)
			{
				ReportError(error);
				ShutdownSystems();
				return null;
			}

			OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
			if (error != EVRInitError.None)
			{
				ReportError(error);
				ShutdownSystems();
				return null;
			}
		}
		catch (System.Exception e)
		{
			Debug.Print(e.Message);
			return null;
		}

		return new SteamVR();
    }

    static SteamVR CreateInstanceBackground()
    {
        try
        {
            var error = EVRInitError.None;
            if (!SteamVR.usingNativeSupport)
            {
                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);
                if (error != EVRInitError.None)
                {
                    ReportError(error);
                    ShutdownSystems();
                    return null;
                }
            }

            // Verify common interfaces are valid.

            // OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
            // if (error != EVRInitError.None)
            // {
            //     ReportError(error);
            //     ShutdownSystems();
            //     return null;
            // }
            //
            // OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
            // if (error != EVRInitError.None)
            // {
            //     ReportError(error);
            //     ShutdownSystems();
            //     return null;
            // }
        }
        catch (System.Exception e)
        {
            Debug.Print(e.Message);
            return null;
        }

        return new SteamVR();
    }

    static void ReportError(EVRInitError error)
	{
		switch (error)
		{
			case EVRInitError.None:
				break;
			case EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime:
                Debug.Print("SteamVR Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
				break;
			case EVRInitError.Init_VRClientDLLNotFound:
				Debug.Print("SteamVR drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
				break;
			case EVRInitError.Driver_RuntimeOutOfDate:
				Debug.Print("SteamVR Initialization Failed!  Make sure device's runtime is up to date.");
				break;
			default:
				Debug.Print(OpenVR.GetStringForHmdError(error));
				break;
		}
	}

	// native interfaces
	public CVRSystem hmd { get; private set; }
	public CVRCompositor compositor { get; private set; }
	public CVROverlay overlay { get; private set; }

	// tracking status
	static public bool initializing { get; private set; }
	static public bool calibrating { get; private set; }
	static public bool outOfRange { get; private set; }

	static public bool[] connected = new bool[OpenVR.k_unMaxTrackedDeviceCount];

	// render values
	public float sceneWidth { get; private set; }
	public float sceneHeight { get; private set; }
	public float aspect { get; private set; }
	public float fieldOfView { get; private set; }
	public Vector2 tanHalfFov { get; private set; }
	public VRTextureBounds_t[] textureBounds { get; private set; }
	public SteamVR_Utils.RigidTransform[] eyes { get; private set; }
	//public EGraphicsAPIConvention graphicsAPI;

	// hmd properties
	public string hmd_TrackingSystemName { get { return GetStringProperty(ETrackedDeviceProperty.Prop_TrackingSystemName_String); } }
	public string hmd_ModelNumber { get { return GetStringProperty(ETrackedDeviceProperty.Prop_ModelNumber_String); } }
	public string hmd_SerialNumber { get { return GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String); } }

	public float hmd_SecondsFromVsyncToPhotons { get { return GetFloatProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float); } }
	public float hmd_DisplayFrequency { get { return GetFloatProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float); } }

	public string GetTrackedDeviceString(uint deviceId)
	{
		var error = ETrackedPropertyError.TrackedProp_Success;
		var capacity = hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, null, 0, ref error);
		if (capacity > 1)
		{
			var result = new System.Text.StringBuilder((int)capacity);
			hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, result, capacity, ref error);
			return result.ToString();
		}
		return null;
	}

	string GetStringProperty(ETrackedDeviceProperty prop)
	{
		var error = ETrackedPropertyError.TrackedProp_Success;
		var capactiy = hmd.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, null, 0, ref error);
		if (capactiy > 1)
		{
			var result = new System.Text.StringBuilder((int)capactiy);
			hmd.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, result, capactiy, ref error);
			return result.ToString();
		}
		return (error != ETrackedPropertyError.TrackedProp_Success) ? error.ToString() : "<unknown>";
	}

	float GetFloatProperty(ETrackedDeviceProperty prop)
	{
		var error = ETrackedPropertyError.TrackedProp_Success;
		return hmd.GetFloatTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, ref error);
	}

#region Event callbacks

	private void OnInitializing(params object[] args)
	{
		initializing = (bool)args[0];
	}

	private void OnCalibrating(params object[] args)
	{
		calibrating = (bool)args[0];
	}

	private void OnOutOfRange(params object[] args)
	{
		outOfRange = (bool)args[0];
	}

	private void OnDeviceConnected(params object[] args)
	{
		var i = (int)args[0];
		connected[i] = (bool)args[1];
	}

	private void OnNewPoses(params object[] args)
	{
		var poses = (TrackedDevicePose_t[])args[0];

		// Update eye offsets to account for IPD changes.
		eyes[0] = new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left));
		eyes[1] = new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right));

		for (int i = 0; i < poses.Length; i++)
		{
			var connected = poses[i].bDeviceIsConnected;
			if (connected != SteamVR.connected[i])
			{
				SteamVR_Utils.Event.Send("device_connected", i, connected);
			}
		}

		if (poses.Length > OpenVR.k_unTrackedDeviceIndex_Hmd)
		{
			var result = poses[OpenVR.k_unTrackedDeviceIndex_Hmd].eTrackingResult;

			var initializing = result == ETrackingResult.Uninitialized;
			if (initializing != SteamVR.initializing)
			{
				SteamVR_Utils.Event.Send("initializing", initializing);
			}

			var calibrating =
				result == ETrackingResult.Calibrating_InProgress ||
				result == ETrackingResult.Calibrating_OutOfRange;
			if (calibrating != SteamVR.calibrating)
			{
				SteamVR_Utils.Event.Send("calibrating", calibrating);
			}

			var outOfRange =
				result == ETrackingResult.Running_OutOfRange ||
				result == ETrackingResult.Calibrating_OutOfRange;
			if (outOfRange != SteamVR.outOfRange)
			{
				SteamVR_Utils.Event.Send("out_of_range", outOfRange);
			}
		}
	}

#endregion

    private float Max4(float w, float x, float y, float z)
    {
        return Math.Max(w, Math.Max(x, Math.Max(y, z)));
    }
    private SteamVR()
	{
		hmd = OpenVR.System;
		Debug.Print("Connected to " + hmd_TrackingSystemName + ":" + hmd_SerialNumber);

		compositor = OpenVR.Compositor;
		overlay = OpenVR.Overlay;

		// Setup render values
		uint w = 0, h = 0;
		hmd.GetRecommendedRenderTargetSize(ref w, ref h);
		sceneWidth = (float)w;
		sceneHeight = (float)h;

		float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
		hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

		float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
		hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

		tanHalfFov = new Vector2(
			Max4(-l_left, l_right, -r_left, r_right),
			Max4(-l_top, l_bottom, -r_top, r_bottom));

		textureBounds = new VRTextureBounds_t[2];

		textureBounds[0].uMin = 0.5f + 0.5f * l_left / tanHalfFov.X;
		textureBounds[0].uMax = 0.5f + 0.5f * l_right / tanHalfFov.X;
		textureBounds[0].vMin = 0.5f - 0.5f * l_bottom / tanHalfFov.Y;
		textureBounds[0].vMax = 0.5f - 0.5f * l_top / tanHalfFov.Y;

		textureBounds[1].uMin = 0.5f + 0.5f * r_left / tanHalfFov.X;
		textureBounds[1].uMax = 0.5f + 0.5f * r_right / tanHalfFov.X;
		textureBounds[1].vMin = 0.5f - 0.5f * r_bottom / tanHalfFov.Y;
		textureBounds[1].vMax = 0.5f - 0.5f * r_top / tanHalfFov.Y;

#if (UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0)
		SteamVR.Unity.SetSubmitParams(textureBounds[0], textureBounds[1], EVRSubmitFlags.Submit_Default);
#endif
		// Grow the recommended size to account for the overlapping fov
		sceneWidth = sceneWidth / Math.Max(textureBounds[0].uMax - textureBounds[0].uMin, textureBounds[1].uMax - textureBounds[1].uMin);
		sceneHeight = sceneHeight / Math.Max(textureBounds[0].vMax - textureBounds[0].vMin, textureBounds[1].vMax - textureBounds[1].vMin);

		aspect = tanHalfFov.X / tanHalfFov.Y;
		fieldOfView = SteamVR_Utils.RadianToDegree(2.0f * Convert.ToSingle(Math.Atan(tanHalfFov.Y)));

		eyes = new SteamVR_Utils.RigidTransform[] {
			new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left)),
			new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right)) };

		//if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
		//	graphicsAPI = EGraphicsAPIConvention.API_OpenGL;
		//else
		//	graphicsAPI = EGraphicsAPIConvention.API_DirectX;

		SteamVR_Utils.Event.Listen("initializing", OnInitializing);
		SteamVR_Utils.Event.Listen("calibrating", OnCalibrating);
		SteamVR_Utils.Event.Listen("out_of_range", OnOutOfRange);
		SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
		SteamVR_Utils.Event.Listen("new_poses", OnNewPoses);
	}

	~SteamVR()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		System.GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		SteamVR_Utils.Event.Remove("initializing", OnInitializing);
		SteamVR_Utils.Event.Remove("calibrating", OnCalibrating);
		SteamVR_Utils.Event.Remove("out_of_range", OnOutOfRange);
		SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
		SteamVR_Utils.Event.Remove("new_poses", OnNewPoses);

		ShutdownSystems();
		_instance = null;
	}

	private static void ShutdownSystems()
	{
#if (UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0)
		OpenVR.Shutdown();
#endif
	}

	// Use this interface to avoid accidentally creating the instance in the process of attempting to dispose of it.
	public static void SafeDispose()
	{
		if (_instance != null)
			_instance.Dispose();
	}

#if (UNITY_5_3 || UNITY_5_2 || UNITY_5_1 || UNITY_5_0)
	// Unityhooks in openvr_api.
	public class Unity
	{
		public const int k_nRenderEventID_WaitGetPoses = 201510020;
		public const int k_nRenderEventID_SubmitL = 201510021;
		public const int k_nRenderEventID_SubmitR = 201510022;
		public const int k_nRenderEventID_Flush = 201510023;
		public const int k_nRenderEventID_PostPresentHandoff = 201510024;

		[DllImport("openvr_api", EntryPoint = "UnityHooks_GetRenderEventFunc")]
		public static extern System.IntPtr GetRenderEventFunc();

		[DllImport("openvr_api", EntryPoint = "UnityHooks_SetSubmitParams")]
		public static extern void SetSubmitParams(VRTextureBounds_t boundsL, VRTextureBounds_t boundsR, EVRSubmitFlags nSubmitFlags);

		[DllImport("openvr_api", EntryPoint = "UnityHooks_SetColorSpace")]
		public static extern void SetColorSpace(EColorSpace eColorSpace);

		[DllImport("openvr_api", EntryPoint = "UnityHooks_EventWriteString")]
		public static extern void EventWriteString([In, MarshalAs(UnmanagedType.LPWStr)] string sEvent);
	}
#endif
}

