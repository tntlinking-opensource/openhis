using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Newtouch.DomainServices
{
    public class PresTemplateDmnService : DmnServiceBase, IPresTemplateDmnService
    {
        private readonly IPresTemplateRepo _presTemplateRepo;
        private readonly IPresTemplateDetailRepo _presTemplateDetailRepo;

        public PresTemplateDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }

        /// <summary>
        /// 查询模板明细
        /// </summary>
        /// <param name="mbId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public PresTemplateBO SelectPresDetailByMbId(string mbId, string orgId,string cflx=null)
        {
            PresTemplateBO bo = new PresTemplateBO();
            var cfEntity = _presTemplateRepo.FindEntity(mbId);
            if (cfEntity == null&& cflx!= ((int)EnumCflx.InspectionPres).ToString() && cflx!= ((int)EnumCflx.ExaminationPres).ToString())
            {
                throw new FailedException("数据异常，未查询到该模板内容");
            }
            var sql = "";
            if (cfEntity != null)
            {
                bo.mbmc = cfEntity.mbmc;
                bo.tieshu = cfEntity.tieshu;
                bo.cfyf = cfEntity.cfyf;
                bo.djbz = cfEntity.djbz;
                bo.mblx = cfEntity.mblx;
                if (cfEntity.cflx == (int)EnumCflx.RehabPres)   //康复处方
                {
                    sql = @"
SELECT mbmx.mxId, mbId, xmCode, xmmc, mczll, pcCode, sl,
        sfxm.dj,
        sfxm.dw,
        sfxm.dwjls,
        pc.yzpcmc AS pcmc,
pc.yzpcmc AS pcmc,
bw,mbmx.zxks,ks.Name AS zxksmc,
        CASE WHEN ISNULL(sfxm.ybdm, '') = '' THEN '否'
             ELSE '是'
        END sfyb
FROM xt_cfmbmx mbmx
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm(nolock) sfxm
    ON sfxm.sfxmCode = mbmx.xmCode
        AND sfxm.OrganizeId = mbmx.OrganizeId
LEFT JOIN[NewtouchHIS_Base]..V_S_xt_yzpc(nolock) pc
   ON pc.yzpcCode = mbmx.pcCode
        AND pc.OrganizeId = mbmx.OrganizeId
LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department(nolock) ks
   ON mbmx.zxks = ks.Code
        AND mbmx.OrganizeId = ks.OrganizeId
WHERE mbmx.OrganizeId = @orgId
        AND mbmx.zt = '1'
        AND sfxm.zt = '1'
        AND pc.zt = '1'
        AND mbmx.mbId = @mbId
                ";
                }
                else if (cfEntity.cflx == (int)EnumCflx.RegularItemPres|| cfEntity.cflx == (int)EnumCflx.Yyhc)   //常规项目处方
                {
                    sql = @"
SELECT mbmx.mxId, mbId, xmCode, xmmc, zl,mbmx.mczll,mbmx.sl,mbmx.pcCode,pc.yzpcmc pcmc,
        sfxm.dj,
        sfxm.dw,sfxm.dwjls,
        CASE WHEN ISNULL(sfxm.ybdm, '') = '' THEN '否'
             ELSE '是'
        END sfyb,mbmx.zxks,ks.Name zxksmc
FROM xt_cfmbmx mbmx
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm(nolock) sfxm ON sfxm.sfxmCode = mbmx.xmCode AND sfxm.OrganizeId = mbmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_yzpc(nolock) pc ON pc.yzpcCode = mbmx.pcCode AND pc.OrganizeId = mbmx.OrganizeId
left join NewtouchHIS_Base..Sys_Department ks on ks.Code=mbmx.zxks and ks.OrganizeId=mbmx.OrganizeId
WHERE mbmx.OrganizeId = @orgId
        AND mbmx.zt = '1'
        AND sfxm.zt = '1'
        AND mbmx.mbId = @mbId
                ";
                }
                else if (cfEntity.cflx == (int)EnumCflx.WMPres || cfEntity.cflx == (int)EnumCflx.TCMPres) //药品处方
                {
                    sql = @"
SELECT mbmx.px,mbmx.mxId, mbmx.mbId, mbmx.ypCode, yp.ypmc, mcjl, mcjldw, yp.jldw AS redundant_jldw, mbmx.yfCode, pcCode, sl, mbmx.dw, zh,
        yp.lsj / yp.bzs * yp.mzcls AS dj,
        yp.mzcldw AS dw,
        pc.yzpcmc AS pcmc,
        yp.ypgg,
        yp.mzcls AS cls,
        yf.yfmc,mbmx.Remark,
mbmx.zxks ,
        CASE WHEN ISNULL(yp.ybdm, '') = '' THEN '否'
             ELSE '是'
        END sfyb,yp.jx jxCode
FROM xt_cfmbmx mbmx
left join xt_cfmb cfmb
on cfmb.mbId = mbmx.mbId
LEFT JOIN NewtouchHIS_Base..V_C_xt_yp yp
ON yp.ypCode = mbmx.ypCode AND yp.OrganizeId=mbmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_yzpc(nolock) pc
ON pc.yzpcCode=mbmx.pcCode AND pc.OrganizeId=mbmx.OrganizeId
LEFT JOIN [NewtouchHIS_Base]..V_S_xt_ypyf(nolock) yf
ON yf.yfCode=mbmx.yfCode
WHERE mbmx.OrganizeId=@orgId
        AND mbmx.zt='1'
        AND cfmb.zt='1'
        AND yp.zt='1'
        --AND pc.zt='1'
		--AND yf.zt='1'
        AND mbmx.mbId=@mbId
        order by mbmx.px 
                    ";
                }
            }
            else {
                sql = @"SELECT mbmx.ztxmId mxId,  mbzt.ztId mbId,mbzt.ztmc, mbmx.sfxmCode xmCode, mbmx.sfxmmc xmmc, sl zl, sl mczll,mbmx.sl,pc.yzpcCode pcCode,pc.yzpcmc pcmc,
        sfxm.dj,
        sfxm.dw,sfxm.dwjls,
        CASE WHEN ISNULL(sfxm.ybdm, '') = '' THEN '否'
             ELSE '是'
        END sfyb
FROM jyjc_ztxm (nolock) mbmx
JOIN jyjc_zt (nolock) mbzt on mbzt.ztId=mbmx.ztId and mbzt.OrganizeId=mbmx.OrganizeId and mbzt.zt='1'
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm(nolock) sfxm ON sfxm.sfxmCode = mbmx.sfxmCode AND sfxm.OrganizeId = mbmx.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_xt_yzpc(nolock) pc ON pc.yzpcmc = 'ST' AND pc.OrganizeId = mbmx.OrganizeId
WHERE mbmx.OrganizeId = @orgId
        AND mbmx.zt = '1'
        AND sfxm.zt = '1'
        AND mbmx.ztId = @mbId";
            }
            bo.mbmxList = this.FindList<PresTemplateDetailVO>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@mbId", mbId) });
            return bo;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="mbObj"></param>
        /// <param name="mxList"></param>
        public string SaveData(PresTemplateEntity mbObj, List<PresTemplateDetailmxVo> mxList)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (!string.IsNullOrWhiteSpace(mbObj.mbId))  //修改
                {
                    mbObj.zt = "1";
                    mbObj.Modify();

                    var dbMbObj = db.IQueryable<PresTemplateEntity>(p => p.mbId == mbObj.mbId).FirstOrDefault();
                    mbObj.tieshu = dbMbObj.tieshu;  //保留贴数、代煎标志、处方用法
                    mbObj.djbz = dbMbObj.djbz;
                    mbObj.cfyf = dbMbObj.cfyf;
                    //这里如果是中药处方，数量不更新也没关系，因为引用时不会采用
                    db.DetacheEntity(dbMbObj);

                    db.Update(mbObj);

                    db.Delete<PresTemplateDetailEntity>(a => a.mbId == mbObj.mbId && a.OrganizeId == mbObj.OrganizeId); //先全删，再新增
                }
                else
                {
                    mbObj.mbId = Guid.NewGuid().ToString();
                    mbObj.Create();

                    db.Insert(mbObj);
                }
                //模板明细表
                int sort = 0;
                foreach (var item in mxList)
                {
                    PresTemplateDetailEntity mbmx = new PresTemplateDetailEntity();
                    item.MapperTo(mbmx);

                    mbmx.mxId = Guid.NewGuid().ToString();
                    mbmx.mbId = mbObj.mbId;
                    mbmx.OrganizeId = mbObj.OrganizeId;
                    mbmx.px = sort;

                    mbmx.Create();
                    db.Insert(mbmx);
                    sort++;
                    if (item.ypCode2!=null&& item.ypCode2 != "")
                    {
                        PresTemplateDetailEntity mbmx2 = new PresTemplateDetailEntity();
                        mbmx2.ypCode = item.ypCode2;
                        mbmx2.xmCode = item.xmCode2;
                        mbmx2.xmmc = item.xmmc2;
                        mbmx2.ypmc = item.ypmc2;
                        mbmx2.mczll = item.mczll2;
                        mbmx2.mcjl = item.mcjl2;
                        mbmx2.mcjldw = item.mcjldw2;
                        mbmx2.yfCode = item.yfCode2;
                        mbmx2.pcCode = item.pcCode2;
                        mbmx2.sl = item.sl2;
                        mbmx2.zl = item.zl2;
                        mbmx2.dw = item.dw2;
                        mbmx2.bw = item.bw2;
                        mbmx2.Remark = item.Remark2;
                        mbmx2.zxks = item.zxks2;
                        mbmx2.mxId = Guid.NewGuid().ToString();
                        mbmx2.mbId = mbObj.mbId;
                        mbmx2.OrganizeId = mbObj.OrganizeId;
                        mbmx2.px = sort;
                        mbmx2.Create();
                        db.Insert(mbmx2);
                        sort++;
                    }
                }
                db.Commit();

                return mbObj.mbId;
            }

        }

        public void DeleteTemplate(string mbId, string orgId)
        {
            if (string.IsNullOrWhiteSpace(mbId))
            {
                throw new FailedException("缺少模板");
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Delete<PresTemplateEntity>(p => p.mbId == mbId && p.zt == "1" && p.OrganizeId == orgId);
                var temdetail = db.IQueryable<PresTemplateDetailEntity>().Where(p => p.mbId == mbId && p.zt == "1" && p.OrganizeId == orgId).ToList();
                if (temdetail != null && temdetail.Count() > 0)
                {
                    foreach (var item in temdetail)
                    {
                        db.Delete<PresTemplateDetailEntity>(p => p.mxId == item.mxId && p.OrganizeId == orgId && p.zt == "1");
                    }
                }
                db.Delete<PresTemplateDetailEntity>(p => p.mbId == mbId && p.zt == "1" && p.OrganizeId == orgId);
                db.Commit();
            }
        }


        public List<PresTemplateTree> SelectCfTemplateList(int cflx,int mblx, string orgId, string deptCode, string userCode,string mbKeyword=null)
        {
            var par = new List<SqlParameter>();
            par.Add(new SqlParameter("@orgId", orgId));
            var mbSql =   @"select cflx,mbId,mbmc,CreateTime,LastModifyTime 
                          from[dbo].[xt_cfmb] where zt='1'  and OrganizeId=@orgId ";
            if (cflx !=0)
            {  
                par.Add(new SqlParameter("@cflx", cflx));
                mbSql += @"  and cflx=@cflx ";
            }
            if (mblx !=0)
            {  
                par.Add(new SqlParameter("@mblx", mblx));
                mbSql += @"  and mblx=@mblx ";
            }
            if (mblx == (int)EnumCfMbLx.personal)
            {
                mbSql += @"  and ysgh=@userCode ";
                par.Add(new SqlParameter("@userCode", userCode));
            }
            else if (mblx == (int)EnumCfMbLx.department)
            {
                mbSql += @"  and ksCode=@deptCode ";
                par.Add(new SqlParameter("@deptCode", deptCode));
            }
            if (!string.IsNullOrWhiteSpace(mbKeyword))
            {
                mbSql += "  AND ( mbmc like '%'+@mbKeyword+'%' or dbo.fn_GetPyJp(mbmc) like '%'+@mbKeyword+'%' )";
                par.Add(new SqlParameter("@mbKeyword", mbKeyword));
            }
            mbSql += "  order by CreateTime desc ";
            return this.FindList<PresTemplateTree>(mbSql, par.ToArray());
        }
    }
}