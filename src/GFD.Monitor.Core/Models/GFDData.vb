Imports System

Namespace Models
    ''' <summary>
    ''' Data real-time dari Ground Fault Detector
    ''' </summary>
    Public Class GFDData
        Public Property Timestamp As DateTime
        Public Property Current As Double ' Ampere
        Public Property State As String ' NORMAL, LATCHED, TIMING
        Public Property RelayStatus As Boolean ' ON/OFF
        Public Property ThresholdCurrent As Double
        
        Public Sub New()
            Timestamp = DateTime.Now
            Current = 0.0
            State = "NORMAL"
            RelayStatus = False
            ThresholdCurrent = 100.0
        End Sub
        
        Public Sub New(current As Double, state As String, relayStatus As Boolean)
            Me.Timestamp = DateTime.Now
            Me.Current = current
            Me.State = state
            Me.RelayStatus = relayStatus
            Me.ThresholdCurrent = 100.0
        End Sub
        
        ''' <summary>
        ''' Apakah ada fault (arus melebihi threshold)
        ''' </summary>
        Public Function IsFault() As Boolean
            Return Current >= ThresholdCurrent
        End Function
        
        ''' <summary>
        ''' Get persentase threshold
        ''' </summary>
        Public Function GetThresholdPercentage() As Double
            If ThresholdCurrent <= 0 Then Return 0
            Return (Current / ThresholdCurrent) * 100
        End Function
        
        ''' <summary>
        ''' Format untuk display
        ''' </summary>
        Public Overrides Function ToString() As String
            Return String.Format("{0:yyyy-MM-dd HH:mm:ss} | I={1:F2}A | State={2} | Relay={3}",
                                Timestamp, Current, State, If(RelayStatus, "ON", "OFF"))
        End Function
    End Class
End Namespace
