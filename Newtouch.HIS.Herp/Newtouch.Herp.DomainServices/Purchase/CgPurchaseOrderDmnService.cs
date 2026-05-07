using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;

namespace Newtouch.Herp.DomainServices
{
    /// <summary>
    /// 采购计划
    /// </summary>
    public class CgPurchaseOrderDmnService : DmnServiceBase, ICgPurchaseOrderDmnService
    {
        public CgPurchaseOrderDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 获取采购计划主表信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="auditState"></param>
        /// <param name="userCode">操作员账号</param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VCgPurchaseOrderEntity> SelectPurchaseOrder(Pagination pagination, string cgdh, DateTime kssj, DateTime jssj, int auditState,
           string userCode, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT cpo.cgdh, cpo.rkbmCode, dept.Name rkbmmc, cpo.auditState, cpo.remark, cpo.CreateTime, cpo.CreatorCode, ss.Name CreatorName
FROM dbo.cg_purchaseOrder(NOLOCK) cpo
INNER JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.purchaseId=cpo.Id AND cpod.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department(NOLOCK) dept ON dept.Code=cpo.rkbmCode AND dept.OrganizeId=cpo.OrganizeId AND dept.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_User(NOLOCK) su ON su.Account=cpo.CreatorCode AND su.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_UserStaff(NOLOCK) sus ON sus.UserId=su.Id AND sus.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Staff(NOLOCK) ss ON ss.Id=sus.StaffId AND ss.OrganizeId=cpo.OrganizeId AND ss.zt='1' 
WHERE cpo.cgdh LIKE '%'+ISNULL(@cgdh,'')+'%'
AND cpo.CreateTime BETWEEN @kssj AND @jssj
AND cpo.zt='1'
AND cpo.OrganizeId=@OrganizeId
AND (cpo.CreatorCode=@userCode OR cpo.LastModifierCode=@userCode)
AND (cpo.auditState=@shzt OR -1=@shzt)
";
            var param = new DbParameter[]
            {
                new SqlParameter("@cgdh", cgdh),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@userCode", userCode),
                new SqlParameter("@shzt", auditState)
            };
            return QueryWithPage<VCgPurchaseOrderEntity>(sql, pagination, param);
        }

        /// <summary>
        /// 查询已审核通过的采购计划单
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="deptCode"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VCgPurchaseOrderEntity> SelectAdoptPurchasingPlanInfo(Pagination pagination, string cgdh, string deptCode, DateTime kssj, DateTime jssj, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT cpo.cgdh, cpo.rkbmCode, dept.Name rkbmmc, cpo.auditState, cpo.remark, cpo.LastModifyTime, cpo.LastModifierCode, ss.Name LastModifierName
FROM dbo.cg_purchaseOrder(NOLOCK) cpo
INNER JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.purchaseId=cpo.Id AND cpod.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department(NOLOCK) dept ON dept.Code=cpo.rkbmCode AND dept.OrganizeId=cpo.OrganizeId AND dept.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_User(NOLOCK) su ON su.Account=cpo.LastModifierCode AND su.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_UserStaff(NOLOCK) sus ON sus.UserId=su.Id AND sus.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Staff(NOLOCK) ss ON ss.Id=sus.StaffId AND ss.OrganizeId=cpo.OrganizeId AND ss.zt='1' 
WHERE cpo.cgdh LIKE '%'+ISNULL(@cgdh,'')+'%'
AND cpo.LastModifyTime BETWEEN @kssj AND @jssj
AND cpo.zt='1'
AND cpo.OrganizeId=@OrganizeId
AND cpo.auditState=1
AND (cpo.rkbmCode=@rkbmCode OR ''=ISNULL(@rkbmCode,''))
";
            var param = new DbParameter[]
            {
                new SqlParameter("@cgdh", cgdh),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@rkbmCode", deptCode)
            };
            return QueryWithPage<VCgPurchaseOrderEntity>(sql, pagination, param);
        }

        /// <summary>
        /// 获取采购计划主表信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="auditState"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VCgPurchaseOrderEntity> SelectPurchaseOrder(Pagination pagination, string cgdh, DateTime kssj, DateTime jssj, int auditState, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT cpo.cgdh, cpo.rkbmCode, dept.Name rkbmmc, cpo.auditState, cpo.remark, cpo.CreateTime, cpo.CreatorCode, ss.Name CreatorName
FROM dbo.cg_purchaseOrder(NOLOCK) cpo
INNER JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.purchaseId=cpo.Id AND cpod.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department(NOLOCK) dept ON dept.Code=cpo.rkbmCode AND dept.OrganizeId=cpo.OrganizeId AND dept.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_User(NOLOCK) su ON su.Account=cpo.CreatorCode AND su.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_UserStaff(NOLOCK) sus ON sus.UserId=su.Id AND sus.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Staff(NOLOCK) ss ON ss.Id=sus.StaffId AND ss.OrganizeId=cpo.OrganizeId AND ss.zt='1' 
WHERE cpo.cgdh LIKE '%'+ISNULL(@cgdh,'')+'%'
AND cpo.CreateTime BETWEEN @kssj AND @jssj
AND cpo.zt='1'
AND cpo.OrganizeId=@OrganizeId
AND (cpo.auditState=@shzt OR -1=@shzt)
";
            var param = new DbParameter[]
            {
                new SqlParameter("@cgdh", cgdh),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@shzt", auditState)
            };
            return QueryWithPage<VCgPurchaseOrderEntity>(sql, pagination, param);
        }

        /// <summary>
        /// 获取采购计划明细信息
        /// </summary>
        /// <param name="cgdh"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgPurchaseOrderDetailEntity> SelectPurchaseOrderDetail(string cgdh, string organizeId)
        {
            const string sql = @"
SELECT wz.name wzmc, lb.name lbmc, cpod.sl, CONCAT(cpod.sl,dw.name) slStr, cpod.sjsl, CONCAT(cpod.sjsl,dw.name) sjslStr, cpod.zhyz, ISNULL(wz.zxqds,0) zxqds
,cpod.unitId, dw.name unitName,wz.gg,ISNULL(wz.brand,'') brand, ISNULL(cj.name,'') sccj,(wz.jj*cpod.zhyz) jj, cpod.productId, cpod.Id pdId
FROM dbo.cg_purchaseOrder(NOLOCK) cpo 
INNER JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.purchaseId=cpo.Id AND cpod.zt='1'
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=cpod.productId AND wz.OrganizeId=cpo.OrganizeId AND wz.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) dw ON dw.Id=cpod.unitId AND dw.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.supplierType=1 AND cj.OrganizeId=cpo.OrganizeId AND cj.zt='1'
WHERE cpo.cgdh=@cgdh
AND cpo.OrganizeId=@OrganizeId
AND cpo.zt='1'
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@cgdh", cgdh)
            };
            return FindList<VCgPurchaseOrderDetailEntity>(sql, param);
        }

        /// <summary>
        /// 查询已审核通过的采购计划单
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cgdh"></param>
        /// <param name="deptCode"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VCgPurchaseOrderEntity> SelectAdoptWaitingPurchasePlanInfo(Pagination pagination, string cgdh, string deptCode, DateTime kssj, DateTime jssj, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT cpo.cgdh, cpo.rkbmCode, dept.Name rkbmmc, cpo.auditState, cpo.remark, cpo.LastModifyTime, cpo.LastModifierCode, ss.Name LastModifierName
FROM dbo.cg_purchaseOrder(NOLOCK) cpo
INNER JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.purchaseId=cpo.Id AND cpod.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.V_S_Sys_Department(NOLOCK) dept ON dept.Code=cpo.rkbmCode AND dept.OrganizeId=cpo.OrganizeId AND dept.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_User(NOLOCK) su ON su.Account=cpo.LastModifierCode AND su.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_UserStaff(NOLOCK) sus ON sus.UserId=su.Id AND sus.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Staff(NOLOCK) ss ON ss.Id=sus.StaffId AND ss.OrganizeId=cpo.OrganizeId AND ss.zt='1' 
WHERE cpo.cgdh LIKE '%'+ISNULL(@cgdh,'')+'%'
AND cpo.LastModifyTime BETWEEN @kssj AND @jssj
AND cpo.zt='1'
AND cpo.OrganizeId=@OrganizeId
AND cpo.auditState=1
AND (cpo.rkbmCode=@rkbmCode OR ''=ISNULL(@rkbmCode,''))
AND cpod.sl>ISNULL(cpod.sjsl,0)
";
            var param = new DbParameter[]
            {
                new SqlParameter("@cgdh", cgdh),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@rkbmCode", deptCode)
            };
            return QueryWithPage<VCgPurchaseOrderEntity>(sql, pagination, param);
        }

        /// <summary>
        /// 获取等待采购的采购计划明细
        /// </summary>
        /// <param name="cgdh"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgPurchaseOrderDetailEntity> SelectWaitingPurchasePlanDetail(string cgdh, string organizeId)
        {
            const string sql = @"
SELECT wz.name wzmc, lb.name lbmc, cpod.sl, CONCAT(cpod.sl,dw.name) slStr, cpod.sjsl, CONCAT(cpod.sjsl,dw.name) sjslStr, zxdw.name zxdwmc
,(CASE WHEN (cpod.sl-cpod.sjsl)>=0 THEN (cpod.sl-cpod.sjsl) ELSE 0 END) jxcgs, cpod.zhyz, ISNULL(wz.zxqds,0) zxqds
,cpod.unitId, dw.name unitName,wz.gg,ISNULL(wz.brand,'') brand, ISNULL(cj.name,'') sccj,(wz.jj*cpod.zhyz) jj, cpod.productId, cpod.Id pdId
,cpod.supplierId,cpod.supplierName,cpo.remark
FROM dbo.cg_purchaseOrder(NOLOCK) cpo 
INNER JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.purchaseId=cpo.Id AND cpod.zt='1'
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=cpod.productId AND wz.OrganizeId=cpo.OrganizeId AND wz.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) dw ON dw.Id=cpod.unitId AND dw.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.supplierType=1 AND cj.OrganizeId=cpo.OrganizeId AND cj.zt='1'
WHERE cpo.cgdh=@cgdh
AND cpo.OrganizeId=@OrganizeId
AND cpo.zt='1'
AND cpod.sl>ISNULL(cpod.sjsl,0)
AND cpo.auditState=1
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@cgdh", cgdh)
            };
            return FindList<VCgPurchaseOrderDetailEntity>(sql, param);
        }
    }
}