using SharpDX.DirectInput;
using Z.Expressions;

namespace DeviceInputMapper;

public static class Executor
{
    public static bool ParseCondition(string condition, string id, double value)
    {
        var getDeviceState = (string id) =>
        {
            if (State.Devices.TryGetValue(id, out var value))
            {
                return value;
            }

            return new Dictionary<string, double>();
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

            return double.NaN;
        };

        State.Devices.TryGetValue(id, out var state);
        var getButtonValue = (string button) =>
        {
            if (state != null && state.TryGetValue(button, out var value))
            {
                return value;
            }

            return double.NaN;
        };

        return Eval.Execute<bool>(condition, new { value, getDeviceState, getDeviceButtonValue, getButtonValue, });
    }

    public static void ParseAction(string action, string id)
    {
        var keyClick = Keyboard.Click;
        var keyPress = Keyboard.Press;
        var keyRelease = Keyboard.Release;

        Eval.Execute(action, new
        {
            keyClick,
            keyPress,
            keyRelease,
        });
    }
}