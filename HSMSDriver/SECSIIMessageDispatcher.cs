using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SecsDriverWrapper
{
    public class SECSIIMessageDispatcher
    {
        protected class GemHandler
        {
            public string Name { get; set; }
            public Func<XElement, bool> Handler { get; set; }
        }

        //private HashSet<GemHandler> _handlers = new HashSet<GemHandler>();
        private Dictionary<string, Func<XElement, bool>> _handlers = new Dictionary<string, Func<XElement, bool>>();

        /// <summary>
        /// Registers the specified command handler.
        /// </summary>
        public void Register(ISECSIIMessageHandler secsIIMessageHandler)
        {
            //Only SxFx method name
            string pattern = @"^S[0-9]{1,3}F[0-9]{1,3}$";
            var handlers = (from item in secsIIMessageHandler.GetType().GetMethods()
                            where Regex.IsMatch(item.Name, pattern) && item.IsPublic && item.ReturnType == typeof(bool)
                            select new GemHandler { Name = item.Name, Handler = (Func<XElement, bool>)Delegate.CreateDelegate(typeof(Func<XElement, bool>), secsIIMessageHandler, item) }).ToList();

            if (_handlers.Keys.Any(n => handlers.Any(b => b.Name == n)))
                throw new ArgumentException("The SECSII message handled by the received handler already has a registered handler.");

            // Register this handler for each of the handler's name
            handlers.ForEach(h => _handlers.Add(h.Name, h.Handler));
        }

        /// <summary>
        /// Processes the message by calling the registered handler.
        /// </summary>
        public bool ProcessMessage(string traceIdentifier, XElement payload, string messageId, string correlationId)
        {
            Func<XElement, bool> handler = null;

            if (_handlers.TryGetValue(messageId, out handler))
            {
                return handler(payload);
            }
            else
                //LogInfo.ErrorFormat(messageId, CIS20EventSource.FormatException(new ArgumentException(string.Format("The SECSII message handler:{0} is not found.", messageId))));
                //throw new ArgumentException(string.Format("The SECSII message handler:{0} is not found.", messageId));
                return false;
        }
    }
}
