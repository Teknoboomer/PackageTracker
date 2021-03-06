using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TrackerConfiguration;
using TrackerModel;
using ExternalTrackingequests;
using System.Globalization;
//using MongoDB.Bson;
//using MongoDB.Driver;


namespace HistoricalTracking
{
    public class HistoricalTrackingAccess
    {
        public bool HadInternalError { get; private set; }
        public string InternalErrorDescription { get; private set; }
        private string _filePath;
        //private MongoClient _dbClient;
        //private IMongoDatabase _database;
        //private IMongoCollection<TrackingInfo> _packagesCollection;

        // If no file path is given to the constructor, use the config file.
        public HistoricalTrackingAccess()

        {
            _filePath = TrackerConfig.HistoryFilePath;
            //try
            //{
            //    _dbClient = new MongoClient("mongodb://localhost:27017?socketTimeoutMS=19000");
            //    _database = _dbClient.GetDatabase("PackageTracker");
            //    _packagesCollection = _database.GetCollection<TrackingInfo>("TrackingInfo");
            //}
            //catch (Exception e)
            //{
            //    string foo = e.Message;
            //}
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
            // Create the XMLDocument along with the root element.
            XDocument historicalTracking = new XDocument();
            XElement root = new XElement("TrackingHistories");
            historicalTracking.Add(root);

            // Loop through the histories, creating a <TrackingInfo> node for each.
            foreach (TrackingInfo history in trackingHistories)
            {

                XElement info = new XElement("TrackingInfo",
                    new XElement("Description", history.Description),
                    new XElement("Delivered", history.TrackingComplete),
                    new XElement("TrackingStatus", history.TrackingStatus),
                    new XElement("CityState", history.CityState),
                    new XElement("DeliveryZip", history.DeliveryZip),
                    new XElement("Inbound", history.Inbound),
                    new XElement("Summary", history.StatusSummary),
                    new XElement("TrackHistory", history.TrackingHistory),
                    new XElement("LastEventDateTime", history.LastEventDateTime),
                    new XElement("FirstEventDateTime", history.FirstEventDateTime));

                info.Add(new XAttribute("Id", history.TrackingId));
                root.Add(info);

                //try
                //{
                //    ReplaceOneResult result = _packagesCollection.ReplaceOne(
                //                    filter: new BsonDocument("TrackingId", history.TrackingId),
                //                    options: new ReplaceOptions { IsUpsert = true },
                //                    replacement: history);
                //}
                //catch (Exception e)
                //{
                //    string foo = e.Message;
                //    // https://drive.google.com/file/d/1ZYGuyUqV0bZJMSPr6Q8uHCQIKk563BQ-/view?usp=sharing
                //    // https://drive.google.com/file/d/1S41rPRgH8NvBUWL9kRvgo2QCNMMtavDR/view?usp=sharing
                //}
            }

            // Save off the XML nodes to a file.
            historicalTracking.Save(_filePath);

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
            bool hadUpdates = false;
            HadInternalError = false;
            
            //try
            //{
            //    FilterDefinition<TrackingInfo> filter = Builders<TrackingInfo>.Filter.Ne(x => x.TrackingId, "");
            //    List<TrackingInfo> packages = _packagesCollection.Find(filter).ToList();
            //}
            //catch (Exception e)
            //{
            //        string foo = e.Message;
            //}

            List<TrackingInfo> trackingHistories = new List<TrackingInfo>();

            // Read in all histories and uppdate the tracking for those not yet delivered.
            XDocument doc = XDocument.Load(_filePath);
            XElement trackInfo = doc.Element("TrackingHistories");
            XElement[] trackingInfos = trackInfo.Elements("TrackingInfo").ToArray();

            // Loop through all of the histories pick out the TrackingInfo data from each history
            // and add the TrackInfo into the list of recovered hisstories.
            // Update the tracking for those not yet delivered before adding them into the list.
            foreach (XElement historyNode in trackingInfos)
            {
                TrackingInfo history = new TrackingInfo();

                history.TrackingId = historyNode.Attribute("Id").Value;
                history.TrackingComplete = Convert.ToBoolean(historyNode.Element("Delivered").Value);
                history.Description = historyNode.Element("Description").Value;
                history.FirstEventDateTime = DateTime.Parse(historyNode.Element("FirstEventDateTime").Value);
                history.LastEventDateTime = DateTime.Parse(historyNode.Element("LastEventDateTime").Value);
                history.TrackingStatus = (TrackingRequestStatus)Enum.Parse(typeof(TrackingRequestStatus), historyNode.Element("TrackingStatus").Value);

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
                            hadUpdates = true;
                            update.Description = history.Description; // Restore the Description.
                            history = update;  // If there was an update, update the history to the update.
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
                        hadUpdates = true; // Save the update.
                    }

                    history.StatusSummary = historyNode.Element("Summary").Value;
                    history.Inbound = Convert.ToBoolean(historyNode.Element("Inbound").Value);
                    history.CityState = historyNode.Element("CityState").Value;
                    history.DeliveryZip = historyNode.Element("DeliveryZip").Value;
                    history.TrackingHistory = historyNode.Element("TrackHistory").Value;
                    history.LastEventDateTime = Convert.ToDateTime(historyNode.Element("LastEventDateTime").Value);
                }

                trackingHistories.Add(history);
            }

            // Update the saved past tracking histories if needed.
            if (hadUpdates)
            {
                SaveHistories(trackingHistories);
            }

            return trackingHistories;
        }
    }
}
