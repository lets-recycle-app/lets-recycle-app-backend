using System.Collections;
using Amazon.Lambda.Core;
using System.Collections.Generic;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsDotnetCsharp
{
    public class Handler
    {
       public ArrayList Tasks(Request request)
       {
           ArrayList tasks = new ArrayList();
           Task t1 = new Task("XA-1001", "Visit Manchester", false);
           Task t2 = new Task("XA-1002", "Visit London", false);
           Task t3 = new Task("XA-1003", "Visit Liverpool", false);

           tasks.Add(t1);
           tasks.Add(t2);
           tasks.Add(t3);
           return tasks;
       }
    }

    public class Task
    {
        public string TaskId { get; }
        public string Description { get; }
        public bool Completion { get; }
        
        public Task(string _taskId, string _description, bool _completion)
        {
            TaskId = _taskId;
            Description = _description;
            Completion = _completion;
        }
    }

    public class Request
    {
      public string Key1 {get; set;}
      public string Key2 {get; set;}
      public string Key3 {get; set;}
    }
}
