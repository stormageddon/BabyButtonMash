# Alt+Tab / Print Screen / Windows Key Blocker

Uses the Windows API `SetWindowsHookEx` with `WH_KEYBOARD_LL` to intercept and block the following system-wide while the program is running:

- **Alt+Tab** (task switcher)
- **Print Screen**
- **Left and Right Windows key**

## Usage

### Run the game with the blocker

The launcher always starts **BabyButtonMash.exe** from the same folder as the launcher. No arguments needed.

1. Export your Godot game as `BabyButtonMash.exe` (or ensure your export uses that name).
2. Copy `AltTabBlocker.exe` into the **same folder** as `BabyButtonMash.exe`. Use the single exe from `bin\Release\netcoreapp3.1\win-x64\publish\` (after `dotnet publish -c Release`) so you don’t need any DLLs.
3. Run `AltTabBlocker.exe`. It will start the game with Alt+Tab, Print Screen, and Windows key blocked. When the player closes the game, the launcher exits and keys work normally again.

Double-click `AltTabBlocker.exe` (or a shortcut to it) to play with the blocker active.

### Use the class in your own app

- Call `AltTabBlocker.Install()` from a thread that has a Windows message loop (e.g. main thread, or a thread running `Application.Run()` or a `GetMessage`/`DispatchMessage` loop).
- Call `AltTabBlocker.Uninstall()` before exit.

Blocked keys: Alt+Tab, Print Screen, Left/Right Windows key.

## Building

- **Single exe (for distribution):** Run `dotnet publish -c Release`. The output is **one file**, `AltTabBlocker.exe`, in `bin\Release\netcoreapp3.1\win-x64\publish\`. Copy only that exe next to your game; no DLLs needed. The exe is larger (~66 MB) because it includes the .NET runtime.
- **Development build:** `dotnet build` produces many DLLs in `bin\Debug\netcoreapp3.1\win-x64\` (the normal .NET layout). Use this when developing; use **publish** when you want to ship or run without the SDK.

## Requirements

- Windows
- .NET Core 3.1 SDK (or later) to build

## Notes

- The launcher looks for `BabyButtonMash.exe` in the same directory as `AltTabBlocker.exe`. It keeps a Windows message loop running in the background (with the console hidden) so the hook keeps working until the game process exits.
- Some security software may flag low-level keyboard hooks.
