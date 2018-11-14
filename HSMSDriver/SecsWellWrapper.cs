using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using HSMSDriver;
using log4net;
using System.Configuration;
using log4net.Core;

namespace SecsDriverWrapper
{
    public class SecsWellWrapper : ISecsDriverWrapper
    {
        private Dictionary<string, SECSPort> secsPortList = new Dictionary<string, SECSPort>(5);
        private Dictionary<string, HostInfo> hostInfoList = new Dictionary<string, HostInfo>(5);
        private string _unitID;
        private SECSPort _secsPort;
        private HostInfo _hostInfo;
        private ILog logger;
        public readonly SECSIIMessageDispatcher _secsIIMessageDispatcher;

        public SECSConnectionMode ConnectionMode { get; set; }
        public SecsWellWrapper()
        {
            logger = LogManager.GetLogger("HSMS");
            _secsIIMessageDispatcher = new SECSIIMessageDispatcher();
            //OpenHSMSPort();
        }

        public void OpenHSMSPort()
        {
            try
            {
                string xmlConfigFile = AppDomain.CurrentDomain.BaseDirectory + "ConfigureFiles\\HSMS\\HostInfo.xml";
                //_logWriter = logWriter;
                foreach (XElement var in XElement.Load(xmlConfigFile).Nodes())
                {
                    _hostInfo = new HostInfo(var);
                    //JsonTextSerializer.DataSerializer.Deserialize<HostInfo>(var as XElement);
                    hostInfoList.Add(_hostInfo.UnitID, _hostInfo);
                }
                if (hostInfoList.Count > 1)
                {
                    foreach (var var in hostInfoList)
                    {
                        _secsPort = new SECSPort(var.Key);
                        SetHSMSDriver(var.Value);
                        _secsPort.OpenPort();
                        secsPortList.Add(var.Key, _secsPort);
                    }
                }
                else
                {
                    _secsPort = new SECSPort("CIS");
                    SetHSMSDriver(_hostInfo);
                    _secsPort.OpenPort();
                }
            }
            catch (Exception e)
            {
                //_logWriter.Write(e.Message, "ApplicationException", 0, 8000, System.Diagnostics.TraceEventType.Error, "HSMSDriver Open Error!");
            }
        }

        public void CloseHSMSPort()
        {
            if (secsPortList.Count > 0)
            {
                foreach (var var in secsPortList)
                    var.Value.ClosePort();
            }
            if (_secsPort != null)
                _secsPort.ClosePort();
        }

        async public void SendAsync(string unitID, string transName, XElement xmlPrimaryMessage)

        {
            try
            {
                if (!string.IsNullOrEmpty(unitID) && secsPortList.ContainsKey(unitID))
                    _secsPort = secsPortList[unitID];
                //else return;
                if (_secsPort.Connected)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            //XElement trans = xmlPrimaryMessage.Element("Primary");
                            SECSTransaction tran = _secsPort.Library.FindTransaction(transName); //new SECSTransaction(stream, function);

                            tran.Primary.Root = XMLToRootSecsItem(xmlPrimaryMessage);
                            tran.Send(_secsPort);
                            //LogInfo.HSMS(string.Format("[Send] {0} : {1}", unitID, tran.Primary));
                            logger.Info(string.Format("[Send] {0} : {1}", unitID, tran.Primary));
                            //_secsPort.SendAsync(transName, xmlPrimaryMessage);
                        }
                        catch (NullReferenceException nullR)
                        {
                            //_logWriter.Write(nullR.Message, "ApplicationException", 0, 8000, System.Diagnostics.TraceEventType.Error, transName + "CustomSendAsync: SECSPort NullReferenceException!");
                        }
                        catch (ArgumentNullException a)
                        {
                            //_logWriter.Write(a.Message, "ApplicationException", 0, 8000, System.Diagnostics.TraceEventType.Error, transName + ":CustomSendAsync Unrecognized Message!");
                        }

                    });
                }
                else
                    DispatchSecondaryInGEMMessage("S99F1", xmlPrimaryMessage);          //Connection Failure
            }
            catch (Exception e)
            {
                //_logWriter.Write(e.Message, "ApplicationException", 0, 8000, System.Diagnostics.TraceEventType.Error, transName + "SendAsync: SecsWellDriver Exception Error!");
            }
        }

        public XElement FindTransaction(string transName)
        {
            return _secsPort.Library.TransToXElement(_secsPort.Library.FindTransaction(transName));
        }

        /// <summary>
        /// Registers the specified SECSII message handler.
        /// </summary>
        public void Register(ISECSIIMessageHandler handler)
        {
            _secsIIMessageDispatcher.Register(handler);
        }

        private void SetHSMSDriver(HostInfo info)
        {
            #region SECSWell Default properties setting           
            _secsPort.PortType = eSECS_PORT_TYPE.HSMS;

            #endregion
            _secsPort.DeviceID = info.DefaultDeviceID;
            #region SECSWell Hsms Properties setting
            _secsPort.HsmsParameters.ConnectionMode = (eHSMS_CONNECT_MODE)Enum.Parse(typeof(eHSMS_CONNECT_MODE), info.ConnectionMode);
            if (_secsPort.HsmsParameters.ConnectionMode == eHSMS_CONNECT_MODE.PASSIVE)
            {
                _secsPort.HsmsParameters.LocalIP = info.LocalIPAddress;
                _secsPort.HsmsParameters.LocalPort = Convert.ToInt32(info.LocalIPPort);
                ConnectionMode = SECSConnectionMode.Passive;
            }
            else
            {
                _secsPort.HsmsParameters.RemoteIP = info.RemoteIPAddress;
                _secsPort.HsmsParameters.RemotePort = Convert.ToInt32(info.RemoteIPPort);
                ConnectionMode = SECSConnectionMode.Active;
            }
            _secsPort.HsmsParameters.T3 = Convert.ToInt32(info.T3);
            _secsPort.HsmsParameters.T5 = Convert.ToInt32(info.T5);
            _secsPort.HsmsParameters.T6 = Convert.ToInt32(info.T6);
            _secsPort.HsmsParameters.T7 = Convert.ToInt32(info.T7);
            _secsPort.HsmsParameters.T8 = Convert.ToInt32(info.T8);
            _secsPort.HsmsParameters.IsHost = true;
            _unitID = info.UnitID;
            #endregion
            string configurFilePath = AppDomain.CurrentDomain.BaseDirectory + "ConfigureFiles\\" + info.SECSLibrary;
            _secsPort.LogLevel = eLOG_LEVEL.SECSII;
            _secsPort.LogPath = AppDomain.CurrentDomain.BaseDirectory + "HSMS";
            _secsPort.Library.Load(configurFilePath);
            _secsPort.OnSECSEvent += OnSECSEventHandler;
        }
        async private void OnSECSEventHandler(SECSPort secsPort, SECSEventType type, SECSTransaction trans, SECSErrors err, string errmsg)
        {
            try
            {
                XElement xmlMessage = secsPort.Library.TransToXElement(trans);
                switch (type)
                {
                    case SECSEventType.PrimaryRcvd:
                        if (err == SECSErrors.None)
                        {
                            if (secsPortList.Count < 1)
                                xmlMessage.Element("Primary").Add(new XElement("UnitName", _unitID));
                            else
                                xmlMessage.Element("Primary").Add(new XElement("UnitName", secsPortList.First(j => j.Value.DeviceID == secsPort.DeviceID).Key));
                            await Task.Factory.StartNew(() => DispatchPrimaryInGEMMessage(secsPort.Name, trans, xmlMessage));

                        }
                        //else
                        //_logWriter.Write(String.Format("Error Code: {0}. {1}", trans.Name, errmsg), "ApplicationException", 0, 8000, System.Diagnostics.TraceEventType.Error, "Primary Recvd Error!");
                        //BOE_CIS20EventSource.Log.ReceiveSECSIIMessage(trans.Name, xmlMessage.Element("Primary").ToString());
                        //_logWriter.Write(xmlMessage.Element("Primary"), "SECSII", 0, 7000, System.Diagnostics.TraceEventType.Information, "Rcvd " + trans.Name);
                        break;
                    case SECSEventType.SecondaryRcvd:
                        string transName = ParseMessageName(trans.Primary.Stream, trans.Secondary.Function);
                        //S9F* SecsDriver 会自动恢复,不用自己处理
                        //if (err == SECSErrors.T3TimeOut && trans.Name == "S1F14")
                        //{
                        //    _dispatchGEMMessage.CommunicationFailure("S1F14", null);
                        //}

                        await Task.Factory.StartNew(() => DispatchSecondaryInGEMMessage(transName, xmlMessage));

                        //_logWriter.Write(xmlMessage.Element("Secondary"), "SECSII", 0, 7000, System.Diagnostics.TraceEventType.Information, "Rcvd " + transName);
                        break;
                    case SECSEventType.PrimarySent:
                        //_logWriter.Write(xmlMessage.Element("Primary"), "SECSII", 0, 7000, System.Diagnostics.TraceEventType.Information, "Sent " + trans.Name);
                        break;
                    case SECSEventType.SecondarySent:
                        //_logWriter.Write(xmlMessage.Element("Secondary"), "SECSII", 0, 7000, System.Diagnostics.TraceEventType.Information, "Reply " + ParseMessageName(trans.Primary.Stream, trans.Secondary.Function));
                        break;
                    case SECSEventType.HSMSDisconnected:
                        DispatchSecondaryInGEMMessage("S99F1", null);
                        //_logWriter.Write("HSMS is Disconnected", "SECSII");
                        break;
                    case SECSEventType.HSMSConnected:
                        DispatchSecondaryInGEMMessage("S1F13", null);
                        //_logWriter.Write("HSMS is Connected", "SECSII");
                        break;
                    case SECSEventType.Error:
                        if (err == SECSErrors.T3TimeOut && trans.Name == "S1F13")
                        {
                            DispatchSecondaryInGEMMessage("S1F14", xmlMessage);
                        }
                        if (err == SECSErrors.ReadError || err == SECSErrors.WriteError)
                            DispatchSecondaryInGEMMessage("S99F2", null);
                        string name = "";
                        if (trans != null)
                        {
                            //if (err == SECSErrors.T3TimeOut)
                            //    name = ParseMessageName(trans.Primary.Stream, trans.Secondary.Function);
                            //else
                            name = trans.Name;
                        }
                        //_logWriter.Write(String.Format(" ErrorCode: {0} {1}", err, errmsg), "ApplicationException", 0, 8000, System.Diagnostics.TraceEventType.Error, name + ": SECSDriver Error!");
                        break;
                }
            }
            catch (NullReferenceException n)
            {
            }
            catch (Exception e)
            {
            }
            //Logging
        }

        private void DispatchPrimaryInGEMMessage(string unitID, SECSTransaction t, XElement xmlTrans)
        {
            if (!t.Name.Contains("S9"))
            {
                try
                {
                    //LogInfo.HSMS("[Receive] " + unitID + " : " + t?.Name + Environment.NewLine + t.Primary);
                    logger.Info("[Receive] " + unitID + " : " + t?.Name + Environment.NewLine + t.Primary);
                    if (!_secsIIMessageDispatcher.ProcessMessage(unitID, xmlTrans, t.Name, t.DeviceID.ToString()))
                        t.Secondary.Function = 0;
                    if (xmlTrans.Element("Secondary") != null)
                    {
                        ReplyAsync(unitID, t, xmlTrans.Element("Secondary"));
                    }
                }
                catch (Exception e)
                {
                }

            }
        }
        private void DispatchSecondaryInGEMMessage(string messageName, XElement xmlMessage)
        {
            try
            {
                switch (messageName)
                {
                    //Communication Establish messages                
                    case "S1F14":
                    case "S1F13":
                    case "S1F2":        //Operator switch Online host accepted: S1F2;
                    case "S1F0":        //Operator switch Online host rejected: S1F0
                    case "S5F2":        //Alarm Report Acknowledge
                    case "S2F18":
                    case "S6F104":      //After Received S6F104, Send S6F11 CEID process end etc.
                    case "S99F1":       //HSMSDisconnected by connection failed 
                    case "S99F2":       //or communication failed
                    case "S1F6":
                    case "S10F2":
                    case "S7F26":
                    case "S7F70":
                    case "S1F4":
                        _secsIIMessageDispatcher.ProcessMessage("SecsWellWrapper", xmlMessage, messageName, "");
                        break;
                    default:
                        _secsIIMessageDispatcher.ProcessMessage("SecsWellWrapper", xmlMessage, messageName, "");
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        async private void ReplyAsync(string unitID, SECSTransaction t, XElement xmlSecondaryMessage)
        {
            if (!string.IsNullOrEmpty(unitID) && secsPortList.ContainsKey(unitID))
                _secsPort = secsPortList[unitID];
            //if (_sECSIILibrary == null) throw new NullReferenceException("SECSLibrary is null!");
            if (_secsPort.Connected == true)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        t.Secondary.Root = XMLToRootSecsItem(xmlSecondaryMessage);
                        t.Reply(_secsPort);
                        //LogInfo.HSMS("[Reply] " + unitID + " : " + t?.Name + Environment.NewLine + t.Secondary);
                        logger.Info("[Reply] " + unitID + " : " + t?.Name + Environment.NewLine + t.Secondary);
                    }
                    catch (ArgumentNullException a)
                    {
                    }

                });
            }
        }

        private SECSItem XMLToRootSecsItem(XElement el)
        {
            SECSItem root;
            if (el.HasElements)
            {
                foreach (XElement i in el.Elements("Item"))
                {
                    //SECSItem item;
                    if (i.Element("Format").Value.ToUpper() == eSECS_FORMAT.LIST.ToString())
                    {
                        root = new SECSItem(eSECS_FORMAT.LIST, i.Element("Name").Value, i.Element("Description").Value);
                        //root.Add(item);                        
                        ParseList(i, ref root);
                        return root;
                        //item.Parent.AddNew(
                    }
                    else
                    {
                        root = new SECSItem((eSECS_FORMAT)Enum.Parse(typeof(eSECS_FORMAT), i.Element("Format").Value.ToString().ToUpper()),
                                                i.Element("Name").Value, i.Element("Description").Value);
                        root.Value = i.Element("Value").Value;
                        return root;
                        //root.Add(item); 
                    }
                }
            }

            root = null;
            return root;
        }
        private void ParseList(XElement el, ref SECSItem item)
        {
            foreach (XElement i in el.Elements("Item"))
            {
                SECSItem subI;
                if (i.Element("Format").Value.ToUpper() == eSECS_FORMAT.LIST.ToString())
                {
                    subI = new SECSItem(eSECS_FORMAT.LIST, i.Element("Name").Value, i.Element("Description").Value);
                    item.Add(subI);
                    ParseList(i, ref subI);
                }
                else
                {
                    subI = new SECSItem((eSECS_FORMAT)Enum.Parse(typeof(eSECS_FORMAT), i.Element("Format").Value.ToString().ToUpper()),
                                                i.Element("Name").Value, i.Element("Description").Value);

                    subI.Value = i.Element("Value").Value;
                    item.Add(subI);
                }
            }
        }

        private string ParseMessageName(int stream, int function)
        {
            StringBuilder transName = new StringBuilder();
            transName.Append("S");
            transName.Append(stream);
            transName.Append("F");
            transName.Append(function);
            return transName.ToString();
        }
        private XElement GetPrimaryElemnt()
        {
            return null;
        }

    }
}
