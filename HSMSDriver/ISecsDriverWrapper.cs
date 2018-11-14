using HSMSDriver;
using System;
using System.Xml.Linq;

namespace SecsDriverWrapper
{
    public enum SECSConnectionMode
    {
        Active,
        Passive,
    }

    public interface ISecsDriverWrapper
    {
        void CloseHSMSPort();
        void OpenHSMSPort();
        void SendAsync(string unitID, string transName, XElement xmlPrimaryMessage);
        XElement FindTransaction(string transName);
        void Register(ISECSIIMessageHandler handler);
    }
}
