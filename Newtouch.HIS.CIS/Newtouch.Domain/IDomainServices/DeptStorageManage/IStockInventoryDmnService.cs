using System.Collections.Generic;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Domain.ValueObjects.Storage;


namespace Newtouch.Domain.IDomainServices.DeptStorageManage
{
    /// <summary>
    /// 库存盘点
    /// </summary>
    public interface IStockInventoryDmnService
    {
        /// <summary>
        /// 获取挂起盘点时间
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VInventoryDateDropDownEntity> GetHangUpPdDates(string warehouseId, string organizeId);

        /// <summary>
        /// 获取所有盘点时间
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VInventoryDateDropDownEntity> GetPdSj(string warehouseId, string organizeId);

        /// <summary>
        /// 生成盘点信息
        /// </summary>
        string GenerateInventoryInfo(string warehouseId, string organizeId);

        /// <summary>
        /// 获取盘点信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="param"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<VInventoryInfoEntity> QueryInventoryInfoList(Pagination pagination, InventorySearchDTO param, string warehouseId, string organizeId);

        /// <summary>
        /// 获取盘点信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="param"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<VInventoryInfoEntity> QueryInventoryInfoListNoPc(Pagination pagination, InventorySearchDTO param, string warehouseId, string organizeId);
        /// <summary>
        /// 取消盘点 
        /// 删除 盘点信息（xt_yp_pdxx）、盘点信息明细（xt_yp_pdxxmx）
        /// </summary>
        void CancelInventory(long pdId);

        /// <summary>
        /// 结束盘点
        /// </summary>
        /// <returns></returns>
        string EndInventory(long pdId, string organizeId, string creatorCode);

        /// <summary>
        /// 批量保存损益信息
        /// </summary>
        /// <param name="syList"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        int InsertSyxxBatch(List<KcSyxxEntity> syList, string warehouseId, string organizeId);

        /// <summary>
        /// 保存变更后的盘点明细
        /// </summary>
        /// <param name="saveInventoryDto"></param>
        /// <param name="pdId"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        string SaveInventoryDetailNoPc(List<SaveInventoryDTO> saveInventoryDto, long pdId, string warehouseId, string organizeId);
    }
}