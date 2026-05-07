using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.ValueObjects
{
	public class SysMSGQueryVO
	{
		public string ypCode { get; set; }
		public int? kykc { get; set; }
		public int? kcsl { get; set; }
		public string yxq { get; set; }
		public string typeas { get; set; }
        public string pc { get; set; }
        public string ph { get; set; }
    }
}
