Imports System
Imports System.IO
Imports System.Text

Namespace Utils
    ''' <summary>
    ''' Simple logging utility untuk debugging
    ''' </summary>
    Public NotInheritable Class Logger
        Private Shared ReadOnly _logFilePath As String = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log")
        Private Shared ReadOnly _syncLock As New Object()
        
        Public Enum LogLevel
            Debug
            Info
            Warning
            [Error]
            Critical
        End Enum
        
        Shared Sub New()
            Try
                Dim dir As String = Path.GetDirectoryName(_logFilePath)
                If Not Directory.Exists(dir) Then
                    Directory.CreateDirectory(dir)
                End If
            Catch
                ' Ignore if we can't create log directory
            End Try
        End Sub
        
        ''' <summary>
        ''' Write log message
        ''' </summary>
        Public Shared Sub Log(level As LogLevel, message As String, Optional ex As Exception = Nothing)
            SyncLock _syncLock
                Try
                    Dim logMessage As String = FormatLogMessage(level, message, ex)
                    
                    ' Console output
                    Console.WriteLine(logMessage)
                    
                    ' File output
                    Try
                        File.AppendAllText(_logFilePath, logMessage & Environment.NewLine, Encoding.UTF8)
                    Catch
                        ' Ignore file write errors
                    End Try
                Catch
                    ' Ignore any logging errors
                End Try
            End SyncLock
        End Sub
        
        Public Shared Sub LogInfo(message As String)
            Log(LogLevel.Info, message)
        End Sub
        
        Public Shared Sub LogWarning(message As String)
            Log(LogLevel.Warning, message)
        End Sub
        
        Public Shared Sub LogError(message As String, Optional ex As Exception = Nothing)
            Log(LogLevel.Error, message, ex)
        End Sub
        
        Public Shared Sub LogDebug(message As String)
            Log(LogLevel.Debug, message)
        End Sub
        
        Public Shared Sub LogCritical(message As String, Optional ex As Exception = Nothing)
            Log(LogLevel.Critical, message, ex)
        End Sub
        
        Private Shared Function FormatLogMessage(level As LogLevel, message As String, 
                                                  Optional ex As Exception = Nothing) As String
            Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            Dim levelStr As String = level.ToString().ToUpper()
            Dim result As String = $"[{timestamp}] [{levelStr}] {message}"
            
            If ex IsNot Nothing Then
                result &= Environment.NewLine & $"Exception: {ex.GetType().Name}" & Environment.NewLine
                result &= $"Message: {ex.Message}" & Environment.NewLine
                result &= $"StackTrace: {ex.StackTrace}"
            End If
            
            Return result
        End Function
    End Class
End Namespace
