using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Herp.Domain.DTO.InputDto;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Domain.IRepository;
using Newtouch.Herp.Infrastructure.Enum;
using Newtouch.HIS.Domain.ValueObjects;

namespace Newtouch.Herp.DomainServices.StorageManage
{
    /// <summary>
    /// 库房管理
    /// </summary>
    public class StorageManageDmnService : DmnServiceBase, IStorageManageDmnService
    {
        private readonly IKfKcxxRepo _kfKcxxRepo;
        private readonly IKfCrkdjRepo _kfCrkdjRepo;
        private readonly IKfCrkmxRepo _kfCrkmxRepo;
        private readonly IKfKcxxDmnService _kfKcxxDmnService;
        private readonly IKfCrkdjDmnService _iKfCrkdjDmnService;

        public StorageManageDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<VSelProductInfoEntity> DepartmentStockListQuery(DepartmentStockListQueryParam param)
        {
            var sql = new StringBuilder(@"
SELECT dbo.f_getComplexWzSlandDw(kykcsl,zhyz, bmdw.name, zxdw.name) slstr
, zxdw.name mindwmc,bmdw.name bmdwmc, a.* 
FROM (
    SELECT p.Id,p.OrganizeId,p.name,p.minUnit zxdwId, rpw.unitId bmdwId 
    ,ISNULL(SUM(kcxx.kcsl),0) kcsl, ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl
    ,p.lsj minlsj, ISNULL(CONVERT(NUMERIC(11,4),p.lsj*rpu.zhyz),0) bmlsj
    ,p.py,p.gg,p.supplierId,s.name supplierName,wt.name lbmc,p.typeId lbId,ISNULL(rpu.zhyz,1) zhyz,ISNULL(rpu.zhyz,1) bmdwzhyz,p.productCode,p.hcgjybdm,p.hcgjybdm gjybdm,p.jj
    FROM dbo.wz_product(NOLOCK) p 
    INNER JOIN dbo.gys_supplier(NOLOCK) s ON p.supplierId = s.Id AND p.OrganizeId = s.OrganizeId AND s.zt = '1'
    INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.OrganizeId = p.OrganizeId AND rpw.productId = p.Id 
    LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=p.Id AND kcxx.warehouseId=rpw.warehouseId AND kcxx.zt='1'
    LEFT JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.Id=kcxx.crkmxId AND mx.pc=kcxx.pc AND mx.ph=kcxx.ph AND mx.zt='1' 
    LEFT JOIN dbo.kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.OrganizeId=p.OrganizeId AND dj.zt='1'
    LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu on rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=p.OrganizeId AND rpu.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) wt ON wt.id = p.typeId AND wt.zt='1'
    WHERE p.OrganizeId = @OrganizeId 
    AND rpw.warehouseId=@warehouseId 
    AND rpw.zt='1'
    AND p.zt = @zt
    AND (p.name LIKE '%'+@keyWord+'%' OR p.py LIKE '%'+@keyWord+'%')
");
            if (!string.IsNullOrWhiteSpace(param.gysId))
            {
                sql.Append("	AND dj.ckbm=@ckbm ");
            }
            if (!string.IsNullOrWhiteSpace(param.deliveryNo))
            {
                sql.AppendLine("	AND dj.deliveryNo=@deliveryNo ");
            }
            sql.Append(@"
    GROUP BY p.Id, p.OrganizeId, p.name, p.py, p.supplierId, s.name, p.minUnit, p.gg, wt.name,p.typeId,p.lsj,rpu.zhyz, p.minUnit, rpw.unitId,p.productCode,p.hcgjybdm,p.jj) a
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=a.bmdwId AND bmdw.zt='1'  
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=a.zxdwId AND zxdw.zt='1' 
");
            var sqlParam = new DbParameter[]
            {
                new SqlParameter("@warehouseId", param.warehouseId??""),
                new SqlParameter("@OrganizeId", param.organizeId),
                new SqlParameter("@keyWord", param.key??""),
                new SqlParameter("@ckbm",param.gysId??""),
                new SqlParameter("@deliveryNo", param.deliveryNo??""),
                new SqlParameter("@zt", param.zt)
            };
            return FindList<VSelProductInfoEntity>(sql.ToString(), sqlParam);
        }

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public List<VSelProductInfoEntity> DepartmentStockListQuery(Pagination pagination, string key, string warehouseId, string organizeId, string zt = "1")
        {
            const string sql = @"
SELECT dbo.f_getComplexWzSlandDw(kykcsl,zhyz, bmdw.name, zxdw.name) slstr
, zxdw.name mindwmc,bmdw.name bmdwmc, a.* 
FROM (
    SELECT p.Id,p.OrganizeId,p.name,p.minUnit zxdwId, rpw.unitId bmdwId 
    ,ISNULL(SUM(kcxx.kcsl),0) kcsl, ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl
    ,p.lsj minlsj, ISNULL(CONVERT(NUMERIC(11,4),p.lsj*rpu.zhyz),0) bmlsj
    ,p.py,p.gg,p.supplierId,s.name supplierName,wt.name lbmc,p.typeId lbId,ISNULL(rpu.zhyz,1) zhyz,ISNULL(rpu.zhyz,1) bmdwzhyz
    FROM dbo.wz_product(NOLOCK) p 
    INNER JOIN dbo.gys_supplier(NOLOCK) s ON p.supplierId = s.Id AND p.OrganizeId = s.OrganizeId AND s.zt = '1'
    INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.OrganizeId = p.OrganizeId AND rpw.productId = p.Id 
    LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=p.Id AND kcxx.warehouseId=rpw.warehouseId AND kcxx.zt='1'
    LEFT JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.Id=kcxx.crkmxId AND mx.pc=kcxx.pc AND mx.ph=kcxx.ph AND mx.zt='1' 
    LEFT JOIN dbo.kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.OrganizeId=p.OrganizeId AND dj.zt='1'
    LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu on rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=p.OrganizeId AND rpu.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) wt ON wt.id = p.typeId AND wt.zt='1'
    WHERE p.OrganizeId = @OrganizeId 
    AND rpw.warehouseId=@warehouseId 
    AND rpw.zt='1'
    AND p.zt = @zt
    AND (p.name LIKE '%'+@keyWord+'%' OR p.py LIKE '%'+@keyWord+'%')
    GROUP BY p.Id, p.OrganizeId, p.name, p.py, p.supplierId, s.name, p.minUnit, p.gg, wt.name,p.typeId,p.lsj,rpu.zhyz, p.minUnit, rpw.unitId
) a
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=a.bmdwId AND bmdw.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=a.zxdwId AND zxdw.zt='1'
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyWord", key??""),
                new SqlParameter("@zt", zt)
            };
            return QueryWithPage<VSelProductInfoEntity>(sql, pagination, param).ToList();
        }

        /// <summary>
        /// 下拉列表物资信息
        /// </summary>
        /// <param name="key">物资名称/拼音</param>
        /// <param name="ckbm"></param>
        /// <param name="deliveryNo">配送单号</param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="zt"></param>
        /// <returns></returns>
        public List<VSelProductInfoEntity> DepartmentStockListQuery(string key, string ckbm, string deliveryNo, string warehouseId, string organizeId, string zt = "1")
        {
            const string sql = @"
SELECT dbo.f_getComplexWzSlandDw(kykcsl,zhyz, bmdw.name, zxdw.name) slstr
, zxdw.name mindwmc,bmdw.name bmdwmc, a.* 
FROM (
	SELECT p.Id,p.OrganizeId,p.name,p.minUnit zxdwId, rpw.unitId bmdwId 
	,ISNULL(SUM(kcxx.kcsl),0) kcsl, ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl
	,p.lsj minlsj, ISNULL(CONVERT(NUMERIC(11,4),p.lsj*rpu.zhyz),0) bmlsj
	,p.py,p.gg,p.supplierId,s.name supplierName,wt.name lbmc,p.typeId lbId,ISNULL(rpu.zhyz,1) zhyz,ISNULL(rpu.zhyz,1) bmdwzhyz
	FROM dbo.wz_product(NOLOCK) p 
	INNER JOIN dbo.gys_supplier(NOLOCK) s ON p.supplierId = s.Id AND p.OrganizeId = s.OrganizeId AND s.zt = '1'
	INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.OrganizeId = p.OrganizeId AND rpw.productId = p.Id 
	LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=p.Id AND kcxx.warehouseId=rpw.warehouseId AND kcxx.zt='1'
	LEFT JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.Id=kcxx.crkmxId AND mx.pc=kcxx.pc AND mx.ph=kcxx.ph AND mx.zt='1' 
	LEFT JOIN dbo.kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.OrganizeId=p.OrganizeId AND dj.zt='1'
	LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu on rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=p.OrganizeId AND rpu.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) wt ON wt.id = p.typeId AND wt.zt='1'
	WHERE p.OrganizeId = @OrganizeId 
	AND rpw.warehouseId=@warehouseId
	AND rpw.zt='1'
	AND p.zt = @zt
	AND (p.name LIKE '%'+@keyWord+'%' OR p.py LIKE '%'+@keyWord+'%')
	--AND dj.djlx=@djlx --外部入库
	AND dj.auditState=@shzt --通过
	AND dj.ckbm=@ckbm --gysId
	AND (dj.deliveryNo=@deliveryNo OR ''=@deliveryNo)
	GROUP BY p.Id, p.OrganizeId, p.name, p.py, p.supplierId, s.name, p.minUnit, p.gg, wt.name,p.typeId,p.lsj,rpu.zhyz, p.minUnit, rpw.unitId
) a
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=a.bmdwId AND bmdw.zt='1' 
LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=a.zxdwId AND zxdw.zt='1'
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyWord", key??""),
                new SqlParameter("@zt", zt),
                new SqlParameter("@djlx", ((int)EnumOutOrInStorageBillType.Wbrk).ToString()),
                new SqlParameter("@shzt", ((int)EnumAuditState.Adopt).ToString()),
                new SqlParameter("@ckbm", ckbm),
                new SqlParameter("@deliveryNo", deliveryNo)
            };
            return FindList<VSelProductInfoEntity>(sql, param);
        }

        /// <summary>
        /// 获取物资批号批次信息  top 20
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="gysId"></param>
        /// <param name="deliveryNo"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<VProductBatchInfoEntity> ProductBatchQuery(string productId, string warehouseId, string organizeId, string gysId = "", string deliveryNo = "", string keyword = "")
        {
            const string sql = @"
SELECT TOP 20 s.ph,s.pc,s.yxq,s.scrq,s.fph,SUM(s.kcsl) kcsl,SUM(s.kykcsl) kykcsl
,s.bmjj,s.minjj,SUM(s.jjzje) jjzje,CONCAT(CONVERT(NUMERIC(11,2),s.bmjj),'元/',s.bmdwmc) jjdwdj
,dbo.f_getComplexWzSlandDw(SUM(s.kykcsl),s.zhyz, s.bmdwmc, s.zxdwmc) slstr 
FROM (
	SELECT kcxx.ph, kcxx.pc, kcxx.yxq, ISNULL(mx.scrq, '') scrq, ISNULL(mx.fph,'') fph
	,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*rpu.zhyz),0) bmjj,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz),0) minjj,ISNULL(CONVERT(NUMERIC(11,2),SUM(kcxx.jj/kcxx.zhyz*(kcxx.kcsl-kcxx.djsl))),0) jjzje
	,ISNULL(SUM(kcxx.kcsl),0) kcsl
	,ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl
	,rpu.zhyz, bmdw.name bmdwmc, zxdw.name zxdwmc
	FROM dbo.kf_kcxx(NOLOCK) kcxx
	INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.productId=kcxx.productId AND rpw.warehouseId=kcxx.warehouseId AND rpw.OrganizeId=kcxx.OrganizeId AND rpw.zt='1'
	INNER JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=kcxx.OrganizeId AND rpu.zt='1'
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw.productId AND wz.OrganizeId=kcxx.OrganizeId AND wz.zt='1'
	INNER JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpw.unitId AND bmdw.zt='1'
	INNER JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
	LEFT JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.Id=kcxx.crkmxId AND mx.zt='1'
	LEFT JOIN dbo.kf_crkdj(NOLOCK) dj ON dj.Id=mx.crkId AND dj.zt='1' AND dj.auditState='1' AND dj.djlx=1
	WHERE kcxx.productId=@proId
	AND kcxx.warehouseId=@warehouseId
	AND kcxx.OrganizeId=@OrganizeId
    AND (kcxx.kcsl-kcxx.djsl)>0
	AND kcxx.zt='1'
	AND (dj.deliveryNo=@deliveryNo OR ''=@deliveryNo)
	AND (dj.ckbm=@ckbm OR ''=@ckbm)
    AND (kcxx.pc LIKE '%'+@keyword+'%' OR kcxx.ph LIKE '%'+@keyword+'%')
	GROUP BY kcxx.ph, kcxx.pc, kcxx.yxq,bmdw.name,zxdw.name,kcxx.jj,mx.scrq,mx.fph,rpu.zhyz,kcxx.zhyz
) s
GROUP BY s.ph,s.pc,s.yxq,s.scrq,s.fph,s.bmjj,s.minjj,s.zhyz,s.bmdwmc,s.zxdwmc
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@deliveryNo", deliveryNo??""),
                new SqlParameter("@ckbm", gysId??""),
                new SqlParameter("@keyword", (keyword??"").Trim()),
                new SqlParameter("@proId", productId??"")
            };
            return FindList<VProductBatchInfoEntity>(sql, param);
        }

        /// <summary>
        /// 外部入库
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mxList"></param>
        public string SaveOutOrInStorageInfo(KfCrkdjEntity dj, List<KfCrkmxEntity> mxList)
        {
            var successList = new List<KfCrkmxEntity>();
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                if (SaveCrkdAndReturnIsExist(dj))
                {
                    var mx = _kfCrkmxRepo.IQueryable(p => p.crkId == dj.Id && p.zt == "1");
                    if (mx != null && mx.Count() > 0)
                    {
                        mx.ToList().ForEach(p =>
                        {
                            _kfCrkmxRepo.Delete(p);
                        });
                    }
                }
                foreach (var item in mxList)
                {
                    item.crkId = dj.Id;
                    if (_kfCrkmxRepo.Insert(item) <= 0) throw new FailedException("保存出入库明细失败");
                    successList.Add(item);
                }
                db.Commit();
                return "";
            }
        }

        /// <summary>
        /// 新老数据转换
        /// </summary>
        /// <param name="dj"></param>
        private bool SaveCrkdAndReturnIsExist(KfCrkdjEntity dj)
        {
            IsExistEffectiveTemporaryDj(dj);
            IsExistPdh(dj.Pdh);
            var oldCrkdj = _kfCrkdjRepo.FindEntity(p => p.Pdh == dj.Pdh && p.OrganizeId == dj.OrganizeId && p.zt == "1");
            if (oldCrkdj != null)
            {
                oldCrkdj.auditState = dj.auditState;
                oldCrkdj.deliveryNo = dj.deliveryNo;
                oldCrkdj.crkfs = dj.crkfs;
                oldCrkdj.ckbm = dj.ckbm;
                oldCrkdj.ckczy = dj.ckczy;
                dj.Id = oldCrkdj.Id;
                oldCrkdj.Modify();
                if (_kfCrkdjRepo.Update(oldCrkdj) <= 0) throw new Exception("保存出入库单信息失败");
                return true;
            }
            if (_kfCrkdjRepo.Insert(dj) <= 0) throw new Exception("保存出入库单信息失败");
            return false;
        }

        /// <summary>
        /// 判断是否存在有效并相同配送单号的暂存单
        /// </summary>
        /// <param name="dj"></param>
        /// <returns></returns>
        private bool IsExistEffectiveTemporaryDj(KfCrkdjEntity dj)
        {
            if (string.IsNullOrWhiteSpace(dj.deliveryNo)) return false;
            var existCrkdj = _kfCrkdjRepo.IQueryable(p => p.deliveryNo == dj.deliveryNo && p.zt == ((int)Enumzt.Enable).ToString());
            var tmpCout = existCrkdj.Count();
            if (existCrkdj != null && tmpCout > 0)
            {
                var existCrkdjList = existCrkdj.ToList();
                existCrkdjList.RemoveAll(p => new string[] {
                        ((int)EnumAuditState.Cancelled).ToString(),
                        ((int)EnumAuditState.Refuse).ToString()
                    }.Contains(p.auditState));
                if (existCrkdjList.Any(p => p.auditState != ((int)EnumAuditState.Temporary).ToString()))
                {
                    throw new Exception(string.Format("配送单【{0}】已提交，如需追加该配送单请先撤回", dj.deliveryNo));
                }
                if (existCrkdjList.Count > 1) throw new Exception(string.Format("单据中存在多张配送单为【{0}】的单据，请确保配送单的唯一性，删除该配送单重复的单据", dj.deliveryNo));
            }
            return false;
        }

        /// <summary>
        /// 验证单据号的唯一性
        /// </summary>
        /// <param name="pdh"></param>
        /// <returns></returns>
        private bool IsExistPdh(string pdh)
        {
            if (string.IsNullOrWhiteSpace(pdh)) throw new Exception("单据号不允许为空");
            var entity = _kfCrkdjRepo.FindEntity(p => p.Pdh == pdh
            && !(new string[]
            {
                ((int)EnumAuditState.Temporary).ToString()
            }).Contains(p.auditState));
            if (entity != null) throw new Exception("存在重复的单据号，请刷新页面后在提交");
            return false;
        }

        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <param name="lbId"></param>
        /// <param name="kzbz"></param>
        /// <param name="wzzt"></param>
        /// <param name="xslkc">显示零库存  1：显示  0：不显示</param>
        /// <param name="ygq">已过期</param>
        /// <param name="mxyx">暂时有效的明细  true：是  false：否</param>
        /// <returns></returns>
        public IList<VProductStorageEntity> GetProductStorage(Pagination pagination,
            string warehouseId,
            string organizeId,
            string keyWord,
            string lbId,
            string kzbz,
            string wzzt,
            string xslkc,
            string ygq,
            string mxyx, string kccg = null)
        {
            var sql = new StringBuilder(@"
SELECT * FROM (
    SELECT rpw.productId, wz.name wzmc,wz.py, ISNULL(lb.name,'') lb
    ,dbo.f_getComplexWzSlandDw(SUM(kcxx.kcsl), rpu.zhyz, bmdw.name, zxdw.name) slStr, ISNULL(SUM(kcxx.kcsl),0) zkc
    ,ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*rpu.zhyz),0) lsj, CONVERT(NUMERIC(11,2),SUM(ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*kcxx.kcsl),0))) lsze
    ,CONVERT(NUMERIC(11,2),SUM(ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*kcxx.kcsl),0))) jjze, ISNULL(bmdw.name,'') bmdwmc
    ,wz.gg, ISNULL(wz.brand,'') brand, gys.name sccj,wz.kcyjz,wz.lsj wzlsj,wz.jj wzjj
    FROM dbo.rel_productWarehouse(NOLOCK) rpw
    INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw.productId AND wz.OrganizeId=rpw.OrganizeId
    LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1'
    LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpu.unitId AND bmdw.zt='1'
    LEFT JOIN dbo.gys_supplier(NOLOCK) gys ON gys.Id=wz.supplierId AND gys.zt='1' AND gys.OrganizeId=rpw.OrganizeId
    LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=rpw.productId AND kcxx.warehouseId=rpw.warehouseId AND kcxx.OrganizeId=rpw.OrganizeId
    WHERE rpw.warehouseId=@warehouseId ");
            sql.Append(@"
    AND rpw.OrganizeId=@OrganizeId
    AND (wz.name LIKE '%'+ @keyword +'%' OR wz.py LIKE '%'+ @keyword +'%')
    AND (wz.typeId=@lbId OR ''=@lbId)
    AND (rpw.zt=@kzbz OR ''=@kzbz)
    AND (wz.zt=@wzzt OR ''=@wzzt) ");
            if (!string.IsNullOrWhiteSpace(mxyx) && "true".Equals(mxyx.Trim().ToLower()))
            {
                sql.AppendLine("AND kcxx.zt='1' ");
            }
            sql.AppendLine(@"
    GROUP BY rpw.productId,wz.name,wz.py,lb.name,wz.lsj,rpu.zhyz,zxdw.name,bmdw.name,wz.gg,wz.brand,gys.name,wz.kcyjz,wz.lsj,wz.jj
) a 
WHERE 1=1 ");
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId ?? ""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@lbId", lbId ?? ""),
                new SqlParameter("@kzbz", kzbz ?? ""),
                new SqlParameter("@wzzt", wzzt ?? ""),
                new SqlParameter("@keyword", keyWord??"")
            };
            if (!string.IsNullOrWhiteSpace(xslkc) && "0".Equals(xslkc.Trim()))
            {
                sql.AppendLine("AND a.zkc<>0 ");
            }
            if (!string.IsNullOrWhiteSpace(ygq) && "true".Equals(ygq.Trim().ToLower()))
            {
                sql.AppendLine("AND a.yxq<GETDATE() ");
            }
            if (!string.IsNullOrWhiteSpace(kccg))
            {
                sql.AppendLine("AND  zkc <=isnull(kcyjz,0) ");
            }
            return QueryWithPage<VProductStorageEntity>(sql.ToString(), pagination, param);
        }

        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public IList<VProductStorageEntity> GetProductStorage(string warehouseId, string organizeId, string keyWord)
        {
            const string sql = @"
SELECT d.*, dbo.f_getComplexWzSlandDw(d.kykcsl, d.zhyz, d.bmdwmc, d.zxdwmc) slStr 
FROM (
	SELECT rpw.productId, lb.name lb, wz.brand, wz.name wzmc, ISNULL(SUM(kcxx.kcsl-kcxx.djsl),0) kykcsl, ISNULL(kcxx.zhyz,1) zhyz
	,wz.gg, ISNULL(kcxx.jj,0) jj, CONVERT(NUMERIC(11,4),ISNULL(kcxx.jj/ISNULL(kcxx.zhyz,1),0)) minjj, bmdw.name bmdwmc, zxdw.name zxdwmc, scs.name sccj
	FROM dbo.rel_productWarehouse(NOLOCK) rpw
	INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=rpw.productId AND wz.zt='1' AND wz.OrganizeId=rpw.OrganizeId
	INNER JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1' 
	LEFT JOIN dbo.kf_kcxx(NOLOCK) kcxx ON kcxx.productId=rpw.productId AND kcxx.OrganizeId=rpw.OrganizeId AND kcxx.zt='1' AND kcxx.warehouseId=rpw.warehouseId
	LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.zhyz=kcxx.zhyz AND rpu.OrganizeId=rpw.OrganizeId AND rpu.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpu.unitId AND bmdw.zt='1' 
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND zxdw.zt='1' 
	LEFT JOIN dbo.gys_supplier(NOLOCK) scs ON scs.Id=wz.supplierId AND scs.supplierType='1' AND scs.zt='1' AND scs.OrganizeId=rpw.OrganizeId
	WHERE rpw.warehouseId=@warehouseId
	AND rpw.OrganizeId=@OrganizeId
	AND rpw.zt='1'
	AND (wz.name LIKE '%'+@keyword+'%' OR wz.py LIKE '%'+@keyword+'%')
	GROUP BY lb.name, rpw.productId,wz.name, kcxx.zhyz, wz.gg, kcxx.jj, bmdw.name, zxdw.name,wz.brand, scs.name
) d
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@keyword", keyWord??"")
            };
            return FindList<VProductStorageEntity>(sql, param);
        }

        /// <summary>
        /// 物资库存分批次批号汇总
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="proId"></param>
        /// <param name="zt">暂时有效的明细  true：是  false：否</param>
        /// <returns></returns>
        public IList<VProductStorageDetailEntity> GetProductStorageDetail(string warehouseId, string organizeId, string proId, string zt)
        {
            const string sql = @"
SELECT s.productId,s.wzmc,s.ph,s.pc,s.yxq,s.zt,s.bmdwmc
,dbo.f_getComplexWzSlandDw(SUM(s.djsl),s.zhyz,s.bmdwmc,s.zxdwmc) bmdjslStr
,dbo.f_getComplexWzSlandDw(SUM(s.kcsl),s.zhyz,s.bmdwmc,s.zxdwmc) bmkcslStr
,s.jj,SUM(s.jjze) jjze
FROM (
	SELECT wz.Id productId, wz.name wzmc, kcxx.ph, kcxx.pc, kcxx.yxq, kcxx.zt, bmdw.name bmdwmc,rpu.zhyz,zxdw.name zxdwmc
	,ISNULL(SUM(kcxx.djsl),0) djsl,ISNULL(SUM(kcxx.kcsl),0) kcsl
	,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*rpu.zhyz),0) jj, ISNULL(CONVERT(NUMERIC(11,2),SUM(kcxx.jj/kcxx.zhyz*kcxx.kcsl)),0) jjze
	FROM dbo.kf_kcxx(NOLOCK) kcxx
	LEFT JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId
	LEFT JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.productId=kcxx.productId AND rpw.warehouseId=kcxx.warehouseId AND rpw.OrganizeId=kcxx.OrganizeId AND rpw.zt='1'
	LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=kcxx.OrganizeId AND rpu.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpw.unitId AND bmdw.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND bmdw.zt='1'
	WHERE kcxx.warehouseId=@warehouseId
	AND kcxx.OrganizeId=@OrganizeId
	AND kcxx.productId=@proId 
	AND (kcxx.zt=@zt OR ''=@zt)
	GROUP BY wz.Id,wz.name,kcxx.ph,kcxx.pc,rpu.zhyz,bmdw.name,zxdw.name,kcxx.zhyz,kcxx.jj,kcxx.yxq,kcxx.zt
) s
GROUP BY s.productId,s.wzmc,s.ph,s.pc,s.yxq,s.zt,s.bmdwmc,s.jj,s.zhyz,s.zxdwmc
";
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@proId", proId ),
                new SqlParameter("@zt",zt??"")
            };
            return FindList<VProductStorageDetailEntity>(sql, param);
        }

        /// <summary>
        /// 获取物资库存
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <param name="keyWord"></param>
        /// <param name="lbId"></param>
        /// <param name="kzbz"></param>
        /// <param name="wzzt"></param>
        /// <param name="xslkc">显示零库存  1：显示  0：不显示</param>
        /// <param name="mxyx">暂时有效的明细  true：是  false：否</param>
        /// <param name="isExpired">true:显示过期  false：显示不过期</param>
        /// <returns></returns>
        public IList<VProductStorageDetailEntity> SelectStorageDetail(Pagination pagination,
            string warehouseId,
            string organizeId,
            string keyWord,
            string lbId,
            string kzbz,
            string wzzt,
            string xslkc,
            string mxyx,
            bool isExpired)
        {
            var sql = new StringBuilder(@"
SELECT * 
FROM (
	SELECT wz.Id productId, wz.name wzmc, kcxx.ph, kcxx.pc, kcxx.yxq, kcxx.zt, bmdw.name bmdwmc,rpu.zhyz,zxdw.name zxdwmc,lb.name lb,wz.gg,wz.brand,cj.name sccj
	,ISNULL(SUM(kcxx.djsl),0) djsl,ISNULL(SUM(kcxx.kcsl),0) kcsl
	,dbo.f_getComplexWzSlandDw(SUM(kcxx.djsl),rpu.zhyz,bmdw.name,zxdw.name) bmdjslStr
	,dbo.f_getComplexWzSlandDw(SUM(kcxx.kcsl),rpu.zhyz,bmdw.name,zxdw.name) bmkcslStr
	,ISNULL(CONVERT(NUMERIC(11,4),kcxx.jj/kcxx.zhyz*rpu.zhyz),0) jj, CONCAT(ISNULL(CONVERT(NUMERIC(11,2),kcxx.jj/kcxx.zhyz*rpu.zhyz),0),'元/',bmdw.name) bmjjdjdw, ISNULL(CONVERT(NUMERIC(11,2),SUM(kcxx.jj/kcxx.zhyz*kcxx.kcsl)),0) jjze
	FROM dbo.kf_kcxx(NOLOCK) kcxx
		LEFT JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId
		LEFT JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.productId=kcxx.productId AND rpw.warehouseId=kcxx.warehouseId AND rpw.OrganizeId=kcxx.OrganizeId 
		LEFT JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.OrganizeId=kcxx.OrganizeId AND rpu.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) bmdw ON bmdw.Id=rpw.unitId AND bmdw.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.wz_unit(NOLOCK) zxdw ON zxdw.Id=wz.minUnit AND bmdw.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.wz_type(NOLOCK) lb ON lb.Id=wz.typeId AND lb.zt='1'
		LEFT JOIN dbo.gys_supplier(NOLOCK) cj ON cj.Id=wz.supplierId AND cj.zt='1'
	WHERE kcxx.warehouseId=@warehouseId
		AND kcxx.OrganizeId=@OrganizeId
		AND (wz.typeId=@lbId OR ''=@lbId)
		AND (wz.zt=@wzzt OR ''=@wzzt) 
		AND (wz.name LIKE '%'+ @keyword +'%' OR wz.py LIKE '%'+ @keyword +'%')
		AND (rpw.zt=@kzbz OR ''=@kzbz)
");
            if (isExpired)
            {
                sql.AppendLine("	AND kcxx.yxq<GETDATE() ");
            }
            else
            {
                sql.AppendLine("	AND kcxx.yxq>=GETDATE() ");
            }
            if (!string.IsNullOrWhiteSpace(mxyx) && "true".Equals(mxyx.Trim().ToLower()))
            {
                sql.AppendLine("    AND kcxx.zt='" + (int)Enumzt.Enable + "' ");
            }
            sql.AppendLine(@"
	GROUP BY wz.Id,wz.name,kcxx.ph,kcxx.pc,rpu.zhyz,bmdw.name,zxdw.name,kcxx.zhyz,kcxx.jj,kcxx.yxq,kcxx.zt,lb.name,wz.gg,wz.brand,cj.name
) s ");
            if (!string.IsNullOrWhiteSpace(xslkc) && "0".Equals(xslkc.Trim()))
            {
                sql.AppendLine("WHERE s.kcsl<>0 ");
            }
            var param = new DbParameter[] {
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@lbId", lbId ?? ""),
                new SqlParameter("@kzbz", kzbz ?? ""),
                new SqlParameter("@wzzt", wzzt ?? ""),
                new SqlParameter("@keyword", keyWord??"")
            };
            return QueryWithPage<VProductStorageDetailEntity>(sql.ToString(), pagination, param);
        }

        #region 外部入库

        /// <summary>
        /// 外部入库 审核通过
        /// </summary>
        /// <param name="kcxx"></param>
        /// <param name="dj"></param>
        public void WbrkAdopt(List<KfKcxxEntity> kcxx, KfCrkdjEntity dj)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                _kfKcxxRepo.Insert(kcxx);
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Adopt).ToString();
                dj.shczy = OperatorProvider.GetCurrent().UserCode;
                dj.rkczy = OperatorProvider.GetCurrent().UserCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
        }

        /// <summary>
        /// 外部入库 作废
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="dj"></param>
        /// <returns></returns>
        public string WbrkCancelled(List<KfCrkmxEntity> mx, KfCrkdjEntity dj)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mx)
                {
                    var execResult = _kfKcxxDmnService.WbrkKkc(p.sl * p.zhyz, p.pc, p.ph, p.productId, dj.OrganizeId, dj.rkbm, dj.CreatorCode);
                    if (!string.IsNullOrWhiteSpace(execResult))
                    {
                        return execResult;
                    }
                }
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Cancelled).ToString();
                dj.shczy = OperatorProvider.GetCurrent().UserCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
            return "";
        }

        #endregion

        #region 外部出库

        /// <summary>
        /// 外部出库/直接出库/内部发货退回
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mxList"></param>
        /// <returns></returns>
        public string Wbck(KfCrkdjEntity dj, List<KfCrkmxEntity> mxList)
        {
            var ret = _kfCrkdjRepo.Insert(dj); //新增kf_crkdj
            if (ret <= 0) return "";
            var successList = new List<KfCrkmxEntity>();
            try
            {
                foreach (var item in mxList)
                {
                    item.crkId = dj.Id;
                    if (_kfCrkmxRepo.Insert(item) <= 0) throw new FailedException("保存出入库明细失败");
                    successList.Add(item);
                    var execResult = _kfKcxxDmnService.FrozenKc(item.sl * item.zhyz, item.pc, item.ph, item.productId, dj.OrganizeId, dj.ckbm, dj.CreatorCode);
                    if (string.IsNullOrWhiteSpace(execResult)) continue;
                    throw new FailedException(execResult);
                }
            }
            catch (FailedException ex)
            {
                _iKfCrkdjDmnService.CancelCrkmx(dj, successList);
                return ex.Msg;
            }
            catch (Exception ex)
            {
                _iKfCrkdjDmnService.CancelCrkmx(dj, successList);
                return ex.Message;
            }
            return "";
        }

        /// <summary>
        /// 外部出库，扣库存，审核通过
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string SubtractKc(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mx)
                {
                    var r = _kfKcxxDmnService.SubtractKcslAndDjsl(p.productId, p.pc, p.ph, dj.ckbm, p.sl * p.zhyz, dj.OrganizeId, userCode);
                    if (!string.IsNullOrWhiteSpace(r))
                    {
                        return r;
                    }
                }
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Adopt).ToString();
                dj.shczy = userCode;
                dj.rkczy = userCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
            return "";
        }

        /// <summary>
        /// 外部出库/直接出库/内部发货退回，解冻，审核不通过
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string Unfreeze(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mx)
                {
                    var r = _kfKcxxDmnService.Unfreeze(p.productId, p.pc, p.ph, dj.ckbm, p.sl * p.zhyz, dj.OrganizeId, userCode);
                    if (!string.IsNullOrWhiteSpace(r))
                    {
                        return r;
                    }
                }
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Refuse).ToString();
                dj.shczy = userCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
            return "";
        }

        /// <summary>
        /// 外部出库 作废
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="dj"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string WbckCancelled(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                if (mx.Select(p => _kfKcxxDmnService.UpdateKcsl(p.productId, p.pc, p.ph, dj.ckbm, p.sl * p.zhyz, dj.OrganizeId, userCode)).Any(execResult => execResult <= 0))
                {
                    return "外部出库作废失败";
                }
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Cancelled).ToString();
                dj.shczy = userCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
            return "";
        }
        #endregion

        #region 直接出库

        /// <summary>
        /// 直接出库/内部发货退回 审核通过
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string DirectDeliveryAdopt(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mx)
                {
                    var r = _kfKcxxDmnService.SubtractKcslAndDjsl(p.productId, p.pc, p.ph, dj.ckbm, p.sl * p.zhyz, dj.OrganizeId, userCode);
                    if (string.IsNullOrWhiteSpace(r))
                    {
                        if (_kfKcxxDmnService.UpdateKcsl(p.productId, p.pc, p.ph, dj.rkbm, p.sl * p.zhyz, dj.OrganizeId, userCode) > 0) continue;
                        var kcxx = new KfKcxxEntity
                        {
                            crkmxId = p.Id,
                            djsl = 0,
                            jj = p.jj,
                            kcsl = p.sl * p.zhyz,
                            OrganizeId = dj.OrganizeId,
                            pc = p.pc,
                            ph = p.ph,
                            productId = p.productId,
                            warehouseId = dj.rkbm,
                            yxq = p.yxq,
                            zhyz = p.zhyz,
                            zt = ((int)Enumzt.Enable).ToString()
                        };
                        kcxx.Create(true);
                        db.Insert(kcxx);
                    }
                    else { return r; }
                }
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Adopt).ToString();
                dj.shczy = userCode;
                dj.rkczy = userCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
            return "";
        }

        /// <summary>
        /// 直接出库/内部发货退回 作废操作
        /// </summary>
        /// <param name="dj"></param>
        /// <param name="mx"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string DirectDeliveryCancel(KfCrkdjEntity dj, List<KfCrkmxEntity> mx, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mx)
                {
                    var execResult = _kfKcxxDmnService.RevokeInstorage(p.sl * p.zhyz, p.pc, p.ph, p.productId, dj.OrganizeId, dj.rkbm, dj.ckbm, userCode);
                    if (!string.IsNullOrWhiteSpace(execResult)) return execResult;
                }
                dj.rksj = DateTime.Now;
                dj.cksj = DateTime.Now;
                dj.auditState = ((int)EnumAuditState.Cancelled).ToString();
                dj.shczy = userCode;
                dj.Modify();
                _kfCrkdjRepo.Update(dj);
                db.Commit();
            }
            return "";
        }

        #endregion

        #region 出库至科室-同步物资到CIS科室库存管理
        public void SynchWz(string djh,string UserCode,string organizeId)
        {
            ExecuteSqlCommand("exec SynchDeptProduct_CIS @Pdh ,@OrganizeId,@UserCode", new SqlParameter("@Pdh", djh), new SqlParameter("@UserCode", UserCode), new SqlParameter("@organizeId", organizeId));
        }

        #endregion


        #region 首页统计

        /// <summary>
        /// 出库数量统计
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VPsiStatisticsByDateEntity> GetCkCountByKf(string warehouseId, string organizeId)
        {
            var sql = new StringBuilder(@"SELECT t.cksj statisticsDate, ISNULL(CONVERT(BIGINT,SUM(t.sl)),0) sl, '出库' itemName FROM ( ");
            sql.AppendLine("  SELECT mx.sl,CONVERT(VARCHAR(7), dj.cksj, 120) cksj ");
            sql.AppendLine("  FROM dbo.kf_crkdj(NOLOCK) dj ");
            sql.AppendLine("  INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id ");
            sql.AppendLine("  WHERE dj.zt='" + (int)Enumzt.Enable + "' AND auditState='" + (int)EnumAuditState.Adopt + "' AND dj.cksj>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01') AND dj.ckbm=@warehouseId AND dj.OrganizeId=@OrganizeId ");
            sql.AppendLine(") t ");
            sql.AppendLine("GROUP BY t.cksj ");
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId??"")
            };
            var result = FindList<VPsiStatisticsByDateEntity>(sql.ToString(), param);
            return result.Count > 0 ? result : new List<VPsiStatisticsByDateEntity> { new VPsiStatisticsByDateEntity { itemName = "出库" } };
        }

        /// <summary>
        /// 入库数量统计
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VPsiStatisticsByDateEntity> GetRkCountByKf(string warehouseId, string organizeId)
        {
            var sql = new StringBuilder(@"SELECT t.rksj statisticsDate, ISNULL(CONVERT(BIGINT,SUM(t.sl)),0) sl, '入库' itemName FROM ( ");
            sql.AppendLine("	SELECT mx.sl,CONVERT(VARCHAR(7), dj.rksj, 120) rksj ");
            sql.AppendLine("	FROM dbo.kf_crkdj(NOLOCK) dj ");
            sql.AppendLine("	INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id ");
            sql.AppendLine("	WHERE dj.zt='" + (int)Enumzt.Enable + "' AND auditState='" + (int)EnumAuditState.Adopt + "' AND dj.rksj>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01') AND dj.rkbm=@warehouseId AND dj.OrganizeId=@OrganizeId ");
            sql.AppendLine(") t ");
            sql.AppendLine("GROUP BY t.rksj ");
            var param = new DbParameter[]
            {
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId??"")
            };
            var result = FindList<VPsiStatisticsByDateEntity>(sql.ToString(), param);
            return result.Count > 0 ? result : new List<VPsiStatisticsByDateEntity> { new VPsiStatisticsByDateEntity { itemName = "入库" } };
        }

        /// <summary>
        /// 损益数量统计
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VPsiStatisticsByDateEntity> GetSyCountByKf(string warehouseId, string organizeId)
        {
            var sql = new StringBuilder(@"SELECT t.sysj statisticsDate, ISNULL(CONVERT(BIGINT,SUM(t.sl)),0) sl, '损益' itemName ");
            sql.AppendLine("FROM (  ");
            sql.AppendLine("	SELECT sy.Sysl/rpu.zhyz sl, CONVERT(VARCHAR(7), sy.CreateTime,120) sysj ");
            sql.AppendLine("	FROM dbo.kc_syxx(NOLOCK) sy ");
            sql.AppendLine("	INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.warehouseId=sy.warehouseId AND rpw.productId=sy.productId AND rpw.zt='1' AND rpw.OrganizeId=sy.OrganizeId ");
            sql.AppendLine("	INNER JOIN dbo.rel_productUnit(NOLOCK) rpu ON rpu.productId=rpw.productId AND rpu.unitId=rpw.unitId AND rpu.zt='1' AND rpu.OrganizeId=sy.OrganizeId ");
            sql.AppendLine("	WHERE sy.warehouseId=@warehouseId AND sy.zt=@zt AND sy.CreateTime>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01') AND sy.OrganizeId=@OrganizeId ");
            sql.AppendLine(") t ");
            sql.AppendLine("GROUP BY t.sysj ");
            var param = new DbParameter[]
            {
                new SqlParameter("@zt", ((int)Enumzt.Enable).ToString()),
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId??"")
            };
            var result = FindList<VPsiStatisticsByDateEntity>(sql.ToString(), param);
            return result.Count > 0 ? result : new List<VPsiStatisticsByDateEntity> { new VPsiStatisticsByDateEntity { itemName = "损益" } };
        }

        /// <summary>
        /// 按单据类型获取入库总数
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<ClassificationStatisticsEntity> GetRkCountByLx(string warehouseId, string organizeId)
        {
            const string sql = @"
SELECT CONVERT(VARCHAR(100),t.djlx) name, ISNULL(CONVERT(BIGINT,SUM(t.sl)),0) y 
FROM (
	SELECT mx.sl,dj.djlx
	FROM dbo.kf_crkdj(NOLOCK) dj
	INNER JOIN dbo.kf_crkmx(NOLOCK) mx ON mx.crkId=dj.Id
	WHERE dj.zt=@zt AND auditState=@shzt AND dj.rksj>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01') AND dj.rkbm=@warehouseId AND dj.OrganizeId=@OrganizeId
) t
GROUP BY t.djlx
";
            var param = new DbParameter[]
            {
                new SqlParameter("@zt", ((int)Enumzt.Enable).ToString()),
                new SqlParameter("@shzt", ((int)EnumAuditState.Adopt).ToString()),
                new SqlParameter("@warehouseId", warehouseId??""),
                new SqlParameter("@OrganizeId", organizeId??"")
            };
            return FindList<ClassificationStatisticsEntity>(sql, param);
        }
        /// <summary>
        /// 获取过期预警和库存预警总条数
        /// </summary>
        /// <param name="kfId"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="gqyjz"></param>
        /// <returns></returns>
        public IList<SysMSGQueryVO> MSGQuery(string kfId, string OrganizeId, int gqyjz)
        {
            const string sqlStr = @"SELECT wzCode,wzName,pc,ph,kykc,kcsl,yxq,typeas from (
	SELECT  wz.Id wzCode,wz.name wzName,''pc,''ph,kcyjz
		,SUM(ISNULL((kcxx.kcsl-kcxx.djsl),0)) kykc,SUM(ISNULL(kcxx.kcsl,0)) kcsl,''yxq,'1'typeas
	FROM [wz_product] (NOLOCK) wz
	INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.OrganizeId = wz.OrganizeId AND rpw.productId = wz.Id 
	INNER JOIN dbo.[kf_kcxx] (NOLOCK) kcxx ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId  and kcxx.warehouseId=@kfId
	WHERE   rpw.warehouseId=@kfId
		AND wz.OrganizeId=@orgId
		AND kcxx.zt='1'
		AND wz.zt='1'
	GROUP BY  wz.Id,wz.name,kcyjz 
	)a where a.kykc<kcyjz
UNION ALL
SELECT wz.Id wzCode,wz.name wzName,kcxx.pc,kcxx.ph,0 kykc,0 kcsl, SUBSTRING(CONVERT(VARCHAR(15),kcxx.yxq, 120),0, 11) yxq,
	case when convert(varchar(10),kcxx.yxq,121)>convert(varchar(10),GETDATE(),121) then '3' else '2' end typeas
FROM dbo.[kf_kcxx](NOLOCK) kcxx 
INNER JOIN [wz_product] wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId AND wz.zt='1' 
INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.OrganizeId = wz.OrganizeId AND rpw.productId = wz.Id AND rpw.warehouseId=@kfId
WHERE kcxx.OrganizeId=@orgId AND kcxx.warehouseId=@kfId
	AND kcxx.zt='1'
	AND kcxx.kcsl>0 
	AND DATEADD(day,@gqyj ,yxq)<GETDATE()  or  DATEDIFF(MONTH, GETDATE(),yxq) <='1'
";
            var partm = new DbParameter[] {
                new SqlParameter("@kfId", kfId),
                new SqlParameter("@orgId", OrganizeId),
                new SqlParameter("@gqyj", gqyjz)
            };
            return FindList<SysMSGQueryVO>(sqlStr, partm);
        }
        #endregion
    }
}
