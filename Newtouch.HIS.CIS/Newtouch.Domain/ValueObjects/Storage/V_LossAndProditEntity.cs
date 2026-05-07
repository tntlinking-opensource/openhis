using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects.Storage
{
    /// <summary>
    /// 损益明细信息
    /// </summary>
    public class VLossAndProditEntity
    {
        /// <summary>
        /// 损益标志
        /// </summary>
        public string sybz { get; set; }

        /// <summary>
        /// 损益原因
        /// </summary>
        public string syyy { get; set; }

        /// <summary>
        /// 单据号
        /// </summary>
        public string djh { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 进价
        /// </summary>
        public decimal jj { get; set; }

        /// <summary>
        /// 报告时间
        /// </summary>
        public DateTime Bgsj { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public string lb { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 产地名称
        /// </summary>
        public string cd { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string Ph { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime Yxq { get; set; }

        /// <summary>
        /// 责任人
        /// </summary>
        public string Zrr { get; set; }

        /// <summary>
        /// 损益数量（带单位）
        /// </summary>
        public string syslStr { get; set; }

        /// <summary>
        /// 零售价
        /// </summary>
        public decimal lsj { get; set; }

        /// <summary>
        /// 零售总额
        /// </summary>
        public decimal ljze { get; set; }
    }
}
