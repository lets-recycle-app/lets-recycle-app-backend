using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace ApiCore
{
    public static class Main
    {
        public const int DataRowReturnLimit = 1000;
        private static int ApiStatusCode { get; set; }

        public static string Body(APIGatewayProxyRequest request)
        {
            string action = request.Path.Replace("/api/", "").Replace("%20", "");
            Database.SetUp();

            IDictionary<string, string> query = request.QueryStringParameters;
            ApiStatusCode = 210;

            return request.HttpMethod switch
            {
                "GET" => ApiGet(action, query),
                "POST" => ApiPost(action, request.Body),
                _ => Result(211, "error, invalid http method [{httpMethod}]", null)
            };
        }

        private static string ApiGet(string action, IDictionary<string, string> query)
        {
            return action switch
            {
                "collect-request" => Collect.Request(query),
                "route-map" => Route.Map(query),
                "route-marker" => Route.Markers(query),
                "route-simulate" => Route.Simulate(query),
                "route-distance" => Route.Distance(query),
                _ => TableName.Get(action, query)
            };
        }


        private static string ApiPost(string action, string body)
        {
            return action switch
            {
                "collect-confirm" => Collect.Confirm(body),
                "collect-update" => Collect.Update(),
                "collect-cancel" => Collect.Cancel(),
                _ => Result(214, "error, service not supported [POST]", null)
            };
        }

        public static string Result(int status, string message, JArray data)
        {
            data ??= new JArray();
            ApiStatusCode = status;

            JObject response = new JObject
            {
                {"status", status},
                {"message", message},
                {"count", data.Count}
            };

            if (data.Count >= DataRowReturnLimit)
            {
                response["limit"] = DataRowReturnLimit;
            }

            response["result"] = data;

            return response.ToString();
        }

        public static IDictionary<string, string> Headers()
        {
            return new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"},
                {"Access-Control-Allow-Methods", "DELETE, POST, GET, OPTIONS"},
                {
                    "Access-Control-Allow-Headers",
                    "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With"
                },
                {"Access-Control-Max-Age", "86400"},
                {"X-Requested-With", "*"}
            };
        }

        public static int StatusCode()
        {
            return ApiStatusCode;
        }
    }
}