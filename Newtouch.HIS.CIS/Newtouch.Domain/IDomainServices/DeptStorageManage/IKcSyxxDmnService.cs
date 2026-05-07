using System.Collections.Generic;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.ValueObjects.Storage;


namespace Newtouch.Domain.IDomainServices.DeptStorageManage
{
    /// <summary>
    /// 损益
    /// </summary>
    public interface IKcSyxxDmnService
    {
        /// <summary>
        /// 获取损益明细
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="param"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<VLossAndProditEntity> SelectLossAndProditInfoList(Pagination pagination, LossAndProditSearchDTO param, string ks, string organizeId);
    }
}