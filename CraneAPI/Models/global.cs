using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
// These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
// located in the SDK\bin folder of the SDK download.
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using CraneAPI.Controllers;
using Newtonsoft.Json;
using CraneAPI.Models;
using CraneAPI.CRM;
using System.IO;
using System.Net;
using System.Web;

namespace CraneAPI.Globals
{

    public class globals
    {
        public static IOrganizationService _orgService;
        public class createCRMContactInput
        {
            public string url { get; set; }
            public string persistedFaceId { get; set; }
            public string faceListId { get; set; }
            public string name { get; set; }

        }

        public static HttpClient SetClientHeaders(HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.projectoxford.ai/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "77d48262ff254746b7c7a152c8fd38aa");
            return client;
        }

        public static async Task<FaceController.FindSimilarOutput> FindSimilarFace(HttpClient client, FindSimilarBindingModel findSimilarInputBody)
        {
            FaceController.FindSimilarOutput findSimilarOutput = new FaceController.FindSimilarOutput
            {
                persistedFaceId = "",
                confidence = 0.00
            };

            using (client = new HttpClient())
            {
                // New code:
                globals.SetClientHeaders(client);

                // HTTP POST
                HttpResponseMessage response = await client.PostAsJsonAsync("face/v1.0/findsimilars", findSimilarInputBody);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var dbo2 = JsonConvert.DeserializeObject<FaceController.FindSimilarOutput[]>(data);
                    findSimilarOutput.persistedFaceId = dbo2[0].persistedFaceId;
                    findSimilarOutput.confidence = dbo2[0].confidence;
                }
            }

            return findSimilarOutput;
        }

        public static async Task<FaceController.AddFaceOutput> AddFaceToFaceList(HttpClient client, AddFaceBindingModel addFaceInputBody)
        {

            FaceController.AddFaceOutput addFaceOutput = new FaceController.AddFaceOutput();
            //{
            //    persistedFaceId = "",
            //};

            using (client = new HttpClient())
            {
                // New code:
                globals.SetClientHeaders(client);

                // HTTP POST
                HttpResponseMessage response = await client.PostAsJsonAsync("face/v1.0/facelists/" + addFaceInputBody.faceListId + "/persistedFaces?userData=" + addFaceInputBody.userData, addFaceInputBody);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var dbo2 = JsonConvert.DeserializeObject<FaceController.AddFaceOutput>(data);
                    addFaceOutput.persistedFaceId = dbo2.persistedFaceId;
                }
            }

            return addFaceOutput;
        }

        public static void ConnectToCRM()
        {

            String connectionString = GetServiceConfiguration();
            // Connect to the CRM web service using a connection string.
            CrmServiceClient conn = new Microsoft.Xrm.Tooling.Connector.CrmServiceClient(connectionString);

            // Cast the proxy client to the IOrganizationService interface.
            _orgService = (IOrganizationService)conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;
        }

        public static async Task<HttpResponseMessage> GetBingDetails(string search)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "652fbb69c13248628deac2340632ac77");

            // Request parameters
            queryString["q"] = search;
            queryString["count"] = "1";
            queryString["offset"] = "0";
            queryString["mkt"] = "en-us";
            queryString["safesearch"] = "Moderate";
            var uri = "https://api.cognitive.microsoft.com/bing/v5.0/search?" + queryString;

            var response = await client.GetAsync(uri);

            return response;
        }

        public static Guid CreateCRMContact(createCRMContactInput CRMInput)
        {
            Guid contactId = new Guid();
            Contact contact = new Contact();
            contact.crmazure_faceId = CRMInput.persistedFaceId;
            contact.FirstName = CRMInput.name.Split(' ')[0];
            contact.LastName = CRMInput.name.Split(' ')[1];

            //Get the image
           
            using (WebClient webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(CRMInput.url);

                contact.EntityImage = imageBytes;
            }

            contactId = _orgService.Create(contact);

            return contactId;
        }

        private static String GetServiceConfiguration()
        {
            // Get available connection strings from app.config.
            int count = System.Configuration.ConfigurationManager.ConnectionStrings.Count;

            // Create a filter list of connection strings so that we have a list of valid
            // connection strings for Microsoft Dynamics CRM only.
            List<KeyValuePair<String, String>> filteredConnectionStrings =
                new List<KeyValuePair<String, String>>();

            for (int a = 0; a < count; a++)
            {
                if (isValidConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[a].ConnectionString))
                    filteredConnectionStrings.Add
                        (new KeyValuePair<string, string>
                            (System.Configuration.ConfigurationManager.ConnectionStrings[a].Name,
                            System.Configuration.ConfigurationManager.ConnectionStrings[a].ConnectionString));
            }

            // No valid connections strings found. Write out and error message.
            if (filteredConnectionStrings.Count == 0)
            {
                Console.WriteLine("An app.config file containing at least one valid Microsoft Dynamics CRM " +
                    "connection string configuration must exist in the run-time folder.");
                Console.WriteLine("\nThere are several commented out example connection strings in " +
                    "the provided app.config file. Uncomment one of them and modify the string according " +
                    "to your Microsoft Dynamics CRM installation. Then re-run the sample.");
                return null;
            }

            // If one valid connection string is found, use that.
            if (filteredConnectionStrings.Count == 1)
            {
                return filteredConnectionStrings[0].Value;
            }

            // If more than one valid connection string is found, let the user decide which to use.
            if (filteredConnectionStrings.Count > 1)
            {
                Console.WriteLine("The following connections are available:");
                Console.WriteLine("------------------------------------------------");

                for (int i = 0; i < filteredConnectionStrings.Count; i++)
                {
                    Console.Write("\n({0}) {1}\t",
                    i + 1, filteredConnectionStrings[i].Key);
                }

                Console.WriteLine();

                Console.Write("\nType the number of the connection to use (1-{0}) [{0}] : ",
                    filteredConnectionStrings.Count);
                String input = Console.ReadLine();
                int configNumber;
                if (input == String.Empty) input = filteredConnectionStrings.Count.ToString();
                if (!Int32.TryParse(input, out configNumber) || configNumber > count ||
                    configNumber == 0)
                {
                    Console.WriteLine("Option not valid.");
                    return null;
                }

                return filteredConnectionStrings[configNumber - 1].Value;

            }
            return null;

        }

        private static Boolean isValidConnectionString(String connectionString)
        {
            // At a minimum, a connection string must contain one of these arguments.
            if (connectionString.Contains("Url=") ||
                connectionString.Contains("Server=") ||
                connectionString.Contains("ServiceUri="))
                return true;

            return false;
        }

    }
}

