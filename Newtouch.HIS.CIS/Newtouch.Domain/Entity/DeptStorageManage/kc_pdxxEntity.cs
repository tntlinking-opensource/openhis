using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;

namespace Newtouch.Domain.Entity.DeptStorageManage
{
    /// <summary>
    /// 库存盘点表
    /// </summary>
    [Table("dept_pdxx")]
    public class KcPdxxEntity : IEntity<KcPdxxEntity>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 组织机构Id（有具体业务的医院）
        /// </summary>
        public string OrganizeId { get; set; }

        /// <summary>
        /// 库房ID
        /// </summary>
        public string warehouseId { get; set; }

        /// <summary>
        ///  开始时间
        /// </summary>
        public DateTime kssj { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? jssj { get; set; }

        /// <summary>
        /// 盘点方式
        /// </summary>
        public short pdfs { get; set; }

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
