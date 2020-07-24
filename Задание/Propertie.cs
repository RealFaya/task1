using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Задание
{
	class Propertie
	{
		public static string User;
		public Dictionary<string, Entity> props = new Dictionary<string, Entity>();

		public Propertie()
		{
			NameValueCollection AppSettings = ConfigurationManager.AppSettings;

			User = AppSettings.Get("User");
			LoadConfig(AppSettings, ConfigurationManager.ConnectionStrings);
		}

		/// <summary>
		/// Запись строк подключения к бд и объема диска из конфигурационного файла
		/// </summary>
		void LoadConfig(NameValueCollection AppSettings, ConnectionStringSettingsCollection Connectiongs)
		{
			string Name;
			
			for(int i = 1; i < Connectiongs.Count; i++)
			{
				Name = Connectiongs[i].Name;

				if(!props.ContainsKey(Name))
				{
					props.Add(Name, new Entity(Connectiongs[i].ConnectionString, Convert.ToDouble(AppSettings.Get(Name).Replace('.', ','))));
				}
			}
		}
	}
}
