using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vJoyInterfaceWrap;

namespace VJoyVive
{
    public partial class ControlPanel : Form
    {
        static double ScaleValue;
        static int ContPovNumber;
        static int MaxValue;

        public ControlPanel()
        {
            InitializeComponent();
            MaxValue = 1;
            ContPovNumber = 0;

            // ensure the panels are square
            panelXYSpace.Height = panelXYSpace.Width;
            panelXY.Height = panelXY.Width;

            ScaleValue = (panelXYSpace.Width - panelXY.Width) * 1.0 / MaxValue;
        }

        private void ControlPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void Setup(int maxValue, int contPovNumber)
        {
            MaxValue = maxValue;
            ContPovNumber = contPovNumber;

            ScaleValue = (panelXYSpace.Width - panelXY.Width) * 1.0 / MaxValue;
        }

        public void Update(vJoy.JoystickState joystickState)
        {
            if (panelXY.InvokeRequired)
            {
                panelXY.Invoke(new MethodInvoker(() => { Update(joystickState); }));
            }
            else
            {
                // use (maxValue - value) on vertical display (Y joystick and throttle) since display is 0 at top and max at the bottom
                panelXY.Location = new Point(Convert.ToInt32(joystickState.AxisX * ScaleValue), Convert.ToInt32((MaxValue - joystickState.AxisY) * ScaleValue));
                panelZ.Location = new Point(panelZ.Left, Convert.ToInt32((MaxValue - joystickState.AxisZ) * ScaleValue));
                panelRot.Location = new Point(Convert.ToInt32(joystickState.AxisZRot * ScaleValue), panelRot.Top);
            }
        }
    }
}
