using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.OutpatientManage;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using Newtouch.Tools.DB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Newtouch.HIS.DomainServices
{
    public class OutPatientChargeQueryDmnService : DmnServiceBase, IOutPatientChargeQueryDmnService
    {
        private readonly IFinancialInvoiceRepo _financialInvoiceEntityRepository;
        private readonly ISysConfigRepo _sysConfigRepo; //系统配置
        public OutPatientChargeQueryDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        #region old SelectRegChargeQuery code
        /// <summary>
        /// 挂号收费查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="kh"></param>
        /// <param name="fph"></param>
        /// <param name="xm"></param>
        /// <param name="syy"></param>
        /// <param name="createTimestart"></param>
        /// <param name="createTimeEnd"></param>
        /// <returns></returns>
        public IList<OutPatientRegChargeVO> SelectRegChargeQuery(Pagination pagination, string kh, string fph, string xm, string syy, DateTime? createTimestart, DateTime? createTimeEnd)
        {
            if (string.IsNullOrWhiteSpace(pagination.sidx))
            {
                throw new Exception("pagination.sidx is required");
            }
            var sortby = pagination.sidx;
            if (!string.IsNullOrWhiteSpace(pagination.sord) && pagination.sord.ToUpper() != "ASC")
            {
                if (!sortby.Contains(",") && !sortby.Contains(" "))
                {
                    sortby += " " + pagination.sord;
                }
            }

            var paraList = new List<SqlParameter>
            {
                new SqlParameter("@kh", kh.Trim()),
                new SqlParameter("@fph", fph.Trim()),
                new SqlParameter("@xm", xm.Trim()),
                new SqlParameter("@CreatorCode", syy.Trim()),
                new SqlParameter("@CreateTimestart", createTimestart),
                new SqlParameter("@CreateTimeEnd", createTimeEnd),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId),
                new SqlParameter("@currPageIndex", pagination.page),
                new SqlParameter("@perRows", pagination.rows),
                new SqlParameter("@orderByParam", sortby)
            };
            var outParameter = new SqlParameter("@records", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            paraList.Add(outParameter);
            var list = FindList<OutPatientRegChargeVO>("exec spSelectGhsfcxRecords @kh,@fph,@xm,@CreatorCode,@CreateTimestart,@CreateTimeEnd,@OrganizeId,@currPageIndex,@perRows,@orderByParam, @records output", paraList.ToArray());
            pagination.records = outParameter.Value.ToInt();
            return list;
        }
        #endregion

        /// <summary>
        /// 挂号收费查询 new SelectRegChargeQuery code
        /// 该查询cxjsnm为退票记录的jsnm 
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="kh"></param>
        /// <param name="fph"></param>
        /// <param name="syy">收银员</param>
        /// <param name="createTimestart"></param>
        /// <param name="createTimeEnd"></param>
        /// <returns></returns>
        public IList<OutPatientRegChargeMVO> RegChargeQuery(Pagination pagination, string kh, string fph, string jsfph, string xm, string syy, DateTime? createTimestart, DateTime? createTimeEnd, DateTime? sfrqTimestart, DateTime? sfrqTimeEnd, string zxlsh)
        {
            var sqlStr = new StringBuilder(@"
SELECT js.blh, js.jsnm,js.jslx,
       CASE WHEN len(gh.kh) >= 28 THEN substring(gh.kh,1,10) ELSE gh.kh END AS kh,
       js.xm,
       gh.xb,
	   gh.mzh,gh.kh sbbh,
       gh.zjh,
       isnull(js.fph, '') AS fph,
       js.zje jszje,js.xjzf jsxjzf,(js.zje-js.xjzf) jsjz,
       ybjyfy.ybjsh, ybjyfy.JBZF,ybjyfy.GBZF,ybjyfy.JBYE,ybjyfy.GBYE,
       js.CreatorCode,
       js.CreateTime,
       ks.name AS ghksmc,
       ISNULL(CASE WHEN fpdy.oldFPH IS NOT NULL OR LEN(LTRIM(RTRIM(fpdy.oldFPH)))>0 THEN fpdy.oldFPH ELSE js.fph END, '') oldfph,
       CASE WHEN fpdy.xfph IS NOT NULL OR fpdy.dyfs=2 THEN '1' ELSE '0' END AS isxfph,
       ISNULL(js.LastModifierCode, '') tCreatorCode,
        sfyuserstaff.Name CreatorName,ghysStaff.Name ghysmc,
        isnull(js.jzsj, js.CreateTime) sfrq,
		brxz.brxzmc,
        gh.zdmc,jzys.Name jzys,xtjz.zlkssj jzsj,mzybjs.setl_id zxlsh
FROM dbo.mz_js(NOLOCK) js
LEFT JOIN mz_js_ybjyfy(nolock) ybjyfy 
    ON ybjyfy.jsnm = js.jsnm
LEFT JOIN dbo.mz_gh(NOLOCK) gh 
    ON gh.ghnm=js.ghnm AND gh.OrganizeId=js.OrganizeId
LEFT JOIN [Newtouch_CIS]..xt_jz xtjz
	ON xtjz.mzh=gh.mzh and xtjz.OrganizeId=gh.OrganizeId and xtjz.zt=1
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department ks 
    ON ks.Code = gh.ks AND ks.zt='1' AND ks.OrganizeId=js.OrganizeId
LEFT JOIN mz_curr_fp(nolock) fpdy 
    ON js.jsnm = fpdy.jsnm AND fpdy.zt='1' AND fpdy.OrganizeId=js.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff sfyuserstaff 
    ON sfyuserstaff.Account = js.CreatorCode AND sfyuserstaff.zt='1' AND sfyuserstaff.OrganizeId=js.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff ghysStaff 
    ON ghysStaff.gh = gh.ys AND ghysStaff.zt='1' AND ghysStaff.OrganizeId=js.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff jzys 
	ON jzys.gh = gh.ys AND jzys.zt='1' AND jzys.OrganizeId=js.OrganizeId
LEFT JOIN xt_brxz brxz
    on brxz.brxz = js.brxz and brxz.zt = '1' and brxz.OrganizeId = js.OrganizeId
LEFT JOIN dbo.xt_brjbxx xx 
    ON xx.blh = gh.blh AND xx.OrganizeId = js.OrganizeId AND xx.zt = '1'
LEFT JOIN dbo.drjk_mzjs_output mzybjs 
    ON mzybjs.setl_id = js.ybjslsh AND mzybjs.zt = '1'
WHERE 
    js.OrganizeId=@OrganizeId and js.zt= '1'
    --未退
    and ISNULL(js.tbz, 0)=0  
");
            var paraList = new List<DbParameter>
            {
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            if (!string.IsNullOrWhiteSpace(xm))
            {
                //sqlStr.AppendLine("AND (js.xm LIKE @xm or js.blh LIKE @blh or xx.py like @py )");
                sqlStr.AppendLine("AND (js.xm LIKE @xm or js.blh LIKE @blh or gh.mzh LIKE @xm or xx.py like @xm)");
                paraList.Add(new SqlParameter("@xm", "%" + xm.Trim() + "%"));
                paraList.Add(new SqlParameter("@blh", "%" + xm.Trim() + "%"));
                // paraList.Add(new SqlParameter("@py", "%" + xm.Trim() + "%")); 查询效率过慢 去除 lixin 20200114
            }
            //if (!string.IsNullOrWhiteSpace(kh))  //暂无刷卡功能取消卡号查询
            //{
            //    sqlStr.AppendLine("AND (gh.kh LIKE @kh or js.blh LIKE @blh )");
            //    paraList.Add(new SqlParameter("@kh", "%" + kh.Trim() + "%"));
            //    paraList.Add(new SqlParameter("@blh", "%" + kh.Trim() + "%"));
            //}
            if (!string.IsNullOrWhiteSpace(fph))
            {
                sqlStr.AppendLine("AND js.fph >= @fph");
                paraList.Add(new SqlParameter("@fph", fph.Trim()));
                //paraList.Add(new SqlParameter("@fph", "%" + fph.Trim() + "%"));
            }
            if (!string.IsNullOrWhiteSpace(jsfph))
            {
                sqlStr.AppendLine("AND js.fph <= @jsfph");
                paraList.Add(new SqlParameter("@jsfph", jsfph.Trim()));
                //paraList.Add(new SqlParameter("@fph", "%" + fph.Trim() + "%"));
            }
            if (!string.IsNullOrWhiteSpace(syy))
            {
                sqlStr.AppendLine("AND js.CreatorCode LIKE @CreatorCode");
                paraList.Add(new SqlParameter("@CreatorCode", "%" + syy.Trim() + "%"));
            }
            if (createTimestart != null)
            {
                var start = Constants.MinDate;
                start = start >= createTimestart ? start : (DateTime)createTimestart;
                sqlStr.AppendLine("AND js.CreateTime >= @BeginCreateTime");
                paraList.Add(new SqlParameter("@BeginCreateTime", start));
            }
            if (createTimeEnd != null)
            {
                var end = Constants.MinDate;
                end = end >= createTimeEnd ? end : (DateTime)createTimeEnd;
                sqlStr.AppendLine("AND js.CreateTime<=@EndCreateTime+' 23:59:59'");
                paraList.Add(new SqlParameter("@EndCreateTime", end));
            }
            if (sfrqTimestart != null)
            {
                var start = Constants.MinDate;
                start = start >= sfrqTimestart ? start : (DateTime)sfrqTimestart;
                sqlStr.AppendLine("AND isnull(js.jzsj, js.CreateTime) >= @Beginsfrq");
                paraList.Add(new SqlParameter("@Beginsfrq", start));
            }
            if (sfrqTimeEnd != null)
            {
                var end = Constants.MinDate;
                end = end >= sfrqTimeEnd ? end : sfrqTimeEnd.Value.AddDays(1);
                sqlStr.AppendLine("AND isnull(js.jzsj, js.CreateTime) < @Endsfrq");
                paraList.Add(new SqlParameter("@Endsfrq", end));
            }
            if (!string.IsNullOrWhiteSpace(zxlsh))
            {
                sqlStr.AppendLine("AND mzybjs.setl_id=@zxlsh");
                paraList.Add(new SqlParameter("@zxlsh", zxlsh));
            }
            return QueryWithPage<OutPatientRegChargeMVO>(sqlStr.ToString(), pagination, paraList.ToArray());
        }

        /// <summary>
        /// 待上传的门诊自费病人结算信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="xm"></param>
        /// <param name="createTimestart"></param>
        /// <param name="createTimeEnd"></param>
        /// <returns></returns>
        public IList<OutPatientRegChargeMVO> ZFRegChargeQuery(Pagination pagination, string xm, DateTime? createTimestart, DateTime? createTimeEnd)
        {
            var sqlStr = new StringBuilder(@"
SELECT DISTINCT js.blh, js.jsnm,js.jslx,
       CASE WHEN len(gh.kh) >= 28 THEN substring(gh.kh,1,10) ELSE gh.kh END AS kh,
       js.xm,
       gh.xb,
	   gh.mzh,gh.kh sbbh,
       gh.zjh,
       isnull(js.fph, '') AS fph,
       js.zje jszje,js.xjzf jsxjzf,(js.zje-js.xjzf) jsjz,
       ybjyfy.ybjsh, ybjyfy.JBZF,ybjyfy.GBZF,ybjyfy.JBYE,ybjyfy.GBYE,
       js.CreatorCode,
       js.CreateTime,
       ks.name AS ghksmc,
       ISNULL(CASE WHEN fpdy.oldFPH IS NOT NULL OR LEN(LTRIM(RTRIM(fpdy.oldFPH)))>0 THEN fpdy.oldFPH ELSE js.fph END, '') oldfph,
       CASE WHEN fpdy.xfph IS NOT NULL OR fpdy.dyfs=2 THEN '1' ELSE '0' END AS isxfph,
       ISNULL(js.LastModifierCode, '') tCreatorCode,
        sfyuserstaff.Name CreatorName,ghysStaff.Name ghysmc,
        isnull(js.jzsj, js.CreateTime) sfrq,
		brxz.brxzmc,
        gh.zdmc,jzys.Name jzys,xtjz.zlkssj jzsj,mzybjs.setl_id zxlsh,
        CASE WHEN drjk.mlbm_id IS NOT NULL THEN '已上传' ELSE '' END AS sfyb
FROM dbo.mz_js(NOLOCK) js
LEFT JOIN mz_js_ybjyfy(nolock) ybjyfy 
    ON ybjyfy.jsnm = js.jsnm
LEFT JOIN dbo.mz_gh(NOLOCK) gh 
    ON gh.ghnm=js.ghnm AND gh.OrganizeId=js.OrganizeId
LEFT JOIN [Newtouch_CIS]..xt_jz xtjz
	ON xtjz.mzh=gh.mzh and xtjz.OrganizeId=gh.OrganizeId and xtjz.zt=1
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department ks 
    ON ks.Code = gh.ks AND ks.zt='1' AND ks.OrganizeId=js.OrganizeId
LEFT JOIN mz_curr_fp(nolock) fpdy 
    ON js.jsnm = fpdy.jsnm AND fpdy.zt='1' AND fpdy.OrganizeId=js.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff sfyuserstaff 
    ON sfyuserstaff.Account = js.CreatorCode AND sfyuserstaff.zt='1' AND sfyuserstaff.OrganizeId=js.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff ghysStaff 
    ON ghysStaff.gh = gh.ys AND ghysStaff.zt='1' AND ghysStaff.OrganizeId=js.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff jzys 
	ON jzys.gh = gh.ys AND jzys.zt='1' AND jzys.OrganizeId=js.OrganizeId
LEFT JOIN xt_brxz brxz
    on brxz.brxz = js.brxz and brxz.zt = '1' and brxz.OrganizeId = js.OrganizeId
LEFT JOIN dbo.xt_brjbxx xx 
    ON xx.blh = gh.blh AND xx.OrganizeId = js.OrganizeId AND xx.zt = '1'
LEFT JOIN dbo.drjk_mzjs_output mzybjs 
    ON mzybjs.setl_id = js.ybjslsh AND mzybjs.zt = '1'
LEFT JOIN dbo.Drjk_jxcsc_output drjk ON drjk.mlbm_id = CONVERT(VARCHAR(50), js.jsnm)  -- 关联条件
WHERE 
    js.OrganizeId=@OrganizeId and js.zt= '1'
    and js.brxz= '0'
    --未退
    and ISNULL(js.tbz, 0)=0 
");
            var paraList = new List<DbParameter>
            {
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            if (!string.IsNullOrWhiteSpace(xm))
            {
                sqlStr.AppendLine("AND (js.xm LIKE @xm or js.blh LIKE @blh or gh.mzh LIKE @xm or xx.py like @xm)");
                paraList.Add(new SqlParameter("@xm", "%" + xm.Trim() + "%"));
                paraList.Add(new SqlParameter("@blh", "%" + xm.Trim() + "%"));
            }

            if (createTimestart != null)
            {
                var start = Constants.MinDate;
                start = start >= createTimestart ? start : (DateTime)createTimestart;
                sqlStr.AppendLine("AND js.CreateTime >= @BeginCreateTime");
                paraList.Add(new SqlParameter("@BeginCreateTime", start));
            }
            if (createTimeEnd != null)
            {
                var end = Constants.MinDate;
                end = end >= createTimeEnd ? end : (DateTime)createTimeEnd;
                sqlStr.AppendLine("AND js.CreateTime<=@EndCreateTime+' 23:59:59'");
                paraList.Add(new SqlParameter("@EndCreateTime", end));
            }
            return QueryWithPage<OutPatientRegChargeMVO>(sqlStr.ToString(), pagination, paraList.ToArray());
        }

        /// <summary>
        /// 获取结算支付方式
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jsnms"></param>
        /// <returns></returns>
        public IList<SettZffsResult> GetSettZffsResultList(string orgId, string jsnms)
        {
            var sql = @"select distinct mzjszffs.jsnm, xjzffs.xjzffs, xjzffs.xjzffsmc
from mz_jszffs mzjszffs
left join xt_xjzffs xjzffs
on xjzffs.xjzffs = mzjszffs.xjzffs
where mzjszffs.zt = '1' and jsnm in (select * from dbo.f_split(@jsnms,','))
and mzjszffs.OrganizeId = @orgId";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@jsnms", jsnms));
            return this.FindList<SettZffsResult>(sql, pars.ToArray());
        }

        /// <summary>
        /// 收费明细
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <returns></returns>
        public IList<OutPatientRegChargeDetailsVO> GetRecordsByJsnm(string jsnm)
        {
            IList<SqlParameter> par = new List<SqlParameter>();
            string sfxmztbs = _sysConfigRepo.GetValueByCode("sfxmztbs", OperatorProvider.GetCurrent().OrganizeId);
            if (sfxmztbs == "false")
            {
                var inParameters = new Dictionary<string, object>
                {

                };
                var strSql = new StringBuilder();
                strSql.Append(@"EXEC spSelectRecordsDetailByJsnm @jsnm=@jsnm,@OrganizeId=@OrganizeId");
                SqlParameter[] para = {
                    new SqlParameter("@jsnm",jsnm),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId),
            };

                return this.FindList<OutPatientRegChargeDetailsVO>(strSql.ToString(), para);
                //IList<OutPatientRegChargeDetailsVO> jj = DbHelper.ExecuteProcedureQueryList<OutPatientRegChargeDetailsVO>("spSelectRecordsDetailByJsnm", inParameters);
                //return jj;
            }
            var sql = @"select cfh,fph,jszt,'' gg,0.00 zfbl,'' pc,sfxm,ypmc,dw,dlmc, 
	case when len(ztId)>0 then sum(convert(decimal(18,4),dj*sl)) else sum(dj) end dj, case when len(ztId)>0 then 1 else  sum(sl) end sl, sum(je) AS je,case when ztId is not null then 1 else sum(klsl) end klsl ,sum(klje) klje,case when sum(ytje)>0 and len(ztId)>0 then '1' else sum(ytsl) end ytsl,sum(ytje) ytje,ysmc,ksmc from (
	SELECT cf.cfh cfh,fph,jszt, (case when xm.ztId is not null then xm.ztId else xm.sfxm end) sfxm, (case when xm.ztmc is not null then xm.ztmc else sfxm.sfxmmc end) AS ypmc,
	(case when xm.ztId is not null then '组套' else xm.dw end) AS dw, (case when xm.ztId is not null then '项目组套组合' else sfdl.dlmc end) dlmc, 
	convert(decimal(18,4),sum(xm.dj)) dj, sum(xm.sl) sl, sum(jsmx.sl*xm.dj) AS je,sum(xm.sl) klsl,sum(xm.je) klje,sum(xm.sl-jsmx.sl) ytsl,sum(xm.je-(jsmx.sl*xm.dj)) ytje,xm.ysmc,xm.ksmc,xm.ztId,xm.ztmc 
	--into #js_pay
	FROM dbo.mz_jsmx jsmx
	INNER JOIN dbo.mz_xm(NOLOCK) xm ON xm.xmnm=jsmx.mxnm AND xm.OrganizeId=jsmx.OrganizeId
    INNER JOIN dbo.mz_js (NOLOCK) mzjs on jsmx.jsnm=mzjs.jsnm and jsmx.OrganizeId=mzjs.OrganizeId
    left join mz_cf cf on cf.cfnm=xm.cfnm and cf.OrganizeId=xm.OrganizeId and cf.zt='1'
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_sfxm sfxm ON sfxm.sfxmCode=xm.sfxm AND sfxm.OrganizeId=jsmx.OrganizeId
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=xm.dl AND sfdl.OrganizeId = jsmx.OrganizeId
	WHERE jsmx.jsnm=@jsnm
	AND jsmx.OrganizeId=@OrganizeId
	group by xm.ztId,xm.ztmc,xm.sfxm,sfxm.sfxmmc,xm.dw,sfdl.dlmc,xm.ysmc,xm.ksmc,cf.cfh,fph,jszt
	) a group by ztId,ztmc,sfxm,ypmc,dw,dlmc,ysmc,ksmc,cfh,fph,jszt
	UNION ALL
	SELECT cf.cfh,fph,jszt,yp.ypgg gg,0.00 zfbl,pc.yzpcmc pc,cfmx.yp sfxm, yp.ypmc, cfmx.dw dw, sfdl.dlmc, 
	cfmx.dj, jsmx.sl, (jsmx.sl*dj) AS je,cfmx.sl klsl,cfmx.je klje,cfmx.sl-jsmx.sl ytsl,cfmx.je-(jsmx.sl*cfmx.dj) ytje,cf.ysmc,cf.ksmc
	FROM dbo.mz_jsmx jsmx
    INNER JOIN dbo.mz_js (NOLOCK) mzjs on jsmx.jsnm=mzjs.jsnm and jsmx.OrganizeId=mzjs.OrganizeId
	INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfmxId=jsmx.cf_mxnm AND cfmx.OrganizeId = jsmx.OrganizeId
    LEFT JOIN NewtouchHIS_Base.dbo.xt_yzpc pc on pc.yzpcCode=cfmx.pccode and pc.OrganizeId=cfmx.OrganizeId
	INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfnm=cfmx.cfnm AND cf.OrganizeId=jsmx.OrganizeId
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=cfmx.yp AND yp.OrganizeId=jsmx.OrganizeId
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode = cfmx.dl AND sfdl.OrganizeId=jsmx.OrganizeId
	WHERE jsmx.jsnm=@jsnm
	AND jsmx.OrganizeId=@OrganizeId ";
            par.Add(new SqlParameter("@jsnm", jsnm));
            par.Add(new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId));
            return this.FindList<OutPatientRegChargeDetailsVO>(sql.ToString(), par.ToArray());


            //var inParameters = new Dictionary<string, object>
            //{
            //    {"@jsnm", jsnm},
            //    {"@OrganizeId", OperatorProvider.GetCurrent().OrganizeId}
            //};

            //return DbHelper.ExecuteProcedureQueryList<OutPatientRegChargeDetailsVO>("spSelectRecordsDetailByJsnm", inParameters);

            //return DbHelper.ExecuteProcedureQueryList<OutPatientRegChargeDetailsVO>("spSelectRecordsByJsnm", inParameters);

        }

        public IList<OutPatRegChargeDetailsMVO> GetRecordsDetailByJsnm(string jsnm)
        {
            var inParameters = new Dictionary<string, object>
            {
                {"@jsnm", jsnm},
                {"@OrganizeId", OperatorProvider.GetCurrent().OrganizeId}
            };
            return DbHelper.ExecuteProcedureQueryList<OutPatRegChargeDetailsMVO>("spSelectRecordsDetailByJsnm", inParameters);
        }
        /// <summary>
        /// 重打/补打发票 mz_js List
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="jsnm"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="kh"></param>
        /// <param name="fph"></param>
        /// <returns></returns>
        public IList<OutPatientReprintOrSuppPrintSettleVO> LoadMzjsRecords(Pagination pagination, string jsnm, DateTime? startDate, DateTime? endDate, string kh, string yfph)
        {
            IList<SqlParameter> inSqlParameterList = new List<SqlParameter>();
            StringBuilder sqlStr = new StringBuilder();

            sqlStr.Append(@"
							 select a.jsnm,d.xm,b.kh,isnull(c.xfph,a.fph) as xfph,e.Name rymc,a.CreateTime,a.zje
                             from mz_js a 
							 inner join mz_gh b on a.ghnm=b.ghnm and b.OrganizeId=@OrganizeId
                             left join MZ_CURR_FP c on a.jsnm = c.jsnm and c.OrganizeId=@OrganizeId
                             inner join xt_brjbxx d on a.patid = d.patid and d.OrganizeId=@OrganizeId
                             left join [NewtouchHIS_Base]..V_S_Sys_Staff e on a.CreatorCode = e.gh and e.OrganizeId=@OrganizeId
                             where a.OrganizeId=@OrganizeId
                                and a.jslx != " + (int)EnumJslx.JCJZ + @" and a.jszt != " + (int)EnumJieSuanZT.YT
                         );
            if (!string.IsNullOrEmpty(jsnm))
            {
                sqlStr.Append(" and a.jsnm = @jsnm");
                inSqlParameterList.Add(new SqlParameter("@jsnm", jsnm.Trim().ToInt()));
            }
            if (!string.IsNullOrEmpty(kh))
            {
                sqlStr.Append(" and kh = @kh");
                inSqlParameterList.Add(new SqlParameter("@kh", kh.Trim()));
            }
            if (!string.IsNullOrEmpty(yfph))
            {
                sqlStr.Append(" and ( a.fph = @fph or c.xfph = @fph )");
                inSqlParameterList.Add(new SqlParameter("@fph", yfph.Trim()));
            }
            if (startDate.HasValue)
            {
                sqlStr.Append(" and a.CreateTime >= @startDate");
                inSqlParameterList.Add(new SqlParameter("@startDate", startDate));
            }
            if (endDate.HasValue)
            {
                sqlStr.Append(" and a.CreateTime < @endDate");
                inSqlParameterList.Add(new SqlParameter("@endDate", endDate));
            }
            sqlStr.Append(" and not EXISTS(SELECT 1 FROM mz_js b WHERE a.jsnm = b.cxjsnm and OrganizeId=@OrganizeId)");

            inSqlParameterList.Add(new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId));
            return this.QueryWithPage<OutPatientReprintOrSuppPrintSettleVO>(sqlStr.ToString(), pagination, inSqlParameterList.ToArray());

        }
        /// <summary>
        /// 门诊挂号项目 jsnmStr
        /// </summary>
        /// <param name="jsnmStr"></param>
        /// <returns></returns>
        public List<OutPatientReprintOrSuppPrintSettleDetailVO> GetOutpatientRegItemList(string jsnmStr)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select mz_gh.kh,mz_ghxm.sfxm,sfxmmc,mz_ghxm.dl,dlmc,mz_ghxm.dj,mz_ghxm.fwfdj,sl,je,mz_ghxm.zfbl,
                            xmnm,mz_ghxm.zfxz,mz_ghxm.ghnm,mz_ghxm.dj+mz_ghxm.fwfdj as djandfwfdj,mz_ghxm.patid,mz_ghxm.CreateTime
                            from mz_ghxm 
                            left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm on mz_ghxm.sfxm = xt_sfxm.sfxmCode and xt_sfxm.OrganizeId=@OrganizeId
                            left join ( select * from NewtouchHIS_Base..V_S_xt_sfdl ) as xt_sfdl on mz_ghxm.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId
                            inner join mz_gh on mz_ghxm.ghnm=mz_gh.ghnm and mz_gh.OrganizeId=@OrganizeId
                            where exists(select 1 from mz_jsmx where jsnm in (select * from dbo.f_split(@jsnmStr,',')) and jslx ='" + ((int)EnumJslx.GH).ToString() + @"' and mxnm = mz_ghxm.xmnm ) 
                                and mz_ghxm.OrganizeId=@OrganizeId
                         ");

            SqlParameter[] param =
                {
                    new SqlParameter("@jsnmStr",jsnmStr),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
                };

            List<OutPatientReprintOrSuppPrintSettleDetailVO> list = this.FindList<OutPatientReprintOrSuppPrintSettleDetailVO>(strSql.ToString(), param);
            return list;
        }
        /// <summary>
        /// 门诊项目 jsnmStr
        /// </summary>
        /// <param name="jsnmStr"></param>
        /// <returns></returns>
        public List<OutPatientReprintOrSuppPrintSettleDetailVO> GetOutpatientItemList(string jsnmStr)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select mz_gh.kh, mz_xm.patid,mz_xm.sfxm,sfxmmc,mz_xm.dl,dlmc,mz_xm.dj,mz_xm.fwfdj,mz_xm.dj+mz_xm.fwfdj as djandfwfdj,mz_jsmx.sl,(mz_xm.dj +mz_xm.fwfdj)*mz_jsmx.sl as je,mz_xm.zfbl,xmnm,mz_xm.zfxz,mz_xm.ghnm,mz_xm.CreateTime
                            from mz_xm
                            left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm on mz_xm.sfxm = xt_sfxm.sfxmCode and xt_sfxm.OrganizeId=@OrganizeId
                            left join NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl on mz_xm.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId 
                            inner join mz_gh on mz_xm.ghnm=mz_gh.ghnm and mz_gh.OrganizeId=@OrganizeId 
                            inner join mz_jsmx on mz_jsmx.mxnm = mz_xm.xmnm and mz_jsmx.jslx !='" + ((int)EnumJslx.GH).ToString() + @"' 
                            where mz_xm.OrganizeId=@OrganizeId 
                                and mz_jsmx.jsnm in (select * from dbo.f_split(@jsnmStr,','))
                         ");
            SqlParameter[] param =
            {
                new SqlParameter("@jsnmStr",jsnmStr),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            List<OutPatientReprintOrSuppPrintSettleDetailVO> list = this.FindList<OutPatientReprintOrSuppPrintSettleDetailVO>(strSql.ToString(), param);
            return list;
        }
        /// <summary>
        /// 门诊处方明细 jsnmStr
        /// </summary>
        /// <param name="jsnmStr"></param>
        /// <returns></returns>
        public List<OutPatientReprintOrSuppPrintSettleDetailVO> GetOutpatientPrescriptDetailList(string jsnmStr)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select mz_gh.kh,cfh,mz_cfmx.yp,ypmc,djdw,dlmc,dj,mz_cfmx.fwfdj,mz_jsmx.sl,mz_jsmx.sl * (dj+mz_cfmx.fwfdj) as je,mz_cfmx.zfbl,mz_cfmx.dl,mz_cfmx.zfxz,
                            mz_cf.ghnm,mz_cf.lyck,mz_cfmx.cls,mz_cfmx.czh,xt_yp.ypbzdm,mz_cfmx.cfnm,dj+mz_cfmx.fwfdj as djandfwfdj,mz_cf.patid,mz_cf.CreateTime
                            from mz_cfmx 
                            left join mz_cf on mz_cfmx.cfnm = mz_cf.cfnm and mz_cf.OrganizeId=@OrganizeId 
                            left join NewtouchHIS_Base..V_S_xt_yp xt_yp on mz_cfmx.yp = xt_yp.ypCode and xt_yp.OrganizeId=@OrganizeId 
                            left join NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl on mz_cfmx.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId 
                            inner join mz_jsmx on mz_jsmx.jslx !='" + ((int)EnumJslx.GH).ToString() + @"' and mz_jsmx.cf_mxnm = mz_cfmx.cfnm and mz_jsmx.OrganizeId=@OrganizeId 
                            inner join mz_gh on mz_cf.ghnm=mz_gh.ghnm and mz_gh.OrganizeId=@OrganizeId 
                            where mz_jsmx.jsnm in  (select * from dbo.f_split(@jsnmStr,','))
                                and mz_cfmx.OrganizeId=@OrganizeId 
                         ");
            SqlParameter[] param =
            {
                new SqlParameter("@jsnmStr",jsnmStr),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            List<OutPatientReprintOrSuppPrintSettleDetailVO> list = this.FindList<OutPatientReprintOrSuppPrintSettleDetailVO>(strSql.ToString(), param);
            return list;
        }
        /// <summary>
        /// 保存发票打印记录
        /// </summary>
        /// <param name="js"></param>
        /// <param name="fph"></param>
        /// <param name="xfph"></param>
        /// <param name="dyfs"></param>
        public void SaveInvoicePrintRecords(OutpatientSettlementEntity js, string fph, string xfph, Enumdyfs dyfs)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                OupatientInvoicePrintEntity entity = new OupatientInvoicePrintEntity();
                entity.dybh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt(OupatientInvoicePrintEntity.GetTableName());
                entity.jsnm = js.jsnm;
                entity.fph = fph;
                entity.xfph = xfph;
                entity.zje = js.zje;
                entity.jsrq = js.CreateTime;
                entity.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                entity.Create();
                entity.dyfs = ((int)dyfs).ToString();//1:补打  2:重打
                db.Insert(entity);
                db.Commit();
            }

        }

        /// <summary>
        /// 门诊处方明细 jsnm
        /// </summary>
        /// <param name="jsnmStr"></param>
        /// <returns></returns>
        public List<OutpatientPrescriptionDetailVO> GetOutpatientPrescriptDetailList(int jsnm)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select cfh,mz_cfmx.yp,ypmc,djdw,dlmc,(mz_cfmx.dj+mz_cfmx.fwfdj) dj,mz_cfmx.fwfdj,mz_jsmx.sl,mz_jsmx.sl * (mz_cfmx.dj+mz_cfmx.fwfdj) as je,mz_cfmx.zfbl,mz_cfmx.dl,mz_cfmx.zfxz,
                            mz_cf.ghnm,mz_cf.lyck,mz_cf.pt_cfnm,mz_cfmx.cls,czh,xt_yp.ypbzdm,mz_cfmx.cfnm,xt_ypsx.ypgg,xt_yp.nbdl,xt_ypsx.ybdm
                            from mz_cfmx 
                            left join mz_cf on mz_cfmx.cfnm = mz_cf.cfnm and mz_cf.OrganizeId=@OrganizeId 
                            left join NewtouchHIS_Base..V_S_xt_yp xt_yp on mz_cfmx.yp = xt_yp.ypCode and xt_yp.OrganizeId=@OrganizeId 
                            left join NewtouchHIS_Base..V_S_xt_sfsx xt_ypsx on mz_cfmx.yp = xt_ypsx.ypCode and xt_ypsx.OrganizeId=@OrganizeId 
                            left join NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl on mz_cfmx.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId 
                            inner join mz_jsmx on mz_jsmx.jslx !='" + ((int)EnumJslx.GH).ToString() + @"'  and mz_jsmx.cf_mxnm = mz_cfmx.cfnm and mz_jsmx.OrganizeId=@OrganizeId 
                            where mz_jsmx.jsnm =@jsnm and mz_cfmx.OrganizeId=@OrganizeId 
 
                         ");
            SqlParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            List<OutpatientPrescriptionDetailVO> list = this.FindList<OutpatientPrescriptionDetailVO>(strSql.ToString(), param);
            return list;
        }

        /// <summary>
        /// 门诊项目 jsnm
        /// </summary>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public List<OutpatienItemVO> GetOutpatientItemList(int jsnm)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select mz_xm.sfxm,sfxmmc,mz_xm.dl,dlmc,(mz_xm.dj +mz_xm.fwfdj) as dj,mz_xm.fwfdj,mz_jsmx.sl,(mz_xm.dj +mz_xm.fwfdj)*mz_jsmx.sl as je,mz_xm.zfbl,xmnm,mz_xm.zfxz,mz_xm.ghnm,xt_sfxm.wjdm,xt_sfxm.nbdlCode,xt_sfxm.ybdm,mz_xm.pt_xmnm as pt_cfnm  
                            from mz_xm
                            left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm on mz_xm.sfxm = xt_sfxm.sfxmCode and xt_sfxm.OrganizeId=@OrganizeId 
                            left join NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl on mz_xm.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId 
                            inner join mz_jsmx on mz_jsmx.mxnm = mz_xm.xmnm and mz_jsmx.jslx !='" + ((int)EnumJslx.GH).ToString() + @"' and mz_jsmx.OrganizeId=@OrganizeId 
                            where mz_jsmx.jsnm = @jsnm and mz_xm.OrganizeId=@OrganizeId 
                         ");
            SqlParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            List<OutpatienItemVO> list = this.FindList<OutpatienItemVO>(strSql.ToString(), param);
            return list;
        }

        /// <summary>
        /// 门诊挂号项目  jsnm
        /// </summary>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public List<OutpatientRegistItemVO> GetOutpatientRegItemList(int jsnm)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select mz_ghxm.sfxm,sfxmmc,mz_ghxm.dl,dlmc,(mz_ghxm.dj+mz_ghxm.fwfdj) as dj,mz_ghxm.fwfdj,sl,(mz_ghxm.dj+mz_ghxm.fwfdj)*mz_ghxm.sl as je,mz_ghxm.zfbl,xmnm,mz_ghxm.zfxz,mz_ghxm.ghnm,xt_sfxm.nbdlCode,xt_sfxm.ybdm,0 as pt_cfnm 
                            from mz_ghxm  with(nolock) 
                            left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm  with(nolock) on mz_ghxm.sfxm = xt_sfxm.sfxmCode  and xt_sfxm.OrganizeId=@OrganizeId 
                            left join (select * from NewtouchHIS_Base..V_S_xt_sfdl with(nolock) ) as xt_sfdl on mz_ghxm.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId  
                            where exists(select 1 from mz_jsmx with(nolock)  where jsnm = @jsnm and jslx ='" + ((int)EnumJslx.GH).ToString() + @"'  and mxnm = mz_ghxm.xmnm  and OrganizeId=@OrganizeId )  
                                and mz_ghxm.OrganizeId=@OrganizeId 
                            ");

            SqlParameter[] param =
                {
                    new SqlParameter("@jsnm",jsnm),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
                };

            List<OutpatientRegistItemVO> list = this.FindList<OutpatientRegistItemVO>(strSql.ToString(), param);
            return list;
        }

        /// <summary>
        /// 门诊结算支付方式
        /// </summary>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public List<OutpatientSettlementPaymentModelVO> GetSettlementPaymentModel(int jsnm)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                        select b.xjzffsmc,a.zfje 
                        from mz_jszffs a 
                        join xt_xjzffs b on a.xjzffs=b.xjzffs and b.OrganizeId=@OrganizeId 
                        where a.jsnm=@jsnm
                            and a.OrganizeId=@OrganizeId 
                    ");
            SqlParameter[] param =
            {
                    new SqlParameter("@jsnm",jsnm),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };

            List<OutpatientSettlementPaymentModelVO> list = this.FindList<OutpatientSettlementPaymentModelVO>(strSql.ToString(), param);
            return list;
        }

        /// <summary>
        /// 重打时变更mz_js和cw_fp的fph
        /// </summary>
        /// <param name="settlementEntity"></param>
        /// <param name="pageFPH"></param>
        public void UpdateInvoiceNo(OutpatientSettlementEntity settlementEntity, string pageFph)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //更新mz_js
                //保存变更日志老记录
                OutpatientSettlementEntity oldSettlementEntity = null;
                oldSettlementEntity = settlementEntity.Clone();
                settlementEntity.fph = pageFph;
                settlementEntity.LastModifierCode = OperatorProvider.GetCurrent().UserCode;
                settlementEntity.LastModifyTime = DateTime.Now;
                db.Update(settlementEntity);
                //保存变更日志
                if (oldSettlementEntity != null)
                {
                    AppLogger.WriteEntityChangeRecordLog(oldSettlementEntity, settlementEntity, OutpatientSettlementEntity.GetTableName(), oldSettlementEntity.jsnm.ToString());
                }
                //插入/更新cw_fp
                FinancialInvoiceEntity fpUpdateEntity, fpInsertEntity;
                _financialInvoiceEntityRepository.UpdateCurrentGetEntitys(pageFph, OperatorProvider.GetCurrent().UserCode, out fpUpdateEntity, out fpInsertEntity, OperatorProvider.GetCurrent().OrganizeId);
                if (fpUpdateEntity != null)
                {
                    db.Update(fpUpdateEntity);
                }
                if (fpInsertEntity != null)
                {
                    db.Insert(fpInsertEntity);
                }
                db.Commit();

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSettOperByOrg(string orgId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"select distinct CreatorCode from mz_js(nolock) where mz_js.OrganizeId=@OrganizeId");
            SqlParameter[] param =
            {
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            return FindList<string>(strSql.ToString(), param).ToList();
        }
        /// <summary>
        /// 获取角色 关联 用户（org）
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IList<RoleUnionUser> GetCurUserIdListRoleId(string roleId)
        {
            var sql = @"select distinct UserId First, b.OrganizeId Second 
from Sys_UserRole(nolock) a
left join Sys_Role(nolock) b
on a.RoleId = b.Id
where a.zt = '1'
and b.Id = @roleId";
            return this.FindList<RoleUnionUser>(sql, new SqlParameter[] {
                new SqlParameter("@roleId",roleId)
            });
        }
    }
}
