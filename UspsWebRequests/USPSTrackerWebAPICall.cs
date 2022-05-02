﻿using System.Net;
using TrackerConfiguration;

namespace ExternalTrackingequests
{
    //
    // Static class for making the USPS tracking web request.
    //
    public static class USPSTrackerWebAPICall
    {
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
            string response;

            // Build the request.
            string request = TrackerConfig.UspsTrackingFieldUrl + " &XML=<TrackFieldRequest USERID=\"" + TrackerConfig.UspsTrackingUserId +
               "\"><Revision>1</Revision><ClientIp>122.3.3</ClientIp><SourceId>DEVELOPER</SourceId>";
            request += "<TrackID ID = \"" + trackingRequest + "\"></TrackID>";
            request += "</TrackFieldRequest>";

            // Get the response.
            try
            {
                using (WebClient client = new WebClient())
                {
                    response = client.DownloadString(request); // Uses GET
                }
            }
            catch (WebException)
            {
                response = "Error: There was a problem reaching the USPS website. Check the Internet connection.";
            }

            return response;
        }
    }
}