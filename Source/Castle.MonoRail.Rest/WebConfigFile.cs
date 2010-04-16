using System;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Rest
{
	public static class WebConfigFile
	{
		public static string GetAllowOriginConfigSetting(IRequest request)
		{
			var settingValue = string.Empty;
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
				foreach (AllowOriginElement allowOrigin in configSection.AllowOriginList)
				{
					if (allowOrigin.Domain.ToLower() == request.Headers["Origin"].ToLower())
					{
						settingValue = allowOrigin.Domain;
						break;
					}
				}
			}

			return settingValue;
		}
	}
}
