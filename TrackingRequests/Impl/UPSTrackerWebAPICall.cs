using System;
using System.Net;
using System.Net.Http;
using TrackerModel;
using TrackingRequests.Interface;

namespace TrackingRequests.Impl
{
    public class UPSTrackerWebAPICall : RequestHandlerInterface
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use.
        static readonly SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        };
        static readonly HttpClient _httpClient = new HttpClient(socketsHttpHandler);
        static readonly string _myIP;
        const string _notYetAvailable = "A status update is not yet available on your package.";

        static UPSTrackerWebAPICall()
        {
            // Get the IP
            _myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
        }

        public TrackingInfo HandleTrackingRequest(string trackingId, string userZip)
        {
            return GetTrackingFieldInfo(trackingId, userZip);
        }

        /// <summary>
        ///     Uses the tracking id to build the XML request to UPS.
        ///     Then uses WebClient to make the web API call. 
        /// </summary>  
        /// <param name="trackingId">
        ///     The tracking ID for the tracking request.
        /// </param>
        /// <returns>
        /// </returns>
        public static TrackingInfo GetTrackingFieldInfo(string trackingId, string userZip)
        {
            string httpResponse;
            bool hadInternalError = false;
            TrackingInfo parsedResponse = new TrackingInfo();

            // Build the request.

            // process the request
            try
            {
            }
            catch (WebException)
            {
                hadInternalError = true;
                httpResponse = "Error: There was a problem reaching the UPS website. Check the Internet connection.";
            }

            // Parse the request

            return parsedResponse;
        }
    }
}
