using FrameworkBase.MultiOrg.Infrastructure;

using Newtouch.Infrastructure;
using Newtouch.Infrastructure.EF;
using System.Linq;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Domain.IRepository.DeptStorageManage;

namespace Newtouch.Herp.Repository
{
    /// <summary>
    /// 损益
    /// </summary>
    public class KcSyxxRepo : RepositoryBase<KcSyxxEntity>, IKcSyxxRepo
    {
        public KcSyxxRepo(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }
    }
}
