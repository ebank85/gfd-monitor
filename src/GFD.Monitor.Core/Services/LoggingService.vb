Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports GFD.Monitor.Core.Models
Imports GFD.Monitor.Core.Utils

Namespace Services
    ''' <summary>
    ''' Service untuk event logging ke CSV
    ''' </summary>
    Public Class LoggingService
        Implements IDisposable
        
        Private _events As New List(Of EventLog)
        Private _batchBuffer As New List(Of EventLog)
        Private _syncLock As New Object()
        Private _lastFlushTime As DateTime = DateTime.Now
        
        Public Sub New()
            ' Ensure log directory exists
            Try
                Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.LOG_DIRECTORY)
                If Not Directory.Exists(logDir) Then
                    Directory.CreateDirectory(logDir)
                End If
            Catch ex As Exception
                Logger.LogWarning($"Could not create log directory: {ex.Message}")
            End Try
        End Sub
        
        ''' <summary>
        ''' Log event ke memory buffer
        ''' </summary>
        Public Sub LogEvent(eventLog As EventLog)
            SyncLock _syncLock
                _events.Add(eventLog)
                _batchBuffer.Add(eventLog)
                
                Logger.LogInfo($"Event logged: {eventLog.EventType} - {eventLog.Detail}")
                
                ' Auto-flush jika buffer penuh atau timeout
                If _batchBuffer.Count >= Constants.CSV_BATCH_SIZE OrElse
                   DateTime.Now.Subtract(_lastFlushTime).TotalMilliseconds >= Constants.CSV_BATCH_TIMEOUT_MS Then
                    FlushToFile()
                End If
            End SyncLock
        End Sub
        
        ''' <summary>
        ''' Get event history
        ''' </summary>
        Public Function GetEventHistory() As List(Of EventLog)
            SyncLock _syncLock
                Return New List(Of EventLog)(_events)
            End SyncLock
        End Function
        
        ''' <summary>
        ''' Get event by type
        ''' </summary>
        Public Function GetEventsByType(eventType As EventLog.EventType) As List(Of EventLog)
            SyncLock _syncLock
                Return _events.Where(Function(e) e.EventType = eventType).ToList()
            End SyncLock
        End Function
        
        ''' <summary>
        ''' Get event dalam range tanggal
        ''' </summary>
        Public Function GetEventsByDateRange(dateFrom As DateTime, dateTo As DateTime) As List(Of EventLog)
            SyncLock _syncLock
                Return _events.Where(Function(e) e.Timestamp >= dateFrom AndAlso e.Timestamp <= dateTo).ToList()
            End SyncLock
        End Function
        
        ''' <summary>
        ''' Clear semua events
        ''' </summary>
        Public Sub ClearEvents()
            SyncLock _syncLock
                _events.Clear()
                _batchBuffer.Clear()
            End SyncLock
        End Sub
        
        ''' <summary>
        ''' Export ke CSV file
        ''' </summary>
        Public Function ExportToCsv(Optional filename As String = Nothing) As Boolean
            Try
                SyncLock _syncLock
                    If String.IsNullOrEmpty(filename) Then
                        filename = CsvExporter.GenerateFilename()
                    End If
                    
                    Dim fullPath As String = CsvExporter.GetCsvPath(filename)
                    Dim result As Boolean = CsvExporter.ExportToCsv(_events, fullPath)
                    
                    If result Then
                        Logger.LogInfo($"Exported {_events.Count} events to {fullPath}")
                    Else
                        Logger.LogWarning($"Failed to export to {fullPath}")
                    End If
                    
                    Return result
                End SyncLock
            Catch ex As Exception
                Logger.LogError($"Export error: {ex.Message}", ex)
                Return False
            End Try
        End Function
        
        ''' <summary>
        ''' Flush buffer ke CSV file
        ''' </summary>
        Private Sub FlushToFile()
            If _batchBuffer.Count = 0 Then Return
            
            Try
                Dim filename As String = CsvExporter.GenerateFilename()
                Dim fullPath As String = CsvExporter.GetCsvPath(filename)
                
                ' Append ke file yang existing
                Using writer As New StreamWriter(fullPath, True)
                    For Each evt In _batchBuffer
                        writer.WriteLine(evt.ToCsvLine())
                    Next
                    writer.Flush()
                End Using
                
                _batchBuffer.Clear()
                _lastFlushTime = DateTime.Now
                
            Catch ex As Exception
                Logger.LogWarning($"Failed to flush to file: {ex.Message}")
            End Try
        End Sub
        
        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _syncLock
                FlushToFile()
            End SyncLock
        End Sub
    End Class
End Namespace
