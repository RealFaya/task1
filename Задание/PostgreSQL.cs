using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Задание
{
	class PostgreSQL : IDisposable
	{
		NpgsqlConnection Connection;
		public string ConnectionString;

		public PostgreSQL() { }

		public void Connecting(string stringConnection)
		{
			if(Connection != null)
			{
				Dispose();
			}

			ConnectionString = stringConnection;
			Connection = new NpgsqlConnection(stringConnection);
			Connection.Open();
		}

		public NpgsqlDataReader ExecuteReader(string Query)
		{
			using(NpgsqlCommand command = new NpgsqlCommand(Query, Connection))
			{
				NpgsqlDataReader reader = command.ExecuteReader();

				return reader;
			}
		}

		public void Dispose()
		{
			Connection.Close();
			Connection = null;
		}
	}
}
