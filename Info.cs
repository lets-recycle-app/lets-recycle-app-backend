namespace AwsDotnetCsharp
{
    public class Info
    {
        public Info(string _depotName, string _depotPostCode, int _depotFeetSize)
        {
            DepotName = _depotName;
            DepotPostCode = _depotPostCode;
            DepotFeetSize = _depotFeetSize;
        }

        public string DepotName { get; set; }
        public string DepotPostCode { get; set; }
        public int DepotFeetSize { get; set; }
    }
}