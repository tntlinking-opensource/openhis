using System;

namespace Newtouch.Herp.Domain.Entity.VEntity
{
    /// <summary>
    /// 物资视图
    /// </summary>
    public class VWzProductEntity
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
        /// 类别、大类ID(必填)
        /// </summary>
        public string typeId { get; set; }

        /// <summary>
        /// 类别、大类(必填)
        /// </summary>
        public string typeName { get; set; }

        /// <summary>
        /// 注册证号
        /// </summary>
        public string zczh { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string py { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string tel { get; set; }

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
        /// 进价
        /// </summary>
        public decimal jj { get; set; }

        /// <summary>
        /// 零售价
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
        /// 单位
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        /// 国家医保代码
        /// </summary>
        public string gjybdm { get; set; }
        /// <summary>
        /// 库存预警值
        /// </summary>
        public int? kcyjz { get; set; }
    }
}
