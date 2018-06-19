using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace efDataExtporter.Helpers
{
    public static class DataLayer
    {

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

       
        public static DataTable GetFTPWatchLocations()
        {
            DataTable dataTable = new DataTable();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            if (connectionString.Contains("UID"))
            {
                dataTable = GetFTPWatchLocationsFromMySQL();
            }
            else if (connectionString.Contains("User ID"))
            {
                dataTable = GetFTPWatchLocationsFromMS_SQL();
            }

            return dataTable;
        }

        private static DataTable GetFTPWatchLocationsFromMySQL()
        {
            DataTable FTPWatchList = new DataTable();

            try
            {
                string sqlQuery = " SELECT `ftp-access`.`ftp-access-id` as `FTPLocationID` ";
                sqlQuery += " FROM `ftp-access` ";
                sqlQuery += " WHERE `engine_id` = 1 ";

                DataTable dataTable = new DataTable();
                //MySqlParameter[] Parms = new MySqlParameter[0];
                //dataTable = MySqlDbUtility.executeSelectQuery(sqlQuery, Parms);
                //if (dataTable != null)
                //{
                //    FTPWatchList = dataTable;
                //}
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return FTPWatchList;
        }

        private static DataTable GetFTPWatchLocationsFromMS_SQL()
        {
            DataTable FTPWatchList = new DataTable();

            //string engineID = GetEngineID();

            try
            {

                //var spParams = new List<SPParameter> {
                //new SPParameter("EngineID", engineID),
                //    };

                //MSSqlDbUtility msSqlDbUtility = new MSSqlDbUtility();

                //FTPWatchList = msSqlDbUtility.executeStoredProc("[Utility].[GetFTPLocationWatchList]", spParams);

            }
            catch (Exception ex)
            {
                _log.Error("GetFTPWatchLocationsFromMS_SQL error", ex);
            }

            return FTPWatchList;
        }


        private static bool IsLocalPath(string xFTPHostOrLocalPath)
        {
            bool isLocal = false;
            const string UNC_PATH = @"\\";
            const string LOCAL_DRIVE = @":";

            if (!String.IsNullOrWhiteSpace(xFTPHostOrLocalPath))
            {
                if (xFTPHostOrLocalPath.StartsWith(UNC_PATH))
                {
                    isLocal = true;
                }
                else if (xFTPHostOrLocalPath.Substring(1, 1) == LOCAL_DRIVE)
                {
                    isLocal = true;
                }
            }
            return isLocal;
        }

    }

}
