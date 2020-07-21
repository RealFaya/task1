using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;

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

		public Dictionary<string, decimal> GetTableAndSize(string Query, ref decimal Size)
		{
			decimal TableSize;
			Dictionary<string, decimal> Result = new Dictionary<string, decimal>();

			using(NpgsqlDataReader Reader = ExecuteReader(Query))
			{
				foreach(DbDataRecord bdElement in Reader)
				{
					TableSize = bdElement.GetDecimal(1);
					Size -= TableSize;

					Result.Add(bdElement.GetString(0), TableSize);
				}
			}

			return Result;
		}

		public void Dispose()
		{
			Connection.Close();
			Connection = null;
		}
	}
}
