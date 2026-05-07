using System;
using System.Collections.Generic;
using Newtouch.Common;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.Infrastructure;
using System.Data.SqlClient;
using Newtouch.Core.Common;
using System.Data.Common;
using Newtouch.HIS.Domain.Entity.SystemManage;
using System.Linq;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public class SysChargeItemDmnService : DmnServiceBase, ISysChargeItemDmnService
    {
        public SysChargeItemDmnService(IBaseDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="Pagination"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public IList<SysChargeItemVO> GetPagintionList(string orgId, Pagination pagination,string sfdl, string zt,string keyword = null)
        {
            var tableName = "";
            if (orgId.Equals("*"))
            {
                tableName = "xt_sfxm_base";
            }
            else
            {
                tableName = "xt_sfxm";
            }
            var sql = $@"
select a.*, b.dlmc sfdlmc, case when a.ybdm IS NULL then '2' when a.ybdm IS NOT NULL AND a.LastYBUploadTime IS NOT NULL AND a.LastYBUploadTime >= a.LastModifyTime then '1' else '0' end isSynch 
from 
    {tableName} a
left join xt_sfdl b
on a.sfdlCode = b.dlCode and a.OrganizeId = b.OrganizeId
where a.OrganizeId = @orgId";
          
            var pars = new List<SqlParameter>() {
                new SqlParameter("@orgId", orgId),
            };
           
                
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (a.sfxmmc like @searchKeyword or a.sfxmCode like @searchKeyword or a.ybdm like @searchKeyword or a.gjybdm like @searchKeyword or a.py like @searchKeyword or a.sccj like @searchKeyword)";
                pars.Add(new SqlParameter("@searchKeyword", "%" + keyword + "%"));
            }
            if (sfdl != "")
            {
                sql += " and a.sfdlCode=@sfdl";
                pars.Add(new SqlParameter("@sfdl", sfdl));
            }
            if (zt != "")
            {
                sql += " and a.zt=@zt";
                pars.Add(new SqlParameter("@zt", zt));
            }
            return this.QueryWithPage<SysChargeItemVO>(sql, pagination, pars.ToArray());
        }

        /// <summary>
        /// 获取医保病人性质 自负/超限比例
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<ShybBrxzblVo> Getybbxbldata(string keyword,string orgId)
        {
            string isczsql = @" select xmcode from NewtouchHIS_Base..[Sh_Ybfyxzbl] where  xmcode=@keyword ";
            var iscz= this.FindList<string>(isczsql, new DbParameter[]
                {
                   new SqlParameter("@keyword", keyword)
                });
            string sql = "";
            var pars = new List<SqlParameter>() { new SqlParameter("@orgId", orgId) };
            if (iscz.Count ==0)
            {
                 sql = @" select a.Code,'' xmcode,'' xmmc,a.Id xzId,a.Name xzmc,0.00 zfbl,0.00 fyxe,0.00 cxbl 
from NewtouchHIS_Base..Sys_ItemsDetail a
WHERE  a.OrganizeId=@orgId  
and ItemId in (select id from NewtouchHIS_Base..Sys_Items where code='shybbrxz' and zt=1 ) order by px desc";
            }
            else {
               sql = @"SELECT a.Code ,b.xmcode,b.xmmc,a.Id xzId,a.Name xzmc,isnull(b.zfbl,0.00) zfbl,isnull(b.fyxe,0.00) fyxe,isnull(b.cxbl,0.00) cxbl 
                           FROM NewtouchHIS_Base..Sys_ItemsDetail a
                           LEFT JOIN NewtouchHIS_Base..[Sh_Ybfyxzbl] b on a.Id = b.xzId and b.xmcode = @keyword
                           WHERE ItemId = 'b3a4e2ab-e8c5-4c63-8b26-e8c0c8500bf2'  and a.OrganizeId =@orgId  order by px desc";
                pars.Add(new SqlParameter("@keyword", keyword));
            }

            
            return this.FindList<ShybBrxzblVo>(sql,pars.ToArray());

        }
        public void SaveYbblValue(List<Sh_YbfyxzblEntity> entity, string xmbm, string xmmc,string orgId,string CreatorCode)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var isczList = db.IQueryable<Sh_YbfyxzblEntity>().Where(p => p.xmcode == xmbm && p.zt == "1" && p.OrganizeId == orgId).ToList();
                if (isczList.Count > 0) {
                    foreach (var item in isczList)
                    {
                        db.Delete<Sh_YbfyxzblEntity>(p => p.Id == item.Id);
                    };
                }
                foreach (var item in entity)
                {
                    Sh_YbfyxzblEntity data = new Sh_YbfyxzblEntity();
                    data = item;
                    data.xmcode = xmbm;
                    data.xmmc = xmmc;
                    data.OrganizeId = orgId;
                    data.zt = "1";
                    data.CreateTime = DateTime.Now;
                    data.CreatorCode = CreatorCode;
                    db.Insert(data);
                };
                db.Commit();
            }
        }

    }
}
