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

    private static int _stateTest = 0;

    public static void AutoRepeat(Keys key, int delay)
    {
        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            _autoRepeatState[key.ToString()].CancellationTokenSource.Cancel();
            _autoRepeatState.Remove(key.ToString());
        }

        var state = new AutoRepeatState
        {
            CancellationTokenSource = new CancellationTokenSource(),
            Delay = delay
        };
        if (!_autoRepeatState.TryAdd(key.ToString(), state))
        {
            _autoRepeatState[key.ToString()] = state;
        }
        else
        {
            keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
            Thread.Sleep(state.Delay);
        }

        Task.Run(() =>
        {
            while (_autoRepeatState.TryGetValue(key.ToString(), out var latestState)
                   && !latestState.CancellationTokenSource.IsCancellationRequested)
            {
                keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
                Thread.Sleep(latestState.Delay);
            }
        }, state.CancellationTokenSource.Token);
    }

    public static void DynamicAutoRepeat(Keys key, int delay)
    {
        AutoRepeatState state;
        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            state = _autoRepeatState[key.ToString()];
            // Console.WriteLine("Current State {0}", state.Delay);
            state.Delay = delay;
            Console.WriteLine("State test: {0}", _stateTest++);
        }
        else
        {
            Console.WriteLine("State test: {0}", _stateTest++);
            state = new AutoRepeatState
            {
                CancellationTokenSource = new CancellationTokenSource(),
                Delay = delay
            };
            _autoRepeatState.Add(key.ToString(), state);

            keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
            Thread.Sleep(state.Delay);

            Task.Run(() =>
            {
                AutoRepeatState latestState;
                while (_autoRepeatState.TryGetValue(key.ToString(), out latestState)
                       && !latestState.CancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("CHECK: {0}", _stateTest);
                    keybd_event((byte)key, 0, KEY_DOWN_EVENT, 0);
                    _autoRepeatState.TryGetValue(key.ToString(), out latestState);
                    Console.WriteLine(latestState.Delay);
                    Thread.Sleep(latestState.Delay);
                }
            }, state.CancellationTokenSource.Token);
        }
    }

    public static void StopAutoRepeat(Keys key)
    {
        if (_autoRepeatState.ContainsKey(key.ToString()))
        {
            _autoRepeatState[key.ToString()].CancellationTokenSource.Cancel();
            _autoRepeatState.Remove(key.ToString());
        }
    }

    public static void StopAllAutoRepeat()
    {
        foreach (var (key, cancellationTokenSource) in _autoRepeatState)
        {
            cancellationTokenSource.CancellationTokenSource.Cancel();
            _autoRepeatState.Remove(key);
        }

        _autoRepeatState.Clear();
    }
}

public struct AutoRepeatState
{
    public CancellationTokenSource CancellationTokenSource { get; set; }
    public int Delay { get; set; }
}