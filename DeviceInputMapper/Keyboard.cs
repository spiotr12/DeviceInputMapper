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

    private static IDictionary<string, CancellationTokenSource> _autoRepeatState = new Dictionary<string, CancellationTokenSource>();

    public static void AutoRepeat(Keys key, int delay)
    {
        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            Console.WriteLine("Cancel autorepeat for {0}", key);
            _autoRepeatState[key.ToString()].Cancel();
            _autoRepeatState.Remove(key.ToString());
        }

        var cancellationTokenSource = new CancellationTokenSource();
        if (!_autoRepeatState.TryAdd(key.ToString(), cancellationTokenSource))
        {
            _autoRepeatState[key.ToString()] = cancellationTokenSource;
        }

        Task.Run(() =>
        {
            Console.WriteLine("Start autorepeat for {0} with {1}ms delay", key, delay);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                keybd_event((byte)key, 0, KEY_UP_EVENT, 0);
                Thread.Sleep(delay);
            }
        }, cancellationTokenSource.Token);
    }

    public static void StopAutoRepeat(Keys key)
    {
        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            Console.WriteLine("Cancel autorepeat for {0}", key);
            _autoRepeatState[key.ToString()].Cancel();
            _autoRepeatState.Remove(key.ToString());
        }
    }

    public static void StopAllAutoRepeat()
    {
        foreach (var (key, cancellationTokenSource) in _autoRepeatState)
        {
            cancellationTokenSource.Cancel();
            _autoRepeatState.Remove(key);
        }

        _autoRepeatState.Clear();
    }
}