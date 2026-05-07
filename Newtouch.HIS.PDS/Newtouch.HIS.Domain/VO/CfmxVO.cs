using System;

namespace Newtouch.HIS.Domain.VO
{
    /// <summary>
    /// 处方明细
    /// </summary>
    public class CfmxVO
    {
        /// <summary>
        /// 发票号
        /// </summary>
        public string fph { get; set; }

        /// <summary>
        /// 处方号
        /// </summary>
        public string cfh { get; set; }

        public string cfnm { get; set; }
        public string ypCode { get; set; }
        /// <summary>
        /// 药品名称
        /// </summary>
        public string ypmc { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 数量（部门单位）
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 数量+单位
        /// </summary>
        public string slStr { get; set; }

        /// <summary>
        /// 部门单位
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 单价（部门单位）
        /// </summary>
        public decimal dj { get; set; }

        /// <summary>
        /// 生产厂家
        /// </summary>
        public string ycmc { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal je { get; set; }

        /// <summary>
        /// 剂量
        /// </summary>
        public decimal jl { get; set; }

        /// <summary>
        /// 剂量单位
        /// </summary>
        public string jldw { get; set; }

        /// <summary>
        /// 用法
        /// </summary>
        public string yfmc { get; set; }

        /// <summary>
        /// 医生嘱托
        /// </summary>
        public string yszt { get; set; }

        /// <summary>
        /// 医生名称
        /// </summary>
        public string ysmc { get; set; }

        /// <summary>
        /// 收费时间
        /// </summary>
        public DateTime sfsj { get; set; }

        /// <summary>
        /// 药品分组
        /// </summary>
        public string czh { get; set; }
        /// <summary>
        /// 国家医保代码
        /// </summary>
        public string gjybdm { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string ph { get; set; }
    }
}