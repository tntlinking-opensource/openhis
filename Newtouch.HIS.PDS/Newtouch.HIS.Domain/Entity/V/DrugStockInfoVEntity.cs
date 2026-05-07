using System;

namespace Newtouch.HIS.Domain.Entity.V
{
    /// <summary>
    /// 药品库存信息 药品选择用
    /// </summary>
    public class DrugStockInfoVEntity
    {
        /// <summary>
        /// 药品代码
        /// </summary>
        public string ypdm { get; set; }

        /// <summary>
        /// 药品名称
        /// </summary>
        public string ypmc { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string py { get; set; }

        /// <summary>
        /// 大类名称
        /// </summary>
        public string dlCode { get; set; }

        /// <summary>
        /// 大类名称
        /// </summary>
        public string dlmc { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime? yxq { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int? kcsl { get; set; }

        /// <summary>
        /// 部门数量
        /// </summary>
        public int? sl { get; set; }

        /// <summary>
        /// 带单位部门数量
        /// </summary>
        public string slStr { get; set; }

        /// <summary>
        /// 带单位部门库存数量
        /// </summary>
        public string kcslStr { get; set; }

        /// <summary>
        /// 带单位部门冻结数量
        /// </summary>
        public string djslStr { get; set; }

        /// <summary>
        /// 包装数单位
        /// </summary>
        public int? bzdwsl { get; set; }

        /// <summary>
        /// 部门单位
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 包装数单位
        /// </summary>
        public string bzdw { get; set; }

        /// <summary>
        /// 最小单位
        /// </summary>
        public string zxdw { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int? zhyz { get; set; }

        /// <summary>
        /// 包装数
        /// </summary>
        public int? bzs { get; set; }

        /// <summary>
        /// 部门批发价
        /// </summary>
        public decimal? pfj { get; set; }

        /// <summary>
        /// 批发总额
        /// </summary>
        public decimal? pjze { get; set; }

        /// <summary>
        /// 部门零售价
        /// </summary>
        public decimal? lsj { get; set; }

        /// <summary>
        /// 最小单位零售价
        /// </summary>
        public decimal? zxdwlsj { get; set; }

        /// <summary>
        /// 零售总额
        /// </summary>
        public decimal? lsze { get; set; }

        /// <summary>
        /// 进价总额
        /// </summary>
        public decimal? jjze { get; set; }

        /// <summary>
        /// 药库批发价
        /// </summary>
        public decimal? ykpfj { get; set; }

        /// <summary>
        /// 药库零售价
        /// </summary>
        public decimal? yklsj { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string ph { get; set; }

        /// <summary>
        /// 生产厂商
        /// </summary>
        public string sccj { get; set; }

        /// <summary>
        /// 药厂名称
        /// </summary>
        public string ycmc { get; set; }

        /// <summary>
        /// 可用库存  最小单位
        /// </summary>
        public int? kykc { get; set; }

        /// <summary>
        /// 批准文号
        /// </summary>
        public string pzwh { get; set; }

        /// <summary>
        /// 零售价单价单位
        /// </summary>
        public string lsjdjdw { get; set; }

        /// <summary>
        /// 批发单价单位
        /// </summary>
        public string pfjdjdw { get; set; }

        /// <summary>
        /// 进价单价单位
        /// </summary>
        public string jjdjdw { get; set; }

        /// <summary>
        /// 最小单位进价
        /// </summary>
        public decimal? zxdwjj { get; set; }

        /// <summary>
        /// 包装单位进价
        /// </summary>
        public decimal? bzdwjj { get; set; }

        /// <summary>
        /// 部门单位进价
        /// </summary>
        public decimal? bmdwjj { get; set; }

        /// <summary>
        /// 库存状态  0-无效  1-有效
        /// </summary>
        public string kczt { get; set; }

		/// <summary>
		/// 供应商名称
		/// </summary>
		public string gysmc { get; set; }
        /// <summary>
        /// 国家医保代码
        /// </summary>
        public string gjybdm { get; set; }

        public string fph { get; set; }
        public int? kcyjz { get; set; }
	}
}
