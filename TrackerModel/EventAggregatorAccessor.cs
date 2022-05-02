using System;

namespace TrackerModel
{
    // This static class provides a Delegate to have the TrackerInfo class inform the VM of a Note change.
    // The Delegate is created and executed in the VM. The TrackerInfo class
    // calls the Delegate whenever the Note is changed by the View in the ListView.
    static public class TrackingInfoDescriptionChangedNotifier
    {
        static public Action<TrackingInfo> DescriptionUpdated;
    }
}