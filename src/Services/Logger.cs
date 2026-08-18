namespace lblScan.Services;


public class Logger
{
    private readonly string _logFilePath;
    private StreamWriter? _writer;

    public Logger(string? logFilePath = null)
    {
        _logFilePath = logFilePath ?? Path.Combine(Directory.GetCurrentDirectory(), "lblscan.log");
    }

    public void Initialize()
    {
        if (File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }

        _writer = new StreamWriter(_logFilePath)
        {
            AutoFlush = true
        };

        Log("=========================");
        Log($"lblScan started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        Log($"Working directory: {Directory.GetCurrentDirectory()}");
        Log($"Log file: {_logFilePath}");
        Log("=========================");
    }

    public void Log(string message)
    {
        if (_writer == null)
        {
            Initialize();
        }

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        _writer?.WriteLine($"[{timestamp}] {message}");
    }

    public void LogDebug(string message)
    {
        Log($"[DEBUG] {message}");
    }

    public void LogInfo(string message)
    {
        Log($"[INFO] {message}");
    }

    public void LogWarning(string message)
    {
        Log($"[WARNING] {message}");
    }

    public void LogError(string message)
    {
        Log($"[ERROR] {message}");
    }

    public void LogError(string message, Exception ex)
    {
        Log($"[ERROR] {message}");
        Log($"[ERROR] Exception: {ex.GetType().Name}: {ex.Message}");
        Log($"[DEBUG] StackTrace: {ex.StackTrace}");
    }

    public void Dispose()
    {
        if (_writer != null)
        {
            Log("=========================");
            Log($"lblScan finished at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            Log("=========================");
            _writer.Dispose();
            _writer = null;
        }
    }
}