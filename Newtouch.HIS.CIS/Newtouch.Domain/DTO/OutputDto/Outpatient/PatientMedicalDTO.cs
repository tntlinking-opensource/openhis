using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.ValueObjects;
using Newtouch.Infrastructure.EF.Attributes;
using System;
using System.Collections.Generic;

namespace Newtouch.Domain.DTO.OutputDto
{
    public class PatientMedicalDTO
    {
        public string hzxm { get; set; }
        public string xmmc { get; set; }
        public string yzxz { get; set; }
        public int? zh { get; set; }
        public string yzh { get; set; }
        public int? px { get; set; }
        public string bedCode { get; set; }
        public string DeptCode { get; set; }
    }
    
}
