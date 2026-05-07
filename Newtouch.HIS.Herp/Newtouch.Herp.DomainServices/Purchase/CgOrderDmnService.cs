using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Infrastructure;
using Newtouch.Herp.Infrastructure.Enum;

namespace Newtouch.Herp.DomainServices
{
    /// <summary>
    /// 采购单
    /// </summary>
    public class CgOrderDmnService : DmnServiceBase, ICgOrderDmnService
    {
        public CgOrderDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 查询采购单信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="orderNo"></param>
        /// <param name="orderType"></param>
        /// <param name="orderProcess"></param>
        /// <param name="organizeId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public IList<VCgOrderEntity> SelectCgOrder(Pagination pagination, string orderNo, int orderType, int orderProcess, string organizeId, DateTime kssj, DateTime jssj)
        {
            var sql = new StringBuilder(@"
SELECT DISTINCT co.orderType, co.orderProcess,co.orderNo, ISNULL(co.remark,'') remark, co.CreateTime, ss.Name CreatorName, co.LastModifyTime, ss1.Name LastModifierName
FROM dbo.cg_order(NOLOCK) co
INNER JOIN dbo.cg_orderDetail(NOLOCK) cod ON cod.orderId=co.Id AND cod.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_User(NOLOCK) su ON su.Account=co.CreatorCode AND su.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_UserStaff(NOLOCK) sus ON sus.UserId=su.Id AND sus.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Staff(NOLOCK) ss ON ss.Id=sus.StaffId AND ss.OrganizeId=co.OrganizeId AND ss.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_User(NOLOCK) su1 ON su1.Account=co.LastModifierCode AND su1.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.Sys_UserStaff(NOLOCK) sus1 ON sus1.UserId=su1.Id AND sus1.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Staff(NOLOCK) ss1 ON ss1.Id=sus1.StaffId AND ss1.OrganizeId=co.OrganizeId AND ss1.zt='1' 
WHERE co.OrganizeId=@OrganizeId
AND co.zt='1'
AND co.orderNo LIKE '%'+ISNULL(@orderNo,'')+'%'
AND co.CreateTime BETWEEN @kssj AND @jssj
");
            var param = new List<DbParameter>
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@orderNo", orderNo),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj)
            };
            switch (orderType)
            {
                case (int)EnumOrderTypeHrp.OfficialOrder:
                case (int)EnumOrderTypeHrp.TempOrder:
                case (int)EnumOrderTypeHrp.BadOrder:
                    sql.AppendLine("AND co.orderType=@orderType ");
                    param.Add(new SqlParameter("@orderType", orderType));
                    break;
            }
            switch (orderProcess)
            {
                case (int)EnumOrderProcess.Waiting:
                case (int)EnumOrderProcess.Complete:
                case (int)EnumOrderProcess.Delivering:
                case (int)EnumOrderProcess.PreparingGoods:
                case (int)EnumOrderProcess.Refusal:
                case (int)EnumOrderProcess.RefusalSign:
                case (int)EnumOrderProcess.SignFor:
                    sql.AppendLine("AND co.orderProcess=@orderProcess  ");
                    param.Add(new SqlParameter("@orderProcess", orderProcess));
                    break;
            }

            return QueryWithPage<VCgOrderEntity>(sql.ToString(), pagination, param.ToArray());
        }

        /// <summary>
        /// 查询采购单明细
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgOrderDetailEntity> SelectCgOrderDetailGroupByCgdh(string orderNo, string organizeId)
        {
            const string sql = @"
SELECT cod.subOrderNo, cpo.cgdh, dept.Name deptName, wz.name wzmc, CONCAT(cod.sl,cod.dwmc) slStr, wz.gg, ISNULL(wz.brand,'') brand, CONCAT(CONVERT(NUMERIC(11,2),cod.jj),'元/',cod.dwmc) jjStr
,ISNULL(cj.name,'') sccj, ISNULL(gys.name,'') gysmc, ISNULL(cod.remark,'') remark, cod.productId,cod.fph
FROM dbo.cg_order(NOLOCK) co
INNER JOIN dbo.cg_orderDetail(NOLOCK) cod ON cod.orderId=co.Id AND cod.zt='1'
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=cod.productId AND wz.OrganizeId=co.OrganizeId AND wz.zt='1'
LEFT JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.Id=cod.pdId AND cpod.zt='1'
LEFT JOIN dbo.cg_purchaseOrder(NOLOCK) cpo ON cpo.Id=cpod.purchaseId AND cpo.OrganizeId=co.OrganizeId AND cpo.zt='1' AND cpo.auditState='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Department(NOLOCK) dept ON dept.Code=cpo.rkbmCode AND dept.OrganizeId=co.OrganizeId AND dept.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON gys.Id=cod.supplierId AND gys.OrganizeId=co.OrganizeId AND gys.supplierType=2 AND gys.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.OrganizeId=co.OrganizeId AND cj.supplierType=1 AND gys.zt='1'
WHERE co.OrganizeId=@OrganizeId
AND co.zt='1'
AND co.orderNo=@orderNo
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@orderNo",orderNo )
            };
            return FindList<VCgOrderDetailEntity>(sql, param);
        }

        /// <summary>
        /// 查询采购单明细
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgOrderDetailEntity> SelectCgOrderDetailNoCgdh(string orderNo, string organizeId)
        {
            const string sql = @"
SELECT d.wzmc, CONCAT(d.sl,d.dwmc) slStr, d.gg, d.brand, CONCAT(CONVERT(NUMERIC(11,2),d.jj),'元/',d.dwmc) jjStr,d.sccj, d.gysmc, d.remark, d.productId
FROM (
	SELECT wz.name wzmc, SUM(cod.sl) sl, wz.gg, ISNULL(wz.brand,'') brand, CONCAT(CONVERT(NUMERIC(11,2),cod.jj),'元/',cod.dwmc) jjStr
	,ISNULL(cj.name,'') sccj, ISNULL(gys.name,'') gysmc, ISNULL(cod.remark,'') remark, cod.productId, cod.dwmc, cod.jj
	FROM dbo.cg_order(NOLOCK) co
	INNER JOIN dbo.cg_orderDetail(NOLOCK) cod ON cod.orderId=co.Id AND cod.zt='1'
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=cod.productId AND wz.OrganizeId=co.OrganizeId AND wz.zt='1'
	LEFT JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.Id=cod.pdId AND cpod.zt='1'
	LEFT JOIN dbo.cg_purchaseOrder(NOLOCK) cpo ON cpo.Id=cpod.purchaseId AND cpo.OrganizeId=co.OrganizeId AND cpo.zt='1' AND cpo.auditState='1'
	LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON gys.Id=cod.supplierId AND gys.OrganizeId=co.OrganizeId AND gys.supplierType=2 AND gys.zt='1'
	LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.OrganizeId=co.OrganizeId AND cj.supplierType=1 AND gys.zt='1'
	WHERE co.OrganizeId=@OrganizeId
	AND co.zt='1'
	AND co.orderNo=@orderNo
	GROUP BY wz.name, wz.gg, wz.brand, cod.jj,cj.name,gys.name, cod.remark,cod.productId, cod.dwmc
) d
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@orderNo",orderNo )
            };
            return FindList<VCgOrderDetailEntity>(sql, param);
        }
        #region 外部入库-采购单导入
        /// <summary>
        /// 外部入库-采购单引用View
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgOrderRkEntity> SelectCgOrderView(DateTime kssj, DateTime jssj, string fph, string orderNo, string organizeId)
        {
            StringBuilder sql = new StringBuilder(@"
SELECT co.CreateTime orderDate,co.orderNo,cod.subOrderNo,gys.name gysmc,cod.fph fp,cod.fph,COUNT(1) cgmxs,SUM(convert(decimal(18,2),cod.sl*cod.jj)) cgje
FROM dbo.cg_order(NOLOCK) co
INNER JOIN dbo.cg_orderDetail(NOLOCK) cod ON cod.orderId=co.Id AND cod.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON gys.Id=cod.supplierId AND gys.OrganizeId=co.OrganizeId AND gys.supplierType=2 AND gys.zt='1'
WHERE co.OrganizeId=@OrganizeId
AND co.zt='1' AND co.ordertype='1' ")
 ;
            if (!string.IsNullOrWhiteSpace(orderNo))
                sql.Append(@" AND co.orderNo =@orderNo");
            if (!string.IsNullOrWhiteSpace(fph))
                sql.Append(@" AND cod.fph =@fph");
            sql.Append(@" AND not exists (select * from [kf_crkmx] rkdjmx with(nolock),[kf_crkdj] rkdj with(nolock) where rkdjmx.fph=cod.fph 
                AND rkdj.Id = rkdjmx.crkId and rkdj.auditstate != '1' and rkdj.OrganizeId =@OrganizeId  )
            group by co.CreateTime,co.orderNo,cod.subOrderNo,gys.name,cod.fph");
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@kssj",kssj ),
                new SqlParameter("@jssj",jssj ),
                new SqlParameter("@orderNo",orderNo??"" ),
                new SqlParameter("@fph",fph??"" )
            };
            return FindList<VCgOrderRkEntity>(sql.ToString(), param);
        }

        /// <summary>
        /// 外部入库-采购单引用
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgOrderImportEntity> SelectCgOrderDetailImport(string subOrderNo, string organizeId)
        {
            StringBuilder sql = new StringBuilder(@"
SELECT  wz.name wzmc,wzlb.name lbmc,wz.gjybdm,cod.sl*cod.zhyz sl,rpu.unitId,rpu.unit unitName,convert(decimal(18,2),cod.sl*cod.jj) jjze,
cod.fph,ISNULL(cod.remark,'') remark,ISNULL(SUM(kcxx.kcsl),0) kcsl, ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl,
CONCAT(ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0),rpu.unit) slStr,wz.gg,ISNULL(cj.name,'') sccj,
wz.supplierId gysId,ISNULL(gys.name,'') gysmc,
wz.lsj,convert(decimal(18,2),cod.sl*cod.zhyz*wz.lsj) lsze,
1 zhyz,wz.Id productId,convert(decimal(18,4),cod.jj/cod.zhyz) jj,wz.lsj minlsj
FROM dbo.cg_order(NOLOCK) co
INNER JOIN dbo.cg_orderDetail(NOLOCK) cod ON cod.orderId=co.Id AND cod.zt='1'
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=cod.productId AND wz.OrganizeId=co.OrganizeId AND wz.zt='1'
LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu on rpu.productId=wz.Id AND rpu.unitId=wz.minunit AND rpu.OrganizeId=wz.OrganizeId AND rpu.zt='1'
LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=wz.Id AND kcxx.warehouseId=@warehouseId AND kcxx.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON gys.Id=cod.supplierId AND gys.OrganizeId=co.OrganizeId AND gys.supplierType=2 AND gys.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.OrganizeId=co.OrganizeId AND cj.supplierType=1 AND gys.zt='1'
INNER JOIN  NewtouchHIS_Base..wz_type wzlb on wzlb.Id=wz.typeId 
WHERE co.OrganizeId=@organizeId
AND co.zt='1'
AND cod.subOrderNo=@subOrderNo
group by 
wz.name ,wzlb.name ,wz.gjybdm,cod.sl,cod.zhyz,rpu.unit,cod.jj,
cod.fph,cod.remark,wz.gg,wz.lsj,cj.name,gys.name,cod.dwmc,wz.supplierId,rpu.unitId,wz.Id ,wz.lsj 
 ");
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@subOrderNo",subOrderNo ),
                new SqlParameter("@warehouseId",Constants.CurrentKf.currentKfId )
            };
            return FindList<VCgOrderImportEntity>(sql.ToString(), param);
        }

        /// <summary>
        /// 外部入库-采购单引用View-明细
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VCgOrderDetailEntity> SelectCgOrderDetail(string subOrderNo, string organizeId)
        {
           StringBuilder sql =new StringBuilder(@"
SELECT co.CreateTime OrderDate,co.orderNo,cod.subOrderNo, cpo.cgdh, dept.Name deptName, wz.name wzmc, CONCAT(cod.sl,cod.dwmc) slStr
,convert(decimal(18,2),cod.sl*cod.jj) jjze, wz.gg, ISNULL(wz.brand,'') brand, CONCAT(CONVERT(NUMERIC(11,2),cod.jj),'元/',cod.dwmc) jjStr
,ISNULL(cj.name,'') sccj, ISNULL(gys.name,'') gysmc, ISNULL(cod.remark,'') remark, cod.productId,cod.fph
FROM dbo.cg_order(NOLOCK) co
INNER JOIN dbo.cg_orderDetail(NOLOCK) cod ON cod.orderId=co.Id AND cod.zt='1'
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=cod.productId AND wz.OrganizeId=co.OrganizeId AND wz.zt='1'
LEFT JOIN dbo.cg_purchaseOrderDetail(NOLOCK) cpod ON cpod.Id=cod.pdId AND cpod.zt='1'
LEFT JOIN dbo.cg_purchaseOrder(NOLOCK) cpo ON cpo.Id=cpod.purchaseId AND cpo.OrganizeId=co.OrganizeId AND cpo.zt='1' AND cpo.auditState='1'
LEFT JOIN NewtouchHIS_Base.dbo.Sys_Department(NOLOCK) dept ON dept.Code=cpo.rkbmCode AND dept.OrganizeId=co.OrganizeId AND dept.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON gys.Id=cod.supplierId AND gys.OrganizeId=co.OrganizeId AND gys.supplierType=2 AND gys.zt='1'
LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.OrganizeId=co.OrganizeId AND cj.supplierType=1 AND gys.zt='1'
WHERE co.OrganizeId=@OrganizeId
AND co.zt='1' AND cod.subOrderNo=@subOrderNo ")
;
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@subOrderNo",subOrderNo ),
            };
            return FindList<VCgOrderDetailEntity>(sql.ToString(), param);
        }
        /// <summary>
        /// 采购发票补填
        /// </summary>
        /// <param name="subOrderNo"></param>
        /// <param name="fph"></param>
        /// <param name="userCode"></param>
        public void PurchaseDdbhUpdate(string subOrderNo, string fph, string userCode)
        {
            string sql = @" update [cg_orderDetail] set fph=@fph,LastModifierCode=@userCode,LastModifyTime=GETDATE()
                        where subOrderNo=@subOrderNo ";
            ExecuteSqlCommand(sql, new SqlParameter("@subOrderNo", subOrderNo), new SqlParameter("@fph", fph), new SqlParameter("@userCode", userCode));
        }
        #endregion
    }
}