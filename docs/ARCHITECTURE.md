# GFD Monitor - Architecture Documentation

## Overview

GFD Monitor adalah aplikasi desktop VB.NET yang dirancang untuk monitoring Ground Fault Detector berbasis STM32. Arsitektur mengikuti pola **layered architecture** dengan pemisahan yang jelas antara UI, Business Logic, dan Data Access.

## Architecture Layers

```
┌─────────────────────────────────────┐
│   Presentation Layer (WinForms)     │  ← GFD.Monitor.Desktop
│   (MainForm, Controls, UI Logic)    │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│    Business Logic Layer             │  ← GFD.Monitor.Core
│   (Services, Processing, Rules)     │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│    Data & Integration Layer         │  ← Core Services
│  (Serial Comm, Logging, Config)     │
└─────────────────────────────────────┘
```

## Project Structure

### 1. **GFD.Monitor.Core**

Core library yang berisi semua business logic dan services.

#### Models Folder
```
Models/
├── SerialConfig.vb       # Konfigurasi serial port
├── GFDData.vb           # Data real-time arus
├── EventLog.vb          # Record event logging
└── SystemStatus.vb      # Status sistem keseluruhan
```

**Diagram Model Relationship:**
```
SystemStatus
├── GFDData (current data)
└── List(Of EventLog) (event history)

SerialConfig (singleton)
└── Port configuration
```

#### Services Folder

**SerialCommunicationService.vb**
- Handle pembukaan/penutupan serial port
- Send/receive data dari STM32
- Parse protocol dan trigger events
- Error handling & auto-reconnect

```vb
Public Class SerialCommunicationService
    Public Event DataReceived(data As GFDData)
    Public Event ConnectionStateChanged(isConnected As Boolean)
    Public Event ErrorOccurred(message As String)
    
    Public Sub Connect(config As SerialConfig)
    Public Sub Disconnect()
    Public Sub SendCommand(command As String)
    Public Sub SendThresholdUpdate(threshold As Double)
    Public Sub SendDelayUpdate(delayMs As Integer)
    Public Sub SendAutoResetUpdate(minutes As Integer)
End Class
```

**DataProcessingService.vb**
- Parse raw serial data
- Validate data
- Apply business rules (fault detection, state transitions)
- Generate events

```vb
Public Class DataProcessingService
    Public Function ParseSerialData(rawData As String) As GFDData
    Public Function CheckFaultCondition(data As GFDData, threshold As Double) As Boolean
    Public Function DetermineState(current As Double, threshold As Double) As String
End Class
```

**LoggingService.vb**
- Simpan event ke file CSV
- Query event history
- Filter & export
- Auto-rotate log file

```vb
Public Class LoggingService
    Public Sub LogEvent(eventLog As EventLog)
    Public Function ExportToCsv(filename As String) As Boolean
    Public Function GetEventHistory(Optional dateFrom As DateTime? = Nothing) As List(Of EventLog)
    Public Function GetEventsByType(eventType As EventLog.EventType) As List(Of EventLog)
End Class
```

**SimulationService.vb**
- Generate simulasi data untuk testing
- Tidak perlu hardware STM32
- Reproducible untuk testing

```vb
Public Class SimulationService
    Public Function GenerateNormalData() As GFDData
    Public Function GenerateFaultData() As GFDData
    Public Sub StartSimulation(intervalMs As Integer)
    Public Sub StopSimulation()
End Class
```

**ConfigManager.vb**
- Load/save konfigurasi dari JSON
- Validate config
- Default settings

```vb
Public Class ConfigManager
    Public Sub LoadConfig(filename As String)
    Public Sub SaveConfig(filename As String)
    Public Function GetSerialConfig() As SerialConfig
    Public Function GetMonitoringConfig() As Dictionary(Of String, Object)
End Class
```

#### Utils Folder
```
Utils/
├── Constants.vb         # Konstanta aplikasi
├── SerialProtocol.vb    # Protocol parsing helper
├── CsvExporter.vb       # CSV export utility
└── Logger.vb            # Logging utility
```

### 2. **GFD.Monitor.Desktop** (WinForms)

Presentation layer dengan UI menggunakan WinForms.

```
GFD.Monitor.Desktop/
├── Forms/
│   ├── MainForm.vb              # Main window (3 tabs)
│   ├── MonitorTabControl.vb     # Monitor tab user control
│   ├── SettingTabControl.vb     # Setting tab user control
│   └── LogTabControl.vb         # Log tab user control
├── Controls/
│   ├── ChartControl.vb          # Custom chart control (OxyPlot)
│   ├── StatusIndicator.vb       # LED indicator control
│   └── CurrentDisplay.vb        # Big current display control
├── Resources/
│   ├── Icons/
│   └── Styles/
└── GFD.Monitor.Desktop.vbproj
```

**MainForm.vb** - Container utama
- Title bar dengan info koneksi
- Menu bar (File, View, Tools, Help)
- Tab control dengan 3 tab
- Status bar bottom
- Manage lifecycle services

**MonitorTabControl.vb**
- Display arus real-time besar
- Threshold bar visual
- Status panel dengan info sistem
- LED indicators
- Action buttons (Simulate Fault, Reset Relay)
- Real-time chart (OxyPlot)

**SettingTabControl.vb**
- Serial connection configuration
- Control mode selection
- Threshold setting
- Delay & auto-reset setting
- Send commands ke STM32

**LogTabControl.vb**
- Event log table
- Filter options
- Save/Clear buttons
- Export ke CSV

### 3. **GFD.Monitor.Tests**

Unit tests menggunakan xUnit.

```
Tests/
├── SerialCommunicationServiceTests.vb
├── DataProcessingServiceTests.vb
├── LoggingServiceTests.vb
└── SimulationServiceTests.vb
```

## Serial Protocol

### Data Format

**Incoming (from STM32):**
```
$GFDATA:Current(A),State,Relay,Timestamp\r\n

Examples:
$GFDATA:2.34,NORMAL,OFF,2024-01-15T14:35:02Z\r\n
$GFDATA:187.50,LATCHED,ON,2024-01-15T14:35:02Z\r\n
$GFACK:OK\r\n
```

**Outgoing (from PC):**
```
$SET_THRESH:100\r\n         # Set threshold ke 100A
$SET_DELAY:500\r\n           # Set delay ke 500ms
$SET_AUTORESET:120\r\n       # Set auto-reset ke 120 menit
$RESET_RELAY\r\n             # Manual reset relay
$TEST_TRIP\r\n               # Trigger test trip
$QUERY_STATUS\r\n            # Request status
```

## Data Flow

### Receiving Data from STM32

```
Serial Port
    │
    ├─> SerialCommunicationService.OnDataReceived()
    │
    ├─> DataProcessingService.ParseSerialData()
    │
    ├─> Validate & Process
    │
    ├─> Update SystemStatus
    │
    ├─> LoggingService.LogEvent()
    │
    ├─> Raise Event: DataUpdated
    │
    └─> MainForm.OnDataUpdated() → Update UI
```

### Sending Command to STM32

```
UI Button Click
    │
    ├─> Validate Input
    │
    ├─> Build Command String
    │
    ├─> SerialCommunicationService.SendCommand()
    │
    ├─> Write to Serial Port
    │
    ├─> LoggingService.LogEvent()
    │
    └─> Update UI Status
```

## State Diagram

```
         ┌─────────────┐
         │   NORMAL    │
         │  I < Thresh │
         └──────┬──────┘
                │
         I > Thresh
         Delay triggered
                │
         ┌──────▼──────┐
         │   LATCHED   │ ◄─────────┐
         │ I > Thresh  │           │
         │ Relay = ON  │   Reset   │
         └──────┬──────┘ triggered │
                │                  │
         Auto-Reset
         OR Manual Reset
                │
                └──────────────────┘
```

## Error Handling

### Serial Communication Errors
- **ConnectionFailed**: Retry dengan exponential backoff
- **PortNotFound**: Show dialog, prompt user
- **DataParsingError**: Log & skip, continue listening
- **Timeout**: Auto-disconnect & show warning

### Application Errors
- **ConfigLoadError**: Use default config
- **CsvExportError**: Show error dialog, allow retry
- **DiskSpace**: Warn & truncate old logs

## Configuration File (config.json)

```json
{
  "serial": {
    "port": "COM3",
    "baudRate": 115200,
    "dataBits": 8,
    "parity": "None",
    "stopBits": "One",
    "handshake": "None",
    "readTimeout": 1000,
    "writeTimeout": 1000
  },
  "monitoring": {
    "thresholdCurrent": 100.0,
    "tripDelay": 500,
    "autoResetMinutes": 120,
    "controlMode": "MANUAL"
  },
  "logging": {
    "enableLogging": true,
    "logDirectory": "./logs",
    "maxLogFileSizeMB": 10,
    "autoRotate": true
  },
  "ui": {
    "theme": "Dark",
    "chartUpdateIntervalMs": 500,
    "chartDataPointsMax": 60
  }
}
```

## Threading Model

- **Main UI Thread**: Handle UI updates
- **Serial Reader Thread**: Listen to serial port (BackgroundWorker)
- **Simulation Thread**: Generate simulasi data (Timer)
- **CSV Writer Thread**: Async write to CSV (Task)

## Performance Considerations

1. **Chart Updates**: Limit ke 500ms interval untuk smooth rendering
2. **Data Points**: Keep last 60 seconds data (60 points @ 1Hz)
3. **CSV Logging**: Batch write setiap 10 events atau 5 detik
4. **Memory**: ~50MB for normal operation

## Security Considerations

1. **Serial Port**: Validate all input dari STM32
2. **Config File**: Store in user's AppData folder
3. **Log Files**: Don't store sensitive data
4. **CSV Export**: Safe encoding (UTF-8)

## Testing Strategy

1. **Unit Tests**: Services logic
2. **Integration Tests**: Serial communication mock
3. **UI Tests**: Manual testing (form interactions)
4. **Simulation Tests**: Data generation & logging

## Dependencies

```xml
<PackageReference Include="System.IO.Ports" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="CsvHelper" Version="30.0.0" />
<PackageReference Include="OxyPlot.WindowsForms" Version="2.1.2" />
```

## Future Enhancements

1. **Database**: Replace CSV with SQLite
2. **Network**: Add TCP/IP communication option
3. **Reporting**: Generate PDF reports
4. **Analytics**: Data analysis & trending
5. **Multi-Device**: Monitor multiple GFD units
6. **Notifications**: Email/SMS alerts
7. **Web Dashboard**: Real-time web view

---

**Last Updated**: 2024-01-15
**Version**: 1.0.0
