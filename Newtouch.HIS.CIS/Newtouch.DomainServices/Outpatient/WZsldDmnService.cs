using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.Domain.DTO;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Newtouch.DomainServices
{
    public class WZsldDmnService : DmnServiceBase, IWZsldDmnService
    {
        public WZsldDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }
        #region 作废
        public List<VSelProductInfoVO> DepartmentStockListQuery(DepartmentStockListQueryParamDTO param)
        {
            var sql = new StringBuilder(@"
SELECT [NewtouchHIS_HERP].dbo.f_getComplexWzSlandDw(kykcsl,zhyz,bmdwmc, mindwmc) slstr
,  a.* from (
SELECT
  p.Id,
  p.OrganizeId,
  p.name,
  p.minUnit zxdwId,
   p.minUnit bmdwId,
  bmdw.name bmdwmc,
  zxdw.name mindwmc,
  ISNULL(SUM(kcxx.kcsl), 0) kcsl,
  ISNULL(SUM(kcxx.kcsl - kcxx.djsl), 0) kykcsl,
  p.lsj minlsj,
  ISNULL(CONVERT (NUMERIC (11, 4), p.lsj * rpu.zhyz), 0) bmlsj,
  p.py,
  p.gg,
  p.supplierId,
  s.name supplierName,
  wt.name lbmc,
  p.typeId lbId,
  ISNULL(rpu.zhyz, 1) zhyz,
  ISNULL(rpu.zhyz, 1) bmdwzhyz
FROM
  [NewtouchHIS_HERP]..wz_product (NOLOCK) p
  INNER JOIN [NewtouchHIS_HERP]..gys_supplier (NOLOCK) s ON p.supplierId = s.Id
  AND p.OrganizeId = s.OrganizeId
  AND s.zt = '1'
  LEFT JOIN dbo.Dept_kcxx (NOLOCK) kcxx ON p.Id = kcxx.productId
  AND p.OrganizeId = kcxx.OrganizeId
  AND p.zt = '1'
  LEFT JOIN NewtouchHIS_herp.dbo.rel_productUnit (NOLOCK) rpu ON rpu.productId = p.Id
  AND rpu.zt = '1' --AND rpu.unitId=wz.minUnit
  LEFT JOIN NewtouchHIS_Base.dbo.wz_unit (NOLOCK) zxdw ON zxdw.Id = p.minUnit
  AND zxdw.zt = '1'
  LEFT JOIN NewtouchHIS_Base.dbo.wz_unit (NOLOCK) bmdw ON bmdw.Id = rpu.unitId
  AND bmdw.zt = '1'
  LEFT JOIN NewtouchHIS_Base.dbo.wz_type (NOLOCK) wt ON wt.id = p.typeId
  AND wt.zt = '1'
WHERE
  kcxx.ks = @warehouseId
  AND p.OrganizeId = @OrganizeId
    AND p.zt = @zt
    AND (p.name LIKE '%'+@keyWord+'%' OR p.py LIKE '%'+@keyWord+'%')
");

            sql.Append(@"
   GROUP BY
kcxx.ks,
  p.Id,
  p.OrganizeId,
  p.name,
  p.py,
  p.supplierId,
  s.name,
  p.minUnit,
  p.gg,
  wt.name,
  p.typeId,
  p.lsj,
  rpu.zhyz,
  p.minUnit,
  bmdw.name,
  zxdw.name )a where zhyz = 1
");
            var sqlParam = new DbParameter[]
            {
                new SqlParameter("@warehouseId", param.warehouseId??""),
                new SqlParameter("@OrganizeId", param.organizeId),
                new SqlParameter("@keyWord", param.key??""),
                new SqlParameter("@zt", param.zt)
            };
            return FindList<VSelProductInfoVO>(sql.ToString(), sqlParam);
        }

        public List<RelWarehouseVO> GetDeptList(string organizeId, string keyword)
        {
            string sql = @"select ID,OrganizeId,name,py,isDefSyn from [NewtouchHIS_herp]..[kf_warehouse] (NOLOCK) where zt='1' and isdefsyn='1'
and OrganizeId=@OrganizeId
and (name like '%'+@keyword+'%' or py LIKE '%'+@keyword+'%')";
            var param = new DbParameter[] {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyword", keyword),
            };

            return FindList<RelWarehouseVO>(sql, param);
        }

        public List<RelWarehouseVO> GetList(string organizeId, string keyword)
        {
            string sql = @"select ID,OrganizeId,name,py,isDefSyn from [NewtouchHIS_herp]..[kf_warehouse] (NOLOCK) where zt='1' and isdefsyn='1'
and OrganizeId=@OrganizeId
and (name like '%'+@keyword+'%' or py LIKE '%'+@keyword+'%')";
            var param = new DbParameter[] {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyword", keyword),
            };

            return FindList<RelWarehouseVO>(sql, param);
        }

        public List<VProductBatchInfoVO> ProductBatchQuery(string productId, string warehouseId, string organizeId, string keyword = "")
        {
            string sql = @"
SELECT TOP 20 s.ph,s.pc,s.yxq,s.scrq,s.fph,SUM(s.kcsl) kcsl,SUM(s.kykcsl) kykcsl
,s.bmjj,s.minjj,SUM(s.jjzje) jjzje,CONCAT(CONVERT(NUMERIC(11,2),s.bmjj),'元/',s.bmdwmc) jjdwdj
,NewtouchHIS_HERP.dbo.f_getComplexWzSlandDw(SUM(s.kykcsl),s.zhyz, s.bmdwmc, s.zxdwmc) slstr ,s.zhyz
FROM (
	SELECT kcxx.ph, kcxx.pc, kcxx.yxq, ISNULL(mx.scrq, '') scrq, ISNULL(mx.fph,'') fph
	,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*rpu.zhyz),0) bmjj,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz),0) minjj,ISNULL(CONVERT(NUMERIC(11,2),SUM(kcxx.jj/kcxx.zhyz*(kcxx.kcsl-kcxx.djsl))),0) jjzje
	,ISNULL(SUM(kcxx.kcsl),0) kcsl
	,ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl
	,rpu.zhyz, bmdw.name bmdwmc, zxdw.name zxdwmc
	FROM dbo.dept_kcxx(NOLOCK) kcxx
	INNER JOIN NewtouchHIS_HERP..rel_productUnit(NOLOCK) rpu ON rpu.productId=kcxx.productId AND rpu.OrganizeId=kcxx.OrganizeId AND rpu.zt='1'
	INNER JOIN NewtouchHIS_HERP..wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId AND wz.zt='1'
	INNER JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpu.unitId AND bmdw.zt='1'
	INNER JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
	LEFT JOIN NewtouchHIS_HERP..kf_crkmx(NOLOCK) mx ON mx.Id=kcxx.crkmxId AND mx.zt='1'
	LEFT JOIN NewtouchHIS_HERP..kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.zt='1' AND dj.auditState='1' AND dj.djlx=1
	WHERE kcxx.productId=@proId
	AND kcxx.ks=@warehouseId
	AND kcxx.OrganizeId=@OrganizeId
    AND (kcxx.kcsl-kcxx.djsl)>0
	AND kcxx.zt='1'
    AND (kcxx.pc LIKE '%'+@keyword+'%' OR kcxx.ph LIKE '%'+@keyword+'%')
	GROUP BY kcxx.ph, kcxx.pc, kcxx.yxq,bmdw.name,zxdw.name,kcxx.jj,mx.scrq,mx.fph,rpu.zhyz,kcxx.zhyz
) s WHERE zhyz = 1
GROUP BY s.ph,s.pc,s.yxq,s.scrq,s.fph,s.bmjj,s.minjj,s.zhyz,s.bmdwmc,s.zxdwmc
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyword", (keyword??"").Trim()),
                new SqlParameter("@proId", productId??"")
            };
            return FindList<VProductBatchInfoVO>(sql, param);
        }

        #endregion

        #region 科室物资库存
        public List<WzTypeVo> GetWzTreeSelectJson()
        {
            return FindList<WzTypeVo>("select Id,name,parentId from NewtouchHIS_Base..wz_type where zt='1'");
        }
        /// <summary>
        /// 库存汇总
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="ks"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <param name="lbId"></param>
        /// <param name="xslkc"></param>
        /// <returns></returns>
        public IList<VProductStorageEntity> GetProductStorage(Pagination pagination, string ks, string organizeId, string keyWord, string lbId,string xslkc)
        {
            StringBuilder sqlstr = new StringBuilder(@"
SELECT *,CAST(lsj as varchar)++'元/'+bmdwmc lsjStr FROM (
    SELECT cast(ROW_NUMBER() over(partition by kcxx.productId,ks order by rpu.zhyz desc) as int) num,
    ks,dept.Name ksName,kcxx.productId, wz.name wzmc,wz.py, ISNULL(lb.name,'') lb
    ,NewtouchHIS_herp.dbo.f_getComplexWzSlandDw(SUM(kcxx.kcsl), rpu.zhyz, bmdw.name, zxdw.name) slStr, ISNULL(SUM(kcxx.kcsl),0) zkc
    ,ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*rpu.zhyz),0) lsj, CONVERT(NUMERIC(11,2),SUM(ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*kcxx.kcsl),0))) lsze
    ,CONVERT(NUMERIC(11,2),SUM(ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*kcxx.kcsl),0))) jjze, ISNULL(bmdw.name,'') bmdwmc
    ,wz.gg, ISNULL(wz.brand,'') brand, gys.name sccj,wz.kcyjz,wz.lsj wzlsj,wz.jj wzjj
    FROM dbo.Dept_kcxx(NOLOCK) kcxx
    INNER JOIN NewtouchHIS_herp.dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId
    LEFT JOIN NewtouchHIS_herp.dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=wz.Id AND rpu.zt='1' --AND rpu.unitId=wz.minUnit 
    LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpu.unitId AND bmdw.zt='1'
    LEFT JOIN NewtouchHIS_herp.dbo.gys_supplier(NOLOCK) gys ON gys.Id=wz.supplierId AND gys.zt='1' AND gys.OrganizeId=wz.OrganizeId
	LEFT JOIN NewtouchHIS_Base..Sys_Department (NOLOCK) dept on dept.Code=kcxx.ks and dept.OrganizeId=kcxx.OrganizeId
    WHERE kcxx.OrganizeId=@organizeId 
      AND (wz.name LIKE '%'+ @keyword +'%' OR wz.py LIKE '%'+ @keyword +'%')
      AND (wz.typeId=@lbId OR ''=@lbId)
");
            if (!string.IsNullOrWhiteSpace(ks))
            {
                sqlstr.AppendLine("AND kcxx.ks=@ks ");
            }
            sqlstr.AppendLine(@"
    GROUP BY kcxx.ks,dept.Name,kcxx.productId,wz.name,wz.py,lb.name,wz.lsj,rpu.zhyz,zxdw.name,bmdw.name,wz.gg,wz.brand,gys.name,wz.kcyjz,wz.lsj,wz.jj
) a 
WHERE  num=1 ");
            var param = new DbParameter[]
           {
                new SqlParameter("@ks", ks ?? ""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@lbId", lbId ?? ""),
                new SqlParameter("@keyword", keyWord??"")
           };
            if (!string.IsNullOrWhiteSpace(xslkc) && "0".Equals(xslkc.Trim()))
            {
                sqlstr.AppendLine("AND a.zkc<>0 ");
            }
            return QueryWithPage<VProductStorageEntity>(sqlstr.ToString(), pagination, param);

        }
        public IList<VProductStorageEntity> GetExpiredProductStorage(Pagination pagination, string ks, string organizeId, string keyWord, string lbId,string xslkc)
        {
            StringBuilder sqlstr = new StringBuilder(@"
SELECT *,CAST(lsj as varchar)++'元/'+bmdwmc lsjStr FROM (
    SELECT cast(ROW_NUMBER() over(partition by kcxx.productId,ks order by rpu.zhyz desc) as int) num,
    ks,dept.Name ksName,kcxx.productId, wz.name wzmc,wz.py, ISNULL(lb.name,'') lb
    ,NewtouchHIS_herp.dbo.f_getComplexWzSlandDw(SUM(kcxx.kcsl), rpu.zhyz, bmdw.name, zxdw.name) slStr, ISNULL(SUM(kcxx.kcsl),0) zkc
    ,ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*rpu.zhyz),0) lsj, CONVERT(NUMERIC(11,2),SUM(ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*kcxx.kcsl),0))) lsze
    ,CONVERT(NUMERIC(11,2),SUM(ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*kcxx.kcsl),0))) jjze, ISNULL(bmdw.name,'') bmdwmc
    ,wz.gg, ISNULL(wz.brand,'') brand, gys.name sccj,wz.kcyjz,wz.lsj wzlsj,wz.jj wzjj, kcxx.yxq,kcxx.pc,kcxx.ph
    FROM dbo.Dept_kcxx(NOLOCK) kcxx
    INNER JOIN NewtouchHIS_herp.dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId
    LEFT JOIN NewtouchHIS_herp.dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=wz.Id AND rpu.zt='1' --AND rpu.unitId=wz.minUnit 
    LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpu.unitId AND bmdw.zt='1'
    LEFT JOIN NewtouchHIS_herp.dbo.gys_supplier(NOLOCK) gys ON gys.Id=wz.supplierId AND gys.zt='1' AND gys.OrganizeId=wz.OrganizeId
	LEFT JOIN NewtouchHIS_Base..Sys_Department (NOLOCK) dept on dept.Code=kcxx.ks and dept.OrganizeId=kcxx.OrganizeId
    WHERE kcxx.OrganizeId=@organizeId 
      AND (wz.name LIKE '%'+ @keyword +'%' OR wz.py LIKE '%'+ @keyword +'%')
      AND (wz.typeId=@lbId OR ''=@lbId)
");
            if (!string.IsNullOrWhiteSpace(ks))
            {
                sqlstr.AppendLine("AND kcxx.ks=@ks ");
            }
            sqlstr.AppendLine(@"
    GROUP BY kcxx.ks,dept.Name,kcxx.productId,wz.name,wz.py,lb.name,wz.lsj,rpu.zhyz,zxdw.name,bmdw.name,wz.gg,wz.brand,gys.name,wz.kcyjz,wz.lsj,wz.jj,kcxx.yxq,kcxx.pc,kcxx.ph
) a 
WHERE  num=1 and yxq < GETDATE() ");
            var param = new DbParameter[]
           {
                new SqlParameter("@ks", ks ?? ""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@lbId", lbId ?? ""),
                new SqlParameter("@keyword", keyWord??"")
           };
            if (!string.IsNullOrWhiteSpace(xslkc) && "0".Equals(xslkc.Trim()))
            {
                sqlstr.AppendLine("AND a.zkc<>0 ");
            }
            return QueryWithPage<VProductStorageEntity>(sqlstr.ToString(), pagination, param);

        }
        
        /// <summary>
        /// 库存批次批号明细
        /// </summary>
        /// <param name="ks"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="proId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public IList<VProductStorageDetailEntity> GetProductStorageDetail(string ks, string OrganizeId, string proId, string zt)
        {
            const string sql= @" SELECT *,cast(jj as varchar)+'元/'+bmdwmc jjStr,CAST(lsj as varchar)++'元/'+bmdwmc lsjStr  FROM (
	SELECT cast(ROW_NUMBER() over(partition by wz.Id,ph,pc order by rpu.zhyz desc) as int) num, kcxx.Id,wz.Id productId
	,wz.name wzmc, kcxx.ph, kcxx.pc, kcxx.yxq, kcxx.zt, bmdw.name bmdwmc,rpu.zhyz,zxdw.name zxdwmc
	,NewtouchHIS_herp.dbo.f_getComplexWzSlandDw((kcxx.djsl),rpu.zhyz,bmdw.name,zxdw.name) bmdjslStr
	,NewtouchHIS_herp.dbo.f_getComplexWzSlandDw((kcxx.kcsl),rpu.zhyz,bmdw.name,zxdw.name) bmkcslStr
	,ISNULL((kcxx.djsl),0) djsl,ISNULL((kcxx.kcsl),0) kcsl
	,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*rpu.zhyz),0) jj, ISNULL(CONVERT(NUMERIC(11,2),(kcxx.jj/kcxx.zhyz*kcxx.kcsl)),0) jjze
	,CONVERT(NUMERIC(11,4),wz.lsj*rpu.zhyz) lsj,CONVERT(NUMERIC(11,2),(wz.lsj*kcxx.kcsl)) lsze
	FROM dbo.Dept_kcxx(NOLOCK) kcxx
	LEFT JOIN NewtouchHIS_herp.dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId
	LEFT JOIN NewtouchHIS_herp.dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=wz.Id --AND rpu.unitId=wz.minUnit AND rpu.OrganizeId=kcxx.OrganizeId AND rpu.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpu.unitId AND bmdw.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND bmdw.zt='1'
	WHERE kcxx.ks=@ks 
	AND kcxx.OrganizeId=@OrganizeId 
	AND kcxx.productId=@proId
	AND (kcxx.zt=@zt OR ''=@zt)
) mx where num=1";

            var param = new DbParameter[]
           {
                new SqlParameter("@ks", ks),
                new SqlParameter("@OrganizeId", OrganizeId),
                new SqlParameter("@proId", proId ),
                new SqlParameter("@zt",zt??"")
           };
            return FindList<VProductStorageDetailEntity>(sql, param);
        }
        /// <summary>
        /// 变更库存可用状态
        /// </summary>
        /// <param name="mxId"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public  int UpdateZt(string mxId,  string OrganizeId, string zt)
        {
            return ExecuteSqlCommand("update Dept_kcxx set zt=@zt where Id=@mxId and OrganizeId=@OrganizeId",  new SqlParameter("@OrganizeId", OrganizeId), new SqlParameter("@zt", zt)
                , new SqlParameter("@mxId", mxId));

        }
        /// <summary>
        /// 同步物资耗材
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="userCode"></param>
        public void UpdateSyncWz(string OrganizeId, string userCode)
        {
            ExecuteSqlCommand("exec NewtouchHIS_herp..SynchDeptProduct_CIS '',@OrganizeId,@userCode", new SqlParameter("@OrganizeId", OrganizeId), new SqlParameter("@userCode", userCode));
        }
        #endregion
    }
}
