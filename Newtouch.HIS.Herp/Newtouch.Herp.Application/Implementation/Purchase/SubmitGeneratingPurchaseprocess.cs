using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using FrameworkBase.MultiOrg.Domain.DBContext.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Herp.Domain.DTO;
using Newtouch.Herp.Domain.DTO.OutputDto;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IRepository;
using Newtouch.Herp.Infrastructure.Common;
using Newtouch.Herp.Infrastructure.Enum;
using Newtouch.Infrastructure.EF;
using Newtouch.Tools;

namespace Newtouch.Herp.Application.Implementation
{
    /// <summary>
    /// 提交采购订购单
    /// </summary>
    public class SubmitGeneratingPurchaseprocess : ProcessorFun<SubmitGeneratingPurchaseDto>
    {

        private readonly List<CgOrderDetailEntity> cgOrderDetailEntities;
        private readonly CgOrderEntity cgOrderEntity;
        private object locker = new object();

        private readonly ICgOrderRepo _cgOrderRepo;
        private readonly ICgOrderDetailRepo _cgOrderDetailRepo;

        public SubmitGeneratingPurchaseprocess(SubmitGeneratingPurchaseDto request) : base(request)
        {
            cgOrderEntity = new CgOrderEntity();
            cgOrderDetailEntities = new List<CgOrderDetailEntity>();
        }

        protected override ActResult Validata()
        {
            if (Request == null || Request.cgmx == null) throw new FailedException("采购单明细不能为空");
            try
            {
                Parallel.ForEach(Request.cgmx, p =>
                {
                    if (p.jxcgs <= 0) throw new Exception(string.Format("【{0}】继续采购数必须大于零", p.wzmc));
                    if (string.IsNullOrWhiteSpace(p.gysId)) throw new Exception(string.Format("【{0}】请选择供应商", p.wzmc));
                    if (p.zxqds > 0)
                    {
                        var zsl = Request.cgmx.FindAll(t => t.productId == p.productId).Sum(s => s.jxcgs);
                        if (zsl * p.zhyz < p.zxqds) throw new Exception(string.Format("【{0}】不能低于最小起订数【{1}】{2}", p.wzmc, p.zxqds, p.zxdwmc));
                    }
                });
            }
            catch (Exception e)
            {
                throw new FailedException(e.InnerException == null ? e.Message : e.Message + e.InnerException.Message);
            }

            return base.Validata();
        }

        /// <summary>
        /// 组装数据
        /// </summary>
        /// <param name="actResult"></param>
        protected override void BeforeAction(ActResult actResult)
        {
            var cloneCgmx = Request.cgmx.ToJson().ToObject<List<VCgPurchaseOrderDetailEntity>>();
            var submitTime = DateTime.Now;
            while (cloneCgmx != null && cloneCgmx.Count > 0)
            {
                var curItem = cloneCgmx.FirstOrDefault();
                if (curItem == null) break;
                var subOrderNo = ReceiptNoManage.GetNewOrderNoV1();
                var cgmxByGrooup = cloneCgmx.FindAll(p => p.gysId == curItem.gysId);//cloneCgmx.FindAll(p => p.productId == curItem.productId);
                Parallel.ForEach(cgmxByGrooup, p =>
                {
                    var item = new CgOrderDetailEntity
                    {
                        supplierId = p.gysId,
                        subOrderNo = subOrderNo,
                        pdId = p.pdId,
                        productId = p.productId,
                        sl = p.jxcgs,
                        zhyz = p.zhyz,
                        jj = p.jj,
                        dwmc = p.unitName,
                        unitId = p.unitId,
                        orderProcess = (int)EnumOrderProcess.Waiting,
                        remark = p.remark,
                        zt = ((int)Enumzt.Enable).ToString(),
                        CreatorCode = Request.userCode,
                        CreateTime = submitTime
                    };
                    lock (locker)
                    {
                        cgOrderDetailEntities.Add(item);
                        cloneCgmx.Remove(p);
                    }
                });
            }

            cgOrderEntity.OrganizeId = Request.organizeId;
            cgOrderEntity.CreateTime = submitTime;
            cgOrderEntity.CreatorCode = Request.userCode;
            cgOrderEntity.orderNo = ReceiptNoManage.GetNewOrderNoV1();
            cgOrderEntity.orderProcess = (int)EnumOrderProcess.Waiting;
            cgOrderEntity.orderType = (int)EnumOrderTypeHrp.TempOrder;
            cgOrderEntity.zt = ((int)Enumzt.Enable).ToString();
            cgOrderEntity.remark = Request.remark;
        }

        protected override void Action(ActResult actResult)
        {
            if (cgOrderDetailEntities.Count == 0)
            {
                actResult.IsSucceed = false;
                actResult.ResultMsg = "请确保存在有效的采购单明细";
                return;
            }
            if (_cgOrderRepo.Insert(cgOrderEntity) > 0)
            {
                try
                {
                    using (var db = new EFDbTransaction(new DefaultDatabaseFactory()).BeginTrans())
                    {
                        cgOrderDetailEntities.ForEach(p =>
                        {
                            #region 插入采购单明细

                            p.orderId = cgOrderEntity.Id;
                            db.Insert(p);

                            #endregion 插入采购单明细

                            #region 更新采购计划实际采购数

                            const string sql = @"
UPDATE dbo.cg_purchaseOrderDetail 
SET sjsl=sjsl+@sl, LastModifyTime=@operateTime, LastModifierCode=@userCode 
WHERE Id=@pdId AND zt='1'
";
                            var param1 = new DbParameter[]
                            {
                                new SqlParameter("@sl",p.sl ),
                                new SqlParameter("@operateTime",p.CreateTime ),
                                new SqlParameter("@userCode",p.CreatorCode ),
                                new SqlParameter("@pdId",p.pdId )
                            };
                            db.ExecuteSqlCommandNoLog(sql, param1);

                            #endregion 更新采购计划实际采购数
                        });
                        db.Commit();
                    }
                }
                catch (Exception e)
                {
                    _cgOrderRepo.Delete(cgOrderEntity);
                    actResult.IsSucceed = false;
                    actResult.ResultMsg = e.Message;
                }
            }
            else
            {
                actResult.IsSucceed = false;
                actResult.ResultMsg = "保存采购住订单失败";
            }
        }
    }
}