using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Timers;

namespace Задание
{
	class Program
	{
		static Timer timer;
		static readonly int Interval = 20;

		static Propertie prop = new Propertie();
		static gGoogle google = new gGoogle(prop);

		static readonly string Query = "select datname, (pg_database_size(datname)  / 1024.0 / 1024 / 1024) as size from pg_database";

		static void Main(string[] args)
		{
			GoGoogle();
			Console.WriteLine("Данные обновлены.\nОбновление данных произойдет через {0} с.\nЧтобы его отменить нажмите любую клавишу.", Interval);
			StartTimer();
			Console.ReadKey();

			timer.Stop();
			timer.Dispose();

			Console.WriteLine("Обновление данных приостановлено.");
			Console.ReadKey();
		}
		
		/// <summary>
		/// Инициализация и запуск таймера
		/// </summary>
		static void StartTimer()
		{
			timer = new Timer(Interval * 1000);
			timer.Elapsed += Timer_Elapsed;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		/// <summary>
		/// Событие по истечению времени таймера
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			GoGoogle();
			Console.WriteLine("Данные обновлены.");
		}

		/// <summary>
		/// Запуск обновления данных в таблице
		/// </summary>
		static void GoGoogle()
		{
			using(PostgreSQL psql = new PostgreSQL())
			{
				Dictionary<string, double> Result;

				foreach(KeyValuePair<string, Entity> element in prop.props)
				{
					psql.Connecting(element.Value.ConnectingString);

					Result = GetTableAndSize(psql.GetDataTable(Query));

					google.FillSheet(element.Key,
						element.Value.DiskSize - Result.Sum(x => x.Value),
						Result);
				}
			}
		}

		/// <summary>
		/// Получение из DataTable Название таблиц и их размер
		/// </summary>
		/// <param name="Table"></param>
		/// <returns></returns>
		static Dictionary<string, double> GetTableAndSize(DataTable Table)
		{
			Dictionary<string, double> Result = new Dictionary<string, double>();

			foreach(DataRow element in Table.Rows)
			{
				Result.Add((string)element["datname"], (double)Math.Round((decimal)element["size"], 1));
			}

			return Result;
		}
	}
}