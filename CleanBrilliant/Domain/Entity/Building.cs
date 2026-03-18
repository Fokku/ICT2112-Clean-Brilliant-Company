namespace CleanBrilliant.Domain.Entity
{
    public class Building : CarbonSource
    {
        public int BuildingId { get; private set; }
        public string Address { get; private set; } = "";
        public double SquareFoot { get; private set; }
        public double ElectricityKWH { get; private set; }
        public double GasVolume { get; private set; }

        private Building() { }

        public Building(int buildingId, string address, double squareFoot, double electricityKWH, double gasVolume)
        {
            BuildingId = buildingId;
            Address = address;
            SquareFoot = squareFoot;
            ElectricityKWH = electricityKWH;
            GasVolume = gasVolume;
        }

        public void SetBuildingId(int id) => BuildingId = id;
        public void SetAddress(string addr) => Address = addr;
        public void SetSquareFoot(double sqft) => SquareFoot = sqft;
        public void SetElectricityKWh(double kwh) => ElectricityKWH = kwh;
        public void SetGasVolume(double vol) => GasVolume = vol;
    }
}
