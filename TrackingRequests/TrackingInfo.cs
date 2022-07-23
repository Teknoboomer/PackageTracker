using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TrackerConfiguration;
using TrackerModel;
using ExternalTrackingequests;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;


namespace HistoricalTracking
{
    public class HistoricalTrackingAccess
    {
        private MongoClient _dbClient;
        private IMongoDatabase _database;
        private IMongoCollection<TrackingInfo> _packagesCollection;

        public HistoricalTrackingAccess()
        {
            try
            {
                _dbClient = new MongoClient("mongodb://localhost:27017?socketTimeoutMS=19000");
                _database = _dbClient.GetDatabase("PackageTracker");
                _packagesCollection = _database.GetCollection<TrackingInfo>("TrackingInfo");
            }
            catch (Exception e)
            {
                string foo = e.Message;
            }
        }

        // Use testMongoDB to create/access the test database.
        // This is included for the automated tests.
        public HistoricalTrackingAccess(string testMongoDB)
        {
            try
            {
                _dbClient = new MongoClient("mongodb://localhost:27017?socketTimeoutMS=19000");
                _database = _dbClient.GetDatabase(testMongoDB);
                _packagesCollection = _database.GetCollection<TrackingInfo>("TrackingInfo");
            }
            catch (Exception e)
            {
                string foo = e.Message;
            }
        }

        /// <summary>
        ///      Save the history to storage.
        /// </summary>
        /// <param name="history">
        ///      The history to save.
        /// </param>
        public void SaveHistory(TrackingInfo history)
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
        public void DeleteHistory(string trackingId)
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
        public List<TrackingInfo> GetSavedHistories()
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
