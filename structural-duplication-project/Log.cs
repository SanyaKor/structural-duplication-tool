using System;
using System.IO;

namespace StructuralDuplication;

public enum LogLevel { Trace = 0, Debug = 1, Info = 2, Warn = 3, Error = 4, Fatal = 5 }

public static class log
{
    static readonly object _sync = new();
    static LogLevel _min = LogLevel.Info;
    static TextWriter? _file;
    static bool _useColors = true;

    public static void setup(LogLevel minimum = LogLevel.Info, string? filePath = null, bool useColors = true)
    {
        _min = minimum;
        _useColors = useColors;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
            _file = TextWriter.Synchronized(new StreamWriter(filePath, append: true) { AutoFlush = true });
        }
        info("[Logger] logger initialized (min={0}{1})",
            minimum,
            filePath is null ? "" : $", file='{Path.GetFullPath(filePath)}'");
        AppDomain.CurrentDomain.ProcessExit += (_, __) => _file?.Dispose();
        Console.CancelKeyPress += (_, __) => _file?.Dispose();
    }

    public static void trace(string msg, params object[] a) => Write(LogLevel.Trace, msg, a);
    public static void debug(string msg, params object[] a) => Write(LogLevel.Debug, msg, a);
    public static void info (string msg, params object[] a) => Write(LogLevel.Info , msg, a);
    public static void warn (string msg, params object[] a) => Write(LogLevel.Warn , msg, a);
    public static void error(string msg, params object[] a) => Write(LogLevel.Error, msg, a);
    public static void error(Exception ex, string msg = "error", params object[] a) => Write(LogLevel.Error, msg + " :: " + ex, a);
    public static void fatal(string msg, params object[] a) => Write(LogLevel.Fatal, msg, a);
    public static void fatal(Exception ex, string msg = "fatal", params object[] a) => Write(LogLevel.Fatal, msg + " :: " + ex, a);

    static void Write(LogLevel level, string msg, params object[] a)
    {
        if (level < _min) return;
        var ts = DateTime.Now.ToString("HH:mm:ss");
        var lvl = level.ToString().ToUpper();
        var line = a is { Length: > 0 } ? string.Format(msg, a) : msg;

        lock (_sync)
        {
            if (_useColors) PushColor(level);
            Console.WriteLine($"[{ts} {lvl}] {line}");
            if (_useColors) Console.ResetColor();

            _file?.WriteLine($"[{ts} {lvl}] {line}");
        }
    }

    static void PushColor(LogLevel level)
    {
        ConsoleColor c = level switch
        {
            LogLevel.Trace => ConsoleColor.DarkGray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info  => ConsoleColor.White,
            LogLevel.Warn  => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };
        Console.ForegroundColor = c;
    }
}