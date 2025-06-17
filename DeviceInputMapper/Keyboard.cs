using System.Runtime.InteropServices;

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

    public static void Hold(Keys key, int holdTime)
    {
        keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
        Thread.Sleep(holdTime);
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

    public static void AutoRepeatClick(Keys key, int delay)
    {
        StopAutoRepeat(key);
        DynamicAutoRepeatClick(key, delay);
    }

    public static void DynamicAutoRepeatClick(Keys key, int delay)
    {
        var thread = new Thread(AutoRepeatClickFn);
        var cts = new CancellationTokenSource();

        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            var state = _autoRepeatState[key.ToString()];
            _autoRepeatState[key.ToString()] = state with { Delay = delay, PreviousDelay = state.Delay };
        }
        else
        {
            _autoRepeatState.Add(key.ToString(), new AutoRepeatState
            {
                Cts = cts,
                Thread = thread,
                Delay = delay,
                PreviousDelay = delay,
            });

            thread.Start(new AutoRepeatParameters { Cts = cts, Key = key, Delay = delay });
        }
    }

    public static void DynamicAutoRepeatClickMinMaxTime(Keys key, double value, int minTime, int maxTime)
    {
        var delay = (int)Math.Round(maxTime * (1 - Math.Abs(value))) + minTime;
        DynamicAutoRepeatClick(key, delay);
    }

    private static void AutoRepeatClickFn(object? obj)
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

    public static void AutoRepeatHold(Keys key, int delay, int holdTime)
    {
        StopAutoRepeat(key);
        DynamicAutoRepeatHold(key, delay, holdTime);
    }

    public static void DynamicAutoRepeatHold(Keys key, int delay, int holdTime)
    {
        var thread = new Thread(AutoRepeatHoldFn);
        var cts = new CancellationTokenSource();

        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            var state = _autoRepeatState[key.ToString()];
            _autoRepeatState[key.ToString()] = state with { Delay = delay, PreviousDelay = state.Delay };
        }
        else
        {
            _autoRepeatState.Add(key.ToString(), new AutoRepeatState
            {
                Cts = cts,
                Thread = thread,
                Delay = delay,
                PreviousDelay = delay,
            });

            thread.Start(new AutoRepeatParameters { Cts = cts, Key = key, Delay = delay, HoldTime = holdTime });
        }
    }

    public static void DynamicAutoRepeatHoldMinMaxTime(Keys key, double value, int minTime, int maxTime, int holdTime)
    {
        var delay = (int)Math.Round(maxTime * (1 - Math.Abs(value))) + minTime;
        var absoluteDelay = delay < 0 ? 0 : delay;
        int newHoldTime = holdTime;
        if (holdTime == -1)
        {
            var maxHoldTime = Math.Abs(minTime) + Math.Abs(maxTime);
            newHoldTime = maxHoldTime - (int)Math.Round(maxHoldTime * (1 - Math.Abs(value)));
            Console.WriteLine(newHoldTime);
        }

        DynamicAutoRepeatHold(key, absoluteDelay, newHoldTime);
    }

    private static void AutoRepeatHoldFn(object? obj)
    {
        if (obj == null)
            return;

        var parameters = (AutoRepeatParameters)obj;
        var key = parameters.Key;
        var cts = parameters.Cts;
        var holdTime = parameters.HoldTime ?? 0;
        var selfKill = false;

        while (!cts.IsCancellationRequested && !selfKill)
        {
            if (_autoRepeatState.TryGetValue(key.ToString(), out var state))
            {
                cts = state.Cts;
                // Console.WriteLine($"{state.Delay}, {state.PreviousDelay}");

                if (state.Delay <= 0 && !state.HoldPressed)
                {
                    // Console.WriteLine("Press");
                    _autoRepeatState[key.ToString()] = state with { HoldPressed = true };
                    Press(key);
                }
                else if (state.Delay > 0 && state.HoldPressed)
                {
                    // Console.WriteLine("Release");
                    _autoRepeatState[key.ToString()] = state with { HoldPressed = false };
                    Release(key);
                }

                if (state.Delay > 0)
                {
                    // Console.WriteLine("Hold start");
                    Hold(key, holdTime);
                    // Console.WriteLine("Hold end");
                    Thread.Sleep(state.Delay >= 0 ? state.Delay : 0);
                }
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
    public int? HoldTime { get; set; }
    public CancellationTokenSource Cts;
}

public struct AutoRepeatState
{
    public int Delay { get; set; }
    public int PreviousDelay { get; set; }
    public int? HoldTime { get; set; }
    public bool HoldPressed { get; set; }
    public CancellationTokenSource Cts;
    public Thread Thread;
}