using ExternalTrackingequests;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using TrackerConfiguration;
using TrackerModel;
using TrackingRequests.Interface;
using Windows.Networking;

namespace TrackingRequests.Impl
{
    //
    // Class with static methods for making the USPS tracking web request.
    //
    public class USPSTrackerWebAPICall : RequestHandlerInterface
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use.
        static readonly SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        };
        static readonly HttpClient _httpClient = new HttpClient(socketsHttpHandler);
        static readonly string _myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
        const string _notYetAvailable = "A status update is not yet available on your package.";

        public TrackingInfo HandleTrackingRequest(string trackingId, string userZip)
        {
            return GetTrackingFieldInfo(trackingId, userZip);
        }

        /// <summary>
        ///     Uses the tracking id to build the XML request to the USPS.
        ///     Then uses WebClient to make the web API call. 
        /// </summary>  
        /// <param name="trackingId">
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
        public static TrackingInfo GetTrackingFieldInfo(string trackingId, string userZip)
        {
            TrackingInfo parsedResponse;

            // Build the request.
            string request = TrackerConfig.UspsTrackingFieldUrl +
                 $" &XML=<TrackFieldRequest USERID=\"{TrackerConfig.UspsTrackingUserId}\">" +
                            "<Revision>1</Revision>" +
                            $"<ClientIp>{_myIP}</ClientIp>" +
                            "<SourceId>DEVELOPER</SourceId>";
            request += $"<TrackID ID = \"{trackingId}\"></TrackID>";
            request += "</TrackFieldRequest>";

            string httpResponse = _httpClient.GetStringAsync(request).Result; // Force synchronous call. (Uses GET)
            parsedResponse = USPSTrackingResponseParser.USPSParseTrackingXml(httpResponse, userZip);

            return parsedResponse;
        }

        /// <summary>
        ///     Method to extract tracking history from the XElement for an individual tracking id
        ///     into a TrackingInfo object. 
        /// </summary>
        /// <param name="trackInfo"></param>
        /// <param name="userZip"></param>
        /// <returns>
        ///     The TrackingInfo object is a general form of the tracking information
        ///     utilized by the view model for presentation to the view.
        /// </returns>
        private static TrackingInfo USPSPopulateTrackingHistoryFromXml(XElement trackInfo, string userZip)
        {
            TrackingInfo history = new TrackingInfo
            {
                TrackingId = trackInfo.Attribute("ID").Value
            };
            XElement error = trackInfo.Element("Error");
            if (error != null)
            {
                history.StatusSummary = GetElementValue(error, "Description");

                // If the description is the generic "not available" one, just say it's not available.
                if (history.StatusSummary.StartsWith(_notYetAvailable))
                    history.StatusSummary = _notYetAvailable + " Make sure you entered the Tracking Number correctly.";
                history.TrackingStatus = TrackingRequestStatus.NoRecord;
            }
            // If there was no error then we have a full httpResponse to parse.
            else
            {
                history.PostOfficeClass = GetElementValue(trackInfo, "Class");
                history.CityState = GetElementValue(trackInfo, "DestinationCity") + ", " + GetElementValue(trackInfo, "DestinationState");
                string destinationZip = GetElementValue(trackInfo, "DestinationZip");

                // Determine if the package is inbound. If userZip is blank, value will be false.
                history.Inbound = destinationZip == userZip;

                DateTime eventDateTime = DateTime.UtcNow; // Date and time are given as two XML elements <EventTime> and <EventDate>, e.g. "11:47 am" and "August 23, 2021".
                string eventSummary; // Summary is displayed as a space separated event date time as mm:hh + event + event city + event state

                // Get the tracking summary
                XElement trackSummary = trackInfo.Element("TrackSummary");
                if (trackSummary != null)
                {
                    // For some responses, there will be no event for the <TrackSummary>, so eventDateTime has been defaulted to DateTime.UtcNow.
                    if (trackSummary.Element("EventDate") != null)
                        eventDateTime = DateTime.Parse(trackSummary.Element("EventDate").Value + " " + trackSummary.Element("EventTime").Value);

                    eventSummary = eventDateTime.ToString("g", CultureInfo.CreateSpecificCulture("en-US")) + " ";

                    if (trackSummary.Element("Event") != null)
                    {
                        eventSummary += trackSummary.Element("Event").Value + " ";
                        eventSummary += trackSummary.Element("EventCity").Value + ", " + trackSummary.Element("EventState").Value;
                    }
                    history.StatusSummary = eventSummary;
                }

                // Get the events. History events are returned in reverse chronological order, flip for presentation.
                // Format into a muliple line single string.
                XElement[] trackEvents = trackInfo.Elements("TrackDetail").ToArray();
                DateTime lastEventTime = DateTime.MinValue;
                for (int i = trackEvents.Length - 1; i >= 0; i--)
                {
                    if (trackEvents[i].Element("EventDate") != null && trackEvents[i].Element("EventTime") != null)
                    {
                        eventDateTime = DateTime.Parse(trackEvents[i].Element("EventDate").Value + " " + trackEvents[i].Element("EventTime").Value);
                        if (lastEventTime < eventDateTime.ToUniversalTime())
                            lastEventTime = eventDateTime.ToUniversalTime();
                    }
                    eventSummary = eventDateTime.ToString("g", CultureInfo.CreateSpecificCulture("en-US")) + " ";
                    eventSummary += GetElementValue(trackEvents[i], "Event") + " ";
                    eventSummary += GetElementValue(trackEvents[i], "EventCity") + ", " + GetElementValue(trackEvents[i], "EventState");
                    history.TrackingHistory += eventSummary + (i > 0 ? "\n" : ""); // Don't add NL after last event.
                }

                // Get the first event DateTime
                // If there are no tracking events, the ID has not yet entered the USPS system. Use DateTime.UtcNow.
                if (trackEvents.Length == 0)
                {
                    history.FirstEventDateTime = eventDateTime.ToUniversalTime();
                    history.LastEventDateTime = DateTime.UtcNow;
                }
                else
                {
                    history.FirstEventDateTime = DateTime.Parse(trackEvents[trackEvents.Length - 1].Element("EventDate").Value + " " +
                        trackEvents[trackEvents.Length - 1].Element("EventTime").Value).ToUniversalTime();
                    history.LastEventDateTime = lastEventTime;
                }

                // Set the delivery status of the package
                history.StatusSummary = GetElementValue(trackInfo, "StatusSummary");
                history.TrackingComplete = history.StatusSummary.Contains("was delivered") || history.StatusSummary.Contains("has been delivered") ||
                    history.StatusSummary.Contains("picked up") && !history.StatusSummary.Contains("USPS picked up"); // Make sure it wasn't picked up by USPS.
                history.TrackingStatus = history.TrackingComplete ? TrackingRequestStatus.Delivered : TrackingRequestStatus.InTransit;
            }

            return history;
        }

        // Convenience routine to return element value or "Missing" if does not exist.
        // Reduces code clutter.
        private static string GetElementValue(XElement element, string elementName)
        {
            return element.Element(elementName) != null ? element.Element(elementName).Value : "Missing";
        }
    }
}