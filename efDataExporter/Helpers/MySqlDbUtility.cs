using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace wnFTPFileTransfer
{
    public static class MySqlDbUtility
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private static MySqlDataAdapter myAdapter;

        public static int RecordExists(String _query, MySqlParameter[] sqlParameter)
        {
            int result = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            if (string.IsNullOrEmpty(connectionString) == false)
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand myCommand = new MySqlCommand(_query, connection))
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
        public static DataTable executeSelectQuery(String _query, MySqlParameter[] sqlParameter)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            DataTable dataTable = new DataTable();

            if (string.IsNullOrEmpty(connectionString) == false)
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand myCommand = new MySqlCommand())
                    {
                        try
                        {
                            dataTable = null;
                            DataSet ds = new DataSet();
                            MySqlDataAdapter myAdapter = new MySqlDataAdapter();
                            myCommand.Connection = connection;
                            myCommand.Connection.Open();
                            myCommand.CommandText = _query;
                            myCommand.Parameters.AddRange(sqlParameter);
                            myAdapter.SelectCommand = myCommand;
                            myCommand.ExecuteNonQuery();
                            myAdapter.Fill(ds);
                            dataTable = ds.Tables[0];
                        }
                        catch (MySqlException ex)
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
        ///  public static int executeUpdateQuery(String _query, MySqlParameter[] sqlParameter)
        /// executeUpdateQuery function is used for execute the Update query.
        /// </summary>
        /// <param name="_query">This is used for SQL Query</param>
        /// <param name="sqlParameter">SQLParameter arrays</param>
        /// <returns>int</returns>
        public static int executeUpdateQuery(String _query, MySqlParameter[] sqlParameter)
        {
            int iResult = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;         
            
            try
            {
                if (string.IsNullOrEmpty(connectionString) == false)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        using (MySqlCommand myCommand = new MySqlCommand())
                        {
                            try
                            {
                                MySqlDataAdapter myAdapter = new MySqlDataAdapter();
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
            catch (MySqlException ex)
            {
                _log.Error(ex);                
            }            
            return iResult;
        }


    }
}
