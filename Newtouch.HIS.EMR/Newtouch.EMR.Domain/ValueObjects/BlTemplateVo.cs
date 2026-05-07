using Newtouch.EMR.Domain.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.EMR.Domain.ValueObjects
{
    public class BlTemplateVo
    {
        /// <summary>
        /// 文书、模板Id
        /// </summary>
        public string templateId { get; set; }
        /// <summary>
        /// 文书、模板名称
        /// </summary>
        public string templateName { get; set; }
        /// <summary>
        /// 文书内容
        /// </summary>
        public string xmldata { get; set; }
        /// <summary>
        /// 病历文书路径
        /// </summary>
        public string bllj { get; set; }
        /// <summary>
        /// 病历文书占用锁定标志
        /// </summary>
        public bool lockStatus { get; set; }

        //门诊病历基本信息
        public TreatEntityVO mzjbxx { get; set; }
        //病人病历基本信息
        public blzybrjbxxVO zybrxx { get; set;}
        /// <summary>
        /// 病历文书病案首页基本信息
        /// </summary>
        public BabasyVO basyVo { get; set; }
    }
}
