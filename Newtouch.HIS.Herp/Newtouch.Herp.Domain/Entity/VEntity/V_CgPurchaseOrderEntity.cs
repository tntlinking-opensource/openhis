using System;

namespace Newtouch.Herp.Domain.Entity.VEntity
{
    /// <summary>
    /// 采购计划主表
    /// </summary>
    public class VCgPurchaseOrderEntity
    {
        /// <summary>
        /// 采购计划单号
        /// </summary>
        public string cgdh { get; set; }

        /// <summary>
        /// 入库部门Code
        /// </summary>
        public string rkbmCode { get; set; }

        /// <summary>
        /// 入库部门名称
        /// </summary>
        public string rkbmmc { get; set; }

        /// <summary>
        /// 审核状态 0-待审核； 1-审核通过； 2-审核不通过；3-已作废；4-暂存
        /// </summary>
        public int auditState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime LastModifyTime { get; set; }

        /// <summary>
        /// 修改者账号
        /// </summary>
        public string LastModifierCode { get; set; }

        /// <summary>
        /// 修改者名称
        /// </summary>
        public string LastModifierName { get; set; }
    }

    /// <summary>
    /// 采购计划明细
    /// </summary>
    public class VCgPurchaseOrderDetailEntity
    {
        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 计划采购数 sl*zhyz=最小单位数量
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 实际采购数 sjsl*zhyz=最小单位数量
        /// </summary>
        public int sjsl { get; set; }

        /// <summary>
        /// 计划采购数（带单位）
        /// </summary>
        public string slStr { get; set; }

        /// <summary>
        /// 实际采购数（带单位）
        /// </summary>
        public string sjslStr { get; set; }

        /// <summary>
        /// 继续采购 sl-sjsl
        /// </summary>
        public int jxcgs { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 单位ID
        /// </summary>
        public string unitId { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string unitName { get; set; }

        /// <summary>
        /// 最小单位名称
        /// </summary>
        public string zxdwmc { get; set; }

        /// <summary>
        /// 物资类别名称
        /// </summary>
        public string lbmc { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string brand { get; set; }

        /// <summary>
        /// 生产厂家名称
        /// </summary>
        public string sccj { get; set; }

        /// <summary>
        /// 进价  jj/zhyz=最小单位进价
        /// </summary>
        public decimal jj { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 采购计划明细ID，cg_purchaseOrderDetail表主键
        /// </summary>
        public long pdId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string gysmc { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        public string gysId { get; set; }

        /// <summary>
        /// 最小起订数(最小单位)
        /// </summary>
        public int zxqds { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string supplierId { get; set; }
        public string supplierName { get; set; }
    }
}