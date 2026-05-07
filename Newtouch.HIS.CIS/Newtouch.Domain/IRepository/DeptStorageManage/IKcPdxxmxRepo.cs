using System.Collections.Generic;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Infrastructure.EF;

namespace Newtouch.Domain.IRepository.DeptStorageManage
{
    /// <summary>
    /// 库存明细
    /// </summary>
    public interface IKcPdxxmxRepo : IRepositoryBase<KcPdxxmxEntity>
    {
        /// <summary>
        /// 盘点保存时 变更数量
        /// </summary>
        /// <param name="inventoryInfoList"></param>
        void UpdateSlBySaveInventoryInfo(List<SaveInventoryDTO> inventoryInfoList);
    }
}