using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.DTO.OutputDto.DRGManage
{
    public class DRGUploadDto
    {
        public string scqk { get; set; }
        public DateTime? scrq { get; set; }
        public string errorMsg { get; set; }
        public string zyh { get; set; }
        public string xm { get; set; }
        public string sfzh { get; set; }
        public string mdtrt_id { get; set; }
        public string setl_id { get; set; }
        public DateTime? ryrq { get; set; }
        public DateTime? cyrq { get; set; }
        public DateTime? basyrq { get; set; }
        public DateTime? jsrq { get; set; }
        public string ksmc { get; set; }
        public string ysmc { get; set; }
        
    }
}
