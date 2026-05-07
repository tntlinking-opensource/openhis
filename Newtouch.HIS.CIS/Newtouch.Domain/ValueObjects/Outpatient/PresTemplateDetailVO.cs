using Newtouch.Domain.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newtouch.Domain.ValueObjects
{
    [NotMapped]
    public class PresTemplateDetailVO : PresTemplateDetailEntity
    {

        /// <summary>
        /// 
        /// </summary>
        public decimal dj { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? dwjls { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pcmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string yfmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypgg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? cls { get; set; }

        public string zxksmc { get; set; }

        /// <summary>
        /// 冗余的字段，页面上在给select填充options的时候用到
        /// </summary>
        public string redundant_jldw { get; set; }
        /// <summary>
        /// 是否医保 add by sunny 秦皇岛三期 引用历史处方时，医保病人无法选择自费项目
        /// </summary>
        public string sfyb { get; set; }
        /// <summary>
        /// add by huangshanshan 门诊模板复制用法和剂型关联，用法无法正常显示
        /// </summary>
        public string jxCode { get; set; }
        public string ztId { get; set; }
        public string ztmc { get; set; }
    }
}
