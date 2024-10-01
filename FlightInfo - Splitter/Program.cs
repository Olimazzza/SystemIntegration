using System;
using System.Xml;
using System.Xml.XPath;

namespace FlightInfo___Splitter
{
        class XmlParser()
    {
        public static void Main(string[] args)
        {
            //Load XML dokumentet
            var xml = new XmlDocument();
            xml.Load("/Users/Oliver/Library/Application Support/JetBrains/Rider2024.2/scratches/scratch.xml");
            XPathNavigator navigator = xml.CreateNavigator();

            //Udtræk passager info
            string passengerXPath = "/FlightDetailsInfoResponse/Passenger";
            XPathNavigator passengerNode = navigator.SelectSingleNode(passengerXPath);
            Console.WriteLine("Passenger Info: " + passengerNode.OuterXml);

            //Udtræk baggage info
            string luggageXPath = "/FlightDetailsInfoResponse/Luggage";
            XPathNodeIterator luggageNodes = navigator.Select(luggageXPath);
            while (luggageNodes.MoveNext())
            {
                Console.WriteLine("Luggage Info: " + luggageNodes.Current.OuterXml);
            }

            var producer = new Producer();

            //Send baggage info
            while(luggageNodes.MoveNext())
            {
                producer.SendLuggageMessage(luggageNodes.Current.OuterXml);
            }
            
            if (passengerNode != null)
            {
                producer.SendPassengerMessage(passengerNode.OuterXml);
            }
            
            
        }
    }
}