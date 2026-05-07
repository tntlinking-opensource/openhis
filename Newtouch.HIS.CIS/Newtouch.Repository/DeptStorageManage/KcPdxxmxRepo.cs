using System.Collections.Generic;
using FrameworkBase.MultiOrg.Infrastructure;

using FrameworkBase.MultiOrg.Repository;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Domain.IRepository.DeptStorageManage;


namespace Newtouch.Herp.Repository
{
    /// <summary>
    /// 盘点明细
    /// </summary>
    public class KcPdxxmxRepo : RepositoryBase<KcPdxxmxEntity>, IKcPdxxmxRepo
    {
        public KcPdxxmxRepo(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 盘点保存时 变更数量
        /// </summary>
        /// <param name="inventoryInfoList"></param>
        public void UpdateSlBySaveInventoryInfo(List<SaveInventoryDTO> inventoryInfoList)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var item in inventoryInfoList)
                {
                    var entity = db.FindEntity<KcPdxxmxEntity>(p => p.Id == item.pdmxId);
                    entity.sjsl = item.sjsl;
                    entity.Modify();
                    db.Update(entity);
                }
                db.Commit();
            }
        }

        /// <summary>
        /// 盘点保存时 变更数量
        /// </summary>
        /// <param name="inventoryInfoList"></param>
        public void UpdateSlBySaveInventoryInfoNoPc(List<SaveInventoryDTO> inventoryInfoList)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                const string sql = @"
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#tmpData') and type='U')
BEGIN
	DROP TABLE #tmpData; 
END
SELECT xm.pc,xm.ph,xm.sjsl 
INTO #tmpData 
FROM dbo.kc_pdxxmx xm 
INNER JOIN dbo.kc_pdxx pd ON pd.Id=xm.pdId 
WHERE xm.productId=@proId
AND pd.Id=@pdId
AND pd.OrganizeId=@OrganizeId
AND pd.warehouseId=@warehouseId ";
                foreach (var item in inventoryInfoList)
                {
                    var entity = db.FindEntity<KcPdxxmxEntity>(p => p.Id == item.pdmxId);
                    entity.sjsl = item.sjsl;
                    entity.Modify();
                    db.Update(entity);
                }
                db.Commit();
            }
        }
    }
}
