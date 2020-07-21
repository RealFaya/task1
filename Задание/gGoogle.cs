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

		string[] Scopes = new string[] { DriveService.Scope.Drive };
		string[] ScopesSheets = new string[] { SheetsService.Scope.Spreadsheets };

		string AppName = "task";
		string SpreadsheetsName = "Размер баз данных PostgreSQL";

		UserCredential userCredential;
		UserCredential sheetCredential;
		DriveService driveService;
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

		UserCredential GetCredential(string jsoneFile, string CredentialPath)
		{
			using(FileStream file = new FileStream(jsoneFile, FileMode.Open, FileAccess.Read))
			{
				return GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(file).Secrets, Scopes, Propertie.GetUser(), CancellationToken.None, new FileDataStore(CredentialPath, true)).Result;
			}
		}

		DriveService GetDriveService(UserCredential Credential)
		{
			return new DriveService(GetInitializer(Credential));
		}

		SheetsService GetSheetService(UserCredential Credential)
		{
			return new SheetsService(GetInitializer(Credential));
		}

		BaseClientService.Initializer GetInitializer(UserCredential Credential)
		{
			return new BaseClientService.Initializer()
			{
				HttpClientInitializer = Credential,
				ApplicationName = AppName
			};
		}

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

		string CreateFile(string SheetName)
		{
			Spreadsheet myNewSheet = new Spreadsheet();
			myNewSheet.Properties = new SpreadsheetProperties();
			myNewSheet.Properties.Title = SpreadsheetsName;

			Sheet sheet = CreateSheet(SheetName);

			myNewSheet.Sheets = new List<Sheet>() { sheet };

			return sheetsService.Spreadsheets.Create(myNewSheet).Execute().SpreadsheetId;
		}

		Sheet CreateSheet(string Title)
		{
			Sheet sheet = new Sheet();
			sheet.Properties = new SheetProperties();
			sheet.Properties.Title = Title;

			return sheet;
		}

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

		void ClearSheet(string sheet)
		{
			string range = string.Format("'{0}'!A1:Z", sheet);
			ClearValuesRequest requestBody = new ClearValuesRequest();

			var deleteRequest = sheetsService.Spreadsheets.Values.Clear(requestBody, SpreadsheetsId, range);
			deleteRequest.Execute();
		}

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

		void FillRow(object[] obj, string Sheet)
		{
			List<object> objList = obj.ToList();

			ValueRange valueRange = new ValueRange();
			valueRange.Values = new List<IList<object>> { objList };

			var appendRequest = sheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetsId, string.Format("'{0}'!A:D", Sheet));
			appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
			appendRequest.Execute();
		}

		bool FindSheet(string Sheet)
		{
			SpreadsheetsResource.GetRequest srgrSheetsName = sheetsService.Spreadsheets.Get(SpreadsheetsId);
			Spreadsheet sSheetsName = srgrSheetsName.Execute();

			return sSheetsName.Sheets.Where(x => x.Properties.Title == Sheet).Count() > 0;
		}
	}
}