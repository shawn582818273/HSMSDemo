using HSMSMessage;
using SecsDriverWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HSMSDemo
{
    class MessageReceiveHandler : IGemMessageHandlerExtension
    {

        public bool S1F1(XElement xmlMessage)
        {
            return true;
        }

        public bool S2F17(XElement xmlMessage)
        {
            return true;
        }

        public bool S6F11(XElement xmlMessage)
        {
            XElement primary = xmlMessage.Element("Primary");
            XElement list = primary.Element("Item");
            string a = list.Elements("Item").ElementAt(0).Element("Value").Value;
            string b = list.Elements("Item").ElementAt(1).Element("Value").Value;
            XElement list2 = list.Elements("Item").ElementAt(2).Element("Item");

            return true;
        }

        public bool S1F5(XElement xml)
        {
            XElement primary = xml.Element("Primary");
            var a = primary.Element("Item").Element("Value").Value;
            //pri
            return true;
        }

        public bool S1F6(XElement xmlMessage)
        {
            //throw new NotImplementedException();
            return true;
        }
    }
}
