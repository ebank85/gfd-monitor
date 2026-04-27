Imports System
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports GFD.Monitor.Core.Models
Imports GFD.Monitor.Core.Utils

Namespace Services
    ''' <summary>
    ''' Service untuk manage konfigurasi aplikasi
    ''' </summary>
    Public Class ConfigManager
        Private _config As JObject
        Private _configPath As String
        
        Public Sub New(Optional configPath As String = Nothing)
            If String.IsNullOrEmpty(configPath) Then
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json")
            End If
            
            _configPath = configPath
            LoadConfig()
        End Sub
        
        ''' <summary>
        ''' Load konfigurasi dari file
        ''' </summary>
        Public Sub LoadConfig()
            Try
                If File.Exists(_configPath) Then
                    Dim json As String = File.ReadAllText(_configPath)
                    _config = JObject.Parse(json)
                    Logger.LogInfo($"Config loaded from {_configPath}")
                Else
                    _config = GetDefaultConfig()
                    Logger.LogInfo("Using default configuration")
                End If
            Catch ex As Exception
                Logger.LogWarning($"Failed to load config: {ex.Message}. Using defaults.")
                _config = GetDefaultConfig()
            End Try
        End Sub
        
        ''' <summary>
        ''' Save konfigurasi ke file
        ''' </summary>
        Public Sub SaveConfig()
            Try
                Dim directory As String = Path.GetDirectoryName(_configPath)
                If Not Directory.Exists(directory) Then
                    Directory.CreateDirectory(directory)
                End If
                
                Dim json As String = _config.ToString(Formatting.Indented)
                File.WriteAllText(_configPath, json)
                Logger.LogInfo($"Config saved to {_configPath}")
            Catch ex As Exception
                Logger.LogError($"Failed to save config: {ex.Message}", ex)
            End Try
        End Sub
        
        ''' <summary>
        ''' Get serial configuration
        ''' </summary>
        Public Function GetSerialConfig() As SerialConfig
            Try
                Dim serialObj = _config("serial")
                If serialObj Is Nothing Then
                    Return New SerialConfig()
                End If
                
                Dim config As New SerialConfig With {
                    .PortName = serialObj("port")?.Value(Of String)() ?? "COM3",
                    .BaudRate = serialObj("baudRate")?.Value(Of Integer)() ?? Constants.DEFAULT_BAUDRATE,
                    .DataBits = serialObj("dataBits")?.Value(Of Integer)() ?? 8,
                    .ReadTimeout = serialObj("readTimeout")?.Value(Of Integer)() ?? Constants.DEFAULT_READ_TIMEOUT,
                    .WriteTimeout = serialObj("writeTimeout")?.Value(Of Integer)() ?? Constants.DEFAULT_WRITE_TIMEOUT
                }
                
                Return config
            Catch ex As Exception
                Logger.LogWarning($"Error reading serial config: {ex.Message}")
                Return New SerialConfig()
            End Try
        End Function
        
        ''' <summary>
        ''' Get monitoring configuration
        ''' </summary>
        Public Function GetMonitoringConfig() As Dictionary(Of String, Object)
            Try
                Dim monObj = _config("monitoring")
                If monObj Is Nothing Then
                    Return GetDefaultMonitoringConfig()
                End If
                
                Dim config As New Dictionary(Of String, Object)
                config("threshold") = monObj("thresholdCurrent")?.Value(Of Double)() ?? Constants.DEFAULT_THRESHOLD_A
                config("tripDelay") = monObj("tripDelay")?.Value(Of Integer)() ?? Constants.DEFAULT_DELAY_MS
                config("autoResetMinutes") = monObj("autoResetMinutes")?.Value(Of Integer)() ?? Constants.DEFAULT_AUTORESET_MIN
                config("controlMode") = monObj("controlMode")?.Value(Of String)() ?? "MANUAL"
                
                Return config
            Catch ex As Exception
                Logger.LogWarning($"Error reading monitoring config: {ex.Message}")
                Return GetDefaultMonitoringConfig()
            End Try
        End Function
        
        ''' <summary>
        ''' Set monitoring configuration
        ''' </summary>
        Public Sub SetMonitoringConfig(threshold As Double, tripDelay As Integer, autoResetMin As Integer)
            Try
                If _config("monitoring") Is Nothing Then
                    _config("monitoring") = New JObject()
                End If
                
                _config("monitoring")("thresholdCurrent") = threshold
                _config("monitoring")("tripDelay") = tripDelay
                _config("monitoring")("autoResetMinutes") = autoResetMin
            Catch ex As Exception
                Logger.LogError($"Failed to set monitoring config: {ex.Message}")
            End Try
        End Sub
        
        Private Function GetDefaultConfig() As JObject
            Dim config = New JObject(
                New JProperty("serial", New JObject(
                    New JProperty("port", "COM3"),
                    New JProperty("baudRate", 115200),
                    New JProperty("dataBits", 8),
                    New JProperty("parity", "None"),
                    New JProperty("stopBits", "One"),
                    New JProperty("readTimeout", 1000),
                    New JProperty("writeTimeout", 1000)
                )),
                New JProperty("monitoring", New JObject(
                    New JProperty("thresholdCurrent", Constants.DEFAULT_THRESHOLD_A),
                    New JProperty("tripDelay", Constants.DEFAULT_DELAY_MS),
                    New JProperty("autoResetMinutes", Constants.DEFAULT_AUTORESET_MIN),
                    New JProperty("controlMode", "MANUAL")
                )),
                New JProperty("logging", New JObject(
                    New JProperty("enableLogging", True),
                    New JProperty("logDirectory", "./logs"),
                    New JProperty("autoRotate", True)
                )),
                New JProperty("ui", New JObject(
                    New JProperty("theme", "Dark"),
                    New JProperty("chartUpdateIntervalMs", 500),
                    New JProperty("chartDataPointsMax", 60)
                ))
            )
            Return config
        End Function
        
        Private Function GetDefaultMonitoringConfig() As Dictionary(Of String, Object)
            Dim config As New Dictionary(Of String, Object)
            config("threshold") = Constants.DEFAULT_THRESHOLD_A
            config("tripDelay") = Constants.DEFAULT_DELAY_MS
            config("autoResetMinutes") = Constants.DEFAULT_AUTORESET_MIN
            config("controlMode") = "MANUAL"
            Return config
        End Function
    End Class
End Namespace
