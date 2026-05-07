using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.DTO.InputDto.Inpatient;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;
using System.Text;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Domain.DTO.OutputDto;

namespace Newtouch.DomainServices.Inpatient
{
    public class InpatientPatientDmnService : DmnServiceBase, IInpatientPatientDmnService
    {
        public InpatientPatientDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        { }

        /// <summary>
        /// 内部类定义(一次性使用)
        /// </summary>
        public class UncheckedAdviceCount
        {
            public string zyh { get; set; }
            public int cnt { get; set; }
        }
        public class GmxxPositive
        {
            public string blh { get; set; }
        }

        /// <summary>
        /// 获取在病区的病人对象集合
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public List<PatientzbqResponseDto> GetzbqPatientList(PatientzbqRequestDto req, string OrganizeId)
        {
            string sqlText = "SELECT " +
                            "z.id,cw.cwCode,cw.cwmc,z.xm,z.sex,z.zyh,z.deptCode,z.ryrq,DATEDIFF(day,z.ryrq,GETDATE()) AS zyts,z.hljb,z.wzjb,ISNULL(CAST(z.sfqj AS VARCHAR(50)),'') AS brzt," +
                            "z.brxzmc,z.zdmc,z.brxzdm,z.blh,s.[Name] AS ysmc," +
                            "CAST(b.nl AS int) AS age,b.nlshow,NULL AS kssj,'' AS ztnr,'none' AS display,0 AS cnt,'0' AS ps " +
                            "FROM zy_brxxk z with(nolock) " +
                            "INNER JOIN[NewtouchHIS_Base].[dbo].[xt_cw] cw with(nolock) ON z.OrganizeId=cw.OrganizeId AND z.WardCode= cw.bqCode AND z.BedCode= cw.cwCode " +
                            "INNER JOIN [NewtouchHIS_Base].[dbo].[Sys_Staff] s with(nolock) ON z.OrganizeId= s.OrganizeId AND z.ysgh= s.gh " +
                            "INNER JOIN [NewtouchHIS_Sett].[dbo].[zy_brjbxx] b with(nolock) ON z.OrganizeId=b.OrganizeId AND z.zyh=b.zyh " +
                            "WHERE " +
                            "z.OrganizeId=@OrganizeId AND z.WardCode=@bqCode AND z.zybz=1 and z.zt='1' " +
                            "AND (@cw='' OR (@cw<>'' AND cw.cwmc LIKE CONCAT(@cw,'%'))) " +
                            "AND(@ysgh='' OR (@ysgh<>'' AND z.ysgh=@ysgh))";
            var dbParams = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", OrganizeId),
                new SqlParameter("@bqCode", req.bqcode),
                new SqlParameter("@cw", req.cw ?? ""),
                new SqlParameter("@ysgh", req.ysgh ?? "")
            };
            var result = this.QueryWithPage<PatientzbqResponseDto>(sqlText, req.pagination, dbParams).ToList();
            if (result.Count > 0)
            {
                List<String> zyhlist = new List<string>();
                result.ForEach(m => zyhlist.Add("'" + m.zyh + "'"));
                string zyhs = string.Join(",", zyhlist);

                List<UncheckedAdviceCount> ucac = GetUncheckedAdviceCount(OrganizeId, zyhs);
                result.ForEach(r => {
                    var ac = ucac.FirstOrDefault(p => p.zyh == r.zyh);
                    if (ac != null)
                    {
                        r.cnt = ac.cnt;
                    }
                });
                ucac.Clear();

                List<GmxxPositive> gmpv = GetGmxxPositive(OrganizeId, zyhs);
                gmpv.ForEach(gm => {
                    var r = result.FirstOrDefault(p => p.blh == gm.blh);
                    if (r != null)
                    {
                        r.ps = "1";
                    }
                });
                gmpv.Clear();
            }
            return result;
        }
        private List<UncheckedAdviceCount> GetUncheckedAdviceCount(String OrganizeId, String zyhs)
        {
            string sqlText = "WITH " +
                            "cte AS " +
                            "(" +
                                "SELECT zyh,COUNT(1) AS cnt FROM zy_cqyz with(nolock) " +
                                "WHERE OrganizeId=@OrganizeId AND yzzt=0 AND zyh IN (" + zyhs + ") AND zt='1' GROUP BY zyh " +
                                "UNION ALL " +
                                "SELECT zyh, COUNT(1) AS cnt FROM zy_lsyz with(nolock) " +
                                "WHERE OrganizeId=@OrganizeId AND yzzt=0 AND zyh IN (" + zyhs + ") AND zt='1' GROUP BY zyh" +
                            ") " +
                            "SELECT cte.zyh,SUM(cte.cnt) AS cnt FROM cte GROUP BY cte.zyh";
            var dbParams = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", OrganizeId)
            };
            return this.FindList<UncheckedAdviceCount>(sqlText, dbParams);
        }
        private List<GmxxPositive> GetGmxxPositive(String OrganizeId, String zyhs)
        {
            string sqlText = "SELECT DISTINCT gm.blh FROM xt_gmxx gm with(nolock) " +
                            "WHERE " +
                            "gm.OrganizeId=@OrganizeId " +
                            "AND gm.blh IN (SELECT zy.blh FROM zy_brxxk zy with(nolock) WHERE zy.OrganizeId=@OrganizeId AND zy.zyh IN (" + zyhs + ")) " +
                            "AND gm.Result='1' AND gm.zt='1'";
            var dbParams = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", OrganizeId)
            };
            return this.FindList<GmxxPositive>(sqlText, dbParams);
        }
        public List<PatientzbqResponseDto> GetzbqPatientList_old(PatientzbqRequestDto req, string OrganizeId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"SELECT  zyxx.id ,
        cw.cwmc ,
        zyxx.xm ,
        zyxx.sex ,
       Convert(int,bzyxx.nl)  as age ,
       bzyxx.nlshow,
        zyxx.zyh ,
        zyxx.ryrq ,
        datediff(day,zyxx.ryrq,getdate()) zyts,
        zyxx.hljb ,
        zyxx.wzjb ,
        ISNULL(CAST(zyxx.sfqj AS VARCHAR(50)) , '') brzt,
        ISNULL(staff.Name,'') ysmc,
		zyxx.brxzmc,
		zyxx.zdmc,zyxx.brxzdm,
        --NULL kssj,
        --NULL ztnr,
		'none' display ,
        0 cnt,
		isnull(psxx.result ,0) ps
FROM    dbo.zy_brxxk zyxx
        INNER JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = zyxx.WardCode
                                                     AND bq.OrganizeId = zyxx.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.bqCode = zyxx.WardCode
                                                    AND cw.cwCode = zyxx.BedCode
                                                    AND cw.OrganizeId = zyxx.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = zyxx.ysgh
                                                           AND staff.OrganizeId = zyxx.OrganizeId
        left join NewtouchHIS_Sett..zy_brjbxx(nolock) bzyxx on bzyxx.zyh=zyxx.zyh and bzyxx.blh=zyxx.blh AND bzyxx.OrganizeId = zyxx.OrganizeId
left join NewtouchHIS_Sett..xt_brjbxx(nolock) xx
on bzyxx.patid = xx.patid AND bzyxx.OrganizeId = xx.OrganizeId  AND xx.zt = '1'
left join (
select blh,result from [Newtouch_CIS].dbo.xt_gmxx 
where result =1 and zt=1 and organizeId=@OrganizeId
group by blh,result 
 ) psxx on zyxx.blh=psxx.blh
WHERE   1 = 1");
            if (req != null)
            {
                if (!string.IsNullOrWhiteSpace(req.bqcode))
                {
                    sqlstr.Append("  AND zyxx.WardCode=@wardcode");
                    par.Add(new SqlParameter("@wardcode", req.bqcode));
                    if (!string.IsNullOrWhiteSpace(req.cw))
                    {
                        sqlstr.Append("  AND (cw.cwmc like @cw OR ISNULL(@cw, '') = '')");
                        par.Add(new SqlParameter("@cw", "%" + req.cw + "%"));
                    }
                        sqlstr.Append(@"        AND ( ( zyxx.ysgh = @ysgh
                                        AND staff.zt = '1')
                                        OR ISNULL(@ysgh, '') = '')");
                        par.Add(new SqlParameter("@ysgh", req.ysgh));
                }
            }
            sqlstr.Append(" and zyxx.zt='1' and cw.zt='1'");
            sqlstr.Append(" and zyxx.zybz=@zybz");
            par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Bqz));
            sqlstr.Append(" and zyxx.OrganizeId=@OrganizeId");
            par.Add(new SqlParameter("@OrganizeId", OrganizeId));
            var result= this.QueryWithPage<PatientzbqResponseDto>(sqlstr.ToString(),req.pagination, par.ToArray()).ToList();
            var yzxx = GetPatientYzxxList(OrganizeId);
            result.ForEach(m => {
                var data = yzxx.FirstOrDefault(n=>n.zyh==m.zyh);
                if (data != null) {
                    m.kssj = data.kssj;
                    m.ztnr = data.ztnr;
                    m.display = data.display;
                    m.cnt = data.cnt;
                }
            });
            return result;
        }

        public List<PatientyzxxResponseDto> GetPatientYzxxList(string OrganizeId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"
                select zyh,OrganizeId,kssj,ztnr into #tab  from  
                (
                select  zyh zyh1,  Max(kssj) kssj1   from zy_lsyz (nolock) c
                where yztag='YCY' and yzzt=2 and organizeId=@OrganizeId
                group by zyh
                ) a
                left join zy_lsyz (nolock) b
                on a.zyh1=b.zyh and a.kssj1=b.kssj and b.organizeId=@OrganizeId
                

                select zyh,OrganizeId ,count(1) cnt into #tab1  from (
                  select 'cq' yzxz,zyh,yzzt,yzlx,xmmc,OrganizeId from zy_cqyz (nolock) where OrganizeId=@OrganizeId and yzzt='0' and  zt='1'
                  union all
                  select 'ls' yzxz,zyh,yzzt,yzlx,xmmc,OrganizeId  from zy_lsyz (nolock) where OrganizeId=@OrganizeId and yzzt='0' and  zt='1'
                  ) as yz
                  group by zyh,OrganizeId

                    select isnull(a.zyh,b.zyh) zyh,isnull(a.OrganizeId,b.OrganizeId) OrganizeId ,a.kssj,
                    case  when (a.kssj) is null then 'none' else null end display,a.ztnr,isnull(b.cnt,0) cnt from #tab a full outer join #tab1 b
                    on a.zyh=b.zyh 
            ");
            par.Add(new SqlParameter("@OrganizeId", OrganizeId));
            return this.FindList<PatientyzxxResponseDto>(sqlstr.ToString(), par.ToArray());
        }

        /// <summary>
        /// 获取已出区的病人对象集合
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public List<PatientycqResponseDto> GetycqPatientList(PatientycqRequestDto req, string OrganizeId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"SELECT  zyxx.id ,
        zyxx.xm ,
        zyxx.zyh ,
        zyxx.cardno ,
        cw.cwmc ,
        zyxx.sex ,
        CAST(FLOOR(DATEDIFF(DY, zyxx.birth, GETDATE()) / 365.25) AS INT) age ,
        zyxx.ryrq ,
        zyxx.cqrq ,
        ISNULL(staff.Name,'') ysmc,
		zyxx.zdmc,zyxx.brxzdm,zyxx.brxzmc,
	b.nlshow
FROM    dbo.zy_brxxk zyxx
        INNER JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = zyxx.WardCode
                                                     AND bq.OrganizeId = zyxx.OrganizeId
        LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.bqCode = zyxx.WardCode
                                                    AND cw.cwCode = zyxx.BedCode
                                                    AND cw.OrganizeId = zyxx.OrganizeId
        left join NewtouchHIS_Sett..zy_brjbxx b on zyxx.OrganizeId=b.OrganizeId and zyxx.zyh=b.zyh 
        LEFT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON staff.gh = zyxx.ysgh
                                                           AND staff.OrganizeId = zyxx.OrganizeId
        WHERE   1 = 1");
            if (req != null)
            {
                if (!string.IsNullOrWhiteSpace(req.bqcode))
                {
                    sqlstr.Append("  AND zyxx.WardCode=@wardcode");
                    par.Add(new SqlParameter("@wardcode", req.bqcode));
                    if (!string.IsNullOrWhiteSpace(req.cw))
                    {
                        sqlstr.Append("  AND (cw.cwmc like @cw OR ISNULL(@cw, '') = '')");
                        par.Add(new SqlParameter("@cw", "%" + req.cw + "%"));
                    }
                   
                    if (req.cqksrq.HasValue && req.cqksrq != DateTime.MinValue)
                    {
                        sqlstr.Append(" AND zyxx.cqrq >= @kssj");
                        par.Add(new SqlParameter("@kssj", req.cqksrq.Value));
                    }
                    if (req.cqjsrq.HasValue && req.cqjsrq != DateTime.MinValue)
                    {
                        sqlstr.Append(" AND zyxx.cqrq < @jssj");
                        par.Add(new SqlParameter("@jssj", req.cqjsrq.Value.AddDays(1).Date));
                    }
                    if (!string.IsNullOrWhiteSpace(req.zyh))
                    {
                        sqlstr.Append(" AND zyxx.zyh=@zyh");
                        par.Add(new SqlParameter("@zyh", req.zyh));
                    }
                    sqlstr.Append(@"        AND ( ( zyxx.ysgh = @ysgh
                                        AND staff.zt = '1')
                                        OR ISNULL(@ysgh, '') = '')");
                    par.Add(new SqlParameter("@ysgh", req.ysgh));
                }
            }
            sqlstr.Append(" and zyxx.zt='1' and cw.zt='1'");
            sqlstr.Append(" and zyxx.zybz in (select * from [dbo].[f_split](@zybz, ','))");
            par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Djz + "," + (int)EnumZYBZ.Zq + "," + (int)EnumZYBZ.Ycy));
            sqlstr.Append(" and zyxx.OrganizeId=@OrganizeId");
            par.Add(new SqlParameter("@OrganizeId", OrganizeId));
            return this.QueryWithPage<PatientycqResponseDto>(sqlstr.ToString(),req.pagination, par.ToArray()).ToList();
        }

        /// <summary>
        /// 住院病人筛选
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="bq"></param>
        /// <param name="zyh"></param>
        /// <param name="xm"></param>
        /// <param name="zybz">在院标志，多个用逗号分割</param>
        /// <returns></returns>
        public IList<InPatientPatientSearchVO> GetInPatSearchPaginationList(Pagination pagination, string orgId, string bq, string zyh, string xm,string ch, string bqCode, string zybz = null)
        {
            var sql = @"select blh, zyh,BedCode bedCode, xm, sex, WardCode, bq.bqmc WardName, birth
, ryrq, rqrq
from zy_brxxk(nolock) zyxx
left join [NewtouchHIS_Base]..V_S_xt_bq bq
on bq.bqCode = zyxx.WardCode and bq.OrganizeId = zyxx.OrganizeId
where zyxx.OrganizeId = @orgId";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(bq))
            {
                sql += " and zyxx.WardCode = @bq";
                pars.Add(new SqlParameter("@bq", bq));
            }
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                sql += " and zyxx.zyh like @searchZyh";
                pars.Add(new SqlParameter("@searchZyh", "%" + zyh + "%"));
            }
            if (!string.IsNullOrWhiteSpace(ch))
            {
                sql += " and zyxx.BedCode like @searchCh";
                pars.Add(new SqlParameter("@searchCh", "%" + ch + "%"));
            }
            if (!string.IsNullOrWhiteSpace(xm))
            {
                sql += " and zyxx.xm like @searchXm";
                pars.Add(new SqlParameter("@searchXm", "%" + xm + "%"));
            }
            if (!string.IsNullOrWhiteSpace(zybz))
            {
                sql += " and zyxx.zybz in (select * from [dbo].[f_split](@zybz, ','))";
                pars.Add(new SqlParameter("@zybz", zybz));
			}

            string[] bqArr = bqCode.Split(',');
            string bqstr = "";
            if (!string.IsNullOrWhiteSpace(bqCode))
            {
                for(int i=0;i< bqArr.Length;i++)
                {
                    if (i == 0)
                    {
                        bqstr += "'" + bqArr[i] + "'";

                    }
                    else
                    {
                        bqstr += ",'" + bqArr[i] + "'";
                    }
                }
            }

				sql += " and bqcode in ( "+ bqstr + " )";			

			return this.QueryWithPage<InPatientPatientSearchVO>(sql, pagination, pars.ToArray());
        }

        /// <summary>
        /// 住院号、姓名、卡号获取病人基本信息 姓名，性别，年龄，病人性质、入院日期
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<patientInfoDto> GetInfoBykeyword(string keyword, string orgId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"SELECT  id Id ,
                        a.zyh ,
                        a.xm ,
                        sex ,
                        b.nlshow  as birth,
                        brxzmc ,
                        a.ryrq
                FROM     zy_brxxk  a left join NewtouchHIS_Sett..zy_brjbxx b on a.OrganizeId=b.OrganizeId and a.zyh=b.zyh
                WHERE   a.OrganizeId = @orgId
                        AND a.zt = '1'
                        AND ( a.zyh LIKE @keyword
                              OR a.xm LIKE @keyword
                              OR cardno LIKE @keyword)");
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            return this.FindList<patientInfoDto>(sqlstr.ToString(), par.ToArray());
        }

        /// <summary>
        /// 住院号获取病人基本信息 姓名，性别，年龄，病人性质
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public patientInfoDto GetInfoByZyh(string zyh, string orgId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"SELECT  zyh ,  
                        xx.blh,
                        xm ,
                        sex ,
                        xx.DeptCode ksdm,
                        ks.name as ksmc,
                        xx.zdmc ,
                        xx.ysgh,
                        ry.name as ysxm,
                        CAST(FLOOR(DATEDIFF(DY, birth, GETDATE()) / 365.25) AS INT) age ,
                        brxzmc ,
                        bq.bqmc ,
                        cw.cwmc ,
                         CONVERT(DATETIME, xx.rqrq, 23) ryrq,
                        xx.cqrq,
                        xx.rqrq,
                        xx.zybz
                FROM    zy_brxxk xx
                                LEFT JOIN NewtouchHIS_Base..V_S_xt_bq bq ON bq.bqCode = xx.WardCode
                                                    AND bq.OrganizeId = xx.OrganizeId
                                                    AND bq.zt = '1'
                LEFT JOIN NewtouchHIS_Base..V_S_xt_cw cw ON cw.cwCode = xx.BedCode
                                                    AND cw.OrganizeId = xx.OrganizeId
                                                    AND cw.zt = '1'
                LEFT JOIN NewtouchHIS_Base..Sys_Department ks on xx.DeptCode = ks.Code 
                                    and ks.OrganizeId = xx.OrganizeId
                LEFT JOIN NewtouchHIS_Base..Sys_Staff  ry  on ry.gh = xx.ysgh 
                                                    AND cw.OrganizeId = xx.OrganizeId
                                                    AND cw.zt = '1'
            where xx.OrganizeId=@orgId and xx.zyh=@zyh and xx.zt='1'");
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@zyh", zyh));
            return this.FirstOrDefault<patientInfoDto>(sqlstr.ToString(), par.ToArray());
        }

        /// <summary>
        /// 住院病人筛选
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="bq"></param>
        /// <param name="zyh"></param>
        /// <param name="xm"></param>
        /// <param name="zybz">在院标志，多个用逗号分割</param>
        /// <returns></returns>
        public IList<InPatientNursingInputVO> GetInPatSearchList(string orgId, string bq,DateTime rq,string sj, string zyh, string zybz = null)
        {
            var sql = @"select blh, zyxx.zyh, xm, sex, WardCode, bq.bqmc WardName, birth
                        , ryrq, rqrq,tz.*
                        ,convert(varchar(50),isnull(xysz,''))+'/'+convert(varchar(50),isnull(xyxz,'')) xysxz
                        from zy_brxxk(nolock) zyxx
                        left join [NewtouchHIS_Base]..V_S_xt_bq bq
                        on bq.bqCode = zyxx.WardCode and bq.OrganizeId = zyxx.OrganizeId
                        LEFT JOIN dbo.zy_brsmtz tz ON tz.zyh = zyxx.zyh
                                      AND tz.OrganizeId = zyxx.OrganizeId
                                      AND tz.rq = @rq
                                       AND ( CASE tz.lrflag
                                              WHEN 1 THEN tz.zdysjd
                                              ELSE tz.sj
                                            END ) = @sj
                                        and tz.zt='1'
                        where zyxx.OrganizeId = @orgId
                        ";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@rq", rq));
            pars.Add(new SqlParameter("@sj", sj));
            pars.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(bq))
            {
                sql += " and zyxx.WardCode = @bq";
                pars.Add(new SqlParameter("@bq", bq));
            }
            if (!string.IsNullOrWhiteSpace(zybz))
            {
                sql += " and zyxx.zybz in (select * from [dbo].[f_split](@zybz, ','))";
                pars.Add(new SqlParameter("@zybz", zybz));
            }
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                sql += " and zyxx.zyh in (select * from [dbo].[f_split](@zyh, ','))";
                pars.Add(new SqlParameter("@zyh", zyh));
            }
            return this.FindList<InPatientNursingInputVO>(sql,  pars.ToArray());
        }
        /// <summary>
        /// 获取全部病人树信息
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IList<InpWardPatTreeVO> GetPatTree(string orgId, string zyzt, string keyword)
        {
            string sql = @"select  distinct b.zyh,b.xm hzxm,b.wardCode bqCode,c.bqmc,b.sex,b.zybz,b.ryrq,b.birth,cw.BedNo,CONVERT(VARCHAR(25),CASE DATEDIFF(DAY, b.ryrq,GETDATE()) WHEN 0 THEN 1 else  DATEDIFF(DAY, b.ryrq,GETDATE())END ) inHosDays,
CAST(FLOOR(DATEDIFF(DY, b.birth, GETDATE()) / 365.25) AS VARCHAR(5)) nl,convert(varchar(50),isnull(b.rqrq,getdate()),120) rqrq,convert(varchar(50),isnull(b.cqrq,getdate()),120) cqrq
from zy_brxxk b with(nolock)  
left join [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) on c.bqcode=b.wardCode and c.OrganizeId=b.OrganizeId and c.zt='1'
left join zy_cwsyjlk cw with(nolock) on cw.zyh=b.zyh and cw.organizeid=b.OrganizeId and cw.zt='1'
                where 1=1 and b.OrganizeId=@orgId  and b.zt='1' ";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(zyzt))
            {
                switch (zyzt)
                {
                    case "zy"://在院：不是已出院，不是作废记录，不是取消入院
                        sql += " and b.zybz in (SELECT * FROM dbo.f_split(@zybz, ','))";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Bqz));
                        break;
                    case "cy"://出院：已出院状态
                        sql += " and b.zybz in (SELECT * FROM dbo.f_split(@zybz, ','))";
                        par.Add(new SqlParameter("@zybz", (int)EnumZYBZ.Ycy + "," + (int)EnumZYBZ.Djz));
                        break;
                }
            }
            else
            {
                sql += " and b.zybz<>'9'";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " and (zyh like @keyword or xm like @keyword)";
                par.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            }
            return this.FindList<InpWardPatTreeVO>(sql, par.ToArray());
        }
        public InpWardPatTreeVO GetPatList(string orgId, string zyh)
        {
            string sql = @"select  distinct b.zyh,b.xm hzxm,b.wardCode bqCode,c.bqmc,b.sex,b.zybz,b.ryrq,b.birth,
CAST(FLOOR(DATEDIFF(DY, b.birth, GETDATE()) / 365.25) AS VARCHAR(5)) nl,convert(varchar(50),isnull(b.rqrq,getdate()),120) rqrq,convert(varchar(50),isnull(b.cqrq,getdate()),120) cqrq
from [Newtouch_CIS]..zy_brxxk b with(nolock)  
left join [NewtouchHIS_Base].[dbo].[V_S_xt_bq] c with(nolock) on c.bqcode=b.wardCode and c.OrganizeId=@orgId and c.zt='1'
                where 1=1 and b.zt='1' ";
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                sql += " and (zyh = @zyh)";
                par.Add(new SqlParameter("@zyh", zyh));
            }
            return this.FirstOrDefault<InpWardPatTreeVO>(sql, par.ToArray());
        }
        /// <summary>
        /// 住院多病人护理录入
        /// </summary>
        /// <param name="entity"></param>
        public void submitmutiple(List<InpatientVitalSignsEntity> entity,string orgId,int? sjd,DateTime rq, string flag)
        {
            if (entity != null && entity.Count() > 0)
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    foreach (var item in entity)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Id))
                        {
                            db.Delete<InpatientVitalSignsEntity>(p=>p.Id==item.Id&&p.zt == "1");
                        }
                        if (db.IQueryable<InpatientVitalSignsEntity>(p => p.zyh == item.zyh && p.rq == item.rq && p.sj == item.sj && p.zdysjd == item.zdysjd && p.lrflag == item.lrflag && p.zt == "1").Any())
                        {
                            throw new FailedException("InpatientVitalSigns_Repeated_Error", "重复录入");
                        }
                        item.OrganizeId = orgId;
                        
                        item.rq = rq;
                        item.lrflag = flag;
                        item.Create(true);
                        if (flag == "1")//自定义录入
                        {
                            item.zdysjd = sjd.ToString();
                        }
                        else {
                            item.sj =int.Parse(sjd.ToString());
                        }
                        db.Insert(item);
                    }
                    db.Commit();                                  
                }
            }
        }

        public InpatientBasicInfoVO GetInpatientBasicInfo(string zyh, string orgId,string zhxz)
        {
            InpatientBasicInfoVO patVo = new InpatientBasicInfoVO();
            string sql = @"SELECT a.[zyh],b.[blh],convert(varchar(20),patid)[patid],
convert(varchar(10),a.[ryrq],120)[ryrq],convert(varchar(10),a.[cqrq],120)[cyrq]
,a.[zybz],convert(varchar(10),zhcode)[zhcode],convert(varchar(10),isnull(zhye,0.00))[zhye],[zhxz]
,Convert(decimal(18,2),isnull([xmzfy],0)+isnull([ypzfy],0)) zje,isnull(b.yjfy,0.00) yjfy,
isnull(b.bje,0.00) bje,isnull(b.ybxjzf,0.00) ybxjzf
  FROM zy_brxxk a with(nolock) 
left join  [dbo].[zy_brxxk_expand] b  with(nolock) on a.OrganizeId=b.OrganizeId and a.zyh=b.zyh and b.zt='1'
  where a.[OrganizeId]=@orgId and a.zt='1' and a.zyh=@zyh";
            patVo = this.FindList<InpatientBasicInfoVO>(sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId),
                new SqlParameter("@zyh",zyh)
            }).FirstOrDefault();
            
            if(patVo != null)
            {
                patVo.patTransList = GetInpatientBedUseInfo(zyh, orgId);                
            }
            return patVo;
        }

        public InpatientBasicInfoVO GetInpatientAccountInfo(string orgId,string zyh,string zhxz)
        {
            InpatientBasicInfoVO accountVo = new InpatientBasicInfoVO();
            string sql = @"select zhye 
from [NewtouchHIS_Sett].dbo.xt_zh a with(nolock)
where OrganizeId=@orgId and zt='1' 
and exists(select 1 from [NewtouchHIS_Sett].dbo.xt_brjbxx b with(nolock),zy_brxxk c with(nolock)
where a.OrganizeId=b.OrganizeId and a.patid=b.patid and b.OrganizeId=c.OrganizeId and b.zt='1' and c.zt='1' and b.blh=c.blh and c.zyh=@zyh 
) ";
            if (!string.IsNullOrWhiteSpace(zhxz))
            {
                sql += " and a.zhxz=" + Convert.ToInt16(zhxz);
            }
            var list= this.FindList<InpatientBasicInfoVO>(sql, new SqlParameter[]
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zyh", zyh)
            });
            if (list != null && list.Count > 0)
            {
                if (list.Count > 1)
                {
                    accountVo.zhye = "账户异常";
                }
                else
                {
                    accountVo.zhye = list.FirstOrDefault().zhye;
                }
            }

            return accountVo;
        }

        public List<InpatientBedUseInfoVO> GetInpatientBedUseInfo(string zyh, string orgId)
        {
            string sql = @"select 	blh,	zyh,	BedCode,	BedNo,	WardCode,	c.bqmc WardName,	RoomCode,	RoomName,	DeptCode,b.name	DeptName,TransWardCode,	TransDeptCode,	TransBedCode,	
[Status],convert(varchar(16),OccBeginDate,120)CreateTime,		convert(varchar(16),OccEndDate,120)LastModifyTime
from zy_cwsyjlk a with(nolock)
left join [NewtouchHIS_Base].dbo.v_s_sys_department b with(nolock) on a.DeptCode=b.code and a.OrganizeId=b.OrganizeId and b.zt='1'
left join [NewtouchHIS_Base].dbo.xt_bq c with(nolock) on a.wardcode=c.bqcode and a.organizeid =c.organizeid  and c.zt='1'
where a.OrganizeId=@orgId and zyh=@zyh
and OccBeginDate is not null
order by OccBeginDate desc ";

            return this.FindList<InpatientBedUseInfoVO>(sql, new SqlParameter[]
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zyh", zyh),
            });
        }

		public List<String> GetWardCodeByStaffId(string StaffId, string orgId)
		{
			string sql = @"select [bqCode] from [NewtouchHIS_Base].dbo.sys_staffward with(nolock) 
                            where zt=1 and StaffId=@StaffId and OrganizeId=@orgId";

			return this.FindList<String>(sql, new SqlParameter[]
			{
				new SqlParameter("@orgId", orgId),
				new SqlParameter("@StaffId", StaffId)
			});
		}
        

        public PatMedInsurSettVO PreSettbyId(string zyh,string orgId)
        {
            try
            {
                string sqlid = @"
declare @zjh varchar(50)=''
select @zjh=zjh from [NewtouchHIS_Sett]..zy_brjbxx with(nolock) where zyh=@zyh and OrganizeId=@orgId and zt='1'

select prejs_id,mdtrt_id,setl_id,psn_no,psn_name,psn_cert_type,certno,gend,naty,brdy,age,insutype,psn_type,cvlserv_flag,setl_time,mdtrt_cert_type,med_type,medfee_sumamt,fulamt_ownpay_amt,overlmt_selfpay,preselfpay_amt,inscp_scp_amt,act_pay_dedc,hifp_pay,pool_prop_selfpay,cvlserv_pay,hifes_pay,hifmi_pay,hifob_pay,maf_pay,hosp_part_amt,oth_pay,fund_pay_sumamt,psn_part_amt,acct_pay,psn_cash_pay,balc,acct_mulaid_pay,medins_setl_id,clr_optins,clr_way,clr_type,zyh,czydm,czrq,zt,zt_czy,zt_rq
from [NewtouchHIS_Sett].[dbo].[drjk_prejs_output] with(nolock)
where zyh=@zyh  and zt='1' 
order by czrq desc
";
                var data1 = FindList<PatMedInsurSettVO>(sqlid, new SqlParameter[] {
                new SqlParameter("zyh",zyh??""),
                new SqlParameter("orgId",orgId)
            });
                PatMedInsurSettVO settvo = new PatMedInsurSettVO();
                if (data1.Count > 0)
                {
                    string sql = @"
select prejs_id,mdtrt_id,setl_id,psn_no,psn_name,psn_cert_type,certno,gend,naty,brdy,age,insutype,psn_type,cvlserv_flag,setl_time,mdtrt_cert_type,med_type,medfee_sumamt,fulamt_ownpay_amt,overlmt_selfpay,preselfpay_amt,inscp_scp_amt,act_pay_dedc,hifp_pay,pool_prop_selfpay,cvlserv_pay,hifes_pay,hifmi_pay,hifob_pay,maf_pay,hosp_part_amt,oth_pay,fund_pay_sumamt,psn_part_amt,acct_pay,psn_cash_pay,balc,acct_mulaid_pay,medins_setl_id,clr_optins,clr_way,clr_type,zyh,czydm,czrq,zt,zt_czy,zt_rq
from [NewtouchHIS_Sett].[dbo].[drjk_prejs_output] with(nolock)
where prejs_id=@prejs_id
";
                    settvo = FirstOrDefault<PatMedInsurSettVO>(sql, new SqlParameter[] {
                new SqlParameter("prejs_id",data1[0].prejs_id)
            });
                }
                return settvo;

            }
            catch (Exception ex)
            {

                throw new FailedException("查询错误" + ex.Message);
            }

        }
        /// <summary>
        /// 预交金额度提醒
        /// </summary>
        /// <param name="patlist"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<PatQfWarnVo> PatYjjWarn(string patlist, string orgId)
        {
            string strsql = @"select
zyxx.zyh,zyxx.brxzdm,isnull(zhye,0.00) zhye,Convert(decimal(18,2),isnull([xmzfy],0)+isnull([ypzfy],0)) zje,
isnull(yjfy,0.00) yjfy,isnull(bje,0.00) bje,isnull(ybxjzf,0.00) ybxjzf,case zyxx.brxzdm
when '0' then  Convert(decimal(18,2),isnull(zhye,0.00)-(isnull([xmzfy],0.00)+isnull([ypzfy],0.00)-isnull(yjfy,0.00))) else 
 Convert(decimal(18,2),isnull(zhye,0.00)-isnull(ybxjzf,0.00)) end zhsy
from zy_brxxk zyxx
left join [zy_brxxk_expand] zyep on zyxx.zyh = zyep.zyh and zyxx.OrganizeId = zyep.OrganizeId and zyep.zt = 1
where 
zyxx.zt = 1 and  zyxx.OrganizeId=@orgId and zyxx.zyh in (select col from dbo.f_split(@patList, ',') where col> '')";
            return this.FindList<PatQfWarnVo>(strsql, new SqlParameter[]
           {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@patlist", patlist),
           });
        }


		public RefSuccess inserghjz(jzdjbrxx brxx)
		{
			var sqlParList = new List<SqlParameter>();
			string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(brxx);
			System.Xml.XmlDocument xmlstr = (System.Xml.XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode(jsonstr, "root");

			sqlParList.Add(new SqlParameter("@xml", xmlstr.InnerXml));
			var outParameter1 = new SqlParameter("@mzhout", System.Data.SqlDbType.VarChar, 50);
			outParameter1.Direction = System.Data.ParameterDirection.Output;
			sqlParList.Add(outParameter1);
			var outParameter2 = new SqlParameter("@blhout", System.Data.SqlDbType.VarChar, 50);
			outParameter2.Direction = System.Data.ParameterDirection.Output;
			sqlParList.Add(outParameter2);
			var outParameter3 = new SqlParameter("@cgbz", System.Data.SqlDbType.Int, 50);
			outParameter3.Direction = System.Data.ParameterDirection.Output;
			sqlParList.Add(outParameter3);
			var outParameter4 = new SqlParameter("@gcts", System.Data.SqlDbType.VarChar, 500);
			outParameter4.Direction = System.Data.ParameterDirection.Output;
			sqlParList.Add(outParameter4);
			_databaseFactory.Get().Database.ExecuteSqlCommand("exec his_cis_jzdj  @xml=@xml, @mzhout=@mzhout out, @blhout=@blhout out,@cgbz=@cgbz out,@gcts=@gcts out", sqlParList.ToArray());

			var val1 = outParameter1.Value;
			var val2 = outParameter2.Value;
			var val3 = outParameter3.Value;//状态
			var val4 = outParameter4.Value;//错误信息
			RefSuccess reflist = new RefSuccess();
			if (val1 != null)
			{
				reflist.mzh = val1.ToString();
			}
			if (val2 != null)
			{
				reflist.blh = val2.ToString();
			}
			if (val3 != null)
			{
				reflist.code = val3.ToString();
			}
			if (val4 != null)
			{
				reflist.message = val4.ToString();
			}
			return reflist;
		}
	} 
}
