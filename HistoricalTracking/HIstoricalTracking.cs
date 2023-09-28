using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerModel;
using Windows.System;

namespace HistoricalTracking
{
    public abstract class HistoricalTrackingAccess
    {
        protected HistoricalTrackingAccess()
        {
        }

        /// <summary>
        ///      Virtual method to save the history to storage.
        /// </summary>
        /// <param name="history">
        ///      The history to save.
        /// </param>
        public virtual void SaveHistory(TrackingInfo history) => throw new NotImplementedException();

        /// <summary>
        ///      Virtual method to delete the history from storage.
        /// </summary>
        /// <param name="history">
        ///      The history to delete.
        /// </param>
        public virtual void DeleteHistory(string trackingId) => throw new NotImplementedException();

        /// <summary>
        ///      Virtual method to etrieve prior histories from storage.
        /// </summary>
        /// <returns>
        ///      A List of TrackingInfo that is the list of saved histories in storage.
        /// </returns>
        public virtual List<TrackingInfo> GetSavedHistories() => throw new NotImplementedException();

        public static HistoricalTrackingAccess? GetTrackingDB(string dbName)
        {
            HistoricalTrackingAccess? db = null;

            if (PtDbConnection.ConnectionString.Contains("mongodb"))
            {
                db = new HistoricalTrackingAccessMongoDB(dbName);
            }
            return db;
        }
    }
}
