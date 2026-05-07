using System;
using System.Collections.Generic;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.DTO.OutOrInStoredOperate;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.Infrastructure;
using Newtouch.PDS.Requset;

namespace Newtouch.HIS.Application.Implementation
{
    /// <summary>
    /// 直接出库
    /// </summary>
    public class DeliveryDirectProcess : ProcessorFun<DjInfoDTO>
    {
        private SysMedicineStorageIOReceiptEntity djmain;
        private List<SysMedicineStorageIOReceiptDetailEntity> djmx;
        private readonly IDeliveryDirectDmnService deliveryDirectDmnService;

        public DeliveryDirectProcess(DjInfoDTO request) : base(request)
        {
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        protected override ActResult Validata()
        {
            if (Request == null)
            {
                throw new FailedException("单据信息不能为空");
            }
            if (Request.mx == null || Request.mx.Count <= 0)
            {
                throw new FailedException("单据明细不能为空");
            }
            return new ActResult();
        }

        /// <summary>
        /// 预处理
        /// </summary>
        protected override void BeforeAction(ActResult actResult)
        {
            djmain = new SysMedicineStorageIOReceiptEntity
            {
                Ckbm = Request.ckbm,
                Ckczy = UserIdentity.UserCode,
                CreateTime = DateTime.Now,
                CreatorCode = UserIdentity.UserCode,
                Crkfsdm = Request.fyfs,
                Czsj = DateTime.Now,
                djlx = Request.djlx,
                OrganizeId = OrganizeId,
                Pdh = Request.djh,
                Rkbm = Request.rkbm,
                px = null,
                shzt = ((int)EnumDjShzt.WaitingApprove).ToString(),
                Sqsj = DateTime.Now,
                zt = "1"
            };
            djmain.Create(true, Guid.NewGuid().ToString());
            djmx = new List<SysMedicineStorageIOReceiptDetailEntity>();
            Request.mx.ForEach(p =>
            {
                var item = new SysMedicineStorageIOReceiptDetailEntity
                {
                    cd = null,
                    Ckbmkc = p.ckbmkc ?? 0,
                    Ckzhyz = p.ckzhyz ?? 0,
                    CreateTime = DateTime.Now,
                    CreatorCode = UserIdentity.UserCode,
                    crkId = djmain.crkId,
                    jj = p.jj,
                    Lsj = p.lsj,
                    pc = p.pc,
                    Pfj = p.pfj,
                    Ph = p.ph,
                    Fph=p.fph,
                    Sl = p.sl,
                    Thyy = "",
                    Yklsj = p.yklsj,
                    Ykpfj = p.ykpfj,
                    Ypdm = p.ypdm,
                    Yxq = p.yxq,
                    Zje = p.zje,
                    zt = "1",
                    ckdw = p.ckdw
                };
                item.Create(true, Guid.NewGuid().ToString());
                djmx.Add(item);
            });
        }

        /// <summary>
        /// 主处理
        /// </summary>
        /// <returns></returns>
        protected override void Action(ActResult actResult)
        {
            var result = deliveryDirectDmnService.SubmitDeliveryDirect(djmain, djmx);
            if (!string.IsNullOrWhiteSpace(result))
            {
                actResult.IsSucceed = false;
                actResult.ResultMsg = result;
            }
        }
    }
}
