using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using static TouchTest.MouseOperations;
//using Microsoft.VisualStudio.TestTools.UITesting;
using SimWinInput;
using System.Windows;

namespace TouchTest
{
    public partial class Form1 : Form
    {
        public System.Windows.Point SavedPoint;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg,
        IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint",
            CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(System.Windows.Point point);

        private const int BM_CLICK = 0x00F5;




        public event EventHandler<System.Windows.Input.TouchEventArgs> TouchDown;
        public Form1()
        {
            InitializeComponent();
            this.TouchDown += MainWindow_TouchEvent;
            //DoMouseClick();
            MousePoint foo = MouseOperations.GetCursorPosition();
            SavedPoint.X = foo.X;
            SavedPoint.Y = foo.Y;
        }


        private void MainWindow_TouchEvent(object sender, TouchEventArgs e)
        {
            var point = e.GetTouchPoint(sender as IInputElement).Position;
            MessageBox.Show($"X: {point.X}, Y: {point.Y}");
        }

        private void button1_Click(object sender, EventArgs e)
        {


            //click
            //var screenPoint = this.PointToScreen(SavedPoint);
            //// Get a handle
            //var handle = WindowFromPoint(screenPoint);
            //// Send the click message
            //if (handle != IntPtr.Zero)
            //{
            //    SendMessage(handle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
            //}


            //click
            //var screenPoint = this.PointToScreen(new Point(button2.Left,
            //button2.Top));
            //// Get a handle
            //var handle = WindowFromPoint(screenPoint);
            //// Send the click message
            //if (handle != IntPtr.Zero)
            //{
            //    SendMessage(handle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
            //}

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hi", "There");
        }

        public void ClickOnPoint(IntPtr wndHandle, System.Windows.Point clientPoint)
        {
            //var cur = Cursor.Current;
            //var oldPos = Cursor.Position;

            ///// get screen coordinates
            //ClientToScreen(wndHandle, ref clientPoint);

            ///// set cursor on coords, and press mouse
            //Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            //var inputMouseDown = new INPUT();
            //inputMouseDown.Type = 0; /// input type mouse
            //inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            //var inputMouseUp = new INPUT();
            //inputMouseUp.Type = 0; /// input type mouse
            //inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

            //var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            //SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            ///// return mouse 
            //Cursor.Position = oldPos;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MousePoint pos = new MousePoint(-1362, 311);
            MouseEvent(MouseEventFlags.LeftDown, pos);
            MouseEvent(MouseEventFlags.LeftUp, pos);
            ClickOnPoint(this.Handle, SavedPoint);
            
            //MouseOperations.MouseEvent(MouseEventFlags.LeftDown);
            //MouseOperations.MouseEvent(MouseEventFlags.LeftUp);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //SimMouse.Click(MouseButtons.Left, SavedPoint.X, SavedPoint.Y);
        }
    }
}
