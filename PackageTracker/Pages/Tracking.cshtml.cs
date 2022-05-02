using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TrackerConfiguration;
using TrackerModel;
using UserTableAccess;
using UspsWebRequests;
using TrackingUserInteraction;

namespace PackageTracker.Pages
{
    public class TrackingModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        private List<TrackingRequest> trackingIds = new List<TrackingRequest>();

        public TrackingModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string dateTime = DateTime.Now.ToShortDateString();
            ViewData["TimeStamp"] = dateTime;
            ViewData["SqlConnection"] = TrackerConfig.SqlConnection;

            GetTracking();
        }

        public void OnPost()
        {
            //Test Tracking Request
            //trackingIds.Add("9449 0112 0081 7822 9007 44");
            //trackingIds.Add("9449011200817822968461");
            //trackingIds.Add("9449011200817822900744");
            //trackingIds.Add("9449011200817829038754");
            //trackingIds.Add("9449011200817829047176");
            //trackingIds.Add("9449011200817801007846");
            //trackingIds.Add("9374889671006176791375");
            //trackingIds.Add("9449011200817801103494");
            //trackingIds.Add("9449011200817801431603");
            //trackingIds.Add("9449011200817801431603");
            //trackingIds.Add("9449011200817801448465");
            //trackingIds.Add("9449011200817801483947");
            //trackingIds.Add("9449011200817801467831");

            GetTracking();
        }

        private void GetTracking()
        {
            //List<string> trackingHistories;

            //HistoricalTrackingAccess trackingTable = new HistoricalTrackingAccess();

            //// Get all tracking from the past selected time interval.
            //string saveFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "USPSTracking.xml");
            //trackingHistories = trackingTable.GetSavedHistories(saveFile);

            //// Add the undelivered packages to get and display them requested tracking
            //foreach (TrackingInfo history in trackingHistories)
            //{
            //    if (!history.TrackingComplete)
            //        trackingIds.Add(history.TrackingId);
            //}

            //// If there are any ids to track, get the tracking histories from USPS and parse them.
            //if (trackingIds.Count > 0)
            //{
            //    string response = TrackerWebAPICall.GetTrackingFieldInfo(trackingIds);
            //    trackingHistories = TrackingResponseParser.ParseTrackingXML(response, "");

            //    foreach (TrackingInfo history in trackingHistories)
            //    {
            //        history.Note = history.TrackingId;
            //    }

            //    // Save the histories to the DB
            //    trackingTable.SaveHistories(trackingHistories);
            //}

        }
    }
}