using FrameworkBase.MultiOrg.Domain.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Domain.Entity
{

    /// <summary>
    /// 治疗项目组合
    /// </summary>
    [Table("mz_gh_zlxmzh")]
    public class TreatmentportfolioEntity : IEntity<TreatmentportfolioEntity>
    {

        /// <summary>
        /// 
        /// </summary>
        public string OrganizeId { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zhmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zhcode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? ord { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zlxm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zlxmmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string zt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public System.Decimal price { get; set; }

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
        public string zlxmpy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string sfdl { get; set; }
        



    }
}
