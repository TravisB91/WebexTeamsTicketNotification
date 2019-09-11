using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace WebexTeamsAPITest
{
    public class WebInteractions
    {

        

        CookieContainer cookies = new CookieContainer();
        HttpClientHandler handler = new HttpClientHandler();

        HttpClient clientTeams;
        HttpClient clientSNOW;

        bool loginSuccess { get; set; }
               
        string snowURL = "https://paychex.service-now.com/";

        string webexTeamsURL = "";

        public string user_name = Environment.UserName;


        public string ticketSysID { get; set; }

        private List<string> tickets = new List<string>();

        public WebInteractions()
        {
            handler.CookieContainer = cookies;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

       
        //this is under the assumption that authenticatoin for SNOW has already been done. In my current environment for the JSONv2 service I had to go a roundabout way.
        //the round about way has been removed as it has company specific data that is not privy to any outside knowledge. 
        public async Task<string> GetResponse(string url)
        {

            string result = " ";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");


            using (var response = await clientSNOW.SendAsync(request).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    loginSuccess = true;
                    using (HttpContent content = response.Content)
                    {
                        result = content.ReadAsStringAsync().Result;
                    }
                }
                else
                {
                    loginSuccess = false;
                }
            }


            return result;
        }  

        

        public async void postToTeams()
        {
            string results;

            List<JSONObjects> jObjects = new List<JSONObjects>();

            jObjects = JSONObjects.userTicketInfo(GetResponse(snowURL + "/incident.do?JSONv2&sysparm_query=assignment_group=aebac70a484e450050b9a061ace9668b^active=true^u_submission_source=self+service^state!=6^ORDERBYDESCnumber").Result);

            if(jObjects.Count > 0)
            {
                foreach (JSONObjects jobject in jObjects)
                {


                    if (tickets.Count > 0)
                    {

                        if (tickets.Contains(jobject.number))
                        {
                            continue;
                        }

                    }

                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        roomId = "<roomId>",
                        text = "New Ticket From " + JSONObjects.JSONParser(GetResponse(snowURL + "sys_user.do?JSONv2&sysparm_query=sys_id=" + jobject.u_affected_user).Result, "user_name") + "\nTicketNumber: " + jobject.number + "\nDescription: " + jobject.description + "\nAssigned To: " + JSONObjects.JSONParser(GetResponse(snowURL + "sys_user.do?JSONv2&sysparm_query=sys_id=" + jobject.assigned_to).Result, "user_name")
                    });

                    clientTeams = new HttpClient(handler);

                    clientTeams.BaseAddress = new Uri("https://api.ciscospark.com/v1/messages");
                    clientTeams.DefaultRequestHeaders.Accept.Clear();
                    clientTeams.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", "<bot token>");
                    clientTeams.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    clientTeams.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");

                    HttpContent content = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var result = await clientTeams.PostAsync(clientTeams.BaseAddress, content).ConfigureAwait(false))
                    {
                        //result.EnsureSuccessStatusCode();

                        using (HttpContent contents = result.Content)
                            results = contents.ReadAsStringAsync().Result;
                    }

                    tickets.Add(jobject.number);
                }
                
            }
           
        }




    }
}
