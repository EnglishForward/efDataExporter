using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace efDataExtporter.Helpers
{
    public class MSSqlDbUtility
    {
        //SqlConnection _db;
        string _connectionString = string.Empty;

        public MSSqlDbUtility(string sConnection)
        {
            _connectionString = GetDBConnectionString(sConnection);
            //_db = new SqlConnection(connectionString);
        }

        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int ExecuteQuery(String _query, SqlParameter[] sqlParameter)
        {
            int result = 0;
            //string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            if (string.IsNullOrEmpty(_connectionString) == false)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand myCommand = new SqlCommand(_query, connection))
                    {
                        try
                        {
                            connection.Open();
                            myCommand.Parameters.AddRange(sqlParameter);
                            result = Convert.ToInt32(myCommand.ExecuteScalar());
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex);
                        }
                        finally
                        {
                            myCommand.Dispose();
                            connection.Close();
                        }
                    }
                }
            }

            return result;
        }

        /// </summary>
        /// <param name="_query"> This is used for My Sql Query</param>
        /// <param name="sqlParameter"> SQLParameter arrays</param>
        /// <returns>DataTable</returns>
        public DataTable executeSelectQuery(String _query, SqlParameter[] sqlParameter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            DataTable dataTable = new DataTable();

            if (string.IsNullOrEmpty(connectionString) == false)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand myCommand = new SqlCommand())
                    {
                        try
                        {
                            dataTable = null;
                            DataSet ds = new DataSet();
                            SqlDataAdapter myAdapter = new SqlDataAdapter();
                            myCommand.Connection = connection;
                            myCommand.Connection.Open();
                            myCommand.CommandText = _query;
                            myCommand.Parameters.AddRange(sqlParameter);
                            myAdapter.SelectCommand = myCommand;
                            myCommand.ExecuteNonQuery();
                            myAdapter.Fill(ds);
                            dataTable = ds.Tables[0];
                        }
                        catch (SqlException ex)
                        {
                            _log.Error(ex);
                            return null;
                        }
                        finally
                        {
                            myCommand.Dispose();
                            myCommand.Connection.Close();
                        }
                    }
                }
            }
            return dataTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_query"></param>
        /// <param name="sqlParameter"></param>
        /// <returns></returns>
        public int executeUpdateQuery(String _query, SqlParameter[] sqlParameter)
        {
            int iResult = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                if (string.IsNullOrEmpty(connectionString) == false)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand myCommand = new SqlCommand())
                        {
                            try
                            {
                                SqlDataAdapter myAdapter = new SqlDataAdapter();
                                DataSet ds = new DataSet();
                                connection.Open();
                                myCommand.Connection = connection;
                                myCommand.CommandText = _query;
                                myCommand.Parameters.AddRange(sqlParameter);
                                myAdapter.InsertCommand = myCommand;
                                iResult = myCommand.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex);
                            }
                            finally
                            {
                                myCommand.Connection.Close();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _log.Error(ex);
            }
            return iResult;
        }

        public DataTable executeStoredProc(string xSPName, List<SqlParameter> sqlParameter)
        {
            
            DataTable dataTable = new DataTable();
            DataSet ds = new DataSet();

            //string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;


            if (string.IsNullOrEmpty(_connectionString) == false)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(xSPName, connection))
                    {
                        if (sqlParameter != null)
                        cmd.Parameters.AddRange(sqlParameter.ToArray());

                        SqlDataReader reader;

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        connection.Open();

                        reader = cmd.ExecuteReader();
                        
                        dataTable.Load(reader);


                    }
                }
            }
                    
            return dataTable;
        }

        private string GetDBConnectionString(string sConnection)
        {
            string connectionStrings = "NOT_DEFINED_IN_CONFIG";

            if (ConfigurationManager.ConnectionStrings[sConnection] != null)
            {
                connectionStrings = ConfigurationManager.ConnectionStrings[sConnection].ToString();
            }

            return connectionStrings;
        }

        public int SaveTableToDB(DataTable dt)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString) == false)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        using (var bulkCopy = new SqlBulkCopy(connection.ConnectionString, SqlBulkCopyOptions.KeepIdentity))
                        {
                            // my DataTable column names match my SQL Column names, 
                            //so I simply made this loop. However if your column names don't match, just pass in which datatable name matches the SQL column name in Column Mappings
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                            }

                            bulkCopy.BulkCopyTimeout = 600;
                            bulkCopy.DestinationTableName = "Admin.ElangEnquiries_Staging";
                            bulkCopy.WriteToServer(dt);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _log.Error("SaveTableToDB :" + ex.ToString());
            }
            return 1;
        }
    }
}
