using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using TrackerConfiguration;
using TrackerModel;
using TrackingRequests.Interface;

namespace TrackingRequests.Impl
{
    //
    // Static class for making the USPS tracking web request.
    //
    public class USPSTrackerWebAPICall : RequestHandlerInterface
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use.
        static readonly SocketsHttpHandler socketsHttpHandler = new SocketsHttpHandler()
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        };
        static readonly HttpClient _httpClient = new HttpClient(socketsHttpHandler);
        static readonly string _myIP;
        const string _notYetAvailable = "A status update is not yet available on your package.";

        static USPSTrackerWebAPICall()
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            // Get the IP
            object foo = Dns.GetHostEntry(hostName).AddressList;
            _myIP = Dns.GetHostEntry(hostName).AddressList[1].ToString();
        }

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
            string httpResponse;
            bool hadInternalError = false;
            TrackingInfo parsedResponse;

            // Build the request.
            string request = TrackerConfig.UspsTrackingFieldUrl +
                 $" &XML=<TrackFieldRequest USERID=\"{TrackerConfig.UspsTrackingUserId}\">" +
                            "<Revision>1</Revision><ClientIp>122.3.3</ClientIp><SourceId>DEVELOPER</SourceId>";
            request += $"<TrackID ID = \"{trackingId}\"></TrackID>";
            request += "</TrackFieldRequest>";

            try
            {
                httpResponse = _httpClient.GetStringAsync(request).Result; // Force synchronous call. (Uses GET)
            }
            catch (WebException)
            {
                hadInternalError = true;
                httpResponse = "Error: There was a problem reaching the USPS website. Check the Internet connection.";
            }

            if (!hadInternalError)
            {
                parsedResponse = USPSParseTrackingXml(httpResponse, userZip);
            }
            else
            {
                parsedResponse = new TrackingInfo();
                parsedResponse.TrackingStatus = TrackingRequestStatus.InternalError;
            }

            return parsedResponse;
        }

        /// <summary>
        ///     Takes the httpResponse from the WebService call to the USPS and parses it.
        ///     THe user's zip is used to indicate an incoming or outgoing package.
        /// </summary>
        /// <param name="responseXml">
        ///     The httpResponse from the GET to USPS for the tracking ID.
        /// </param>
        /// <param name="userZip">
        ///     The user's zip.
        /// </param>
        /// <param name="userZip">
        ///     The descrption.
        /// </param>
        /// <returns>
        ///     The TrackingInfo objects that is parsed out of the USPS httpResponse.
        /// </returns>
        public static TrackingInfo USPSParseTrackingXml(string responseXml, string userZip)
        {
            TrackingInfo parsedResponse = new TrackingInfo();
            string errorSummary = "";
            bool hadError = true;  // Assume there was an error.

            try
            {
                // Parse the tracking events for this tracking id. If there is no error, get the list of tracking events.
                XDocument xmlDoc = XDocument.Parse(responseXml);
                XElement root = xmlDoc.Element("TrackResponse");

                // An error can be returned from USPS if there is a glitch in the request. Not likely, but can happoen.
                // Report as an internal error for the Status Summary.
                if (xmlDoc.Element("Error") != null)
                {
                    errorSummary = "There was an internal error. \n" + xmlDoc.Element("Error").Value;
                }
                // Check for a resonse that does not have a <TrackResponse> node. This can happen if there
                // is a httpResponse from other than USPS. This turned up during testing when ATT&T's network had a node malfunction
                // and gave an error httpResponse that would show up in a webpage for user diagnostics.
                else if (root == null)
                {
                    errorSummary = "There was an external error. Check the Internet connection.";
                }
                else if (root.Element("Error") != null)
                {
                    errorSummary = root.Element("Error").Value;
                }
                else
                {
                    XElement trackResponse = root.Element("TrackInfo");

                    // An error can be returned by USPS if it finds something it does not like in the request, again not likely but can happen.
                    XElement error = trackResponse.Element("Error");
                    if (trackResponse.Element("Error") == null)
                    {
                        hadError = false;

                        // USPS can return multiple <TrackInfo> elements, but we just use the first.
                        XElement trackInfo = root.Elements("TrackInfo").First();
                        parsedResponse = USPSPopulateTrackingHistoryFromXml(trackInfo, userZip);
                    }
                    else
                    {
                        errorSummary = trackResponse.Element("Error").Value;
                    }
                }
            }
            catch (Exception e)
            {
                // Add the exception description into the event list so it will display as the StatusSummary.
                errorSummary = "There was an external error. Check the Internet connection. Error: \n\t" + e.Message;
            }

            // If there was an error, set the Summary and Status.
            if (hadError)
            {
                parsedResponse.StatusSummary = errorSummary;
                parsedResponse.TrackingStatus = TrackingRequestStatus.InternalError;
            }

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
            TrackingInfo history = new TrackingInfo();

            history.TrackingId = trackInfo.Attribute("ID").Value;
            XElement error = trackInfo.Element("Error");
            if (error != null)
            {
                history.StatusSummary = GetElementValue(error, "Description");

                // If the description is the generic "not available" one, just say it's not available.
                if (history.StatusSummary.StartsWith(_notYetAvailable))
                    history.StatusSummary = _notYetAvailable + " Make sure you entered the Tracking Number correctly.";
                history.TrackingStatus = TrackingRequestStatus.NoRecord;
                history.LastEventDateTime = DateTime.Now;
            }
            // If there was no error then we have a full httpResponse to parse.
            else
            {
                history.PostOfficeClass = GetElementValue(trackInfo, "Class");
                history.CityState = GetElementValue(trackInfo, "DestinationCity") + ", " + GetElementValue(trackInfo, "DestinationState");
                string destinationZip = GetElementValue(trackInfo, "DestinationZip");

                // Determine if the package is inbound. If userZip is blank, value will be false.
                history.Inbound = destinationZip == userZip;

                DateTime eventDateTime = DateTime.Now; // Date and time are given as two XML elements <EventTime> and <EventDate>, e.g. "11:47 am" and "August 23, 2021".
                string eventSummary; // Summary is displayed as a space separated event date time as mm:hh + event + event city + event state

                // Get the tracking summary
                XElement trackSummary = trackInfo.Element("TrackSummary");
                if (trackSummary != null)
                {
                    // For some responses, there will be no event for the <TrackSummary>, so evnetDateTime has been defaulted to DateTime.Now.
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

                // Get the events. History events are returned in reversee chronological order, flip for presentation.
                // Format into a muliple line single string.
                XElement[] trackEvents = trackInfo.Elements("TrackDetail").ToArray();
                history.LastEventDateTime = DateTime.MinValue;
                for (int i = trackEvents.Length - 1; i >= 0; i--)
                {
                    if (trackEvents[i].Element("EventDate") != null && trackEvents[i].Element("EventTime") != null)
                    {
                        eventDateTime = DateTime.Parse(trackEvents[i].Element("EventDate").Value + " " + trackEvents[i].Element("EventTime").Value);
                        if (history.LastEventDateTime < eventDateTime.ToUniversalTime())
                            history.LastEventDateTime = eventDateTime.ToUniversalTime();
                    }
                    eventSummary = eventDateTime.ToString("g", CultureInfo.CreateSpecificCulture("en-US")) + " ";
                    eventSummary += GetElementValue(trackEvents[i], "Event") + " ";
                    eventSummary += GetElementValue(trackEvents[i], "EventCity") + ", " + GetElementValue(trackEvents[i], "EventState");
                    history.TrackingHistory += eventSummary + (i > 0 ? "\n" : ""); // Don't add NL after last event.
                }

                // Give LastEventDateTime a value if it has not been updated.
                if (history.LastEventDateTime == DateTime.MinValue)
                    history.LastEventDateTime = DateTime.UtcNow;

                // Get the first event DateTime
                // If there are no tracking events, the ID has not yet entered the USPS system. Use the tracking summary DateTime.
                if (trackEvents.Length == 0)
                    history.FirstEventDateTime = eventDateTime.ToUniversalTime();
                else
                    history.FirstEventDateTime = DateTime.Parse(trackEvents[trackEvents.Length - 1].Element("EventDate").Value + " " +
                        trackEvents[trackEvents.Length - 1].Element("EventTime").Value).ToUniversalTime();

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