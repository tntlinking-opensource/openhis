using System;
using Newtouch.Infrastructure.EF.Attributes;

namespace Newtouch.Domain.ValueObjects.Storage
{
    /// <summary>
    /// 结转明细
    /// </summary>
    public class VCarryOverDetailEntity
    {
        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 结转数量（带单位）
        /// </summary>
        public string jzsl { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string ph { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string supplierName { get; set; }

        /// <summary>
        /// 零售价 对应zhyz单位
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal lsj { get; set; }

        /// <summary>
        /// 零售总额
        /// </summary>
        public decimal lsze { get; set; }

        /// <summary>
        /// 部门单位名称
        /// </summary>
        public string bmdwmc { get; set; }

        /// <summary>
        /// 库存数量 最小单位
        /// </summary>
        public int kcsl { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
