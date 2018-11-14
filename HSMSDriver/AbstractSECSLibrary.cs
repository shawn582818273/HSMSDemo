using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HSMSDriver
{
	internal abstract class AbstractSECSLibrary
	{
		private string desc;

		private string name;

		public string Description
		{
			get
			{
				return this.desc;
			}
			set
			{
				this.desc = value;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		protected AbstractSECSLibrary() : this("TTSKY AbstractSECSLibrary", "TTSKY SECS Default Libray")
		{
		}

		protected AbstractSECSLibrary(string name, string desc)
		{
			this.name = "";
			this.desc = "";
			this.Name = name;
			this.Description = desc;
		}

		public abstract void AddTransaction(SECSTransaction trans);

		internal abstract bool CheckSecsItemFormat(ref SECSItem rcvd, ref SECSItem format);

		public static AbstractSECSLibrary CreateSECSLibrary(string filename)
		{
			XDocument document = XDocument.Load(filename);
			if (document.Element("SECSLibrary") == null)
			{
				if (document.Element("Library") != null)
				{
					WinAbstractSECSLibrary library2 = new WinAbstractSECSLibrary();
					if (library2.Load(filename))
					{
						return library2;
					}
				}
				return null;
			}
			MessageAbstractSECSLibrary library3 = new MessageAbstractSECSLibrary();
			if (library3.Load(filename))
			{
				return library3;
			}
			return null;
		}

		internal abstract bool FindFunction(int stream, int function);

		public abstract SECSMessage FindMessage(string name);

		internal abstract List<SECSMessage> FindMessage(int stream, int function);

		internal abstract bool FindStream(int stream);

		public abstract SECSTransaction FindTransaction(string name);

		public abstract bool Load(string filename);

		public abstract void RemoveTransaction(string name);

		public abstract void Save(string filename);

		public abstract XElement SECSItemToXElement(SECSItem root);

		public abstract XElement TransToXElement(SECSTransaction trans);

		public abstract string TransToXml(SECSTransaction trans);

		public abstract SECSItem XElementToSECSItem(XElement element);

		public abstract SECSTransaction XElementToTrans(XElement element);

		public abstract SECSTransaction XmlToTrans(string xmlnode);
	}
}
