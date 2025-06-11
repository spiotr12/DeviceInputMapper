using System.Runtime.InteropServices;
using SharpDX.DirectInput;

namespace DeviceInputMapper;

public class Keyboard
{
    // [DllImport("user32.dll", SetLastError = true)]
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    public static readonly int KEY_DOWN_EVENT = 0x0001;
    public static readonly int KEY_UP_EVENT = 0x0002;

    public static void Click(Keys key)
    {
        keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
        keybd_event((byte)key, 0, KEY_UP_EVENT, 0);
    }

    public static void Press(Keys key)
    {
        keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
    }

    public static void Release(Keys key)
    {
        keybd_event((byte)key, 0, KEY_UP_EVENT, 0);
    }
}