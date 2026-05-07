using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Utils;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Domain.IRepository;
using Newtouch.Herp.Domain.ValueObjects;
using Newtouch.Herp.Infrastructure;
using Newtouch.Herp.Infrastructure.Enum;
using Newtouch.Herp.Infrastructure.Log;

namespace Newtouch.Herp.DomainServices.Product
{
    /// <summary>
    /// 物资操作
    /// </summary>
    public class WzProductDmnService : DmnServiceBase, IWzProductDmnService
    {
        private readonly IWzProductRepo wzProductRepo;
        private readonly IKfWarehouseRepo kfWarehouseRepo;
        private readonly IRelProductWarehouseRepo _relProductWarehouseRepo;

        public WzProductDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        /// <summary>
        /// get product list
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public IList<VWzProductEntity> GetList(Pagination pagination, string organizeId, string lb, string zt, string keyWord)
        {
            string sql = @"
SELECT product.[Id],product.[OrganizeId],product.[name], wzdw.name minUnit,product.[typeId],wzt.name typeName
,product.[zczh],product.[py],product.[imageUrl],product.[brand],product.[gg],product.[supplierId]
,supplier.name supplierName,product.[zxqds],product.[jj],product.[lsj]
,product.[sflkc],product.[sffy],product.[sfgt],product.[zt],product.[CreatorCode],product.[CreateTime]
,product.[LastModifyTime],product.[LastModifierCode],product.gjybdm,product.kcyjz
FROM [dbo].[wz_product](NOLOCK) product
LEFT JOIN dbo.gys_supplier(NOLOCK) supplier ON supplier.Id=product.supplierId AND supplier.OrganizeId=product.OrganizeId AND supplier.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) wzt ON wzt.Id=product.typeId AND wzt.zt='1'
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) wzdw ON wzdw.Id=product.minUnit AND wzdw.zt='1'
WHERE product.OrganizeId=@OrganizeId
AND (product.name LIKE '%'+@keyWord+'%' OR product.py LIKE '%'+@keyWord+'%')
AND (product.zt=@zt OR ''=@zt ) 
";

            if (!string.IsNullOrEmpty(lb))
            {
                sql += " AND wzt.Id = @lb";
            }
            var param = new DbParameter[]
            {
                new SqlParameter("@keyWord", keyWord??""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@lb", lb),
                new SqlParameter("@zt", zt)
            };
            return QueryWithPage<VWzProductEntity>(sql, pagination, param);
        }

        /// <summary>
        /// 根据ID删除物资信息
        /// </summary>
        /// <param name="id"></param>
        public void DeleteProduct(string id)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (DeleteProductToSfxm(id) <= 0)
                {
                    throw new Exception("删除对应的收费项目时失败");
                }
                db.Delete<RelProductWarehouseEntity>(p => p.productId == id);
                db.Delete<RelProductUnitEntity>(p => p.productId == id);
                db.Delete<WzProductEntity>(p => p.Id == id);
                db.Commit();
            }
        }

        /// <summary>
        /// 根据物资单位关联关系ID获取物资和单位信息
        /// </summary>
        /// <param name="relId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VProductUnitEntity> GetProductAndUnitByProId(string relId, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT * 
FROM (
	SELECT rel.productId, pro.name productName, dw.Id unitId, dw.name unitName, pro.zt productZt, dw.zt unitZt 
	FROM dbo.wz_product(NOLOCK) pro
	INNER JOIN dbo.rel_productWarehouse(NOLOCK) relpw ON relpw.productId=pro.Id AND relpw.OrganizeId=pro.OrganizeId
	INNER JOIN dbo.rel_productUnit(NOLOCK) rel ON rel.productId=pro.Id AND rel.OrganizeId=pro.OrganizeId AND rel.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) dw ON dw.Id=rel.unitId
	WHERE pro.OrganizeId=@OrganizeId
	AND relpw.Id=@relId 
	UNION ALL
	SELECT pro.Id productId, pro.name productName, zxdw.Id unitId, zxdw.name unitName, pro.zt productZt, zxdw.zt unitZt
	FROM dbo.wz_product(NOLOCK) pro
	INNER JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=pro.minUnit AND zxdw.zt='1'
	INNER JOIN dbo.rel_productWarehouse(NOLOCK) relpw ON relpw.productId=pro.Id AND relpw.OrganizeId=pro.OrganizeId
	WHERE pro.OrganizeId=@OrganizeId
	AND relpw.Id=@relId 
) d
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@relId", relId??"")
            };
            return FindList<VProductUnitEntity>(sql, param);
        }

        /// <summary>
        /// 获取本部门物资类别
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public List<VBmwzlbEntity> GetBmWzLb(string warehouseId, string organizeId, string zt = "1")
        {
            const string sql = @"
SELECT DISTINCT lb.Id, lb.name lbmc
FROM dbo.rel_productWarehouse(NOLOCK) relpw
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=relpw.productId AND wz.OrganizeId=relpw.OrganizeId
INNER JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId
WHERE (lb.zt=@zt or ''=@zt)
AND relpw.OrganizeId=@OrganizeId
AND relpw.warehouseId=@warehouseId
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@zt", zt)
            };
            return FindList<VBmwzlbEntity>(sql, param);
        }

        /// <summary>
        /// 修改物资价格
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lsj"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public int UpdateProductPrice(string id, decimal lsj, string userCode, string organizeId, string zt = "1")
        {
            const string sql = @"
UPDATE dbo.wz_product SET lsj=@lsj, LastModifierCode=@userCode, LastModifyTime=GETDATE()
WHERE Id=@proId AND OrganizeId=@OrganizeId AND zt=@zt
SELECT @@ROWCOUNT;
";
            var param = new DbParameter[]
            {
                new SqlParameter("@proId", id),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@lsj", lsj),
                new SqlParameter("@userCode", userCode),
                new SqlParameter("@zt", zt)
            };
            return FindList<int>(sql, param).FirstOrDefault();
        }

        /// <summary>
        /// 获取物资信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public VProductInfoEntity GetProductInfo(string id, string organizeId)
        {
            const string sql = @"
SELECT wz.[Id],wz.[OrganizeId],wz.[name],wz.[typeId],wz.[zczh],wz.[py],wz.[imageUrl],wz.[brand],wz.[gg],
wz.[supplierId],gys.name supplierName,wz.[minUnit],wz.[zxqds],wz.[jj],wz.[lsj],wz.[sflkc],wz.[sffy],wz.[sfgt],
wz.[zt],wz.[CreatorCode],wz.[CreateTime],wz.[LastModifyTime],wz.[LastModifierCode],wz.[productCode],
wz.[hcgjybdm],wz.[iswzsame],wz.[zfxz],wz.[zfbl],wz.[gjybdm],wz.[ybdm],wz.[zblb],wz.[hslb],wz.kcyjz
FROM [NewtouchHIS_herp].[dbo].[wz_product](NOLOCK) wz
LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON wz.supplierId=gys.Id AND gys.zt='1'
WHERE wz.OrganizeId=@OrganizeId
AND wz.Id=@Id
";
            var param = new DbParameter[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@OrganizeId", organizeId),
            };
            return FirstOrDefault<VProductInfoEntity>(sql, param);
        }

        #region 首页统计

        /// <summary>
        /// 获取待处理事件总数
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public NeedDealCountVO GetNeedDealCountByZkf(string warehouseId, string organizeId)
        {
            try
            {
                var sql = new StringBuilder("DECLARE @tjshCount BIGINT, @wbrkCount BIGINT, @nbthCount BIGINT, @ckdshCount BIGINT, @expriedWzCount BIGINT;");
                sql.AppendLine("--调价审核 ");
                sql.AppendLine("SELECT @tjshCount=COUNT(wztjId) FROM dbo.wz_priceAdjustment(NOLOCK) WHERE shzt='" + (int)EnumTjShzt.Waiting + "' AND warehouseId=@warehouseId AND OrganizeId=@OrganizeId AND zt='" + (int)Enumzt.Enable + "' ");
                sql.AppendLine("--外部入库待审核  ");
                sql.AppendLine("SELECT @wbrkCount=COUNT(Id) FROM (  ");
                sql.AppendLine("SELECT DISTINCT dj.Id FROM dbo.kf_crkdj(NOLOCK) dj ");
                sql.AppendLine("INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id ");
                sql.AppendLine("WHERE dj.Rkbm=@warehouseId AND dj.OrganizeId=@OrganizeId AND dj.zt='" + (int)Enumzt.Enable + "' AND dj.auditState='" + (int)EnumAuditState.Waiting + "' AND dj.djlx=" + (int)EnumOutOrInStorageBillType.Wbrk);
                sql.AppendLine(") a ");
                sql.AppendLine("--内部退货待审核 ");
                sql.AppendLine("SELECT @nbthCount=COUNT(Id) FROM ( ");
                sql.AppendLine("SELECT DISTINCT dj.Id FROM dbo.kf_crkdj(NOLOCK) dj ");
                sql.AppendLine("INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id ");
                sql.AppendLine("WHERE dj.Rkbm=@warehouseId AND dj.OrganizeId=@OrganizeId AND dj.zt='" + (int)Enumzt.Enable + "' AND dj.auditState='" + (int)EnumAuditState.Waiting + "' AND dj.djlx=" + (int)EnumOutOrInStorageBillType.Nbth);
                sql.AppendLine(") a ");
                sql.AppendLine("--出库待审核 ");
                sql.AppendLine("SELECT @ckdshCount=COUNT(Id) FROM ( ");
                sql.AppendLine("SELECT DISTINCT dj.Id ");
                sql.AppendLine("FROM dbo.kf_crkdj(NOLOCK) dj ");
                sql.AppendLine("INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id ");
                sql.AppendLine("WHERE dj.Ckbm=@warehouseId AND dj.OrganizeId=@OrganizeId AND dj.zt='" + (int)Enumzt.Enable + "' AND dj.auditState='" + (int)EnumAuditState.Waiting + "' and dj.djlx=" + (int)EnumOutOrInStorageBillType.Wbck);
                sql.AppendLine(") a ");
                sql.AppendLine("--过期药品数量 ");
                sql.AppendLine("SELECT @expriedWzCount=COUNT(a.Id) FROM ( ");
                sql.AppendLine("	SELECT DISTINCT a.Id,a.ph,a.pc FROM dbo.kf_kcxx(NOLOCK) a ");
                sql.AppendLine("    INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.productId=a.productId AND rpw.warehouseId=a.warehouseId AND rpw.OrganizeId=a.OrganizeId AND rpw.zt='1' ");
                sql.AppendLine("    WHERE a.kcsl > 0 AND a.yxq < GETDATE() AND a.warehouseId=@warehouseId AND a.OrganizeId=@OrganizeId AND a.zt='" + (int)Enumzt.Enable + "' ");
                sql.AppendLine(") a ");
                sql.AppendLine("SELECT ISNULL(@tjshCount,0) tjshCount,ISNULL(@wbrkCount,0) wbrkCount,ISNULL(@nbthCount,0) nbthCount,ISNULL(@ckdshCount,0) ckdshCount,ISNULL(@expriedWzCount,0) expriedWzCount ");
                var param = new DbParameter[]
                {
                    new SqlParameter("@warehouseId",warehouseId??""),
                    new SqlParameter("@OrganizeId", organizeId??"")
                };
                return FindList<NeedDealCountVO>(sql.ToString(), param).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    { Constants.KfId, warehouseId},
                    { Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        /// <summary>
        /// 获取待处理事件总数
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public NeedDealCountVO GetNeedDealCountByKskf(string warehouseId, string organizeId)
        {
            try
            {
                var ago = -31;
                var td = ConfigurationHelper.GetAppConfigValue("tjwzDaysAgo");
                int.TryParse(td, out ago);
                var sql = new StringBuilder();
                sql.AppendLine("DECLARE @tjwzCount BIGINT, @rkdshCount BIGINT, @expriedWzCount BIGINT; ");
                sql.AppendLine("--调价物资 ");
                sql.AppendLine("SELECT @tjwzCount=COUNT(wztjId) FROM ( ");
                sql.AppendLine("SELECT DISTINCT tj.wztjId ");
                sql.AppendLine("FROM dbo.wz_priceAdjustment(NOLOCK) tj ");
                sql.AppendLine("INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.productId=tj.productId AND rpw.warehouseId=tj.warehouseId AND rpw.zt='" + (int)Enumzt.Enable + "' ");
                sql.AppendLine("WHERE tj.zxbz='" + (int)EnumTjZxbz.Already + "' and tj.warehouseId=@warehouseId AND tj.OrganizeId=@OrganizeId AND tj.zt='" + (int)Enumzt.Enable + "' AND tj.zxsj>=DATEADD(DAY, " + ago + ", GETDATE()) ");
                sql.AppendLine(") a ");
                sql.AppendLine("--入库审核 ");
                sql.AppendLine("SELECT @rkdshCount=COUNT(Id) FROM ( ");
                sql.AppendLine("SELECT DISTINCT dj.Id FROM dbo.kf_crkdj(NOLOCK) dj ");
                sql.AppendLine("INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id ");
                sql.AppendLine("WHERE dj.Rkbm=@warehouseId AND dj.OrganizeId=@OrganizeId AND dj.zt='" + (int)Enumzt.Enable + "' AND dj.auditState='" + (int)EnumAuditState.Waiting + "' ");
                sql.AppendLine(") a ");
                sql.AppendLine("--过期物资数量 ");
                sql.AppendLine("SELECT @expriedWzCount=COUNT(a.Id) FROM ( ");
                sql.AppendLine("	SELECT DISTINCT a.Id FROM dbo.kf_kcxx(NOLOCK) a WHERE a.kcsl > 0 AND a.yxq < GETDATE() AND a.warehouseId=@warehouseId AND a.OrganizeId=@OrganizeId AND a.zt='" + (int)Enumzt.Enable + "' ");
                sql.AppendLine(") a ");
                sql.AppendLine("SELECT ISNULL(@tjwzCount,0) tjwzCount, ISNULL(@rkdshCount,0) rkdshCount, ISNULL(@expriedWzCount,0) expriedWzCount ");
                var param = new DbParameter[]
                {
                    new SqlParameter("@warehouseId",warehouseId??""),
                    new SqlParameter("@OrganizeId", organizeId??"")
                };
                return FindList<NeedDealCountVO>(sql.ToString(), param).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    { Constants.KfId, warehouseId??""},
                    { Constants.OrganizeId, (OperatorProvider.GetCurrent()!=null?OperatorProvider.GetCurrent().OrganizeId:"")??""}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        #endregion

        /// <summary>
        /// 获取物资代码
        /// </summary>
        /// <returns></returns>
        public string GetProductCode()
        {
            var sql = new StringBuilder();
            sql.AppendLine("declare @productCode varchar(20) ");
            sql.AppendLine("select @productCode = max(sfxmid)+1  FROM [NewtouchHIS_Base].[dbo].[xt_sfxm] ");
            sql.AppendLine("set @productCode = 'wz'+ @productCode ");
            sql.AppendLine("WHILE EXISTS(select productCode from wz_product where productCode = @productCode) ");
            sql.AppendLine("BEGIN ");
            sql.AppendLine("SET @productCode = @productCode + 'x' ");
            sql.AppendLine("END ");
            sql.AppendLine("select @productCode ");
            return FirstOrDefault<string>(sql.ToString());
        }

        /// <summary>
        /// 检查物资代码是否已存在
        /// </summary>
        /// <returns></returns>
        public string CheckProductCode(string productCode, string keyValue)
        {
            const string sql = "select productCode from wz_product where productCode=@productCode and Id!=@keyValue";
            var param = new DbParameter[] {
                new SqlParameter("@productCode", productCode),
                new SqlParameter("@keyValue", keyValue),
            };
            return FirstOrDefault<string>(sql, param);

        }

        /// <summary>
        /// 插入物资数据并同步到收费项目中
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertProductToSfxm(WzProductEntity entity)
        {
            const string sql = @"
DECLARE @sfdlCode VARCHAR(50)
SELECT @sfdlCode = Value FROM [NewtouchHIS_herp].[dbo].[Sys_Config] where Code='sfdlCode'
DECLARE @unit VARCHAR(50)
SELECT @unit=name FROM [NewtouchHIS_Base].[dbo].[wz_unit]  WHERE id=@minUnit
INSERT INTO [NewtouchHIS_Base].[dbo].[xt_sfxm]
([sfxmCode]
,[sfxmmc]
,[sfdlCode]
,[OrganizeId]
,[py]
,[dw]
,[dj]
,[zfbl]
,[zfxz]
,[mzzybz]
,[ssbz]
,[tsbz]
,[sfbz]
,[CreatorCode]
,[CreateTime]
,[zt]
,[gg]) VALUES
(@productCode
,@name
,@sfdlCode
,@OrganizeId
,@py
,@unit
,@lsj
,0
,'1'
,'0'
,'0'
,'0'
,'1'
,@CreatorCode
,@CreateTime
,@zt
,@gg)
";
            var param = new DbParameter[] {
                new SqlParameter("@productCode", entity.productCode),
                new SqlParameter("@name", entity.name),
                new SqlParameter("@OrganizeId", entity.OrganizeId),
                new SqlParameter("@py", entity.py),
                new SqlParameter("@lsj", entity.lsj),
                new SqlParameter("@CreatorCode", entity.CreatorCode),
                new SqlParameter("@CreateTime", entity.CreateTime),
                new SqlParameter("@zt", entity.zt),
                new SqlParameter("@gg", string.IsNullOrEmpty(entity.gg) ?"":entity.gg),
                new SqlParameter("@minUnit", entity.minUnit),

            };
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                if (wzProductRepo.Insert(entity) <= 0)//给物资表插入物资数据
                {
					throw new Exception("新增失败");
				}
                if (SynProductToWarehouse(entity) < 0)//将物资关联上默认同步的库房
                {
                    throw new Exception("将物资同步到库房时失败");
                }
				//if (ExecuteSqlCommand(sql, param) <= 0)//将物资数据同步到收费项目
				//{
				//	throw new Exception("新增失败");
				//}
				db.Commit();
            }
			
			return 1;
        }

		public int InsertTOSfxm(string orgid, string wzcode)
		{
			var sql = "exec herp_物资同步到收费项目 @orgId,@wzcode";
			var param = new DbParameter[] {
				new SqlParameter("@orgId", orgid),
				new SqlParameter("@wzcode", wzcode),
			};
			int refcount = ExecuteSqlCommand(sql, param);
			return refcount;
		}

		/// <summary>
		/// 更新物资数据并同步到收费项目中
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int updateProductToSfxm(WzProductEntity entity)
        {
            const string sql = @"
DECLARE @oldProductCode VARCHAR(50)
SELECT @oldProductCode = productCode FROM [NewtouchHIS_herp].[dbo].[wz_product] WHERE Id=@Id
DECLARE @unit VARCHAR(50)
SELECT @unit=name FROM  [NewtouchHIS_Base].[dbo].[wz_unit]  WHERE id=@minUnit
UPDATE [NewtouchHIS_Base].[dbo].[xt_sfxm] set 
[sfxmCode]=@productCode
,[sfxmmc]=@name
,[OrganizeId]=@OrganizeId
,[py]=@py
,[dw]=@unit
,[dj]=@lsj
,[LastModifierCode]=@LastModifierCode
,[LastModifyTime]=@LastModifyTime
,[zt]=@zt
,[gg]=@gg 
WHERE sfxmCode=@oldProductCode
";
            var param = new DbParameter[] {
                new SqlParameter("@Id", entity.Id),
                new SqlParameter("@productCode", entity.productCode),
                new SqlParameter("@name", entity.name),
                new SqlParameter("@OrganizeId", entity.OrganizeId),
                new SqlParameter("@py", entity.py),
                new SqlParameter("@lsj", entity.lsj),
                new SqlParameter("@LastModifierCode", entity.LastModifierCode),
                new SqlParameter("@LastModifyTime", entity.LastModifyTime),
                new SqlParameter("@zt", entity.zt),
                new SqlParameter("@gg", string.IsNullOrEmpty(entity.gg) ?"":entity.gg),
                new SqlParameter("@minUnit", entity.minUnit),

            };
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //if (ExecuteSqlCommand(sql, param) <= 0)//将物资数据同步到收费项目
                //{
                //    throw new Exception("将物资同步到收费项目时失败");
                //}
                ExecuteSqlCommand(sql, param);
                if (wzProductRepo.Update(entity) <= 0)//更新物资表物资数据
                {
                    throw new Exception("修改失败");
                }
                db.Commit();
            }
            return 1;
        }

        /// <summary>
        /// 删除收费项目中的物资
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteProductToSfxm(string id)
        {
            const string sql = @"
DELETE FROM  [NewtouchHIS_Base].[dbo].[xt_sfxm] WHERE sfxmcode=(SELECT productCode FROM [wz_product] WHERE id=@id)";
            var param = new DbParameter[] {
                new SqlParameter("@id", id),
            };
            return ExecuteSqlCommand(sql, param);
        }

        /// <summary>
        /// 同步物资到默认库房
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int SynProductToWarehouse(WzProductEntity entity)
        {

            var targetWar = kfWarehouseRepo.IQueryable(p => p.OrganizeId == entity.OrganizeId && p.isDefSyn == "1" && p.zt == ((int)Enumzt.Enable).ToString()).ToList();
            var targetRel = new List<RelProductWarehouseEntity>();
            var existPro = _relProductWarehouseRepo.IQueryable(p => p.productId == entity.Id && p.OrganizeId == entity.OrganizeId).Select(q => q.warehouseId).ToList();
            targetWar.ForEach(p =>
            {
                if (existPro.Contains(p.Id)) return;
                var item = new RelProductWarehouseEntity
                {
                    OrganizeId = entity.OrganizeId,
                    productId = entity.Id,
                    productName = entity.name,
                    warehouseId = p.Id,
                    warehouseName = p.name,
                    unitId = entity.minUnit,
                    zt = ((int)Enumzt.Enable).ToString()
                };
                item.Create(true);
                targetRel.Add(item);
            });
            return targetRel.Count > 0 ? _relProductWarehouseRepo.Insert(targetRel) : 0;
        }

        /// <summary>
        /// 获取产品明细
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="organizeId"></param>
        /// <param name="topCount">默认 top 500</param>
        /// <returns></returns>
        public List<VProductInfoDo> SelectProductDetail(string keyword, string organizeId, int topCount = 500)
        {
            var sql = string.Format(@"
SELECT DISTINCT {0} wz.Id productId, lb.name lbmc, wz.brand, wz.name wzmc, wz.gg, zxdw.name zxdwmc, wz.minUnit unitId, ISNULL(scs.name,'') sccj
FROM dbo.wz_product(NOLOCK) wz 
INNER JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1' 
LEFT JOIN dbo.gys_supplier(NOLOCK) scs ON scs.Id=wz.supplierId AND scs.supplierType='1' AND scs.OrganizeId=wz.OrganizeId AND scs.zt='1' 
WHERE wz.OrganizeId=@OrganizeId
AND wz.zt='1'
AND (wz.name LIKE '%'+@keyword+'%' OR wz.py LIKE '%'+@keyword+'%')
", topCount > 0 ? ("TOP " + topCount) : "");
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyword", keyword??"")
            };
            return FindList<VProductInfoDo>(sql, param);
        }

        /// <summary>
        /// 查询物资所有单位
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<ProductUnit> SelectProductUnits(string productId, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT rpu.unitId, dw.name dwmc, rpu.zhyz, rpu.productId 
FROM dbo.rel_productUnit(NOLOCK) rpu
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpu.productId AND wz.OrganizeId=rpu.OrganizeId AND wz.zt='1'
INNER JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) dw ON dw.Id=rpu.unitId AND dw.zt='1' 
WHERE rpu.zt='1'
AND rpu.OrganizeId=@OrganizeId
AND rpu.productId=@productId
";
            var param = new DbParameter[]
            {
                new SqlParameter("@productId", productId),
                new SqlParameter("@OrganizeId",organizeId )
            };
            return FindList<ProductUnit>(sql, param);
        }
    }
}
