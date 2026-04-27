Imports System
Imports System.IO
Imports System.Text
Imports GFD.Monitor.Core.Models

Namespace Utils
    ''' <summary>
    ''' Utility untuk export event log ke CSV
    ''' </summary>
    Public NotInheritable Class CsvExporter
        
        ''' <summary>
        ''' Export event log ke file CSV
        ''' </summary>
        Public Shared Function ExportToCsv(events As List(Of EventLog), filename As String) As Boolean
            Try
                ' Ensure directory exists
                Dim dirPath As String = Path.GetDirectoryName(filename)
                If Not String.IsNullOrEmpty(dirPath) AndAlso Not Directory.Exists(dirPath) Then
                    Directory.CreateDirectory(dirPath)
                End If
                
                Using writer As New StreamWriter(filename, False, Encoding.UTF8)
                    ' Write header
                    writer.WriteLine("Timestamp,Event,Current(A),State,Relay,Detail")
                    
                    ' Write data rows
                    For Each evt In events
                        writer.WriteLine(evt.ToCsvLine())
                    Next
                    
                    writer.Flush()
                End Using
                
                Return True
                
            Catch ex As Exception
                Return False
            End Try
        End Function
        
        ''' <summary>
        ''' Generate CSV filename dengan timestamp
        ''' </summary>
        Public Shared Function GenerateFilename() As String
            Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd")
            Dim filename As String = $"{Constants.LOG_FILE_PREFIX}{timestamp}{Constants.LOG_FILE_EXTENSION}"
            Dim filepath As String = Path.Combine(Constants.LOG_DIRECTORY, filename)
            Return filepath
        End Function
        
        ''' <summary>
        ''' Get full path untuk file CSV
        ''' </summary>
        Public Shared Function GetCsvPath(Optional filename As String = Nothing) As String
            If String.IsNullOrEmpty(filename) Then
                filename = GenerateFilename()
            End If
            
            If Path.IsPathRooted(filename) Then
                Return filename
            Else
                Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)
            End If
        End Function
    End Class
End Namespace
