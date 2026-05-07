using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;

namespace Newtouch.Domain.Entity.DeptStorageManage
{
    /// <summary>
    /// 库存损益原因
    /// </summary>
    [Table("dept_syyy")]
    public class DeptKcSyyyEntity : IEntity<DeptKcSyyyEntity>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// 损溢原因
        /// </summary>
        public string syyy { get; set; }

        /// <summary>
        /// 0 报损，1 报溢
        /// </summary>
        public string sybz { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string zt { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? px { get; set; }

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
    }
}
