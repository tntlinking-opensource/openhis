using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Infrastructure;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Newtouch.DomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public class PrescriptionDmnService : DmnServiceBase, IPrescriptionDmnService
    {
        public PrescriptionDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }

        /// <summary>
        /// 历史处方树明细
        /// </summary>
        /// <param name="cfId"></param>
        public IList<HistoryPresFormSelectVO> GetHistoryPresDetailByCfId(string cfId)
        {
            //处方明细
            var sql = @"
SELECT cfmxId,Createtime, zh, xmmc, cfmx.ypmc, ypgg,
 cast(isnull(mcjl,0) as varchar)+'('+mcjldw+')' AS mcjl,
 yfmc, dj, mczll, cfmx.pcCode, pc.yzpcmc pcmc, sl, bw,
 cast(isnull(isnull(zl,0),0) as varchar)+'('+dw+')' AS zl, 
 je,cfmx.zxks,ks.name AS zxksmc,
        CASE WHEN ISNULL(yp.ybdm, '') = '' THEN '否'
             ELSE '是'
        END sfyb
FROM xt_cfmx cfmx
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_yzpc(nolock) pc
ON pc.yzpcCode=cfmx.pcCode AND pc.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_C_xt_yp(nolock) yp
ON yp.ypCode=cfmx.ypCode AND yp.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_ypyf(nolock) yf
ON yf.yfCode=cfmx.yfCode
LEFT JOIN [NewtouchHIS_Base]..V_S_Sys_Department(nolock) ks
ON ks.Code=cfmx.zxks AND ks.OrganizeId=cfmx.OrganizeId
WHERE cfmx.cfId=@cfId and cfmx.zt='1' --and cfmx.ypmc is not null
                    ";
            var list = this.FindList<HistoryPresFormSelectVO>(sql, new[] { new SqlParameter("@cfId", cfId) });
            return list;
        }

        /// <summary>
        /// 根据cfId或cfmxIdStr查询明细
        /// </summary>
        /// <param name="cflx"></param>
        /// <param name="cfId"></param>
        /// <param name="cfmxIdStr"></param>
        /// 20190619 huangshanshan 去除cf.zt='1'和cfmx.zt='1'的限制，可复制作废处方
        /// <returns></returns>
        public List<PrescriptionDetailQueryVO> GetPresDetailList(int cflx, string cfId, string cfmxIdStr)
        { 
            var sql = "";
            var pars = new List<SqlParameter>();
            List<PrescriptionDetailQueryVO> list = new List<PrescriptionDetailQueryVO>();
            if (cflx == (int)EnumCflx.RehabPres)   //康复处方
            {
                sql += @"
SELECT cf.cfId, cfmx.xmCode, cfmx.xmmc, cfmx.mczll,cfmx.pcCode, cfmx.sl, cfmx.dw, 
        isnull(cfmx.mczll,0)/isnull(sfxm.dwjls,1)*isnull(cfmx.sl,0) AS zl,   --根据最新的dwjls，算出总量
        isnull(cfmx.mczll,0)/isnull(sfxm.dwjls,1)*isnull(cfmx.sl,0)*isnull(sfxm.dj,0) je,   --根据最新的dwjls和dj，算出总额
        ";
            }
            else if (cflx == (int)EnumCflx.RegularItemPres|| cflx == (int)EnumCflx.Yyhc)   //常规项目处方  医用耗材处方
            {
                sql += @"
SELECT cf.cfId, cfmx.xmCode, cfmx.xmmc, cfmx.mczll,cfmx.pcCode, cfmx.sl, cfmx.dw, cfmx.ztId,cfmx.ztmc , 
        isnull(cfmx.mczll,0)/isnull(sfxm.dwjls,1)*isnull(cfmx.sl,0) AS zl,   --根据最新的dwjls，算出总量
        isnull(cfmx.mczll,0)/isnull(sfxm.dwjls,1)*isnull(cfmx.sl,0)*isnull(sfxm.dj,0) je,   --根据最新的dwjls和dj，算出总额
        ";
            }
            if (cflx == (int)EnumCflx.InspectionPres || cflx == (int)EnumCflx.ExaminationPres)   //检验、检查处方
            {
                sql += @"
SELECT cf.cfId, cfmx.xmCode, cfmx.xmmc, cfmx.mczll,cfmx.pcCode, cfmx.sl, cfmx.dw, 
        CONVERT(DECIMAL(9,2),(isnull(cfmx.sl,0)*isnull(sfxm.dj,0))) je,   --根据最新的dj，算出总额
        ";
            }
            if (cflx == (int)EnumCflx.RehabPres || cflx == (int)EnumCflx.RegularItemPres || cflx == (int)EnumCflx.Yyhc || cflx == (int)EnumCflx.InspectionPres || cflx == (int)EnumCflx.ExaminationPres)
            {
                sql += @"
        sfxm.dj,
        sfxm.dw,
        sfxm.dwjls,
        sfxm.jjcl,
        pc.yzpcmc AS pcmc,
        cf.cfh AS cfh,
       -- cfmx.zxks zxks,
		--yfbm.yfbmmc zxksmc,
        cfmx.urgent,
        cfmx.bw,
cfmx.purpose,
cfmx.Remark,   --嘱托
cfmx.zxks,
Department.Name AS zxksmc,
cfmx.zxsj 
FROM xt_cfmx cfmx
INNER JOIN xt_cf(nolock) cf
    ON cf.cfId=cfmx.cfId
        AND cf.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_yzpc(nolock) pc
    ON pc.yzpcCode=cfmx.pcCode
        AND pc.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfxm(nolock) sfxm 
    ON sfxm.sfxmCode=cfmx.xmCode
        AND sfxm.OrganizeId=cfmx.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department Department
    ON Department.Code=cfmx.zxks
        AND Department.OrganizeId= cfmx.OrganizeId 
left join [NewtouchHIS_Base]..xt_yfbm(nolock)  yfbm
    ON yfbm.yfbmCode=cfmx.zxks and yfbm.OrganizeId=yfbm.OrganizeId 
WHERE 1=1";
                if (!string.IsNullOrWhiteSpace(cfId))
                {
                    sql += " and cfmx.cfId = @cfId";
                    pars.Add(new SqlParameter("@cfId", cfId));
                }
                else
                {
                    sql += " AND cfmx.cfmxId in (select * from dbo.f_split(@cfmxIdStr,','))";
                    pars.Add(new SqlParameter("@cfmxIdStr", cfmxIdStr));
                }
            }
            if (cflx == (int)EnumCflx.WMPres || cflx == (int)EnumCflx.TCMPres) //药品处方
            {
                sql = @"
SELECT cf.cfId, cfmx.ypCode,cfmx.ypmc,cfmx.mcjl,cfmx.mcjldw,cfmx.mcjldw AS mcjldwwwwwww
,yp.jldw AS redundant_jldw
,cfmx.yfCode,cfmx.pcCode,cfmx.sl,cfmx.zl,cfmx.dw,
        CONVERT(DECIMAL(9,2),(isnull(cfmx.sl,0)*(yp.lsj / yp.bzs * yp.mzcls))) AS je,  --根据最新的dj算出je
        cfmx.zh,cf.tieshu,cf.cfyf,cf.djbz,
        yp.lsj / yp.bzs * yp.mzcls AS dj,  --最新的dj
        yp.mzcldw AS dw,
        pc.yzpcmc AS pcmc,
        yp.ypgg,
        yp.mzcls AS cls,
        yf.yfmc,
        cf.cfh AS cfh,
		cfmx.zxks zxks,
		yfbm.yfbmmc zxksmc,
yp.jx jxCode,
cfmx.ds,
cfmx.Remark    --嘱托
FROM xt_cfmx cfmx
INNER JOIN xt_cf(nolock) cf
    ON cf.cfId=cfmx.cfId
        AND cf.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_yzpc(nolock) pc
    ON pc.yzpcCode=cfmx.pcCode
        AND pc.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_C_xt_yp(nolock) yp
    ON yp.ypCode=cfmx.ypCode
        AND yp.OrganizeId=cfmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_ypyf(nolock) yf
    ON yf.yfCode=cfmx.yfCode
left join [NewtouchHIS_Base]..xt_yfbm(nolock)  yfbm
    ON yfbm.yfbmCode=cfmx.zxks and yfbm.OrganizeId=yfbm.OrganizeId
WHERE 1=1";
                if (!string.IsNullOrWhiteSpace(cfId))
                {
                    sql += " and cfmx.cfId = @cfId";
                    pars.Add(new SqlParameter("@cfId", cfId));
                }
                else
                {
                    sql += " AND cfmx.cfmxId in (select * from dbo.f_split(@cfmxIdStr,','))";
                    pars.Add(new SqlParameter("@cfmxIdStr", cfmxIdStr));
                }
            }
            if (!string.IsNullOrWhiteSpace(sql))
            {
                list = this.FindList<PrescriptionDetailQueryVO>(sql, pars.ToArray());
            }
            return list;
        }
        /// <summary>
        /// 医保患者审核收费项目是否含自费
        /// </summary>
        /// <param name="sfxmCode"></param>
        /// <param name="cflx"></param>
        /// <returns></returns>
        public List<string> ValidateMedicalInsurance(Dictionary<int,string> dic,string orgId) {
            var sql = "";
            List<string> rtnlist = new List<string>();
            if (dic!=null&&dic.Count()>0)
            {
                foreach (var item in dic)
                {
                    if (item.Key == (int)EnumCflx.RehabPres || item.Key == (int)EnumCflx.RegularItemPres || item.Key == (int)EnumCflx.ExaminationPres || item.Key == (int)EnumCflx.InspectionPres)
                    {
                        sql = @"SELECT
                            '【' + sfxmmc + '】'
                    FROM    NewtouchHIS_Base..V_S_xt_sfxm
                    WHERE   sfxmCode IN ( SELECT    *
                                          FROM      dbo.f_split(@sfxmCode, ',') )
                            AND OrganizeId = @orgId AND ISNULL(ybdm, '') = '';";
                    }
                    else if (item.Key == (int)EnumCflx.TCMPres || item.Key == (int)EnumCflx.WMPres)
                    {
                        sql = @"SELECT
                        '【' + ypmc + '】'
                FROM    NewtouchHIS_Base..V_C_xt_yp
                WHERE   ypCode IN ( SELECT  *
                                    FROM    dbo.f_split(@sfxmCode, ',') )
                        AND OrganizeId = @orgId AND ISNULL(ybdm, '') = '';";
                    }
                    var pars = new List<SqlParameter>();
                    pars.Add(new SqlParameter("@sfxmCode", item.Value));
                    pars.Add(new SqlParameter("@orgId", orgId));
                    rtnlist.AddRange(this.FindList<string>(sql, pars.ToArray()).ToList());
                }
            }

            return rtnlist;
        }

		/// <summary>
		/// 开立物资项目冻结库存数量
		/// </summary>
		/// <param name="cfh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public int Sumwzdj(string[] cfh, string orgId, string rygh)
		{
			foreach (var item in cfh)
			{
				string sql = "exec [开立物资冻结库存量] @cfh,@orgId,@rygh";//"exec NewtouchHIS_herp..herp_开立物资冻结库存量 @cfh,@orgId,@rygh";
                var pars = new List<SqlParameter>();
				pars.Add(new SqlParameter("@cfh", item));
				pars.Add(new SqlParameter("@orgId", orgId));
				pars.Add(new SqlParameter("@rygh", rygh));
				this.ExecuteSqlCommand(sql, pars.ToArray());
			}

			return 1;
		}
		/// <summary>
		/// 开立物资项目冻结库存数量作废
		/// </summary>
		/// <param name="cfh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public int ZUOFwzdj(string cfh, string orgId, string rygh)
		{
			if (cfh == "")
			{
				return 1;
			}
			string sql = "exec 开立物资冻结库存量_作废 @cfh,@orgId,@rygh";//"exec NewtouchHIS_herp..herp_开立物资冻结库存量_作废 @cfh,@orgId,@rygh";
            var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@cfh", cfh));
			pars.Add(new SqlParameter("@orgId", orgId));
			pars.Add(new SqlParameter("@rygh", rygh));
			this.ExecuteSqlCommand(sql, pars.ToArray());

			return 1;
		}
        /// <summary>
        /// 医保电子处方
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<string> getdzcfnrxs(string cfh, string orgId)
        {
            var sqlpar = new List<SqlParameter>();
            var par = new StringBuilder();
            par.Append(@"select top 1 mzh from [NewtouchHIS_Sett]..Dzcf_D003_output where cfh=@cfh and OrganizeId=@orgId and zt='1'");
            sqlpar.Add(new SqlParameter("@cfh", cfh));
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            var resultList = this.FindList<string>(par.ToString(), sqlpar.ToArray());
            return resultList;
        }
    }
}
