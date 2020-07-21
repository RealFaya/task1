using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Задание
{
	class Properties
	{
		public Dictionary<string, object[]> props;

		public Properties()
		{
			props = new Dictionary<string, object[]>();

			LoadConnectStrings(ConfigurationManager.AppSettings, ConfigurationManager.ConnectionStrings);
		}

		/// <summary>
		/// Запись строк подключения к бд и объема диска
		/// </summary>
		public void LoadConnectStrings(NameValueCollection AppSettings, ConnectionStringSettingsCollection ConnectionStrings)
		{
			string Name;

			for(int i = 1; i < ConnectionStrings.Count; i++)
			{
				Name = ConnectionStrings[i].Name;
				props.Add(Name, new object[] { Convert.ToDecimal(AppSettings.Get(Name).Replace('.', ',')), ConnectionStrings[i].ConnectionString });
			}
		}
	}
}
