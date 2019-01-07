/////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// This project demonstrates how to write a simple vJoy feeder in C#
//
// You can compile it with either #define ROBUST OR #define EFFICIENT
// The fuctionality is similar - 
// The ROBUST section demonstrate the usage of functions that are easy and safe to use but are less efficient
// The EFFICIENT ection demonstrate the usage of functions that are more efficient
//
// Functionality:
//	The program starts with creating one joystick object. 
//	Then it petches the device id from the command-line and makes sure that it is within range
//	After testing that the driver is enabled it gets information about the driver
//	Gets information about the specified virtual device
//	This feeder uses only a few axes. It checks their existence and 
//	checks the number of buttons and POV Hat switches.
//	Then the feeder acquires the virtual device
//	Here starts and endless loop that feedes data into the virtual device
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////
//#define ROBUST
//#define EFFICIENT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Valve.VR;

// Don't forget to add this
using vJoyInterfaceWrap;

namespace HTCViveDroneController
{
    class Program
    {
        static Mutex mutex = new Mutex(true, "VjoyVive");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // check if an instance of the application is already running
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new HTCViveDroneController(args));
            }
            else
            {
                MessageBox.Show("Application already running...");
            }
        }
    }
}
