namespace DeviceInputMapper;

public interface Handler
{
    public abstract Task Prepare();
    public bool EnableLogging { get; set; }
}