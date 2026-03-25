namespace CleanBrilliant.Domain.Entity
{
    public abstract class TransportHub
    {
        public int HubId { get; set; }
        public string HubType { get; set; } = "";
        public double HubToHubDist { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string HubPostalCode { get; set; } = "";
    }

    public class Airport : TransportHub
    {
        public Airport() { HubType = "air"; HubToHubDist = 300; }
    }

    public class TrainStation : TransportHub
    {
        public TrainStation() { HubType = "rail"; HubToHubDist = 480; }
    }

    public class ShippingPorts : TransportHub
    {
        public ShippingPorts() { HubType = "ship"; HubToHubDist = 1000; }
    }
}
