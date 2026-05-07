using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.DTO.OutputDto.DRGManage
{
    public class sqszUploadDto
    {
        public string zyh { get; set; }
        public string xm { get; set; }
        public string yzh { get; set; }
        public string zdmc { get; set; }
        public string xmmc { get; set; }
        public string sl { get; set; }
        public string ks { get; set; }
        public DateTime? scrq { get; set; }
        public string errorMsg { get; set; }
        public string yzlx { get; set; }

    }

	public class mxlist
	{
		public string xmmc { get; set; }
		public string kdrq { get; set; }
		public string kdys { get; set; }
		public string kdks { get; set; }
		public string xmywmc { get; set; }
		public string wjz { get; set; }
		public string xmdw { get; set; }
		public string ckz { get; set; }
		public string jyjg { get; set; }
		public string lcyx { get; set; }
	}
}
