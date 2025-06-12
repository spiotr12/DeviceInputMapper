using SharpDX.DirectInput;
using Z.Expressions;

namespace DeviceInputMapper;

abstract class DirectInputHandler<T, TRaw, TUpdate>
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

    public virtual Task Run()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("{0} [{1}]\n> Task listening to events\n", _config.InstanceName, _config.InstanceGuid);

            // Poll events from joystick
            while (true)
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
                        State.Devices.Add(_id, new Dictionary<string, (double value, object rawValue)>());
                    }

                    if (!State.Devices[_id].ContainsKey(button))
                    {
                        State.Devices[_id].Add(button, (value, rawValue));
                    }

                    State.Devices[_id][button] = (value, rawValue);

                    if (EnableLogging)
                    {
                        Console.WriteLine(State.ToString());
                    }

                    Handle(state, button, value, rawValue);
                }
            }
        }, new CancellationToken());
    }

    protected virtual void Handle(TUpdate state, string button, double value, object rawValue)
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

    protected abstract double ParseValue(TUpdate state);
}