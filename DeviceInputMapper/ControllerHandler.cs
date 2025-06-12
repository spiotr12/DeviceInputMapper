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
                    if (!State.Devices.ContainsKey(_id))
                    {
                        State.Devices.Add(_id, ToDictionary());
                    }
                    else
                    {
                        UpdateDictionary(State.Devices[_id]);
                    }

                    foreach (var button in Enum.GetValues(typeof(GamepadButtonFlags)).Cast<GamepadButtonFlags>())
                    {
                        Handle(button.ToString(), State.Devices[_id][button.ToString()]);
                    }

                    _previous = _controller.GetState().Gamepad.ToString();
                }
            }
        }, new CancellationToken());
    }

    private void Handle(string button, double value)
    {
        if (GetCurrentModeConfig().TryGetValue(button, out var commands))
        {
            foreach (var command in commands)
            {
                if (Executor.ParseCondition(command.Condition, _id, value))
                {
                    Executor.ParseAction(command.Action, _id);
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

    private IDictionary<string, double> ToDictionary()
    {
        var dic = new Dictionary<string, double>();

        var buttons = _controller.GetState().Gamepad.Buttons;

        dic.Add("None", (buttons & GamepadButtonFlags.None) == GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("DPadUp", (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("DPadDown", (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("DPadLeft", (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("DPadRight", (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("Start", (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("Back", (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("LeftThumb", (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("RightThumb", (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("LeftShoulder", (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("RightShoulder", (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("A", (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("B", (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("X", (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("Y", (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0);

        dic.Add("LeftThumbX", _controller.GetState().Gamepad.LeftThumbX); // TODO: Parse
        dic.Add("LeftThumbY", _controller.GetState().Gamepad.LeftThumbY); // TODO: Parse
        dic.Add("LeftTrigger", _controller.GetState().Gamepad.LeftTrigger); // TODO: Parse
        dic.Add("RightThumbX", _controller.GetState().Gamepad.RightThumbX); // TODO: Parse
        dic.Add("RightThumbY", _controller.GetState().Gamepad.RightThumbY); // TODO: Parse
        dic.Add("RightTrigger", _controller.GetState().Gamepad.RightTrigger); // TODO: Parse

        return dic;
    }

    private void UpdateDictionary(IDictionary<string, double> dic)
    {
        var buttons = _controller.GetState().Gamepad.Buttons;

        dic["None"] = (buttons & GamepadButtonFlags.None) == GamepadButtonFlags.None ? 1 : (double)0;
        dic["DPadUp"] = (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["DPadDown"] = (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["DPadLeft"] = (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["DPadRight"] = (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["Start"] = (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["Back"] = (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["LeftThumb"] = (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["RightThumb"] = (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["LeftShoulder"] = (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["RightShoulder"] = (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["A"] = (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["B"] = (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["X"] = (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0;
        dic["Y"] = (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0;

        dic["LeftThumbX"] = _controller.GetState().Gamepad.LeftThumbX; // TODO: Parse
        dic["LeftThumbY"] = _controller.GetState().Gamepad.LeftThumbY; // TODO: Parse
        dic["LeftTrigger"] = _controller.GetState().Gamepad.LeftTrigger; // TODO: Parse
        dic["RightThumbX"] = _controller.GetState().Gamepad.RightThumbX; // TODO: Parse
        dic["RightThumbY"] = _controller.GetState().Gamepad.RightThumbY; // TODO: Parse
        dic["RightTrigger"] = _controller.GetState().Gamepad.RightTrigger; // TODO: Parse
    }
}