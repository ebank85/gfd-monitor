Imports System
Imports GFD.Monitor.Core.Models
Imports GFD.Monitor.Core.Utils

Namespace Services
    ''' <summary>
    ''' Service untuk processing data dan business logic
    ''' </summary>
    Public Class DataProcessingService
        
        ''' <summary>
        ''' Check apakah ada fault condition
        ''' </summary>
        Public Shared Function CheckFaultCondition(data As GFDData, threshold As Double) As Boolean
            Return data.Current >= threshold
        End Function
        
        ''' <summary>
        ''' Determine state dari data
        ''' </summary>
        Public Shared Function DetermineState(current As Double, threshold As Double, 
                                               currentState As String) As String
            If current >= threshold Then
                If currentState = Constants.STATE_NORMAL Then
                    Return Constants.STATE_TIMING
                Else
                    Return Constants.STATE_LATCHED
                End If
            Else
                Return Constants.STATE_NORMAL
            End If
        End Function
        
        ''' <summary>
        ''' Calculate threshold percentage
        ''' </summary>
        Public Shared Function CalculateThresholdPercentage(current As Double, threshold As Double) As Double
            If threshold <= 0 Then Return 0
            Return (current / threshold) * 100
        End Function
        
        ''' <summary>
        ''' Validate data
        ''' </summary>
        Public Shared Function ValidateData(data As GFDData) As Boolean
            Return data.Current >= 0 AndAlso data.Current <= 2000 AndAlso
                   Not String.IsNullOrEmpty(data.State) AndAlso
                   data.ThresholdCurrent > 0
        End Function
        
        ''' <summary>
        ''' Generate log event dari data
        ''' </summary>
        Public Shared Function GenerateEventLog(eventType As EventLog.EventType,
                                                 data As GFDData,
                                                 detail As String) As EventLog
            Return New EventLog(
                eventType,
                data.Current,
                data.State,
                data.RelayStatus,
                detail)
        End Function
    End Class
End Namespace
