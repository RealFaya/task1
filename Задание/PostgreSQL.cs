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

			ConnectionString = stringConnection;
			Connection = new NpgsqlConnection(stringConnection);
			Connection.Open();
		}

		/// <summary>
		/// Получение имен таблиц и размера
		/// </summary>
		/// <param name="Query">Запрос</param>
		/// <param name="Size">Объем места на диске</param>
		/// <returns>Имя таблицы - размер таблицы</returns>
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

		/// <summary>
		/// Получение данных с базы
		/// </summary>
		/// <param name="Query">Запрос</param>
		/// <returns>Результат запроса</returns>
		NpgsqlDataReader ExecuteReader(string Query)
		{
			using(NpgsqlCommand command = new NpgsqlCommand(Query, Connection))
			{
				NpgsqlDataReader reader = command.ExecuteReader();

				return reader;
			}
		}

		/// <summary>
		/// Освобождение данных
		/// </summary>
		public void Dispose()
		{
			Connection.Close();
			Connection = null;
		}
	}
}
