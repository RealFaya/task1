using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Задание
{
	class Propertie
	{
		public Dictionary<string, Entity<double>> props;

		public Propertie()
		{
			props = new Dictionary<string, Entity<double>>();

			LoadConnectStrings(ConfigurationManager.AppSettings, ConfigurationManager.ConnectionStrings);
		}

		/// <summary>
		/// Запись строк подключения к бд и объема диска из конфигурационного файла
		/// </summary>
		void LoadConnectStrings(NameValueCollection AppSettings, ConnectionStringSettingsCollection ConnectionStrings)
		{
			string Name;

			for(int i = 1; i < ConnectionStrings.Count; i++)
			{
				Name = ConnectionStrings[i].Name;
				props.Add(Name, new Entity<double>(ConnectionStrings[i].ConnectionString, Convert.ToDouble(AppSettings.Get(Name).Replace('.', ','))));
			}
		}

		/// <summary>
		/// Получение пользователя из поля User
		/// </summary>
		/// <returns></returns>
		public static string GetUser()
		{
			return ConfigurationManager.AppSettings.Get("User");
		}
	}
}
