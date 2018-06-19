using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
//MongoDB.Driver;
//using MongoDB.Driver.Builders;
//using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using System.Configuration;
using efDataExtporter.DTO;
using System.Data;
using efDataExtporter.Helpers;
using System.Data.SqlClient;
using System.Diagnostics;

namespace efDataExporter.Helpers
{
    public class MongoDBUtility
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string _connectionString = string.Empty;

        public MongoDBUtility()
        {
            _connectionString = GetDBConnectionString();
        }

        public int GetEnquiries()
        {
            int recordsAdded = 0;

            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                MSSqlDbUtility msu = new MSSqlDbUtility("AdminConnection");
                SqlParameter[] sqlParameter = new SqlParameter[0];
                msu.ExecuteQuery("DELETE FROM [Admin].[ElangEnquiries_Staging]", sqlParameter);

                string _connectionString = GetDBConnectionString();// "mongodb://kudu1.vm.xc.io:27017";
                IMongoClient client = new MongoClient(_connectionString);
                
                //string[] _connectionParts = _connectionString.Split(':');
                //MongoClient client = new MongoClient(
                //         new MongoClientSettings
                //         {
                //             Server = new MongoServerAddress(_connectionParts[1], Int32.Parse(_connectionParts[2])),
                //             // Giving 3 seconds for a MongoDB server to be up before we throw
                //             ServerSelectionTimeout = TimeSpan.FromSeconds(600)
                //         });

                IMongoDatabase database = client.GetDatabase("elanguest");
                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("enquiry");

                var filter = Builders<BsonDocument>.Filter.Gt("utcCreated", DateTime.Now.AddDays(-7));
                
                var cursor = collection.Find(filter);
                DataTable dt = new DataTable(cursor.ToString());

                var list = cursor.ToList();

                stopwatch.Stop();
                _log.Info($"Records retrieved :{list.Count} in {stopwatch.ElapsedMilliseconds}ms");

                foreach (BsonDocument doc in list)
                {
                    foreach (BsonElement elm in doc.Elements)
                    {
                        if (!dt.Columns.Contains(elm.Name))
                        {
                            dt.Columns.Add(new DataColumn(elm.Name));
                        }

                    }
                    DataRow dr = dt.NewRow();
                    foreach (BsonElement elm in doc.Elements)
                    {
                        dr[elm.Name] = elm.Value;

                    }
                    dt.Rows.Add(dr);
                }


                msu.SaveTableToDB(dt);

                string sSQL = "INSERT INTO [Admin].[ElangEnquiries] (";
                sSQL += "   [_id]";
                sSQL += " , [utcCreated]";
                sSQL += " , [searchableText] ";
                sSQL += " ,[reference] ";
                sSQL += " ,[firstName] ";
                sSQL += " ,[email] ";
                sSQL += " ,[comments] ";
                sSQL += " ,[trackingData] ";
                sSQL += " ,[utcUpdated]  ";
                sSQL += " ,[nationality] ";
                sSQL += " ,[course1Approx] ";
                sSQL += " ,[course2Approx] ";
                sSQL += " ,[attributes] ";
                sSQL += " ,[lang] ";
                sSQL += " ,[gender] ";
                sSQL += " ,[age] ";
                sSQL += " ,[course1] ";
                sSQL += " ,[accommodation] ";
                sSQL += " ,[accommodationFrom] ";
                sSQL += " ,[accommodationTo] ";
                sSQL += " ,[levelOfEnglish] ";
                sSQL += " ,[course1From] ";
                sSQL += " ,[course1To] ";
                sSQL += " ,[telephone] ";
                sSQL += " ,[travelFrom] ";
                sSQL += " ,[travelTo] ";
                sSQL += " ,[arrivalTransfer] ";
                sSQL += " ,[departureTransfer] ";
                sSQL += " ,[course2] ";
                sSQL += " ,[course2From] ";
                sSQL += " ,[course2To] ";
                sSQL += " ,[ImportDate] ";
                sSQL += " ) ";
                sSQL += " SELECT [_id]";
                sSQL += " , [utcCreated]";
                sSQL += " , [searchableText] ";
                sSQL += " ,[reference] ";
                sSQL += " ,[firstName] ";
                sSQL += " ,[email] ";
                sSQL += " ,[comments] ";
                sSQL += " ,[trackingData] ";
                sSQL += " ,[utcUpdated]  ";
                sSQL += " ,[nationality] ";
                sSQL += " ,[course1Approx] ";
                sSQL += " ,[course2Approx] ";
                sSQL += " ,[attributes] ";
                sSQL += " ,[lang] ";
                sSQL += " ,[gender] ";
                sSQL += " ,[age] ";
                sSQL += " ,[course1] ";
                sSQL += " ,[accommodation] ";
                sSQL += " ,[accommodationFrom] ";
                sSQL += " ,[accommodationTo] ";
                sSQL += " ,[levelOfEnglish] ";
                sSQL += " ,[course1From] ";
                sSQL += " ,[course1To] ";
                sSQL += " ,[telephone] ";
                sSQL += " ,[travelFrom] ";
                sSQL += " ,[travelTo] ";
                sSQL += " ,[arrivalTransfer] ";
                sSQL += " ,[departureTransfer] ";
                sSQL += " ,[course2] ";
                sSQL += " ,[course2From] ";
                sSQL += " ,[course2To] ";
                sSQL += " ,GetUTCDate() ";
                sSQL += " FROM [Admin].[ElangEnquiries_Staging]";
                sSQL += " WHERE [_id] NOT IN (SELECT [_id] FROM [Admin].[ElangEnquiries]) ";

                recordsAdded = msu.ExecuteQuery(sSQL, sqlParameter);
            }
            catch (Exception ex)
            {
                _log.Error("GetEnquiries error :" + ex.ToString());
            }

            return recordsAdded;
        }
        public DataTable executeSelectQuery(ExportData xExportData)
        {
            DataTable dt = new DataTable();

            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("test");

                IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("students");

            

            GetMongoDBData(collection);



            const string connectionString = "mongodb://localhost";

            // Create a MongoClient object by using the connection string
            var zclient = new MongoClient(connectionString);

            // Use the client to access the 'test' database
            IMongoDatabase zdatabase = zclient.GetDatabase("test");

            

            //IList<String> collectionNames = db..GetCollectionNames();
            //var filteredCollections = new List<string>();
            //var hiddenCollectionCriteria = new string[] { "cubicle", "tmp.", ".$", "system.indexes" };

            //foreach (string collectionName in collectionNames)
            //{
            //}


            //var commandResult = collection.RunCommand(aggregationCommand);
            //var response = commandResult.Response;
            //foreach (BsonDocument result in response["results"].AsBsonArray)
            //{
            //    // process result
            //}



            //using (IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(new BsonDocument()))
            //{
            //    while (await cursor.MoveNextAsync())
            //    {
            //        IEnumerable<BsonDocument> batch = cursor.Current;
            //        foreach (BsonDocument document in batch)
            //        {
            //            Console.WriteLine(document);
            //            Console.WriteLine();
            //        }
            //    }
            //}


            return dt;
        }

        private async void GetMongoDBData(IMongoCollection<BsonDocument> collection)
        {
            var queryProvider = new MongoDbCSharpQuery();
            //queryProvider.
            //IList<string> collections = queryProvider.GetCollections("localhost", "test", "27017");
            var documents = await collection.Find(new BsonDocument()).ToListAsync();


            using (IAsyncCursor<BsonDocument> cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<BsonDocument> batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        Console.WriteLine(document);
                        Console.WriteLine();
                    }
                }
            }
        }

        private string GetDBConnectionString()
        {
            string connectionStrings = "NOT_DEFINED_IN_CONFIG";

            if (ConfigurationManager.ConnectionStrings["MongoDBConnectionString"] != null)
            {
                connectionStrings = ConfigurationManager.ConnectionStrings["MongoDBConnectionString"].ToString();
            }

            return connectionStrings;
        }




    }
}
