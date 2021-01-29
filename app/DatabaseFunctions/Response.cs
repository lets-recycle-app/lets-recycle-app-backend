using System.Collections.Generic;
using Newtonsoft.Json;

namespace DatabaseFunctions
{
    public class Response
    {
        public Response()
        {
            Body = "";
            StatusCode = 500;

            Headers = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"}
            }, Formatting.Indented);
        }

        public int StatusCode { get; set; }
        public string Body { get; set; }
        public string Headers { get; }
    }
}