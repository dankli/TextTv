using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using TextTv.AppContext.Infrastructure.Contracts;

namespace TextTv.PhoneApp.Infrastructure
{
    public class NotifierTaskHandler : INotifierTaskHandler
    {
        private readonly Action _taskCompleted;

        public NotifierTaskHandler(Action taskCompleted = null)
        {
            _taskCompleted = taskCompleted;
        }

        public async Task<bool> RegisterTask(uint interval = 15)
        {
            try
            {
                BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
                if (status == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity ||
                    status == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity)
                {
                    var isRegistered = BackgroundTaskRegistration.AllTasks.Any(x => x.Value.Name == "Notification task");

                    if (!isRegistered)
                    {
                        BackgroundTaskBuilder builder = new BackgroundTaskBuilder
                        {
                            Name = "Notification task",
                            TaskEntryPoint = "TextTv.Background.NotifyPageChangeTask"
                        };

                        builder.SetTrigger(new TimeTrigger(interval, false));
                        builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                        BackgroundTaskRegistration registration = builder.Register();
                        registration.Completed += RegistrationOnCompleted;
                    }

                    return true;
                }
            }
            catch 
            {
                Debug.WriteLine("The access has already been granted");
                return false;
            }

            return false;
        }

        private void RegistrationOnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            if (_taskCompleted != null)
            {
                _taskCompleted();
            }
        }

        public void UnRegisterTask()
        {
            var isRegistered = BackgroundTaskRegistration.AllTasks.Any(x => x.Value.Name == "Notification task");

            if (isRegistered)
            {
                KeyValuePair<Guid, IBackgroundTaskRegistration> task = BackgroundTaskRegistration.AllTasks.First(x => x.Value.Name == "Notification task");

                task.Value.Unregister(true);
            }
        }
    }
}
