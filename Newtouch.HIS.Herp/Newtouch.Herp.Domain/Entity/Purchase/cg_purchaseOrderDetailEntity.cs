using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;

namespace Newtouch.Herp.Domain.Entity
{
    /// <summary>
    /// 采购_采购计划明细
    /// </summary>
    [Table("cg_purchaseOrderDetail")]
    public class CgPurchaseOrderDetailEntity : IEntity<CgPurchaseOrderDetailEntity>
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 采购计划单ID
        /// </summary>
        public long purchaseId { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 计划采购数 sl*zhyz=最小单位数量
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 实际采购数 sjsl*zhyz=最小单位数量
        /// </summary>
        public int sjsl { get; set; }

        /// <summary>
        /// 单位ID（冗余）
        /// </summary>
        public string unitId { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 状态 0:作废；1.有效
        /// </summary>
        public string zt { get; set; }

        /// <summary>
        /// 创建用户ID
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 最后修改用户ID
        /// </summary>
        public string LastModifierCode { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string supplierId { get; set; }
        public string supplierName { get; set; }
    }
}