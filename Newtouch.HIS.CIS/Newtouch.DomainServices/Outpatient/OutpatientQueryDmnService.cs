using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Domain.IDomainServices.Outpatient;
using Newtouch.Domain.ValueObjects.Outpatient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.DomainServices.Outpatient
{
    public class OutpatientQueryDmnService : DmnServiceBase, IOutpatientQueryDmnService
    {
        public OutpatientQueryDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }


        public IList<OutpatientConsultRecordVO> GetConsultRecordGridJson(Pagination pagination, string orgId, string kssj, string jssj,string ysgh)
        {
            string sql = @" select jz.blh,
jz.mzh,jz.xm,jz.xb,jz.nlshow nl,gh.zdmc,jz.zlkssj,isnull(jz.zljssj,isnull(jz.lastmodifytime,jz.createtime)) zljssj,ghksmc ks,staff.name ys,brxx.xian_dz dz,'' zg
from [xt_jz] jz
left join [NewtouchHIS_Sett].dbo.mz_gh gh
on gh.mzh=jz.mzh and gh.OrganizeId=jz.OrganizeId and gh.zt=jz.zt
left join [NewtouchHIS_Sett].dbo.xt_brjbxx brxx
on brxx.patid=gh.patid and  brxx.OrganizeId=gh.OrganizeId and brxx.zt=gh.zt
left join [NewtouchHIS_Base]..[Sys_Staff] staff on staff.gh=jz.jzys and staff.OrganizeId=jz.OrganizeId and staff.zt=jz.zt
where jz.zt=1 and jz.OrganizeId=@orgId 
and jz.zlkssj between @kssj and @jssj 
";
            IList<SqlParameter> parlist = new List<SqlParameter>();
            parlist.Add(new SqlParameter("@orgId", orgId));
            parlist.Add(new SqlParameter("@kssj", kssj));
            parlist.Add(new SqlParameter("@jssj", jssj + " 23:59:59"));
            if (ysgh!=null&& ysgh!="")
            {
                sql += " and jz.jzys=@ysgh";
                parlist.Add(new SqlParameter("@ysgh", ysgh));
            }
            return this.QueryWithPage<OutpatientConsultRecordVO>(sql, pagination, parlist.ToArray(), false);
        }

        public IList<OutpatientConsultDetailVO> GetConsultDetailGridJson(Pagination pagination, string orgId, string kssj, string jssj, string ysgh, string keyword)
        {
            string sql = @" select  
jz.mzh,jz.xm,jz.xb,jz.nlshow nl,
ghksmc ks,
staff.name ys,
case gh.mjzbz  when 1 then '普通门诊' when 2 then '急诊' when 3 then '专家门诊' when 4 then '特病门诊' when 5 then '重大疾病门诊' when 6 then '慢性病门诊' when 7 then '居民两病' when 8 then '意外伤害门诊' when 9 then '生育门诊' when 10 then '耐多药结核门诊' when 11 then '儿童两病门诊' end  jzlx,  
case when jz.fzbz=1 then '否' else '是' end cz,
jz.zlkssj,isnull(jz.zljssj,isnull(jz.lastmodifytime,jz.createtime)) zljssj,
case jzzt when '1' then '未就诊' when '2' then '就诊中' when '3' then '就诊结束' end jzzt,
gh.zdmc,
jz.kh jzkh,
case when jz.fzbz<>0 then Convert(varchar(50),jzrq,23)  else null end fzrq,
Convert(varchar(50),jz.csny,23) csny,jz.zjh,brxzmc,'' ylzh,
case hy when '1' then '未婚' when '2' then '已婚' else '不详' end hy,
zy,mzmc mz , '' xx,
case when jz.fzbz<>0 then null else Convert(varchar(50),jz.ghczsj,23)  end czrq,
dh,xian_dz,dwmc,dwdh,lxrgx,lxr,lxrdh,'' lxrdw,jjlxr_dz

from [xt_jz] jz
left join [NewtouchHIS_Sett].dbo.mz_gh gh
on gh.mzh=jz.mzh and gh.OrganizeId=jz.OrganizeId and gh.zt=jz.zt
left join [NewtouchHIS_Sett].dbo.xt_brjbxx brxx
on brxx.patid=gh.patid and  brxx.OrganizeId=gh.OrganizeId and brxx.zt=gh.zt
left join [NewtouchHIS_Base].[dbo].[xt_mz] mz on mz.mzCode=brxx.mz and mz.zt=brxx.zt
left join [NewtouchHIS_Base]..[Sys_Staff] staff on staff.gh=jz.jzys and staff.OrganizeId=jz.OrganizeId and staff.zt=jz.zt
where jz.zt=1 and jz.OrganizeId=@orgId 
and jz.zlkssj between @kssj and @jssj 
and jz.jzys=@ysgh";


            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and ( jz.mzh like @keyword or jz.xm like @keyword or jz.ghksmc like @keyword )";
            }


            return this.QueryWithPage<OutpatientConsultDetailVO>(sql, pagination, new[] {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj + " 23:59:59"),
                new SqlParameter("@ysgh", ysgh),
                new SqlParameter("@keyword", "%" + keyword.Trim() + "%")
            }, false);
        }
        public IList<OutpatientDetailVO> GetOutpatientDetailGridJson(Pagination pagination, string orgId, string kssj, string jssj,string yscode)
        {
            string sql = @" 
select 
jz.zlkssj jzsj,
jz.mzh mzh,
jz.xm xm,
jz.xb xb,
jz.brxzmc,
ks.Name jzks,
sum(case cf.cflx when '5' then 0 else 1 end) cfs,
isnull(sum(case cf.cflx when '5' then 0 else cf.zje end),0) cfje,
sum(case cf.cflx when '5' then 1 else 0 end) jcs,
isnull(sum(case cf.cflx when '5' then cf.zje else 0 end),0) jcje,
0.00 fjxm
from xt_jz jz
 left join [NewtouchHIS_Sett].dbo.mz_gh gh
on gh.mzh=jz.mzh and gh.OrganizeId=jz.OrganizeId and gh.zt=jz.zt
left join [NewtouchHIS_Sett].dbo.xt_brjbxx brxx
on brxx.patid=gh.patid and  brxx.OrganizeId=gh.OrganizeId and brxx.zt=gh.zt
left join [NewtouchHIS_Base]..V_S_Sys_Department ks on ks.OrganizeId=jz.OrganizeId and ks.Code=jz.jzks
left join xt_cf cf on cf.jzId=jz.jzId and cf.zt='1'
where jz.zt='1'
and jz.zlkssj>=@kssj
and jz.zlkssj<=@jssj
and jz.OrganizeId=@orgId
and jz.jzys=@jzys
group by jz.zlkssj,
jz.mzh,
jz.xm,
jz.xb,
jz.brxzmc,
ks.Name ";
            IList<OutpatientDetailVO> vo = this.QueryWithPage<OutpatientDetailVO>(sql, pagination, new[] {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@jzys", yscode),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj + " 23:59:59"),
            });
            return vo;
        }
        /// <summary>
        /// 门诊详细资料明细
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<OutpatientDetailMXVO> GetOutpatientDetailMXGridJson(string mzh,string orgId)
        {
            string sql = @" 
select cf.cflx,cfh ,yh.Name ys,cf.CreateTime kdrq,case when cflx='4' or cflx='5' then '('+cfmx.ztmc+')'+cfmx.xmmc else isnull(cfmx.ypmc,cfmx.xmmc) end xmmc,
cfmx.sl zl,yf.yfmc ypyf,pc.yzpcmcsm,cfmx.dw,cfmx.je ,sfbz,tbz
from xt_jz jz  (nolock)
left join xt_cf cf (nolock) on cf.jzId=jz.jzId and cf.zt='1' and cf.OrganizeId=jz.OrganizeId
left join xt_cfmx cfmx (nolock) on cf.cfId=cfmx.cfId and cfmx.zt='1' and cf.OrganizeId=cfmx.OrganizeId
left join [NewtouchHIS_Base]..Sys_Staff yh on yh.gh=cf.ys and cf.OrganizeId=yh.OrganizeId
left join [NewtouchHIS_Base]..V_S_xt_ypyf yf on yf.yfCode=cfmx.yfCode 
left join [NewtouchHIS_Base]..xt_yzpc pc on pc.yzpccode=cfmx.pcCode and pc.organizeid=cfmx.organizeid
where  jz.zt='1'
and jz.mzh=@mzh
and jz.OrganizeId=@orgId
order by cflx desc,cfh
";
            IList<OutpatientDetailMXVO> vo = this.FindList<OutpatientDetailMXVO>(sql, new[] {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@mzh", mzh),
            });
            return vo;


        }
        public IList<OutpatientReservationVO> GetReservationGridJson(Pagination pagination, string orgId, string kssj, string jssj, string yscode,string xm)
        {
            string sql = @" 
select yy.yyrq,case yy.yysj when '1' then '上午' when '2' then '下午' end yysj,
yy.mzh,
jz.xm,
jz.xb,
yy.yyks,
yh.name yyys,
yy.yylxfs
 from mz_yykz yy
left join xt_jz jz on yy.mzh=jz.mzh and jz.OrganizeId=yy.OrganizeId and jz.zt='1'
left join [NewtouchHIS_Base]..V_S_Sys_Staff yh on yh.gh=yy.yyys and yh.OrganizeId=yy.OrganizeId and yh.zt='1'
where yy.zt='1'
and yy.OrganizeId=@orgId
and yy.yyrq>=@kssj
and yy.yyrq<=@jssj
and yy.yyys=@jzys
";
            if (xm!=null&&xm!="")
            {
                sql += " and jz.xm like '%'+@xm+'%'";
            }
            try
            {
                IList<OutpatientReservationVO> vo = this.QueryWithPage<OutpatientReservationVO>(sql, pagination, new[] {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@jzys", yscode),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj + " 23:59:59"),
                new SqlParameter("@xm", xm),
            });
                return vo;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

		public IList<OutpatientConsultRecordVO> GetlsblInfoJson(Pagination pagination, string orgId, string blh, string ysgh)
		{
			string sqlstr = @" select  jz.blh,jz.jzId,
jz.mzh,jz.xm,jz.xb,jz.nlshow nl,gh.zdmc,jz.zlkssj,isnull(jz.zljssj,jz.lastmodifytime) zljssj,ghksmc ks,staff.name ys,brxx.xian_dz dz,'' zg
from [xt_jz] jz
left join [NewtouchHIS_Sett].dbo.mz_gh gh
on gh.mzh=jz.mzh and gh.OrganizeId=jz.OrganizeId and gh.zt=jz.zt
left join [NewtouchHIS_Sett].dbo.xt_brjbxx brxx
on brxx.patid=gh.patid and  brxx.OrganizeId=gh.OrganizeId and brxx.zt=gh.zt
left join [NewtouchHIS_Base]..[Sys_Staff] staff on staff.gh=jz.jzys and staff.OrganizeId=jz.OrganizeId and staff.zt=jz.zt
where jz.zt=1 and jz.OrganizeId=@orgId 
and jz.blh=@blh ";

			return this.QueryWithPage<OutpatientConsultRecordVO>(sqlstr, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@blh", blh),
				new SqlParameter("@ysgh", ysgh)
			}, false);
		}
        /// <summary>
        /// 门诊病历记录-历史处方明细
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="orgId"></param>
        /// <param name="jzid"></param>
        /// <param name="ysgh"></param>
        /// <returns></returns>
		public IList<OutpatientCfmxVO> GetlsjzcfblInfoJson(Pagination pagination, string orgId, string jzid, string ysgh)
		{
			string sqlstr = @" select a.cfh,isnull(b.xmCode,b.ypCode) sfxmcode, isnull(b.xmmc,b.ypmc) sfxmmc,b.sl,b.dj,b.dw,
b.je,sta.Name klys,dep.Name klks,a.cflx,a.sfbz,a.tbz
from xt_cf a with(nolock)
left join xt_cfmx b with(nolock) on a.cfId=b.cfId and b.zt='1' and a.OrganizeId=b.OrganizeId
left join NewtouchHIS_Base..Sys_Staff sta on sta.gh=a.ys and a.OrganizeId=sta.OrganizeId 
left join NewtouchHIS_Base..Sys_Department dep on dep.Code=a.ks and a.OrganizeId=b.OrganizeId
where a.jzId=@jzid and a.OrganizeId=@orgId  and a.zt=1 ";

			return this.QueryWithPage<OutpatientCfmxVO>(sqlstr, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@jzid", jzid),
				new SqlParameter("@ysgh", ysgh)
			}, false);
		}
	}
}
