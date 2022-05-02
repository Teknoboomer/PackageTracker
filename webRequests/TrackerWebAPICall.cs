using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TrackerUtility
{
    public class TrackerWebAPICall
    {
        private string UspsUserId = "818DEVEL4717";
        private const string baseTrackingUrl = "http://production.shippingapis.com/ShippingAPI.dll?API=TrackV2 &XML=<TrackRequest USERID=\"uuuuu\">";
        private const string trackingIdXml = "<TrackID ID = \"xxxxx\"></TrackID>";
        private const string trackingTrailer = "</TrackRequest>";

        private WebClient client = new WebClient();

        public TrackerWebAPICall(string id)
        {
            UspsUserId = id;
        }

        public string GetTrackingInfo (List<string> trackingIds)
        {
            string request = "http://production.shippingapis.com/ShippingAPI.dll? API = TrackV2 & XML =<TrackRequest USERID = \"818DEVEL4717\"><TrackID ID = \"9449011200817829047138 \"></TrackID></TrackRequest>";
            Console.WriteLine(request);
            Console.WriteLine();

            request = baseTrackingUrl.Replace("uuuuu", UspsUserId);
            for (int i = 0; i < trackingIds.Count; i++)
            {
                request += "\t\n" + trackingIdXml.Replace("xxxxx", trackingIds[i]);
            }
            request += "\n" + trackingTrailer;


            Console.WriteLine(request);
            Console.WriteLine();

            string response = client.DownloadString(request);
            Console.WriteLine(response);


            return response;
        }

    }
}
