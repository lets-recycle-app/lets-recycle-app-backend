using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Routing
{
    public class Response
    {
        public Response()
        {
            Body = "";
            StatusCode = 500;

            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"}
            };
        }

        public string Body { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; }

        public void Show()
        {
            Console.WriteLine(JsonConvert.SerializeObject(Headers, Formatting.Indented));
            Console.WriteLine(Body);
            Console.WriteLine($"Status: {StatusCode}");
        }
    }
}