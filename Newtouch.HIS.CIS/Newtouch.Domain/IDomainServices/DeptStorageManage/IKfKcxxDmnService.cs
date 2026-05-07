using System.Collections.Generic;
using Newtouch.Domain.ValueObjects.Storage;

namespace Newtouch.Domain.IDomainServices.DeptStorageManage
{
    /// <summary>
    /// 库存操作
    /// </summary>
    public interface IKfKcxxDmnService
    {
        /// <summary>
        /// 修改库存数量
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="pc"></param>
        /// <param name="ph"></param>
        /// <param name="warehouseId"></param>
        /// <param name="sl">最小单位数量</param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns>返回受影响行</returns>
        int UpdateKcsl(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode);

        // /// <summary>
        // /// 修改库存数量
        // /// </summary>
        // /// <param name="updateKcslDto"></param>
        // /// <returns></returns>
        // string UpdateKcsl(List<UpdateKcslDTO> updateKcslDto);

        /// <summary>
        /// 追加库存，不修改冻结数量 返回影响行
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="pc"></param>
        /// <param name="ph"></param>
        /// <param name="warehouseId"></param>
        /// <param name="sl">最小单位数量  必须为正数</param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns>返回影响行</returns>
        int AddKcsl(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode);
        
        // // /// <summary>
        // // /// 修改库存数量，不修改冻结数量
        // // /// </summary>
        // // /// <param name="updateKcslDto"></param>
        // // /// <returns></returns>
        // // string JustSubtractKcsl(List<UpdateKcslDTO> updateKcslDto);
        // //
        // //
        // // // /// <summary>
        // // // /// 修改库存数量，不修改冻结数量
        // // // /// </summary>
        // // // /// <param name="updateKcslDto"></param>
        // // // /// <param name="db"></param>
        // // // /// <returns></returns>
        // // // string JustSubtractKcslNoTrans(List<UpdateKcslDTO> updateKcslDto, EFDbTransaction db = null);
        // // //
        // // // /// <summary>
        // // // /// 修改库存数量，不修改冻结数量
        // // // /// </summary>
        // // // /// <param name="updateKcslDto"></param>
        // // // /// <param name="db"></param>
        // // // /// <returns></returns>
        // // // string JustSubtractKcsl(UpdateKcslDTO updateKcslDto, EFDbTransaction db = null);
        //
        // /// <summary>
        // /// 减库存数量、冻结数量（出库可用）
        // /// </summary>
        // /// <param name="productId"></param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="warehouseId"></param>
        // /// <param name="sl"></param>
        // /// <param name="organizeId"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // string SubtractKcslAndDjsl(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode);
        //
        // /// <summary>
        // /// 解冻
        // /// </summary>
        // /// <param name="productId"></param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="warehouseId"></param>
        // /// <param name="sl">最小单位数量</param>
        // /// <param name="organizeId"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // string Unfreeze(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode);
        //
        // /// <summary>
        // /// 冻结库存
        // /// </summary>
        // /// <param name="sl">最小单位数量</param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="productId"></param>
        // /// <param name="organizeId"></param>
        // /// <param name="warehouseId"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // string FrozenKc(int sl, string pc, string ph, string productId, string organizeId, string warehouseId, string userCode);
        //
        // /// <summary>
        // /// 外部入库 扣库存(作废)
        // /// </summary>
        // /// <param name="sl">最小单位数量</param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="productId"></param>
        // /// <param name="organizeId"></param>
        // /// <param name="warehouseId"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // string WbrkKkc(int sl, string pc, string ph, string productId, string organizeId, string warehouseId, string userCode);
        //
        // /// <summary>
        // /// 撤销入库（直接出库作废）
        // /// </summary>
        // /// <param name="sl">最小单位数量</param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="productId"></param>
        // /// <param name="organizeId"></param>
        // /// <param name="rkbm"></param>
        // /// <param name="ckbm"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // string RevokeInstorage(int sl, string pc, string ph, string productId, string organizeId, string rkbm, string ckbm, string userCode);

        /// <summary>
        /// 获取库存信息 结转用
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<VKcxxEntity> GetKcxxForJz(string warehouseId, string organizeId);
    }
}