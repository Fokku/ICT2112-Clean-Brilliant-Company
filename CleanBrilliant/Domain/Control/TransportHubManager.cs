using CleanBrilliant.Domain.Entity;

namespace CleanBrilliant.Domain.Control
{
    public class TransportHubManager
    {
        private readonly List<TransportHub> _hubs = new();

        public TransportHubManager()
        {
            _hubs.Add(new Airport());
            _hubs.Add(new TrainStation());
            _hubs.Add(new ShippingPorts());
        }

        public double GetTransportHubDistance(string shippingMethod)
        {
            var hub = _hubs.FirstOrDefault(h =>
                h.HubType.Equals(shippingMethod, StringComparison.OrdinalIgnoreCase));
            return hub?.HubToHubDist ?? 0;
        }
    }
}
