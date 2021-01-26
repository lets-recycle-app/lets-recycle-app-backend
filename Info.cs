namespace AwsDotnetCsharp
{
    public class Info
    {
        public Info(string _depotName, string _depotPostCode, int _depotFleetSize)
        {
            DepotName = _depotName;
            DepotPostCode = _depotPostCode;
            DepotFleetSize = _depotFleetSize;
        }

        public string DepotName { get; set; }
        public string DepotPostCode { get; set; }
        public int DepotFleetSize { get; set; }
    }
}