using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.IDomainServices.DeptStorageManage;
using Newtouch.Domain.ValueObjects.Storage;
using Newtouch.Infrastructure;

namespace Newtouch.DomainServices.DeptStorageManage
{
    /// <summary>
    /// 库存结转
    /// </summary>
    public class KcKcjzDmnService : DmnServiceBase, IKcKcjzDmnService
    {
        public KcKcjzDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// 获取结转明细
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="jzsj"></param>
        /// <param name="keyWord"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VCarryOverDetailEntity> SelectCarryOverDetail(Pagination pagination, string jzsj, string keyWord,
            string warehouseId, string organizeId)
        {
            const string sql = @"
 select * from ( SELECT  CAST
  (ROW_NUMBER () OVER (partition BY jz.productId, jz.warehouseId ORDER BY rpu.zhyz DESC) AS INT) num,
  wz.name wzmc,
  jz.productId,
  wz.gg,
  NewtouchHIS_Herp.dbo.f_getComplexWzSlandDw (jz.kcsl, jz.zhyz, zxdw.name, zxdw.name) jzsl,
  jz.pc,
  jz.ph,
  gys.name supplierName,
  ISNULL(CONVERT (NUMERIC (11, 4), wz.lsj * rpu.zhyz), 0) lsj,
  ISNULL(CONVERT (NUMERIC (11, 4), wz.lsj * jz.kcsl), 0) lsze,
  zxdw.name bmdwmc,
  jz.kcsl,
  jz.CreateTime
FROM
  dbo.dept_kcjz (NOLOCK) jz
  INNER JOIN NewtouchHIS_Herp.dbo.wz_product (NOLOCK) wz ON wz.Id = jz.productId
  AND wz.OrganizeId = jz.OrganizeId
  AND wz.zt = '1'
  LEFT JOIN NewtouchHIS_Herp.dbo.rel_productUnit (NOLOCK) rpu ON rpu.productId = jz.productId
  AND rpu.OrganizeId = jz.OrganizeId
  AND rpu.zt = '1'
  LEFT JOIN NewtouchHIS_Base.dbo.wz_unit (NOLOCK) zxdw ON zxdw.Id = wz.minUnit
  AND zxdw.zt = '1'
  LEFT JOIN NewtouchHIS_herp.dbo.gys_supplier (NOLOCK) gys ON gys.Id = wz.supplierId
  AND gys.OrganizeId = jz.OrganizeId
  AND gys.zt = '1'   
WHERE jz.warehouseId=@warehouseId
AND jz.OrganizeId=@OrganizeId
AND (CONVERT(VARCHAR(19), jz.jzsj, 120)=@jzsj OR ''=@jzsj)
AND (wz.name LIKE '%'+@keyWord+'%' OR wz.py LIKE '%'+@keyWord+'%')) a 
WHERE  a.num=1
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@jzsj", jzsj),
                new SqlParameter("@keyWord", keyWord ?? "")
            };
            return QueryWithPage<VCarryOverDetailEntity>(sql, pagination, param);
        }

        /// <summary>
        /// 结转
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string CarryOverProduct(string warehouseId, string organizeId, string userCode)
        {
            const string sql = @"
IF OBJECT_ID(N'tempdb.dbo.#tmpData', N'U') IS NOT NULL
BEGIN
	DROP TABLE #tmpData;
END

SELECT NEWID() Id, kcxx.productId, kcxx.OrganizeId, kcxx.ks as warehouseId , kcxx.ph, kcxx.pc, kcxx.yxq, kcxx.kcsl, ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*kcxx.zhyz),0) bmlsj, kcxx.jj, kcxx.zhyz
INTO #tmpData
FROM dbo.dept_kcxx(NOLOCK) kcxx
INNER JOIN NewtouchHIS_herp.dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId AND wz.zt='1'
WHERE kcxx.OrganizeId=@OrganizeId
AND kcxx.ks=@warehouseId
AND kcxx.zt='1'

DECLARE @dateNow DATETIME;
SET @dateNow=GETDATE();
BEGIN TRY
	BEGIN TRANSACTION
	WHILE EXISTS(SELECT 1 FROM #tmpData)
	BEGIN
		IF OBJECT_ID(N'tempdb..#tmpItems', N'U') IS NOT NULL
		BEGIN
			DROP TABLE #tmpItems;
		END
		SELECT TOP 50 * INTO #tmpItems FROM #tmpData;
		INSERT INTO dbo.dept_kcjz( Id ,OrganizeId ,warehouseId ,productId ,ph ,pc ,yxq ,kcsl ,bmlsj ,jj ,zhyz ,jzsj ,zt ,px ,CreatorCode ,CreateTime ,LastModifyTime ,LastModifierCode)
						  SELECT Id, OrganizeId, warehouseId, productId, ph, pc, yxq, kcsl, bmlsj, jj, zhyz, @dateNow jzsj,'1' zt,NULL px,@userCode CreatorCode,@dateNow CreateTime,NULL LastModifyTime,NULL LastModifierCode FROM #tmpItems ;
		
		DELETE FROM #tmpData WHERE Id IN (SELECT Id FROM #tmpItems);
	END
	COMMIT TRANSACTION
	SELECT '';
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
	SELECT ERROR_MESSAGE();
END CATCH 
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@userCode", userCode)
            };
            return FindList<string>(sql, param).FirstOrDefault();
        }

        /// <summary>
        /// 进销存统计
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="searchParam"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public IList<VPsiStatisticsEntity> GetPsiStatistics(Pagination pagination, PsiStatisticsDTO searchParam, string warehouseId, string organizeId)
        {
            var sql = new StringBuilder(@"
SELECT rpw.productId, wz.name wzmc, lb.name lbmc, wz.gg, bmdw.name bmdwmc
,dbo.f_getComplexWzSlandDw(qc.qckcsl, rpu.zhyz, bmdw.name, zxdw.name) qcsl
,dbo.f_getComplexWzSlandDw(rk.rkkcsl, rpu.zhyz, bmdw.name, zxdw.name) rksl
,dbo.f_getComplexWzSlandDw(ck.ckkcsl, rpu.zhyz, bmdw.name, zxdw.name) cksl
,dbo.f_getComplexWzSlandDw(sy.sysl, rpu.zhyz, bmdw.name, zxdw.name) sysl
,dbo.f_getComplexWzSlandDw(qm.qmkcsl, rpu.zhyz, bmdw.name, zxdw.name) qmsl
,qc.lsze qclsze, rk.lsze rklsze, ck.lsze cklsze, sy.lsze sylsze, qm.lsze qmlsze, tjsy.tjsyze
FROM dbo.rel_productWarehouse(NOLOCK) rpw 
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw.productId AND wz.OrganizeId=rpw.OrganizeId
LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpw.unitId AND bmdw.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=rpw.OrganizeId AND rpu.zt='1'
LEFT JOIN (
	SELECT rpw2.productId, ISNULL(SUM(jz.kcsl), 0) qckcsl, CONVERT(DECIMAL(11, 2), ISNULL(SUM(jz.bmlsj/jz.zhyz*jz.kcsl), 0)) lsze 
	FROM dbo.rel_productWarehouse(NOLOCK) rpw2 
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw2.productId AND wz.OrganizeId=rpw2.OrganizeId
	LEFT JOIN dbo.kc_kcjz(NOLOCK) jz ON jz.productId=rpw2.productId AND jz.OrganizeId=rpw2.OrganizeId AND CONVERT(VARCHAR(19),jz.jzsj,120)=@kssj AND jz.zt='1' AND jz.warehouseId=rpw2.warehouseId
	WHERE rpw2.warehouseId=@warehouseId
	AND rpw2.OrganizeId=@OrganizeId
	GROUP BY rpw2.productId
) qc ON qc.productId=rpw.productId
LEFT JOIN (
	SELECT rpw2.productId, ISNULL(SUM(rkdjmx.sl*rkdjmx.zhyz), 0) rkkcsl, CONVERT(DECIMAL(11, 2), ISNULL(SUM(rkdjmx.lsj*rkdjmx.sl), 0)) lsze 
	FROM dbo.rel_productWarehouse(NOLOCK) rpw2 
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw2.productId AND wz.OrganizeId=rpw2.OrganizeId
	LEFT JOIN (
		SELECT mx.productId, mx.sl, mx.zhyz, mx.lsj
		FROM dbo.kf_crkmx(NOLOCK) mx
		INNER JOIN dbo.kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.OrganizeId=@OrganizeId
		WHERE dj.auditState=@shzt
		AND dj.rkbm=@warehouseId
		AND dj.rksj BETWEEN CONVERT(DATETIME, @kssj ) AND CONVERT(DATETIME, @jssj)
		AND dj.zt='1'
		AND mx.zt='1'
	) rkdjmx ON rkdjmx.productId=rpw2.productId
	WHERE rpw2.OrganizeId=@OrganizeId
	AND rpw2.warehouseId=@warehouseId
	GROUP BY rpw2.productId
) rk ON rk.productId=rpw.productId
LEFT JOIN (
	SELECT rpw2.productId, ISNULL(SUM(ckdjmx.sl*ckdjmx.zhyz), 0) ckkcsl, CONVERT(DECIMAL(11, 2), ISNULL(SUM(ckdjmx.lsj*ckdjmx.sl), 0)) lsze 
	FROM dbo.rel_productWarehouse(NOLOCK) rpw2 
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw2.productId AND wz.OrganizeId=rpw2.OrganizeId
	LEFT JOIN (
		SELECT mx.productId, mx.sl, mx.zhyz, mx.lsj
		FROM dbo.kf_crkmx(NOLOCK) mx
		INNER JOIN dbo.kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.OrganizeId=@OrganizeId
		WHERE dj.auditState=@shzt
		AND dj.ckbm=@warehouseId
		AND dj.cksj BETWEEN CONVERT(DATETIME, @kssj ) AND CONVERT(DATETIME, @jssj)
		AND dj.zt='1'
		AND mx.zt='1'
	) ckdjmx ON ckdjmx.productId=rpw2.productId
	WHERE rpw2.OrganizeId=@OrganizeId
	AND rpw2.warehouseId=@warehouseId
	GROUP BY rpw2.productId
) ck ON ck.productId=rpw.productId
LEFT JOIN (
	SELECT rpw2.productId, ISNULL(SUM(syxx.Sysl), 0) sysl, CONVERT(DECIMAL(11, 2), ISNULL(SUM(syxx.Lsj/syxx.Zhyz*syxx.Sysl), 0)) lsze 
	FROM dbo.rel_productWarehouse(NOLOCK) rpw2 
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw2.productId AND wz.OrganizeId=rpw2.OrganizeId 
	LEFT JOIN dbo.kc_syxx(NOLOCK) syxx ON syxx.productId=rpw2.productId AND syxx.OrganizeId=rpw2.OrganizeId AND syxx.warehouseId=rpw2.warehouseId AND syxx.Bgsj BETWEEN CONVERT(DATETIME, @kssj ) AND CONVERT(DATETIME, @jssj) AND syxx.zt='1'
	WHERE rpw2.warehouseId=@warehouseId
	AND rpw2.OrganizeId=@OrganizeId
	GROUP BY rpw2.productId
) sy ON sy.productId=rpw.productId ");
            if (string.IsNullOrWhiteSpace(searchParam.jsjzsj))
            {
                sql.Append(@"
LEFT JOIN (
	SELECT rpw2.productId, ISNULL(SUM(kcxx.kcsl), 0) qmkcsl, CONVERT(DECIMAL(11, 2), ISNULL(SUM(wz.lsj*kcxx.kcsl), 0)) lsze 
	FROM dbo.rel_productWarehouse(NOLOCK) rpw2 
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw2.productId AND wz.OrganizeId=rpw2.OrganizeId 
	LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=rpw2.productId AND kcxx.OrganizeId=rpw2.OrganizeId AND kcxx.warehouseId=rpw2.warehouseId AND kcxx.zt='1'
	WHERE rpw2.warehouseId=@warehouseId
	AND rpw2.OrganizeId=@OrganizeId
	GROUP BY rpw2.productId
) qm ON qm.productId=rpw.productId ");
            }
            else
            {
                sql.Append(@"
LEFT JOIN(
SELECT rpw2.productId, ISNULL(SUM(jz2.kcsl), 0) qmkcsl, CONVERT(DECIMAL(11, 2), ISNULL(SUM(jz2.bmlsj / jz2.zhyz * jz2.kcsl), 0)) lsze
  FROM dbo.rel_productWarehouse(NOLOCK) rpw2
  INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id = rpw2.productId AND wz.OrganizeId = rpw2.OrganizeId
  LEFT JOIN dbo.kc_kcjz(NOLOCK) jz2 ON jz2.productId = rpw2.productId AND jz2.OrganizeId = rpw2.OrganizeId AND jz2.warehouseId = rpw2.warehouseId AND CONVERT(VARCHAR(19), jz2.jzsj, 120) = @jssj AND jz2.zt = '1'
  WHERE rpw2.warehouseId = @warehouseId
  AND rpw2.OrganizeId = @OrganizeId
  GROUP BY rpw2.productId
) qm ON qm.productId = rpw.productId ");
            }
            sql.Append(@"
LEFT JOIN (
	SELECT rpw2.productId, CONVERT(DECIMAL(11, 2), ISNULL(SUM(tjsy.lsjtjlr), 0)) tjsyze
	FROM dbo.rel_productWarehouse(NOLOCK) rpw2 
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw2.productId AND wz.OrganizeId=rpw2.OrganizeId 
	LEFT JOIN dbo.wz_priceAdjustmentProfitAndLoss(NOLOCK) tjsy ON tjsy.productId=rpw2.productId AND tjsy.OrganizeId=rpw2.OrganizeId AND tjsy.warehouseId=rpw2.warehouseId AND tjsy.tjsj BETWEEN CONVERT(DATETIME, @kssj) AND CONVERT(DATETIME, @jssj) AND tjsy.zt='1'
	WHERE rpw2.warehouseId=@warehouseId
	AND rpw2.OrganizeId=@OrganizeId
	GROUP BY rpw2.productId 
) tjsy ON tjsy.productId=rpw.productId
WHERE rpw.OrganizeId=@OrganizeId
AND rpw.warehouseId=@warehouseId
AND (wz.zt=@zt OR ''=@zt)
AND (wz.name LIKE '%'+@keyWord+'%' OR wz.py LIKE '%'+@keyWord+'%')
AND (wz.typeId=@lbId OR ''=@lbId) 
");
            if (string.IsNullOrWhiteSpace(searchParam.noPSI) || "0".Equals(searchParam.noPSI))
            {
                sql.AppendLine("AND (rk.rkkcsl<>0 OR ck.ckkcsl<>0 OR sy.sysl<>0 ) ");
            }
            var param = new DbParameter[]
            {
                    new SqlParameter("@OrganizeId", organizeId),
                    new SqlParameter("@warehouseId", warehouseId),
                    new SqlParameter("@zt", searchParam.wzzt ?? ""),
                    new SqlParameter("@keyWord", searchParam.srm ?? ""),
                    new SqlParameter("@lbId", searchParam.dl ?? ""),
                    new SqlParameter("@kssj", searchParam.ksjzsj??""),
                    new SqlParameter("@jssj", searchParam.jsjzsj??DateTime.Now.ToString(Constants.DateTimeFormat)),
                    new SqlParameter("@shzt", ((int) EnumAuditState.Adopt).ToString())
            };
            return QueryWithPage<VPsiStatisticsEntity>(sql.ToString(), pagination, param);
        }
    }
}
