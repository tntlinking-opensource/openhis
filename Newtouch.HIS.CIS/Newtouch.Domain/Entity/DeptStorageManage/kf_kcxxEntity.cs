using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrameworkBase.MultiOrg.Domain.Entity;
using Newtouch.Infrastructure.EF.Attributes;

namespace Newtouch.Domain.Entity.DeptStorageManage
{
    /// <summary>
    /// 库存表
    /// </summary>
    [Table("kf_kcxx")]
    public class KfKcxxEntity : IEntity<KfKcxxEntity>
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
        /// 库房ID
        /// </summary>
        public string warehouseId { get; set; }

        /// <summary>
        ///  物资ID
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
        /// 有效期
        /// </summary>
        public DateTime yxq { get; set; }

        /// <summary>
        /// 库存数量 最小单位数量
        /// </summary>
        public int kcsl { get; set; }

        /// <summary>
        /// 冻结数量 最小单位数量
        /// </summary>
        public int djsl { get; set; }

        /// <summary>
        /// 出入库明细ID
        /// </summary>
        public long crkmxId { get; set; }

        /// <summary>
        /// 进价 转化因子对应单位
        /// </summary>
        [DecimalPrecision(11, 4)]
        public decimal jj { get; set; }

        /// <summary>
        /// 转化因子，转化成当前库房单位
        /// </summary>
        public int zhyz { get; set; }

        /// <summary>
        /// 库存锁 0/null：未上锁      >0：已锁
        /// </summary>
        public int? locked { get; set; }

        /// <summary>
        /// 状态
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
    }
}
