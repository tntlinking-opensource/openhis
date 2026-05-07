using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.HIS.Domain.DTO.InputDto.OutpatientAccounting;
using Newtouch.HIS.Domain.DTO.OutputDto;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.HospitalizationManage;
using Newtouch.HIS.Domain.ValueObjects.PatientManage;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Newtouch.HIS.DomainServices
{
    public class BookkeepInHosDmnService : DmnServiceBase, IBookkeepInHosDmnService
    {
        private readonly IHospPatientBasicInfoRepo _hospPatientBasicInfoRepo;
        private readonly IOutpatientAccountDetailRepo _outpatientAccountDetailRepo;
        private readonly IHospItemBillingRepo _hospItemBillingRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IHospDrugBillingRepo _hospdrugbillingRepo;
        public BookkeepInHosDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        /// <summary>
        /// 查询收费项目模板
        /// </summary>
        /// <param name="ks"></param>
        /// <returns></returns>
        public List<ChargeItemTemplateVO> GetChargeTemplate(string ks, string orgid)
        {
            //var kslist = ks.Split(',').Distinct().ToList();
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT  a.sfmbbh ,
                            a.sfmb ,
                            a.sfmbmc ,
                            a.ks ,
                            b.Name ksmc
                    FROM    xt_sfmb a
                            left JOIN [NewtouchHIS_Base]..V_S_Sys_Department b 
                            ON a.ks = b.Code and b.OrganizeId = @OrganizeId
                    WHERE   1 = 1 AND a.ZT = 1 and a.OrganizeId = @OrganizeId");
            SqlParameter[] par = {
                       //new SqlParameter("@ks",ks ?? ""),
                        //new SqlParameter("@ks",kslist),
                        new SqlParameter("@OrganizeId",orgid)
                };
            if (!string.IsNullOrEmpty(ks) && ks != ",")
            {
                var kslist = "'" + string.Join("','", ks.Split(',').Distinct().ToList()) + "'";
                sql.Append(" and (b.Code in (" + kslist + ") or isnull(a.ks,'') = '') --and (b.Code = @ks or '' = @ks)");
            }
            return this.FindList<ChargeItemTemplateVO>(sql.ToString(), par);
        }

        /// <summary>
        /// 根据收费项目编号获取模板内容
        /// </summary>
        /// <param name="sfxmbh"></param>
        /// <returns></returns>
        public List<TemplateContentVO> GetChargeItemContent(string sfmbbh, string orgId)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT  DISTINCT A.sfmbxmId ,
                        CAST(A.sl AS VARCHAR(30)) sl ,
                        A.zll,A.zxcs,
                        B.py ,
                        B.sfxmCode sfxm,
                        B.sfxmmc ,
                        B.dw ,
                        B.dj ,
                        B.zfbl ,
                        B.zfxz ,
                        B.sfxmCode ,
                        B.ybdm ,
                        B.nbdlCode ,
                        B.dwjls ,
                        B.jjcl ,
                        C.dlmc ,
                        C.dlCode dl ,
                        '' AS gg ,
                        '' AS jl ,
                        '' AS yf ,
                        ISNULL(a.duration, b.duration) duration,
                        '' AS pl ,
                        '' AS jx,
                        a.kflb ,
                        itemdetail.Name kflbmc,
						dept.Code zxks, dept.Name zxksmc,
						yzpc.yzpcCode yzpc, yzpc.yzpcmc yzpcmc,
                        A.bw
                FROM    xt_sfmbxm A
                        INNER JOIN NewtouchHIS_Base..V_S_xt_sfxm B ON A.sfxm = B.sfxmCode
                                                                      AND B.zt = '1' AND b.OrganizeId=a.OrganizeId
                        INNER JOIN NewtouchHIS_Base..V_S_xt_sfdl C ON B.sfdlCode = C.dlCode AND c.OrganizeId=a.OrganizeId
                        LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail itemdetail ON ( itemdetail.OrganizeId = a.OrganizeId
                                                              OR itemdetail.OrganizeId = '*'
                                                              )
                                                              AND itemdetail.CateCode = 'RehabTreatmentMethod'
                                                              AND itemdetail.Code = a.kflb
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department dept ON dept.Code = A.zxks AND dept.OrganizeId=a.OrganizeId and dept.zt = '1'
LEFT JOIN NewtouchHIS_Base..V_S_xt_yzpc yzpc ON yzpc.yzpcCode = A.yzpc AND yzpc.OrganizeId=a.OrganizeId and yzpc.zt = '1'
                WHERE A.zt='1' and  sfmbbh = @sfmbbh
                        AND A.OrganizeId = @OrganizeId");
            SqlParameter[] par = new SqlParameter[] {
                     new SqlParameter("@sfmbbh",sfmbbh),
                      new SqlParameter("@OrganizeId",orgId)
            };
            return this.FindList<TemplateContentVO>(sql.ToString(), par);
        }

        #region 康复项目记账（由GRS收费项目扩展而来）

        /// <summary>
        /// 根据住院号 查询 记账计划
        /// </summary>
        /// <param name="zyhList"></param>
        /// <param name="zxrq"></param>
        public IList<HospAccountingPlanVO> BeenAccountingPlanQuery(string orgId, string zyh)
        {
            var sql = @"select *,d.[name] zxksmc from
(
select m.jzjhmxId, a.xm patientName, m.yzxz, m.StartDate, m.EndDate, yp.ypCode sfxmCode, yp.ypmc sfxmmc
, sfdl.dlCode sfdlCode, sfdl.dlmc sfdlmc
, yp.zycldw dw
, CONVERT(DECIMAL(19,4),
case when yp.zycldw = yp.bzdw then yp.lsj else (yp.lsj / yp.bzs * yp.zycls) end) dj
--用jzjh里的时长
, m.duration
, ISNULL(m.zcs, 0) sl
, m.bz
,m.ys, m.ysmc, m.ks, m.ksmc
,m.zxzt
,m.LastEexcutionTime
,m.yzlx
,m.CreateTime
,m.kflb
,m.zll
,0 dwjls
,1 jjcl,m.zxks
from zy_brjbxx a with(nolock),zy_jzjhmx m with(nolock)
left join [NewtouchHIS_Base]..V_S_xt_yp yp with(nolock)
on m.sfxmCode = yp.ypCode and yp.OrganizeId = @orgId
left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl with(nolock)
on yp.dlCode = sfdl.dlCode and sfdl.OrganizeId = @orgId
where m.OrganizeId = @orgId and m.zyh = a.zyh and a.OrganizeId = @orgId
and m.yzlx = '1'    --药品
--该病人
and a.zyh = @zyh
AND ( CASE WHEN m.zxzt = 3
AND ISNULL(m.yzxcs, 0) = 0 THEN 1
ELSE 0
END ) != 1
union all

select m.jzjhmxId, a.xm patientName, m.yzxz, m.StartDate, m.EndDate, sfxm.sfxmCode, sfxm.sfxmmc
, sfdl.dlCode sfdlCode, sfdl.dlmc sfdlmc
, sfxm.dw
,sfxm.dj
--用jzjh里的时长
, m.duration
, ISNULL(m.zcs, 0) sl
, m.bz
,m.ys, m.ysmc, m.ks, m.ksmc
,m.zxzt
,m.LastEexcutionTime
,m.yzlx
,m.CreateTime
,m.kflb
,m.zll
,sfxm.dwjls dwjls
,sfxm.jjcl,m.zxks
from zy_jzjhmx m with(nolock)
inner join zy_brjbxx a with(nolock)
on m.zyh = a.zyh and a.OrganizeId = @orgId
left join [NewtouchHIS_Base]..V_S_xt_sfxm sfxm with(nolock)
on m.sfxmCode = sfxm.sfxmCode and sfxm.OrganizeId = @orgId
left join [NewtouchHIS_Base]..V_S_xt_sfdl sfdl with(nolock)
on sfxm.sfdlCode = sfdl.dlCode and sfdl.OrganizeId = @orgId
where m.OrganizeId = @orgId
and m.yzlx = '2'    --项目
--该病人
and a.zyh = @zyh
AND ( CASE WHEN m.zxzt = 3
AND ISNULL(m.yzxcs, 0) = 0 THEN 1
ELSE 0
END ) != 1
) a
left join  NewtouchHIS_Base.dbo.Sys_Department d with(nolock) on a.zxks=d.Code and d.OrganizeId=@orgId and d.zt='1'
order by StartDate desc, a.CreateTime desc"; 
            return this.FindList<HospAccountingPlanVO>(sql
                    , new[] { new SqlParameter("@orgId", orgId)
                    ,new SqlParameter("@zyh", zyh)
                });
        }

        /// <summary>
        /// 保存住院记账计划
        /// </summary>
        /// <param name="jzjh"></param>
        /// <param name="jhmxEntityList"></param>
        /// <param name="ysEntityList"></param>
        public void SaveAccountingPlan(HospAccountingPlanEntity jzjh, List<HospAccountingPlanDetailEntity> jhmxEntityList, List<string> updateToStopJzjhxmIdList)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Insert(jzjh);

                foreach (var mx in jhmxEntityList)
                {
                    db.Insert(mx);
                }

                foreach (var jzjhmx in db.IQueryable<HospAccountingPlanDetailEntity>(p => updateToStopJzjhxmIdList.Contains(p.jzjhmxId)).ToList())
                {
                    if (jzjhmx.zxzt == (int)EnumJzjhZXZT.None)
                    {
                        ////从未执行过 del
                        //db.Delete(jzjhmx);
                        if (string.IsNullOrWhiteSpace(jzjhmx.yzxcs.ToString()) || jzjhmx.yzxcs == 0)
                        {

                            var zyjfb = db.IQueryable<HospItemBillingEntity>().Where(p => p.jzjhmxId == jzjhmx.jzjhmxId && p.zt == "1" && p.OrganizeId == jzjhmx.OrganizeId).ToList();
                            //        var zyjfb = db.FindList<HospItemBillingEntity>(@"SELECT  *
                            //            FROM    dbo.zy_xmjfb
                            //            WHERE   jzjhmxId = @jzjhmxId
                            //                    AND zt = '1'
                            //                    AND OrganizeId = @orgId", new[] { new SqlParameter("@orgId", jzjhmx.jzjhmxId)
                            //,new SqlParameter("@jzjhmxId", jzjhmx.jzjhmxId) });
                            foreach (var item in zyjfb)
                            {
                                item.zt = "0";//无效
                                item.Modify();
                                db.Update(item, new List<string> { "zt" });
                            };
                            var zyjzjhmx = db.IQueryable<HospAccountingPlanDetailEntity>().FirstOrDefault(p => p.jzjhmxId == jzjhmx.jzjhmxId && p.zt == "1" && p.OrganizeId == jzjhmx.OrganizeId);
                            //zyjzjhmx.zt = "0";//无效
                            jzjhmx.zxzt = (int)EnumJzjhZXZT.Stopped;
                            zyjzjhmx.Modify();
                            db.Update(zyjzjhmx, new List<string> { "zt", "zxzt" });
                        }
                    }
                    else
                    {
                        jzjhmx.zxzt = (int)EnumJzjhZXZT.Stopped;
                        jzjhmx.Modify();
                        db.Update(jzjhmx);
                    }
                }

                db.Commit();
            }
        }

        /// <summary>
        /// 获取机构住院病人列表（当前医生、存在未执行记账计划）
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<PendingExecutionPatientVO> GetPendingExecutionPatientVOList(string orgId,string kscode)
        {
            var sql = string.Format(@"WITH    cteTree
          AS ( SELECT   *
               FROM     [NewtouchHIS_Base]..V_S_Sys_Department(NOLOCK)
               WHERE    ISNULL(@zxks, '') <> ''
                        AND OrganizeId = @orgId
                        AND Code = @zxks
                        AND zt = '1' --第一个查询作为递归的基点(锚点)
               UNION ALL
               SELECT   [NewtouchHIS_Base]..V_S_Sys_Department.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
               FROM     cteTree
                        INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Department(NOLOCK) ON cteTree.Id = [NewtouchHIS_Base]..V_S_Sys_Department.ParentId
                                                              AND [NewtouchHIS_Base]..V_S_Sys_Department.zt = '1'
             )
        SELECT DISTINCT
        xx.py ,
        b.bqCode ,
        a.xm ,
        a.zyh ,
        a.patid ,a.blh,
        ybba.sycs ybsycs,
        a.ryrq ghrq,
        a.CreateTime
        FROM    zy_jzjhmx m
        inner join zy_brjbxx a
        on m.zyh = a.zyh and a.OrganizeId = @orgId and a.zt = '1'
        inner JOIN dbo.xt_brjbxx xx ON xx.blh = a.blh
                                           AND xx.OrganizeId =@orgId
                                           AND xx.zt = '1'
        left join [NewtouchHIS_Base]..V_S_xt_bq b
        on a.bq = b.bqCode and b.OrganizeId = @orgId
        --join医保备案
        left join xt_ybbab ybba
        on a.patId = ybba.patId and ybba.OrganizeId = @orgId 
        and ybba.zt = '1' and ybba.ksrq <= getdate() and getdate() <= ybba.jsrq

        where m.OrganizeId = @orgId 
        --在院标志
        and a.zybz not in ({0})
        and m.zt = '1' 
        --执行状态
        and (m.zxzt = 0 or m.zxzt = 1)
        --执行科室
                        AND ( ISNULL(m.zxks, '') = ''
                              OR m.zxks IN ( SELECT Code
                                             FROM   cteTree ))
        --是该医生治疗的
        --and m.ys is not null and (',' + m.ys + ',') like @ysCode
        AND ISNULL(a.blh, '') != ''
        ORDER BY a.ryrq,a.CreateTime", ((int)EnumZYBZ.Wry + "," + (int)EnumZYBZ.Ycy));
            var pars = new List<SqlParameter>() {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zxks", kscode)
            };
            return this.FindList<PendingExecutionPatientVO>(sql, pars.ToArray());
        }

        /// <summary>
        /// 获取机构门诊病人列表（当前医生、存在未执行记账计划）
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<PendingExecutionPatientVO> GetOutPendingExecutionPatientVOList(string orgId, string kscode)
        {
           
            var sql = string.Format(@"
                        WITH    cteTree
          AS ( SELECT   *
               FROM     [NewtouchHIS_Base]..V_S_Sys_Department(NOLOCK)
               WHERE    ISNULL(@zxks, '') <> ''
                        AND OrganizeId = @orgId
                        AND Code = @zxks
                        AND zt = '1' --第一个查询作为递归的基点(锚点)
               UNION ALL
               SELECT   [NewtouchHIS_Base]..V_S_Sys_Department.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
               FROM     cteTree
                        INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Department(NOLOCK) ON cteTree.Id = [NewtouchHIS_Base]..V_S_Sys_Department.ParentId
                                                              AND [NewtouchHIS_Base]..V_S_Sys_Department.zt = '1'
             )
                        SELECT DISTINCT
                        xx.py ,
                        a.xm ,
                        a.mzh zyh ,
                        a.patid ,
                        a.blh,
		                a.ghrq,
                        a.CreateTime
                FROM    mz_jzjhmx m
                        JOIN mz_xm xm ON xm.jzjhmxId = m.jzjhmxId
                                         AND xm.OrganizeId = m.OrganizeId
                        INNER JOIN mz_gh a ON xm.ghnm = a.ghnm
                                              AND a.OrganizeId = @orgId
                                              AND a.zt = '1'
                        inner JOIN dbo.xt_brjbxx xx ON xx.blh = a.blh
                                           AND xx.OrganizeId = @orgId AND xx.zt = '1'
                        --join医保备案
                        LEFT JOIN xt_ybbab ybba ON a.patId = ybba.patId
                                                   AND ybba.OrganizeId = @orgId
                                                   AND ybba.zt = '1'
                                                   AND ybba.ksrq <= GETDATE()
                                                   AND GETDATE() <= ybba.jsrq
                WHERE   m.OrganizeId = @orgId 
                        --在院标志
                        AND m.zt = '1' 
                        --执行状态
                        AND ( m.zxzt = 0
                              OR m.zxzt = 1
                            )
                        AND xm.zt = '1'
                        --执行科室
                        AND ( ISNULL(m.zxks, '') = ''
                              OR m.zxks IN ( SELECT Code
                                             FROM   cteTree ))
                        --是该医生治疗的
                        --AND m.ys IS NOT NULL
                        --AND ( ',' + m.ys + ',' ) LIKE @ysCode 
                        ORDER BY a.ghrq,
                        a.CreateTime");
            var pars = new List<SqlParameter>() {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zxks", kscode),
            };
            return this.FindList<PendingExecutionPatientVO>(sql, pars.ToArray());
        }

        /// <summary>
        /// 待执行住院记账计划 查询（仅收费项目，不包含药品）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyhStr"></param>
        /// <param name="gh"></param>
        /// <param name="zxks"></param>
        /// <param name="isRehabDoctor"></param>
        /// <returns></returns>
        public IList<HospAccountingPlanWaitingExeVO> WaitingAccountingPlanQuery(string orgId, string zyhStr,string gh, string zxks, bool isRehabDoctor)
        {
            var sql = string.Format(@"WITH cteTree
        AS (SELECT * FROM [NewtouchHIS_Base]..V_S_Sys_Department(nolock)
              WHERE isnull(@zxks,'') <> '' and OrganizeId = @orgId and Code = @zxks and zt = '1' --第一个查询作为递归的基点(锚点)
            UNION ALL
            SELECT [NewtouchHIS_Base]..V_S_Sys_Department.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
              FROM
                   cteTree INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Department(nolock) ON cteTree.Id= [NewtouchHIS_Base]..V_S_Sys_Department.ParentId and [NewtouchHIS_Base]..V_S_Sys_Department.zt = '1') 

select m.jzjhId,m.jzjhmxId, a.xm patientName, m.yzxz,
                sfxm.sfxmmc, m.sl, isnull(jhmxDj.dj, sfxm.dj) dj, m.ysmc, isnull(jhmxDj.dj, sfxm.dj)*m.sl zje, m.bz
                ,m.LastEexcutionTime
                ,ISNULL(m.kflb, (SELECT TOP 1
                            xm.kflb
                     FROM   dbo.zy_xmjfb xm
                     WHERE  xm.jzjhmxId = m.jzjhmxId
                            AND xm.sfxm = m.sfxmCode
                            AND xm.zt = '1'
                            AND xm.OrganizeId = @orgId
                     ORDER BY xm.CreateTime DESC)) kflb
                ,m.zcs,isnull(m.yzxcs,0)  yzxcs
                ,m.zll
                ,CASE WHEN @isRehabDoctor <> 0--治疗师
                          THEN ( CASE WHEN ISNULL(m.yzxcs, 0) = 0 THEN @gh
                                      WHEN ISNULL(m.yzxcs, 0) > 0 THEN (SELECT TOP 1
                                                xm.ssry
                                         FROM   dbo.zy_xmjfb xm
                                         WHERE  xm.jzjhmxId = m.jzjhmxId
                                                AND xm.sfxm = m.sfxmCode
                                                AND xm.OrganizeId = @orgId
                                                AND xm.zt = '1'
                                         ORDER BY xm.CreateTime DESC)
                                 END )
                     WHEN @isRehabDoctor = 0--非治疗师
                          THEN ( SELECT TOP 1
                                    xm.ssry
                             FROM   dbo.zy_xmjfb xm
                             WHERE  xm.jzjhmxId = m.jzjhmxId
                                    AND xm.sfxm = m.sfxmCode
                                    AND xm.OrganizeId = @orgId
                                    AND xm.zt = '1'
                             ORDER BY xm.CreateTime DESC )
                END zlsgh,
            ( SELECT    COUNT(1)
              FROM      dbo.zy_xmjfb
              WHERE     jzjhmxId = m.jzjhmxId
                        AND zt = '1'
                        AND OrganizeId =  @orgId
                        AND ssrq = CONVERT(CHAR(10), GETDATE(), 120)
            ) dtzxcs,
			m.zxks,
	        m.CreateTime entryTime
                from zy_jzjhmx m
                inner join zy_brjbxx a
                on m.zyh = a.zyh and a.OrganizeId = @orgId
                left join [NewtouchHIS_Base]..V_S_xt_sfxm sfxm
                on m.sfxmCode = sfxm.sfxmCode and sfxm.OrganizeId = @orgId
                left join (select jzjhmxId, max(dj)dj from zy_xmjfb where OrganizeId = @orgId group by jzjhmxId) jhmxDj
				on jhmxDj.jzjhmxId = m.jzjhmxId
                where m.OrganizeId = @orgId
                and m.yzlx = '2'    --项目计划
                --and xmjf.jfbbh is null  --zxrq没有执行计费过
                --在院标志
                and a.zybz in ({0}) and m.zt = '1'
                --执行状态
                and (m.zxzt = 0 or m.zxzt = 1)
                --日期范围
                --and m.StartDate <= @zxrq
                --and (m.EndDate is null or DATEADD(DAY,1,m.EndDate) > @zxrq)
                --是该医生治疗的
                --and m.ys is not null and (',' + m.ys + ',') like @ysCode
                --该病人
                and a.zyh in (select * from dbo.f_split(@zyhStr,','))
				--执行科室
				and (isnull(m.zxks,'') = '' or m.zxks in(select Code from cteTree))

				order by m.zyh--m.CreateTime", ((int)EnumZYBZ.Bqz) + "," + ((int)EnumZYBZ.Djz));
            return this.FindList<HospAccountingPlanWaitingExeVO>(sql
                    , new[] { new SqlParameter("@orgId", orgId)
                    ,new SqlParameter("@zyhStr", zyhStr)
                    ,new SqlParameter("@gh", gh)
                    ,new SqlParameter("@zxks", zxks)
                    ,new SqlParameter("@isRehabDoctor", isRehabDoctor)
                });
        }

        /// <summary>
        /// 待执行门诊记账计划 查询（仅收费项目，不包含药品）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="Str"></param>
        /// <param name="zxrq"></param>
        /// <param name="ysCode"></param>
        /// <returns></returns>
        public IList<HospAccountingPlanWaitingExeVO> WaitingOutAccountingPlanQuery(string orgId, string Str, string gh, string zxks,bool isRehabDoctor)
        {
            var sql = string.Format(@"WITH cteTree
        AS (SELECT * FROM [NewtouchHIS_Base]..V_S_Sys_Department(nolock)
              WHERE isnull(@zxks,'') <> '' and OrganizeId = @orgId and Code = @zxks and zt = '1' --第一个查询作为递归的基点(锚点)
            UNION ALL
            SELECT [NewtouchHIS_Base]..V_S_Sys_Department.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
              FROM
                   cteTree INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Department(nolock) ON cteTree.Id= [NewtouchHIS_Base]..V_S_Sys_Department.ParentId and [NewtouchHIS_Base]..V_S_Sys_Department.zt = '1') 

SELECT  m.jzjhId,m.jzjhmxId ,
        gh.xm patientName ,
        sfxm.sfxmmc ,
        m.sl ,
        xm.dj ,
        m.ysmc ,
        xm.dj * m.sl zje ,
        m.bz ,
        m.LastEexcutionTime,
        ISNULL(m.kflb,  ( SELECT TOP 1
                            zx.kflb
                     FROM   dbo.mz_jzjhmxzx zx
                     WHERE  zx.jzjhmxId = m.jzjhmxId
                            AND zx.zt = '1'
                            AND zx.OrganizeId = @orgId ORDER BY zx.CreateTime DESC
                   )) kflb ,
        m.zcs,
        isnull(m.yzxcs,0)  yzxcs,
        m.zll,
           CASE WHEN @isRehabDoctor <> 0--治疗师
                  THEN ( CASE WHEN ISNULL(m.yzxcs, 0) = 0 THEN @gh
                              WHEN ISNULL(m.yzxcs, 0) > 0 THEN (SELECT TOP 1
                                    zx.zlsgh
                             FROM   dbo.mz_jzjhmxzx zx
                             WHERE  zx.jzjhmxId = m.jzjhmxId
                                    AND zx.zt = '1'
                                    AND zx.OrganizeId = @orgId ORDER BY zx.CreateTime DESC)
                         END )
             WHEN @isRehabDoctor = 0--非治疗师
                  THEN ( SELECT TOP 1
                                    zx.zlsgh
                             FROM   dbo.mz_jzjhmxzx zx
                             WHERE  zx.jzjhmxId = m.jzjhmxId
                                    AND zx.zt = '1'
                                    AND zx.OrganizeId = @orgId ORDER BY zx.CreateTime DESC )
        END zlsgh,
  ( SELECT    COUNT(1)
              FROM      dbo.mz_jzjhmxzx
              WHERE     jzjhmxId = m.jzjhmxId
                        AND OrganizeId =  @orgId
                        AND zt = '1'
                        AND zxsj = CONVERT(CHAR(10), GETDATE(), 120)
            ) dtzxcs ,
m.zxks,
	m.CreateTime entryTime
from
(
select * from mz_gh(nolock) where mzh in (SELECT  * FROM  dbo.f_split(@zyhStr, ','))
) gh
inner join mz_xm(nolock) xm
ON gh.ghnm = xm.ghnm AND gh.OrganizeId = @orgId and xm.zt = '1'
inner join mz_jzjhmx(nolock) m
ON xm.jzjhmxId = m.jzjhmxId AND xm.OrganizeId = @orgId and m.zt = '1'
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_sfxm sfxm
ON m.sfxmCode = sfxm.sfxmCode AND sfxm.OrganizeId = @orgId

WHERE m.OrganizeId = @orgId
AND gh.zt = '1'
--执行状态
AND (m.zxzt = 0 OR m.zxzt = 1)
--执行科室
and (isnull(m.zxks,'') = '' or m.zxks in(select Code from cteTree))
ORDER BY gh.ghnm --m.CreateTime");
            return this.FindList<HospAccountingPlanWaitingExeVO>(sql
                , new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@zyhStr", Str)
                ,new SqlParameter("@gh", gh)
                ,new SqlParameter("@zxks", zxks??"")
                ,new SqlParameter("@isRehabDoctor", isRehabDoctor)
                });
        }

        /// <summary>
        /// 执行记账计划，生成计费
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zxItem"></param>
        /// <param name="curOprCode"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="zxrq">治疗日期</param>
        public void ExecuteAccountingPlan(string orgId, IList<zxGirdDto> zxItem, string curOprCode, string curOprDeptCode, DateTime zxrq,out Dictionary<string, string> jfbStr)
        {
            jfbStr = new Dictionary<string, string>();
            string jfbbh = "";
            if (zxrq.Date > DateTime.Now.Date)
            {
                throw new FailedException("不能提前执行记账计划");
            }
            bool needCommit = false;
            if (zxItem != null && zxItem.Count() > 0)
            {
                
                string zyh = "";
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    foreach (var item in zxItem)
                    {
                        var jzjhmxEntity = db.IQueryable<HospAccountingPlanDetailEntity>().FirstOrDefault(p => p.zt == "1" && p.OrganizeId == orgId && p.jzjhmxId == item.jzjhmxId);
                        if (zyh!= jzjhmxEntity.zyh)
                        {
                            zyh += ","+ jzjhmxEntity.zyh;
                        }
                        if (jzjhmxEntity.zxzt == (int)EnumJzjhZXZT.None || jzjhmxEntity.zxzt == (int)EnumJzjhZXZT.Part)
                        {
                            zyh = jzjhmxEntity.zyh;
                            if (
                                    (!jzjhmxEntity.EndDate.HasValue || jzjhmxEntity.EndDate.Value.Date >= zxrq.Date)
                                &&
                                    zxrq >= jzjhmxEntity.StartDate.Date
                                )
                            {
                                //另外 zxrq得在日期范围内
                                //zxzt与实际执行不一致会导致多执行？
                                if (jzjhmxEntity.yzlx == "1") //1药品
                                {
                                    if (!executeMedicineAccountingPlan(db, jzjhmxEntity, curOprCode, curOprDeptCode, zxrq))
                                    {
                                        continue;
                                    }
                                }
                                else if (jzjhmxEntity.yzlx == "2")    //2项目
                                {
                                    if (!executeItemAccountingPlan(db, jzjhmxEntity, curOprCode, curOprDeptCode, zxrq, item, out jfbbh))
                                    {
                                        continue;
                                    }
                                    else if (!string.IsNullOrEmpty(jzjhmxEntity.yzId))
                                    {
                                        if (jfbStr != null && jfbStr.Count() > 0 && jfbStr.ContainsKey(jzjhmxEntity.zyh))
                                        {
                                            var newdic = jfbStr.Keys.ToArray<string>();
                                            for (int i = 0; i < newdic.Length; i++)
                                            {
                                                if (newdic[i] == jzjhmxEntity.zyh)
                                                {

                                                    string v = "";
                                                    jfbStr.TryGetValue(newdic[i], out v);
                                                    var newv = v + jfbbh + ",";
                                                    jfbStr.Remove(newdic[i]);
                                                    jfbStr.Add(jzjhmxEntity.zyh, newv);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            jfbStr.Add(jzjhmxEntity.zyh, jfbbh + ",");
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                if (!jzjhmxEntity.LastEexcutionTime.HasValue || jzjhmxEntity.LastEexcutionTime.Value.Date < zxrq.Date)
                                {
                                    jzjhmxEntity.LastEexcutionTime = zxrq;
                                }

                                jzjhmxEntity.LastModifierCode = curOprCode;
                                jzjhmxEntity.LastModifyTime = DateTime.Now;

                                //执行状态

                                db.Update(jzjhmxEntity);
                                needCommit = true;
                            }
                            else {
                                var sfxmSql = "select * from [NewtouchHIS_Base]..V_S_xt_sfxm(nolock) where sfxmCode = @sfxmCode and OrganizeId = @orgId";
                                var sfxm = this.FirstOrDefault<SysChargeItemVEntity>(sfxmSql, new[] { new SqlParameter("@sfxmCode", jzjhmxEntity.sfxmCode)
                                , new SqlParameter("@orgId", jzjhmxEntity.OrganizeId) });
                                throw new FailedException(sfxm.sfxmmc+",执行日期不能小于开单日期");
                            }
                        }
                        //
                        
                    }

                    if (needCommit)
                    {
                        db.Commit();
                    }
                    if (zyh!=null&&zyh != "")
                    {
                        _hospdrugbillingRepo.Updatezy_brxxexpand(orgId, zyh);
                    }
                    //否则没必要Commit
                }
            }

            //更新病人医保使用次数
        }

        /// <summary>
        /// 执行门诊记账计划
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mxIdList"></param>
        /// <param name="curOprCode"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="zxrq"></param>
        public void ExecuteOutAccountingPlan(string orgId, IList<zxGirdDto> zxItem, string curOprCode, string curOprDeptCode, DateTime zxrq)
        {
            if (zxrq.Date > DateTime.Now.Date)
            {
                throw new FailedException("不能提前执行记账计划");
            }
            bool needCommit = false;
            if (zxItem != null && zxItem.Count() > 0)
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    foreach (var item in zxItem)
                    {
                        var jzjhmxEntity = db.IQueryable<OutpatientAccountDetailEntity>().FirstOrDefault(p => p.zt == "1" && p.OrganizeId == orgId && p.jzjhmxId == item.jzjhmxId);
                        if (jzjhmxEntity.zxzt == (int)EnumJzjhZXZT.None || jzjhmxEntity.zxzt == (int)EnumJzjhZXZT.Part)
                        {
                            if (
                                    (!jzjhmxEntity.EndDate.HasValue || jzjhmxEntity.EndDate.Value.Date >= zxrq)
                                &&
                                    zxrq >= jzjhmxEntity.StartDate.Value.Date
                                )
                            {
                                //另外 zxrq得在日期范围内
                                //zxzt与实际执行不一致会导致多执行？
                                //仅项目
                                if (!executeItemOutAccountingPlan(db, jzjhmxEntity, curOprCode, curOprDeptCode, zxrq, item))
                                {
                                    continue;
                                }

                                if (!jzjhmxEntity.LastEexcutionTime.HasValue || jzjhmxEntity.LastEexcutionTime.Value.Date < zxrq.Date)
                                {
                                    jzjhmxEntity.LastEexcutionTime = zxrq;
                                }
                                jzjhmxEntity.LastModifierCode = curOprCode;
                                jzjhmxEntity.LastModifyTime = DateTime.Now;

                                db.Update(jzjhmxEntity);
                                needCommit = true;
                            }
                            
                            //db.ExecuteSqlCommand();
                        }
                    }

                    if (needCommit)
                    {
                        db.Commit();
                    }
                    //否则没必要Commit
                }
            }

            //医保备案 更新剩余次数
        }

        /// <summary>
        /// 停止 记账计划
        /// </summary>
        /// <param name="mxIdList">jzjhmxId列表</param>
        /// <param name="stopDate"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="curOprCode"></param>
        public void StopAccountingPlan(IList<string> mxIdList, DateTime stopDate, string curOprDeptCode, string curOprCode)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var mxList = db.IQueryable<HospAccountingPlanDetailEntity>(p => mxIdList.Contains(p.jzjhmxId)).ToList();
                foreach (var mx in mxList)
                {
                    if (mx.zxzt == (int)EnumJzjhZXZT.None || mx.zxzt == (int)EnumJzjhZXZT.Part)
                    //未执行、执行部分的 才能 停止
                    {
                        mx.LastModifierCode = curOprCode;
                        mx.LastModifyTime = DateTime.Now;
                        if (stopDate.Date == DateTime.Now.Date)
                        {
                            mx.zxzt = (int)EnumJzjhZXZT.Stopped;
                        }
                        else if (stopDate.Date > DateTime.Now.Date)
                        {
                            mx.PreStopDate = stopDate.Date;
                        }
                        else
                        {
                            continue;   //表单提交错误，提交了今天之前的日期
                        }
                        db.Update(mx);
                    }
                }
                db.Commit();
            }
        }

        /// <summary>
        /// 取消预停止
        /// </summary>
        /// <param name="jzjhmxIdList">jzjhmxId列表</param>
        public void CancelPreStopAccountingPlan(IList<string> mxIdList, string curOprCode, string curOprDeptCode)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var mxList = db.IQueryable<HospAccountingPlanDetailEntity>(p => mxIdList.Contains(p.jzjhmxId)).ToList();
                foreach (var mx in mxList)
                {
                    if (mx.PreStopDate.HasValue)
                    {
                        mx.LastModifierCode = curOprCode;
                        mx.LastModifyTime = DateTime.Now;
                        mx.PreStopDate = null;
                        db.Update(mx);
                    }
                }
                db.Commit();
            }
        }

        #endregion

        #region 记账查询

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zxzt"></param>
        /// <param name="orgId"></param>
        /// <param name="zls"></param>
        /// <param name="cxzt"></param>
        /// <param name="jzjhmxId"></param>
        /// <returns></returns>
        public List<AccountingExecuteVO> SelectAccountingExecuteList(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, int? zxzt, string orgId
            , string zls, string cxzt, string jzjhmxId
            , string sfzt)
        {
            StringBuilder strSql = new StringBuilder();
            var pars = new List<SqlParameter>();
            strSql.Append(@"
select Convert(varchar(50),xmjf.jfbbh) zxjlId
, jzjhmx.jzjhmxId jzjhmxId, brjbxx.xm xm
,brjbxx.blh
,jzjhmx.zyh mzzyh
,sfxm.sfxmmc
,sfxm.dw
,xmjf.zt zt
,xmjf.zzll zll
,xmjf.sl sl
,xmjf.dj dj
,(xmjf.dj * xmjf.sl) je
,staff.Name zlsmc
,xmjf.ssrq zlrq
, us.Name Creater
,xmjf.CreateTime CreateTime
,sfxm.zfxz
,xmjf.kflb
,case when xmjf.zt = '1' and zyhjsjsrq.jsjsrq is not null and zyhjsjsrq.jsjsrq >= xmjf.CreateTime then '1' else '0' end sfzt
from zy_xmjfb(nolock) xmjf
left join zy_jzjhmx(nolock) jzjhmx
on xmjf.jzjhmxId = jzjhmx.jzjhmxId
LEFT JOIN zy_brjbxx(nolock) brjbxx
ON brjbxx.zyh=jzjhmx.zyh AND brjbxx.OrganizeId=@orgId
 INNER JOIN dbo.xt_brjbxx xx ON xx.blh = brjbxx.blh
                                       AND xx.OrganizeId =@orgId
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm(nolock) sfxm
ON sfxm.sfxmCode=jzjhmx.sfxmCode AND sfxm.OrganizeId=@orgId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff(nolock) staff
ON staff.gh=xmjf.ssry AND staff.OrganizeId=@orgId
LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff us ON us.Account = xmjf.CreatorCode
AND us.OrganizeId = @orgId
--最后结算时间
left join
(
select zyh, max(CreateTime) jsjsrq
from zy_js(nolock)
where OrganizeId = @orgId and zt = '1'
--jszt 1 已结
and jszt = '1'
and jsjsrq is not null
and jsnm not in(
	select cxjsnm from zy_js(nolock)
	where OrganizeId = @orgId and zt = '1'
	--jszt 2 已退
	and jszt = '2'
)
group by zyh
) zyhjsjsrq
on zyhjsjsrq.zyh = xmjf.zyh
where 1 = 1
and xmjf.OrganizeId = @orgId and jzjhmx.zt = '1' AND xx.zt = '1'");
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(" AND (brjbxx.blh like @keyword or brjbxx.xm like @keyword or brjbxx.zyh like @keyword or xx.py like @keyword)");
                pars.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
            }
            if (kssj.HasValue)
            {
                //strSql.Append(" AND xmjf.ssrq >= @kssj ");
                strSql.Append(" AND xmjf.CreateTime >= @kssj ");
                pars.Add(new SqlParameter("@kssj", kssj.Value));
            }
            if (jssj.HasValue)
            {
                //strSql.Append(" AND xmjf.ssrq < @jssj ");
                strSql.Append(" AND xmjf.CreateTime < @jssj ");
                pars.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
            }
            if (zxzt.HasValue)
            {
                strSql.Append(" AND jzjhmx.zxzt = @jhzxzt ");
                pars.Add(new SqlParameter("@jhzxzt", zxzt.Value));
            }
            if (!string.IsNullOrWhiteSpace(zls))
            {
                strSql.Append(" AND xmjf.ssry = @zls ");
                pars.Add(new SqlParameter("@zls", zls));
            }
            if (!string.IsNullOrWhiteSpace(cxzt))
            {
                strSql.Append(" AND xmjf.zt = @cxzt ");
                pars.Add(new SqlParameter("@cxzt", cxzt));
            }
            if (!string.IsNullOrWhiteSpace(sfzt))
            {
                if (sfzt == "1")
                {
                    //已收费   //应该同时是有效记录
                    strSql.Append(" and xmjf.zt = '1' AND zyhjsjsrq.jsjsrq is not null and zyhjsjsrq.jsjsrq >= xmjf.CreateTime");
                }
                else
                {
                    //未收费
                    strSql.Append(" AND (xmjf.zt = '0' or zyhjsjsrq.jsjsrq is null or zyhjsjsrq.jsjsrq < xmjf.CreateTime)");
                }
            }

            var list = this.QueryWithPage<AccountingExecuteVO>(strSql.ToString(), pagination, pars.ToArray()).ToList();
            return list;
        }

        /// <summary>
        /// 门诊计划执行查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zxzt"></param>
        /// <param name="orgId"></param>
        /// <param name="zls"></param>
        /// <param name="cxzt"></param>
        /// <param name="jzjhmxId"></param>
        /// <param name="sfzt"></param>
        /// <returns></returns>
        public List<AccountingExecuteVO> SelectOutAccountingExecuteList(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, int? zxzt, string orgId
            , string zls, string cxzt, string jzjhmxId
            , string sfzt)

        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strCountSql = new StringBuilder();
            strSql.Append(@"SELECT  zx.id zxjlId ,
        zx.jzjhmxId ,
        gh.xm ,
        gh.blh ,
        gh.mzh mzzyh ,
        sfxm.sfxmmc ,
        sfxm.dw ,
        zx.zt ,
        zx.zll ,
        zx.sl ,
        xm.dj ,
        staff.Name zlsmc ,
        zx.zxsj zlrq,
        sfxm.zfxz,
        zx.kflb,
		us.Name Creater,
        zx.CreateTime CreateTime ,
        CAST(ISNULL(ROUND(zx.sl * xm.dj, 2), 0) AS DECIMAL(19, 2)) je
FROM    dbo.mz_jzjhmxzx (NOLOCK) zx
        LEFT JOIN dbo.mz_jzjhmx mx ON mx.jzjhmxId = zx.jzjhmxId AND mx.OrganizeId=@orgId
        INNER JOIN dbo.mz_jzjh jh ON jh.jzjhId = mx.jzjhId AND jh.OrganizeId=@orgId
        INNER JOIN dbo.mz_gh gh ON gh.ghnm = jh.ghnm AND gh.OrganizeId=@orgId
        INNER JOIN dbo.xt_brjbxx xx ON xx.blh = gh.blh
                                       AND xx.OrganizeId =@orgId
        LEFT JOIN mz_xm xm ON xm.jzjhmxId = zx.jzjhmxId AND xm.OrganizeId=@orgId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm (NOLOCK) sfxm ON sfxm.sfxmCode = mx.sfxmCode
                                                              AND sfxm.OrganizeId = @orgId
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff (NOLOCK) staff ON staff.gh = zx.zlsgh
                                                              AND staff.OrganizeId = @orgId									  
        LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff us ON us.Account = zx.CreatorCode
                                                            AND us.OrganizeId = @orgId
WHERE zx.OrganizeId=@orgId and xx.zt='1' ");
            //countSql
            strCountSql.Append(@"SELECT  count(1)
FROM    dbo.mz_jzjhmxzx (NOLOCK) zx
        LEFT JOIN dbo.mz_jzjhmx mx ON mx.jzjhmxId = zx.jzjhmxId AND mx.OrganizeId=@orgId
        INNER JOIN dbo.mz_jzjh jh ON jh.jzjhId = mx.jzjhId AND jh.OrganizeId=@orgId
        INNER JOIN dbo.mz_gh gh ON gh.ghnm = jh.ghnm AND gh.OrganizeId=@orgId
        INNER JOIN dbo.xt_brjbxx xx ON xx.blh = gh.blh
                                       AND xx.OrganizeId =@orgId
WHERE zx.OrganizeId=@orgId and xx.zt='1'");
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(" AND ( gh.blh LIKE @keyword OR gh.xm LIKE @keyword OR gh.mzh LIKE @keyword or xx.py like @keyword)");
                strCountSql.Append(" AND ( gh.blh LIKE @keyword OR gh.xm LIKE @keyword OR gh.mzh LIKE @keyword or xx.py like @keyword)");
                pars.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
            }
            if (kssj.HasValue)
            {
                //strSql.Append(" AND zx.zxsj >= @kssj ");
                //strCountSql.Append(" AND zx.zxsj >= @kssj ");
                strSql.Append(" AND zx.CreateTime >= @kssj ");
                strCountSql.Append(" AND zx.CreateTime >= @kssj ");
                pars.Add(new SqlParameter("@kssj", kssj.Value));
            }
            if (jssj.HasValue)
            {
                //strSql.Append(" AND zx.zxsj < @jssj ");
                //strCountSql.Append(" AND zx.zxsj < @jssj ");
                strSql.Append(" AND zx.CreateTime < @jssj ");
                strCountSql.Append(" AND zx.CreateTime < @jssj ");
                pars.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
            }
            if (zxzt.HasValue)
            {
                strSql.Append(" AND mx.zxzt = @jhzxzt ");
                strCountSql.Append(" AND mx.zxzt = @jhzxzt ");
                pars.Add(new SqlParameter("@jhzxzt", zxzt.Value));
            }
            if (!string.IsNullOrWhiteSpace(zls))
            {
                strSql.Append(" AND zx.zlsgh = @zls ");
                strCountSql.Append(" AND zx.zlsgh = @zls ");
                pars.Add(new SqlParameter("@zls", zls));
            }
            if (!string.IsNullOrWhiteSpace(cxzt))
            {
                strSql.Append(" AND zx.zt = @cxzt ");
                strCountSql.Append(" AND zx.zt = @cxzt ");
                pars.Add(new SqlParameter("@cxzt", cxzt));
            }
            var list = this.QueryWithPage<AccountingExecuteVO>(strSql.ToString(), pagination, pars.ToArray()
                , countSql : strCountSql.ToString()).ToList();
            return list;
        }

        #endregion

        #region 记账查询
        /// <summary>
        /// 记账计划查询
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<AccountingPlanVO> SelectAccountingPlanList(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, int? zxzt, string orgId, int? yzxz)
        {
            StringBuilder strSql = new StringBuilder();
            var pars = new List<SqlParameter>();
            strSql.Append(@"
SELECT   jzjhmx.jzjhmxId,
         jzjhmx.zxzt,
         brjbxx.xm,
		 brjbxx.blh,
		 jzjhmx.zyh mzzyh,
         sfxm.sfxmmc,
         sfxm.dw,
		 jzjhmx.zll,
		 jzjhmx.zcs,
        jzjhmx.sl,
		 zhzxsjItem.yzxcs,
case when jzjhmx.yzxz = '2' 
then null 
else (isnull(jzjhmx.zcs,0) - isnull(zhzxsjItem.yzxcs,0)) end sycs,
		 --jzjhmx.LastEexcutionTime,
		 zhzxsjItem.zhzlrq LastEexcutionTime,
         jzjhmx.startDate,
         jzjhmx.endDate,
         jzjhmx.bz,
         jzjhmx.CreateTime,
		 zhzxsjItem.zhxtzxsj,
            jzjhmx.yzxz
FROM zy_jzjhmx(nolock) jzjhmx
LEFT JOIN zy_brjbxx(nolock) brjbxx
    ON brjbxx.zyh=jzjhmx.zyh AND brjbxx.OrganizeId=@orgId
 INNER JOIN dbo.xt_brjbxx xx ON xx.blh = brjbxx.blh
                                       AND xx.OrganizeId =@orgId
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm(nolock) sfxm
    ON sfxm.sfxmCode=jzjhmx.sfxmCode AND sfxm.OrganizeId=@orgId
left join (
	select jzjhmxId, max(CreateTime) zhxtzxsj, max(ssrq) zhzlrq, count(1) yzxcs
	from zy_xmjfb(nolock)
	where zt = '1' and jzjhmxId is not null and OrganizeId = @orgId
	group by jzjhmxId
) as zhzxsjItem
on zhzxsjItem.jzjhmxId = jzjhmx.jzjhmxId
WHERE jzjhmx.OrganizeId=@orgId and jzjhmx.zt = '1' AND xx.zt = '1'
                         ");
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(" AND (brjbxx.blh like @keyword or brjbxx.xm like @keyword or brjbxx.zyh like @keyword or xx.py like @keyword)");
                pars.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
            }
            if (kssj.HasValue)
            {
                strSql.Append(" AND jzjhmx.CreateTime>=@kssj ");
                pars.Add(new SqlParameter("@kssj", kssj.Value));
            }
            if (jssj.HasValue)
            {
                strSql.Append(" AND jzjhmx.CreateTime < @jssj ");
                pars.Add(new SqlParameter("@jssj", jssj.Value.AddDays(1).Date));
            }
            if (zxzt.HasValue)
            {
                strSql.Append(" AND jzjhmx.zxzt = @jhzxzt ");
                pars.Add(new SqlParameter("@jhzxzt", zxzt.Value));
            }
            if (yzxz.HasValue)
            {
                strSql.Append(" AND jzjhmx.yzxz = @jhyzxz");
                pars.Add(new SqlParameter("@jhyzxz", yzxz.Value.ToString()));
            }
            var list = this.QueryWithPage<AccountingPlanVO>(strSql.ToString(), pagination, pars.ToArray()).ToList();
            return list;
        }

        /// <summary>
        /// 根据住院号 获取所有计费（项目/药品）总金额
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public decimal? GetJfZjeByZyh(string zyh, string orgId)
        {
            var sql = @"select sum(je)
from
(
select sum(isnull(dj,0) * isnull(sl,0)) je from zy_xmjfb
where (@zyh = '' or zyh = @zyh) and OrganizeId = @orgId and zt = '1'
union
select sum(isnull(dj,0) * isnull(sl,0)) je from zy_ypjfb
where (@zyh = '' or zyh = @zyh) and OrganizeId = @orgId and zt = '1'
) as alljfmx";
            return this.FirstOrDefault<decimal?>(sql, new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@zyh", zyh ?? "")});
        }

        /// <summary>
        /// 查询记账计划执行详情
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<AccoutingPlanDetailVO> GetFormPlan(Pagination pagination, string jzjhmxId, string orgId, string from)
        {
            if (!string.IsNullOrWhiteSpace(from))
            {
                var sql = "";
                if (from.ToLower() == "mz")
                {
                    sql = @"SELECT  zx.jzjhmxId ,
                        zx.Id zxId,
                        xx.xm ,
                        sfxm.sfxmmc ,
                        sfxm.dw ,
                        gh.mzh mzzyh ,
                        zx.zll ,
                        xx.blh ,
                        zx.sl ,
                        zx.zxsj zlrq,
                        items.Name kflb ,
                        staff.Name zls,
                        CAST(ISNULL(ROUND(zx.sl * xm.dj, 2),0) AS DECIMAL(19, 2)) je ,
                        sfxm.zfxz,
                        zx.CreateTime,
                        zx.zt
                FROM    dbo.mz_jzjhmxzx zx
                        JOIN dbo.mz_jzjhmx mx ON mx.jzjhmxId = zx.jzjhmxId
                                                 AND mx.OrganizeId = @orgId
                        LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON sfxm.sfxmCode = mx.sfxmCode
                                                                        AND sfxm.OrganizeId = @orgId
                        RIGHT JOIN dbo.mz_jzjh jh ON jh.jzjhId = mx.jzjhId
                                                     AND jh.OrganizeId = @orgId
                        RIGHT JOIN mz_gh gh ON gh.ghnm = jh.ghnm
                                               AND gh.OrganizeId = @orgId
                        LEFT JOIN mz_xm xm ON xm.jzjhmxId = zx.jzjhmxId
                              AND xm.OrganizeId = @orgId
                        LEFT JOIN dbo.xt_brjbxx xx ON xx.patid = jh.patid
                                                      AND xx.OrganizeId = @orgId
                        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = zx.zlsgh
                                                                           AND staff.OrganizeId = @orgId
                         LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail items ON items.catecode = 'RehabTreatmentMethod'
                                                                                        AND items.Code = zx.kflb
                                                                                        AND (items.OrganizeId = @orgId  OR items.OrganizeId = '*')
                WHERE   zx.OrganizeId = @orgId
                        AND zx.jzjhmxId = @jzjhmxId
                        AND gh.zt = 1";
                }
                else
                {
                    sql = @"SELECT  jfb.jzjhmxId , 
                             CAST(jfb.jfbbh AS VARCHAR(50)) zxId , 
                            xx.xm ,
                            sfxm.sfxmmc ,
                            sfxm.dw ,
                            xx.zyh mzzyh,
                            xx.blh ,
                            items.Name kflb,
                            jfb.ssrq zlrq,
                            staff.Name zls,  
                            mx.zll,
                            jfb.sl,
                            CAST(ISNULL(ROUND(jfb.sl * jfb.dj, 2),0) AS DECIMAL(19, 2)) je ,
                            sfxm.zfxz,
                            jfb.CreateTime,
                            jfb.zt
                    FROM dbo.zy_xmjfb jfb
                            LEFT JOIN dbo.zy_jzjhmx mx ON mx.jzjhmxId = jfb.jzjhmxId
                                      AND mx.OrganizeId = @orgId
                            LEFT JOIN dbo.zy_brjbxx xx ON jfb.zyh = xx.zyh
                                                          AND xx.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON sfxm.sfxmCode = jfb.sfxm
                                                                            AND sfxm.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail items ON items.catecode = 'RehabTreatmentMethod'
                                                                                  AND items.Code = jfb.kflb
                                                                                AND (items.OrganizeId = @orgId  OR items.OrganizeId = '*')
                                            and (items.OrganizeId = @orgId  OR items.OrganizeId = '*')
                            LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = jfb.ssry
                                                                               AND staff.OrganizeId = @orgId
                                                                             
                    WHERE jfb.OrganizeId = @orgId
                            AND jfb.jzjhmxId=@jzjhmxId
                            AND staff.zt = '1'";
                }
                return this.QueryWithPage<AccoutingPlanDetailVO>(sql, pagination,
                    new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@jzjhmxId", jzjhmxId ?? "")});
            }
            return null;
        }

        /// <summary>
        /// 查询记账计划执行详情
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public AccoutingPlanDetailVO GetFormPlan(string zxId, string orgId, string from)
        {
            if (!string.IsNullOrWhiteSpace(from))
            {
                var sql = "";
                if (from.ToLower() == "mz")
                {
                    sql = @"SELECT  zx.jzjhmxId ,
                        zx.Id zxId,
                        xx.xm ,
                        sfxm.sfxmmc ,
                        sfxm.dw ,
                        gh.mzh mzzyh ,
                        zx.zll ,
                        xx.blh ,
                        zx.sl ,
                        zx.zxsj zlrq,
                        zx.kflb,
                        staff.Name zls,
                        zx.zlsgh,
                        CAST(ISNULL(ROUND(zx.sl * xm.dj, 2),0) AS DECIMAL(19, 2)) je ,
                        sfxm.zfxz,
                        zx.CreateTime,
                        zx.zt
                FROM    dbo.mz_jzjhmxzx zx
                        JOIN dbo.mz_jzjhmx mx ON mx.jzjhmxId = zx.jzjhmxId
                                                 AND mx.OrganizeId = @orgId
                        LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON sfxm.sfxmCode = mx.sfxmCode
                                                                        AND sfxm.OrganizeId = @orgId
                        RIGHT JOIN dbo.mz_jzjh jh ON jh.jzjhId = mx.jzjhId
                                                     AND jh.OrganizeId = @orgId
                        RIGHT JOIN mz_gh gh ON gh.ghnm = jh.ghnm
                                               AND gh.OrganizeId = @orgId
                        LEFT JOIN mz_xm xm ON xm.jzjhmxId = zx.jzjhmxId
                              AND xm.OrganizeId = @orgId
                        LEFT JOIN dbo.xt_brjbxx xx ON xx.patid = jh.patid
                                                      AND xx.OrganizeId = @orgId
                        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = zx.zlsgh
                                                                           AND staff.OrganizeId = @orgId
                         LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail items ON items.catecode = 'RehabTreatmentMethod'
                                                                                        AND items.Code = zx.kflb
                                                                                        AND (items.OrganizeId = @orgId  OR items.OrganizeId = '*')
                WHERE   zx.OrganizeId = @orgId
                        AND zx.Id = @zxId
                        AND gh.zt = 1";
                }
                else
                {
                    sql = @"SELECT  jfb.jzjhmxId , 
                             CAST(jfb.jfbbh AS VARCHAR(50)) zxId , 
                            xx.xm ,
                            sfxm.sfxmmc ,
                            sfxm.dw ,
                            xx.zyh mzzyh,
                            xx.blh ,
                            jfb.kflb,
                            jfb.ssrq zlrq,
                            staff.Name zls, 
                            jfb.ssry zlsgh, 
                            mx.zll,
                            jfb.sl,
                            CAST(ISNULL(ROUND(jfb.sl * jfb.dj, 2),0) AS DECIMAL(19, 2)) je ,
                            sfxm.zfxz,
                            jfb.CreateTime,
                            jfb.zt
                    FROM dbo.zy_xmjfb jfb
                            LEFT JOIN dbo.zy_jzjhmx mx ON mx.jzjhmxId = jfb.jzjhmxId
                                      AND mx.OrganizeId = @orgId
                            LEFT JOIN dbo.zy_brjbxx xx ON jfb.zyh = xx.zyh
                                                          AND xx.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm ON sfxm.sfxmCode = jfb.sfxm
                                                                            AND sfxm.OrganizeId = @orgId
                            LEFT JOIN NewtouchHIS_Base..V_C_Sys_ItemsDetail items ON items.catecode = 'RehabTreatmentMethod'
                                                                                  AND items.Code = jfb.kflb
                                                                                AND (items.OrganizeId = @orgId  OR items.OrganizeId = '*')
                                            and (items.OrganizeId = @orgId  OR items.OrganizeId = '*')
                            LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = jfb.ssry
                                                                               AND staff.OrganizeId = @orgId
                                                                             
                    WHERE jfb.OrganizeId = @orgId
                            AND jfb.jfbbh=@zxId
                            AND staff.zt = '1'";
                }
                return this.FirstOrDefault<AccoutingPlanDetailVO>(sql,
                    new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@zxId", zxId ?? "")});
            }
            return null;
        }
        #endregion

        #region 记账计划相关业务逻辑
        /// <summary>
        /// 保存住院计费
        /// </summary>
        public void SaveInpatientAccounting(string zyh, IList<InpatientAccountingItemDto> xmList, string orgId, string curUserCode)
        {
            if (xmList == null || xmList.Count == 0)
            {
                throw new FailedException("缺少计费信息");
            }
            if (xmList.Any(p => p.ysList == null || p.ysList.Count == 0))
            {
                throw new FailedException("请给收费选择治疗师");
            }

            var drugBillingEntityList = new List<HospDrugBillingEntity>();
            var itemBillingEntityList = new List<HospItemBillingEntity>();

            //要更新至 已确认
            var updateToConfirmItemOuterIdList = new List<string>();

            var brjbxx = _hospPatientBasicInfoRepo.IQueryable().Where(p => p.zyh == zyh).FirstOrDefault();

            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //同一次提交
                foreach (var item in xmList)
                {
                    //if (!item.ysList.Any(p => p.gh == this.UserIdentity.rygh))
                    //{
                    //    throw new FailedException("只能记账参与治疗的项目");
                    //}

                    string ys = null;
                    string ysmc = null;
                    string ks = null;
                    string ksmc = null;
                    getYsKsInfo(item.ysList, ref ys, ref ysmc, ref ks, ref ksmc);
                    //新增记录
                    if (item.yzlx == "1")
                    {
                        //药品
                        var jfEntity = new HospDrugBillingEntity()
                        {
                            jfbbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_ypjfb"),
                            OrganizeId = orgId,
                            zyh = zyh,
                            tdrq = item.tdrq,
                            yp = item.sfxmCode,
                            dl = item.sfdlCode,
                            ys = ys,
                            ysmc = ysmc,
                            ks = ks,
                            ksmc = ksmc,
                            bq = brjbxx.bq,
                            cw = brjbxx.cw,
                            dj = item.dj ?? 0,
                            sl = item.sl,
                            jfdw = item.dw,
                            kflb = item.kflb,
                        };
                        jfEntity.Create();

                        db.Insert(jfEntity);
                    }
                    else
                    {
                        //收费项目
                        var jfEntity = new HospItemBillingEntity()
                        {
                            jfbbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb"),
                            OrganizeId = orgId,
                            zyh = zyh,
                            tdrq = item.tdrq,
                            sfxm = item.sfxmCode,
                            dl = item.sfdlCode,
                            ys = ys,
                            ysmc = ysmc,
                            ks = ks,
                            ksmc = ksmc,
                            bq = brjbxx.bq,
                            cw = brjbxx.cw,
                            dj = item.dj ?? 0,
                            sl = item.sl,
                            jfdw = item.dw,
                            kflb = item.kflb,
                            duration = item.duration,
                        };

                        jfEntity.Create();

                        db.Insert(jfEntity);

                        //
                        if (!string.IsNullOrWhiteSpace(item.outerId))
                        {
                            var syncItem = db.IQueryable<SyncTreatmentServiceRecordEntity>(p => p.zt == "1" && item.outerId == p.outerId && p.siteId == orgId).FirstOrDefault();
                            if (syncItem != null)
                            {
                                //更新 确认标志
                                if (string.IsNullOrEmpty(syncItem.clr))
                                {
                                    syncItem.cxclbz = false;
                                    syncItem.clr = curUserCode;
                                    syncItem.clsj = DateTime.Now;
                                }
                                else
                                {
                                    syncItem.cxclbz = true;
                                    syncItem.zhclr = curUserCode;
                                    syncItem.zhclsj = DateTime.Now;
                                }
                                syncItem.clzt = 2;
                                syncItem.jfbId = jfEntity.jfbbh;
                                syncItem.Modify();
                                db.Update(syncItem);
                            }
                        }
                    }
                }
                db.Commit();
            }

            foreach (var date in xmList.Select(p => p.tdrq.Date).Distinct())
            {
                //更新病人医保使用次数
                this.ExecuteSqlCommand("exec sp_ybba_gxsycs_zyhStr @orgId, @zyhStr, @date", new[] {
                        new SqlParameter("@orgId", orgId)
                        ,new SqlParameter("@zyhStr", zyh)
                        ,new SqlParameter("@date", date)
                    });
            }

            return;
        }

        /// <summary>
        /// 撤销执行入库
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="zxbh"></param>
        /// <param name="orgId"></param>
        /// <param name="from"></param>
        public void RevokeExec(string jzjhmxId, string zxbh, string orgId, string from)
        {
            if (!string.IsNullOrWhiteSpace(from))
            {

                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (from.ToLower() == "mz")
                    {
                        var jzjhmx = db.IQueryable<OutpatientAccountDetailEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
                        if (jzjhmx != null)
                        {
                            StopOutpatientLongPlan(db, jzjhmxId, zxbh, orgId);
                        }
                    }
                    else if (from.ToLower() == "zy")
                    {
                        int bh = Convert.ToInt32(zxbh);
                        var jzjhmx = db.IQueryable<HospAccountingPlanDetailEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
                        if (jzjhmx != null)
                        {
                            var xmitem = db.IQueryable<HospItemBillingEntity>(p => p.jfbbh == bh && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
                            var zyInfo = _hospPatientBasicInfoRepo.FindEntity(p => p.zyh == xmitem.zyh && p.OrganizeId == orgId && p.zt == "1");
                            var jsrq = GetLastValidMidwaySettTime(jzjhmx.zyh, orgId);
                            if (zyInfo.zybz == ((int)EnumZYBZ.Ycy).ToString())
                            {
                                throw new FailedException("该病人已出院，无法撤销执行，请确认！");
                            }
                            if (jsrq != null && jsrq > xmitem.CreateTime)
                            {
                                throw new FailedException("该费用已结算，无法撤销执行，请确认！");
                            }

                            StopInpatientLongPlan(db, bh, orgId);
                        }

                    }
                    db.Commit();
                }
            }
        }

        /// <summary>
        /// 更改执行的治疗师和执行时间
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="zxbh"></param>
        /// <param name="orgId"></param>
        /// <param name="from"></param>
        public void UpdateExec(cxzxGirdDto zxItem, string from, string orgId)
        {
            if (!string.IsNullOrWhiteSpace(from))
            {

                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (from.ToLower() == "mz")
                    {
                        UpdateOutpatientTempPlan(db, zxItem, orgId);
                    }
                    else
                    {
                        UpdateInpatientTempPlan(db, zxItem, orgId);
                    }
                    db.Commit();
                }
            }
        }


        /// <summary>
        /// 更改门诊执行的治疗师和执行时间
        /// </summary>
        /// <param name="db"></param>
        /// <param name="zxItem"></param>
        /// <param name="orgId"></param>
        private void UpdateOutpatientTempPlan(Infrastructure.EF.EFDbTransaction db, cxzxGirdDto zxItem, string orgId)
        {
            var item = db.IQueryable<OutpatientItemExeEntity>(p => p.Id == zxItem.zxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            item.zlsgh = zxItem.zlsgh;
            item.zxsj = zxItem.zxsj;
            item.kflb = zxItem.kflb;
            item.Modify(item.Id);
            db.Update(item);
        }

        /// <summary>
        /// 更改住院执行的治疗师和执行时间
        /// </summary>
        /// <param name="db"></param>
        /// <param name="zxItem"></param>
        /// <param name="orgId"></param>
        private void UpdateInpatientTempPlan(Infrastructure.EF.EFDbTransaction db, cxzxGirdDto zxItem, string orgId)
        {
            int jfbbh = int.Parse(zxItem.zxId);
            var item = db.IQueryable<HospItemBillingEntity>(p => p.jfbbh == jfbbh && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            item.ssry = zxItem.zlsgh;
            item.ssrq = zxItem.zxsj;
            item.kflb = zxItem.kflb;
            item.Modify(item.jfbbh);
            db.Update(item);
        }

        /// <summary>
        /// 停止门诊临时记账计划
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        private void StopOutpatientTempPlan(Infrastructure.EF.EFDbTransaction db, string jzjhmxId, string orgId)
        {
            var xmitem = db.IQueryable<OutpatientItemEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            xmitem.zt = "0";
            xmitem.Modify();
            db.Update(xmitem, new List<string> { "zt" });
            //2.结算明细
            var jsmxitem = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.mxnm == xmitem.xmnm && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            jsmxitem.zt = "0";
            db.Update(jsmxitem, new List<string> { "zt" });
            //2.结算明细
            var js = db.IQueryable<OutpatientSettlementEntity>(p => p.jsnm == jsmxitem.jsnm && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            js.zt = "0";
            db.Update(js, new List<string> { "zt" });
            //3.更新mz_jzjhmx
            var jzjhmx = db.IQueryable<OutpatientAccountDetailEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            jzjhmx.zxzt = (int)EnumJzjhZXZT.Stopped;
            jzjhmx.yzxcs = 0;
            db.Update(jzjhmx, new List<string> { "zxzt" });
        }

        /// <summary>
        /// 停止门诊长期记账计划
        /// </summary>
        /// <param name="zxbh"></param>
        /// <param name="orgId"></param>
        private void StopOutpatientLongPlan(Infrastructure.EF.EFDbTransaction db, string jzjhmxId, string zxbh, string orgId)
        {
            //1.执行表
            var zxitem = db.IQueryable<OutpatientItemExeEntity>(p => p.Id == zxbh && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            if (zxitem == null)
            {
                throw new FailedException("门诊执行记录不存在，数据有误，请核实！");
            }
            zxitem.zt = "0";
            zxitem.Modify();
            db.Update(zxitem, new List<string> { "zt" });
            //2.更新mz_jzjhmx
            var jzjhmx = db.IQueryable<OutpatientAccountDetailEntity>(p => p.jzjhmxId == zxitem.jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            var oldzxzt = jzjhmx.zxzt;
            if (jzjhmx.yzxcs > 0)
            {
                jzjhmx.yzxcs = jzjhmx.yzxcs - 1;
                //180801已停止的也能撤销执行明细    //暂不可能已完成
                //181015已完成的也能撤销执行明细
                if (!(jzjhmx.zxzt == (int)EnumJzjhZXZT.Stopped))
                {
                    if (jzjhmx.yzxcs == 0)
                    {
                        jzjhmx.zxzt = (int)EnumJzjhZXZT.None;
                    }
                    else if (jzjhmx.yzxcs > 0)
                    {
                        jzjhmx.zxzt = (int)EnumJzjhZXZT.Part;
                    }
                }
            }
            db.Update(jzjhmx, new List<string> { "yzxcs", "zxzt" });
            //3更新门诊项目的执行ssbz
            if (oldzxzt == (int)EnumJzjhZXZT.Finished)
            {
                var mzxmEntity = db.IQueryable<OutpatientItemEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
                if (mzxmEntity != null)
                {
                    mzxmEntity.ssbz = "9";  //实施过程中 这样 不可退费
                    mzxmEntity.Modify();
                    db.Update(mzxmEntity, new List<string> { "ssbz" });
                }
            }
        }

        /// <summary>
        /// 撤销执行
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        private void StopInpatientLongPlan(Infrastructure.EF.EFDbTransaction db, int zxbh, string orgId)
        {
            //1.计费表
            var xmitem = db.IQueryable<HospItemBillingEntity>(p => p.jfbbh == zxbh && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            xmitem.zt = "0";
            xmitem.Modify();
            db.Update(xmitem, new List<string> { "zt" });
            //2.更新zy_jzjhmx
            var jzjhmx = db.IQueryable<HospAccountingPlanDetailEntity>(p => p.jzjhmxId == xmitem.jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            if (jzjhmx.yzxcs > 0)
            {
                jzjhmx.yzxcs = jzjhmx.yzxcs - 1;
                //180801已停止的也能撤销执行明细    //暂不可能已完成
                //181015已完成的也能撤销执行明细
                if (!(jzjhmx.zxzt == (int)EnumJzjhZXZT.Stopped))
                {
                    if (jzjhmx.yzxcs == 0)
                    {
                        jzjhmx.zxzt = (int)EnumJzjhZXZT.None;
                    }
                    else if (jzjhmx.yzxcs > 0)
                    {
                        jzjhmx.zxzt = (int)EnumJzjhZXZT.Part;
                    }
                }
            }
            db.Update(jzjhmx, new List<string> { "yzxcs", "zxzt" });
        }

        /// <summary>
        /// 停止住院临时记账计划
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        private void StopInpatientTempPlan(Infrastructure.EF.EFDbTransaction db, string jzjhmxId, string orgId)
        {
            //var jsnm=0;
            var xmitem = db.IQueryable<HospItemBillingEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            xmitem.zt = "0";
            xmitem.Modify();
            db.Update(xmitem, new List<string> { "zt" });
            ////2.结算明细
            //var jsmxitem = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.mxnm == xmitem.jfbbh && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            //jsmxitem.zt = "0";
            //jsnm = jsmxitem.jsnm;
            //db.Update(jsmxitem, new List<string> { "zt" });
            ////3.结算表
            //var js = db.IQueryable<OutpatientSettlementEntity>(p => p.jsnm == jsnm && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            //js.zt = "0";
            //db.Update(js, new List<string> { "zt" });
            //4.更新zy_jzjhmx
            var jzjhmx = db.IQueryable<HospAccountingPlanDetailEntity>(p => p.jzjhmxId == jzjhmxId && p.zt == "1" && p.OrganizeId == orgId).FirstOrDefault();
            jzjhmx.zxzt = (int)EnumJzjhZXZT.Stopped;
            jzjhmx.yzxcs = 0;
            db.Update(jzjhmx, new List<string> { "zxzt" });
        }
        #endregion

        #region private methods

        /// <summary>
        /// 执行药品记账计划
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mx"></param>
        /// <param name="curOprCode"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="zxrq"></param>
        private bool executeMedicineAccountingPlan(Newtouch.Infrastructure.EF.EFDbTransaction db, HospAccountingPlanDetailEntity mx, string curOprCode, string curOprDeptCode, DateTime zxrq)
        {
            //对应药品
            if (this.FirstOrDefault<int>(@"select 1 from zy_ypjfb(nolock) where jzjhmxId = @jzjhmxId 
                   and tdrq >= @zxrq and tdrq < DATEADD(DAY,1,@zxrq)"   //sql比较日期部分
                , new[] { new SqlParameter("@jzjhmxId", mx.jzjhmxId), new SqlParameter("@zxrq", zxrq.Date) }) > 0)
            {
                return false;   //同一天 重复执行了
            }

            var ypSql = "select * from [NewtouchHIS_Base]..V_C_xt_yp(nolock) where ypCode = @ypCode and OrganizeId = @orgId";
            var yp = db.FirstOrDefault<SysMedicineComplexVEntity>(ypSql, new[] { new SqlParameter("@ypCode", mx.sfxmCode)
                                , new SqlParameter("@orgId", mx.OrganizeId) });
            if (!yp.zycls.HasValue)
            {
                return false;   //这个药品有问题，没有住院拆零数
            }
            var brjbxx = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == mx.zyh).FirstOrDefault();
            //在时间范围内
            //执行该计划 生成新的计费项目
            var jfEntity = new HospDrugBillingEntity()
            {
                jfbbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_ypjfb"),
                OrganizeId = mx.OrganizeId,
                zyh = mx.zyh,
                tdrq = zxrq,
                yp = mx.sfxmCode,
                bzdm = yp.ypbzdm,
                dl = yp.dlCode,
                cls = (short)yp.zycls.Value, //住院拆零数
                ys = mx.ys,
                ysmc = mx.ysmc,
                ks = mx.ks,
                ksmc = mx.ksmc,
                bq = brjbxx.bq,
                cw = brjbxx.cw,
                dj = (decimal)Math.Round(((yp.lsj / yp.bzs) * (yp.zycls.Value)), 4, MidpointRounding.AwayFromZero),
                sl = mx.sl,
                jfdw = yp.zycldw,
                zfbl = yp.zfbl,
                zfxz = yp.zfxz,
                zxks = curOprDeptCode,
                yzxz = mx.yzxz,
                yzzt = "1", //未撤销
                ybbm = yp.ybdm,
                kflb = mx.kflb,
                yzwym = mx.yzId
            };
            jfEntity.Create();
            db.Insert(jfEntity);
            _hospdrugbillingRepo.Updatezy_brxxexpand(mx.OrganizeId, mx.zyh);
            return true;
        }

        /// <summary>
        /// 执行治疗项目记账计划
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jzjhmxEntity"></param>
        /// <param name="curOprCode"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="zxrq"></param>
        private bool executeItemAccountingPlan(Infrastructure.EF.EFDbTransaction db, HospAccountingPlanDetailEntity jzjhmxEntity, string curOprCode, string curOprDeptCode, DateTime zxrq, zxGirdDto zxItem,out string jfbbh)
        {
            var sfxmSql = "select * from [NewtouchHIS_Base]..V_S_xt_sfxm(nolock) where sfxmCode = @sfxmCode and OrganizeId = @orgId";
            var sfxm = this.FirstOrDefault<SysChargeItemVEntity>(sfxmSql, new[] { new SqlParameter("@sfxmCode", jzjhmxEntity.sfxmCode)
                                , new SqlParameter("@orgId", jzjhmxEntity.OrganizeId) });

            var dcsl = 0;
            if (sfxm != null)
            {
                //单次数量
                dcsl = CommmHelper.CalcSfxmSl(jzjhmxEntity.zll, sfxm.dwjls, sfxm.jjcl);
            }
            if (dcsl < 0)
            {
                jfbbh = null;
                return false;   //计费失败
            }
            //病区中 待结账
            var zybzArr = new string[] { ((int)EnumZYBZ.Bqz).ToString(), ((int)EnumZYBZ.Djz).ToString() }.ToList();
            var brjbxx = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == jzjhmxEntity.zyh && zybzArr.Contains(p.zybz)).FirstOrDefault();
            //在时间范围内
            //执行该计划 生成新的计费项目
            var jfEntity = new HospItemBillingEntity()
            {
                jfbbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb"),
                OrganizeId = jzjhmxEntity.OrganizeId,
                zyh = jzjhmxEntity.zyh,
                tdrq = zxrq,
                sfxm = jzjhmxEntity.sfxmCode,
                dl = sfxm.sfdlCode,
                ys = brjbxx.doctor ?? "",
                ysmc = "",
                ks = brjbxx.ks ?? "",
                ksmc = "",
                bq = brjbxx.bq,
                cw = brjbxx.cw,
                dj = sfxm.dj,
                sl = dcsl,
                jfdw = sfxm.dw,
                zfbl = sfxm.zfbl,
                zfxz = sfxm.zfxz,
                ssbz = "1",   //已实施
                ssrq = zxrq,
                zxks = curOprDeptCode,
                yzxz = jzjhmxEntity.yzxz,
                yzzt = "1", //未撤销
                ybbm = sfxm.ybdm,
                jzjhmxId = jzjhmxEntity.jzjhmxId,
                kflb = zxItem.kflb,
                zzll = jzjhmxEntity.zll,
                ssry = zxItem.zlsgh,
                yzwym = jzjhmxEntity.yzId
            };
            jfEntity.Create();
            db.Insert(jfEntity);
            jfbbh = jfEntity.jfbbh.ToString();
            return true;
        }

        /// <summary>
        /// 执行治疗门诊项目记账计划
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jzjhmxEntity"></param>
        /// <param name="curOprCode"></param>
        /// <param name="curOprDeptCode"></param>
        /// <param name="zxrq"></param>
        /// <param name="zxItem"></param>
        /// <returns></returns>
        private bool executeItemOutAccountingPlan(Infrastructure.EF.EFDbTransaction db, OutpatientAccountDetailEntity jzjhmxEntity, string curOprCode, string curOprDeptCode, DateTime zxrq, zxGirdDto zxItem)
        {
            //对应项目
            var xmsql = @"SELECT  *
                        FROM mz_xm(nolock) xm
                        WHERE   xm.OrganizeId = @orgId
                                AND xm.jzjhmxId = @jzjhmxId and zt='1'";
            var mzxm = db.FirstOrDefault<OutpatientItemEntity>(xmsql, new[] { new SqlParameter("@jzjhmxId", jzjhmxEntity.jzjhmxId)
                                , new SqlParameter("@orgId", jzjhmxEntity.OrganizeId) });
            if (mzxm == null)
            {
                return false;   //不存在记账计划项目信息
            }
            var sfxmSql = "select * from [NewtouchHIS_Base]..V_S_xt_sfxm(nolock) where sfxmCode = @sfxmCode and OrganizeId = @orgId";
            var sfxm = db.FirstOrDefault<SysChargeItemVEntity>(sfxmSql, new[] { new SqlParameter("@sfxmCode", jzjhmxEntity.sfxmCode)
                                , new SqlParameter("@orgId", jzjhmxEntity.OrganizeId) });
            //执行该计划 生成新的计费项目
            var jfEntity = new OutpatientItemExeEntity()
            {
                Id = Guid.NewGuid().ToString(),
                kflb = zxItem.kflb,
                zxsj = zxrq,
                OrganizeId = jzjhmxEntity.OrganizeId,
                sl = jzjhmxEntity.sl,
                jzjhmxId = jzjhmxEntity.jzjhmxId,
                zll = jzjhmxEntity.zll,
                zlsgh = zxItem.zlsgh
            };
            jfEntity.Create();
            db.Insert(jfEntity);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ysList"></param>
        private void getYsKsInfo(IList<InpatientAccountingPlanItemDoctorDto> ysList
            , ref string ys, ref string ysmc, ref string ks, ref string ksmc)
        {
            ys = ysmc = ks = ksmc = "";
            foreach (var ysItem in ysList)
            {
                if (string.IsNullOrWhiteSpace(ysItem.gh)
                    || string.IsNullOrWhiteSpace(ysItem.Name)
                    || string.IsNullOrWhiteSpace(ysItem.ks)
                    || string.IsNullOrWhiteSpace(ysItem.ksmc))
                {
                    throw new FailedException("治疗师不明确");
                }
                ys += ysItem.gh + ",";
                ysmc += ysItem.Name + ",";
                ks += ysItem.ks + ",";
                ksmc += ysItem.ksmc + ",";
            }
            ys = ys.Trim(',');
            ysmc = ysmc.Trim(',');
            ks = ks.Trim(',');
            ksmc = ksmc.Trim(',');
        }

        #endregion

        #region 住院记账，和门诊记账同个页面版本

        /// <summary>
        /// 更新住院计划 剩余次数，同时停止需要停止的计划
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgId"></param>
        /// <param name="jzjhmxIdList"></param>
        public void updateHospitaljzjh(string orgId, IList<string> jzjhmxIdList)
        {
            if (string.IsNullOrWhiteSpace(orgId) || jzjhmxIdList == null || jzjhmxIdList.Count == 0)
            {
                return;
            }
            //
            IList<string> dstnFieldNameList = new List<string>() {
                    "yzxcs", "zxzt"
            };
            //
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                foreach (var jzjhmxId in jzjhmxIdList)
                {
                    //更新zy_jzjhmx
                    var jzjhmxEntity = db.IQueryable<HospAccountingPlanDetailEntity>(p => p.jzjhmxId == jzjhmxId && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                    string yzxcssql = "";
                    if (jzjhmxEntity.yzlx == "1") //1药品
                    {
                        yzxcssql = @"SELECT  COUNT(*)
                        FROM dbo.zy_ypjfb(nolock)
                        WHERE   jzjhmxId = @jzjhmxId
                                AND zt = '1' AND OrganizeId = @orgId";
                    }
                    else
                    {
                        yzxcssql = @"SELECT  COUNT(*)
                        FROM dbo.zy_xmjfb(nolock)
                        WHERE   jzjhmxId = @jzjhmxId
                                AND zt = '1' AND OrganizeId = @orgId";
                    }
                    int yzxcs = db.FirstOrDefault<int>(yzxcssql, new[] {
                    new SqlParameter("@jzjhmxId", jzjhmxId)
                    , new SqlParameter("@orgId", orgId) });
                    jzjhmxEntity.yzxcs = yzxcs;
                    if (jzjhmxEntity.yzxz == "1")
                    {
                        if (jzjhmxEntity.zcs == yzxcs)
                        {
                            jzjhmxEntity.zxzt = (int)EnumJzjhZXZT.Finished;
                        }
                        else if (yzxcs > 0)
                        {
                            jzjhmxEntity.zxzt = (int)EnumJzjhZXZT.Part;
                        }
                        else
                        {
                            jzjhmxEntity.zxzt = (int)EnumJzjhZXZT.None;
                        }
                    }
                    else
                    {
                        if (yzxcs > 0)
                        {
                            jzjhmxEntity.zxzt = (int)EnumJzjhZXZT.Part;
                        }
                        else
                        {
                            jzjhmxEntity.zxzt = (int)EnumJzjhZXZT.None;
                        }
                        //长期必须手动停止  
                        //这里暂不做每天执行最大次数的判断
                    }
                    jzjhmxEntity.Modify();
                    db.Update(jzjhmxEntity, dstnFieldNameList);
                }
                db.Commit();
            }
        }

        /// <summary>
        /// 更新门诊剩余记账次数，判断是否需要停止计划
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public bool StopOutpatientjzjh(string jzjhmxId, string orgId)
        {
            bool Istop = false;
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //更新zy_jzjhmx
                var jzjhmx = db.IQueryable<OutpatientAccountDetailEntity>().FirstOrDefault(p => p.jzjhmxId == jzjhmxId && p.OrganizeId == orgId && p.zt == "1");
                string yzxcssql = @"SELECT  COUNT(*)
                                        FROM [dbo].[mz_jzjhmxzx]
                        WHERE   jzjhmxId = @jzjhmxId
                                AND zt = 1
                                AND OrganizeId = @orgId";
                var yzxcs = FirstOrDefault<int>(yzxcssql, new[] { new SqlParameter("@jzjhmxId", jzjhmxId)
                                , new SqlParameter("@orgId", orgId) });
                //_outpatientAccountDetailRepo.DetacheEntity(oldjzjhmx);
                //var jzjhmx = oldjzjhmx.Clone();
                jzjhmx.yzxcs = yzxcs;
                IList<string> dstnFieldNameList = new List<string>();
                dstnFieldNameList.Add("yzxcs");
                if (jzjhmx.zcs == yzxcs)
                {
                    Istop = true;
                }
                else
                {
                    jzjhmx.zxzt = (int)EnumJzjhZXZT.Part;
                    dstnFieldNameList.Add("zxzt");
                }
                jzjhmx.Modify();
                db.Update(jzjhmx, dstnFieldNameList, attach: false);
                db.Commit();
            }
            return Istop;
        }

        /// <summary>
        /// 已执行的康复项目同步到秦皇岛中间库
        /// </summary>
        /// <param name="jfbStr"></param>
        /// <param name="orgId"></param>
        public void SyncInpatientToInterfaceBasic(Dictionary<string, string> jfbStr,string orgId, DateTime? zxrq,string rygh)
        {
            if (jfbStr!=null&&jfbStr.Count()>0)
            {
                foreach (var item in jfbStr)
                {
                    this.ExecuteSqlCommand("exec skd_SyncInpatientToInterfaceBasic @zyh,@orgId, @jfbStr,@zxrq", new[] {
                        new SqlParameter("@zyh", item.Key)
                        ,new SqlParameter("@orgId", orgId)
                        ,new SqlParameter("@zxrq", zxrq)
                        ,new SqlParameter("@jfbStr", item.Value)
                    });
                }
            }
        }
        #endregion

        #region 费用结算 出院结算

        /// <summary>
        /// 获取最后一次结算 结束结算日期
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public DateTime? GetLastValidMidwaySettTime(string zyh, string orgId)
        {
            var sql = @"select max(jsjsrq) jsjsrq from zy_js(nolock)
where OrganizeId = @orgId and zyh = @zyh and zt = '1'
--jszt 1 已结
and jszt = '1'
and jsnm not in(
	select cxjsnm from zy_js(nolock)
	where OrganizeId = @orgId and zyh = @zyh and zt = '1'
	--jszt 2 已退
	and jszt = '2'
)";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@zyh", zyh));
            pars.Add(new SqlParameter("@orgId", orgId));
            return this.FirstOrDefault<DateTime?>(sql, pars.ToArray());
        }

        /// <summary>
        /// 根据住院号 获取时间段内的 项目计费明细（考虑退费的）
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<HosFeeItemInfoDto> GetFeeItemPaginationList(Pagination pagination, string zyh, string orgId, DateTime startTime, DateTime endTime)
        {
            var sql = @"
select xmjf.zyh
,xmjf.jfbbh xmjfbbh
,xmjf.dj, hbxmjf.sl
,sfxm.sfxmmc sfxmmc
,staff.Name ssrymc
,xmjf.ssrq
,xmjf.CreateTime

from
(
--合并项目减费（退费） 数量 s
select grpId jfbbh, sum(sl) sl
from
(
select case when isnull(cxzyjfbbh, 0) <> 0 then cxzyjfbbh else jfbbh end grpId, sl
from zy_xmjfb(nolock)
where OrganizeId = @orgId and zyh = @zyh and zt = '1'
) as hbxmjf
group by grpId
having sum(sl) > 0
--合并项目减费（退费） 数量 e
) hbxmjf
--
left join zy_xmjfb(nolock) xmjf
on xmjf.jfbbh = hbxmjf.jfbbh

left join [NewtouchHIS_Base]..V_S_xt_sfxm sfxm
on sfxm.sfxmCode = xmjf.sfxm and sfxm.OrganizeId = xmjf.OrganizeId
left join [NewtouchHIS_Base]..V_S_Sys_Staff staff
on staff.gh = xmjf.ssry and staff.OrganizeId = xmjf.OrganizeId
where xmjf.zyh = @zyh and xmjf.OrganizeId = @orgId and xmjf.zt = '1'
and xmjf.CreateTime > @startTime
and xmjf.CreateTime <= @endTime";
            var pars = new List<SqlParameter>() { };
            pars.Add(new SqlParameter("@zyh", zyh ?? ""));
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@startTime", startTime));
            pars.Add(new SqlParameter("@endTime", endTime));
            return this.QueryWithPage<HosFeeItemInfoDto>(sql, pagination, pars.ToArray());
        }

        /// <summary>
        /// 根据住院号 获取时间段内的 项目药品计费明细（考虑退费的）
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<HosFeeItemInfoDto> GetChargeItemPaginationList(Pagination pagination, string zyh, string orgId, DateTime startTime, DateTime endTime)
        {
            var sql = @"select aaa.*,staff.Name ssrymc from (select zyh,jfbbh xmjfbbh,xmjf.dj,sl,je,sfxm.sfxmmc sfxmmc,ssry,ssrq,xmjf.CreateTime,tdrq from [V_C_Sys_HbtfZyXmjfb] xmjf
left join [NewtouchHIS_Base]..V_S_xt_sfxm sfxm
on sfxm.sfxmCode = xmjf.sfxm and sfxm.OrganizeId = xmjf.OrganizeId
 where xmjf.OrganizeId = @orgId and xmjf.zyh = @zyh and xmjf.zt = '1'
union all
select zyh,jfbbh xmjfbbh,dj,sl,je,yp.ypmc sfxmmc,ys ssry,tdrq ssrq,ypjf.CreateTime,tdrq from [V_C_Sys_HbtfZyYpjfb] ypjf
left join [NewtouchHIS_Base]..V_S_xt_yp yp
on yp.[ypCode]=ypjf.yp and yp.OrganizeId = ypjf.OrganizeId
where ypjf.OrganizeId = @orgId and ypjf.zyh = @zyh and ypjf.zt = '1') as aaa
left join [NewtouchHIS_Base]..V_S_Sys_Staff staff
on staff.gh = aaa.ssry and staff.OrganizeId = @orgId
where  aaa.tdrq > @startTime
and aaa.tdrq <= @endTime";
            var pars = new List<SqlParameter>() { };
            pars.Add(new SqlParameter("@zyh", zyh ?? ""));
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@startTime", startTime));
            pars.Add(new SqlParameter("@endTime", endTime));
            return this.QueryWithPage<HosFeeItemInfoDto>(sql, pagination, pars.ToArray());
        }


        /// <summary>
        /// 中途结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="startTime">调用之前请明确开始日期</param>
        /// <param name="endTime"></param>
        /// <param name="expectedCount"></param>
        public void MidwaySettlement(string zyh, string orgId, DateTime startTime, DateTime endTime, int? expectedCount = null)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var zybrInfo = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();

                //计费项目
                var xmjfEntityList = _hospItemBillingRepo.GetItemFeeEntityListByTime(zyh, orgId, startTime, endTime, db.IQueryable<HospItemBillingEntity>());

                if (expectedCount.HasValue && expectedCount.Value != xmjfEntityList.Count)
                {
                    throw new FailedException("过程中计费项目发生变更");
                }
                if (xmjfEntityList.Count == 0)
                {
                    throw new FailedException("无未结费用，操作失败");
                }

                var newjsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_js");

                var newJsmxList = new List<HospSettlementDetailEntity>();
                foreach (var item in xmjfEntityList)
                {
                    var jsmxItem = new HospSettlementDetailEntity()
                    {
                        jsmxbh = newjsnm,
                        OrganizeId = orgId,
                        jsnm = newjsnm,
                        xmjfbbh = item.jfbbh,
                        yzlx = "2",
                        jyje = item.dj * item.sl,
                    };
                    jsmxItem.Create();
                    db.Insert(jsmxItem);
                    newJsmxList.Add(jsmxItem);
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
                jsItem.xjzf = jsItem.zje;
                jsItem.ysk = jsItem.zje;
                jsItem.xjzffs = "0";
                jsItem.Create();
                db.Insert(jsItem);

                db.Commit();
            }
        }

        /// <summary>
        /// 撤销最后一次中途结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        public void CancelTheLastMidwaySett(string zyh, string zffs1, string orgId,out decimal refoundmoney)
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
                //撤销结算内码
                var csjsnm = ztjsList[0].jsnm;
                //结算明细
                var cxjsmnList = db.IQueryable<HospSettlementDetailEntity>(p => p.jsnm == csjsnm && p.zt == "1").ToList();

                var newjsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_js");
                //3.中途结算
                var jsItem = new HospSettlementEntity()
                {
                    jsnm = newjsnm,
                    OrganizeId = orgId,
                    zyh = zyh,
                    brxz = ztjsList[0].brxz,
                    zyts = ztjsList[0].zyts,
                    zje = ztjsList[0].zje,
                    jszt = "2", //1已结 2已退
                    jsxz = "2", //1出院结算 2中途
                    jsksrq = ztjsList[0].jsksrq,
                    jsjsrq = ztjsList[0].jsjsrq,
                    cxjsnm = csjsnm,
                    cxjsyy = null,  //?
                };
                jsItem.zlfy = ztjsList[0].zlfy;
                jsItem.xjzf = ztjsList[0].xjzf;
                jsItem.ysk = ztjsList[0].ysk;
                jsItem.xjzffs = zffs1;
                jsItem.Create();
                db.Insert(jsItem);
                //结算明细
                foreach (var mx in cxjsmnList)
                {
                    var jsmxItem = new HospSettlementDetailEntity()
                    {
                        jsmxbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jsmx"),
                        OrganizeId = orgId,
                        jsnm = newjsnm,
                        xmjfbbh = mx.xmjfbbh,
                        ypjfbbh=mx.ypjfbbh,
                        yzlx = mx.yzlx,
                        jyje = mx.jyje,
                    };
                    jsmxItem.Create();
                    db.Insert(jsmxItem);
                }

                db.Commit();
                
                refoundmoney = jsItem.xjzf;
            }
        }

        /// <summary>
        /// 出院结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="cyrq"></param>
        public void DischargeSettlement(string zyh, string orgId, DateTime cyrq)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //1.先查出所有撤销的记录
                var cxztjsList = db.IQueryable<HospSettlementEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.jszt == "2").Select(p => p.cxjsnm);
                //已产生的 结算列表 jszt 1已结 且 为被撤销
                //已排序 最新的有效结算 排在最前
                var ztjsList = db.IQueryable<HospSettlementEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.jszt == "1" && !cxztjsList.Contains(p.jsnm)).OrderByDescending(p => p.CreateTime).ToList();
                //jsxz 1 出院结算
                if (ztjsList.Count > 0 && ztjsList[0].jsxz == "1")
                {
                    throw new FailedException("重复出院");  //应该肯定是中途结算
                }
                //住院病人信息
                var zybrInfo = db.IQueryable<HospPatientBasicInfoEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                //2.未结明细 计费项目
                var stTime = ztjsList.Select(t => t.jsjsrq).Max() ?? zybrInfo.ryrq;
                var cyrqTime = (cyrq.Date == DateTime.Now.Date ? DateTime.Now : cyrq.AddDays(1).AddMilliseconds(-1));

                var xmjfEntityList = _hospItemBillingRepo.GetItemFeeEntityListByTime(zyh, orgId, stTime, cyrqTime, db.IQueryable<HospItemBillingEntity>());

                var newjsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_js");

                var newJsmxList = new List<HospSettlementDetailEntity>();
                foreach (var item in xmjfEntityList)
                {
                    var jsmxItem = new HospSettlementDetailEntity()
                    {
                        jsmxbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_jsmx"),
                        OrganizeId = orgId,
                        jsnm = newjsnm,
                        xmjfbbh = item.jfbbh,
                        yzlx = "2",
                        jyje = item.dj * item.sl,
                    };
                    jsmxItem.Create();
                    db.Insert(jsmxItem);
                    newJsmxList.Add(jsmxItem);
                }

                //3.出院结算
                var jsItem = new HospSettlementEntity()
                {
                    jsnm = newjsnm,
                    OrganizeId = orgId,
                    zyh = zyh,
                    brxz = zybrInfo.brxz,
                    zyts = DateTimeHelper.GetInHospDays(zybrInfo.ryrq, cyrqTime),
                    zje = ztjsList.Select(p => p.zje).Sum() + newJsmxList.Select(p => p.jyje.Value).Sum(),
                    jszt = "1", //已结
                    jsxz = "1", //1出院结算
                    jsksrq = stTime,    //第一次结算的日期（默认会入院日期）
                    jsjsrq = cyrqTime,
                };
                jsItem.zlfy = jsItem.zje;
                jsItem.xjzf = jsItem.zje;
                jsItem.ysk = jsItem.zje;
                jsItem.xjzffs = "0";
                jsItem.Create();
                db.Insert(jsItem);

                zybrInfo.zybz = ((int)EnumZYBZ.Ycy).ToString();
                zybrInfo.cyrq = cyrq;
                zybrInfo.Modify();
                db.Update(zybrInfo);

                db.Commit();
            }
        }
        public DateTime? GetLastValidSettTime(string zyh, string orgId)
        {
            var sql = @"select max(jsjsrq) jsjsrq from zy_js(nolock)
where OrganizeId = @orgId and zyh = @zyh and zt = '1'
--jszt 1 已结
and jszt = '1'
and jsnm not in(
	select cxjsnm from zy_js(nolock)
	where OrganizeId = @orgId and zyh = @zyh and zt = '1'
	--jszt 2 已退
	and jszt = '2'
)";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@zyh", zyh));
            pars.Add(new SqlParameter("@orgId", orgId));
            return this.FirstOrDefault<DateTime?>(sql, pars.ToArray());
        }
        #endregion

        #region 住院记账业务
        /// <summary>
        /// 根据项目编号获取自负性质
        /// </summary>
        /// <param name="sfxmCode"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public string getzfxzById(string sfxmCode,string orgId)
        {
            string sql = "select zfxz from [NewtouchHIS_Base].dbo.xt_sfxm where zt=1 and organizeId=@orgId and sfxmCode=@sfxmCode ";
            return this.FirstOrDefault<string>(sql, new[] { new SqlParameter("@sfxmCode", sfxmCode)
                                , new SqlParameter("@orgId", orgId) });
           
        }
        /// <summary>
        /// 物资耗材库存验证
        /// </summary>
        /// <param name="jfbhs"></param>
        /// <returns></returns>
        public List<HospItemFeeVO> Verification(string xmCodes,string orgId,string ks)
        {
            const string sql = @" select ks,productCode,SUM(kcsl-djsl) kykc from Newtouch_CIS..Dept_kcxx with(nolock)
where ks=@ks  and  OrganizeId=@orgId and zt='1' 
and productCode in (select col from f_split(@sfxmCode,','))
group by ks,productCode";
            return this.FindList<HospItemFeeVO>(sql, new[] { new SqlParameter("@sfxmCode", xmCodes)
                                , new SqlParameter("@orgId", orgId), new SqlParameter("@ks", ks) });

        }

        /// <summary>
        /// 住院记账-项目、物资耗材、药品计费
        /// </summary>
        /// <param name="xmjfbEntitylist"></param>
        /// <param name="ypjfbEntitylist"></param>
        /// <returns></returns>
        public string SaveFee(List<HospItemBillingEntity> xmjfbEntitylist, List<HospDrugBillingEntity> ypjfbEntitylist)
        {
            string result = "";
            try {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (xmjfbEntitylist != null && xmjfbEntitylist.Count > 0)
                    {
                        foreach (var item in xmjfbEntitylist)
                        {
                            db.Insert(item);
                        }
                    }
                    if (ypjfbEntitylist != null && ypjfbEntitylist.Count > 0)
                    {
                        foreach (var item in ypjfbEntitylist)
                        {
                            db.Insert(item);
                        }
                    }
                    db.Commit();
                }
            }
            catch (Exception e)
            {
                result ="记账失败："+ e.Message;
                throw;
            }
            return result;
        }
        #endregion
    }
}
