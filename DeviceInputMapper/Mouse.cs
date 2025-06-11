using System.Runtime.InteropServices;

namespace DeviceInputMapper;

public class Mouse
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern void mouse_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
}