using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Infrastructure.EF;

namespace Newtouch.Domain.IRepository.DeptStorageManage
{
    /// <summary>
    /// 盘点
    /// </summary>
    public interface IKcPdxxRepo : IRepositoryBase<KcPdxxEntity>
    {
        /// <summary>
        /// 根据盘点ID获取盘点信息
        /// </summary>
        /// <param name="pdId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        KcPdxxEntity SelectPdInfByPdId(long pdId, string organizeId);

        /// <summary>
        /// 判断是否有未完成的盘点 （结束）
        /// </summary>
        /// <returns></returns>
        int SelectUnfinishedInventoryByPdId(long pdId, string organizeId);
    }
}