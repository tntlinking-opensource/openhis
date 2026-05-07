using System;

namespace Newtouch.Herp.Domain.Entity.VEntity
{
    /// <summary>
    /// 采购单信息
    /// </summary>
    public class VCgOrderEntity
    {

        /// <summary>
        /// 订单类型 0：暂存单；1：正式单
        /// </summary>
        public int orderType { get; set; }

        /// <summary>
        /// 采购订单处理流程 -1：拒处理； 0：待处理； 1：备货； 2：配送； 3：签收； 4：完成； 5：拒签 
        /// </summary>
        public int orderProcess { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string orderNo { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string LastModifierName { get; set; }
    }

    /// <summary>
    /// 采购单明细
    /// </summary>
    public class VCgOrderDetailEntity
    {
        /// <summary>
        /// 子订单号
        /// </summary>
        public string subOrderNo { get; set; }

        /// <summary>
        /// 采购计划单号
        /// </summary>
        public string cgdh { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 数量+单位
        /// </summary>
        public string slStr { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string brand { get; set; }

        /// <summary>
        ///进价+单位
        /// </summary>
        public string jjStr { get; set; }

        /// <summary>
        /// 厂家名称
        /// </summary>
        public string sccj { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string gysmc { get; set; }

        /// <summary>
        /// 明细备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public string productId { get; set; }
        /// <summary>
        /// 发票号
        /// </summary>
        public string fph { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string orderNo { get; set; }
        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime orderData { get; set; }
        /// <summary>
        /// 进价总额
        /// </summary>
        public decimal? jjze { get; set; }
    }
    public class VCgOrderRkEntity
    {
        /// <summary>
        /// 订单时间
        /// </summary>
        public DateTime orderDate { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string orderNo { get; set; }
        /// <summary>
        /// 子订单号-同一订单号根据供应商分组
        /// </summary>
        public string subOrderNo { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string gysmc { get; set; }
        /// <summary>
        /// 发票
        /// </summary>
        public string fp { get; set; }
        /// <summary>
        /// 发票
        /// </summary>
        public string fph { get; set; }
        /// <summary>
        /// 采购明细数
        /// </summary>
        public int cgmxs { get; set; }
        /// <summary>
        /// 采购金额
        /// </summary>
        public decimal? cgje { get; set; }
    }

    public class VCgOrderImportEntity
    {
        public string wzmc { get; set; }
        public string lbmc { get; set; }
        public string gjybdm { get; set; }
        public int sl { get; set; }
        public string unitId { get; set; }
        public string unitName { get; set; }
        public decimal jjze { get; set; }
        public string fph { get; set; }
        public string remark { get; set; }
        public int kcsl { get; set; }
        public int kykcsl { get; set; }
        public string slStr { get; set; }
        public string gg { get; set; }
        public string sccj { get; set; }
        public string gysId { get; set; }
        public string gysmc { get; set; }
        public decimal lsj { get; set; }
        public decimal lsze { get; set; }
        public int zhyz { get; set; }
        public string productId { get; set; }
        public decimal jj { get; set; }
        public decimal minlsj { get; set; }
    }
}