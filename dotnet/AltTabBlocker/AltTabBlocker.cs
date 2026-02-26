using System;
using System.Runtime.InteropServices;

namespace AltTabBlocker
{
/// <summary>
/// Uses SetWindowsHookEx (WH_KEYBOARD_LL) to intercept and block Alt+Tab, Print Screen, and Windows key on Windows.
/// Must be used from a thread that pumps messages (e.g. the main thread or a dedicated thread with a message loop).
/// </summary>
public static class AltTabBlocker
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    private const int VK_TAB = 0x09;
    private const int VK_SNAPSHOT = 0x2C;   // Print Screen
    private const int VK_LWIN = 0x5B;        // Left Windows key
    private const int VK_RWIN = 0x5C;       // Right Windows key
    private const int LLKHF_ALTDOWN = 0x20;
    private const int HC_ACTION = 0;

    private static IntPtr _hookId = IntPtr.Zero;
    private static LowLevelKeyboardProc _proc;
    private static readonly object _lock = new object();

    /// <summary>
    /// Delegate kept alive to prevent GC from collecting the hook callback.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>
    /// Installs the low-level keyboard hook and starts blocking Alt+Tab, Print Screen, and Windows key.
    /// Call from a thread that has a message loop (e.g. main thread or one running Application.Run).
    /// </summary>
    /// <returns>True if the hook was installed successfully.</returns>
    public static bool Install()
    {
        lock (_lock)
        {
            if (_hookId != IntPtr.Zero)
                return true;

            _proc = HookCallback;
            using var _ = System.Diagnostics.Process.GetCurrentProcess();
            IntPtr hMod = GetModuleHandle(null);
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hMod, 0);
            return _hookId != IntPtr.Zero;
        }
    }

    /// <summary>
    /// Removes the hook and stops blocking keys.
    /// </summary>
    /// <returns>True if the hook was removed successfully.</returns>
    public static bool Uninstall()
    {
        lock (_lock)
        {
            if (_hookId == IntPtr.Zero)
                return true;

            bool ok = UnhookWindowsHookEx(_hookId);
            if (ok)
                _hookId = IntPtr.Zero;
            return ok;
        }
    }

    /// <summary>
    /// Returns true if the hook is currently active.
    /// </summary>
    public static bool IsInstalled => _hookId != IntPtr.Zero;

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode != HC_ACTION || lParam == IntPtr.Zero)
            return CallNextHookEx(_hookId, nCode, wParam, lParam);

        var kbd = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

        bool isKeyDown = wParam == (IntPtr)WM_SYSKEYDOWN || wParam == (IntPtr)WM_KEYDOWN;
        bool isKeyUp = wParam == (IntPtr)WM_SYSKEYUP || wParam == (IntPtr)WM_KEYUP;

        // Block Alt+Tab: Tab key with Alt key down
        bool altTab = isKeyDown && kbd.vkCode == VK_TAB && (kbd.flags & LLKHF_ALTDOWN) != 0;

        // Block Print Screen (key down and key up)
        bool printScreen = (isKeyDown || isKeyUp) && kbd.vkCode == VK_SNAPSHOT;

        // Block Left and Right Windows key (key down and key up)
        bool winKey = (isKeyDown || isKeyUp) && (kbd.vkCode == VK_LWIN || kbd.vkCode == VK_RWIN);

        if (altTab || printScreen || winKey)
            return (IntPtr)1; // Block: do not pass to the rest of the hook chain or the system

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }
}
}
