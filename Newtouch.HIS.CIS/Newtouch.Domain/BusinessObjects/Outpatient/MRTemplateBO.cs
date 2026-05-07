using Newtouch.Domain.Entity;
using Newtouch.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using Newtouch.Domain.Entity.Outpatient;

namespace Newtouch.Domain.BusinessObjects
{
    public class MRTemplateBO
    {
        #region 模板内容
        /// <summary>
        /// 
        /// </summary>
        public string mbmc { get; set; }
        /// <summary>
        /// 主诉
        /// </summary>
        public string zs { get; set; }

        /// <summary>
        /// 现病史
        /// </summary>
        public string xbs { get; set; }

        /// <summary>
        /// 既往史
        /// </summary>
        public string jws { get; set; }

        /// <summary>
        /// 查体
        /// </summary>
        public string ct { get; set; }
        /// <summary>
        /// 处理
        /// </summary>
        public string clfa { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public List<WMDiagnosisHtmlVO> xyzdList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<TCMDiagnosisHtmlVO> zyzdList { get; set; }
        
        public List<CfTemplateGroupPackageEntity> mbztList { get; set; }
        /// <summary>
        /// 月经史
        /// </summary>
        public string yjs { get; set; }
        /// <summary>
        /// 过敏史
        /// </summary>
        public string gms { get; set; }
        /// <summary>
        /// 婚姻
        /// </summary>
        public string hy { get; set; }
    }

    public class MRTemplateListBO
    {
        public string mbId { get; set; }
        public string mbmc { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? LastModifyTime { get; set; }
    }
}
