using System;

namespace HSMSDriver
{
    internal class SECSLogConfigure
    {
        private eLOG_LEVEL loglevel = eLOG_LEVEL.SECSII;

        private string path = AppDomain.CurrentDomain.BaseDirectory;

        public eLOG_LEVEL LogLevel
        {
            get
            {
                return this.loglevel;
            }
            set
            {
                this.loglevel = value;
            }
        }

        public string LogPath
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        public string Name
        {
            get;
            set;
        }
    }
}
