using System;

namespace Newtouch.Domain.DTO.InputDto
{
    /// <summary>
    /// 损益提交
    /// </summary>
    public class LossAndProditSubmitDTO
    {
        /// <summary>
        /// 损益情况
        /// </summary>
        public string syqk { get; set; }

        /// <summary>
        /// 最小单位数
        /// </summary>
        public int Sysl { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 损益原因
        /// </summary>
        public string syyy { get; set; }

        /// <summary>
        /// 损益原因ID
        /// </summary>
        public string syyyId { get; set; }

        /// <summary>
        /// 损益标志
        /// </summary>
        public string sybz { get; set; }

        /// <summary>
        /// 单据号
        /// </summary>
        public string Djh { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 进价  转化因子对应进价
        /// </summary>
        public decimal Jj { get; set; }

        /// <summary>
        /// 零售价  转化因子对应进价
        /// </summary>
        public decimal Lsj { get; set; }

        /// <summary>
        /// 零售总额
        /// </summary>
        public decimal Lsjze { get; set; }

        /// <summary>
        /// 最小单位名称
        /// </summary>
        public string zxdw { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 生产厂商
        /// </summary>
        public string sccj { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

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
        /// 责任人名称
        /// </summary>
        public string zrrmc { get; set; }

        /// <summary>
        /// 剩余库存 现有库存 最小单位
        /// </summary>
        public int Sykc { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 单位 价格单位，与zhyz一直
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// 数量，部门单位
        /// </summary>
        public int bmsl { get; set; }

        /// <summary>
        /// 数量，最小单位
        /// </summary>
        public int zxsl { get; set; }

        /// <summary>
        /// 数量，带单位  
        /// </summary>
        public string slstr { get; set; }

        /// <summary>
        /// jqgrid row id
        /// </summary>
        public int jqRowId { get; set; }
    }
}
