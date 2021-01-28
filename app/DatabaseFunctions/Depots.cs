namespace DatabaseFunctions
{
    public class Depots
    {
        public int depotId;
        public string depotName;
        public int fleetSize;
        public string postCode;

        public Depots(int _depotId, string _depotName, string _postCode, int _fleetSize)
        {
            depotId = _depotId;
            depotName = _depotName;
            postCode = _postCode;
            fleetSize = _fleetSize;
        }
    }
}