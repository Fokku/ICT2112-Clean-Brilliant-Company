namespace CleanBrilliant.Domain.Entity
{
    public abstract class TotalCF
    {
        public string DeliveryMethod { get; set; } = "";
        public float TotalCarbonFootprint { get; set; }

        public abstract string GetReferenceId();
        public abstract void SetReferenceId(string id);
    }

    public class TotalCustomerCF : TotalCF
    {
        public string OrderId { get; set; } = "";
        public override string GetReferenceId() => OrderId;
        public override void SetReferenceId(string id) => OrderId = id;
    }

    public class TotalRestockCF : TotalCF
    {
        public string RestockId { get; set; } = "";
        public override string GetReferenceId() => RestockId;
        public override void SetReferenceId(string id) => RestockId = id;
    }
}
