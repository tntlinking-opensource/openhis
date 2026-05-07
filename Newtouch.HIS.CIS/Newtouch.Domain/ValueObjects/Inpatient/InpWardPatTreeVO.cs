using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects
{
    public class InpWardPatTreeVO
    {
        public string OrganizeId { get; set; }
        public string bqCode { get; set; }
        public string bqmc { get; set; }
        /// <summary>
        /// 住院号
        /// </summary>
        public string zyh { get; set; }
        /// <summary>
        /// 患者姓名
        /// </summary>
        public string hzxm { get; set; }

        public string sex { get; set; }

        public string nl { get; set; }

        /// <summary>
        /// 床位号
        /// </summary>
        public string BedNo { get; set; }
        public string rqrq { get; set; }
        public string cqrq { get; set; }
        public DateTime ryrq { get; set; }
        public DateTime birth { get; set; }
        public string inHosDays  { get; set; }
        public string brxzmc { get; set; }
    }
}
