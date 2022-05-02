using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TrackerConfiguration;
using TrackerModel;
using TrackingUserInteraction;
using UserTableAccess;
using UspsWebRequests;

namespace DAL_Runner
{
    //    For Company:  DEVELOPER
    //Your Username is 818DEVEL4717
    //Your Password is 168BH52TF614

    class Program
    {
        static void Main(string[] args)
        {
            bool success;
            DateTime lastLogin;
            List<TrackingInfo> trackingHistories;
            List<string> trackingIds = new List<string>();
                                       //     < LinearGradientBrush EndPoint = "0.5,1" StartPoint = "0.5,0" >
   
                                       //< GradientStop Color = "#FF818187" Offset = "0" />
      
                                       //   < GradientStop Color = "LightGray" Offset = "0.957" />
         
                                       //  </ LinearGradientBrush >


                     User user = new User { UserName = "Runeweaver", Password = "My Password", Zipcode = "77459" };

            // Check to see if user exists and create it if not.
            UserInfo userInfo = new UserInfo { User = user };
            success = userInfo.CheckUser(out lastLogin) || userInfo.CreateUser();

            HistoricalTrackingAccess _historicalTracking = new HistoricalTrackingAccess();

            // Get all packages in the last time period, i.e. week, monthe, quarter, year.
        string saveFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "USPSTracking.xml");
        trackingHistories = _historicalTracking.GetSavedHistories(user, saveFile);

            // Add the undelivered packages to the requested tracking
             
            foreach (TrackingInfo history in trackingHistories)
            {
                if (!history.TrackingComplete)
                    trackingIds.Add(history.TrackingId);
            }

            //Test Tracking Request
            //trackingIds.Add("9449011200817801686133");
            //trackingIds.Add("9449011200817822968461");
            //trackingIds.Add("9449011200817822900744");
            //trackingIds.Add("9449011200817829038754");
            //trackingIds.Add("9449011200817829047176");
            //trackingIds.Add("9449011200817801007846");
            //trackingIds.Add("9374889671006176791375");
        trackingIds.Add("9449011200817801103494");
        trackingIds.Add("9449011200817801431603");
        trackingIds.Add("9449011200817801431603");
        trackingIds.Add("9449011200817801448465");
        trackingIds.Add("9449011200817801483947");
        trackingIds.Add("9449011200817801467831");
        trackingIds.Add("9505506628561317285400");
        trackingIds.Add("9505510467561333610857");
        trackingIds.Add("9505506628561336289502");


            // If there are any ids to track, get the tracking histories from USPS.
            if (trackingIds.Count > 0)
            {
                string response = TrackerWebAPICall.GetTrackingFieldInfo(trackingIds);

                 trackingHistories = TrackingResponseParser.ParseTrackingXML(response, user.Zipcode);

                foreach (TrackingInfo history in trackingHistories)
                {
                    history.Note = "";
                    history.Note = "Pam's package";
                    Console.WriteLine("Tracking ID: " + history.TrackingId);
                    Console.WriteLine("  Summary: " + history.StatusSummary + "\n");

                    Console.WriteLine("\tEvent: " + history.TrackingHistory);
                }
                // Save the histories to the DB
                // https://fkkland.in.net/ https://sunnyday.in.net/tags/young/page/26/ https://family-nudism.pw/page/2/ https://garilas.site/ https://nudism-beauty.com/tags/photo https://onlynud.site/ https://viphentai.club/page/235/ https://secrethentai.club/page/130/
                _historicalTracking.SaveHistories(user, trackingHistories);
            }
        }
    }
};
  