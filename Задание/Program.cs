using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace Задание
{
	class Program
	{
		static void Main(string[] args)
		{
			using(PostgreSQL psql = new PostgreSQL())
			{
				Properties prop = new Properties();
				decimal TableSize, Size;
				string Query = "select datname, pg_database_size(datname) / 1024.0 / 1024 / 1024 from pg_database";

				foreach(KeyValuePair<string, object[]> element in prop.props)
				{
					Size = (decimal)element.Value[0];
					psql.Connecting((string)element.Value[1]);

					using(NpgsqlDataReader Reader = psql.ExecuteReader(Query))
					{
						foreach(DbDataRecord bdElement in Reader)
						{
							TableSize = bdElement.GetDecimal(1);
							Size -= TableSize;

							Console.WriteLine(string.Format("{0} | {1} - {2:N1}", element.Key, bdElement.GetString(0), TableSize));
						}
					}

					Console.WriteLine(string.Format("{0} | Свободно: {1:N1}", element.Key, Size));
				}
			}
			
			Console.ReadKey();
		}
	}
}