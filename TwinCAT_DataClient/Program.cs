using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using RestSharp;

namespace TwinCAT_DataClient
{
    class Program
    {
        //static string url = "http://127.0.0.1:8000/api";
        static string url = "https://proyecto-33-twincat-web-manage.herokuapp.com/api";

        static AdsClient adsClient = new AdsClient();

        static Timer TemperatureTimer;
        static Timer ProducedPiecesTimer;
        static Timer SyncProcessTimer;

        static Random rand = new Random();

        static double TemperatureInterval = TimeSpan.FromSeconds(30).TotalMilliseconds;
        static double ProducedPiecesInterval = TimeSpan.FromMinutes(1).TotalMilliseconds;
        static double SyncProcessInterval = TimeSpan.FromSeconds(5).TotalMilliseconds;

        static double ProducedPieces = -1;

        static void Main(string[] args)
        {
            TemperatureTimer = new Timer();
            TemperatureTimer.AutoReset = true;
            TemperatureTimer.Interval = TemperatureInterval;
            TemperatureTimer.Elapsed += TemperatureTimer_Elapsed;
            TemperatureTimer.Start();

            ProducedPiecesTimer = new Timer();
            ProducedPiecesTimer.AutoReset = true;
            ProducedPiecesTimer.Interval = ProducedPiecesInterval;
            ProducedPiecesTimer.Elapsed += ProducedPiecesTimer_Elapsed;
            ProducedPiecesTimer.Start();

            SyncProcessTimer = new Timer();
            SyncProcessTimer.AutoReset = true;
            SyncProcessTimer.Interval = SyncProcessInterval;
            SyncProcessTimer.Elapsed += SyncProcessTimer_Elapsed;
            SyncProcessTimer.Start();


            double pieces = adsClient.GetProducedPieces();
            ProducedPieces = pieces;
            PostData("process_log", new Dictionary<string, string> { { "message", "TwinCAT Web Manager data client started running. Total pieces produced: " + pieces }, { "type", "info" } });


            Console.ReadLine();
        }

        class ProcessInfo
        {
            public long id;
            public string created_at;
            public string updated_at;
            public bool isRunning;
            public double syncInterval;
            public double tempVariation;
        }

        private static void SyncProcessTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string data = GetData("process_info");

            ProcessInfo info = JsonConvert.DeserializeObject<ProcessInfo>(data);

            if (info.id == 0)
                return;


            bool shouldRun = info.isRunning;
            bool isRunning = adsClient.IsProcessRunning();


            if(isRunning != shouldRun)
            {
                if(shouldRun)
                {
                    adsClient.StartProcess();
                    PostData("process_log", new Dictionary<string, string> { { "message", "Got signal to start process on the physical PLC via TwinCAT Web Manager" }, { "type", "warning" } });
                    PostData("process_log", new Dictionary<string, string> { { "message", "Process started via TwinCAT Web Manager" } , {"type", "info"} });
                }else
                {
                    adsClient.StopProcess();
                    PostData("process_log", new Dictionary<string, string> { { "message", "Process was stopped on the physical PLC via TwinCAT Web Manager" }, { "type", "danger" } });
                }
            }

            double tempSyncInterval = info.syncInterval;

            if(TimeSpan.FromSeconds(tempSyncInterval).TotalMilliseconds != TemperatureInterval)
            {
                TemperatureInterval = TimeSpan.FromSeconds(tempSyncInterval).TotalMilliseconds;
                TemperatureTimer.Stop();
                TemperatureTimer.Interval = TemperatureInterval;
                TemperatureTimer.Start();

                PostData("process_log", new Dictionary<string, string> { { "message", "Temperature Sync Interval was changed to " + tempSyncInterval}, { "type", "info" } });
            }

            double tempVariation = info.tempVariation;
            double real_tempVariation = adsClient.GetTempVariation();

            if(tempVariation != real_tempVariation)
            {
                adsClient.SetTempVariation(tempVariation);
                PostData("process_log", new Dictionary<string, string> { { "message", "Temperature Variation was changed to " + tempVariation }, { "type", "info" } });
            }


        }

        
        private static void ProducedPiecesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double pieces = adsClient.GetProducedPieces();

            UpdateData("process_info", new Dictionary<string, string> { { "producedPieces", pieces.ToString() } });

            if (pieces - ProducedPieces >= 0)
            {
                PostData("process_log", new Dictionary<string, string> { { "message", "New batch of " + (pieces - ProducedPieces) + " pieces has been produced. Total amount produced: " + pieces}, { "type", "default" } });
                ProducedPieces = pieces;
            }
                
        }

        private static void TemperatureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double temp = adsClient.GetTemperature();

            PostData("temperature", new Dictionary<string, string> { { "temp", temp.ToString() } });
        }

        static void UpdateData(string route, Dictionary<string, string> data)
        {
            var client = new RestClient(url);

            string timestamp = GetTimestamp();

            RestRequest request = new RestRequest(route, Method.PUT);

            request.AddParameter("timestamp", timestamp);

            foreach (KeyValuePair<string, string> param in data)
                request.AddParameter(param.Key, param.Value);


            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            Console.WriteLine(content);
        }

        static string GetData(string route)
        {
            var client = new RestClient(url);

            RestRequest request = new RestRequest(route, Method.GET);

            // execute the request
            IRestResponse response = client.Execute(request);
            string content = response.Content; // raw content as string

            Console.WriteLine(content);

            return content;
        }


        static void PostData(string route, Dictionary<string, string> data)
        {
            var client = new RestClient(url);

            string timestamp = GetTimestamp();

            RestRequest request = new RestRequest(route, Method.POST);

            request.AddParameter("timestamp", timestamp);

            foreach(KeyValuePair<string, string> param in data)
                request.AddParameter(param.Key, param.Value);


            // execute the request
            IRestResponse response = client.Execute(request);
            string content = response.Content; // raw content as string

            Console.WriteLine(content);
        }

        static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("o");
        }
    }
}
