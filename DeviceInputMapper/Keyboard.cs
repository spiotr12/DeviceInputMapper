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

    private static IDictionary<string, AutoRepeatState> _autoRepeatState = new Dictionary<string, AutoRepeatState>();

    public static void AutoRepeat(Keys key, int delay)
    {
        StopAutoRepeat(key);
        DynamicAutoRepeat(key, delay);
    }

    public static void DynamicAutoRepeat(Keys key, int delay)
    {
        var thread = new Thread(AutoRepeatFn);
        var cts = new CancellationTokenSource();

        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            var state = _autoRepeatState[key.ToString()];
            _autoRepeatState[key.ToString()] = state with { Delay = delay };
        }
        else
        {
            _autoRepeatState.Add(key.ToString(), new AutoRepeatState
            {
                Cts = cts,
                Thread = thread,
                Delay = delay,
            });

            thread.Start(new AutoRepeatParameters { Cts = cts, Key = key, Delay = delay });
        }
    }

    private static void AutoRepeatFn(object? obj)
    {
        if (obj == null)
            return;

        var parameters = (AutoRepeatParameters)obj;
        var key = parameters.Key;
        var cts = parameters.Cts;
        var selfKill = false;

        while (!cts.IsCancellationRequested && !selfKill)
        {
            if (_autoRepeatState.TryGetValue(key.ToString(), out var state))
            {
                cts = state.Cts;
                Click(key);
                Thread.Sleep(state.Delay);
            }
            else
            {
                selfKill = true;
            }
        }
    }

    public static void StopAutoRepeat(Keys key)
    {
        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            var state = _autoRepeatState[key.ToString()];

            Release(key);
            state.Cts.Cancel();
            _autoRepeatState.Remove(key.ToString());
        }
    }

    public static void StopAllAutoRepeat()
    {
        foreach (var (key, state) in _autoRepeatState)
        {
            Release(Enum.Parse<Keys>(key));
            state.Cts.Cancel();
            _autoRepeatState.Remove(key);
        }

        _autoRepeatState.Clear();
    }
}

public struct AutoRepeatParameters
{
    public Keys Key;
    public int Delay;
    public CancellationTokenSource Cts;
}

public struct AutoRepeatState
{
    public int Delay { get; set; }
    public CancellationTokenSource Cts;
    public Thread Thread;
}