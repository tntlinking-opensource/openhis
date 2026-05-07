using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Infrastructure.EF.Attributes;

namespace Newtouch.Domain.Entity.DeptStorageManage
{
    /// <summary>
    /// 库存盘点信息明细
    /// </summary>
    [Table("dept_pdxxmx")]
    public class KcPdxxmxEntity : IEntity<KcPdxxmxEntity>
    {
        /// <summary>
        /// 主键 明细ID
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 盘点主表ID
        /// </summary>
        public long pdId { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        ///  批号
        /// </summary>
        public string ph { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime? yxq { get; set; }

        /// <summary>
        /// 理论数量
        /// </summary>
        public int llsl { get; set; }

        /// <summary>
        /// 实际数量
        /// </summary>
        public int sjsl { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 零售价
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal lsj { get; set; }

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
