Namespace Utils
    ''' <summary>
    ''' Konstanta aplikasi
    ''' </summary>
    Public NotInheritable Class Constants
        ' Application Info
        Public Const APP_NAME As String = "GFD Monitor"
        Public Const APP_VERSION As String = "1.0.0"
        Public Const APP_TITLE As String = "GFD Monitor - Ground Fault Detector"
        
        ' Serial Communication
        Public Const DEFAULT_BAUDRATE As Integer = 115200
        Public Const DEFAULT_DATABITS As Integer = 8
        Public Const DEFAULT_READ_TIMEOUT As Integer = 1000
        Public Const DEFAULT_WRITE_TIMEOUT As Integer = 1000
        Public Const SERIAL_RECONNECT_DELAY_MS As Integer = 2000
        Public Const SERIAL_RECONNECT_MAX_RETRIES As Integer = 5
        
        ' Protocol
        Public Const PROTOCOL_PREFIX As String = "$"
        Public Const PROTOCOL_SUFFIX As String = "\r\n"
        Public Const GFDATA_CMD As String = "GFDATA"
        Public Const GFACK_CMD As String = "GFACK"
        Public Const SET_THRESH_CMD As String = "SET_THRESH"
        Public Const SET_DELAY_CMD As String = "SET_DELAY"
        Public Const SET_AUTORESET_CMD As String = "SET_AUTORESET"
        Public Const RESET_RELAY_CMD As String = "RESET_RELAY"
        Public Const TEST_TRIP_CMD As String = "TEST_TRIP"
        Public Const QUERY_STATUS_CMD As String = "QUERY_STATUS"
        
        ' Monitoring Settings
        Public Const MIN_THRESHOLD_A As Double = 20.0
        Public Const MAX_THRESHOLD_A As Double = 900.0
        Public Const DEFAULT_THRESHOLD_A As Double = 100.0
        Public Const MIN_DELAY_MS As Integer = 0
        Public Const MAX_DELAY_MS As Integer = 60000
        Public Const DEFAULT_DELAY_MS As Integer = 500
        Public Const MIN_AUTORESET_MIN As Integer = 0
        Public Const MAX_AUTORESET_MIN As Integer = 1440
        Public Const DEFAULT_AUTORESET_MIN As Integer = 120
        
        ' Simulation
        Public Const SIMULATION_INTERVAL_MS As Integer = 500
        Public Const SIMULATION_NORMAL_MIN As Double = 1.5
        Public Const SIMULATION_NORMAL_MAX As Double = 3.0
        Public Const SIMULATION_FAULT_MIN As Double = 150.0
        Public Const SIMULATION_FAULT_MAX As Double = 250.0
        
        ' Chart
        Public Const CHART_UPDATE_INTERVAL_MS As Integer = 500
        Public Const CHART_DATA_POINTS_MAX As Integer = 60 ' 30 seconds @ 2Hz
        Public Const CHART_DISPLAY_DURATION_SEC As Integer = 30
        
        ' CSV Logging
        Public Const LOG_DIRECTORY As String = "logs"
        Public Const LOG_FILE_PREFIX As String = "GFD_Log_"
        Public Const LOG_FILE_EXTENSION As String = ".csv"
        Public Const CSV_BATCH_SIZE As Integer = 10
        Public Const CSV_BATCH_TIMEOUT_MS As Integer = 5000
        
        ' UI
        Public Const WINDOW_WIDTH As Integer = 860
        Public Const WINDOW_HEIGHT As Integer = 600
        Public Const STATUS_BAR_HEIGHT As Integer = 24
        
        ' States
        Public Const STATE_NORMAL As String = "NORMAL"
        Public Const STATE_LATCHED As String = "LATCHED"
        Public Const STATE_TIMING As String = "TIMING"
        
        ' Colors (Dark Theme)
        Public Const COLOR_BG_BASE As String = "#2b2d30"
        Public Const COLOR_BG_PANEL As String = "#35383d"
        Public Const COLOR_TEXT_PRIMARY As String = "#d4d6da"
        Public Const COLOR_ACCENT_GREEN As String = "#3ec97a"
        Public Const COLOR_ACCENT_RED As String = "#e84545"
        Public Const COLOR_ACCENT_AMBER As String = "#f0a500"
    End Class
End Namespace
