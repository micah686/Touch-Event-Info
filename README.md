# Touch-Event-Info

Function used for testing button clicks
### C#
```cs
private static Random random = new Random();
public static string RandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
      .Select(s => s[random.Next(s.Length)]).ToArray());
}
```

### VB

```vb
Private Shared random As Random = New Random()
    Public Shared Function RandomString(ByVal length As Integer) As String
        Const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
        Return New String(Enumerable.Repeat(chars, length).[Select](Function(s) s(random.Next(s.Length))).ToArray())
    End Function
```


# C#

Sample way of sending mouse clicks without moving the mouse
```cs
public class MouseSimulator
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public SendInputEventType type;
        public MouseKeybdhardwareInputUnion mkhi;
    }
    [StructLayout(LayoutKind.Explicit)]
    struct MouseKeybdhardwareInputUnion
    {
        [FieldOffset(0)]
        public MouseInputData mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }
    struct MouseInputData
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public MouseEventFlags dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    [Flags]
    enum MouseEventFlags : uint
    {
        MOUSEEVENTF_MOVE = 0x0001,
        MOUSEEVENTF_LEFTDOWN = 0x0002,
        MOUSEEVENTF_LEFTUP = 0x0004,
        MOUSEEVENTF_RIGHTDOWN = 0x0008,
        MOUSEEVENTF_RIGHTUP = 0x0010,
        MOUSEEVENTF_MIDDLEDOWN = 0x0020,
        MOUSEEVENTF_MIDDLEUP = 0x0040,
        MOUSEEVENTF_XDOWN = 0x0080,
        MOUSEEVENTF_XUP = 0x0100,
        MOUSEEVENTF_WHEEL = 0x0800,
        MOUSEEVENTF_VIRTUALDESK = 0x4000,
        MOUSEEVENTF_ABSOLUTE = 0x8000
    }
    enum SendInputEventType : int
    {
        InputMouse,
        InputKeyboard,
        InputHardware
    }

    public static void MoveMouseButton(int x, int y)
    {
        INPUT mouseMoveInput = new INPUT();
        mouseMoveInput.type = SendInputEventType.InputMouse;
        mouseMoveInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE| MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
        mouseMoveInput.mkhi.mi.dx = x;  
        mouseMoveInput.mkhi.mi.dy = y;  
        SendInput(1, ref mouseMoveInput, Marshal.SizeOf(new INPUT()));
    }
}
```


Prevent focus, but allow interaction
```cs
private const int WS_EX_NOACTIVATE = 0x08000000;
private const int GWL_EXSTYLE = -20;

[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
private static extern int GetWindowLong(IntPtr hwnd, int index);

[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

...

var window = new Window();
window.SourceInitialized += (s, e) => {
    var interopHelper = new WindowInteropHelper(window);
    int exStyle = GetWindowLong(interopHelper.Handle, GWL_EXSTYLE);
    SetWindowLong(interopHelper.Handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
};
window.Show();
```


DIsable conversion of Touch events to mouse events
```cs
using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// As long as this object exists all mouse events created from a touch event for legacy support will be disabled.
    /// </summary>
    class DisableTouchConversionToMouse : IDisposable
    {
        static readonly LowLevelMouseProc hookCallback = HookCallback;
        static IntPtr hookId = IntPtr.Zero;

        public DisableTouchConversionToMouse()
        {
            hookId = SetHook(hookCallback);
        }

        static IntPtr SetHook(LowLevelMouseProc proc)
        {
            var moduleHandle = UnsafeNativeMethods.GetModuleHandle(null);

            var setHookResult = UnsafeNativeMethods.SetWindowsHookEx(WH_MOUSE_LL, proc, moduleHandle, 0);
            if (setHookResult == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            return setHookResult;
        }

        delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var info = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                var extraInfo = (uint)info.dwExtraInfo.ToInt32();
                if ((extraInfo & MOUSEEVENTF_FROMTOUCH) == MOUSEEVENTF_FROMTOUCH)
                {
                    return new IntPtr(1);
                }
            }

            return UnsafeNativeMethods.CallNextHookEx(hookId, nCode, wParam, lParam);
        }
        
        bool disposed;

        public void Dispose()
        {
            if (disposed) return;

            UnsafeNativeMethods.UnhookWindowsHookEx(hookId);
            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~DisableTouchConversionToMouse()
        {
            Dispose();
        }

        #region Interop

        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable FieldCanBeMadeReadOnly.Local

        const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;
        const int WH_MOUSE_LL = 14;

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {

            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [SuppressUnmanagedCodeSecurity]
        static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod,
                uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);
        }

        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }
```

WindowsSDK7 WMTouch Example (https://github.com/pauldotknopf/WindowsSDK7-Samples/tree/master/Touch/MTScratchpadWMTouch/CS)
```cs
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Permissions;

namespace Microsoft.Samples.Touch.MTScratchpadWMTouch
{
    // Base class for multi-touch aware form.
    // Receives touch notifications through Windows messages and converts them
    // to touch events Touchdown, Touchup and Touchmove.
    public class WMTouchForm : Form
    {
        ///////////////////////////////////////////////////////////////////////
        // Public interface

        // Constructor
        [SecurityPermission(SecurityAction.Demand)]
        public WMTouchForm()
        {
            // Setup handlers
            Load += new System.EventHandler(this.OnLoadHandler);

            // GetTouchInputInfo needs to be
            // passed the size of the structure it will be filling.
            // We get the size upfront so it can be used later.
            touchInputSize = Marshal.SizeOf(new TOUCHINPUT());
        }

        ///////////////////////////////////////////////////////////////////////
        // Protected members, for derived classes.

        // Touch event handlers
        protected event EventHandler<WMTouchEventArgs> Touchdown;   // touch down event handler
        protected event EventHandler<WMTouchEventArgs> Touchup;     // touch up event handler
        protected event EventHandler<WMTouchEventArgs> TouchMove;   // touch move event handler

        // EventArgs passed to Touch handlers
        protected class WMTouchEventArgs : System.EventArgs
        {
            // Private data members
            private int x;                  // touch x client coordinate in pixels
            private int y;                  // touch y client coordinate in pixels
            private int id;                 // contact ID
            private int mask;               // mask which fields in the structure are valid
            private int flags;              // flags
            private int time;               // touch event time
            private int contactX;           // x size of the contact area in pixels
            private int contactY;           // y size of the contact area in pixels

            // Access to data members
            public int LocationX
            {
                get { return x; }
                set { x = value; }
            }
            public int LocationY
            {
                get { return y; }
                set { y = value; }
            }
            public int Id
            {
                get { return id; }
                set { id = value; }
            }
            public int Flags
            {
                get { return flags; }
                set { flags = value; }
            }
            public int Mask
            {
                get { return mask; }
                set { mask = value; }
            }
            public int Time
            {
                get { return time; }
                set { time = value; }
            }
            public int ContactX
            {
                get { return contactX; }
                set { contactX = value; }
            }
            public int ContactY
            {
                get { return contactY; }
                set { contactY = value; }
            }
            public bool IsPrimaryContact
            {
                get { return (flags & TOUCHEVENTF_PRIMARY) != 0; }
            }

            // Constructor
            public WMTouchEventArgs()
            {
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Private class definitions, structures, attributes and native
        // functions

        // Multitouch/Touch glue (from winuser.h file)
        // Since the managed layer between C# and WinAPI functions does not 
        // exist at the moment for multi-touch related functions this part of 
        // the code is required to replicate definitions from winuser.h file.

        // Touch event window message constants [winuser.h]
        private const int WM_TOUCH = 0x0240;

        // Touch event flags ((TOUCHINPUT.dwFlags) [winuser.h]
        private const int TOUCHEVENTF_MOVE = 0x0001;
        private const int TOUCHEVENTF_DOWN = 0x0002;
        private const int TOUCHEVENTF_UP = 0x0004;
        private const int TOUCHEVENTF_INRANGE = 0x0008;
        private const int TOUCHEVENTF_PRIMARY = 0x0010;
        private const int TOUCHEVENTF_NOCOALESCE = 0x0020;
        private const int TOUCHEVENTF_PEN = 0x0040;

        // Touch input mask values (TOUCHINPUT.dwMask) [winuser.h]
        private const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 0x0001; // the dwTime field contains a system generated value
        private const int TOUCHINPUTMASKF_EXTRAINFO = 0x0002; // the dwExtraInfo field is valid
        private const int TOUCHINPUTMASKF_CONTACTAREA = 0x0004; // the cxContact and cyContact fields are valid

        // Touch API defined structures [winuser.h]
        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public System.IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public System.IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTS
        {
            public short x;
            public short y;
        }

        // Currently touch/multitouch access is done through unmanaged code
        // We must p/invoke into user32 [winuser.h]
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterTouchWindow(System.IntPtr hWnd, ulong ulFlags);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTouchInputInfo(System.IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern void CloseTouchInputHandle(System.IntPtr lParam);

        // Attributes
        private int touchInputSize;        // size of TOUCHINPUT structure

        ///////////////////////////////////////////////////////////////////////
        // Private methods

        // OnLoad window event handler: Registers the form for multi-touch input.
        // in:
        //      sender      object that has sent the event
        //      e           event arguments
        private void OnLoadHandler(Object sender, EventArgs e)
        {
            try
            {
                // Registering the window for multi-touch, using the default settings.
                // p/invoking into user32.dll
                if (!RegisterTouchWindow(this.Handle, 0))
                {
                    Debug.Print("ERROR: Could not register window for multi-touch");
                }
            }
            catch (Exception exception)
            {
                Debug.Print("ERROR: RegisterTouchWindow API not available");
                Debug.Print(exception.ToString());
                MessageBox.Show("RegisterTouchWindow API not available", "MTScratchpadWMTouch ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
            }
        }

        // Window procedure. Receives WM_ messages.
        // Translates WM_TOUCH window messages to touch events.
        // Normally, touch events are sufficient for a derived class,
        // but the window procedure can be overriden, if needed.
        // in:
        //      m       message
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            // Decode and handle WM_TOUCH message.
            bool handled;
            switch (m.Msg)
            {
                case WM_TOUCH:
                    handled = DecodeTouch(ref m);
                    break;
                default:
                    handled = false;
                    break;
            }

            // Call parent WndProc for default message processing.
            base.WndProc(ref m);

            if (handled)
            {
                // Acknowledge event if handled.
                m.Result = new System.IntPtr(1);
            }
        }

        // Extracts lower 16-bit word from an 32-bit int.
        // in:
        //      number      int
        // returns:
        //      lower word
        private static int LoWord(int number)
        {
            return (number & 0xffff);
        }

        // Decodes and handles WM_TOUCH message.
        // Unpacks message arguments and invokes appropriate touch events.
        // in:
        //      m           window message
        // returns:
        //      whether the message has been handled
        private bool DecodeTouch(ref Message m)
        {
            // More than one touchinput may be associated with a touch message,
            // so an array is needed to get all event information.
            int inputCount = LoWord(m.WParam.ToInt32()); // Number of touch inputs, actual per-contact messages

            TOUCHINPUT[] inputs; // Array of TOUCHINPUT structures
            inputs = new TOUCHINPUT[inputCount]; // Allocate the storage for the parameters of the per-contact messages

            // Unpack message parameters into the array of TOUCHINPUT structures, each
            // representing a message for one single contact.
            if (!GetTouchInputInfo(m.LParam, inputCount, inputs, touchInputSize))
            {
                // Get touch info failed.
                return false;
            }

            // For each contact, dispatch the message to the appropriate message
            // handler.
            bool handled = false; // Boolean, is message handled
            for (int i = 0; i < inputCount; i++)
            {
                TOUCHINPUT ti = inputs[i];

                // Assign a handler to this message.
                EventHandler<WMTouchEventArgs> handler = null;     // Touch event handler
                if ((ti.dwFlags & TOUCHEVENTF_DOWN) != 0)
                {
                    handler = Touchdown;
                }
                else if ((ti.dwFlags & TOUCHEVENTF_UP) != 0)
                {
                    handler = Touchup;
                }
                else if ((ti.dwFlags & TOUCHEVENTF_MOVE) != 0)
                {
                    handler = TouchMove;
                }

                // Convert message parameters into touch event arguments and handle the event.
                if (handler != null)
                {
                    // Convert the raw touchinput message into a touchevent.
                    WMTouchEventArgs te = new WMTouchEventArgs(); // Touch event arguments

                    // TOUCHINFO point coordinates and contact size is in 1/100 of a pixel; convert it to pixels.
                    // Also convert screen to client coordinates.
                    te.ContactY = ti.cyContact/100;
                    te.ContactX = ti.cxContact/100;
                    te.Id = ti.dwID;
                    {
                        Point pt = PointToClient(new Point(ti.x/100, ti.y/100));
                        te.LocationX = pt.X;
                        te.LocationY = pt.Y;
                    }
                    te.Time = ti.dwTime;
                    te.Mask = ti.dwMask;
                    te.Flags = ti.dwFlags;

                    // Invoke the event handler.
                    handler(this, te);

                    // Mark this event as handled.
                    handled = true;
                }
            }

            CloseTouchInputHandle(m.LParam);

            return handled;
        }
    }
}
```


# VB

Click without moving mouse
```vb
Imports System.Runtime.InteropServices

Friend Shared Sub TouchEventOccured(ByVal sender As Object, ByVal e As EventArgs)
        Dim args As Win32.WMTouchEventArgs = CType(e, Win32.WMTouchEventArgs)
        Dim pt As Point = New Point(args.LocationX, args.LocationY)
        ClickWithoutMouseUsb(pt)
    End Sub

Public Shared Sub ClickWithoutMouseUsb(ByVal point As Point)
        Dim pnt As Point = point
        Dim hWind As IntPtr = WindowFromPoint(pnt)
        If hWind <> IntPtr.Zero Then
            SendMessage(hWind, BM_CLICK, 0, IntPtr.Zero)
        End If
    End Sub


    'Usb Win32 APIs and structs
    <DllImport("user32.dll")>
    Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByRef lParam As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Shared Function WindowFromPoint(ByVal pnt As Point) As IntPtr
    End Function

    Const BM_CLICK As Integer = &HF5& 'This is eqivilant to a left click down and then left click up
```

A way of getting touch events
```vb
Imports System
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Windows.Forms.Integration

Public Const WM_POINTERDOWN As Integer = &H0246
        Public Const WM_POINTERUP As Integer = &H0247
        Public Const WM_POINTERUPDATE As Integer = &H0245

        Public Enum POINTER_INPUT_TYPE As Integer
            PT_POINTER = &H00000001
            PT_TOUCH = &H00000002
            PT_PEN = &H00000003
            PT_MOUSE = &H00000004
        End Enum

        Public Shared Function GET_POINTERID_WPARAM(ByVal wParam As UInteger) As UInteger
            Return wParam And &HFFFF
        End Function

        <DllImport("User32.dll")>
        Public Shared Function GetPointerType(ByVal pPointerID As UInteger, <Out> ByRef pPointerType As POINTER_INPUT_TYPE) As Boolean
        End Function

        Protected Overrides Sub WndProc(ByRef m As Message)
            Dim handled As Boolean = False
            Dim pointerID As UInteger
            Dim pointerType As POINTER_INPUT_TYPE

            Select Case m.Msg
                Case WM_POINTERDOWN
                    pointerID = GET_POINTERID_WPARAM(CUInt(m.WParam))

                    If GetPointerType(pointerID, pointerType) Then

                        Select Case pointerType
                            Case POINTER_INPUT_TYPE.PT_PEN
                                ' Stylus Down
                                handled = True
                            Case POINTER_INPUT_TYPE.PT_TOUCH
                                ' Touch down
                                Debug.WriteLine("touch event")
                                handled = True
                        End Select
                    End If
            End Select

            If handled Then m.Result = CType(1, IntPtr)
            MyBase.WndProc(m)
        End Sub
```


Win32 API constants, structs, P/Invoke,...

```vb
Imports System.Runtime.InteropServices
Public Class Win32
    'constants
    Friend Const TOUCHEVENTF_MOVE As Integer = &H1
    Friend Const TOUCHEVENTF_DOWN As Integer = &H2
    Friend Const TOUCHEVENTF_UP As Integer = &H4
    Private Const TOUCHEVENTF_PRIMARY As Integer = &H10
    Friend Const BM_CLICK As Integer = &HF5& 'This is eqivilant to a left click down and then left click up
    Friend Const WM_TOUCH As Integer = &H240

    Public Const WM_POINTERDOWN As Integer = &H246
    Public Const WM_POINTERUP As Integer = &H247
    Public Const WM_POINTERUPDATE As Integer = &H245


    Public Enum POINTER_INPUT_TYPE As Integer
        PT_POINTER = &H1
        PT_TOUCH = &H2
        PT_PEN = &H3
        PT_MOUSE = &H4
    End Enum

    'Win32API Interops
    <DllImport("user32.dll")>
    Friend Shared Function WindowFromPoint(ByVal pnt As Point) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Friend Shared Function RegisterTouchWindow(ByVal hWnd As IntPtr, ByVal ulFlags As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Shared Function GetTouchInputInfo(ByVal hTouchInput As IntPtr, ByVal cInputs As Integer,
        <[In], Out> ByVal pInputs As TOUCHINPUT(), ByVal cbSize As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Shared Sub CloseTouchInputHandle(ByVal lParam As IntPtr)
    End Sub

    Public Shared Function GET_POINTERID_WPARAM(ByVal wParam As UInteger) As UInteger
        Return (wParam And &HFFFF)
    End Function

    Public Declare Function GetPointerType Lib "User32.dll" (ByVal pPointerID As UInteger, <Out> ByRef pPointerType As POINTER_INPUT_TYPE) As Boolean

    <DllImport("user32.dll")>
    Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByRef lParam As IntPtr) As IntPtr
    End Function


    <StructLayout(LayoutKind.Sequential)>
    Public Structure TOUCHINPUT
        Public x As Integer
        Public y As Integer
        Public hSource As IntPtr
        Public dwID As Integer
        Public dwFlags As Integer
        Public dwMask As Integer
        Public dwTime As Integer
        Public dwExtraInfo As IntPtr
        Public cxContact As Integer
        Public cyContact As Integer
    End Structure

    Friend Class WMTouchEventArgs
        Inherits EventArgs

        ' Private data members
        Private _x As Integer                  ' touch x client coordinate in pixels
        Private _y As Integer                  ' touch y client coordinate in pixels
        Private _id As Integer                 ' contact ID
        Private _mask As Integer               ' mask which fields in the structure are valid
        Private _flags As Integer              ' flags
        Private _time As Integer               ' touch event time
        Private _contactX As Integer           ' x size of the contact area in pixels
        Private _contactY As Integer           ' y size of the contact area in pixels

        ' Access to data members
        Public Property LocationX As Integer
            Get
                Return _x
            End Get
            Set(ByVal value As Integer)
                _x = value
            End Set
        End Property

        Public Property LocationY As Integer
            Get
                Return _y
            End Get
            Set(ByVal value As Integer)
                _y = value
            End Set
        End Property

        Public Property Id As Integer
            Get
                Return _id
            End Get
            Set(ByVal value As Integer)
                _id = value
            End Set
        End Property

        Public Property Flags As Integer
            Get
                Return _flags
            End Get
            Set(ByVal value As Integer)
                _flags = value
            End Set
        End Property

        Public Property Mask As Integer
            Get
                Return _mask
            End Get
            Set(ByVal value As Integer)
                _mask = value
            End Set
        End Property

        Public Property Time As Integer
            Get
                Return _time
            End Get
            Set(ByVal value As Integer)
                _time = value
            End Set
        End Property

        Public Property ContactX As Integer
            Get
                Return _contactX
            End Get
            Set(ByVal value As Integer)
                _contactX = value
            End Set
        End Property

        Public Property ContactY As Integer
            Get
                Return _contactY
            End Get
            Set(ByVal value As Integer)
                _contactY = value
            End Set
        End Property

        Public ReadOnly Property IsPrimaryContact As Boolean
            Get
                Return (_flags And TOUCHEVENTF_PRIMARY) <> 0
            End Get
        End Property

        ' Constructor
        Public Sub New()
        End Sub
    End Class

End Class

```

Register Touch handle is needed for program to listen to touch events properly
```vb
Public Sub New()
Win32.RegisterTouchWindow(Handle, 0)''Handle is the handle of the Form/control
End Sub
```

Sample BaseForm for inheriting
```vb
Imports System.Runtime.InteropServices
Public Class frmBase
    Inherits System.Windows.Forms.Form

Private Sub BaseForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Win32.RegisterTouchWindow(Handle, 0)
        WndHandle = Handle
        touchInputSize = Marshal.SizeOf(New Win32.TOUCHINPUT())
        AddHandler TouchEvent, AddressOf Common.TouchEventOccured
    End Sub
    
Protected Overrides Sub WndProc(ByRef m As Message)
        'Decode And Handle WM_TOUCH message.
        Dim handled As Boolean

        Select Case m.Msg
            Case Win32.WM_TOUCH
                handled = DecodeTouch(m)
            Case Else
                handled = False
        End Select
        ' Call parent WndProc for default message processing.
        MyBase.WndProc(m)

        If handled Then
            ' Acknowledge event if handled.
            m.Result = New IntPtr(1)
        End If
    End Sub
    
Private Function DecodeTouch(ByRef m As Message) As Boolean
        Dim inputCount As Integer = Common.LoWord(m.WParam.ToInt32())
        Dim inputs As Win32.TOUCHINPUT() ' Array of TOUCHINPUT structures
        inputs = New Win32.TOUCHINPUT(inputCount - 1) {} ' Allocate the storage for the parameters of the per-contact messages
        If Not Win32.GetTouchInputInfo(m.LParam, inputCount, inputs, touchInputSize) Then
            Return False
        End If

        Dim handled As Boolean = False

        For i As Integer = 0 To inputCount - 1
            Dim ti As Win32.TOUCHINPUT = inputs(i)
            Dim handler As EventHandler(Of Win32.WMTouchEventArgs) = Nothing

            If (ti.dwFlags And Win32.TOUCHEVENTF_UP) <> 0 Then
                handler = TouchEventEvent            
            End If

            If handler IsNot Nothing Then
                Dim te As Win32.WMTouchEventArgs = New Win32.WMTouchEventArgs()
                ' TOUCHINFO point coordinates and contact size is in 1/100 of a pixel; convert it to pixels.
                ' Also convert screen to client coordinates.
                te.ContactY = CInt(ti.cyContact / 100)
                te.ContactX = CInt(ti.cxContact / 100)
                te.Id = ti.dwID

                If True Then
                    Dim pt As Point = MyBase.PointToClient(New Point(CInt(ti.x / 100), CInt(ti.y / 100)))
                    te.LocationX = pt.X
                    te.LocationY = pt.Y
                End If

                te.Time = ti.dwTime
                te.Mask = ti.dwMask
                te.Flags = ti.dwFlags

                ' Invoke the event handler.
                handler(Me, te)

                ' Mark this event as handled.
                handled = True
            End If
        Next

        Win32.CloseTouchInputHandle(m.LParam)
        Return handled
    End Function


End Class
```
