using System;

namespace TrackerModel
{/// <summary>
 /// 
 /// These are the state that a package can be in.
 /// The state is used to color the display of the information.
 /// The associated color can be found in TrackingStatusConverter.cs under TrackingViews.
 /// 
 /// </summary>
    public enum TrackingRequestStatus
    {
        Delivered,
        InTransit,
        NoRecord,
        Lost,
        InternalError
    }

    /// <summary>
    /// 
    /// This is the information stored for each package. It is designed
    /// to be independent of the carrier. All historical data is stored either in
    /// an XML file or, alternatively, in a database.
    /// 
    /// </summary>
    public class TrackingInfo
    {
        public Object Id { get; set; }
        public string TrackingId { get; set; }
        public string PostOfficeClass { get; set; }
        public bool Inbound { get; set; }
        public string CityState { get; set; }
        public string DeliveryZip { get; set; }
        public TrackingRequestStatus TrackingStatus { get; set; }
        public string StatusSummary { get; set; }
        public string TrackingHistory { get; set; }
        public DateTime LastEventDateTime { get; set; }
        public DateTime FirstEventDateTime { get; set; }
        public bool TrackingComplete { get; set; }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;

                // Delegate will be null until history loaded to turn off
                // saving of the histories. A tracking history is saved
                // whenever the Description is updated. 
                TrackingInfoChangedNotifier.DescriptionUpdated?.Invoke(this);
            }
        }
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;

                // Collapse any other expanded items.
                if (value)
                    TrackingInfoChangedNotifier.ExpanderUpdated?.Invoke(this);

            }
        }

        public TrackingInfo()
        {
            LastEventDateTime = DateTime.Now; // Defaullt to DateTime.Now in case there are not yet any events.
            FirstEventDateTime = DateTime.Now; // Defaullt to DateTime.Now in case there are not yet any events.
            CityState = "";
            DeliveryZip = "";
            TrackingId = "";
            Description = "";
            TrackingStatus = TrackingRequestStatus.InternalError;
            StatusSummary = "";
            TrackingHistory = "";
            TrackingComplete = false;
            Inbound = false;
        }

        /// <summary>
        /// Clone the TrackingInfo object.
        /// </summary>
        /// <param name="info"></param>
        public TrackingInfo(TrackingInfo info)
        {
            LastEventDateTime = info.LastEventDateTime;
            FirstEventDateTime = info.FirstEventDateTime;
            CityState = info.CityState;
            DeliveryZip = info.DeliveryZip; ;
            TrackingId = info.TrackingId;
            Description = info.Description;
            TrackingStatus = info.TrackingStatus;
            StatusSummary = info.StatusSummary;
            TrackingHistory = info.TrackingHistory;
            TrackingComplete = info.TrackingComplete;
            Inbound = info.Inbound;
        }
    }
}
