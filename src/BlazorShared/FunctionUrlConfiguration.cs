namespace BlazorShared;

public class FunctionUrlConfiguration
{
    public const string CONFIG_NAME = "Functions";

    public string FunctionKey { get; set; }
    public string BaseUrl { get; set; }
    public string OrderItemsReserverFunction { get; set; }
    public string DeliveryOrderProcessorFunction { get; set; }
}
