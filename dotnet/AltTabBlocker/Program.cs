using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace AltTabBlocker
{
internal static class Program
{
    private const uint WM_QUIT = 0x0012;

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool PostThreadMessage(uint threadId, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool FreeConsole();

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x, y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
        public uint lPrivate;
    }

    private const string GameFileName = "BabyButtonMash.exe";

    [STAThread]
    static void Main(string[] args)
    {
        // Game exe must be in the same directory as this launcher
        string launcherDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string gamePath = Path.Combine(launcherDir ?? Environment.CurrentDirectory, GameFileName);

        if (!File.Exists(gamePath))
        {
            Console.WriteLine("Game not found: " + gamePath);
            return;
        }

        if (!AltTabBlocker.Install())
        {
            Console.WriteLine("Failed to install hook. Error: " + Marshal.GetLastWin32Error());
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = gamePath,
            WorkingDirectory = Path.GetDirectoryName(gamePath),
            UseShellExecute = false
        };
        Process gameProcess;
        try
        {
            gameProcess = Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to start game: " + ex.Message);
            AltTabBlocker.Uninstall();
            return;
        }

        uint mainThreadId = GetCurrentThreadId();
        var waitThread = new Thread(() =>
        {
            gameProcess.WaitForExit();
            PostThreadMessage(mainThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        })
        { IsBackground = true };
        waitThread.Start();

        FreeConsole();

        try
        {
            while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0) != 0)
                DispatchMessage(ref msg);
        }
        finally
        {
            AltTabBlocker.Uninstall();
        }
    }
}
}
