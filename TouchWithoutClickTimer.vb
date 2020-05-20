Imports System.Runtime.InteropServices

Public Class Form1


    <DllImport("user32.dll")>
    Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, ByRef lParam As IntPtr) As IntPtr

    End Function

    <DllImport("user32.dll")>
    Shared Function WindowFromPoint(ByVal pnt As Point) As IntPtr

    End Function

    Const BM_CLICK As Integer = &HF5&

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Timer1.Enabled = True

        Timer1.Interval = 2000
    End Sub


    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        'NOTE: you need to specify a Point (X,Y), in order for it to get the window handle of it
        'So, if you need to test it, use cursor.position, put a breakpoint there, and then record the X,Y values once your cursor is over the right spot
        'then, stop the program, and use the New Point(X,Y) with the recorded values to have it click the correct inputs

        'Dim pnt As Point = New Point(-1453, 339)
        Dim pnt As Point = Cursor.Position

        ' Dim pnt As Point = Me.PointToScreen(Button1.Location)


        Dim hWnd As IntPtr = WindowFromPoint(pnt)

        If hWnd <> IntPtr.Zero Then

            SendMessage(hWnd, BM_CLICK, 0, IntPtr.Zero)

            ' SendMessage(Me.Button1.Handle, BM_CLICK, 0, IntPtr.Zero)

        End If
    End Sub

End Class
