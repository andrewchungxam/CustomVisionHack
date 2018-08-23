using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MyCommonLibrary;

namespace CustomVisionRDRFunction
{
    public static class GetPredictions
    {
        [FunctionName(nameof(GetPredictions))]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{databaseId}/{collectionId}")]
        HttpRequestMessage req, string databaseId, string collectionId, TraceWriter log)
        {
            try
            {
                //Using a Route, the databaseId and collectionId params will automatically be populated with the data in the URL
                //We aren't actually doing anything with these IDs at this point other than writing them out
                //but you could potentially pass them into a dataservice to process.
                //In our case, the CosmosDataService has these values hardcoded

                Console.WriteLine($"GetPredictions called [databaseID:{databaseId}, collectionID: {collectionId}]");
                var list = CosmosDataService.Instance.GetItemsAsync<Prediction>();
                return req.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception e)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.GetBaseException().Message);
            }
        }
    }


    //public static class GetPredictions
    //{
    //    [FunctionName("GetPredictions")]
    //    public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
    //    {
    //        log.Info("C# HTTP trigger function processed a request.");

    //        // parse query parameter
    //        string name = req.GetQueryNameValuePairs()
    //            .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
    //            .Value;

    //        // Get request body
    //        dynamic data = await req.Content.ReadAsAsync<object>();

    //        // Set name to query string or body data
    //        name = name ?? data?.name;

    //        return name == null
    //            ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
    //            : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
    //    }
}
