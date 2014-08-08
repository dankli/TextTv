using System;

namespace TextTv.Shared.Model
{
    public class ResponseResult
    {
        public DateTimeOffset? Date { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string Markup { get; set; }
        public string ETag { get; set; }
    }
}
