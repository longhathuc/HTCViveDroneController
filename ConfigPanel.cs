﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ViveButtons = HTCViveDroneController.HTCViveDroneController.ViveButtons;
using ButtonMap = HTCViveDroneController.HTCViveDroneController.ButtonMap;
using System.Diagnostics;

namespace HTCViveDroneController
{
    public partial class ConfigPanel : Form
    {
        #region Variables
        class ConfigItem
        {
            public bool IsPrimary { get; }
            public ViveButtons ViveButton { get; }
            public ButtonMap Map { get; set;  }

            public ConfigItem(bool isPrimary, ViveButtons buttons, ButtonMap map)
            {
                ViveButton = buttons;
                Map = map;
                IsPrimary = isPrimary;
            }
        }

        class ConfigHatItem
        {
            public ConfigItem BaseItem { get; }
            public ButtonMap.HatDir Dir { get; }

            public ConfigHatItem(ConfigItem baseItem, ButtonMap.HatDir dir)
            {
                BaseItem = baseItem;
                Dir = dir;
            }
        }

        private Dictionary<ComboBox, ConfigItem> _configItems = new Dictionary<ComboBox, ConfigItem>();
        private Dictionary<ComboBox, ConfigHatItem> _configHatItems = new Dictionary<ComboBox, ConfigHatItem>();
        private List<Control> _primaryHatDirControls = new List<Control>();
        private List<Control> _secondaryHatDirControls = new List<Control>();
        private bool _initialized = false;
        private int _cfgRowStart;
        private bool _changesMade = false;
        private const string _newConfigName = "New...";
        private const string _cancelText = "Cancel";
        private const string _backText = "Back";
        #endregion

        #region Form Init
        public ConfigPanel()
        {
            InitializeComponent();
            _cfgRowStart = tlpConfig.RowCount - 2;
        }

        private void ConfigPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Setup the form with all the controls to allow configuring the controllers
        /// </summary>
        /// <param name="primaryMap">button map for primary controller</param>
        /// <param name="secondaryMap">button map for secondary controller</param>
        /// <param name="primaryIsRight">is primary controller right hand?</param>
        public new void Show()
        {            
            Setup(HTCViveDroneController.CurrentConfiguration.Name);
            base.Show();
        }

        private void Setup(string configName)
        {
            const int columnPrimaryLbl = 1;
            const int columnPrimaryCmb = 2;
            const int columnSecondaryLbl = 4;
            const int columnSecondaryCmb = 5;

            _initialized = false;

            _changesMade = false;
            btnOK.Enabled = false;
            btnAccept.Enabled = false;
            btnCancel.Text = _backText;

            HTCViveDroneController.Configuration config = HTCViveDroneController.GetConfig(configName); // this can return a config with a different name, so configName isn't necessarily correct anymore
            configName = config.Name; 

            // setup config selector
            cmbConfigurations.Items.Clear();   
            foreach (HTCViveDroneController.Configuration c in HTCViveDroneController.Configurations)
            {
                cmbConfigurations.Items.Add(c.Name);
            }
            cmbConfigurations.Items.Add(_newConfigName);
            cmbConfigurations.SelectedItem = config.Name;

            //// remove all the button controls before setting them up again
            //foreach (Control ctrl in tlpConfig.Controls)
            //{
            //    int row = tlpConfig.GetPositionFromControl(ctrl).Row;
            //    if ((ctrl != flpButtons) && (row >= _cfgRowStart))
            //    {
            //        tlpConfig.Controls.Remove(ctrl);
            //        ctrl.Dispose();
            //    }
            //}
            //tlpConfig.SetRow(flpButtons, _cfgRowStart + 1);
            //tlpConfig.RowCount = _cfgRowStart + 2;
            //
            //_configItems.Clear();
            //_primaryHatDirControls.Clear();
            //_secondaryHatDirControls.Clear();

            cmbHandedness.SelectedIndex = config.RightHanded ? 0 : 1;
            cmbControlType.SelectedIndex = config.ControlTypeJoystick ? 0 : 1;
            cmbPrimaryEnableType.SelectedIndex = config.GripTypeTogglePrimary ? 1 : 0;
            cmbSecondaryEnableType.SelectedIndex = config.GripTypeToggleSecondary ? 1 : 0;

            if (_configItems.Count == 0)
            {
                // Primary button controls
                int rowIndex = _cfgRowStart;
                foreach (KeyValuePair<ViveButtons, ButtonMap> item in config.PrimaryMap)
                {
                    NewRow(rowIndex);
                    tlpConfig.Controls.Add(NewLabel(item.Value.Name), columnPrimaryLbl, rowIndex);
                    tlpConfig.Controls.Add(NewComboBox(item.Key, item.Value, true), columnPrimaryCmb, rowIndex);
                    rowIndex++;
                    if (item.Key == ViveButtons.TOUCHPAD)
                    {
                        ComboBox touchpadCmb = tlpConfig.GetControlFromPosition(columnPrimaryCmb, rowIndex - 1) as ComboBox;
                        foreach (ButtonMap.HatDir dir in Enum.GetValues(typeof(ButtonMap.HatDir)))
                        {
                            Label hatLabel = NewLabel(TitleCase(Enum.GetName(typeof(ButtonMap.HatDir), dir)));
                            ComboBox hatComboBox = NewComboBox(touchpadCmb, dir);
                            _primaryHatDirControls.Add(hatLabel);
                            _primaryHatDirControls.Add(hatComboBox);

                            NewRow(rowIndex);
                            tlpConfig.Controls.Add(hatLabel, columnPrimaryLbl, rowIndex);
                            tlpConfig.Controls.Add(hatComboBox, columnPrimaryCmb, rowIndex); // use menu button so it acts like regular button
                            rowIndex++;
                        }
                    }
                }

                // Secondary button controls
                rowIndex = _cfgRowStart;
                foreach (KeyValuePair<ViveButtons, ButtonMap> item in config.SecondaryMap)
                {
                    NewRow(rowIndex);
                    tlpConfig.Controls.Add(NewLabel(item.Value.Name), columnSecondaryLbl, rowIndex);
                    tlpConfig.Controls.Add(NewComboBox(item.Key, item.Value, false), columnSecondaryCmb, rowIndex);
                    rowIndex++;
                    if (item.Key == ViveButtons.TOUCHPAD)
                    {
                        ComboBox touchpadCmb = tlpConfig.GetControlFromPosition(columnSecondaryCmb, rowIndex - 1) as ComboBox;
                        foreach (ButtonMap.HatDir dir in Enum.GetValues(typeof(ButtonMap.HatDir)))
                        {
                            Label hatLabel = NewLabel(TitleCase(Enum.GetName(typeof(ButtonMap.HatDir), dir)));
                            ComboBox hatComboBox = NewComboBox(touchpadCmb, dir);
                            _secondaryHatDirControls.Add(hatLabel);
                            _secondaryHatDirControls.Add(hatComboBox);

                            NewRow(rowIndex);
                            tlpConfig.Controls.Add(hatLabel, columnSecondaryLbl, rowIndex);
                            tlpConfig.Controls.Add(hatComboBox, columnSecondaryCmb, rowIndex); // use menu button so it acts like regular button
                            rowIndex++;
                        }
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<ViveButtons, ButtonMap> cfg in config.PrimaryMap)
                {
                    ViveButtons cfgViveButtons = cfg.Key;
                    ButtonMap cfgButtonMap = cfg.Value;

                    foreach (KeyValuePair<ComboBox, ConfigItem> cboxItem in _configItems)
                    {
                        ConfigItem item = cboxItem.Value;
                        if (item.IsPrimary && item.ViveButton == cfgViveButtons)
                        {
                            item.Map = cfgButtonMap;
                            break;
                        }
                    }
                }
                foreach (KeyValuePair<ViveButtons, ButtonMap> cfg in config.SecondaryMap)
                {
                    ViveButtons cfgViveButtons = cfg.Key;
                    ButtonMap cfgButtonMap = cfg.Value;

                    foreach (KeyValuePair<ComboBox, ConfigItem> cboxItem in _configItems)
                    {
                        ConfigItem item = cboxItem.Value;
                        if (!item.IsPrimary && item.ViveButton == cfgViveButtons)
                        {
                            item.Map = cfgButtonMap;
                            break;
                        }
                    }
                }
            }
            _initialized = true;
            ResetConfig();
        }

        public void Reset()
        {
            ResetConfig();
        }
        #endregion

        #region Private Methods
        private void SaveConfig(string name)
        {
            HTCViveDroneController.Configuration config = HTCViveDroneController.GetConfig(name);

            // First Save to the main structure, then save the structure to disk
            config.RightHanded = cmbHandedness.SelectedIndex == 0;
            config.ControlTypeJoystick = cmbControlType.SelectedIndex == 0;
            config.GripTypeTogglePrimary = cmbPrimaryEnableType.SelectedIndex == 1;
            config.GripTypeToggleSecondary = cmbSecondaryEnableType.SelectedIndex == 1;

            foreach (KeyValuePair<ComboBox, ConfigItem> item in _configItems)
            {
                bool isPrimary = item.Value.IsPrimary;

                ButtonMap btnMap = item.Value.Map;
                ButtonMap.JoyButton defaultJoy = btnMap.DefaultJoyButton;
                ButtonMap.SpecialButton defaultSpecial = btnMap.DefaultSpeicalButton;

                switch ((ComboBoxItems)item.Key.SelectedIndex)
                {
                    case ComboBoxItems.CENTER:
                        btnMap.SetButtonMap(ButtonMap.SpecialButton.CENTER_JOYSTICK);
                        break;
                    case ComboBoxItems.CENTER_RUDDER:
                        btnMap.SetButtonMap(ButtonMap.SpecialButton.CENTER_RUDDER);
                        break;
                    case ComboBoxItems.THROTTLE_OFF:
                        btnMap.SetButtonMap(ButtonMap.SpecialButton.THROTTLE_ZERO);
                        break;
                    case ComboBoxItems.THROTTLE_MID:
                        btnMap.SetButtonMap(ButtonMap.SpecialButton.THROTTLE_HALF);
                        break;
                    case ComboBoxItems.THROTTLE_MAX:
                        btnMap.SetButtonMap(ButtonMap.SpecialButton.THROTTLE_MAX);
                        break;
                    case ComboBoxItems.DHAT:
                        btnMap.SetButtonMap(isPrimary ? ButtonMap.SpecialButton.HAT_1 : ButtonMap.SpecialButton.HAT_2);
                        ResetHatButtons(btnMap.HatButton, isPrimary);
                        break;
                    case ComboBoxItems.AHAT:
                        btnMap.SetButtonMap(isPrimary ? ButtonMap.SpecialButton.HAT_12 : ButtonMap.SpecialButton.HAT_34);
                        ResetHatButtons(btnMap.HatButton, isPrimary);
                        break;
                    case ComboBoxItems.ENABLE:
                        btnMap.SetButtonMap(ButtonMap.SpecialButton.JOYSTICK_ENABLE);
                        break;
                    case ComboBoxItems.DEFAULT:
                    default:
                        if (defaultJoy != ButtonMap.JoyButton.NONE) btnMap.SetButtonMap(defaultJoy);
                        else btnMap.SetButtonMap(defaultSpecial);
                        break;
                }
            }

            foreach (KeyValuePair<ComboBox, ConfigHatItem> item in _configHatItems)
            {
                ButtonMap btnMap = item.Value.BaseItem.Map;
                if ((btnMap.ButtonSpecial == ButtonMap.SpecialButton.HAT_1) || (btnMap.ButtonSpecial == ButtonMap.SpecialButton.HAT_2))
                {
                    bool isPrimary = item.Value.BaseItem.IsPrimary;
                    ButtonMap.HatButtons hat = btnMap.HatButton;
                    ButtonMap.HatDir dir = item.Value.Dir;

                    switch ((ComboBoxItems)item.Key.SelectedIndex)
                    {
                        case ComboBoxItems.CENTER:
                            hat.SetButton(dir, ButtonMap.SpecialButton.CENTER_JOYSTICK);
                            break;
                        case ComboBoxItems.CENTER_RUDDER:
                            hat.SetButton(dir, ButtonMap.SpecialButton.CENTER_RUDDER);
                            break;
                        case ComboBoxItems.THROTTLE_OFF:
                            hat.SetButton(dir, ButtonMap.SpecialButton.THROTTLE_ZERO);
                            break;
                        case ComboBoxItems.THROTTLE_MID:
                            hat.SetButton(dir, ButtonMap.SpecialButton.THROTTLE_HALF);
                            break;
                        case ComboBoxItems.THROTTLE_MAX:
                            hat.SetButton(dir, ButtonMap.SpecialButton.THROTTLE_MAX);
                            break;
                        case ComboBoxItems.DEFAULT:
                        default:
                            hat.SetToDefalt(dir);
                            break;
                    }
                }
            }
            
            // Save to disk
            HTCViveDroneController.SaveConfig();
        }

        private void ResetConfig()
        {
            _initialized = false;
            cmbHandedness.SelectedIndex = HTCViveDroneController.CurrentConfiguration.RightHanded ? 0 : 1;
            cmbControlType.SelectedIndex = HTCViveDroneController.CurrentConfiguration.ControlTypeJoystick ? 0 : 1;
            cmbPrimaryEnableType.SelectedIndex = HTCViveDroneController.CurrentConfiguration.GripTypeTogglePrimary ? 1 : 0;
            cmbSecondaryEnableType.SelectedIndex = HTCViveDroneController.CurrentConfiguration.GripTypeToggleSecondary ? 1 : 0;

            foreach (KeyValuePair<ComboBox, ConfigItem> item in _configItems)
            {
                bool isPrimary = item.Value.IsPrimary;
                ComboBoxItems resetValue = ComboBoxItems.DEFAULT;
                ButtonMap btnMap = item.Value.Map;

                if (btnMap.ButtonBit == ButtonMap.JoyButton.NONE)
                {
                    switch (btnMap.ButtonSpecial)
                    {
                        case ButtonMap.SpecialButton.CENTER_JOYSTICK: resetValue = ComboBoxItems.CENTER; break;
                        case ButtonMap.SpecialButton.CENTER_RUDDER: resetValue = ComboBoxItems.CENTER_RUDDER; break;
                        case ButtonMap.SpecialButton.THROTTLE_ZERO: resetValue = ComboBoxItems.THROTTLE_OFF; break;
                        case ButtonMap.SpecialButton.THROTTLE_HALF: resetValue = ComboBoxItems.THROTTLE_MID; break;
                        case ButtonMap.SpecialButton.THROTTLE_MAX: resetValue = ComboBoxItems.THROTTLE_MAX; break;
                        case ButtonMap.SpecialButton.HAT_1: resetValue = ComboBoxItems.DHAT; break;
                        case ButtonMap.SpecialButton.HAT_2: resetValue = ComboBoxItems.DHAT; break;
                        case ButtonMap.SpecialButton.HAT_12: resetValue = ComboBoxItems.AHAT; break;
                        case ButtonMap.SpecialButton.HAT_34: resetValue = ComboBoxItems.AHAT; break;
                        case ButtonMap.SpecialButton.JOYSTICK_ENABLE: resetValue = ComboBoxItems.ENABLE; break;
                    }
                }
                item.Key.SelectedIndex = (int)resetValue;
            }

            foreach (KeyValuePair<ComboBox, ConfigHatItem> item in _configHatItems)
            {
                bool isPrimary = item.Value.BaseItem.IsPrimary;
                ComboBoxItems resetValue = ComboBoxItems.DEFAULT;
                ButtonMap btnMap = item.Value.BaseItem.Map;
                ButtonMap.HatButtons hat = btnMap.HatButton;
                ButtonMap.HatDir dir = item.Value.Dir;
                ButtonMap.SpecialButton btn = hat.GetSpecialButton(dir);

                if ( btn != ButtonMap.SpecialButton.NONE)
                {
                    switch (btn)
                    {
                        case ButtonMap.SpecialButton.CENTER_JOYSTICK: resetValue = ComboBoxItems.CENTER; break;
                        case ButtonMap.SpecialButton.CENTER_RUDDER: resetValue = ComboBoxItems.CENTER_RUDDER; break;
                        case ButtonMap.SpecialButton.THROTTLE_ZERO: resetValue = ComboBoxItems.THROTTLE_OFF; break;
                        case ButtonMap.SpecialButton.THROTTLE_HALF: resetValue = ComboBoxItems.THROTTLE_MID; break;
                        case ButtonMap.SpecialButton.THROTTLE_MAX: resetValue = ComboBoxItems.THROTTLE_MAX; break;
                        case ButtonMap.SpecialButton.JOYSTICK_ENABLE: resetValue = ComboBoxItems.ENABLE; break;
                    }
                }

                item.Key.SelectedIndex = (int)resetValue;
            }
            SetFormClientSize();
            _initialized = true;
        }

        private void ResetHatButtons(ButtonMap.HatButtons hat, bool isPrimary)
        {
            if (hat != null)
            {
                hat.SetupHatButton(ButtonMap.HatDir.UP, isPrimary ? ButtonMap.JoyButton.PRIMARY_UP : ButtonMap.JoyButton.SECONDARY_UP);
                hat.SetupHatButton(ButtonMap.HatDir.DOWN, isPrimary ? ButtonMap.JoyButton.PRIMARY_DOWN : ButtonMap.JoyButton.SECONDARY_DOWN);
                hat.SetupHatButton(ButtonMap.HatDir.LEFT, isPrimary ? ButtonMap.JoyButton.PRIMARY_LEFT : ButtonMap.JoyButton.SECONDARY_LEFT);
                hat.SetupHatButton(ButtonMap.HatDir.RIGHT, isPrimary ? ButtonMap.JoyButton.PRIMARY_RIGHT : ButtonMap.JoyButton.SECONDARY_RIGHT);
            }
        }

        private string TitleCase(string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
        }

        private void SetFormClientSize()
        {
            // setup the form size
            //int cmbRowHeights = (cmbHandedness.Height + cmbHandedness.Margin.Top + cmbHandedness.Margin.Bottom) * (tlpConfig.RowCount - 3 - _hatRowsHidden);
            //int buttonRowHeight = btnAccept.Height + btnAccept.Margin.Top + btnAccept.Margin.Bottom + flpButtons.Margin.Top + flpButtons.Margin.Bottom;
            //this.ClientSize = new Size(this.ClientSize.Width, (cmbRowHeights + buttonRowHeight) * 100 / 100);

            float rowHeights = 0;
            TableLayoutRowStyleCollection styles = tlpConfig.RowStyles;
            foreach (RowStyle style in styles)
            {
                rowHeights += style.Height;
            }
            this.ClientSize = new Size(this.ClientSize.Width, Convert.ToInt32(rowHeights));
        }

        /// <summary>
        /// Add new rows to the table layout panel such that we insert the given row and keep the current bottom row
        /// </summary>
        /// <param name="rowIndex">the row index to add a row at</param>
        private void NewRow(int rowIndex)
        {
            if (rowIndex < tlpConfig.RowCount )
            {
                tlpConfig.RowCount++;
                tlpConfig.RowStyles.Insert(rowIndex, new RowStyle(SizeType.AutoSize));
            }

            // move the buttons to the bottom
            tlpConfig.SetRow(flpButtons, tlpConfig.RowCount - 1);
        }

        /// <summary>
        /// Create a new label for a configuration item
        /// </summary>
        /// <param name="text">text to show on the label</param>
        /// <param name="header">is this a header item</param>
        /// <returns>a new label</returns>
        private Label NewLabel(string text)
        {
            Label lbl = new Label
            {
                Text = text,
                Visible = true,
                AutoSize = true,
                Anchor = AnchorStyles.Right,

                Font = lblHandedness.Font.Clone() as Font
            };

            return lbl;
        }

        private enum ComboBoxItems { DEFAULT=0, CENTER, CENTER_RUDDER, THROTTLE_OFF, THROTTLE_MID, THROTTLE_MAX, ENABLE, DHAT, AHAT, MAX_ENUM }

        /// <summary>
        /// Get the string for combo box options
        /// </summary>
        /// <param name="item">the item to get string for</param>
        /// <param name="isPrimary">is this for the primary controller?</param>
        /// <returns>a string representation of the item</returns>
        private string ComboBoxItemString(ComboBoxItems item, bool isPrimary = true)
        {
            string result = "unknown";
            switch (item)
            {
                case ComboBoxItems.DEFAULT:      result = "Default"; break;
                case ComboBoxItems.CENTER:       result = "Center Joystick"; break;
                case ComboBoxItems.CENTER_RUDDER:result = "Center Rudder"; break;
                case ComboBoxItems.THROTTLE_OFF: result = "Throttle Off"; break;
                case ComboBoxItems.THROTTLE_MID: result = "Throttle Half"; break;
                case ComboBoxItems.THROTTLE_MAX: result = "Throttle Full"; break;
                case ComboBoxItems.DHAT:         result = "Button Hat"; break;
                case ComboBoxItems.AHAT:         result = "Analog Hat"; break;
                case ComboBoxItems.ENABLE:       result = isPrimary ? "Joystick Enable" : "Throttle Enable"; break;
            }
            return result;
        }

        /// <summary>
        /// Create the base for a new combo box - only used by NewComboBox() variants
        /// </summary>
        /// <param name="isPrimary">is this for the primary controller?</param>
        /// <returns>a new combo box</returns>
        private ComboBox NewComboBoxBase(bool isPrimary)
        {
            ComboBox cmb = new ComboBox
            {
                Visible = true,
                Dock = DockStyle.Fill
            };

            for (ComboBoxItems i = ComboBoxItems.DEFAULT; i <= ComboBoxItems.ENABLE; i++)
            {
                cmb.Items.Add(ComboBoxItemString(i, isPrimary));
            }
            cmb.SelectedIndexChanged += new System.EventHandler(Cmb_SelectedIndexChanged);
            return cmb;
        }

        /// <summary>
        /// Create new combo box for vive buttons
        /// </summary>
        /// <param name="buttonType">which vive button</param>
        /// <param name="buttonMap">structure holding infor on button mapping</param>
        /// <param name="isPrimary">is this for the primary controller?</param>
        /// <returns>a new combo box</returns>
        private ComboBox NewComboBox(ViveButtons buttonType, ButtonMap buttonMap, bool isPrimary)
        {
            ComboBox cmb = NewComboBoxBase(isPrimary);
            if (buttonType == ViveButtons.TOUCHPAD)
            {
                for (ComboBoxItems i = ComboBoxItems.ENABLE + 1; i < ComboBoxItems.MAX_ENUM; i++)
                {
                    //Diable Analog Hat for now
                    if (i != ComboBoxItems.AHAT) cmb.Items.Add(ComboBoxItemString(i));
                }
            }
            if (buttonType == ViveButtons.GRIP) cmb.SelectedIndex = (int)ComboBoxItems.ENABLE;
            else cmb.SelectedIndex = (int)ComboBoxItems.DEFAULT;

            cmb.Font = cmbHandedness.Font.Clone() as Font;

            _configItems.Add(cmb, new ConfigItem(isPrimary, buttonType, buttonMap));
            return cmb;
        }

        /// <summary>
        /// Create new combo box for touchpad directional controls
        /// </summary>
        /// <param name="touchpadCmb">the combobox for the base touchpad</param>
        /// <param name="dir">the direction (up/down/left/right)</param>
        /// <returns>a new combo box</returns>
        private ComboBox NewComboBox(ComboBox touchpadCmb, ButtonMap.HatDir dir)
        {
            ConfigItem baseItem = _configItems[touchpadCmb];
            ComboBox cmb = NewComboBoxBase(baseItem.IsPrimary);
            cmb.SelectedIndex = (int)ComboBoxItems.DEFAULT;
            _configHatItems.Add(cmb, new ConfigHatItem(baseItem, dir));
            return cmb;
        }
        #endregion

        #region Form Controls

        private void btnAccept_Click(object sender, EventArgs e)
        {
            _changesMade = false;
            btnOK.Enabled = false;
            btnAccept.Enabled = false;
            btnCancel.Text = _backText;

            string name = cmbConfigurations.SelectedItem.ToString();
            SaveConfig(name);
            HTCViveDroneController.SetCurrentConfig(name);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            btnAccept_Click(sender, e);
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Cmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cbox = sender as ComboBox;
            if (_initialized)
            {
                // cmbConfigurations.Tag holds the selected index before the change
                if (!cbox.SelectedItem.Equals(cbox.Tag))
                {
                    _changesMade = true;
                    btnOK.Enabled = true;
                    btnAccept.Enabled = true;
                    btnCancel.Text = _cancelText;
                }
            }
            cbox.Tag = cbox.SelectedItem;
        }

        private void cmbConfigurations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initialized)
            {
                // cmbConfigurations.Tag holds the selected index before the change
                if (!cmbConfigurations.SelectedItem.Equals(cmbConfigurations.Tag))
                {
                    bool loadNewConfig = true;
                    if (_changesMade)
                    {
                        DialogResult result = MessageBox.Show("Changes were made.  Accept changes?", "Save Changes", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            SaveConfig(cmbConfigurations.Tag.ToString());
                        }

                        if (result == DialogResult.Cancel)
                        {
                            cmbConfigurations.SelectedItem = cmbConfigurations.Tag; // set config selector back to what it was previously
                            loadNewConfig = false;
                        }
                    } 

                    if (_newConfigName.Equals(cmbConfigurations.SelectedItem.ToString()))
                    {
                        bool success = false;

                        StringRequester sr = new StringRequester("Enter the new configuration name");

                        while (!success)
                        {
                            DialogResult result = sr.ShowDialog(this);
                            if (result == DialogResult.OK)
                            {
                                string name = sr.Response;
                                if (HTCViveDroneController.ConfigExists(name))
                                {
                                    MessageBox.Show("Configuration with that name already exists, choose another.");
                                }
                                else
                                {
                                    Setup(name); // load the new configuration
                                    success = true;
                                }
                            }
                            else
                            {
                                cmbConfigurations.SelectedItem = cmbConfigurations.Tag; // set config selector back to what it was previously
                                success = true;
                            }
                        }
                    }
                    else if (loadNewConfig) Setup(cmbConfigurations.SelectedItem.ToString()); // load the new configuration
                }
            }
            cmbConfigurations.Tag = cmbConfigurations.SelectedItem;
        }
        #endregion


    }
}