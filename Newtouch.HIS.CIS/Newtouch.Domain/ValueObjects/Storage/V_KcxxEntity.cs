using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects.Storage
{
    /// <summary>
    /// 库存信息 结转用
    /// </summary>
    public class VKcxxEntity
    {
        /// <summary>
        /// 物资ID
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
        public DateTime? yxq { get; set; }

        /// <summary>
        /// 库存数量 最小单位
        /// </summary>
        public int kcsl { get; set; }

        /// <summary>
        /// 部门零售价 转化因子对应单位
        /// </summary>
        public decimal bmlsj { get; set; }

        /// <summary>
        /// 进价 转化因子对应单位
        /// </summary>
        public decimal jj { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }
    }
}
