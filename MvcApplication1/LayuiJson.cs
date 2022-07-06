using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1
{
    public class LayuiJson
    {
        public int code { get; set; }
        public string msg { get; set; }
        public int count { get; set; }
        public List<object> data { get; set; }

        public int gqid { get; set; }
        public int gsid { get; set; }
        public int zjid { get; set; }
        public string gq { get; set; }
        public string gsname { get; set; }
        public string zjname { get; set; }
        public string mp3 { get; set; }
        public int gdid { get; set; }
        public string gqimg { get; set; }
        public int gqhot { get; set; }
    }
}