using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TrackerModel;

namespace ExternalTrackingequests
{
    // Static class for parsing the USPS TrackFieldRequest response.
    // Note that all relevant data is extracted but some may not be used by whatever UI is in place.
    // There are no UI dependencies.
    // Documentation for the TrackFieldRequest response XML can be found at https://www.usps.com/business/web-tools-apis/documentation-updates.htm.
    public static class USPSTrackingResponseParser
    {
        const string _notYetAvailable = "A status update is not yet available on your package.";

        /// <summary>
        ///     Takes the response from the WebService call to the USPS and parses it.
        ///     THe user's zip is used to indicate an incoming or outgoing package.
        /// </summary>
        /// <param name="responseXml">
        ///     The response from the GET to USPS for the tracking ID.
        /// </param>
        /// <param name="userZip">
        ///     The user's zip.
        /// </param>
        /// <returns>
        ///     A List of TrackingInfo objects that are parsed out of the USPS response.
        ///     Multiple tracking IDs can be contained in the response. The application
        ///     only processes one ID at a time, but the ability to process more is provided.
        /// </returns>
        public static List<TrackingInfo> USPSParseTrackingXml(string responseXml, string userZip)
        {
            List<TrackingInfo> trackResponseEvents = new List<TrackingInfo>();
            string errorSummary = "";
            bool hadError = true;  // Assume there was an error.

            try
            {
                // Parse the tracking events for this tracking id. If there is no error, get the list of tracking events.
                XDocument xmlDoc = XDocument.Parse(responseXml);
                XElement trackResponse = xmlDoc.Element("TrackResponse");

                // An error can be returned from USPS if there is a glitch in the request. Not likely, but can happoen.
                // Report as an internal error for the Status Summary.
                if (xmlDoc.Element("Error") != null)
                {
                    errorSummary = "There was an internal error. \n" + xmlDoc.Element("Error").Value;
                }
                // Check for a resonse that does not have a <TrackResponse> node. This can happen if there
                // is a response from other than USPS. This turned up during testing when ATT&T's network had a node malfunction
                // and gave an error response that would show up in a webpage for user diagnostics.
                else if (trackResponse == null)
                {
                    errorSummary = "There was an external error. Check the Internet connection.";
                }
                else
                {
                    // An error can be returned by USPS if it finds something it does not like in the request, again not likely but can happen.
                    XElement error = trackResponse.Element("Error");
                    if (trackResponse.Element("Error") == null)
                    {
                        hadError = false;

                        // Extract the tracking response for each tracking number and then extract the events for each.
                        IEnumerable<XElement> trackIdElements = trackResponse.Elements("TrackInfo");
                        // For each tracking Id and top off with the TrackSummary.
                        foreach (XElement trackInfo in trackIdElements)
                        {
                            TrackingInfo response = USPSPopulateTrackingHistoryFromXml(trackInfo, userZip);
                            trackResponseEvents.Add(response);
                        }
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
                TrackingInfo errorHistory = new TrackingInfo
                {
                    StatusSummary = errorSummary,
                    TrackingStatus = TrackingRequestStatus.InternalError
                };
                trackResponseEvents.Add(errorHistory);
            }

            return trackResponseEvents;
        }

        // 

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
            // If there was no error then we have a full response to parse.
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
                    // For some responses, there will be no event for the <TrackSummary>. Use DateTime.Now as the default.
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
                        if (history.LastEventDateTime < eventDateTime)
                            history.LastEventDateTime = eventDateTime;
                    }
                    eventSummary = eventDateTime.ToString("g", CultureInfo.CreateSpecificCulture("en-US")) + " ";
                    eventSummary += GetElementValue(trackEvents[i], "Event") + " ";
                    eventSummary += GetElementValue(trackEvents[i], "EventCity") + ", " + GetElementValue(trackEvents[i], "EventState");
                    history.TrackingHistory += eventSummary + (i > 0 ? "\n" : ""); // Don't add NL after last event.
                }

                // Give LastEventDateTime a value if it has not been updated.
                if (history.LastEventDateTime == DateTime.MinValue)
                    history.LastEventDateTime = DateTime.Now;

                // Get the first event DateTime
                // If there are no tracking events, the ID has not yet entered the USPS system. Use the tracking summary DateTime.
                if (trackEvents.Length == 0)
                    history.FirstEventDateTime = eventDateTime;
                else
                    history.FirstEventDateTime = DateTime.Parse(trackEvents[0].Element("EventDate").Value + " " + trackEvents[0].Element("EventTime").Value);

                // Set the delivery status of the package
                history.StatusSummary = GetElementValue(trackInfo, "StatusSummary");
                history.TrackingComplete = history.StatusSummary.Contains("was delivered") || history.StatusSummary.Contains("picked up");
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
