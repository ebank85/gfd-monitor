Namespace Models
    ''' <summary>
    ''' Konfigurasi untuk komunikasi serial dengan STM32
    ''' </summary>
    Public Class SerialConfig
        Public Property PortName As String = "COM3"
        Public Property BaudRate As Integer = 115200
        Public Property DataBits As Integer = 8
        Public Property Parity As IO.Ports.Parity = IO.Ports.Parity.None
        Public Property StopBits As IO.Ports.StopBits = IO.Ports.StopBits.One
        Public Property Handshake As IO.Ports.Handshake = IO.Ports.Handshake.None
        Public Property ReadTimeout As Integer = 1000
        Public Property WriteTimeout As Integer = 1000
        
        ''' <summary>
        ''' Validasi konfigurasi serial
        ''' </summary>
        Public Function IsValid() As Boolean
            Return Not String.IsNullOrEmpty(PortName) AndAlso
                   BaudRate > 0 AndAlso
                   DataBits > 0
        End Function
        
        ''' <summary>
        ''' Clone konfigurasi
        ''' </summary>
        Public Function Clone() As SerialConfig
            Return New SerialConfig With {
                .PortName = Me.PortName,
                .BaudRate = Me.BaudRate,
                .DataBits = Me.DataBits,
                .Parity = Me.Parity,
                .StopBits = Me.StopBits,
                .Handshake = Me.Handshake,
                .ReadTimeout = Me.ReadTimeout,
                .WriteTimeout = Me.WriteTimeout
            }
        End Function
    End Class
End Namespace
