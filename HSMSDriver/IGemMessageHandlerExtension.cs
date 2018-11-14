using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SecsDriverWrapper
{

    public interface IGemMessageHandlerExtension : ISECSIIMessageHandler
    {
        bool S1F1(XElement xmlMessage);
        bool S1F5(XElement xmlMessage);
        bool S1F6(XElement xmlMessage);
        bool S2F17(XElement xmlMessage);
        bool S6F11(XElement xmlMessage);
    }
}
