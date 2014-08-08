using System;

namespace TextTv.Shared.Model
{
    public class MonitorPage
    {
        public int Page { get; set; }
        public DateTime TimeToMonitor { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public DateTimeOffset? Date { get; set; }
        public string ETag { get; set; }
    }
}
