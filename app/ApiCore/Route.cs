using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static ApiCore.Main;

namespace ApiCore
{
    public static class Route
    {
        public static string Simulate(IDictionary<string, string> query)
        {
            Console.WriteLine("In route simulation module.");
            
            return Result(232, "route simulated.", null);
        }
    }
}