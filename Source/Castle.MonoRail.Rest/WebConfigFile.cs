using System;
using System.Collections.Specialized;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Rest
{
	public static class WebConfigFile
	{
		public static NameValueCollection GetAllowConfigSettings(IRequest request)
		{
			var settingValue = string.Empty;
			var allowSettings = new NameValueCollection();
			MonoRailRestConfigSection configSection;
			try
			{
				configSection = MonoRailRestConfigSection.Instance;
			}
			catch (Exception)
			{
				configSection = null;
			}
			if (configSection != null)
			{
				SetAllowOrigin(request, configSection, allowSettings);
				SetAllowHeaders(request, configSection, allowSettings);
				SetAllowCredentials(configSection, allowSettings);
			}

			return allowSettings;
		}

		private static void SetAllowOrigin(IRequest request, MonoRailRestConfigSection configSection, NameValueCollection allowSettings)
		{
			foreach (AllowOriginElement allowOrigin in configSection.AllowOriginList)
			{
				if (string.Compare(allowOrigin.Domain, request.Headers["Origin"], true) == 0)
				{
					allowSettings["Origin"] = allowOrigin.Domain;
					break;
				}
			}
		}

		private static void SetAllowHeaders(IRequest request, MonoRailRestConfigSection configSection, NameValueCollection allowSettings)
		{
			var count = 0;
			foreach (AllowHeaderElement allowHeader in configSection.AllowHeaderList)
			{
				var accessControlHeaderString = request.Headers.Get("Access-Control-Request-Headers");
				if (!string.IsNullOrEmpty(accessControlHeaderString))
				{
					allowSettings["Header"] += allowHeader.Name;
					if (count < configSection.AllowHeaderList.Count - 1)
					{
						allowSettings["Header"] += ", ";
					}
					count++;
				}
			}
		}

		private static void SetAllowCredentials(MonoRailRestConfigSection configSection, NameValueCollection allowSettings)
		{
			bool allowCredentials;
			if (!string.IsNullOrEmpty(configSection.AllowCredentials.Allow)
				&& bool.TryParse(configSection.AllowCredentials.Allow, out allowCredentials))
			{
				allowSettings["Credentials"] = configSection.AllowCredentials.Allow;
			}
		}
	}

}
