using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SignableSendTwoSigAPIDemo
{
    class Program
    {
        static string getTemplates = "https://api.signable.co.uk/v1/templates?offset=0&limit=3";

        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            Console.WriteLine("Enter your API Key from  https://app.signable.co.uk/api");
            string apiKey = Console.ReadLine();

            var webClient = CreateWebClient(apiKey);

            GetDetail(webClient, getTemplates);

            Console.WriteLine("Enter first name of the person who is getting this email");
            string firstName = Console.ReadLine();
            Console.WriteLine("Enter surname of the person who is getting this email");
            string surName = Console.ReadLine();
            Console.WriteLine("Enter email of the person who is getting this email");
            string email = Console.ReadLine();

            var envelopesParties = new EnvelopeParties();
            envelopesParties.party_name = firstName + " " + surName;
            envelopesParties.party_email = email;
            envelopesParties.party_id = "1454030"; //template_parties, party_id
            envelopesParties.party_message = "Please sign this!";

            Console.WriteLine("Enter item for the report");
            string reportItem = Console.ReadLine();
            Console.WriteLine("Enter total for the report");
            string reportTotal = Console.ReadLine();

            var documentMergeFields = new List<MergeFields>();
            documentMergeFields.Add(new MergeFields() { field_id = "3572840", field_value = reportItem });
            documentMergeFields.Add(new MergeFields() { field_id = "3572841", field_value = reportTotal });

            var envelopeDoucments = new EnvelopeDocuments();
            envelopeDoucments.document_title = "Signature request (no password)";
            envelopeDoucments.document_template_fingerprint = "8cf5eeda20ab327400b92461cd305315";
            envelopeDoucments.document_merge_fields = documentMergeFields;


            var javaScriptSerializer = new JavaScriptSerializer();

            var envelopePartiesList = new List<EnvelopeParties>();
            envelopePartiesList.Add(envelopesParties);
            var envelopePartiesJSONString = javaScriptSerializer.Serialize(envelopePartiesList);

            var envelopeDocumentsList = new List<EnvelopeDocuments>();
            envelopeDocumentsList.Add(envelopeDoucments);
            var envelopeDocumentsJSONString = javaScriptSerializer.Serialize(envelopeDocumentsList);

            SendEmail(apiKey, "Credit Card Receipt", envelopePartiesJSONString, envelopeDocumentsJSONString);


            Console.WriteLine("End");
            Console.WriteLine("Press any key to end ...");
            Console.ReadLine();
        }

        private static WebClient CreateWebClient(string apiKey)
        {
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(apiKey + ":x"));
            var authorization = string.Format("Basic {0}", credentials);
            webClient.Headers[HttpRequestHeader.Authorization] = authorization;
            return webClient;
        }

        private static void GetDetail(WebClient webClient, string apiCommand)
        {
            var results = webClient.DownloadString(apiCommand);
            Console.WriteLine(results);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadLine();
        }

        private static void SendEmail(string apiKey, string envelopeTitle, string envelopePartiesJsonString, string envelopeDocumentsJsonString)
        {
            var boundary = "---" + Guid.NewGuid();

            var parameterWrapper = "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n--" + boundary;

            var bytes = Encoding.UTF8.GetBytes(apiKey + ":x");
            var authorization = Convert.ToBase64String(bytes);

            var sb = new StringBuilder();
            sb.Append("--" + boundary);
            sb.Append(string.Format(parameterWrapper, "envelope_title", envelopeTitle));
            sb.Append(string.Format(parameterWrapper, "envelope_parties", envelopePartiesJsonString));
            sb.Append(string.Format(parameterWrapper, "envelope_documents", envelopeDocumentsJsonString));
            var envelopeBody = sb.ToString();

            var client = new RestClient("https://api.signable.co.uk/v1/envelopes");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Basic " + authorization);

            request.AddHeader("content-type", "multipart/form-data; boundary=" + boundary);
            request.AddParameter("multipart/form-data; boundary=" + boundary, envelopeBody, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            Console.WriteLine(response.Content);
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }

    internal class EnvelopeParties
    {
        public string party_email { get; set; }
        public string party_id { get; set; }
        public string party_message { get; set; }
        public string party_name { get; set; }
    }

    internal class MergeFields
    {
        public string field_id { get; set; }
        public string field_value { get; set; }
    }

    internal class EnvelopeDocuments
    {
        public string document_template_fingerprint { get; set; }
        public string document_title { get; set; }
        public List<MergeFields> document_merge_fields { get; set; }
    }
}
