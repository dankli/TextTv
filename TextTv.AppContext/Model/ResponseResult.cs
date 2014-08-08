using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextTv.AppContext.Model
{
    public class ResponseResult
    {
        public DateTimeOffset? Date { get; set; }
        public DateTimeOffset? LastModified { get; set; }
        public string Markup { get; set; }
        public string ETag { get; set; }
    }
}
