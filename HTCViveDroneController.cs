using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Valve.VR;
using vJoyInterfaceWrap;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;

namespace HTCViveDroneController
{
    public partial class HTCViveDroneController : Form
    {
        #region Variables
        // constants
        private const string _defaultConfigName = "Default";
        public const string _vjoyMonitorBin = "JoyMonitor.exe";
        public const string _vjoyConfigBin = "vJoyConf.exe";
        public const string _vjoyDefaultPath = "c:\\Program Files\\vJoy\\x64";

        // Public Settings
        public const int MAX_NUM_HATS = 4; // max allowed by vjoy
        static public uint _vjoyId = 0; // changing this will change which vjoy device we're connected to, 0 = not connected
        static public string _vjoyPath = ""; // path to the jvoy executables
        static public bool _showControlPanel = false;
        static public double _hapticZonePercent = 0.05; // if you go this far or further from the ends, you get haptic buzz
        static public bool _invertXAxis = false;
        static public bool _invertYAxis = true;
        static public bool _invertZAxis = true;
        static public bool _invertZRAxis = false;
        static public float _touchpadCenterMargin = 0.3f; // how much of the touchpad counts as the center (% of radius from center)
        static public bool[] _hatIsButtons = new bool[MAX_NUM_HATS];


        public enum ReleaseAction { NONE, CENTER_JOYSTICK, CENTER_RUDDER, CENTER_TROTTLE, YAW_ENABLE, PITCH_ENABLE };
        public static ReleaseAction actionOnReleasePrimary = ReleaseAction.NONE;   // action on release of the grip 
        public static ReleaseAction actionOnReleaseSecondary = ReleaseAction.NONE; // action on release of the grip 

        public enum ViveButtons
        {
            APP_MENU = EVRButtonId.k_EButton_ApplicationMenu,
            GRIP = EVRButtonId.k_EButton_Grip,
            TOUCHPAD = EVRButtonId.k_EButton_SteamVR_Touchpad,
            TRIGGER = EVRButtonId.k_EButton_SteamVR_Trigger,
            HAIR_TRIGGER = EVRButtonId.k_EButton_Max, // we use this for the hair trigger
        };

        // Variables
        private static SteamVR _steamVR;
       // private static ControlPanel _controlPanel = new ControlPanel();
        private static ConfigPanel _configPanel = new ConfigPanel();
        private static int _pollCycleMSec = 40;
        private static ushort _hapticPulseUSec = 3999;

        private static SteamVR_Utils.RigidTransform _gripDownRTPrimary;   // tracks location of move start
        private static SteamVR_Utils.RigidTransform _gripDownRTSecondary; // tracks location of move start
        // keep track of button states
        private static Dictionary<ViveButtons, bool> _buttonsPrimary = new Dictionary<ViveButtons, bool>();
        private static Dictionary<ViveButtons, bool> _buttonsSecondary = new Dictionary<ViveButtons, bool>();

        // map button to what we want it to do
        [Serializable()]
        public class Configuration
        {
            public readonly string Name;
            public bool RightHanded = true; // right (true) or left (false) handed for primary controller
            public bool ControlTypeJoystick = true; // joystick (true) or flightstick (false) simulation
            public bool GripTypeTogglePrimary = false;   // is the grip a toggle (true) or hold (false) type
            public bool GripTypeToggleSecondary = false; // is the grip a toggle (true) or hold (false) type
            public Dictionary<ViveButtons, ButtonMap> PrimaryMap = new Dictionary<ViveButtons, ButtonMap>();
            public Dictionary<ViveButtons, ButtonMap> SecondaryMap = new Dictionary<ViveButtons, ButtonMap>();

            public Configuration(string name)
            {
                int index = 1;
                string newName = name;
                while (HTCViveDroneController._configurations.Find(x => newName.Equals(x.Name)) != null)
                {
                    newName = name + "_" + index.ToString();
                    index++;
                }
                Name = newName;
                FillInViveButtonMap(newName, PrimaryMap, SecondaryMap);
            }
        }
        public static List<Configuration> Configurations { get { return _configurations; } }
        private static List<Configuration> _configurations = new List<Configuration>();
        private static Configuration _currentConfiguration;
        public static Configuration CurrentConfiguration {
            get { return _currentConfiguration; }
            set { _currentConfiguration = value; _instance.Text = "HTCViveDroneController - " + value.Name; }
        }

        private const double _MotionFullScaleZ = 0.15;
        private const double _RotationFullScaleDegrees = 180 / 4;

        private static bool _gripPrimary = false;   // is grip button pressed
        private static bool _gripSecondary = false; // is grip button pressed
        private static bool _gripHoldingPrimary = false;   // is grip in holding state
        private static bool _gripHoldingSecondary = false; // is grip in holding state

        private static bool _initialized = false;
        private static bool _shuttingDown = false;
        private static HTCViveDroneController _instance;       
        private static Thread _handler = null;

        private const string _configKeyVjoyId = "VJOY_DEVICE_ID";

        private static readonly string _appDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\HTCViveDroneController";
        private static readonly string _configFileName = _appDir + "\\config.bin";
        private static readonly string _logFileName = _appDir + "\\log.txt";
        private static bool _logStarted = false;
        private static BlockingCollection<string> _logQueue = new BlockingCollection<string>();
        #endregion

        #region Form Init
        public HTCViveDroneController(string[] args)
        {
            Task.Factory.StartNew(() => LogTask(), TaskCreationOptions.LongRunning);

            string errMsg = null; // if this get's set, then we show/log the error and exit the application
            _instance = this;

            Directory.CreateDirectory(_appDir); // does nothing if directory already exists

            InitializeComponent();
            lblDeviceId.Width = cmbVJoyId.Width;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Log("HTCViveDroneController Version " + version.Major.ToString() + "." + version.Minor.ToString());
            lblStatus.Text = "Checking Vjoy driver...";

            // get default button setup
            _configurations.Add(new Configuration(_defaultConfigName));

            // Read config from disk
            string currentConfigName = LoadConfig();
            CurrentConfiguration = GetConfig(currentConfigName);

            // Create one joystick object and a position structure.
            vJoy joystick = new vJoy();

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                btnVjoyConfig_Click(null, null);
                MessageBox.Show("VJoy driver not enabled, enable it and then press OK", "HTCViveDroneController", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (!joystick.vJoyEnabled())  errMsg = "vJoy driver not enabled.\n";
            }
            if (errMsg == null)
            {
                Log(string.Format("Vjoy Vendor: {0}  Product:{1}  Version:{2}", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString()));

                // Test if DLL matches the driver
                UInt32 DllVer = 0, DrvVer = 0;
                bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
                if (match)
                {
                    Log(string.Format("VJoy Driver and DLL version Match: {0:X}", DllVer));

                    // fill in the vjoy device list
                    cmbVJoyId.Items.Clear();
                    cmbVJoyId.Items.Add("Select Device");
                    for (uint id = 1; id <= 16; id++)
                    {
                        VjdStat status = joystick.GetVJDStatus(id);
                        switch (status)
                        {
                            case VjdStat.VJD_STAT_OWN:
                            case VjdStat.VJD_STAT_FREE:
                                cmbVJoyId.Items.Add(id.ToString());
                                break;
                        }
                    }
                    cmbVJoyId.SelectedIndex = 0;
                    int numItems = cmbVJoyId.Items.Count;
                    if (numItems < 2)
                    {
                        errMsg = "No Vjoy devices available";
                    }
                    else if (cmbVJoyId.Items.Count == 2)
                    {
                        // there's only one item (the other is the message to select an item) so lets just use it by default
                        cmbVJoyId.SelectedIndex = 1;
                    }
                    else
                    {
                        // use id of zero or what was last saved in the appsettings if it's valid
                        string idStr = System.Configuration.ConfigurationManager.AppSettings[_configKeyVjoyId];
                        if (cmbVJoyId.Items.Contains(idStr))
                        {
                            cmbVJoyId.SelectedItem = idStr;
                        }
                    }

                    if (errMsg == null) // no error so far
                    {
                        // Connect to steamVR
                        lblStatus.Text = "Connecting to SteamVR";
                        _steamVR = SteamVR.instance;
                        if (_steamVR == null)
                        {
                            new SteamVrStartDialog().ShowDialog();
                            _steamVR = SteamVR.instance;
                        }
                }
                else
                {
                    errMsg = string.Format("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);
                }
            }


            if (errMsg == null)
            {
                    if (OpenVR.System == null)
                    {
                        errMsg = "SteamVR not started.";
                    }
                }
            }

            if (errMsg == null)
            {
                _handler = new Thread(() =>
                    ControllerHandler(joystick)
                );
                _handler.Start();
            }
            else
            {
                _shuttingDown = true;
                Log(errMsg);
                MessageBox.Show(errMsg, "Error");
                Load += (s, e) => Close();
            }
        }

        private void HTCViveDroneController_Shown(object sender, EventArgs e)
        {
            _initialized = true;
        }

        private void HTCViveDroneController_FormClosing(object sender, FormClosingEventArgs e)
        {
            _shuttingDown = true;
            if (_handler != null)
            {
                if (_handler.IsAlive)
                {
                    // wait for thread to exit
                    Thread.Sleep(_pollCycleMSec);
                }
                if (_handler.IsAlive)
                {
                    // something is wrong, wait a bit longer before forcing an exit
                    Thread.Sleep(_pollCycleMSec * 2);
                }
                if (_handler.IsAlive)
                {
                    // force the handler to exit
                    _handler.Abort();
                }
            }
            _logQueue.CompleteAdding();
        }
        #endregion

        #region Public Methods
        public static bool ConfigExists(string name)
        {
            Configuration config = _configurations.Find(x => name.Equals(x.Name));
            return config != null;
        }

        /// <summary>
        /// Get the configuration with the given name and set it to the default
        /// </summary>
        /// <param name="name">name to find</param>
        /// <returns>the given configuration, a new one is created if it doesn't exist</returns>
        public static Configuration GetConfig(string name)
        {
            Configuration config = _configurations.Find(x => name.Equals(x.Name));
            if (config == null)
            {
                config = new Configuration(name);
                _configurations.Add(config);
            }
            CurrentConfiguration = config;
            return config;
        }

        public static void SetCurrentConfig(string name)
        {
            Configuration config = GetConfig(name);
            if (config != null)
            {
                CurrentConfiguration = config;
            }
        }
        /// <summary>
        /// Update the status label on the main form
        /// </summary>
        /// <param name="msg">message to show</param>
        /// <param name="logIt">true to log to the log file as well</param>
        public static void UpdateStatus(string msg, bool logIt = true)
        {
            if (_instance.lblStatus.InvokeRequired)
            {
                // BeginInvoke sets up to run when can
                // this.Invoke() will block until after UI runs the code
                _instance.BeginInvoke((MethodInvoker)delegate
                {
                    UpdateStatus(msg, logIt);
                });
            }
            else
            {
                _instance.lblStatus.Text = msg;
                if (logIt) Log(msg);
            }
        }

        public static void Assert(string msg)
        {
            System.Media.SystemSounds.Exclamation.Play();
            UpdateStatus(msg, false);
            _logQueue.Add(msg + "\n" + Environment.StackTrace);
        }

        /// <summary>
        /// Log a message to the log file
        /// </summary>
        /// <param name="msg">message to log</param>
        public static void Log(string msg)
        {
            _logQueue.Add(msg);
        }

        public static void SaveConfig()
        {
            if (_configFileName != null)
            {
                Stream stream = null;
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    

                    stream = File.Open(_configFileName, FileMode.Create);
                    
                    formatter.Serialize(stream, _vjoyPath);
                    formatter.Serialize(stream, CurrentConfiguration.Name);
                    formatter.Serialize(stream, _configurations);
                }
                catch (Exception ex)
                {
                    Log("Unable to save configuration.\n" + ex.Message);
                }
                if (stream != null) stream.Close();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load in the configuration from disk
        /// </summary>
        /// <returns>the 'current config name' that was saved</returns>
        private static string LoadConfig()
        {
            string currentConfigName = _defaultConfigName;
            if ((_configFileName != null) && File.Exists(_configFileName))
            {
                Stream stream = null;
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream = File.Open(_configFileName, FileMode.Open);
                    
                    _vjoyPath = (string)formatter.Deserialize(stream);
                    currentConfigName = (string)formatter.Deserialize(stream);
                    _configurations = (List<Configuration>)formatter.Deserialize(stream);
                }
                catch ( Exception ex )
                {
                    Log("Unable to load configuration.\n" + ex.Message);
                }
                if (stream != null) stream.Close();
            }
            return currentConfigName;
        }

        /// <summary>
        /// the log writing thread
        /// </summary>
        private static void LogTask()
        {
            if (!_logStarted)
            {
                // trim the head of the file if it gets too large
                int trimSizeBytes = (int)(0.5 * 1024 * 1024);
                if (File.Exists(_logFileName) && (new System.IO.FileInfo(_logFileName).Length > trimSizeBytes))
                {
                    trimSizeBytes = (int)(trimSizeBytes * 0.5);
                    using (MemoryStream ms = new MemoryStream(trimSizeBytes))
                    {
                        using (FileStream s = new FileStream(_logFileName, FileMode.Open, FileAccess.ReadWrite))
                        {
                            s.Seek(-trimSizeBytes, SeekOrigin.End);
                            s.CopyTo(ms);
                            s.SetLength(trimSizeBytes);
                            s.Position = 0;
                            ms.Position = 0;
                            ms.CopyTo(s);
                        }
                    }
                }
            }
            using (StreamWriter w = File.AppendText(_logFileName))
            {
                foreach (string s in _logQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        if (!_logStarted)
                        {
                            w.WriteLine("-----");
                            _logStarted = true;
                        }
                        w.WriteLine("{0} :: {1}", DateTime.Now.ToString(), s);
                        w.Flush();
                    }
                    catch
                    {
                        // just ignore inability to write logs
                    }
                }
            }
        }

        private static void ResetVJoyDeviceId()
        {
            _vjoyId = 0;
            if (_instance.cmbVJoyId.InvokeRequired)
            {
                // BeginInvoke sets up to run when can
                // this.Invoke() will block until after UI runs the code
                _instance.BeginInvoke((MethodInvoker)delegate
                {
                    ResetVJoyDeviceId();
                });
            }
            else
            {
                if (_instance.cmbVJoyId.SelectedIndex != 0) _instance.cmbVJoyId.SelectedIndex = 0;
            }
        }
        #endregion

        #region Controller
        static void ControllerHandler(vJoy joystick)
        {
            bool success;
            uint id;
            vJoy.JoystickState iReport = new vJoy.JoystickState();

            // wait for the form to finish loading
            while (!_shuttingDown && !_initialized) { Thread.Sleep(500); }

            while (!_shuttingDown)
            {
                success = false;
                id = _vjoyId;

                if (id == 0)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    // Get the state of the requested device
                    VjdStat status = joystick.GetVJDStatus(id);
                    switch (status)
                    {
                        case VjdStat.VJD_STAT_OWN:
                        case VjdStat.VJD_STAT_FREE:
                            if (!joystick.AcquireVJD(id))
                            {
                                UpdateStatus("Failed to acquire vJoy device number " + id.ToString());
                            }
                            else
                            {
                                Log("Acquired vJoy device number " + id.ToString());
                                success = true;
                            }
                            break;
                        case VjdStat.VJD_STAT_BUSY:
                            UpdateStatus("vJoy Device " + id.ToString() + " is already owned by another feeder\nCannot continue\n");
                            break;
                        case VjdStat.VJD_STAT_MISS:
                            UpdateStatus("vJoy Device " + id.ToString() + " is not installed or disabled\nCannot continue\n");
                            break;
                        default:
                            UpdateStatus("vJoy Device " + id.ToString() + " general error\nCannot continue\n");
                            break;
                    };
                    if (!success) ResetVJoyDeviceId();
                }

                if (success)
                {
                    // Check which axes are supported
                    _joystickSettings.AxisXExists = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
                    _joystickSettings.AxisYExists = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
                    _joystickSettings.AxisZExists = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
                    _joystickSettings.AxisRXExists = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
                    _joystickSettings.AxisRZExists = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);

                    // Get the number of buttons and POV Hat switchessupported by this vJoy device
                    _joystickSettings.NumberButtons = joystick.GetVJDButtonNumber(id);
                    _joystickSettings.NumberContinuousPovHats = joystick.GetVJDContPovNumber(id);
                    _joystickSettings.NumberDescretePovHats = joystick.GetVJDDiscPovNumber(id);

                    long maxValue = 0;
                    joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxValue);
                    _joystickSettings.MaxJoystickValue = (int)maxValue;

                    _joystickSettings.MinHapticRange = Convert.ToInt32(-_joystickSettings.MaxJoystickValue * _hapticZonePercent);
                    _joystickSettings.MaxHapticRange = Convert.ToInt32(_joystickSettings.MaxJoystickValue * (1.0 + _hapticZonePercent));

                    // Verify joystick configuration
                    if (_joystickSettings.NumberButtons < 18)
                    {
                        UpdateStatus("vJoy Device requires 18 buttons. Device " + _vjoyId + " only has " + _joystickSettings.NumberButtons + " buttons");
                        success = false;
                    }
                    else if (!_joystickSettings.AxisXExists)
                    {
                        UpdateStatus("vJoy Device requires an X axis");
                        success = false;
                    }
                    else if (!_joystickSettings.AxisYExists)
                    {
                        UpdateStatus("vJoy Device requires an Y axis");
                        success = false;
                    }
                    else if (!_joystickSettings.AxisZExists)
                    {
                        UpdateStatus("vJoy Device requires an Z axis");
                        success = false;
                    }
                    else if (!_joystickSettings.AxisRZExists)
                    {
                        UpdateStatus("vJoy Device requires an ZRotation axis");
                        success = false;
                    }

                    if (success)
                    {
                        UpdateStatus("Setting configuration...", false);

                        JoyStickLockedPrimary.Reset(_joystickSettings.MaxJoystickValue / 2);
                        JoyStickLockedSecondary.Reset(0);

                       // _controlPanel.Setup((int)_joystickSettings.MaxJoystickValue, _joystickSettings.NumberContinuousPovHats);

                        UpdateStatus("Ready", false);

                        while (!_shuttingDown && (_vjoyId == id))
                        {
                            DateTime startTime = DateTime.Now;
                            ReadFromViveController(id, ref iReport);

                            /*** Feed the driver with the position packet - is fails then wait for input then try to re-acquire device ***/
                            if (!joystick.UpdateVJD(id, ref iReport))
                            {
                                bool ready = false;
                                UpdateStatus("vJoy device " + id.ToString() + " disconnected, reaquiring...");
                                for (int cnt = 0; cnt < 100 && !ready; cnt++)
                                {
                                    System.Threading.Thread.Sleep(50); VjdStat status;
                                    status = joystick.GetVJDStatus(id);
                                    // Acquire the target
                                    ready = ((status != VjdStat.VJD_STAT_OWN) && (status != VjdStat.VJD_STAT_UNKN) && ((status != VjdStat.VJD_STAT_FREE) || (joystick.AcquireVJD(id))));
                                }
                                if (ready) UpdateStatus("Ready");
                                else
                                {
                                    UpdateStatus("Unable to reaquire vJoy device.");
                                    ResetVJoyDeviceId(); // threadsafe reset the combobox
                                }
                            }

                            Application.DoEvents();
                            DoThreadSleep();
                        }
                    }
                    if (!success) ResetVJoyDeviceId();
                }
            }
        }

        class JoyStickPosition
        {
            public float X = 0;
            public float Y = 0;
            public float Z = 0;
            public float XR = 0;
            public float ZR = 0;
            private float ResetValue = 0;
            public void Reset(int value) { ResetValue = value; X = Y = Z = XR = ZR = ResetValue; }
            public void Reset(float value) { ResetValue = value; X = Y = Z = XR = ZR = ResetValue; }
            public void Reset() { X = Y = Z = XR = ZR = ResetValue; }
            public void ResetRotation() { XR = ZR = ResetValue; }
        }
        static JoyStickPosition JoyStickLockedPrimary = new JoyStickPosition();
        static JoyStickPosition JoyStickLockedSecondary = new JoyStickPosition();

        [Serializable()]
        public class ButtonMap
        {
            public enum HatDir { UP, DOWN, LEFT, RIGHT, CENTER };
            public enum JoyButton
            {
                NONE = -1,
                PRIMARY_TRIGGER_HAIR = 1,
                PRIMARY_TRIGGER,
                PRIMARY_MENU,
                PRIMARY_GRIP,
                PRIMARY_UP,
                PRIMARY_RIGHT,
                PRIMARY_DOWN,
                PRIMARY_LEFT,
                PRIMARY_CENTER,
                SECONDARY_TRIGGER_HAIR,
                SECONDARY_TRIGGER,
                SECONDARY_MENU,
                SECONDARY_GRIP,
                SECONDARY_UP,
                SECONDARY_RIGHT,
                SECONDARY_DOWN,
                SECONDARY_LEFT,
                SECONDARY_CENTER,
            }

            public enum SpecialButton
            {
                NONE,
                CENTER_JOYSTICK,
                CENTER_RUDDER,
                THROTTLE_ZERO,
                THROTTLE_HALF,
                THROTTLE_MAX,
                JOYSTICK_ENABLE,                
                YAW_ENABLE,
                PITCH_ENABLE,
                ROLL_ENABLE,
                // Descrete hats
                HAT_1,
                HAT_2,
                // Analog hats
                HAT_12,
                HAT_34
            }

            public string Name { get; set; } = "Undefined";

            [Serializable()]
            public class HatButtons
            {
                private Dictionary<HatDir, JoyButton> JoyMap = new Dictionary<HatDir, JoyButton>();
                private Dictionary<HatDir, SpecialButton> SpecialMap = new Dictionary<HatDir, SpecialButton>();
                private Dictionary<HatDir, JoyButton> DefaultJoyMap = new Dictionary<HatDir, JoyButton>();

                public HatButtons()
                {
                    foreach (HatDir dir in Enum.GetValues(typeof(HatDir)))
                    {
                        JoyMap[dir] = JoyButton.NONE;
                        SpecialMap[dir] = SpecialButton.NONE;
                    }
                }

                /// <summary>
                /// Used at initialization time to setup the hat
                /// </summary>
                /// <param name="dir"></param>
                /// <param name="jbutton"></param>
                public void SetupHatButton(HatDir dir, JoyButton jbutton = JoyButton.NONE)
                {
                    DefaultJoyMap[dir] = jbutton;
                    JoyMap[dir] = jbutton;
                    SpecialMap[dir] = SpecialButton.NONE;
                }

                //public void SetHatButton(HatDir dir, SpecialButton sbutton = SpecialButton.NONE)
                //{
                //    JoyMap[dir] = JoyButton.NONE;
                //    SpecialMap[dir] = sbutton;
                //}

                public JoyButton GetJoyButton(HatDir dir)
                {
                    return JoyMap[dir];
                }

                public SpecialButton GetSpecialButton(HatDir dir)
                {
                    return SpecialMap[dir];
                }

                public void SetButton(HatDir dir, JoyButton btn)
                {
                    JoyMap[dir] = btn;
                    SpecialMap[dir] = SpecialButton.NONE;
                }

                public void SetButton(HatDir dir, SpecialButton btn)
                {
                    SpecialMap[dir] = btn;
                    JoyMap[dir] = JoyButton.NONE;
                }

                public void SetToDefalt(HatDir dir)
                {
                    JoyMap[dir] = DefaultJoyMap[dir];
                }
            }

            private JoyButton _buttonBit = JoyButton.NONE;
            private SpecialButton _buttonSpecial = SpecialButton.NONE;

            public JoyButton ButtonBit { get { return _buttonBit; } }
            public SpecialButton ButtonSpecial { get { return _buttonSpecial; } }
            public JoyButton DefaultJoyButton { get; } = JoyButton.NONE;
            public SpecialButton DefaultSpecialButton { get; } = SpecialButton.NONE;
            public HatButtons HatButton { get; } = null;

            /// <summary>
            /// Constructor for all button except touchpad
            /// </summary>
            /// <param name="buttonBit"></param>
            public ButtonMap(JoyButton buttonBit = JoyButton.NONE)
            {
                DefaultJoyButton = buttonBit;
                SetButtonMap(buttonBit);
            }
            
            /// <summary>
            /// Contructor for touchpad 
            /// </summary>
            /// <param name="specialButton">should be hat1 or hat2</param>
            public ButtonMap(SpecialButton specialButton = SpecialButton.NONE)
            {
                HatButton = new HatButtons();
                DefaultSpecialButton = specialButton;
                SetButtonMap(specialButton);
            }

            public void SetButtonMap(JoyButton buttonBit)
            {
                _buttonBit = buttonBit;
                _buttonSpecial = SpecialButton.NONE;
            }
            public void SetButtonMap(SpecialButton specialButton)
            {
                _buttonBit = JoyButton.NONE;
                _buttonSpecial = specialButton;
            }
        }
        class JoyStickSettings
        {
            public int NumberContinuousPovHats = 0;
            public int NumberDescretePovHats = 0;
            public int NumberButtons = 0;
            public int MaxJoystickValue = 1;
            public int MinHapticRange = 0;
            public int MaxHapticRange = 1;
            public bool AxisXExists = false;
            public bool AxisYExists = false;
            public bool AxisZExists = false;
            public bool AxisRXExists = false;
            public bool AxisRZExists = false;
        };
        private static JoyStickSettings _joystickSettings = new JoyStickSettings();

        private static void FillInViveButtonMap(string configName, Dictionary<ViveButtons,ButtonMap> primaryMap, Dictionary<ViveButtons, ButtonMap> secondaryMap)
        {
            _hatIsButtons[0] = _hatIsButtons[1] = true;

            // PRIMARY DEFAULTS
            primaryMap[ViveButtons.GRIP] = new ButtonMap(ButtonMap.JoyButton.PRIMARY_GRIP);
            primaryMap[ViveButtons.APP_MENU] = new ButtonMap(ButtonMap.JoyButton.PRIMARY_MENU);
            primaryMap[ViveButtons.TRIGGER] = new ButtonMap(ButtonMap.JoyButton.PRIMARY_TRIGGER);
            primaryMap[ViveButtons.HAIR_TRIGGER] = new ButtonMap(ButtonMap.JoyButton.PRIMARY_TRIGGER_HAIR);
            primaryMap[ViveButtons.TOUCHPAD] = new ButtonMap(ButtonMap.SpecialButton.HAT_1);
            if (_hatIsButtons[0])
            {
                primaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.UP, ButtonMap.JoyButton.PRIMARY_UP);
                primaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.RIGHT, ButtonMap.JoyButton.PRIMARY_RIGHT);
                primaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.DOWN, ButtonMap.JoyButton.PRIMARY_DOWN);
                primaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.LEFT, ButtonMap.JoyButton.PRIMARY_LEFT);
                primaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.CENTER, ButtonMap.JoyButton.PRIMARY_CENTER);
            }

            // SECONDARY DEFAULTS
            secondaryMap[ViveButtons.GRIP] = new ButtonMap(ButtonMap.JoyButton.SECONDARY_GRIP);
            secondaryMap[ViveButtons.APP_MENU] = new ButtonMap(ButtonMap.JoyButton.SECONDARY_MENU);
            secondaryMap[ViveButtons.TRIGGER] = new ButtonMap(ButtonMap.JoyButton.SECONDARY_TRIGGER);
            secondaryMap[ViveButtons.HAIR_TRIGGER] = new ButtonMap(ButtonMap.JoyButton.SECONDARY_TRIGGER_HAIR);
            secondaryMap[ViveButtons.TOUCHPAD] = new ButtonMap(ButtonMap.SpecialButton.HAT_2);
            if (_hatIsButtons[1])
            {
                secondaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.UP, ButtonMap.JoyButton.SECONDARY_UP);
                secondaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.RIGHT, ButtonMap.JoyButton.SECONDARY_RIGHT);
                secondaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.DOWN, ButtonMap.JoyButton.SECONDARY_DOWN);
                secondaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.LEFT, ButtonMap.JoyButton.SECONDARY_LEFT);
                secondaryMap[ViveButtons.TOUCHPAD].HatButton.SetupHatButton(ButtonMap.HatDir.CENTER, ButtonMap.JoyButton.SECONDARY_CENTER);
            }

            // button names
            primaryMap[ViveButtons.GRIP].Name = "Grip Button";
            primaryMap[ViveButtons.APP_MENU].Name = "Application Button";
            primaryMap[ViveButtons.TRIGGER].Name = "Trigger Bottom";
            primaryMap[ViveButtons.HAIR_TRIGGER].Name = "Trigger";
            primaryMap[ViveButtons.TOUCHPAD].Name = "Touchpad";
            secondaryMap[ViveButtons.GRIP].Name = "Grip Button";
            secondaryMap[ViveButtons.APP_MENU].Name = "Application Button";
            secondaryMap[ViveButtons.TRIGGER].Name = "Trigger Bottom";
            secondaryMap[ViveButtons.HAIR_TRIGGER].Name = "Trigger";
            secondaryMap[ViveButtons.TOUCHPAD].Name = "Touchpad";

            // Default config
            primaryMap[ViveButtons.GRIP].SetButtonMap(ButtonMap.SpecialButton.JOYSTICK_ENABLE);
            secondaryMap[ViveButtons.GRIP].SetButtonMap(ButtonMap.SpecialButton.JOYSTICK_ENABLE);
        }

        private static void ReadFromViveController(uint id, ref vJoy.JoystickState iReport)
        {
            uint[] povCont = new uint[MAX_NUM_HATS];
            byte[] pov = new byte[MAX_NUM_HATS];
            pov[0] = pov[1] = pov[2] = pov[3] = 0xFF; // neutral position
            povCont[0] = povCont[1] = povCont[2] = povCont[3] = 0xFFFFFFFF; // neutral position

            iReport.AxisX = Convert.ToInt32(JoyStickLockedPrimary.X);
            iReport.AxisY = Convert.ToInt32(JoyStickLockedPrimary.Y);
            iReport.AxisZ = Convert.ToInt32(JoyStickLockedSecondary.Z);
            iReport.AxisXRot = Convert.ToInt32(JoyStickLockedPrimary.XR);
            iReport.AxisZRot = Convert.ToInt32(JoyStickLockedPrimary.ZR);
            iReport.Buttons = 0;
            if (_joystickSettings.NumberContinuousPovHats > 0)
            {
                iReport.bHats = 0xFFFFFFFF; // Neutral state
                iReport.bHatsEx1 = 0xFFFFFFFF; // Neutral state
                iReport.bHatsEx2 = 0xFFFFFFFF; // Neutral state
                iReport.bHatsEx3 = 0xFFFFFFFF; // Neutral state
            }
            else
            {
                iReport.bHats = 0xFFFFFFFF; // Neutral state
            }


            // udpate the values
            SteamVR_Controller.Update();

            // right Controller
            int rightIndex = (int)OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (rightIndex >= 0)
            {
                ProcessButtons(true, rightIndex, ref iReport);
            }

            // left Controller
            int leftIndex = (int)OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (leftIndex >= 0)
            {
                ProcessButtons(false, leftIndex, ref iReport);
            }

            // fill in the report
            iReport.bDevice = (byte)id;

            //Debug.Print(iReport.AxisX.ToString() + "," + iReport.AxisY.ToString() + "  " + iReport.AxisZRot.ToString());
        }

        static private ButtonMap.HatDir GetHatDirection(SteamVR_Controller.Device device)
        {
            ButtonMap.HatDir dir = ButtonMap.HatDir.CENTER;
            Vector2 touchpadPos = device.GetAxis();
            if (_joystickSettings.NumberContinuousPovHats == 0)
            {
                // Descrete touchpads - 0=up, 1=right, 2=down, 3=left, 4=up
                if ((Math.Abs(touchpadPos.X) < _touchpadCenterMargin) && (Math.Abs(touchpadPos.Y) < _touchpadCenterMargin)) dir = ButtonMap.HatDir.CENTER;
                else if ((touchpadPos.Y > 0) && (Math.Abs(touchpadPos.X) <= touchpadPos.Y)) dir = ButtonMap.HatDir.UP;
                else if ((touchpadPos.Y < 0) && (Math.Abs(touchpadPos.X) < -touchpadPos.Y)) dir = ButtonMap.HatDir.DOWN;
                else if ((touchpadPos.X > 0) && (Math.Abs(touchpadPos.Y) <= touchpadPos.X)) dir = ButtonMap.HatDir.RIGHT;
                else if ((touchpadPos.X < 0) && (Math.Abs(touchpadPos.Y) < -touchpadPos.X)) dir = ButtonMap.HatDir.LEFT;
            }
            else Assert("Not implemented: continuous hats");

            return dir;
        }

        static private void ProcessButtons(bool isRightHand, int controllerIndex, ref vJoy.JoystickState iReport)
        {
            //string debugPrint = "";
            //string formatF = "+0.000;-0.000";
            int joystickCenter = (int)(_joystickSettings.MaxJoystickValue / 2);
            List<ButtonMap.SpecialButton> speicalButtons = new List<ButtonMap.SpecialButton>();

            SteamVR_Controller.Device device = SteamVR_Controller.Input(controllerIndex);
            if (device.valid && device.connected)
            {
                bool gripping = false;
                bool gripReleased = false;
                bool isPrimary = CurrentConfiguration.RightHanded == isRightHand;
                bool pitchEnabled = false;
                bool yawEnabled = false;
                bool rollEnabled = false;

                // handle primary vs secondary variables - read only
                Dictionary<ViveButtons, bool> buttons = isPrimary ? _buttonsPrimary : _buttonsSecondary;
                bool gripTypeToggle = isPrimary ? CurrentConfiguration.GripTypeTogglePrimary : CurrentConfiguration.GripTypeToggleSecondary;
                ReleaseAction actionOnRelease = isPrimary ? actionOnReleasePrimary : actionOnReleaseSecondary;
                // handle primary vs secondary variables - modifiable
                SteamVR_Utils.RigidTransform gripDownRT = isPrimary ? _gripDownRTPrimary : _gripDownRTSecondary;
                bool grip = isPrimary ? _gripPrimary : _gripSecondary;
                bool gripHolding = isPrimary ? _gripHoldingPrimary : _gripHoldingSecondary;
                Dictionary<ViveButtons, ButtonMap> buttonMapping = isPrimary ? CurrentConfiguration.PrimaryMap : CurrentConfiguration.SecondaryMap;


                // Read Position
                HmdMatrix34_t pose = device.GetPose().mDeviceToAbsoluteTracking;
                SteamVR_Utils.RigidTransform rt = new SteamVR_Utils.RigidTransform(pose);
                //debugPrint += " P: " + rt.pos.X.ToString(formatF) + "," + rt.pos.Y.ToString(formatF) + "," + rt.pos.Z.ToString(formatF);
                //debugPrint += "  Rot: " + rt.rot.X.ToString(formatF) + "," + rt.rot.Y.ToString(formatF) + "," + rt.rot.Z.ToString(formatF);
                //Debug.Print(debugPrint);

                // Read Buttons
                //debugPrint += "  Buttons: ";
                foreach (ViveButtons button in Enum.GetValues(typeof(ViveButtons)))
                {
                    bool buttonDown;
                    if (button == ViveButtons.HAIR_TRIGGER) buttonDown = device.GetHairTrigger();
                    else buttonDown = device.GetPress((EVRButtonId)button);

                    buttons[button] = buttonDown;
                    //debugPrint += buttons[button] ? "1" : "0";
                    ButtonMap buttonMap = buttonMapping[button];
                    if (buttonMap.ButtonBit != ButtonMap.JoyButton.NONE)
                    {
                        if (buttonDown)
                        {
                            iReport.Buttons |= (uint)1 << ((int)buttonMap.ButtonBit - 1);
                        }
                    }
                    else if (buttonMap.ButtonSpecial != ButtonMap.SpecialButton.NONE)
                    {
                        speicalButtons.Clear();
                        // handle the hats mapping speically since they can add multiple buttons to the list
                        if ((buttonMap.ButtonSpecial == ButtonMap.SpecialButton.HAT_1) ||
                            (buttonMap.ButtonSpecial == ButtonMap.SpecialButton.HAT_2))
                        {

                            if (buttonDown)
                            {
                                ButtonMap.HatDir dir = GetHatDirection(device);
                                ButtonMap.SpecialButton sbutton = buttonMap.HatButton.GetSpecialButton(dir);
                                if (sbutton != ButtonMap.SpecialButton.NONE)
                                {
                                    speicalButtons.Add(sbutton);
                                }
                                ButtonMap.JoyButton jbutton = buttonMap.HatButton.GetJoyButton(dir);
                                if (jbutton != ButtonMap.JoyButton.NONE)
                                {
                                    iReport.Buttons |= (uint)1 << ((int)jbutton - 1);
                                }
                            }
                        }
                        else speicalButtons.Add(buttonMap.ButtonSpecial);

                        foreach (ButtonMap.SpecialButton sbutton in speicalButtons)
                        {
                            switch (sbutton)

                            {
                                case ButtonMap.SpecialButton.YAW_ENABLE:
                                    if (buttonDown)                                                                           
                                        yawEnabled = true;                                       
                                       
                                    break;

                                case ButtonMap.SpecialButton.PITCH_ENABLE:
                                    if (buttonDown)                                    
                                        pitchEnabled = true;                                       
                                    break;

                                case ButtonMap.SpecialButton.ROLL_ENABLE:
                                    if (buttonDown)
                                        rollEnabled = true;
                                    break;



                                case ButtonMap.SpecialButton.CENTER_JOYSTICK:
                                    if (buttonDown)
                                    {
                                        JoyStickLockedPrimary.Reset();
                                        if (isPrimary)
                                        {
                                            iReport.AxisX = joystickCenter;
                                            iReport.AxisY = joystickCenter;
                                            iReport.AxisXRot = joystickCenter;
                                            iReport.AxisZRot = joystickCenter;
                                            gripDownRT = rt;
                                        }
                                        else Assert("Not implemented: center joystick from throttle controller");
                                    }
                                    break;

                                case ButtonMap.SpecialButton.CENTER_RUDDER:
                                    if (buttonDown)
                                    {
                                        JoyStickLockedPrimary.ResetRotation();
                                        if (isPrimary)
                                        {
                                            iReport.AxisXRot = joystickCenter;
                                            iReport.AxisZRot = joystickCenter;
                                        }
                                        else Assert("Not implemented: center rudder from throttle controller");
                                    }
                                    break;

                                case ButtonMap.SpecialButton.THROTTLE_ZERO:
                                    if (buttonDown)
                                    {
                                        if (!isPrimary)
                                        {
                                            iReport.AxisZ = (_invertZAxis ? _joystickSettings.MaxJoystickValue : 0);
                                            JoyStickLockedSecondary.Reset(iReport.AxisZ);
                                            gripDownRT = rt;
                                        }
                                        else Assert("Not implemented: zero throttle from joystick controller");
                                    }
                                    break;
                                case ButtonMap.SpecialButton.THROTTLE_HALF:
                                    if (buttonDown)
                                    {
                                        if (!isPrimary)
                                        {
                                            iReport.AxisZ = joystickCenter;
                                            JoyStickLockedSecondary.Reset(joystickCenter);
                                            gripDownRT = rt;
                                        }
                                        else Assert("Not implemented: center throttle from joystick controller");
                                    }
                                    break;
                                case ButtonMap.SpecialButton.THROTTLE_MAX:
                                    if (buttonDown)
                                    {
                                        if (!isPrimary)
                                        {
                                            iReport.AxisZ = (_invertZAxis ? 0 : _joystickSettings.MaxJoystickValue);
                                            JoyStickLockedSecondary.Reset(iReport.AxisZ);
                                            gripDownRT = rt;
                                        }
                                        else Assert("Not implemented: full throttle from joystick controller");
                                    }
                                    break;

                                case ButtonMap.SpecialButton.JOYSTICK_ENABLE:
                                    gripping = buttonDown;
                                    if (grip != gripping)
                                    {
                                        bool gripChanged = true;
                                        if (!gripTypeToggle) gripHolding = gripping;
                                        else if (gripping) gripHolding = !gripHolding;
                                        else gripChanged = false;

                                        if (gripChanged && gripHolding)
                                        {
                                            gripDownRT = rt;
                                        }
                                        else if (gripChanged && !gripHolding)
                                        {
                                            gripReleased = true;
                                        }
                                        grip = gripping;
                                    }
                                    break;

                                case ButtonMap.SpecialButton.HAT_1:
                                case ButtonMap.SpecialButton.HAT_2:                             
                                    if (buttonDown)
                                    {
                                        ButtonMap.HatDir dir = GetHatDirection(device);
                                        if (_joystickSettings.NumberContinuousPovHats == 0)
                                        {
                                            // Descrete touchpads - 0=up, 1=right, 2=down, 3=left, 4=up
                                            byte pov = 0;
                                            if (dir == ButtonMap.HatDir.UP) pov = 0;
                                            if (dir == ButtonMap.HatDir.DOWN) pov = 2;
                                            if (dir == ButtonMap.HatDir.RIGHT) pov = 1;
                                            if (dir == ButtonMap.HatDir.LEFT) pov = 3;
                                            switch (buttonMap.ButtonSpecial)
                                            {
                                                case ButtonMap.SpecialButton.HAT_1: iReport.bHats &= 0xfffffff0; iReport.bHats |= (uint)pov; break;
                                                case ButtonMap.SpecialButton.HAT_2: iReport.bHats &= 0xffffff0f; iReport.bHats |= (uint)pov << 4; break;                                               
                                            }
                                        }
                                        else
                                        {
                                            Assert("Descrete Hat connot be assigned to a continous hat");
                                        }

                                    }
                                    break;
                                case ButtonMap.SpecialButton.HAT_12:
                                case ButtonMap.SpecialButton.HAT_34:
                                    if (buttonDown)
                                    {
                                        Vector2 touchpadPos = device.GetAxis();
                                        if (_joystickSettings.NumberContinuousPovHats > 0)
                                        {
                                            // Continuous touchpads
                                            const uint STEP_SIZE = 35901; // per vjoy documentation
                                            uint halfStep = STEP_SIZE / 2;
                                            uint povX = Convert.ToUInt32(touchpadPos.X * halfStep + halfStep);
                                            uint povY = Convert.ToUInt32(touchpadPos.Y * halfStep + halfStep);
                                            switch (buttonMap.ButtonSpecial)
                                            {
                                                case ButtonMap.SpecialButton.HAT_12: iReport.bHats = povX; iReport.bHatsEx1 = povY; break;
                                                case ButtonMap.SpecialButton.HAT_34: iReport.bHatsEx2 = povX; iReport.bHatsEx3 = povY; break;
                                            }
                                        }
                                        else
                                        {
                                            Assert("Continous Hat connot be assigned to a descrete hat");
                                        }

                                    }
                                    break;
                                default:
                                    Assert("Unhandled case");
                                    break;
                            }
                        }
                    }
                }
                //Debug.Print(debugPrint);
                // Trigger touch
                //bool touchingTrigger = device.GetTouch(EVRButtonId.k_EButton_SteamVR_Trigger);
                //if (touchingTrigger) debugPrint += " t";

                // Process joystick motions
                if (gripHolding || gripReleased)
                {

                    // grip is down, report motion
                    SteamVR_Utils.Transform tstart = new SteamVR_Utils.Transform();
                    SteamVR_Utils.Transform tend = new SteamVR_Utils.Transform();
                    tstart.rotation = gripDownRT.rot;
                    tstart.position = gripDownRT.pos;
                    tend.rotation = rt.rot;
                    tend.position = rt.pos;
                    SteamVR_Utils.RigidTransform rtLocal = new SteamVR_Utils.RigidTransform(tstart, tend);

                    if (isPrimary)
                    {
                        // PRIMARY = JOYSTICK
                        Vector3 angle = SteamVR_Utils.AngleFromQ2(rtLocal.rot);
                        //debugPrint += " Pos: " + rtLocal.pos.X.ToString(formatF) + "," + rtLocal.pos.Y.ToString(formatF) + "," + rtLocal.pos.Z.ToString(formatF);
                        //debugPrint += " S: " + tstart.rotation.X.ToString(formatF) + "," + tstart.rotation.Y.ToString(formatF) + ", " + tstart.rotation.Z.ToString(formatF) + " E: " + tend.rotation.X.ToString(formatF) + "," + tend.rotation.Y.ToString(formatF) + ", " + tend.rotation.Z.ToString(formatF);
                        //debugPrint += " Angle: " + angle.ToString();
                        //Debug.Print(debugPrint);
                       
                        // convert angle to +/-180
                        while (angle.X < -180) angle.X += 360;
                        while (angle.X > 180) angle.X -= 360;
                        while (angle.Y < -180) angle.Y += 360;
                        while (angle.Y > 180) angle.Y -= 360;
                        while (angle.Z < -180) angle.Z += 360;
                        while (angle.Z > 180) angle.Z -= 360;

                        // controller angle is 0 to 360, joystick is 0 to maxJoystickValue
                        // but we can't really move the controller the full range without twising a wrist, so use 4x the range
                        // swap x and y (x angle is y axis)




                       

                        // enforce boundaries
                        //int AxisX = Convert.ToInt32((_invertXAxis ? -1 : 1) * angle.Y / _RotationFullScaleDegrees * joystickCenter + JoyStickLockedPrimary.X);
                        //AxisX = Math.Max(Math.Min(iReport.AxisX, _joystickSettings.MaxJoystickValue), 0);

                        //if (yawEnabled)
                        //{
                            iReport.AxisX = Convert.ToInt32((_invertXAxis ? -1 : 1) * angle.Y / _RotationFullScaleDegrees * joystickCenter + JoyStickLockedPrimary.X);
                         
                        //}
                       
                       
                           
                        
                    
                       // if (pitchEnabled)
                        //{
                            if (CurrentConfiguration.ControlTypeJoystick)
                                iReport.AxisY = Convert.ToInt32((_invertYAxis ? -1 : 1) * angle.X / _RotationFullScaleDegrees * joystickCenter + JoyStickLockedPrimary.Y);
                            else
                                iReport.AxisY = Convert.ToInt32((_invertYAxis ? 1 : -1) * rtLocal.pos.Y / _MotionFullScaleZ * joystickCenter + JoyStickLockedPrimary.Y);
                           
                            //JoyStickLockedPrimary.Y = iReport.AxisY;
                          
                        //}

                       // if (rollEnabled)
                       // {
                            iReport.AxisZRot = Convert.ToInt32((_invertZRAxis ? -1 : 1) * angle.Z / _RotationFullScaleDegrees * joystickCenter + JoyStickLockedPrimary.ZR);
                           
                        //JoyStickLockedPrimary.ZR = iReport.AxisZRot;

                        //  }

                        //// Shake if outsize range
                        if (gripHolding && (
                            (iReport.AxisX < _joystickSettings.MinHapticRange) ||
                            (iReport.AxisX > _joystickSettings.MaxHapticRange) ||
                            (iReport.AxisY < _joystickSettings.MinHapticRange) ||
                            (iReport.AxisY > _joystickSettings.MaxHapticRange) ||
                            (iReport.AxisZRot < _joystickSettings.MinHapticRange) ||
                            (iReport.AxisZRot > _joystickSettings.MaxHapticRange)))
                        {
                            VibrateController(device, 1000);
                        }
                        iReport.AxisX = Math.Max(Math.Min(iReport.AxisX, _joystickSettings.MaxJoystickValue), 0);
                        iReport.AxisY = Math.Max(Math.Min(iReport.AxisY, _joystickSettings.MaxJoystickValue), 0);
                        iReport.AxisZRot = Math.Max(Math.Min(iReport.AxisZRot, _joystickSettings.MaxJoystickValue), 0);
                    }
                    else
                    {
                        // SECONDARY = THROTTLE
                        // throttle is simply the z axis position 
                        iReport.AxisZ = Convert.ToInt32((_invertZAxis ? 1 : -1) * rtLocal.pos.Y / _MotionFullScaleZ * joystickCenter + JoyStickLockedSecondary.Z);
                        iReport.AxisXRot = Convert.ToInt32((_invertZAxis ? 1 : -1) * rtLocal.pos.X / _MotionFullScaleZ * joystickCenter + JoyStickLockedSecondary.XR);
                        // Shake if outsize range
                        if (gripHolding && (
                            (iReport.AxisZ < _joystickSettings.MinHapticRange) ||
                            (iReport.AxisZ > _joystickSettings.MaxHapticRange)))
                        {
                            VibrateController(device, 50);
                        }

                        // enforce boundaries
                        iReport.AxisZ = Math.Max(Math.Min(iReport.AxisZ, _joystickSettings.MaxJoystickValue), 0);

                        //debugPrint += " SPos: " + rtLocal.pos.X.ToString(formatF) + "," + rtLocal.pos.Y.ToString(formatF) + "," + rtLocal.pos.Z.ToString(formatF);
                        //Debug.Print(debugPrint);

                       
                        iReport.AxisXRot = Math.Max(Math.Min(iReport.AxisXRot, _joystickSettings.MaxJoystickValue), 0);
                    }

                    if (gripReleased)
                    {
                        switch (actionOnRelease)
                        {
                            case ReleaseAction.CENTER_JOYSTICK:
                                JoyStickLockedPrimary.Reset();
                                break;
                            case ReleaseAction.CENTER_RUDDER:
                                JoyStickLockedPrimary.ResetRotation();
                                break;                           
                            case ReleaseAction.CENTER_TROTTLE:                          
                                Assert("Not implemented");
                                break;
                            case ReleaseAction.NONE:
                            default:
                                // hold the position
                                if (isPrimary)
                                {
                                    JoyStickLockedPrimary.X = iReport.AxisX;
                                    JoyStickLockedPrimary.Y = iReport.AxisY;
                                    JoyStickLockedPrimary.ZR = iReport.AxisZRot;
                                }
                                else
                                {
                                    JoyStickLockedSecondary.Z = iReport.AxisZ;
                                }
                                break;
                        }
                    }
                }

                // handle primary vs secondary variables - modifiable
                if (isPrimary)
                {
                    _gripDownRTPrimary = gripDownRT;
                    _gripPrimary = grip;
                    _gripHoldingPrimary = gripHolding;
                }
                else
                {
                    _gripDownRTSecondary = gripDownRT;
                    _gripSecondary = grip;
                    _gripHoldingSecondary = gripHolding;
                }
            }
        }

        static private void VibrateController(SteamVR_Controller.Device device, int strengthPercent)
        {
            int loops = Convert.ToInt32(_pollCycleMSec * 1000 / _hapticPulseUSec * strengthPercent / 100);
            _haptics[device] = loops * strengthPercent / 100;
        }

        static Dictionary<SteamVR_Controller.Device, int> _haptics = new Dictionary<SteamVR_Controller.Device, int>(); // device and pulses needed
        static private void DoThreadSleep()
        {
            if (_haptics.Count > 0)
            {
                int loops = Convert.ToInt32(_pollCycleMSec * 1000 / _hapticPulseUSec);
                for (int i = 0; i < loops; i++)
                {
                    foreach (KeyValuePair<SteamVR_Controller.Device, int> haptic in _haptics)
                    {
                        if (i < haptic.Value) haptic.Key.TriggerHapticPulse(_hapticPulseUSec);
                    }
                }
                _haptics.Clear();
            }
            else
            {
                System.Threading.Thread.Sleep(_pollCycleMSec);
            }
        }

        private bool VjoyPathIsValid() { return SetVjoyPathIfValid(_vjoyPath); }

        /// <summary>
        /// check path for validity, if valid will set _vjoyPath and return true
        /// </summary>
        /// <param name="vjPath">path to check</param>
        /// <returns>true if selected path is valid</returns>
        private bool SetVjoyPathIfValid(string vjPath)
        {
            bool pathValid = false;
            if (File.Exists(vjPath)) vjPath = Path.GetDirectoryName(vjPath);
            if (!string.IsNullOrWhiteSpace(vjPath) && Directory.Exists(vjPath) && File.Exists(vjPath + "\\" + _vjoyMonitorBin))
            {
                if (vjPath != _vjoyPath)
                {
                    _vjoyPath = vjPath;
                    SaveConfig();
                }
                pathValid = true;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(_vjoyDefaultPath) && Directory.Exists(_vjoyDefaultPath) && File.Exists(_vjoyDefaultPath + "\\" + _vjoyMonitorBin))
                {
                    if (_vjoyPath != _vjoyDefaultPath)
                    {
                        _vjoyPath = _vjoyDefaultPath;
                        SaveConfig();
                    }
                    pathValid = true;
                }
            }
            return pathValid;
        }
        
        private void PromptForVjoyPath()
        {
            bool isValid = false;

            // ask user to find vjoy executables
            while (!isValid)
            {
                openFileDialog1.InitialDirectory = "c:\\program files";
                openFileDialog1.FileName = _vjoyMonitorBin;
                openFileDialog1.Multiselect = false;
                openFileDialog1.Filter = "VJoy Monitor|" + _vjoyMonitorBin;
                openFileDialog1.RestoreDirectory = true;
                if (DialogResult.OK == openFileDialog1.ShowDialog())
                {
                    isValid = SetVjoyPathIfValid(openFileDialog1.FileName);
                }
                else isValid = true; // not reall valid, but we exit the loop

                if (!isValid) MessageBox.Show("That path is not valid.  Find the path to " + _vjoyMonitorBin + "\nUsually something like C:\\Program Files\\VJoy\\x64", "Invalid path");
            }
        }
        #endregion

        #region Form Controls
        private void menuItemViewLog_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_logFileName);
        }

      

        private void cmbVJoyId_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = cmbVJoyId.SelectedItem.ToString();
            // cmbVJoyId.Tag holds previous index so we don't do anything unless it's changed
            if (!item.Equals(cmbVJoyId.Tag))
            {
                // cmbVJoyId.SelectedIndex <= 0 will return 0, otherwise just return cmbVJoyId.SelectedIndex
                if (cmbVJoyId.SelectedIndex > 0)
                {
                    uint.TryParse(item, out _vjoyId);
                }
                else ResetVJoyDeviceId();

                if (_vjoyId > 0) System.Configuration.ConfigurationManager.AppSettings[_configKeyVjoyId] = cmbVJoyId.SelectedItem.ToString();

                cmbVJoyId.Tag = item;
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            _configPanel.Show();
        }

        private void btnVjoyConfig_Click(object sender, EventArgs e)
        {
            if (!VjoyPathIsValid()) PromptForVjoyPath();
            if (VjoyPathIsValid())
            {
                // launch vjoy config
                Process[] runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_vjoyConfigBin));
                if (runningProcess.Length == 0)
                {
                    Process.Start(_vjoyPath + "\\" + _vjoyConfigBin);
                }
                else
                {
                    WindowHelper.BringProcessToFront(runningProcess[0]);
                }
            }
        }

        private void btnVjoyMonitor_Click(object sender, EventArgs e)
        {
            if (!VjoyPathIsValid()) PromptForVjoyPath();
            if (VjoyPathIsValid())
            {
                // launch vjoy monitor
                Process[] runningProcess = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_vjoyMonitorBin));
                if (runningProcess.Length == 0)
                {
                    Process.Start(_vjoyPath + "\\" + _vjoyMonitorBin);
                }
                else
                {
                    WindowHelper.BringProcessToFront(runningProcess[0]);
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if ((_configFileName != null) && File.Exists(_configFileName))
            {
                File.Delete(_configFileName);
            }
        }
        #endregion

        public static class WindowHelper
        {
            public static void BringProcessToFront(Process process)
            {
                IntPtr handle = process.MainWindowHandle;
                if (IsIconic(handle))
                {
                    ShowWindow(handle, SW_RESTORE);
                }

                SetForegroundWindow(handle);
            }

            const int SW_RESTORE = 9;

            [System.Runtime.InteropServices.DllImport("User32.dll")]
            private static extern bool SetForegroundWindow(IntPtr handle);
            [System.Runtime.InteropServices.DllImport("User32.dll")]
            private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
            [System.Runtime.InteropServices.DllImport("User32.dll")]
            private static extern bool IsIconic(IntPtr handle);
        }
    }
}
