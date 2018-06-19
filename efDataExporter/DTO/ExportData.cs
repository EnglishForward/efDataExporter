using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace efDataExtporter.DTO
{
    public class ExportData
    {
        public int ExportDataID { get; set; }
        public string Description { get; set; }
        public int RunStatusID { get; set; }

        public string TransportType { get; set; }

        public string FTPFilename { get; set; }
        public string ExportDataType { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string Recipient { get; set; }
        public string SQLQuery { get; set; }
        public bool ShowHeaders { get; set; }
        //public bool SplitData { get; set; }
        public bool SendWhenNoData { get; set; }

        //public int Recipient_FTP_ID { get; set; }
        public DateTime NextRun { get; set; }
        //public int PostExportDataSPID { get; set; }
        public DateTime LastRunStartTime { get; set; }
        public DateTime LastRunEndTime { get; set; }
        public string RecurrenceIntervalType { get; set; }
        public int RecurrenceInterval { get; set; }
        public string SourceDatabase { get; set; }

        public List<ExportData> GetAllScheduledToRun()
        {
            List<ExportData> exportDataEntries = new List<ExportData>();

            efDataExtporter.Helpers.MSSqlDbUtility sqlu = new Helpers.MSSqlDbUtility("AdminConnection");
            DataTable dt = sqlu.executeStoredProc("Admin.GetAllExportDataToRun", null);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow Row in dt.Rows)
                {
                    try
                    {
                        ExportData exportData = new ExportData();
                        exportData.ExportDataID = Convert.ToInt32(Row["ExportDataID"].ToString());

                        exportData.Description = Row["Description"].ToString();
                        exportData.EmailSubject = Row["EmailSubject"].ToString();
                        exportData.EmailBody = Row["EmailBody"].ToString();
                        exportData.Recipient = Row["EmailRecipients"].ToString();

                        exportData.ExportDataType = Row["ExportDataType"].ToString();
                        exportData.SQLQuery = Row["SQLQuery"].ToString();

                        exportData.TransportType = Row["TransportType"].ToString();
                        exportData.RecurrenceIntervalType = Row["RecurrenceIntervalType"].ToString();
                        exportData.RecurrenceInterval = Convert.ToInt32(Row["RecurrenceInterval"].ToString());
                        exportData.SendWhenNoData = Convert.ToBoolean(Row["SendWhenNoData"].ToString());
                        exportData.ShowHeaders = Convert.ToBoolean(Row["ShowHeaders"].ToString() != null ? Row["ShowHeaders"].ToString() : "0");
                        exportData.SourceDatabase = Row["SourceDatabase"].ToString();

                        if (Row["LastRunStartTime"].ToString() != "")
                        {
                            exportData.LastRunStartTime = DateTime.Parse(Row["LastRunStartTime"].ToString());
                        }

                        if (Row["LastRunEndTime"].ToString() != "")
                        {
                            exportData.LastRunEndTime = DateTime.Parse(Row["LastRunEndTime"].ToString());
                        }

                        if (Row["NextRunTime"].ToString() != "")
                        {
                            exportData.NextRun = DateTime.Parse(Row["NextRunTime"].ToString());
                        }

                        exportData.LastRunStartTime = System.DateTime.UtcNow;
                        exportData.LastRunEndTime = System.DateTime.UtcNow;
                        exportDataEntries.Add(exportData);
                    }
                    catch (Exception ex)
                    {
                        //_log.Error("Failed to add data export entry to list calling GetAllExportDataByCategory", ex);
                    }

                }
            }
            return exportDataEntries;
        }

        public void Save()
        {
            efDataExtporter.Helpers.MSSqlDbUtility sqlu = new Helpers.MSSqlDbUtility("AdminConnection");

            List <SqlParameter> sqlParam = new List<SqlParameter>()
                 {
                     new SqlParameter("@ExportDataID", SqlDbType.Int) {Value = this.ExportDataID},
                     new SqlParameter("@RunStatusID", SqlDbType.NVarChar) {Value = this.RunStatusID},
                     new SqlParameter("@NextRun", SqlDbType.DateTime) {Value = this.NextRun},
                     new SqlParameter("@LastRunStartTime", SqlDbType.DateTime) {Value = this.LastRunStartTime},
                     new SqlParameter("@LastRunEndTime", SqlDbType.DateTime) {Value = this.LastRunEndTime}

                 };

            DataTable dt = sqlu.executeStoredProc("Admin.UpdateExportDataStatus", sqlParam);
        }


    }
}
