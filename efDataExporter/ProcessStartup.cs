using System.Threading;
using System.Configuration;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Collections.Concurrent;
using efDataExtporter.DTO;

namespace efDataExtporter
{
    public class ProcessStartup
    {
        //default heartbeat interval (only used if not configured in app.config
        private const int DEFAULT_HEARTBEAT_INTERVAL = 60000;

        //logger object
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private CancellationTokenSource _CancelToken;

        /// <summary>
        /// Initialise application
        /// </summary>
        public void Init(CancellationTokenSource xCancelToken)
        {
            _CancelToken = xCancelToken;

            //log startup screen
            //wnDirect.WNDCodeLib.Utils.ShowStartUpText();

            //ProcessHeartbeat heartBeat = new ProcessHeartbeat();
            //string statusConnectionString = ConfigurationManager.ConnectionStrings["StatusDBConnectionString"].ConnectionString;
            //if (heartBeat.Start(statusConnectionString, xCancelToken))
            //{
                InitialiseProcessing();
                Console.ReadLine();
            //}
            //else
            //{
            //    //failed to add entry so there is a problem
            //    //shutdown
            //    log.Error("Unable to update process entry so initiating shutdown");
            //    Shutdown();
            //}
        }

        /// <summary>
        /// initialises the email processing
        /// </summary>
        /// <param name="xProcessID">process id</param>
        /// <param name="xCancelToken">cancellation token</param>
        private void InitialiseProcessing()
        {
            log.Info("Initialising processing");

            var blockingCollection = new BlockingCollection<ExportData>();
            
                //monitor data exporter table
                ExportDataMonitor monitor = new ExportDataMonitor();
                monitor.Start(_CancelToken, blockingCollection);

                //export data
                Exporter exporter = new Exporter();
                exporter.Start(_CancelToken, blockingCollection);
            
        }


        /// <summary>
        /// clean up application pre shutdown
        /// </summary>
        public void Shutdown()
        {
            _CancelToken.Cancel();
            log.Info("Shutting down...");
            //AlertMonitorService.Send("Shutting down", "This process is shutting down");

        }

        /// <summary>
        /// get configured exporter category id
        /// </summary>
        /// <returns>data exporter category id</returns>
        protected int GetExporterCategory()
        {
            int exporterCategoryID = 0;

            try
            {
                exporterCategoryID = Convert.ToInt32(ConfigurationManager.AppSettings["DataExporterCategory"].ToString());
            }
            catch(Exception)
            {
                log.Error("Configuration error, unable to find config setting 'DataExporterCategory'");
            }
            return exporterCategoryID;
        }
    }
}
