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

		public gGoogle()
		{
			string CredentialPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			CredentialPath = Path.Combine(CredentialPath, "driveApiCredentials", "credentials.json");

			userCredential = GetCredential("client_secret.json", CredentialPath);
			driveService = GetDriveService(userCredential);

			sheetCredential = GetCredential("credentials.json", CredentialPath);
			sheetsService = GetSheetService(sheetCredential);
		}

		/// <summary>
		/// Авторизация пользователя, и получение доступа к google drive (Токен авторизации)
		/// </summary>
		/// <param name="jsoneFile">Имя json файла</param>
		/// <param name="CredentialPath">Путь где будет сохраняться файл</param>
		/// <returns>Токен авторизации</returns>
		UserCredential GetCredential(string jsoneFile, string CredentialPath)
		{
			using(FileStream file = new FileStream(jsoneFile, FileMode.Open, FileAccess.Read))
			{
				return GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(file).Secrets, Scopes, Propertie.GetUser(), CancellationToken.None, new FileDataStore(CredentialPath, true)).Result;
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
		/// <param name="FirstSheetName">Название первого листа если файла нет на диске</param>
		public void FindId(string FirstSheetName)
		{
			foreach(File file in driveService.Files.List().Execute().Files)
			{
				if(file.Name == SpreadsheetsName)
				{
					SpreadsheetsId = file.Id;

					return;
				}
			}

			SpreadsheetsId = CreateFile(FirstSheetName);
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
			List<Request> requests = new List<Request> { request };

			BatchUpdateSpreadsheetRequest body = new BatchUpdateSpreadsheetRequest()
			{
				Requests = requests
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
		/// <param name="Size">Остаточный объем на диске (в GB)</param>
		/// <param name="Info">Имя таблицы - размер таблицы (в GB)</param>
		public void FillSheet(string Server, decimal Size, Dictionary<string, decimal> Info)
		{
			if(FindSheet(Server))
			{
				ClearSheet(Server);
			}
			else
			{
				AddSheet(Server);
			}

			string Date = DateTime.Now.ToShortDateString();
			FillRow(new object[] { "Сервер", "База данных", "Размер в ГБ", "Дата обновления" }, Server);

			foreach(KeyValuePair<string, decimal> element in Info)
			{
				FillRow(new object[] { Server, element.Key, Math.Round(element.Value, 1), Date }, Server);
			}

			FillRow(new object[] { Server, "Свободно", Math.Round(Size, 1), Date }, Server);
		}

		/// <summary>
		/// Запись данных в стоку
		/// </summary>
		/// <param name="obj">Массив значений</param>
		/// <param name="Sheet">Имя листа</param>
		void FillRow(object[] obj, string Sheet)
		{
			List<object> objList = obj.ToList();

			ValueRange valueRange = new ValueRange();
			valueRange.Values = new List<IList<object>> { objList };

			var appendRequest = sheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetsId, string.Format("'{0}'!A:Z", Sheet));
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			appendRequest.Execute();
		}

		/// <summary>
		/// Поиск листа в файле
		/// </summary>
		/// <param name="Sheet">Имя лист</param>
		/// <returns>true - лист есть в файле / false - листа нет в файле</returns>
		bool FindSheet(string Sheet)
		{
			SpreadsheetsResource.GetRequest srgrSheetsName = sheetsService.Spreadsheets.Get(SpreadsheetsId);
			Spreadsheet sSheetsName = srgrSheetsName.Execute();

			return sSheetsName.Sheets.Where(x => x.Properties.Title == Sheet).Count() > 0;
		}
	}
}