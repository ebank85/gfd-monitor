Imports System
Imports GFD.Monitor.Core.Models

Namespace Utils
    ''' <summary>
    ''' Helper untuk parsing dan building protocol komunikasi serial
    ''' </summary>
    Public NotInheritable Class SerialProtocol
        
        ''' <summary>
        ''' Parse data dari STM32 format: $GFDATA:Current,State,Relay,Timestamp\r\n
        ''' </summary>
        Public Shared Function ParseGFData(rawData As String) As GFDData
            Try
                ' Clean data
                Dim cleanData As String = rawData.Trim()
                
                ' Check prefix dan suffix
                If Not cleanData.StartsWith(Constants.PROTOCOL_PREFIX) Then
                    Throw New FormatException("Invalid protocol prefix")
                End If
                
                ' Remove prefix
                cleanData = cleanData.Substring(1)
                
                ' Split command dan data
                Dim parts As String() = cleanData.Split(":"c)
                If parts.Length <> 2 Then
                    Throw New FormatException("Invalid protocol format")
                End If
                
                Dim command As String = parts(0).Trim()
                Dim dataStr As String = parts(1).Trim()
                
                ' Validate command
                If command <> Constants.GFDATA_CMD Then
                    Throw New FormatException($"Unknown command: {command}")
                End If
                
                ' Parse data fields
                Dim fields As String() = dataStr.Split(","c)
                If fields.Length < 3 Then
                    Throw New FormatException("Missing data fields")
                End If
                
                Dim current As Double = Double.Parse(fields(0).Trim())
                Dim state As String = fields(1).Trim()
                Dim relayStr As String = fields(2).Trim()
                Dim relay As Boolean = relayStr = "ON"
                
                Dim gfdData = New GFDData(current, state, relay)
                
                ' Parse timestamp jika ada
                If fields.Length >= 4 Then
                    If DateTime.TryParse(fields(3).Trim(), gfdData.Timestamp) Then
                        ' Timestamp updated
                    End If
                End If
                
                Return gfdData
                
            Catch ex As Exception
                Throw New FormatException($"Failed to parse GFData: {ex.Message}", ex)
            End Try
        End Function
        
        ''' <summary>
        ''' Build command untuk SET_THRESH
        ''' </summary>
        Public Shared Function BuildSetThreshCommand(threshold As Double) As String
            If threshold < Constants.MIN_THRESHOLD_A OrElse threshold > Constants.MAX_THRESHOLD_A Then
                Throw New ArgumentOutOfRangeException(nameof(threshold), 
                    $"Threshold must be between {Constants.MIN_THRESHOLD_A} and {Constants.MAX_THRESHOLD_A}")
            End If
            Return $"{Constants.PROTOCOL_PREFIX}{Constants.SET_THRESH_CMD}:{threshold:F2}{Constants.PROTOCOL_SUFFIX}"
        End Function
        
        ''' <summary>
        ''' Build command untuk SET_DELAY
        ''' </summary>
        Public Shared Function BuildSetDelayCommand(delayMs As Integer) As String
            If delayMs < Constants.MIN_DELAY_MS OrElse delayMs > Constants.MAX_DELAY_MS Then
                Throw New ArgumentOutOfRangeException(nameof(delayMs),
                    $"Delay must be between {Constants.MIN_DELAY_MS} and {Constants.MAX_DELAY_MS}")
            End If
            Return $"{Constants.PROTOCOL_PREFIX}{Constants.SET_DELAY_CMD}:{delayMs}{Constants.PROTOCOL_SUFFIX}"
        End Function
        
        ''' <summary>
        ''' Build command untuk SET_AUTORESET
        ''' </summary>
        Public Shared Function BuildSetAutoResetCommand(minutes As Integer) As String
            If minutes < Constants.MIN_AUTORESET_MIN OrElse minutes > Constants.MAX_AUTORESET_MIN Then
                Throw New ArgumentOutOfRangeException(nameof(minutes),
                    $"Auto reset must be between {Constants.MIN_AUTORESET_MIN} and {Constants.MAX_AUTORESET_MIN}")
            End If
            Return $"{Constants.PROTOCOL_PREFIX}{Constants.SET_AUTORESET_CMD}:{minutes}{Constants.PROTOCOL_SUFFIX}"
        End Function
        
        ''' <summary>
        ''' Build command untuk RESET_RELAY
        ''' </summary>
        Public Shared Function BuildResetRelayCommand() As String
            Return $"{Constants.PROTOCOL_PREFIX}{Constants.RESET_RELAY_CMD}{Constants.PROTOCOL_SUFFIX}"
        End Function
        
        ''' <summary>
        ''' Build command untuk TEST_TRIP
        ''' </summary>
        Public Shared Function BuildTestTripCommand() As String
            Return $"{Constants.PROTOCOL_PREFIX}{Constants.TEST_TRIP_CMD}{Constants.PROTOCOL_SUFFIX}"
        End Function
        
        ''' <summary>
        ''' Build command untuk QUERY_STATUS
        ''' </summary>
        Public Shared Function BuildQueryStatusCommand() As String
            Return $"{Constants.PROTOCOL_PREFIX}{Constants.QUERY_STATUS_CMD}{Constants.PROTOCOL_SUFFIX}"
        End Function
        
        ''' <summary>
        ''' Check apakah data berisi ACK
        ''' </summary>
        Public Shared Function IsAck(rawData As String) As Boolean
            Return rawData.Trim().StartsWith($"{Constants.PROTOCOL_PREFIX}{Constants.GFACK_CMD}")
        End Function
        
        ''' <summary>
        ''' Check apakah data adalah GFDATA
        ''' </summary>
        Public Shared Function IsGFData(rawData As String) As Boolean
            Return rawData.Trim().StartsWith($"{Constants.PROTOCOL_PREFIX}{Constants.GFDATA_CMD}")
        End Function
    End Class
End Namespace
