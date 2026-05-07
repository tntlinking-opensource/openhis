using System;

namespace Newtouch.Domain.DTO.InputDto
{
    /// <summary>
    /// 损益查询条件
    /// </summary>
    public class LossAndProditSearchDTO
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime startTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime endTime { get; set; }

        /// <summary>
        /// 损益原因
        /// </summary>
        public string syyy { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string inputCode { get; set; }

        /// <summary>
        /// 损益标志  0：损  1：益  -1：全部
        /// </summary>
        public string sybz { get; set; }
    }
}
