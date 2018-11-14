using HSMSMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecsDriverWrapper;
using HSMSDriver;
using System.Xml.Linq;

namespace HSMSDemo
{
    class MessageSender
    {
        private ISecsDriverWrapper Wrapper;

        public MessageSender(ISecsDriverWrapper wrapper)
        {
            this.Wrapper = wrapper;
        }

        //public void Send(S1F5 msg)
        //{
        //}
        public void Send(S2F31 msg)
        {
            XElement primary = new XElement("Primary");
            var time = NewItem("Time", "", "ASCII");
            time.Add(new XElement("Value", msg.time));
            primary.Add(time);
            Wrapper.SendAsync("Line05-8MMAO05", "S2F31", primary);
        }
        public void Send(S2F37 msg)
        {
            XElement primary = new XElement("Primary");
            var list = NewItem("L", "", "LIST");
            list.Add(NewItem("CEED", "", "ASCII", msg.CEED));
            var list2 = NewItem("L", "", "LIST");
            list2.Add(NewItem("CEID", "", "ASCII", msg.CEID));
            list.Add(list2);
            primary.Add(list);
            Wrapper.SendAsync("Line05-8MMAO05", "S2F37", primary);
        }
        private XElement NewItem(string name, string description, string format)
        {
            return 
                new XElement("Item",new XElement("Name", name),
                new XElement("Description", description),
                new XElement("Format", format));
        }
        private XElement NewItem(string name, string description, string format, string value)
        {
            return 
                new XElement("Item",new XElement("Name", name),
                new XElement("Description", description),
                new XElement("Format", format),
                new XElement("Value", value));
        }
    }
}
