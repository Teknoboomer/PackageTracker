using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerUtility
{
    public class TrackResponseItem
    {
        public TrackResponseItem(string trackInfoId, string trackSummary, bool delivered, string trackXml, Stack<string> trackingHistory)
        {
            TrackInfoId = trackInfoId;
            TrackSummary = trackSummary;
            TrackXml = trackXml;
            TrackDelivered = delivered;
            TrackingHistory = trackingHistory;
        }
        public string TrackInfoId { get; private set; }
        public string TrackSummary { get; private set; }
        public bool TrackDelivered { get; private set; }
        public string TrackXml { get; private set; }
        public Stack<string> TrackingHistory { get; private set; }

    }
}
