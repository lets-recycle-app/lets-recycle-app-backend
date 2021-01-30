using System;
using Routing;

namespace RoutingTest
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RoutingTest [~/api/{service}/{options+}]");
                return 1;
            }

            string apiRoute = args[0];
            Console.WriteLine(apiRoute);

            string path = @"http://www.test.com/article/12345?order=2";
            string path2 = "";

            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                Uri uriPath = new Uri(path);
                
                foreach (var segment in uriPath.Segments)
                {
                    Console.WriteLine(segment);
                }

                Console.WriteLine(uriPath.Host);
            }
            else
            {
                Console.WriteLine("Bad Uri");
            }


            string requestName = "depots";
            string requestId = "0";

            RouteFarm routeFarm = new RouteFarm(apiRoute);

            routeFarm.Process();
            routeFarm.ShowResponse();

            return 0;
        }
    }
}