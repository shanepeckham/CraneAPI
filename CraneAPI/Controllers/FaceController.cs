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

        public class detectBodyOutput
        {
            public Guid faceId { get; set; }
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
                faceId = new Guid(),
            };

            //var faceServiceClient = new FaceServiceClient("77d48262ff254746b7c7a152c8fd38aa");
            //var faces = await faceServiceClient.DetectAsync(url, false, false);

            //foreach (var face in faces)
            //{
            //    detectOutput.faceId = face.FaceId;
            //}




            //    await RunDetectAsync(Url);

            using (var client = new HttpClient())
            {
                // New code:
                globals.SetClientHeaders(client);

                // HTTP POST
                detectBodyInput db = new detectBodyInput() { url = url };
                HttpResponseMessage response = await client.PostAsJsonAsync("face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false", db);
                if (response.IsSuccessStatusCode)
                {
                    string s = response.ToString();
                    detectBodyOutput dbo = new detectBodyOutput();

                    string data = await response.Content.ReadAsStringAsync();
                    //  string data = await response.Content.ReadAsStringAsync();

                    //     JavaScriptSerializer JSserializer = new JavaScriptSerializer();
                    //deserialize to your class
                    //  var detectedFace = JSserializer.Deserialize<detectBodyOutput[]>(data);
                    var dbo2 = JsonConvert.DeserializeObject<detectBodyOutput[]>(data);
                    detectOutput.faceId = dbo2[0].faceId;
                    // detectBodyOutput dbo = await response.Content.ReadAsAsync<detectBodyOutput>();
                    //    var dbo = await response.Content.ReadAsAsync<string>();
                    //    List<faceRectangle> data = JsonConvert.DeserializeObject<List<faceRectangle>>(response.Content.ReadAsAsync<faceRectangle>().ToString());
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

