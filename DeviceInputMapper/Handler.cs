namespace DeviceInputMapper;

public interface Handler
{
    public abstract (Thread thread, CancellationTokenSource cts) Prepare();
    public bool EnableLogging { get; set; }
}