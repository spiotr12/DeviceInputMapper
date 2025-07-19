using SharpDX.DirectInput;

namespace DeviceInputMapper;

abstract class DirectInputHandler<T, TRaw, TUpdate> : Handler
    where T : class, IDeviceState<TRaw, TUpdate>, new()
    where TRaw : struct
    where TUpdate : struct, IStateUpdate
{
    public bool EnableLogging { get; set; }

    private readonly string _id;
    protected readonly DeviceConfig _config;
    protected readonly CustomDevice<T, TRaw, TUpdate> _device;

    public DirectInputHandler(string id, DeviceConfig config, CustomDevice<T, TRaw, TUpdate> device)
    {
        _id = id;
        _config = config;
        _device = device;

        _device.Properties.BufferSize = 128;
        _device.Acquire();
    }

    protected abstract string GetButtonName(TUpdate state);

    public (Task task, CancellationTokenSource cts) Prepare()
    {
        var cts = new CancellationTokenSource();
        var task = new Task(() => RunHandler(cts.Token), cts.Token);
        return (task, cts);
    }

    private void RunHandler(CancellationToken ctsToken)
    {
        Console.WriteLine("\"{0}\" [{1}]\n> Listening to events\n", _config.InstanceName, _config.InstanceGuid);

        // Poll events from joystick
        while (!ctsToken.IsCancellationRequested)
        {
            _device.Poll();
            var data = _device.GetBufferedData();
            foreach (var state in data)
            {
                if (EnableLogging)
                {
                    Console.WriteLine("{0} [{1}] ({2})", _config.InstanceName, _config.InputDeviceType, _id);
                    Console.WriteLine("> {0}\n", state);
                }

                // Update global state
                var button = GetButtonName(state);
                var rawValue = state.Value;
                var value = ParseValue(state);

                if (!State.Devices.ContainsKey(_id))
                {
                    State.Devices.Add(_id, new Dictionary<string, StateValue>());
                }

                if (!State.Devices[_id].ContainsKey(button))
                {
                    State.Devices[_id].Add(button, new StateValue { value = value, rawValue = rawValue });
                }

                State.Devices[_id][button] = (new StateValue { value = value, rawValue = rawValue });

                if (EnableLogging)
                {
                    Console.WriteLine(State.ToString());
                }

                Handle(state, button, value, rawValue);
            }
        }

        // ctsToken.ThrowIfCancellationRequested();
    }

    protected virtual void Handle(TUpdate state, string button, double value, object rawValue)
    {
        var processAllowed = State.Config?.ForProcesses == null ||
                             State.Config.ForProcesses.Contains(Window.GetForegroundWindowFilePath());

        if (GetCurrentModeConfig().TryGetValue(button, out var buttonConfig))
        {
            foreach (var action in buttonConfig.Actions)
            {
                if (processAllowed)
                {
                    if (EnableLogging)
                        Console.WriteLine($"Process file: ${Window.GetForegroundWindowFilePath()}");

                    if (Executor.ParseCondition(action.Condition, _id, button, value, rawValue))
                    {
                        Executor.ParseAction(action.Action, _id, button, value, rawValue);
                    }
                }
                else
                {
                    if (EnableLogging)
                        Console.WriteLine($"Process file: ${Window.GetForegroundWindowFilePath()}");
                }
            }
        }
    }

    protected IDictionary<string, InputMappingConfig> GetCurrentModeConfig()
    {
        if (_config.Configs == null || _config.Configs[State.CurrentMode] == null)
        {
            throw new Exception($"Mode \"{State.CurrentMode}\" for \"{_id}\" was not found");
        }

        return _config.Configs[State.CurrentMode];
    }

    protected abstract double ParseValue(TUpdate state);
}