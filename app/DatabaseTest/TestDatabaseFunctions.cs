namespace DatabaseFunctions
{
    class Program
    {
        static void Main(string[] args)
        {
            string requestName = "drivers";
            string requestId = "0";

            RequestFarm requestFarm = new RequestFarm(requestName, requestId);

            requestFarm.Process();
            requestFarm.ShowResponse();
        }
    }
}