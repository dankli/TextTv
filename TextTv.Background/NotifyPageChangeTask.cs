using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using TextTv.AppContext.Infrastructure;
using TextTv.AppContext.Model;
using TextTv.Windows.AppContext;

namespace TextTv.Background
{
    public sealed class NotifyPageChangeTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            SyncPagesIO syncPagesIo = new SyncPagesIO();
            var deferral = taskInstance.GetDeferral();
            try
            {
                await syncPagesIo.Initialize();
                List<MonitorPage> bruttoMonitorPages = await syncPagesIo.ReadMonitorPagesFromLocalStorage();
                List<MonitorPage> nettoMonitorPages = bruttoMonitorPages.Where(mp => mp.TimeToMonitor > DateTime.Now).ToList();

                List<MonitorPage> changedPages = this.FindChangedPages(nettoMonitorPages);

                if (changedPages.Any())
                {
                    this.SendNotification("SvtText: Ändring på sida {0}", changedPages);
                }

                await syncPagesIo.WriteMonitorPagesToLocalStorage(nettoMonitorPages);
            }
            finally 
            {
                deferral.Complete();    
            }
        }

        private List<MonitorPage> FindChangedPages(IEnumerable<MonitorPage> monitorPages)
        {
            var apiCaller = new ApiCaller();
            var result = new List<MonitorPage>();
            
            List<Task> tasks = new List<Task>();
            foreach (MonitorPage monitorPage in monitorPages)
            {
                var mp = monitorPage;
                Task task = Task.Run(async () =>
                {
                    ResponseResult webContent = await apiCaller.GetSvtTextForWeb(mp.Page).ConfigureAwait(false);

                    if (this.Modified(webContent, mp))
                    {
                        result.Add(mp);
                    }
                });
                
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            return result;
        }

        private bool Modified(ResponseResult webContent, MonitorPage monitorPage)
        {
            if (monitorPage.LastModified.HasValue && webContent.LastModified.HasValue)
            {
                return monitorPage.LastModified.Value < webContent.LastModified.Value;
            }

            if (string.IsNullOrWhiteSpace(monitorPage.ETag) == false && string.IsNullOrWhiteSpace(webContent.ETag) == false)
            {
                return monitorPage.ETag.ToLowerInvariant() != webContent.ETag.ToLowerInvariant();
            }

            return false;
        }

        private void SendNotification(string notificationTextFormat, IEnumerable<MonitorPage> changedPages)
        {
            DateTimeOffset offset = new DateTimeOffset(DateTime.Now.AddSeconds(5));
            foreach (MonitorPage monitorPage in changedPages)
            {
                var p = monitorPage;
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                XmlElement element = toastXml.GetElementsByTagName("toast").First() as XmlElement;
                if (element != null)
                {
                    element.SetAttribute("launch", "{\"type\":\"toast\",\"page\":\"" + p.Page + "\"}");
                }

                XmlNodeList elements = toastXml.GetElementsByTagName("text");
                foreach (IXmlNode node in elements)
                {
                    node.InnerText = string.Format(notificationTextFormat, p.Page);
                }

                IReadOnlyList<ScheduledToastNotification> toasts = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();

                ToastNotifier toastNotifer = ToastNotificationManager.CreateToastNotifier();
                if (toasts.Any())
                {
                    var notification = toasts.FirstOrDefault(stn => stn.Id == monitorPage.Page.ToString());
                    if (notification != null)
                    {
                        toastNotifer.RemoveFromSchedule(notification);
                    }
                }
                
                ScheduledToastNotification scheduledToastNotification = new ScheduledToastNotification(toastXml, offset)
                {
                    Id = monitorPage.Page.ToString()
                };

                toastNotifer.AddToSchedule(scheduledToastNotification);
                offset = offset.AddSeconds(20);
            }
        }
    }
}
