using FrameworkBase.MultiOrg.Domain.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newtouch.HIS.Domain.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [Table("xt_sfmbxm")]
    public class SysChargeTemplateItemMappEntity : IEntity<SysChargeTemplateItemMappEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string sfmbxmId { get; set; }

        /// <summary>
        /// 组织机构Id（有具体业务的医院）
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string sfmbbh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string sfxm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LastModifierCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? px { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zt { get; set; }
        public int? sl { get; set; }
        public int? duration { get; set; }
        public string kflb { get; set; }

        /// <summary>
        /// 治疗量
        /// </summary>
        public int? zll { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int? zxcs { get; set; }

        /// <summary>
        /// 执行科室编码
        /// </summary>
        public string zxks { get; set; }

        /// <summary>
        /// 医嘱频次
        /// </summary>
        public string yzpc { get; set; }

        /// <summary>
        /// 部位（康复治疗建议）
        /// </summary>
        public string bw { get; set; }

        public decimal? dj { get; set; }

    }
}
