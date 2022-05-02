using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TrackerUtility
{
    public class TrackingResponse
    {
        public List<TrackResponseItem> TrackResponseItems { get; private set; }

        public TrackingResponse(string responseString)
        {
            ParseResponseXML(responseString);
        }

        private void ParseResponseXML(string responseXml)
        {
            Console.WriteLine(responseXml);

            TrackResponseItems = new List<TrackResponseItem>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseXml);

            string xpath = "TrackingResponse/TrackInfo";
            XmlNodeList nodes = xmlDoc.SelectNodes(xpath);

            // Extract the tracking history for each track Id.
            XmlNodeList trackInfoNodes = xmlDoc.GetElementsByTagName("TrackInfo");
            foreach (XmlNode trackInfo in trackInfoNodes)
            {
                string trackingId = trackInfo.Attributes["ID"].Value;
                XmlNode summaryNode = trackInfo.SelectSingleNode("TrackSummary");
                string summary = summaryNode.InnerText;

                bool delivered = summary.IndexOf("delivered") >= 0;

                // Get the XML for the tracking history (to be stored in DB). Add <TrackResponse> to reuse this code when extracting from DB.
                string xml = "<?xml version=\"1.0\" encoding=\"UTF - 8\"?>  <TrackResponse> " + trackInfo.OuterXml + "</TrackResponse>";
                Stack<string> details = new Stack<string>();

                // Extract each detail in the tracking history.
                XmlNodeList detailNodes = trackInfo.SelectNodes("TrackDetail");
                foreach (XmlNode detailNode in detailNodes)
                {
                    details.Push(detailNode.InnerText);
                }

                // One for each tracking Id.
                TrackResponseItem response = new TrackResponseItem(trackingId, summary, delivered, xml, details);
                TrackResponseItems.Add(response);
            }
        }
    }
}
