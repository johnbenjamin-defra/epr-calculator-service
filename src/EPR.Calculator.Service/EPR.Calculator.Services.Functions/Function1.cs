using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace EPR.Calculator.Services.Functions
{
    public static class Function1
    {
        [FunctionName("ReadData")]
        public static void Run([ServiceBusTrigger("defra.epr.calculator.run", AccessRights.Manage, Connection = "TestConnection")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
