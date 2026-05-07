namespace Newtouch.Domain.DTO.InputDto
{
    /// <summary>
    /// 修改库存参数
    /// </summary>
    public class UpdateKcslDTO
    {
        /// <summary>
        /// 物资ID
        /// </summary>
        public string productId { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        public string pc { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string ph { get; set; }

        /// <summary>
        /// 目标库房
        /// </summary>
        public string ks { get; set; }

        /// <summary>
        /// 最小单位数量
        /// </summary>
        public int sl { get; set; }

        /// <summary>
        /// 组织结构
        /// </summary>
        public string organizeId { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string userCode { get; set; }
    }
}
