using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace TwinCAT_DataClient
{
    class Program
    {
        static string url = "http://127.0.0.1:8000/api";

        static void Main(string[] args)
        {

            //.SetFragment("after-hash");

            PostTemp();
            Console.ReadLine();
        }

        static void PostTemp()
        {
            var client = new RestClient(url);
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            string time = DateTime.Now.ToString("h:mm:ss");
            string date = DateTime.Today.ToString("yyyy-MM-dd");

            var request = new RestRequest("temperature", Method.POST);
            request.AddParameter("time", time);
            request.AddParameter("date", date);
            request.AddParameter("temp", 17.0);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            Console.WriteLine(content);
        }
    }
}
