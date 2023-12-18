namespace BlazorShared;

public class QueueConfiguration
{
    public const string CONFIG_NAME = "Queues";

    public string ConnectionString { get; set; }
    public string ReservedItemsQueue { get; set; }
}
