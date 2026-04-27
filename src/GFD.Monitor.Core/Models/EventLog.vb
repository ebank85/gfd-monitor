Imports System

Namespace Models
    ''' <summary>
    ''' Record event untuk logging
    ''' </summary>
    Public Class EventLog
        Public Enum EventType
            FaultDetected
            Trip
            Reset
            AutoReset
            TestTrip
            Normal
            Connected
            Disconnected
            ConfigChanged
        End Enum
        
        Public Property Timestamp As DateTime
        Public Property EventType As EventType
        Public Property Current As Double
        Public Property State As String
        Public Property RelayStatus As Boolean
        Public Property Detail As String
        
        Public Sub New()
            Timestamp = DateTime.Now
            Current = 0.0
            State = "NORMAL"
            RelayStatus = False
            Detail = ""
        End Sub
        
        Public Sub New(eventType As EventType, current As Double, state As String, 
                      relayStatus As Boolean, detail As String)
            Me.Timestamp = DateTime.Now
            Me.EventType = eventType
            Me.Current = current
            Me.State = state
            Me.RelayStatus = relayStatus
            Me.Detail = detail
        End Sub
        
        ''' <summary>
        ''' Convert ke CSV format
        ''' </summary>
        Public Function ToCsvLine() As String
            Dim eventName As String = EventType.ToString()
            Dim currentStr As String = If(Current > 0, Current.ToString("F2"), "—")
            Dim relayStr As String = If(RelayStatus, "ON", "OFF")
            
            Return String.Format("{0:yyyy-MM-dd HH:mm:ss},{1},{2},{3},{4},{5}",
                                Timestamp, eventName, currentStr, State, relayStr, Detail)
        End Function
        
        Public Overrides Function ToString() As String
            Return String.Format("[{0:HH:mm:ss}] {1}: {2}A | {3}",
                                Timestamp, EventType.ToString(), Current.ToString("F2"), Detail)
        End Function
    End Class
End Namespace
