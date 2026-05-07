using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.BusinessObjects.HospitalizationManage;
using Newtouch.HIS.Domain.DTO;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.OutputDto;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.OutpatientManage;
using Newtouch.HIS.Domain.Entity.PatientManage;
using Newtouch.HIS.Domain.IDomainServices.HospitalizationManage;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.IRepository.OutpatientManage;
using Newtouch.HIS.Domain.IRepository.PatientManage;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.HospitalizationManage;
using Newtouch.HIS.Proxy.guian.DTO;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using static Newtouch.Infrastructure.Constants;

namespace Newtouch.HIS.DomainServices.HospitalizationManage
{
	public class DischargeSettleDmnService : DmnServiceBase, IDischargeSettleDmnService
	{
		private readonly IHospPatientBasicInfoRepo _hospPatientBasicInfoRepo;
		private readonly ISysConfigRepo _sysConfigRepo;
		private readonly IFinancialInvoiceRepo _financialInvoiceRepo;
		private readonly IHospItemBillingRepo _hospItemBillingRepo;
		private readonly ICqybSett05Repo _cqybSett05Repo;
		private readonly ICqybSett23Repo _iCqybSett23Repo;
		private readonly ICqybUploadInPres04Repo _cqybUploadInPres04Repo;
		public DischargeSettleDmnService(IDefaultDatabaseFactory databaseFactory)
			: base(databaseFactory)
		{
		}

		#region 出院结算
		/// <summary>
		/// 根据住院号获取病人基本信息
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="OrganizeId"></param>
		/// <returns></returns>
		public List<InpatientSettPatInfoVO> SelectInpatientSettPatInfo(string zyh, string orgId)
		{
			StringBuilder sqlStr = new StringBuilder();
			sqlStr.Append(@"
--第一诊断
DECLARE @ryzdmc varchar(20) DECLARE @ryzdicd10 varchar(20)
SELECT top 1 @ryzdmc = n.zdmc,
         @ryzdicd10 = n.icd10
FROM zy_rydzd m with(nolock)
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_zd n
    ON m.zdCode = n.zdCode
        AND (n.OrganizeId=@orgId
        OR n.OrganizeId = '*')
WHERE m.zyh = @zyh
        AND m.OrganizeId=@orgId
ORDER BY  m.zdpx

SELECT zybrjbxx.zyh,
         zybrjbxx.zybz,
         zybrjbxx.patId,zybrjbxx.kh,zybrjbxx.CardType,zybrjbxx.CardTypeName,
         brjbxx.xm,
         brjbxx.csny,
         zybrjbxx.zjh,
         brxz.brxzbh,
         zybrjbxx.brxz,
         brxz.ybjylx,
         brxz.brxzlb,
         brxz.brxzmc,
         --brxz.brxzmc,
         zybrjbxx.ryrq,
         zybrjbxx.cyrq,
         brjbxx.blh,
         brjbxx.xb,
         brzh.zh as zyyjjzh,
         zybrjbxx.ks AS ksCode,
         Department.Name AS ksmc,
		 --住院退费add
		 @ryzdmc as ryzdmc,@ryzdicd10 as ryzdicd10,
         d.zdmc cyzd,
		 d.zddm cyzdicd10,
         zybrjbxx.cw,
         zh.zhye,
		 cd.cblb
FROM zy_brjbxx zybrjbxx with(nolock)
LEFT JOIN xt_brjbxx brjbxx
    ON zybrjbxx.patid = brjbxx.patid  AND brjbxx.zt='1'
        AND brjbxx.OrganizeId=@orgId
LEFT JOIN xt_brxz brxz
    ON brxz.brxz = zybrjbxx.brxz AND brxz.zt='1'
        AND brxz.OrganizeId=@orgId
LEFT JOIN xt_brzh brzh
    ON zybrjbxx.zyh = brzh.zyh and brzh.OrganizeId=@orgId  AND brzh.zt='1'
LEFT JOIN [NewtouchHIS_Base]..V_S_Sys_Department Department
    ON Department.Code = zybrjbxx.ks
        AND Department.OrganizeId=@orgId  AND Department.zt='1'
 LEFT JOIN [Newtouch_CIS]..[zy_PatDxInfo] d ON d.zyh = zybrjbxx.zyh  AND d.zt='1'
                                                     AND ( d.OrganizeId = @orgId 
                                                           --OR d.OrganizeId = '*'
														   and d.zdlb='2' and d.zdlx='0'
                                                         )
left join  zy_zh zh with(nolock) on zh.zyh=zybrjbxx.zyh and zh.OrganizeId= @orgId and zh.zt='1'
left join xt_card cd on cd.CardNo=zybrjbxx.kh and zybrjbxx.OrganizeId=cd.OrganizeId and cd.zt='1'
WHERE zybrjbxx.zyh=@zyh
        AND zybrjbxx.OrganizeId=@orgId and zybrjbxx.zt=1
                        ");
			SqlParameter[] param =
				{
					new SqlParameter("@zyh",zyh),
					new SqlParameter("@orgId",orgId)
				};
			var list = this.FindList<InpatientSettPatInfoVO>(sqlStr.ToString(), param).ToList();
			return list;
		}

		/// <summary>
		/// 根据住院号获取病人 医保相关信息
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public InpatientSettYbPatInfoVO GetInpatientSettYbPatInfo(string zyh, string orgId)
		{
			var sql = @"select zybrxx.zyh, ybzydj.prm_akc190 jzbh, ybzydj.prm_aac001 sbbh, ybzydj.prm_yab003 fzxbh, ybzydj.prm_aka130 zhifuleibie, ybzydj.prm_ykb065 sbbf
--,zybrxx.OrganizeId
from zy_brjbxx zybrxx
inner join Gzyb_OutRYBL_21 ybzydj
on ybzydj.prm_ykc010 = zybrxx.zyh and ybzydj.OrganizeId = zybrxx.OrganizeId
where zybrxx.zt = '1'
and zybrxx.zyh = @zyh and zybrxx.OrganizeId = @orgId";
			var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@zyh", zyh));
			pars.Add(new SqlParameter("@orgId", orgId));
			return this.FirstOrDefault<InpatientSettYbPatInfoVO>(sql, pars.ToArray());
		}
		public decimal GetInpatSettFeeSum(string zyh, string orgId, string sczt, string xmmc, DateTime? kssj = null, DateTime? jssj = null)
		{
			string sql = @"select isnull(sum(je),0.00) je from(
select je from(
select CONVERT(numeric(10,2),ypjfb.dj*ypjfb.sl) je,case ISNULL(fymx.feedetl_sn,'0') when '0' then '未上传' else '已上传' end  ybsczt from zy_ypjfb (NOLOCK) ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp (NOLOCK) yp ON yp.ypCode = ypjfb.yp    
										AND yp.zt = '1'    
										AND yp.OrganizeId = ypjfb.OrganizeId
left join Drjk_zyfymxsc_input (NOLOCK) fymx on SUBSTRING(fymx.feedetl_sn,3,50)=ypjfb.jfbbh and replace(fymx.zyh,'_t','')=ypjfb.zyh and fymx.zt='1'  
 where ypjfb.zyh = @zyh and ypjfb.OrganizeId = @orgId and ypjfb.zt= '1'
 and tdrq < @jssj  and isnull(yp.ypmc,'') like @xmmc
  and NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.ypjfbbh,a.OrganizeId,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm AND jsmx.OrganizeId = a.OrganizeId
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1' ) m WHERE (ypjfb.jfbbh=m.ypjfbbh or ypjfb.cxzyjfbbh=m.ypjfbbh) AND ypjfb.OrganizeId=m.OrganizeId  AND ypjfb.zyh=m.zyh
										)
) yp where (ybsczt=@sczt or @sczt='') 
union all
select je from (
 select CONVERT(numeric(10,2),xmjfb.dj*xmjfb.sl) je, case ISNULL(fymx.feedetl_sn,'0') when '0' then '未上传' else '已上传' end ybsczt from zy_xmjfb (NOLOCK) xmjfb
 LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm (NOLOCK) sfxm ON sfxm.sfxmCode = xmjfb.sfxm    
                                                      AND sfxm.zt = '1'    
                                                      AND sfxm.OrganizeId = xmjfb.OrganizeId
left join Drjk_zyfymxsc_input (NOLOCK) fymx on SUBSTRING(fymx.feedetl_sn,3,50)=xmjfb.jfbbh and replace(fymx.zyh,'_t','')=xmjfb.zyh and fymx.zt='1'            
 where xmjfb.zyh = @zyh and xmjfb.OrganizeId =@orgId and xmjfb.zt= '1'
 and xmjfb.tdrq < @jssj and isnull(sfxm.sfxmmc,'') like @xmmc
  and  NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.xmjfbbh,a.OrganizeId,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm
                                WHERE  a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1')m WHERE (xmjfb.jfbbh=m.xmjfbbh or xmjfb.cxzyjfbbh=m.xmjfbbh) AND xmjfb.OrganizeId=m.OrganizeId  AND xmjfb.zyh=m.zyh )

) xm where (ybsczt=@sczt or @sczt='') 
) hz";
			var parlist = new List<SqlParameter>();
			parlist.Add(new SqlParameter("@zyh", zyh.Trim()));
			parlist.Add(new SqlParameter("@orgId", orgId.Trim()));
			//parlist.Add(new SqlParameter("@kssj", kssj));
			parlist.Add(new SqlParameter("@jssj", jssj));
			parlist.Add(new SqlParameter("@sczt", sczt ?? ""));
			parlist.Add(new SqlParameter("@xmmc", "%" + xmmc + "%"));
			return this.FirstOrDefault<decimal>(sql.ToString(), parlist.ToArray());
		}

		/// <summary>
		/// 治疗项目计费表（zy_xmjfb）
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="OrganizeId"></param>
		/// <returns></returns>
		public List<TreatmentItemFeeDetailVO> SelectTreatmentItemList(string zyh, string orgId, DateTime? kssj, DateTime? jssj, string ks = null)
		{
			StringBuilder sqlStr = new StringBuilder();
			var parlist = new List<SqlParameter>();
			sqlStr.Append(@" SELECT '0' as isYp, xmjfb.jfbbh ,
                            xmjfb.jfdw dw ,
                            xmjfb.dj ,
                            xmjfb.sfxm ,
                            sfxm.sfxmmc ,
                            xmjfb.dl ,
                            sfdl.dlmc ,
                            xmjfb.CreateTime ,
                            xmjfb.jmje ,
                            xmjfb.jmbl ,
                            xmjfb.zfbl ,
                            xmjfb.zfxz ,
                            xmjfb.ybbm ,
                            xmjfb.tdrq ,
                            xmjfb.sl ,
                            xmjfb.sl tsl ,
                            xmjfb.je,
                            --( ISNULL(xmjfb.dj, 0) * ISNULL(xmjfb.sl, 0) ) AS je,
                            (select top 1 sl from zy_xmjfb where jfbbh=xmjfb.jfbbh) ylsl
                    FROM    [dbo].[V_C_Sys_HbtfZyXmjfb] xmjfb
                                                LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfdl sfdl ON xmjfb.dl = sfdl.dlCode
                                                                                                  AND sfdl.OrganizeId = @orgId
                                                LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON xmjfb.sfxm = sfxm.sfxmCode
                                                                                            AND sfxm.OrganizeId = @orgId
                    WHERE   xmjfb.OrganizeId = @orgId and xmjfb.zyh=@zyh  ");
			if (kssj.HasValue)
			{
				//sqlStr.Append(" AND xmjfb.createtime >= @kssj");
				sqlStr.Append(" AND xmjfb.tdrq >= @kssj");
				parlist.Add(new SqlParameter("@kssj", kssj));
			}
			if (jssj.HasValue)
			{
				sqlStr.Append(" AND xmjfb.tdrq < @jssj");
				parlist.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
			}

			if (!string.IsNullOrWhiteSpace(ks))
			{
				sqlStr.Append(" AND xmjfb.ks =@ks");
				parlist.Add(new SqlParameter("@ks", ks));
			}
			parlist.Add(new SqlParameter("@zyh", zyh.Trim()));
			parlist.Add(new SqlParameter("@orgId", orgId.Trim()));
			var list = this.FindList<TreatmentItemFeeDetailVO>(sqlStr.ToString(), parlist.ToArray()).ToList();
			return list;
		}

		/// <summary>
		/// 获取未收费项目计费
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <param name="kssj"></param>
		/// <param name="jssj"></param>
		/// <param name="ks"></param>
		/// <returns></returns>
		public List<TreatmentItemFeeDetailVO> SelectWsfItemList(string zyh, string orgId, DateTime? kssj, DateTime? jssj, string ks = null, string xmlb = null, string xmmc = null)
		{
			StringBuilder sqlStr = new StringBuilder();
			var parlist = new List<SqlParameter>();
			sqlStr.Append(@" SELECT '0' as isYp, xmjfb.jfbbh ,yzwym,
                            xmjfb.jfdw dw ,
                            xmjfb.dj ,
                            xmjfb.sfxm ,
                            sfxm.sfxmmc ,
                            xmjfb.dl ,
                            sfdl.dlmc dlmc,
                            sfmb.sfmbmc ,
                            convert(varchar(80),isnull(''+sfmb.sfmbmc+'|','')+sfdl.dlmc) dlmc ,
                            xmjfb.CreateTime ,
                            xmjfb.jmje ,
                            xmjfb.jmbl ,
                            xmjfb.zfbl ,
                            xmjfb.zfxz ,
                            xmjfb.ybbm ,
                            xmjfb.tdrq ,
                            xmjfb.sl ,
                            xmjfb.sl tsl ,
                            ( ISNULL(xmjfb.dj, 0) * ISNULL(xmjfb.sl, 0) ) AS je,
                            (select top 1 sl from zy_xmjfb where jfbbh=xmjfb.jfbbh) ylsl,sfmb.ztbh,sfmb.dcztbs,sfmb.ztsl ztylsl,xmjfb.ztsl,
							case when lsyz.yzlx in ('6','7') then lsyz.ztmc else lsyz.xmmc end as yzmc
                    FROM    [dbo].[V_C_Sys_WsfZyXmjfb] xmjfb
                                                LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfdl sfdl ON xmjfb.dl = sfdl.dlCode
                                                                                                  AND sfdl.OrganizeId = @orgId
                                                LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON xmjfb.sfxm = sfxm.sfxmCode
                                                                                            AND sfxm.OrganizeId = @orgId
LEFT JOIN (select a.ztbh,b.sfmbmc,b.sfmb,'收费项目组合'sfmblb,a.jfbbh,a.dcztbs,a.ztsl from zy_xmjfb a  with(nolock)
left join xt_sfmb b on a.ztbh=b.sfmbbh and a.OrganizeId=b.OrganizeId  where a.zyh=@zyh and a. ztbh is not null) sfmb on sfmb.jfbbh=xmjfb.jfbbh
LEFT JOIN Newtouch_CIS..zy_lsyz lsyz on (lsyz.Id = xmjfb.yzwym)
                    WHERE   xmjfb.OrganizeId = @orgId and xmjfb.zyh=@zyh  ");
			if (kssj.HasValue)
			{
				sqlStr.Append(" AND xmjfb.tdrq >= @kssj");
				parlist.Add(new SqlParameter("@kssj", kssj));
			}
			if (jssj.HasValue)
			{
				sqlStr.Append(" AND xmjfb.tdrq < @jssj");
				parlist.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
			}

			if (!string.IsNullOrWhiteSpace(ks))
			{
				sqlStr.Append(" AND xmjfb.ks =@ks");
				parlist.Add(new SqlParameter("@ks", ks));
			}
			if (!string.IsNullOrWhiteSpace(xmlb))
			{
				sqlStr.Append(" AND (sfdl.dlmc like @xmlb  or [NewtouchHIS_Base].[dbo].[fun_getPY](sfdl.dlmc) like @xmlb)");
				parlist.Add(new SqlParameter("@xmlb", "%" + xmlb + "%"));
			}
			if (!string.IsNullOrWhiteSpace(xmmc))
			{
				sqlStr.Append(" AND (sfxm.sfxmmc like @xmmc  or [NewtouchHIS_Base].[dbo].[fun_getPY](sfxm.sfxmmc) like @xmmc)");
				parlist.Add(new SqlParameter("@xmmc", "%" + xmmc + "%"));
			}

			parlist.Add(new SqlParameter("@zyh", zyh.Trim()));
			parlist.Add(new SqlParameter("@orgId", orgId.Trim()));
			var list = this.FindList<TreatmentItemFeeDetailVO>(sqlStr.ToString(), parlist.ToArray()).ToList();
			return list;
		}
		/// <summary>
		/// 药品计费表
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public List<DrugFeeDetailVO> SelectDrugList(string zyh, string orgId, DateTime? kssj = null, DateTime? jssj = null, string ks = null)
		{
			StringBuilder sqlStr = new StringBuilder();
			var parlist = new List<SqlParameter>();
			sqlStr.Append(@"SELECT '1' as isYp, 
                            ypjfb.jfbbh ,
                            ypjfb.jfdw dw ,
                            ypjfb.dj ,
                            ypjfb.yp sfxm ,
                            yp.ypmc sfxmmc ,
                            ypjfb.dl ,
                            sfdl.dlmc ,
                            ypjfb.CreateTime ,
                            ypjfb.jmje ,
                            ypjfb.jmbl ,
                            isnull(ypjfb.zfbl,0) zfbl,
                            ypjfb.zfxz ,
                            ypjfb.ybbm ,
                            ypjfb.tdrq ,
                            ypjfb.sl ,
                            ypjfb.sl tsl ,
                            ypjfb.je,
                            --( ISNULL(ypjfb.dj, 0) * ISNULL(ypjfb.sl, 0) ) AS je,
							(select top 1 sl from zy_ypjfb where jfbbh=ypjfb.jfbbh) ylsl
                    FROM    [dbo].[V_C_Sys_HbtfZyYpjfb] ypjfb
                            LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfdl sfdl ON ypjfb.dl = sfdl.dlCode
                                                                              AND sfdl.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_yp yp ON ypjfb.yp = yp.ypCode
                                                                        AND yp.OrganizeId = @orgId
                            WHERE   ypjfb.OrganizeId = @orgId and ypjfb.zyh=@zyh  ");
			if (kssj.HasValue)
			{
				//sqlStr.Append(" AND ypjfb.createtime >= @kssj");
				sqlStr.Append(" AND ypjfb.tdrq >= @kssj");
				parlist.Add(new SqlParameter("@kssj", kssj));
			}
			if (jssj.HasValue)
			{
				sqlStr.Append(" AND ypjfb.tdrq < @jssj");
				parlist.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
			}
			if (!string.IsNullOrWhiteSpace(ks))
			{
				sqlStr.Append(" AND ypjfb.ks =@ks");
				parlist.Add(new SqlParameter("@ks", ks));
			}
			parlist.Add(new SqlParameter("@zyh", zyh.Trim()));
			parlist.Add(new SqlParameter("@orgId", orgId.Trim()));
			var list = this.FindList<DrugFeeDetailVO>(sqlStr.ToString(), parlist.ToArray()).ToList();
			return list;
		}

		/// <summary>
		/// 获取未收费药品计费表
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public List<DrugFeeDetailVO> SelectWsfDrugList(string zyh, string orgId, DateTime? kssj = null, DateTime? jssj = null, string ks = null)
		{
			StringBuilder sqlStr = new StringBuilder();
			var parlist = new List<SqlParameter>();
			sqlStr.Append(@"SELECT '1' as isYp, 
                            ypjfb.jfbbh ,yzywm,
                            ypjfb.jfdw dw ,
                            ypjfb.dj ,
                            ypjfb.yp sfxm ,
                            yp.ypmc sfxmmc ,
                            ypjfb.dl ,
                            sfdl.dlmc ,
                            ypjfb.CreateTime ,
                            ypjfb.jmje ,
                            ypjfb.jmbl ,
                            isnull(ypjfb.zfbl,0) zfbl,
                            ypjfb.zfxz ,
                            ypjfb.ybbm ,
                            ypjfb.tdrq ,
                            ypjfb.sl ,
                            ypjfb.sl tsl ,
                            ( ISNULL(ypjfb.dj, 0) * ISNULL(ypjfb.sl, 0) ) AS je,
							(select top 1 sl from zy_ypjfb where jfbbh=ypjfb.jfbbh) ylsl
                    FROM    [dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
                            LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfdl sfdl ON ypjfb.dl = sfdl.dlCode
                                                                              AND sfdl.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_yp yp ON ypjfb.yp = yp.ypCode
                                                                        AND yp.OrganizeId = @orgId
                            WHERE   ypjfb.OrganizeId = @orgId and ypjfb.zyh=@zyh  ");
			if (kssj.HasValue)
			{
				sqlStr.Append(" AND ypjfb.tdrq >= @kssj");
				parlist.Add(new SqlParameter("@kssj", kssj));
			}
			if (jssj.HasValue)
			{
				sqlStr.Append(" AND ypjfb.tdrq < @jssj");
				parlist.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
			}
			if (!string.IsNullOrWhiteSpace(ks))
			{
				sqlStr.Append(" AND ypjfb.ks =@ks");
				parlist.Add(new SqlParameter("@ks", ks));
			}
			parlist.Add(new SqlParameter("@zyh", zyh.Trim()));
			parlist.Add(new SqlParameter("@orgId", orgId.Trim()));
			var list = this.FindList<DrugFeeDetailVO>(sqlStr.ToString(), parlist.ToArray()).ToList();
			return list;
		}
		/// <summary>
		/// 获取未收费药品计费表
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public List<DrugFeeDetailVO> SelectWsfDrugList(string zyh, string orgId, DateTime? kssj = null, DateTime? jssj = null, string ks = null, string xmlb = null, string xmmc = null)
		{
			StringBuilder sqlStr = new StringBuilder();
			var parlist = new List<SqlParameter>();
			sqlStr.Append(@"SELECT '1' as isYp, 
                            ypjfb.jfbbh ,
                            ypjfb.jfdw dw ,
                            ypjfb.dj ,
                            ypjfb.yp sfxm ,
                            yp.ypmc sfxmmc ,
                            ypjfb.dl ,
                            sfdl.dlmc ,
                            ypjfb.CreateTime ,
                            ypjfb.jmje ,
                            ypjfb.jmbl ,
                            isnull(ypjfb.zfbl,0) zfbl,
                            ypjfb.zfxz ,
                            ypjfb.ybbm ,
                            ypjfb.tdrq ,
                            ypjfb.sl ,
                            ypjfb.sl tsl ,
                            ( ISNULL(ypjfb.dj, 0) * ISNULL(ypjfb.sl, 0) ) AS je,
							(select top 1 sl from zy_ypjfb where jfbbh=ypjfb.jfbbh) ylsl
                    FROM    [dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
                            LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfdl sfdl ON ypjfb.dl = sfdl.dlCode
                                                                              AND sfdl.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_yp yp ON ypjfb.yp = yp.ypCode
                                                                        AND yp.OrganizeId = @orgId
                            WHERE   ypjfb.OrganizeId = @orgId and ypjfb.zyh=@zyh  ");
			if (kssj.HasValue)
			{
				sqlStr.Append(" AND ypjfb.tdrq >= @kssj");
				parlist.Add(new SqlParameter("@kssj", kssj));
			}
			if (jssj.HasValue)
			{
				sqlStr.Append(" AND ypjfb.tdrq < @jssj");
				parlist.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
			}
			if (!string.IsNullOrWhiteSpace(ks))
			{
				sqlStr.Append(" AND ypjfb.ks =@ks");
				parlist.Add(new SqlParameter("@ks", ks));
			}
			if (!string.IsNullOrWhiteSpace(xmlb))
			{
				sqlStr.Append(" AND (sfdl.dlmc like @xmlb or [NewtouchHIS_Base].[dbo].[fun_getPY](sfdl.dlmc) like @xmlb)");
				parlist.Add(new SqlParameter("@xmlb", "%" + xmlb + "%"));
			}
			if (!string.IsNullOrWhiteSpace(xmmc))
			{
				sqlStr.Append(" AND (yp.ypmc like @xmmc or [NewtouchHIS_Base].[dbo].[fun_getPY](yp.ypmc) like @xmmc)");
				parlist.Add(new SqlParameter("@xmmc", "%" + xmmc + "%"));
			}
			parlist.Add(new SqlParameter("@zyh", zyh.Trim()));
			parlist.Add(new SqlParameter("@orgId", orgId.Trim()));
			var list = this.FindList<DrugFeeDetailVO>(sqlStr.ToString(), parlist.ToArray()).ToList();
			return list;
		}
		/// <summary>
		/// 非治疗项目计费表
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="OrganizeId"></param>
		/// <returns></returns>
		public List<NonTreatmentItemFeeDetailVO> SelectNonTreatmentItemList(string zyh, string orgId)
		{
			StringBuilder sqlStr = new StringBuilder();
			sqlStr.Append(@"
--isYP 是否是药品 0：非 1：是
SELECT '0' as isYP,*
FROM 
    (SELECT *
    FROM 
        (SELECT jfbid as fzlxmjfbId,
         sum(sl) AS sl,
         sum(sl) AS tsl,
         sum(je) AS je
        FROM ( select
            CASE
            WHEN (cxjfbid is NOT null) THEN
            cxjfbid
            WHEN (cxjfbid is null) THEN
            jfbid
            END jfbid,sl,je
        FROM [xt_fzlxmjfb]
        WHERE OrganizeId=@orgId and zyh = @zyh) AS a
        GROUP BY  jfbid )as b
        WHERE sl<>0 ) AS c
    LEFT JOIN 
    (SELECT fzlxmjfb.jfbId,
        sfxm.dw,
        sfxm.dj,
        fzlxmjfb.sfxmCode,
        sfxm.sfxmmc,
        fzlxmjfb.dlCode dl,
        sfdl.dlmc,
        fzlxmjfb.CreateTime
    FROM xt_fzlxmjfb fzlxmjfb
    LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfdl sfdl
        ON fzlxmjfb.dlCode = sfdl.dlCode
            AND sfdl.OrganizeId = @orgId
    LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfxm sfxm
        ON fzlxmjfb.sfxmCode = sfxm.sfxmCode
            AND sfxm.OrganizeId = @orgId
    WHERE fzlxmjfb.sl>0
            AND fzlxmjfb.zyh = @zyh
            AND fzlxmjfb.OrganizeId = @orgId ) AS d
    ON d.jfbId=c.fzlxmjfbId 
                        ");
			SqlParameter[] param =
				{
					new SqlParameter("@zyh",zyh),
					new SqlParameter("@orgId",orgId)
				};
			var list = this.FindList<NonTreatmentItemFeeDetailVO>(sqlStr.ToString(), param).ToList();
			return list;
		}

		/// <summary>
		/// 保存结算
		/// </summary>
		/// <param name="settpatInfo"></param>
		/// <param name="settleItemsBo"></param>
		/// <param name="expectedcyrq"></param>
		/// <param name="orgId"></param>
		/// <param name="fph"></param>
		/// <param name="feeRelated">金额及支付信息</param>
		/// <param name="ybfeeRelated">医保相关费用信息</param>
		public void SaveSett(InpatientSettPatInfoVO settpatInfo, InpatientSettleItemBO settleItemsBo, DateTime expectedcyrq, string orgId, string fph, InpatientSettFeeRelatedDTO feeRelated
			, CQZyjs05Dto ybfeeRelated, S14OutResponseDTO xnhfeeRelated
			, string outTradeNo, string jslx, out int jsnm)
		{
			jsnm = 0;
			bool updateZybrjbxx = false;
			//根据zyh获取病人信息
			HospPatientBasicInfoEntity brjbxx = _hospPatientBasicInfoRepo.GetInpatientInfoByZyh(settpatInfo.zyh, orgId);
			//保存变更日志老记录
			HospPatientBasicInfoEntity oldbrjbxx = null;
			var medicalInsurance = _sysConfigRepo.GetValueByCode("Inpatient_MedicalInsurance", orgId);
			if (brjbxx == null)
			{
				throw new FailedCodeException("OUTPAT_NO_PATIENT_INFORMATION");
			}
			oldbrjbxx = brjbxx.Clone();
			brjbxx.zybz = ((int)EnumZYBZ.Ycy).ToString();
			brjbxx.cyrq = expectedcyrq;
			brjbxx.cyjdrq = DateTime.Now;   //应该是当前
			brjbxx.cyjdry = OperatorProvider.GetCurrent().UserCode;

			updateZybrjbxx = true;

			using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
			{
				//变更住院病人信息
				if (updateZybrjbxx)
				{
					brjbxx.Modify();
					db.Update(brjbxx);
				}

				//zy_js
				HospSettlementEntity jszbEntity = new HospSettlementEntity();
				jszbEntity.jsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_js");
				jszbEntity.OrganizeId = orgId;
				jszbEntity.zyh = settpatInfo.zyh;
				jszbEntity.brxz = settpatInfo.brxz;
				jszbEntity.zyts = DateTimeHelper.GetInHospDays(settpatInfo.ryrq.Value, expectedcyrq);
				jszbEntity.zje = feeRelated.zje.Value;
				jszbEntity.xjzf = feeRelated.xjzfys.Value;
				jszbEntity.fph = fph;
				jszbEntity.jszt = ((int)EnumJieSuanZT.YJ).ToString();
				jszbEntity.jsxz = "1";   //1:出院结算 2：中途结算
				jszbEntity.jsksrq = settpatInfo.ryrq;  //只对中途结算有意义
				jszbEntity.jsjsrq = expectedcyrq;      //只对中途结算有意义
				jszbEntity.ysk = feeRelated.ssk;    //预收款100  100 找零20
				jszbEntity.zl = feeRelated.zhaoling;   //找零20
				jszbEntity.OutTradeNo = outTradeNo;
                jszbEntity.zkbl = feeRelated.zkbl;
                jszbEntity.jmje = feeRelated.jmje;
                jszbEntity.ybjslsh = feeRelated.ybjslsh;

                jszbEntity.Create();
				db.Insert(jszbEntity);

				UpdateCurrentFinancialInvoice(db, orgId, fph);
				if (medicalInsurance == "chongqing")
				{
					var S23Entity =
						_iCqybSett23Repo.FindEntity(p => p.dyjylsh == feeRelated.dyjylsh && p.zt == "1");
					if (S23Entity != null)
					{
						S23Entity.jsnm = jszbEntity.jsnm;
						S23Entity.Modify();
						db.Update(S23Entity);
					}
				}
				decimal mxzje = 0;

				//zy_jsmx (zy_xmjfb) 
				foreach (var item in settleItemsBo.TreatmentItemList)
				{
					HospSettlementDetailEntity zyjsmx = new HospSettlementDetailEntity();
					zyjsmx.jsmxbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jsmx");
					zyjsmx.OrganizeId = orgId;
					zyjsmx.jsnm = jszbEntity.jsnm;
					zyjsmx.xmjfbbh = item.jfbbh;
					zyjsmx.yzlx = ((int)EnumYiZhuXZ.XM).ToString();
					if (feeRelated.ver == "2")
					{
						zyjsmx.jyje = Math.Round(item.je, 2, MidpointRounding.AwayFromZero);
					}
					else
					{
						zyjsmx.jyje = Math.Round(item.dj * item.sl, 2, MidpointRounding.AwayFromZero);
					}
					zyjsmx.Create();
					db.Insert(zyjsmx);
					//
					mxzje += zyjsmx.jyje.Value;
				}

				//zy_jsmx (zy_ypjfb)
				foreach (var item in settleItemsBo.DrugList)
				{
					HospSettlementDetailEntity zyjsmx = new HospSettlementDetailEntity();
					zyjsmx.jsmxbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jsmx");
					zyjsmx.OrganizeId = orgId;
					zyjsmx.jsnm = jszbEntity.jsnm;
					zyjsmx.ypjfbbh = item.jfbbh;
					zyjsmx.yzlx = ((int)EnumYiZhuXZ.YP).ToString();
					if (feeRelated.ver == "2")
					{
						zyjsmx.jyje = Math.Round(item.je, 2, MidpointRounding.AwayFromZero);
					}
					else
					{
						zyjsmx.jyje = Math.Round(item.dj * item.sl, 2, MidpointRounding.AwayFromZero);
					}
					zyjsmx.Create();
					db.Insert(zyjsmx);
					//
					mxzje += zyjsmx.jyje.Value;
				}
				//////////////不再验证，无意义，一旦不相等，流程回退很难
				//if (mxzje != feeRelated.zje)
				//{
				//    throw new Exception("结算总金额异常："+ mxzje.ToString()+ "不等于" + feeRelated.zje.ToString());
				//}

				if (feeRelated.orglxjzfys.HasValue && feeRelated.xjzfys.HasValue)
				{
					//要减去记账部分 等金额
					if (feeRelated.orglxjzfys.Value != jszbEntity.zje - 0)
					{

						//贵安医保
						if (medicalInsurance == "guian")
						{
							if (jslx == "1" && (ybfeeRelated == null || ybfeeRelated.calc_xjzf != feeRelated.orglxjzfys.Value))
							{
								throw new FailedException("ERROR_SETT_JE_ERROR", "结算金额异常:" + ybfeeRelated.calc_xjzf.ToString() + "不等于" + feeRelated.orglxjzfys.Value.ToString());
							}
						}
						if (medicalInsurance == "chongqing")
						{
							//if (jslx == "1" && (ybfeeRelated == null || ybfeeRelated.xjzf != feeRelated.orglxjzfys.Value))
							//{
							//	throw new FailedException("ERROR_SETT_JE_ERROR", "结算金额异常:" + ybfeeRelated.calc_xjzf.ToString() + "不等于" + feeRelated.orglxjzfys.Value.ToString());
							//}
						}
					}
				}
				else
				{
					//???xjzf有必要赋值么
					//要减去记账部分 等金额
					jszbEntity.xjzf = jszbEntity.zje - 0;
				}

				if (feeRelated.ssk.HasValue && feeRelated.zhaoling.HasValue)
				{
                    jszbEntity.xjwc = (decimal)feeRelated.xjwc;
                    //现金误差 收到的-应收的
                    //jszbEntity.xjwc = jszbEntity.xjzf - (feeRelated.ssk.Value - feeRelated.zhaoling.Value);
                    //if (Math.Abs(jszbEntity.xjwc) >= (decimal)0.1)
                    //{
                    //	throw new FailedException("ERROR_SSK_ZHAOLING", "实收找零金额异常");
                    //}
                }

				if (jslx == "1" && ybfeeRelated != null)
				{
					if (medicalInsurance == "chongqing")
					{
						if (!string.IsNullOrEmpty(ybfeeRelated.jylsh))
						{
							//重庆医保
							//保存医保结算相关费用
							var ybFeeEntity = new CqybSett05Entity();
							ybfeeRelated.MapperTo(ybFeeEntity);
							ybFeeEntity.jslb = "3";
							ybFeeEntity.OrganizeId = orgId;
							//zxjssj = hcybfeeRelated.zxjssj;
							ybFeeEntity.jsnm = jszbEntity.jsnm;
							ybFeeEntity.Create();
							ybFeeEntity.zt = "1";
							db.Insert(ybFeeEntity);
						}
					}
				}
				PaymentModelAccountReserveII(db, jszbEntity, feeRelated);

				db.Commit();

				jsnm = jszbEntity.jsnm;
			}
																																			
		}

		/// <summary>
		/// 获取按大类分组费用
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public IList<HospFeeChargeCategoryGroupVO> GetHospGroupFeeVOList(string zyh, string orgId, string ver)
		{
			var sql = @"select dljf.dl dlCode, sfdl.dlmc, dljf.je from (
select dl, sum(je) je from( ";
			if (ver == "2")//合并费用的合计与不合并费用合计 sum保留两位小数 存在不等情况 过渡用
			{
				sql += @" select dl, CONVERT(NUMERIC(10,2), isnull(je, 0)) je from [V_C_Sys_WsfZyXmjfb]
where zyh = @zyh and OrganizeId = @orgId and zt= '1'
union all
select dl, CONVERT(NUMERIC(10,2), isnull(je, 0) ) je from [V_C_Sys_WsfZyYpjfb]
where zyh = @zyh and OrganizeId = @orgId and zt= '1'";
			}
			else
			{
				sql += @" select dl, CONVERT(NUMERIC(10,2), isnull(dj, 0) * isnull(sl, 0)) je from [V_C_Sys_WsfZyXmjfb]
where zyh = @zyh and OrganizeId = @orgId and zt= '1'
union all
select dl, CONVERT(NUMERIC(10,2), isnull(dj, 0) * isnull(sl, 0)) je from [V_C_Sys_WsfZyYpjfb]
where zyh = @zyh and OrganizeId = @orgId and zt= '1'";

			}
			sql += @" ) jfmx
group by dl
) as dljf
left join[NewtouchHIS_Base]..V_S_xt_sfdl sfdl
on sfdl.dlCode = dljf.dl and sfdl.OrganizeId = @orgId";
			return this.FindList<HospFeeChargeCategoryGroupVO>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@zyh", zyh) });
		}
		public IList<HospFeeChargeCategoryGroupDetailVO> GetHospGroupFeeVOList(Pagination pagination, string zyh, string orgId, string sfdl)
		{
			var sql = @"select dd.dlCode,dd.dlmc,dd.sfxm,dd.sfxmmc,Convert(decimal(18,2),dd.dj)dj,sum(dd.sl)sl,dd.jfdw,dd.zfxz,dd.zzfbz,sum(dd.je)je
from(
select dl dlCode,b.dlmc,sfxm,c.sfxmmc,a.dj,sl,jfdw,
(case a.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10,2), isnull(a.dj, 0) * isnull(sl, 0)) je ,c.gg ,case isnull(zzfbz,0) when '1' then '是' else '否' end zzfbz
from [V_C_Sys_WsfZyXmjfb] a
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl b with(nolock) on a.OrganizeId=b.OrganizeId and a.dl=b.dlCode and b.zt='1'
left join NewtouchHIS_Base.dbo.V_S_xt_sfxm c with(nolock) on a.OrganizeId=c.OrganizeId and a.sfxm=c.sfxmcode and c.zt='1'
where zyh =@zyh and a.OrganizeId =@orgId and b.zt= '1'
union all
select dl dlCode,bb.dlmc,yp,cc.ypmc,aa.dj,sl,jfdw,
(case aa.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10,2), isnull(aa.dj, 0) * isnull(sl, 0)) je ,cc.ypgg,case isnull(zzfbz,0) when '1' then '是' else '否' end zzfbz
from [V_C_Sys_WsfZyYpjfb] aa
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl bb with(nolock) on aa.OrganizeId=bb.OrganizeId and aa.dl=bb.dlCode and bb.zt='1'
left join NewtouchHIS_Base.dbo.V_C_xt_yp cc with(nolock) on aa.OrganizeId=cc.OrganizeId and aa.yp=cc.ypcode and cc.zt='1'
where zyh = @zyh and aa.OrganizeId = @orgId and bb.zt= '1'
)dd
group by  dd.dlCode,dd.dlmc,dd.sfxm,dd.sfxmmc,dd.dj,dd.jfdw,dd.zfxz,dd.zzfbz
";
			if (!string.IsNullOrEmpty(sfdl))
			{
				sql = "select * from (" + sql + ") r where r.dlCode=@dl ";
			}
			return this.QueryWithPage<HospFeeChargeCategoryGroupDetailVO>(sql, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@dl", sfdl??"")
			});
		}
		public IList<HospFeeChargeCategoryGroupDetailVO> GetHospGroupFeeVOData(Pagination pagination, string zyh, string orgId, string keyword = "")
		{
			var sql = @"select dd.dlCode,dd.dlmc,dd.sfxm,dd.sfxmmc,py,dd.dj,sum(dd.sl)sl,dd.jfdw,dd.zfxz,dd.zzfbz,sum(dd.je)je
from(
select dl dlCode,b.dlmc,sfxm,c.sfxmmc,a.dj,sl,jfdw,c.py,
(case a.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10,2), isnull(a.dj, 0) * isnull(sl, 0)) je ,c.gg ,case isnull(zzfbz,0) when '1' then '是' else '否' end zzfbz
from [V_C_Sys_WsfZyXmjfb] a
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl b with(nolock) on a.OrganizeId=b.OrganizeId and a.dl=b.dlCode and b.zt='1'
left join NewtouchHIS_Base.dbo.V_S_xt_sfxm c with(nolock) on a.OrganizeId=c.OrganizeId and a.sfxm=c.sfxmcode and c.zt='1'
where zyh =@zyh and a.OrganizeId =@orgId and b.zt= '1'
union all
select dl dlCode,bb.dlmc,yp,cc.ypmc,aa.dj,sl,jfdw,cc.py,
(case aa.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10,2), isnull(aa.dj, 0) * isnull(sl, 0)) je ,cc.ypgg,case isnull(zzfbz,0) when '1' then '是' else '否' end zzfbz
from [V_C_Sys_WsfZyYpjfb] aa
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl bb with(nolock) on aa.OrganizeId=bb.OrganizeId and aa.dl=bb.dlCode and bb.zt='1'
left join NewtouchHIS_Base.dbo.V_C_xt_yp cc with(nolock) on aa.OrganizeId=cc.OrganizeId and aa.yp=cc.ypcode and cc.zt='1'
where zyh = @zyh and aa.OrganizeId = @orgId and bb.zt= '1'
)dd
group by  dd.dlCode,dd.dlmc,dd.sfxm,dd.sfxmmc,dd.dj,dd.jfdw,dd.zfxz,dd.zzfbz,py
";
			if (!string.IsNullOrEmpty(keyword))
			{
				sql = "select * from (" + sql + ") r where r.dlCode like @keyword or  r.py like @keyword or r.sfxmmc like @keyword ";
			}
			return this.QueryWithPage<HospFeeChargeCategoryGroupDetailVO>(sql, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@keyword", "%" + keyword.Trim() + "%")
			});
		}
		/// <summary>
		/// 获取收费项目明细
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <param name="sfxmCode"></param>
		/// <returns></returns>
		public IList<HospFeeChargeCategoryGroupDetailVO> GetDetailedQuery(Pagination pagination, string zyh, string orgId, string sfxmCode, string zfbz, decimal? dj)
		{
			var sql = @"select * from(
select dl dlCode,b.dlmc,convert(varchar(10),tdrq,120)tdrq,sfxm,c.sfxmmc,a.dj,sl,jfdw,
(case a.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10, 2), isnull(a.dj, 0) * isnull(sl, 0)) je ,c.gg ,
case isnull(zzfbz, 0) when '1' then '是' else '否' end zzfbz
from[V_C_Sys_WsfZyXmjfb] a
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl b with(nolock) on a.OrganizeId = b.OrganizeId and a.dl = b.dlCode and b.zt = '1'
left join NewtouchHIS_Base.dbo.V_S_xt_sfxm c with(nolock) on a.OrganizeId = c.OrganizeId and a.sfxm = c.sfxmcode and c.zt = '1'
where zyh = @zyh and a.OrganizeId = @orgId and b.zt = '1' and sfxm = @sfxmCode  and Convert(decimal(18,2),a.dj)= @dj
union all
select dl dlCode,bb.dlmc,convert(varchar(10), tdrq, 120)tdrq,yp,cc.ypmc,aa.dj,sl,jfdw,
(case aa.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10, 2), isnull(aa.dj, 0) * isnull(sl, 0)) je ,cc.ypgg,
case isnull(zzfbz, 0) when '1' then '是' else '否' end zzfbz
from[V_C_Sys_WsfZyYpjfb] aa
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl bb with(nolock) on aa.OrganizeId = bb.OrganizeId and aa.dl = bb.dlCode and bb.zt = '1'
left join NewtouchHIS_Base.dbo.V_C_xt_yp cc with(nolock) on aa.OrganizeId = cc.OrganizeId and aa.yp = cc.ypcode and cc.zt = '1'
where zyh = @zyh and aa.OrganizeId = @orgId and bb.zt = '1' and yp = @sfxmCode and Convert(decimal(18,2),aa.dj)= @dj) dd
where dd.zzfbz=@zfbz 
"
;
			return this.QueryWithPage<HospFeeChargeCategoryGroupDetailVO>(sql, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@sfxmCode", sfxmCode??""),
				new SqlParameter("@zfbz", zfbz??""),
				new SqlParameter("@dj", dj)
			});
		}
		/// <summary>
		/// 获取收费项目明细
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <param name="sfxmCode"></param>V
		/// <returns></returns>
		public IList<HospFeeChargeCategoryGroupDetailVO> GetFyzdDetailedQuery(Pagination pagination, string zyh, string orgId, string sfxmCode)
		{
			var sql = @"select dl dlCode,b.dlmc,convert(varchar(10),tdrq,120)tdrq,sfxm,c.sfxmmc,a.dj,sl,jfdw,
(case a.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10, 2), isnull(a.dj, 0) * isnull(sl, 0)) je ,c.gg ,case isnull(zzfbz, 0) when '1' then '是' else '否' end zzfbz
from[V_C_Sys_WsfZyXmjfb] a
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl b with(nolock) on a.OrganizeId = b.OrganizeId and a.dl = b.dlCode and b.zt = '1'
left join NewtouchHIS_Base.dbo.V_S_xt_sfxm c with(nolock) on a.OrganizeId = c.OrganizeId and a.sfxm = c.sfxmcode and c.zt = '1'
where zyh = @zyh and a.OrganizeId = @orgId and b.zt = '1' and sfxm = @sfxmCode
union all
select dl dlCode,bb.dlmc,convert(varchar(10), tdrq, 120)tdrq,yp,cc.ypmc,aa.dj,sl,jfdw,
(case aa.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' end)zfxz,
CONVERT(NUMERIC(10, 2), isnull(aa.dj, 0) * isnull(sl, 0)) je ,cc.ypgg,case isnull(zzfbz, 0) when '1' then '是' else '否' end zzfbz
from[V_C_Sys_WsfZyYpjfb] aa
left join NewtouchHIS_Base.dbo.V_S_xt_sfdl bb with(nolock) on aa.OrganizeId = bb.OrganizeId and aa.dl = bb.dlCode and bb.zt = '1'
left join NewtouchHIS_Base.dbo.V_C_xt_yp cc with(nolock) on aa.OrganizeId = cc.OrganizeId and aa.yp = cc.ypcode and cc.zt = '1'
where zyh = @zyh and aa.OrganizeId = @orgId and bb.zt = '1' and yp = @sfxmCode";
			return this.QueryWithPage<HospFeeChargeCategoryGroupDetailVO>(sql, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@sfxmCode", sfxmCode??"")
			});
		}

		/// <summary>
		/// 获取住院费用明细
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <param name="sczt"></param>
		/// <param name="kssj"></param>
		/// <param name="jssj"></param>
		/// <returns></returns>
		public IList<HospFeeUploadDetailVO> GetHospXmYpFeeVOList(Pagination pagination, string zyh, string orgId, string sczt, string xmmc, DateTime? kssj, DateTime jssj,string isnewyb)
		{
			var sql = @" select * from(
select 'YP'+ CONVERT(VARCHAR(20), ypjfb.jfbbh) jfbbh,isnull(ypjfb.cxzyjfbbh,0) cxzyjfbbh,ypjfb.tdrq,ypjfb.yp,ypjfb.ys,ypjfb.ysmc,ypjfb.ks,ypjfb.ksmc,CONVERT(numeric(10,4),ypjfb.dj) dj,CONVERT(numeric(10,4),ypjfb.sl) sl,CONVERT(numeric(10,2),ypjfb.dj*ypjfb.sl) je,
ypjfb.jfdw,yp.ypmc,yp.gjybdm,yp.ybdm,(case yp.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' else yp.zfxz end) zfxz,fymx.feedetl_sn,
case isnull(zzfbz,0) when '0' then '否' when '1' then '是' end zzfbz, case ISNULL(fymx.feedetl_sn,'0') when '0' then '未上传' else '已上传' end  ybsczt,sfdl.dlmc,fymx.zyh from zy_ypjfb (NOLOCK) ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp (NOLOCK) yp ON yp.ypCode = ypjfb.yp    
										AND yp.zt = '1'    
										AND yp.OrganizeId = ypjfb.OrganizeId
left join Drjk_zyfymxsc_input (NOLOCK) fymx on SUBSTRING(fymx.feedetl_sn,3,50)=ypjfb.jfbbh and replace(fymx.zyh,'_t','')=ypjfb.zyh and fymx.zt='1' 
left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl on sfdl.dlCode = ypjfb.dl and sfdl.OrganizeId = ypjfb.OrganizeId 
 where ypjfb.zyh =@zyh and ypjfb.OrganizeId =@orgId and ypjfb.zt= '1'
 and tdrq < @jssj  and isnull(yp.ypmc,'') like @xmmc
  and NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.ypjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm AND jsmx.OrganizeId = a.OrganizeId
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1' ) m WHERE (ypjfb.jfbbh=m.ypjfbbh or ypjfb.cxzyjfbbh=m.ypjfbbh) AND ypjfb.OrganizeId=m.OrganizeId  AND ypjfb.zyh=m.zyh

										)
) yp where (ybsczt=@sczt or @sczt='') and CHARINDEX('_t',isnull(zyh,1))=0
union all
select * from (
 select 'XM'+CONVERT(VARCHAR(20),xmjfb.jfbbh) jfbbh,isnull(xmjfb.cxzyjfbbh,0) cxzyjfbbh, tdrq,xmjfb.sfxm yp,xmjfb.ys,staff.Name ysmc,xmjfb.ks,ks.Name ksmc,xmjfb.dj,xmjfb.sl,CONVERT(numeric(10,2),xmjfb.dj*xmjfb.sl) je,
xmjfb.jfdw,sfxm.sfxmmc ypmc,sfxm.gjybdm,sfxm.ybdm,(case sfxm.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' else sfxm.zfxz end) zfxz,fymx.feedetl_sn,
case isnull(zzfbz,0) when '0' then '否' when '1' then '是' end zzfbz, case ISNULL(fymx.feedetl_sn,'0') when '0' then '未上传' else '已上传' end ybsczt, sfdl.dlmc,fymx.zyh from zy_xmjfb (NOLOCK) xmjfb
 LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm (NOLOCK) sfxm ON sfxm.sfxmCode = xmjfb.sfxm    
                                                      AND sfxm.zt = '1'    
                                                      AND sfxm.OrganizeId = xmjfb.OrganizeId
 LEFT JOIN NewtouchHIS_Base..Sys_Department (NOLOCK) ks ON ks.OrganizeId = xmjfb.OrganizeId    
                                                              AND ks.Code = xmjfb.ks    
                                                              AND ks.zt = '1' 
LEFT JOIN NewtouchHIS_Base..Sys_Staff (NOLOCK) staff ON staff.OrganizeId = xmjfb.OrganizeId    
AND staff.gh = xmjfb.ys    
AND staff.zt = '1'
left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl on sfdl.dlCode = xmjfb.dl and sfdl.OrganizeId = xmjfb.OrganizeId 
left join Drjk_zyfymxsc_input (NOLOCK) fymx on SUBSTRING(fymx.feedetl_sn,3,50)=xmjfb.jfbbh and replace(fymx.zyh,'_t','')=xmjfb.zyh and fymx.zt='1'            
 where xmjfb.zyh =@zyh and xmjfb.OrganizeId =@orgId and xmjfb.zt= '1'
 and xmjfb.tdrq < @jssj and isnull(sfxm.sfxmmc,'') like @xmmc
  and  NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.xmjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1')m WHERE (xmjfb.jfbbh=m.xmjfbbh or xmjfb.cxzyjfbbh=m.xmjfbbh) AND xmjfb.OrganizeId=m.OrganizeId AND xmjfb.zyh=m.zyh)

) xm where (ybsczt=@sczt or @sczt='') and CHARINDEX('_t',isnull(zyh,1))=0 

";
            if (isnewyb=="1")
            {
                sql = @"select * from(
select 'YP'+ CONVERT(VARCHAR(20), ypjfb.jfbbh) jfbbh,isnull(ypjfb.cxzyjfbbh,0) cxzyjfbbh,ypjfb.tdrq,ypjfb.yp,ypjfb.ys,ypjfb.ysmc,ypjfb.ks,ypjfb.ksmc,CONVERT(numeric(10,4),ypjfb.dj) dj,CONVERT(numeric(10,4),ypjfb.sl) sl,CONVERT(numeric(10,2),ypjfb.dj*ypjfb.sl) je,
ypjfb.jfdw,yp.ypmc,yp.gjybdm,yp.ybdm,(case yp.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' else yp.zfxz end) zfxz,convert(varchar(100),fymx.xh) feedetl_sn,
case isnull(zzfbz,0) when '0' then '否' when '1' then '是' end zzfbz, case ISNULL(fymx.xh,'0') when '0' then '未上传' else '已上传' end  ybsczt,sfdl.dlmc,ypjfb.zyh from zy_ypjfb (NOLOCK) ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp (NOLOCK) yp ON yp.ypCode = ypjfb.yp    
										AND yp.zt = '1'    
										AND yp.OrganizeId = ypjfb.OrganizeId
left join Ybjk_SN01_Mxxzy_Input (NOLOCK) fymx on fymx.xh=ypjfb.jfbbh and fymx.mzzyh=ypjfb.zyh and fymx.zt='1' 
left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl on sfdl.dlCode = ypjfb.dl and sfdl.OrganizeId = ypjfb.OrganizeId 
 where ypjfb.zyh =@zyh and ypjfb.OrganizeId =@orgId and ypjfb.zt= '1'
 and tdrq < @jssj  and isnull(yp.ypmc,'') like @xmmc
  and NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.ypjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm AND jsmx.OrganizeId = a.OrganizeId
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1' ) m WHERE (ypjfb.jfbbh=m.ypjfbbh or ypjfb.cxzyjfbbh=m.ypjfbbh) AND ypjfb.OrganizeId=m.OrganizeId  AND ypjfb.zyh=m.zyh

										)
) yp where (ybsczt=@sczt or @sczt='') and CHARINDEX('_t',isnull(zyh,1))=0
union all
select * from (
 select 'XM'+CONVERT(VARCHAR(20),xmjfb.jfbbh) jfbbh,isnull(xmjfb.cxzyjfbbh,0) cxzyjfbbh, tdrq,xmjfb.sfxm yp,xmjfb.ys,staff.Name ysmc,xmjfb.ks,ks.Name ksmc,xmjfb.dj,xmjfb.sl,CONVERT(numeric(10,2),xmjfb.dj*xmjfb.sl) je,
xmjfb.jfdw,sfxm.sfxmmc ypmc,sfxm.gjybdm,sfxm.ybdm,(case sfxm.zfxz when 1 then '自费' when 4 then '甲类' when 5 then '乙类' when 6 then '丙类' else sfxm.zfxz end) zfxz,convert(varchar(100),fymx.xh) feedetl_sn,
case isnull(zzfbz,0) when '0' then '否' when '1' then '是' end zzfbz, case ISNULL(fymx.xh,'0') when '0' then '未上传' else '已上传' end ybsczt, sfdl.dlmc,xmjfb.zyh from zy_xmjfb (NOLOCK) xmjfb
 LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm (NOLOCK) sfxm ON sfxm.sfxmCode = xmjfb.sfxm    
                                                      AND sfxm.zt = '1'    
                                                      AND sfxm.OrganizeId = xmjfb.OrganizeId
 LEFT JOIN NewtouchHIS_Base..Sys_Department (NOLOCK) ks ON ks.OrganizeId = xmjfb.OrganizeId    
                                                              AND ks.Code = xmjfb.ks    
                                                              AND ks.zt = '1' 
LEFT JOIN NewtouchHIS_Base..Sys_Staff (NOLOCK) staff ON staff.OrganizeId = xmjfb.OrganizeId    
AND staff.gh = xmjfb.ys    
AND staff.zt = '1'
left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl on sfdl.dlCode = xmjfb.dl and sfdl.OrganizeId = xmjfb.OrganizeId 
left join Ybjk_SN01_Mxxzy_Input (NOLOCK) fymx on xh=xmjfb.jfbbh and  fymx.mzzyh=xmjfb.zyh and fymx.zt='1'        
 where xmjfb.zyh =@zyh and xmjfb.OrganizeId =@orgId and xmjfb.zt= '1'
 and xmjfb.tdrq < @jssj and isnull(sfxm.sfxmmc,'') like @xmmc
  and  NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.xmjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1')m WHERE (xmjfb.jfbbh=m.xmjfbbh or xmjfb.cxzyjfbbh=m.xmjfbbh) AND xmjfb.OrganizeId=m.OrganizeId AND xmjfb.zyh=m.zyh)

) xm where (ybsczt=@sczt or @sczt='') and CHARINDEX('_t',isnull(zyh,1))=0 ";
            }
			return this.QueryWithPage<HospFeeUploadDetailVO>(sql, pagination, new[] {
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@zyh", zyh),
                //new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
				new SqlParameter("@sczt", sczt??""),
				new SqlParameter("@xmmc", "%"+xmmc+"%")
			});
		}
		#endregion

		#region 取消出院结算

		/// <summary>
		/// 取消出院结算
		/// </summary>
		/// <param name="preSettlEntity"></param>
		/// <param name="settlDetailList"></param>
		/// <param name="cancelReason"></param>
		/// <param name="orgId"></param>
		public void DoCancel(HospSettlementEntity preSettlEntity, List<HospSettlementDetailEntity> settlDetailList, string cancelReason,string cancelyblsh,string orgId)
		{
			bool updateZybrjbxx = false;
			//根据zyh获取病人信息
			var brjbxxList = _hospPatientBasicInfoRepo.IQueryable().Where(a => a.zt == "1" && a.zyh == preSettlEntity.zyh && a.OrganizeId == orgId && a.zybz == ((int)EnumZYBZ.Ycy).ToString()).ToList();
			if (brjbxxList == null)
			{
				throw new FailedCodeException("HOSP_INPATIENT_BASICINFO_IS_NOT_EXIST");
			}
			if (brjbxxList.Count() > 1)
			{
				throw new FailedCodeException("MOREINFO_IS_INVALID");
			}
			HospPatientBasicInfoEntity brjbxx = brjbxxList.FirstOrDefault();

			//保存变更日志老记录
			HospPatientBasicInfoEntity oldbrjbxx = null;

			if (brjbxx == null)
			{
				throw new FailedCodeException("OUTPAT_NO_PATIENT_INFORMATION");
			}
			oldbrjbxx = brjbxx.Clone();
			brjbxx.zybz = ((int)EnumZYBZ.Djz).ToString();
			//brjbxx.cyrq = null;   //不能重置
			brjbxx.cyjdrq = null;
			brjbxx.cyjdry = null;

			updateZybrjbxx = true;

			using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
			{
				//变更住院病人信息
				if (updateZybrjbxx)
				{
					brjbxx.Modify();
					db.Update(brjbxx);
					//保存变更日志
					if (oldbrjbxx != null)
					{
						AppLogger.WriteEntityChangeRecordLog(oldbrjbxx, brjbxx, HospPatientBasicInfoEntity.GetTableName(), oldbrjbxx.syxh.ToString());
					}
				}

				//insert zy_js
				var jszbEntity = new HospSettlementEntity();
				jszbEntity = preSettlEntity.Clone();
				jszbEntity.jsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_js");
				jszbEntity.cxjsnm = preSettlEntity.jsnm;
				jszbEntity.jszt = ((int)EnumJieSuanZT.YT).ToString();
				jszbEntity.cxjsyy = cancelReason;
                jszbEntity.ybjslsh = cancelyblsh;
				jszbEntity.Create();
				db.Insert(jszbEntity);

				//insert zy_jsmx
				foreach (var item in settlDetailList)
				{
					var jsmxEntity = new HospSettlementDetailEntity();
					jsmxEntity = item.Clone();
					jsmxEntity.jsmxbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jsmx");
					jsmxEntity.jsnm = jszbEntity.jsnm;
					jsmxEntity.Create();
					db.Insert(jsmxEntity);
				}

				//如果之前是预交金支付，要还款
				var preYjjPayEntity = db.IQueryable<HospSettlementPaymentModelEntity>(p => p.jsnm == preSettlEntity.jsnm && p.xjzffs == xtzffs.ZYYJZHZF).FirstOrDefault();
				if (preYjjPayEntity != null)
				{
					//给预交金账户充值
					var patid = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == jszbEntity.zyh && p.OrganizeId == jszbEntity.OrganizeId).Select(p => p.patid).FirstOrDefault();
					if (patid <= 0)
					{
						return;
					}

					decimal szje = 0;
					decimal zhye = 0;
					var payways = _sysConfigRepo.GetValueByCode("Intpatient_Sett_Open_Dzzf", orgId);
					var accountEntity =
						db.IQueryable<InpatientAccountEntity>(p => p.zyh == jszbEntity.zyh && p.zhCode == preYjjPayEntity.zh)
							.FirstOrDefault();

                    szje = preYjjPayEntity.zfje;
					if (accountEntity != null)
					{
						var zhszEntity = new InpatientAccountRevenueAndExpenseEntity()
						{
							OrganizeId = jszbEntity.OrganizeId,
							zhCode = accountEntity.zhCode,
							szje = szje,
							zhye = accountEntity.zhye + szje,
							pzh = null,
							szxz = (int)EnumSZXZ.zyjsth,
							xjzffs = Constants.xtzffs.ZYYJZHZF,
							jsnm = jszbEntity.jsnm,
							zt = "1",
						};
						zhszEntity.Create(true);
						db.Insert(zhszEntity);
						//
						accountEntity.zhye = zhszEntity.zhye;
						db.Update(accountEntity);
					}
					else
					{
						throw new FailedException("ERROR_ACCOUNT_INFO", "获取预交金账户信息失败");
					}
				}

				db.Commit();
			}
		}

		#endregion

		#region 模拟结算（GA）



		#endregion

		#region private methods

		/// <summary>
		/// 更新当前发票号
		/// </summary>
		/// <param name="db"></param>
		/// <param name="orgId"></param>
		/// <param name="fph"></param>
		private void UpdateCurrentFinancialInvoice(Infrastructure.EF.EFDbTransaction db, string orgId, string fph)
		{
			if (!string.IsNullOrWhiteSpace(fph))
			{
				//可用发票更新
				//插入/更新cw_fp
				var opr = Newtouch.Common.Operator.OperatorProvider.GetCurrent();
				if (opr != null)
				{
					FinancialInvoiceEntity fpUpdateEntity, fpInsertEntity;
					_financialInvoiceRepo.UpdateCurrentGetEntitys(fph, opr.UserCode, out fpUpdateEntity, out fpInsertEntity, orgId);
					if (fpUpdateEntity != null)
					{
						db.Update(fpUpdateEntity);
					}
					if (fpInsertEntity != null)
					{
						db.Insert(fpInsertEntity);
					}
				}
				else
				{
					throw new FailedException("FINANCIALINVOICE_UPDATE_ERROR", "发票号更新异常");
				}
			}
		}

		/// <summary>
		/// 结算支付方式、预交金支付 账户收支
		/// </summary>
		/// <param name="db"></param>
		/// <param name="jszbEntity"></param>
		/// <param name="feeRelated"></param>
		private void PaymentModelAccountReserve(Infrastructure.EF.EFDbTransaction db, HospSettlementEntity jszbEntity
			, InpatientSettFeeRelatedDTO feeRelated)
		{
			HospSettlementPaymentModelEntity zfEntity1 = null;
			HospSettlementPaymentModelEntity zfEntity2 = null;

			if (!string.IsNullOrWhiteSpace(feeRelated.zffs1))
			{
				zfEntity1 = new HospSettlementPaymentModelEntity();
				zfEntity1.zyjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jszffs");
				zfEntity1.OrganizeId = jszbEntity.OrganizeId;
				zfEntity1.jsnm = jszbEntity.jsnm;
				zfEntity1.xjzffs = feeRelated.zffs1;
				zfEntity1.zfje = feeRelated.zfje1.Value;
				zfEntity1.zt = "1";
				zfEntity1.Create();
				db.Insert(zfEntity1);
			}
			if (!string.IsNullOrWhiteSpace(feeRelated.zffs2) && (feeRelated.zfje2 ?? 0) > 0)
			{
				zfEntity2 = new HospSettlementPaymentModelEntity();
				zfEntity2.zyjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jszffs");
				zfEntity2.OrganizeId = jszbEntity.OrganizeId;
				zfEntity2.jsnm = jszbEntity.jsnm;
				zfEntity2.xjzffs = feeRelated.zffs2;
				zfEntity2.zfje = feeRelated.zfje2.Value;
				zfEntity2.zt = "1";
				zfEntity2.Create();
				zfEntity2.CreateTime = DateTime.Now.AddSeconds(1);
				db.Insert(zfEntity2);
			}

			//如果只有一种支付方式 存入js.xjzffs
			jszbEntity.xjzffs = zfEntity2 == null ? zfEntity1.xjzffs : null;

			//预交金支付 构建账户收支  //预交金支付 一定作为第一支付方式
			if (zfEntity1 != null && zfEntity1.xjzffs == xtzffs.ZYYJZHZF)
			{
				var patid = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == jszbEntity.zyh && p.OrganizeId == jszbEntity.OrganizeId).Select(p => p.patid).FirstOrDefault();
				if (patid <= 0)
				{
					return;
				}
				var accountEntity = db.IQueryable<SysAccountEntity>(p => p.patid == patid && p.zt == "1").FirstOrDefault();
				if (accountEntity != null)
				{
					zfEntity1.zh = accountEntity.zhCode;
					if (!(zfEntity1.zfje == accountEntity.zhye
						|| (zfEntity1.zfje <= feeRelated.xjzfys.Value && zfEntity1.zfje <= accountEntity.zhye)))
					{
						throw new FailedException("", "预交账户支付金额异常");
					}
					var zhszEntity = new SysAccountRevenueAndExpenseEntity()
					{
						OrganizeId = jszbEntity.OrganizeId,
						zhCode = accountEntity.zhCode,
						patid = accountEntity.patid,
						szje = 0 - Math.Min(zfEntity1.zfje, feeRelated.xjzfys.Value),
						zhye = accountEntity.zhye - Math.Min(zfEntity1.zfje, feeRelated.xjzfys.Value),
						pzh = null,
						szxz = (int)EnumSZXZ.zyjs,
						xjzffs = Constants.xtzffs.ZYYJZHZF,
						jsnm = jszbEntity.jsnm,
						zt = "1",
					};
					zhszEntity.Create(true);
					db.Insert(zhszEntity);
					//
					accountEntity.zhye = zhszEntity.zhye;
					db.Update(accountEntity);
					//预交金支付 账户收支
					if (zfEntity1.zfje > feeRelated.xjzfys.Value)
					{
						//还有一条收支取款（应该是预交金余额全退）
						var zhszEntity2 = new SysAccountRevenueAndExpenseEntity()
						{
							OrganizeId = jszbEntity.OrganizeId,
							zhCode = accountEntity.zhCode,
							patid = accountEntity.patid,
							szje = 0 - (zfEntity1.zfje - feeRelated.xjzfys.Value),
							zhye = accountEntity.zhye - (zfEntity1.zfje - feeRelated.xjzfys.Value),
							pzh = null,
							szxz = (int)EnumSZXZ.qk,
							//xjzffs = Constants.xtzffs.ZYYJZHZF,
							xjzffs = "0",   //现金
							zt = "1",
						};
						zhszEntity2.Create(true);
						zhszEntity2.CreateTime = DateTime.Now.AddSeconds(1);
						db.Insert(zhszEntity2);
						//
						accountEntity.zhye = zhszEntity2.zhye;
						//db.Update(accountEntity);
					}
				}
				else
				{
					throw new FailedException("ERROR_ACCOUNT_INFO", "获取预交金账户信息失败");
				}
			}
		}

		private void PaymentModelAccountReserveII(Infrastructure.EF.EFDbTransaction db, HospSettlementEntity jszbEntity
			, InpatientSettFeeRelatedDTO feeRelated)
		{
			HospSettlementPaymentModelEntity zfEntity1 = null;
			HospSettlementPaymentModelEntity zfEntity2 = null;

			if (feeRelated.yjjzfje.Value > 0)
			{
				zfEntity1 = new HospSettlementPaymentModelEntity();
				zfEntity1.zyjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jszffs");
				zfEntity1.OrganizeId = jszbEntity.OrganizeId;
				zfEntity1.jsnm = jszbEntity.jsnm;
				zfEntity1.xjzffs = xtzffs.ZYYJZHZF;
				zfEntity1.zfje = feeRelated.yjjzfje.Value;
				zfEntity1.zt = "1";
				zfEntity1.Create();
				db.Insert(zfEntity1);
				jszbEntity.xjzffs = xtzffs.ZYYJZHZF;
			}
			if (feeRelated.patZflist.Count() > 0)
			{
                foreach (var item in feeRelated.patZflist)
                {
                    zfEntity2 = new HospSettlementPaymentModelEntity();
                    zfEntity2.zyjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jszffs");
                    zfEntity2.OrganizeId = jszbEntity.OrganizeId;
                    zfEntity2.jsnm = jszbEntity.jsnm;
                    zfEntity2.xjzffs = item.zffsmc;
                    zfEntity2.zfje = (decimal)item.zfje;
                    zfEntity2.zt = "1";
                    zfEntity2.Create();
                    zfEntity2.CreateTime = DateTime.Now.AddSeconds(1);
                    db.Insert(zfEntity2);
                }
				jszbEntity.xjzffs = feeRelated.djjesszffs;
			}

			//预交金支付 构建账户收支  //预交金支付 一定作为第一支付方式
			if (zfEntity1 != null && zfEntity1.xjzffs == xtzffs.ZYYJZHZF)
			{
				var patid = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == jszbEntity.zyh && p.OrganizeId == jszbEntity.OrganizeId).Select(p => p.patid).FirstOrDefault();
				if (patid <= 0)
				{
					return;
				}
				decimal tye = 0;
				if (feeRelated.yjjtye.Value > 0)
				{
					tye = feeRelated.yjjtye.Value;
				}
				var accountEntity = db.IQueryable<InpatientAccountEntity>(p => p.patid == patid && p.zhxz == ((int)EnumXTZHXZ.ZYYJKZH) && p.zt == "1" && p.OrganizeId == jszbEntity.OrganizeId).FirstOrDefault();
				if (accountEntity != null)
				{
					zfEntity1.zh = accountEntity.zhCode;
					//if (!((zfEntity1.zfje + tye) <= accountEntity.zhye && zfEntity1.zfje <= feeRelated.xjzfys.Value))
					//{
					//	throw new FailedException("", "预交金账户信息有更新，请重新结算");
					//}
					var zhszEntity = new InpatientAccountRevenueAndExpenseEntity()
					{
                        OrganizeId = jszbEntity.OrganizeId,
                        zhCode = accountEntity.zhCode,
                        zyh = accountEntity.zyh,
                        szje = 0 - zfEntity1.zfje,
                        zhye = accountEntity.zhye - zfEntity1.zfje,
                        pzh = null,
                        szxz = (int)EnumSZXZ.zyjs,
                        xjzffs = Constants.xtzffs.ZYYJZHZF,
                        jsnm = jszbEntity.jsnm,
                        zt = "1",
					};
					zhszEntity.Create(true);
					db.Insert(zhszEntity);
					//
					accountEntity.zhye = zhszEntity.zhye;
					db.Update(accountEntity);
					//预交金支付 账户收支
					if (feeRelated.yjjtye.Value > 0 && accountEntity.zhye == feeRelated.yjjtye.Value)
					{
						//还有一条收支取款（应该是预交金余额全退）
						var zhszEntity2 = new InpatientAccountRevenueAndExpenseEntity()
						{
							OrganizeId = jszbEntity.OrganizeId,
							zhCode = accountEntity.zhCode,
                            zyh = accountEntity.zyh,
                            szje = 0 - feeRelated.yjjtye.Value,
							zhye = accountEntity.zhye - feeRelated.yjjtye.Value,
							pzh = null,
							szxz = (int)EnumSZXZ.zyjsth,
							//xjzffs = Constants.xtzffs.ZYYJZHZF,
							xjzffs = Constants.xtzffs.XJZF,   //现金
							zt = "1",
						};
						zhszEntity2.Create(true);
						zhszEntity2.CreateTime = DateTime.Now.AddSeconds(1);
						db.Insert(zhszEntity2);
						//
						accountEntity.zhye = zhszEntity2.zhye;
					}
					else if (feeRelated.yjjtye.Value > 0)
					{
						//throw new FailedException("ERROR_ACCOUNT_INFO", "预交账户结算成功，退余额失败");
					}

					db.Update(accountEntity);
				}
				else
				{
					throw new FailedException("ERROR_ACCOUNT_INFO", "获取预交金账户信息失败");
				}
			}
		}

		#endregion

		public InZyfymxxrDto GetUploadFeeDetails(Pagination pagination, string zyh, string orgId, string usrName, out decimal ybzje)
		{
			InZyfymxxrDto dtoRes = new InZyfymxxrDto();
			StringBuilder strsql = new StringBuilder();
			IList<SqlParameter> inSqlParameterList = null;
			//利用分页去查询，yka105字段排序，但此字段可能药品和项目会重复，所以添加前缀
			strsql.Append(@"SELECT  'YP' +CONVERT(VARCHAR(18),ypjfb.jfbbh) 'yka105' ,
                                    'YP' +ypjfb.yp 'ykd125' ,
                                    ISNULL(yp.ypmc,'') 'ykd126' ,
                                    ISNULL(yp.ybdm,'') 'yka002' ,
                                    ISNULL(yp.ypmc,'') 'yka003' ,
                                    ISNULL(ypjfb.sl,1) 'akc226' ,
                                    ypjfb.dj 'akc225' ,
                                    CONVERT(DECIMAL(14,4),ypjfb.sl * ypjfb.dj) 'yka315' ,
                                    ISNULL(ypjfb.ks,'') 'yka097',
                                    '' 'yka098',
                                    '' 'yka100' ,
                                    '' 'yka101' ,
                                    '' 'ykd106' ,
                                    '' 'yka102' ,
                                    '' 'ykd102' ,
                                    '' 'yka099' ,
                         CASE WHEN LEN(ISNULL(ypjfb.yzwym, '')) > 0
                         THEN ( CASE WHEN SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                                    0, 10) = SUBSTRING(CONVERT(VARCHAR(15), brxx.ryrq, 120),
                                                              0, 10)
                                     THEN SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                                    0, 11) + ' 23:59:59'
                                     ELSE SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                                    0, 11)
                                          + SUBSTRING(CONVERT(VARCHAR(30), ypjfb.CreateTime, 120),
                                                      11, 12)
                                END )
                         ELSE SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                        0, 11) + ' 23:59:59'
                    END 'yke123' ,
                                    '' 'ykc141' ,
                                    NULL 'aae036' ,
                                    '' 'aae013' ,
                                    '' 'yke201' ,
                                    ISNULL(ypjfb.jfdw,'') 'yka295' ,
                                    ISNULL(yp.ypgg,'') 'aka074' ,
                                    ISNULL(yp.jx,'') 'aka070' ,
                                    '' 'yae374' ,
                                    '' 'yke009' ,
                                    1 'yke186'
                            FROM    [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
                                    LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp
                                                                                AND yp.zt = '1'
                                                                                AND yp.OrganizeId = ypjfb.OrganizeId
LEFT JOIN NewtouchHIS_Sett..zy_brjbxx brxx ON brxx.OrganizeId = ypjfb.OrganizeId
                                                              AND brxx.zt = '1'
                                                              AND brxx.zyh = ypjfb.zyh
                            WHERE ypjfb.zyh=@zyh AND ypjfb.OrganizeId = @orgId
                            AND LEN(ISNULL(yp.ybdm,'') )>0 AND yp.ybbz=1 AND ypjfb.sl !=0
                            UNION ALL
                            SELECT  'XM' +CONVERT(VARCHAR(18),xmjfb.jfbbh) 'yka105' ,
                                    'XM' +ISNULL(xmjfb.sfxm,'') 'ykd125' ,
                                    ISNULL(xm.sfxmmc,'') 'ykd126' ,
                                    ISNULL(xm.ybdm,'') 'yka002' ,
                                    ISNULL(xm.sfxmmc,'') 'yka003' ,
                                    ISNULL(xmjfb.sl,1) 'akc226' ,
                                    xmjfb.dj 'akc225' ,
                                    CONVERT(DECIMAL(14,4),xmjfb.sl * xmjfb.dj) 'yka315' ,
                                    ISNULL(xmjfb.ks,'') 'yka097',
                                    '' 'yka098',
                                    '' 'yka100' ,
                                    ''  'yka101' ,
                                    '' 'ykd106' ,
                                    '' 'yka102' ,
                                    '' 'ykd102' ,
                                    '' 'yka099' ,
                       CASE WHEN LEN(ISNULL(xmjfb.yzwym, '')) > 0
                         THEN ( CASE WHEN SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                                    0, 11) = SUBSTRING(CONVERT(VARCHAR(15), brxx.ryrq, 120),
                                                              0, 11)
                                     THEN SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                                    0, 11) + ' 23:59:59'
                                     ELSE SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                                    0, 11)
                                          + SUBSTRING(CONVERT(VARCHAR(30), xmjfb.CreateTime, 120),
                                                      11, 12)
                                END )
                         ELSE SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                        0, 11) + ' 23:59:59'
                    END 'yke123' ,
                                    '' 'ykc141' ,
                                    NULL 'aae036' ,
                                    '' 'aae013' ,
                                    '' 'yke201' ,
                                    ISNULL(xmjfb.jfdw,'') 'yka295' ,
                                    '' 'aka074' ,
                                    '' 'aka070' ,
                                    '' 'yae374' ,
                                    '' 'yke009' ,
                                    1 'yke186'
                            FROM    [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb
                                    LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm
                                                                                  AND xm.zt = '1'
                                                                                  AND xm.OrganizeId = xmjfb.OrganizeId
 LEFT JOIN NewtouchHIS_Sett..zy_brjbxx brxx ON brxx.OrganizeId = xmjfb.OrganizeId
                                                              AND brxx.zt = '1'
                                                              AND brxx.zyh = xmjfb.zyh
                            WHERE xmjfb.zyh=@zyh  AND xmjfb.OrganizeId = @orgId
                            AND LEN(ISNULL(xm.ybdm,'') )>0 AND xm.ybbz=1  AND xmjfb.sl !=0
                    ");
			inSqlParameterList = new List<SqlParameter>();
			inSqlParameterList.Add(new SqlParameter("@zyh", zyh));
			inSqlParameterList.Add(new SqlParameter("@orgId", orgId));
			dtoRes.zyfymx = this.QueryWithPage<ZyfymxNodeDto>(strsql.ToString(), pagination, inSqlParameterList.ToArray()).ToList();
			ybzje = Convert.ToDecimal(0.000);
			foreach (var item in dtoRes.zyfymx)
			{
				ybzje += item.yka315;
				item.ykc141 = usrName;
				item.aae036 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			}
			InpatientSettYbPatInfoVO patinfo = GetInpatientSettYbPatInfo(zyh, orgId);
			if (patinfo != null)
			{
				dtoRes.akc190 = patinfo.jzbh;
				dtoRes.aka130 = patinfo.zhifuleibie;
				dtoRes.yab003 = patinfo.fzxbh;
				dtoRes.aac001 = patinfo.sbbh;
				dtoRes.ykb065 = patinfo.sbbf;
			}
			return dtoRes;
		}


		#region 贵安新农合



		/// <summary>
		/// 根据住院号获取病人 医保相关信息
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public InpatientSettXnhPatInfoVO GetInpatientSettXnhPatInfo(string zyh, string orgId)
		{
			var sql = @"  SELECT  a.zyh ,d.patid,a.zybz,
        b.Name doctor ,
        a.xm name ,
        c.inpId inpId ,
        d.xnhylzh bookNo ,
        '' familyNo ,
        d.xnhgrbm memberNo
FROM    [NewtouchHIS_Sett]..zy_brjbxx a
        LEFT JOIN [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b ON a.doctor = b.Account
                                                              AND b.zt = '1'
        LEFT JOIN [NewtouchHIS_Sett].[dbo].[Gaxnh_S04] c ON a.zyh = c.zyh
                                                            AND a.OrganizeId = c.OrganizeId
                                                            AND c.zt = '1'
		LEFT JOIN NewtouchHIS_Sett..xt_brjbxx d ON a.blh=d.blh AND a.OrganizeId=d.OrganizeId AND d.zt='1'
WHERE   a.zyh = @zyh
        AND a.OrganizeId = @orgId
        AND a.zt = '1'  ";
			var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@zyh", zyh));
			pars.Add(new SqlParameter("@orgId", orgId));
			return this.FirstOrDefault<InpatientSettXnhPatInfoVO>(sql, pars.ToArray());
		}

		/// <summary>
		/// 获取新农合医保结算上传明细
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <param name="usrName"></param>
		/// <param name="ybzje"></param>
		/// <returns></returns>
		public S10RequestDTO GetUploadXnhFeeDetails(string zyh, string orgId, out decimal nhfyzje)
		{
			S10RequestDTO dtoRes = new S10RequestDTO();
			StringBuilder strsql = new StringBuilder();
			IList<SqlParameter> inSqlParameterList = null;

			strsql.Append(@"SELECT  ISNULL(xm.sfxmmc, '') 'detailName' ,
        ISNULL(xm.xnhybdm, '') 'detailCode' ,
        'XM' + CONVERT(VARCHAR(18), xmjfb.jfbbh) 'hisDetailCode' ,
        'XM' + ISNULL(xmjfb.sfxm, '') 'detailHosCode' ,
        ttdetail.TTCode 'typeCode' ,
        ISNULL(xmjfb.sl, 1) 'num' ,
        xmjfb.dj 'price' ,
        CONVERT(DECIMAL(14, 4), xmjfb.sl * xmjfb.dj) 'totalCost' ,
        CONVERT(VARCHAR(18),CONVERT(DATE, xmjfb.tdrq, 120)) 'date' ,
        ISNULL(xmjfb.jfdw, '') 'unit' ,
        '' 'standard' ,
        '' 'formulations' ,
        '' 'recipeID'
FROM    [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb
        LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm
                                                      AND xm.zt = '1'
                                                      AND xm.OrganizeId = xmjfb.OrganizeId
        LEFT JOIN [NewtouchHIS_Sett]..TTCataloguesComparisonDetail ttdetail ON xm.sfdlCode = ttdetail.Code
WHERE   xmjfb.zyh = @zyh
        AND xmjfb.OrganizeId = @orgId
        AND LEN(ISNULL(xm.xnhybdm, '')) > 0
        AND xm.ybbz = 1
        AND xmjfb.sl != 0
        AND ttdetail.MainId IN (
        SELECT  Id
        FROM    [NewtouchHIS_Sett]..TTCataloguesComparisonMain
        WHERE   TTCode = 'gaxnh'
                AND Code = 'mllb'
                AND OrganizeId = @orgId )
UNION
SELECT  ISNULL(yp.ypmc, '') 'detailName' ,
        ISNULL(yp.xnhybdm, '') 'detailCode' ,
        'YP' + CONVERT(VARCHAR(18), ypjfb.jfbbh) 'hisDetailCode' ,
        'YP' + ISNULL(ypjfb.yp, '') 'detailHosCode' ,
        ttdetail.TTCode 'typeCode' ,
        ISNULL(ypjfb.sl, 1) 'num' ,
        ypjfb.dj 'price' ,
        CONVERT(DECIMAL(14, 4), ypjfb.sl * ypjfb.dj) 'totalCost' ,
        CONVERT(VARCHAR(18),CONVERT(DATE, ypjfb.tdrq, 120)) 'date' ,
        ISNULL(ypjfb.jfdw, '') 'unit' ,
        '' 'standard' ,
        '' 'formulations' ,
        '' 'recipeID'
FROM    [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
        LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp
                                                    AND yp.zt = '1'
                                                    AND yp.OrganizeId = ypjfb.OrganizeId
        LEFT JOIN [NewtouchHIS_Sett]..TTCataloguesComparisonDetail ttdetail ON yp.dlCode = ttdetail.Code
WHERE   ypjfb.zyh = @zyh
        AND ypjfb.OrganizeId = @orgId
        AND LEN(ISNULL(yp.xnhybdm, '')) > 0
        AND yp.ybbz = 1
        AND ypjfb.sl != 0
        AND ttdetail.MainId IN (
        SELECT  Id
        FROM    [NewtouchHIS_Sett]..TTCataloguesComparisonMain
        WHERE   TTCode = 'gaxnh'
                AND Code = 'mllb'
                AND OrganizeId = @orgId )
                    ");
			dtoRes.list = this.FindList<S10detail>(strsql.ToString(), new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@zyh", zyh) });
			nhfyzje = Convert.ToDecimal(0.000);
			if (dtoRes.list != null && dtoRes.list.Count > 0)
			{
				foreach (var item in dtoRes.list)
				{
					nhfyzje += item.totalCost;
				}
			}
			InpatientSettXnhPatInfoVO patinfo = GetInpatientSettXnhPatInfo(zyh, orgId);
			if (patinfo != null)
			{
				dtoRes.memberNo = patinfo.memberNo;
				dtoRes.isTransProvincial = "0";
				dtoRes.inpId = patinfo.inpId;
				dtoRes.familyNo = patinfo.familyNo;
				dtoRes.doctor = patinfo.doctor;
				dtoRes.bookNo = patinfo.bookNo;
				dtoRes.name = patinfo.name;
				dtoRes.rows = dtoRes.list.Count.ToString();
			}
			return dtoRes;
		}

		public S07RequestDTO GetXnhS07RequestDTO(string zyh, string orgId)
		{
			var sql = @" 
SELECT  e.inpId inpId ,
        '0' isTransProvincial,
        CONVERT(VARCHAR(18),CONVERT(DATE, b.cqrq, 120)) dischargeDate ,
        c.TTCode dischargeDepartments ,
        d.TTCode dischargeStatus
FROM    [NewtouchHIS_Sett].[dbo].[zy_brjbxx] a
        LEFT JOIN [Newtouch_CIS].[dbo].[zy_brxxk] b ON a.zyh = b.zyh
                                                       AND b.zt = '1'
                                                       AND b.zybz != '9'
                                                        AND b.OrganizeId = a.OrganizeId
        LEFT JOIN [NewtouchHIS_Sett].[dbo].[TTCataloguesComparisonDetail] c ON c.MainId IN (
                                                              SELECT
                                                              Id
                                                              FROM
                                                              [NewtouchHIS_Sett].[dbo].[TTCataloguesComparisonMain]
                                                              WHERE
                                                              TTCode = 'gaxnh'
                                                              AND Code = 'ks'
                                                              AND OrganizeId = @orgId )
                                                              AND b.DeptCode = c.Code
        LEFT JOIN [NewtouchHIS_Sett].[dbo].[TTCataloguesComparisonDetail] d ON d.MainId IN (
                                                              SELECT
                                                              Id
                                                              FROM
                                                              [NewtouchHIS_Sett].[dbo].[TTCataloguesComparisonMain]
                                                              WHERE
                                                              TTCode = 'gaxnh'
                                                              AND Code = 'cyzt'
                                                              AND OrganizeId = @orgId )
                                                              AND b.cyfs = d.Code
      LEFT JOIN [NewtouchHIS_Sett].[dbo].[Gaxnh_S04] e ON a.zyh=e.zyh and a.OrganizeId=e.OrganizeId and e.zt='1' 
WHERE   a.OrganizeId = @orgId
        AND a.zt = '1'
        AND a.zybz != '9'
        AND a.zyh=@zyh ";
			var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@zyh", zyh));
			pars.Add(new SqlParameter("@orgId", orgId));
			return this.FirstOrDefault<S07RequestDTO>(sql, pars.ToArray());
		}

		public string GetZyhByGrbm(string xnhgrbm, string sfjs, string orgId)
		{
			string sql = @" SELECT  zyh
FROM    [NewtouchHIS_Sett].[dbo].[zy_brjbxx] a
        LEFT JOIN NewtouchHIS_Sett..xt_brjbxx b ON a.patid = b.patid
                                                   AND a.OrganizeId = b.OrganizeId
                                                   AND b.zt = '1'
                                                   
";

			sql += " WHERE   b.xnhgrbm = @xnhgrbm AND a.OrganizeId = @OrganizeId AND a.zt = '1'";
			if (sfjs == "0")
			{
				sql += " AND a.zybz = '" + ((int)EnumZYBZ.Djz).ToString() + "' ";
			}

			if (sfjs == "1")
			{
				sql += " AND a.zybz = '" + ((int)EnumZYBZ.Ycy).ToString() + "' ";
			}
			SqlParameter[] para =
			{
				new SqlParameter("@xnhgrbm",xnhgrbm),
				new SqlParameter("@OrganizeId",orgId)
			};
			string zyh = this.FindList<string>(sql, para).FirstOrDefault();
			zyh = zyh == null ? "" : zyh;
			return zyh;
		}

		#endregion

		#region 重庆医保
		public UploadPrescriptionsInPut GetCQUploadFeeDetails(Pagination pagination, string zyh, string orgId, string usercode, out decimal ybzje, out decimal zfzje)
		{
			StringBuilder strsql = new StringBuilder();
			IList<SqlParameter> inSqlParameterList = null;
			//利用分页去查询，yka105字段排序，但此字段可能药品和项目会重复，所以添加前缀
			strsql.Append(@"SELECT  'YP' + CONVERT(VARCHAR(20), ypjfb.jfbbh) 'cfh' ,
        'YP' + ypjfb.yp 'yynm' ,
        ISNULL(yp.ypmc, '') 'xmmc' ,
        ISNULL(yp.ybdm, '') 'xmyblsh' ,
        ISNULL(ypjfb.sl, 1) 'sl' ,
        ypjfb.dj 'dj' ,
        NULL jzbz ,
        NULL jbr ,
        ypjfb.jfdw dw ,
        yp.ypgg gg ,
        yp.jx ,
        'YP' + CONVERT(VARCHAR(20), ypjfb.jfbbh) cxmxlsh ,
        staff.Name cfssmc ,
        CONVERT(DECIMAL(14, 4), ypjfb.sl * ypjfb.dj) 'je' ,
        ISNULL(ks.ybksbm, '') 'ksbm' ,--ISNULL(ypjfb.ks, '') 'ksbm' ,
        ks.Name 'ksmc' ,
        --ypjfb.ys ysbm ,
        isnull(staff.zjh,ypjfb.ys) ysbm,
        NULL mcyl ,
        NULL yfbz ,-----
        NULL zxzq ,
        '1' xzlb ,
        cast(ypjfb.zzfbz as varchar(100)) zzfbz,
        NULL dcyyjl ,
        NULL dcyyjldw ,
        NULL dcyl ,
        NULL zxjldw ,
        NULL qyzl ,
        NULL yytj ,-----
        CONVERT(VARCHAR(20),CASE WHEN LEN(ISNULL(ypjfb.yzwym, '')) > 0
             THEN ( CASE WHEN SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                        0, 10) = SUBSTRING(CONVERT(VARCHAR(15), brxx.ryrq, 120),
                                                           0, 10)
                         THEN SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                        0, 11) + ' 23:59:59'
                         ELSE SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120),
                                        0, 11)
                              + SUBSTRING(CONVERT(VARCHAR(30), ypjfb.CreateTime, 120),
                                          11, 12)
                    END )
             ELSE SUBSTRING(CONVERT(VARCHAR(15), ypjfb.tdrq, 120), 0, 11)
                  + ' 23:59:59'
        END,120)  'kfrq',
		yp.gjybdm gjmldm,staff.gjybdm gjysbm
FROM    [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
        LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp
                                                    AND yp.zt = '1'
                                                    AND yp.OrganizeId = ypjfb.OrganizeId
        LEFT JOIN NewtouchHIS_Sett..zy_brjbxx brxx ON brxx.OrganizeId = ypjfb.OrganizeId
                                                      AND brxx.zt = '1'
                                                      AND brxx.zyh = ypjfb.zyh
        LEFT JOIN NewtouchHIS_Base..Sys_Staff (NOLOCK) staff ON staff.OrganizeId = ypjfb.OrganizeId
                                                              AND staff.gh = ypjfb.ys
                                                              AND staff.zt = '1'
        LEFT JOIN NewtouchHIS_Base..Sys_Department (NOLOCK) ks ON ks.OrganizeId = ypjfb.OrganizeId
                                                              AND ks.Code = ypjfb.ks
                                                              AND ks.zt = '1'
WHERE   ypjfb.zyh = @zyh
        AND ypjfb.OrganizeId = @orgId
        AND LEN(ISNULL(yp.ybdm, '')) > 0
        --AND yp.ybbz = 1
        AND ypjfb.sl != 0
        AND ypjfb.jfbbh NOT IN (SELECT SUBSTRING(cfh,3,50) FROM NewtouchHIS_Sett..cqyb_OutPutInPar04 WHERE zt='1' AND zymzh=@zyh AND OrganizeId= @orgId AND jytype='2')
UNION ALL
SELECT  'XM' + CONVERT(VARCHAR(18), xmjfb.jfbbh) 'cfh' ,
        'XM' + xmjfb.sfxm 'yynm' ,
        ISNULL(xm.sfxmmc, '') 'xmmc' ,
        ISNULL(xm.ybdm, '') 'xmyblsh' ,
        ISNULL(xmjfb.sl, 1) 'sl' ,
        xmjfb.dj 'dj' ,
        NULL jzbz ,
        NULL jbr ,
        xmjfb.jfdw dw ,
        '' gg ,
        '' jx ,
        'YP' + CONVERT(VARCHAR(18), xmjfb.jfbbh) cxmxlsh ,
        staff.Name cfssmc ,
        CONVERT(DECIMAL(14, 4), xmjfb.sl * xmjfb.dj) 'je' ,
        ISNULL(ks.ybksbm, '') 'ksbm' ,--ISNULL(xmjfb.ks, '') 'ksbm' ,
        ks.Name 'ksmc' ,
        staff.zjh ysbm ,
        NULL mcyl ,
        NULL yfbz ,-----
        NULL zxzq ,
        '1' xzlb ,
        NULL zzfbz ,
        NULL dcyyjl ,
        NULL dcyyjldw ,
        NULL dcyl ,
        NULL zxjldw ,
        NULL qyzl ,
        NULL yytj ,-----
        CONVERT(VARCHAR(20),CASE WHEN LEN(ISNULL(xmjfb.yzwym, '')) > 0
             THEN ( CASE WHEN SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                        0, 10) = SUBSTRING(CONVERT(VARCHAR(15), brxx.ryrq, 120),
                                                           0, 10)
                         THEN SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                        0, 11) + ' 23:59:59'
                         ELSE SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120),
                                        0, 11)
                              + SUBSTRING(CONVERT(VARCHAR(30), xmjfb.CreateTime, 120),
                                          11, 12)
                    END )
             ELSE SUBSTRING(CONVERT(VARCHAR(15), xmjfb.tdrq, 120), 0, 11)
                  + ' 23:59:59'
        END,120) 'kfrq',
        xm.gjybdm gjmldm,staff.gjybdm gjysbm
FROM    [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb
        LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm
                                                      AND xm.zt = '1'
                                                      AND xm.OrganizeId = xmjfb.OrganizeId
        LEFT JOIN NewtouchHIS_Sett..zy_brjbxx brxx ON brxx.OrganizeId = xmjfb.OrganizeId
                                                      AND brxx.zt = '1'
                                                      AND brxx.zyh = xmjfb.zyh
		LEFT JOIN NewtouchHIS_Base..Sys_Staff (NOLOCK) staff ON staff.OrganizeId = xmjfb.OrganizeId
                                                              AND staff.gh = xmjfb.ys
                                                              AND staff.zt = '1'
        LEFT JOIN NewtouchHIS_Base..Sys_Department (NOLOCK) ks ON ks.OrganizeId = xmjfb.OrganizeId
                                                              AND ks.Code = xmjfb.ks
                                                              AND ks.zt = '1'
WHERE   xmjfb.zyh = @zyh
        AND xmjfb.OrganizeId = @orgId
        AND LEN(ISNULL(xm.ybdm, '')) > 0
        --AND xm.ybbz = 1
        AND xmjfb.sl != 0
        AND xmjfb.jfbbh NOT IN (SELECT SUBSTRING(cfh,3,50) FROM NewtouchHIS_Sett..cqyb_OutPutInPar04 WHERE zt='1' AND zymzh=@zyh AND OrganizeId= @orgId AND jytype='2')
");
			inSqlParameterList = new List<SqlParameter>();
			inSqlParameterList.Add(new SqlParameter("@zyh", zyh));
			inSqlParameterList.Add(new SqlParameter("@orgId", orgId));
			var ybdata = this.QueryWithPage<UploadPrescriptionsListInPut>(strsql.ToString(), pagination, inSqlParameterList.ToArray()).ToList();
			ybzje = Convert.ToDecimal(0.0000);
			zfzje = Convert.ToDecimal(0.0000);
			foreach (var item in ybdata)
			{
				item.cxmxlsh = "";//该字段可能恒为null，为了
				item.jbr = usercode;
				if (!string.IsNullOrEmpty(item.sl.ToString()) && !string.IsNullOrEmpty(item.je.ToString()) && Convert.ToDecimal(item.sl) > 0 && Convert.ToDecimal(item.je) > 0)
				{
					ybzje += Convert.ToDecimal(item.je);
				}
			}
			UploadPrescriptionsInPut ChongQingMainAllOfMzjsModel = new UploadPrescriptionsInPut()
			{
				zymzh = zyh,
				cflist = ybdata
			};

			return ChongQingMainAllOfMzjsModel;
		}

		public string Getgjybdm(string mzzyh, string cfnms, string cflx, string orgId)
		{
			string sql = "";
			var pars = new List<SqlParameter>();
			if (cflx == "1")
			{
				sql = @"select  xmmc from(
	SELECT '药品:'+ypmc xmmc
	FROM mz_cf(NOLOCK) cf 
	INNER JOIN mz_cfmx(NOLOCK) mx ON cf.cfnm = mx.cfnm AND mx.OrganizeId=cf.OrganizeId AND mx.zt = '1'
	INNER JOIN dbo.mz_gh(NOLOCK) gh ON gh.ghnm=cf.ghnm AND gh.zt='1' AND gh.OrganizeId=cf.OrganizeId
	INNER JOIN NewtouchHIS_Base.dbo.V_C_xt_yp yp ON yp.ypCode=mx.yp AND yp.OrganizeId=cf.OrganizeId AND yp.zt='1' AND isnull(yp.zfxz,'1')!='1'
	WHERE cf.OrganizeId =@orgId	
	AND cf.zt= '1' AND cf.cfzt = '0' --处方有效且未收费
	AND gh.mzh=@mzzyh
    AND cf.cfnm IN (SELECT * FROM dbo.f_split(@cfnms, ',')) --选择的处方内码
	AND yp.gjybdm IS  NULL
	UNION ALL
	SELECT  '项目:'+sfxm.sfxmmc xmmc 
	FROM dbo.mz_xm(NOLOCK) xm
	INNER JOIN dbo.mz_gh(NOLOCK) gh ON gh.ghnm=xm.ghnm and gh.OrganizeId=xm.OrganizeId AND gh.zt='1'
	INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfnm = xm.cfnm AND cf.OrganizeId = xm.OrganizeId
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_sfxm sfxm ON sfxm.sfxmCode = xm.sfxm AND sfxm.OrganizeId = xm.OrganizeId AND isnull(sfxm.zfxz,'1')!='1' 
	WHERE xm.OrganizeId =@orgId	
	AND xm.zt = '1' and xm.xmzt = '0' --有效且未收费	
	and (cf.zt is null or (cf.zt= '1' and cf.cfzt = '0')) --未关联处方 或 处方有效且未收费
	AND gh.mzh=@mzzyh
    AND cf.cfnm IN  (SELECT * FROM dbo.f_split(@cfnms, ',')) --选择的处方内码
	AND sfxm.gjybdm IS  NULL
	) as t";
				pars.Add(new SqlParameter("@cfnms", cfnms));
			}
			else if (cflx == "2")
			{
				sql = @" select  * from( select '药品:'+ypmc  xmmc from [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp AND yp.zt = '1'  AND yp.OrganizeId = ypjfb.OrganizeId  
WHERE   ypjfb.zyh =  @mzzyh    
        AND ypjfb.OrganizeId =@orgId   
        AND yp.zfxz!=1    --1:自费 4:甲 5:已 6:丙  
        AND ypjfb.sl != 0 and yp.gjybdm is null
union all 
select '项目:'+xm.sfxmmc xmmc   from   [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb  
  LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm  
                                                      AND xm.zt = '1'  
                                                      AND xm.OrganizeId = xmjfb.OrganizeId  
WHERE   xmjfb.zyh =  @mzzyh   
        AND xmjfb.OrganizeId =@orgId    
         AND xm.zfxz!=1      --1:自费 4:甲 5:已 6:丙
        AND xmjfb.sl != 0  and xm.gjybdm is null ) as t";
			}

			pars.Add(new SqlParameter("@mzzyh", mzzyh));
			pars.Add(new SqlParameter("@orgId", orgId));

			var xmmc = this.FirstOrDefault<string>(sql, pars.ToArray());
			return xmmc;
		}

		public decimal GetCQAlreadyUploadFeeDetails(string zyh, string orgId)
		{
			var sql = @" select CONVERT(NUMERIC(10,2),ISNULL(SUM(je),0.00)) je from(
select CONVERT(DECIMAL(14, 2), CONVERT(DECIMAL(14, 4),(yp.bzs*ypjfb.dj)) *  CONVERT(DECIMAL(14, 4), ypjfb.sl/yp.bzs)) je  from [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp AND yp.zt = '1'  AND yp.OrganizeId = ypjfb.OrganizeId  
WHERE   ypjfb.zyh = @zyh  
        AND ypjfb.OrganizeId =@orgId  
        AND isnull(yp.zfxz,'1')!='1'  
        AND ypjfb.sl != 0 
union all 
select CONVERT(NUMERIC(10,2),ISNULL(xmjfb.dj*xmjfb.sl,0.00)) je  from   [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb  
  LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm  
                                                      AND xm.zt = '1'  
                                                      AND xm.OrganizeId = xmjfb.OrganizeId  
WHERE   xmjfb.zyh = @zyh  
        AND xmjfb.OrganizeId = @orgId  
        AND isnull(xm.zfxz,'1')!='1'
        AND xmjfb.sl != 0  
) as t
";
			var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@zyh", zyh));
			pars.Add(new SqlParameter("@orgId", orgId));
			var ybzje = this.FirstOrDefault<decimal>(sql, pars.ToArray());
			return ybzje;
		}

		public HosCqybJsPatInfoVO GetCQInpatientSettYbPatInfo(string zyh, string orgId)
		{
			var sql = @"SELECT zybrxx.ryrq,zybrxx.cyrq ,
         dept.ybksbm cyks,--ciszyxx.DeptCode cyks ,
        isnull(staff.zjh,ciszyxx.ysgh) cyys,staff.gjybdm cyysgjbm
FROM    zy_brjbxx zybrxx
        INNER JOIN Newtouch_CIS..zy_brxxk ciszyxx ON ciszyxx.OrganizeId = zybrxx.OrganizeId
                                                     AND ciszyxx.zyh = zybrxx.zyh
                                                     AND ciszyxx.zt = '1'
        LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_Sys_Staff] staff ON staff.OrganizeId = ciszyxx.OrganizeId
                                                              AND staff.gh = ciszyxx.ysgh
                                                              AND staff.zt = '1'
        left join  NewtouchHIS_Base.dbo.V_S_Sys_Department dept on dept.code=ciszyxx.DeptCode and dept.OrganizeId=ciszyxx.OrganizeId and dept.zt='1'
WHERE   zybrxx.zt = '1'
        AND zybrxx.zyh = @zyh
        AND zybrxx.OrganizeId = @orgId ";
			var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@zyh", zyh));
			pars.Add(new SqlParameter("@orgId", orgId));
			var vo = this.FirstOrDefault<HosCqybJsPatInfoVO_cr>(sql, pars.ToArray());
			var cyrq = vo.cyrq;
			if (!vo.cyrq.HasValue)
			{
				cyrq = DateTime.Now;
			}
			return new HosCqybJsPatInfoVO()
			{
				zycr = DateTimeHelper.GetInHospDays(vo.ryrq, Convert.ToDateTime(cyrq)).ToString(),
				cyks = vo.cyks,
				cyys = vo.cyys,
				cyysgjbm = vo.cyysgjbm,
				cyrq = cyrq.Value.ToString("yyyy-MM-dd")
			};

		}

		#region  重庆住院中途结算
		/// <summary>
		/// 中途结算
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <param name="startTime">调用之前请明确开始日期</param>
		/// <param name="endTime"></param>
		/// <param name="expectedCount"></param>
		public void commitPartialSettle(string zyh, string orgId, DateTime startTime, DateTime endTime, InpatientSettFeeRelatedDTO feeRelated, CQZyjs05Dto ybfeeRelated, string jslx, InpatientSettleItemBO xmypjfbList, out int jsnm, int? expectedCount = null)
		{
			jsnm = 0;
			using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
			{
				var zybrInfo = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();

				//计费项目
				//var xmjfEntityList = _hospItemBillingRepo.GetItemFeeEntityListByTime(zyh, orgId, startTime, endTime, db.IQueryable<HospItemBillingEntity>());

				if (expectedCount.HasValue && expectedCount.Value != xmypjfbList.TreatmentItemList.Count)
				{
					throw new FailedException("过程中计费项目发生变更");
				}
				//if (xmjfEntityList.Count == 0&&ypjfbList.Count==0)
				//{
				//    throw new FailedException("无未结费用，操作失败");
				//}

				var newjsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_js");
				jsnm = newjsnm;
				var newJsmxList = new List<HospSettlementDetailEntity>();
				foreach (var item in xmypjfbList.TreatmentItemList)
				{
					var jsmxItem = new HospSettlementDetailEntity()
					{
						jsmxbh = newjsnm,
						OrganizeId = orgId,
						jsnm = newjsnm,
						xmjfbbh = item.jfbbh,
						yzlx = "2",
						jyje = Math.Round(item.dj * item.sl, 2, MidpointRounding.AwayFromZero)
					};
					if (feeRelated.ver == "2")
					{
						jsmxItem.jyje = Math.Round(item.je, 2, MidpointRounding.AwayFromZero);
					}
					jsmxItem.Create();
					db.Insert(jsmxItem);
					newJsmxList.Add(jsmxItem);
				}

				if (xmypjfbList.DrugList != null && xmypjfbList.DrugList.Count > 0)
				{
					foreach (var item in xmypjfbList.DrugList)
					{
						var jsmxItem = new HospSettlementDetailEntity()
						{
							jsmxbh = newjsnm,
							OrganizeId = orgId,
							jsnm = newjsnm,
							ypjfbbh = item.jfbbh,
							yzlx = "1",
							jyje = Math.Round(item.dj * item.sl, 2, MidpointRounding.AwayFromZero),
						};
						if (feeRelated.ver == "2")
						{
							jsmxItem.jyje = Math.Round(item.je, 2, MidpointRounding.AwayFromZero);
						}
						jsmxItem.Create();
						db.Insert(jsmxItem);
						newJsmxList.Add(jsmxItem);
					}
				}


				var jsItem = new HospSettlementEntity()
				{
					jsnm = newjsnm,
					OrganizeId = orgId,
					zyh = zyh,
					brxz = zybrInfo.brxz,
					zyts = DateTimeHelper.GetInHospDays(zybrInfo.ryrq, endTime),
					zje = newJsmxList.Select(p => p.jyje.Value).Sum(),
					jszt = "1", //已结
					jsxz = "2", //中途结算
					jsksrq = startTime,
					jsjsrq = endTime,
				};
				jsItem.zlfy = jsItem.zje;
				jsItem.xjzf = feeRelated.xjzfys.Value;
				jsItem.ysk = feeRelated.ssk;
				jsItem.zl = feeRelated.zhaoling;
                jsItem.zkbl = feeRelated.zkbl;
                //jsItem.xjzffs = "0";
                jsItem.Create();
				db.Insert(jsItem);

				var medicalInsurance = _sysConfigRepo.GetValueByCode("Inpatient_MedicalInsurance", orgId);

				if (!(feeRelated.orglxjzfys.HasValue && feeRelated.xjzfys.HasValue))
				{
					//???xjzf有必要赋值么
					//要减去记账部分 等金额
					jsItem.xjzf = jsItem.zje - 0;
				}


				if (feeRelated.ssk.HasValue && feeRelated.zhaoling.HasValue)
				{
					//现金误差 收到的-应收的
					jsItem.xjwc = jsItem.xjzf - (feeRelated.ssk.Value - feeRelated.zhaoling.Value);
					if (Math.Abs(jsItem.xjwc) >= (decimal)0.1)
					{
						throw new FailedException("ERROR_SSK_ZHAOLING", "实收找零金额异常");
					}
				}

				if (jslx == "1" && ybfeeRelated != null)
				{
					if (medicalInsurance == "chongqing")
					{
						if (!string.IsNullOrEmpty(ybfeeRelated.jylsh))
						{
							//重庆医保
							//保存医保结算相关费用
							var ybFeeEntity = new CqybSett05Entity();
							ybfeeRelated.MapperTo(ybFeeEntity);
							ybFeeEntity.jslb = "3";
							ybFeeEntity.OrganizeId = orgId;
							ybFeeEntity.jsnm = jsItem.jsnm;
							ybFeeEntity.Create();
							ybFeeEntity.zt = "1";
							db.Insert(ybFeeEntity);
						}
					}
				}

				PaymentModelAccountReserveII(db, jsItem, feeRelated);

				db.Commit();
			}
		}

		/// <summary>
		/// 转出院结算更新病人基本信息
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		public void UpdatePatientInfo(string zyh, string orgId)
		{
			bool updateZybrjbxx = false;
			//根据zyh获取病人信息
			HospPatientBasicInfoEntity brjbxx = _hospPatientBasicInfoRepo.GetInpatientInfoByZyh(zyh, orgId);
			//保存变更日志老记录
			HospPatientBasicInfoEntity oldbrjbxx = null;
			if (brjbxx == null)
			{
				throw new FailedCodeException("OUTPAT_NO_PATIENT_INFORMATION");
			}
			oldbrjbxx = brjbxx.Clone();
			brjbxx.zybz = ((int)EnumZYBZ.Ycy).ToString();
			brjbxx.cyrq = DateTime.Now;
			brjbxx.cyjdrq = DateTime.Now;   //应该是当前
			brjbxx.cyjdry = OperatorProvider.GetCurrent().UserCode;

			updateZybrjbxx = true;

			using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
			{
				//变更住院病人信息
				if (updateZybrjbxx)
				{
					brjbxx.Modify();
					db.Update(brjbxx);
				}

				db.Commit();
			}
		}
		#endregion


		#region 重庆取消结算

		/// <summary>
		/// 最后一次交易流水号
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public int GetCQLastJsnm(string zyh, string orgId)
		{
			using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
			{
				//1.先查出所有撤销的记录
				var cxztjsList = db.IQueryable<HospSettlementEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.jszt == "2").Select(p => p.cxjsnm);
				//已产生的 结算列表 jszt 1已结 且 为被撤销
				//已排序 最新的有效结算 排在最前
				var ztjsList = db.IQueryable<HospSettlementEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.jszt == "1" && !cxztjsList.Contains(p.jsnm)).OrderByDescending(p => p.CreateTime).ToList();

				if (ztjsList.Count > 0 && ztjsList[0].jsxz == "1")
				{
					throw new FailedException("病人已出院，操作失败");
				}
				else if (ztjsList.Count == 0)
				{
					throw new FailedException("暂无历史结算，操作失败");
				}
				return ztjsList[0].jsnm;
			}
		}
		#endregion

		/// <summary>
		/// 最后一次交易流水号
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public string GetCQLastLsh(int jsnm, string orgId)
		{
			CqybSett05Entity entity =
		   _cqybSett05Repo.FindEntity(p => p.jsnm == jsnm && p.zt == "1" && p.OrganizeId == orgId);
			return entity.jylsh;
		}

		public CancelSettInfo GetCancelSettInfo(int jsnm, string orgId)
		{
			StringBuilder strSql = new StringBuilder();
			strSql.Append(@"
SELECT  b.jylsh mdtrt_id,js.zyh hisId,js.ybjslsh setl_id,
        kh.cbdbm insuplc_admdvs,kh.grbh psn_no,xzlx insutype,
        '' operatorId,'' operatorName
FROM    zy_js js 
		left join [dbo].[cqyb_OutPut05] jsld on js.jsnm=jsld.jsnm and js.OrganizeId=jsld.OrganizeId and jsld.zt='1'
		left join NewtouchHIS_Sett..zy_brjbxx a on js.zyh=a.zyh and js.OrganizeId=a.OrganizeId and a.zt='1'
        left join [cqyb_OutPut02] b on a.zyh=b.zymzh and a.OrganizeId=b.OrganizeId and b.zt='1'
        LEFT JOIN [NewtouchHIS_Sett].[dbo].[xt_brjbxx] e ON e.blh = a.blh
                                                            AND e.OrganizeId = a.OrganizeId
                                                            AND e.zt = '1'
        left join NewtouchHIS_Sett..xt_card kh on kh.cardno=a.kh and kh.CardType=a.cardtype
       
WHERE   js.jsnm = @jsnm
        AND js.OrganizeId = @OrganizeId
        AND js.zt = '1' ");
			SqlParameter[] par =
			{
				new SqlParameter("@jsnm", jsnm),
				new SqlParameter("@OrganizeId", orgId)
			};
			return this.FirstOrDefault<CancelSettInfo>(strSql.ToString(), par);
		}

		#region 医保未审批项目
		/// <summary>
		/// 获取未审批明细
		/// </summary>
		/// <param name="cfmxlshstr">交易流水号字符串</param>
		/// <param name="orgId"></param>
		/// <param name="FinalType">审批类型</param>
		/// <param name="highPrices">高收费价格</param>
		/// <returns></returns>
		public IList<RequestApprovalVO> sweep_expired_approvals(string cfmxlshstr, string orgId, string zyh)
		{
			if (string.IsNullOrWhiteSpace(zyh))
			{
				throw new FailedException("缺少住院号");
			}

			var sql = "";
			var pars = new List<SqlParameter>();

			sql = @"SELECT cqyb04.cfh,cqyb04.jylsh,dl.dlmc + '未审批' AS fyzt, xx.patid, jfb.*
FROM (
	SELECT zyh, ypjfb.jfbbh, ypjfb.dl, yp.ypmc AS sfxmmc, yp.ypgg
		, ypjfb.createtime, ypjfb.dj, sl
		, CONVERT(DECIMAL(14, 4), sl * ypjfb.dj) AS zje
	FROM V_C_Sys_WsfZyYpjfb ypjfb
		LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp
		ON yp.ypCode = ypjfb.yp
			AND yp.zt = '1'
			AND yp.OrganizeId = ypjfb.OrganizeId
	WHERE ypjfb.OrganizeId = @orgId and ypjfb.zt='1'
	UNION ALL
	SELECT xmjfb.zyh, xmjfb.jfbbh, xmjfb.dl, sfxm.sfxmmc, '' AS ypgg
		, xmjfb.createtime, xmjfb.dj, sl
		, CONVERT(DECIMAL(14, 4), sl * xmjfb.dj) AS zje
	FROM V_C_Sys_WsfZyXmjfb xmjfb
		LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm
		ON sfxm.sfxmCode = xmjfb.sfxm
			AND sfxm.zt = '1'
			AND sfxm.OrganizeId = xmjfb.OrganizeId
	WHERE xmjfb.OrganizeId = @orgId and xmjfb.zt='1'
) jfb
	LEFT JOIN NewtouchHIS_Base..V_S_xt_sfdl dl
	ON dl.dlCode = jfb.dl
		AND dl.OrganizeId = @orgId
	INNER JOIN zy_brjbxx xx
	ON jfb.zyh = xx.zyh
		AND xx.OrganizeId = @orgId
	  inner join cqyb_OutPut04 cqyb04 on 
  cqyb04.Id=(select top 1 Id from cqyb_OutPut04 where cfh = 'YP' + CONVERT(VARCHAR(18), jfb.jfbbh)
		OR cfh = 'XM' + CONVERT(VARCHAR(18), jfb.jfbbh)  order by cqyb_OutPut04.CreateTime desc) 
  and cqyb04.OrganizeId=@orgId
WHERE cqyb04.jylsh IN (
		SELECT *
		FROM dbo.f_split(@cfmxlshstr, ',')
	)
	AND jfb.zyh = @zyh";

			pars.Add(new SqlParameter("@cfmxlshstr", cfmxlshstr));
			pars.Add(new SqlParameter("@orgId", orgId));
			pars.Add(new SqlParameter("@zyh", zyh));
			//pars.Add(new SqlParameter("@cfmxlshstr", "040000005902250"));//cfmxlshstr
			//pars.Add(new SqlParameter("@orgId", orgId));
			//pars.Add(new SqlParameter("@zyh", "00196"));//zyh
			return this.FindList<RequestApprovalVO>(sql, pars.ToArray());
		}


		public void updatespbz(string cfhstr, string orgId, string usercode)
		{
			var sql = "";
			var pars = new List<SqlParameter>();
			if (string.IsNullOrWhiteSpace(cfhstr))
			{
				throw new FailedException("处方号为空");
			}

			sql = @"update cqyb_OutPutInPar04 set spbz='1',LastModifyTime=getdate(),LastModifierCode=@usercode where cfh in (select * from dbo.f_split(@cfhstr,',')) and OrganizeId=@orgId and jytype='2'";
			pars.Add(new SqlParameter("@cfhstr", cfhstr));
			pars.Add(new SqlParameter("@orgId", orgId));
			pars.Add(new SqlParameter("@usercode", usercode));
			ExecuteSqlCommand(sql, pars.ToArray());
		}
		#endregion
		#endregion


		#region 重庆医保数据传输
		public InpatIentFeeObj GetHospPreInfo(string zyh, string orgId, DateTime jzsj)
		{
			string sql = @"select  ISNULL(SUM(je),0.00) zje,ISNULL(SUM(ybje),0.00) ybzje,(select isnull(cyrq,GETDATE()) from zy_brjbxx where zyh= @zyh    and zt='1' 
and OrganizeId = @orgId  ) cyrq from(
select CONVERT(NUMERIC(10,2), isnull(je, 0)) je ,
case yp.zfxz when '1' then 0.00 else CONVERT(DECIMAL(14, 2), ISNULL(je,0.00)) end ybje  from [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp AND yp.zt = '1'  AND yp.OrganizeId = ypjfb.OrganizeId  
WHERE   ypjfb.zyh = @zyh  and  tdrq<@jzsj
        AND ypjfb.OrganizeId =@orgId 
       -- and yp.zfxz!='1' --1:自费 4:甲 5:已 6:丙 
        --AND ypjfb.sl != 0 
union all 
select CONVERT(NUMERIC(10,2), isnull(xmjfb.je, 0)) je,
case xm.zfxz when '1' then 0.00 else CONVERT(NUMERIC(10,2),ISNULL(xmjfb.je,0.00)) end ybje  from   [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb  
  LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm  
                                                      AND xm.zt = '1'  
                                                      AND xm.OrganizeId = xmjfb.OrganizeId  
WHERE   xmjfb.zyh = @zyh and  xmjfb.tdrq<@jzsj
        AND xmjfb.OrganizeId = @orgId  
      --  and xm.zfxz!='1' --1:自费 4:甲 5:已 6:丙
        --AND xmjfb.sl != 0  
) as t";

			SqlParameter[] par =
			{
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@jzsj", jzsj)
			};
			return this.FirstOrDefault<InpatIentFeeObj>(sql.ToString(), par);
		}
		/// <summary>
		/// 是否存在未上传的明细
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public string ValialUploadData(string zyh, string orgId, DateTime jssj)
		{
			string sql = @"select '计费编号:'+cast(jfbbh as varchar) +'名称:'+yp.ypmc xmmc  from zy_ypjfb (NOLOCK) ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp (NOLOCK) yp ON yp.ypCode = ypjfb.yp    
										AND yp.zt = '1'    
										AND yp.OrganizeId = ypjfb.OrganizeId
left join Drjk_zyfymxsc_input (NOLOCK) fymx on SUBSTRING(fymx.feedetl_sn,3,50)=ypjfb.jfbbh  and fymx.zt='1'  
 where ypjfb.zyh =@zyh and ypjfb.OrganizeId =@orgId  and ypjfb.zt= '1' AND ypjfb.sl != 0   and tdrq<@jssj
 and yp.zfxz!='1' --1:自费 4:甲 5:已 6:丙
 and ISNULL(fymx.feedetl_sn,'0')='0' --未上传
  and NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.ypjfbbh,a.OrganizeId,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm AND jsmx.OrganizeId = a.OrganizeId
                                WHERE  a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1' ) m WHERE (ypjfb.jfbbh=m.ypjfbbh or ypjfb.cxzyjfbbh=m.ypjfbbh) AND ypjfb.OrganizeId=m.OrganizeId AND ypjfb.zyh=m.zyh

										) 
union all
select  '计费编号:'+cast(jfbbh as varchar)+'名称:'+sfxm.sfxmmc xmmc 
 from zy_xmjfb (NOLOCK) xmjfb
 LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm (NOLOCK) sfxm ON sfxm.sfxmCode = xmjfb.sfxm    
                                                      AND sfxm.zt = '1'    
                                                      AND sfxm.OrganizeId = xmjfb.OrganizeId
left join Drjk_zyfymxsc_input (NOLOCK) fymx on SUBSTRING(fymx.feedetl_sn,3,50)=xmjfb.jfbbh  and fymx.zt='1'            
 where xmjfb.zyh =@zyh and xmjfb.OrganizeId =@orgId and xmjfb.zt= '1' AND xmjfb.sl != 0 and xmjfb.tdrq<@jssj
and ISNULL(fymx.feedetl_sn,'0')='0'
 and sfxm.zfxz!=1   --1:自费 4:甲 5:已 6:丙
  and  NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.xmjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm
                                WHERE  a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1')m WHERE (xmjfb.jfbbh=m.xmjfbbh or xmjfb.cxzyjfbbh=m.xmjfbbh) AND xmjfb.OrganizeId=m.OrganizeId AND xmjfb.zyh=m.zyh)

";
			SqlParameter[] par =
			{
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@jssj", jssj)
			};
			return this.FirstOrDefault<string>(sql.ToString(), par);
		}
        /// <summary>
		/// 是否存在未上传的明细
		/// </summary>
		/// <param name="zyh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public string ValialUploadDataShyb(string zyh, string orgId, DateTime jssj)
        {
            string sql = @"select '计费编号:'+cast(jfbbh as varchar) +'名称:'+yp.ypmc xmmc  from zy_ypjfb (NOLOCK) ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp (NOLOCK) yp ON yp.ypCode = ypjfb.yp    
										AND yp.zt = '1'    
										AND yp.OrganizeId = ypjfb.OrganizeId
left join Ybjk_SN01_Mxxzy_Input (NOLOCK) fymx on xh=ypjfb.jfbbh  and fymx.zt='1'  
 where ypjfb.zyh =@zyh and ypjfb.OrganizeId =@orgId  and ypjfb.zt= '1' AND ypjfb.sl != 0   and tdrq<@jssj
 and yp.zfxz!='1' --1:自费 4:甲 5:已 6:丙
 and ISNULL(fymx.xh,'0')='0' --未上传
  and NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.ypjfbbh,a.OrganizeId,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm AND jsmx.OrganizeId = a.OrganizeId
                                WHERE  a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1' ) m WHERE (ypjfb.jfbbh=m.ypjfbbh or ypjfb.cxzyjfbbh=m.ypjfbbh) AND ypjfb.OrganizeId=m.OrganizeId AND ypjfb.zyh=m.zyh

										) 
union all
select  '计费编号:'+cast(jfbbh as varchar)+'名称:'+sfxm.sfxmmc xmmc 
 from zy_xmjfb (NOLOCK) xmjfb
 LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm (NOLOCK) sfxm ON sfxm.sfxmCode = xmjfb.sfxm    
                                                      AND sfxm.zt = '1'    
                                                      AND sfxm.OrganizeId = xmjfb.OrganizeId
left join Ybjk_SN01_Mxxzy_Input (NOLOCK) fymx on fymx.xh=xmjfb.jfbbh  and fymx.zt='1'            
 where xmjfb.zyh =@zyh and xmjfb.OrganizeId =@orgId and xmjfb.zt= '1' AND xmjfb.sl != 0 and xmjfb.tdrq<@jssj
and ISNULL(fymx.xh,'0')='0'
 and sfxm.zfxz!=1   --1:自费 4:甲 5:已 6:丙
  and  NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.xmjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm
                                WHERE  a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1')m WHERE (xmjfb.jfbbh=m.xmjfbbh or xmjfb.cxzyjfbbh=m.xmjfbbh) AND xmjfb.OrganizeId=m.OrganizeId AND xmjfb.zyh=m.zyh)
";
            SqlParameter[] par =
            {
                new SqlParameter("@zyh", zyh),
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@jssj", jssj)
            };
            return this.FirstOrDefault<string>(sql.ToString(), par);
        }
        public string ValialPartialUploadData(string zyh, string orgId, DateTime jssj)
		{
			var sql = @"select feedetl_sn from drjk_zyfymxsc_input (nolock) where zyh=@zyh and fee_ocur_time>@jssj ";

			SqlParameter[] par =
			{
				new SqlParameter("@zyh", zyh),
				new SqlParameter("@jssj", jssj)
			};
			return this.FirstOrDefault<string>(sql.ToString(), par);
		}

		public InpatIentFeeInfo GetCQAlreadyUploadFeeDetailsV2(string zyh, string orgId, DateTime jssj)
		{
			var sql = @" select CONVERT(NUMERIC(10,2),ISNULL(SUM(je),0.00)) zje,(select isnull(cyrq,GETDATE()) from zy_brjbxx where zyh=@zyh and zt='1' 
and OrganizeId = @orgId ) cyrq from(
select CONVERT(DECIMAL(14, 2),isnull(je,0.00)) je  from [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyYpjfb] ypjfb
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp ON yp.ypCode = ypjfb.yp AND yp.zt = '1'  AND yp.OrganizeId = ypjfb.OrganizeId  
WHERE   ypjfb.zyh = @zyh  and tdrq<@jssj
        AND ypjfb.OrganizeId =@orgId  
        and yp.zfxz!='1' --1:自费 4:甲 5:已 6:丙 
        AND ypjfb.sl != 0 
union all 
select CONVERT(NUMERIC(10,2),ISNULL(xmjfb.je,0.00)) je  from   [NewtouchHIS_Sett].[dbo].[V_C_Sys_WsfZyXmjfb] xmjfb  
  LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm xm ON xm.sfxmCode = xmjfb.sfxm  
                                                      AND xm.zt = '1'  
                                                      AND xm.OrganizeId = xmjfb.OrganizeId  
WHERE   xmjfb.zyh = @zyh  and xmjfb.tdrq<@jssj
        AND xmjfb.OrganizeId = @orgId  
        and xm.zfxz!='1' --1:自费 4:甲 5:已 6:丙
        AND xmjfb.sl != 0  
) as t
";
			var pars = new List<SqlParameter>();
			pars.Add(new SqlParameter("@zyh", zyh));
			pars.Add(new SqlParameter("@orgId", orgId));
			pars.Add(new SqlParameter("@jssj", jssj));
			var ybzje = this.FirstOrDefault<InpatIentFeeInfo>(sql, pars.ToArray());
			return ybzje;
		}

		/// <summary>
		/// 保存医保预结算结果
		/// </summary>
		/// <param name="YuJieSuan"></param>
		public void SaveSett2303(CqybSett2303Entity YuJieSuan)
		{
			using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
			{
				//CqybSett2303Entity cqybSett2303Entity = new CqybSett2303Entity();
				//YuJieSuan.MapperTo(cqybSett2303Entity);
				db.Insert(YuJieSuan);
				db.Commit();
			}
		}
        #endregion

        /// <summary>
        /// 根据住院号获取LIS/PACS报告未完成数量
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public int CountLisIncompletezy(string orgId, string zyh)
        {
            string sql = @" 
select count(*) from [Newtouch_CIS].[dbo].[zy_lsyz] where zyh=@zyh and (syncStatus is null or syncStatus<>2) and yzlx in('6','7') and organizeId=@orgId and zt='1'";
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@zyh", zyh));
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            return this.FirstOrDefault<int>(sql, sqlpar.ToArray());
        }
    }

}
