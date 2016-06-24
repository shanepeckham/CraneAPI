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
using System.IO;

namespace CraneAPI.Controllers
{
    public class BulkLoadController : ApiController
    {
        public class bulkBodyOutput
        {
            public string value { get; set; }

        }

        // POST api/face/detect
        [Route("BulkLoad")]
        [SwaggerOperation("BulkLoad")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> bulkInputBody(BulkBindingModel bulkInput)
        {
            bulkBodyOutput bulkOutput = new bulkBodyOutput
            {
                value = "",
            };

            string file = bulkInput.file;

      //      string text = System.IO.File.ReadAllText(@file);

            //var faceServiceClient = new FaceServiceClient("77d48262ff254746b7c7a152c8fd38aa");
            //var faces = await faceServiceClient.DetectAsync(file, false, false);

            // Display the file contents to the console. Variable text is a string.
          //  System.Console.WriteLine("Contents of WriteText.txt = {0}", text);

            // Example #2
            // Read each line of the file into a string array. Each element
            // of the array is one line of the file.
            string[] lines = System.IO.File.ReadAllLines(@file);

            // Display the file contents by using a foreach loop.
            System.Console.WriteLine("Contents of WriteLines2.txt = ");
            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                Console.WriteLine("\t" + line);
            }

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();

            return Ok(bulkOutput);
        }
    }
}
