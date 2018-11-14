using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ComponentModel;

namespace SecsDriverWrapper
{
    public enum EQPOrHost
    {
        HOST,
        EQP,
    }
    public class HostInfo : INotifyPropertyChanged
    {
        #region Primitive Property  

        private string _secsLibXMLFile;

        public string SECSLibrary
        {
            get { return _secsLibXMLFile; }
            set { _secsLibXMLFile = value; }
        }

        private short _defaultDeviceID;

        public short DefaultDeviceID
        {
            get { return _defaultDeviceID; }
            set { _defaultDeviceID = value; }
        }

        /// <summary>
        /// Active, Passive, Alternating
        /// </summary>
        private string _connectionMode;

        public string ConnectionMode
        {
            get { return _connectionMode; }
            set
            {
                if (_connectionMode != value)
                {
                    _connectionMode = value;
                    OnPropertyChanged(_connectionMode);
                }
            }
        }
        private string _localIPAddress;

        public string LocalIPAddress
        {
            get { return _localIPAddress; }
            set
            {
                if (_localIPAddress != value)
                {

                    _localIPAddress = value;
                    OnPropertyChanged(_localIPAddress);
                }
            }
        }
        private uint _localIPPort;

        public uint LocalIPPort
        {
            get { return _localIPPort; }
            set
            {
                if (_localIPPort != value)
                {
                    _localIPPort = value;
                    OnPropertyChanged(_localIPPort.ToString());
                }
            }
        }
        private string _remoteIPAddress;

        public string RemoteIPAddress
        {
            get { return _remoteIPAddress; }
            set
            {
                if (_remoteIPAddress != value)
                {
                    _remoteIPAddress = value;
                    OnPropertyChanged(_remoteIPAddress);
                }
            }
        }
        private uint _remoteIPPort;

        public uint RemoteIPPort
        {
            get { return _remoteIPPort; }
            set
            {
                if (_remoteIPPort != value)
                {

                    _remoteIPPort = value;
                    OnPropertyChanged(_remoteIPPort.ToString());
                }
            }
        }
        private string _unitID;

        public string UnitID
        {
            get { return _unitID; }
            set
            {
                if (_unitID != value)
                {
                    _unitID = value;
                    OnPropertyChanged(_unitID);
                }
            }
        }
        //public EQPOrHost 
        private uint _t3;

        public uint T3
        {
            get { return _t3; }
            set { _t3 = value; }
        }
        private uint _t5;

        public uint T5
        {
            get { return _t5; }
            set { _t5 = value; }
        }
        private uint _t6;

        public uint T6
        {
            get { return _t6; }
            set { _t6 = value; }
        }
        private uint _t7;

        public uint T7
        {
            get { return _t7; }
            set { _t7 = value; }
        }
        private uint _t8;

        public uint T8
        {
            get { return _t8; }
            set { _t8 = value; }
        }
        #endregion
        public HostInfo()
        {
        }
        public HostInfo(XElement xml)
        {
            this.SECSLibrary = xml.Element("SECSLibrary").Value;
            this.DefaultDeviceID = short.Parse(xml.Element("DefaultDeviceID").Value);
            this.ConnectionMode = xml.Element("ConnectionMode").Value;
            this.LocalIPAddress = xml.Element("LocalIPAddress").Value;
            this.LocalIPPort = uint.Parse(xml.Element("LocalIPPort").Value);
            this.RemoteIPAddress = xml.Element("RemoteIPAddress").Value;
            this.RemoteIPPort = uint.Parse(xml.Element("RemoteIPPort").Value);
            this.UnitID = xml.Element("UnitID").Value;
            this.T3 = uint.Parse(xml.Element("T3").Value);
            this.T5 = uint.Parse(xml.Element("T5").Value);
            this.T6 = uint.Parse(xml.Element("T6").Value);
            this.T7 = uint.Parse(xml.Element("T7").Value);
            this.T8 = uint.Parse(xml.Element("T8").Value);
        }
        public HostInfo(string configFile)
        {
            ReadConfigure(configFile);
            _configureSource = configFile;

        }

        private string _configureSource;

        public void SaveHostInfo()
        {
            XElement writeFile = XElement.Load(_configureSource);

            writeFile.SetElementValue("DefaultDeviceID", _defaultDeviceID);
            XElement sub = writeFile.Element("HSMS");
            sub.SetElementValue("ConnectionMode", _connectionMode);
            sub.SetElementValue("LocalIPAddress", _localIPAddress);
            sub.SetElementValue("LocalIPPort", _localIPPort);
            sub.SetElementValue("RemoteIPAddress", _remoteIPAddress);
            sub.SetElementValue("RemoteIPPort", _remoteIPPort);
            sub.SetElementValue("T3", _t3);
            sub.SetElementValue("T5", _t5);
            sub.SetElementValue("T6", _t6);
            sub.SetElementValue("T7", _t7);
            sub.SetElementValue("T8", _t8);
        }
        /// <summary>
        /// we use xml to store host information
        /// </summary>
        /// <param name="configFile"></param>
        private void ReadConfigure(string configFile)
        {
            XElement conf = XElement.Load(configFile);
            _defaultDeviceID = Convert.ToInt16(conf.Element("DefaultDeviceID").Value);
            _secsLibXMLFile = conf.Element("SECSLibrary").Value;
            _connectionMode = conf.Element("HSMS").Element("ConnectionMode").Value.ToUpper();
            _localIPAddress = conf.Element("HSMS").Element("LocalIPAddress").Value;
            _localIPPort = Convert.ToUInt32(conf.Element("HSMS").Element("LocalIPPort").Value.ToString());
            _remoteIPAddress = conf.Element("HSMS").Element("RemoteIPAddress").Value;
            _remoteIPPort = Convert.ToUInt32(conf.Element("HSMS").Element("RemoteIPPort").Value.ToString());
            _unitID = conf.Element("HSMS").Element("UnitID").Value;
            _t3 = Convert.ToUInt32(conf.Element("HSMS").Element("T3").Value.ToString());
            _t5 = Convert.ToUInt32(conf.Element("HSMS").Element("T5").Value.ToString());
            _t6 = Convert.ToUInt32(conf.Element("HSMS").Element("T6").Value.ToString());
            _t7 = Convert.ToUInt32(conf.Element("HSMS").Element("T7").Value.ToString());
            _t8 = Convert.ToUInt32(conf.Element("HSMS").Element("T8").Value.ToString());
        }

        private void OnPropertyChanged(string property)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
        private event PropertyChangedEventHandler _propertyChanged;
    }
}
