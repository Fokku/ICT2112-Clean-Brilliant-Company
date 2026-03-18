namespace CleanBrilliant.Domain.Entity
{
    public abstract class DistanceCF
    {
        public string DeliveryMethod { get; set; } = "";
        public float DistanceCarbon { get; set; }

        public abstract string GetReferenceId();
        public abstract void SetReferenceId(string id);
    }

    public class OrderDistanceCF : DistanceCF
    {
        public string OrderId { get; set; } = "";
        public override string GetReferenceId() => OrderId;
        public override void SetReferenceId(string id) => OrderId = id;
    }

    public class SupplierDistanceCF : DistanceCF
    {
        public string RestockId { get; set; } = "";
        public override string GetReferenceId() => RestockId;
        public override void SetReferenceId(string id) => RestockId = id;
    }
}
