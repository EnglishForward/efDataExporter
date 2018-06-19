using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using efDataExtporter.DTO;
using efDataExporter.Helpers;
using System.Data.SqlClient;

namespace efDataExtporter
{
    public class Exporter
    {
        private CancellationTokenSource _CancelToken;

        //logger object
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _timeOut;
        private int _AlertScheduleDelayWarningMinutes;
        private string _InstanceName;

        public void Start(CancellationTokenSource xCancelToken, BlockingCollection<ExportData> xProcessingQueue)
        {
            _log.Debug("Starting Exporter thread...");
            _CancelToken = xCancelToken;

            GetConfigurationSettings();

            foreach (ExportData ExportData in xProcessingQueue.GetConsumingEnumerable())
            {
                if (xCancelToken.IsCancellationRequested)
                {
                    _log.Debug("Stopping Exporter thread");
                    break;
                }
                else
                {
                    try
                    {

                        if (ExportData != null)
                        {
                            Task.Factory.StartNew(() =>
                                {
                                    int exportLogID = StartSummaryLog(ExportData.ExportDataID);

                                    bool success = false;

                                    try
                                    {
                                        success = Export(ExportData);
                                    }
                                    catch (Exception ex)
                                    {
                                        _log.Error("Unexpected error attempting to export " + ExportData.Description, ex);
                                        //wnDirect.WNDCodeLib.Monitors.AlertMonitorService.Send("Failed to export data " + ExportData.Description, ex.Message);
                                    }

                                    if (success)
                                    {
                                        StopSummaryLog(exportLogID);
                                    }
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Unexpected error", ex);
                    }
                }
            }
        }

        private void GetConfigurationSettings()
        {
            try
            {
                _timeOut = ConfigurationManager.AppSettings["ConnectionTimeOut"].ToString();
            }
            catch
            {
                _timeOut = string.Empty;
            }

            try
            {
                _AlertScheduleDelayWarningMinutes = Int32.Parse(ConfigurationManager.AppSettings["AlertScheduleDelayWarningMinutes"].ToString());
            }
            catch
            {
                _AlertScheduleDelayWarningMinutes = 5;
            }

            try
            {
                _InstanceName = ConfigurationManager.AppSettings["processInstanceID"].ToString();
            }
            catch (Exception)
            {
                _InstanceName = "efDataExtporter";

            }
        }

        protected bool Export(ExportData xExportData)
        {
            //get data
            bool exportSuccess = false;

            DateTime dbNextRun = DateTime.UtcNow;
            DateTime thisRunStartTimeStamp = System.DateTime.UtcNow;

            try
            {
                _log.Info(String.Format("Starting '{0}' export", xExportData.Description));

                TimeSpan ts = new TimeSpan();
                ts = thisRunStartTimeStamp - dbNextRun;
                if (ts.TotalMinutes > _AlertScheduleDelayWarningMinutes)
                {
                    string message = string.Format("Data Export executing after scheduled interval by {0} min - {1}", Math.Round(ts.TotalMinutes, 2), xExportData.Description);
                    _log.Info(message);
                }

                DataTable spData = GetData(xExportData);

                _log.Info("SP rows count: " + spData.Rows.Count);

                if ((spData.Rows.Count > 0) ||
                    (xExportData.SendWhenNoData))
                {
                    _log.Info("Recipient: " + xExportData.Recipient);

                    if (xExportData.Recipient != "")
                    {
                        //send
                        xExportData.FTPFilename = xExportData.Description;

                        if (Send(spData,
                             xExportData.ExportDataType,
                               xExportData.TransportType,
                               xExportData.Recipient,
                                xExportData.EmailSubject,
                                xExportData.EmailBody,
                                xExportData.FTPFilename,
                                xExportData.ShowHeaders))
                        {
                            //update status
                            xExportData.RunStatusID = (int)ExportDataStatus.STATUS.RUN_COMPLETE;
                            exportSuccess = true;
                        }
                        else
                        {
                            _log.Info(String.Format("{0} failed to send", xExportData.Description));
                            xExportData.RunStatusID = (int)ExportDataStatus.STATUS.RUN_FAILED;
                        }
                    }
                    else
                    {
                        _log.Error("No Recipients so nowhere to send");
                        //update status
                        xExportData.RunStatusID = (int)ExportDataStatus.STATUS.RUN_COMPLETE;
                    }
                }
                else
                {
                    _log.Info("No data found for " + xExportData.Description + " and not set to send when no data");

                    //update status
                    xExportData.RunStatusID = (int)ExportDataStatus.STATUS.RUN_COMPLETE;
                    exportSuccess = true;
                }


            }
            catch (Exception ex)
            {
                _log.Error("Unexpected error attempting to export " + xExportData.Description, ex);
                EmailSender ems = new EmailSender();
                ems.Send("glennmalcolmwilkinson@gmail.com", "DataExporter error", ex.ToString(), "");
            }
            finally
            {
                xExportData.RunStatusID = (int)ExportDataStatus.STATUS.RUN_COMPLETE;

                xExportData.LastRunStartTime = thisRunStartTimeStamp;
                xExportData.LastRunEndTime = DateTime.UtcNow;
                //xExportData.NextRun = dbNextRun;
                do
                {
                    xExportData.NextRun = GetNextRunTime(xExportData.NextRun, xExportData.RecurrenceIntervalType, xExportData.RecurrenceInterval);

                } while (xExportData.NextRun <= DateTime.UtcNow);

                xExportData.LastRunEndTime = System.DateTime.UtcNow;

                xExportData.Save();

                _log.Info(String.Format("{0} exported with status {1}", xExportData.Description, (ExportDataStatus.STATUS)xExportData.RunStatusID));
                _log.Info(String.Format("Next run for {0} will be at {1} UTC ", xExportData.Description, xExportData.NextRun.ToString("yyyy-MM-dd HH:mm:ss")));

            }

            return exportSuccess;
        }

        protected static int StartSummaryLog(int xExportDataID)
        {
            int logID = -1;
            ExportDataSummaryLog exportLog = new ExportDataSummaryLog();
            exportLog.ExportDataID = xExportDataID;
            exportLog.ExportDataStatusID = (int)ExportDataStatus.STATUS.RUNNING;
            exportLog.StartTime = DateTime.UtcNow;
            logID = exportLog.Save();

            return logID;
        }

        protected static void StopSummaryLog(int xLogID)
        {
            //update logging
            ExportDataSummaryLog exportLog = new ExportDataSummaryLog();
            exportLog.Get(xLogID);
            exportLog.EndTime = DateTime.UtcNow;
            exportLog.Save();
        }


        protected DataTable GetData(ExportData xExportData)
        {
            DataTable spData = new DataTable();

            if (xExportData.SourceDatabase == "EF-MONGO")
            {
                if (xExportData.SQLQuery == "GET_ENQUIRY_DATA")
                {
                    MongoDBUtility mongDB = new MongoDBUtility();
                    int i = mongDB.GetEnquiries();
                    _log.Info($"Records added {xExportData.SQLQuery} : {i}");
                }
            }
            else
            {
                efDataExtporter.Helpers.MSSqlDbUtility sqlu = new Helpers.MSSqlDbUtility(xExportData.SourceDatabase);
                if (xExportData.SQLQuery.Trim().Contains(" "))
                {
                    string[] param = xExportData.SQLQuery.Split(' ');
                    xExportData.SQLQuery = param[0];
                    List<SqlParameter> sqlParam = new List<SqlParameter>()
                                 {
                                     new SqlParameter("Days", SqlDbType.Int) {Value = param[1]},
                                 };
                    spData = sqlu.executeStoredProc(xExportData.SQLQuery, sqlParam);
                }
                else
                {
                    spData = sqlu.executeStoredProc(xExportData.SQLQuery, null);
                }
            }

            return spData;
        }

        /// <summary>
        /// convert data to requested type
        /// </summary>
        /// <param name="xData">data</param>
        /// <param name="xConvertTypeID">type to convert to</param>
        /// <returns>string</returns>
        protected string Convert(DataTable xData, string xConvertType, bool xShowHeaders = false)
        {
            string exportData = String.Empty;

            switch (xConvertType)
            {
                case "CSV":
                    exportData = DataConverter.ConvertToCSV(xData, xShowHeaders);
                    break;
                case "HTML":
                    HtmlConverter htmlConvert = new HtmlConverter();
                    exportData = htmlConvert.Convert(xData);
                    break;

            }
            return exportData;
        }

        /// <summary>
        /// Calculate NextRunTime based on RecurrenceIntervalTypeID and RecurrenceInterval
        /// </summary>
        /// <param name="xRecurrenceIntervalTypeID"></param>
        /// <param name="xRecurrenceInterval"></param>
        /// <returns></returns>
        protected DateTime GetNextRunTime(DateTime? xScheduledLastRun, string xRecurrenceIntervalType, int xRecurrenceInterval)
        {

            DateTime nextRun = DateTime.UtcNow;
            if (xScheduledLastRun.HasValue)
            {
                nextRun = (DateTime)xScheduledLastRun;
            }

            try
            {
                string recurrenceType = xRecurrenceIntervalType;

                _log.Info(string.Format("GetNextRunTime for RecurrenceIntervalTypeID : {0} ({1}) and xRecurrenceInterval {2} Scheduled Last Run :{3}", xRecurrenceIntervalType, recurrenceType, xRecurrenceInterval, xScheduledLastRun));

                do
                {
                    switch (recurrenceType)
                    {
                        case "SECOND":
                            nextRun = nextRun.AddSeconds(xRecurrenceInterval);
                            break;
                        case "MINUTE":
                            nextRun = nextRun.AddMinutes(xRecurrenceInterval);
                            break;
                        case "HOUR":
                            nextRun = nextRun.AddHours(xRecurrenceInterval);
                            break;
                        case "DAY":
                            nextRun = nextRun.AddDays(xRecurrenceInterval);
                            break;
                        case "WEEK":
                            nextRun = nextRun.AddDays(xRecurrenceInterval * 7);
                            break;
                        case "MONTH":
                            nextRun = nextRun.AddMonths(xRecurrenceInterval);
                            break;
                        case "YEAR":
                            nextRun = nextRun.AddYears(xRecurrenceInterval);
                            break;
                    }
                } while (nextRun <= DateTime.UtcNow);

            }
            catch (Exception)
            {
                _log.Error("Next run time could not be calculated, unknown recurrence interval type  " + xRecurrenceIntervalType);
            }
            return nextRun;
        }

        /// <summary>
        /// send data string by transport type
        /// </summary>
        /// <param name="xData">data string</param>
        /// <param name="xTransportTypeID">transport type (e.g. FTP, Email)</param>
        /// <param name="xRecipient_FTP_ID">ftp or email group id</param>
        /// <returns>true if send success, else false</returns>
        protected bool Send(DataTable xData,
                            string xDataType,
                            string xTransportType,
                            string xRecipient,
                            string xEmailSubject,
                            string xEmailBody,
                            string xFTPFilename,
                            bool xShowHeaders = false)
        {
            bool sendSuccess = false;

            string transportType = xTransportType;


            List<DataTable> xDataSets;
            xDataSets = new List<DataTable>();
            xDataSets.Add(xData);

            foreach (DataTable _xData in xDataSets)
            {
                //convert to data type
                string dataToSend = string.Empty;
                //string contentToSend = string.Empty;
                xEmailBody = ReplaceContentPlaceHolders(xEmailBody, _xData);
                xEmailSubject = ReplaceContentPlaceHolders(xEmailSubject, _xData);

                if (xDataType.Contains(","))
                {
                    dataToSend = Convert(_xData, "CSV", xShowHeaders);
                    xEmailBody += Convert(_xData, "HTML", xShowHeaders);
                }
                else
                {
                    dataToSend = Convert(_xData, xDataType, xShowHeaders);
                }

                string ftpHost = string.Empty;

                string dataFileName = "";
                if (dataToSend.Trim().Length > 0)
                {
                    try
                    {
                        dataFileName = ReplaceFilenamePlaceHolders(xFTPFilename);
                        dataFileName += "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                        dataFileName = Path.Combine(ConfigurationManager.AppSettings["WorkingFolder"].ToString(), dataFileName);
                        if (dataToSend.IndexOf("<HTML>", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            dataFileName += ".html";
                        }
                        else
                        {
                            dataFileName += @".csv";
                        }

                        System.IO.File.WriteAllText(dataFileName, dataToSend);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Failed to send, unexpected error ", ex);
                    }
                }


                switch (transportType)
                {
                    case "FTP":
                        #region Get FTP settings


                        #endregion
                        SendByFTPOrLocal(dataToSend, dataFileName);//, ftpCredentials, ftpLocation);
                        break;
                    case "EMAIL":
                        SendByEmail(xRecipient, xEmailSubject, xEmailBody, dataToSend, dataFileName);
                        break;
                }

                sendSuccess = true;
            }

            //}
            //else
            //{
            //    _log.Info("No data to send");
            //    //no data is not a fail
            //    sendSuccess = true;
            //}

            _log.Info("SendData returns: " + sendSuccess);

            return sendSuccess;
        }

        /// <summary>
        /// send data via email
        /// </summary>
        /// <param name="xNotificationGroupID">notification group id</param>
        /// <param name="xEmailSubject">email subject</param>
        /// <param name="xEmailBody">email content</param>
        /// <param name="xDataToSend">data</param>
        protected void SendByEmail(string xRecipient, string xEmailSubject, string xEmailBody, string xDataToSend, string xAttachmentFilename)
        {
            EmailSender sender = new EmailSender();

            sender.Send(xRecipient, xEmailSubject, xEmailBody, xAttachmentFilename);
            _log.Info("Email sent to " + xRecipient + " with subject " + xEmailSubject);

        }

        protected string ReplacePlaceholders(string xContent, string xDynamicData)
        {
            string dynamicContent = xContent.Replace("[spData]", xDynamicData);

            return dynamicContent;
        }

        protected bool CreateExportFile(string xContents, string xFullFilePath)
        {
            bool createStatus = false;

            Encoding utf8WithoutBom = new UTF8Encoding(false);

            try
            {
                //convert string to file
                using (StreamWriter sw = new StreamWriter(File.Open(xFullFilePath, FileMode.Create), utf8WithoutBom))
                {
                    sw.WriteLine(xContents);
                }

                createStatus = true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to create export file " + xFullFilePath, ex);
            }


            return createStatus;
        }

        /// <summary>
        /// send data via FTP
        /// </summary>
        /// <param name="xData"></param>
        /// <param name="xFTPLocationID"></param>
        /// <param name="xFTPFilename">ftp filename</param>
        protected void SendByFTPOrLocal(string xData, string xFTPFilename)//, FTPCredentials xFTPCredentials, FTPLocation xFtpLocation)
        {
            _log.Debug("Attempting to transfer file " + xFTPFilename);

            //get send folder to keep copy of files sent
            string sendFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "send");

            //if not exist, create it
            if (!Directory.Exists(sendFolder))
            {
                Directory.CreateDirectory(sendFolder);
            }

            string fullFilePath = sendFolder + "\\" + xFTPFilename;

            if (CreateExportFile(xData, fullFilePath))
            {
                //if file exists continue
                if (File.Exists(fullFilePath))
                {
                    //if (Utils.IsLocalPath(xFtpLocation.Host))
                    //{
                    //    //string destinationPath = Path.Combine(xFtpLocation.Host, xFTPFilename);
                    //    string destinationPath = Path.Combine(xFTPCredentials.FTPHost, xFTPFilename);
                    //    SendToLocal(destinationPath, fullFilePath);
                    //}
                    //else
                    //{
                    //    SendByFTP(xFTPCredentials, xFtpLocation, fullFilePath);// ftpLocation, fullFilePath);
                    //}

                }
                else
                {
                    Exception ex = new Exception(fullFilePath + " was not found");
                    throw ex;
                }
            }

        }

        protected void SendToLocal(string xDestinationPath, string xSourcePath)
        {
            try
            {
                if (!File.Exists(xSourcePath))
                {
                    _log.Error(xSourcePath + " not found when attempting to copy to " + xDestinationPath);
                }
                else
                {
                    if (File.Exists(xDestinationPath))
                    {
                        string filePrefix = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_";
                        string newFilename = filePrefix + xDestinationPath;
                        _log.Info(xDestinationPath + " already exists, renaming it to " + newFilename);
                        File.Copy(xDestinationPath, newFilename);
                    }

                    File.Copy(xSourcePath, xDestinationPath);
                    _log.Info(String.Format("Manifest successfully copied from {0} to {1}", xSourcePath, xDestinationPath));
                }

            }
            catch (Exception ex)
            {
                _log.Error(String.Format("Error sending {0} to {1}", xSourcePath, xDestinationPath), ex);
            }

        }


     
        protected static string GetFTPHost(DataTable xExportData)
        {
            string ftpHost = String.Empty;
            string ftpHostColumnName = "_FTPHost";

            if (xExportData.Columns.Contains(ftpHostColumnName))
            {
                //take the first row (ftphostname should be same on all rows)
                DataRow exportRowData = xExportData.Rows[0];

                try
                {
                    ftpHost = exportRowData[ftpHostColumnName].ToString();
                }
                catch (Exception)
                {
                    _log.Warn(String.Format("Failed to use retrieve ftphostname field name {0}", exportRowData[ftpHostColumnName].ToString()));
                }
            }
            return ftpHost;
        }

        protected static string ReplaceFilenamePlaceHolders(string xFilename)
        {
            string replacedFilename = xFilename.Replace("[yyyy]", DateTime.UtcNow.ToString("yyyy"));
            replacedFilename = replacedFilename.Replace("[Y]", DateTime.UtcNow.ToString("Y"));
            replacedFilename = replacedFilename.Replace("[y]", DateTime.UtcNow.ToString("y"));
            replacedFilename = replacedFilename.Replace("[M]", DateTime.UtcNow.ToString("M"));
            replacedFilename = replacedFilename.Replace("[MM]", DateTime.UtcNow.ToString("MM"));
            replacedFilename = replacedFilename.Replace("[MMM]", DateTime.UtcNow.ToString("MMM"));
            replacedFilename = replacedFilename.Replace("[d]", DateTime.UtcNow.ToString("d"));
            replacedFilename = replacedFilename.Replace("[dd]", DateTime.UtcNow.ToString("dd"));
            replacedFilename = replacedFilename.Replace("[HH]", DateTime.UtcNow.ToString("HH"));
            replacedFilename = replacedFilename.Replace("[mm]", DateTime.UtcNow.ToString("mm"));
            replacedFilename = replacedFilename.Replace("[ss]", DateTime.UtcNow.ToString("ss"));

            string dynamicFieldPattern = @"\[_(.*?)\]";
            MatchCollection matches = Regex.Matches(replacedFilename, dynamicFieldPattern);

            //if (matches.Count > 0)
            //{
            //    foreach (Match match in matches)
            //    {
            //        string dynamicFieldName = match.Groups[0].Value;
            //        string columnName = dynamicFieldName.Replace("[", "").Replace("]", "");
            //        string fieldValue = GetLastFieldValue(xData, columnName);
            //        replacedFilename = replacedFilename.Replace(dynamicFieldName, fieldValue);
            //    }
            //}
            return replacedFilename;
        }


        protected static string ReplaceContentPlaceHolders(string xContent, DataTable _xData)
        {
            string replacedContent = xContent;

            if (_xData.Rows.Count > 0)
            {
                DataRow dr = _xData.Rows[0];

                replacedContent = replacedContent.Replace("[DATETIME]", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                replacedContent = replacedContent.Replace("[DATE]", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                replacedContent = replacedContent.Replace("[TIME]", DateTime.UtcNow.ToString("HH:mm:ss"));

                foreach (DataColumn c in _xData.Columns)
                {
                    if (c.ColumnName.StartsWith("_"))
                    {
                        replacedContent = replacedContent.Replace("[" + c.ColumnName + "]", dr[c.ColumnName].ToString());
                    }
                }
            }

            return replacedContent;
        }

        protected static string GetLastFieldValue(DataTable xData, string xColumnName)
        {
            string lastFieldValue = String.Empty;

            if ((xData.Rows.Count > 0) && (xData.Columns.Contains(xColumnName)))
            {
                lastFieldValue = xData.Rows[xData.Rows.Count - 1][xColumnName].ToString();
            }
            return lastFieldValue;
        }

        /// <summary>
        /// add entries exported
        /// </summary>
        /// <param name="xExportDataID">eport data it</param>
        /// <param name="xData">data</param>
        protected void AddExportDataLogEntries(int xExportDataID, DataTable xData)
        {
            if (xData != null)
            {
                //ExportDataLog exportLog = new ExportDataLog();
                //foreach (DataRow Row in xData.Rows)
                //{
                //    exportLog.ExportDataID = xExportDataID;
                //    int exportedID = 0;
                //    Int32.TryParse(Row["_ExportedID"].ToString(), out exportedID);
                //    exportLog.ExportedID = exportedID;
                //    exportLog.Timestamp = DateTime.UtcNow;
                //    exportLog.Save();

                //}
            }
        }

    }
}
