using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using TrackerModel;


namespace HistoricalTracking
{
    public static class HistoricalTrackingAccessMongoDB
    {
        private static MongoClient _dbClient;
        private static IMongoDatabase _database;
        private static IMongoCollection<TrackingInfo> _packagesCollection;

        public static void InitializeDB(string dbName)
        {
            try
            {
                _dbClient = new MongoClient("mongodb://localhost:27017?socketTimeoutMS=19000");
                _database = _dbClient.GetDatabase(dbName);
                _packagesCollection = _database.GetCollection<TrackingInfo>("TrackingInfo");
            }
            catch (Exception e)
            {
                throw new Exception("InitializeDB: " + e.Message);
            }
        }

        /// <summary>
        ///      Save the history to storage.
        /// </summary>
        /// <param name="history">
        ///      The history to save.
        /// </param>
        public static void SaveHistory(TrackingInfo history)
        {
            // Loop through the histories, creating a <TrackingInfo> node for each.
            try
            {
                if (history.Id == null)
                    history.Id = Guid.NewGuid();
                ReplaceOneResult result = _packagesCollection.ReplaceOne(
                                filter: new BsonDocument("TrackingId", history.TrackingId),
                                options: new ReplaceOptions { IsUpsert = true },
                                replacement: history);
            }
            catch (Exception e)
            {
                throw new Exception("GetSavedHistories: " + e.Message);
            }

            return;
        }

        /// <summary>
        ///      Delete the history from storage.
        /// </summary>
        /// <param name="history">
        ///      The history to delete.
        /// </param>
        public static void DeleteHistory(string trackingId)
        {
            // Loop through the histories, creating a <TrackingInfo> node for each.
            try
            {
                FilterDefinition<TrackingInfo> deleteFilter = Builders<TrackingInfo>.Filter.Eq("TrackingId", trackingId);
                DeleteResult result = _packagesCollection.DeleteOne(deleteFilter);
            }
            catch (Exception e)
            {
                throw new Exception("GetSavedHistories: " + e.Message);
            }

            return;
        }

        /// <summary>
        ///      Retrieves prior histories from storage.
        /// </summary>
        /// <returns>
        ///      A List of TrackingInfo that is the list of saved histories in storage.
        /// </returns>
        public static List<TrackingInfo> GetSavedHistories()
        {
            List<TrackingInfo> trackingHistories = new List<TrackingInfo>();
            try
            {
                FilterDefinition<TrackingInfo> filter = Builders<TrackingInfo>.Filter.Ne(x => x.TrackingId, "");
                trackingHistories = _packagesCollection.Find(filter).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("GetSavedHistories: " + e.Message);
            }

            return trackingHistories;
        }
    }
}
