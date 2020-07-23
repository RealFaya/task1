using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using File = Google.Apis.Drive.v3.Data.File;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using System.Net;
using System.CodeDom;

namespace Задание
{
	class gGoogle
	{
		string SpreadsheetsId;

		readonly string[] Scopes = new string[] { DriveService.Scope.Drive };
		readonly string[] ScopesSheets = new string[] { SheetsService.Scope.Spreadsheets };

		readonly string AppName = "task";
		readonly string SpreadsheetsName = "Размер баз данных PostgreSQL";

		UserCredential userCredential;
		DriveService driveService;

		UserCredential sheetCredential;
		SheetsService sheetsService;

		public gGoogle(Propertie porp)
		{
			string CredentialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			CredentialPath = Path.Combine(CredentialPath, "driveApiCredentials", "credentials.json");

			userCredential = GetCredential("client_secret.json", CredentialPath, Scopes);
			driveService = GetDriveService(userCredential);

			sheetCredential = GetCredential("credentials.json", CredentialPath, ScopesSheets);
			sheetsService = GetSheetService(sheetCredential);

			FindId(porp.props.FirstOrDefault().Key);
		}

		/// <summary>
		/// Авторизация пользователя, и получение доступа к google drive (Токен авторизации)
		/// </summary>
		/// <param name="jsoneFile">Имя json файла</param>
		/// <param name="CredentialPath">Путь где будет сохраняться файл</param>
		/// <param name="scopes">scopes</param>
		/// <returns>Токен авторизации</returns>
		UserCredential GetCredential(string jsoneFile, string CredentialPath, string[] scopes)
		{
			using(FileStream file = new FileStream(jsoneFile, FileMode.Open, FileAccess.Read))
			{
				return GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(file).Secrets, scopes, Propertie.User, CancellationToken.None, new FileDataStore(CredentialPath, true)).Result;
			}
		}

		/// <summary>
		/// Служба google drive, для работы с диском
		/// </summary>
		/// <param name="Credential">Токен авторизации пользователя</param>
		/// <returns>Служба google drive</returns>
		DriveService GetDriveService(UserCredential Credential)
		{
			return new DriveService(GetInitializer(Credential));
		}

		/// <summary>
		/// Служба google sheet, для работы с таблицами
		/// </summary>
		/// <param name="Credential">Токен авторизации для работы с таблицами</param>
		/// <returns>Служба google sheet</returns>
		SheetsService GetSheetService(UserCredential Credential)
		{
			return new SheetsService(GetInitializer(Credential));
		}

		/// <summary>
		/// Инициализация токена для служб
		/// </summary>
		/// <param name="Credential">Токен авторизации</param>
		/// <returns></returns>
		BaseClientService.Initializer GetInitializer(UserCredential Credential)
		{
			return new BaseClientService.Initializer()
			{
				HttpClientInitializer = Credential,
				ApplicationName = AppName
			};
		}

		/// <summary>
		/// Получение Id файла на google drive в который будет происходить запись данных
		/// </summary>
		/// <param name="FirstSheetName">Название первого листа если файла нет на диске то он создается</param>
		void FindId(string FirstSheetName)
		{
			SpreadsheetsId = driveService.Files.List().Execute().Files.Where(x => x.Name == SpreadsheetsName)?.FirstOrDefault()?.Id ?? CreateFile(FirstSheetName);
		}

		/// <summary>
		/// Создание файла на google drive
		/// </summary>
		/// <param name="SheetName">Название первого листа</param>
		/// <returns>Id файла</returns>
		string CreateFile(string SheetName)
		{
			Spreadsheet myNewSheet = new Spreadsheet();
			myNewSheet.Properties = new SpreadsheetProperties();
			myNewSheet.Properties.Title = SpreadsheetsName;

			Sheet sheet = new Sheet();
			sheet.Properties = new SheetProperties();
			sheet.Properties.Title = SheetName;

			myNewSheet.Sheets = new List<Sheet>() { sheet };

			return sheetsService.Spreadsheets.Create(myNewSheet).Execute().SpreadsheetId;
		}

		/// <summary>
		/// Добавление нового листа в таблицу
		/// </summary>
		/// <param name="Title">Имя добавляемого листа</param>
		void AddSheet(string Title)
		{
			Request request = new Request
			{
				AddSheet = new AddSheetRequest
				{
					Properties = new SheetProperties
					{
						Title = Title
					}
				}
			};
			
			BatchUpdateSpreadsheetRequest body = new BatchUpdateSpreadsheetRequest
			{
				Requests = new List<Request> { request }
			};

			sheetsService.Spreadsheets.BatchUpdate(body, SpreadsheetsId).Execute();
		}

		/// <summary>
		/// Очистка листа от старой информации
		/// </summary>
		/// <param name="sheet"></param>
		void ClearSheet(string sheet)
		{
			string range = string.Format("'{0}'!A1:Z", sheet);
			ClearValuesRequest requestBody = new ClearValuesRequest();

			sheetsService.Spreadsheets.Values.Clear(requestBody, SpreadsheetsId, range).Execute();
		}

		/// <summary>
		/// Заполнение листа данными
		/// </summary>
		/// <param name="Server">Имя листа</param>
		/// <param name="DiskSize">Остаточный объем на диске (в GB)</param>
		/// <param name="Info">Имя таблицы - размер таблицы (в GB)</param>
		public void FillSheet(string Server, double DiskSize, Dictionary<string, double> Info)
		{
			int? SheetId = FindSheet(Server);

			if(SheetId != null)
			{
				ClearSheet(Server);
			}
			else
			{
				AddSheet(Server);
				SheetId = FindSheet(Server);
			}

			List<Request> requests = new List<Request>();
			List<RowData> rowDatas = new List<RowData>();
			string Date = DateTime.Now.ToShortDateString();

			rowDatas.Add(new RowData { Values = AddCellsInRow("Сервер", "База данных", "Размер в ГБ", "Дата обновления") });

			foreach(KeyValuePair<string, double> element in Info)
			{
				rowDatas.Add(new RowData { Values = AddCellsInRow(Server, element.Key, element.Value, Date) });
			}

			rowDatas.Add(new RowData { Values = AddCellsInRow(Server, "Свободно", DiskSize, Date) });

			requests.Add(new Request
			{
				AppendCells = new AppendCellsRequest
				{
					Rows = rowDatas,
					SheetId = SheetId,
					Fields = "userEnteredValue"
				}
			});

			BatchUpdateSpreadsheetRequest batchUpdate = new BatchUpdateSpreadsheetRequest
			{
				Requests = requests
			};

			sheetsService.Spreadsheets.BatchUpdate(batchUpdate, SpreadsheetsId).Execute();
		}

		/// <summary>
		/// Добавление значений в ячейки
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		List<CellData> AddCellsInRow(params object[] args)
		{
			ExtendedValue value;
			List<CellData> values = new List<CellData>();

			foreach(object element in args)
			{
				value = new ExtendedValue();

				if(element.GetType().Equals(typeof(double)))
				{
					value.NumberValue = (double)element;
				}
				else
				{
					value.StringValue = element.ToString();
				}
				
				values.Add(new CellData
				{
					UserEnteredValue = value
				});
			}

			return values;
		}

		/// <summary>
		/// Поиск листа в файле
		/// </summary>
		/// <param name="Sheet">Имя лист</param>
		/// <returns>id листа, если нету то null</returns>
		int? FindSheet(string Sheet)
		{
			SpreadsheetsResource.GetRequest srgrSheetsName = sheetsService.Spreadsheets.Get(SpreadsheetsId);
			Spreadsheet sSheetsName = srgrSheetsName.Execute();

			return sSheetsName.Sheets.Where(x => x.Properties.Title == Sheet)?.FirstOrDefault()?.Properties.SheetId;
		}
	}
}