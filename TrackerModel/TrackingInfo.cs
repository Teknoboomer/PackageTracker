using System;

namespace TrackerModel
{
    public enum TrackingRequestStatus
    {
        Delivered,
        InTransit,
        NoRecord,
        Lost,
        InternalError
    }
    public class TrackingInfo
    {
        public string TrackingId { get; set; }
        public string PostOfficeClass { get; set; }
        public bool Inbound { get; set; }
        public string CityState { get; set; }
        public string DeliveryZip { get; set; }
        public TrackingRequestStatus TrackingStatus { get; set; }
        public string StatusSummary { get; set; }
        public string TrackingHistory { get; set; }
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
                TrackingInfoDescriptionChangedNotifier.DescriptionUpdated?.Invoke(this);
            }
        }
        public DateTime LastEventDateTime { get; set; }
        public DateTime FirstEventDateTime { get; set; }
        public bool TrackingComplete { get; set; }

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
    }
}
