Imports System
Importing System.Threading
Importing System.Threading.Tasks
Importing GFD.Monitor.Core.Models
Importing GFD.Monitor.Core.Utils

Namespace Services
    ''' <summary>
    ''' Service untuk simulasi data tanpa hardware STM32
    ''' </summary>
    Public Class SimulationService
        Implements IDisposable
        
        Public Event DataGenerated(data As GFDData)
        
        Private _random As New Random()
        Private _timer As Threading.Timer
        Private _isRunning As Boolean = False
        Private _isFault As Boolean = False
        Private _currentValue As Double = 2.0
        Private _syncLock As New Object()
        
        ''' <summary>
        ''' Start simulasi
        ''' </summary>
        Public Sub Start(Optional intervalMs As Integer = Constants.SIMULATION_INTERVAL_MS)
            SyncLock _syncLock
                If _isRunning Then Return
                
                _isRunning = True
                _timer = New Threading.Timer(
                    New TimerCallback(AddressOf GenerateData),
                    Nothing,
                    intervalMs,
                    intervalMs)
                
                Logger.LogInfo($"Simulation started (interval: {intervalMs}ms)")
            End SyncLock
        End Sub
        
        ''' <summary>
        ''' Stop simulasi
        ''' </summary>
        Public Sub [Stop]()
            SyncLock _syncLock
                If Not _isRunning Then Return
                
                _isRunning = False
                If _timer IsNot Nothing Then
                    _timer.Dispose()
                    _timer = Nothing
                End If
                
                Logger.LogInfo("Simulation stopped")
            End SyncLock
        End Sub
        
        ''' <summary>
        ''' Trigger fault simulasi
        ''' </summary>
        Public Sub TriggerFault()
            SyncLock _syncLock
                _isFault = True
                _currentValue = _random.NextDouble() * (Constants.SIMULATION_FAULT_MAX - Constants.SIMULATION_FAULT_MIN) + Constants.SIMULATION_FAULT_MIN
            End SyncLock
        End Sub
        
        ''' <summary>
        ''' Reset fault
        ''' </summary>
        Public Sub ResetFault()
            SyncLock _syncLock
                _isFault = False
                _currentValue = 2.0
            End SyncLock
        End Sub
        
        Private Sub GenerateData(state As Object)
            If Not _isRunning Then Return
            
            Try
                SyncLock _syncLock
                    Dim gfdData As New GFDData()
                    
                    If _isFault Then
                        ' Fault mode
                        _currentValue += (_random.NextDouble() - 0.5) * 20
                        _currentValue = Math.Max(Constants.SIMULATION_FAULT_MIN,
                                               Math.Min(Constants.SIMULATION_FAULT_MAX, _currentValue))
                        gfdData.Current = _currentValue
                        gfdData.State = If(_currentValue >= Constants.DEFAULT_THRESHOLD_A,
                                          Constants.STATE_LATCHED,
                                          Constants.STATE_NORMAL)
                        gfdData.RelayStatus = (_currentValue >= Constants.DEFAULT_THRESHOLD_A)
                    Else
                        ' Normal mode
                        _currentValue += (_random.NextDouble() - 0.5) * 0.5
                        _currentValue = Math.Max(Constants.SIMULATION_NORMAL_MIN,
                                               Math.Min(Constants.SIMULATION_NORMAL_MAX, _currentValue))
                        gfdData.Current = _currentValue
                        gfdData.State = Constants.STATE_NORMAL
                        gfdData.RelayStatus = False
                    End If
                    
                    gfdData.ThresholdCurrent = Constants.DEFAULT_THRESHOLD_A
                    
                    RaiseEvent DataGenerated(gfdData)
                End SyncLock
            Catch ex As Exception
                Logger.LogError($"Simulation error: {ex.Message}")
            End Try
        End Sub
        
        Public ReadOnly Property IsRunning As Boolean
            Get
                SyncLock _syncLock
                    Return _isRunning
                End SyncLock
            End Get
        End Property
        
        Public ReadOnly Property IsFault As Boolean
            Get
                SyncLock _syncLock
                    Return _isFault
                End SyncLock
            End Get
        End Property
        
        Public Sub Dispose() Implements IDisposable.Dispose
            [Stop]()
        End Sub
    End Class
End Namespace
