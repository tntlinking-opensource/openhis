using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Infrastructure.EF.Attributes;

namespace Newtouch.Herp.Domain.Entity
{
    /// <summary>
    /// 物资
    /// </summary>
    [Table("wz_product")]
    public class WzProductEntity : IEntity<WzProductEntity>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// 组织机构Id（有具体业务的医院）
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 物资代码
        /// </summary>
        public string productCode { get; set; }

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
        /// 最小单位
        /// </summary>
        public string minUnit { get; set; }

        /// <summary>
        /// 最小起订数(最小单位)
        /// </summary>
        public int? zxqds { get; set; }

        /// <summary>
        /// 进价，最小单位 
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal jj { get; set; }

        /// <summary>
        /// 零售价，最小单位 
        /// </summary>
        [DecimalPrecision(11, 4)]
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
		[DecimalPrecision(10, 2)]
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
        /// <summary>
        /// 库存预警值
        /// </summary>
        public int? kcyjz { get; set; }
	}
}
