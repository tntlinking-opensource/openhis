using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Newtouch.HIS.Domain.Entity
{
    /// <summary>
    /// 系统药品属性
    /// </summary>
    [Table("xt_ypsx")]
    public class SysMedicinePropertyEntity : IEntity<SysMedicinePropertyEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int ypsxId { get; set; }

        /// <summary>
        /// 关联系统药品药品编号
        /// </summary>
        public int ypId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypCode { get; set; }

        /// <summary>
        /// --0 无需审核 1 需要审核   －－一期暂时不做 2006.06.21
        /// </summary>
        public string shbz { get; set; }

        /// <summary>
        /// 0 普通项目 1 特殊项目 refer xt_yptsbz   特殊药品说明是特定的病人性质才可使用的。
        /// </summary>
        public string tsbz { get; set; }

        /// <summary>
        /// 包含了原来的“皮试标志”
        /// </summary>
        public string jsbz { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string gzy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string mzy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string yljsy { get; set; }

        /// <summary>
        /// 0 无需招标   1 市招标   2 区招标   3 竞价   4 询价
        /// </summary>
        public string zbbz { get; set; }

        /// <summary>
        /// 默认用法来自xt_ypyf
        /// </summary>
        public string zlff { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string sjap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? yl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string yldw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ypgg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ybdm { get; set; }

        /// <summary>
        /// 医保控量设置字段
        /// </summary>
        public int? syts { get; set; }

        /// <summary>
        /// 医保控量设置字段
        /// </summary>
        public decimal? dczdjl { get; set; }

        /// <summary>
        /// 医保控量设置字段
        /// </summary>
        public decimal? dczdsl { get; set; }

        /// <summary>
        /// 医保控量设置字段
        /// </summary>
        public decimal? ljzdjl { get; set; }

        /// <summary>
        /// 医保控量设置字段
        /// </summary>
        public decimal? ljzdsl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string pzwh { get; set; }

        /// <summary>
        /// 来自系统药品特殊属性xt_ypxz
        /// </summary>
        public string yptssx { get; set; }

        /// <summary>
        /// 来自xt_ypfl
        /// </summary>
        public string ypflCode { get; set; }

        /// <summary>
        /// 0：每顿取整收费1：每次取整收费
        /// </summary>
        public string jzlx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? mrbzq { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? zjtzsj { get; set; }

        /// <summary>
        /// xg：修改,xz：新增,ty：停用,qy：启用,tj：调价
        /// </summary>
        public string xglx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ghdw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? ypcd { get; set; }

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
        public string zt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? px { get; set; }

        /// <summary>
        /// 组织Id
        /// </summary>
        public string OrganizeId { get; set; }
        /// <summary>
        /// 药品同步医保时间
        /// </summary>
        public DateTime? LastYBUploadTime { get; set; }
        /// <summary>
        /// 默认剂量
        /// </summary>
        public decimal? mrjl { get; set; }
        /// <summary>
        /// 默认频次
        /// </summary>
        public string mrpc { get; set; }
        /// <summary>
        /// 医保标志
        /// </summary>
        public string ybbz { get; set; }
        /// <summary>
        /// 新农合医保代码
        /// </summary>
        public string xnhybdm { get; set; }
        /// <summary>
        /// 新农合医保代码
        /// </summary>
        public string gjybdm { get; set; }
        public string gjybmc { get; set; }
        /// <summary>
        /// 单次限量
        /// </summary>
        public decimal? dcxl { get; set; }
        /// <summary>
        /// 慢病限量
        /// </summary>
        public decimal? mbxl { get; set; }

        /// <summary>
        /// 默认用法
        /// </summary>
        public string mryf { get; set; }
        /// <summary>        /// 医保规格        /// </summary>        public string ybgg { get; set; }
        public int? kcyjz { get; set; }
    }
}
