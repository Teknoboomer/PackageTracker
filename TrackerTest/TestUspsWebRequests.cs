using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Linq;
using TrackerModel;
using ExternalTrackingequests;
using System;
using HistoricalTracking;
using TrackerConfiguration;
using System.IO;

namespace TrackerTest
{
    public class UspsResponseTests
    {
        private Guid password = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PasswordSecurity()
        {
            TrackerSecurity security = new TrackerSecurity();
            security.LogRounds = 11;
            string hashedPassword = security.GetHashedPassword("Alphonse", password.ToString());

            // Make sure the hash comes out the same twice in a row.
            Assert.AreEqual(hashedPassword, security.GetHashedPassword("Alphonse", password.ToString()));
        }

        [Test]
        public void ActiveUSPSWebServiceCall()
        {
            string knownResponse = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<Error><Number>80040B19</Number><Description>XML Syntax Error: Please check the XML request to see if it can be parsed.(B)</Description><Source>USPSCOM::DoAuth</Source></Error>";
            string response = USPSTrackerWebAPICall.GetTrackingFieldInfoAsync("EJ123456780US");

            Assert.AreEqual(knownResponse, response);
        }

        [Test]
        public void TestExternalErrorResponse()
        {
            string externalErrorRecordResponse = "<Error> " + "\n" +
                    "Had an error " + "\n" +
                "</Error>";

            List<TrackingInfo> trackingHistories = USPSTrackingResponseParser.USPSParseTrackingXml(externalErrorRecordResponse, "77459");
            Assert.AreEqual(trackingHistories.Count, 1);
            Assert.AreEqual(trackingHistories[0].TrackingStatus, TrackingRequestStatus.InternalError);
            Assert.AreEqual(trackingHistories[0].StatusSummary, "There was an internal error. \n \nHad an error \n");
        }

        [Test]
        public void TestTrackingErrorResponse()
        {
            string trackingErrorResponse = "<TrackResponse> " + "\n" +
                "<Error>" + "\n" +
                    "An error occurred" + "\n" +
                "</Error>" + "\n" +
                "</TrackResponse>";

            List<TrackingInfo> trackingHistories = USPSTrackingResponseParser.USPSParseTrackingXml(trackingErrorResponse, "77459");
            Assert.AreEqual(trackingHistories.Count, 1);
            Assert.AreEqual(trackingHistories[0].TrackingStatus, TrackingRequestStatus.InternalError);
            Assert.AreEqual(trackingHistories[0].StatusSummary, "\nAn error occurred\n");
        }

        [Test]
        public void TestNoRecordResponse()
        {
            string noRecordResponse = "<TrackResponse> " + "\n" +
                "<TrackInfo ID =\"EJ123456780US\">" + "\n" +
                    "<TrackSummary> There is no record of that mail item. If it was mailed recently, It may not yet be tracked. Please try again later.</TrackSummary> " + "\n" +
                "</TrackInfo>" + "\n" +
                "</TrackResponse>";

            List<TrackingInfo> trackingHistories = USPSTrackingResponseParser.USPSParseTrackingXml(noRecordResponse, "77459");
            Assert.AreEqual(trackingHistories.Count, 1);
        }

        [Test]
        public void TestNotYetResponse()
        {
            string notYetResponse = "<?xml version =\"1.0\" encoding=\"UTF-8\"?>" + "\n" +
                "<TrackResponse>" + "\n" +
                    "<TrackInfo ID = \"9449011200817801431609\"> " + "\n" +
                        "<Error>" + "\n" +
                             "<Number>-2147219283</Number>" + "\n" +
                             "<Description>A status update is not yet available on your package.It will be available when the shipper provides an update or the package is delivered to USPS.Check back soon.Sign up for Informed Delivery<SUP>reg; </SUP> to receive notifications for packages addressed to you.</Description>" + "\n" +
                              "<HelpFile/>" + "\n" +
                              "<HelpContext/>" + "\n" +
                        "</Error>" + "\n" +
                    "</TrackInfo>" + "\n" +
                "</TrackResponse>" + "\n";


            List<TrackingInfo> trackingHistories = USPSTrackingResponseParser.USPSParseTrackingXml(notYetResponse, "77459");
            Assert.AreEqual(trackingHistories.Count, 1);
        }

        [Test]
        public void StaticTestNormmalInboundResponse()
        {
            string firstResponse = "<?xml version = \"1.0\" encoding = \"UTF-8\"?>" + "\n" +
                "<TrackResponse>" + "\n" +
                    "<TrackInfo ID = \"9449011200817803288113\">" + "\n" +
                            "<Class> Media Mail </Class>" + "\n" +
                            "<ClassOfMailCode> BS </ClassOfMailCode>" + "\n" +
                            "<DestinationCity> MEDICAL LAKE </DestinationCity>" + "\n" +
                            "<DestinationState> WA </DestinationState>" + "\n" +
                            "<DestinationZip> 99022 </DestinationZip>" + "\n" +
                            "<EmailEnabled> true </EmailEnabled>" + "\n" +
                            "<KahalaIndicator> false </KahalaIndicator>" + "\n" +
                            "<MailTypeCode> DM </MailTypeCode>" + "\n" +
                            "<MPDATE> 2022 - 04 - 03 14:23:09.000000 </MPDATE>" + "\n" +
                            "<MPSUFFIX> 196196353 </MPSUFFIX>" + "\n" +
                            "<PodEnabled> false </PodEnabled>" + "\n" +
                            "<TPodEnabled> false </TPodEnabled>" + "\n" +
                            "<RestoreEnabled> false </RestoreEnabled>" + "\n" +
                            "<RramEnabled> false </RramEnabled>" + "\n" +
                            "<RreEnabled> false </RreEnabled>" + "\n" +
                            "<Service> USPS Tracking<SUP> &#174;</SUP></Service>" + "\n" +
                            "<ServiceTypeCode> 490 </ServiceTypeCode>" + " \n" +
                            "<Status> Shipping Label Created, USPS Awaiting Item</Status>" + "\n" +
                            "<StatusCategory> Pre - Shipment </StatusCategory>" + "\n" +
                            "<StatusSummary> A shipping label has been prepared for your item at 6:19 pm on April 3, 2022 in MISSOURI CITY, TX 77459.This does not indicate receipt by the USPS or the actual mailing date.</StatusSummary>" + "\n" +
                            "<TABLECODE> T </TABLECODE>" + "\n" +
                        "<TrackSummary>" + "\n" +
                            "<EventTime> 6:19 pm </EventTime>" + "\n" +
                            "<EventDate> April 3, 2022 </EventDate>" + "\n" +
                            "<Event> Shipping Label Created, USPS Awaiting Item </Event>" + "\n" +
                            "<EventCity> MISSOURI CITY </EventCity>" + "\n" +
                            "<EventState> TX </EventState>" + "\n" +
                            "<EventZIPCode> 77459 </EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent> false </AuthorizedAgent>" + "\n" +
                            "<EventCode> GX </EventCode>" + "\n" +
                            "<DeliveryAttributeCode> 33 </DeliveryAttributeCode>" + "\n" +
                        "</TrackSummary>" + "\n" +
                    "</TrackInfo>" + "\n" +
                "</TrackResponse>" + "\n";

            List<TrackingInfo> trackingHistories = USPSTrackingResponseParser.USPSParseTrackingXml(firstResponse, "77459");
            Assert.AreEqual(trackingHistories.Count, 1);

            TrackingInfo trackingInfo = trackingHistories[0];
            Assert.AreEqual(trackingInfo.TrackingStatus, TrackingRequestStatus.InTransit);
        }

        [Test]
        public void FullGoodResponse()
        {
            string goodResponse =
            "<TrackResponse> " + "\n" +
                "<TrackInfo ID = \"9374889671006176791375\">" + "\n" +
                    "<Class>Parcel Select Lightweight</Class>" + "\n" +
                    "<ClassOfMailCode>LW</ClassOfMailCode>" + "\n" +
                    "<DestinationCity>MISSOURI CITY</DestinationCity>" + "\n" +
                    "<DestinationState>TX</DestinationState>" + "\n" +
                    "<DestinationZip>77459</DestinationZip>" + "\n" +
                    "<EmailEnabled>true</EmailEnabled>" + "\n" +
                    "<KahalaIndicator>false</KahalaIndicator>" + "\n" +
                    "<MailTypeCode>DM</MailTypeCode>" + "\n" +
                    "<MPDATE>2021 - 10 - 01 03:33:54.000000</MPDATE>" + "\n" +
                    "<MPSUFFIX>632337242</MPSUFFIX>" + "\n" +
                    "<OriginCity>MISSOURI CITY</OriginCity>" + "\n" +
                    "<OriginState>TX</OriginState>" + "\n" +
                    "<OriginZip>77459</OriginZip>" + "\n" +
                    "<PodEnabled>false</PodEnabled>" + "\n" +
                    "<TPodEnabled>false</TPodEnabled>" + "\n" +
                    "<RestoreEnabled>false</RestoreEnabled>" + "\n" +
                    "<RramEnabled>false</RramEnabled>" + "\n" +
                    "<RreEnabled>false</RreEnabled>" + "\n" +
                    "<Service>USPS Tracking</Service>" + "\n" +
                    "<ServiceTypeCode>748</ServiceTypeCode>" + "\n" +
                    "<Status>Delivered, In / At Mailbox</Status>" + "\n" +
                    "<StatusCategory>Delivered</StatusCategory>" + "\n" +
                    "<StatusSummary>Your item was delivered in or at the mailbox at 2:50 pm on October 3, 2021 in MISSOURI CITY, TX 77459.</StatusSummary>" + "\n" +
                    "<TABLECODE>T</TABLECODE>" + "\n" +
                    "<TrackSummary>" + "\n" +
                    "<EventTime>2:50 pm</EventTime>" + "\n" +
                    "<EventDate>October 3, 2021</EventDate>" + "\n" +
                    "<Event>Delivered, In / At Mailbox</Event>" + "\n" +
                    "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                    "<EventState>TX</EventState>" + "\n" +
                    "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                    "<EventCountry/>" + "\n" +
                    "<FirmName/>" + "\n" +
                    "<Name/>" + "\n" +
                    "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                    "<EventCode>01</EventCode>" + "\n" +
                    "<DeliveryAttributeCode>01</DeliveryAttributeCode>" + "\n" +
                    "</TrackSummary>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:54 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Out for Delivery</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>OF</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:43 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Arrived at Hub</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>07</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>4:21 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>USPS in possession of item</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>03</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>1:00 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>HUMBLE</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77338</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>10:09 pm</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>12:52 am</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Picked Up by Shipping Partner, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>80</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                    "</TrackInfo>" + "\n" +
                "</TrackResponse>" + "\n";

            List<TrackingInfo> trackingHistories = USPSTrackingResponseParser.USPSParseTrackingXml(goodResponse, "77459");
            Assert.AreEqual(trackingHistories.Count, 1);

            TrackingInfo trackingInfo = trackingHistories[0];
            Assert.AreEqual(trackingInfo.TrackingStatus, TrackingRequestStatus.Delivered);
            Assert.AreEqual(trackingInfo.TrackingId, "9374889671006176791375");

            // Make sure we picked out latest DateTime from the TrackDetail nodes.
            DateTime latest = new DateTime(2021, 10, 3, 7, 54, 0);
            Assert.AreEqual(latest, trackingInfo.LastEventDateTime);
        }

        [Test]
        public void SaveRestore()
        {
            string goodResponse =
            "<TrackResponse> " + "\n" +
                "<TrackInfo ID = \"9374889671006176791375\">" + "\n" +
                    "<Class>Parcel Select Lightweight</Class>" + "\n" +
                    "<ClassOfMailCode>LW</ClassOfMailCode>" + "\n" +
                    "<DestinationCity>MISSOURI CITY</DestinationCity>" + "\n" +
                    "<DestinationState>TX</DestinationState>" + "\n" +
                    "<DestinationZip>77459</DestinationZip>" + "\n" +
                    "<EmailEnabled>true</EmailEnabled>" + "\n" +
                    "<KahalaIndicator>false</KahalaIndicator>" + "\n" +
                    "<MailTypeCode>DM</MailTypeCode>" + "\n" +
                    "<MPDATE>2021 - 10 - 01 03:33:54.000000</MPDATE>" + "\n" +
                    "<MPSUFFIX>632337242</MPSUFFIX>" + "\n" +
                    "<OriginCity>MISSOURI CITY</OriginCity>" + "\n" +
                    "<OriginState>TX</OriginState>" + "\n" +
                    "<OriginZip>77459</OriginZip>" + "\n" +
                    "<PodEnabled>false</PodEnabled>" + "\n" +
                    "<TPodEnabled>false</TPodEnabled>" + "\n" +
                    "<RestoreEnabled>false</RestoreEnabled>" + "\n" +
                    "<RramEnabled>false</RramEnabled>" + "\n" +
                    "<RreEnabled>false</RreEnabled>" + "\n" +
                    "<Service>USPS Tracking</Service>" + "\n" +
                    "<ServiceTypeCode>748</ServiceTypeCode>" + "\n" +
                    "<Status>Delivered, In / At Mailbox</Status>" + "\n" +
                    "<StatusCategory>Delivered</StatusCategory>" + "\n" +
                    "<StatusSummary>Your item was delivered in or at the mailbox at 2:50 pm on October 3, 2021 in MISSOURI CITY, TX 77459.</StatusSummary>" + "\n" +
                    "<TABLECODE>T</TABLECODE>" + "\n" +
                    "<TrackSummary>" + "\n" +
                    "<EventTime>2:50 pm</EventTime>" + "\n" +
                    "<EventDate>October 3, 2021</EventDate>" + "\n" +
                    "<Event>Delivered, In / At Mailbox</Event>" + "\n" +
                    "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                    "<EventState>TX</EventState>" + "\n" +
                    "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                    "<EventCountry/>" + "\n" +
                    "<FirmName/>" + "\n" +
                    "<Name/>" + "\n" +
                    "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                    "<EventCode>01</EventCode>" + "\n" +
                    "<DeliveryAttributeCode>01</DeliveryAttributeCode>" + "\n" +
                    "</TrackSummary>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:54 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Out for Delivery</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>OF</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:43 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Arrived at Hub</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>07</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>4:21 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>USPS in possession of item</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>03</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>1:00 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>HUMBLE</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77338</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>10:09 pm</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>12:52 am</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Picked Up by Shipping Partner, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>80</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                    "</TrackInfo>" + "\n" +
                "<TrackInfo ID = \"9374889671006176791375\">" + "\n" +
                    "<Class>Parcel Select Lightweight</Class>" + "\n" +
                    "<ClassOfMailCode>LW</ClassOfMailCode>" + "\n" +
                    "<DestinationCity>MISSOURI CITY</DestinationCity>" + "\n" +
                    "<DestinationState>TX</DestinationState>" + "\n" +
                    "<DestinationZip>77459</DestinationZip>" + "\n" +
                    "<EmailEnabled>true</EmailEnabled>" + "\n" +
                    "<KahalaIndicator>false</KahalaIndicator>" + "\n" +
                    "<MailTypeCode>DM</MailTypeCode>" + "\n" +
                    "<MPDATE>2021 - 10 - 01 03:33:54.000000</MPDATE>" + "\n" +
                    "<MPSUFFIX>632337242</MPSUFFIX>" + "\n" +
                    "<OriginCity>MISSOURI CITY</OriginCity>" + "\n" +
                    "<OriginState>TX</OriginState>" + "\n" +
                    "<OriginZip>77459</OriginZip>" + "\n" +
                    "<PodEnabled>false</PodEnabled>" + "\n" +
                    "<TPodEnabled>false</TPodEnabled>" + "\n" +
                    "<RestoreEnabled>false</RestoreEnabled>" + "\n" +
                    "<RramEnabled>false</RramEnabled>" + "\n" +
                    "<RreEnabled>false</RreEnabled>" + "\n" +
                    "<Service>USPS Tracking</Service>" + "\n" +
                    "<ServiceTypeCode>748</ServiceTypeCode>" + "\n" +
                    "<Status>Delivered, In / At Mailbox</Status>" + "\n" +
                    "<StatusCategory>Delivered</StatusCategory>" + "\n" +
                    "<StatusSummary>Your item was delivered in or at the mailbox at 2:50 pm on October 3, 2021 in MISSOURI CITY, TX 77459.</StatusSummary>" + "\n" +
                    "<TABLECODE>T</TABLECODE>" + "\n" +
                    "<TrackSummary>" + "\n" +
                    "<EventTime>2:50 pm</EventTime>" + "\n" +
                    "<EventDate>October 3, 2021</EventDate>" + "\n" +
                    "<Event>Delivered, In / At Mailbox</Event>" + "\n" +
                    "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                    "<EventState>TX</EventState>" + "\n" +
                    "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                    "<EventCountry/>" + "\n" +
                    "<FirmName/>" + "\n" +
                    "<Name/>" + "\n" +
                    "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                    "<EventCode>01</EventCode>" + "\n" +
                    "<DeliveryAttributeCode>01</DeliveryAttributeCode>" + "\n" +
                    "</TrackSummary>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:54 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Out for Delivery</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>OF</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:43 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Arrived at Hub</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>07</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>4:21 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>USPS in possession of item</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>03</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>1:00 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>HUMBLE</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77338</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>10:09 pm</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>12:52 am</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Picked Up by Shipping Partner, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>80</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                    "</TrackInfo>" + "\n" +
                "</TrackResponse>" + "\n";

            HistoricalTrackingAccess historicalTracking = new HistoricalTrackingAccess(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestUSPSTracking.xml"));
            List<TrackingInfo> histories = USPSTrackingResponseParser.USPSParseTrackingXml(goodResponse, "77459");
            Assert.AreEqual(histories.Count, 2); // Should be two TrackingInfos.

            // Save the histories and read them back in. Then compare the two.
            historicalTracking.SaveHistories(histories);

            List<TrackingInfo> savedHistories = historicalTracking.GetSavedHistories();
            Assert.AreEqual(savedHistories.Count, 2); // Should still be two TrackingInfos.

            for (int i = 0; i < savedHistories.Count; i++)
            {
                TrackingInfo savedHistory = savedHistories[i];
                TrackingInfo history = histories[i];

                Assert.AreEqual(savedHistory.FirstEventDateTime, history.LastEventDateTime);
                Assert.AreEqual(savedHistory.LastEventDateTime, history.LastEventDateTime);
                Assert.AreEqual(savedHistory.TrackingStatus, history.TrackingStatus);
            }
        }
        [Test]
        public void SaveDetectLost()
        {
            string goodResponse =
            "<TrackResponse> " + "\n" +
                "<TrackInfo ID = \"9374889671006176791375\">" + "\n" +
                    "<Class>Parcel Select Lightweight</Class>" + "\n" +
                    "<ClassOfMailCode>LW</ClassOfMailCode>" + "\n" +
                    "<DestinationCity>MISSOURI CITY</DestinationCity>" + "\n" +
                    "<DestinationState>TX</DestinationState>" + "\n" +
                    "<DestinationZip>77459</DestinationZip>" + "\n" +
                    "<EmailEnabled>true</EmailEnabled>" + "\n" +
                    "<KahalaIndicator>false</KahalaIndicator>" + "\n" +
                    "<MailTypeCode>DM</MailTypeCode>" + "\n" +
                    "<MPDATE>2021 - 10 - 01 03:33:54.000000</MPDATE>" + "\n" +
                    "<MPSUFFIX>632337242</MPSUFFIX>" + "\n" +
                    "<OriginCity>MISSOURI CITY</OriginCity>" + "\n" +
                    "<OriginState>TX</OriginState>" + "\n" +
                    "<OriginZip>77459</OriginZip>" + "\n" +
                    "<PodEnabled>false</PodEnabled>" + "\n" +
                    "<TPodEnabled>false</TPodEnabled>" + "\n" +
                    "<RestoreEnabled>false</RestoreEnabled>" + "\n" +
                    "<RramEnabled>false</RramEnabled>" + "\n" +
                    "<RreEnabled>false</RreEnabled>" + "\n" +
                    "<Service>USPS Tracking</Service>" + "\n" +
                    "<ServiceTypeCode>748</ServiceTypeCode>" + "\n" +
                    "<Status>Delivered, In / At Mailbox</Status>" + "\n" +
                    "<StatusCategory>Delivered</StatusCategory>" + "\n" +
                    "<StatusSummary>Your item was not delivered in or at the mailbox at 2:50 pm on October 3, 2021 in MISSOURI CITY, TX 77459.</StatusSummary>" + "\n" +
                    "<TABLECODE>T</TABLECODE>" + "\n" +
                    "<TrackSummary>" + "\n" +
                    "<EventTime>2:50 pm</EventTime>" + "\n" +
                    "<EventDate>October 3, 2021</EventDate>" + "\n" +
                    "<Event>Delivered, In / At Mailbox</Event>" + "\n" +
                    "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                    "<EventState>TX</EventState>" + "\n" +
                    "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                    "<EventCountry/>" + "\n" +
                    "<FirmName/>" + "\n" +
                    "<Name/>" + "\n" +
                    "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                    "<EventCode>01</EventCode>" + "\n" +
                    "<DeliveryAttributeCode>01</DeliveryAttributeCode>" + "\n" +
                    "</TrackSummary>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:54 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Out for Delivery</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>OF</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>7:43 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Arrived at Hub</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>07</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>4:21 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>USPS in possession of item</Event>" + "\n" +
                            "<EventCity>MISSOURI CITY</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77459</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>03</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>1:00 am</EventTime>" + "\n" +
                            "<EventDate>October 3, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>HUMBLE</EventCity>" + "\n" +
                            "<EventState>TX</EventState>" + "\n" +
                            "<EventZIPCode>77338</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>10:09 pm</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Departed Shipping Partner Facility, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>82</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                        "<TrackDetail>" + "\n" +
                            "<EventTime>12:52 am</EventTime>" + "\n" +
                            "<EventDate>October 1, 2021</EventDate>" + "\n" +
                            "<Event>Picked Up by Shipping Partner, USPS Awaiting Item</Event>" + "\n" +
                            "<EventCity>OKLAHOMA CITY</EventCity>" + "\n" +
                            "<EventState>OK</EventState>" + "\n" +
                            "<EventZIPCode>73159</EventZIPCode>" + "\n" +
                            "<EventCountry/>" + "\n" +
                            "<FirmName/>" + "\n" +
                            "<Name/>" + "\n" +
                            "<AuthorizedAgent>false</AuthorizedAgent>" + "\n" +
                            "<EventCode>80</EventCode>" + "\n" +
                        "</TrackDetail>" + "\n" +
                    "</TrackInfo>" + "\n" +
                "</TrackResponse>" + "\n";

            HistoricalTrackingAccess historicalTracking = new HistoricalTrackingAccess(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestUSPSTracking.xml"));
            List<TrackingInfo> histories = USPSTrackingResponseParser.USPSParseTrackingXml(goodResponse, "77459");
            Assert.AreEqual(histories.Count, 1); // Should be two TrackingInfos.

            // Save the histories and read them back in. Then compare the two.
            historicalTracking.SaveHistories(histories);

            List<TrackingInfo> savedHistories = historicalTracking.GetSavedHistories();
            Assert.AreEqual(savedHistories.Count, 1); // Should still be two TrackingInfos.

            Assert.AreEqual(savedHistories[0].TrackingStatus, TrackingRequestStatus.Lost); // TrackingStatus should be lost.
        }

    }
}