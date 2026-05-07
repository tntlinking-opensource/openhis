using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.IDomainServices.DeptStorageManage;
using Newtouch.Domain.ValueObjects.Storage;
using Newtouch.Infrastructure;

namespace Newtouch.DomainServices.DeptStorageManage
{
    /// <summary>
    /// 损益信息
    /// </summary>
    public class KcSyxxDmnService : DmnServiceBase, IKcSyxxDmnService
    {
        public KcSyxxDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 获取损益明细信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="param"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VLossAndProditEntity> SelectLossAndProditInfoList(Pagination pagination, LossAndProditSearchDTO param, string ks, string organizeId)
        {
            var sql = new StringBuilder(@"
SELECT yy.sybz, yy.syyy, sy.Djh djh, wz.name wzmc, kcxx.jj, sy.Bgsj, lb.name lb, wz.gg, cs.name cd, sy.Ph, sy.Yxq, sy.Zrr, sy.productId
, NewtouchHIS_herp.dbo.f_getComplexWzSlandDw(sy.Sysl, sy.Zhyz, dw.name, zxdw.name) syslStr
, (wz.lsj*sy.Zhyz) lsj
, (wz.lsj*sy.Sysl) ljze
FROM dbo.dept_syxx(NOLOCK) sy
INNER JOIN dbo.dept_syyy(NOLOCK) yy ON yy.Id=sy.Syyy
LEFT JOIN  NewtouchHIS_herp.dbo.wz_product(NOLOCK) wz ON wz.Id=sy.productId AND wz.OrganizeId=sy.OrganizeId
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) dw ON dw.Id=sy.UnitId 
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit
INNER JOIN dbo.dept_kcxx(NOLOCK) kcxx ON kcxx.productId=sy.productId AND kcxx.ks=sy.ks AND kcxx.OrganizeId=sy.OrganizeId AND kcxx.pc=sy.pc AND kcxx.ph=sy.Ph
LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId 
LEFT JOIN NewtouchHIS_herp.dbo.gys_supplier(NOLOCK) cs ON cs.Id=wz.supplierId AND cs.OrganizeId=sy.OrganizeId
WHERE sy.OrganizeId=@OrganizeId
AND sy.ks=@ks
AND (yy.sybz=@sybz OR '-1'=@sybz)
AND sy.Bgsj BETWEEN @kssj AND @jssj
");
            var p = new List<DbParameter>
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@ks", ks),
                new SqlParameter("@sybz", param.sybz)
            };
            param.startTime = param.startTime < Constants.MinDateTime ? Constants.MinDateTime : param.startTime;
            param.endTime = param.endTime < Constants.MinDateTime ? Constants.MinDateTime : param.endTime;
            p.Add(new SqlParameter("@kssj", param.startTime));
            p.Add(new SqlParameter("@jssj", param.endTime));
            if (!string.IsNullOrWhiteSpace(param.syyy))
            {
                sql.AppendLine("AND yy.Id=@syyy ");
                p.Add(new SqlParameter("@syyy", param.syyy));
            }
            if (!string.IsNullOrWhiteSpace(param.inputCode))
            {
                sql.AppendLine("AND (wz.py LIKE '%'+@keyWord+'%' OR wz.name LIKE '%'+ @keyWord +'%') ");
                p.Add(new SqlParameter("@keyWord", param.inputCode));
            }
            return QueryWithPage<VLossAndProditEntity>(sql.ToString(), pagination, p.ToArray());
        }
    }
}
