using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using Newtouch.Domain.Entity;
using Newtouch.Domain.Entity.Outpatient;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;

namespace Newtouch.HIS.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class CfTemplateGroupPackageRepo : RepositoryBase<CfTemplateGroupPackageEntity>, ICfTemplateGroupPackageRepo
    {
        public CfTemplateGroupPackageRepo(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
    }
}
