using System.Collections.Generic;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.ValueObjects.Storage;


namespace Newtouch.Domain.IDomainServices.DeptStorageManage
{
    /// <summary>
    /// 库存结转
    /// </summary>
    public interface IKcKcjzDmnService
    {
        /// <summary>
        /// 获取结转明细
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="jzsj"></param>
        /// <param name="keyWord"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<VCarryOverDetailEntity> SelectCarryOverDetail(Pagination pagination, string jzsj, string keyWord, string warehouseId, string organizeId);

        /// <summary>
        /// 结转
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        string CarryOverProduct(string warehouseId, string organizeId, string userCode);

        /// <summary>
        /// 进销存统计
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="searchParam"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<VPsiStatisticsEntity> GetPsiStatistics(Pagination pagination, PsiStatisticsDTO searchParam, string warehouseId, string organizeId);
    }
}