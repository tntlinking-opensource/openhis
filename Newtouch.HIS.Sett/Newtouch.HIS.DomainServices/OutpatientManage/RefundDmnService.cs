using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using Newtouch.Tools.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DbParameter = System.Data.Common.DbParameter;

namespace Newtouch.HIS.DomainServices
{
    public class RefundDmnService : DmnServiceBase, IRefundDmnService
    {
        private readonly ISysPatientBasicInfoRepo _sysPatBasicInfoRepository;
        private OperatorModel userModel => OperatorProvider.GetCurrent(); //获取当前登录用户对象
        private readonly IFinancialInvoiceRepo _financialInvoiceRepo;//发票号
        private readonly IOutpatientSettlementRepo _outPatientSettleRepo;
        private readonly ISysUserDmnService _sysUserDmnService;

        public RefundDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }
        //添加查询发票添加 时间的查询条件
        public string getWhere(string startTime, string endTime)
        {
            string where = "";
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                where = "MZ_JS.CreateTime > '" + startTime + "'";

                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    where = where + " and MZ_JS.CreateTime <'" + Convert.ToDateTime(endTime).AddDays(1) + "'";

                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    where = "MZ_JS.CreateTime < '" + endTime + "'";

                }
            }
            return where;
        }

        public List<MZJSInfo> GetFPHByKh(string kh, string startTime, string endTime, string fph)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                            select c.patid from mz_gh c 
                            left join xt_brjbxx d on c.patid=d.patid and d.OrganizeId=@OrganizeId
                            where kh=@kh and c.OrganizeId=@OrganizeId
                        ");
            SqlParameter[] param =
            {
                new SqlParameter("@kh",kh),
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };
            var result = this.FindList<int>(strSql.ToString(), param).ToList();
            var dt = new List<MZJSInfo>();
            string where = getWhere(startTime, endTime);
            if (result.Count > 0)
            {
                dt = GetMzJsJoinMzFpdy(result[0], fph, where);
                int patid = result[0];
                string sql = @"select xm from xt_brjbxx where patid=@patid and OrganizeId=@OrganizeId";
                SqlParameter[] para =
                {
                    new SqlParameter("@patid",patid),
                    new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
                };
                string name = this.FindList<string>(sql, para)[0];

                if (dt.Count > 0)
                {
                    dt.ForEach(p =>
                    {
                        p.xm = name;
                        p.kh = kh;
                    });
                }
                else
                {
                    throw new FailedCodeException("OUTPAT_NO_PATIENT_INFORMATION");
                }
            }
            else
            {
                throw new FailedCodeException("OUTPAT_REFUNG_FP_NULL");
            }

            return dt;
        }
        /// 结算关联发票打印
        /// </summary>
        /// <param name="brnm"></param>
        /// <param name="fph"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<MZJSInfo> GetMzJsJoinMzFpdy(int PATID, string fph, string where)
        {
            StringBuilder sqlBuilder = new StringBuilder("SELECT '' as xm,'' as kh,MZ_JS.JSNM,PATID,BRXZBH,BRXZ,JSLX,MZ_JS.ZJE,ZLFY,ZFFY,FLZFFY,JZFY,XJZF,XJWC,");
            sqlBuilder.AppendLine("ZHJZ,XJZFFS,ISNULL(MZ_FPDY.XFPH,MZ_JS.FPH) AS FPH,JSZT,CXJSNM,ZH,MZ_JS.CreatorCode,MZ_JS.CreateTime,FPDM,JMBL,JMJE,JYLX,YSK,ZL,XJWC,MZ_JS.FPH as oldFPH FROM MZ_JS WITH(NOLOCK)");
            sqlBuilder.AppendLine("LEFT JOIN ( ");
            sqlBuilder.AppendLine("    SELECT * FROM MZ_FPDY AS A_SET WITH(NOLOCK) WHERE CreateTime = (SELECT MAX(CreateTime) FROM MZ_FPDY AS B_SET WITH(NOLOCK) WHERE B_SET.JSNM = A_SET.JSNM and B_SET.OrganizeId=@OrganizeId) and A_SET.OrganizeId=@OrganizeId ");
            //sqlBuilder.AppendLine(") AS MZ_FPDY ON MZ_JS.FPH = MZ_FPDY.FPH ");    //修改成结算内码
            sqlBuilder.AppendLine(") AS MZ_FPDY ON MZ_JS.JSNM = MZ_FPDY.JSNM ");
            //sqlBuilder.AppendLine(string.Format("WHERE BRNM = {0} AND LEN(ISNULL(MZ_JS.FPH,'')) > 0 AND JSZT <> 2 ", brnm));
            sqlBuilder.AppendLine(string.Format("WHERE PATID = {0}  AND JSZT <> 2 ", PATID));     //无发票也可以退费。
            sqlBuilder.AppendLine(" AND  NOT EXISTS  (select 1 FROM mz_js b WITH(NOLOCK) WHERE b.jszt='2' and b.cxjsnm=MZ_JS.jsnm and b.OrganizeId=@OrganizeId ) and MZ_JS.OrganizeId=@OrganizeId");      //已经退的发票在退费界面不再显示


            if (!string.IsNullOrWhiteSpace(where))
            {

                sqlBuilder.Append(string.Format("AND {0} ", where));
            }
            sqlBuilder.Append(" ORDER BY MZ_JS.CreateTime DESC ");

            SqlParameter[] param =
            {
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };

            if (!string.IsNullOrWhiteSpace(fph))
            {
                return this.FindList<MZJSInfo>(sqlBuilder.ToString(), param).Where(p => p.fph.Contains(fph)).ToList();
            }
            return this.FindList<MZJSInfo>(sqlBuilder.ToString(), param).ToList();

        }

        public List<MZJS> GetMZJSByJsnm(int jsnm)
        {

            StringBuilder sqlBuilder = new StringBuilder(" select top 1 patid,mz_ghxm.brxz,brxzmc,mz_ghxm.ks ks,[NewtouchHIS_Base]..V_S_Sys_Department.Name ksmc,ys,xtry.name rymc,ghnm from mz_ghxm  with(nolock)  ");
            sqlBuilder.AppendLine(" left join (select * from xt_brxz  with(nolock)  where zt = 1 ) as xt_brxz on mz_ghxm.brxz = xt_brxz.brxz and xt_brxz.OrganizeId=@OrganizeId");
            sqlBuilder.AppendLine(" left join [NewtouchHIS_Base]..V_S_Sys_Staff xtry on mz_ghxm.ys = xtry.gh and xtry.OrganizeId=@OrganizeId left join [NewtouchHIS_Base]..V_S_Sys_Department on mz_ghxm.ks = [NewtouchHIS_Base]..V_S_Sys_Department.Code and [NewtouchHIS_Base]..V_S_Sys_Department.OrganizeId = @OrganizeId where exists ( ");
            sqlBuilder.AppendLine(" select mxnm from mz_jsmx  with(nolock)  where exists(select * from mz_js where ");
            sqlBuilder.AppendLine(" jsnm = @jsnm and jslx='0' and mz_js.jsnm = mz_jsmx.jsnm and mz_js.OrganizeId=@OrganizeId) ");        //发票号为空，用结算内码查询
            sqlBuilder.AppendLine(" and mxnm = mz_ghxm.xmnm) ");
            sqlBuilder.AppendLine(" And mz_ghxm.OrganizeId = @OrganizeId ");

            sqlBuilder.AppendLine(" union ");
            sqlBuilder.AppendLine(" select top 1 patid,mz_xm.brxz,brxzmc,mz_xm.ks,[NewtouchHIS_Base]..V_S_Sys_Department.Name ksmc,ys,xtry.name rymc,ghnm from mz_xm   with(nolock)  ");
            sqlBuilder.AppendLine(" left join (select * from xt_brxz  with(nolock)  where zt = 1 ) as xt_brxz  ");
            sqlBuilder.AppendLine(" on mz_xm.brxz = xt_brxz.brxz and xt_brxz.OrganizeId = @OrganizeId");
            sqlBuilder.AppendLine(" left join [NewtouchHIS_Base]..V_S_Sys_Staff xtry on mz_xm.ys = xtry.gh and xtry.OrganizeId = @OrganizeId ");
            sqlBuilder.AppendLine(" left join [NewtouchHIS_Base]..V_S_Sys_Department on mz_xm.ks = [NewtouchHIS_Base]..V_S_Sys_Department.Code and [NewtouchHIS_Base]..V_S_Sys_Department.OrganizeId = @OrganizeId ");
            sqlBuilder.AppendLine(" where exists ( select mxnm from mz_jsmx  with(nolock)  where exists( select * from mz_js where jsnm = @jsnm and jslx !='0' and mz_js.jsnm = mz_jsmx.jsnm and mz_js.OrganizeId=@OrganizeId) and mxnm = mz_xm.xmnm) and mz_xm.OrganizeId=@OrganizeId ");//发票号为空，用结算内码查询
            sqlBuilder.AppendLine(" union ");
            sqlBuilder.AppendLine(" select top 1 patid,mz_cf.brxz,brxzmc,mz_cf.ks,[NewtouchHIS_Base]..V_S_Sys_Department.Name ksmc,ys,xtry.name rymc,ghnm from mz_cfmx  with(nolock) ");
            sqlBuilder.AppendLine(" left join mz_cf  with(nolock) on mz_cfmx.cfnm = mz_cf.cfnm and mz_cf.OrganizeId = @OrganizeId  ");
            sqlBuilder.AppendLine(" left join (select * from xt_brxz  with(nolock) where zt = 1) as xt_brxz on mz_cf.brxz = xt_brxz.brxz and xt_brxz.OrganizeId = @OrganizeId ");
            sqlBuilder.AppendLine(" left join [NewtouchHIS_Base]..V_S_Sys_Staff xtry on mz_cf.ys = xtry.gh and xtry.OrganizeId = @OrganizeId ");
            sqlBuilder.AppendLine(" left join [NewtouchHIS_Base]..V_S_Sys_Department on mz_cf.ks = [NewtouchHIS_Base]..V_S_Sys_Department.Code and [NewtouchHIS_Base]..V_S_Sys_Department.OrganizeId = @OrganizeId  ");
            sqlBuilder.AppendLine(" where exists ( select cf_mxnm from mz_jsmx  with(nolock)  where exists( select * from mz_js  with(nolock) where jsnm = @jsnm and jslx !='0'  and mz_js.jsnm = mz_jsmx.jsnm and mz_js.OrganizeId = @OrganizeId ) and mxbm = mz_cfmx.yp and cf_mxnm =mz_cfmx.cfnm) ");//发票号为空，用结算内码查询    
            SqlParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm.ToString()),
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };

            var result = this.FindList<MZJS>(sqlBuilder.ToString(), param);


            return result;
        }
        /// <summary>
        /// 按结算内码查门诊项目
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <returns></returns>
        /// 门诊项目明细
        public List<GridViewMx> GetMz_xmRecordsByJsnm(int jsnm)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
                            select 'mz_xm' as InfoSet,'' as CHUFANGHAO,'' as DANWEI,'1' as IS_RETURN, '0' as czh, mz_xm.sfxm,sfxmmc,mz_xm.dl,dlmc,(mz_xm.dj + mz_xm.fwfdj) as dj,mz_xm.fwfdj,mz_jsmx.sl,(mz_xm.dj + mz_xm.fwfdj) * mz_jsmx.sl as je,mz_xm.zfbl,xmnm,mz_xm.zfxz,mz_xm.ghnm,xt_sfxm.wjdm,xt_sfxm.nbdlCode as nbdl,xt_sfxm.ybdm,mz_xm.pt_xmnm as pt_cfnm
                            from mz_xm
                            left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm on mz_xm.sfxm = xt_sfxm.sfxmCode and xt_sfxm.OrganizeId = @OrganizeId
                            left join NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl on mz_xm.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId = @OrganizeId
                            inner join mz_jsmx on mz_jsmx.mxnm = mz_xm.xmnm and mz_jsmx.jslx != '0' and mz_jsmx.OrganizeId = @OrganizeId
                            where mz_jsmx.jsnm = @jsnm and mz_xm.OrganizeId=@OrganizeId
                        ");

            SqlParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };

            var result = this.FindList<GridViewMx>(sqlBuilder.ToString(), param);

            //string strSQL = @"
            //                select mz_xm.sfxm,sfxmmc,mz_xm.dl,dlmc,(mz_xm.dj + mz_xm.fwfdj) as dj,mz_xm.fwfdj,mz_jsmx.sl,(mz_xm.dj + mz_xm.fwfdj) * mz_jsmx.sl as je,mz_xm.zfbl,xmnm,mz_xm.zfxz,mz_xm.ghnm,xt_sfxm.wjdm,xt_sfxm.nbdlCode,xt_sfxm.ybdm,mz_xm.pt_xmnm as pt_cfnm
            //                from mz_xm
            //                left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm on mz_xm.sfxm = xt_sfxm.sfxmCode 
            //                left join NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl on mz_xm.dl = xt_sfdl.dlCode 
            //                inner join mz_jsmx on mz_jsmx.mxnm = mz_xm.xmnm and mz_jsmx.jslx != '0' 
            //                where mz_jsmx.jsnm = '" + jsnm + "'";

            //var data = DbHelper.ExecuteSqlCommand2(strSQL);
            //var result = new List<GridViewMx>();
            //if (data.Rows.Count > 0)
            //{

            //    for (int i = 0; i < data.Rows.Count; i++)
            //    {
            //        var dr = data.Rows[i];
            //        var refund = new GridViewMx();
            //        refund.tag = dr;
            //        refund.InfoSet = "mz_xm";
            //        refund.DL = string.Format("{0}", dr["DL"]);
            //        refund.CHUFANGHAO = string.Empty;
            //        refund.DLMC = string.Format("{0}", dr["DLMC"]);
            //        refund.YPMC = string.Format("{0}", dr["SFXMMC"]);
            //        refund.DANWEI = string.Empty;
            //        refund.DJ = string.Format("{0}", dr["DJ"]);
            //        refund.ZFBL = string.Format("{0}", dr["ZFBL"]);
            //        refund.SL = string.Format("{0}", dr["SL"]);
            //        refund.RETURNS_SL = Tool.AnyToInt32(dr["SL"]);
            //        refund.JE = string.Format("{0}", Tool.AnyToDecimal(dr["JE"]).ToString("0.00"));
            //        refund.IS_RETURN = "1";
            //        refund.czh = "0";
            //        refund.SFXM = string.Format("{0}", dr["SFXM"]);
            //        // refund.CF_MXNM= 0;
            //        refund.FWFDJ = Tool.AnyToString(dr["FWFDJ"]);
            //        //fangyi  20160802  开始  添加医生站的处方内码
            //        //   refund.pt_cfnm= Tool.AnyToInt32(dr["pt_cfnm"]);
            //        result.Add(refund);
            //    }
            //}
            return result;
        }
        /// <summary>
        /// 按结算内码查挂号项目
        /// </summary>
        /// <param name="jsnm">结算内码</param> 
        /// <returns></returns>
        /// 门诊挂号项目明细
        public List<GridViewMx> GetMz_ghxmRecordsByJsnm(int jsnm)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
                                SELECT 
                                        'mz_ghxm' as InfoSet,
                                        '' as CHUFANGHAO,
                                        '' as DANWEI,
                                        case mz_ghxm.dl when '55' then 0 else sl end as RETURNS_SL,
                                        case mz_ghxm.dl when '55' then '0' else '1' end as IS_RETURN,
                                        '0' as czh,
                                        mz_ghxm.sfxm,
                                        sfxmmc,
                                        mz_ghxm.dl,
                                        dlmc,
                                        convert(varchar(50),(mz_ghxm.dj+mz_ghxm.fwfdj)) AS dj,
                                        convert(varchar(50),mz_ghxm.fwfdj) fwfdj,
                                        convert(varchar(50),sl) sl,
                                        convert(varchar(50),(mz_ghxm.dj+mz_ghxm.fwfdj)*mz_ghxm.sl) AS je,
                                        convert(varchar(50),mz_ghxm.zfbl) zfbl,
                                        xmnm,
                                        mz_ghxm.zfxz,
                                        mz_ghxm.ghnm,
                                        xt_sfxm.nbdlCode as nbdl,
                                        xt_sfxm.ybdm,
                                        0 AS pt_cfnm
                                FROM mz_ghxm with(nolock)
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm with(nolock)
                                    ON mz_ghxm.sfxm = xt_sfxm.sfxmCode and xt_sfxm.OrganizeId=@OrganizeId
                                LEFT JOIN 
                                    (SELECT *
                                    FROM NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl with(nolock) ) AS xt_sfdl
                                    ON mz_ghxm.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId
                                WHERE exists
                                    (SELECT 1
                                    FROM mz_jsmx with(nolock)
                                    WHERE jsnm = @jsnm
                                            AND jslx ='0'
                                            AND mxnm = mz_ghxm.xmnm and OrganizeId=@OrganizeId) 
                                    and mz_ghxm.OrganizeId=@OrganizeId
                            ");

            SqlParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };

            var result = this.FindList<GridViewMx>(sqlBuilder.ToString(), param);




            //List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            //paramList.Add(new KeyValuePair<string, string>("@jsnm", string.Format("{0}", jsnm)));

            //StringBuilder sqlBuilder = new StringBuilder("select mz_ghxm.sfxm,sfxmmc,mz_ghxm.dl,dlmc,(mz_ghxm.dj+mz_ghxm.fwfdj) as dj,mz_ghxm.fwfdj,sl,(mz_ghxm.dj+mz_ghxm.fwfdj)*mz_ghxm.sl as je,mz_ghxm.zfbl,xmnm,mz_ghxm.zfxz,mz_ghxm.ghnm,xt_sfxm.nbdlCode,xt_sfxm.ybdm,0 as pt_cfnm from mz_ghxm  with(nolock) ");
            //sqlBuilder.Append("left join NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm  with(nolock) on mz_ghxm.sfxm = xt_sfxm.sfxmCode left join ( ");
            //sqlBuilder.Append("select * from NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl  with(nolock) ) as xt_sfdl on mz_ghxm.dl = xt_sfdl.dlCode ");
            //sqlBuilder.Append("where exists(select 1 from mz_jsmx with(nolock)  where jsnm = @jsnm and jslx ='0'  and mxnm = mz_ghxm.xmnm) ");

            //var data = DbHelper.GetDataTableBySql(sqlBuilder.ToString(), paramList);
            //var result = new List<GridViewMx>();
            //if (data.Rows.Count > 0)
            //{
            //    for (int i = 0; i < data.Rows.Count; i++)
            //    {
            //        var dr = data.Rows[i];
            //        var refund = new GridViewMx();
            //        refund.tag = dr;
            //        refund.InfoSet = "mz_ghxm";
            //        refund.DL = string.Format("{0}", dr["DL"]);
            //        refund.CHUFANGHAO = string.Empty;
            //        refund.DLMC = string.Format("{0}", dr["DLMC"]);
            //        refund.YPMC = string.Format("{0}", dr["SFXMMC"]);
            //        refund.DANWEI = string.Empty;
            //        refund.DJ = string.Format("{0}", dr["DJ"]);
            //        refund.ZFBL = string.Format("{0}", dr["ZFBL"]);
            //        refund.SL = string.Format("{0}", dr["SL"]);
            //        refund.JE = string.Format("{0}", Tool.AnyToDecimal(dr["JE"]).ToString("0.00"));
            //        if (Tool.AnyToString(dr["DL"]) == "55")  //磁卡费默认不退
            //        {
            //            refund.RETURNS_SL = 0;
            //            refund.IS_RETURN = "0";
            //        }
            //        else
            //        {
            //            refund.RETURNS_SL = Tool.AnyToInt32(dr["SL"]);
            //            refund.IS_RETURN = "1";
            //        }
            //        refund.czh = "0";
            //        refund.SFXM = string.Format("{0}", dr["SFXM"]);
            //        //  refund.CF_MXNM = 0;
            //        refund.FWFDJ = Tool.AnyToString(dr["FWFDJ"]);
            //        //fangyi  20160802  开始  添加医生站的处方内码
            //        // refund.pt_cfnm = Tools.AnyToInt32(dr["pt_cfnm"]);
            //        //fangyi  20160802  结束
            //        result.Add(refund);
            //    }
            //}
            return result;
        }
        /// 按结算内码查处方明细
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <returns></returns>
        /// 门诊处方明细
        public DataTable GetMz_cfmxRecords(int jsnm)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
                                SELECT 
                                        'mz_cfmx' as InfoSet,
                                        '1' as IS_RETURN,
                                        mz_cf.cfnm CF_MXNM,
                                        cfh as CHUFANGHAO,
                                        mz_cfmx.yp as SFXM,
                                        ypmc,
                                        djdw as DANWEI,
                                        dlmc,
                                        convert(varchar(50),(mz_cfmx.dj+mz_cfmx.fwfdj)) dj,
                                        convert(varchar(50),mz_cfmx.fwfdj) fwfdj,
                                        convert(varchar(50),mz_jsmx.sl) sl,
                                        convert(varchar(50),(mz_jsmx.sl * (mz_cfmx.dj+mz_cfmx.fwfdj)))AS je,
                                        convert(varchar(50),mz_cfmx.zfbl) zfbl,
                                        mz_cfmx.dl,
                                        mz_cfmx.zfxz,
                                         mz_cf.ghnm,
                                        mz_cf.lyck,
                                        mz_cf.pt_cfnm,
                                        mz_cfmx.cls,
                                        xt_yp.ypbzdm,
                                        mz_cfmx.cfnm,
                                        mz_cfmx.czh,
                                        xt_ypsx.ypgg,
                                        xt_yp.nbdl,
                                        xt_ypsx.ybdm
                                FROM mz_cfmx
                                LEFT JOIN mz_cf
                                    ON mz_cfmx.cfnm = mz_cf.cfnm and mz_cf.OrganizeId=@OrganizeId
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_yp xt_yp
                                    ON mz_cfmx.yp = xt_yp.ypCode and xt_yp.OrganizeId=@OrganizeId
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_ypsx xt_ypsx
                                    ON mz_cfmx.yp = xt_ypsx.ypCode and xt_ypsx.OrganizeId=@OrganizeId
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl
                                    ON mz_cfmx.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId
                                INNER JOIN mz_jsmx
                                    ON mz_jsmx.jslx !='0' and mz_jsmx.OrganizeId=@OrganizeId
                                        --AND mz_jsmx.mxbm = mz_cfmx.yp
                                        AND mz_jsmx.cf_mxnm = mz_cfmx.cfnm
                                WHERE mz_jsmx.jsnm =@jsnm
                                And mz_cfmx.OrganizeId = @OrganizeId
                            ");

            SqlParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };


            var result = this.FindList<GridViewMx>(sqlBuilder.ToString(), param);
            var data = result.ToDataTable();


            return data;
        }
        public List<GridViewMx> GetMz_cfmxRecordsByJsnm(int jsnm)
        {

            var data = GetMz_cfmxRecords(jsnm);
            var result = new List<GridViewMx>();
            if (data.Rows.Count > 0)
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    var dr = data.Rows[i];
                    var refund = new GridViewMx();
                    refund.tag = dr;
                    refund.InfoSet = "mz_cfmx";
                    refund.DL = string.Format("{0}", dr["DL"]);
                    refund.CHUFANGHAO = string.Format("{0}", dr["CHUFANGHAO"]);
                    refund.DLMC = string.Format("{0}", dr["DLMC"]);
                    refund.YPMC = string.Format("{0}", dr["YPMC"]);
                    refund.DANWEI = string.Format("{0}", dr["DANWEI"]);
                    refund.DJ = string.Format("{0}", dr["DJ"]);
                    refund.ZFBL = string.Format("{0}", dr["ZFBL"]);
                    refund.SL = string.Format("{0}", dr["SL"]);
                    refund.RETURNS_SL = dr["SL"].ToDecimal();
                    refund.JE = string.Format("{0}", dr["JE"].ToDecimal().ToString("0.00"));
                    refund.IS_RETURN = "1";
                    refund.czh = string.Format("{0}", dr["czh"]);
                    refund.SFXM = string.Format("{0}", dr["SFXM"]);
                    refund.CF_MXNM = !string.IsNullOrWhiteSpace(dr["CF_MXNM"].ToString()) ? dr["CF_MXNM"].ToInt() : 0;
                    refund.FWFDJ = dr["FWFDJ"].ToString();
                    refund.NBDL = string.Format("{0}", dr["NBDL"]);
                    refund.GHNM = dr["GHNM"].ToInt();
                    result.Add(refund);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据病人内码获取除挂号之外的剩余数量
        /// </summary>
        /// <param name="brnm"></param>
        /// <returns></returns>
        public decimal getSysl(int patid, int ghnm)
        {
            string strSQL = string.Format(@"
select isnull(sum(sysl),0) as sysl
from
(
	select case when jszt = 2 then -a.sl else a.sl end as sysl
	from mz_jsmx a
	inner join mz_js js on a.jsnm = js.jsnm and js.OrganizeId='{2}'
	where exists ( select * from mz_xm xm where xm.ghnm={1} and a.mxnm = xm.xmnm and xm.OrganizeId='{2}')
	and js.patid = {0} and js.jslx <> '0'
    and a.OrganizeId='{2}'

	union

	select case when jszt = 2 then -a.sl else a.sl end as sysl
	from mz_jsmx a
	inner join mz_js js on a.jsnm = js.jsnm and js.OrganizeId='{2}'
	where exists ( select * from mz_cf cf inner join mz_cfmx mx on cf.cfnm = mx.cfnm where cf.ghnm={1} and a.cf_mxnm = mx.cfnm and mx.OrganizeId='{2}' and cf.OrganizeId='{2}')
	and js.patid = {0} and js.jslx <> '0'
    and a.OrganizeId='{2}'
)a
", patid.ToString(), ghnm.ToString(), OperatorProvider.GetCurrent().OrganizeId);

            DataTable dt = DbHelper.ExecuteSqlCommand2(strSQL);
            if (dt == null || dt.Rows.Count == 0)
                return 0;
            else
                return Convert.ToDecimal(dt.Rows[0]["sysl"].ToString());
        }

        /// <summary>
        /// 门诊结算支付方式
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <returns></returns>
        public DataTable GetMzJszffsForJsnm(int jsnm)
        {
            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("@jsnm", string.Format("{0}", jsnm)));
            paramList.Add(new KeyValuePair<string, string>("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId));
            StringBuilder sqlBuilder = new StringBuilder("SELECT ROW_NUMBER()OVER(ORDER BY XJZFFSMC) AS NUM, MZ_JSZFFS.*,XJZFFSMC  FROM MZ_JSZFFS ");
            sqlBuilder.Append("LEFT JOIN XT_XJZFFS ON MZ_JSZFFS.XJZFFS = XT_XJZFFS.XJZFFS ");
            sqlBuilder.Append("WHERE XT_XJZFFS.ZT = '1' AND MZ_JSZFFS.JSNM = @jsnm and MZ_JSZFFS.OrganizeId=@OrganizeId ");
            return DbHelper.GetDataTableBySql(sqlBuilder.ToString(), paramList);
        }
        /// <summary>
        /// 获取当前挂号信息的挂号结算及结算明细信息
        /// </summary>
        /// <param name="brnm"></param>
        /// <returns></returns>
        public List<OutpatientRegistNonAttendanceEntity> getGhjsByGhnm(int tmpGhnm)
        {
            List<KeyValuePair<string, string>> paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("@ghnm", string.Format("{0}", tmpGhnm)));
            paramList.Add(new KeyValuePair<string, string>("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId));

            string strSQL = @"
select a.*,c.sfxm,b.jsmxnm from mz_js a 
inner join mz_jsmx b on a.jsnm = b.jsnm and b.OrganizeId=@OrganizeId
inner join mz_ghxm c on b.mxnm = c.xmnm and c.OrganizeId=@OrganizeId 
where c.ghnm =@ghnm and b.jslx = '0'
and a.OrganizeId=@OrganizeId
";
            var data = DbHelper.GetDataTableBySql(strSQL.ToString(), paramList);
            var mz_ghthList = new List<OutpatientRegistNonAttendanceEntity>();
            if (data.Rows.Count > 0)
            {
                foreach (DataRow mxDr in data.Rows)
                {
                    OutpatientRegistNonAttendanceEntity th = new OutpatientRegistNonAttendanceEntity();
                    th.thnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_ghth");
                    th.ghnm = tmpGhnm;
                    th.jsnm = Convert.ToInt32(mxDr["jsnm"].ToString());
                    th.jsmxnm = Convert.ToInt32(mxDr["jsmxnm"].ToString());
                    th.thry = userModel.UserCode;
                    th.Create();
                    mz_ghthList.Add(th);
                }
            }
            return mz_ghthList;
        }

        public SysPatientNatureEntity GetBrxzByBrbh(string brxz)
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<SysPatientNatureEntity>().FirstOrDefault(p => p.brxz == brxz && p.OrganizeId == OrganizeId);
        }
        /// <summary>
        /// 根据首拼查询收费项目
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public List<SysPatientChargeWaiverEntity> GetXT_brsfjm(string parmBrxz)
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<SysPatientChargeWaiverEntity>().Where(p => p.zt == "1" && p.brxz == parmBrxz && p.OrganizeId == OrganizeId).ToList();

        }

        /// 根据挂号内码获取挂号信息
        public List<OutpatientRegistEntity> getGhs(List<int> ghnmList)
        {
            return _dataContext.Set<OutpatientRegistEntity>().Where(p => (ghnmList.Contains(p.ghnm))).ToList();

        }
        //根据病人内码获取病人信息
        public SysPatientBasicInfoEntity GetBrjbxxByPatid(int patid)
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<SysPatientBasicInfoEntity>().FirstOrDefault(p => p.patid == patid && p.OrganizeId == OrganizeId);

        }
        ///根据表名获取主键
        public int GetPrimaryKeyByTableName(string tableName)
        {
            return EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt(tableName);

        }

        public List<SysPatientChargeAlgorithmEntity> getMzActive()
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<SysPatientChargeAlgorithmEntity>().Where(p => p.zt == "1" && (p.mzzybz == "0" || p.mzzybz == "1") && p.OrganizeId == OrganizeId).ToList();
        }



        // public List<SysPatientBasicInfoEntity> GetXt_brjbxxByKh(string kh)
        // {
        //     StringBuilder strSql = new StringBuilder();
        //     strSql.Append(@"
        //select c.kh,c.patid,d.xm from mz_gh c left join  xt_brjbxx d on c.patid=d.patid where kh=@kh");
        //     SqlParameter[] param =
        //  {
        //         new SqlParameter("@kh",kh)
        //     };
        //     var result = this.FindList<SysPatientBasicInfoEntity>(strSql.ToString(), param);
        //     return result;
        // }



        //            --是否重复退费
        //select count(jsnm) from mz_js b where  b.jszt='2' 
        // and b.cxjsnm = 1621 jsnm
        // and b.fph = '1000001813'
        public int getTFForFPH(int jsnm, string fph)
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<OutpatientSettlementEntity>().Where(p => p.cxjsnm == jsnm && p.fph == fph && p.jszt == 2 && p.OrganizeId == OrganizeId).ToList().Count;
        }
        public List<OutpatientSettlementDetailEntity> getMzJsMx(string jslx, int jsnm)
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<OutpatientSettlementDetailEntity>().Where(p => p.jslx == jslx && p.jsnm == jsnm && p.OrganizeId == OrganizeId).OrderBy(p => p.jsmxnm).ToList();
        }
        public List<OutpatientSettlementPaymentModelEntity> GetJszffsByJsnm(int jsnm)
        {
            var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
            return _dataContext.Set<OutpatientSettlementPaymentModelEntity>().Where(p => p.jsnm == jsnm && p.OrganizeId == OrganizeId).ToList();
        }

        public OutpatientSettlementEntity GetMZJSFromMz_jsByJsnm(int jsnm)
        {
            return _outPatientSettleRepo.SelectMZJS(jsnm, OperatorProvider.GetCurrent().OrganizeId);
        }

        public List<SysPatientAccountRevenueAndExpenseEntity> getInsertSqlSzjlList(List<SysPatientAccountRevenueAndExpenseEntity> szjlList)
        {
            var szjlListData = new List<SysPatientAccountRevenueAndExpenseEntity>();

            foreach (SysPatientAccountRevenueAndExpenseEntity t in szjlList)
            {
                var OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                var data2 = _dataContext.Set<SysPatientAccountEntity>().FirstOrDefault(p => p.zh == t.zh && p.OrganizeId == OrganizeId).zhye;
                if (data2 != -1)
                {
                    var data3 = _dataContext.Set<SysPatientAccountRevenueAndExpenseEntity>().Where(p => p.zh == t.zh && p.OrganizeId == OrganizeId).OrderByDescending(p => p.zhszjlbh).ToList()[0].zhye;
                    if (data3 + t.szje < 0)
                    {

                        throw new FailedCodeException("余额不足，请重新支付！");
                    }
                    else
                    {
                        data2 = data2 + data3;
                    }
                }
                else
                {
                    data2 = _dataContext.Set<SysPatientAccountRevenueAndExpenseEntity>().Where(p => p.zh == t.zh && p.OrganizeId == OrganizeId).OrderByDescending(p => p.zhszjlbh).ToList()[0].zhye;

                }
                t.zhye = data2;
                szjlListData.Add(t);
            }
            return szjlListData;
        }
        public bool CheckFPH(string fph)
        {

            bool result = true;
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (ExistsForInvoiceNo(fph))
                {

                    FinancialInvoiceEntity fpUpdateEntity, fpInsertEntity;
                    _financialInvoiceRepo.UpdateCurrentGetEntitys(fph, userModel.UserCode, out fpUpdateEntity, out fpInsertEntity, OperatorProvider.GetCurrent().OrganizeId);


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
                else
                {
                    result = false;
                }


            }
            return result;
        }

        public bool ExistsForInvoiceNo(string invoiceNo)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"
                           select 1 from 
                            (
                            select top 1 fph from zy_js where fph = @invoiceNo and OrganizeId=@OrganizeId
                            union 
                            select top 1 fph from mz_js where fph = @invoiceNo and OrganizeId=@OrganizeId
                            ) t
                        ");
            SqlParameter[] param =
            {
                new SqlParameter("@invoiceNo",invoiceNo),
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };

            return this.FindList<int>(strSql.ToString(), param).Any();
        }
        /// <summary>
        /// 给门诊结算信息加上病人信息
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        public OutpatientSettlementEntity SetPatInfo(OutpatientSettlementEntity js)
        {
            var info = _sysPatBasicInfoRepository.GetInfoByPatid(js.patid.ToString(), OperatorProvider.GetCurrent().OrganizeId);
            if (info != null)
            {
                js.xm = info.xm;
                js.xb = info.xb;
                js.blh = info.blh;
                js.csny = info.csny;
                js.zjlx = info.zjlx;
                js.zjh = info.zjh;
            }
            else
            {
                js.xm = "病人信息不存在";
                js.xb = "1";
                js.blh = "病人信息不存在";
            }
            return js;
        }

        /// <summary>
        /// 全退  mz_js mz_jsmx mz_ghth
        /// </summary>
        /// <param name="mzJs"></param>
        /// <param name="mzJsmxList"></param>
        /// <param name="ghjsDt"></param>
        /// <returns></returns>
        public bool mzJsFullBack(OutpatientSettlementEntity mzJs, List<OutpatientSettlementDetailEntity> mzJsmxList, List<OutpatientRegistNonAttendanceEntity> ghjsDt = null)
        {
            bool result = true;
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                try
                {

                    #region 门诊结算
                    var jsnmNew = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_js");
                    mzJs.CreatorCode = userModel.UserCode;
                    mzJs.CreateTime = DateTime.Now;
                    mzJs.jsnm = jsnmNew;
                    db.Insert(SetPatInfo(mzJs));
                    AppLogger.Info("***********************************************************");
                    AppLogger.Info("门诊结算 mzJs.jsnm: " + mzJs.jsnm.ToString());
                    foreach (var item in mzJsmxList)
                    {
                        item.jsnm = jsnmNew;
                        item.CreatorCode = userModel.UserCode;
                        item.CreateTime = DateTime.Now;
                        item.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                        item.jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx");
                        db.Insert(item);
                        AppLogger.Info("结算明细 item.jsmxnm: " + item.jsmxnm.ToString());
                    }
                    #endregion

                    #region mz_ghth 门诊退号
                    //GRS退费不需要退号
                    if (ghjsDt.Count > 0 && ghjsDt != null)
                    {
                        foreach (OutpatientRegistNonAttendanceEntity ghjs in ghjsDt)
                        {
                            ghjs.CreatorCode = userModel.UserCode;
                            ghjs.CreateTime = DateTime.Now;
                            ghjs.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                            db.Insert(ghjs);
                            AppLogger.Info("ghjs挂号: " + ghjs.thnm.ToString());

                        }
                    }
                    #endregion

                    db.Commit();
                }
                catch (Exception ex)
                {
                    throw new FailedCodeException(ex.Message);
                }
            }
            AppLogger.Info("result：" + result);
            return result;
        }

        /// <summary>
        /// 插入退费剩余部分 mz_js mz_jsmx mz_jszffs mz_jsdl 数据库操作
        /// </summary>
        /// <param name="js"></param>
        /// <param name="listMzJsMx"></param>
        /// <param name="jszffsArray"></param>
        /// <param name="jsdlList"></param>
        /// <returns></returns>
        public bool InsertRemainFromReturns(OutpatientSettlementEntity js, List<OutpatientSettlementDetailEntity> listMzJsMx, List<OutpatientSettlementPaymentModelEntity> jszffsArray, List<OutpatientSettlementCategoryEntity> jsdlList)
        {
            bool result = true;
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                try
                {
                    AppLogger.Info("***********************************************************");
                    AppLogger.Info("插入退费剩余部分");

                    #region 门诊结算
                    var mzjs = SetPatInfo(js);
                    mzjs.CreatorCode = userModel.UserCode;
                    mzjs.CreateTime = DateTime.Now;
                    var jsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_js");
                    mzjs.jsnm = jsnm;
                    mzjs.zt = "1";
                    mzjs.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                    mzjs.ghnm = js.ghnm;
                    db.Insert(mzjs);
                    #endregion
                    AppLogger.Info("门诊结算 mzJs.jsnm: " + mzjs.jsnm);
                    #region 结算明细
                    foreach (OutpatientSettlementDetailEntity item in listMzJsMx)
                    {
                        item.jsnm = jsnm;
                        item.CreatorCode = userModel.UserCode;
                        item.CreateTime = DateTime.Now;
                        item.zt = "1";
                        item.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                        item.jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx");
                        db.Insert(item);
                        AppLogger.Info("结算明细 item.jsmxnm: " + item.jsmxnm);

                        //  sqlStrList.Add(DAL_mz_jsmx.Instance.getInserSql(item));
                    }
                    #endregion

                    #region 结算大类

                    //if (jsdlList != null & jsdlList.Count > 0)
                    //{
                    //    foreach (OutpatientSettlementCategoryEntity jsdl in jsdlList)
                    //    {
                    //        jsdl.jsnm = jsnm;
                    //        jsdl.CreatorCode = userModel.UserCode;
                    //        jsdl.CreateTime = DateTime.Now;
                    //        jsdl.jsrq = DateTime.Now;
                    //        jsdl.zt = "1";
                    //        jsdl.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                    //        jsdl.Create(true);  //自增string 主键
                    //        db.Insert(jsdl);
                    //        AppLogger.Info("结算大类: " + jsdl.jsdlId.ToString());
                    //        // sqlStrList.Add(DAL_mz_jsdl.Instance.getInsertSql(jsdl));
                    //    }
                    //    AppLogger.Info("插入成功！" + jsdlList.Count);
                    //}
                    #endregion

                    db.Commit();
                }

                catch (Exception ex)
                {
                    result = false;
                    throw new FailedCodeException(ex.Message);
                }
            }

            return result;
        }


        #region GRS门诊退费

        /// <summary>
        /// 门诊患者筛选
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="blh"></param>
        /// <param name="mzh"></param>
        /// <param name="xm"></param>
        /// <param name="orgId"></param>
        /// <param name="usercode"></param>
        /// <param name="jiuzhenbiaozhi"></param>
        /// <returns></returns>
        public List<OutpatAccInfoDto> GetBasicInfoSearchList(Pagination pagination, string blh, string mzh, string xm, string kssj, string jssj, string orgId, string usercode, string jiuzhenbiaozhi)
        {
            var strSql = new StringBuilder();
            strSql.Append(@"SELECT  A.patid,
        a.blh,
        A.xm,
        A.xb,
        A.csny,
        A.zjlx,
        A.zjh,
        MAX(c.CardNo) kh, -- 使用 MAX 确保每个字段只取一个值
        CAST(FLOOR(DATEDIFF(DY, a.csny, GETDATE()) / 365.25) AS INT) nl,
        MAX(xz.brxz) brxz,
        MAX(xz.brxzmc) brxzmc,
        MAX(yb.sycs) sycs,
        A.dh,
        A.dybh,
        MAX(gh.CreateTime) ghsj,
        gh.mzh,
        A.phone
FROM    [dbo].[xt_brjbxx] AS A WITH ( NOLOCK )
		INNER JOIN dbo.xt_card c ON c.patid = a.patid
									AND A.OrganizeId = c.OrganizeId
		LEFT JOIN xt_brxz xz ON xz.brxz = c.brxz
								AND xz.OrganizeId = c.OrganizeId
		LEFT JOIN xt_ybbab yb ON yb.patid = a.patid
									AND xz.OrganizeId = yb.OrganizeId
		RIGHT JOIN dbo.mz_gh gh ON gh.patid=a.patid 
		AND gh.OrganizeId=c.OrganizeId
WHERE  gh.ghrq >=@kssj and gh.ghrq<@jssj and ( gh.mzh like @mzh OR @mzh = '%%')
       and (isnull(@jiuzhenbiaozhi, '') = '' or isnull(gh.jzbz,'1') in (select * from [f_split](@jiuzhenbiaozhi,',')))
       AND ( gh.xm like @xm or A.py like @xm OR @xm = '%%')
	   AND ( a.blh like @blh OR @blh ='%%')
       AND a.OrganizeId = @OrganizeId and gh.zt='1' 
		--and isnull(gh.ghzt,'') <> '0'  --排除挂号未结                                
	   AND isnull(gh.ghzt,'') <> '2'  --排除已退
       AND (isnull(@usercode, '') = '' or gh.CreatorCode=@usercode)
GROUP BY A.patid, a.blh, A.xm, A.xb, A.csny, A.zjlx, A.zjh, A.dh, A.dybh, gh.mzh, A.phone");
            DbParameter[] param =
            {
                new SqlParameter("@xm","%"+(xm??"")+"%"),
                 new SqlParameter("@mzh","%"+(mzh??"")+"%"),
                 new SqlParameter("@blh","%"+(blh??"")+"%"),
                 new SqlParameter("@OrganizeId",orgId),
                 new SqlParameter("@usercode",usercode ?? ""),
                new SqlParameter("@jiuzhenbiaozhi",jiuzhenbiaozhi ?? ""),
                new SqlParameter("@kssj",Convert.ToDateTime(kssj).ToString("yyyy-MM-dd") ?? ""),
                new SqlParameter("@jssj",Convert.ToDateTime(jssj).AddDays(1).ToString("yyyy-MM-dd") ?? ""),
            };
            return QueryWithPage<OutpatAccInfoDto>(strSql.ToString(), pagination, param).ToList();
        }

        /// <summary>
        /// 病人管理查询病人信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="blh"></param>
        /// <param name="xm"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<OutpatAccInfoDto> GetBasicInfoSearchListInRegister(Pagination pagination, string blh, string xm, string orgId, string zjh,string zjlx)
        {
            var strSql = new StringBuilder();
            strSql.Append(@" SELECT  A.patid ,
                                a.blh ,
                                A.xm ,
                                A.xb ,
                                A.csny ,
                                A.zjlx ,
                                A.zjh ,
								c.CardTypeName,
                                c.CardType,
								c.CardId,
                                c.CardNo kh ,
                            
                                CAST(FLOOR(DATEDIFF(DY, a.csny, GETDATE()) / 365.25) AS INT) nl ,
                                xz.brxz ,
                                xz.brxzmc ,
                                yb.sycs ,
                                A.dh ,
                                A.dybh ,
                                A.phone,
                                a.CreateTime
                        FROM    [dbo].[xt_brjbxx] AS A WITH ( NOLOCK )
                                 INNER JOIN dbo.xt_card c ON c.patid = a.patid
                                                            AND A.OrganizeId = c.OrganizeId and c.zt=1
                                LEFT JOIN xt_brxz xz ON xz.brxz = c.brxz
                                                        AND xz.OrganizeId = a.OrganizeId and xz.zt=1
                                LEFT JOIN xt_ybbab yb ON yb.patid = a.patid
                                                         AND xz.OrganizeId = a.OrganizeId
                        WHERE   ( A.blh like @blh --OR C.CardNo like @blh
                                  OR @blh = '%%'
                                )
                                AND ( @xm = '%%' or A.xm like @xm or A.py like @xm
                                    )
                                AND ( @zjh = '%%' or A.zjh like @zjh
                                    )
                                AND a.OrganizeId = @OrganizeId AND a.zt = 1
                                --AND c.zt = 1
");
            DbParameter[] param =
            {
                new SqlParameter("@xm","%"+(xm??"")+"%"),
                 new SqlParameter("@blh","%"+(blh??"")+"%"),
                 new SqlParameter("@zjh", string.IsNullOrWhiteSpace(zjlx)? "%"+(zjh??"")+"%":zjh),
                new SqlParameter("@OrganizeId",orgId)
            };
            return QueryWithPage<OutpatAccInfoDto>(strSql.ToString(), pagination, param).ToList();
        }

        /// <summary>
        /// 病人门诊登记，浮层查询病人信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<OutpatAccInfoDto> GetBasicInfoSearchListInRegister(string keyword, string orgId)
        {
            var strSql = new StringBuilder();
            strSql.Append(@" SELECT  A.patid ,
                                a.blh ,
                                A.xm ,
                                A.xb ,
                                A.csny ,
                                ISNULL( A.zjlx,'') zjlx,
                                 ISNULL( A.zjh,'') zjh,
                                --A.klx ,
                                --c.CardNo kh ,
                                a.jsr ,
                                CAST(FLOOR(DATEDIFF(DY, a.csny, GETDATE()) / 365.25) AS INT) nl ,
                                xz.brxz ,
                                xz.brxzmc ,
                                A.dh ,
                                A.dybh ,
                                ISNULL(A.phone,'') phone,
                               ISNULL(A.brly,'') brly,
(SELECT COUNT(1) FROM dbo.mz_gh WHERE patid=a.patid and zt=1 AND OrganizeId=a.OrganizeId AND ghrq=CONVERT(varchar(10), GETDATE(), 23)) sycs,
                                a.jjllrgx lxrgx,
		                        a.jjlldh lxrdh,
		                        a.jjllr lxr
                        FROM    [dbo].[xt_brjbxx] AS A WITH ( NOLOCK )
                                INNER JOIN dbo.xt_card c ON c.patid = a.patid
                                                            AND A.OrganizeId = c.OrganizeId
                                LEFT JOIN xt_brxz xz ON xz.brxz = c.brxz
                                                        AND xz.OrganizeId = a.OrganizeId
                                                LEFT JOIN ( SELECT TOP 1
                                            *
                                    FROM    mz_gh
                                    WHERE   zt = 1
                                            AND OrganizeId = @OrganizeId
                                    ORDER BY CreateTime DESC
                                  ) gh ON gh.patid = a.patid
                        WHERE    ( A.blh like @keyword
                                  OR @keyword = '%%'
                                 or A.xm like @keyword or A.py like @keyword
                                    )
                                AND a.OrganizeId = @OrganizeId AND a.zt = 1
                                --AND c.zt = 1");
            DbParameter[] param =
            {
                new SqlParameter("@keyword","%"+(keyword??"")+"%"),
                new SqlParameter("@OrganizeId",orgId)
            };
            return FindList<OutpatAccInfoDto>(strSql.ToString(), param).ToList();
        }
        public List<PatInfoQuery> Getjzjsxx(string type, string keyword, string grbh, string orgId)
        {
            var strSql = new StringBuilder();
            strSql.Append(@"select * from(");
            if (type == "1")
            {
                strSql.Append(@"select distinct '门诊' type, b.jzid,convert(varchar(20),b.CreateTime,120) jzsj,b.mzh zymzh,c.jsnm");
            }
            else
            {
                strSql.Append(@"select distinct '门诊' type ,b.mzh zymzh,c.jsnm, b.jzid, c.ybjslsh jylsh ,convert(varchar(20),c.CreateTime,120) jssj");
            }
            strSql.Append(@" from xt_brjbxx a
join mz_gh b on a.patid=b.patid and a.OrganizeId=b.OrganizeId
join xt_card kh on kh.CardNo=b.kh and kh.CardType=b.CardType and kh.OrganizeId=b.OrganizeId and kh.zt='1'
join mz_js c on b.patid=c.patid and b.ghnm=c.ghnm and c.OrganizeId=b.OrganizeId
				  and c.jsnm not in(select cxjsnm  from mz_js where jszt='2')
left join cqyb_OutPut05 d on d.jsnm=c.jsnm and d.OrganizeId=c.OrganizeId where kh.grbh=@rybh 
and a.OrganizeId=@orgId ");
            if (type == "1")
            {
                strSql.Append(@" union all select distinct '住院' type, c.jylsh jzid ,convert(varchar(20),b.createtime,120) jzsj,b.zyh zymzh,d.jsnm ");
            }
            else
            {
                strSql.Append(@" union all select distinct '住院' type,b.zyh zymzh,d.jsnm ,c.jylsh jzid,d.ybjslsh jylsh,convert(varchar(20),d.createtime,120) jssj");
            }
            strSql.Append(@" from xt_brjbxx a
join zy_brjbxx b on a.patid=b.patid and a.OrganizeId=b.OrganizeId
join xt_card kh on kh.CardNo=b.kh and kh.CardType=b.CardType and kh.OrganizeId=b.OrganizeId and kh.zt='1'
join cqyb_OutPut02 c on c.zymzh=b.zyh and c.OrganizeId=b.OrganizeId
join zy_js d on d.zyh=b.zyh and d.OrganizeId=b.OrganizeId and d.jsnm not in (select cxjsnm from zy_js where jszt='2')
left join cqyb_OutPut05 e on e.jsnm=d.jsnm and e.OrganizeId=d.OrganizeId where kh.grbh=@rybh and a.OrganizeId=@orgId
");
            if (type == "1")
            {
                strSql.Append(@" ) s where  ( jzid like @keyword OR  @keyword = '%%') order by type, jzsj desc ");
            }
            else
            {
                strSql.Append(@" ) s where  ( jylsh like @keyword OR @keyword = '%%') order by type, jssj desc ");
            }
            DbParameter[] param =
           {
                new SqlParameter("@keyword","%"+(keyword??"")+"%"),
                new SqlParameter("@rybh",grbh),
                new SqlParameter("@orgId",orgId)
            };
            return FindList<PatInfoQuery>(strSql.ToString(), param).ToList();
        }
        /// <summary>
        /// 根据病历号获取结算信息
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<MZJSInfo> GetjsInfoByblh(string blh, string startTime, string endTime, string orgId)
        {
            #region 根据病历号查询patid
            var strSql = new StringBuilder();
            strSql.Append(@"
                            SELECT  patid
                    FROM    dbo.xt_brjbxx
                    WHERE   blh = @blh
                            AND OrganizeId = @OrganizeId");
            DbParameter[] param =
            {
                new SqlParameter("@blh",blh),
                new SqlParameter("@OrganizeId",orgId)
            };
            var result = FindList<int>(strSql.ToString(), param).ToList();
            if (result == null || result.Count < 1)
            {
                throw new FailedCodeException("OUTPAT_BLH_ISINVALID");
            }
            var patid = result[0];
            #endregion

            #region 根据patid查询结算信息
            string where = getWhere(startTime, endTime);
            var strjsSql = new StringBuilder();
            strjsSql.Append(@" SELECT  xx.xm AS xm ,
                            mz_js.blh ,
                            MZ_JS.JSNM ,
                            mz_js.PATID ,
                            --BRXZBH ,
                            mz_js.BRXZ ,
                            JSLX ,
                            MZ_JS.ZJE ,
                            ZLFY ,
                            ZFFY ,
                            FLZFFY ,
                            JZFY ,
                            XJZF ,
                            XJWC ,
                            ZHJZ ,
                            XJZFFS ,
                            JSZT ,
                            CXJSNM ,
                            isnull(mz_js.ZH,0) zh,
                            MZ_JS.CreatorCode ,
                            MZ_JS.jzsj CreateTime,
                            FPDM ,
                            JMBL ,
                            JMJE ,
                            JYLX ,
                            isnull(YSK,0.00) YSK,
                            isnull(ZL,0.00) ZL,
                            XJWC ,
                            MZ_JS.FPH AS oldFPH,
                            staff.Name czr
                    FROM    MZ_JS WITH ( NOLOCK )
                            LEFT JOIN dbo.xt_brjbxx xx ON xx.patid = dbo.mz_js.patid
                            LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff staff ON staff.Account = mz_js.CreatorCode
                                                              AND staff.OrganizeId = dbo.mz_js.OrganizeId
                    WHERE   xx.PATID = @patid
                            AND JSZT <> 2
                            AND NOT EXISTS ( SELECT 1
                                             FROM   mz_js b WITH ( NOLOCK )
                                             WHERE  b.jszt = '2'
                                                    AND b.cxjsnm = MZ_JS.jsnm
                                                    AND b.OrganizeId = @orgId )
                            AND MZ_JS.OrganizeId = @orgId ");
            DbParameter[] jspar =
            {
                new SqlParameter("@patid",patid),
                new SqlParameter("@orgId",orgId)
             };
            if (!string.IsNullOrWhiteSpace(where))
            {

                strjsSql.Append($"AND {@where} ");
            }
            strjsSql.Append(" ORDER BY MZ_JS.CreateTime DESC ");

            var dt = FindList<MZJSInfo>(strjsSql.ToString(), jspar).ToList();
            if (dt.Count < 0)
            {
                throw new FailedCodeException("OUTPAT_NO_PATIENT_INFORMATION");
            }
            #endregion
            return dt;
        }

        /// <summary>
        /// 按结算内码查门诊项目
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        /// 门诊项目明细
        public List<GridViewMx> GetMz_xmDetailByJsnm(int jsnm, string orgId)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
                            SELECT  DISTINCT
                            'mz_xm' as InfoSet,
                            mz_xm.sfxm SFXM,
                            mz_jsmx.jsnm,
                            xt_sfxm.sfxmmc YPMC,
                            mz_xm.dl ,
                            mz_xm.ysmc,
                            dlCode DL,
                            sfdl.dlmc DLMC,
                            CAST(mz_xm.dj AS VARCHAR) AS DJ ,
                            CAST(mz_xm.fwfdj AS VARCHAR) AS fwfdj ,
                            CAST(mz_jsmx.sl AS VARCHAR) sl ,
                            mz_jsmx.sl RETURNS_SL,
                            CAST(mz_xm.dj * mz_jsmx.sl AS VARCHAR) AS JE ,
                            CAST(mz_xm.zfbl AS VARCHAR) zfbl ,
                            xmnm XMNM, 
                            mz_xm.zfxz ZFXZ,
                            mz_xm.ghnm GHNM,
                            --xt_sfxm.wjdm ,
                            --xt_sfxm.nbdlCode AS NBDL ,
                            --xt_sfxm.ybdm ,
                            --mz_xm.pt_xmnm AS pt_cfnm,
                            mx.zlsc duration,
                            --CAST(xt_sfxm.DJ AS VARCHAR) DJ,
                            xt_sfxm.dw DANWEI,
                            mx.bz,
                            mx.jzsj
                     FROM   mz_xm
                            LEFT JOIN dbo.mz_jzjhmx mx ON mx.jzjhmxId = mz_xm.jzjhmxId
                            AND mx.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xt_sfxm ON mx.sfxmCode = xt_sfxm.sfxmCode
                            AND xt_sfxm.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_sfdl sfdl ON mz_xm.dl = sfdl.dlcode
                            AND sfdl.OrganizeId = @orgId
                            INNER JOIN mz_jsmx ON mz_jsmx.mxnm = mz_xm.xmnm
                                                  AND mz_jsmx.jslx != '0'
                                                  AND mz_jsmx.OrganizeId = @orgId
                     WHERE  mz_jsmx.jsnm = @jsnm
                            AND mz_xm.OrganizeId = @orgId");

            DbParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@orgId",orgId)
            };

            var data = FindList<GridViewMx>(sqlBuilder.ToString(), param).ToDataTable();
            var result = new List<GridViewMx>();
            if (data.Rows.Count <= 0) return result;
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var dr = data.Rows[i];
                var refund = new GridViewMx
                {
                    id = i + 1,
                    tag = dr,
                    InfoSet = "mz_xm",
                    SFXM = $"{dr["SFXM"]}",
                    jsnm = dr["jsnm"].ToInt(),
                    YPMC = $"{dr["YPMC"]}",
                    ysmc = $"{dr["ysmc"]}",
                    DL = $"{dr["DL"]}",
                    DLMC = $"{dr["DLMC"]}",
                    DJ = $"{dr["DJ"]}",
                    FWFDJ = dr["FWFDJ"].ToString(),
                    RETURNS_SL = dr["SL"].ToDecimal(),
                    SL = $"{dr["SL"]}",
                    JE = $"{dr["JE"].ToDecimal():0.00}",
                    ZFBL = $"{dr["ZFBL"]}",
                    XMNM = !string.IsNullOrWhiteSpace(dr["XMNM"].ToString()) ? dr["XMNM"].ToInt() : 0,
                    GHNM = dr["GHNM"].ToInt(),
                    duration = dr["duration"].ToInt(),
                    DANWEI = $"{dr["DANWEI"]}",
                    bz = $"{dr["bz"]}",
                    jzsj = $"{dr["jzsj"]}".ToDate(),
                    IS_RETURN = "1"
                };
                result.Add(refund);
            }
            return result;
        }

        /// <summary>
        /// 根据结算内码获取结算项目信息
        /// </summary>
        /// <param name="jsnm"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<MZJS> GetMZJSByJsnmInAcc(int jsnm, string orgId)
        {

            StringBuilder sqlBuilder = new StringBuilder(@" 
                            SELECT TOP 1
                            patid,
                            mz_xm.brxz,
                            brxzmc,
                            mz_xm.ks,
                                [NewtouchHIS_Base]..V_S_Sys_Department.Name ksmc,
                                ys,
                                xtry.name rymc,
                                ghnm
                            FROM   mz_xm WITH(NOLOCK)
                            LEFT JOIN(SELECT *
                                FROM    xt_brxz WITH(NOLOCK)
                            WHERE   zt = 1
                                ) AS xt_brxz ON mz_xm.brxz = xt_brxz.brxz
                            AND xt_brxz.OrganizeId = @orgId
                            LEFT JOIN[NewtouchHIS_Base]..V_S_Sys_Staff xtry ON mz_xm.ys = xtry.gh
                            AND xtry.OrganizeId = @orgId
                            LEFT JOIN[NewtouchHIS_Base]..V_S_Sys_Department ON mz_xm.ks = [NewtouchHIS_Base]..V_S_Sys_Department.Code
                            AND[NewtouchHIS_Base]..V_S_Sys_Department.OrganizeId = @orgId
                            WHERE  EXISTS(SELECT mxnm
                            FROM   mz_jsmx WITH(NOLOCK)
                            WHERE  EXISTS(SELECT *
                                FROM   mz_js
                            WHERE  jsnm = @jsnm
                            AND jslx != '0'
                            AND mz_js.jsnm = mz_jsmx.jsnm
                            AND mz_js.OrganizeId = @orgId)
                            AND mxnm = mz_xm.xmnm)
                            AND mz_xm.OrganizeId = @orgId

                            union

                             SELECT TOP 1
                                patid ,
                                mz_cf.brxz ,
                                brxzmc ,
                                mz_cf.ks ,
                                [NewtouchHIS_Base]..V_S_Sys_Department.Name ksmc ,
                                ys ,
                                xtry.name rymc ,
                                ghnm
                         FROM   mz_cfmx WITH ( NOLOCK )
                                LEFT JOIN mz_cf WITH ( NOLOCK ) ON mz_cfmx.cfnm = mz_cf.cfnm
                                                                   AND mz_cf.OrganizeId = @orgId
                                LEFT JOIN ( SELECT  *
                                            FROM    xt_brxz WITH ( NOLOCK )
                                            WHERE   zt = 1
                                          ) AS xt_brxz ON mz_cf.brxz = xt_brxz.brxz
                                                          AND xt_brxz.OrganizeId = @orgId
                                LEFT JOIN [NewtouchHIS_Base]..V_S_Sys_Staff xtry ON mz_cf.ys = xtry.gh
                                                                                    AND xtry.OrganizeId = @orgId
                                LEFT JOIN [NewtouchHIS_Base]..V_S_Sys_Department ON mz_cf.ks = [NewtouchHIS_Base]..V_S_Sys_Department.Code
                                                                                    AND [NewtouchHIS_Base]..V_S_Sys_Department.OrganizeId = @orgId
                         WHERE  EXISTS ( SELECT  cf_mxnm
                                         FROM   mz_jsmx WITH ( NOLOCK )
                                         WHERE  EXISTS ( SELECT *
                                                         FROM   mz_js WITH ( NOLOCK )
                                                         WHERE  jsnm = @jsnm
                                                                AND jslx != '0'
                                                                AND mz_js.jsnm = mz_jsmx.jsnm
                                                                AND mz_js.OrganizeId = @orgId )
                                                --AND mxbm = mz_cfmx.yp
                                                AND cf_mxnm = mz_cfmx.cfnm )");//发票号为空，用结算内码查询    
            DbParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm.ToString()),
                new SqlParameter("@orgId",orgId)
            };

            var result = this.FindList<MZJS>(sqlBuilder.ToString(), param);
            return result;
        }


        public List<GridViewMx> GetGridViewMx(int jsnm, string orgId)
        {
            //门诊项目
            var data = GetMz_xmDetailByJsnm(jsnm, orgId);
            //门诊处方
            var data2 = GetMz_cfmxDetailsByJsnm(jsnm, orgId);
            var id = data.Count;
            if (data2.Count > 0)
            {
                data2.ForEach(p =>
                {
                    id = id + 1;
                    p.id = id;
                    data.Add(p);
                });
            }

            return data;
        }

        private List<GridViewMx> GetMz_cfmxDetailsByJsnm(int jsnm, string orgId)
        {

            var data = GetMz_cfmxDetails(jsnm, orgId);
            var result = new List<GridViewMx>();
            if (data.Rows.Count <= 0) return result;
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var dr = data.Rows[i];
                var refund = new GridViewMx
                {
                    tag = dr,
                    InfoSet = "mz_cfmx",
                    DL = $"{dr["DL"]}",
                    CHUFANGHAO = $"{dr["CHUFANGHAO"]}",
                    DLMC = $"{dr["DLMC"]}",
                    YPMC = $"{dr["YPMC"]}",
                    ysmc = $"{dr["ysmc"]}",
                    DANWEI = $"{dr["DANWEI"]}",
                    DJ = $"{dr["DJ"]}",
                    ZFBL = $"{dr["ZFBL"]}",
                    SL = $"{dr["SL"]}",
                    RETURNS_SL = dr["SL"].ToDecimal(),
                    JE = $"{dr["JE"].ToDecimal():0.00}",
                    IS_RETURN = "1",
                    czh = $"{dr["czh"]}",
                    SFXM = $"{dr["SFXM"]}",
                    CF_MXNM = !string.IsNullOrWhiteSpace(dr["CF_MXNM"].ToString()) ? dr["CF_MXNM"].ToInt() : 0,
                    FWFDJ = dr["FWFDJ"].ToString(),
                    NBDL = $"{dr["NBDL"]}",
                    jsnm = dr["jsnm"].ToInt(),
                    jzsj = $"{dr["jzsj"]}".ToDate(),
                    GHNM = dr["GHNM"].ToInt()
                };
                result.Add(refund);
            }
            return result;
        }

        /// <summary>
        /// 按结算内码查处方明细
        /// </summary>
        /// <param name="jsnm"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private DataTable GetMz_cfmxDetails(int jsnm, string orgId)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
                                SELECT distinct  mz_jsmx.jsnm,
                                        'mz_cfmx' as InfoSet,
                                        '1' as IS_RETURN,
                                        0 as duration,
                                        mz_cf.cfnm CF_MXNM,
                                        cfh as CHUFANGHAO,
                                        mz_cfmx.yp as SFXM,
                                        ypmc,
                                        djdw as DANWEI,
                                        dlmc,
                                        convert(varchar(50),(mz_cfmx.dj+mz_cfmx.fwfdj)) dj,
                                        convert(varchar(50),mz_cfmx.fwfdj) fwfdj,
                                        convert(varchar(50),mz_jsmx.sl) sl,
                                        convert(varchar(50),(mz_jsmx.sl * (mz_cfmx.dj+mz_cfmx.fwfdj)))AS je,
                                        convert(varchar(50),mz_cfmx.zfbl) zfbl,
                                        mz_cfmx.dl,
                                        mz_cfmx.zfxz,
                                         mz_cf.ghnm,
                                        mz_cf.lyck,
                                        --mz_cf.pt_cfnm,
                                        mz_cfmx.cls,
                                        xt_yp.ypbzdm,
                                        mz_cfmx.cfnm,
                                        mz_cfmx.czh,
                                        xt_ypsx.ypgg,
                                        xt_yp.nbdl,
                                        xt_ypsx.ybdm,
                                        mx.jzsj,
										mz_cf.ys,
										mz_cf.ks,
										mz_cf.ysmc,
										mz_cf.ksmc
                                FROM mz_cfmx
                                LEFT JOIN mz_cf
                                    ON mz_cfmx.cfnm = mz_cf.cfnm and mz_cf.OrganizeId=@OrganizeId
                                LEFT JOIN dbo.mz_jzjhmx mx ON mx.jzjhmxId = mz_cfmx.jzjhmxId
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_yp xt_yp
                                    ON mz_cfmx.yp = xt_yp.ypCode and xt_yp.OrganizeId=@OrganizeId
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_ypsx xt_ypsx
                                    ON mz_cfmx.yp = xt_ypsx.ypCode and xt_ypsx.OrganizeId=@OrganizeId
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_sfdl xt_sfdl
                                    ON mz_cfmx.dl = xt_sfdl.dlCode and xt_sfdl.OrganizeId=@OrganizeId
                                INNER JOIN mz_jsmx
                                    ON mz_jsmx.jslx !='0' and mz_jsmx.OrganizeId=@OrganizeId
                                        --AND mz_jsmx.mxbm = mz_cfmx.yp
                                        AND mz_jsmx.cf_mxnm = mz_cfmx.cfnm
                                WHERE mz_jsmx.jsnm =@jsnm
                                And mz_cfmx.OrganizeId = @OrganizeId
                            ");

            DbParameter[] param =
            {
                new SqlParameter("@jsnm",jsnm),
                new SqlParameter("@OrganizeId",orgId)
            };


            var result = this.FindList<GridViewMx>(sqlBuilder.ToString(), param);
            var data = result.ToDataTable();

            return data;
        }

        /// <summary>
        /// 全退时更新医保次数
        /// </summary>
        /// <param name="jzrq"></param>
        /// <param name="IsReturnAll"></param>
        /// <param name="orgId"></param>
        /// <param name="patid"></param>
        public void Minusybcs(DateTime jzrq, bool IsReturnAll, string orgId, int patid)
        {
            if (!IsReturnAll) return;
            var strSql = new StringBuilder();
            string isClinic = "2";//医保
            DbParameter[] opar = { new SqlParameter("@Id", orgId) };
            var isZhenSuo = FirstOrDefault<string>("SELECT CategoryCode FROM NewtouchHIS_Base..Sys_Organize WHERE Id=@Id", opar);
            if (isZhenSuo == "Clinic")
            {
                isClinic = "1";//1表示诊所，用商保
            }
            strSql.Append(@" exec sp_bxba_gxsycs @bxtype,@orgId, @patId, @date");
            SqlParameter[] parameters =
            {
                new SqlParameter("@bxType", isClinic),
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@patId", patid),
                new SqlParameter("@date", jzrq)
            };
            ExecuteSqlCommand(strSql.ToString(), parameters);
        }

        #endregion

        #region 退费2018版

        /// <summary>
        /// 门诊退费 结算 主记录 Query
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public IList<OutPatientRefundableJsVO> RefundableJsQuery(string orgId, DateTime? kssj, DateTime? jssj, string mzh)
        {
            var sql = @"select
mzjs.jsnm, ybfy.ybjsh, mzjs.zje jszje, isnull(mzjs.xjzf,0) jsxjzf
,isnull(mzjs.jzsj, mzjs.CreateTime) sfrq
,mzjs.CreateTime CreateTime
,mzjs.CreatorCode CreatorCode
,userstaff.Name CreatorUserName
,mzjs.jslx,mzjs.fph
,ybfy.ZFY,ybfy.XJZF,ybfy.YBFY,ybfy.TSFY,ybfy.JBZF,ybfy.GBZF,ybfy.SUMZFDYBFY,ybfy.ZFDYBFY,ybfy.JBYE,ybfy.GBYE,ybfy.KBXYBFY,ybfy.KBXTSFY
,ybfy.TCZF,ybfy.JZZF,ybfy.DKC023
--现金支付方式 --多种时不行
,xjzffs.xjzffsmc xjzffsmc
from mz_js mzjs
left join mz_gh mzgh
on mzgh.ghnm = mzjs.ghnm and mzgh.OrganizeId = mzjs.OrganizeId
left join [NewtouchHIS_Base]..V_C_Sys_UserStaff userstaff
on userstaff.Account = mzjs.CreatorCode and userstaff.OrganizeId = mzjs.OrganizeId
left join mz_js_ybjyfy ybfy
on ybfy.jsnm = mzjs.jsnm and ybfy.OrganizeId = mzjs.OrganizeId
left join xt_xjzffs xjzffs
on mzjs.xjzffs = xjzffs.xjzffs
where mzjs.OrganizeId = @orgId and mzjs.zt = '1' and mzgh.mzh = @mzh
and isnull(mzjs.tbz, 0) = 0 
--排除欠费预结的结算记录
and isnull(mzjs.isQfyj, 0) = 0
and (@kssj is null or isnull(mzjs.jzsj, mzjs.CreateTime) >= @kssj)
and (@jssj is null or isnull(mzjs.jzsj, mzjs.CreateTime) < @jssj)
order by mzjs.CreateTime
";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@kssj", kssj.HasValue ? kssj.Value.Date : new DateTime(1970, 1, 1)));
            pars.Add(new SqlParameter("@jssj", jssj.HasValue ? jssj.Value.AddDays(1).Date : new DateTime(2099, 12, 31)));
            pars.Add(new SqlParameter("@mzh", mzh));

            return this.FindList<OutPatientRefundableJsVO>(sql, pars.ToArray());
        }

        /// <summary>
        /// 门诊退费 Query
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public IList<OutPatientRefundableVO> RefundableQuery(string orgId, int jsnm)
        {
            const string sql = @"
     select
     	mzjs.jsnm,mzjs.jslx, mzjs.zje, mzjsmx.jsmxnm, mzjsmx.mxnm xmjfId, mzjsmx.cf_mxnm ypjfId, mzjsmx.sl,ISNULL(mzjsmx.ktsl,0) ktsl , mzjsmx.jyje
     	,isnull(mzjs.jzsj, mzjs.CreateTime) sfrq
     	,mzjs.CreateTime CreateTime,mzjs.ghnm into #jsmx
     	from mz_js mzjs
     	left join mz_jsmx mzjsmx
     	on mzjsmx.jsnm = mzjs.jsnm and mzjsmx.OrganizeId = mzjs.OrganizeId
     	where mzjs.OrganizeId =@orgId and mzjs.zt = '1' and mzjsmx.zt = '1' and mzjs.jsnm = @jsnm
     	and isnull(mzjs.tbz, 0) = 0
         --去一部分脏数据
     	and mzjsmx.jyje is not null
     
         select a.jzjhmxId, a.zxzt, a.sl mcjfsl, count(b.Id) yzxcs into #jhyzxcs
         from mz_jzjhmx a
         left join mz_jzjhmxzx b
         on b.jzjhmxId = a.jzjhmxId and b.zt = '1'
         where a.zt = '1' and a.OrganizeId =  @orgId
         group by a.jzjhmxId, a.zxzt, a.sl
     
     select xmjsmx.ghnm,xmjsmx.jsmxnm,xmjsmx.jslx
     ,xmjsmx.sl 
     --20181120实施过程中的 显示出来
     ,case when mzxm.ssbz = '9' then 0 else (case when isnull(#jhyzxcs.yzxcs,0) = 0 then xmjsmx.sl else (xmjsmx.sl - #jhyzxcs.yzxcs * #jhyzxcs.mcjfsl) end) end ktsl
      ,CASE
                WHEN mzxm.ssbz = '9' THEN
                    0
                ELSE
            (CASE
                 WHEN ISNULL(#jhyzxcs.yzxcs, 0) = 0 THEN
                     xmjsmx.sl
                 ELSE
            (xmjsmx.sl - #jhyzxcs.yzxcs * #jhyzxcs.mcjfsl)
             END
            )
            END tsl
     ,xmjsmx.jyje jsmxje
     ,mzxm.dj, 2 feeType, mzxm.dw
     ,cf.cfh
     ,cf.cflxmc
     ,convert(varchar(10),cf.sqdzt) sqdzt
     ,sfxm.sfxmmc mc
     ,ztmc
	 ,ztId
	 ,ztsl
     ,sfmc.sfmb
     --,convert(varchar(80),isnull('('+sfmc.sfmblb+')','')+sfxm.sfxmmc) mc 
     ,sfdl.dlmc dlmc
     ,'' czh
     , cf.cfnm, sfxm.sfxmCode sfxmCode
     ,sfdl.dlCode sfdlCode
     ,sfxm.zfbl,sfxm.zfxz
     ,mzxm.xmnm, null cfmxId
     ,mzxm.ks, mzxm.ys, mzxm.ysmc
     , mzxm.CreateTime klsj
     --20181120有关联计划，且计划未执行
     , case when #jhyzxcs.zxzt = 0 then Convert(bit, 1) else null end zljhwzx
     from
     (
     	select * from #jsmx where isnull(xmjfId, 0) <> 0
     ) xmjsmx
     left join mz_xm mzxm
     on mzxm.xmnm = xmjsmx.xmjfId
     left join mz_cf cf
     on mzxm.cfnm = cf.cfnm and cf.OrganizeId = mzxm.OrganizeId
     left join [NewtouchHIS_Base]..V_S_xt_sfxm sfxm
     on sfxm.sfxmCode = mzxm.sfxm and sfxm.OrganizeId = mzxm.OrganizeId
     left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl
     on sfdl.dlCode = sfxm.sfdlCode and sfdl.OrganizeId = sfxm.OrganizeId
     left join #jhyzxcs
     on #jhyzxcs.jzjhmxId = mzxm.jzjhmxId 
     left join (select a.xmnm,a.cfnm,'收费项目组合'sfmblb,a.OrganizeId,sfmb  from mz_xm a with(nolock) left join xt_sfmb b with(nolock) on a.ztId=b.sfmbbh and a.ztmc=b.sfmbmc where a.zt='1' and b.zt='1') sfmc 
     on sfmc.xmnm=mzxm.xmnm and sfmc.cfnm=mzxm.cfnm and sfmc.OrganizeId=mzxm.OrganizeId 
     where 1 = 1
     --20181120实施过程中的 显示出来
     --实施过程中的不能退
     --and isnull(mzxm.ssbz, '0') <> '9'
     --能关联到门诊项目
     and mzxm.xmnm is not null
     
     union all
     
     select ypjsmx.ghnm,ypjsmx.jsmxnm,ypjsmx.jslx
     ,ypjsmx.sl 
     --未发药 全部可退，  --已发药需要药房药库接口告知  已退药全部退掉后可以退费
     ,case when cf.fybz='2' and tymx.tysl is null then ypjsmx.ktsl when cf.fybz = '2' and tymx.tysl>yp.bzs then  CONVERT(numeric, isnull(tymx.tysl,0)/yp.bzs) else ypjsmx.sl end ktsl
     ,case when cf.fybz='2' and tymx.tysl is null then ypjsmx.ktsl when cf.fybz = '2' and tymx.tysl>yp.bzs then CONVERT(numeric, isnull(tymx.tysl,0)/yp.bzs) else ypjsmx.sl end tsl
     ,ypjsmx.jyje jsmxje
     ,ypmx.dj, 1 feeType, ypmx.dw
     ,cf.cfh
     ,cf.cflxmc
     ,convert(varchar(10),cf.sqdzt) sqdzt
     ,yp.ypmc mc
     ,'' ztmc
	 ,'' ztId
	 ,0 ztsl
	 ,'' sfmb
     ,sfdl.dlmc dlmc
     ,ypmx.czh czh
     , cf.cfnm, yp.ypCode sfxmCode
     ,sfdl.dlCode sfdlCode
     ,yp.zfbl,yp.zfxz
     ,null xmnm, ypmx.cfmxId cfmxId
     ,cf.ks,cf.ys, cf.ysmc
     , ypmx.CreateTime klsj
     , null zljhwzx
     from
     (
     	select * from #jsmx where isnull(ypjfId, 0) <> 0
     ) ypjsmx
     left join mz_cfmx ypmx
     on ypmx.cfmxId = ypjsmx.ypjfId
     left join mz_cf cf
     on ypmx.cfnm = cf.cfnm and cf.OrganizeId = ypmx.OrganizeId
     left join (select cfh,ypcode,sum(sl)tysl from [NewtouchHIS_PDS]..mz_tfmx where zt='1'  group by cfh,ypcode
      ) tymx on tymx.cfh=cf.cfh and tymx.ypCode=ypmx.yp
     left join [NewtouchHIS_Base]..V_S_xt_yp yp
     on yp.ypCode = ypmx.yp and yp.OrganizeId = ypmx.OrganizeId
     left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl
     on sfdl.dlCode = yp.dlCode and sfdl.OrganizeId = yp.OrganizeId
     where 1 = 1
     --能关联到药品处方明细
     and ypmx.cfmxId is not null
     
     drop table #jsmx
     drop table #jhyzxcs ";

            var pars = new DbParameter[]
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@jsnm", jsnm)
            };
            return FindList<OutPatientRefundableVO>(sql, pars);
        }

        #endregion

        /// <summary>
        /// 门诊/住院计划录入 患者检索筛选
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="usercode"></param>
        /// <param name="jiuzhenbiaozhi"></param>
        /// <returns></returns>
        public List<OutpatAccInfoDto> GetBasicInfoSearchList(string keyword, string orgId, string usercode, string jiuzhenbiaozhi, string from = "mz", string brzybzType = null)
        {
            var strSql = new StringBuilder();
            if (from == "mz")
            {
                strSql.Append(@" SELECT  A.patid ,
                                a.blh ,
                                A.xm ,
                                A.xb ,
                                A.csny ,
                                A.zjlx ,
                                A.zjh ,
                                c.CardNo kh ,
                                a.jsr ,
                                CAST(FLOOR(DATEDIFF(DY, a.csny, GETDATE()) / 365.25) AS INT) nl ,
                                xz.brxz ,
                                xz.brxzmc ,
                                yb.sycs ,
                                A.dh ,
                                A.dybh ,
                                gh.CreateTime ghsj,
                                gh.mzh,
                                A.phone
                        FROM    [dbo].[xt_brjbxx] AS A WITH ( NOLOCK )
                                INNER JOIN dbo.xt_card c ON c.patid = a.patid
                                                            AND A.OrganizeId = c.OrganizeId
                                LEFT JOIN xt_brxz xz ON xz.brxz = a.brxz
                                                        AND xz.OrganizeId = c.OrganizeId
                                LEFT JOIN xt_ybbab yb ON yb.patid = a.patid
                                                         AND xz.OrganizeId = yb.OrganizeId
		                        RIGHT JOIN dbo.mz_gh gh ON gh.patid=a.patid 
		                        AND gh.OrganizeId=c.OrganizeId
                        WHERE    ( gh.mzh LIKE @keyword
                                  OR gh.xm LIKE @keyword
                                  OR A.py LIKE @keyword
                                  OR a.blh LIKE @keyword
                                  OR @keyword = '%%')
                                and (isnull(@jiuzhenbiaozhi, '') = '' or isnull(gh.jzbz,'1') in (select * from [f_split](@jiuzhenbiaozhi,',')))
                                AND a.OrganizeId = @OrganizeId and gh.zt='1' and isnull(gh.ghzt,'') <> '2'  --排除已退
                                and (isnull(@usercode, '') = '' or gh.CreatorCode=@usercode)");
                DbParameter[] param =
                {
                new SqlParameter("@keyword","%"+(keyword??"")+"%"),
                 new SqlParameter("@OrganizeId",orgId),
                 new SqlParameter("@usercode",usercode ?? ""),
                new SqlParameter("@jiuzhenbiaozhi",jiuzhenbiaozhi ?? ""),
            };
                return FindList<OutpatAccInfoDto>(strSql.ToString(), param).ToList();
            }
            else if (from == "zy")
            {
                strSql.Append(@" SELECT  A.patid ,
                                A.blh ,
                                A.xm ,
                                A.xb ,
                                A.csny ,
                                A.zjlx ,
                                A.zjh ,
                                c.kh kh ,
                                A.jsr ,
                                CAST(FLOOR(DATEDIFF(DY, A.csny, GETDATE()) / 365.25) AS INT) nl ,
                                xz.brxz ,
                                xz.brxzmc ,
                                0 sycs ,
                                A.dh ,
                                A.dybh ,
                                c.rqrq ghsj ,
                                c.zyh mzh ,
                                A.phone
                        FROM    [dbo].[xt_brjbxx] AS A WITH ( NOLOCK )
                                RIGHT JOIN dbo.zy_brjbxx c ON c.patid = A.patid
                                                              AND A.OrganizeId = c.OrganizeId
                                LEFT JOIN xt_brxz xz ON xz.brxz = A.brxz
                                                        AND xz.OrganizeId = c.OrganizeId
                        WHERE   ( c.zyh LIKE @keyword
                                  OR A.xm LIKE @keyword
                                  OR A.py LIKE @keyword
                                  OR A.blh LIKE @keyword
                                  OR @keyword = '%%'
                                )
                                AND A.zt = '1'
                                AND c.zt = '1'
                                AND c.zybz IN (select * from [f_split](@brzybzType,','))
                                AND c.OrganizeId = @orgId;");
                DbParameter[] param =
                {
                new SqlParameter("@keyword","%"+(keyword??"")+"%"),
                 new SqlParameter("@orgId",orgId),
                 new SqlParameter("@brzybzType",brzybzType??((int)EnumZYBZ.Wry).ToString())
                };
                return FindList<OutpatAccInfoDto>(strSql.ToString(), param).ToList();
            }
            return null;
        }

        #region 医保退费

        /// <summary>
        /// 获取待退费信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public IList<OutPatientRefundableGuiAnJsVO> RefundableGuiAnJsQuery(string orgId, DateTime? kssj, DateTime? jssj, string mzh)
        {
            const string sql = @"
SELECT mzjs.jsnm, ybfy.prm_yka103 ybjsh, mzjs.zje jszje, isnull(mzjs.xjzf,0) jsxjzf ,isnull(mzjs.jzsj, mzjs.CreateTime) sfrq 
,mzjs.CreateTime CreateTime ,mzjs.CreatorCode CreatorCode ,userstaff.Name CreatorUserName ,mzjs.jslx,mzjs.fph,
ybfy.prm_akc190,ybfy.prm_yab003,'11' prm_aka130,ybfy.prm_yka103,'' prm_aae011,'' prm_ykc141,'' prm_aae036, '' prm_aae013,'' prm_ykb065,ybfy.prm_aac001
--现金支付方式 --多种时不行
,xjzffs.xjzffsmc xjzffsmc, ISNULL(xsr.outpId,'') outpId
FROM mz_js mzjs
LEFT JOIN mz_gh mzgh ON mzgh.ghnm = mzjs.ghnm and mzgh.OrganizeId = mzjs.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_C_Sys_UserStaff userstaff ON userstaff.Account = mzjs.CreatorCode and userstaff.OrganizeId = mzjs.OrganizeId
LEFT JOIN mz_js_gaybjyfy ybfy ON ybfy.jsnm = mzjs.jsnm and ybfy.OrganizeId = mzjs.OrganizeId AND LEN(ISNULL(ybfy.prm_yka103,'')) >0
LEFT JOIN xt_xjzffs xjzffs ON mzjs.xjzffs = xjzffs.xjzffs
LEFT JOIN dbo.mz_xnh_settResult(NOLOCK) xsr ON xsr.jsnm=mzjs.jsnm AND xsr.OrganizeId=mzjs.OrganizeId AND mzgh.zt='1'
WHERE mzjs.OrganizeId = @orgId and mzjs.zt = '1' and mzgh.mzh = @mzh
AND ISNULL(mzjs.tbz, 0) = 0 
--排除欠费预结的结算记录
AND ISNULL(mzjs.isQfyj, 0) = 0
AND (@kssj is null or isnull(mzjs.jzsj, mzjs.CreateTime) >= @kssj)
AND (@jssj is null or isnull(mzjs.jzsj, mzjs.CreateTime) < @jssj)
ORDER BY mzjs.CreateTime
                    ";
            var pars = new DbParameter[]
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@kssj", kssj.HasValue ? kssj.Value.Date : new DateTime(1970, 1, 1)),
                new SqlParameter("@jssj", jssj.HasValue ? jssj.Value.AddDays(1).Date : new DateTime(2099, 12, 31)),
                new SqlParameter("@mzh", mzh)
            };

            return FindList<OutPatientRefundableGuiAnJsVO>(sql, pars.ToArray());
        }

        #endregion

        #region
        /// <summary>
        /// 获取待退费信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public IList<OutPatChongQingVO> RefundableChongQingQuery(string orgId, DateTime? kssj, DateTime? jssj, string mzh)
        {
            const string sql = @"
					SELECT  mzgh.mzh,mzgh.ghly,
                    mzjs.jsnm ,
					mzjs.ybjslsh ybjsh ,mzgh.jzid,ybfy.chrg_bchno pch,ybfy.med_type yllb,mzgh.bzbm bzbm,
					mzjs.zje jszje ,
					ISNULL(mzjs.xjzf, 0) jsxjzf ,
					ISNULL(mzjs.jzsj, mzjs.CreateTime) sfrq ,
					mzjs.CreateTime CreateTime ,
					mzjs.CreatorCode CreatorCode ,
					userstaff.Name CreatorUserName ,
					mzjs.jslx ,
					mzjs.fph
			--现金支付方式 --多种时不行
					,
					xjzffs.xjzffsmc+':'+cast(jszffs.zfje as varchar) xjzffsmc
					into #jsxx
			FROM    mz_js mzjs with(nolock)
					LEFT JOIN mz_gh mzgh with(nolock) ON mzgh.ghnm = mzjs.ghnm
											AND mzgh.OrganizeId = mzjs.OrganizeId
					LEFT JOIN [NewtouchHIS_Base]..V_C_Sys_UserStaff userstaff ON userstaff.Account = mzjs.CreatorCode
																		  AND userstaff.OrganizeId = mzjs.OrganizeId
					LEFT JOIN drjk_mzjs_input ybfy with(nolock) ON ybfy.setl_id = mzjs.ybjslsh
													AND ybfy.zt = '1'
					LEFT JOIN mz_jszffs jszffs ON jszffs.jsnm=mzjs.jsnm and jszffs.OrganizeId=mzjs.OrganizeId and jszffs.zt='1' and jszffs.zfje>0
					LEFT JOIN xt_xjzffs xjzffs ON jszffs.xjzffs = xjzffs.xjzffs
			WHERE   mzjs.OrganizeId = @orgId
					AND mzjs.zt = '1'
					AND mzgh.mzh = @mzh
					AND ISNULL(mzjs.tbz, 0) = 0 
			--排除欠费预结的结算记录
					AND ISNULL(mzjs.isQfyj, 0) = 0
					AND ( @kssj IS NULL
						  OR ISNULL(mzjs.jzsj, mzjs.CreateTime) >= @kssj
						)
					AND ( @jssj IS NULL
						  OR ISNULL(mzjs.jzsj, mzjs.CreateTime) < @jssj
						)
            select mzh,ghly,jsnm,ybjsh,jzid,pch,yllb,bzbm,jszje,jsxjzf,sfrq,createtime,creatorcode,creatorusername,jslx,fph
			    ,xjzffsmc = ( stuff((select ',' + xjzffsmc from #jsxx where jsnm = jsxx.jsnm for xml path('')), 1, 1, '') )
			    from #jsxx jsxx
			    group by mzh,ghly,jsnm,ybjsh,jzid,pch,yllb,bzbm,jszje,jsxjzf,sfrq,createtime,creatorcode,creatorusername,jslx,fph
			    order by CreateTime
		;
                    ";
            var pars = new DbParameter[]
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@kssj", kssj.HasValue ? kssj.Value.Date : new DateTime(1970, 1, 1)),
                new SqlParameter("@jssj", jssj.HasValue ? jssj.Value.AddDays(1).Date : new DateTime(2099, 12, 31)),
                new SqlParameter("@mzh", mzh)
            };

            return FindList<OutPatChongQingVO>(sql, pars.ToArray());
        }
        #endregion
        /// <summary>
        /// 门诊住院预交金患者浮层
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="orgId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<OutpatAccInfoDto> GetZyMzYjjPatientSearch(string keyword, string orgId, string type)
        {
            var strSql = new StringBuilder();
            var zyhcx = " ";
            if (type == "mz")
            {
                strSql.Append(@" SELECT distinct top 50 A.patid ,
                                a.blh ,
                                A.xm ,
                                A.xb ,
                                A.csny ,
                                ISNULL( A.zjlx,'') zjlx,
                                 ISNULL( A.zjh,'') zjh,
                                CAST(FLOOR(DATEDIFF(DY, a.csny, GETDATE()) / 365.25) AS INT) nl ,
                              --  xz.brxz ,
                               -- xz.brxzmc ,
                                A.dh ,
                                A.dybh ,
                                ISNULL(A.phone,'') phone,
                               ISNULL(A.brly,'') brly,
                                a.jjllrgx lxrgx,
		                        a.jjlldh lxrdh,
		                        a.jjllr lxr
                        FROM    [dbo].[xt_brjbxx] AS A with ( nolock )
                                INNER JOIN mz_gh gh with ( nolock ) on gh.patid = a.patid
                                                        AND a.OrganizeId = gh.OrganizeId
								LEFT JOIN xt_card kh with ( nolock ) on kh.CardNo=gh.kh
														AND kh.OrganizeId=gh.OrganizeId
								LEFT JOIN xt_brxz xz with ( nolock ) ON xz.brxz = kh.brxz
                                                        AND xz.OrganizeId = kh.OrganizeId
                                ");
            }
            else
            {

                strSql.Append(@" SELECT distinct top 50 A.patid ,
                                a.blh ,
                                A.xm ,
                                A.xb ,
                                A.csny ,
                                ISNULL( A.zjlx,'') zjlx,
                                ISNULL( A.zjh,'') zjh,
                                CAST(FLOOR( zy.nl ) AS INT) nl,
                                --xz.brxz ,
                               -- xz.brxzmc ,
                                A.dh ,    
                                A.dybh ,
                                ISNULL(A.phone,'') phone,
                                ISNULL(A.brly,'') brly,
                                a.jjllrgx lxrgx,
		                        a.jjlldh lxrdh,
		                        a.jjllr lxr,zy.zyh,
                                ( CASE zy.zybz
                                            WHEN 0 THEN '入院登记'
                                            WHEN 1 THEN '病区中'
                                            WHEN 2 THEN '病区出院'
                                            WHEN 3 THEN '病人出院'
                                            WHEN 9 THEN '取消入院'
                                            ELSE ''
                                          END ) zybz 
                        FROM    [dbo].[xt_brjbxx] AS A WITH ( NOLOCK )
                                INNER JOIN zy_brjbxx zy on zy.patid=a.patid
														AND a.OrganizeId = zy.OrganizeId and zybz <>'9'
								LEFT JOIN xt_brxz xz ON xz.brxz = zy.brxz
                                                        AND xz.OrganizeId = a.OrganizeId and xz.zt=a.zt 
                                ");
                zyhcx = " or zy.zyh like @keyword ";
            }

            strSql.Append(@"  WHERE    ( @keyword = '%%' " + zyhcx + "" +
                " or A.xm like @keyword or A.py like @keyword or A.blh like @keyword)" +
                "AND a.OrganizeId = @OrganizeId AND a.zt = 1");

            DbParameter[] param =
            {
                new SqlParameter("@keyword","%"+(keyword??"")+"%"),
                new SqlParameter("@OrganizeId",orgId)
            };
            return FindList<OutpatAccInfoDto>(strSql.ToString(), param).ToList();
        }
    }

}
