Imports System
Imports System.IO.Ports
Imports System.Threading
Imports System.Threading.Tasks
Imports GFD.Monitor.Core.Models
Imports GFD.Monitor.Core.Utils

Namespace Services
    ''' <summary>
    ''' Service untuk komunikasi serial dengan STM32
    ''' </summary>
    Public Class SerialCommunicationService
        Implements IDisposable
        
        Public Event DataReceived(data As GFDData)
        Public Event ConnectionStateChanged(isConnected As Boolean)
        Public Event ErrorOccurred(message As String)
        Public Event CommandSent(command As String)
        Public Event AckReceived()
        
        Private _serialPort As SerialPort
        Private _config As SerialConfig
        Private _isConnected As Boolean = False
        Private _isDisposing As Boolean = False
        Private _readThread As Thread
        Private _reconnectAttempts As Integer = 0
        Private _lastDataTime As DateTime = DateTime.Now
        
        ''' <summary>
        ''' Connect ke serial port
        ''' </summary>
        Public Sub Connect(config As SerialConfig)
            Try
                If _isConnected Then
                    Logger.LogWarning($"Already connected to {_config.PortName}")
                    Return
                End If
                
                If Not config.IsValid() Then
                    Throw New ArgumentException("Invalid serial configuration")
                End If
                
                _config = config.Clone()
                
                _serialPort = New SerialPort(
                    _config.PortName,
                    _config.BaudRate,
                    _config.Parity,
                    _config.DataBits,
                    _config.StopBits)
                
                _serialPort.Handshake = _config.Handshake
                _serialPort.ReadTimeout = _config.ReadTimeout
                _serialPort.WriteTimeout = _config.WriteTimeout
                _serialPort.DtrEnable = True
                _serialPort.RtsEnable = True
                
                _serialPort.Open()
                _isConnected = True
                _reconnectAttempts = 0
                _lastDataTime = DateTime.Now
                
                Logger.LogInfo($"Connected to {_config.PortName} @ {_config.BaudRate} baud")
                
                RaiseEvent ConnectionStateChanged(_isConnected)
                
                ' Start reader thread
                StartReaderThread()
                
            Catch ex As Exception
                _isConnected = False
                Logger.LogError($"Failed to connect: {ex.Message}", ex)
                RaiseEvent ErrorOccurred(ex.Message)
                RaiseEvent ConnectionStateChanged(False)
            End Try
        End Sub
        
        ''' <summary>
        ''' Disconnect dari serial port
        ''' </summary>
        Public Sub Disconnect()
            Try
                If Not _isConnected Then Return
                
                _isConnected = False
                
                If _readThread IsNot Nothing AndAlso _readThread.IsAlive Then
                    _readThread.Join(1000)
                End If
                
                If _serialPort IsNot Nothing AndAlso _serialPort.IsOpen Then
                    _serialPort.Close()
                End If
                
                Logger.LogInfo("Disconnected from serial port")
                RaiseEvent ConnectionStateChanged(False)
                
            Catch ex As Exception
                Logger.LogError($"Error during disconnect: {ex.Message}", ex)
            End Try
        End Sub
        
        ''' <summary>
        ''' Send command ke STM32
        ''' </summary>
        Public Sub SendCommand(command As String)
            Try
                If Not _isConnected OrElse _serialPort Is Nothing OrElse Not _serialPort.IsOpen Then
                    Throw New InvalidOperationException("Serial port not connected")
                End If
                
                _serialPort.WriteLine(command)
                Logger.LogDebug($"Sent: {command}")
                RaiseEvent CommandSent(command)
                
            Catch ex As Exception
                Logger.LogError($"Failed to send command: {ex.Message}", ex)
                RaiseEvent ErrorOccurred(ex.Message)
                HandleConnectionLoss()
            End Try
        End Sub
        
        ''' <summary>
        ''' Send threshold update command
        ''' </summary>
        Public Sub SendThresholdUpdate(threshold As Double)
            Try
                Dim cmd As String = SerialProtocol.BuildSetThreshCommand(threshold)
                SendCommand(cmd)
            Catch ex As Exception
                RaiseEvent ErrorOccurred(ex.Message)
            End Try
        End Sub
        
        ''' <summary>
        ''' Send delay update command
        ''' </summary>
        Public Sub SendDelayUpdate(delayMs As Integer)
            Try
                Dim cmd As String = SerialProtocol.BuildSetDelayCommand(delayMs)
                SendCommand(cmd)
            Catch ex As Exception
                RaiseEvent ErrorOccurred(ex.Message)
            End Try
        End Sub
        
        ''' <summary>
        ''' Send auto-reset update command
        ''' </summary>
        Public Sub SendAutoResetUpdate(minutes As Integer)
            Try
                Dim cmd As String = SerialProtocol.BuildSetAutoResetCommand(minutes)
                SendCommand(cmd)
            Catch ex As Exception
                RaiseEvent ErrorOccurred(ex.Message)
            End Try
        End Sub
        
        ''' <summary>
        ''' Send reset relay command
        ''' </summary>
        Public Sub SendResetRelay()
            Try
                Dim cmd As String = SerialProtocol.BuildResetRelayCommand()
                SendCommand(cmd)
            Catch ex As Exception
                RaiseEvent ErrorOccurred(ex.Message)
            End Try
        End Sub
        
        ''' <summary>
        ''' Send test trip command
        ''' </summary>
        Public Sub SendTestTrip()
            Try
                Dim cmd As String = SerialProtocol.BuildTestTripCommand()
                SendCommand(cmd)
            Catch ex As Exception
                RaiseEvent ErrorOccurred(ex.Message)
            End Try
        End Sub
        
        ''' <summary>
        ''' Get connection status
        ''' </summary>
        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _isConnected AndAlso _serialPort IsNot Nothing AndAlso _serialPort.IsOpen
            End Get
        End Property
        
        Private Sub StartReaderThread()
            _readThread = New Thread(AddressOf ReaderThreadProc) With {
                .Name = "SerialReaderThread",
                .IsBackground = True
            }
            _readThread.Start()
        End Sub
        
        Private Sub ReaderThreadProc()
            Try
                While _isConnected AndAlso Not _isDisposing
                    Try
                        If _serialPort.BytesToRead > 0 Then
                            Dim data As String = _serialPort.ReadLine()
                            If String.IsNullOrWhiteSpace(data) Then Continue While
                            
                            _lastDataTime = DateTime.Now
                            Logger.LogDebug($"Received: {data}")
                            
                            If SerialProtocol.IsGFData(data) Then
                                Try
                                    Dim gfdData = SerialProtocol.ParseGFData(data)
                                    RaiseEvent DataReceived(gfdData)
                                Catch ex As Exception
                                    Logger.LogError($"Parse error: {ex.Message}")
                                    RaiseEvent ErrorOccurred($"Parse error: {ex.Message}")
                                End Try
                            ElseIf SerialProtocol.IsAck(data) Then
                                RaiseEvent AckReceived()
                            End If
                        Else
                            Thread.Sleep(10)
                        End If
                    Catch ex As TimeoutException
                        ' Normal timeout, continue listening
                        Thread.Sleep(50)
                    Catch ex As Exception
                        If _isConnected Then
                            Logger.LogError($"Reader error: {ex.Message}")
                            RaiseEvent ErrorOccurred(ex.Message)
                            HandleConnectionLoss()
                        End If
                    End Try
                End While
            Catch ex As Exception
                Logger.LogError($"Reader thread error: {ex.Message}", ex)
            Finally
                _isConnected = False
            End Try
        End Sub
        
        Private Sub HandleConnectionLoss()
            Try
                _isConnected = False
                If _serialPort IsNot Nothing AndAlso _serialPort.IsOpen Then
                    _serialPort.Close()
                End If
                RaiseEvent ConnectionStateChanged(False)
            Catch
            End Try
        End Sub
        
        Public Sub Dispose() Implements IDisposable.Dispose
            _isDisposing = True
            Disconnect()
            If _serialPort IsNot Nothing Then
                _serialPort.Dispose()
            End If
        End Sub
    End Class
End Namespace
