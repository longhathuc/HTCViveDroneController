//========= Copyright 2014, Valve Corporation, All rights reserved. ===========
//
// Purpose: Utilities for working with SteamVR
//
//=============================================================================

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Numerics;
using Valve.VR;
using System.Diagnostics;

public static class SteamVR_Utils
{
	public class Event
	{
		public delegate void Handler(params object[] args);

		public static void Listen(string message, Handler action)
		{
			var actions = listeners[message] as Handler;
			if (actions != null)
			{
				listeners[message] = actions + action;
			}
			else
			{
				listeners[message] = action;
			}
		}

		public static void Remove(string message, Handler action)
		{
			var actions = listeners[message] as Handler;
			if (actions != null)
			{
				listeners[message] = actions - action;
			}
		}

		public static void Send(string message, params object[] args)
		{
			var actions = listeners[message] as Handler;
			if (actions != null)
			{
				actions(args);
			}
		}

		private static Hashtable listeners = new Hashtable();
	}
    public static T Clamp<T>(T value, T max, T min)
    where T : System.IComparable<T>
    {
        T result = value;
        if (value.CompareTo(max) > 0)
            result = max;
        if (value.CompareTo(min) < 0)
            result = min;
        return result;
    }

    // this version does not clamp [0..1]
    public static Quaternion Slerp(Quaternion A, Quaternion B, float t)
	{
		var cosom = Clamp(A.X * B.X + A.Y * B.Y + A.Z * B.Z + A.W * B.W, -1.0f, 1.0f);
		if (cosom < 0.0f)
		{
			B = new Quaternion(-B.X, -B.Y, -B.Z, -B.W);
			cosom = -cosom;
		}

		float sclp, sclq;
		if ((1.0f - cosom) > 0.0001f)
		{
			var omega = Math.Acos(cosom);
			var sinom = Math.Sin(omega);
			sclp = Convert.ToSingle(Math.Sin((1.0 - t) * omega) / sinom);
			sclq = Convert.ToSingle(Math.Sin(t * omega) / sinom);
		}
		else
		{
			// "from" and "to" very close, so do linear interp
			sclp = 1.0f - t;
			sclq = t;
		}

		return new Quaternion(
			sclp * A.X + sclq * B.X,
			sclp * A.Y + sclq * B.Y,
			sclp * A.Z + sclq * B.Z,
			sclp * A.W + sclq * B.W);
	}

	public static Vector3 Lerp(Vector3 A, Vector3 B, float t)
	{
		return new Vector3(
			Lerp(A.X, B.X, t),
			Lerp(A.Y, B.Y, t),
			Lerp(A.Z, B.Z, t));
	}

	public static float Lerp(float A, float B, float t)
	{
		return A + (B - A) * t;
	}

	public static double Lerp(double A, double B, double t)
	{
		return A + (B - A) * t;
	}

	public static float InverseLerp(Vector3 A, Vector3 B, Vector3 result)
	{
		return Vector3.Dot(result - A, B - A);
	}

	public static float InverseLerp(float A, float B, float result)
	{
		return (result - A) / (B - A);
	}

	public static double InverseLerp(double A, double B, double result)
	{
		return (result - A) / (B - A);
	}

	public static float Saturate(float A)
	{
		return (A < 0) ? 0 : (A > 1) ? 1 : A;
	}

	public static Vector2 Saturate(Vector2 A)
	{
		return new Vector2(Saturate(A.X), Saturate(A.Y));
	}

	public static float Abs(float A)
	{
		return (A < 0) ? -A : A;
	}

	public static Vector2 Abs(Vector2 A)
	{
		return new Vector2(Abs(A.X), Abs(A.Y));
	}

	private static float _copysign(float sizeval, float signval)
	{
		return Math.Sign(signval) == 1 ? Math.Abs(sizeval) : -Math.Abs(sizeval);
	}

	public static Quaternion GetRotation(this Matrix4x4 matrix)
	{
		Quaternion q = new Quaternion();
		q.W = Convert.ToSingle(Math.Sqrt(Math.Max(0, 1 + matrix.M11 + matrix.M22 + matrix.M33)) / 2);
		q.X = Convert.ToSingle(Math.Sqrt(Math.Max(0, 1 + matrix.M11 - matrix.M22 - matrix.M33)) / 2);
		q.Y = Convert.ToSingle(Math.Sqrt(Math.Max(0, 1 - matrix.M11 + matrix.M22 - matrix.M33)) / 2);
        q.Z = Convert.ToSingle(Math.Sqrt(Math.Max(0, 1 - matrix.M11 - matrix.M22 + matrix.M33)) / 2);
		q.X = _copysign(q.X, matrix.M32 - matrix.M23);
		q.Y = _copysign(q.Y, matrix.M13 - matrix.M31);
		q.Z = _copysign(q.Z, matrix.M21 - matrix.M12);
		return q;
	}

	public static Vector3 GetPosition(this Matrix4x4 matrix)
	{
		var x = matrix.M14;
		var y = matrix.M24;
		var z = matrix.M34;

		return new Vector3(x, y, z);
	}

	public static Vector3 GetScale(this Matrix4x4 m)
	{
		var x = Convert.ToSingle(Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13));
		var y = Convert.ToSingle(Math.Sqrt(m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23));
		var z = Convert.ToSingle(Math.Sqrt(m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33));

		return new Vector3(x, y, z);
	}

    public struct Transform
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 position;
        public Quaternion rotation;

    }
    public static Vector3 QuaternionMultiplyVector3(Quaternion quat, Vector3 vec)
    {
        float num  = quat.X * 2f;
        float num2 = quat.Y * 2f;
        float num3 = quat.Z * 2f;
        float num4 = quat.X * num;
        float num5 = quat.Y * num2;
        float num6 = quat.Z * num3;
        float num7 = quat.X * num2;
        float num8 = quat.X * num3;
        float num9 = quat.Y * num3;
        float num10 = quat.W * num;
        float num11 = quat.W * num2;
        float num12 = quat.W * num3;
        Vector3 result;
        result.X = (1f - (num5 + num6)) * vec.X + (num7 - num12) * vec.Y + (num8 + num11) * vec.Z;
        result.Y = (num7 + num12) * vec.X + (1f - (num4 + num6)) * vec.Y + (num9 - num10) * vec.Z;
        result.Z = (num8 - num11) * vec.X + (num9 + num10) * vec.Y + (1f - (num4 + num5)) * vec.Z;
        return result;
    }

    [System.Serializable]
	public struct RigidTransform
	{
		public Vector3 pos;
		public Quaternion rot;

		public static RigidTransform identity
		{
			get { return new RigidTransform(Vector3.Zero, Quaternion.Identity); }
		}

		public static RigidTransform FromLocal(Transform t)
		{
			return new RigidTransform(t.localPosition, t.localRotation);
		}

		public RigidTransform(Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}

		public RigidTransform(Transform t)
		{
			this.pos = t.position;
			this.rot = t.rotation;
		}

		public RigidTransform(Transform from, Transform to)
		{
            Quaternion inv = Quaternion.Inverse(from.rotation);
            Vector3 invV3 = new Vector3(inv.X, inv.Y, inv.Z);
			rot = inv * to.rotation;
            //pos = invV3 * (to.position - from.position);
            pos = Vector3.Transform(to.position - from.position, inv);
		}

		public RigidTransform(HmdMatrix34_t pose)
		{
			var m = Matrix4x4.Identity;

			m.M11 =  pose.m0;
			m.M12 =  pose.m1;
			m.M13 = -pose.m2;
			m.M14 =  pose.m3;
            
			m.M21 =  pose.m4;
			m.M22 =  pose.m5;
			m.M23 = -pose.m6;
			m.M24 =  pose.m7;
            
			m.M31 = -pose.m8;
			m.M32 = -pose.m9;
			m.M33 =  pose.m10;
			m.M34 = -pose.m11;

			this.pos = m.GetPosition();
			this.rot = m.GetRotation();
		}

		public RigidTransform(HmdMatrix44_t pose)
		{
			var m = Matrix4x4.Identity;

			m.M11 =  pose.m0;
			m.M12 =  pose.m1;
			m.M13 = -pose.m2;
			m.M14 =  pose.m3;

			m.M21 =  pose.m4;
			m.M22 =  pose.m5;
			m.M23 = -pose.m6;
			m.M24 =  pose.m7;

			m.M31 = -pose.m8;
			m.M32 = -pose.m9;
			m.M33 =  pose.m10;
            m.M34 = -pose.m11;

			m.M41 =  pose.m12;
			m.M42 =  pose.m13;
			m.M43 = -pose.m14;
            m.M44 =  pose.m15;

			this.pos = m.GetPosition();
			this.rot = m.GetRotation();
		}

		public HmdMatrix44_t ToHmdMatrix44()
		{
            //var m = Matrix4x4.tr(pos, rot, Vector3.one);
			var pose = new HmdMatrix44_t();

            //pose.m0  =  m[0, 0];
            //pose.m1  =  m[0, 1];
            //pose.m2  = -m[0, 2];
            //pose.m3  =  m[0, 3];
            //
            //pose.m4  =  m[1, 0];
            //pose.m5  =  m[1, 1];
            //pose.m6  = -m[1, 2];
            //pose.m7  =  m[1, 3];
            //
            //pose.m8  = -m[2, 0];
            //pose.m9  = -m[2, 1];
            //pose.m10 =  m[2, 2];
            //pose.m11 = -m[2, 3];
            //
            //pose.m12 =  m[3, 0];
            //pose.m13 =  m[3, 1];
            //pose.m14 = -m[3, 2];
            //pose.m15 =  m[3, 3];
            Debug.Assert(false, "Not implemented");
			return pose;
		}

		public HmdMatrix34_t ToHmdMatrix34()
		{
			//var m = Matrix4x4.TRS(pos, rot, Vector3.one);
			var pose = new HmdMatrix34_t();

			//pose.m0  =  m[0, 0];
            //pose.m1  =  m[0, 1];
			//pose.m2  = -m[0, 2];
			//pose.m3  =  m[0, 3];
            //
			//pose.m4  =  m[1, 0];
			//pose.m5  =  m[1, 1];
			//pose.m6  = -m[1, 2];
			//pose.m7  =  m[1, 3];
            //
			//pose.m8  = -m[2, 0];
			//pose.m9  = -m[2, 1];
			//pose.m10 =  m[2, 2];
			//pose.m11 = -m[2, 3];
            Debug.Assert(false, "Not implemented");

            return pose;
		}

		public override bool Equals(object o)
		{
			if (o is RigidTransform)
			{
				RigidTransform t = (RigidTransform)o;
				return pos == t.pos && rot == t.rot;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return pos.GetHashCode() ^ rot.GetHashCode();
		}

		public static bool operator ==(RigidTransform a, RigidTransform b)
		{
			return a.pos == b.pos && a.rot == b.rot;
		}

		public static bool operator !=(RigidTransform a, RigidTransform b)
		{
			return a.pos != b.pos || a.rot != b.rot;
		}

		public static RigidTransform operator *(RigidTransform a, RigidTransform b)
		{
            return new RigidTransform(a.pos + QuaternionMultiplyVector3(a.rot, b.pos), a.rot * b.rot);
		}

		public void Inverse()
		{
			rot = Quaternion.Inverse(rot);
			pos = -QuaternionMultiplyVector3(rot, pos);
		}

		public RigidTransform GetInverse()
		{
			var t = new RigidTransform(pos, rot);
			t.Inverse();
			return t;
		}

		public void Multiply(RigidTransform a, RigidTransform b)
		{
			rot = a.rot * b.rot;
			pos = a.pos + QuaternionMultiplyVector3(a.rot, b.pos);
		}

		public Vector3 InverseTransformPoint(Vector3 point)
		{
			return QuaternionMultiplyVector3(Quaternion.Inverse(rot), (point - pos));
		}

		public Vector3 TransformPoint(Vector3 point)
		{
			return pos + QuaternionMultiplyVector3(rot, point);
		}

		public static Vector3 operator *(RigidTransform t, Vector3 v)
		{
			return t.TransformPoint(v);
		}

		public static RigidTransform Interpolate(RigidTransform a, RigidTransform b, float t)
		{
			return new RigidTransform(Vector3.Lerp(a.pos, b.pos, t), Quaternion.Slerp(a.rot, b.rot, t));
		}

		public void Interpolate(RigidTransform to, float t)
		{
			pos = SteamVR_Utils.Lerp(pos, to.pos, t);
			rot = SteamVR_Utils.Slerp(rot, to.rot, t);
		}
	}

	//public static Mesh CreateHiddenAreaMesh(HiddenAreaMesh_t src, VRTextureBounds_t bounds)
	//{
	//	if (src.unTriangleCount == 0)
	//		return null;
    //
	//	var data = new float[src.unTriangleCount * 3 * 2]; //HmdVector2_t
	//	Marshal.Copy(src.pVertexData, data, 0, data.Length);
    //
	//	var vertices = new Vector3[src.unTriangleCount * 3 + 12];
	//	var indices = new int[src.unTriangleCount * 3 + 24];
    //
	//	var x0 = 2.0f * bounds.uMin - 1.0f;
	//	var x1 = 2.0f * bounds.uMax - 1.0f;
	//	var y0 = 2.0f * bounds.vMin - 1.0f;
	//	var y1 = 2.0f * bounds.vMax - 1.0f;
    //
	//	for (int i = 0, j = 0; i < src.unTriangleCount * 3; i++)
	//	{
	//		var x = Lerp(x0, x1, data[j++]);
	//		var y = Lerp(y0, y1, data[j++]);
	//		vertices[i] = new Vector3(x, y, 0.0f);
	//		indices[i] = i;
	//	}
    //
	//	// Add border
	//	var offset = (int)src.unTriangleCount * 3;
	//	var iVert = offset;
	//	vertices[iVert++] = new Vector3(-1, -1, 0);
	//	vertices[iVert++] = new Vector3(x0, -1, 0);
	//	vertices[iVert++] = new Vector3(-1,  1, 0);
	//	vertices[iVert++] = new Vector3(x0,  1, 0);
	//	vertices[iVert++] = new Vector3(x1, -1, 0);
	//	vertices[iVert++] = new Vector3( 1, -1, 0);
	//	vertices[iVert++] = new Vector3(x1,  1, 0);
	//	vertices[iVert++] = new Vector3( 1,  1, 0);
	//	vertices[iVert++] = new Vector3(x0, y0, 0);
	//	vertices[iVert++] = new Vector3(x1, y0, 0);
	//	vertices[iVert++] = new Vector3(x0, y1, 0);
	//	vertices[iVert++] = new Vector3(x1, y1, 0);
    //
	//	var iTri = offset;
	//	indices[iTri++] = offset + 0;
	//	indices[iTri++] = offset + 1;
	//	indices[iTri++] = offset + 2;
	//	indices[iTri++] = offset + 2;
	//	indices[iTri++] = offset + 1;
	//	indices[iTri++] = offset + 3;
	//	indices[iTri++] = offset + 4;
	//	indices[iTri++] = offset + 5;
	//	indices[iTri++] = offset + 6;
	//	indices[iTri++] = offset + 6;
	//	indices[iTri++] = offset + 5;
	//	indices[iTri++] = offset + 7;
	//	indices[iTri++] = offset + 1;
	//	indices[iTri++] = offset + 4;
	//	indices[iTri++] = offset + 8;
	//	indices[iTri++] = offset + 8;
	//	indices[iTri++] = offset + 4;
	//	indices[iTri++] = offset + 9;
	//	indices[iTri++] = offset + 10;
	//	indices[iTri++] = offset + 11;
	//	indices[iTri++] = offset + 3;
	//	indices[iTri++] = offset + 3;
	//	indices[iTri++] = offset + 11;
	//	indices[iTri++] = offset + 6;
    //
	//	var mesh = new Mesh();
	//	mesh.vertices = vertices;
	//	mesh.triangles = indices;
    //    mesh.bounds = new Bounds( Vector3.zero, new Vector3( float.MaxValue, float.MaxValue, float.MaxValue ) ); // Prevent frustum culling from culling this mesh
	//	return mesh;
	//}

	public delegate object SystemFn(CVRSystem system, params object[] args);

	public static object CallSystemFn(SystemFn fn, params object[] args)
	{
		var initOpenVR = (!SteamVR.active && !SteamVR.usingNativeSupport);
		if (initOpenVR)
		{
			var error = EVRInitError.None;
			OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
		}

		var system = OpenVR.System;
		var result = (system != null) ? fn(system, args) : null;

		if (initOpenVR)
			OpenVR.Shutdown();

		return result;
	}

	public static void QueueEventOnRenderThread(int eventID)
	{
#if (UNITY_5_0 || UNITY_5_1)
		GL.IssuePluginEvent(eventID);
#elif (UNITY_5_2 || UNITY_5_3)
		GL.IssuePluginEvent(SteamVR.Unity.GetRenderEventFunc(), eventID);
#endif
	}
    public static Vector3 Normalize(Vector3 A)
    {
        float XY = Convert.ToSingle(Math.Sqrt(A.X * A.X + A.Y * A.Y));
        float distance = Convert.ToSingle(Math.Sqrt(XY * XY + A.Z * A.Z));
        return new Vector3(A.X / distance, A.Y / distance, A.Z / distance);
    }

    // BDG MODS //
    public static float DegreeToRadian(float angle)
    {
        return Convert.ToSingle(Math.PI * angle / 180.0);
    }
    public static Vector3 DegreeToRadian(Vector3 v)
    {
        return Convert.ToSingle(Math.PI / 180.0) * v;
    }
    public static float RadianToDegree(float angle)
    {
        return Convert.ToSingle(angle * (180.0 / Math.PI));
    }
    public static Vector3 RadianToDegree(Vector3 v)
    {
        return Convert.ToSingle(180.0 / Math.PI) * v;
    }

    public static Quaternion AngleToQ(Vector3 v)
    {
        return AngleToQ(v.Y, v.X, v.Z);
    }

    public static Quaternion AngleToQ(float yaw, float pitch, float roll)
    {
        yaw = DegreeToRadian(yaw);
        pitch = DegreeToRadian(pitch);
        roll = DegreeToRadian(roll);
        float rollOver2 = roll * 0.5f;
        float sinRollOver2 = (float)Math.Sin((double)rollOver2);
        float cosRollOver2 = (float)Math.Cos((double)rollOver2);
        float pitchOver2 = pitch * 0.5f;
        float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
        float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
        float yawOver2 = yaw * 0.5f;
        float sinYawOver2 = (float)Math.Sin((double)yawOver2);
        float cosYawOver2 = (float)Math.Cos((double)yawOver2);
        Quaternion result;
        result.W = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
        result.X = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
        result.Y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
        result.Z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

        return result;
    }

    public static Vector3 AngleFromQ2(Quaternion q1)
    {
        float sqw = q1.W * q1.W;
        float sqx = q1.X * q1.X;
        float sqy = q1.Y * q1.Y;
        float sqz = q1.Z * q1.Z;
        float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        float test = q1.X * q1.W - q1.Y * q1.Z;
        Vector3 v;

        if (test > 0.4995f * unit)
        { // singularity at north pole
            v.Y = Convert.ToSingle(2.0 * Math.Atan2(q1.Y, q1.X));
            v.X = Convert.ToSingle(Math.PI / 2.0);
            v.Z = 0;
            return NormalizeAngles(RadianToDegree(v));
        }
        if (test < -0.4995f * unit)
        { // singularity at south pole
            v.Y = Convert.ToSingle(-2.0 * Math.Atan2(q1.Y, q1.X));
            v.X = Convert.ToSingle(-Math.PI / 2.0);
            v.Z = 0;
            return NormalizeAngles(RadianToDegree(v));
        }
        Quaternion q = new Quaternion(q1.W, q1.Z, q1.X, q1.Y);
        v.Y = (float)Math.Atan2(2f * q.X * q.W + 2.0 * q.Y * q.Z, 1 - 2.0 * (q.Z * q.Z + q.W * q.W));     // Yaw
        v.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
        v.Z = (float)Math.Atan2(2f * q.X * q.Y + 2.0 * q.Z * q.W, 1 - 2.0 * (q.Y * q.Y + q.Z * q.Z));      // Roll
        return NormalizeAngles(RadianToDegree(v));
    }

    static Vector3 NormalizeAngles(Vector3 angles)
    {
        angles.X = NormalizeAngle(angles.X);
        angles.Y = NormalizeAngle(angles.Y);
        angles.Z = NormalizeAngle(angles.Z);
        return angles;
    }

    static float NormalizeAngle(float angle)
    {
        while (angle > 360)
            angle -= 360;
        while (angle < 0)
            angle += 360;
        return angle;
    }
}

