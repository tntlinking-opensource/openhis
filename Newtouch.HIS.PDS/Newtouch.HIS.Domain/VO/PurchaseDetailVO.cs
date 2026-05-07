using Newtouch.HIS.Domain.Entity.PharmacyDrugStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.VO
{
    public class PurchaseDetailVO
    {
        public string ypmc { get; set; }
        public string cglxmc { get; set; }
        public string splxmc { get; set; }
        public string cgjldwmc { get; set; }
        public string dcpsbsmc { get; set; }
        public string yqmc { get; set; }
        public string psyqmc { get; set; }


        public string cgmxId { get; set; }
        /// <summary>
        /// 采购编号
        /// </summary>
        public string cgId { get; set; }
        public string OrganizeId { get; set; }
        /// <summary>
        /// 药品编号
        /// </summary>
        public string ypCode { get; set; }
        /// <summary>
        /// 顺序号
        /// </summary>
        public int sxh { get; set; }
        /// <summary>
        /// 采购模式
        /// </summary>
        public string cglx { get; set; }
        /// <summary>
        /// 商品类型
        /// </summary>
        public string splx { get; set; }
        /// <summary>
        /// 统编代码
        /// </summary>
        public string zxspbm { get; set; }
        /// <summary>
        /// 采购计量单位
        /// </summary>
        public string cgjldw { get; set; }
        /// <summary>
        /// 规格包装
        /// </summary>
        public string ggbz { get; set; }
        /// <summary>
        /// 采购数量
        /// </summary>
        public decimal cgsl { get; set; }
        /// <summary>
        /// 采购单价
        /// </summary>
        public decimal cgdj { get; set; }
        /// <summary>
        /// 药企编码
        /// </summary>
        public string yqbm { get; set; }
        /// <summary>
        /// 多次配送标识（0不允许  1允许）
        /// </summary>
        public string dcpsbs { get; set; }
        /// <summary>
        /// 备注说明
        /// </summary>
        public string bzsm { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastModifyTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LastModifierCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zt { get; set; }
        public decimal? zje { get; set; }
        public string sccj { get; set; }
    }

    public class PurchaseVo 
    {
        public string cgId { get; set; }
        /// <summary>
        /// 组织机构
        /// </summary>
        public string OrganizeId { get; set; }
        /// <summary>
        /// 订单时间
        /// </summary>
        public string ddsj { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public string czlx { get; set; }

        /// <summary>
        /// 医保编码
        /// </summary>
        public string yybm { get; set; }
        /// <summary>
        /// 配送点编码
        /// </summary>
        public string psdbm { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public string ddlx { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string ddbh { get; set; }
        /// <summary>
        /// 医院计划单号
        /// </summary>
        public string yyjhdh { get; set; }
        /// <summary>
        /// 最晚到货日期
        /// </summary>
        public string zwdhrq { get; set; }
        /// <summary>
        /// 记录数
        /// </summary>
        public int jls { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public int ddzt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatorCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zt { get; set; }
        public string gysCode { get; set; }
        public string gysName { get; set; }
        public decimal? zje { get; set; }
        public string fph { get; set; }
    }
}
