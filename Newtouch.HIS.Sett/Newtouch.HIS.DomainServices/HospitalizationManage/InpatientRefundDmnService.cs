using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices.HospitalizationManage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Newtouch.HIS.DomainServices.HospitalizationManage
{
    public class InpatientRefundDmnService : DmnServiceBase, IInpatientRefundDmnService
    {
        public InpatientRefundDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        /// <summary>
        /// 保存退费
        /// </summary>
        /// <param name="xmjfbEntitylist"></param>
        /// <param name="ypjfbEntitylist"></param>
        public string SaveRefund(List<HospItemBillingEntity> xmjfbEntitylist, List<HospDrugBillingEntity> ypjfbEntitylist, string zyh, string orgId)
        {
            string result = "";
            try {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (xmjfbEntitylist != null && xmjfbEntitylist.Count > 0)
                    {
                        foreach (var item in xmjfbEntitylist)
                        {
                            var ktsl = ValidTfsl(orgId, zyh, item.cxzyjfbbh);
                            if (ktsl > Convert.ToDecimal(0.00))
                            {
                                db.Insert(item);
                            }
                        }
                    }
                    if (ypjfbEntitylist != null && ypjfbEntitylist.Count > 0)
                    {
                        foreach (var item in ypjfbEntitylist)
                        {
                            var ktsl = ValidTfsl(orgId, zyh, item.cxzyjfbbh);
                            if (ktsl > Convert.ToDecimal(0.00))
                            {
                                db.Insert(item);
                            }
                        }
                    }
                    db.Commit();
                }
            } catch (Exception e) {
                result ="退费失败："+ e.Message;
                throw;
            }
            return result;
        }
        /// <summary>
        /// 验证可退数量
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <param name="jfbbh"></param>
        /// <returns></returns>
        public decimal ValidTfsl(string orgId,string zyh ,int jfbbh)
        {
            string sql = @"  select isnull(sum(sl),0.00) ktsl from(
 select sl from zy_ypjfb where (jfbbh=@jfbbh or cxzyjfbbh=@jfbbh) and OrganizeId=@orgId and zt='1'
 union all
 select sl from zy_xmjfb where (jfbbh=@jfbbh or cxzyjfbbh=@jfbbh) and OrganizeId=@orgId and zt='1') d";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@zyh", zyh));
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@jfbbh", jfbbh));
            var vo = this.FirstOrDefault<decimal>(sql, pars.ToArray());
            return vo;
        }

    }
}
