using System;

namespace DatabaseFunctions
{
    public class Depots
    {
        public Int32 depotId { get; set; }
        public string depotName { get; set; }
        public string postCode { get; set; }
        public Int32 fleetSize { get; set; }
    }
    
    public class Drivers
    {
        public Int32 driverId { get; set; }
        public Int32 depotId { get; set; }
        public string driverName { get; set; }
        public Int32 truckSize { get; set; }
        public string userName { get; set; }
        public string apiKey { get; set; }
    }
}