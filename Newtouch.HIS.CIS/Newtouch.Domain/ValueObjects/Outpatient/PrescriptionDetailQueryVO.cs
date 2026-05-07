using Newtouch.Domain.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newtouch.Domain.ValueObjects
{
    [NotMapped]
    public class PrescriptionDetailQueryVO: PrescriptionDetailEntity
    {
        /// <summary>
        /// 药品字典表的jldw
        /// </summary>
        public string redundant_jldw { get; set; }
        /// <summary>
        /// 保存选择的jldw 同mcjldw
        /// </summary>
        public string mcjldwwwwwww { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pcmc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ypgg { get; set; }
        /// <summary>
        /// 门诊单位拆零数（开的、算钱的一定是用门诊单位）
        /// </summary>
        public decimal? cls { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string yfmc { get; set; }
        /// <summary>
        /// 单位计量数
        /// </summary>
        public int? dwjls { get; set; }
        /// <summary>
        /// 计价策略
        /// </summary>
        public int? jjcl { get; set; }
        /// <summary>
        /// 处方号
        /// </summary>
        public string cfh { get; set; }

        /// <summary>
        /// 执行科室名称
        /// </summary>
        public string zxksmc { get; set; }

        public string jxCode { get; set; }

        public bool? tbz { get; set; }
        public string sfdlCode { get; set; }
        public string sfdlmc { get; set; }
        public decimal? jlzhxs { get; set; }
        public int? zxcs { get; set; }
        public int? tieshu { get; set; }

    }
}
