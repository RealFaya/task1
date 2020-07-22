using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Timers;

namespace Задание
{
	class Program
	{
		static Timer timer;
		static readonly int Interval = 10;

		static gGoogle google = new gGoogle();
		static Propertie prop = new Propertie();

		static readonly string Query = "select datname, pg_database_size(datname) / 1024.0 / 1024 / 1024 from pg_database";

		static void Main(string[] args)
		{
			GoGoogel();
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
			GoGoogel();
			Console.WriteLine("Данные обновлены.");
		}

		/// <summary>
		/// Запуск обновления данных в таблице
		/// </summary>
		static void GoGoogel()
		{
			google.FindId(prop.props.FirstOrDefault().Key);

			using(PostgreSQL psql = new PostgreSQL())
			{
				decimal Size;
				Dictionary<string, decimal> Result;

				foreach(KeyValuePair<string, Entity<decimal>> element in prop.props)
				{
					Size = element.Value.Size;
					psql.Connecting(element.Value._String);
					Result = psql.GetTableAndSize(Query, ref Size);

					google.FillSheet(element.Key, Size, Result);
				}
			}
		}
	}
}