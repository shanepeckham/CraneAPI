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
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using CraneAPI.CRM;


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

            string[] lines = File.ReadAllLines(@file);
            int iCount = 0;
            Guid contactId = new Guid();
            // Display the file contents by using a foreach loop.
            foreach (string line in lines)
            {
                string name = line.Split(',')[0];
                string imageUrl = line.Split(',')[1];
                
                Guid contactFaceId = new Guid();

                AddFaceBindingModel AddFaceInput = new AddFaceBindingModel
                {
                    faceListId = bulkInput.faceListId,
                    url = imageUrl,
                    userData = name
                };

                // Now we create the Face in the FaceList 
                FaceController.AddFaceOutput addFaceOutput = new FaceController.AddFaceOutput();
                using (var client = new HttpClient())
                {
                    globals.SetClientHeaders(client);
                    addFaceOutput = await globals.AddFaceToFaceList(client, AddFaceInput);
                    //Now we create Contact in CRM
                    globals.createCRMContactInput crmInput = new globals.createCRMContactInput();
                    crmInput.name = name;
                    crmInput.faceListId = bulkInput.faceListId;
                    crmInput.persistedFaceId = addFaceOutput.persistedFaceId;
                    crmInput.url = imageUrl;

                    if (iCount == 0)
                    {
                        globals.ConnectToCRM();
                        HttpResponseMessage response = new HttpResponseMessage();
                        response = await globals.GetBingDetails(name.ToString());
                     
                        string jsonString = await response.Content.ReadAsStringAsync();
                      
                        var dbo2 = JsonConvert.DeserializeObject<globals.RootObject>(jsonString);

                        crmInput.snippet = dbo2.webPages.value[0].snippet;

                        contactId = globals.CreateCRMContact(crmInput);

                    }

                    //Create the associated face
                    if (addFaceOutput.persistedFaceId != null)
                    {
                        contactFaceId = globals.AddFaceToCRMContact(contactId, addFaceOutput.persistedFaceId);
                    }
                }

                iCount++;
            }
            return Ok(bulkOutput);
        }
    }
}
