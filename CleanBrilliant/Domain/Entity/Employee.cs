namespace CleanBrilliant.Domain.Entity
{
    public class Employee : CarbonSource
    {
        public int EmployeeId { get; private set; }
        public WorkMode EmployeeWorkmode { get; private set; }
        public double TravelDistance { get; private set; }
        public TransportMode TransportMode { get; private set; }
        public int DaysInOffice { get; private set; }

        private Employee() { }

        public Employee(int employeeId, WorkMode workmode, double travelDistance, TransportMode transportMode, int daysInOffice)
        {
            EmployeeId = employeeId;
            EmployeeWorkmode = workmode;
            TravelDistance = travelDistance;
            TransportMode = transportMode;
            DaysInOffice = daysInOffice;
        }

        public void SetStaffId(int id) => EmployeeId = id;
        public void SetTransportMode(TransportMode mode) => TransportMode = mode;
        public void SetTravelDistance(double km) => TravelDistance = km;
        public void SetDaysInOffice(int days) => DaysInOffice = days;
        public void SetWorkMode(WorkMode mode) => EmployeeWorkmode = mode;
    }
}
