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
using Microsoft.Xrm.Sdk.Query;
using StackExchange.Redis;

namespace CraneAPI.Globals
{

    public class globals
    {
        public static IOrganizationService _orgService;
        public static ConnectionMultiplexer connection;

        public class QueryCRMFaceOutput
        {
            public List<CRMFaceItems> CRMFaceItems { get; set; }

        }
        public class cacheFaceToContact
        {
            public string persistedFaceId { get; set; }
            public string contactId { get; set; }
        }
        public class CRMFaceItems
        {
            public string name { get; set; }
            public string description { get; set; }
            public double confidence { get; set; }
            public string url { get; set; }
        }
        public class createCRMContactInput
        {
            public string url { get; set; }
            public string persistedFaceId { get; set; }
            public string faceListId { get; set; }
            public string name { get; set; }
            public string snippet { get; set; }

        }


        public class QueryContext
        {
            public string originalQuery { get; set; }
            public string alteredQuery { get; set; }
            public string alterationOverrideQuery { get; set; }
            public bool adultIntent { get; set; }
        }

        public class About
        {
            public string name { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public List<About> about { get; set; }
            public string displayUrl { get; set; }
            public string snippet { get; set; }
            public string dateLastCrawled { get; set; }
        }

        public class WebPages
        {
            public string webSearchUrl { get; set; }
            public int totalEstimatedMatches { get; set; }
            public List<Value> value { get; set; }
        }

        public class Value2
        {
            public string id { get; set; }
        }

        public class Item
        {
            public string answerType { get; set; }
            public int resultIndex { get; set; }
            public Value2 value { get; set; }
        }

        public class Mainline
        {
            public List<Item> items { get; set; }
        }

        public class RankingResponse
        {
            public Mainline mainline { get; set; }
        }

        public class RootObject
        {
            public string _type { get; set; }
            public QueryContext queryContext { get; set; }
            public WebPages webPages { get; set; }
            public RankingResponse rankingResponse { get; set; }
        }

        // bing
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
        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("cranecache.redis.cache.windows.net:6380,password=Dd99O7WPaA1HHCY9uIAfNbmPo6fpGQI8xQn5k50Gmr0=,ssl=True,abortConnect=True");
        });

  

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

        public static int buildCRMFaceCache()
        {
            globals.connection = globals.Connection;
            Entity face = new Entity("crmazure_faces");
            //Create a query expression specifying the link entity alias and the columns of the link entity that you want to return
            QueryExpression qe = new QueryExpression();
            qe.EntityName = "crmazure_faces";
            qe.ColumnSet = new ColumnSet();
            qe.ColumnSet.AddColumns("crmazure_name", "crmazure_faceid");

            //qe.LinkEntities[0].Columns.AddColumns("name", "crmazure_faceid");
            //qe.LinkEntities[0].EntityAlias = "primarycontact";
            qe.Criteria = new FilterExpression();
            qe.Criteria.AddCondition("crmazure_name", ConditionOperator.NotNull);
            qe.Criteria.AddCondition("crmazure_faceid", ConditionOperator.NotNull);

            EntityCollection ec = _orgService.RetrieveMultiple(qe);
            int icount = 0;
            IDatabase cache = Connection.GetDatabase();
            CRMFaceItems crmFaceItems = new CRMFaceItems();

            foreach (var a in ec.Entities)
            {
                // If key exists, it is overwritten.
               
                EntityReference ContactFace = a.GetAttributeValue<EntityReference>("crmazure_faceid");
                string contactId = ContactFace.Id.ToString();
                cache.StringSet(a.GetAttributeValue<string>("crmazure_name"), contactId);

                QueryExpression qc = new QueryExpression();
                qc.EntityName = "contact";
                qc.ColumnSet = new ColumnSet();
                qc.ColumnSet.AddColumns("firstname", "lastname", "description", "websiteurl");

                qc.Criteria = new FilterExpression();
                qc.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);

                EntityCollection econ = _orgService.RetrieveMultiple(qc);
                foreach (var con in econ.Entities)
                {

                    crmFaceItems.name = con.GetAttributeValue<string>("firstname") + " " + con.GetAttributeValue<string>("lastname");
                    crmFaceItems.description = con.GetAttributeValue<string>("description");
                    crmFaceItems.url = con.GetAttributeValue<string>("websiteurl");

                    cache.StringSet(contactId, JsonConvert.SerializeObject(crmFaceItems));

                }


                icount++;
            }

            return icount;
        }

        public static CRMFaceItems QueryCRMForFace(string persistedFaceId, double confidence)
        {
            QueryCRMFaceOutput CRMFaceOutput = new QueryCRMFaceOutput();
            CRMFaceItems crmFaceItems = new CRMFaceItems();
  
            Entity face = new Entity("crmazure_faces");
            Entity contact = new Entity("contact");

            //Create a query expression specifying the link entity alias and the columns of the link entity that you want to return
            QueryExpression qe = new QueryExpression();
            qe.EntityName = "crmazure_faces";
            qe.ColumnSet = new ColumnSet();
            qe.ColumnSet.AddColumns("crmazure_name", "crmazure_faceid");

            //qe.LinkEntities[0].Columns.AddColumns("name", "crmazure_faceid");
            //qe.LinkEntities[0].EntityAlias = "primarycontact";
            qe.Criteria = new FilterExpression();
            qe.Criteria.AddCondition("crmazure_name", ConditionOperator.Equal, persistedFaceId);

            EntityCollection ec = _orgService.RetrieveMultiple(qe);
            int icount = 0;

            foreach (var a in ec.Entities)
            {
                EntityReference ContactFace = a.GetAttributeValue<EntityReference>("crmazure_faceid");
                Guid contactId = ContactFace.Id;
                QueryExpression qc = new QueryExpression();
                qc.EntityName = "contact";
                qc.ColumnSet = new ColumnSet();
                qc.ColumnSet.AddColumns("firstname", "lastname", "description", "websiteurl");

                qc.Criteria = new FilterExpression();
                qc.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId.ToString());

                EntityCollection econ = _orgService.RetrieveMultiple(qc);
                foreach (var con in econ.Entities)
                {
                  
                    crmFaceItems.name = con.GetAttributeValue<string>("firstname") + " " + con.GetAttributeValue<string>("lastname");
                    crmFaceItems.description = con.GetAttributeValue<string>("description");
                    crmFaceItems.url = con.GetAttributeValue<string>("websiteurl");
                    crmFaceItems.confidence = confidence;
                    icount++;
                }
            }

          //  CRMFaceOutput.CRMFaceItems = crmFaceItems;

            return crmFaceItems;
        }
        public static void foo_OnClick()
        {
            var sem = "sem";
        }
        public static CRMFaceItems QueryCRMForFaceCache(string persistedFaceId, double confidence)
        {
            QueryCRMFaceOutput CRMFaceOutput = new QueryCRMFaceOutput();
            IDatabase cache = Connection.GetDatabase();

            Entity face = new Entity("crmazure_faces");
            Entity contact = new Entity("contact");
            string contactId = cache.StringGet(persistedFaceId);

            CRMFaceItems crmFaceItems = JsonConvert.DeserializeObject<CRMFaceItems>(cache.StringGet(contactId));

            return crmFaceItems;
        }

        public static Guid AddFaceToCRMContact(Guid contactId, string persistedFaceId)
        {
            Guid contactFaceId = new Guid();

            crmazure_faces crmFace = new crmazure_faces();
            crmFace.crmazure_name = persistedFaceId;
            contactFaceId = _orgService.Create(crmFace);

            EntityReferenceCollection relatedFace = new EntityReferenceCollection();
            relatedFace.Add(new EntityReference(crmazure_faces.EntityLogicalName, contactFaceId));

            Relationship faceRelationship = new Relationship("crmazure_contact_crmazure_faces");
            _orgService.Associate(Contact.EntityLogicalName, contactId, faceRelationship, relatedFace);
            //crmazure_faces contactFace = new crmazure_faces();
            //EntityReference contact = new EntityReference();
            //contact.Id = contactId;
            //contact.LogicalName = "Contact";

            //contact.Id = contactId;
            //contactFace.crmazure_FaceId = contact;
            //contactFace.crmazure_ContactFaceId = persistedFaceId;
            //contactFace.crmazure_name = persistedFaceId;
            //contactFaceId = _orgService.Create(contactFace);

            //Relationship faceRelationship = new Relationship("crmazure_contact_crmazure_faces");
            //EntityCollection relatedFacesToCreate = new EntityCollection
            //{
            //    EntityName = crmazure_faces.EntityLogicalName,
            //    Entities =
            //            {
            //                new crmazure_faces{crmazure_ContactFaceId = persistedFaceId, crmazure_facesId = contactId}
            //            }
            //};

            //accountToCreate.RelatedEntities.Add(letterRelationship, relatedLettersToCreate);


            return contactFaceId;
        }

        public static Guid CreateCRMContact(createCRMContactInput CRMInput)
        {
            Guid contactId = new Guid();
            Contact contact = new Contact();
        //    contact.crmazure_faceId = CRMInput.persistedFaceId;
            contact.FirstName = CRMInput.name.Split(' ')[0];
            contact.LastName = CRMInput.name.Split(' ')[1];
            if (CRMInput.snippet.Length > 1999)
                {
                    contact.Description = CRMInput.snippet.Substring(0, 1999);
                }
            else
                {
                contact.Description = CRMInput.snippet;
            }

            

            //Get the image

            using (WebClient webClient = new WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(CRMInput.url);

                contact.EntityImage = imageBytes;
            }

            contact.WebSiteUrl = CRMInput.url;
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

