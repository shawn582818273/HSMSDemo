using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HSMSDriver
{
	public sealed class SECSLibrary
	{
		private AbstractSECSLibrary library;

		public string Description
		{
			get
			{
				return this.library.Description;
			}
			set
			{
				this.library.Description = value;
			}
		}

		public string Name
		{
			get
			{
				return this.library.Name;
			}
			set
			{
				this.library.Name = value;
			}
		}

		public IList<SECSMessage> SECSMessageList
		{
			get
			{
				if (this.library is MessageAbstractSECSLibrary)
				{
					MessageAbstractSECSLibrary library = this.library as MessageAbstractSECSLibrary;
					return library.SECSMessageList;
				}
				List<SECSMessage> list = new List<SECSMessage>();
				WinAbstractSECSLibrary library2 = this.library as WinAbstractSECSLibrary;
				if (library2 == null)
				{
					return null;
				}
				foreach (KeyValuePair<string, SECSTransaction> pair in library2.TransList)
				{
					if (pair.Value.Primary != null)
					{
						list.Add(pair.Value.Primary);
					}
					if (pair.Value.Secondary != null)
					{
						list.Add(pair.Value.Secondary);
					}
				}
				return list;
			}
		}

		public IList<SECSTransaction> TransactionList
		{
			get
			{
				if (this.library is MessageAbstractSECSLibrary)
				{
					return null;
				}
				WinAbstractSECSLibrary library = this.library as WinAbstractSECSLibrary;
				List<SECSTransaction> list = new List<SECSTransaction>();
				foreach (KeyValuePair<string, SECSTransaction> pair in library.TransList)
				{
					list.Add(pair.Value);
				}
				return list;
			}
		}

		public SECSLibrary() : this("TTSKY AbstractSECSLibrary", "TTSKY SECS Default Libray")
		{
		}

		public SECSLibrary(string name, string desc)
		{
			this.library = new WinAbstractSECSLibrary();
			this.Name = name;
			this.Description = desc;
		}

		public void AddTransaction(SECSTransaction trans)
		{
			this.library.AddTransaction(trans);
		}

		internal bool CheckSecsItemFormat(ref SECSItem rcvd, ref SECSItem format)
		{
			return this.library.CheckSecsItemFormat(ref rcvd, ref format);
		}

		public bool FindFunction(int stream, int function)
		{
			return this.library.FindFunction(stream, function);
		}

		public SECSMessage FindMessage(string name)
		{
			return this.library.FindMessage(name);
		}

		public List<SECSMessage> FindMessage(int stream, int function)
		{
			return this.library.FindMessage(stream, function);
		}

		public bool FindStream(int stream)
		{
			return this.library.FindStream(stream);
		}

		public SECSTransaction FindTransaction(string name)
		{
			return this.library.FindTransaction(name);
		}

		public bool Load(string filename)
		{
			bool result;
			try
			{
				AbstractSECSLibrary library = AbstractSECSLibrary.CreateSECSLibrary(filename);
				if (library != null)
				{
					library.Name = this.library.Name;
					library.Description = this.library.Description;
					this.library = library;
					result = true;
				}
				else
				{
					result = false;
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		public void RemoveTransaction(string name)
		{
			this.library.RemoveTransaction(name);
		}

		public void Save(string filename)
		{
			this.library.Save(filename);
		}

		public XElement SECSItemToXElement(SECSItem root)
		{
			return this.library.SECSItemToXElement(root);
		}

		public XElement TransToXElement(SECSTransaction trans)
		{
			return this.library.TransToXElement(trans);
		}

		public string TransToXml(SECSTransaction trans)
		{
			return this.library.TransToXml(trans);
		}

		public SECSItem XElementToSECSItem(XElement element)
		{
			return this.library.XElementToSECSItem(element);
		}

		public SECSTransaction XElementToTrans(XElement element)
		{
			return this.library.XElementToTrans(element);
		}

		public SECSTransaction XmlToTrans(string xmlnode)
		{
			return this.library.XmlToTrans(xmlnode);
		}
	}
}
