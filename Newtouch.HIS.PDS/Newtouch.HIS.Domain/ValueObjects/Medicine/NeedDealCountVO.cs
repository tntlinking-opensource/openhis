namespace Newtouch.HIS.Domain.ValueObjects
{
    /// <summary>
    /// 待处理事件汇总
    /// </summary>
    public class NeedDealCountVO
    {
        /// <summary>
        /// 调价待审核
        /// </summary>
        public long? tjshCount { get; set; }

        /// <summary>
        /// 调价药品
        /// </summary>
        public long? tjypCount { get; set; }

        /// <summary>
        /// 门诊待排
        /// </summary>
        public long? mzdpCount { get; set; }

        /// <summary>
        /// 门诊待发药
        /// </summary>
        public long? mzdfCount { get; set; }

        /// <summary>
        /// 住院待排
        /// </summary>
        public long? zydpCount { get; set; }

        /// <summary>
        /// 住院待发药
        /// </summary>
        public long? zydfCount { get; set; }

        /// <summary>
        /// 住院待退药
        /// </summary>
        public long? zydtCount { get; set; }

        /// <summary>
        /// 申领待审核
        /// </summary>
        public long? sldshCount { get; set; }

        /// <summary>
        /// 出库待审核
        /// </summary>
        public long? ckdshCount { get; set; }

        /// <summary>
        /// 入库待审核
        /// </summary>
        public long? rkdshCount { get; set; }

        /// <summary>
        /// 过期药品数量
        /// </summary>
        public long? expiryDrugCount { get; set; }

        /// <summary>
        /// 当日过期药品数量
        /// </summary>
        public long? todayDrugCount { get; set; }

    }
}
