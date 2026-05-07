using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects.Inpatient
{
    /// <summary>
    /// 医嘱查询Grid View
    /// </summary>
   public class AdviceListGridVO
    {
        public string zyh { get; set; }
        public string xm { get; set; }
        public string yzlb { get; set; }
        public string Id { get; set; }
        public int yzlx { get; set; }
        public DateTime kssj { get; set; }
        public string ysmc { get; set; }
        public string yzmc { get; set; }
        public string yzjl { get; set; }
        public string yfmc { get; set; }
        public string yzpcmc { get; set; }
        public int? zh { get; set; }
        public DateTime? tzsj { get; set; }
        public string tzr { get; set; }
        public string zxr { get; set; }
        public int yzzt { get; set; }
        public DateTime? shsj { get; set; }
        public DateTime? zxsj { get; set; }
		public string deptName { get; set; }
        public string yztag { get; set; }
        public string yztagName { get; set; }
        public int? isjf { get; set; }
        public string ispscs { get; set; }
        public int? zzfbz { get; set; }
		public int sl { get; set; }
		public string zycldw { get; set; } 
        /// <summary>
        /// 是否组套 Y:是N:否
        /// </summary>
        public string iszt { get; set; }
        public string yfztbs { get; set; }
        public int? yply { get; set; }
        /// <summary>
        /// 是否附属医嘱
        /// </summary>
        public string isfsyz { get; set; }
        /// <summary>
        /// 医嘱号
        /// </summary>
        public string yzh { get; set; }
    }
}
