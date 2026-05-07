using FrameworkBase.MultiOrg.Infrastructure;

using Newtouch.Infrastructure;
using Newtouch.Infrastructure.EF;
using System.Linq;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Domain.IRepository.DeptStorageManage;

namespace Newtouch.Herp.Repository
{
    /// <summary>
    /// 盘点
    /// </summary>
    public class KcPdxxRepo : RepositoryBase<KcPdxxEntity>, IKcPdxxRepo
    {
        public KcPdxxRepo(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 根据盘点ID获取盘点信息
        /// </summary>
        /// <param name="pdId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public KcPdxxEntity SelectPdInfByPdId(long pdId, string organizeId)
        {
            return IQueryable().FirstOrDefault(p => p.Id == pdId && p.OrganizeId == organizeId);
        }

        /// <summary>
        /// 判断是否有未完成的盘点 （结束）
        /// </summary>
        /// <returns></returns>
        public int SelectUnfinishedInventoryByPdId(long pdId, string organizeId)
        {
            return IQueryable().Count(a => a.Id == pdId && a.jssj == null && a.OrganizeId == organizeId);
        }
    }
}
