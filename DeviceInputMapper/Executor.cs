using System.Dynamic;
using Z.Expressions;

namespace DeviceInputMapper;

public static class Executor
{
    private static readonly IDictionary<string, object> _dynamicState = new Dictionary<string, object>();

    public static bool ParseCondition(string? condition, string id, double value, object rawValue)
    {
        var getDeviceState = (string id) =>
        {
            if (State.Devices.TryGetValue(id, out var value))
            {
                return value;
            }

            return new Dictionary<string, (double value, object rawValue)>();
        };

        var getDeviceButtonState = (string id, string button) =>
        {
            if (State.Devices.TryGetValue(id, out var st))
            {
                if (st.TryGetValue(button, out var stateValue))
                {
                    return new
                    {
                        value = stateValue.value,
                        rawValue = (object)stateValue.rawValue,
                    };
                }
            }

            return new { value = double.NaN, rawValue = (object)double.NaN };
        };

        var getDeviceButtonValue = (string id, string button) => { return getDeviceButtonState(id, button).value; };
        var getDeviceButtonRawValue = (string id, string button) => { return getDeviceButtonState(id, button).rawValue; };

        State.Devices.TryGetValue(id, out var deviceState);
        var getButtonState = (string button) =>
        {
            if (deviceState != null && deviceState.TryGetValue(button, out var stateValue))
            {
                return new
                {
                    value = stateValue.value,
                    rawValue = (object)stateValue.rawValue,
                };
            }

            return new { value = double.NaN, rawValue = (object)double.NaN };
        };

        var getButtonValue = (string button) => { return getButtonState(button).value; };
        var getButtonRawValue = (string button) => { return getButtonState(button).rawValue; };

        var stateToString = () => State.DeviceToString(id);
        var deviceStateToString = (string i) => State.DeviceToString(i);
        var globalStateToString = () => State.ToString();


        var getDynamicStateValue = (string key) =>
        {
            try
            {
                return _dynamicState[key];
            }
            catch (Exception e)
            {
            }

            return null;
        };
        var setDynamicStateValue = (string key, object value) =>
        {
            if (_dynamicState.TryGetValue("myVar", out var myVar))
            {
                _dynamicState["myVar"] = value;
            }
            else
            {
                _dynamicState.Add("myVar", value);
            }
        };

        var log = (object msg) => Console.WriteLine(msg);

        var test = () =>
        {
            Console.WriteLine("TESTING");
            Console.WriteLine(getButtonState("Buttons9"));
            return true;
        };

        if (condition == null || condition.ToLower().Equals("true"))
        {
            return true;
        }

        try
        {
            return Eval.Execute<bool>(condition, new
            {
                id,
                value,
                rawValue,
                mode = State.Mode,
                state = State.GetDevice(id),
                globalState = State.Devices,

                getDynamicStateValue,
                setDynamicStateValue,

                stateToString,
                deviceStateToString,
                globalStateToString,

                getDeviceButtonValue,
                getDeviceButtonRawValue,

                getButtonValue,
                getButtonRawValue,

                log,
                test,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }

    public static void ParseAction(string action, string id, double value, object rawValue)
    {
        State.Devices.TryGetValue(id, out var deviceState);

        var keyClick = (string key) => Keyboard.Click(Enum.Parse<Keys>(key));
        var keyHold = (string key, int delay) => Keyboard.Hold(Enum.Parse<Keys>(key), delay);
        var keyPress = (string key) => Keyboard.Press(Enum.Parse<Keys>(key));
        var keyRelease = (string key) => Keyboard.Release(Enum.Parse<Keys>(key));

        var keyAutoRepeat = (string key, int delay) => Keyboard.AutoRepeat(Enum.Parse<Keys>(key), delay);
        var keyDynamicAutoRepeat = (string key, int delay) => Keyboard.DynamicAutoRepeat(Enum.Parse<Keys>(key), delay);
        var keyStopAutoRepeat = (string key) => Keyboard.StopAutoRepeat(Enum.Parse<Keys>(key));
        var keyStopAllAutoRepeat = () => Keyboard.StopAllAutoRepeat();

        var stateToString = () => State.DeviceToString(id);
        var deviceStateToString = (string i) => State.DeviceToString(i);
        var globalStateToString = () => State.ToString();

        var getDynamicStateValue = (string key) =>
        {
            try
            {
                return _dynamicState[key];
            }
            catch (Exception e)
            {
            }

            return null;
        };
        var setDynamicStateValue = (string key, object value) =>
        {
            if (_dynamicState.TryGetValue("myVar", out var myVar))
            {
                _dynamicState["myVar"] = value;
            }
            else
            {
                _dynamicState.Add("myVar", value);
            }
        };

        var log = (object msg) => Console.WriteLine(msg);

        try
        {
            Eval.Execute(action, new
            {
                id,
                value,
                rawValue,
                mode = State.Mode,
                state = State.GetDevice(id),
                globalState = State.Devices,

                getDynamicStateValue,
                setDynamicStateValue,

                stateToString,
                deviceStateToString,
                globalStateToString,

                keyClick,
                keyHold,
                keyPress,
                keyRelease,

                keyAutoRepeat,
                keyDynamicAutoRepeat,
                keyStopAutoRepeat,
                keyStopAllAutoRepeat,

                log,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}