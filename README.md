# GFD Monitor - Ground Fault Detector Monitor

Aplikasi desktop VB.NET untuk monitoring Ground Fault Detector berbasis STM32 dengan komunikasi serial real-time.

## Features

✅ **Monitor Tab** - Real-time monitoring arus ground fault dengan tampilan grafis
✅ **Setting Tab** - Konfigurasi threshold, delay, dan mode operasi
✅ **Event Log Tab** - Pencatatan event dengan export ke CSV
✅ **Serial Communication** - Komunikasi real-time dengan STM32
✅ **Data Simulation** - Mode simulasi untuk testing tanpa hardware
✅ **Real-time Charting** - Visualisasi trend arus dengan OxyPlot
✅ **CSV Logging** - Automatic export event log ke file

## System Requirements

- **.NET 6/7/8** runtime
- **Windows 7 SP1** atau lebih baru
- **USB to Serial adapter** (untuk koneksi STM32)
- **Minimum:** 2GB RAM, 100MB disk space

## Project Structure

```
GFD-Monitor/
├── src/
│   ├── GFD.Monitor.Core/          # Core business logic & services
│   │   ├── Models/                # Data models
│   │   ├── Services/              # Core services
│   │   ├── Utils/                 # Utilities
│   │   └── GFD.Monitor.Core.vbproj
│   ├── GFD.Monitor.Desktop/       # WinForms UI
│   │   ├── Forms/                 # Main forms
│   │   ├── Controls/              # Custom controls
│   │   ├── Resources/             # Icons, images
│   │   └── GFD.Monitor.Desktop.vbproj
│   └── GFD.Monitor.Tests/         # Unit tests
├── docs/                          # Documentation
├── GFD.Monitor.sln               # Solution file
├── .gitignore
├── LICENSE (MIT)
└── README.md
```

## Getting Started

### Prerequisites
```bash
# Clone repository
git clone https://github.com/ebank85/gfd-monitor.git
cd gfd-monitor

# Restore dependencies
dotnet restore
```

### Build & Run
```bash
# Build solution
dotnet build

# Run Desktop Application
cd src/GFD.Monitor.Desktop
dotnet run
```

### Configuration

Buat file `config.json` di folder aplikasi:
```json
{
  "serial": {
    "port": "COM3",
    "baudRate": 115200,
    "dataBits": 8,
    "parity": "None",
    "stopBits": "One",
    "handshake": "None"
  },
  "monitoring": {
    "thresholdCurrent": 100.0,
    "tripDelay": 500,
    "autoResetMinutes": 120
  },
  "logging": {
    "enableLogging": true,
    "logDirectory": "./logs",
    "maxLogFileSizeMB": 10
  }
}
```

## Serial Protocol

Standard komunikasi dengan STM32:

### Data Format
```
$GFDATA:Current(A),State,Relay,Timestamp\r\n
# Example:
$GFDATA:2.34,NORMAL,OFF,2024-01-15T14:35:02Z\r\n
# Fault:
$GFDATA:187.50,LATCHED,ON,2024-01-15T14:35:02Z\r\n
# Response (STM32 acknowledge):
$GFACK:OK\r\n
# Command format:
$SET_THRESH:100\r\n
$SET_DELAY:500\r\n
$SET_AUTORESET:120\r\n
$RESET_RELAY\r\n
$TEST_TRIP\r\n
$QUERY_STATUS\r\n
```

## Usage

### Monitor Tab
1. Hubungkan STM32 via USB
2. Pilih COM port & baud rate
3. Klik "CONNECT"
4. Monitor real-time arus dan status
5. Gunakan tombol "SIMULATE FAULT" untuk testing

### Setting Tab
1. Set threshold arus (20-900A)
2. Set trip delay (0-60000ms)
3. Set auto-reset (0-1440 min)
4. Pilih mode MANUAL atau AUTO
5. Klik "SEND" untuk apply settings

### Event Log Tab
1. Semua event otomatis tercatat
2. Filter by type (ALL, FAULT, TRIP, RESET)
3. Export ke CSV dengan tombol "Save CSV"
4. Clear log dengan tombol "Clear"

## Data Simulation Mode

Untuk testing tanpa hardware STM32:

1. Pada tab Setting, jangan hubungkan ke COM port
2. Aplikasi otomatis masuk mode simulasi
3. Data arus akan berfluktuasi secara random
4. Gunakan tombol "SIMULATE FAULT" untuk trigger
5. Event log akan tercatat normal

## CSV Export Format

```csv
Timestamp,Event,Current(A),State,Relay,Detail
2024-01-15T14:35:02Z,FAULT_DETECTED,187.50,LATCHED,ON,Threshold: 100A exceeded
2024-01-15T14:35:02Z,TRIP,187.50,LATCHED,ON,Delay: 500ms · Relay ON
2024-01-15T14:35:40Z,RESET,—,NORMAL,OFF,Source: BUTTON
```

## Architecture

See [ARCHITECTURE.md](docs/ARCHITECTURE.md) untuk detail teknis.

## Contributing

Contributions welcome! Please:
1. Fork repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## License

MIT License - see [LICENSE](LICENSE) file for details

## Support

Untuk issue atau pertanyaan:
- Buka [GitHub Issues](https://github.com/ebank85/gfd-monitor/issues)
- Atau contact maintainer

## Changelog

### v1.0.0 (Initial Release)
- ✅ Serial communication dengan STM32
- ✅ Real-time monitoring dengan chart
- ✅ Setting configuration
- ✅ Event logging ke CSV
- ✅ Data simulation mode
- ✅ 3-tab interface (Monitor, Setting, Log)

---

**Made with ❤️ for Ground Fault Detection Systems**
