using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Common.Web;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Domain.ViewModels;
using Newtouch.Infrastructure;
using Newtouch.Tools;

namespace Newtouch.DomainServices
{
    public class OrderAuditDmnService : DmnServiceBase, IOrderAuditDmnService
    {
        public readonly IInpatientLongTermOrderRepo _InpatientLongTermOrderRepo;
        public readonly IInpatientSTATOrderRepo _InpatientSTATOrderRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IAllergyRepo _allergyRepo;
        public OrderAuditDmnService(IDefaultDatabaseFactory databaseFactory, IInpatientLongTermOrderRepo InpatientLongTermOrderRepo,
                                    IInpatientSTATOrderRepo InpatientSTATOrderRepo)
            : base(databaseFactory)
        {
            this._InpatientLongTermOrderRepo = InpatientLongTermOrderRepo;
            this._InpatientSTATOrderRepo = InpatientSTATOrderRepo;
        }

        public IList<InpWardPatTreeVO> GetPatWardTree(string staffId)
        {
            string sql = @"select a.OrganizeId, b.bqCode,c.bqmc,d.zyh,d.hzxm
                        from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
	                        [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
	                        [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) 
                        left join [dbo].[zy_cqyz] d with(nolock) on c.organizeid=d.organizeid and d.wardcode=c.bqcode and d.yzzt=0
                        where a.staffid=@staffId and a.zt=1 
                        and a.OrganizeId=b.organizeid and a.staffid=b.staffid
                        and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
                        group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,d.zyh,d.hzxm
                        union
                        select a.organizeid, b.bqcode,c.bqmc,e.zyh,e.hzxm 
                        from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
	                        [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
	                        [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) 
                        left join [dbo].[zy_lsyz] e with(nolock) on c.organizeid=e.organizeid and e.wardcode=c.bqcode and e.yzzt=0
                        where a.staffid=@staffId and a.zt=1 
                        and a.OrganizeId=b.organizeid and a.staffid=b.staffid
                        and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
                        group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,e.zyh,e.hzxm";
            return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@staffId", staffId) });
        }
        /// <summary>
        /// 获取待审核医嘱病区树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetWardTree(string staffId)
        {
            string sql = @"select a.organizeid, b.bqcode,c.bqmc,'' zyh,'' hzxm 
                            from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) 
                            where a.staffid=@staffId and a.zt=1 and b.zt=1 and c.zt=1
                            and a.OrganizeId=b.organizeid and a.staffid=b.staffid
                            and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
                            group by a.organizeid, b.bqcode,c.bqmc ";
            return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@staffId", staffId) });
        }

        /// <summary>
        /// 获取待审核医嘱患者树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatTree(string staffId, string isQX)
        {
            //是否授权护士康复医嘱执行权限
            if (string.IsNullOrWhiteSpace(isQX))
            {
                isQX = "1";
            }
            string sql = @"exec Yzsh_Tree @isQX,@staffId";
            return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@isQX", isQX), new SqlParameter("@staffId", staffId) });
        }

        /// <summary>
        /// 获取待审核医嘱患者树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatTree(string staffId)
        {
            string sql = @"select a.OrganizeId, b.bqCode,c.bqmc,d.zyh,d.hzxm,cw.BedNo,xx.sex,xx.ryrq,xx.birth,
CAST(FLOOR(DATEDIFF(DY, xx.birth, GETDATE()) / 365.25) AS VARCHAR(5)) nl,
CONVERT(VARCHAR(25),CASE DATEDIFF(DAY, xx.ryrq,GETDATE()) WHEN 0 THEN 1 else  DATEDIFF(DAY, xx.ryrq,GETDATE())END ) inHosDays
                            from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock),  
	                            [dbo].[zy_cqyz] d with(nolock) ,[dbo].[zy_cwsyjlk] cw with(nolock),
                                [Newtouch_CIS]..zy_brxxk xx with(nolock) 
                            where a.staffid=@staffId and a.zt=1 and b.zt=1 and c.zt=1 
                            and a.OrganizeId=b.organizeid and a.staffid=b.staffid
                            and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
                            and c.organizeid=cw.organizeid and d.zyh=cw.zyh and cw.zt='1'
                            and c.organizeid=d.organizeid and d.wardcode=c.bqcode and d.yzzt in(0,3)  and d.zt=1
                            and xx.organizeid=cw.organizeid and xx.zyh=cw.zyh and xx.zt='1'
                            group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,d.zyh,d.hzxm,cw.BedNo,xx.sex,xx.ryrq,xx.birth
                            union
                            select a.organizeid, b.bqcode,c.bqmc,e.zyh,e.hzxm ,cw.BedNo,xx.sex,xx.ryrq,xx.birth,
CAST(FLOOR(DATEDIFF(DY, xx.birth, GETDATE()) / 365.25) AS VARCHAR(5)) nl,
CONVERT(VARCHAR(25),CASE DATEDIFF(DAY, xx.ryrq,GETDATE()) WHEN 0 THEN 1 else  DATEDIFF(DAY, xx.ryrq,GETDATE())END ) inHosDays
                            from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock), 
	                            [dbo].[zy_lsyz] e with(nolock) ,[dbo].[zy_cwsyjlk] cw with(nolock),
                                [Newtouch_CIS]..zy_brxxk xx with(nolock)
                            where a.staffid=@staffId and a.zt=1 and b.zt=1 and c.zt=1 
                            and a.OrganizeId=b.organizeid and a.staffid=b.staffid
                            and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
                            and cw.organizeid=e.organizeid and e.zyh=cw.zyh and cw.zt='1'
                            and c.organizeid=e.organizeid and e.wardcode=c.bqcode and e.yzzt in(0,3) and e.zt=1
                            and xx.organizeid=cw.organizeid and xx.zyh=cw.zyh and xx.zt='1'
                            group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,e.zyh,e.hzxm, cw.BedNo,xx.sex,xx.ryrq,xx.birth";
            return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@staffId", staffId) });
        }
        /// <summary>
        /// 获取分页医嘱列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="patList"></param>
        /// <returns></returns>
        public IList<OrderAuditVO> GetOrderAuditYZList(Pagination pagination, string patList, string orgId, string IsRehabAuthtoNurse = null)
        {
            string strkfwhere = "";
            if (string.IsNullOrWhiteSpace(patList))
            {
                return null;
            }
            patList = patList.Trim();
            if (patList.EndsWith(","))
            {
                patList = patList.Substring(0, patList.Length - 1).Trim();
                if (patList.Length == 0)
                {
                    return null;
                }
            }
            if (IsRehabAuthtoNurse == "0") //护士无康复审核授权
            {
                strkfwhere = " and yzlx<>@yzlx ";
            }
            string sql = @"select iszt,zyh,max(yzid) yzid, yzxz,yzxzsm,max(kssj) kssj,xmmc,ypjl,yzmc,yzjl,yfmc,yzpcmc,max(tzsj) tzsj,tzysgh,tzr,Creator,zh,zh1,OrganizeId,yzzt,yzlx,hzxm,yztag,yztagName,isjf,ispscs,yzh
,psjgms,psjg,yfztbs,yply,sum(case when yzlx in ('6','7') then dj*sl else dj end) dj,sum(je) je,case when zh1 is null or zh1='' then  sum(sl) else 1 end sl,slstr,Px
from (
select iszt,a.zyh, a.id yzid,2 yzxz,'长期' yzxzsm,a.kssj,case when ztid is not null then ztmc else a.xmmc end xmmc,a.ypjl,
case when ztid is not null then (case when a.yzzt=3 then '[停]'+a.ztmc else a.ztmc end) else 
	(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end) end as yzmc,  CONCAT(CONVERT(float,ypjl),a.dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc,
a.tzsj,a.tzysgh,a.tzr ,b.Name Creator,a.zh,
case when ztid is not null then yzh else '' end zh1,a.OrganizeId,yzzt,a.yzlx,a.hzxm,
a.yztag,case a.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else a.yztag end yztagName,
isnull(a.isjf,1) isjf,isnull(a.ispscs,'') ispscs,a.yzh,c.Remark psjgms,c.result psjg,yfztbs,yply
,isnull(convert(numeric(18,4),isnull(case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end,sfxm.dj)),0) dj
,convert(numeric(18,2),isnull(convert(numeric(18,4),case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end)*sl,sfxm.dj*sl)) je
,sl,(cast(sl as varchar)+isnull(yp.zycldw,'')) slstr,case when ztid is not null then '1' else a.Px end Px
from  (
select 'N' iszt,1 num,*
from [dbo].[zy_cqyz] a with(nolock) 
where a.zt='1' and a.yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,','))
and a.OrganizeId=@orgId and ztid is null " + strkfwhere + @"
union all
select 'Y' iszt,row_number() over(partition by zyh,ztid,yzh order by createtime desc) num,*
from [dbo].[zy_cqyz] a with(nolock) 
where  a.zt='1' and a.yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,','))
and a.OrganizeId=@orgId and ztid is not null and yfztbs is  null " + strkfwhere + @"
) a 
left join xt_gmxx c with(nolock) on a.id=c.yzid and c.zt='1'
left join [NewtouchHIS_Base].[dbo].[Sys_Staff] b with(nolock) on c.CreatorCode=b.gh and c.OrganizeId=b.OrganizeId
LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on a.ypyfdm = ypyf.yfCode 
LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on a.pcCode = yzpc.yzpcCode and yzpc.OrganizeId=@orgId
LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_sfxm] sfxm on a.xmdm=sfxm.sfxmCode and a.OrganizeId=sfxm.OrganizeId
LEFT JOIN  [NewtouchHIS_Base].[dbo].[xt_yp] yp on a.xmdm=yp.ypCode and a.OrganizeId=yp.OrganizeId
--where num=1
union all
select iszt,a.zyh,a.id yzid,1 yzxz,'临时' yzxzsm,a.kssj,case when ztid is not null then ztmc else a.xmmc end xmmc,a.ypjl,
case when ztid is not null then (case when a.yzzt=3 then '[停]'+a.ztmc else a.ztmc end) else 
	(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end) end as yzmc, CONCAT(CONVERT(float,ypjl),a.dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc,
a.zfsj,a.zfysgh,a.zfr ,b.Name Creator,a.zh,
case when ztId is not null then yzh else '' end zh1,a.OrganizeId,yzzt,a.yzlx,a.hzxm,
a.yztag,case a.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else a.yztag end yztagName,
isnull(a.isjf,1) isjf,a.ispscs,a.yzh,c.Remark psjgms,c.result psjg,yfztbs,yply
,isnull(convert(numeric(18,4),isnull((case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end),sfxm.dj)),0) dj
,case when yzlx='10' then isnull(isnull(yp.lsj*sl*ypjl,sfxm.dj*sl*ypjl),0) else 
convert(numeric(18,2),isnull(convert(numeric(18,4),case when yp.zycldw=yp.bzdw then yp.lsj else yp.lsj/bzs end)*sl,sfxm.dj*sl)) end je,
case when yzlx='10' then cast(isnull(ypjl,1) as int)*sl else sl end sl,
cast(case when yzlx='10' then cast(isnull(ypjl,1) as int)*sl when yzlx in('6','7') then '1' else sl end as varchar)+ isnull(yp.zycldw,'') slstr,
case when ztId is not null then '1' else a.Px end Px
from (
select 'N' iszt,1 num,*
from [dbo].[zy_lsyz] a with(nolock) 
where a.zt='1' and a.yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,','))
and a.OrganizeId=@orgId and ztid is null " + strkfwhere + @"
union all
select 'Y' iszt,row_number() over(partition by zyh,ztid,yzh order by createtime desc) num,*
from [dbo].[zy_lsyz] a with(nolock) 
where a.zt='1' and a.yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,','))
and a.OrganizeId=@orgId and ztid is not null and yfztbs is  null " + strkfwhere + @"
) a 
left join xt_gmxx c with(nolock) on a.id=c.yzid and c.zt='1'
left join [NewtouchHIS_Base].[dbo].[Sys_Staff] b with(nolock) on c.CreatorCode=b.gh and c.OrganizeId=b.OrganizeId 
LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on a.ypyfdm = ypyf.yfCode 
LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on a.pcCode = yzpc.yzpcCode and yzpc.OrganizeId=@orgId
LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_sfxm] sfxm on a.xmdm=sfxm.sfxmCode and a.OrganizeId=sfxm.OrganizeId
LEFT JOIN  [NewtouchHIS_Base].[dbo].[xt_yp] yp on a.xmdm=yp.ypCode and a.OrganizeId=yp.OrganizeId
--where num=1
) ttt where ttt.kssj < getDate()
group by iszt,zyh, yzxz,yzxzsm,xmmc,ypjl,yzmc,yzjl,yfmc,yzpcmc,tzysgh,tzr,Creator,zh,zh1,OrganizeId,yzzt,yzlx,hzxm,yztag,yztagName,isjf,ispscs,yzh
,psjgms,psjg,yfztbs,yply,Px,slstr";

            return this.QueryWithPage<OrderAuditVO>(sql, pagination, new[] {
                new SqlParameter("@zyh", patList),
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@yzlx", IsRehabAuthtoNurse=="1"?"":((int)EnumYzlx.rehab).ToString())
            }, false);
        }

        /// <summary>
        /// 获取康复医嘱列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="patList"></param>
        /// <returns></returns>
        public IList<OrderAuditVO> GetOrderAuditYZList_KF(Pagination pagination, string patList, string orgId, string zxks)
        {
            string sql = @"select a.zyh,a.id yzid,2 yzxz,'长期' yzxzsm,a.kssj,a.xmmc,a.ypjl,
(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end)yznr,a.tzsj,a.tzysgh,a.tzr ,
b.Name Creator,a.zh,a.OrganizeId,yzzt,zxksdm,a.yzlx,a.hzxm
from [dbo].[zy_cqyz] a with(nolock) 
left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId
where a.zt=1 and a.yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
and a.OrganizeId=@orgId and a.yzlx =@kfyz and zxksdm=@zxks  AND Convert(datetime,a.kssj)< Convert(datetime,getDate()) 
union
select a.zyh,a.id yzid,1 yzxz,'临时' yzxzsm,a.kssj,a.xmmc,a.ypjl,
(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end)yznr,a.zfsj,a.zfysgh,a.zfr ,
b.Name Creator,a.zh,a.OrganizeId,yzzt,zxksdm,a.yzlx,a.hzxm
from [dbo].[zy_lsyz] a with(nolock) 
left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId 
where a.zt=1 and a.yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
and a.OrganizeId=@orgId and a.yzlx =@kfyz and zxksdm=@zxks  AND Convert(datetime,a.kssj)< Convert(datetime,getDate())";

            return this.QueryWithPage<OrderAuditVO>(sql, pagination, new[] {
                new SqlParameter("@zyh", patList),
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@kfyz", (int)EnumYzlx.rehab),
                new SqlParameter("@zxks", zxks??""),
            }, false);
        }
        /// <summary>
        /// 获取分页医嘱列表by医嘱性质/长期或临时
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="patList"></param>
        /// <param name="Yzxz"></param>
        /// <returns></returns>
        public IList<OrderAuditVO> GetOrderAuditYZListbyYzxz(string orgId, string patList, int Yzxz)
        {
            string sql = "";
            if (Yzxz == Convert.ToInt32(EnumYzxz.Cq))
            {
                sql = @"select zyh,id yzid,2 yzxz,'长期' yzxzsm,kssj,xmmc,ypjl,yznr,tzsj,tzysgh,tzr ,''Creator,a.zh,a.OrganizeId,yzzt,a.yzlx,a.hzxm
                    from [dbo].[zy_cqyz] with(nolock) 
                    where OrganizeId=@orgId and a.zt=1 and yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
                    ";
            }
            else if (Yzxz == Convert.ToInt32(EnumYzxz.Ls))
            {

                sql = @"
                    select zyh,id yzid,1 yzxz,'临时' yzxzsm,kssj,xmmc,ypjl,yznr,zfsj tzsj,zfysgh tzysgh,zfr  tzr,''Creator,a.zh,a.OrganizeId,yzzt,a.yzlx,a.hzxm
                    from [dbo].[zy_lsyz] with(nolock) 
                    where OrganizeId=@orgId and  a.zt=1 and yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')";
            }
            return this.FindList<OrderAuditVO>(sql, new[] { new SqlParameter("@zyh", patList), new SqlParameter("@orgId", orgId) });
        }

        /// <summary>
        /// 提交审核状态
        /// </summary>
        /// <param name="orderList"></param>
        //public void OrderAuditSubmit(OperatorModel user,IList<OrderAuditVO> orderList)
        //{
        //    using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans()) {
        //        if (orderList.Count > 0)
        //        {
        //            foreach (var Item in orderList)
        //            {
        //                if (Item.yzxz == Convert.ToInt32(EnumYzxz.Cq))
        //                {
        //                    var cqyzList = db.FindEntity<InpatientLongTermOrderEntity>(Item.yzid);
        //                    if (cqyzList.tzsj <= DateTime.Now && cqyzList.yzzt == Convert.ToInt32(EnumYzzt.DC))
        //                    {
        //                        cqyzList.yzzt = Convert.ToInt32(EnumYzzt.TZ);
        //                    }
        //                    else if (cqyzList.yzzt == Convert.ToInt32(EnumYzzt.DC) && cqyzList.tzsj > DateTime.Now && cqyzList.zxsj != null)
        //                    {
        //                        cqyzList.yzzt = Convert.ToInt32(EnumYzzt.Zx);
        //                    }
        //                    else
        //                    {
        //                        cqyzList.yzzt = Convert.ToInt32(EnumYzzt.Sh);
        //                    }

        //                    cqyzList.shsj = DateTime.Now;
        //                    cqyzList.shr = user.rygh;
        //                    _InpatientLongTermOrderRepo.SubmitForm(cqyzList, Item.yzid);


        //                }
        //                else if (Item.yzxz == Convert.ToInt32(EnumYzxz.Ls))
        //                {
        //                    var lsyzList = db.FindEntity<InpatientSTATOrderEntity>(Item.yzid);
        //                    if (lsyzList.yzzt == Convert.ToInt32(EnumYzzt.DC))
        //                    {
        //                        lsyzList.yzzt = Convert.ToInt32(EnumYzzt.TZ);
        //                    }
        //                    else
        //                    {
        //                        lsyzList.yzzt = Convert.ToInt32(EnumYzzt.Sh);
        //                    }
        //                    lsyzList.shsj = DateTime.Now;
        //                    lsyzList.shr = user.rygh;
        //                    _InpatientSTATOrderRepo.SubmitForm(lsyzList, Item.yzid);
        //                }
        //            }

        //            db.Commit();
        //        }
        //    }
        //}
        /// <summary>
        /// 审核长期/临时/全部 测试性能不如sql 暂不用
        /// </summary>
        /// <param name="user"></param>
        /// <param name="patList"></param>
        /// <param name="Yzxz"></param>
        public void OrderAuditSubmitbyPat1(OperatorModel user, string patList, int Yzxz)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {

                if (Yzxz == Convert.ToInt32(EnumYzxz.Cq) || Yzxz == 0) ////审核长期
                {
                    var listcq = GetOrderAuditYZListbyYzxz(user.OrganizeId, patList, Convert.ToInt32(EnumYzxz.Cq));
                    if (listcq.Count > 0)
                    {
                        foreach (var Item in listcq)
                        {
                            var cqyzList = db.FindEntity<InpatientLongTermOrderEntity>(Item.yzid);
                            cqyzList.yzzt = Convert.ToInt32(EnumYzzt.Sh);
                            cqyzList.shsj = DateTime.Now;
                            cqyzList.shr = user.rygh;
                            _InpatientLongTermOrderRepo.SubmitForm(cqyzList, Item.yzid);
                        }
                    }
                }

                if (Yzxz == Convert.ToInt32(EnumYzxz.Ls) || Yzxz == 0) //审核临时
                {
                    var listls = GetOrderAuditYZListbyYzxz(user.OrganizeId, patList, Convert.ToInt32(EnumYzxz.Ls));
                    if (listls.Count > 0)
                    {
                        foreach (var Itemls in listls)
                        {
                            var lsyzList = db.FindEntity<InpatientSTATOrderEntity>(Itemls.yzid);
                            lsyzList.yzzt = Convert.ToInt32(EnumYzzt.Sh);
                            lsyzList.shsj = DateTime.Now;
                            lsyzList.shr = user.rygh;
                            _InpatientSTATOrderRepo.SubmitForm(lsyzList, Itemls.yzid);
                        }
                    }
                }

                db.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orderList"></param>
        public string OrderAuditSubmit(OperatorModel user, IList<OrderAuditVO> orderList)
        {
            string listcq = "";
            string listcqzt = "";
            //string listcqyzh = "";
            string listls = "";
            //string listlsyzh = "";
            string jyjcyzhs = "";
            string jyjcIds = "";
            string cqyzIds = "";

            string liscqyfzt = "";
            string lislsyfzt = "";
            //string PSidcq = "";  //排除需做未做皮试、皮试阳性
            //string PSidls = "";
            //string pssql = "";
            //string psretid = "";
            if (orderList.Count > 0)
            {
                listcq = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Cq && p.zh1 == null).Select(p => p.yzid).Distinct().ToArray());
                listcqzt = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Cq && p.zh1 != null).Select(p => p.zh1).Distinct().ToArray());

                listls = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Ls && p.zh1 == null).Select(p => p.yzid).Distinct().ToArray());
                jyjcyzhs = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Ls && p.zh1 != null).Select(p => p.zh1).Distinct().ToArray());
                //用法绑定组套
                liscqyfzt = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Cq && p.yfztbs != null).Select(p => p.yfztbs).Distinct().ToArray());
                lislsyfzt = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Ls && p.yfztbs != null).Select(p => p.yfztbs).Distinct().ToArray());

                //listcqyzh = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Cq && p.yzlxmc == EnumYzlx.Yp.GetDescription()).Select(p => p.yzh).Distinct().ToArray());
                //listlsyzh = string.Join("','", orderList.Where(p => p.yzxz == (int)EnumYzxz.Ls && p.yzlxmc == EnumYzlx.Yp.GetDescription()).Select(p => p.yzh).Distinct().ToArray());
                //foreach (var item in orderList)
                //{
                //    if (item.yzxz == Convert.ToInt32(EnumYzxz.Ls))
                //    {
                //        if (item.yzlxmc == EnumYzlx.jy.GetDescription() || item.yzlxmc == EnumYzlx.jc.GetDescription())
                //        {
                //            jyjcyzhs += item.zh1 + "','";
                //        }
                //        else {
                //            listls += item.yzid + "','";
                //        }
                //    }
                //    else //if (Item.yzxz == Convert.ToInt32(EnumYzxz.Cq))
                //    {
                //        listcq += item.yzid + "','";                        
                //    }
                //}
                if (!string.IsNullOrWhiteSpace(listcqzt) || !string.IsNullOrWhiteSpace(liscqyfzt))
                {
                    string strsql = "";
                    if (!string.IsNullOrWhiteSpace(listcqzt)) {
                        strsql = @"select Id from zy_cqyz with(nolock) where yzh in('" + listcqzt + "') and OrganizeId=@orgId and zt=1 ";
                    }
                    if (!string.IsNullOrWhiteSpace(listcqzt) && !string.IsNullOrWhiteSpace(liscqyfzt))
                        strsql += " union all";
                    if (!string.IsNullOrWhiteSpace(liscqyfzt))
                    {
                        strsql += @" 
                                select Id from zy_cqyz with(nolock) where yfztbs in('" + liscqyfzt + "') and OrganizeId=@orgId and zt=1 ";
                    }
                    var cqyzId = FindList<yzinfoVo>(strsql, new SqlParameter[] {
                        new SqlParameter("orgId",user.OrganizeId),
                        new SqlParameter("jyyz",((int)EnumYzlx.jy).ToString() ),
                        new SqlParameter("jcyz",((int)EnumYzlx.jc).ToString() ),
                    }).Select(p => p.Id).ToArray();
                    if (cqyzId != null && cqyzId.Length > 0)
                    {
                        cqyzIds = string.Join("','", cqyzId);
                    }
                    if (!string.IsNullOrWhiteSpace(cqyzIds))
                    {
                        listcq += "','" + cqyzIds;
                    }
                }
                if (!string.IsNullOrWhiteSpace(jyjcyzhs) || !string.IsNullOrWhiteSpace(lislsyfzt))
                {
                    string strsql = "";
                    if (!string.IsNullOrWhiteSpace(jyjcyzhs))
                    {
                        strsql = @"select Id from zy_lsyz with(nolock) where yzh in('" + jyjcyzhs + "') and OrganizeId=@orgId and zt=1 ";
                    }
                    if (!string.IsNullOrWhiteSpace(jyjcyzhs) && !string.IsNullOrWhiteSpace(lislsyfzt))
                        strsql += " union all";
                    if (!string.IsNullOrWhiteSpace(lislsyfzt))
                    {
                        strsql += @" 
                                select Id from zy_lsyz with(nolock) where yfztbs in('" + lislsyfzt + "') and OrganizeId=@orgId and zt=1";
                    }
                    var jyjc = FindList<yzinfoVo>(strsql, new SqlParameter[] {
                        new SqlParameter("orgId",user.OrganizeId),
                    }).Select(p => p.Id).ToArray();
                    if (jyjc != null && jyjc.Length > 0)
                    {
                        jyjcIds = string.Join("','", jyjc);
                    }
                    if (!string.IsNullOrWhiteSpace(jyjcIds))
                    {
                        listls += "','" + jyjcIds;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(listcq) && string.IsNullOrWhiteSpace(listls)) { return ""; }
            #region 取消皮试排除
            //            if (!string.IsNullOrWhiteSpace(listcq))
            //            {
            //                pssql = @"select Id yzid
            //from zy_cqyz c with(nolock)
            //where c.OrganizeId=@orgId and c.yzzt=0 and c.yzlx=@yzlx and c.zt='1'
            //and exists(select 1 from(
            //	select a.Id,isnull(convert(varchar(50),a.zh),newid()) zh,a.yzh ,a.OrganizeId
            //	from zy_cqyz a with(nolock)
            //	left join xt_gmxx b with(nolock) on a.id=b.yzid and a.OrganizeId=b.OrganizeId and b.zt='1'
            //	where a.OrganizeId=@orgId 
            //	and a.yzh in('" + listcqyzh + @"')
            //	and a.yzzt=0 and a.yzlx=@yzlx
            //	and a.zt='1' and a.ispscs='1' and (b.Result is null or b.Result='1')
            //	) d 
            //where c.OrganizeId=d.OrganizeId and (c.id=d.id or (c.yzh=d.yzh and convert(varchar(50),c.zh)=d.zh))  )  ";
            //                var gmcqlist = FindList<OrderAuditVO>(pssql, new SqlParameter[] {
            //                    new SqlParameter("orgId",user.OrganizeId),
            //                    new SqlParameter("yzlx",(int)EnumYzlx.Yp)
            //                });
            //                if (gmcqlist != null && gmcqlist.Count > 0)
            //                {
            //                    PSidcq = string.Join("','", gmcqlist.Select(p=>p.yzid));
            //                    psretid= string.Join(",", gmcqlist.Select(p => p.yzid));
            //                }
            //            }
            //            if (!string.IsNullOrWhiteSpace(listls))
            //            {
            //                pssql = @"select Id yzid
            //from zy_lsyz c with(nolock)
            //where c.OrganizeId=@orgId and c.yzzt=0 and c.yzlx=@yzlx and c.zt='1'
            //and exists(select 1 from(
            //	select a.Id,isnull(convert(varchar(50),a.zh),newid())zh,a.yzh ,a.OrganizeId
            //	from zy_lsyz a with(nolock)
            //	left join xt_gmxx b with(nolock) on a.id=b.yzid and a.OrganizeId=b.OrganizeId and b.zt='1'
            //	where a.OrganizeId=@orgId 
            //	and a.yzh in('" + listlsyzh + @"')
            //	and a.yzzt=0 and a.yzlx=@yzlx
            //	and a.zt='1' and a.ispscs='1' and (b.Result is null or b.Result='1'
            //	)) d 
            //	where c.OrganizeId=d.OrganizeId and (c.id=d.id or (c.yzh=d.yzh and convert(varchar(50),c.zh)=d.zh))  )";
            //                var gmlslist = FindList<OrderAuditVO>(pssql, new SqlParameter[] {
            //                    new SqlParameter("orgId",user.OrganizeId),
            //                    new SqlParameter("yzlx",(int)EnumYzlx.Yp)
            //                });
            //                if (gmlslist != null && gmlslist.Count > 0)
            //                {
            //                    PSidls = string.Join("','", gmlslist.Select(p => p.yzid));
            //                    psretid = string.Join(",", gmlslist.Select(p => p.yzid));
            //                }
            //            }
            #endregion
            try
            {
                SqlParameter[] para = {
                            new SqlParameter("@shr", user.rygh),
                            new SqlParameter("@listcq", listcq),
                            new SqlParameter("@listls", listls),
                            new SqlParameter("@account", user.UserCode),
                            new SqlParameter("@orgId", user.OrganizeId)
                };

                string sql = "";
                //string sqlwherecq = "";
                //string sqlwherels = "";
                //if (!string.IsNullOrWhiteSpace(PSidcq))
                //{
                //    sqlwherecq += " and id not in('"+PSidcq+"')";
                //}
                //if (!string.IsNullOrWhiteSpace(PSidls))
                //{
                //    sqlwherels += " and id not in('" + PSidls + "')";
                //}
                if (listcq != "")
                {
                    sql += @"
update a
set a.yzzt=(case when yzzt=3 then (
					case when tzsj>getdate() then 
					(
						CASE when zxsj is null then 1 else 2 END
					) 
					ELSE 4 END
				) 
				ELSE 1 END
			)
	,a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
from  zy_cqyz  a  
where a.zt=1 and a.yzzt in(0,3) 
AND exists(select 1 
            from zy_cqyz b with(nolock)
            where id in('" + listcq + @"')
            and a.zh=b.zh and a.zyh=b.zyh and a.OrganizeId=b.OrganizeId) 
                
update a
set a.yzzt=(case when yzzt=3 then 
				(
					case when tzsj>getdate() then 
					(
						CASE when zxsj is null then 1 else 2 END
					) 
					ELSE 4 END
				) 
				ELSE 1 END
			)
	,a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
from zy_cqyz  a  
where a.zt=1 and a.yzzt in(0,3) 
AND Id in('" + listcq + @"') 
and id<>''                ";
                }
                if (listls != "")
                {
                    sql += @"
update a
set a.yzzt=(case when yzzt=3 then 4 else 1 end),
a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
from  zy_lsyz  a 
where a.zt=1 and a.yzzt in(0,3) and 
exists( select 1
from zy_lsyz b with(nolock)
where id in('" + listls + @"') and id<>'' 
and a.zh=b.zh and a.zyh=b.zyh and a.organizeid=b.organizeid)

update a
set a.yzzt=(case when yzzt=3 then 4 else 1 end),
a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
from zy_lsyz a
where a.zt=1 and a.yzzt in(0,3) and id<>''  and 
Id in('" + listls + @"')  ;";
                }

                ExecuteSqlCommand(sql, para);

                sql = "exec usp_zy_OrderAudit_hljbCheck @orgId=@orgId,@lsList=@lsList,@cqList=@cqList,@zyhList=@zyhList ;";
                ExecuteSqlCommand(sql, new SqlParameter[] {
                    new SqlParameter("orgId",user.OrganizeId),
                    new SqlParameter("lsList",listls),
                    new SqlParameter("cqList",listcq),
                    new SqlParameter("zyhList",""),
                    new SqlParameter("user", user.rygh),
                });


                return "";

            }
            catch (Exception ex)
            {
                throw new FailedException("审核提交失败，" + ex.InnerException);
            }

        }

        #region 审核长期和全部

        public string OrderAuditSubmitbyPat(OperatorModel user, string patList, int Yzxz, IList<OrderAuditVO> orderList, string IsRehabAuthtoNurse = null, bool Iskf = false, string zxks = null)
        {

            string strkfwhere = "";
            string sql = "";
            string pssql = "";
            //string PSidcq = "";
            string PSidls = "";
            string psretid = "";
            if (IsRehabAuthtoNurse == "0" && Iskf)
            {
                strkfwhere = " and a.yzlx=" + (int)EnumYzlx.rehab + " and a.zxksdm='" + zxks + "'";
            }
            else if (IsRehabAuthtoNurse == "0" && !Iskf)
            {
                strkfwhere = " and a.yzlx<>" + (int)EnumYzlx.rehab;
            }
            //文字医嘱默认正常待审审核之后为执行状态
            try
            {
                sql = "";
                var para = new[] {
                            new SqlParameter("@shr", user.rygh),
                            new SqlParameter("@zyh", patList),
                            new SqlParameter("@account", user.UserCode),
                            new SqlParameter("@OrganizeId", user.OrganizeId)
                };
                if (Yzxz == Convert.ToInt32(EnumYzxz.Cq) || Yzxz == 0) //审核长期
                {
                    //皮试
                    //                    pssql = @"select Id yzid
                    //from zy_cqyz c with(nolock)
                    //where c.OrganizeId=@orgId and c.yzzt=0 and c.yzlx=@yzlx and c.zt='1'
                    //and exists(select 1 from(
                    //	select a.Id,isnull(convert(varchar(50),a.zh),newid()) zh,a.yzh ,a.OrganizeId
                    //	from zy_cqyz a with(nolock)
                    //	left join xt_gmxx b with(nolock) on a.id=b.yzid and a.OrganizeId=b.OrganizeId and b.zt='1'
                    //	where a.OrganizeId=@orgId 
                    //	and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
                    //	and a.yzzt=0 and a.yzlx=@yzlx
                    //	and a.zt='1' and a.ispscs='1' and (b.Result is null or b.Result='1')
                    //	) d 
                    //where c.OrganizeId=d.OrganizeId and (c.id=d.id or (c.yzh=d.yzh and convert(varchar(50),c.zh)=d.zh))  )  ";
                    //                    var gmcqlist = FindList<OrderAuditVO>(pssql, new SqlParameter[] {
                    //                        new SqlParameter("orgId",user.OrganizeId),
                    //                        new SqlParameter("yzlx",(int)EnumYzlx.Yp),
                    //                        new SqlParameter("zyh",patList)
                    //                    });
                    //if (gmcqlist != null && gmcqlist.Count > 0)
                    //{
                    //    PSidcq = string.Join("','", gmcqlist.Select(p => p.yzid));
                    //    psretid += string.Join(",", gmcqlist.Select(p => p.yzid));
                    //    if (!string.IsNullOrWhiteSpace(PSidcq))
                    //    {
                    //        strkfwhere += " and a.id not in('" + PSidcq + "')";
                    //    }
                    //}


                    sql += @"
update a
set a.yzzt=(case when yzzt=3 then (
				case when tzsj>getdate() then (
					CASE when zxsj is null then 1 else 2 END
				) 
				ELSE 4 end) else 1 END
			)
	,a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
from zy_cqyz a
where a.OrganizeId=@OrganizeId and a.zt=1 and yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')  " + strkfwhere;
                }

                if (Yzxz == Convert.ToInt32(EnumYzxz.Ls) || Yzxz == 0) //审核临时
                {
                    //                    pssql = @"select Id yzid
                    //from zy_lsyz c with(nolock)
                    //where c.OrganizeId=@orgId and c.yzzt=0 and c.yzlx=@yzlx and c.zt='1'
                    //and exists(select 1 from(
                    //	select a.Id,isnull(convert(varchar(50),a.zh),newid())zh,a.yzh ,a.OrganizeId
                    //	from zy_lsyz a with(nolock)
                    //	left join xt_gmxx b with(nolock) on a.id=b.yzid and a.OrganizeId=b.OrganizeId and b.zt='1'
                    //	where a.OrganizeId=@orgId 
                    //	and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
                    //	and a.yzzt=0 and a.yzlx=@yzlx
                    //	and a.zt='1' and a.ispscs='1' and (b.Result is null or b.Result='1'
                    //	)) d 
                    //	where c.OrganizeId=d.OrganizeId and (c.id=d.id or (c.yzh=d.yzh and convert(varchar(50),c.zh)=d.zh))  )";
                    //                    var gmlslist = FindList<OrderAuditVO>(pssql, new SqlParameter[] {
                    //                    new SqlParameter("orgId",user.OrganizeId),
                    //                    new SqlParameter("yzlx",(int)EnumYzlx.Yp),
                    //                    new SqlParameter("zyh",patList)
                    //                });
                    //                    if (gmlslist != null && gmlslist.Count > 0)
                    //                    {
                    //                        PSidls = string.Join("','", gmlslist.Select(p => p.yzid));
                    //                        psretid +=string.IsNullOrWhiteSpace(psretid)==true?"":","+ string.Join(",", gmlslist.Select(p => p.yzid));
                    //                        if (!string.IsNullOrWhiteSpace(PSidls))
                    //                        {
                    //                            strkfwhere += " and a.id not in('" + PSidls + "')";
                    //                        }
                    //                    }

                    sql += @"
update a
set a.yzzt=(
		CASE when yzzt=3 then 4 else 1 END
	)
	,a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
from zy_lsyz a
where a.OrganizeId=@OrganizeId and a.zt=1 and yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
" + strkfwhere;
                }
                var count = ExecuteSqlCommand(sql, para);

                sql = "exec usp_zy_OrderAudit_hljbCheck @orgId=@orgId,@lsList=@lsList,@cqList=@cqList,@zyhList=@zyhList ;";
                var hljbList = this.FindList<PatientzbqResponseDto>(sql, new SqlParameter[] {
                    new SqlParameter("orgId",user.OrganizeId),
                    new SqlParameter("lsList",""),
                    new SqlParameter("cqList",""),
                    new SqlParameter("zyhList",patList),
                    new SqlParameter("user",user.rygh),
                });

            }
            catch (Exception ex)
            {
                throw new FailedException("审核提交失败，" + ex.InnerException);
            }

            return psretid;
        }


        #endregion


        /// <summary>
        /// 审核长期/临时/全部 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="patList"></param>
        /// <param name="Yzxz"></param>
        //public void OrderAuditSubmitbyPat(OperatorModel user, string patList, int Yzxz,string IsRehabAuthtoNurse = null, bool Iskf = false, string zxks = null)
        //{
        //    string strkfwhere = "";
        //    string sql = "";
        //    if (IsRehabAuthtoNurse == "0" && Iskf)
        //    {
        //        strkfwhere = " and a.yzlx=" + (int)EnumYzlx.rehab + " and a.zxksdm='" + zxks + "'";
        //    }
        //    else if (IsRehabAuthtoNurse == "0" && !Iskf)
        //    {
        //        strkfwhere = " and a.yzlx<>" + (int)EnumYzlx.rehab;
        //    }
        //    //文字医嘱默认正常待审审核之后为执行状态
        //    try
        //    {
        //        sql = "";
        //        var para = new[] {
        //                            new SqlParameter("@shr", user.rygh),
        //                            new SqlParameter("@zyh", patList),
        //                            new SqlParameter("@account", user.UserCode),
        //                            new SqlParameter("@OrganizeId", user.OrganizeId)
        //                };
        //        if (Yzxz == Convert.ToInt32(EnumYzxz.Cq) || Yzxz == 0) //审核长期
        //        {

        //            sql += @"
        //update a
        //set a.yzzt=(case when yzzt=3 then (
        //				case when tzsj>getdate() then (
        //					CASE when zxsj is null then 1 else 2 END
        //				) 
        //				ELSE 4 end) else 1 END
        //			)
        //	,a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
        //from zy_cqyz a
        //where a.OrganizeId=@OrganizeId and a.zt=1 and yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')  " + strkfwhere;
        //        }

        //        if (Yzxz == Convert.ToInt32(EnumYzxz.Ls) || Yzxz == 0) //审核临时
        //        {
        //            sql += @"
        //update a
        //set a.yzzt=(
        //		CASE when yzzt=3 then 4 else 1 END
        //	)
        //	,a.shr=@shr,a.shsj=getdate(),a.LastModifyTime=getdate(),LastModifierCode=@account
        //from zy_lsyz a
        //where a.OrganizeId=@OrganizeId and a.zt=1 and yzzt in(0,3) and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
        //" + strkfwhere;
        //        }
        //        var count = ExecuteSqlCommand(sql, para);

        //        sql = "exec usp_zy_OrderAudit_hljbCheck @orgId=@orgId,@lsList=@lsList,@cqList=@cqList,@zyhList=@zyhList ;";
        //        var hljbList = this.FindList<PatientzbqResponseDto>(sql, new SqlParameter[] {
        //                    new SqlParameter("orgId",user.OrganizeId),
        //                    new SqlParameter("lsList",""),
        //                    new SqlParameter("cqList",""),
        //                    new SqlParameter("zyhList",patList),
        //                    new SqlParameter("user",user.rygh),
        //                });

        //        //var hljbdt = SqlQueryForDataTatable(sql, new Dictionary<string, string> { { "@orgId", user.OrganizeId }, { "@lsList", "" }, { "@cqList", "" }, { "@zyhList", patList } });

        //        //                if (hljbList != null && hljbList.Count <= 0) return;
        //        //                sql = "";
        //        //                //更新住院病人信息护理级别
        //        //                foreach (var pathljb in hljbList)
        //        //                {
        //        //                    sql += @"update a
        //        //set a.hljb='" + pathljb.hljb + @"',a.lastmodifytime=getdate(),lastmodifiercode='" + user.UserCode + @"'
        //        //from zy_brxxk a 
        //        //where a.zyh='" + pathljb.zyh + "' and a.organizeid='" + user.OrganizeId + "' and a.zt=1 ; ";
        //        //                }

        //        //                ExecuteSqlCommand(sql);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new FailedException("审核提交失败，" + ex.InnerException);
        //    }

        //}







        //皮试录入树
        public IList<SkintestVO> SkintestVO(string orgId, string keyword, string selectkey)
        {
            string sqlwhere = "";
            string sql = "";

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sqlwhere += " and (a.zyh like @keyword or a.hzxm like @keyword) ";
            }

            if (string.IsNullOrWhiteSpace(selectkey))
            {
                throw new FailedException("请选择查询类别");
            }
            else if (selectkey == "0")  //未录入
            {
                sqlwhere += " and not exists(select 1 from xt_gmxx x with(nolock) where a.id=x.yzid and x.zt='1')";

                sql = @"select * from (select a.WardCode,b.bqmc,a.zyh,a.hzxm 
from [Newtouch_CIS].[dbo].[zy_cqyz] a with(nolock)
left join [NewtouchHIS_Base].dbo.xt_bq b with(nolock) on a.WardCode=b.bqCode and b.zt='1' and a.OrganizeId=b.OrganizeId
where a.OrganizeId=@orgId and a.zt='1' and  a.ispscs='1'" + sqlwhere;
                sql += @"
union all
select a.WardCode,b.bqmc,a.zyh,a.hzxm 
from [Newtouch_CIS].[dbo].[zy_lsyz] a with(nolock)
left join [NewtouchHIS_Base].dbo.xt_bq b with(nolock) on a.WardCode=b.bqCode and b.zt='1' and a.OrganizeId=b.OrganizeId
where a.OrganizeId=@orgId and a.zt='1' and  a.ispscs='1'" + sqlwhere;
                sql += ") as tb group by zyh,WardCode,bqmc,hzxm";
            }
            else if (selectkey == "1")
            {
                sql = @"select *
from (select  a.WardCode,c.bqmc,a.zyh,a.hzxm
from xt_gmxx b with(nolock),zy_cqyz a with(nolock)
left join NewtouchHIS_Base.dbo.xt_bq c with(nolock) on a.WardCode=c.bqCode and a.OrganizeId=c.OrganizeId 
where b.yzid=a.id and b.zt='1' and a.zt='1'
union 
select  a.WardCode,c.bqmc,a.zyh,a.hzxm
from xt_gmxx b with(nolock),zy_lsyz a with(nolock)
left join NewtouchHIS_Base.dbo.xt_bq c with(nolock) on a.WardCode=c.bqCode and a.OrganizeId=c.OrganizeId 
where b.yzid=a.id and b.zt='1' and a.zt='1') as tb
group by WardCode,bqmc,zyh,hzxm";
            }
            return this.FindList<SkintestVO>(sql, new SqlParameter[] {
                new SqlParameter("orgId",orgId),
                new SqlParameter("keyword","%"+keyword+"%")
            });

        }

        public IList<SkintestqueryVO> Skintestquery(Pagination pagination, string patList, string orgId, string selectkey)
        {
            string sqlwhere = "";
            string sql = "";
            if (string.IsNullOrWhiteSpace(patList))
            {
                return null;
            }
            if (selectkey == "1")
            {
                sql = @"select a.zyh,a.id yzid,2 yzxz,'长期' yzxzsm,a.kssj,a.xmmc,a.xmdm,a.ypjl,
(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end)yznr,a.tzsj,a.tzysgh,a.tzr ,d.CreatorName Creator,a.zh,'' zh1,a.OrganizeId,yzzt,
--a.yzlx,
(case when d.Remark>'' then d.Remark else ''  end) lrjg,
a.hzxm,
a.yztag,case a.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else a.yztag end yztagName,
isnull(a.isjf,1) isjf,ispscs,d.[CreateTime],d.[CreatorCode],d.[CreatorName],d.[LastModifyTime],d.[LastModifierCode],d.[LastModifierName]
from xt_gmxx d with(nolock),zy_cqyz a with(nolock)
left join NewtouchHIS_Base.dbo.xt_bq c with(nolock) on a.WardCode=c.bqCode and a.OrganizeId=c.OrganizeId 
where  d.OrganizeId=@orgId and d.yzid=a.id and d.zt='1' and a.zt='1' 
and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
union all
select a.zyh,a.id yzid,1 yzxz,'临时' yzxzsm,a.kssj,a.xmmc,a.xmdm,a.ypjl,
(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end)yznr,a.zfsj,a.zfysgh,a.zfr ,d.CreatorName Creator,a.zh,'' zh1,a.OrganizeId,yzzt,
--a.yzlx,
(case when d.Remark>'' then d.Remark else ''  end) lrjg,
a.hzxm,
a.yztag,case a.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else a.yztag end yztagName,
isnull(a.isjf,1) isjf,ispscs,d.[CreateTime],d.[CreatorCode],d.[CreatorName],d.[LastModifyTime],d.[LastModifierCode],d.[LastModifierName]
from xt_gmxx d with(nolock),zy_lsyz a with(nolock)
left join NewtouchHIS_Base.dbo.xt_bq c with(nolock) on a.WardCode=c.bqCode and a.OrganizeId=c.OrganizeId 
where  d.OrganizeId=@orgId and d.yzid=a.id and d.zt='1' and a.zt='1'
and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
";
            }
            else if (selectkey == "0")
            {
                sqlwhere += " and not exists(select 1 from xt_gmxx x with(nolock) where a.id=x.yzid and x.zt='1')";
                sql = @"select a.zyh,a.id yzid,2 yzxz,'长期' yzxzsm,a.kssj,a.xmmc,a.xmdm,a.ypjl,
(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end)yznr,a.tzsj,a.tzysgh,a.tzr ,b.Name Creator,a.zh,'' zh1,a.OrganizeId,yzzt,
--a.yzlx,
(case when d.Remark>'' then d.Remark  else  '' end) lrjg,
a.hzxm,
a.yztag,case a.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else a.yztag end yztagName,
isnull(a.isjf,1) isjf,ispscs,'长' yzlb
from [dbo].[zy_cqyz] a with(nolock) 
left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId
left join xt_gmxx d with(nolock) on a.Id=d.yzid and a.OrganizeId=d.OrganizeId
where a.OrganizeId=@orgId  and  a.zt=1 and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
and a.ispscs='1' " + sqlwhere;
                sql += @"
union all
select a.zyh, a.id yzid,1 yzxz,'临时' yzxzsm,a.kssj,a.xmmc,a.xmdm,a.ypjl,
(case when a.yzzt=3 then '[停]'+a.yznr else a.yznr end)yznr,a.zfsj,a.zfysgh,a.zfr ,b.Name Creator,a.zh,'' zh1,a.OrganizeId,yzzt,
--a.yzlx,
(case when d.Remark>'' then d.Remark  else  '' end) lrjg,
a.hzxm,
a.yztag,case a.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else a.yztag end yztagName,
isnull(a.isjf,1) isjf,ispscs,'长' yzlb
from [dbo].[zy_lsyz] a with(nolock) 
left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId
left join xt_gmxx d with(nolock) on a.Id=d.yzid and a.OrganizeId=d.OrganizeId
where a.OrganizeId=@orgId  and a.zt=1 and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
and  a.ispscs='1' " + sqlwhere;
            }

            return this.QueryWithPage<SkintestqueryVO>(sql, pagination, new[] {
                new SqlParameter("@zyh", patList),
                new SqlParameter("@orgId", orgId)
            }, false);
        }

        public void Inputskintestresults(OperatorModel user, IList<SkintestqueryVO> orderList)
        {
            try
            {

                if (orderList.Count > 0)
                {
                    foreach (var item in orderList)
                    {
                        string Remark = item.psbz == "0" ? "阴性" : "阳性";
                        var ety = _allergyRepo.FindEntity(p => p.yzid == item.yzid && p.zt == "1");
                        if (ety != null && !string.IsNullOrWhiteSpace(ety.yzid))
                        {
                            ety.Remark = Remark;
                            ety.LastModifierName = user.UserName;
                            ety.Modify();
                            _allergyRepo.Update(ety);
                        }
                        else
                        {
                            string sql = string.Format(@"insert into [Newtouch_CIS].dbo.xt_gmxx 
    select
    newid(), blh, '" + item.hzxm + "', sex, '" + item.xmdm + "', '" + item.xmmc + "', '" + item.psbz + "', '" + Remark + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "','" + user.UserCode + "', " +
            "'" + user.UserName + "',NULL,null,null ,2 ,'" + item.yzid.Trim() + "','', '" + user.OrganizeId.Trim() + "', '1','' , '" + item.xmdm + "'" +
    @" from zy_brxxk with(nolock)
    where zyh = '" + item.zyh.Trim() + "' and OrganizeId ='" + user.OrganizeId.Trim() + "' and zt = '1'");

                            ExecuteSqlCommand(sql);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new FailedException("审核提交失败，" + ex.InnerException);
            }
        }

        //审批提示是否做皮试
        public IList<DisplayinformationVO> Displayinformation(string patList, string orgId)
        {
            string sql = @"select a.Id yzid,a.zyh,a.hzxm xm,a.xmmc,(case when a.ispscs='1'  and b.Result is null then '未录入' when b.Result<>'' then b.Remark  end) ispscs,b.Result,
b.Remark ,'2' yzxz,a.zh
from [dbo].[zy_cqyz] a with(nolock)
left join [Newtouch_CIS].dbo.xt_gmxx b with(nolock) on a.Id=b.yzid and a.OrganizeId=b.OrganizeId 
where a.OrganizeId=@orgId and a.Id in(select col from dbo.f_split(@patList,',') where col>'' )  and a.ispscs='1' 
union all
select a.Id yzid,a.zyh,a.hzxm xm,a.xmmc,(case when a.ispscs='1'  and b.Result is null then '未录入' when b.Result<>'' then b.Remark  end) ispscs,b.Result,
b.Remark  ,'1' yzxz,a.zh
from [dbo].[zy_lsyz] a with(nolock)
left join [Newtouch_CIS].dbo.xt_gmxx b with(nolock) on a.Id=b.yzid and a.OrganizeId=b.OrganizeId 
where a.OrganizeId=@orgId and a.Id in(select col from dbo.f_split(@patList,',') where col>'' )  and a.ispscs='1' 
";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@patList", patList));

            return this.FindList<DisplayinformationVO>(sql, par.ToArray());



        }
        /// <summary>
        /// 皮试录入
        /// </summary>
        /// <param name="user"></param>
        /// <param name="yzids"></param>
        public string Enteragain(OperatorModel user, string yzids, string lrjg)
        {
            if (string.IsNullOrWhiteSpace(yzids))
            {
                throw new FailedException("请选择皮试项目");
            }
            string sql = "select * from xt_gmxx with(nolock) where OrganizeId=@orgId and yzid in(select col from dbo.f_split( @yzid,',')) and zt='1'";
            var exist = FindList<AllergyEntity>(sql, new SqlParameter[] {
                new SqlParameter("orgId",user.OrganizeId),
                new SqlParameter("yzid",yzids)
            });
            if (exist != null && exist.Count > 0)
            {
                throw new FailedException("部分项目已录入，请刷新页面重试");
            }
            try
            {
                sql = @"insert into [Newtouch_CIS].dbo.xt_gmxx ([Id],[blh],[xm],[sex],[xmCode],[xmmc],[Result],[Remark],
[CreateTime],[CreatorCode],[CreatorName],[LastModifyTime],[LastModifierCode],[LastModifierName],
[mzzybz],[yzid],[cfmxid],[OrganizeId],[zt],[gmlx],[ypCode])
select newid(),b.blh,b.xm,b.sex,a.xmdm,a.xmmc,
@lrjg,(case when @lrjg='1' then '阳性' else '阴性' end  ),
getdate(),@user,@username,null,null,null,
'2',a.Id,null,a.OrganizeId,'1',null,a.xmdm 
from [Newtouch_CIS].[dbo].[zy_lsyz] a
left join[Newtouch_CIS].[dbo].[zy_brxxk] b on a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1'
where a.Id in (select col from dbo.f_split( @yzid,',')) and a.OrganizeId=@orgId and a.zt='1'
and not exists(select 1 from xt_gmxx c with(nolock) where a.OrganizeId=c.OrganizeId and a.Id=c.Id and c.zt='1')";
                ExecuteSqlCommand(sql, new SqlParameter[] {
                    new SqlParameter("orgId",user.OrganizeId),
                    //new SqlParameter("yzzt",Convert.ToInt32((int)EnumYzzt.Ds)),
                    new SqlParameter("yzid",yzids),
                    new SqlParameter("lrjg",lrjg),
                    new SqlParameter("user",user.rygh),
                    new SqlParameter("username",user.UserName),
                });
                sql = @"insert into [Newtouch_CIS].dbo.xt_gmxx ([Id],[blh],[xm],[sex],[xmCode],[xmmc],[Result],[Remark],
[CreateTime],[CreatorCode],[CreatorName],[LastModifyTime],[LastModifierCode],[LastModifierName],
[mzzybz],[yzid],[cfmxid],[OrganizeId],[zt],[gmlx],[ypCode])
select newid(),b.blh,b.xm,b.sex,a.xmdm,a.xmmc,
@lrjg,(case when @lrjg='1' then '阳性' else '阴性' end  ),
getdate(),@user,@username,null,null,null,
'2',a.Id,null,a.OrganizeId,'1',null,a.xmdm 
from [Newtouch_CIS].[dbo].[zy_cqyz] a
left join[Newtouch_CIS].[dbo].[zy_brxxk] b on a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1'
where a.Id in (select col from dbo.f_split( @yzid,',')) and a.OrganizeId=@orgId and a.zt='1'
--and a.yzzt=@yzzt
and not exists(select 1 from xt_gmxx c with(nolock) where a.OrganizeId=c.OrganizeId and a.Id=c.Id and c.zt='1')";
                ExecuteSqlCommand(sql, new SqlParameter[] {
                new SqlParameter("orgId",user.OrganizeId),
                //new SqlParameter("yzzt",Convert.ToInt32((int)EnumYzzt.Ds)),
                new SqlParameter("yzid",yzids),
                new SqlParameter("lrjg",lrjg),
                new SqlParameter("user",user.rygh),
                new SqlParameter("username",user.UserName),
            });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;

        }
        public string Enteragain(OperatorModel user, string zyh, string yzid, string lrjg, string yzlb)
        {
            string sql = "";
            var cq = _InpatientLongTermOrderRepo.FindEntity(p => p.Id == yzid);
            if (cq != null)
            {
                if (cq.yzzt != (int)EnumYzzt.Ds)
                {
                    throw new FailedException("医嘱已审核通过，无法修改结果");
                }
            }
            else
            {
                var ls = _InpatientSTATOrderRepo.FindEntity(p => p.Id == yzid);
                if (ls != null)
                {
                    if (ls.yzzt != (int)EnumYzzt.Ds)
                    {
                        return "医嘱已审核通过，无法修改结果";
                    }
                }
            }
            var ety = _allergyRepo.FindEntity(p => p.yzid == yzid && p.OrganizeId == user.OrganizeId && p.zt == "1");
            if (ety != null && !string.IsNullOrWhiteSpace(ety.yzid))
            {
                ety.Result = lrjg;
                ety.Remark = lrjg == "0" ? "阴性" : "阳性";
                ety.LastModifierName = user.UserName;
                ety.Modify();
                _allergyRepo.Update(ety);
                return "1";
            }
            if (yzlb == "1")
            {
                sql = @"   select b.blh,b.xm hzxm,b.sex,a.xmdm,a.xmmc,a.Id yzid,a.OrganizeId 
    from [Newtouch_CIS].[dbo].[zy_lsyz] a
    left join[Newtouch_CIS].[dbo].[zy_brxxk] b on a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1'
    where a.Id= @yzid and a.OrganizeId=@orgId and a.zt='1'";
            }
            else
            {
                sql = @"   select b.blh,b.xm hzxm,b.sex,a.xmdm,a.xmmc,a.Id yzid,a.OrganizeId 
    from [Newtouch_CIS].[dbo].[zy_cqyz] a
    left join[Newtouch_CIS].[dbo].[zy_brxxk] b on a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1'
    where a.Id= @yzid and a.OrganizeId=@orgId and a.zt='1'";
            }

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", user.OrganizeId));
            par.Add(new SqlParameter("@zyh", zyh));
            par.Add(new SqlParameter("@yzid", yzid));

            var data = FindList<SkintestqueryVO>(sql, par.ToArray());
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    string Remark = lrjg == "0" ? "阴性" : "阳性";
                    string sql2 = string.Format(@"insert into [Newtouch_CIS].dbo.xt_gmxx 
values(
newid(), '" + item.blh + "', '" + item.hzxm + "', '" + item.sex + "', '" + item.xmdm + "', '" + item.xmmc + "', '" + lrjg + "', '" + Remark + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "','" + user.UserCode + "', " +
    "'" + user.UserName + "',NULL,NULL,NULL ,2 ,'" + item.yzid.Trim() + "','', '" + user.OrganizeId.Trim() + "', '1','' , '" + item.xmdm + "','')");

                    ExecuteSqlCommand(sql2);
                }
                return "1";
            }
            else
            {
                return "0";
            }



        }
        /// <summary>
        /// 住院医嘱发药查询
        /// </summary>
        /// <param name="xm"></param>
        /// <param name="bqCode"></param>
        /// <param name="ypmc"></param>
        /// <param name="cw"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        public IList<OrdersDrugsVO> GetOrdersDrugsGridJson(Pagination pagination, string xm, string bqCode, string ypmc, string cw, string zyh, DateTime kssj, DateTime jssj, string OrganizeId)
        {
            var sql = @"select zh,zyh,patientName,ypmc,id,cw,slStr,jxmc,ypgg,pcmc,ylStr,yznr,yzxzmc,zlff,ycmc,CreatorCode,CreateTime
from  ( select
b.zyh,b.patientName,b.cw,b.ypCode,c.ypmc,b.pcmc,
ISNULL(CONVERT(VARCHAR(50), b.zh), '') zh,
b.yl, b.yldw, CONCAT(b.yl, b.yldw) ylStr
,(case when b.yzxz = '1' then '临时' when b.yzxz = '2' then '长期' else '/' END) yzxzmc,
b.zlff,c.ycmc,CONCAT(b.dj, '元/', c.djdw) djStr,
(case when a.operateType = '1' then NewtouchHIS_PDS.dbo.f_getYfbmYpComplexYpSlandDw(SUM(d.sl), d.fyyf, b.ypCode, @OrganizeId) when a.operateType = '2' then NewtouchHIS_PDS.dbo.f_getYfbmYpComplexYpSlandDw(SUM(a.sl), d.fyyf, b.ypCode, @OrganizeId) end) slStr,
(case when a.operateType = '1' then CONVERT(NUMERIC(11,2),ISNULL(SUM(b.dj / b.zhyz * d.sl), 0)) when a.operateType = '2' then CONVERT(NUMERIC(11,2),ISNULL(SUM(b.dj / b.zhyz * a.sl), 0)) end) je,
a.Id,a.yzId,a.zxId,a.CreateTime,a.CreatorCode,d.pc,e.jxmc,ypsx.ypgg,
(case when cqyz.yznr is not null then cqyz.yznr when cqyz.yznr is null then lsyz.yznr end )yznr
from NewtouchHIS_PDS.dbo.zy_ypyzczjl a with(nolock)
,NewtouchHIS_PDS.dbo.zy_ypyzxx b with(nolock)
left join dbo.zy_cqyz cqyz on b.yzId = cqyz.Id and b.OrganizeId = cqyz.OrganizeId
left join dbo.zy_lsyz lsyz on b.yzId = lsyz.Id and b.OrganizeId = lsyz.OrganizeId
left join NewtouchHIS_Base.dbo.V_S_xt_yp c with(nolock) on b.OrganizeId = c.OrganizeId and b.ypCode = c.ypCode
left join NewtouchHIS_PDS.dbo.zy_ypyzzxph d with(nolock) on b.OrganizeId = d.OrganizeId and b.id = d.zyypxxId and d.zt = '1'
left JOIN NewtouchHIS_Base.dbo.xt_ypjx(NOLOCK) e on e.jxCode = c.jx AND e.zt = '1'
left JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId = c.ypId AND ypsx.OrganizeId = c.OrganizeId
where a.ypyzxxId = b.Id and a.OrganizeId = b.OrganizeId and a.OrganizeId = @OrganizeId
and a.CreateTime BETWEEN '" + kssj + "' AND '" + jssj + "'";

            if (!string.IsNullOrWhiteSpace(xm))
            {
                sql += (" AND b.patientName LIKE '%'+@xm+'%' ");
            }
            if (!string.IsNullOrEmpty(bqCode))
            {
                sql += (" AND b.bqCode=@bqCode ");
            }

            if (!string.IsNullOrEmpty(ypmc))
            {
                sql += (" AND c.ypmc LIKE '%'+@ypmc+'%' ");
            }
            if (!string.IsNullOrEmpty(cw))
            {
                sql += (" AND b.cw LIKE '%'+@cw+'%' ");
            }
            if (!string.IsNullOrEmpty(zyh))
            {
                sql += (" AND b.zyh = @zyh");
            }
            sql += (@" group by a.operateType,
b.zyh, b.patientName, b.cw, b.ypCode, c.ypmc, b.pcmc, b.yl, b.yldw, b.zh,
a.Id, a.yzId, a.zxId, a.ypCode, b.yzxz, b.zlff, c.ycmc, b.dj, c.djdw, a.CreateTime, a.CreatorCode,d.pc,e.jxmc,ypsx.ypgg,d.fyyf,cqyz.yznr,lsyz.yznr )dd
group by zh,zyh,patientName,ypmc,id,cw,slStr,jxmc,ypgg,pcmc,ylStr,yznr,yzxzmc,zlff,ycmc,CreatorCode,CreateTime ");
            var parms = new List<SqlParameter>
            {
                new SqlParameter("@OrganizeId", OrganizeId),
                new SqlParameter("@xm", xm??""),
                new SqlParameter("@bqCode", bqCode??""),
                new SqlParameter("@ypmc", ypmc??""),
                new SqlParameter("@cw", cw??""),
                new SqlParameter("@zyh", zyh??""),
        };

            return QueryWithPage<OrdersDrugsVO>(sql, pagination, parms.ToArray(), false);
        }


        /// <summary>
        /// 通过药房代码获取科室
        /// </summary>
        /// <param name="ykbmCode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<SysDeptWardRelVO> GetTheLowerKsCodeList(string orgId, string keyword = "")
        {
            var strSql = new StringBuilder(@"
select a.DepartmentCode,b.[Name] DepartmentName,a.bqCode,c.bqmc,c.py bqpy
from [NewtouchHIS_Base].[dbo].[Sys_DepartmentWardRelation] a with(nolock)
left join NewtouchHIS_Base.dbo.sys_department b with(nolock) on a.DepartmentId=b.id and a.OrganizeId=b.OrganizeId and b.zt='1'
left join NewtouchHIS_Base.dbo.xt_bq c with(nolock) on a.bqCode=c.bqCode and a.OrganizeId=c.OrganizeId and c.zt='1'
where a.OrganizeId=@OrganizeId and a.zt='1'
AND (c.bqmc LIKE '%'+@keyword+'%' OR c.py LIKE '%'+@keyword+'%' OR a.bqCode LIKE '%'+@keyword+'%')
");
            var param = new List<SqlParameter>
            {
                new SqlParameter("@OrganizeId", orgId),
                new SqlParameter("@keyword", (keyword??"").Trim())
            };
            return FindList<SysDeptWardRelVO>(strSql.ToString(), param.ToArray());
        }
        /// <summary>
        /// 保存备药申请单
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orgId"></param>
        /// <param name="Djnr"></param>
        /// <returns></returns>
        public string PrepareMedicine(OperatorModel user, string orgId, BYDjInfoDTO Djnr)
        {
            try
            {
                if (Djnr.isdeldj != null && Djnr.isdeldj != "")
                {
                    string sql = "delete from zy_bqksby where Id=@Id and OrganizeId=@orgId and zt='1' ";
                    SqlParameter[] para = {
                            new SqlParameter("@Id", Djnr.isdeldj),
                            new SqlParameter("@orgId", orgId) };
                    ExecuteSqlCommand(sql, para);
                    string sql2 = "delete from zy_bqksbymx where ById=@Id and OrganizeId=@orgId and zt='1' ";
                    SqlParameter[] para2 = {
                            new SqlParameter("@Id", Djnr.isdeldj),
                            new SqlParameter("@orgId", orgId) };
                    ExecuteSqlCommand(sql2, para2);
                }
                var shzt = "0";
                if (Djnr.issavesubmit == "1")
                {
                    shzt = "1";
                }
                var request = new
                {
                    OrganizeId = orgId,
                    yhgh = user.rygh,
                    yplist = Djnr
                };
                using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    var ksbyid = Guid.NewGuid().ToString();
                    var byEntity = new PreparationdrugsEntity
                    {
                        Id = ksbyid,
                        OrganizeId = orgId,
                        yfbm = Djnr.yfbm,
                        bqbm = Djnr.rkbm,
                        ksbm = Djnr.ksbm,
                        djh = Djnr.djh,
                        shzt = shzt,
                        zt = "1",
                        CreatorCode = user.rygh,
                        CreateTime = DateTime.Now,
                        LastModifierCode = "",
                        LastModifyTime = null,
                    };
                    
                    db.Insert(byEntity);
                    foreach (var byxx in Djnr.mx)
                    {
                        var bymxEntity = new PreparationdrugsMXEntity
                        {
                            Id = Guid.NewGuid().ToString(),
                            byId = ksbyid,
                            OrganizeId = orgId,
                            ypdm = byxx.ypdm,
                            sl = byxx.sl.ToString(),
                            dw = byxx.ckdw,
                            zhyz = byxx.ckzhyz.ToString(),
                            pfj = byxx.ykpfj.ToString(),
                            lsj = byxx.yklsj.ToString(),
                            zje = byxx.zje.ToString(),
                            zt = "1",
                            CreatorCode = user.rygh,
                            CreateTime = DateTime.Now,
                            LastModifierCode = "",
                            LastModifyTime = null,
                            gg = byxx.gg,
                            yplb = byxx.dlmc,
                            yxq = byxx.yxq,
                            sccj = byxx.sccj,
                            ypmc = byxx.ypmc,
                            pc = byxx.pc,
                            ph = byxx.ph
                        };
                        db.Insert(bymxEntity);
                    };
                    db.Commit();
                }
                if (Djnr.issavesubmit == "1")//提交状态发送给药房
                {
                    Djnr.bqbm = Djnr.rkbm;
                    foreach (var byxx in Djnr.mx)
                    {
                        byxx.dw = byxx.ckdw;
                        byxx.zhyz = byxx.ckzhyz.ToString();
                        byxx.pfj = byxx.ykpfj.ToString();
                        byxx.lsj = byxx.yklsj.ToString();
                        byxx.yplb = byxx.dlmc;
                    }
                    var apires = SiteYfykAPIHelper.Request<APIRequestHelper.DefaultResponse>("/api/Stock/PrepareMedicine", request, autoAppendToken: false);
                    if (apires.code == APIRequestHelper.ResponseResultCode.SUCCESS && apires.msg == "")
                    {
                        return "保存并提交成功！";
                    }
                    else {
                        return "调用药房接口失败，请联系开发人员！";
                    }
                }
                return "科室备药保存成功！";
            }
            catch (Exception ex)
            {
                return "" + ex.InnerException.ToString();
            }
        }
        /// <summary>
        /// 提交备药申请单
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orgId"></param>
        /// <param name="Djnr"></param>
        /// <returns></returns>
        public string PrepareMedicineSubmit(string ById, string orgId, OperatorModel user)
        {
            try
            {
                var strSql = new StringBuilder(@"
select yfbm,bqbm,ksbm,djh,shzt from  zy_bqksby where  OrganizeId=@orgId and zt='1' and Id=@byId
");
                var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@byId", (ById??"").Trim())
            };
                
                var bYDjInfoDTO=FindList<BYDjInfoSubmit>(strSql.ToString(), param.ToArray());
                if (bYDjInfoDTO == null)
                {
                    return "发送失败，单据查询失败！";
                }
                BYDjInfoDTO bYDjInfo = new BYDjInfoDTO();
                foreach (var item in bYDjInfoDTO)
                {
                    bYDjInfo.ksbm = item.ksbm;
                    //bYDjInfo.rkbm = item.rkbm;
                    bYDjInfo.yfbm = item.yfbm;
                    bYDjInfo.bqbm = item.bqbm;
                    bYDjInfo.djh = item.djh;
                    bYDjInfo.shzt = item.shzt;
                }
                var strSql2 = new StringBuilder(@"
select * from  zy_bqksbymx where  OrganizeId=@orgId and zt='1' and byId=@byId
");
                var param2 = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@byId", (ById??"").Trim())
            };
                bYDjInfo.mx = FindList<DjDetailDTO>(strSql2.ToString(), param2.ToArray());
                if (bYDjInfo.mx==null)
                {
                    return "发送失败，单据明细查询失败！";
                }
                var request = new
                {
                    OrganizeId = orgId,
                    yhgh = user.rygh,
                    yplist = bYDjInfo
                };
                var apires = SiteYfykAPIHelper.Request<APIRequestHelper.DefaultResponse>("/api/Stock/PrepareMedicine", request, autoAppendToken: false);
                if (apires.code == APIRequestHelper.ResponseResultCode.SUCCESS && apires.msg == "")
                {
                    string sql = "update zy_bqksby set shzt='1' where Id=@Id and OrganizeId=@orgId and zt='1' ";
                    SqlParameter[] para = {
                            new SqlParameter("@Id", ById),
                            new SqlParameter("@orgId", orgId)
                };
                    ExecuteSqlCommand(sql, para);
                    return "成功";
                }
                else
                {
                    return "调用药房接口失败，请联系开发人员！";
                }
            }
            catch (Exception ex)
            {
                return "" + ex.InnerException.ToString();
            }
        }
        /// <summary>
        /// 撤回备药申请单
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orgId"></param>
        /// <param name="Djnr"></param>
        /// <returns></returns>
        public string PrepareMedicineback(string Djh, string orgId, OperatorModel user)
        {
            try
            {
                var request = new
                {
                    Djh= Djh,
                    OrganizeId = orgId,
                    yhgh = user.rygh,
                    shzt = "1",
                };
                var apires = SiteYfykAPIHelper.Request<APIRequestHelper.DefaultResponse>("/api/Stock/WithdrawalPreparation", request, autoAppendToken: false);
                if (apires.code == APIRequestHelper.ResponseResultCode.SUCCESS && apires.msg == "")
                {
                    string sql = "update zy_bqksby set shzt='0' where djh=@djh and OrganizeId=@orgId and zt='1' ";
                    SqlParameter[] para = {
                            new SqlParameter("@djh", Djh),
                            new SqlParameter("@orgId", orgId)
                };
                    ExecuteSqlCommand(sql, para);
                    return "成功";
                }
                else
                {
                    return "调用药房接口失败，请联系开发人员！";
                }
            }
            catch (Exception ex)
            {
                return "" + ex.InnerException.ToString();
            }
        }
        /// <summary>
        /// 作废备药申请单
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orgId"></param>
        /// <param name="Djnr"></param>
        /// <returns></returns>
        public string PrepareMedicinedelete(string ById, string orgId, OperatorModel user)
        {
            try
            {
                
               string sql = "update zy_bqksby set shzt='9' where Id=@ById and OrganizeId=@orgId and zt='1' ";
                SqlParameter[] para = {
                            new SqlParameter("@ById", ById),
                            new SqlParameter("@orgId", orgId) };
               ExecuteSqlCommand(sql, para);
               return "成功";
            }
            catch (Exception ex)
            {
                return "" + ex.InnerException.ToString();
            }
        }
        public List<DrugStockInfoVEntity> GetDrugAndStock(string yfcode, string keyWord, string organizeid)
        {
            const string sql = @"
SELECT s.dlmc,s.ypmc,s.ypCode ypdm,s.ypgg gg,SUM(s.kykc) kykc, [NewtouchHIS_PDS].dbo.f_getComplexYpSlandDw(SUM(s.kykc),s.zhyz,s.bmdw,s.zxdw) slStr,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs
,s.bzdw,s.zxdw,s.pzwh,CONVERT(NUMERIC(11,4),s.zxdwlsj) zxdwlsj,CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz) lsj,CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz) pfj,s.ycmc sccj,s.yklsj,s.ykpfj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw,yxq,pc,ph
FROM 
(
	SELECT sfdl.dlmc, yp.ypmc, bmypxx.Ypdm ypCode, ypsx.ypgg, (kcxx.kcsl-kcxx.djsl) kykc, [NewtouchHIS_PDS].dbo.f_getyfbmDw(@yfbmCode, bmypxx.Ypdm, @Organizeid) bmdw
	, [NewtouchHIS_PDS].dbo.f_getyfbmZhyz(@YfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(ypsx.pzwh,'') pzwh,yp.ycmc
	,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj,kcxx.yxq,kcxx.pc,kcxx.ph
	FROM [NewtouchHIS_PDS].dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=bmypxx.OrganizeId
	LEFT JOIN [NewtouchHIS_PDS].dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId AND kcxx.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE bmypxx.yfbmCode=@yfbmCode
	AND bmypxx.zt='1'
	AND bmypxx.OrganizeId=@Organizeid
	AND (yp.ypCode LIKE '%'+@keyWord+'%' OR yp.ypmc LIKE '%'+@keyWord+'%' OR yp.py LIKE '%'+@keyWord+'%')
) s
GROUP BY s.dlmc,s.ypmc,s.ypCode,s.ypgg,s.bmdw,s.zhyz,s.bzs,s.bzdw,s.zxdw,s.pzwh,s.zxdwlsj,s.zxdwpfj,s.ycmc,s.yklsj,s.ykpfj,s.yxq,s.pc,s.ph
";
            var parms = new List<SqlParameter>
            {
                new SqlParameter("@yfbmCode", yfcode??""),
                new SqlParameter("@Organizeid", organizeid??""),
                new SqlParameter("@keyWord", keyWord??"")
            };
            return FindList<DrugStockInfoVEntity>(sql, parms.ToArray());
        }
        /// <summary>
        /// 通过药库代码获取发药药房
        /// </summary>
        /// <param name="ykbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<PharmacyInfoVEntity> GetTheLowerYfbmCodeList(string keyword, string organizeId)
        {
            var strSql = new StringBuilder(@"
SELECT Distinct [yfbmCode] as yfbmCode
      ,[yfbmmc] as yfbmmc,py
  FROM [NewtouchHIS_Base].[dbo].[xt_yfbm] 
  where zt = '1'
  and OrganizeId=@OrganizeId
  and yjbmjb='2'
AND (yfbmCode LIKE '%'+@keyword+'%' OR yfbmmc LIKE '%'+@keyword+'%' OR yfbmId LIKE '%'+@keyword+'%') ");
            var param = new List<SqlParameter>
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyword", keyword)
            };
            return FindList<PharmacyInfoVEntity>(strSql.ToString(), param.ToArray());

        }
        
             public IList<PreparationStockVO> PreparationStockGridJson(string orgId, Pagination pagination, string ypmc)
        {
            var strSql = new StringBuilder(@"
select yp.ypmc,yp.ypcode,yp.ypgg,yp.py,kcxx.kcsl,convert(varchar(50),kcxx.yxq,120) yxq,yp.ycmc,kcxx.yfbmCode,yf.yfbmmc yfmc from xt_ksby_kcxx kcxx
left join [NewtouchHIS_Base].dbo.V_C_xt_yp yp on kcxx.ypdm=yp.ypcode and kcxx.OrganizeId=yp.OrganizeId 
left join [NewtouchHIS_Base].dbo.V_S_xt_yfbm yf on yf.yfbmCode=kcxx.yfbmCode and kcxx.OrganizeId=yf.OrganizeId 
and yp.zt='1'
where kcxx.OrganizeId=@orgId
");
            if (!string.IsNullOrWhiteSpace(ypmc))
            {
                strSql.Append(" and yp.ypmc like @ypmc ");
            }
            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@ypmc", ypmc),
            };

            return this.QueryWithPage<PreparationStockVO>(strSql.ToString(), pagination, param.ToArray());

        }
        public IList<PreparationdrugsVO> PrepareMedicineApplyGridJson(string orgId, Pagination pagination, string ksbyzt, DateTime? kssj = null, DateTime? jssj = null)
        {
            var strSql = new StringBuilder(@"
select a.*,b.yfbmmc yfmc,c.bqmc,d.name ksmc from zy_bqksby a
left join [NewtouchHIS_Base].[dbo].[xt_yfbm] b on a.yfbm=b.yfbmCode and a.OrganizeId =b.OrganizeId
left join NewtouchHIS_Base.dbo.xt_bq  c on a.bqbm=c.bqCode and a.OrganizeId =c.OrganizeId
left join NewtouchHIS_Base.dbo.sys_department d on a.ksbm=d.code and a.OrganizeId =d.OrganizeId
  where a.zt = '1'
  and a.OrganizeId=@orgId
");

            if (!(string.IsNullOrEmpty(ksbyzt)))
            {
                strSql.Append(" AND a.shzt=@ksbyzt ");
            }

            if (!( kssj.IsEmpty() || jssj.IsEmpty())  )
            {
                strSql.Append(" AND a.createTime between @kssj and @jssj+' 23:59:59' ");
            }


            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@ksbyzt", ksbyzt),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj)
            };

            return this.QueryWithPage<PreparationdrugsVO>(strSql.ToString(), pagination, param.ToArray());

        }
        /// <summary>
        /// 查询单据明细
        /// </summary>
        /// <param name="djId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<PreparationdrugsMXVO> QueryPrepareMedicine(string djId,string orgId)
        {
            var strSql = new StringBuilder(@"
select a.* from zy_bqksbymx a
  where a.zt = '1'
  and a.OrganizeId=@orgId
  and a.byId=@Id
");

           

            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@Id", djId),
            };

            return this.FindList<PreparationdrugsMXVO>(strSql.ToString(), param.ToArray());

        }
        /// <summary>
        /// 查询单据
        /// </summary>
        /// <param name="djId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<PreparationdrugsVO> QueryPrepareMedicinebyId(string djId, string orgId)
        {
            var strSql = new StringBuilder(@"
select a.*,b.yfbmmc yfmc,c.bqmc,d.name ksmc from zy_bqksby a
left join [NewtouchHIS_Base].[dbo].[xt_yfbm] b on a.yfbm=b.yfbmCode and a.OrganizeId =b.OrganizeId
left join NewtouchHIS_Base.dbo.xt_bq  c on a.bqbm=c.bqCode and a.OrganizeId =c.OrganizeId
left join NewtouchHIS_Base.dbo.sys_department d on a.ksbm=d.code and a.OrganizeId =d.OrganizeId
  where a.zt = '1'
  and a.OrganizeId=@orgId
  and a.Id=@Id
");
            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@Id", djId),
            };

            return this.FindList<PreparationdrugsVO>(strSql.ToString(), param.ToArray());

        }
        /// <summary>
         /// 获取药品数量
         /// </summary>
         /// <param name="djId"></param>
         /// <param name="orgId"></param>
         /// <returns></returns>
        public string BydjQueryKykc(string ypbm,string pc,string ph, string yfbm, string orgId)
        {
            try
            {

                string sql = @"select convert(varchar(50),sum((kcxx.kcsl-kcxx.djsl))) kykc from [NewtouchHIS_PDS].dbo.xt_yp_kcxx(NOLOCK) kcxx 
where ypdm=@ypbm and ph=@ph and pc=@pc and OrganizeId=@orgId  and yfbmCode=@yfbm  ";
                SqlParameter[] para = {
                            new SqlParameter("@ypbm", ypbm),
                            new SqlParameter("@pc", pc),
                            new SqlParameter("@ph", ph),
                            new SqlParameter("@orgId", orgId),
                            new SqlParameter("@yfbm", yfbm) };
                return this.FirstOrDefault<string>(sql.ToString(), para.ToArray());
            }
            catch (Exception ex)
            {
                return "" + ex.InnerException.ToString();
            }
        }

        /// <summary>
        /// 获取皮试阳性信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        public IList<SkintestInfoVO> GetSkinTestInfoGridJson(Pagination pagination,string zyh, string OrganizeId)
        {
            var sql = @"select a.xmCode,a.xmmc,a.result,a.remark from [Newtouch_CIS].dbo.xt_gmxx a
left join zy_brxxk b on a.blh=b.blh and a.organizeId =b.organizeId and a.zt=b.zt
where result =1 and a.zt=1 and a.organizeId=@OrganizeId
";
            if (!string.IsNullOrEmpty(zyh))
            {
                sql += (" AND b.zyh = @zyh");
            }
                     var parms = new List<SqlParameter>
            {
                new SqlParameter("@OrganizeId", OrganizeId),
                new SqlParameter("@zyh", zyh??""),
        };

            return QueryWithPage<SkintestInfoVO>(sql, pagination, parms.ToArray(), false);
        }
    }
}
