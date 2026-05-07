using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.IDomainServices.DeptStorageManage;
using Newtouch.Domain.IRepository.DeptStorageManage;
using Newtouch.Domain.ValueObjects.Storage;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.TSql;

namespace Newtouch.DomainServices.DeptStorageManage
{
    /// <summary>
    /// 库存维护
    /// </summary>
    public class KfKcxxDmnService : DmnServiceBase, IKfKcxxDmnService
    {
        private readonly IKfKcxxRepo _kfKcxxRepo;
        // private readonly IWzProductRepo _wzProductRepo;

        public KfKcxxDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

         /// <summary>
         /// 修改库存数量(加库存)，不修改冻结数量  返回影响行
         /// </summary>
         /// <param name="productId"></param>
         /// <param name="pc"></param>
         /// <param name="ph"></param>
         /// <param name="warehouseId"></param>
         /// <param name="sl">最小单位数量</param>
         /// <param name="organizeId"></param>
         /// <param name="userCode"></param>
         /// <returns>返回影响行</returns>
         public int UpdateKcsl(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode)
         {
             if (sl >= 0) return AddKcsl(productId, pc, ph, warehouseId, sl, organizeId, userCode);
             var subtractResult = JustSubtractKcsl(new UpdateKcslDTO { organizeId = organizeId, sl = (-1 * sl), userCode = userCode, ks = warehouseId,pc = pc, ph = ph, productId = productId });
             if (!string.IsNullOrWhiteSpace(subtractResult)) throw new System.Exception(subtractResult);
             return 1;
         }
//
//         /// <summary>
//         /// 修改库存数量，不修改冻结数量  该方法为批量修改相同批次批号的物资，慎用
//         /// </summary>
//         /// <param name="updateKcslDto"></param>
//         /// <returns></returns>
//         public string UpdateKcsl(List<UpdateKcslDTO> updateKcslDto)
//         {
//             if (updateKcslDto == null || updateKcslDto.Count <= 0) return "请传入需要修改库存信息";
//             using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
//             {
//                 foreach (var p in updateKcslDto)
//                 {
//                     if (UpdateKcsl(p.productId, p.pc, p.ph, p.warehouseId, p.sl, p.organizeId, p.userCode) <= 0)
//                     {
//                         var wz = _wzProductRepo.FindEntity(o => o.Id == p.productId && o.OrganizeId == p.organizeId && o.zt == ((int)Enumzt.Enable).ToString());
//                         return string.Format("修改物资【{0}】的库存失败", wz.name ?? "");
//                     }
//                 }
//                 db.Commit();
//             }
//             return "";
//         }
//
        /// <summary>
        /// 追加库存，不修改冻结数量 返回影响行
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="pc"></param>
        /// <param name="ph"></param>
        /// <param name="warehouseId"></param>
        /// <param name="sl">最小单位数量  必须为正数</param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns>返回影响行</returns>
        public int AddKcsl(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode)
        {
            if (sl <= 0) throw new System.Exception("数量必须为正数");
            const string sql = @"
UPDATE dbo.dept_kcxx 
SET kcsl=kcsl+@sl, LastModifierCode=@userCode, LastModifyTime=GETDATE()
WHERE Id=(SELECT TOP 1 Id FROM dbo.dept_kcxx WHERE productId=@proId 
    AND ks=@warehouseId 
    AND OrganizeId=@OrganizeId
    AND pc=@pc
    AND ph=@ph
    AND zt='1' 
)
AND ks=@warehouseId 
AND OrganizeId=@OrganizeId
AND pc=@pc
AND ph=@ph
AND zt='1'
SELECT @@ROWCOUNT ;
";
            var param = new DbParameter[]
            {
                new SqlParameter("@proId", productId),
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@userCode", userCode),
                new SqlParameter("@sl", sl),
                new SqlParameter("@pc", pc),
                new SqlParameter("@ph", ph)
            };
            return FirstOrDefault<int>(sql, param);
        }
//
         // /// <summary>
         // /// 修改库存数量，不修改冻结数量
         // /// </summary>
         // /// <param name="updateKcslDto"></param>
         // /// <returns></returns>
         // public string JustSubtractKcsl(List<UpdateKcslDTO> updateKcslDto)
         // {
         //     if (updateKcslDto == null || updateKcslDto.Count <= 0) return "库存信息不能为空";
         //     using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
         //     {
         //         foreach (var p in updateKcslDto)
         //         {
         //             var result = JustSubtractKcsl(p);
         //             if (!string.IsNullOrWhiteSpace(result)) return result;
         //         }
         //         db.Commit();
         //     }
         //     return "";
         // }
//
//         /// <summary>
//         /// 修改库存数量，不修改冻结数量
//         /// </summary>
//         /// <param name="updateKcslDto"></param>
//         /// <param name="db"></param>
//         /// <returns></returns>
//         public string JustSubtractKcslNoTrans(List<UpdateKcslDTO> updateKcslDto, Newtouch.Infrastructure.EF.EFDbTransaction db = null)
//         {
//             if (updateKcslDto == null || updateKcslDto.Count <= 0) return "库存信息不能为空";
//             foreach (var p in updateKcslDto)
//             {
//                 var result = JustSubtractKcsl(p, db);
//                 if (!string.IsNullOrWhiteSpace(result)) return result;
//             }
//             return "";
//         }
//
         /// <summary>
         /// 修改库存数量，不修改冻结数量
         /// </summary>
         /// <param name="updateKcslDto"></param>
         /// <param name="db"></param>
         /// <returns></returns>
         public string JustSubtractKcsl(UpdateKcslDTO updateKcslDto, Newtouch.Infrastructure.EF.EFDbTransaction db = null)
         {
             if (updateKcslDto == null) return "库存信息不能为空";
             var param = new DbParameter[]
             {
                 new SqlParameter("@proId",updateKcslDto.productId),
                 new SqlParameter("@warehouseId", updateKcslDto.ks),
                 new SqlParameter("@OrganizeId", updateKcslDto.organizeId),
                 new SqlParameter("@userCode", updateKcslDto.userCode),
                 new SqlParameter("@sl", updateKcslDto.sl),
                 new SqlParameter("@pc", updateKcslDto.pc),
                 new SqlParameter("@ph", updateKcslDto.ph)
             };
             return db != null ? db.FirstOrDefault<string>(StockOperate.just_subtract_kcsl, param)
                 : FirstOrDefault<string>(StockOperate.just_subtract_kcsl, param);
         }
//
//         /// <summary>
//         /// 减库存数量、冻结数量（出库可用）
//         /// </summary>
//         /// <param name="productId"></param>
//         /// <param name="pc"></param>
//         /// <param name="ph"></param>
//         /// <param name="warehouseId"></param>
//         /// <param name="sl">最小单位数量</param>
//         /// <param name="organizeId"></param>
//         /// <param name="userCode"></param>
//         /// <returns></returns>
//         public string SubtractKcslAndDjsl(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode)
//         {
//             var param = new DbParameter[]
//             {
//                 new SqlParameter("@proId", productId),
//                 new SqlParameter("@warehouseId", warehouseId),
//                 new SqlParameter("@OrganizeId", organizeId),
//                 new SqlParameter("@userCode", userCode),
//                 new SqlParameter("@sl", sl),
//                 new SqlParameter("@pc", pc),
//                 new SqlParameter("@ph", ph)
//             };
//             return FirstOrDefault<string>(StockOperate.subtract_freeze_and_kcsl, param);
//         }
//
//         /// <summary>
//         /// 解冻
//         /// </summary>
//         /// <param name="productId"></param>
//         /// <param name="pc"></param>
//         /// <param name="ph"></param>
//         /// <param name="warehouseId"></param>
//         /// <param name="sl">最小单位数量</param>
//         /// <param name="organizeId"></param>
//         /// <param name="userCode"></param>
//         /// <returns></returns>
//         public string Unfreeze(string productId, string pc, string ph, string warehouseId, int sl, string organizeId, string userCode)
//         {
//             var param = new DbParameter[]
//             {
//                 new SqlParameter("@proId", productId),
//                 new SqlParameter("@warehouseId", warehouseId),
//                 new SqlParameter("@OrganizeId", organizeId),
//                 new SqlParameter("@userCode", userCode),
//                 new SqlParameter("@sl", sl),
//                 new SqlParameter("@pc", pc),
//                 new SqlParameter("@ph", ph)
//             };
//             return FirstOrDefault<string>(StockOperate.just_unfreeze, param);
//         }
//
//         /// <summary>
//         /// 冻结库存
//         /// </summary>
//         /// <param name="sl">最小单位数量</param>
//         /// <param name="pc"></param>
//         /// <param name="ph"></param>
//         /// <param name="productId"></param>
//         /// <param name="organizeId"></param>
//         /// <param name="warehouseId"></param>
//         /// <param name="userCode"></param>
//         /// <returns></returns>
//         public string FrozenKc(int sl, string pc, string ph, string productId, string organizeId, string warehouseId, string userCode)
//         {
//             var param = new DbParameter[]
//             {
//                 new SqlParameter("@proId", productId),
//                 new SqlParameter("@pc", pc),
//                 new SqlParameter("@ph", ph),
//                 new SqlParameter("@OrganizeId", organizeId),
//                 new SqlParameter("@warehouseId", warehouseId),
//                 new SqlParameter("@userCode", userCode),
//                 new SqlParameter("@sl", sl)
//             };
//             return FindList<string>(StockOperate.just_frozen_kcsl, param.ToArray()).FirstOrDefault();
//         }

        // /// <summary>
        // /// 外部入库 扣库存(作废)
        // /// </summary>
        // /// <param name="sl">最小单位数量</param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="productId"></param>
        // /// <param name="organizeId"></param>
        // /// <param name="warehouseId"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // public string WbrkKkc(int sl, string pc, string ph, string productId, string organizeId, string warehouseId, string userCode)
        // {
        //     var param = new DbParameter[]
        //     {
        //         new SqlParameter("@pc", pc),
        //         new SqlParameter("@ph", ph),
        //         new SqlParameter("@OrganizeId", organizeId),
        //         new SqlParameter("@warehouseId", warehouseId),
        //         new SqlParameter("@proId", productId),
        //         new SqlParameter("@zt", ((int) Enumzt.Enable).ToString()),
        //         new SqlParameter("@sl", sl),
        //         new SqlParameter("@userCode", userCode)
        //     };
        //     return FindList<string>(StockOperate.cancel_subtract_kcsl, param.ToArray()).FirstOrDefault();
        // }

        // /// <summary>
        // /// 撤销入库（直接出库作废） 若要使用该方法请套上事务，方法内不做回滚
        // /// </summary>
        // /// <param name="sl">最小单位数量</param>
        // /// <param name="pc"></param>
        // /// <param name="ph"></param>
        // /// <param name="productId"></param>
        // /// <param name="organizeId"></param>
        // /// <param name="rkbm"></param>
        // /// <param name="ckbm"></param>
        // /// <param name="userCode"></param>
        // /// <returns></returns>
        // public string RevokeInstorage(int sl, string pc, string ph, string productId, string organizeId, string rkbm, string ckbm, string userCode)
        // {
        //     //入库部门扣库存
        //     var substractResult = JustSubtractKcsl(new UpdateKcslDTO { organizeId = organizeId, pc = pc, ph = ph, productId = productId, sl = sl, userCode = userCode, warehouseId = rkbm });
        //     if (!string.IsNullOrWhiteSpace(substractResult)) return substractResult;
        //
        //     //出库部门还库存
        //     if (UpdateKcsl(productId, pc, ph, ckbm, sl, organizeId, userCode) <= 0)
        //     {
        //         return "还库存失败";
        //     }
        //     return "";
        // }

        /// <summary>
        /// 获取库存信息 结转用
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<VKcxxEntity> GetKcxxForJz(string warehouseId, string organizeId)
        {
            const string sql = @"
SELECT kcxx.productId, kcxx.ph, kcxx.pc, kcxx.yxq, kcxx.kcsl, ISNULL(CONVERT(NUMERIC(11,4),wz.lsj*kcxx.zhyz),0) bmlsj, kcxx.jj, kcxx.zhyz
FROM dbo.kf_kcxx(NOLOCK) kcxx
INNER JOIN dbo.rel_productWarehouse(NOLOCK) rpw ON rpw.productId=kcxx.productId AND rpw.warehouseId=kcxx.warehouseId AND rpw.OrganizeId=kcxx.OrganizeId AND rpw.zt='1'
INNER JOIN dbo.wz_product(NOLOCK) wz ON wz.Id=kcxx.productId AND wz.OrganizeId=kcxx.OrganizeId AND wz.zt='1'
WHERE kcxx.OrganizeId=@OrganizeId
AND kcxx.warehouseId=@warehouseId
AND kcxx.zt=@zt
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@warehouseId", warehouseId),
                new SqlParameter("@zt", ((int) Enumzt.Enable).ToString()),
            };
            return FindList<VKcxxEntity>(sql, param);
        }

        //        /// <summary>
        //        /// 撤销入库（直接出库作废）
        //        /// </summary>
        //        /// <param name="sl">最小单位数量</param>
        //        /// <param name="pc"></param>
        //        /// <param name="ph"></param>
        //        /// <param name="productId"></param>
        //        /// <param name="organizeId"></param>
        //        /// <param name="rkbm"></param>
        //        /// <param name="ckbm"></param>
        //        /// <param name="userCode"></param>
        //        /// <returns></returns>
        //        public string RevokeInstorage(int sl, string pc, string ph, string productId, string organizeId, string rkbm, string ckbm, string userCode)
        //        {
        //            const string sql = @"
        //DECLARE @rkkcsl INT;--入库库存
        //DECLARE @rkdjsl INT;--入库冻结库存
        //DECLARE @res INT;
        //SELECT @rkkcsl=kcsl, @rkdjsl=djsl FROM dbo.kf_kcxx WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt=@zt AND warehouseId=@rkbm
        //IF @sl <= (@rkkcsl-@rkdjsl)
        //BEGIN
        //	UPDATE dbo.kf_kcxx 
        //	SET kcsl=kcsl-@sl, LastModifierCode=@userCode, LastModifyTime=GETDATE() 
        //	WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt=@zt AND warehouseId=@rkbm;
        //	SELECT @res=@@ROWCOUNT;
        //	IF @res>0
        //	BEGIN
        //		UPDATE dbo.kf_kcxx
        //		SET kcsl=kcsl+@sl, LastModifierCode=@userCode, LastModifyTime=GETDATE()
        //		WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt=@zt AND warehouseId=@ckbm;
        //		SELECT @res=@@ROWCOUNT ;
        //		IF @res>0
        //		BEGIN
        //			SELECT '';
        //		END
        //		ELSE
        //		BEGIN
        //			SELECT '出库部门入库存失败';
        //		END
        //	END 
        //	ELSE
        //	BEGIN
        //		SELECT '入库部门出库存失败';
        //	END
        //END
        //ELSE 
        //BEGIN
        //	SELECT '库存不足';
        //END 
        //";
        //            var param = new DbParameter[]
        //            {
        //                new SqlParameter("@pc", pc),
        //                new SqlParameter("@ph", ph),
        //                new SqlParameter("@OrganizeId", organizeId),
        //                new SqlParameter("@rkbm", rkbm),
        //                new SqlParameter("@ckbm", ckbm),
        //                new SqlParameter("@proId", productId),
        //                new SqlParameter("@zt", ((int) Enumzt.Enable).ToString()),
        //                new SqlParameter("@sl", sl),
        //                new SqlParameter("@userCode",userCode)
        //            };
        //            return FindList<string>(sql, param.ToArray()).FirstOrDefault();
        //        }
    }
}
