using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TrackerConfiguration;

namespace ExternalTrackingequests
{
    //
    // Static class for making the USPS tracking web request.
    //
    public static class USPSTrackerWebAPICall
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use.
        static readonly HttpClient _httpClient = new HttpClient();
        static readonly string _myIP;

        static USPSTrackerWebAPICall()
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            // Get the IP
            _myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
        }

        /// <summary>
        ///     Uses the tracking id to build the XML request to the USPS.
        ///     Then uses WebClient to make the web API call. 
        /// </summary>  
        /// <param name="trackingRequest">
        ///     The tracking ID for the tracking request. Up to 10 tracking
        ///     IDs may be contained in each API request to the Web Tools server,
        ///     but the application requests only one.
        /// </param>
        /// <returns>
        ///     Documentation for the return XML can be found at:
        ///     https://www.usps.com/business/web-tools-apis/documentation-updates.htm
        ///     
        ///     The class TrackingUSPSResponseParser can be referenced as to what
        ///     fields are utilized from the XML.
        /// </returns>

        public static string GetTrackingFieldInfo(string trackingRequest)
        {
            string response = null;

            // Build the request.
            string request = TrackerConfig.UspsTrackingFieldUrl +
                 $" &XML=<TrackFieldRequest USERID=\"{TrackerConfig.UspsTrackingUserId}\">" +
                            "<Revision>1</Revision><ClientIp>122.3.3</ClientIp><SourceId>DEVELOPER</SourceId>";
            request +=     $"<TrackID ID = \"{trackingRequest}\"></TrackID>";
            request +=  "</TrackFieldRequest>";

                try
                {
                    response = _httpClient.GetStringAsync(request).Result; // Force synchronous call. (Uses GET)
                }
                catch (WebException)
                {
                    response = "Error: There was a problem reaching the USPS website. Check the Internet connection.";
                }

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return response;
        }
    }
}