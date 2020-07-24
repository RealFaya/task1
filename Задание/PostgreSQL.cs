using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Задание
{
	class PostgreSQL : IDisposable
	{
		NpgsqlConnection Connection;

		public PostgreSQL() { }

		/// <summary>
		/// Подключение к базе
		/// </summary>
		/// <param name="stringConnection">Строка подключения к базе</param>
		public void Connecting(string stringConnection)
		{
			if(Connection != null)
			{
				Dispose();
			}

			Connection = new NpgsqlConnection(stringConnection);
			Connection.Open();
		}

		/// <summary>
		/// Получение данных в DataTable
		/// </summary>
		/// <param name="Query"></param>
		/// <returns></returns>
		public DataTable GetDataTable(string Query)
		{
			using(NpgsqlCommand command = new NpgsqlCommand(Query, Connection))
			{
				DataTable Result = new DataTable();
				NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();

				dataAdapter.SelectCommand = command;
				dataAdapter.Fill(Result);

				return Result;
			}
		}

		/// <summary>
		/// Освобождение данных
		/// </summary>
		public void Dispose()
		{
			if(Connection.State == ConnectionState.Open)
			{
				Connection.Close();
			}

			Connection = null;
		}
	}
}