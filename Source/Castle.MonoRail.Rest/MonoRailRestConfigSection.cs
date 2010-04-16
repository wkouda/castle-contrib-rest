using System.Collections.Generic;
using System.Configuration;
using Castle.Core.Configuration;

namespace Castle.MonoRail.Rest
{
	public class MonoRailRestConfigSection : ConfigurationSection
	{
		private static MonoRailRestConfigSection instance = null;
		public static MonoRailRestConfigSection Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (MonoRailRestConfigSection)ConfigurationManager.GetSection("monorail.rest");
				}
				return instance;
			}
		}

		[ConfigurationProperty("allowOrigins")]
		public AllowOriginElementCollection AllowOriginList
		{
			get
			{
				return this["allowOrigins"] as AllowOriginElementCollection;
			}
		}
	}

	[ConfigurationCollection(typeof(AllowOriginElement), AddItemName = "origin")]
	public class AllowOriginElementCollection : ConfigurationElementCollection
	{
		public AllowOriginElement this[int index]
		{
			get
			{
				return BaseGet(index) as AllowOriginElement;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new AllowOriginElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((AllowOriginElement)element).Domain;
		}
	}

	public class AllowOriginElement : ConfigurationElement
	{
		[ConfigurationProperty("domain")]
		public string Domain
		{
			get
			{
				return (string)this["domain"];
			}
		}
	}
}
