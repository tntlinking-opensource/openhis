using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;

namespace Newtouch.Domain.Entity.Outpatient
{
    [Table("xt_blmbzt")]
    public class CfTemplateGroupPackageEntity: IEntity<CfTemplateGroupPackageEntity>
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public string mbztId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string mbId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string mbmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ztId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ztmc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CreatorCode { get; set; }

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
        
        public int cflx { get; set; }

    }
}