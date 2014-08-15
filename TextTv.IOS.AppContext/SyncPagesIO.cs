using System;
using System.IO;
using TextTv.Shared.Model;
using System.Collections.Generic;
using TextTv.Shared.Infrastructure.Contracts;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using MonoTouch.Foundation;

namespace TextTv.IOS.AppContext
{
	public class SyncPagesIO : ISyncPagesIO
	{
		private DirectoryInfo folder;
		private FileInfo file;
		private List<MonitorPage> monitorPages;
		private bool isDirty;
		private const string syncPagesFolderName = "SyncPages";
		private const string syncPagesFileName = "pages.json";

		public SyncPagesIO ()
		{
			this.monitorPages = new List<MonitorPage> ();
			this.isDirty = true;
		}

		#region ISyncPagesIO implementation

		public async Task Initialize ()
		{
			this.folder = await this.CreateFolder (syncPagesFolderName).ConfigureAwait(false);
			this.file = await this.CreateFile (this.folder, syncPagesFileName).ConfigureAwait(false);
			this.monitorPages = await this.ReadMonitorPagesFromLocalStorage ().ConfigureAwait(false);
		}

		public async System.Threading.Tasks.Task Clear ()
		{
			this.monitorPages = new List<MonitorPage> ();
			if (this.file != null) {
				await this.WriteToFile (string.Empty).ConfigureAwait (false);
			}
		}

		public async System.Threading.Tasks.Task WriteMonitorPagesToLocalStorage (List<MonitorPage> pages)
		{
			this.monitorPages = pages;
			if (pages.Any ()) {
				await this.WriteToFile (string.Empty).ConfigureAwait (false);
			} else {
				string content = JsonConvert.SerializeObject (pages);
				await this.WriteToFile (content).ConfigureAwait (false);
			}
		}

		public async System.Threading.Tasks.Task<List<MonitorPage>> ReadMonitorPagesFromLocalStorage ()
		{
			if (this.isDirty == false) {
				return this.monitorPages;
			}

			string content = await this.ReadFromFile();

			if (string.IsNullOrWhiteSpace (content)) {
				this.monitorPages = new List<MonitorPage> ();
				this.isDirty = false;
				return this.monitorPages;
			}

			bool failedRead = false;
			try{
				List<MonitorPage> pages = JsonConvert.DeserializeObject<List<MonitorPage>>(content);
				this.monitorPages = pages;
				this.isDirty = false;
				return pages;
			}
			catch(Exception exception) {
				this.isDirty = true;
				failedRead = true;
			}

			if (failedRead) {
				await WriteToFile (string.Empty).ConfigureAwait(false);
				this.monitorPages = new List<MonitorPage> ();
			}

			return this.monitorPages;
		}

		#endregion

		private async Task WriteToFile (string content)
		{
			await Task.Factory.StartNew (() => {
				File.WriteAllText (this.file.FullName, content);
			}); 
		}

		private async Task<string> ReadFromFile ()
		{
			Task<string> task = Task.Factory.StartNew (() => {
				return File.ReadAllText(this.file.FullName);
			});

			string content = await task;
			return content;
		}

		private Task<FileInfo> CreateFile(DirectoryInfo storageFolder, string fileName){
			Task<FileInfo> task = Task.Factory.StartNew (() => {
				var filePath = Path.Combine(storageFolder.FullName, fileName);
				var fileInfo = new FileInfo(filePath);

				if(fileInfo.Exists == false){
					using(fileInfo.Create()){}
				}

				return fileInfo;
			});

			return task;
		}

		private Task<DirectoryInfo> CreateFolder (string syncPagesFolderName)
		{
			Task<DirectoryInfo> task = Task.Factory.StartNew (() => {
				var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), syncPagesFolderName);
				DirectoryInfo directory = new DirectoryInfo (path);
				if (directory.Exists == false) {
					directory.Create ();
				}

				return directory;
			});

			return task;

		}
	}
}

