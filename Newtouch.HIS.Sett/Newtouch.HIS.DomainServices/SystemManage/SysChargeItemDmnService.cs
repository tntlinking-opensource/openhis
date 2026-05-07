using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.ValueObjects.SystemManage;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Newtouch.Core.Common;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public class SysChargeItemDmnService : DmnServiceBase, ISysChargeItemDmnService
    {

        public SysChargeItemDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 查询收费项目和药品
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="zlfzlbz"></param>
        /// <param name="mzzybz"></param>
        /// <returns></returns>
        public IList<SysChargeItemAndMedicineVO> SelectItemAndMedicineSearch(string keyword, string orgId, bool? zlfzlbz = true, string mzzybz = null)
        {
            if (string.IsNullOrWhiteSpace(orgId))
            {
                return null;
            }
            var sb = new StringBuilder();

            if (zlfzlbz.HasValue)
            {
                if (zlfzlbz.Value)
                {
                    //仅治疗项目
                    sb.Append(@"
WITH cteTree
AS (
    select * from [NewtouchHIS_Base]..V_S_xt_sfdl where dllb in (1,2) and OrganizeId = @orgId
)");
                }
                else
                {
                    //仅非治疗项目
                    sb.Append(@"
WITH cteTree
AS (
    select * from [NewtouchHIS_Base]..V_S_xt_sfdl where dllb = 3 and OrganizeId = @orgId
)");
                }
            }

            var par = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId)
            };
            sb.Append(@"
select top 800 * 
from(");

            #region 药品start
            sb.AppendFormat(@"
    select aaa.ypCode sfxmCode, aaa.ypmc sfxmmc, aaa.dlCode sfdlCode, bbb.dlmc sfdlmc
    , case {0} when '0' then aaa.bzdw when '1' then aaa.mzcldw when '2' then aaa.zycldw end dw
    , CONVERT(DECIMAL(19,4), case {0} when '0' then aaa.lsj when '1' then (case when aaa.mzcldw = aaa.bzdw then aaa.lsj else (aaa.lsj / aaa.bzs * aaa.mzcls) end) when '2' then (case when aaa.zycldw = aaa.bzdw then aaa.lsj else (aaa.lsj / aaa.bzs * aaa.zycls) end) 	end) dj
    , aaa.py, '' duration
    ,'1' yzlx   --1药品 2项目
    ,aaa.px
    ,'' bz
    ,1 dwjls
    ,1 jjcl,
    aaa.jl,
    aaa.jldw
    from [NewtouchHIS_Base]..V_S_xt_yp(nolock) aaa
    INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx(NOLOCK) ypsx ON ypsx.ypId=aaa.ypId AND ypsx.zt='1' AND ypsx.OrganizeId=aaa.OrganizeId
    left join [NewtouchHIS_Base]..V_S_xt_sfdl(nolock) bbb on aaa.dlCode = bbb.dlCode and bbb.OrganizeId = @orgId
    where aaa.OrganizeId = @orgId 
    and aaa.zt = '1' 
    and bbb.zt = '1'
", mzzybz ?? "0");
            //这一块的逻辑有点复杂，暂时不加。 需要用到 yfbm yfbm_yp
            //并不是xt_yp的mzzybz字段
            //if (!string.IsNullOrWhiteSpace(mzzybz))
            //{
            //    
            //}
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                par.Add(new SqlParameter("@ypsearchkeyword", "%" + keyword.Trim() + "%"));
                sb.Append(@"
    and (aaa.ypCode like @ypsearchkeyword or aaa.ypmc like @ypsearchkeyword or aaa.py like @ypsearchkeyword)");
            }
            #endregion 药品end

            sb.AppendLine(@"union all");
            #region 项目start
            sb.Append(@"
    select sfxmCode, sfxmmc, a.sfdlCode sfdlCode, b.dlmc sfdlmc, dw, CONVERT(DECIMAL(9,2),dj) dj, a.py,  isnull(a.duration,'') duration 
    ,'2' yzlx   --1药品 2项目
    ,px
    ,isnull(a.bz,'') bz
    ,isnull(a.dwjls,0) dwjls
    ,isnull(a.jjcl,1) jjcl
    ,0.00 jl
    ,'' jldw
    from[NewtouchHIS_Base]..V_S_xt_sfxm(nolock) a
    left join[NewtouchHIS_Base]..V_S_xt_sfdl(nolock) b on a.sfdlCode = b.dlCode and b.OrganizeId = @orgId
    where a.OrganizeId = @orgId 
    and a.zt = '1' 
    and b.zt = '1'
");
            if (zlfzlbz.HasValue)
            {
                sb.AppendLine("   and b.dlCode in (SELECT dlCode FROM cteTree)");
            }
            if (!string.IsNullOrWhiteSpace(mzzybz))
            {
                par.Add(new SqlParameter("@mzzybz", mzzybz));
                sb.AppendLine("   and (a.mzzybz = @mzzybz or a.mzzybz = '0')");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                par.Add(new SqlParameter("@xmsearchkeyword", "%" + keyword.Trim() + "%"));
                sb.AppendLine(@"   and (a.sfxmCode like @xmsearchkeyword or a.sfxmmc like @xmsearchkeyword or a.py like @xmsearchkeyword)");
            }
            #endregion 项目end

            sb.AppendLine(") as tttttttttt");
            sb.AppendLine("order by yzlx, isnull(px, 99999999), sfxmmc");

            return this.FindList<SysChargeItemAndMedicineVO>(sb.ToString(), par.ToArray());
        }

        /// <summary>
        /// 项目检索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="zlfzlbz">true 治疗项目 false 非治疗项目 null都包含</param>
        /// <param name="mzzybz"></param>
        /// <returns></returns>
        public IList<SysChargeItemVO> SelectSearch(string keyword, string orgId, bool? zlfzlbz = null, string mzzybz = null, string dlCode = null, bool isContansChildDl = true)
        {
            if (string.IsNullOrWhiteSpace(orgId))
            {
                return null;
            }
            var sb = new StringBuilder();
            if (zlfzlbz.HasValue)
            {
                if (zlfzlbz.Value)
                {
                    //仅治疗项目
                    sb.Append(@"
WITH cteTree
        AS (select * from [NewtouchHIS_Base]..V_S_xt_sfdl where dllb in (1,2) and OrganizeId = @orgId)");
                }
                else
                {
                    //仅非治疗项目
                    sb.Append(@"
WITH cteTree
        AS (select * from [NewtouchHIS_Base]..V_S_xt_sfdl where dllb = 3 and OrganizeId = @orgId)");
                }
            }
            sb.Append(@"select top 800 
sfxmCode, 
sfxmmc, 
a.sfdlCode sfdlCode, 
b.dlmc sfdlmc,
dw, 
CAST(ROUND(dj,2) AS NUMERIC(12, 2)) dj,
a.mzzybz,
a.py, 
a.duration
,isnull(a.bz,'') bz
,isnull(a.dwjls,0) dwjls
,a.jjcl
,a.zfxz
from[NewtouchHIS_Base]..V_S_xt_sfxm(nolock) a
left join[NewtouchHIS_Base]..V_S_xt_sfdl(nolock) b
on a.sfdlCode = b.dlCode and b.OrganizeId = @orgId

where a.OrganizeId = @orgId and a.zt = '1' and b.zt = '1'
");

            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));

            if (zlfzlbz.HasValue)
            {
                sb.Append(" and b.dlCode in (SELECT dlCode FROM cteTree)");
            }
            if (!string.IsNullOrWhiteSpace(mzzybz))
            {
                par.Add(new SqlParameter("@mzzybz", mzzybz));
                sb.Append(" and (a.mzzybz = @mzzybz or a.mzzybz = '0')");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                par.Add(new SqlParameter("@searchkeyword", "%" + keyword.Trim() + "%"));
                sb.Append(@" and (a.sfxmCode like @searchkeyword or a.sfxmmc like @searchkeyword or a.py like @searchkeyword)");
            }
            sb.Append(" order by isnull(px, 99999999), a.sfxmmc");

            return this.FindList<SysChargeItemVO>(sb.ToString(), par.ToArray());
        }

        /// <summary>
        /// 获取大类
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<SysChargeClassVO> GetSFDLList(string orgId)
        {
            if (string.IsNullOrWhiteSpace(orgId))
            {
                return null;
            }
            var sb = new StringBuilder();
            sb.Append(@"
                    SELECT  *
                    FROM    NewtouchHIS_Base..V_S_xt_sfdl
                    WHERE   OrganizeId = @orgId and zt = '1'");
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            return FindList<SysChargeClassVO>(sb.ToString(), par.ToArray());
        }

        /// <summary>
        /// 收费模板页面index list
        /// </summary>
        /// <returns></returns>
        public IList<SysChargeTemplateGridVO> Search(Pagination pagination, string keyword, string organizeId,string yxbz)
        {
            var sql = @"SELECT  SUM(ISNULL(sfxm.dj, '0.00') * ISNULL(xm.sl, 0)) zje ,
                        mb.sfmb ,
                        mb.sfmbbh ,
                        mb.sfmbmc ,
                        mb.mzzybz ,
                        mb.zt ,
                        mb.ks ,
                        mb.CreateTime
                FROM    xt_sfmb(NOLOCK) mb
                       LEFT JOIN dbo.xt_sfmbxm xm ON xm.sfmbbh = mb.sfmbbh
                                                      AND xm.OrganizeId = mb.OrganizeId
                        LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON sfxm.sfxmCode = xm.sfxm
                                                                        AND sfxm.OrganizeId = xm.OrganizeId
                WHERE mb.OrganizeId = @orgId and mb.zt=@yxbz";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", organizeId));
            pars.Add(new SqlParameter("@yxbz", yxbz));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (sfmbmc like @searchkeyword or sfmb like @searchkeyword)";
                pars.Add(new SqlParameter("@searchkeyword", "%" + keyword + "%"));
            }
            sql += @" GROUP BY mb.sfmbbh ,
                        mb.sfmb ,
                        mb.sfmbmc ,
                        mb.mzzybz ,
                        mb.zt ,
                        mb.ks ,
                        mb.CreateTime";
            return this.QueryWithPage<SysChargeTemplateGridVO>(sql, pagination, pars.ToArray());
        }


        /// <summary>
        /// 获取用法
        /// </summary>
        /// <returns></returns>
        public IList<SysUsageVO> GetYF()
        {
            try
            {
                var sql = @"
SELECT yfId,yfCode,yfmc,yplx,zt
  FROM [NewtouchHIS_Base]..[V_S_xt_ypyf]
WHERE zt='1'
                        ";
                return this.FindList<SysUsageVO>(sql);
            }
            catch (System.Exception ex)
            {
                throw new FailedCodeException(ex.Message);
            }
        }
    }
}
