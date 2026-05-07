using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO.OutputDto.Outpatient;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Apply;
using Newtouch.DomainServices.TSQL;
using Newtouch.Infrastructure;
using Newtouch.PDS.Requset.Zyypyz;
using Newtouch.Tools;

namespace Newtouch.DomainServices
{
    public class OrderExecutionDmnService : DmnServiceBase, IOrderExecutionDmnService
    {
        public readonly IInpatientLongTermOrderRepo _InpatientLongTermOrderRepo;
        public readonly IInpatientSTATOrderRepo _InpatientSTATOrderRepo;
        public readonly IInpatientFeeDetailRepo _InpatientFeeDetailRepo;
        public readonly IInpatientMedicineGrantRepo _InpatientMedicineGrantRepo;
        public OrderExecutionDmnService(IDefaultDatabaseFactory databaseFactory, IInpatientLongTermOrderRepo InpatientLongTermOrderRepo,
                                    IInpatientSTATOrderRepo InpatientSTATOrderRepo, IInpatientFeeDetailRepo InpatientFeeDetailRepo, IInpatientMedicineGrantRepo InpatientMedicineGrantRepo)
            : base(databaseFactory)
        {
            this._InpatientLongTermOrderRepo = InpatientLongTermOrderRepo;
            this._InpatientSTATOrderRepo = InpatientSTATOrderRepo;
            this._InpatientFeeDetailRepo = InpatientFeeDetailRepo;
            this._InpatientMedicineGrantRepo = InpatientMedicineGrantRepo;

        }

        //public IList<InpWardPatTreeVO> GetPatWardTree(string staffId, DateTime Vzxsj)
        //{
        //    string sql = @"select a.OrganizeId, b.bqCode,c.bqmc,d.zyh,d.hzxm
        //                from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
        //                 [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
        //                 [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) 
        //                left join [dbo].[zy_cqyz] d with(nolock) on c.organizeid=d.organizeid and d.wardcode=c.bqcode and d.yzzt in (1,2) and d.yzlx !=3 AND (CONVERT(DATE,d.tzsj) IS NULL OR CONVERT(DATE,d.tzsj)> =Convert(DATE,@zxsj))  AND (Convert(DATE,d.zxsj) IS NULL OR Convert(DATE,d.zxsj)<Convert(DATE,@zxsj)) and d.zt=1
        //                where a.staffid=@staffId and a.zt=1 
        //                and a.OrganizeId=b.organizeid and a.staffid=b.staffid
        //                and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
        //                group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,d.zyh,d.hzxm
        //                union
        //                select a.organizeid, b.bqcode,c.bqmc,e.zyh,e.hzxm 
        //                from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
        //                 [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
        //                 [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) 
        //                left join [dbo].[zy_lsyz] e with(nolock) on c.organizeid=e.organizeid and e.wardcode=c.bqcode and e.yzzt in (1,2) and e.yzlx !=3  AND (CONVERT(DATE,e.zfsj) IS NULL OR CONVERT(DATE,e.zfsj)> =Convert(DATE,@zxsj))  AND (Convert(DATE,e.zxsj) IS NULL OR Convert(DATE,e.zxsj)<Convert(DATE,@zxsj)) and e.zt=1
        //                where a.staffid=@staffId and a.zt=1  
        //                and a.OrganizeId=b.organizeid and a.staffid=b.staffid
        //                and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
        //                group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,e.zyh,e.hzxm";
        //    return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@staffId", staffId), new SqlParameter("@zxsj", Vzxsj) });

        //}

        #region 医嘱执行 页面查询
        /// <summary>
        /// 获取待执行医嘱病区树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetWardTree(string staffId)
        {
            string sql = @"select a.organizeid, b.bqcode,c.bqmc,'' zyh,'' hzxm 
                            from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
	                            [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) 
                            where a.staffid=@staffId and a.zt=1 
                            and a.OrganizeId=b.organizeid and a.staffid=b.staffid
                            and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
                            group by a.organizeid, b.bqcode,c.bqmc ";
            return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@staffId", staffId) });
        }
        /// <summary>
        /// 获取待执行医嘱患者树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatTree(string staffId, DateTime Vzxsj,bool wnes,string orgId)
        {
            var inParameters = new List<SqlParameter>();
            inParameters.Add(new SqlParameter("@staffId", staffId));
            inParameters.Add(new SqlParameter("@Vzxsj", Vzxsj));
            inParameters.Add(new SqlParameter("@wnes", wnes));
            inParameters.Add(new SqlParameter("@orgId", orgId));
            return this.FindList<InpWardPatTreeVO>("exec [usp_zy_OrderExecutionGetPatTree] @staffId,@Vzxsj,@orgId,@wnes", inParameters.ToArray());
            //string sql = @"select a.OrganizeId, b.bqCode,c.bqmc,d.zyh,d.hzxm
            //                from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
            //                 [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
            //                 [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock),
            //                 [dbo].[zy_cqyz] d with(nolock)  
            //                where a.staffid=@staffId and a.zt=1 
            //                and a.OrganizeId=b.organizeid and a.staffid=b.staffid
            //                and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
            //                and c.organizeid=d.organizeid and d.wardcode=c.bqcode and d.yzzt in (1,2) and d.zt=1
            //                and d.yzlx !=3 AND (CONVERT(DATE,d.tzsj) IS NULL OR CONVERT(DATE,d.tzsj)> =Convert(DATE,@zxsj)) AND (Convert(DATE,d.zxsj) IS NULL OR Convert(DATE,d.zxsj)<Convert(DATE,@zxsj))
            //                group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,d.zyh,d.hzxm
            //                union
            //                select a.organizeid, b.bqcode,c.bqmc,e.zyh,e.hzxm 
            //                from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
            //                 [NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
            //                 [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock),
            //                 [dbo].[zy_lsyz] e with(nolock) 
            //                where a.staffid=@staffId and a.zt=1 
            //                and a.OrganizeId=b.organizeid and a.staffid=b.staffid
            //                and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
            //                and c.organizeid=e.organizeid and e.wardcode=c.bqcode and e.yzzt =1 and e.zt=1
            //                and e.yzlx !=3  AND (CONVERT(DATE,e.zfsj) IS NULL OR CONVERT(DATE,e.zfsj)> =Convert(DATE,@zxsj))  AND (Convert(DATE,e.zxsj) IS NULL OR Convert(DATE,e.zxsj)<Convert(DATE,@zxsj))
            //                group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,e.zyh,e.hzxm ";
            //return this.FindList<InpWardPatTreeVO>(sql, new[] { new SqlParameter("@staffId", staffId), new SqlParameter("@zxsj", Vzxsj) });
        }

        /// <summary>
        /// 获取待执行医嘱患者树（包括文字医嘱）
        /// </summary>
        /// <param name="staffId"></param>
        /// <param name="vzxsj"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatTreeIncludeWzyz(string staffId, DateTime vzxsj)
        {
            const string sql = @"
select a.OrganizeId, b.bqCode,c.bqmc,d.zyh,d.hzxm
from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
[NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
[NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock),
[dbo].[zy_cqyz] d with(nolock)  
where a.staffid=@staffId and a.zt=1 
and a.OrganizeId=b.organizeid and a.staffid=b.staffid
and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
and c.organizeid=d.organizeid and d.wardcode=c.bqcode and d.yzzt in (1,2) and d.zt=1
and (CONVERT(DATE,d.tzsj) IS NULL OR CONVERT(DATE,d.tzsj)> =Convert(DATE,@zxsj)) AND (Convert(DATE,d.zxsj) IS NULL OR Convert(DATE,d.zxsj)<Convert(DATE,@zxsj))
group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,d.zyh,d.hzxm
union
select a.organizeid, b.bqcode,c.bqmc,e.zyh,e.hzxm 
from [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] a with(nolock),
[NewtouchHIS_Base].[dbo].[V_S_Sys_StaffWard] b with(nolock),
[NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock),
[dbo].[zy_lsyz] e with(nolock) 
where a.staffid=@staffId and a.zt=1 
and a.OrganizeId=b.organizeid and a.staffid=b.staffid
and b.OrganizeId=c.organizeid and b.bqcode=c.bqcode
and c.organizeid=e.organizeid and e.wardcode=c.bqcode and e.yzzt =1 and e.zt=1
and (CONVERT(DATE,e.zfsj) IS NULL OR CONVERT(DATE,e.zfsj)> =Convert(DATE,@zxsj))  AND (Convert(DATE,e.zxsj) IS NULL OR Convert(DATE,e.zxsj)<Convert(DATE,@zxsj))
group by a.organizeid, a.userid,a.staffid,a.account,a.gh,a.name ,b.bqcode,c.bqmc,e.zyh,e.hzxm
";
            var param = new DbParameter[]
            {
                new SqlParameter("@staffId", staffId),
                new SqlParameter("@zxsj", vzxsj)
            };
            return FindList<InpWardPatTreeVO>(sql, param);
        }

        /// <summary>
        /// 获取分页医嘱列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="patList"></param>
        /// <param name="Vzxsj"></param>
        /// <returns></returns>
        public IList<OrderExecutionVO> GetOrderExecutionYZList(ref Pagination pagination, string patList, string orgId, string Vzxsj,bool wnes, string IsRehabAuthtoNurse = null, bool Iskf = false,string zxks=null)
        {
            var inParameters = new List<SqlParameter>();
            inParameters.Add(new SqlParameter("@patList", patList));
            inParameters.Add(new SqlParameter("@vzxsj", Vzxsj));
            inParameters.Add(new SqlParameter("@wnes", wnes));
            inParameters.Add(new SqlParameter("@IsRehabAuthtoNurse", IsRehabAuthtoNurse??""));
            inParameters.Add(new SqlParameter("@Iskf", Iskf == true ? "1" : "0"));
            inParameters.Add(new SqlParameter("@zxks", zxks??""));
            inParameters.Add(new SqlParameter("@orgId", orgId));
            inParameters.Add(new SqlParameter("@page", pagination.page));
            inParameters.Add(new SqlParameter("@rows", pagination.rows));
            inParameters.Add(new SqlParameter("@sidx", pagination.sidx ?? ""));
            inParameters.Add(new SqlParameter("@sord", pagination.sord ?? ""));
            var outParameter = new SqlParameter("@records", System.Data.SqlDbType.Int);
            outParameter.Direction = ParameterDirection.Output;
            inParameters.Add(outParameter);
            var list= this.FindList<OrderExecutionVO>(@"exec [usp_zy_OrderExecutionGetOrderExecutionYZList] @patList=@patList,@vzxsj=@vzxsj,@orgId=@orgId,@wnes=@wnes ,@IsRehabAuthtoNurse=@IsRehabAuthtoNurse,@Iskf=@Iskf,@zxks=@zxks
--
, @page=@page, @rows=@rows, @sidx=@sidx, @sord=@sord
--
,@records=@records out", inParameters.ToArray());
            pagination.records = Convert.ToInt32(outParameter.Value);
            return list;
            //            const string sql = @" 
            //Select * 
            //FROM( 
            //	SELECT hzxm,zyh,id yzid,2 yzxz,'长期' yzxzsm,kssj,xmmc,ypjl,yznr, ISNULL(c.dj,0) dj,a.tzsj tzsj,tzysgh,tzr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,a.zxr zxr
            //	from [dbo].[zy_cqyz] a with(nolock) 
            //	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c  ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt='1'
            //	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId
            //	where a.OrganizeId=@orgId AND a.yzzt in (1,2) AND a.yzlx !=3
            //	AND Convert(DATE,a.kssj)< =Convert(DATE,@zxsj) 
            //	AND (Convert(DATE,a.tzsj) IS NULL OR Convert(DATE,a.tzsj)>=Convert(DATE,@zxsj)) 
            //	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)<Convert(DATE,@zxsj)) 
            //	AND a.zt=1 And a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
            //	union
            //	select hzxm,zyh, id yzid,1 yzxz,'临时' yzxzsm,kssj,xmmc,ypjl,yznr,ISNULL(c.dj,0) dj,a.zfsj tzsj,zfysgh,zfr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,a.zxr zxr
            //	from [dbo].[zy_lsyz] a  with(nolock)
            //	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c with(nolock) ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt='1'
            //	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId	 
            //	where a.OrganizeId=@orgId AND a.yzzt =1
            //	AND  Convert(DATE,a.kssj)< =Convert(DATE,@zxsj) AND a.yzlx !=3  
            //	AND (Convert(DATE,a.zfsj) IS NULL OR Convert(DATE,a.zfsj)>=Convert(DATE,@zxsj))
            //	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)< Convert(DATE,@zxsj))
            //	AND a.zt=1 and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
            //) k
            //";
            //            var param = new DbParameter[]
            //            {
            //                new SqlParameter("@zxsj",Vzxsj),
            //                new SqlParameter("@orgId",orgId),
            //                new SqlParameter("@zyh", patList)
            //            };
            //            return QueryWithPage<OrderExecutionVO>(sql, pagination, param);
        }

        /// <summary>
        /// 获取分页医嘱列表(包括文字医嘱)
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="patList"></param>
        /// <param name="orgId"></param>
        /// <param name="vzxsj"></param>
        /// <returns></returns>
        public IList<OrderExecutionVO> GetOrderExecutionYZListIncludeWzyz(Pagination pagination, string patList, string orgId, string vzxsj)
        {
            const string sql = @" 
Select * 
FROM( 
	SELECT hzxm,zyh,id yzid,2 yzxz,'长期' yzxzsm,kssj,xmmc,ypjl,yznr, ISNULL(c.dj,0) dj,a.tzsj tzsj,tzysgh,tzr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,a.zxr zxr
	from [dbo].[zy_cqyz] a with(nolock) 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c  ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt='1'
	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId
	where a.OrganizeId=@OrganizeId AND a.yzzt in (1,2)
	AND Convert(DATE,a.kssj)< =Convert(DATE,@zxsj) 
	AND (Convert(DATE,a.tzsj) IS NULL OR Convert(DATE,a.tzsj)>=Convert(DATE,@zxsj)) 
	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)<Convert(DATE,@zxsj)) 
	AND a.zt=1 And a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
	union All
	select hzxm,zyh, id yzid,1 yzxz,'临时' yzxzsm,kssj,xmmc,ypjl,yznr,ISNULL(c.dj,0) dj,a.zfsj tzsj,zfysgh,zfr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,a.zxr zxr
	from [dbo].[zy_lsyz] a  with(nolock)
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c with(nolock) ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt='1'
	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId	 
	where a.OrganizeId=@OrganizeId AND a.yzzt =1
	AND  Convert(DATE,a.kssj)< =Convert(DATE,@zxsj)
	AND (Convert(DATE,a.zfsj) IS NULL OR Convert(DATE,a.zfsj)>=Convert(DATE,@zxsj))
	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)< Convert(DATE,@zxsj))
	AND a.zt=1 and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')
) k
";
            var param = new DbParameter[]
            {
                new SqlParameter("@zxsj",vzxsj),
                new SqlParameter("@OrganizeId",orgId),
                new SqlParameter("@zyh", patList)
            };
            return QueryWithPage<OrderExecutionVO>(sql, pagination, param);
        }

        /// <summary>
        /// 获取分页医嘱列表by医嘱性质/长期或临时 暂不用
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="patList"></param>
        /// <param name="Yzxz"></param>
        /// <returns></returns>
        public IList<OrderExecutionVO> GetOrderExecutionYZListbyYzxz(string patList, int Yzxz)
        {
            string sql = "";
            if (Yzxz == Convert.ToInt32(EnumYzxz.Cq))
            {
                sql = @"select id yzid,2 yzxz,'长期' yzxzsm,kssj,xmmc,ypjl,yznr,tzsj,tzysgh,tzr 
                    from [dbo].[zy_cqyz] with(nolock) 
                    where yzzt=1 and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
                    ";
            }
            else if (Yzxz == Convert.ToInt32(EnumYzxz.Ls))
            {

                sql = @"
                    select id yzid,1 yzxz,'临时' yzxzsm,kssj,xmmc,ypjl,yznr,zfsj tzsj,zfysgh tzysgh,zfr  
                    from [dbo].[zy_lsyz] with(nolock) 
                    where yzzt=1 and zyh in(select col from dbo.f_split(@zyh,',') where col>'')";
            }
            return this.FindList<OrderExecutionVO>(sql, new[] { new SqlParameter("@zyh", patList) });
        }


        /// <summary>
        /// 获取执行医嘱ApiList
        /// </summary>
        /// <param name="orderExeList">yzid,yzxx,zyh,yzxl</param>
        /// <returns></returns>
        public List<YzDetail> GetapiListOld(OperatorModel user, IList<ApiResponseVO> orderExeList, DateTime Vzxsj, int lyxh)
        {

            try
            {
                List<YzDetail> orderExecutionDto = new List<YzDetail>();
                if (orderExeList.Count > 0)
                {
                    foreach (var Item in orderExeList)
                    {
                        string sql = "";
                        sql += @"SELECT cqyz.Id yzid,CONVERT(BIGINT,0) lyxh,hzxm patientName,cqyz.zyh zyh, xmdm ypCode, '' dl,CONVERT(DECIMAL(10),sl) sl,yf.yfmc zlff,
       cqyz.zxsjd sjap, pc.yzpcmc pcmc , CONVERT(DECIMAL(10,2),ypjl) yl, dw yldw,cqyz.ysgh ysgh,cqyz.kssj ksrq, getdate() jsrq,zxksdm fyyf,@cqyzxz yzxz,ztnr yzbz,
		cqyz.zxcs zxsl,cqyz.DeptCode ksCode,bq.bqCode bqCode,cw.cwmc cw,0 fybz,@zxr yzzxsqr,cqyz.zh zh,(case when cqyz.yzlx=@zcy then cqyz.sl else null end)ts,cqyz.yzlx
		FROM dbo.zy_cqyz cqyz with(nolock)
		INNER JOIN  dbo.zy_brxxk brxx  with(nolock) ON brxx.zyh=cqyz.zyh AND
		                                   brxx.OrganizeId=cqyz.OrganizeId 
	    LEFT JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = cqyz.WardCode
                                                  AND bq.OrganizeId = cqyz.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.bqCode = cqyz.WardCode
		                                         AND cw.cwCode=brxx.BedCode
                                                 AND cw.OrganizeId = cqyz.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = cqyz.ysgh
                               AND staff.OrganizeId = cqyz.OrganizeId 
		LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] pc ON pc.yzpcCode=cqyz.pcCode AND pc.OrganizeId=cqyz.OrganizeId
		AND pc.zt='1' 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] yf ON yf.yfCode=cqyz.ypyfdm 
		AND yf.zt='1' 
       WHERE cqyz.id=@yzid 
        UNION
  SELECT lsyz.Id yzid, CONVERT(BIGINT,0) lyxh,hzxm patientName,lsyz.zyh zyh, xmdm ypCode, '' dl,CONVERT(DECIMAL(10),sl) sl,yf.yfmc zlff,lsyz.zxsjd
         sjap, pc.yzpcmc pcmc, CONVERT(DECIMAL(10,2),ypjl) yl, dw yldw,lsyz.ysgh ysgh,lsyz.kssj ksrq,getdate() jsrq,zxksdm fyyf, @lsyzxz yzxz,ztnr yzbz,
		lsyz.zxcs zxsl, lsyz.DeptCode ksCode,bq.bqCode bqCode,cw.cwmc cw,0 fybz,@zxr yzzxsqr,lsyz.zh zh ,(case when lsyz.yzlx=@zcy then lsyz.sl else null end)ts,lsyz.yzlx
		FROM dbo.zy_lsyz lsyz  with(nolock)
		INNER JOIN  dbo.zy_brxxk brxx  with(nolock) ON brxx.zyh=lsyz.zyh AND
		                                   brxx.OrganizeId=lsyz.OrganizeId 
	    LEFT JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = lsyz.WardCode
                                                 AND bq.OrganizeId = lsyz.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.bqCode = lsyz.WardCode
		                                         AND cw.cwCode=brxx.BedCode
                                                 AND cw.OrganizeId = lsyz.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = lsyz.ysgh
                                        AND staff.OrganizeId = lsyz.OrganizeId  
		LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] pc ON pc.yzpcCode=lsyz.pcCode AND pc.OrganizeId=lsyz.OrganizeId
		AND pc.zt='1' 
        LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] yf ON yf.yfCode=lsyz.ypyfdm 
		AND yf.zt='1' 
        WHERE lsyz.id=@yzid ";
                        YzDetail orderdto = this.FindList<YzDetail>(sql, new[]
                        { new SqlParameter("@cqyzxz",((int)EnumYzxz.Cq).ToString()),
                            new SqlParameter("@yzid", Item.yzid),
                            new SqlParameter("@lsyzxz", ((int)EnumYzxz.Ls).ToString()),
                            new SqlParameter("@zxr", user.UserCode),
                            new SqlParameter("@zcy", (int)EnumYzlx.zcy),
                        }).FirstOrDefault();
                        orderdto.lyxh = lyxh;
                        orderdto.zxrq = Vzxsj;
                        orderExecutionDto.Add(orderdto);
                    }
                    return orderExecutionDto;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region 获取执行医嘱ApiList new 
        public List<YzDetail> GetapiList(OperatorModel user, IList<ApiResponseVO> orderExeList, DateTime Vzxsj, int lyxh)
        {
            try {
                List<YzDetail> orderExecutionDto = new List<YzDetail>();
                string yzidstr = "";
                if (orderExeList != null && orderExeList.Count > 0)
                {
                    var yzArr = orderExeList.Select(p=>p.yzid).ToArray();
                    yzidstr = string.Join("','", yzArr);
                }
                string sql = @"SELECT cqyz.Id yzid,hzxm patientName,cqyz.zyh zyh, xmdm ypCode, '' dl,
        (case when cqyz.yzlx=@zcy then CONVERT(DECIMAL(10),sl*ypjl) else CONVERT(DECIMAL(10),sl) end) sl,yf.yfmc zlff,
       cqyz.zxsjd sjap, pc.yzpcmc pcmc , CONVERT(DECIMAL(10,2),ypjl) yl, dw yldw,cqyz.ysgh ysgh,cqyz.kssj ksrq, getdate() jsrq,zxksdm fyyf,@cqyzxz yzxz,ztnr yzbz,
		isnull(cqyz.zxcs,1) zxsl,cqyz.DeptCode ksCode,bq.bqCode bqCode,cw.cwmc cw,0 fybz,@zxr yzzxsqr,cqyz.zh zh,
        (case when cqyz.yzlx=@zcy then convert(decimal(10,0),cqyz.sl) else null end)ts,cqyz.yzlx,CONVERT(BIGINT,@lyxh) lyxh,@zxrq zxrq
		FROM dbo.zy_cqyz cqyz with(nolock)
		INNER JOIN  dbo.zy_brxxk brxx  with(nolock) ON brxx.zyh=cqyz.zyh AND
		                                   brxx.OrganizeId=cqyz.OrganizeId 
	    LEFT JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = cqyz.WardCode
                                                  AND bq.OrganizeId = cqyz.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.bqCode = cqyz.WardCode
		                                         AND cw.cwCode=brxx.BedCode
                                                 AND cw.OrganizeId = cqyz.OrganizeId  and cw.sfzy=1
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = cqyz.ysgh
                               AND staff.OrganizeId = cqyz.OrganizeId 
		LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] pc ON pc.yzpcCode=cqyz.pcCode AND pc.OrganizeId=cqyz.OrganizeId
		AND pc.zt='1' 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] yf ON yf.yfCode=cqyz.ypyfdm 
		AND yf.zt='1' 
       WHERE  cqyz.zt='1' and brxx.zt='1' and  brxx.zybz<>@exceptbrzt  and cqyz.id in('" + yzidstr + @"')
        UNION
  SELECT lsyz.Id yzid, hzxm patientName,lsyz.zyh zyh, xmdm ypCode, '' dl,
(case when lsyz.yzlx=@zcy then CONVERT(DECIMAL(10),sl*ypjl) else CONVERT(DECIMAL(10),sl) end) sl,yf.yfmc zlff,lsyz.zxsjd
         sjap, pc.yzpcmc pcmc, CONVERT(DECIMAL(10,2),ypjl) yl, dw yldw,lsyz.ysgh ysgh,lsyz.kssj ksrq,getdate() jsrq,zxksdm fyyf, @lsyzxz yzxz,ztnr yzbz,
		isnull(lsyz.zxcs,'1') zxsl, lsyz.DeptCode ksCode,bq.bqCode bqCode,cw.cwmc cw,0 fybz,@zxr yzzxsqr,lsyz.zh zh ,
(case when lsyz.yzlx=@zcy then convert(decimal(10,0),lsyz.sl) else null end)ts,lsyz.yzlx ,CONVERT(BIGINT,@lyxh) lyxh,@zxrq zxrq
		FROM dbo.zy_lsyz lsyz  with(nolock)
		INNER JOIN  dbo.zy_brxxk brxx  with(nolock) ON brxx.zyh=lsyz.zyh AND
		                                   brxx.OrganizeId=lsyz.OrganizeId 
	    LEFT JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = lsyz.WardCode
                                                 AND bq.OrganizeId = lsyz.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.bqCode = lsyz.WardCode
		                                         AND cw.cwCode=brxx.BedCode
                                                 AND cw.OrganizeId = lsyz.OrganizeId and cw.sfzy=1
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = lsyz.ysgh
                                        AND staff.OrganizeId = lsyz.OrganizeId  
		LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] pc ON pc.yzpcCode=lsyz.pcCode AND pc.OrganizeId=lsyz.OrganizeId
		AND pc.zt='1' 
        LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] yf ON yf.yfCode=lsyz.ypyfdm 
		AND yf.zt='1' 
        WHERE lsyz.zt='1' and brxx.zt='1' and brxx.zybz<>@exceptbrzt and lsyz.id in('" + yzidstr + @"') ";
                 orderExecutionDto = this.FindList<YzDetail>(sql, new[]
                        { new SqlParameter("@cqyzxz",((int)EnumYzxz.Cq).ToString()),
                            new SqlParameter("@lsyzxz", ((int)EnumYzxz.Ls).ToString()),
                            new SqlParameter("@zxr", user.UserCode),
                            new SqlParameter("@zcy", (int)EnumYzlx.zcy),
                            new SqlParameter("@lyxh", lyxh),
                            new SqlParameter("@zxrq", Vzxsj),
                            new SqlParameter("@exceptbrzt", (int)EnumZYBZ.Wry),
                        });
                return orderExecutionDto;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

        #region 医嘱执行 执行操作

        /// <summary>
        /// 提交执行 药品
        /// </summary>
        /// <param name="orderList"></param>
        public string OrderExecutionSubmit(OperatorModel user, IList<YzDetail> orderExeList, int lyxh, DateTime zxrq)
        {
            try
            {
                string yzxhlist = string.Empty;
                if (orderExeList.Count > 0)
                {
                    foreach (var Item in orderExeList)
                    {
                        yzxhlist += Item.yzId + ",";
                    }
                    yzxhlist = yzxhlist.Substring(0, yzxhlist.Length - 1);
                    var inParameters = new List<SqlParameter>();
                    inParameters.Add(new SqlParameter("@yzxhlist", yzxhlist));
                    inParameters.Add(new SqlParameter("@lyxh", lyxh));
                    inParameters.Add(new SqlParameter("@zxrq", zxrq));
                    inParameters.Add(new SqlParameter("@czyh", user.rygh));

                    var outParameter = new SqlParameter("@errmsg", System.Data.SqlDbType.VarChar, 300);
                    outParameter.Direction = System.Data.ParameterDirection.Output;
                    inParameters.Add(outParameter);
                    _databaseFactory.Get().Database.ExecuteSqlCommand("exec usp_zy_OrderExecution @yzxhlist,@lyxh,@zxrq,@czyh,@errmsg out ", inParameters.ToArray());
                    
                    return outParameter.Value.ToString();
                }
                return "T|执行药品成功";
            }
            catch (Exception ex)
            {
                return "F|执行失败：" + ex.InnerException;
            }
        }
        /// <summary>
        /// 执行医嘱信息 药品项目执行 by 长期/临时/全部 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orderExeList"></param>
        /// <param name="lyxh"></param>
        /// <param name="zxrq"></param>
        /// <param name="yzxz"></param>
        /// <returns></returns>
        public string OrderExecutionSubmitbyYzxz(OperatorModel user, IList<YzDetail> orderExeList, int lyxh, DateTime zxrq,int? yzxz,int? isyp)
        {
            try
            {
                if (yzxz == null || orderExeList.Count == 0)
                {
                    return "F|执行失败：无效操作，请刷新重试" ;
                }

                var zyhArr = orderExeList.Select(p => p.zyh).Distinct().ToArray();
                string zyhs = string.Join(",", zyhArr);
                if (!string.IsNullOrWhiteSpace(zyhs) && yzxz != null)
                {
                    return FirstOrDefault<string>("exec [dbo].[usp_zy_OrderExecutionbyYzxz] @orgId=@orgId,@zyhs=@zyhs,@czyh=@czyh,@zxrq=@zxrq,@lyxh=@lyxh,@yzxz=@yzxz ",
                        new DbParameter[]{
                                    new SqlParameter("@lyxh", lyxh),
                                    new SqlParameter("@zxrq", zxrq),
                                    new SqlParameter("@czyh", user.rygh),
                                    new SqlParameter("@orgId", user.OrganizeId),
                                    new SqlParameter("@zyhs", zyhs??""),
                                    new SqlParameter("@yzxz", yzxz??-1),
                                    new SqlParameter("@isyp", isyp??-1),
                         });
                }
                else
                {
                    return "F|无效操作，请刷新重试";
                }
            }
            catch (Exception ex)
            {
                return "F|执行失败：" + ex.InnerException;
            }
        }
        /// <summary>
        /// 不计费、科室备药开立医嘱执行
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orderExeList"></param>
        /// <param name="lyxh"></param>
        /// <param name="zxrq"></param>
        /// <returns></returns>
        public string NoFeeOrderExecutionSubmit(OperatorModel user, IList<ApiResponseVO> orderExeList, int lyxh, DateTime zxrq)
        {
            try
            {
                string yzxhlist = string.Empty;
                if (orderExeList.Count > 0)
                {
                    foreach (var Item in orderExeList)
                    {
                        yzxhlist += Item.yzid + ",";
                    }
                    yzxhlist = yzxhlist.Substring(0, yzxhlist.Length - 1);
                    var inParameters = new List<SqlParameter>();
                    inParameters.Add(new SqlParameter("@yzxhlist", yzxhlist));
                    inParameters.Add(new SqlParameter("@lyxh", lyxh));
                    inParameters.Add(new SqlParameter("@zxrq", zxrq));
                    inParameters.Add(new SqlParameter("@czyh", user.rygh));

                    var outParameter = new SqlParameter("@errmsg", System.Data.SqlDbType.VarChar, 300);
                    outParameter.Direction = System.Data.ParameterDirection.Output;
                    inParameters.Add(outParameter);
                    var rusult= FirstOrDefault<string>("exec usp_zy_OrderExecution @yzxhlist,@lyxh,@zxrq,@czyh,@errmsg out",
                       inParameters.ToArray());
                    return outParameter.Value.ToString();
                }
                return "T|执行药品成功";
            }
            catch (Exception ex)
            {
                return "F|执行失败：" + ex.InnerException;
            }
        }

        /// <summary>
        /// 执行膳食费用项目执行(不包含文字医嘱)
        /// </summary>
        public string OrderExecutionXM(OperatorModel user, IList<ApiResponseVO> orderExeList, int lyxh, DateTime zxrq,string orgId,string medicalInsurance, int? yzxz = null)
        {
            try
            {                
                if (orderExeList.Count <= 0) return "T|执行项目成功";
                
                if (yzxz != null)
                {
                    string zyhs = "";
                    var zyhArr = orderExeList.Select(p => p.zyh).ToArray();
                    zyhs = string.Join(",", zyhArr);
                    if (!string.IsNullOrWhiteSpace(zyhs) && yzxz != null)
                    {
                        return FirstOrDefault<string>("exec [dbo].[usp_zy_OrderExecutionbyYzxz] @orgId=@orgId,@zyhs=@zyhs,@yzxz=@yzxz,@lyxh=@lyxh,@zxrq=@zxrq,@czyh=@czyh ",
                            new DbParameter[]{
                                    new SqlParameter("@lyxh", lyxh),
                                    new SqlParameter("@zxrq", zxrq),
                                    new SqlParameter("@czyh", user.rygh),
                                    new SqlParameter("@orgId", orgId),
                                    new SqlParameter("@zyhs", zyhs??""),
                                    new SqlParameter("@yzxz", yzxz??-1),
                             });
                    }
                    else
                    {
                        return "F|获取执行数据失败，请刷新重试";
                    }
                }
                var yzxhlist = string.Empty;
                foreach (var item in orderExeList)
                {
                    if (item.yzlx == Convert.ToInt32(EnumYzlx.jy) || item.yzlx == Convert.ToInt32(EnumYzlx.jc))
                    {
                        string strsql = @"select Id from zy_lsyz where yzh=@yzid and zyh=@zyh and OrganizeId=@orgId and zt=1 and (yzlx=@jyyz or yzlx=@jcyz)";
                        var jyjcyz = this.FindList<yzinfoVo>(strsql, new[] { new SqlParameter("@yzid", item.zh1),
                           new SqlParameter("@zyh", item.zyh), new SqlParameter("@orgId", orgId),new SqlParameter("@jyyz",((int)EnumYzlx.jy).ToString()) ,new SqlParameter("@jcyz",((int)EnumYzlx.jc).ToString() )});
                        if (jyjcyz != null && jyjcyz.Count() > 0)
                        {
                            foreach (var jyjcxm in jyjcyz)
                            {
                                yzxhlist += jyjcxm.Id + ",";
                            }
                        }
                    }
                    else
                    {
                        yzxhlist += item.yzid + ",";
                    }
                    
                }
                yzxhlist = yzxhlist.Trim(',');
                //db.Commit();
                var inParameters = new List<SqlParameter>
                {
                    new SqlParameter("@yzxhlist", yzxhlist),
                    new SqlParameter("@lyxh", lyxh),
                    new SqlParameter("@zxrq", zxrq),
                    new SqlParameter("@czyh", user.rygh)
                };

                var outParameter = new SqlParameter("@errmsg", System.Data.SqlDbType.VarChar, 300)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                inParameters.Add(outParameter);
                if (medicalInsurance=="on")
                {
                    _databaseFactory.Get().Database.ExecuteSqlCommand("exec usp_zy_qhdkfOrderExecution @yzxhlist,@lyxh,@zxrq,@czyh,@errmsg out ", inParameters.ToArray());
                }
                else
                {
                    _databaseFactory.Get().Database.ExecuteSqlCommand("exec usp_zy_OrderExecution @yzxhlist,@lyxh,@zxrq,@czyh,@errmsg out ", inParameters.ToArray());
                }
                
                return outParameter.Value.ToString();

            }
            catch (Exception ex)
            {
                return "F|执行失败：" + ex.InnerException;
            }
        }

        /// <summary>
        /// 通用项目执行
        /// </summary>
        /// <param name="rygh"></param>
        /// <param name="orderExeList"></param>
        /// <param name="lyxh"></param>
        /// <param name="zxrq"></param>
        /// <returns></returns>
        public string OrderExecutionXmWithWzyz(string rygh, IList<ApiResponseVO> orderExeList, int lyxh, DateTime zxrq,string orgId, int? yzxz = null)
        {
            try
            {
                string jyjcyz = "";
                string zyhs = "";
                if (orderExeList.Count <= 0) return "T|执行项目成功";
                var yzxhlist = string.Empty;
                var yzxhlistbindfee = string.Empty;
                var zyhArr = orderExeList.Select(p => p.zyh).Distinct().ToArray();
                zyhs = string.Join(",", zyhArr);
                if (yzxz != null)//执行全部/长期、临时医嘱
                {
                    if (!string.IsNullOrWhiteSpace(zyhs) && yzxz != null)
                    {
                        return FirstOrDefault<string>("exec [dbo].[usp_zy_OrderExecution_TyxmWithWzbyYzlx] @orgId=@orgId,@zyhs=@zyhs,@czyh=@czyh,@zxrq=@zxrq,@lyxh=@lyxh,@yzxz=@yzxz ",
                            new DbParameter[]{
                                    new SqlParameter("@lyxh", lyxh),
                                    new SqlParameter("@zxrq", zxrq),
                                    new SqlParameter("@czyh", rygh),
                                    new SqlParameter("@orgId", orgId),
                                    new SqlParameter("@zyhs", zyhs??""),
                                    new SqlParameter("@yzxz", yzxz??-1),
                             });
                    }
                    else
                    {
                        return "F|无效操作，请刷新重试";
                    }
                }
                else {//执行当前选择医嘱
                    foreach (var item in orderExeList)
                    {
                        //if ((item.yzlx == Convert.ToInt32(EnumYzlx.jy) || item.yzlx == Convert.ToInt32(EnumYzlx.jc)) && item.zh1 != null)
                        if (item.zh1 != null)
                        {
                            jyjcyz += item.zh1 + ",";
                            //string strsql = @"select Id from zy_lsyz where yzh=@yzid and zyh=@zyh and OrganizeId=@orgId and zt=1 and (yzlx=@jyyz or yzlx=@jcyz)";
                            //var jyjcyz = this.FindList<yzinfoVo>(strsql, new[] { new SqlParameter("@yzid", item.zh1),
                            //   new SqlParameter("@zyh", item.zyh), new SqlParameter("@orgId", orgId),new SqlParameter("@jyyz",((int)EnumYzlx.jy).ToString()),new SqlParameter("@jcyz",((int)EnumYzlx.jc).ToString() ) });
                            //if (jyjcyz != null && jyjcyz.Count() > 0)
                            //{
                            //    foreach (var jyjcxm in jyjcyz)
                            //    {
                            //        yzxhlist += jyjcxm.Id + ",";
                            //    }
                            //}
                        }
                        else if (item.yfztbs!=null)
                        {
                            string strsql = @"select Id from zy_lsyz with (nolock) where  zyh=@zyh and yfztbs=@yfztbs  and OrganizeId=@orgId and zt=1 and yzlx=@yzlx 
                                                union all
                                                select Id from zy_cqyz with (nolock) where zyh=@zyh and yfztbs=@yfztbs and OrganizeId=@orgId and zt=1 and yzlx=@yzlx
";
                            var yfztbsIds = this.FindList<yzinfoVo>(strsql, new[] { new SqlParameter("@yfztbs", item.yfztbs),
                                new SqlParameter("@zyh", item.zyh), new SqlParameter("@orgId", orgId),new SqlParameter("@yzlx",((int)EnumYzlx.sfxm).ToString())});
                            if (yfztbsIds.Count() > 0)
                            {
                                foreach (var yfzt in yfztbsIds)
                                {
                                    yzxhlist += yfzt.Id + ",";
                                }
                            }
                        }
                        else
                        {
                            yzxhlist += item.yzid + ",";
                        }
                        if (item.yzid!=null)
                        {
                            yzxhlistbindfee += item.yzid + ",";
                        }
                    }
                    yzxhlist = yzxhlist.Trim(',');
                }                
                var inParameters = new DbParameter[]
                 {
                        new SqlParameter("@yzxhlist", yzxhlist),
                        new SqlParameter("@lyxh", lyxh),
                        new SqlParameter("@zxrq", zxrq),
                        new SqlParameter("@czyh", rygh),
                        new SqlParameter("@jyjclist", jyjcyz),
                        new SqlParameter("@orgId", orgId)
                 };
                var xm = FirstOrDefault<string>("exec [dbo].[usp_zy_OrderExecution_TyxmWithWz] @orgId=@orgId,@yzxhlist=@yzxhlist,@jyjcyz=@jyjclist,@czyh=@czyh,@zxrq=@zxrq,@lyxh=@lyxh", inParameters);
                System.Threading.Thread.Sleep(1000);
                //SyncPatFee(orgId, zyhs, 0);
                Updatezy_brxxexpand(orgId, zyhs);
                return xm;
                
                //var inParameters = new DbParameter[]
                //{
                //    new SqlParameter("@yzxhlist", yzxhlist),
                //    new SqlParameter("@lyxh", lyxh),
                //    new SqlParameter("@zxrq", zxrq),
                //    new SqlParameter("@czyh", rygh)
                //};
                //return FirstOrDefault<string>(InpatientYz.zyOrderExecutionWithWzYz, inParameters);
            }
            catch (Exception ex)
            {
                return "F|执行失败：" + ex.InnerException;
            }
        }
        #endregion
        
        #region 病人费用实时计算
        public void Updatezy_brxxexpand(string OrganizeId, string zyh)
        {
            try
            {
                string sql = @" exec Newtouch_CIS..usp_zy_brxxexpand_update @orgId,@zyh";
                SqlParameter[] para ={
                new SqlParameter("@orgId",OrganizeId),
                 new SqlParameter("@zyh",zyh)
                };
                int i = this.ExecuteSqlCommand(sql, para);
            }
            catch (Exception)
            {
            }

        }
        #endregion
        
        #region 生成项目费用
        /// <summary>
        /// 同步最新CPOE项目费用
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        public void SyncPatFee(string orgId, string zyh, int zxtype)
        {
            //是否启用费用同步最新机制
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                string sql = "";
                if (zxtype == 0) //同步项目费用
                {
                    sql = @" exec [NewtouchHIS_Sett]..[skd_syncxmjfbcope] @orgId=@orgId,@lqrq=@lqrq,@zyh=@zyh ";
                }
                else //同步药品费用
                {
                    sql = @" exec [NewtouchHIS_Sett]..[skd_syncypjfbcope] @orgId=@orgId,@lqrq=@lqrq,@zyh=@zyh";
                }
                SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@lqrq",DateTime.Now),
                        new SqlParameter("@zyh",zyh)
                    };
                db.ExecuteSqlCommand(sql, para);
                db.Commit();
            }
        }
        #endregion
        
        #region 执行医嘱查询
        /// <summary>
        /// 获取执行全部医嘱ALLYZ
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="zxlx"></param>
        /// <param name="zxsj"></param>
        /// <returns></returns>
        public List<ApiResponseVO> GetAllYZ(string zyh, int zxlx, DateTime Vzxsj, string IsRehabAuthtoNurse = null)
        {
            string strkfwhere = "";
            if (!string.IsNullOrWhiteSpace(IsRehabAuthtoNurse))
            {
                strkfwhere = " and yzlx<>" + (int)EnumYzlx.rehab;
            }
            try
            {
                List<ApiResponseVO> orderExecutionDto = new List<ApiResponseVO>();
                string sql = "";
                if (zxlx != Convert.ToInt32(EnumYzxz.Ls))
                {
                    sql += @"SELECT a.Id yzid,ISNULL(c.dj,0) dj,sl,@cqyzxx yzxz,yzlx yzlx,deptCode,a.xmdm, a.xmmc ypmc, hzxm patientName,a.zyh zyh,a.zxsj zxsj,case (ISNULL(isjf,1)) when '0' then '否' else '是' end isjf
FROM dbo.zy_cqyz a
LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c  ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt='1'
Where a.yzzt in (1,2) AND a.yzlx !=3 " + strkfwhere + @"
AND  Convert(DATE,kssj)< =Convert(DATE,@zxrq) AND (Convert(DATE,tzsj) IS NULL OR Convert(DATE,tzsj)>=Convert(DATE,@zxrq))AND (Convert(DATE,zxsj) IS NULL OR Convert(DATE,zxsj)<Convert(DATE,@zxrq))  and a.zt=1 and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')";
                }
                if (zxlx == 0)
                {
                    sql += @" UNION ";
                }
                if (zxlx != Convert.ToInt32(EnumYzxz.Cq))
                {
                    sql += @"SELECT a.Id yzid,ISNULL(c.dj,0) dj,sl,@lsyzxx  yzxz,yzlx yzlx,deptCode,a.xmdm,a.xmmc ypmc, hzxm patientName,a.zyh zyh,a.zxsj zxsj,case (ISNULL(isjf,1)) when '0' then '否' else '是' end isjf FROM dbo.zy_lsyz a
LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c  ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt='1'
Where a.yzzt =1 AND a.yzlx !=3  " + strkfwhere + @"
AND  Convert(DATE,kssj)< =Convert(DATE,@zxrq)  AND (Convert(DATE,zfsj) IS NULL OR Convert(DATE,zfsj)>=Convert(DATE,@zxrq))  AND (Convert(DATE,zxsj) IS NULL OR Convert(DATE,zxsj)<Convert(DATE,@zxrq)) and a.zt=1 and a.zyh in(select col from dbo.f_split(@zyh,',') where col>'')";
                }
                return this.FindList<ApiResponseVO>(sql, new[] { new SqlParameter("@zyh", zyh), new SqlParameter("@zxrq", Vzxsj), new SqlParameter("@lsyzxx ", EnumYzxz.Ls), new SqlParameter("@cqyzxx", EnumYzxz.Cq) });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 获取执行全部医嘱ALLYZ
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="zxlx"></param>
        /// <param name="Vzxsj"></param>
        /// <returns></returns>
        public List<ApiResponseVO> GetAllYZWithWzYz(string orgId, string zyh, int zxlx, DateTime Vzxsj,string IsRehabAuthtoNurse = null)
        {
            string strkfwhere = "";
            if (!string.IsNullOrWhiteSpace(IsRehabAuthtoNurse))
            {
                strkfwhere = " and yzlx<>"+(int)EnumYzlx.rehab;
            }
            var sql = new StringBuilder();
            if (zxlx != Convert.ToInt32(EnumYzxz.Ls))
            {
                sql.Append(@"
SELECT a.Id yzid,ISNULL(c.dj,0) dj,sl,@cqyzxx yzxz,yzlx yzlx,deptCode, a.xmdm,a.xmmc ypmc, hzxm patientName,a.zyh zyh,a.zxsj zxsj,case (ISNULL(isjf,1)) when '0' then '否' else '是' end isjf,Convert(varchar(50),yply) yply
FROM dbo.zy_cqyz(nolock) a
LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt='1'
Where a.yzzt in (1,2)  and a.OrganizeId=@orgId
AND Convert(DATE,kssj) <= Convert(DATE,@zxrq) 
AND (Convert(DATE,tzsj) IS NULL OR Convert(DATE,tzsj) >= Convert(DATE,@zxrq)) AND (Convert(DATE,zxsj) IS NULL OR Convert(DATE,zxsj) < Convert(DATE,@zxrq))  
and a.zt=1 
and a.zyh in (select col from dbo.f_split(@zyh,',') where col>'') " + strkfwhere);
            }
            if (zxlx == 0)
            {
                sql.Append(@" 
UNION ");
            }
            if (zxlx != Convert.ToInt32(EnumYzxz.Cq))
            {
                sql.Append(@"
SELECT a.Id yzid,ISNULL(c.dj,0) dj,sl,@lsyzxx  yzxz,yzlx yzlx,deptCode,a.xmdm,a.xmmc ypmc, hzxm patientName,a.zyh zyh,a.zxsj zxsj ,case (ISNULL(isjf,1)) when '0' then '否' else '是' end isjf,Convert(varchar(50),yply) yply
FROM dbo.zy_lsyz(nolock) a
LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt='1'
Where a.yzzt =1  and a.OrganizeId=@orgId
AND  Convert(DATE,kssj)< =Convert(DATE,@zxrq)  
AND (Convert(DATE,zfsj) IS NULL OR Convert(DATE,zfsj)>=Convert(DATE,@zxrq))  
AND (Convert(DATE,zxsj) IS NULL OR Convert(DATE,zxsj)<Convert(DATE,@zxrq)) 
and a.zt=1 
and a.zyh in (select col from dbo.f_split(@zyh,',') where col>'')" + strkfwhere);
            }

            var param = new DbParameter[]
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zxrq", Vzxsj),
                new SqlParameter("@lsyzxx ", EnumYzxz.Ls),
                new SqlParameter("@cqyzxx", EnumYzxz.Cq)
            };
            return FindList<ApiResponseVO>(sql.ToString(), param);
        }

        public List<ApiResponseVO> GetkfYz(string orgId,string zyh, int zxlx, DateTime Vzxsj,string zxks = null)
        {
            StringBuilder sql = new StringBuilder();
            if (zxlx != Convert.ToInt32(EnumYzxz.Ls))
            {
                sql.Append(@"
SELECT a.Id yzid,ISNULL(c.dj,0) dj,@cqyzxx yzxz,yzlx yzlx, a.xmmc ypmc, hzxm patientName,a.zyh zyh,a.zxsj zxsj
FROM dbo.zy_cqyz(nolock) a
LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt='1'
Where a.yzzt in (1,2) and a.OrganizeId=@orgId and a.yzlx=" + (int)EnumYzlx.rehab+@" and a.zxksdm=@zxks
AND Convert(DATE,kssj) <= Convert(DATE,@zxrq) 
AND (Convert(DATE,tzsj) IS NULL OR Convert(DATE,tzsj) >= Convert(DATE,@zxrq)) AND (Convert(DATE,zxsj) IS NULL OR Convert(DATE,zxsj) < Convert(DATE,@zxrq))  
and a.zt=1 
and a.zyh in (select col from dbo.f_split(@zyh,',') where col>'') ");
            }
            if (zxlx == 0)
            {
                sql.Append(@" 
UNION ");
            }
            if (zxlx != Convert.ToInt32(EnumYzxz.Cq))
            {
                sql.Append(@"
SELECT a.Id yzid,ISNULL(c.dj,0) dj,@lsyzxx  yzxz,yzlx yzlx,a.xmmc ypmc, hzxm patientName,a.zyh zyh,a.zxsj zxsj 
FROM dbo.zy_lsyz(nolock) a
LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt='1'
Where a.yzzt =1 and a.OrganizeId=@orgId  and a.yzlx=" + (int)EnumYzlx.rehab + @" and a.zxksdm=@zxks
AND  Convert(DATE,kssj)< =Convert(DATE,@zxrq)  
AND (Convert(DATE,zfsj) IS NULL OR Convert(DATE,zfsj)>=Convert(DATE,@zxrq))  
AND (Convert(DATE,zxsj) IS NULL OR Convert(DATE,zxsj)<Convert(DATE,@zxrq)) 
and a.zt=1 
and a.zyh in (select col from dbo.f_split(@zyh,',') where col>'')");
            }

            var param = new DbParameter[]
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@zxks", zxks),
                new SqlParameter("@zxrq", Vzxsj),
                new SqlParameter("@lsyzxx ", EnumYzxz.Ls),
                new SqlParameter("@cqyzxx", EnumYzxz.Cq)
            };
            return FindList<ApiResponseVO>(sql.ToString(), param);
        }

        /// <summary>
        /// 可以执行的医嘱信息
        /// </summary>
        /// <param name="OrderList">Tzxsj,yzid,zyh,yzlx,ypmc</param>
        /// <param name="Vzxsj"></param>
        /// <returns></returns>
        public string IsOKOrderExecution(IList<ApiResponseVO> OrderList, DateTime Vzxsj, string orgId, string user =null)
        {
            try
            {
                if (OrderList.Count > 0)
                {
                    string yzidstr = string.Join("','", OrderList.Select(p => p.yzid).ToArray());
                    //耗材物资库存验证
                    List<ApiResponseVO> hclist = OrderList.Where(m =>!string.IsNullOrWhiteSpace(m.xmdm)&& m.xmdm.Contains("wz")).ToList();
                    List<HospItemFeeVO> kcxx = new List<HospItemFeeVO>(); 
                    if (hclist.Count>0) {
                        kcxx= Verification(string.Join(",", hclist.Select(p => p.xmdm).Distinct()), orgId, string.Join(",", hclist.Select(p => p.deptCode).Distinct()));
                        var hcEx = hclist.GroupBy(p=>new { p.xmdm,p.ypmc}).Select(m=> new HospItemFeeVO { productCode=m.Key.xmdm, productName = m.Key.ypmc, kykc=m.Sum(p=>p.sl) }).ToList();
                        foreach (var i in hcEx) {
                            var resultNum = kcxx.Where(p => p.productCode == i.productCode && p.kykc >= i.kykc).Count();
                            if (resultNum == 0)
                                return "F|耗材为：" + i.productName + " 库存不足！";
                        }
                    }
                    string sql = @"select yzxh yzid	from zy_fymxk  with(nolock)
                           where yzxh in('"+ yzidstr + "')  and zt='1' and zxrq>=@zxrqday and zxrq<@zxrqaddday ";
                    var ck = FindList<ApiResponseVO>(sql, new SqlParameter[] {
                        new SqlParameter("zxrqday",Convert.ToDateTime(Vzxsj.ToShortDateString())),
                        new SqlParameter("zxrqaddday",Convert.ToDateTime(Vzxsj.AddDays(1).ToShortDateString()))
                    }).FirstOrDefault();
                    if (ck != null && !string.IsNullOrWhiteSpace(ck.yzid))
                    {
                        return "F|部分医嘱已执行，请刷新页面！";
                    }
                    foreach (var Item in OrderList)
                    {                        
                        if (Item.yzxz==2)
                        {
                           var yzentity= _InpatientLongTermOrderRepo.FindEntity(Item.yzid);
                            if (yzentity==null)
                            {
                                return "F|找不到该医嘱！";
                            }
                            var zxsjdaydiff = DateTimeManger.DateDiff(DateInterval.Day, DateTime.Parse(Convert.ToDateTime(Item.zxsj).ToShortDateString()), Vzxsj);
                            switch (yzentity.zxzqdw) {
                                case (int)EnumZxzqdw.Day:
                                    DateTime Beforeday = Convert.ToDateTime(Vzxsj.AddDays(-1).ToShortDateString());
                                    if (Beforeday >= Convert.ToDateTime(DateTime.Now.ToShortDateString()) && Item.zxsj != null && (zxsjdaydiff / yzentity.zxzq!=1|| zxsjdaydiff % yzentity.zxzq!=0))
                                    {
                                        return "F|住院号为:" + Item.zyh + "的患者，药品/项目名称为:" + Item.ypmc + "的医嘱不能隔次执行！";
                                    }
                                    break;
                                default:break;
                            }
                        }
                    }
                    //锁定医嘱执行状态

                    return "T|有可执行医嘱";

                }
                else
                {
                    return "F|没有可执行医嘱";
                }

            }
            catch (Exception ex)
            {
                throw new FailedException("执行失败" + ex.InnerException);
            }

        }

        /// <summary>
        /// 物资耗材库存验证
        /// </summary>
        /// <param name="xmCodes"></param>
        /// <param name="orgId"></param>
        /// <param name="ks"></param>
        /// <returns></returns>
        public List<HospItemFeeVO> Verification(string xmCodes, string orgId, string ks)
        {
            const string sql = @" select productCode,SUM(kcsl-djsl) kykc from Newtouch_CIS..Dept_kcxx with(nolock)
where OrganizeId=@orgId and zt='1' and ks in (select col from f_split(@ks,','))
and productCode in (select col from f_split(@sfxmCode,','))
group by productCode";
            return this.FindList<HospItemFeeVO>(sql, new[] { new SqlParameter("@sfxmCode", xmCodes)
                                , new SqlParameter("@orgId", orgId), new SqlParameter("@ks", ks) });
        }
        #endregion

        #region 消息提醒
        public IList<SysMSGQueryVO> MSGQuery(string gh,string orgId)
        {
            var sql = string.Format(@" exec [Newtouch_CIS].[dbo].[MessageQuery] @gh=@gh,@orgId=@orgId ");
            return FindList<SysMSGQueryVO>(sql, new SqlParameter[] {
                 new SqlParameter("@gh",gh),
                        new SqlParameter("@orgId",orgId)
                    });
        }
        #endregion

        #region pace接口
        public CheckApplicationfromDTO pushApplicationform(ApiResponseVO orderList, string orgid, string typezt)
        {
            CheckApplicationfromDTO checkApplicationfromDTO = new CheckApplicationfromDTO();
            string sql = @"select c.xm,(case when c.sex='1'then '男' else '女'end)xb,convert(datetime,c.birth) csny,c.sfzh zjh,c.WardCode WardNo,a.yzh CardID,a.yzh Reqno, b.CreateTime Indate,b.ysgh DoctorID,e.name DoctorName,a.zyh RecordID,b.ztmc ApplyName,c.DeptCode ApplyDepartmentCode,d.Name ApplyDepartmentName ,b.id Seqno,b.xmdm ItemID,     b.xmmc ItemName,convert(varchar(10),b.sl) Qty  from 
(select zyh,yzh,ztid,ztmc,OrganizeId from zy_lsyz a 
where a.zyh=@zyh and id=@yzid and OrganizeId=@orgId and a.yzlx='7' and a.zt='1') a
left join  zy_lsyz b on a.yzh=b.yzh and a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1'
left join zy_brxxk c on a.zyh=c.zyh and c.zt='1' and a.OrganizeId=c.OrganizeId
left join [NewtouchHIS_Base]..Sys_Department d on c.DeptCode=d.code and c.OrganizeId=d.OrganizeId and d.zt='1'
left join [NewtouchHIS_Base]..[Sys_Staff] e on c.ysgh=e.gh and c.OrganizeId=e.OrganizeId and e.zt='1'
left join [NewtouchHIS_Base]..[xt_bq] f on c.WardCode=f.bqcode and c.OrganizeId=f.OrganizeId and f.zt='1'
left join [Newtouch_CIS]..jyjc_zt g on g.ztid=b.ztId and b.OrganizeId=g.OrganizeId and g.Type='2' and g.zt='1'
";
			sql = "exec ZY_pacsxx @zyh,@yzid,@orgId";
            var cfmxlist = new List<CheckfromDTO>(); 
            try
            {
                cfmxlist = this.FindList<CheckfromDTO>(sql,
                new[] { new SqlParameter("@orgId", orgid), new SqlParameter("@zyh", orderList.zyh), new SqlParameter("@yzid", orderList.yzid) });
            }
            catch (Exception e)
            {

                throw;
            }


            if (cfmxlist.Count == 0 || cfmxlist == null)
            {
                return null;
            }
            RequestHost requestHost = new RequestHost();
            requestHost.MedNo = orgid;
            requestHost.MedName = "上海明德五官科医院";

            Patient patient = new Patient();
            patient.Name = cfmxlist[0].xm;
            patient.Gender = cfmxlist[0].xb;
            patient.Birthday = cfmxlist[0].csny;
            patient.IdentityID = cfmxlist[0].zjh;
            patient.WardNo = cfmxlist[0].WardNo;
            patient.BedNo = "";
            patient.CardID = cfmxlist[0].CardID;

            Request request = new Request();
            request.Reqno = cfmxlist[0].Reqno;
            request.Indate = DateTime.Parse(cfmxlist[0].Indate.ToDateTimeString());
            request.DoctorID = cfmxlist[0].DoctorID;
            request.DoctorName = cfmxlist[0].DoctorName;
            request.DoctorIdCard = "";
            request.RecordID = cfmxlist[0].RecordID;
            request.Source = "MagQ700002";
            request.Pic = "";
            request.PicDetail = cfmxlist[0].PicDetail;
            request.ApplyName = cfmxlist[0].ApplyName;
            request.WhichNo = null;
            request.IsVaild = typezt;
            request.IsCharge = "";
            request.ChargeDate = DateTime.Now;
            request.InvoiceID = "";
            request.CancelDate = null;
            request.Comment = "";
            request.ApplyDepartmentCode = cfmxlist[0].ApplyDepartmentCode;
            request.ApplyDepartmentName = cfmxlist[0].ApplyDepartmentName;
            request.DiagnoseName = "";

            List<Order> order = new List<Order>();
            foreach (var item in cfmxlist)
            {
                Order itemorder = new Order();
                itemorder.Seqno = item.Seqno;
                itemorder.Reqno = item.Reqno;
                itemorder.ItemID = item.ItemID;
                itemorder.ItemName = item.ItemName;
                itemorder.BodyPart = "眼部";
                itemorder.Qty = item.Qty;
                itemorder.Price = "";
                itemorder.SN = "";
                order.Add(itemorder);
            }
            request.Order = order;
            patient.Request = request;
            checkApplicationfromDTO.Patient = patient;
            checkApplicationfromDTO.RequestHost = requestHost;
            return checkApplicationfromDTO;
        }
        #endregion

        #region 医技执行
        public IList<JyjcExecVo> GetJyjcSqd(Pagination pagination,string orgId, DateTime kssj, DateTime jssj,string zxzt, string hzlx,string fylx,
            string sqdlx,string keyword=null)
        {
            StringBuilder sqlstr = new StringBuilder();
            StringBuilder mzsqlstr = new StringBuilder ();
            StringBuilder zysqlstr = new StringBuilder();
            var parm = new List<SqlParameter> { };
            mzsqlstr.Append(@" select CONVERT(varchar(50),NEWID()) Id, '门诊' hzlx,jzxx.mzh mzzyh,jzxx.xm hzxm,jzxx.zjh,a.cflx,
        case a.cflx when '4' then '检验' when '5' then '检查' end cflxstr,a.cfh sqdh,mb.mbmc sqdlx,a.CreateTime kdrq,
        a.ys shr,a.ys kdys,d.Name kdysmc,b.ztId,b.ztmc,'' gg,
		'项' dw, 1 sl,sum(CONVERT(decimal(12,2),b.dj * b.sl)) dj,
		sum(CONVERT(decimal(12,2),b.dj * b.sl)) je,a.cfh,e.Code kdks,e.Name kdksmc,d.Name jzr,c.Code zxks ,c.Name zxksmc,
        staff.Name zxr,zxzt.zxrq
from [Newtouch_CIS]..xt_cf a with(nolock)
left join [Newtouch_CIS]..xt_cfmx b with(nolock) on a.cfid=b.cfid and a.organizeid=b.organizeid 
join [Newtouch_CIS]..xt_jz jzxx  with(nolock) on jzxx.jzId=a.jzId and jzxx.OrganizeId=a.OrganizeId
left join jyjc_mb mb on mb.mbId=b.ztmbId and mb.OrganizeId=a.OrganizeId 
left join NewtouchHIS_Base.dbo.V_S_Sys_Department c on b.zxks=c.Code and b.organizeid=c.organizeid
left join [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] d on a.ys=d.gh and a.organizeid =d.organizeid
left join NewtouchHIS_Base.dbo.V_S_Sys_Department e on a.ks=e.Code and a.organizeid=e.organizeid
left join Newtouch_CIS..xt_jyjcexec zxzt with(nolock) on zxzt.sqdh=a.cfh and b.ztId=zxzt.ztId and zxzt.OrganizeId=a.OrganizeId and zxzt.zt=1
left join [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] staff on zxzt.zxr=staff.gh and zxzt.organizeid =staff.organizeid
where a.zt=1 and b.zt=1 and a.organizeid=@orgId
    and a.CreateTime>=@kssj and a.CreateTime<=@jssj ");
           
            zysqlstr.Append(@"SELECT  CONVERT(varchar(50),NEWID()) Id,
	  '住院' hzlx,a.zyh mzzyh,hzxm,zyxx.sfzh zjh,a.yzlx cflx,case a.yzlx when '6' then '检验' when '7' then '检查' end cflxstr,
        a.yzh sqdh,mb.mbmc sqdlx, kssj kdrq,a.shr,a.ysgh kdys,kdry.Name kdysmc,a.ztId,a.ztmc,'' gg,
	   '项' dw,1 sl, SUM(CONVERT(decimal(12,2),c.dj*a.sl)) dj,SUM(CONVERT(decimal(12,2),c.dj*a.sl)) je,yzh cfh
         ,jzks.code kdks,jzks.Name kdksmc,staff.Name jzr,zxks.Code zxks,zxks.Name zxksmc,staff1.Name zxr,zxzt.zxrq
	from [dbo].[zy_lsyz] a  with(nolock)
	LEFT JOIN zy_brxxk zyxx with(nolock) ON zyxx.zyh=a.zyh and zyxx.OrganizeId=a.OrganizeId and zyxx.zt=1
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm]c with(nolock) 
		ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt='1'
    LEFT JOIN jyjc_mb mb on mb.mbId=a.ztmbId and mb.OrganizeId=a.OrganizeId 
    LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department jzks on a.DeptCode=jzks.Code and a.organizeid=jzks.organizeid
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department zxks on a.zxksdm=zxks.Code and a.organizeid=zxks.organizeid
    LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] kdry on kdry.gh=a.ysgh and kdry.organizeid =a.organizeid
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] staff on a.ysgh=staff.gh and a.organizeid =staff.organizeid
    LEFT JOIN Newtouch_CIS..xt_jyjcexec zxzt with(nolock) on zxzt.sqdh=a.yzh and zxzt.ztId=a.ztId and zxzt.OrganizeId=a.OrganizeId and zxzt.zt=1
    LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] staff1 on staff1.gh=zxzt.zxr and staff1.OrganizeId=zxzt.OrganizeId	
    where  a.zt=1 AND  a.OrganizeId=@orgId AND a.yzzt =2  AND  isjf=0 
	AND a.kssj>=@kssj and a.kssj<=@jssj ");
            if (fylx != "qb")
            {
                mzsqlstr.Append(" and a.cflx=@cflx");
                parm.Add(new SqlParameter("@cflx", fylx == "jy" ? "4" : "5"));

                zysqlstr.AppendLine(" and a.yzlx=@fylx");
                parm.Add(new SqlParameter("@fylx", fylx == "jy" ? "6" : "7"));
            }
            else
            {
                mzsqlstr.Append(" and (a.cflx = 5 or a.cflx=4)");

                zysqlstr.AppendLine("  and a.yzlx in('6','7') ");
            }
            if (sqdlx != "qb")
            {
                mzsqlstr.Append(" and mb.mbId=@sqdlx");
                parm.Add(new SqlParameter("@sqdlx", sqdlx));

                zysqlstr.AppendLine(" and mb.mbId=@sqdlx1");
                parm.Add(new SqlParameter("@sqdlx1", sqdlx));
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                mzsqlstr.Append(@" and (jzxx.xm like @keyword or jzxx.mzh like @keyword or jzxx.zjh like @keyword
                                    or a.cfh like @keyword )");
                zysqlstr.AppendLine(@" and (zyxx.xm like @keyword or a.zyh like @keyword or zyxx.sfzh like @keyword
                                    or a.yzh like @keyword )");
            }
            if (zxzt == ((int)Enumzxzt.wzx).ToString())
            {
                mzsqlstr.Append(@" and zxzt.zxr is null ");
                zysqlstr.Append(@" and zxzt.zxr is null ");
            }
            else {
                mzsqlstr.Append(@" and zxzt.zt='1' ");
                zysqlstr.Append(@" and zxzt.zt='1' ");
            }
            mzsqlstr.Append(@" group by 
                        jzxx.mzh,jzxx.xm,jzxx.zjh,b.ztid,a.cflx,a.cfh,a.createtime,b.ztmc,c.Code,c.name,
                        d.name,d.gh,e.Code,e.Name,staff.Name,zxzt.zxrq,a.ys,mb.mbmc");

            zysqlstr.Append(@" GROUP BY
		                a.zyh,hzxm,zyxx.sfzh,a.yzlx,a.yzh, kssj,a.shr,a.ysgh,kdry.Name,a.ztId,a.ztmc,yzh,jzks.Code,jzks.Name,
                        staff.Name,zxks.Code,zxks.Name,staff1.Name,zxzt.zxrq,mbmc");
            if(!string.IsNullOrWhiteSpace(keyword))
                parm.Add(new SqlParameter("@keyword", '%' + keyword.Trim() + '%'));
            parm.Add(new SqlParameter("@orgId", orgId));
            parm.Add(new SqlParameter("@kssj", kssj));
            parm.Add(new SqlParameter("@jssj", jssj));

            if (hzlx == "mz")
                sqlstr = mzsqlstr;
            else if (hzlx == "zy")
                sqlstr = zysqlstr;
            else
                sqlstr = mzsqlstr.Append(" union all ").Append(zysqlstr);
            return QueryWithPage<JyjcExecVo>(sqlstr.ToString(), pagination, parm.ToArray());
        }


        public IList<JyjcExecVo> GetJyjcSqdRecord(Pagination pagination, string orgId, DateTime kssj, DateTime jssj, string hzlx, string fylx,
           string sqdlx, string keyword = null)
        {
            StringBuilder sqlstr = new StringBuilder();
            var parm = new List<SqlParameter> { };
            sqlstr.Append(@" select zxjl.Id,mzzyh,zxjl.xm hzxm,hzlx,case hzlx when 1 then xtjz.zjh when 2 then zyxx.sfzh  end zjh
	 ,fylx,sqdh,sqdlx,ztmc,gg, dw
	 ,sl,je,jzr.Name jzr,kdrq ,klks.Name kdksmc,kdry.Name kdysmc ,zxry.Name zxr,zxrq,zxks.Name zxksmc
from xt_jyjcexec zxjl with(nolock)
left join xt_jz xtjz with(nolock) on xtjz.mzh=zxjl.mzzyh and xtjz.OrganizeId=zxjl.OrganizeId and zxjl.hzlx=1 and xtjz.zt=1
left join zy_brxxk zyxx with(nolock) on zyxx.zyh=zxjl.mzzyh and zxjl.OrganizeId=zyxx.OrganizeId and zxjl.hzlx=2  and zyxx.zt=1
left join NewtouchHIS_Base.dbo.V_S_Sys_Department klks on klks.Code=zxjl.kdks and klks.organizeid=zxjl.organizeid
left join NewtouchHIS_Base.dbo.V_S_Sys_Department zxks on zxks.Code=zxjl.zxks and zxks.organizeid=zxjl.organizeid
left join [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] jzr on jzr.gh=zxjl.shr and jzr.organizeid =zxjl.organizeid
left join [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] zxry on zxry.gh=zxjl.zxr and zxry.organizeid =zxjl.organizeid
left join [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] kdry on kdry.gh=zxjl.kdys and kdry.organizeid =zxjl.organizeid
where zxjl.zt=1  and zxjl.OrganizeId=@orgId and zxjl.zxrq>=@kssj and zxjl.zxrq<=@jssj
 ");
            if (hzlx!="qb")
            {
                sqlstr.Append(" and zxjl.hzlx=@hzlx");
                parm.Add(new SqlParameter("@hzlx", hzlx == "mz" ? "1" : "2"));
            }
            if (sqdlx != "qb")
            {
                sqlstr.Append(" and zxjl.sqdlx=@sqdlx");
                parm.Add(new SqlParameter("@sqdlx", sqdlx));
            }
            if (fylx != "qb")
            {
                sqlstr.Append(" and (zxjl.fylx=@mzcflx or zxjl.fylx =@zycflx)");
                parm.Add(new SqlParameter("@mzcflx", fylx == "jy" ? "4" : "5"));
                parm.Add(new SqlParameter("@zycflx", fylx == "jy" ? "6" : "7"));
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sqlstr.Append(@" and (zxjl.xm like @keyword or zxjl.mzzyh like @keyword or xtjz.zjh like @keyword
                                 or zyxx.sfzh like @keyword or zxjl.sqdh like @keyword )");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
                parm.Add(new SqlParameter("@keyword", '%' + keyword.Trim() + '%'));
            parm.Add(new SqlParameter("@orgId", orgId));
            parm.Add(new SqlParameter("@kssj", kssj));
            parm.Add(new SqlParameter("@jssj", jssj));

            return QueryWithPage<JyjcExecVo>(sqlstr.ToString(), pagination, parm.ToArray());
        }
        #endregion

    }
}
