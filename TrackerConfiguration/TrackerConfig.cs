using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace TrackerConfiguration
{

    public static class TrackerConfig
    {
        public static string UspsTrackingUserId { get; private set; }
        public static string UspsTrackingFieldUrl { get; private set; }
        public static string SqlConnection { get; private set; }
        public static string HistoryFilePath { get; private set; }

        static TrackerConfig()
        {
            IConfiguration _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            SqlConnection = _config["SqlConnection"];
            UspsTrackingUserId = _config["UspsUserTrackingId"];
            UspsTrackingFieldUrl = "http://production.shippingapis.com/ShippingAPI.dll?API=TrackV2";


            HistoryFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "USPSTracking.xml");
        }
    }
}

