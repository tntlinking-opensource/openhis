using System.Collections.Generic;
using System.Web;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.ValueObjects.Storage;


namespace Newtouch.Application.Interface
{
    /// <summary>
    /// 库存盘点
    /// </summary>
    public interface IStockInventoryApp
    {
        /// <summary>
        /// 开始盘点
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        VInventoryDateDropDownEntity StartInventory(string warehouseId, string organizeId);

        /// <summary>
        /// 盘点保存
        /// </summary>
        /// <param name="inventoryInfoList"></param>
        /// <param name="pdId">盘点单ID</param>
        /// <param name="noPc">  0：按批次盘点 1：不按批次盘点</param>
        void SaveInventoryInfo(List<SaveInventoryDTO> inventoryInfoList, long pdId, string noPc = "0");

        /// <summary>
        /// 取消盘点
        /// </summary>
        /// <param name="pdId"></param>
        void CancelInventory(long pdId);

        /// <summary>
        /// 结束盘点
        /// </summary>
        /// <returns></returns>
        void EndInventory(long pdId);

        /// <summary>
        /// 提交损益
        /// </summary>
        /// <param name="plist"></param>
        /// <param name="warehouseId"></param>
        /// <returns>error msg</returns>
        string LossAndProfitSubmit(List<LossAndProditSubmitDTO> plist, string warehouseId);
    }
}