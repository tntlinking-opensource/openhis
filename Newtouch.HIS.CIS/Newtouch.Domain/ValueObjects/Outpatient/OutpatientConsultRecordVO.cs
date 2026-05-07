using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects.Outpatient
{
    public class OutpatientConsultRecordVO
    {
		public string blh { get; set; }
		public string jzId { get; set; }
		public string mzh { get; set; }
        public string xm { get; set; }
        public string xb { get; set; }
        public string nl { get; set; }
        public string zdmc { get; set; }
        public DateTime? zlkssj { get; set; }
        public DateTime? zljssj { get; set; }
        public string ks { get; set; }
        public string ys { get; set; }
        public string dz { get; set; }
        public string zg { get; set; }
    }

	public class OutpatientCfmxVO
	{
		public int cflx { get; set; }
		public string cfh { get; set; }
		public string sfxmcode { get; set; }
		public string sfxmmc { get; set; }
		public int sl { get; set; }
		public decimal dj { get; set; }
		public string dw { get; set; }
		public decimal je { get; set; }
		public string klys { get; set; }
		public string klks { get; set; }
        public bool sfbz { get; set; }
        public bool? tbz { get; set; }
	}
}
