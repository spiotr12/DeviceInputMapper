using Z.Expressions;

namespace DeviceInputMapper;

public static class Executor
{
    public static bool ParseCondition(string condition, string id, double value, double rawValue)
    {
        var getDeviceState = (string id) =>
        {
            if (State.Devices.TryGetValue(id, out var value))
            {
                return value;
            }

            return new Dictionary<string, (double value, double rawValue)>();
        };

        var getDeviceButtonValue = (string id, string button) =>
        {
            if (State.Devices.TryGetValue(id, out var st))
            {
                if (st.TryGetValue(button, out var value))
                {
                    return value;
                }
            }

            return (value: double.NaN, rawValue: double.NaN);
        };

        State.Devices.TryGetValue(id, out var state);
        var getButtonValue = (string button) =>
        {
            if (state != null && state.TryGetValue(button, out var value))
            {
                return value;
            }

            return (value: double.NaN, rawValue: double.NaN);
        };

        var log = (string msg) => Console.WriteLine(msg.ToString());

        return Eval.Execute<bool>(condition, new
        {
            value,
            rawValue,

            getDeviceState,
            getDeviceButtonValue,
            getButtonValue,

            log,
        });
    }

    public static void ParseAction(string action, string id)
    {
        var keyClick = Keyboard.Click;
        var keyPress = Keyboard.Press;
        var keyRelease = Keyboard.Release;
        var keyAutoRepeat = Keyboard.AutoRepeat;
        var keyStopAutoRepeat = Keyboard.StopAutoRepeat;
        var keyStopAllAutoRepeat = Keyboard.StopAllAutoRepeat;
        var log = (string msg) => Console.WriteLine(msg.ToString());

        Eval.Execute(action, new
        {
            keyClick,
            keyPress,
            keyRelease,

            keyAutoRepeat,
            keyStopAutoRepeat,
            keyStopAllAutoRepeat,

            log,
        });
    }
}