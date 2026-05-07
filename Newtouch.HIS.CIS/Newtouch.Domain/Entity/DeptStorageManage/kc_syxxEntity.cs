using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Infrastructure.EF.Attributes;

namespace Newtouch.Domain.Entity.DeptStorageManage
{
    /// <summary>
    /// 库存损益
    /// </summary>
    [Table("dept_syxx")]
    public class KcSyxxEntity : IEntity<KcSyxxEntity>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 组织机构（医院）
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 库房代码
        /// </summary>
        public string ks { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string Ph { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime? Yxq { get; set; }

        /// <summary>
        /// 默认值Getdate()
        /// </summary>
        public DateTime Bgsj { get; set; }

        /// <summary>
        /// 正数：报溢，负数：报损 最小单位数
        /// </summary>
        public int Sysl { get; set; }

        /// <summary>
        /// 单位ID  与价格配合使用，和zhyz成套
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// 零售价  转化因子对应进价
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal Lsj { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int Zhyz { get; set; }

        /// <summary>
        /// 损益原因  kc_syyy表主键
        /// </summary>
        public string Syyy { get; set; }

        /// <summary>
        /// 责任人
        /// </summary>
        public string Zrr { get; set; }

        /// <summary>
        /// 单据号
        /// </summary>
        public string Djh { get; set; }

        /// <summary>
        /// 剩余库存 现有库存 最小单位 kcsl
        /// </summary>
        public int Sykc { get; set; }

        /// <summary>
        /// 进价
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal Jj { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string zt { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? px { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 最后修改人
        /// </summary>
        public string LastModifierCode { get; set; }
    }
}
