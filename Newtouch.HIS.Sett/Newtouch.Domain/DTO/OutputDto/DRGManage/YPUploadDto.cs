using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.DTO.OutputDto.DRGManage
{
    public class YPUploadDto
    {
        public string   mlbm_id { get; set; }
        public string scqk { get; set; }
        public DateTime? scrq { get; set; }
        public string errorMsg { get; set; }
        public string ypmc { get; set; }
        public string pch { get; set; }
    }
}
