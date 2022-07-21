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
        public bool HadInternalError { get; private set; }
        public string InternalErrorDescription { get; private set; }
        private string _filePath;
        private MongoClient _dbClient;
        private IMongoDatabase _database;
        private IMongoCollection<TrackingInfo> _packagesCollection;

        // If no file path is given to the constructor, use the config file.
        public HistoricalTrackingAccess()

        {
            _filePath = TrackerConfig.HistoryFilePath;
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

        // Use the path given in the constructor for the path to save
        // and restore the file. This is included for the automated tests.
        public HistoricalTrackingAccess(string path)

        {
            _filePath = path;
        }

        /// <summary>
        /// Retrieve the prior histories from storage.
        /// </summary>
        /// <param name="trackingHistories">
        /// The list of histories to save.
        /// </param>
        public void SaveHistories(List<TrackingInfo> trackingHistories)
        {

            // Loop through the histories, creating a <TrackingInfo> node for each.
            foreach (TrackingInfo history in trackingHistories)
            {
                try
                {
                    ReplaceOneResult result = _packagesCollection.ReplaceOne(
                                    filter: new BsonDocument("TrackingId", history.TrackingId),
                                    options: new ReplaceOptions { IsUpsert = true },
                                    replacement: history);
                }
                catch (Exception e)
                {
                    string foo = e.Message;
                }
            }

            return;
        }

        /// <summary>
        /// Save the history to storage.
        /// </summary>
        /// <param name="history">
        /// The history to save.
        /// </param>
        public void SaveHistory(TrackingInfo history)
        {
            // Loop through the histories, creating a <TrackingInfo> node for each. 9505506628562200335967
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
                string foo = e.Message;
            }

            return;
        }

        /// <summary>
        /// Delete the history from storage.
        /// </summary>
        /// <param name="history">
        /// The history to delete.
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
                string foo = e.Message;
            }

            return;
        }

        /// <summary>
        /// Retrieves prior histories from storage.
        /// </summary>
        /// <returns>
        /// A List of TrackingInfo that is the list of saved histories in storage.
        /// </returns>
        public List<TrackingInfo> GetSavedHistories()
        {
            HadInternalError = false;

            List<TrackingInfo> trackingHistories = new List<TrackingInfo>();
            try
            {
                FilterDefinition<TrackingInfo> filter = Builders<TrackingInfo>.Filter.Ne(x => x.TrackingId, "");
                trackingHistories = _packagesCollection.Find(filter).ToList();
                trackingHistories.Sort((x, y) => -x.FirstEventDateTime.CompareTo(y.FirstEventDateTime));
            }
            catch (Exception e)
            {
                string foo = e.Message;
            }

            // Loop through all of the histories pick out the TrackingInfo data from each history
            // and add the TrackInfo into the list of recovered hisstories.
            // Update the tracking for those not yet delivered before adding them into the list.
            for (int i = 0; i < trackingHistories.Count; i++)
            {
                TrackingInfo history = trackingHistories[i];

                // Do not update outdated undelivered tracking requests. USPS IDs for valid for only 120 days.
                if (!history.TrackingComplete && history.FirstEventDateTime >= DateTime.Now.AddDays(-120))
                {
                    string response = USPSTrackerWebAPICall.GetTrackingFieldInfo(history.TrackingId);
                    if (response.StartsWith("Error"))
                    {
                        HadInternalError = true;
                        InternalErrorDescription = response;
                    }
                    else
                    {
                        TrackingInfo update = USPSTrackingResponseParser.USPSParseTrackingXml(response, "", history.Description);
                        if (update.TrackingStatus == TrackingRequestStatus.InternalError)
                        {
                            HadInternalError = true;
                            InternalErrorDescription = update.StatusSummary;
                        }
                        else
                        {
                            update.Description = history.Description; // Restore the Description.
                            update.Id = history.Id; // Restore the Description.
                            trackingHistories[i] = update;  // Update the history.
                            SaveHistory(trackingHistories[i]);
                        }
                    }
                }
                else
                {
                    // If it was not delivered and the ID has expired, set the status to Lost and tracking completed.
                    if (!history.TrackingComplete && history.FirstEventDateTime < DateTime.Now.AddDays(-120))
                    {
                        history.TrackingComplete = true;
                        history.TrackingStatus = TrackingRequestStatus.Lost;
                        trackingHistories[i] = history;  // Update the history.
                        SaveHistory(trackingHistories[i]);
                    }
                }
            }

            return trackingHistories;
        }
    }
}
