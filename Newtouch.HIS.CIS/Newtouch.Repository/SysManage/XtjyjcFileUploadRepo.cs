using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.Repository.SysManage
{
    public class XtjyjcFileUploadRepo : RepositoryBase<XtjyjcFileUploadEntity>, IXtjyjcFileUploadRepo
    {
        public XtjyjcFileUploadRepo(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }
        public void SubmitForm(List<XtjyjcFileUploadEntity> entityList)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var item in entityList)
                {
                    var reportList = this.IQueryable().Where(p => p.Sqdh == item.Sqdh && p.FileName==item.FileName && p.OrganizeId == item.OrganizeId && p.zt == "1").FirstOrDefault();
                    if (reportList != null) {
                        //reportList.zt = "0";
                        //db.Update(reportList);
                        db.Delete(reportList);
                    }
                        
                    XtjyjcFileUploadEntity entity = item;
                    entity.Create(true);
                    db.Insert(entity);
                }
                db.Commit();
            }
        }
        public void DeleteForm(string keyValue)
        {
            var entity = this.FindEntity(keyValue);
            this.Delete(entity);
        }
    }
}
