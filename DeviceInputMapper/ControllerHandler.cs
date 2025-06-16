using SharpDX.XInput;

namespace DeviceInputMapper;

class ControllerHandler : Handler
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

    public Task Prepare()
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
        if (GetCurrentModeConfig().TryGetValue(button, out var buttonConfig))
        {
            foreach (var action in buttonConfig.Actions)
            {
                if (Executor.ParseCondition(action.Condition, _id, button, value, rawValue))
                {
                    Executor.ParseAction(action.Action, _id, button, value, rawValue);
                }
            }
        }
    }

    protected IDictionary<string, ButtonConfig> GetCurrentModeConfig()
    {
        if (_config.Configs == null || _config.Configs[State.Mode] == null)
        {
            throw new Exception("No device configuration found for this mode");
        }

        return _config.Configs[State.Mode];
    }

    private IDictionary<string, StateValue> ToDictionary()
    {
        var dic = new Dictionary<string, StateValue>();

        var buttons = _controller.GetState().Gamepad.Buttons;

        // dic.Add("None", (buttons & GamepadButtonFlags.None) == GamepadButtonFlags.None ? 1 : (double)0);
        dic.Add("DPadUp", new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("DPadDown", new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("DPadLeft", new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("DPadRight", new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("Start", new StateValue
        {
            value = (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("Back", new StateValue
        {
            value = (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("LeftThumb", new StateValue
        {
            value = (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("RightThumb", new StateValue
        {
            value = (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("LeftShoulder", new StateValue
        {
            value = (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("RightShoulder", new StateValue
        {
            value = (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("A", new StateValue
        {
            value = (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("B", new StateValue
        {
            value = (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("X", new StateValue
        {
            value = (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0
        });
        dic.Add("Y", new StateValue
        {
            value = (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0
        });

        dic.Add("LeftThumbX", new StateValue
        {
            value = _controller.GetState().Gamepad.LeftThumbX,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.LeftThumbX)
        });
        dic.Add("LeftThumbY", new StateValue
        {
            value = _controller.GetState().Gamepad.LeftThumbY,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.LeftThumbY)
        });
        dic.Add("RightThumbX", new StateValue
        {
            value = _controller.GetState().Gamepad.RightThumbX,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.RightThumbX)
        });
        dic.Add("RightThumbY", new StateValue
        {
            value = _controller.GetState().Gamepad.RightThumbY,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.RightThumbY)
        });

        dic.Add("LeftTrigger", new StateValue
        {
            value = _controller.GetState().Gamepad.LeftTrigger,
            rawValue = ParseTriggerValue(_controller.GetState().Gamepad.LeftTrigger)
        });
        dic.Add("RightTrigger", new StateValue
        {
            value = _controller.GetState().Gamepad.RightTrigger,
            rawValue = ParseTriggerValue(_controller.GetState().Gamepad.RightTrigger)
        });

        return dic;
    }

    private void UpdateDictionary(IDictionary<string, StateValue> dic)
    {
        var buttons = _controller.GetState().Gamepad.Buttons;

        // dic["None"] = (buttons & GamepadButtonFlags.None) == GamepadButtonFlags.None ? 1 : (double)0;
        dic["DPadUp"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadUp) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["DPadDown"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadDown) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["DPadLeft"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadLeft) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["DPadRight"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.DPadRight) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["Start"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.Start) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["Back"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.Back) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["LeftThumb"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.LeftThumb) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["RightThumb"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.RightThumb) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["LeftShoulder"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.LeftShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["RightShoulder"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.RightShoulder) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["A"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.A) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["B"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.B) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["X"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.X) != GamepadButtonFlags.None ? 1 : (double)0
        };
        dic["Y"] = new StateValue
        {
            value = (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0,
            rawValue = (buttons & GamepadButtonFlags.Y) != GamepadButtonFlags.None ? 1 : (double)0
        };

        dic["LeftThumbX"] = new StateValue
        {
            value = _controller.GetState().Gamepad.LeftThumbX,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.LeftThumbX)
        };
        dic["LeftThumbY"] = new StateValue
        {
            value = _controller.GetState().Gamepad.LeftThumbY,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.LeftThumbY)
        };
        dic["RightThumbX"] = new StateValue
        {
            value = _controller.GetState().Gamepad.RightThumbX,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.RightThumbX)
        };
        dic["RightThumbY"] = new StateValue
        {
            value = _controller.GetState().Gamepad.RightThumbY,
            rawValue = ParseThumbValue(_controller.GetState().Gamepad.RightThumbY)
        };

        dic["LeftTrigger"] = new StateValue
        {
            value = _controller.GetState().Gamepad.LeftTrigger,
            rawValue = ParseTriggerValue(_controller.GetState().Gamepad.LeftTrigger)
        };
        dic["RightTrigger"] = new StateValue
        {
            value = _controller.GetState().Gamepad.RightTrigger,
            rawValue = ParseTriggerValue(_controller.GetState().Gamepad.RightTrigger)
        };
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