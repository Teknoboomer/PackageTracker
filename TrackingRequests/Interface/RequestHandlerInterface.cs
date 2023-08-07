using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerModel;

namespace TrackingRequests.Interface
{
    internal interface RequestHandlerInterface
    {
        TrackingInfo HandleTrackingRequest(string trackingRequest, string userZip);
    }
}
