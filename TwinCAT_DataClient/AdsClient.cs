using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace TwinCAT_DataClient
{
    class AdsClient
    {
        private TcAdsClient adsClient;

        private int hTemperature;
        private int hProducedPieces;
        private int hIsRunning;
        private int hTempVariation;

        public AdsClient()
        {
            adsClient = new TcAdsClient();

            // Connect to local PLC - Runtime 1 - TwinCAT2 Port=801, TwinCAT3 Port=851
            adsClient.Connect(851);

            try
            {
                hTemperature = adsClient.CreateVariableHandle("MAIN.Temperature");
                hProducedPieces = adsClient.CreateVariableHandle("MAIN.ProducedPieces");
                hIsRunning = adsClient.CreateVariableHandle("MAIN.IsRunning");
                hTempVariation = adsClient.CreateVariableHandle("MAIN.TempVariation");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }


            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
        }

        public double GetTemperature()
        {
            try
            {
                double temp = (double)adsClient.ReadAny(hTemperature, typeof(double));

                return temp;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }

            return double.NaN;
        }

        public double GetProducedPieces()
        {
            try
            {
                double pieces = (double)adsClient.ReadAny(hProducedPieces, typeof(double));

                return pieces;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }

            return double.NaN;
        }

        public bool IsProcessRunning()
        {
            try
            {
                bool isRunning = (bool)adsClient.ReadAny(hIsRunning, typeof(bool));

                return isRunning;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }

            return false;
        }

        public void StopProcess()
        {
            try
            {
                adsClient.WriteAny(hIsRunning, false);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }
        }

        public void StartProcess()
        {
            try
            {
                adsClient.WriteAny(hIsRunning, true);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }
        }

        public double GetTempVariation()
        {
            try
            {
                double tempVariation = (double)adsClient.ReadAny(hTempVariation, typeof(double));

                return tempVariation;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }

            return -1;
        }

        public void SetTempVariation(double tempVariation)
        {
            try
            {
                adsClient.WriteAny(hTempVariation, tempVariation);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            //enable resources
            try
            {
                adsClient.DeleteVariableHandle(hTemperature);
                adsClient.DeleteVariableHandle(hProducedPieces);
                adsClient.DeleteVariableHandle(hIsRunning);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.ReadLine();
            }

            adsClient.Dispose();
        }
    }
}

