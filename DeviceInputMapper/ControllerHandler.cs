using SharpDX.DirectInput;
using SharpDX.XInput;
using Z.Expressions;

namespace DeviceInputMapper;

class ControllerHandler
{
    public bool EnableLogging { get; set; }

    private readonly string _id;
    private readonly DeviceConfig _config;
    private readonly Controller _controller;

    private string _previous = "";

    private string[] _controllerAxis =
    [
        "LeftThumbX",
        "LeftThumbY",
        "RightThumbX",
        "RightThumbY",
        "LeftTrigger",
        "RightTrigger"
    ];

    public ControllerHandler(string id, DeviceConfig config, Controller controller)
    {
        _id = id;
        _config = config;
        _controller = controller;
    }

    // protected string GetButtonName(JoystickUpdate state)
    // {
    //     return state.Offset.ToString();
    // }

    public virtual Task Run()
    {
        return Task.Run(() =>
        {
            while (_controller.IsConnected)
            {
                // Console.WriteLine(_controller.GetState().Gamepad);

                if (_previous != _controller.GetState().Gamepad.ToString())
                {
                    if (EnableLogging)
                    {
                        Console.WriteLine("{0} [{1}] ({2})", _config.InstanceName, _config.InputDeviceType, _id);
                        Console.WriteLine("> {0}\n", _controller.GetState().Gamepad.ToString());
                    }

                    if (!State.Devices.ContainsKey(_id))
                    {
                        State.Devices.Add(_id, ToDictionary());
                    }
                    else
                    {
                        UpdateDictionary(State.Devices[_id]);
                    }

                    if (EnableLogging)
                    {
                        Console.WriteLine(State.ToString());
                    }

                    // buttons
                    foreach (var button in Enum.GetValues(typeof(GamepadButtonFlags)).Cast<GamepadButtonFlags>())
                    {
                        if (button != GamepadButtonFlags.None)
                        {
                            var value = State.Devices[_id][button.ToString()].value;
                            var rawValue = State.Devices[_id][button.ToString()].rawValue;
                            Handle(button.ToString(), value, rawValue);
                        }
                    }

                    foreach (var axis in _controllerAxis)
                    {
                        var value = State.Devices[_id][axis].value;
                        var rawValue = State.Devices[_id][axis].rawValue;
                        Handle(axis, value, rawValue);
                    }

                    _previous = _controller.GetState().Gamepad.ToString();
                }
            }
        }, new CancellationToken());
    }

    private void Handle(string button, double value, object rawValue)
    {
        if (GetCurrentModeConfig().TryGetValue(button, out var commands))
        {
            foreach (var command in commands)
            {
                if (Executor.ParseCondition(command.Condition, _id, value, rawValue))
                {
                    Executor.ParseAction(command.Action, _id, value, rawValue);
                }
            }
        }
    }

    protected IDictionary<string, IEnumerable<InputConfig>> GetCurrentModeConfig()
    {
        if (_config.Modes == null || _config.Modes[State.Mode] == null)
        {
            throw new Exception("No device configuration found for this mode");
        }

        return _config.Modes[State.Mode];
    }

    private IDictionary<string, (double value, object rawValue)> ToDictionary()
    {
        var dic = new Dictionary<string, (double value, object rawValue)>();

        var buttons = _controller.GetState().Gamepad.Buttons;

        // dic.Add("None", (buttons & GamepadButtonFlags.None) == GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("DPadUp", (
            value: (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("DPadDown", (
            value: (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("DPadLeft", (
            value: (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("DPadRight", (
            value: (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("Start", (
            value: (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("Back", (
            value: (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("LeftThumb", (
            value: (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("RightThumb", (
            value: (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("LeftShoulder", (
            value: (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("RightShoulder", (
            value: (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("A", (
            value: (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("B", (
            value: (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("X", (
            value: (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0
        ));
        dic.Add("Y", (
            value: (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0
        ));

        dic.Add("LeftThumbX", (
            value: _controller.GetState().Gamepad.LeftThumbX,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.LeftThumbX)
        ));
        dic.Add("LeftThumbY", (
            value: _controller.GetState().Gamepad.LeftThumbY,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.LeftThumbY)
        ));
        dic.Add("RightThumbX", (
            value: _controller.GetState().Gamepad.RightThumbX,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.RightThumbX)
        ));
        dic.Add("RightThumbY", (
            value: _controller.GetState().Gamepad.RightThumbY,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.RightThumbY)
        ));

        dic.Add("LeftTrigger", (
            value: _controller.GetState().Gamepad.LeftTrigger,
            rawValue: ParseTriggerValue(_controller.GetState().Gamepad.LeftTrigger)
        ));
        dic.Add("RightTrigger", (
            value: _controller.GetState().Gamepad.RightTrigger,
            rawValue: ParseTriggerValue(_controller.GetState().Gamepad.RightTrigger)
        ));

        return dic;
    }

    private void UpdateDictionary(IDictionary<string, (double value, object rawValue)> dic)
    {
        var buttons = _controller.GetState().Gamepad.Buttons;

        // dic["None"] = (buttons & GamepadButtonFlags.None) == GamepadButtonFlags.None ? 1 : (double)0;
        dic["DPadUp"] = (
            value: (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["DPadDown"] = (
            value: (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["DPadLeft"] = (
            value: (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["DPadRight"] = (
            value: (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["Start"] = (
            value: (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["Back"] = (
            value: (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["LeftThumb"] = (
            value: (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["RightThumb"] = (
            value: (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["LeftShoulder"] = (
            value: (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["RightShoulder"] = (
            value: (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["A"] = (
            value: (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["B"] = (
            value: (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["X"] = (
            value: (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0
        );
        dic["Y"] = (
            value: (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue: (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0
        );

        dic["LeftThumbX"] = (
            value: _controller.GetState().Gamepad.LeftThumbX,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.LeftThumbX)
        );
        dic["LeftThumbY"] = (
            value: _controller.GetState().Gamepad.LeftThumbY,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.LeftThumbY)
        );
        dic["RightThumbX"] = (
            value: _controller.GetState().Gamepad.RightThumbX,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.RightThumbX)
        );
        dic["RightThumbY"] = (
            value: _controller.GetState().Gamepad.RightThumbY,
            rawValue: ParseThumbValue(_controller.GetState().Gamepad.RightThumbY)
        );

        dic["LeftTrigger"] = (
            value: _controller.GetState().Gamepad.LeftTrigger,
            rawValue: ParseTriggerValue(_controller.GetState().Gamepad.LeftTrigger)
        );
        dic["RightTrigger"] = (
            value: _controller.GetState().Gamepad.RightTrigger,
            rawValue: ParseTriggerValue(_controller.GetState().Gamepad.RightTrigger)
        );
    }

    private double ParseThumbValue(short value)
    {
        return Math.Round(((double)value / 32767), 4);
    }

    private double ParseTriggerValue(byte value)
    {
        return Math.Round(((double)value / 255), 4);
    }
}