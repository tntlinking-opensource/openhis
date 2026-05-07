using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.HIS.Domain.DTO.PharmacyDrugStorage;
using Newtouch.HIS.Domain.Entity.PharmacyDrugStorage;
using Newtouch.HIS.Domain.IDomainServices.PharmacyDrugStorage;
using Newtouch.HIS.Domain.VO;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.HIS.DomainServices.PharmacyDrugStorage
{
    public class PurchaseDmnService : DmnServiceBase, IPurchaseDmnService
    {
        public PurchaseDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }

        public List<PurchaseEntity> QueryPurchasebyId(string cgId, string orgId) {

            var strSql = new StringBuilder(@"
select * from xt_yp_cg a
  where a.zt = '1'
  and a.OrganizeId=@orgId
  and a.cgId=@cgId
");
            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@cgId", cgId),
            };

            return this.FindList<PurchaseEntity>(strSql.ToString(), param.ToArray());
        }
        public List<PurchaseDetailVO> QueryPurchaseDetailbyId(string cgId, string orgId)
        {

            var strSql = new StringBuilder(@"

select cgmx.*,yp.ypmc,yp.ycmc sccj,gys.gysmc yqmc ,
case cglx when 1 then '医保范围招标' when 2 then '医保范围未招标' end cglxmc,
case splx when 1 then '药品类' when 2 then '医用耗材器械类' when 9 then '其他' end splxmc,
case cgjldw when 1 then '计价单位' when 2 then '最小使用单位'  end cgjldwmc,
case dcpsbs when 0 then '不允许' when 1 then '允许' end dcpsbsmc
from xt_yp_cgmx cgmx
left join [NewtouchHIS_Base].dbo.xt_yp yp on cgmx.ypCode =yp.ypCode and cgmx.OrganizeId=yp.OrganizeId and cgmx.zt=yp.zt
left join [NewtouchHIS_Base].[dbo].[xt_ypgys] gys on cgmx.yqbm=gys.gysCode and cgmx.OrganizeId=gys.OrganizeId and cgmx.zt=gys.zt

  where cgmx.zt = '1'
  and cgmx.OrganizeId=@orgId
  and cgmx.cgId=@cgId
order by sxh 
");


            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@cgId", cgId),
            };

            return this.FindList<PurchaseDetailVO>(strSql.ToString(), param.ToArray());
        }

        public string PurchaseSubmit(PurchaseEntity cgEntity, List<PurchaseDetailEntity> cgmxList,string orgId, OperatorModel user)
        {
            try
            {
                //更新
                if (cgEntity.cgId != null)
                {
                    //更新采购列表
                    //string sql = "delete from zy_ksbyth where Id=@Id and OrganizeId=@orgId and zt='1' ";
                    //SqlParameter[] para = {
                    //        new SqlParameter("@Id", Djnr.isdeldj),
                    //        new SqlParameter("@orgId", orgId) };
                    //ExecuteSqlCommand(sql, para);

                    //删除采购明细
                    string sql2 = "delete from xt_yp_cgmx where cgId=@cgId and OrganizeId=@orgId and zt='1' ";
                    SqlParameter[] para2 = {
                            new SqlParameter("@cgId", cgEntity.cgId),
                            new SqlParameter("@orgId", orgId) };
                    ExecuteSqlCommand(sql2, para2);
                    //再添加采购明细
                    return "采购单据修改成功!";
                }

                //if (Djnr.issavesubmit == "1")
                //{
                //    shzt = "2";//提交
                //    tjsj = DateTime.Now.Date;
                //}
                //var request = new
                //{
                //    OrganizeId = orgId,
                //    yhgh = user.rygh,
                //    yplist = Djnr
                //};

                //新增
                else
                {
                    using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                    {
                        //cgEntity.cgId = Guid.NewGuid().ToString();
                        //cgEntity.OrganizeId = orgId;
                        //cgEntity.ddsj = DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm");
                        //cgEntity.ddbh = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("xt_cg_ddbh", orgId, "{0:D4}", true);
                        //cgEntity.ddzt = 1; //1已保存
                        //cgEntity.zt = "1";
                        //cgEntity.CreatorCode = user.rygh;
                        //cgEntity.CreateTime = DateTime.Now;
                        //cgEntity.LastModifierCode = "";
                        //cgEntity.LastModifyTime = null;
                        //db.Insert(cgEntity);
                        var cg = new PurchaseEntity
                        {
                            cgId = Guid.NewGuid().ToString(),
                            OrganizeId = orgId,
                            ddsj = DateTime.Now.ToString("yyyy-MM-dd HH:ss:mm"),
                            ddbh = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("xt_cg_ddbh", orgId, "{0:D4}", true),
                            ddzt = 1,//1已保存
                            zt = "1",
                            CreatorCode = user.rygh,
                            CreateTime = DateTime.Now,
                            LastModifierCode = "",
                            LastModifyTime = null,
                            czlx = cgEntity.czlx,
                            ddlx = cgEntity.ddlx,
                            psdbm = cgEntity.psdbm,
                            yybm = cgEntity.yybm,
                            yyjhdh = cgEntity.yyjhdh,
                            zwdhrq = cgEntity.zwdhrq,
                        };
                        db.Insert(cg);
                        foreach (var cgmxEntity in cgmxList)
                        {
                            cgmxEntity.cgmxId = Guid.NewGuid().ToString();
                            cgmxEntity.cgId = cgEntity.cgId;
                            cgmxEntity.OrganizeId = orgId;
                            cgEntity.zt = "1";
                            cgEntity.CreatorCode = user.rygh;
                            cgEntity.CreateTime = DateTime.Now;
                            cgEntity.LastModifierCode = "";
                            cgEntity.LastModifyTime = null;
                           // db.Insert(cgmxEntity);
                        };
                        db.Commit();
                    }
                    return "采购单据保存成功!";
                }
                //if (Djnr.issavesubmit == "1")
                //{
                //    var apires = SiteYfykAPIHelper.Request<APIRequestHelper.DefaultResponse>("/api/Stock/PrepareMedicineReturn", request, autoAppendToken: false);
                //    if (apires.code == APIRequestHelper.ResponseResultCode.SUCCESS && apires.msg == "")
                //    {
                //        return "保存并提交成功！";
                //    }
                //    else
                //    {
                //        return "调用药房接口失败，请联系开发人员！";
                //    }
                //}
                //return "科室备药退回保存成功！";
            }
            catch (Exception ex)
            {
                return "" + ex.InnerException.ToString();
            }
        }


        /// <summary>
        /// 获取采购明细及药品信息(药品入库-采购单引用)
        /// </summary>
        /// <param name="cgId"></param>
        /// <param name="orgId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public List<PurchaseStoreDTO> QueryPurchaseStorebyId(string cgId, string orgId,string yfbmCode)
        {

            var strSql = new StringBuilder(@" SELECT  cgmxId,ypCode,ypName ypmc,sxh,cglx,splx,zxspbm,cgjldw,ggbz,cgsl,cgdj,zje,dw,yqbm,yqmc,dlmc,SUM(kykc) kykc, 
dbo.f_getComplexYpSlandDw(SUM(kykc),zhyz,bmdw,zxdw) slStr,bmdw,zhyz,bzs,bzdw,zxdw,pzwh,ycmc sccj,CONVERT(NUMERIC(11,4),zxdwlsj) zxdwlsj,
CONVERT(NUMERIC(11,4),zxdwpfj) zxdwpfj,yklsj,ykpfj,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),zxdwlsj*zhyz))+'元/'+bmdw) lsjdjdw 
FROM (
	SELECT cgmx.cgmxId,cgmx.ypCode,cgmx.ypName,cgmx.sxh,cgmx.cglx,cgmx.splx,cgmx.zxspbm,cgmx.cgjldw,cgmx.ggbz,cgmx.cgsl,cgmx.cgdj,
		cgmx.dcpsbs,cgmx.bzsm,cgmx.dw,cgmx.zje,cgmx.yqbm,gys.gysmc yqmc,
		sfdl.dlmc, (kcxx.kcsl-kcxx.djsl) kykc,
		dbo.f_getyfbmDw(@yfbmCode, bmypxx.Ypdm, @orgId) bmdw
		,dbo.f_getyfbmZhyz(@yfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(yp.pzwh,'') pzwh,yp.ycmc
		,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj
	FROM xt_yp_cgmx cgmx (nolock)
	INNER JOIN xt_yp_bmypxx(NOLOCK) bmypxx ON cgmx.ypCode=bmypxx.Ypdm AND cgmx.OrganizeId=bmypxx.OrganizeId and  bmypxx.yfbmCode=@yfbmCode
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=cgmx.ypCode  AND kcxx.OrganizeId=cgmx.OrganizeId AND kcxx.zt='1' AND kcxx.yfbmCode=@yfbmCode
	LEFT JOIN [NewtouchHIS_Base].dbo.V_S_xt_yp yp (nolock) on cgmx.ypCode =yp.ypCode and cgmx.OrganizeId=yp.OrganizeId and cgmx.zt=yp.zt
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypgys] gys (nolock) on cgmx.yqbm=gys.gysCode and cgmx.OrganizeId=gys.OrganizeId and cgmx.zt=gys.zt
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE cgmx.zt = '1'
	  and cgmx.OrganizeId=@orgId
	  and cgmx.cgId=@cgId
  ) a
 GROUP BY cgmxId,ypCode,ypName,sxh,cglx,splx,zxspbm,cgjldw,ggbz,cgsl,cgdj,yqbm,yqmc,dlmc,bmdw,zhyz,bzs,bzdw,zxdw
 ,pzwh,ycmc,zxdwlsj,zxdwpfj,yklsj,ykpfj,dw,zje
 order by sxh");


            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@cgId", cgId),
                new SqlParameter("@yfbmCode", yfbmCode),
            };

            return this.FindList<PurchaseStoreDTO>(strSql.ToString(), param.ToArray());
        }



        /// <summary>
        /// 获取发票明细及药品信息 (药品入库-采购单引用)
        /// </summary>
        /// <param name="cgId"></param>
        /// <param name="orgId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public List<BillStoreDTO> QueryBillStorebyId(string fph, string orgId, string yfbmCode)
        {

            var strSql = new StringBuilder(@"

select * from 
(
select fpmx.*,yp.ypmc ,gys.gysmc yqmc ,yp.ypCode
,case dlsgbz when 0 then '否' when 1 then '是' end dlsgbzmc
,case sfwpsfp when 0 then '否' when 1 then '是' end sfwpsfpmc
,case splx when 1 then '药品类' when 1 then '医用耗材类' when 3 then '医疗器械类' when 9 then '其他' end splxmc
,case sfch when 0 then '否' when 1 then '是' end sfchmc
from xt_yp_fpmx fpmx
left join  NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx on fpmx.zxspbm=ypsx.gjybdm and fpmx.OrganizeId=ypsx.OrganizeId and fpmx.zt=ypsx.zt
left join [NewtouchHIS_Base].dbo.xt_yp yp on ypsx.ypCode =yp.ypCode and ypsx.OrganizeId=yp.OrganizeId and ypsx.zt=yp.zt
left join [NewtouchHIS_Base].[dbo].[xt_ypgys] gys on fpmx.yqbm=gys.gysCode and fpmx.OrganizeId=gys.OrganizeId and fpmx.zt=gys.zt

  where fpmx.zt = '1'
  and fpmx.OrganizeId=@orgId
  and fpmx.fph=@fph
  ) a
  left join (
SELECT  s.dlmc,s.ypmc,s.ypCode ypdm,s.ypgg gg,SUM(s.kykc) kykc, dbo.f_getComplexYpSlandDw(SUM(s.kykc),s.zhyz,s.bmdw,s.zxdw) slStr,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs
,s.bzdw,s.zxdw,s.pzwh,CONVERT(NUMERIC(11,4),s.zxdwlsj) zxdwlsj,CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz) lsj,CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz) pfj,s.ycmc sccj,s.yklsj,s.ykpfj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw
FROM 
(
	SELECT sfdl.dlmc, yp.ypmc, bmypxx.Ypdm ypCode, ypsx.ypgg, (kcxx.kcsl-kcxx.djsl) kykc, dbo.f_getyfbmDw(@yfbmCode, bmypxx.Ypdm, @orgId) bmdw
	, dbo.f_getyfbmZhyz(@YfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(ypsx.pzwh,'') pzwh,yp.ycmc
	,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj
	FROM dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=bmypxx.OrganizeId
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId AND kcxx.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE bmypxx.yfbmCode=@yfbmCode
	AND bmypxx.zt='1'
	AND bmypxx.OrganizeId=@orgId
	--AND (yp.ypCode LIKE '%'+@keyWord+'%' OR yp.ypmc LIKE '%'+@keyWord+'%' OR yp.py LIKE '%'+@keyWord+'%')
) s
GROUP BY s.dlmc,s.ypmc,s.ypCode,s.ypgg,s.bmdw,s.zhyz,s.bzs,s.bzdw,s.zxdw,s.pzwh,s.zxdwlsj,s.zxdwpfj,s.ycmc,s.yklsj,s.ykpfj

) b on a.ypCode=b.ypdm
");


            var param = new List<SqlParameter>
            {
                new SqlParameter("@orgId", orgId),
                new SqlParameter("@fph", fph),
                new SqlParameter("@yfbmCode", yfbmCode),
            };

            return this.FindList<BillStoreDTO>(strSql.ToString(), param.ToArray());
        }



    }
}
;