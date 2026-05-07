using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.BusinessObjects;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.HIS.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Newtouch.DomainServices
{
    public class InspectionTemplateDmnService : DmnServiceBase, IInspectionTemplateDmnService
    {
        private readonly IInspectionTemplateRepo _inspectionTemplateRepo;
        private readonly ITemplateGroupPackageRepo _templateGroupPackageRepo;
        public InspectionTemplateDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 根据type 查询检验检查模板
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jyjcmbLx">检验 or 检查</param>
        public List<InspectionTemplateTreeVO> GetTemplateListByType(string orgId, int jyjcmbLx)
        {
            var sql = @"
SELECT mb.mbId,
        mb.mbmc,
        dl.dlmc
FROM [dbo].[jyjc_mb](nolock) mb
LEFT JOIN [dbo].[jyjc_dl](nolock) dl
    ON dl.dlCode=mb.dlCode
        AND dl.OrganizeId=mb.OrganizeId
WHERE mb.zt='1'
        AND dl.zt='1'
        AND mb.[type]=@jyjcmbLx
        AND mb.OrganizeId=@orgId
                        ";
            var list = this.FindList<InspectionTemplateTreeVO>(sql, new[] { new SqlParameter("@jyjcmbLx", jyjcmbLx), new SqlParameter("@orgId", orgId) });
            return list;
        }

        /// <summary>
        /// 根据mbId 查询模板下组套项目
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mbId"></param>
        public List<GPackageTreeDetailVO> GetTemplateDetailByMbId(string orgId, string mbId,string ztKeyword)
        {
            var sql = @"
SELECT zt.ztId, zt.ztmc
FROM [dbo].[jyjc_mb] mb
LEFT JOIN [dbo].[jyjc_mbzt] mbzt
    ON mbzt.mbId=mb.mbId
        AND mbzt.OrganizeId=mb.OrganizeId
LEFT JOIN [dbo].[jyjc_zt] zt
    ON zt.ztId=mbzt.ztId
        AND zt.OrganizeId=mb.OrganizeId
WHERE zt.ztId is not null and mb.mbId=@mbId
        AND mb.OrganizeId=@orgId
        AND mb.zt='1' AND mbzt.zt='1' AND zt.zt='1' 
        AND ( zt.ztmc like '%'+@ztKeyword+'%' or dbo.fn_GetPyJp(zt.ztmc) like '%'+@ztKeyword+'%' )
 ";
            var list = this.FindList<GPackageTreeDetailVO>(sql, new[] { new SqlParameter("@mbId", mbId), new SqlParameter("@orgId", orgId) , new SqlParameter("@ztKeyword",string.IsNullOrEmpty(ztKeyword)==true ? "": ztKeyword) });
            return list;
        }


        /// <summary>
        /// 根据ztId查询组套下的收费项目
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="ztId"></param>
        /// <returns></returns>
        public List<GPackageZTTreeDetailVO> GetGPackageDetailByZtId(string orgId, string ztId,string mbId=null)
        {
            var sql = @"
SELECT ztxm.ztId,ztxm.sl,''bw, zt.ztmc, ztxm.sfxmCode AS xmCode,ztxm.sfxmCode AS xmdm, sfxm.sfxmmc AS xmmc, sfxm.dj, sfxm.dw, mb.zxks, Department.Name AS zxksmc,
        CASE WHEN ISNULL(sfxm.ybdm, '') = '' THEN '否'
             ELSE '是'
        END sfyb,
	   sfxm.sqlx,ztxm.px
FROM [dbo].[jyjc_ztxm] ztxm
LEFT JOIN [dbo].[jyjc_zt] zt  ON zt.ztId=ztxm.ztId  AND zt.OrganizeId=ztxm.OrganizeId
LEFT JOIN [dbo].[jyjc_mbzt] mbzt  ON mbzt.ztId=ztxm.ztId  AND mbzt.OrganizeId=ztxm.OrganizeId
LEFT JOIN [dbo].[jyjc_mb] mb  ON mb.mbId=mbzt.mbId  AND mb.OrganizeId=ztxm.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm  ON sfxm.sfxmCode=ztxm.sfxmCode and sfxm.OrganizeId=ztxm.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department Department  ON Department.Code=mb.zxks  AND Department.OrganizeId= ztxm.OrganizeId
WHERE ztxm.OrganizeId=@orgId AND mb.zt='1' AND mbzt.zt='1' AND zt.zt='1' AND ztxm.zt='1' AND sfxm.zt='1' 
      ";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            if (ztId.Contains(","))
            {
                sql += " and ztxm.ztId in(select col from dbo.f_split(@ztId ,',')) ";
                pars.Add(new SqlParameter("@ztId", ztId));
            }
            else if (!string.IsNullOrWhiteSpace(ztId))
            {
                sql += " and ztxm.ztId=@ztId ";
                pars.Add(new SqlParameter("@ztId", ztId));
            }
            if (!string.IsNullOrWhiteSpace(mbId))
            {
                sql += " and mb.mbId=@mbId ";
                pars.Add(new SqlParameter("@mbId", mbId));
            }
            sql += "  order by px     ";
            var list = this.FindList<GPackageZTTreeDetailVO>(sql, pars.ToArray());
            return list;
        }

        public List<GPackageZTTreeDetailVO> GetGPackageInfoByZtId(string orgId, string ztId,string mbId)
        {
            var sql = @"
SELECT ztxm.ztId,zt.ztmc,round(sum(ztxm.sl*sfxm.dj),2) zhje,mb.zxks, Department.Name AS zxksmc,sfxm.sqlx
FROM [dbo].[jyjc_ztxm] ztxm with(nolock)
LEFT JOIN [dbo].[jyjc_zt] zt with(nolock)   ON zt.ztId=ztxm.ztId   AND zt.OrganizeId=ztxm.OrganizeId
LEFT JOIN [dbo].[jyjc_mbzt] mbzt with(nolock)   ON mbzt.ztId=ztxm.ztId   AND mbzt.OrganizeId=ztxm.OrganizeId
LEFT JOIN [dbo].[jyjc_mb] mb with(nolock) ON mb.mbId=mbzt.mbId AND mb.OrganizeId=ztxm.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm  with(nolock) ON sfxm.sfxmCode=ztxm.sfxmCode and sfxm.OrganizeId=ztxm.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department Department  with(nolock) ON Department.Code=mb.zxks AND Department.OrganizeId= ztxm.OrganizeId
WHERE ztxm.ztId=@ztId AND mb.mbId=@mbId  AND ztxm.OrganizeId=@orgId AND mb.zt='1' AND mbzt.zt='1' AND zt.zt='1' AND ztxm.zt='1' AND sfxm.zt='1' 
group by  ztxm.ztId,zt.ztmc,mb.zxks, Department.Name,sfxm.sqlx ";
            var list = this.FindList<GPackageZTTreeDetailVO>(sql, new[] { new SqlParameter("@ztId", ztId), new SqlParameter("@mbId", mbId), new SqlParameter("@orgId", orgId) });
            return list;
        }

        /// <summary>
        /// 模板详情
        /// </summary>
        /// <param name="ztId"></param>
        public InspectionTemplateDetailBO GetTemplateDetail(string mbId, string orgId)
        {
            if (string.IsNullOrWhiteSpace(mbId))
            {
                throw new FailedException("数据异常，mbId为空或null");
            }

            //模板信息
            var sqlllll = @"
SELECT mbId, mb.[Type], mb.mbmc, mb.dlCode, mb.zxks, Department.Name AS zxksmc,mb.zt
FROM [dbo].[jyjc_mb] mb
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department Department
    ON Department.Code=mb.zxks
        AND Department.OrganizeId= mb.OrganizeId
WHERE mb.mbId=@mbId
        AND mb.OrganizeId=@orgId
                        ";
            var mbEntity = this.FirstOrDefault<InspectionTemplateVO>(sqlllll, new[] { new SqlParameter("@mbId", mbId), new SqlParameter("@orgId", orgId) });

            if (mbEntity == null)
            {
                throw new FailedException("数据异常，没有找到组套信息");
            }

            //模板组套
            var sql = @"
SELECT zutao.*      --zutao.ztId, zutao.ztmc, zutao.[Type], zutao.[Description], zutao.Remark
FROM [dbo].[jyjc_mb] mb
RIGHT JOIN [dbo].[jyjc_mbzt] mbzt
    ON mbzt.mbId=mb.mbId
        AND mbzt.OrganizeId=mb.OrganizeId
LEFT JOIN [dbo].[jyjc_zt] zutao
    ON zutao.ztId=mbzt.ztId
        AND zutao.OrganizeId=mb.OrganizeId
WHERE mb.mbId=@mbId
        AND mb.OrganizeId=@orgId
                    ";
            var mbztList = this.FindList<GroupPackageEntity>(sql, new[] { new SqlParameter("@mbId", mbId), new SqlParameter("@orgId", orgId) });

            var bo = new InspectionTemplateDetailBO();
            bo.mbEntity = mbEntity;
            bo.mbztList = mbztList;
            return bo;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ztobj"></param>
        /// <param name="ztxmlist"></param>
        public void SaveData(InspectionTemplateEntity mbobj, List<TemplateGroupPackageEntity> mbztlist)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //模板
                if (!string.IsNullOrWhiteSpace(mbobj.mbId))  //修改
                {
                    mbobj.Modify();

                    db.Update(mbobj);

                    db.Delete<TemplateGroupPackageEntity>(a => a.mbId == mbobj.mbId); //先全删，再新增
                }
                else
                {
                    mbobj.mbId = Guid.NewGuid().ToString();
                    mbobj.Create();

                    db.Insert(mbobj);
                }
                
                //模板组套
                if (mbztlist != null)
                {
                    foreach (var item in mbztlist)
                    {
                        TemplateGroupPackageEntity mbzt = new TemplateGroupPackageEntity();
                        mbzt.mbztId = Guid.NewGuid().ToString();
                        mbzt.OrganizeId = mbobj.OrganizeId;
                        mbzt.mbId = mbobj.mbId;
                        mbzt.mbmc = mbobj.mbmc;
                        mbzt.ztId = item.ztId;
                        mbzt.ztmc = item.ztmc;
                        mbzt.Create();

                        db.Insert(mbzt);
                    }
                }

                db.Commit();
            }

        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ztId"></param>
        public void DeleteData(string mbId)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                db.Delete<InspectionTemplateEntity>(a => a.mbId == mbId);
                db.Delete<TemplateGroupPackageEntity>(a => a.mbId == mbId);

                db.Commit();
            }
        }
    }
}
