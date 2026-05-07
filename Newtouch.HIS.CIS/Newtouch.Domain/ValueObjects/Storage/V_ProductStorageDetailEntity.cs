using System;

namespace Newtouch.Domain.ValueObjects.Storage
{
    /// <summary>
    /// 物资库存分批次批号汇总
    /// </summary>
    public class VProductStorageDetailEntity
    {
        public string Id { get;set; }
        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 批号
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
        /// 状态
        /// </summary>
        public string zt { get; set; }

        /// <summary>
        /// 部门单位名称
        /// </summary>
        public string bmdwmc { get; set; }

        /// <summary>
        /// 最小单位名称
        /// </summary>
        public string zxdwmc { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 部门冻结数量  带单位
        /// </summary>
        public string bmdjslStr { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int? kcsl { get; set; }

        /// <summary>
        /// 冻结数量
        /// </summary>
        public int? djsl { get; set; }

        /// <summary>
        /// 部门库存数量 带单位
        /// </summary>
        public string bmkcslStr { get; set; }

        /// <summary>
        /// 进价总金额
        /// </summary>
        public decimal jjze { get; set; }

        /// <summary>
        /// 进价
        /// </summary>
        public decimal jj { get; set; }

        /// <summary>
        /// 进价带单位
        /// </summary>
        public string jjStr { get; set; }
        /// <summary>
        /// 进价
        /// </summary>
        public decimal lsj { get; set; }
        /// <summary>
        /// 零售总额
        /// </summary>
        public decimal lsze { get; set; }
        /// <summary>
        /// 零售价带单位
        /// </summary>
        public string lsjStr { get; set; }
        /// <summary>
        /// 类别名称
        /// </summary>
        public string lb { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string brand { get; set; }

        /// <summary>
        /// 生产厂商名称
        /// </summary>
        public string sccj { get; set; }

        /// <summary>
        /// 单价单位
        /// </summary>
        public string bmjjdjdw { get; set; }
    }
}
