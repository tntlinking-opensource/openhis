using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects.DrugStorage;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Newtouch.HIS.DomainServices.OutOrInStoredOperate
{

    /// <summary>
    /// 直接出库
    /// </summary>
    public class DeliveryDirectDmnService : DmnServiceBase, IDeliveryDirectDmnService
    {
        private readonly IKcxxDmnService kcxxDmnService;
        private readonly ISysMedicineStorageIOReceiptRepo crkdj;
        private readonly ISysMedicineStorageIOReceiptDetailRepo crdjmx;

        public DeliveryDirectDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 提交直接出库
        /// </summary>
        /// <returns></returns>
        public string SubmitDeliveryDirect(SysMedicineStorageIOReceiptEntity dj, List<SysMedicineStorageIOReceiptDetailEntity> mx)
        {
            var result = "";
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var d in mx)
                {
                    var frozenStockResult = kcxxDmnService.FrozenStockReduceByDeliveryDirect(d.Ypdm, dj.OrganizeId, d.pc, d.Ph, d.Sl * d.Ckzhyz, dj.Ckbm);
                    if (!string.IsNullOrWhiteSpace(frozenStockResult))
                    {
                        return frozenStockResult;
                    }
                }
                crkdj.Insert(dj);
                crdjmx.Insert(mx);
                db.Commit();
            }
            return result;
        }
        /// <summary>
        /// 获取入库发票
        /// </summary>
        /// <param name="djlx"></param>
        /// <param name="fph"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public List<BillFphVo> GetRkFphData(int djlx, string fph, DateTime kssj, DateTime jssj)
        {
            var sql=new StringBuilder(@"select  Pdh,Rkbm,yf.yfbmmc Rkmc,Ckbm,gys.gysmc Ckmc,Rksj,mx.Fph,mx.pc,sum(mx.Zje) Zje,sum(kcxx.kcsl)kcsl,sum(kcxx.djsl) kcsl
                        from xt_yp_crkdj zb (nolock)
                        join xt_yp_crkmx mx (nolock) on zb.crkId=mx.crkId
                        join xt_yp_kcxx kcxx (nolock) on kcxx.ypdm=mx.Ypdm and mx.Ph=kcxx.ph and mx.pc=kcxx.pc and kcxx.yfbmCode=@yfbmCode and kcxx.OrganizeId=@orgId 
                        left join NewtouchHIS_Base..xt_ypgys gys on gys.gysCode=zb.Ckbm and gys.OrganizeId=zb.OrganizeId
                        left join NewtouchHIS_Base..V_S_xt_yfbm yf on yf.yfbmCode=zb.Rkbm and yf.OrganizeId=zb.OrganizeId
                        where zb.djlx=@djlx and zb.Rkbm=@yfbmCode and zb.rksj>=@kssj and zb.rksj<=@jssj
                            and zb.OrganizeId=@orgId and zb.zt='1'
                       ");
            var parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@yfbmCode", Constants.CurrentYfbm.yfbmCode));
            parms.Add(new SqlParameter("@orgId", OperatorProvider.GetCurrent().OrganizeId));
            parms.Add(new SqlParameter("@kssj", kssj));
            parms.Add(new SqlParameter("@jssj", jssj));
            parms.Add(new SqlParameter("@djlx", djlx));
            if (!string.IsNullOrWhiteSpace(fph)) {
                sql.AppendLine(@" and mx.Fph like @fph ");
                parms.Add(new SqlParameter("@fph", fph.Trim() + "%"));
            }
            sql.AppendLine(@" group by Pdh,Rkbm,Ckbm,Rksj,Fph,gys.gysmc,yf.yfbmmc,mx.pc
                        having sum(kcxx.kcsl)-sum(kcxx.djsl)>0
                        order by Rksj Desc ");
            return this.FindList<BillFphVo>(sql.ToString(), parms.ToArray());

        }
        /// <summary>
        /// 药库入库发票明细
        /// </summary>
        /// <param name="djlx"></param>
        /// <param name="fph"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public List<BillFphMxVo> GetRkFphMxData(int djlx, string fph, string pc)
        {
            var sql = new StringBuilder(@"select  mx.ypdm,yp.ypmc,dl.dlmc,mx.pc,mx.fph,cast(convert(decimal(12,0),(kcxx.kcsl-kcxx.djsl)/kcxx.zhyz) as varchar)+rkdw slStr,
convert(decimal(12,0),(kcxx.kcsl-kcxx.djsl)/kcxx.zhyz) sl,
rkdw dw,yp.ypgg gg,mx.Ph ph,convert(varchar(10),mx.Yxq,121) yxq,yp.ycmc sccj,
cast(convert(decimal(10,2),mx.Yklsj) as varchar(20))+'元/'+mx.rkdw lsjdjdw,convert(decimal(12,2),yp.lsj*(kcxx.kcsl-kcxx.djsl)/kcxx.zhyz) lsze,
yp.bzs,yp.bzdw,yp.zxdw,convert(decimal(12,0),(kcxx.kcsl-kcxx.djsl)) kykc,mx.Pfj pfj,mx.Lsj lsj,CONVERT(decimal(12,4),yp.lsj/yp.bzs) zxdwlsj,
mx.ykpfj,mx.yklsj,convert(decimal(12,4),mx.jj/mx.Rkzhyz) zxdwjj,mx.jj bzdwjj
from xt_yp_crkdj zb (nolock)
join xt_yp_crkmx mx (nolock) on zb.crkId=mx.crkId
join xt_yp_kcxx kcxx (nolock) on kcxx.ypdm=mx.Ypdm and mx.Ph=kcxx.ph and mx.pc=kcxx.pc and kcxx.yfbmCode=@yfbmCode and kcxx.OrganizeId=@orgId 
left join NewtouchHIS_Base..V_C_xt_yp yp on yp.ypCode =mx.Ypdm and yp.organizeId=@orgId
left join NewtouchHIS_Base..V_S_xt_sfdl dl on dl.dlCode = yp.dlCode and dl.organizeId=@orgId
left join NewtouchHIS_Base..xt_ypgys gys on gys.gysCode=zb.Ckbm and gys.OrganizeId=zb.OrganizeId
left join NewtouchHIS_Base..V_S_xt_yfbm yf on yf.yfbmCode=zb.Rkbm and yf.OrganizeId=zb.OrganizeId
WHERE zb.djlx=@djlx and zb.Rkbm=@yfbmCode
and zb.OrganizeId=@orgId and zb.zt='1'
and mx.Fph=@fph and mx.pc=@pc
                       ");
            var parms = new List<SqlParameter>();
            parms.Add(new SqlParameter("@yfbmCode", Constants.CurrentYfbm.yfbmCode));
            parms.Add(new SqlParameter("@orgId", OperatorProvider.GetCurrent().OrganizeId));
            parms.Add(new SqlParameter("@djlx", djlx));
            parms.Add(new SqlParameter("@fph", fph));
            parms.Add(new SqlParameter("@pc", pc));
            return this.FindList<BillFphMxVo>(sql.ToString(), parms.ToArray());
        }
    }
}
