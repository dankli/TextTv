using System.Collections.Generic;

namespace TextTv.Shared.Model
{
    public class PageInfo
    {
        public string id { get; set; }
        public string num { get; set; }
        public string title { get; set; }
        public List<string> content { get; set; }
        public string next_page { get; set; }
        public string prev_page { get; set; }
        public int date_updated_unix { get; set; }
        public string permalink { get; set; }
    }
}
