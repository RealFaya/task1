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
		/// Получение имен таблиц и размера
		/// </summary>
		/// <param name="Query">Запрос</param>
		/// <param name="Size">Объем места на диске</param>
		/// <returns>Имя таблицы - размер таблицы</returns>
		public Dictionary<string, double> GetTableAndSize(string Query, ref double Size)
		{
			double TableSize;
			Dictionary<string, double> Result = new Dictionary<string, double>();

			using(NpgsqlDataReader Reader = ExecuteReader(Query))
			{
				foreach(DbDataRecord bdElement in Reader)
				{
					TableSize = Convert.ToDouble(bdElement.GetDecimal(1));
					Size -= TableSize;

					Result.Add(bdElement.GetString(0), TableSize);
				}
			}

			return Result;
		}

		/// <summary>
		/// Позучение данных в DataTable
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
			if(Connection.State == ConnectionState.Open)
			{
				Connection.Close();
			}

			Connection = null;
		}
	}
}
