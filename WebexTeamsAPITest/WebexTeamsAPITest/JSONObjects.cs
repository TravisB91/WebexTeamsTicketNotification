using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;

namespace WebexTeamsAPITest
{
    public class JSONObjects
    {

        public string number { get; set; }
        public string assigned_to { get; set; }
        public string u_affected_user { get; set; }
        public string description { get; set; }


        public static List<JSONObjects> userTicketInfo(string json)
        {

            List<JSONObjects> jobject = new List<JSONObjects>();

            try
            {

                JObject result = new JObject();
                result = JObject.Parse(json);

                JArray records = (JArray)result["records"];

                int recentTickets = 0;

                if(records.Count > 5)
                {
                    recentTickets = 5;
                }
                else if(records.Count <= 5)
                {
                    recentTickets = records.Count();
                }

                for (int i = 0; i < recentTickets; i++)
                {
                    JSONObjects placeholder = new JSONObjects();

                    JToken findNumber = records[i]["number"];
                    JToken findShDesc = records[i]["affected_user"];
                    JToken findIncSt = records[i]["description"];
                    JToken findAssignedto = records[i]["assigned_to"];

                    placeholder.number = findNumber.ToString();
                    placeholder.u_affected_user = findShDesc.ToString();
                    placeholder.description = findIncSt.ToString();
                    placeholder.assigned_to = findAssignedto.ToString();

                    jobject.Add(placeholder);
                }
            }
            catch(Newtonsoft.Json.JsonReaderException je)
            {
                return jobject;
            }


            return jobject;

        }

        public static string JSONParser(string json, string field)
        {
            try
            {
                JObject result = new JObject();

                result = JObject.Parse(json);

                JArray records = (JArray)result["records"];

                JToken findItem = records[0][field];

                return findItem.ToString();
            }
            catch (ArgumentOutOfRangeException e)
            {
                return " ";
            }
            catch(NullReferenceException ne) //this will be for the Work_notes box or any pars that doesn't have the "records" jArray
            {
                JObject result = new JObject(); //Issue is that this part of the method will still throw an exception if the "work_notes" are in the field. 

                result = JObject.Parse(json);

                JToken findItem = result[field];

                return findItem.ToString();
            }

            return " ";
        }


        public static string MergeJSON(string json1, string json2)
        {

            string merge1 = json1.Remove(json1.IndexOf("}"));

            string merge2 = json2.Remove(json2.IndexOf("{"), json2.IndexOf("{") + 1);

            return merge1 + ", " + merge2;
        }

    }
}
