using System;
using System.Threading;
using System.ServiceProcess;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Data;
using efDataExporter.Helpers;

namespace efDataExtporter
{
    class Program
    {
        //default heartbeat interval (only used if not configured in app.config
        private const int DEFAULT_HEARTBEAT_INTERVAL = 60000;

        //logger object
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


       

        static void Main(string[] args)
        {
            var cancelTokenSource = new CancellationTokenSource();

            //MongoDBUtility mdbu = new MongoDBUtility();
            //mdbu.GetEnquiries();

            if (Environment.UserInteractive)
            {

                ProcessStartup processStartup = new ProcessStartup();

                //initialise program
                processStartup.Init(cancelTokenSource);

                //wait
                do
                {
                    if (cancelTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    while (!Console.KeyAvailable)
                    {
                        // Do something

                        if (cancelTokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
            else
            {
                // Run as Windows Service.
                var ServicesToRun = new ServiceBase[]
                    {
                        new ServiceController()
                    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
