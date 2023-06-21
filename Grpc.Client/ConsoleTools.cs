using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Client;

static class ConsoleTools
{
    #region MinMax window

    const Int32 SW_MINIMIZE = 6;
    const Int32 SW_MAXIMIZE = 3;

    [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow([In] IntPtr hWnd, [In] Int32 nCmdShow);

    public static void MinimizeConsoleWindow()
    {
        IntPtr hWndConsole = GetConsoleWindow();
        ShowWindow(hWndConsole, SW_MINIMIZE);
    }

    public static void MaximizeConsoleWindow()
    {
        IntPtr hWndConsole = GetConsoleWindow();
        ShowWindow(hWndConsole, SW_MAXIMIZE);
    }

    #endregion

    private static readonly ConsoleColor _bc = Console.BackgroundColor;
    private static readonly ConsoleColor _fc = Console.ForegroundColor;
    private static readonly DateTime _start = DateTime.Now;
    private static readonly BlockingCollection<(string, ConsoleColor?, ConsoleColor?)> _logQueue = new();

    static ConsoleTools()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        Task.Run(() =>
        {
            int logCnt = 0;
            while (!_logQueue.IsCompleted)
            {
                if (!_logQueue.TryTake(out var item))
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (item.Item1 == "inc")
                {
                    LogCountValue(logCnt++);
                    continue;
                }

                LogLine(item);
            }
        });
    }

    public static void WriteCounter()
    {
        Console.Write("Log counter: ");
        (cntLeft, cntTop) = Console.GetCursorPosition();
        Console.WriteLine();
    }

    private static DateTime lastCntLog;
    private static int cntTop;
    private static int cntLeft;
    private static void LogCountValue(int cnt)
    {
        if ((DateTime.Now - lastCntLog).TotalMilliseconds < 100)
        {
            return;
        }

        lastCntLog = DateTime.Now;
        LogCount?.Invoke(cnt);

        if (cntLeft > 0 || cntTop > 0)
        {
            var (l, t) = Console.GetCursorPosition();
            Console.SetCursorPosition(cntLeft, cntTop);
            Console.Write(cnt);
            Console.SetCursorPosition(l, t);
        }
    }

    public static event Action<string>? OnLog;

    private static DateTime _last = DateTime.MinValue;
    private static void LogLine((string, ConsoleColor?, ConsoleColor?) item)
    {
        OnLog?.Invoke(item.Item1);

        if ((DateTime.Now - _last).TotalMilliseconds > 100)
        {
            _last = DateTime.Now;
            ConsoleTools.LogCore("---------------------------------------------------------------", ConsoleColor.Green);
        }

        ConsoleTools.LogCore(item.Item1, item.Item2, item.Item3);
    }

    private static void LogCore(string str, ConsoleColor? fColor = null, ConsoleColor? bColor = null)
    {
        var ms = (DateTime.Now - _start).TotalSeconds;
        Console.BackgroundColor = bColor ?? _bc;
        Console.ForegroundColor = fColor ?? _fc;
        Console.WriteLine($"{ms,6:N2} - {str}");
        Console.BackgroundColor = _bc;
        Console.ForegroundColor = _fc;
    }

    public static void Log(string str, ConsoleColor? fColor = null, ConsoleColor? bColor = null)
    {
        _logQueue.Add((str, fColor, bColor));
    }

    public static void LogCountInc()
    {
        _logQueue.Add(("inc", null, null));
    }

    public static event Action<int>? LogCount;
}