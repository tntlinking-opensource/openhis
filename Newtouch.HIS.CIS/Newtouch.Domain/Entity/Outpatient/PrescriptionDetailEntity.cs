using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.EF.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newtouch.Domain.Entity
{
    /// <summary>
    /// 
    /// </summary>
    [Table("xt_cfmx")]
    public class PrescriptionDetailEntity : IEntity<PrescriptionDetailEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string cfmxId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 关联处方表
        /// </summary>
        public string cfId { get; set; }

        /// <summary>
        /// 关联base库的收费项目表
        /// </summary>
        public string xmCode { get; set; }

        /// <summary>
        /// 冗余字段
        /// </summary>
        public string xmmc { get; set; }

        /// <summary>
        /// 关联base库的药品表
        /// </summary>
        public string ypCode { get; set; }

        /// <summary>
        /// 冗余代码
        /// </summary>
        public string ypmc { get; set; }
        /// <summary>
        /// 每次治疗量  针对康复项目
        /// </summary>
        public int? mczll { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DecimalPrecision(11,4)]
        public decimal? mcjl { get; set; }

        /// <summary>
        /// 取base库的药品属性表
        /// </summary>
        public string mcjldw { get; set; }

        /// <summary>
        /// 关联base库的药品用法表
        /// </summary>
        public string yfCode { get; set; }

        /// <summary>
        /// 关联base库的医嘱频次
        /// </summary>
        public string pcCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 总计费数量
        /// </summary>
        public int? zl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DecimalPrecision(11,4)]
        public decimal dj { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal je { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CreatorCode { get; set; }

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
        public string zt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string bw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string urgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string purpose { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string zxks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? zxsj { get; set; }
        /// <summary>
        /// 医保唯一码
        /// </summary>
        public string ybwym { get; set; }
        /// <summary>
        /// 限制使用标志0无限制 1不符合限制规定 2符合限制规定
        /// </summary>
        public string xzsybz { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public decimal? ts { get; set; }
        /// <summary>
        /// 组套所属模板
        /// </summary>
        public string ztmbId { get; set; }
        /// <summary>
        /// 组套id
        /// </summary>
        public string ztId { get; set; }
        /// <summary>
        /// 组套名称
        /// </summary>
        public string ztmc { get; set; }
        /// <summary>
        /// 组套编号
        /// </summary>
        public string ztCode { get; set; }

        /// <summary>
        /// 抗生素原因
        /// </summary>
        public string kssReason { get; set; }

        /// <summary>
        /// 部位方法
        /// </summary>
        public string bwff { get; set; }

        public string sqlx { get; set; }

        public int? px { get; set; }
        /// <summary>
        /// 滴速
        /// </summary>
        public int? ds { get; set; }

        /// <summary>
        /// 转自费标志
        /// </summary>
        public int? zzfbz { get; set; }

        //皮试标志
        public string ispsbz { get; set; }

        //留观标志
        public string islgbz { get; set; }
        /// <summary>
        /// 组套数量
        /// </summary>
        public int? ztsl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? sfzt { get; set; }
        public string zysm { get; set; }
        /// <summary>
        /// 用法对应的项目组套
        /// </summary>
        public string syncfbz { get; set; }
        /// <summary>
        /// 国家医保代码
        /// </summary>
        public string gjybdm { get; set; }
    }
}
