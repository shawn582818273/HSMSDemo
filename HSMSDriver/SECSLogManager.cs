using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace HSMSDriver
{
	internal sealed class SECSLogManager
	{
		private XmlElement elementLog4net;

		private static readonly Lazy<SECSLogManager> lazy = new Lazy<SECSLogManager>(() => new SECSLogManager());

		private List<string> namelist = new List<string>();

		private XmlDocument xmlDocument = new XmlDocument();

		public static SECSLogManager Instance
		{
			get
			{
				SECSLogManager result;
				lock (SECSLogManager.lazy)
				{
					if (SECSLogManager.lazy != null)
					{
						result = SECSLogManager.lazy.Value;
					}
					else
					{
						result = new SECSLogManager();
					}
				}
				return result;
			}
		}

		private SECSLogManager()
		{
			this.xmlDocument.AppendChild(this.xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement newChild = this.xmlDocument.CreateElement("configuration");
			this.xmlDocument.AppendChild(newChild);
			this.elementLog4net = this.xmlDocument.CreateElement("log4net");
			newChild.AppendChild(this.elementLog4net);
			XmlElement element2 = this.xmlDocument.CreateElement("root");
			XmlElement element3 = this.xmlDocument.CreateElement("level");
			XmlAttribute node = this.xmlDocument.CreateAttribute("value");
			node.Value = "DEBUG";
			element3.Attributes.Append(node);
			element2.AppendChild(element3);
			this.elementLog4net.AppendChild(element2);
		}

		private XmlElement CreateAppenderRef(string value)
		{
			XmlElement element = this.xmlDocument.CreateElement("appender-ref");
			element.Attributes.Append(this.CreateAttribute("ref", value));
			return element;
		}

		private XmlAttribute CreateAttribute(string name, string value)
		{
			XmlAttribute attribute = this.xmlDocument.CreateAttribute(name);
			attribute.Value = value;
			return attribute;
		}

		private XmlElement CreateDEBUGAppender(string name, string path, eLOG_LEVEL level)
		{
			XmlElement xmlAppender = this.xmlDocument.CreateElement("appender");
			xmlAppender.Attributes.Append(this.CreateAttribute("name", name + "-DEBUG"));
			xmlAppender.Attributes.Append(this.CreateAttribute("type", "log4net.Appender.RollingFileAppender"));
			if (level == eLOG_LEVEL.DEBUG)
			{
				this.InitializeAppender(xmlAppender, Path.Combine(path, "DEBUG"), "ERROR", "DEBUG");
				return xmlAppender;
			}
			this.InitializeAppender(xmlAppender, Path.Combine(path, "DEBUG"), "DEBUG", "DEBUG");
			return xmlAppender;
		}

		private XmlElement CreateERRORAppender(string name, string path)
		{
			XmlElement xmlAppender = this.xmlDocument.CreateElement("appender");
			xmlAppender.Attributes.Append(this.CreateAttribute("name", name + "-ERROR"));
			xmlAppender.Attributes.Append(this.CreateAttribute("type", "log4net.Appender.RollingFileAppender"));
			this.InitializeAppender(xmlAppender, Path.Combine(path, "ERROR"), "ERROR", "ERROR");
			return xmlAppender;
		}

		private XmlElement CreateLogger(string name, string path, string level)
		{
			XmlElement element = this.xmlDocument.CreateElement("logger");
			element.Attributes.Append(this.CreateAttribute("name", name));
			XmlElement newChild = this.xmlDocument.CreateElement("additivity");
			newChild.Attributes.Append(this.CreateAttribute("value", "false"));
			element.AppendChild(newChild);
			element.AppendChild(this.CreateAppenderRef(name + "-SECSI"));
			element.AppendChild(this.CreateAppenderRef(name + "-SECSII"));
			element.AppendChild(this.CreateAppenderRef(name + "-DEBUG"));
			element.AppendChild(this.CreateAppenderRef(name + "-ERROR"));
			XmlElement element2 = this.xmlDocument.CreateElement("Level");
			element2.Attributes.Append(this.CreateAttribute("value", level));
			element.AppendChild(element2);
			return element;
		}

		public void CreateNewLogger(SECSLogConfigure configure)
		{
			if (!this.namelist.Contains(configure.Name))
			{
				this.namelist.Add(configure.Name);
				this.elementLog4net.AppendChild(this.CreateLogger(configure.Name, configure.LogPath, this.GetLogLevel(configure.LogLevel)));
				this.elementLog4net.AppendChild(this.CreateSECSIAppender(configure.Name, configure.LogPath));
				this.elementLog4net.AppendChild(this.CreateSECSIIAppender(configure.Name, configure.LogPath));
				this.elementLog4net.AppendChild(this.CreateDEBUGAppender(configure.Name, configure.LogPath, configure.LogLevel));
				this.elementLog4net.AppendChild(this.CreateERRORAppender(configure.Name, configure.LogPath));
				if (LogManager.GetAllRepositories().FirstOrDefault((ILoggerRepository r) => r.Name == "SECSwell") == null)
				{
					XmlConfigurator.Configure(LogManager.CreateRepository("SECSwell"), this.elementLog4net);
					return;
				}
				XmlConfigurator.Configure(LogManager.GetRepository("SECSwell"), this.elementLog4net);
			}
		}

		private XmlElement CreateParameter(string name, string value)
		{
			XmlElement element = this.xmlDocument.CreateElement("param");
			XmlAttribute node = this.xmlDocument.CreateAttribute("name");
			node.Value = name;
			XmlAttribute attribute2 = this.xmlDocument.CreateAttribute("value");
			attribute2.Value = value;
			element.Attributes.Append(node);
			element.Attributes.Append(attribute2);
			return element;
		}

		private XmlElement CreateSECSIAppender(string name, string path)
		{
			XmlElement xmlAppender = this.xmlDocument.CreateElement("appender");
			xmlAppender.Attributes.Append(this.CreateAttribute("name", name + "-SECSI"));
			xmlAppender.Attributes.Append(this.CreateAttribute("type", "log4net.Appender.RollingFileAppender"));
			this.InitializeAppender(xmlAppender, Path.Combine(path, "SECSI"), "INFO", "INFO");
			return xmlAppender;
		}

		private XmlElement CreateSECSIIAppender(string name, string path)
		{
			XmlElement xmlAppender = this.xmlDocument.CreateElement("appender");
			xmlAppender.Attributes.Append(this.CreateAttribute("name", name + "-SECSII"));
			xmlAppender.Attributes.Append(this.CreateAttribute("type", "log4net.Appender.RollingFileAppender"));
			this.InitializeAppender(xmlAppender, Path.Combine(path, "SECSII"), "WARN", "WARN");
			return xmlAppender;
		}

		private string GetLogLevel(eLOG_LEVEL level)
		{
			if (level == eLOG_LEVEL.ERROR)
			{
				return "ERROR";
			}
			if (level == eLOG_LEVEL.SECSII)
			{
				return "WARN";
			}
			if (level != eLOG_LEVEL.SECSI && level == eLOG_LEVEL.DEBUG)
			{
				return "DEBUG";
			}
			return "INFO";
		}

		private void InitializeAppender(XmlElement xmlAppender, string path, string maxloglevel, string minloglevel)
		{
			XmlAttribute node = this.xmlDocument.CreateAttribute("type");
			node.Value = "log4net.Appender.RollingFileAppender";
			xmlAppender.Attributes.Append(node);
			xmlAppender.AppendChild(this.CreateParameter("File", path));
			xmlAppender.AppendChild(this.CreateParameter("AppendToFile", "true"));
			xmlAppender.AppendChild(this.CreateParameter("StaticLogFileName", "false"));
			xmlAppender.AppendChild(this.CreateParameter("DatePattern", "/yyyy-MM-dd/yyyyMMddHH\".txt\""));
			xmlAppender.AppendChild(this.CreateParameter("MaxSizeRollBackups", "50"));
			xmlAppender.AppendChild(this.CreateParameter("RollingStyle", "Date"));
			XmlElement newChild = this.xmlDocument.CreateElement("filter");
			newChild.Attributes.Append(this.CreateAttribute("type", "log4net.Filter.LevelRangeFilter"));
			newChild.AppendChild(this.CreateParameter("LevelMin", minloglevel));
			newChild.AppendChild(this.CreateParameter("LevelMax", maxloglevel));
			xmlAppender.AppendChild(newChild);
			XmlElement element2 = this.xmlDocument.CreateElement("layout");
			element2.Attributes.Append(this.CreateAttribute("type", "log4net.Layout.PatternLayout,log4net"));
			element2.AppendChild(this.CreateParameter("ConversionPattern", "%d %m%n"));
			xmlAppender.AppendChild(element2);
		}
	}
}
