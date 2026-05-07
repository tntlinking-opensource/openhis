using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.Entity.PharmacyDrugStorage;
using Newtouch.HIS.Domain.IRepository.PharmacyDrugStorage;
using Newtouch.HIS.Domain.VO;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.Repository.PharmacyDrugStorage
{

    public class PurchaseRepo : RepositoryBase<PurchaseEntity>, IPurchaseRepo
    {
        public PurchaseRepo(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        public IList<PurchaseVo> GetPurchaseGridJson(Pagination pagination, DateTime kssj, DateTime jssj, string OrganizeId,int ddzt, string yyjhdjh = null, string gysCode = null, string ddbh = null, string ddyy = null)
        {
            var sql =new StringBuilder(@"select  cg.cgId,cg.OrganizeId,cg.ddsj,cg.czlx,cg.yybm,cg.psdbm,cg.ddlx,cg.ddbh,cg.ddbh fph,cg.yyjhdh,
cg.zwdhrq,cg.jls,cg.ddzt,cg.CreateTime,cg.CreatorCode,cg.gysCode,cg.gysName,sum(isnull(cgmx.zje,convert(decimal(18,2),cgsl*cgdj))) zje 
from xt_yp_cg cg (nolock)
left join xt_yp_cgmx cgmx (nolock) on cgmx.cgId=cg.cgId and cgmx.OrganizeId=cg.OrganizeId
where cg.zt=1 and cg.organizeId=@OrganizeId
and cg.createtime BETWEEN @kssj  AND  @jssj+' 23:59:59' ");

            if (ddzt != 0)
            {
                sql.Append(" and ddzt=@ddzt");
            }
            if (!string.IsNullOrWhiteSpace(yyjhdjh))
            {
                sql.Append(" and yyjhdh like @yyjhdh");
            }
            if (!string.IsNullOrWhiteSpace(ddbh))
            {
                sql.Append(" and ddbh like @ddbh");
            }
            if (!string.IsNullOrWhiteSpace(gysCode))
            {
                sql.Append(" and gysCode in (select col from f_split(@gysCode,',')) ");
            }
            if (!string.IsNullOrWhiteSpace(ddyy))
            {
                sql.Append(@" and  not exists (select * from xt_yp_crkmx rkdjmx,xt_yp_crkdj rkdj where rkdjmx.fph=cg.ddbh 

    and rkdj.crkId = rkdjmx.crkId and rkdj.shzt != '2' and rkdj.OrganizeId =@OrganizeId )");
            }
            sql.Append(@" group by cg.cgId,cg.OrganizeId,cg.ddsj,cg.czlx,cg.yybm,cg.psdbm,cg.ddlx,cg.ddbh,cg.yyjhdh,
cg.zwdhrq, cg.jls, cg.ddzt, cg.CreateTime, cg.CreatorCode, cg.gysCode, cg.gysName");
            var parms = new List<SqlParameter>
            {
                new SqlParameter("@OrganizeId", OrganizeId),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@ddzt", ddzt),
                new SqlParameter("@ddbh", ddbh+'%'),
                new SqlParameter("@yyjhdh", yyjhdjh+'%'),
                new SqlParameter("@gysCode", gysCode??""),
            };

            return QueryWithPage<PurchaseVo>(sql.ToString(), pagination, parms.ToArray(), false);
        }

        public void PurchaseDelete(string cgId, string orgId )
        {
                var dbEntity = this.FindEntity(cgId);
                //properties
                dbEntity.zt = "0";
                dbEntity.Modify(cgId);
                this.Update(dbEntity);
           
        }
        /// <summary>
        /// 批量修改审核状态
        /// </summary>
        /// <param name="cgId"></param>
        /// <param name="ddzt"></param>
        /// <param name="orgId"></param>
        public void PurchaseStateUpdate(string cgId,int ddzt, string orgId)
        {
            string sqlstr = "update [xt_yp_cg] set ddzt=@ddzt,LastModifyTime=GETDATE(),LastModifierCode=@userCode  where OrganizeId=@orgId and cgId in (select col from f_split(@cgId,','))";
            ExecuteSqlCommand(sqlstr, new SqlParameter("@cgId", cgId), new SqlParameter("@ddzt", ddzt), new SqlParameter("@userCode", OperatorProvider.GetCurrent().UserCode), new SqlParameter("@orgId", orgId));
            //var dbEntity = this.FindEntity(cgId);
            ////properties
            //dbEntity.ddzt = ddzt;
            //dbEntity.Modify(cgId);
            //this.Update(dbEntity);

        }

        /// <summary>
        /// 更新采购单的订单编号
        /// </summary>
        /// <param name="cgId"></param>
        /// <param name="ddbh"></param>
        /// <param name="orgId"></param>

        public void PurchaseDdbhUpdate(string cgId, string ddbh, string orgId)
        {
            var dbEntity = this.FindEntity(cgId);
            //properties
            dbEntity.ddbh = ddbh;
            dbEntity.Modify(cgId);
            this.Update(dbEntity);

        }
        public string SubmitForm(PurchaseEntity entity,string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                var dbEntity = this.FindEntity(keyValue);
                dbEntity.ddlx = entity.ddlx;
                dbEntity.czlx = entity.czlx;
                dbEntity.jls = entity.jls;
                dbEntity.gysCode = entity.gysCode;
                dbEntity.gysName = entity.gysName;
                dbEntity.Modify(keyValue);
                this.Update(dbEntity);
                return keyValue;
            }
            else
            {
                entity.cgId = Guid.NewGuid().ToString();
                entity.ddsj = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entity.ddzt = 1; //1已保存
                entity.Create(true);
                this.Insert(entity);
                return entity.cgId;
            }
        }

    }
}
