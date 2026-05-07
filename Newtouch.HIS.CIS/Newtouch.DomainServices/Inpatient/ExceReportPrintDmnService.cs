using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Newtouch.DomainServices.Inpatient
{
    public class ExceReportPrintDmnService : DmnServiceBase, IExceReportPrintDmnService
    {
        private readonly ISysConfigRepo _sysConfigRepo;
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="databaseFactory">EF上下文工厂</param>
        public ExceReportPrintDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {

        }

        /// <summary>
        /// 获取病区发药患者树
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatTree(string orgId,string zyzt,string keyword)
        {
            string sql = @"select distinct b.zyh,b.xm hzxm,b.wardCode bqCode,c.bqmc,b.sex,b.ryrq, b.birth,cw.BedNo, 
CONVERT(VARCHAR(25),CASE DATEDIFF(DAY, b.ryrq,GETDATE()) WHEN 0 THEN 1 else  DATEDIFF(DAY, b.ryrq,GETDATE())END ) inHosDays,
CAST(FLOOR(DATEDIFF(DY, b.birth, GETDATE()) / 365.25) AS VARCHAR(5)) nl,b.brxzmc
from(select distinct zyh from zy_cqyz with(nolock)  where yzzt='2' and OrganizeId=@orgId  
                union all
                select distinct zyh from zy_lsyz with(nolock)  where yzzt='2' and OrganizeId=@orgId ) a 
                inner join zy_brxxk b with(nolock)  on a.zyh=b.zyh and b.OrganizeId=@orgId and zt='1'
                left join [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) on c.bqcode=b.wardCode and c.OrganizeId=@orgId
                left join zy_cwsyjlk cw with(nolock) on cw.zyh=b.zyh and cw.OrganizeId=@orgId and cw.zt='1'
                where 1=1";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(zyzt))
            {
                switch (zyzt) {
                    case "zy"://在院：不是已出院，不是作废记录，不是取消入院
                        sql += " and b.zybz not in (SELECT * FROM dbo.f_split(@zybz, ','))";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Ycy + "," + (int)EnumZYBZ.Wry+","+(int)EnumZYBZ.Djz));
                        break;
                    case "cy"://出院：已出院状态
                        sql += " and b.zybz in (SELECT * FROM dbo.f_split(@zybz, ','))";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Ycy + "," + (int)EnumZYBZ.Djz));
                        break;
                }
            }
            else {
                sql += " and b.zybz<>'9'";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (b.zyh like @keyword or b.xm like @keyword)";
                par.Add(new SqlParameter("@keyword", "%"+keyword+"%"));
            }
            return this.FindList<InpWardPatTreeVO>(sql, par.ToArray());
        }

        public IList<ExecReportReportRightVO> GetExecDetailGridJson(string orgId,string zyh,DateTime zxsj,string zxdlb,string yzxz) {
            string sqlzxrq = ""; 
            if (!(!string.IsNullOrWhiteSpace(zxdlb) && zxdlb == "yzd" && yzxz == "2"))  //长期医嘱全部打印
            {
                sqlzxrq = " and CONVERT(date, zxrq)=convert(date,@zxsj) ";
            }
            string sql = @" select  yzxz,clbz, yzId,tb1.yzh,zh, yznr,zxsj,shr,yzlx,ypyfdm,lyxh,shz,zxz,convert(varchar(10),isjf)isjf  ,zyh
            from(
            select tb.*,case when ztmc='' then '1' else num  end num1 from (
            select a.*,aaa.lyxh,b.Name shz,c.Name zxz ,Row_Number() OVER (partition by yzh,ztmc ORDER BY zxsj desc) num from 
            (
            select yzxh,CONVERT(INT,lyxh) lyxh,CreatorCode from zy_tyssqqk with(nolock) where OrganizeId=@orgId " + sqlzxrq + @" and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
            union all
            select yzxh,CONVERT(INT,lyxh) lyxh,CreatorCode from zy_fyqqk with(nolock) where OrganizeId=@orgId " + sqlzxrq + @" and zyh in(select col from dbo.f_split(@zyh,',') where col>'') 
            union
            select yzxh,null lyxh,CreatorCode from zy_fymxk with(nolock) where OrganizeId=@orgId " + sqlzxrq + @" and zyh in(select col from dbo.f_split(@zyh,',') where col>'') and isjf=0
            )
            aaa  join 
            (
            select '长' clbz,isjf,'2' yzxz,Id yzId,yzh,zh,case when ztid is not null then ztmc else yznr end yznr,zxsj,shr,yzlx,ypyfdm,xmdm,isnull(ztmc,'') ztmc ,yfztbs ,zyh
            from zy_cqyz with(nolock)
            where zt=1 and OrganizeId=@orgId and zyh in(select col from dbo.f_split(@zyh,',') where col>'') and (yfztbs is null or yzlx<>@yzlxs)
            union all
            select '临' clbz,isjf,'1' yzxz,Id yzId,yzh,zh,case when ztid is not null then ztmc else yznr end yznr,zxsj,shr,yzlx,ypyfdm,xmdm ,isnull(ztmc,'') ztmc,yfztbs,zyh
            from zy_lsyz with(nolock)
            where  zt=1 and OrganizeId=@orgId and zyh in(select col from dbo.f_split(@zyh,',') where col>'') and (yfztbs is null or yzlx<>@yzlxs)
            ) a on aaa.yzxh=a.yzId
			left join NewtouchHIS_Base..V_S_Sys_Staff b on b.gh=a.shr and b.OrganizeId=@orgId
            left join NewtouchHIS_Base..V_S_Sys_Staff c on c.gh=aaa.CreatorCode and c.OrganizeId=@orgId 
            where 1=1 ";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@zyh", zyh));
            par.Add(new SqlParameter("@zxsj", zxsj));
            par.Add(new SqlParameter("@yzlxs", (int)EnumYzlx.sfxm));
            var ControlzsyfCode = _sysConfigRepo.GetValueByCode("zsyfpz", orgId);
            var ControlwhyfCode = _sysConfigRepo.GetValueByCode("whyfpz", orgId);
            string zxlbCode = _sysConfigRepo.GetValueByCode("zxdlb_" + zxdlb, orgId);
            var ControlypbjyfCode = _sysConfigRepo.GetValueByCode("ypbjyfpz", orgId);
            if (!string.IsNullOrWhiteSpace(yzxz) &&yzxz!="qb")
            {
                sql += " and yzxz=@yzxz ";
                par.Add(new SqlParameter("@yzxz",yzxz));
            }
            if (!string.IsNullOrWhiteSpace(zxdlb))
            {
                switch (zxdlb)
                {
                    case "zld":
                        sql += " and a.yzlx in (SELECT * FROM dbo.f_split(@yzlx, ','))";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.Yp+","+(int)EnumYzlx.Cydy + "," + (int)EnumYzlx.zcy));
                        break;
                    case "zsz":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlzsyfCode));
                        break;
                    case "whd":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlwhyfCode));
                        break;
                    case "whbq":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlwhyfCode));
                        break;
                    case "jyd":
                        sql += " and a.yzlx = @yzlx";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.jy));
                        break;
                    case "jcd":
                        sql += " and a.yzlx = @yzlx";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.jc));
                        break;
					case "kfyd_yf":
                    case "zsd_yf":
                    case "syd_yf":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", zxlbCode));
                        break;
                    case "zld_yzlx":
                    case "zcy_yzlx":
                        sql += " and a.yzlx in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", zxlbCode));
                        break;
                    case "hld_sfdl":
                        sql += @" and exists(select 1 from NewtouchHIS_Base.dbo.V_S_xt_sfxm sfxm with(nolock) 
where a.xmdm =sfxm.sfxmCode and sfxm.OrganizeId=@orgId and sfxm.zt='1' 
and  sfxm.sfdlcode in (SELECT * FROM dbo.f_split(@printval, ',')))";
                        par.Add(new SqlParameter("@printval", zxlbCode));
                        break;
                    case "jyztdy":
                        sql += " and a.yzlx = @yzlx";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.jy));
                        break;
                    case "ypbj":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlypbjyfCode));
                        break;
                }
            }
            sql += @" )tb ) tb1

where num1=1 ";
           
            return this.FindList<ExecReportReportRightVO>(sql,par.ToArray());
        }
        /// <summary>
        /// 执行单打印查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <param name="zxsj"></param>
        /// <param name="zxsjend"></param>
        /// <param name="zxdlb"></param>
        /// <param name="yzxz"></param>
        /// <returns></returns>

        public IList<ExecReportReportRightVO> QueryExecDetailGridJson(Pagination pagination, string orgId, string zyh, DateTime zxsj, DateTime zxsjend, string zxdlb,string yzxz)
        {
            string sql = @" select zyh,hzxm,yzxz, clbz,min(yzId) yzId,tb1.yzh,zh,tb1.yznr
            ,zxrq zxsj,shr,yzlx,ypyfdm,lyxh,shz,zxz,convert(varchar(10),isjf)isjf,sum(dj) dj,sum(case when num1>1 then 0 else sl end) sl,sum(je) je,px  from(
            select tb.*,case when ztmc='' then '1' else num end num1 from 
            (
            select a.*,aaa.dj,case when yzlx='10' then aaa.sl*mcsl else aaa.sl end sl,case when yzlx='10' then CONVERT(decimal(18,2),dj*sl*mcsl) else aaa.je end je,aaa.lyxh,b.Name shz,c.Name zxz ,Row_Number() OVER (partition by yzh,ztmc ORDER BY zxrq desc) num,zxrq from 
            (select yzxh,dj,sl,mcsl,CONVERT(decimal(18,2),dj*sl) je,CONVERT(INT,lyxh) lyxh,CreatorCode,zxrq from zy_tyssqqk with(nolock) where OrganizeId=@orgId and zxrq>=@zxsj and zxrq<=@zxsjend and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
            union all
            select yzxh,ypdj,ypsl,mcsl,CONVERT(decimal(18,2),ypdj*ypsl) je,CONVERT(INT,lyxh) lyxh,CreatorCode,zxrq from zy_fyqqk with(nolock) where OrganizeId=@orgId and zxrq>=@zxsj and zxrq<=@zxsjend and zyh in(select col from dbo.f_split(@zyh,',') where col>'')
            union
            select yzxh,dj,sl,1.00 mcsl,CONVERT(decimal(18,2),dj*sl) je,null lyxh,CreatorCode,zxrq from zy_fymxk with(nolock) where OrganizeId=@orgId and zxrq>=@zxsj and zxrq<=@zxsjend and zyh in(select col from dbo.f_split(@zyh,',') where col>'') and isjf=0
            )
            aaa  join 
            (select zyh,isjf,px,'长' clbz,'2' yzxz,hzxm,Id yzId,yzh,zh,case when ztid is not null then ztmc else yznr end yznr,shr,yzlx,ypyfdm,xmdm,isnull(ztmc,'') ztmc,yfztbs 
            from zy_cqyz with(nolock) 
            where  zt=1 and OrganizeId=@orgId and zyh in(select col from dbo.f_split(@zyh,',') where col>'') and (yfztbs is null or yzlx<>@yzlxs)
            union all
            select zyh,isjf,case when ztId is not null then '1' else px end px,'临' clbz,'1' yzxz,hzxm,Id yzId,yzh,zh,case when ztid is not null then ztmc else yznr end yznr,shr,yzlx,ypyfdm,xmdm,isnull(ztmc,'') ztmc,yfztbs 
            from zy_lsyz with(nolock) 
            where zt=1 and OrganizeId=@orgId and zyh in(select col from dbo.f_split(@zyh,',') where col>'')  and (yfztbs is null or yzlx<>@yzlxs)
            ) a on aaa.yzxh=a.yzId
		    left join NewtouchHIS_Base..V_S_Sys_Staff b on b.gh=a.shr and b.OrganizeId=@orgId
            left join NewtouchHIS_Base..V_S_Sys_Staff c on c.gh=aaa.CreatorCode and c.OrganizeId=@orgId where 1=1 ";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@zyh", zyh));
            par.Add(new SqlParameter("@zxsj", zxsj));
            par.Add(new SqlParameter("@zxsjend", zxsjend));
            par.Add(new SqlParameter("@yzlxs",(int)EnumYzlx.sfxm));
            var ControlzsyfCode = _sysConfigRepo.GetValueByCode("zsyfpz", orgId);
            var ControlwhyfCode = _sysConfigRepo.GetValueByCode("whyfpz", orgId);
            var ControlypbjyfCode = _sysConfigRepo.GetValueByCode("ypbjyfpz", orgId);
            string zxlbCode = _sysConfigRepo.GetValueByCode("zxdlb_" + zxdlb, orgId);

            if (!string.IsNullOrWhiteSpace(yzxz) && yzxz != "qb")
            {
                sql += " and yzxz=@yzxz ";
                par.Add(new SqlParameter("@yzxz", yzxz));
            }
            if (!string.IsNullOrWhiteSpace(zxdlb))
            {
                switch (zxdlb)
                {
                    case "zld":
                        sql += " and a.yzlx in (SELECT * FROM dbo.f_split(@yzlx, ','))";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.Yp + "," + (int)EnumYzlx.Cydy + "," + (int)EnumYzlx.zcy));
                        break;
                    case "zsz":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlzsyfCode));
                        break;
                    case "whd":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlwhyfCode));
                        break;
                    case "whbq":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlwhyfCode));
                        break;
                    case "jyd":
                    case "jyztdy":
                        sql += " and a.yzlx = @yzlx";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.jy));
                        break;
                    case "jcd":
                        sql += " and a.yzlx = @yzlx";
                        par.Add(new SqlParameter("@yzlx", (int)EnumYzlx.jc));
                        break;
                    case "kfyd_yf":
                    case "zsd_yf":
                    case "syd_yf":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", zxlbCode));
                        break;
                    case "zld_yzlx":
                    case "zcy_yzlx":
                        sql += " and a.yzlx in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", zxlbCode));
                        break;
                    case "hld_sfdl":
                        sql += @" and exists(select 1 from NewtouchHIS_Base.dbo.V_S_xt_sfxm sfxm with(nolock) 
where a.xmdm =sfxm.sfxmCode and sfxm.OrganizeId=@orgId and sfxm.zt='1' 
and  sfxm.sfdlcode in (SELECT * FROM dbo.f_split(@printval, ',')))";
                        par.Add(new SqlParameter("@printval", zxlbCode));
                        break;
                    case "ypbj":
                        sql += " and a.ypyfdm in (SELECT * FROM dbo.f_split(@printval, ','))";
                        par.Add(new SqlParameter("@printval", ControlypbjyfCode));
                        break;
                }
            }
            sql += @" )tb ) tb1
group by zyh,hzxm,yzxz,clbz,yzh,zh,yznr,zxrq,shr,yzlx,ypyfdm,lyxh,shz,zxz,isjf,px
--where num1=1 
";
            
            return QueryWithPage<ExecReportReportRightVO>(sql, pagination, par.ToArray());
        }

        /// <summary>
        /// 患者中心 left tree
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyzt"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatCenterTree(string orgId, string zyzt, string keyword)
        {
            string sql = @"select  distinct ciszyxx.zyh,ciszyxx.xm hzxm,ciszyxx.wardCode bqCode,bq.bqmc,ciszyxx.sex,ciszyxx.ryrq, ciszyxx.birth,cw.cwmc BedNo, 
CONVERT(VARCHAR(25),CASE DATEDIFF(DAY, ciszyxx.ryrq,GETDATE()) WHEN 0 THEN 1 else  DATEDIFF(DAY, ciszyxx.ryrq,GETDATE())END ) inHosDays,
CAST(FLOOR(DATEDIFF(DY, ciszyxx.birth, GETDATE()) / 365.25) AS VARCHAR(5)) nl,ciszyxx.brxzmc,
convert(varchar(50),isnull(ciszyxx.rqrq,getdate()),120) rqrq,convert(varchar(50),isnull(ciszyxx.cqrq,getdate()),120) cqrq
from zy_brxxk ciszyxx with(nolock)
left join [NewtouchHIS_Base].[dbo].[V_S_xt_bq] bq with(nolock) on bq.bqcode=ciszyxx.wardCode and bq.OrganizeId=ciszyxx.OrganizeId
left join [NewtouchHIS_Base]..xt_cw cw with(nolock) on cw.cwCode=ciszyxx.BedCode and cw.OrganizeId=ciszyxx.OrganizeId and cw.zt='1'
join NewtouchHIS_Sett..zy_brjbxx  zyxx with(nolock)  on zyxx.zyh=ciszyxx.zyh and zyxx.OrganizeId=ciszyxx.OrganizeId and zyxx.zt='1'
where ciszyxx.OrganizeId=@orgId ";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(zyzt))
            {
                switch (zyzt)
                {
                    case "zy"://在院：不是已出院，不是作废记录，不是取消入院
                        sql += " and zyxx.zybz not in (SELECT * FROM dbo.f_split(@zybz, ','))";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Ycy + "," + (int)EnumZYBZ.Wry+","+ (int)EnumZYBZ.Djz));
                        break;
                    case "cq"://出区：出区未结算
                        sql += " and zyxx.zybz = @zybz";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Djz));
                        break;
                    case "cy"://出院：已出院状态
                        sql += " and zyxx.zybz = @zybz";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Ycy));
                        break;
                }
            }
            else
            {
                sql += " and zyxx.zybz<>'9'";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (zyxx.zyh like @keyword or zyxx.xm like @keyword)";
                par.Add(new SqlParameter("@keyword", keyword + "%"));
            }
            return this.FindList<InpWardPatTreeVO>(sql, par.ToArray());
        }
    }
}
