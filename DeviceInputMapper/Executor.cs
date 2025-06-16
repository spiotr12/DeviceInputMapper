using Z.Expressions;

namespace DeviceInputMapper;

public struct HelperFunctions
{
    public Func<string, IDictionary<string, StateValue>> GetDeviceState { get; set; }
    public Func<string, string, StateValue> GetDeviceButtonState { get; set; }
    public Func<string, string, double> GetDeviceButtonValue { get; set; }
    public Func<string, string, object> GetDeviceButtonRawValue { get; set; }
    public Func<string, StateValue> GetButtonState { get; set; }
    public Func<string, double> GetButtonValue { get; set; }
    public Func<string, object> GetButtonRawValue { get; set; }
    public Func<string> StateToString { get; set; }
    public Func<string, string> DeviceStateToString { get; set; }
    public Func<string> GlobalStateToString { get; set; }
    public Func<string, object?> GetDynamicStateValue { get; set; }
    public Action<string, object> SetDynamicStateValue { get; set; }

    public Action<string> KeyClick { get; set; }
    public Action<string, int> KeyHold { get; set; }
    public Action<string> KeyPress { get; set; }
    public Action<string> KeyRelease { get; set; }
    public Action<string, int> KeyAutoRepeat { get; set; }
    public Action<string, int> KeyDynamicAutoRepeat { get; set; }
    public Action<string> KeyStopAutoRepeat { get; set; }
    public Action KeyStopAllAutoRepeat { get; set; }

    public Action<object> Log { get; set; }
    public Func<bool> Test { get; set; }
}

public static class Executor
{
    public static readonly IDictionary<string, object> DynamicState = new Dictionary<string, object>();

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
                        rawValue = stateValue.rawValue,
                    };
                }
            }

            return new StateValue { value = double.NaN, rawValue = double.NaN };
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
                    rawValue = stateValue.rawValue,
                };
            }

            return new StateValue { value = double.NaN, rawValue = double.NaN };
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
                return DynamicState[key];
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        };

        // Save dynamic state value
        var setDynamicStateValue = (string key, object value) => { DynamicState[key] = value; };

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
            GetDeviceState = getDeviceState,
            GetDeviceButtonState = getDeviceButtonState,
            GetDeviceButtonValue = getDeviceButtonValue,
            GetDeviceButtonRawValue = getDeviceButtonRawValue,
            GetButtonState = getButtonState,
            GetButtonValue = getButtonValue,
            GetButtonRawValue = getButtonRawValue,
            StateToString = stateToString,
            DeviceStateToString = deviceStateToString,
            GlobalStateToString = globalStateToString,
            GetDynamicStateValue = getDynamicStateValue,
            SetDynamicStateValue = setDynamicStateValue,

            KeyClick = keyClick,
            KeyHold = keyHold,
            KeyPress = keyPress,
            KeyRelease = keyRelease,
            KeyAutoRepeat = keyAutoRepeat,
            KeyDynamicAutoRepeat = keyDynamicAutoRepeat,
            KeyStopAutoRepeat = keyStopAutoRepeat,
            KeyStopAllAutoRepeat = keyStopAllAutoRepeat,

            Log = log,
            Test = test,
        };
    }

    public static bool ParseCondition(string? condition, string id, string button, ButtonConfig buttonConfig, double value, object rawValue)
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
                getDeviceState = helpers.GetDeviceState,
                getDeviceButtonState = helpers.GetDeviceButtonState,
                getDeviceButtonValue = helpers.GetDeviceButtonValue,
                getDeviceButtonRawValue = helpers.GetDeviceButtonRawValue,
                getButtonState = helpers.GetButtonState,
                getButtonValue = helpers.GetButtonValue,
                getButtonRawValue = helpers.GetButtonRawValue,
                stateToString = helpers.StateToString,
                deviceStateToString = helpers.DeviceStateToString,
                globalStateToString = helpers.GlobalStateToString,
                getDynamicStateValue = helpers.GetDynamicStateValue,
                setDynamicStateValue = helpers.SetDynamicStateValue,
                log = helpers.Log,
                test = helpers.Test,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in condition {0} > {1} > {2}", id, button, condition);
            Console.WriteLine(e);
        }

        return false;
    }

    public static void ParseAction(string action, string id, string button, ButtonConfig buttonConfig, double value, object rawValue)
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
                getDynamicStateValue = helpers.GetDynamicStateValue,
                setDynamicStateValue = helpers.SetDynamicStateValue,
                stateToString = helpers.StateToString,
                deviceStateToString = helpers.DeviceStateToString,
                globalStateToString = helpers.GlobalStateToString,
                keyClick = helpers.KeyClick,
                keyHold = helpers.KeyHold,
                keyPress = helpers.KeyPress,
                keyRelease = helpers.KeyRelease,
                keyAutoRepeat = helpers.KeyAutoRepeat,
                keyDynamicAutoRepeat = helpers.KeyDynamicAutoRepeat,
                keyStopAutoRepeat = helpers.KeyStopAutoRepeat,
                keyStopAllAutoRepeat = helpers.KeyStopAllAutoRepeat,
                log = helpers.Log,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in action {0} > {1} > {2}", id, button, action);
            Console.WriteLine(e);
        }
    }
}