using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.Entity.PharmacyDrugStorage
{
    [Table("xt_yp_cgmx")]
    public class PurchaseDetailEntity : IEntity <PurchaseDetailEntity>
    {
        [Key]
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
        public string ypName { get; set; }
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
        /// 药企编码/供应商
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
        public string py { get; set; }
        public decimal? zje { get; set; }
    }
    
}
