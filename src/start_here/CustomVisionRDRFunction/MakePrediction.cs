using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using MyCommonLibrary;

namespace CustomVisionRDRFunction
{
    public static class MakePrediction
    {
        //[FunctionName("MakePrediction")]
        //public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        //{
        //    log.Info("C# HTTP trigger function processed a request.");

        //    // parse query parameter
        //    string name = req.GetQueryNameValuePairs()
        //        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        //        .Value;

        //    // Get request body
        //    dynamic data = await req.Content.ReadAsAsync<object>();

        //    // Set name to query string or body data
        //    name = name ?? data?.name;

        //    return name == null
        //        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        //        : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        //}

        static CloudBlobContainer _blobContainer;

        [FunctionName(nameof(MakePrediction))]
        public static async Task<HttpResponseMessage> RunMakePrediction([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/MakePrediction")]HttpRequestMessage req, TraceWriter log)
        {

            try
            {
                //VERSION 1
                ////Extract the content body of the request 
                //var stream = await req.Content.ReadAsStreamAsync();

                ////Retrieve a URL for the image being stored
                //var imageUrl = await UploadImageToBlobStorage(stream);

                ////Return the URL as the response content
                //return req.CreateResponse(HttpStatusCode.OK, imageUrl);


                //VERSION 2
                //========================================================================
                // Add this within the try clause
                //========================================================================

                //var stream = await req.Content.ReadAsStreamAsync();
                //var prediction = new Prediction
                //{
                //    TimeStamp = DateTime.UtcNow,
                //    UserId = Guid.NewGuid().ToString(),
                //    ImageUrl = await UploadImageToBlobStorage(stream)
                //};

                //await CosmosDataService.Instance.InsertItemAsync(prediction);
                //return req.CreateResponse(HttpStatusCode.OK, prediction);

                //VERSION 3
                var stream = await req.Content.ReadAsStreamAsync();
                var prediction = new Prediction
                {
                    //ProjectId = "YOUR_PROJECT_ID", //This is the custom vision project we are predicting against
                    //PredictionKey = "YOUR_PREDICTION_KEY", //This is the custom vision project's prediction key we are predicting against

                    ProjectId = "3e152a33-92ee-4326-b4c9-c8068edab9d8", //"YOUR_PROJECT_ID", //This is the custom vision project we are predicting against
                    PredictionKey = "eafbd7b247bd41c1afd6d6da352f7099", //"YOUR_PREDICTION_KEY", //This is the custom vision project's prediction key we are predicting against
                    
                    TimeStamp = DateTime.UtcNow,
                    UserId = Guid.NewGuid().ToString(),
                    ImageUrl = await UploadImageToBlobStorage(stream),
                    Results = new Dictionary<string, decimal>()
                };

                var endpoint = new PredictionEndpoint { ApiKey = prediction.PredictionKey };
                //This is where we run our prediction against the default iteration
                var result = endpoint.PredictImageUrl(new Guid(prediction.ProjectId), new ImageUrl(prediction.ImageUrl));

                // Loop over each prediction and write out the results
                foreach (var outcome in result.Predictions)
                {
                    if (outcome.Probability > .70)
                        prediction.Results.Add(outcome.Tag, (decimal)outcome.Probability);
                }

                await CosmosDataService.Instance.InsertItemAsync(prediction);
                return req.CreateResponse(HttpStatusCode.OK, prediction);


            }
            catch (Exception e)
            {
                //Catch and unwind any exceptions that might be thrown and return the reason (non-production)
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.GetBaseException().Message);
            }

            async Task<string> UploadImageToBlobStorage(Stream stream)
            {
                //Create a new blob block Id
                var blobId = Guid.NewGuid().ToString() + ".jpg";

                if (_blobContainer == null)
                {
                    //You can set your endpoint here as a string in code or just set it to pull from your App Settings
                    var containerName = "images";

                    var endpoint = $"https://customvisionrdr.blob.core.windows.net/images?sv=2017-11-09&ss=b&srt=sco&sp=rwdlac&se=2018-09-01T07:03:34Z&st=2018-08-21T23:03:34Z&spr=https&sig=Tdr03QWaQ7vCiy6Ut1G%2Fs05Kaw7iZ3JbhubljsMhKXA%3D";
                    //var endpoint = $"https://YOURSTORAGEACCOUNT.blob.core.windows.net/{containerName}/?sv=2017-04-17&ss=MAKE_SURE_TO_GET_A_SAS_TOKEN-01-06T04:57:40Z&st=2018-01-05T20:57:40Z&spr=https&sig=YE2ZWYTvRax4jRUmBpZSzaCFDd8ZwM3pxSDHYWVn0dY%3D";
                    _blobContainer = new CloudBlobContainer(new Uri(endpoint));
                }

                //Create a new block to store this uploaded image data
                var blockBlob = _blobContainer.GetBlockBlobReference(blobId);
                blockBlob.Properties.ContentType = "image/jpg";

                //You can even store metadata with the content
                blockBlob.Metadata.Add("createdFor", "This Custom Vision Hackathon");

                //Upload and return the new URL associated w/ this blob content
                await blockBlob.UploadFromStreamAsync(stream);
                return blockBlob.StorageUri.PrimaryUri.ToString();
            }
        }





    }
}
