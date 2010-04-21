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

		[ConfigurationProperty("allowHeaders")]
		public AllowHeaderElementCollection AllowHeaderList
		{
			get
			{
				return this["allowHeaders"] as AllowHeaderElementCollection;
			}
		}

		[ConfigurationProperty("allowCredentials")]
		public AllowCredentialsElement AllowCredentials
		{
			get
			{
				return this["allowCredentials"] as AllowCredentialsElement;
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

	[ConfigurationCollection(typeof(AllowHeaderElement), AddItemName = "header")]
	public class AllowHeaderElementCollection : ConfigurationElementCollection
	{
		public AllowHeaderElement this[int index]
		{
			get
			{
				return BaseGet(index) as AllowHeaderElement;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new AllowHeaderElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((AllowHeaderElement)element).Name;
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

	public class AllowHeaderElement : ConfigurationElement
	{
		[ConfigurationProperty("name")]
		public string Name
		{
			get
			{
				return (string)this["name"];
			}
		}
	}

	public class AllowCredentialsElement : ConfigurationElement
	{
		[ConfigurationProperty("allow")]
		public string Allow
		{
			get
			{
				return (string)this["allow"];
			}
		}
	}
}
