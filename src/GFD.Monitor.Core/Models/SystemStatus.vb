Imports System

Namespace Models
    ''' <summary>
    ''' Status keseluruhan sistem
    ''' </summary>
    Public Class SystemStatus
        Public Property IsConnected As Boolean
        Public Property CurrentData As GFDData
        Public Property ThresholdCurrent As Double = 100.0
        Public Property TripDelayMs As Integer = 500
        Public Property AutoResetMinutes As Integer = 120
        Public Property ControlMode As String = "MANUAL" ' MANUAL atau AUTO
        Public Property IsSimulating As Boolean = False
        Public Property StartTime As DateTime
        Public Property LastUpdateTime As DateTime
        
        Public Sub New()
            IsConnected = False
            CurrentData = New GFDData()
            StartTime = DateTime.Now
            LastUpdateTime = DateTime.Now
        End Sub
        
        ''' <summary>
        ''' Get uptime dalam format HH:MM:SS
        ''' </summary>
        Public Function GetUptime() As String
            Dim span As TimeSpan = DateTime.Now.Subtract(StartTime)
            Return String.Format("{0:D2}:{1:D2}:{2:D2}", 
                                CInt(span.TotalHours), span.Minutes, span.Seconds)
        End Function
        
        ''' <summary>
        ''' Get status string
        ''' </summary>
        Public Function GetStatusString() As String
            Dim connStr As String = If(IsConnected, "CONNECTED", "DISCONNECTED")
            Dim simStr As String = If(IsSimulating, " (SIMULATION)", "")
            Return String.Format("{0}{1} | State: {2} | I: {3:F2}A | Relay: {4}",
                                connStr, simStr, CurrentData.State, 
                                CurrentData.Current, 
                                If(CurrentData.RelayStatus, "ON", "OFF"))
        End Function
    End Class
End Namespace
