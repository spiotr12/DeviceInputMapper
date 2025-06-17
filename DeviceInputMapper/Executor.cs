using EventQueue;
using Z.Expressions;

namespace DeviceInputMapper;

public struct HelperFunctions
{
    public Action<string> ChangeMode { get; set; }
    public Action PreviousMode { get; set; }
    public Action<IEnumerable<string>, bool> IterateModes { get; set; }

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
    public Action ReloadJsonConfig { get; set; }
    public Func<bool> Test { get; set; }
    public Action Exit { get; set; }
}

public static class Executor
{
    public static readonly IDictionary<string, object> DynamicState = new Dictionary<string, object>();

    private static HelperFunctions GetHelperFunctions(string id)
    {
        var changeMode = (string nextMode) =>
        {
            if ((bool)State.Config?.Modes.ContainsKey(nextMode))
            {
                State.PreviousMode = State.CurrentMode;
                State.CurrentMode = nextMode;
            }
            else
            {
                throw new Exception($"Mode does not exists: {nextMode}");
            }
        };

        var previousMode = () => { changeMode(State.PreviousMode ?? State.Config?.DefaultMode ?? "Default"); };

        var iterateModes = (IEnumerable<string> modesOrder, bool reverse) =>
        {
            var list = new List<string>(modesOrder);
            var modeIndex = list.IndexOf(State.CurrentMode);
            int nextIndex = reverse ? modeIndex - 1 : modeIndex + 1;

            if (nextIndex < 0 || modeIndex > list.Count - 1)
            {
                nextIndex = reverse ? list.Count - 1 : 0;
            }

            changeMode(list[nextIndex]);
        };

        var getDeviceState = (string deviceGuid) =>
        {
            if (State.Devices.TryGetValue(deviceGuid, out var stateValue))
            {
                return stateValue;
            }

            return new Dictionary<string, StateValue>();
        };

        var getDeviceButtonState = (string deviceGuid, string button) =>
        {
            if (State.Devices.TryGetValue(deviceGuid, out var st))
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

        var getDeviceButtonValue = (string deviceGuid, string button) => { return getDeviceButtonState(deviceGuid, button).value; };
        var getDeviceButtonRawValue = (string deviceGuid, string button) => { return getDeviceButtonState(deviceGuid, button).rawValue; };

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
            catch (Exception e)
            {
                Console.WriteLine(e);
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
        var log = (object obj) => Console.WriteLine(obj);

        // Test function
        var test = () =>
        {
            Console.WriteLine("TESTING");
            Console.WriteLine(getButtonState("Buttons9"));
            return true;
        };

        var exit = () => { EventBus.Emit(Event.Exit); };
        var reloadJsonConfig = () => { EventBus.Emit(Event.Reload); };

        return new HelperFunctions
        {
            ChangeMode = changeMode,
            PreviousMode = previousMode,
            IterateModes = iterateModes,

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
            ReloadJsonConfig = reloadJsonConfig,
            Test = test,
            Exit = exit,
        };
    }

    public static bool ParseCondition(string? condition, string id, string button, double value, object rawValue)
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
                button,
                value,
                rawValue,
                currentMode = State.CurrentMode,
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

    public static void ParseAction(string action, string id, string button, double value, object rawValue)
    {
        var helpers = GetHelperFunctions(id);

        try
        {
            Eval.Execute(action, new
            {
                id,
                button,
                value,
                rawValue,
                currentMode = State.CurrentMode,
                state = State.GetDevice(id),
                globalState = State.Devices,
                changeMode = helpers.ChangeMode,
                previousMode = helpers.PreviousMode,
                iterateModes = helpers.IterateModes,
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
                reloadJsonConfig = helpers.ReloadJsonConfig,
                exit = helpers.Exit,
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error in action {0} > {1} > {2}", id, button, action);
            Console.WriteLine(e);
        }
    }
}