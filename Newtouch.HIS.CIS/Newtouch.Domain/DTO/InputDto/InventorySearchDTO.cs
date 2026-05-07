using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Domain.DTO.InputDto
{
    /// <summary>
    /// 盘点查询条件
    /// </summary>
    public class InventorySearchDTO
    {
        /// <summary>
        /// 盘点ID
        /// </summary>
        public long pdId { get; set; }

        /// <summary>
        /// 盘点时间
        /// </summary>
        public string pdsj { get; set; }

        /// <summary>
        /// 关键字  物资名称/拼音
        /// </summary>
        public string keyWord { get; set; }

        /// <summary>
        /// 物资状态 0：无效  1：有效
        /// </summary>
        public string wzzt { get; set; }

        /// <summary>
        /// 列别ID
        /// </summary>
        public string lb { get; set; }

        /// <summary>
        /// 库存显示 -1或空:全部
        /// </summary>
        public int kcxs { get; set; }

    }

    /// <summary>
    /// 保存盘点
    /// </summary>
    public class SaveInventoryDTO
    {
        /// <summary>
        /// 盘点明细ID
        /// </summary>
        public long? pdmxId { get; set; }

        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 实际数量
        /// </summary>
        public int sjsl { get; set; }
    }
}
