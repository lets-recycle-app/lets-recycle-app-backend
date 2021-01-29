using System.Collections.Generic;
using Newtonsoft.Json;

namespace DatabaseFunctions
{
    public class Response
    {
        public Response()
        {
            Body = "X";
            StatusCode = 500;
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"}
            };
            
        }

        public string Body { get; set; }
        
        /*
        public string Body
        {
          
            get => JsonConvert.SerializeObject(Body);
            set { }
        }
        */
        

        public Dictionary<string, string> Headers { get; set; }
        public int StatusCode { get; set; }
    }
}