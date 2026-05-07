namespace Newtouch.Domain.ValueObjects.Storage
{
    /// <summary>
    /// 进销存统计
    /// </summary>
    public class VPsiStatisticsEntity
    {
        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string lbmc { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 本部门单位名称
        /// </summary>
        public string bmdwmc { get; set; }

        /// <summary>
        /// 期初数量，带单位
        /// </summary>
        public string qcsl { get; set; }

        /// <summary>
        /// 入库数量，带单位
        /// </summary>
        public string rksl { get; set; }

        /// <summary>
        /// 出库数量，带单位
        /// </summary>
        public string cksl { get; set; }

        /// <summary>
        /// 损益数量，带单位
        /// </summary>
        public string sysl { get; set; }

        /// <summary>
        /// 期末数量，带单位
        /// </summary>
        public string qmsl { get; set; }

        /// <summary>
        /// 期初零售总额
        /// </summary>
        public decimal qclsze { get; set; }

        /// <summary>
        /// 入库零售总额
        /// </summary>
        public decimal rklsze { get; set; }

        /// <summary>
        /// 出库零售总额
        /// </summary>
        public decimal cklsze { get; set; }

        /// <summary>
        /// 损益零售总额
        /// </summary>
        public decimal sylsze { get; set; }

        /// <summary>
        /// 期末零售总额
        /// </summary>
        public decimal qmlsze { get; set; }

        /// <summary>
        /// 调价损益零售总额
        /// </summary>
        public decimal tjsyze { get; set; }
    }
}
