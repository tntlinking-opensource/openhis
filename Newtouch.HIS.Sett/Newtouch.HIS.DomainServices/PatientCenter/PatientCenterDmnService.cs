using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.ReportTemplateVO;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.PatientManage;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Newtouch.HIS.DomainServices
{
    public class PatientCenterDmnService: DmnServiceBase, IPatientCenterDmnService
    {
        public PatientCenterDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }


        #region 患者信息
        public HosPatientVo PatBrxzInfo(string patid,string orgId)
        {
            //            string sql = @"select a.patid, a.brxz,(case when a.brxz='1' and a.cblb='3' then '离休医保' 
            //when a.brxz='1' and a.xzlx is not null and a.xzlx='310' then '职工医保'
            //when a.brxz='1' and a.xzlx is not null and a.xzlx='390' then '居民医保'
            //else --xz.brxzmc
            //(case when a.brxz='1' and a.cblb<>'3' and a.lscblb='1' then '职工医保'
            //when a.brxz='1' and a.cblb<>'3' and a.lscblb='2' then'居民医保'
            //else b.brxzmc end) end) brxzmc,patid
            //from xt_brjbxx a with(nolock)
            //left join xt_brxz b with(nolock) on a.brxz=b.brxz and a.OrganizeId=b.OrganizeId and b.zt='1'
            //where a.patid=@patid and a.organizeid=@orgId";
            //            return FirstOrDefault<HosPatientVo>(sql, new SqlParameter[] {
            //                        new SqlParameter("orgId",orgId),
            //                        new SqlParameter("patid",patid)
            //                    });
            return null;
        }

        public PatientCenterVO PatientBasic(string zyh,string mzh,string keyword,string blh,string orgId, string ywlx="")
        {
            PatientCenterVO pat = new PatientCenterVO();
            string sql = "";
            switch (ywlx)
            {
                case "mz":
                    sql = @"select mzh,xm,xb,csny,blh,zjlx,zjh,CardType,CardTypeName,brly,ghnm,patid,brxz,ghly,mjzbz,ks,ys,jzbz,jzxh,jzrq,ghlx,ghf,zlf,ckf,gbf,jsrq,ghxz,fzbz,zdicd10,zdmc,kh,ghzt,xlbrbz,ghrq,ybjsh,jzyy,tjr,yyghId,nlshow,outpId,ghlybz,ScheduId,jzid,jzpzlx,bzbm,bzmc
from mz_gh a with(nolock)
where a.organizeid = @orgId and a.zt = '1' ";
                    if (!string.IsNullOrWhiteSpace(mzh))
                    {
                        sql += " and a.mzh = @mzh  ";
                    }
                    if (!string.IsNullOrWhiteSpace(blh))
                    {
                        sql += " and a.blh = @blh  ";
                    }
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        keyword = "%" + keyword + "%";
                        sql += " and exists(select 1 from xt_brjbxx b with(nolock) where a.patid=b.patid and a.organizeid=b.organizeid and b.zt='1' and( b.xm like @keyword or b.py like @keyword))  ";
                    }
                    if (string.IsNullOrWhiteSpace(mzh) && string.IsNullOrWhiteSpace(blh) && string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " and a.CreateTime>=@zjdjrq ";
                    }
                    pat.mzinfolist = FindList<OutpatientVO>(sql, new SqlParameter[] {
                        new SqlParameter("orgId",orgId),
                        new SqlParameter("mzh",mzh),
                        new SqlParameter("blh",blh),
                        new SqlParameter("zjdjrq",DateTime.Now.AddDays(-1).ToShortDateString()),
                        new SqlParameter("keyword",keyword??"")
                    });
                    break;
                case "zy":
                    sql = @"select a.zyh,a.patid,a.brxz,a.zybz,a.ks,a.bq,a.ryrq,a.rytj,a.rqry,a.rqrq,a.zy,a.mz,a.gj,a.cs_sheng,a.cs_shi,a.cs_xian,a.hu_sheng,a.hu_shi,a.hu_xian,a.hu_dz,a.xian_sheng,a.xian_shi,a.xian_xian,a.xian_dz,a.hy,a.bje,a.lxr,a.lxrgx,a.lxrdh,a.lxrdz,a.cyjdry,a.cyjdrq,a.cyrq,a.cyzd,a.cybq,a.lxrjtdh,a.lxrWebchat,a.lxrEmail,a.lxr2,a.lxrgx2,a.lxryddh2,a.lxrjtdh2,a.lxrWebchat2,a.lxrEmail2,a.lxrdz2,a.gms,ys,doctor,a.CreatorCode,a.CreateTime,a.LastModifierCode,a.LastModifyTime,kh,CardType,CardTypeName,jkjl,cw,rybq,ryzd,a.zt,a.xm,a.xb,a.blh,a.csny,a.zjh,a.zjlx,a.nl,a.brly,nlshow,zcyy,a.jjlxr_sheng,a.jjlxr_shi,a.jjlxr_dz,a.jjlxr_xian,jzid,bzbm,bzmc,jzlx
,d.cwmc,c.brxzmc
from zy_brjbxx a with(nolock)
left join xt_brxz c with(nolock) on a.brxz=c.brxz and a.OrganizeId=c.OrganizeId and c.zt='1'
left join[NewtouchHIS_Base].dbo.xt_cw d on a.cw=d.cwcode and a.bq=d.bqcode and a.OrganizeId=d.OrganizeId and d.zt='1'
left join xt_brjbxx k on a.patid=k.patid  and k.zt='1'
where a.organizeid=@orgId and a.zt='1' ";
                    if (!string.IsNullOrWhiteSpace(zyh))
                    {
                        sql += " and a.zyh = @zyh  ";
                    }
                    if (!string.IsNullOrWhiteSpace(blh)|| !string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " and exists(select 1 from xt_brjbxx b with(nolock) where a.patid=b.patid and a.organizeid=b.organizeid and b.zt='1' ";
                        if (!string.IsNullOrWhiteSpace(blh))
                        {
                            sql += " and b.blh=@blh ";
                        }
                        if (!string.IsNullOrWhiteSpace(keyword))
                        {
                            keyword = "%" + keyword + "%";
                            sql += "  and( b.xm like @keyword or b.py like @keyword)  ";
                        }
                        sql += " )";
                    }

                    if (string.IsNullOrWhiteSpace(zyh) && string.IsNullOrWhiteSpace(blh) && string.IsNullOrWhiteSpace(keyword))
                    {
                        sql += " and a.CreateTime>=@zjdjrq ";
                    }
                    pat.zyinfolist = FindList<HosPatientVo>(sql, new SqlParameter[] {
                        new SqlParameter("orgId",orgId),
                        new SqlParameter("zyh",zyh),
                        new SqlParameter("blh",blh??""),
                        new SqlParameter("zjdjrq",DateTime.Now.AddDays(-30).ToShortDateString()),
                        new SqlParameter("keyword",keyword??"")
                    });
                    break;
            }
            if (string.IsNullOrWhiteSpace(sql))
            {
                sql = @"select patid,brxz,blh,xm,py,xb,csny,zjh,dz,dh,cs_sheng,cs_shi,cs_xian,hu_sheng,hu_shi,hu_xian,hu_dz,xian_sheng,xian_shi,xian_xian,xian_dz,qx,yb,dybh,dwdm,qxdm,dwmc,pzh,ylxm,pzksrq,pzzzrq,pzzd,zh,zt,xl,gj,hf,mz,zy,ywxm,yddh,dzxz,gzdw,gzdz,jjllr,jjlldh,gzdwdh,bz,bxgs,zhbz,phone,wechat,email,zjlxfs,zjlx,CreatorCode,CreateTime,LastModifyTime,LastModifierCode,px,jsr,brly,jjllrgx,sbbh,cbdbm,zxsbbf,xnhgrbm,xnhylzh,cblb,grbh,jjlxr_sheng,dwbh,jjlxr_dz,zftoybId,jjlxr_xian,jjlxr_shi,jjllxr,xzlx,lscblb
from xt_brjbxx c with(nolock)
where c.organizeid=@orgId and c.zt='1' and c.CreateTime>=@zjdjrq ";
                pat.basic = FindList<PatientBasicAndCardInfoVO>(sql, new SqlParameter[] {
                    new SqlParameter("orgId",orgId),
                    new SqlParameter("zjdjrq",DateTime.Now.AddDays(-30).ToShortDateString())
                });
            }
            return pat;
        }
        /// <summary>
        /// 历次住院
        /// </summary>
        /// <param name="patid"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<HosPatientVo> InHospitalHistory(string patid,string zyh, string orgId)
        {
            string sql = @"select convert(int,row_number()over(order by syxh)) zycx,* from(
select distinct a.syxh, a.zyh,a.patid,a.brxz,(case when a.LastModifyTime>=e.LastModifyTime then a.zybz else convert(varchar(10),e.zybz) end )zybz,
a.xm,a.xb,a.blh,a.csny,a.zjh,a.zjlx,a.nl,a.nlshow,
a.brly,a.ks ryks,f.Name ryksmc,a.bq ryward,h.bqmc rywardname,convert(varchar(20),a.ryrq,120)rysj,a.rytj,a.zy,a.mz,a.gj,a.hy,
a.cs_sheng,a.cs_shi,a.cs_xian,a.hu_sheng,a.hu_shi,a.hu_xian,a.hu_dz,a.xian_sheng,a.xian_shi,a.xian_xian,a.xian_dz,
a.lxr,a.lxrgx,a.lxrdh,lxrdz,cyjdry,convert(varchar(20),cyjdrq,120)cyjdrq2,cyrq,(case when e.cqrq is null then null else convert(varchar(20),e.cqrq,120) end )cqsj,
cyzd,cybq,lxrjtdh,lxrWebchat,lxrEmail,lxr2,lxrgx2,lxryddh2,lxrjtdh2,lxrWebchat2,lxrEmail2,lxrdz2,
bje,a.gms,ys,doctor,kh,a.CardType,CardTypeName,jkjl,cw,rybq,e.zddm ryzdicd10,e.zdmc ryzdmc,e.cyzddm,e.cyzdmc,
zcyy,a.jjlxr_sheng,a.jjlxr_shi,a.jjlxr_dz,a.jjlxr_xian,jzid,bzbm,bzmc,jzlx
,c.brxzmc,
d.cwmc,e.DeptCode ksCode,g.Name ksmc,e.WardCode ward,i.bqmc wardname
,isnull(m.zhye,0.00) zhye
--,convert(decimal(18,2),isnull(j.xmzfy,0.00)+isnull(j.ypzfy,0.00)) zfy,isnull(j.yjfy,0.00) yjfy,
--(case when a.zybz in (" + (int)EnumZYBZ.Bqz+","+ (int)EnumZYBZ.Djz+","+ (int)EnumZYBZ.Zq + @") then convert(decimal(18,2),isnull(j.zhye,0.00)-isnull(j.xmzfy,0.00)-isnull(j.ypzfy,0.00)) else 0.00 end)djfy
from zy_brjbxx a with(nolock)
left join xt_brjbxx k with(nolock) on a.patid=k.patid and a.organizeid=k.organizeid and k.zt='1'
left join Newtouch_CIS.dbo.zy_brxxk e with(nolock) on a.zyh=e.zyh and a.OrganizeId=e.OrganizeId and e.zt='1'
left join xt_brxz c with(nolock) on a.brxz=c.brxz and a.OrganizeId=c.OrganizeId and c.zt='1'
left join[NewtouchHIS_Base].dbo.xt_cw d on a.cw=d.cwcode and a.bq=d.bqcode and a.OrganizeId=d.OrganizeId and d.zt='1'
left join NewtouchHIS_Base.dbo.sys_department f with(nolock) on a.ks=f.code and a.OrganizeId=f.OrganizeId 
left join NewtouchHIS_Base.dbo.sys_department g with(nolock) on e.DeptCode=g.code and e.OrganizeId=g.OrganizeId 
left join NewtouchHIS_Base.dbo.xt_bq h with(nolock) on a.bq=h.bqcode and a.OrganizeId=h.OrganizeId 
left join NewtouchHIS_Base.dbo.xt_bq i with(nolock) on e.WardCode=i.bqcode and e.OrganizeId=i.OrganizeId 
--left join Newtouch_CIS.dbo.zy_brxxk_expand j with(nolock) on e.zyh=j.zyh and e.OrganizeId=j.OrganizeId and j.zt='1'
left join zy_zh m with(nolock) on a.zyh=m.zyh and a.organizeid=m.organizeid and m.zt='1'
where a.organizeid=@orgId and a.patid=@patid and a.zt='1' and a.zybz<>9 
) zypat
  ";
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                sql += " where zyh=@zyh ";
            }
            return FindList<HosPatientVo>(sql, new SqlParameter[] {
                new SqlParameter("orgId",orgId),
                new SqlParameter("patid",patid),
                new SqlParameter("zyh",zyh??"")
            });
        }
        /// <summary>
        /// 住院医生信息
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public HosPatientVo InHospitalDoctorInfo(string zyh, string orgId,ref HosPatientVo patvo)
        {
            string sql = @"select zyh,max(case when [type]=" + (int)EnumYslx.ZyDoc + @" then ysgh else '' end)zyys,
max(case when [type]=" + (int)EnumYslx.ZyDoc+ @" then ysmc else '' end)zyysmc,
max(case when [type]=" + (int)EnumYslx.ZzDoc + @" then ysgh else '' end)zzys,
max(case when [type]=" + (int)EnumYslx.ZzDoc + @" then ysmc else '' end)zzysmc,
max(case when [type]=" + (int)EnumYslx.ZrDoc + @" then ysgh else '' end)zrys,
max(case when [type]=" + (int)EnumYslx.ZrDoc + @" then ysmc else '' end)zrysmc
from Newtouch_CIS.dbo.zy_PatDocInfo with(nolock)
where organizeid=@orgId and zyh=@zyh and zt='1'
group by zyh ";
            HosPatientVo docvo = FirstOrDefault<HosPatientVo>(sql, new SqlParameter[] {
                new SqlParameter("orgId",orgId),
                new SqlParameter("zyh",zyh)
            });
            if (docvo != null)
            {
                patvo.zyys = docvo.zyys;
                patvo.zyysmc = docvo.zyysmc;
                patvo.zrys = docvo.zrys;
                patvo.zrysmc = docvo.zrysmc;
                patvo.zzys = docvo.zzys;
                patvo.zzysmc = docvo.zzysmc;
            }
            return patvo;
        }

        public IList<PatientSettleHisVO> InHospitalSett(string zyh,string jsxz,string orgId)
        {
            string sql= @"select a.jsnm,a.OrganizeId,zyh,brxz,zyts,zje,zlfy,zffy,flzffy,jzfy,xjzf,xjwc,zhjz,xjzffs,fph,jszt,cxjsnm,cxjsyy,zh,jsxz,jsksrq,jsjsrq,fpdm,jmje,jylx,ysk,zl,sfrq,OutTradeNo
,b.jylsh setl_id,a.CreateTime setl_time
from zy_js a with(nolock)
left join cqyb_OutPut05 b with(nolock) on a.jsnm=b.jsnm and b.zt='1' and a.OrganizeId=b.OrganizeId
where a.zyh=@zyh and a.organizeid=@orgId and a.zt='1' 
 ";
            if (!string.IsNullOrWhiteSpace(jsxz))
            {
                sql += " and a.jsxz=@jsxz ";
            }
            return FindList<PatientSettleHisVO>(sql, new SqlParameter[] {
                new SqlParameter("zyh",zyh),
                new SqlParameter("orgId",orgId),
                new SqlParameter("jsxz",jsxz??""),
            });
        }

        public PatientSettleHisVO InHospitalSettbyJsnm(string jsnm, string settid, string orgId)
        {
            PatientSettleHisVO settvo = new PatientSettleHisVO();
            if (!string.IsNullOrWhiteSpace(settid))
            {
                settvo = MedInsurSettbyId(settid);
            }
            else
            {
                string sql = @" select b.xjzffsmc, a.jsnm,a.OrganizeId,zyh,brxz,zyts,zje,zlfy,zffy,flzffy,jzfy,xjzf,xjwc,zhjz,
 a.xjzffs,fph,jszt,cxjsnm,cxjsyy,a.zh,jsxz,jsksrq,jsjsrq,fpdm,jmje,jylx,ysk,zl,sfrq,OutTradeNo
,a.CreateTime setl_time,zje medfee_sumamt
from zy_js a with(nolock)
left join xt_xjzffs b with(nolock) on a.xjzffs=b.xjzffs
where a.jsnm=@jsnm and a.organizeid=@orgId and a.zt='1' 
 ";
                settvo = FirstOrDefault<PatientSettleHisVO>(sql, new SqlParameter[] {
                    new SqlParameter("jsnm",jsnm),
                    new SqlParameter("orgId",orgId),
                    new SqlParameter("settid",settid??""),
                });
            }
            return settvo;
        }
        #endregion


        #region 费用信息
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
        public IList<HospFeeUploadDetailVO> GetHospXmYpFeeVOList(Pagination pagination, string zyh, string orgId)
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
 
  and NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.ypjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm AND jsmx.OrganizeId = a.OrganizeId
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1' ) m WHERE (ypjfb.jfbbh=m.ypjfbbh or ypjfb.cxzyjfbbh=m.ypjfbbh) AND ypjfb.OrganizeId=m.OrganizeId  AND ypjfb.zyh=m.zyh

										)
) yp where CHARINDEX('_t',isnull(zyh,1))=0
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
 
  and  NOT EXISTS ( SELECT 1 FROM (
                                SELECT  jsmx.xmjfbbh,a.OrganizeId ,a.zyh --排除已冲销
                                FROM    zy_js a
                                        INNER JOIN zy_jsmx jsmx ON a.jsnm = jsmx.jsnm
                                WHERE   a.zyh=@zyh and NOT EXISTS ( SELECT 1
                                                     FROM   zy_js b
                                                     WHERE  a.jsnm = b.cxjsnm )
                                        AND jszt = '1')m WHERE (xmjfb.jfbbh=m.xmjfbbh or xmjfb.cxzyjfbbh=m.xmjfbbh) AND xmjfb.OrganizeId=m.OrganizeId AND xmjfb.zyh=m.zyh)

) xm where CHARINDEX('_t',isnull(zyh,1))=0 

";
            return this.QueryWithPage<HospFeeUploadDetailVO>(sql, pagination, new[] {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@zyh", zyh),
                //new SqlParameter("@kssj", kssj),
               
            });
        }
        /// <summary>
        /// 获取计费表数据
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="group">1 sfdl 2 sfxm 3 sfxm+jfrq 4 无</param>
        /// <param name="sfdl"></param>
        /// <param name="sfxm"></param>
        /// <param name="orgId"></param>
        /// <param name="kssj">yyyy-mm-dd</param>
        /// <param name="jssj">yyyy-mm-dd</param>
        /// <returns></returns>
        public IList<HospFeeChargeCategoryGroupDetailVO> GetPatjfbInfo(string zyh,string group,string sfdl,string sfxm,string orgId,string kssj, string jssj)
        {
            List<HospFeeChargeCategoryGroupDetailVO> list = new List<HospFeeChargeCategoryGroupDetailVO>();
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                string sql = @"exec dbo.rpt_InpatientFeeStac @zyh=@zyh,@orgId=@orgId,@group=@group,@sfdl=@sfdl,@sfxm=@sfxm,@ksrq=@ksrq,@jsrq=@jsrq;";
                list = FindList<HospFeeChargeCategoryGroupDetailVO>(sql, new SqlParameter[] {
                    new SqlParameter("zyh",zyh),
                    new SqlParameter("orgId",orgId),
                    new SqlParameter("group",group),
                    new SqlParameter("sfdl",sfdl??""),
                    new SqlParameter("sfxm",sfxm??""),
                    new SqlParameter("ksrq",kssj??""),
                    new SqlParameter("jsrq",jssj??""),
                });
            }
            return list;
        }
        public IList<HospFeeChargeCategoryGroupDetailVO> GetPatjfbInfo(string zyh,string group,string sfdl,string sfxm,string orgId,string kssj, string jssj, string keyword)
        {
            IList<HospFeeChargeCategoryGroupDetailVO> list = new List<HospFeeChargeCategoryGroupDetailVO>();
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = "%" + keyword + "%";
                }
                string sql = @"exec dbo.rpt_InpatientFeeStac @zyh=@zyh,@orgId=@orgId,@group=@group,@sfdl=@sfdl,@sfxm=@sfxm,@keyword=@keyword,@ksrq=@ksrq,@jsrq=@jsrq;";
                list = FindList<HospFeeChargeCategoryGroupDetailVO>(sql, new SqlParameter[] {
                    new SqlParameter("zyh",zyh),
                    new SqlParameter("orgId",orgId),
                    new SqlParameter("group",group),
                    new SqlParameter("sfdl",sfdl??""),
                    new SqlParameter("sfxm",sfxm??""),
                    new SqlParameter("ksrq",kssj??""),
                    new SqlParameter("jsrq",jssj??""),
                    new SqlParameter("keyword",keyword??"")
                });
            }
            return list;
        }

        #endregion


        #region 预交金
        public List<PatAccPayVO> GetAdvancePayment(string zyh, string orgId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"select  a.szje,a.zhye,a.pzh,s.Name Creator,a.CreateTime,a.szxz,b.xjzffsmc,b.xjzffs,a.Id,szjl.Id tId  
from [NewtouchHIS_Sett].[dbo].[zy_brjbxx] z with(nolock) 
left join zy_zhszjl a WITH ( NOLOCK ) on z.zyh=a.zyh and a.OrganizeId=z.OrganizeId and z.zt='1' 
LEFT JOIN xt_xjzffs b WITH ( NOLOCK ) ON a.xjzffs = b.xjzffs 
LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff s ON s.Account=a.CreatorCode 
LEFT JOIN dbo.zy_zhszjl szjl ON szjl.pzh = a.pzh 
AND szjl.szje + a.szje = 0 AND a.szje > 0  
AND szjl.zt = '1' 
WHERE z.zyh=@zyh and  
a.OrganizeId =@orgId AND a.zt = '1' 
ORDER BY a.CreateTime DESC");
            SqlParameter[] par = {
                    new SqlParameter("@zyh", zyh),
                    new SqlParameter("@orgId", orgId)
                };
            return this.FindList<PatAccPayVO>(strSql.ToString(), par);
        }
        #endregion


        #region 诊断

        public IList<PatZDListVO> GetDiagLsit( string orgId, string zyh)
        {
//            string zdsql = @"SELECT Id,[BAH],[ZYH],[ZDOrder],[JBDM],[JBMC],[RYBQ],[RYBQMS]
//,[CYQK],[CYQKMS]
//,[LastModifierCode],[OrganizeId]
//  FROM [Newtouch_MRMS].[dbo].[mr_basy_zd] with(nolock)
//where zt='1' and OrganizeId=@orgId and ZYH=@zyh   and JBDM<>'999999999' ";
            
            string zdsql = @"declare @jsrq datetime
select @jsrq=max(createtime)
from [NewtouchHIS_Sett].[dbo].[zy_js]   with(nolock) 
where zyh=@zyh  and OrganizeId=@orgId and zt='1'  and jszt=1 and isnull(cxjsnm,0)=0

select zyh,zdCode,zdmc,zdlb,zdsj,rybq,cyqk,zdlx,zdly,(case when sfsc='1' then '是' else '否' end)sfsc from ( 
select zyh,zdCode,zdmc,zdlb,zdsj,rybq,cyqk,zdlx,zdly,sfsc,row_number() over(partition by zdCode,zdlb order by zdsj) pm from ( 
select a.zyh,a.zddm zdcode,a.zdmc,(case when a.zdlb=1 then '入院诊断' when a.zdlb=2 then '出院诊断' else '其他'end)zdlb
,CONVERT(varchar(100), a.CreateTime, 23) zdsj,(case when b.wzjb is null then '无' else '有' end) rybq, b.cyfs cyqk,(case when a.zdlx=0 then '主要诊断' else '其他诊断' end)zdlx,'医生站' zdly 
,(case when a.CreateTime<@jsrq  then '1' else '0' end) sfsc ,zdlx zdpx
from [Newtouch_CIS].[dbo].[zy_brxxk] b with(nolock) 
left join [Newtouch_CIS].[dbo].zy_patdxinfo a with(nolock) 
on a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1' 
where a.zt='1' and a.OrganizeId=@orgId and  a.zyh=@zyh 
and a.zddm<>'999999999'
union all 
select a.zyh,a.zdcode,a.zdmc,'入院诊断' zdlb,CONVERT(varchar(100), a.CreateTime, 23) zdsj,null rybq, 
null cyqk,(case when a.zdpx=1 then '主要诊断' else '其他诊断' end) zdlx,'结算系统' zdly 
,(case when a.CreateTime<@jsrq  then '1' else '0' end) sfsc ,zdpx
from [Newtouch_CIS].[dbo].[zy_brxxk] b with(nolock) 
left join [NewtouchHIS_Sett].[dbo].[zy_rydzd] a with(nolock) 
on a.zyh=b.zyh and a.OrganizeId=b.OrganizeId and b.zt='1' 
where a.zt='1' and a.OrganizeId=@orgId and  a.zyh=@zyh 
union all
select z.zyh,z.JBDM zdcode,z.JBMC zdmc,'出院诊断' zdlb,CONVERT(varchar(100), z.CreateTime, 23) zdsj,z.RYBQMS rybq,z.CYQK, 
(case when z.ZDOrder='1' then '主要诊断' else '其他诊断' end) zdlx,'病案' zdly 
,(case when z.createtime<@jsrq then '1' else '0' end) sfsc  ,z.ZDOrder
from [Newtouch_MRMS].[dbo].[mr_basy_zd] z with(nolock) 
where z.zt='1' and z.jbdm<>'999999999' and  z.OrganizeId=@orgId and z.zyh=@zyh 
) s 
group by zyh,zdCode,zdmc,zdlb,zdsj,rybq,cyqk,zdlx,zdly,sfsc
) q where pm='1' order by zdlx desc;";

            var zdlist = FindList<PatZDListVO>(zdsql, new SqlParameter[] {
            new SqlParameter("@orgId",orgId),
            new SqlParameter("@zyh",zyh)
        }).ToList();

            return zdlist;
        }

        //      public IList<PatZDListVO> GetPatHisZDInfo( string zyh, string orgId, int? zdlb)
        //      {
        //          string sql = @"  select OrganizeId,ZYH,ZDOrder,JBDM ,JBMC,RYBQ,RYBQMS,CYQK,CYQKMS 
        //from [Newtouch_MRMS]..V_EMR_MrBasyDiag with(nolock) where organizeid=@orgId and zyh=@zyh ";
        //          return this.FindList<PatZDListVO>(sql, new SqlParameter[] {
        //              new SqlParameter("@orgId", orgId),
        //              new SqlParameter("@zyh",zyh)}).ToList();
        //      }

        #endregion

        #region 医保相关
        /// <summary>
        /// 预结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<PatMedInsurSettVO> MedInsurPreSettList(string zyh,string orgId)
        {
            string sql = @"
declare @zjh varchar(50)=''
select @zjh=zjh from zy_brjbxx with(nolock) where zyh=@zyh and OrganizeId=@orgId and zt='1'

select prejs_id,mdtrt_id,setl_id,psn_no,psn_name,psn_cert_type,certno,gend,naty,brdy,age,insutype,psn_type,cvlserv_flag,setl_time,mdtrt_cert_type,med_type,medfee_sumamt,fulamt_ownpay_amt,overlmt_selfpay,preselfpay_amt,inscp_scp_amt,act_pay_dedc,hifp_pay,pool_prop_selfpay,cvlserv_pay,hifes_pay,hifmi_pay,hifob_pay,maf_pay,hosp_part_amt,oth_pay,fund_pay_sumamt,psn_part_amt,acct_pay,psn_cash_pay,balc,acct_mulaid_pay,medins_setl_id,clr_optins,clr_way,clr_type,zyh,czydm,czrq,zt,zt_czy,zt_rq
from [dbo].[drjk_prejs_output] with(nolock)
where zyh=@zyh and certno=@zjh and zt='1' 
order by czrq desc
";
            return FindList<PatMedInsurSettVO>(sql, new SqlParameter[] {
                new SqlParameter("zyh",zyh??""),
                new SqlParameter("orgId",orgId)
            });
        }
        public PatMedInsurSettVO PreSettbyId(string presettId)
        {
            string sql = @"
select prejs_id,mdtrt_id,setl_id,psn_no,psn_name,psn_cert_type,certno,gend,naty,brdy,age,insutype,psn_type,cvlserv_flag,setl_time,mdtrt_cert_type,med_type,medfee_sumamt,fulamt_ownpay_amt,overlmt_selfpay,preselfpay_amt,inscp_scp_amt,act_pay_dedc,hifp_pay,pool_prop_selfpay,cvlserv_pay,hifes_pay,hifmi_pay,hifob_pay,maf_pay,hosp_part_amt,oth_pay,fund_pay_sumamt,psn_part_amt,acct_pay,psn_cash_pay,balc,acct_mulaid_pay,medins_setl_id,clr_optins,clr_way,clr_type,zyh,czydm,czrq,zt,zt_czy,zt_rq
from [dbo].[drjk_prejs_output] with(nolock)
where prejs_id=@prejs_id
";
            return FirstOrDefault<PatMedInsurSettVO>(sql, new SqlParameter[] {
                new SqlParameter("prejs_id",presettId)
            });
        }
        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<PatMedInsurSettVO> MedInsurSettList(string zyh, string orgId)
        {
            string sql = @"
declare @zjh varchar(50)=''
select @zjh=zjh from zy_brjbxx with(nolock) where zyh=@zyh and OrganizeId=@orgId and zt='1'

select c.organizeid,c.jsnm,c.jslb,b.mdtrt_cert_no,b.mdtrt_cert_type,b.mid_setl_flag,a.mdtrt_id,a.setl_id,a.psn_no,a.psn_name,a.psn_cert_type,a.certno,a.gend,a.naty,a.brdy,a.age,a.insutype,a.psn_type,a.cvlserv_flag,a.setl_time,a.mdtrt_cert_type,a.med_type,a.medfee_sumamt,a.fulamt_ownpay_amt,a.overlmt_selfpay,a.preselfpay_amt,a.inscp_scp_amt,a.act_pay_dedc,a.hifp_pay,a.pool_prop_selfpay,a.cvlserv_pay,a.hifes_pay,a.hifmi_pay,a.hifob_pay,a.maf_pay,a.hosp_part_amt,a.oth_pay,a.fund_pay_sumamt,a.psn_part_amt,a.acct_pay,a.psn_cash_pay,a.balc,a.acct_mulaid_pay,a.medins_setl_id,a.clr_optins,a.clr_way,a.clr_type,a.zyh,a.czydm,a.czrq,a.zt,a.zt_czy,a.zt_rq,a.jsqd_sclsh,a.jsqd_scrq,a.jsqd_scczy
from drjk_zyjs_output a with(nolock) ,drjk_zyjs_input b with(nolock) 
left join cqyb_OutPut05 c with(nolock) on b.setl_id=c.jylsh and c.zt='1'
where  a.setl_id=b.setl_id and a.zyh=@zyh and a.certno=@zjh and a.zt='1' and b.zt='1'
order by a.setl_time desc
";
            return FindList<PatMedInsurSettVO>(sql, new SqlParameter[] {
                new SqlParameter("zyh",zyh??""),
                new SqlParameter("orgId",orgId)
            });
        }
        public PatientSettleHisVO MedInsurSettbyId(string settId)
        {
            string sql = @"
select a.mdtrt_id,a.setl_id,a.psn_no,a.psn_name,a.psn_cert_type,a.certno,a.gend,a.naty,a.brdy,a.age,a.insutype,a.psn_type,a.cvlserv_flag,a.setl_time,a.mdtrt_cert_type,a.med_type,a.medfee_sumamt,a.fulamt_ownpay_amt,a.overlmt_selfpay,a.preselfpay_amt,a.inscp_scp_amt,a.act_pay_dedc,a.hifp_pay,a.pool_prop_selfpay,a.cvlserv_pay,a.hifes_pay,a.hifmi_pay,a.hifob_pay,a.maf_pay,a.hosp_part_amt,a.oth_pay,a.fund_pay_sumamt,a.psn_part_amt,a.acct_pay,a.psn_cash_pay,a.balc,a.acct_mulaid_pay,a.medins_setl_id,a.clr_optins,a.clr_way,a.clr_type,a.zyh,a.czydm,a.czrq,a.zt,a.zt_czy,a.zt_rq,a.jsqd_sclsh,a.jsqd_scrq,a.jsqd_scczy
 from drjk_zyjs_output a with(nolock)  
 where  a.setl_id=@setl_id
";
            return FirstOrDefault<PatientSettleHisVO>(sql, new SqlParameter[] {
                new SqlParameter("setl_id",settId)
            });
        }
        #endregion
    }
}
