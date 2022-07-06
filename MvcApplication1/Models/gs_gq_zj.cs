using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    public class gs_gq_zj
    {
        public int gsid { get; set; }
        public string gsname { get; set; }
        public string gsimg { get; set; }

        public int gqid { get; set; }
        public int zjid { get; set; }
        public string gqimg { get; set; }
        public string js { get; set; }
        public Nullable<int> jb { get; set; }
        public string jbtest { get; set; }
        public string gqname { get; set; }
        public Nullable<int> picedq { get; set; }
        public Nullable<int> typeid { get; set; }
        public string texts { get; set; }
        public string sc { get; set; }
        public System.DateTime times { get; set; }
        public int userid { get; set; }
        public string mp3 { get; set; }

        public string zjname { get; set; }
        public int picezj { get; set; }
        public int zjhot { get; set; }
        public string Zhuanjimg { get; set; }
        public Nullable<decimal> gqmoney { get; set; }
    }
}