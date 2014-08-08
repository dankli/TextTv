using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using TextTv.Shared.Infrastructure.Contracts;
using TextTv.Shared.Model;

namespace TextTv.Windows.AppContext
{
    public sealed class SyncPagesIO : ISyncPagesIO
    {
        private StorageFolder folder;
        private StorageFile file;
        private List<MonitorPage> monitorPages;
        private bool isDirty;
        private const string syncPagesFolderName = "SyncPages";
        private const string syncPagesFileName = "pages.json";

        public SyncPagesIO()
        {
            this.monitorPages = new List<MonitorPage>();
            this.isDirty = true;
        }

        public async Task Initialize()
        {
            this.folder = await this.CreateFolder(syncPagesFolderName).ConfigureAwait(false);
            this.file = await this.CreateFile(this.folder, syncPagesFileName).ConfigureAwait(false);
            this.monitorPages = await this.ReadMonitorPagesFromLocalStorage().ConfigureAwait(false);
        }

        public async Task Clear()
        {
            this.monitorPages = new List<MonitorPage>();
            if (this.file != null)
            {
                await FileIO.WriteTextAsync(this.file, string.Empty).AsTask().ConfigureAwait(false);
            }
        }

        public async Task WriteMonitorPagesToLocalStorage(List<MonitorPage> pages)
        {
            this.monitorPages = pages;

            if (pages.Any() == false)
            {
                await FileIO.WriteTextAsync(this.file, string.Empty).AsTask().ConfigureAwait(false);
            }
            else
            {
                string content = JsonConvert.SerializeObject(pages);
                await FileIO.WriteTextAsync(this.file, content).AsTask().ConfigureAwait(false); ;
            }
        }

        public async Task<List<MonitorPage>> ReadMonitorPagesFromLocalStorage()
        {
            if (this.isDirty == false)
            {
                return this.monitorPages;
            }

            string content = await FileIO.ReadTextAsync(this.file).AsTask().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(content))
            {
                this.monitorPages = new List<MonitorPage>();
                this.isDirty = false;
                return this.monitorPages;
            }

            bool failedRead = false;
            try
            {
                List<MonitorPage> pages = JsonConvert.DeserializeObject<List<MonitorPage>>(content);
                this.monitorPages = pages;
                this.isDirty = false;
                return pages;
            }
            catch 
            {
                this.isDirty = true;
                failedRead = true;
            }

            if (failedRead)
            {
                await FileIO.WriteTextAsync(this.file, string.Empty);
                this.monitorPages = new List<MonitorPage>();
            }

            return this.monitorPages;
        }

        private async Task<StorageFile> CreateFile(StorageFolder storageFolder, string fileName)
        {
            return await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        }

        private async Task<StorageFolder> CreateFolder(string folderName)
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }
    }
}
