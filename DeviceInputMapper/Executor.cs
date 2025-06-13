using Z.Expressions;

namespace DeviceInputMapper;

public struct HelperFunctions
{
    public Func<string, IDictionary<string, StateValue>> getDeviceState { get; set; }
    public Func<string, string, StateValue> getDeviceButtonState { get; set; }
    public Func<string, string, double> getDeviceButtonValue { get; set; }
    public Func<string, string, object> getDeviceButtonRawValue { get; set; }
    public Func<string, StateValue> getButtonState { get; set; }
    public Func<string, double> getButtonValue { get; set; }
    public Func<string, object> getButtonRawValue { get; set; }
    public Func<string> stateToString { get; set; }
    public Func<string, string> deviceStateToString { get; set; }
    public Func<string> globalStateToString { get; set; }
    public Func<string, object?> getDynamicStateValue { get; set; }
    public Action<string, object> setDynamicStateValue { get; set; }

    public Action<string> keyClick { get; set; }
    public Action<string, int> keyHold { get; set; }
    public Action<string> keyPress { get; set; }
    public Action<string> keyRelease { get; set; }
    public Action<string, int> keyAutoRepeat { get; set; }
    public Action<string, int> keyDynamicAutoRepeat { get; set; }
    public Action<string> keyStopAutoRepeat { get; set; }
    public Action keyStopAllAutoRepeat { get; set; }

    public Action<object> log { get; set; }
    public Func<bool> test { get; set; }
}

public static class Executor
{
    private static readonly IDictionary<string, object> _dynamicState = new Dictionary<string, object>();

    private static HelperFunctions GetHelperFunctions(string id)
    {
        var getDeviceState = (string identifier) =>
        {
            if (State.Devices.TryGetValue(identifier, out var stateValue))
            {
                return stateValue;
            }

            return new Dictionary<string, StateValue>();
        };

        var getDeviceButtonState = (string identifier, string button) =>
        {
            if (State.Devices.TryGetValue(identifier, out var st))
            {
                if (st.TryGetValue(button, out var stateValue))
                {
                    return new StateValue
                    {
                        value = stateValue.value,
                        rawValue = (object)stateValue.rawValue,
                    };
                }
            }

            return new StateValue { value = double.NaN, rawValue = (object)double.NaN };
        };

        var getDeviceButtonValue = (string identifier, string button) => { return getDeviceButtonState(identifier, button).value; };
        var getDeviceButtonRawValue = (string identifier, string button) => { return getDeviceButtonState(identifier, button).rawValue; };

        State.Devices.TryGetValue(id, out var deviceState);
        var getButtonState = (string button) =>
        {
            if (deviceState != null && deviceState.TryGetValue(button, out var stateValue))
            {
                return new StateValue
                {
                    value = stateValue.value,
                    rawValue = (object)stateValue.rawValue,
                };
            }

            return new StateValue { value = double.NaN, rawValue = (object)double.NaN };
        };

        var getButtonValue = (string button) => { return getButtonState(button).value; };
        var getButtonRawValue = (string button) => { return getButtonState(button).rawValue; };

        var stateToString = () => State.DeviceToString(id);
        var deviceStateToString = (string identifier) => State.DeviceToString(identifier);
        var globalStateToString = () => State.ToString();

        // Get dynamic state value
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

        // Save dynamic state value
        var setDynamicStateValue = (string key, object value) =>
        {
            if (_dynamicState.ContainsKey(key))
            {
                _dynamicState[key] = value;
            }
            else
            {
                _dynamicState.Add(key, value);
            }
        };

        var keyClick = (string key) => Keyboard.Click(Enum.Parse<Keys>(key));
        var keyHold = (string key, int delay) => Keyboard.Hold(Enum.Parse<Keys>(key), delay);
        var keyPress = (string key) => Keyboard.Press(Enum.Parse<Keys>(key));
        var keyRelease = (string key) => Keyboard.Release(Enum.Parse<Keys>(key));

        var keyAutoRepeat = (string key, int delay) => Keyboard.AutoRepeat(Enum.Parse<Keys>(key), delay);
        var keyDynamicAutoRepeat = (string key, int delay) => Keyboard.DynamicAutoRepeat(Enum.Parse<Keys>(key), delay);
        var keyStopAutoRepeat = (string key) => Keyboard.StopAutoRepeat(Enum.Parse<Keys>(key));
        var keyStopAllAutoRepeat = () => Keyboard.StopAllAutoRepeat();

        // Log function
        var log = (object msg) => Console.WriteLine(msg);

        // Test function
        var test = () =>
        {
            Console.WriteLine("TESTING");
            Console.WriteLine(getButtonState("Buttons9"));
            return true;
        };

        return new HelperFunctions
        {
            getDeviceState = getDeviceState,
            getDeviceButtonState = getDeviceButtonState,
            getDeviceButtonValue = getDeviceButtonValue,
            getDeviceButtonRawValue = getDeviceButtonRawValue,
            getButtonState = getButtonState,
            getButtonValue = getButtonValue,
            getButtonRawValue = getButtonRawValue,
            stateToString = stateToString,
            deviceStateToString = deviceStateToString,
            globalStateToString = globalStateToString,
            getDynamicStateValue = getDynamicStateValue,
            setDynamicStateValue = setDynamicStateValue,

            keyClick = keyClick,
            keyHold = keyHold,
            keyPress = keyPress,
            keyRelease = keyRelease,
            keyAutoRepeat = keyAutoRepeat,
            keyDynamicAutoRepeat = keyDynamicAutoRepeat,
            keyStopAutoRepeat = keyStopAutoRepeat,
            keyStopAllAutoRepeat = keyStopAllAutoRepeat,

            log = log,
            test = test,
        };
    }

    public static bool ParseCondition(string? condition, string id, double value, object rawValue)
    {
        var helpers = GetHelperFunctions(id);

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

                helpers.getDeviceState,
                helpers.getDeviceButtonState,
                helpers.getDeviceButtonValue,
                helpers.getDeviceButtonRawValue,
                helpers.getButtonState,
                helpers.getButtonValue,
                helpers.getButtonRawValue,
                helpers.stateToString,
                helpers.deviceStateToString,
                helpers.globalStateToString,
                helpers.getDynamicStateValue,
                helpers.setDynamicStateValue,

                helpers.log,
                helpers.test,
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
        var helpers = GetHelperFunctions(id);

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

                helpers.getDynamicStateValue,
                helpers.setDynamicStateValue,
                helpers.stateToString,
                helpers.deviceStateToString,
                helpers.globalStateToString,

                helpers.keyClick,
                helpers.keyHold,
                helpers.keyPress,
                helpers.keyRelease,
                helpers.keyAutoRepeat,
                helpers.keyDynamicAutoRepeat,
                helpers.keyStopAutoRepeat,
                helpers.keyStopAllAutoRepeat,

                helpers.log,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}