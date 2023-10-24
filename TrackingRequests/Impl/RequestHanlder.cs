using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerModel;
using TrackingRequests.Interface;

namespace TrackingRequests.Impl
{
    public class RequestHanlder : RequestHandlerInterface
    {
        private string _requestId;
        private RequestHandlerInterface _handler;

        // Request handler will either be USPS or UPS.
        public RequestHanlder(string trackingId)
        {
            _requestId = trackingId;
            if (!trackingId.StartsWith("1Z"))
            {
                _handler = new USPSTrackerWebAPICall();
            }
        }

        public TrackingInfo HandleTrackingRequest(string trackingRequest, string userZip)
        {
            TrackingInfo trackingInfo = _handler.HandleTrackingRequest(trackingRequest, userZip);
            return trackingInfo;
        }
    }
}
