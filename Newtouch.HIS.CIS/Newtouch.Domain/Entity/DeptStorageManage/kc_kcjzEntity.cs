using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Infrastructure.EF.Attributes;

namespace Newtouch.Domain.Entity.DeptStorageManage
{
    /// <summary>
    /// 库存_库存结转
    /// </summary>
    [Table("dept_kcjz")]
    public class KcKcjzEntity : IEntity<KcKcjzEntity>
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
        /// 库房代码
        /// </summary>
        public string warehouseId { get; set; }

        /// <summary>
        /// 物资ID(必填)
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string ph { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        public DateTime? yxq { get; set; }

        /// <summary>
        /// 库存数量，最小单位数量
        /// </summary>
        public int kcsl { get; set; }

        /// <summary>
        /// 部门零售价，转化因子对应单位
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal bmlsj { get; set; }

        /// <summary>
        /// 进价，转化因子对应单位
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal jj { get; set; }

        /// <summary>
        /// 转化因子(必填)
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 结转时间
        /// </summary>
        public DateTime jzsj { get; set; }

        /// <summary>
        /// 状态  0:作废；1.有效
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
