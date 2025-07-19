using System.ComponentModel;
using System.Diagnostics;

namespace DeviceInputMapper;

public class Window
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    public static Process? GetForegroundProcess()
    {
        IntPtr hwnd = GetForegroundWindow();

        // The foreground window can be NULL in certain circumstances,
        // such as when a window is losing activation.
        if (hwnd == null)
            return null;

        uint pid;
        GetWindowThreadProcessId(hwnd, out pid);

        foreach (Process p in Process.GetProcesses())
        {
            if (p.Id == pid)
                return p;
        }

        return null;
    }

    public static string GetForegroundWindowFilePath()
    {
        try
        {
            var proc = GetForegroundProcess();
            if (proc == null || proc.MainModule == null)
            {
                return "Unknown";
            }

            return proc.MainModule.FileName;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }

        return "Unknown";
    }
}