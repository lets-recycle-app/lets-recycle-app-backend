namespace Routing
{
    public class Depots
    {
        public int depotId { get; set; }
        public string depotName { get; set; }
        public string postCode { get; set; }
        public int fleetSize { get; set; }
    }

    public class Drivers
    {
        public int driverId { get; set; }
        public int depotId { get; set; }
        public string driverName { get; set; }
        public int truckSize { get; set; }
        public string userName { get; set; }
        public string apiKey { get; set; }
    }
}