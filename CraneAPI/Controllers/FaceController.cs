using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using Microsoft.ProjectOxford.Face;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using CraneAPI.Models;
using CraneAPI.Globals;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace CraneAPI.Controllers
{
    public class FaceController : ApiController
    {

        class detectBodyInput
        {
            public string url { get; set; }
        }

        public class faceRectangle
        {
            public int top { get; set; }
            public int left { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class AddFaceOutput
        {
            public string persistedFaceId { get; set; }
        }

        public class FindSimilarOutput
        {
            public string persistedFaceId { get; set; }
            public double confidence { get; set; }
        }

        public class detectBodyOutput
        {
            public string faceId { get; set; }
            public faceRectangle faceRectangles { get; set; }

        }


        // POST api/face/detect
        [Route("Detect")]
        [SwaggerOperation("Detect")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> detectInputBody(DetectBindingModel detectInput)
        {

            string url = detectInput.url;

            detectBodyOutput detectOutput = new detectBodyOutput
            {
                faceId = "",
            };

            using (var client = new HttpClient())
            {
                // Start Face ai
                globals.SetClientHeaders(client);

                // Detect a face
                detectBodyInput db = new detectBodyInput() { url = url };
                HttpResponseMessage response = await client.PostAsJsonAsync("face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false", db);
                if (response.IsSuccessStatusCode)
                {
                    detectBodyOutput dbo = new detectBodyOutput();
                    string data = await response.Content.ReadAsStringAsync();
                    var dbo2 = JsonConvert.DeserializeObject<detectBodyOutput[]>(data);
                    detectOutput.faceId = dbo2[0].faceId;

                    FindSimilarBindingModel findSimilarInput = new FindSimilarBindingModel
                    {
                        faceId = dbo2[0].faceId,
                        faceListId = detectInput.faceListId,
                        maxNumOfCandidatesReturned = 10
                    };

                    //Now we Find similar
                    FindSimilarOutput findSimilarOuput = new FindSimilarOutput();
                    findSimilarOuput = await globals.FindSimilarFace(client, findSimilarInput);

                    //Now we get the details from CRM
                    globals.ConnectToCRM();
                    globals.QueryCRMFaceOutput crmFaceOutput =  globals.QueryCRMForFace(findSimilarOuput.persistedFaceId, findSimilarOuput.confidence);

                }

            }

            //ConnectToCRM();
            return Ok(detectOutput);
        }

        // POST api/face/detect
        [Route("AddFaceToFaceList")]
        [SwaggerOperation("AddFaceToFaceList")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> AddFaceInputBody(AddFaceBindingModel AddFaceInput)
        {
            AddFaceOutput addFaceOutput = new AddFaceOutput();
            using (var client = new HttpClient())
            {
                globals.SetClientHeaders(client);

                addFaceOutput = await globals.AddFaceToFaceList(client, AddFaceInput);
            }

            return Ok(addFaceOutput);
        }

        static async Task RunDetectAsync(string Url)
        {
            Guid faceId;
            var faceServiceClient = new FaceServiceClient("77d48262ff254746b7c7a152c8fd38aa");
            var faces = await faceServiceClient.DetectAsync(Url, false, false);

            foreach (var face in faces)
            {
                faceId = face.FaceId;
            }
            // return faceId;
        }

    }

}

