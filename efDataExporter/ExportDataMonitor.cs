using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using efDataExtporter.DTO;


namespace efDataExtporter
{
    public class ExportDataMonitor
    {
        //logger object
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
               (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// start thread to monitor for data to export
        /// </summary>
        /// <param name="xCancelToken">cancellation token</param>
        /// <param name="xProcessingQueue">processing queue</param>
        /// <param name="xExportDataCategoryID">export data category</param>
        public void Start(CancellationTokenSource xCancelToken, BlockingCollection<ExportData> xProcessingQueue)
        {
            _log.Info("Starting ExportDataMonitor thread...");

            int checkInterval = 500 ; 

            ExportData exportData = new ExportData();
            var task = Task.Factory.StartNew(async () =>  // <- marked async
            {
                while (true)
                {
                    if (xCancelToken.IsCancellationRequested)
                    {
                        _log.Debug("Stopping ExportDataMonitor thread");
                        break;
                    }
                    else
                    {

                        try
                        {
                            QueueDataToExport(xCancelToken, xProcessingQueue);
                        }
                        catch(Exception ex)
                        {
                            _log.Error("Unexpected error queuing data for export: ", ex);
                        }
                        

                        await Task.Delay(checkInterval, xCancelToken.Token); // <- await with cancellation
                    }
                }
            }, xCancelToken.Token);
        }

        /// <summary>
        /// queue data to be sent
        /// this gets the data from the db and updates status to running
        /// it also places it on a queue for another thread to process
        /// </summary>
        /// <param name="xCancelToken">cancellation token</param>
        /// <param name="xProcessingQueue">processing queue</param>
        /// <param name="xExportDataCategoryID">category id</param>
        public void QueueDataToExport(CancellationTokenSource xCancelToken, BlockingCollection<ExportData> xProcessingQueue)
        {
           ExportData exportData = new ExportData();
           List<ExportData> exportDataList = exportData.GetAllScheduledToRun();

            if(exportDataList.Count > 0)
            {
                _log.Debug($"Schedules found : {exportDataList.Count} Queuing...");
                foreach(ExportData dataToQueue in exportDataList)
                {
                    if (xCancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        //set status to queued
                        dataToQueue.RunStatusID = (int)ExportDataStatus.STATUS.RUNNING;
                        dataToQueue.Save();

                        xProcessingQueue.Add(dataToQueue);
                        _log.Info(dataToQueue.ExportDataID + " id queued for delivery");
                    }
                    
                }
            }
        }
    }
}
