using System.Collections;
using Amazon.Lambda.Core;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsDotnetCsharp
{
    public class Handler
    {
        
       public APIGatewayProxyResponse GetData(APIGatewayProxyRequest request)
       {
           string userId = request.PathParameters["userId"];
           ArrayList dataList = new ArrayList();

           if (userId == "dan")
           {
               Info t1 = new Info("GR-1001", "Visit Greece", false);
               dataList.Add(t1);
           }
           else
           {
               Info t1 = new Info("UK-1001", "Visit Manchester", false);
               Info t2 = new Info("UK-1002", "Visit London", false);
               Info t3 = new Info("UK-1003", "Visit XLiverpool", false);
               
               dataList.Add(t1);
               dataList.Add(t2);
               dataList.Add(t3);
           }
           
           //LambdaLogger.Log("Getting Data For " +userId);
           
           return new APIGatewayProxyResponse
           {
                Headers = new Dictionary<string, string>
                { 
                    { "Content-Type", "application/json" }, 
                    { "Access-Control-Allow-Origin", "*" } 
                },
                Body = JsonSerializer.Serialize(dataList),
                StatusCode = 200
           };
       }
       
       public APIGatewayProxyResponse SaveData(APIGatewayProxyRequest request)
       {
           return new APIGatewayProxyResponse
           {
               Headers = new Dictionary<string, string>
               { 
                   { "Content-Type", "application/json" }, 
                   { "Access-Control-Allow-Origin", "*" } 
               },
               Body = "Data Saved",
               StatusCode = 200
           };
       }
    }

    public class Info
    {
        public string DataId { get; }
        public string Description { get; }
        public bool Completion { get; }
        
        public Info(string dataId, string _description, bool _completion)
        {
            DataId = dataId;
            Description = _description;
            Completion = _completion;
        }
    }
}
