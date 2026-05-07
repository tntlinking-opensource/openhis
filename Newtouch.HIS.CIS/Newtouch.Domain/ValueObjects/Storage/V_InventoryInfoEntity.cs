using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.ValueObjects.Storage
{
    /// <summary>
    /// 盘点信息
    /// </summary>
    public class VInventoryInfoEntity
    {
        /// <summary>
        /// 物资名称
        /// </summary>
        public string wzmc { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string gg { get; set; }

        /// <summary>
        /// 理论数（带单位）
        /// </summary>
        public string llsl { get; set; }
        
        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 部门单位数量
        /// </summary>
        public int deptSjsl { get; set; }

        /// <summary>
        /// 部门单位
        /// </summary>
        public string deptdw { get; set; }

        /// <summary>
        /// 最小单位数量
        /// </summary>
        public int minSjsl { get; set; }

        /// <summary>
        /// 最小单位
        /// </summary>
        public string zxdw { get; set; }

        /// <summary>
        /// 转化因子
        /// </summary>
        public int zhyz { get; set; }

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
        /// 零售单价
        /// </summary>
        public decimal lsj { get; set; }

        /// <summary>
        /// 理论零售总额
        /// </summary>
        public decimal lllsje { get; set; }

        /// <summary>
        /// 实际零售总额
        /// </summary>
        public decimal sjlsje { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string py { get; set; }

        /// <summary>
        /// 盘点明细ID
        /// </summary>
        public long pdmxId { get; set; }
        
        /// <summary>
        /// 盘点创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
    }
}
