using System;

namespace Newtouch.Herp.Domain.Entity.VEntity
{
    /// <summary>
    /// 物资
    /// </summary>
    public class VProductInfoEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 组织机构Id（有具体业务的医院）
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 物资名称(必填)
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 类别、大类(必填)
        /// </summary>
        public string typeId { get; set; }

        /// <summary>
        /// 注册证号
        /// </summary>
        public string zczh { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string py { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string imageUrl { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string brand { get; set; }

        /// <summary>
        /// 型号规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        public string supplierId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string supplierName { get; set; }

        /// <summary>
        /// 最小单位
        /// </summary>
        public string minUnit { get; set; }

        /// <summary>
        /// 最小起订数
        /// </summary>
        public int? zxqds { get; set; }

        /// <summary>
        /// 进价，最小单位 
        /// </summary>
        public decimal jj { get; set; }

        /// <summary>
        /// 零售价 最小单位
        /// </summary>
        public decimal lsj { get; set; }

        /// <summary>
        /// 是否零库存
        /// </summary>
        public string sflkc { get; set; }

        /// <summary>
        /// 是否复用
        /// </summary>
        public string sffy { get; set; }

        /// <summary>
        /// 是否跟台
        /// </summary>
        public string sfgt { get; set; }

        /// <summary>
        /// 状态  0:作废；1.有效
        /// </summary>
        public string zt { get; set; }

        /// <summary>
        /// 物资代码
        /// </summary>
        public string productCode { get; set; }

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

		/// <summary>
		/// 物资细分代码(采购)
		/// </summary>
		public string hcgjybdm { get; set; }

		/// <summary>
		/// 医疗物资标志(同步到BASE库中) 1:医疗物资 0: 其他物资(不需要同步)
		/// </summary>
		public string iswzsame { get; set; }

		/// <summary>
		/// 自负性质
		/// </summary>
		public string zfxz { get; set; }

		/// <summary>
		/// 自负比例
		/// </summary>
		public decimal? zfbl { get; set; }

		/// <summary>
		/// 国家医保代码
		/// </summary>
		public string gjybdm { get; set; }

		/// <summary>
		/// 省医保代码
		/// </summary>
		public string ybdm { get; set; }
        
        /// <summary>
        /// 账簿类别
        /// </summary>
        public string zblb { get; set; }
        /// <summary>
        /// 核算类别
        /// </summary>
        public string hslb { get; set; }
        public int? kcyjz { get; set; }
	}

    /// <summary>
    /// 产品信息
    /// </summary>
    public class VProductInfoDo
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string lbmc { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string brand { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 最小单位名称
        /// </summary>
        public string zxdwmc { get; set; }

        /// <summary>
        /// 最小单位ID
        /// </summary>
        public string unitId { get; set; }

        /// <summary>
        /// 生产厂家
        /// </summary>
        public string sccj { get; set; }
    }

    /// <summary>
    /// 物资单位
    /// </summary>
    public class ProductUnit
    {

        /// <summary>
        /// 单位ID
        /// </summary>
        public string unitId { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string dwmc { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public string productId { get; set; }
    }
}
