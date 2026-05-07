using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Domain.IDomainServices.Outpatient;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Domain.ValueObjects.Outpatient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.DomainServices.Outpatient
{
    public class OutpatientConsultDmnService : DmnServiceBase, IOutpatientConsultDmnService
    {
        public OutpatientConsultDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }

        public IList<OutpatientConsultVO> GetExpertInfo(Pagination pagination, string ksCode, string keyword, string ghrq, string orgId)
        {
            string sql = @" select 
gh.ghnm,jzxh,xm,ghrq,ks ksCode,dept.Name ksmc,gh.ys,staff.Name ysmc,zs.zsCode,zs.zsmc 
from [NewtouchHIS_Sett].dbo.mz_gh gh 
left join [NewtouchHIS_Base].[dbo].[Sys_Staff] staff on gh.ys=staff.gh and gh.OrganizeId=staff.OrganizeId and gh.zt=staff.zt
left join [NewtouchHIS_Base].dbo.sys_department dept on gh.ks=dept. Code and  gh.OrganizeId=dept.OrganizeId and gh.zt=dept.zt
--left join [NewtouchHIS_Base].dbo.Sys_StaffConsult  sc on staff.Id=sc.staffId and staff.OrganizeId=sc.OrganizeId and staff.zt=sc.zt
left join [NewtouchHIS_Base].dbo.xt_zs  zs on staff.gh=zs.ys and staff.OrganizeId=zs.OrganizeId and staff.zt=zs.zt
where 
gh.zt=1 and gh.organizeId=@orgId and gh.ghzt<>'2'
and jzbz=1  --待就诊
and mjzbz=3  --专家门诊
and ghrq >=@ghrq+' 00:00:00' and ghrq<=@ghrq+' 23:59:59'
--and ghrq >='2023-01-01 00:00:00' and ghrq<='2023-06-15 23:59:59'

";
            if (!string.IsNullOrWhiteSpace(ksCode))
            {
                sql += " and ks=@ksCode ";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (@keyword='%%' or xm like @keyword or zs.zsCode like @keyword  or zs.zsmc like @keyword)";
            }

            return this.QueryWithPage<OutpatientConsultVO>(sql, pagination, new[] {
                new SqlParameter("@ksCode", ksCode),
                new SqlParameter("@keyword", "%"+keyword.Trim()+"%"),
                new SqlParameter("@ghrq", ghrq),
                new SqlParameter("@orgId", orgId)
            }, false);
        }
        public IList<OutpatientConsultVO> GetNormalInfo(Pagination pagination, string ksCode, string keyword, string ghrq, string orgId)
        {
            string sql = @" select 
isnull(ghzs.id,0) ghzsId,gh.ghnm,jzxh,xm,ghrq,ks ksCode,dept.Name ksmc,gh.ys,staff.Name ysmc,zs.zsCode,zs.zsmc,ghzs.calledstu
from [NewtouchHIS_Sett].dbo.mz_gh gh 
left join [NewtouchHIS_Base].[dbo].[Sys_Staff] staff on gh.ys=staff.gh and gh.OrganizeId=staff.OrganizeId and gh.zt=staff.zt
left join [NewtouchHIS_Base].dbo.sys_department dept on gh.ks=dept. Code and  gh.OrganizeId=dept.OrganizeId and gh.zt=dept.zt
--left join [NewtouchHIS_Base].dbo.Sys_StaffConsult  sc on staff.Id=sc.staffId and staff.OrganizeId=sc.OrganizeId and staff.zt=sc.zt
--left join [NewtouchHIS_Base].dbo.xt_zs  zs on staff.gh=zs.ys and staff.OrganizeId=zs.OrganizeId and staff.zt=zs.zt
left join mz_ghzs ghzs on gh.ghnm=ghzs.ghnm and gh.OrganizeId=ghzs.OrganizeId and gh.zt=ghzs.zt
left join [NewtouchHIS_Base].dbo.xt_zs  zs on ghzs.zsCode=zs.zsCode and ghzs.OrganizeId=zs.OrganizeId and ghzs.zt=zs.zt
where 
gh.zt=1 and gh.organizeId=@orgId and gh.ghzt<>'2'
and jzbz=1  --待就诊
and mjzbz=1  --专家门诊
and ghrq >=@ghrq+' 00:00:00' and ghrq<=@ghrq+' 23:59:59'
--and ghrq >='2023-01-01 00:00:00' and ghrq<='2023-06-15 23:59:59'

";
            if (!string.IsNullOrWhiteSpace(ksCode))
            {
                sql += " and ks=@ksCode ";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (@keyword='%%' or xm like @keyword or zs.zsCode like @keyword  or zs.zsmc like @keyword)";
            }

            return this.QueryWithPage<OutpatientConsultVO>(sql, pagination, new[] {
                new SqlParameter("@ksCode", ksCode),
                new SqlParameter("@keyword", "%"+keyword.Trim()+"%"),
                new SqlParameter("@ghrq", ghrq),
                new SqlParameter("@orgId", orgId)
            }, false);
        }

        /// <summary>
        /// 获取诊室列表
        /// </summary>
        /// <param name="ksCode"></param>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<RehabVO> SelectConsultList(string ksCode, string keyword, string orgId)
        {
            string sql = @"
select py,zsCode code,zsmc Name from [NewtouchHIS_Base].dbo.xt_zs  where zt=1 and OrganizeId=@orgId and xh<>0";

            if (!string.IsNullOrWhiteSpace(ksCode))
            {
                sql += " and ksCode=@ksCode ";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (@keyword='%%' or py like @keyword or zsCode like @keyword  or zsmc like @keyword)";
            }

            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@ksCode", ksCode));
            par.Add(new SqlParameter("@keyword", keyword));
            return this.FindList<RehabVO>(sql.ToString(), par.ToArray());
        }

        /// <summary>
        /// 根据科室获取各诊室待就诊患者数量
        /// </summary>
        /// <param name="ksCode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<OutpatientConsultCountVO> getConsultCount(string ksCode, string orgId,string ghrq)
        {
            string sql = @"
select dept.name ksmc, zsmc,count(*) num
 from [NewtouchHIS_Sett].dbo.mz_gh gh 
 left join [NewtouchHIS_Base].dbo.sys_department dept on gh.ks=dept. Code and  gh.OrganizeId=dept.OrganizeId and gh.zt=dept.zt
 left join mz_ghzs ghzs on gh.ghnm=ghzs.ghnm and gh.OrganizeId=ghzs.OrganizeId and gh.zt=ghzs.zt
left join [NewtouchHIS_Base].dbo.xt_zs  zs on ghzs.zsCode=zs.zsCode and ghzs.OrganizeId=zs.OrganizeId and ghzs.zt=zs.zt
 where 
gh.zt=1 and gh.organizeId=@orgId
and jzbz=1  --待就诊
 and mjzbz=1
and ghrq >=@ghrq+' 00:00:00' and ghrq<=@ghrq+' 23:59:59'
--and ghrq >='2023-01-01 00:00:00' and ghrq<='2023-06-15 23:59:59'
 and zsmc is not null
";

            if (!string.IsNullOrWhiteSpace(ksCode))
            {
                sql += " and ksCode=@ksCode ";
            }
            sql += "group by dept.name,zsmc";
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@ksCode", ksCode));
            par.Add(new SqlParameter("@ghrq", ghrq));
            return this.FindList<OutpatientConsultCountVO>(sql.ToString(), par.ToArray());
        }

        /// <summary>
        /// 获取分诊叫号列表
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="ksCode"></param>
        /// <param name="keyword"></param>
        /// <param name="ghrq"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<OutpatientConsultVO> GetConsultCall(string ksCode, string keyword, string ghrq, string orgId)
        {
//            string sql = @" select top 6
//ghzs.Id ghzsId,gh.ghnm,jzxh,xm,ghrq,ks ksCode,dept.Name ksmc,gh.ys,staff.Name ysmc,zs.zsCode,zs.zsmc 
//from [NewtouchHIS_Sett].dbo.mz_gh gh 
//left join [NewtouchHIS_Base].[dbo].[Sys_Staff] staff on gh.ys=staff.gh and gh.OrganizeId=staff.OrganizeId and gh.zt=staff.zt
//left join [NewtouchHIS_Base].dbo.sys_department dept on gh.ks=dept. Code and  gh.OrganizeId=dept.OrganizeId and gh.zt=dept.zt
//--left join [NewtouchHIS_Base].dbo.Sys_StaffConsult  sc on staff.Id=sc.staffId and staff.OrganizeId=sc.OrganizeId and staff.zt=sc.zt
//--left join [NewtouchHIS_Base].dbo.xt_zs  zs on staff.gh=zs.ys and staff.OrganizeId=zs.OrganizeId and staff.zt=zs.zt
//left join mz_ghzs ghzs on gh.ghnm=ghzs.ghnm and gh.OrganizeId=ghzs.OrganizeId and gh.zt=ghzs.zt and ghzs.calledstu in(1,5)
//left join [NewtouchHIS_Base].dbo.xt_zs  zs on ghzs.zsCode=zs.zsCode and ghzs.OrganizeId=zs.OrganizeId and ghzs.zt=zs.zt
//where 
//gh.zt=1 and gh.organizeId=@orgId
//and jzbz=1  --待就诊
//and mjzbz=1  --专家门诊
//and ghrq >=@ghrq+' 00:00:00' and ghrq<=@ghrq+' 23:59:59'
//--and ghrq >='2023-01-01 00:00:00' and ghrq<='2023-06-15 23:59:59'
//and zs.zsCode is not null
//";
//            if (!string.IsNullOrWhiteSpace(ksCode))
//            {
//                sql += " and ks=@ksCode ";
//            }
//            if (!string.IsNullOrWhiteSpace(keyword))
//            {
//                sql += " and (@keyword='%%' or xm like @keyword or zs.zsCode like @keyword  or zs.zsmc like @keyword)";
//            }
//            sql += " order by zs.zsCode asc,ghzs.calledstu desc,jzxh asc ";
//            var par = new List<SqlParameter>();
//            par.Add(new SqlParameter("@ksCode", ksCode));
//            par.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
//            par.Add(new SqlParameter("@ghrq", ghrq));
//            par.Add(new SqlParameter("@orgId", orgId));
//            return this.FindList<OutpatientConsultVO>(sql.ToString(), par.ToArray());


            var inParameters = new List<SqlParameter>();
            inParameters.Add(new SqlParameter("@ksCode", ksCode));
            inParameters.Add(new SqlParameter("@keyword", keyword.Trim()));
            inParameters.Add(new SqlParameter("@ghrq", ghrq));
            inParameters.Add(new SqlParameter("@orgId", orgId));
            return this.FindList<OutpatientConsultVO>("exec usp_mz_GetConsultCall @ksCode,@ghrq,@orgId,@keyword", inParameters.ToArray());

        }


        #region 分诊科室

        /// <summary>
        /// 根据查询条件获取有效科室列表
        /// </summary>
        /// <param name="code"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<RehabVO> GetDeptListByKeyValue(Pagination pagination, string orgId, string keyValue)
        {
            var sql = "select Code,Name,py from NewtouchHIS_Base.dbo.Sys_Department  where zt=1 and OrganizeId = @orgId and zlks='1' ";
            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                sql += " and (@keyValue='%%' or Name like @keyValue or Code like @keyValue)";
            }
            return this.QueryWithPage<RehabVO>(sql, pagination, new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@keyValue", "%"+keyValue.Trim()+"%")});
        }

        /// <summary>
        /// 获取科室下的诊室及医生
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public IList<OutpatientConsultDoctorVO> GetConsultDoctorByDept(string orgId, string ksCode)
        {
            //var sql = " select zs.zsCode,zs.zsmc,zsys.rq,zsys.gh,staff.name ysxm,zslc,zsfh from [NewtouchHIS_Base].dbo.xt_zs zs " +
            //    "left join mz_zsys zsys on zs.zsCode = zsys.zsCode and zs.organizeId = zsys.organizeID and zs.zt = zsys.zt and zsys.rq between dateadd(day,-1,GETDATE()) and  GETDATE()" +
            //    "left join[NewtouchHIS_Base].[dbo].[Sys_Staff] staff on staff.gh=zsys.gh and staff.OrganizeId=zsys.OrganizeId and staff.zt=zsys.zt " +
            //    "where zs.zt=1 and zs.OrganizeId = @orgId ";

			var sqlstr = @" select zs.zsCode,zs.zsmc,GETDATE() rq,zs.ys gh,zs.ysmc ysxm,zslc,zsfh from [NewtouchHIS_Base].dbo.xt_zs zs 
where zs.zt=1 and zs.OrganizeId = @orgId ";

            if (!string.IsNullOrWhiteSpace(ksCode))
            {
                //sql += " and ksCode=@ksCode ";
				sqlstr += " and ksCode=@ksCode ";
			}
            return this.FindList<OutpatientConsultDoctorVO>(sqlstr, new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@ksCode", ksCode)});
        }

        #endregion

        #region 诊室信息（小屏）
        public OutpatientConsultInfoVO GetConsultInfo(string orgId, string zsCode,string ghrq) {

            var inParameters = new List<SqlParameter>();
            inParameters.Add(new SqlParameter("@orgId", orgId));
            inParameters.Add(new SqlParameter("@zsCode", zsCode));
            inParameters.Add(new SqlParameter("@ghrq", ghrq));
            return this.FirstOrDefault<OutpatientConsultInfoVO>("exec usp_mz_GetConsultInfo @orgId,@zsCode,@ghrq", inParameters.ToArray());

            }
 //       public OutpatientConsultInfoVO GetConsultInfo(string orgId, string zsCode)
 //       {
 //           var sql = " select top 1 dept.name ksmc,zs.zsmc,staff.name ysxm" +
 //               ",staff.[Description] ysjs," +
 //"jzz.xm jzz, jzz.jzxh jzzxh," +
 //"djz.xm djz, djz.jzxh djzxh" +
 //" from mz_zsys zsys" +
 //" left join[NewtouchHIS_Base].dbo.xt_zs zs on zsys.zsCode=zs.zsCode" +
 ////" left join[Newtouch_CIS].dbo.mz_ghzs ghzs on ghzs.zsCode= zsys.zsCode and rq between dateadd(day,-1, GETDATE()) and GETDATE()" +
 //" left join [NewtouchHIS_Base].[dbo].[Sys_Staff] staff on staff.gh = zsys.gh and staff.OrganizeId= zsys.OrganizeId and staff.zt= zsys.zt" +
 //" left join [NewtouchHIS_Base].dbo.Sys_Department dept on zs.ksCode= dept.Code" +

 //" left join" +
 //           " (select top 1 xm, jzxh from mz_ghzs ghzs" +
 //           " left join [NewtouchHIS_Sett].dbo.mz_gh gh on gh.ghnm = ghzs.ghnm  and jzbz = 2 and mjzbz = 1" +
 //           " where ghzs.zsCode= @zsCode " +
 //           //" and gh.ghrq between dateadd(day,-1, GETDATE()) and GETDATE()" +
 //           " order by jzxh desc) djz on 1=1" +

 //" left join" +
 //           " (select top 1 xm, jzxh from mz_ghzs ghzs" +
 //           " left join [NewtouchHIS_Sett].dbo.mz_gh gh on gh.ghnm = ghzs.ghnm  and jzbz = 1 and mjzbz = 1" +
 //           " where ghzs.zsCode=@zsCode " +
 //           //" and gh.ghrq between dateadd(day,-1, GETDATE()) and GETDATE()" +
 //           " order by jzxh desc) jzz on 1=1" +
 // " where zsys.organizeId=@orgId and zsys.zt= 1" +
 // " and rq between dateadd(day,-1, GETDATE()) and GETDATE()";

 //           if (!string.IsNullOrWhiteSpace(zsCode))
 //           {
 //               sql += " and zsys.zsCode=@zsCode ";
 //           }

 //           return FirstOrDefault<OutpatientConsultInfoVO>(sql, new[] {
 //               new SqlParameter("@orgId", orgId),
 //               new SqlParameter("@zsCode", zsCode)
 //           });
 //       }
        #endregion


		public OutpatientConsultVO GetConsultnext(string orgid, string ghnm,string mzh)
		{
			string strsql = @"select *,d.Name ksmc from mz_ghzs a
left join [NewtouchHIS_Base].dbo.xt_zs b on a.zsCode=b.zsCode and a.OrganizeId=b.OrganizeId and b.zt=1
left join NewtouchHIS_Sett..mz_gh c on a.ghnm=c.ghnm and a.OrganizeId=c.OrganizeId and c.zt='1'
left join NewtouchHIS_Base..Sys_Department d on c.ks=d.Code and c.OrganizeId=d.OrganizeId  
where a.zt='1' and a.OrganizeId=@orgid";
			var inParameters = new List<SqlParameter>();
			if (!string.IsNullOrWhiteSpace(ghnm))
			{
				strsql += " and a.ghnm=@ghnm  ";
				inParameters.Add(new SqlParameter("@ghnm", ghnm));
			}
			if (!string.IsNullOrWhiteSpace(mzh))
			{
				strsql += " and c.mzh=@mzh  ";
				inParameters.Add(new SqlParameter("@mzh", mzh));
			}
			
			
			inParameters.Add(new SqlParameter("@orgid", orgid));
			return this.FirstOrDefault<OutpatientConsultVO>(strsql, inParameters.ToArray());
		}

		public int UpdataZSinsert(string mzh, string rygh, string orgid)
		{
			string sqlstr = "exec mz_待就诊无指定医生叫号自动分诊 @mzh,@orgid,@rygh";
			return this.ExecuteSqlCommand(sqlstr, new[] { new SqlParameter("@rygh", rygh)
				,new SqlParameter("@orgid", orgid),new SqlParameter("@mzh",mzh) });
		}
        /// <summary>
        /// 叫号修改状态
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="calledstu"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
		public int UpdatePatient(string mzh, int calledstu, string orgid)
		{
			var sqlstr = @"UPDATE b set b.calledstu=@calledstu from NewtouchHIS_Sett..mz_gh a
left join mz_ghzs b on a.ghnm=b.ghnm and a.OrganizeId=b.OrganizeId and b.zt='1'
where a.zt='1' and a.OrganizeId=@orgid and a.mzh=@mzh and b.ghnm is not null";

			return this.ExecuteSqlCommand(sqlstr, new[] { new SqlParameter("@calledstu", calledstu)
				,new SqlParameter("@orgid", orgid),new SqlParameter("@mzh",mzh) });
		}
        /// <summary>
        /// 验证叫号状态
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
		public int ISfalgPatient(string mzh, string orgid)
		{
			string sqlstr = @"select case when b.calledstu in('1','5') then 0 else 1 end flag from NewtouchHIS_Sett..mz_gh a
left join mz_ghzs b on a.ghnm=b.ghnm and a.OrganizeId=b.OrganizeId and b.zt='1'
where a.zt='1' and a.OrganizeId=@orgid and a.mzh=@mzh  and b.ghnm is not null";
			var par = new List<SqlParameter>();
			par.Add(new SqlParameter("@orgid", orgid));
			par.Add(new SqlParameter("@mzh", mzh));
			return this.FirstOrDefault<int>(sqlstr, par.ToArray());
		}
        /// <summary>
        /// 获取机构名称
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public string GetOrgName(string orgId) {
            string sqlstr = @"select Name from NewtouchHIS_Base..[Sys_Organize] where Id=@orgId";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgid", orgId));
            return this.FirstOrDefault<string>(sqlstr, par.ToArray());
        }

    }
}
