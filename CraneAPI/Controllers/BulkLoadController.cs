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

            string[] lines = System.IO.File.ReadAllLines(@file);

            // Display the file contents by using a foreach loop.
            foreach (string line in lines)
            {
                string name = line.Split(',')[0];
                string imageUrl = line.Split(',')[1];

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
                    globals.ConnectToCRM();  
                    Account account = new Account { Name = "Fourth Coffee" };
                    account.AccountCategoryCode = new OptionSetValue((int)AccountAccountCategoryCode.PreferredCustomer);
                    account.CustomerTypeCode = new OptionSetValue((int)AccountCustomerTypeCode.Investor);

                    // Create an account record named Fourth Coffee.
                    _accountId = _orgService.Create(account);
                }

            }
            return Ok(bulkOutput);
        }
    }
}
