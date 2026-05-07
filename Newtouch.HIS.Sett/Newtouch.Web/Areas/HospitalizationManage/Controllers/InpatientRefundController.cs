using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Application.Interface.HospitalizationManage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IDomainServices.HospitalizationManage;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects.HospitalizationManage;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.IRepository;

namespace Newtouch.HIS.Web.Areas.HospitalizationManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class InpatientRefundController : ControllerBase
    {
        private readonly IInpatientRefundApp _inpatientRefundApp;
        private readonly IHospItemBillingRepo _hospItemBillingRepo;
        private readonly IInpatientRefundDmnService _inpatientRefundDmnService;
        private readonly IHospDrugBillingRepo _hospDrugBillingRepo;
        private readonly IBookkeepInHosDmnService _BookkeepInHosDmnService;
        private readonly ISysConfigRepo _SysConfigRepo;
        private readonly IHospDrugBillingRepo _hospdrugbillingRepo;

        public override ActionResult  Index()
        {
            return View();

        }
        public ActionResult RefundDetailConfirmForm()
        {
            return View();
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult InpatientRefundQuery(string zyh, DateTime? kssj, DateTime? jssj, string xmlb, string xmmc, string conf = null)
        {
            string ks = "";
            if (!string.IsNullOrWhiteSpace(conf))
            {
                ks = _SysConfigRepo.GetValueByCode(conf, OrganizeId);
            }
            var dto = _inpatientRefundApp.InpatientRefundQuery(zyh, kssj, jssj, ks, xmlb, xmmc);
            var ztlist = dto.InpatientSettleItemBO.TreatmentItemList.Where(p => p.ztbh != null).ToList();
            //收费组套数据处理
            if (ztlist.Count()>0)
            {
                ztlist = ztlist.GroupBy(m => new { m.isYP, m.sfmbmc, m.tdrq, m.ztbh, m.ztsl,m.ztylsl, m.dcztbs }).Select(p =>
                new TreatmentItemFeeDetailVO {
                    isYP = p.Key.isYP,
                    jfbbh = p.Max(m => m.jfbbh),
                    yzwym="",
                    dw = "套",
                    dj = Math.Round(Convert.ToDecimal(p.Sum(m => m.je) / p.Key.ztsl), 2, MidpointRounding.AwayFromZero),
                    sfxm = p.Key.sfmbmc,
                    dl = p.Key.sfmbmc,
                    dlmc = p.Key.sfmbmc,
                    sfxmmc = p.Key.sfmbmc,
                    CreateTime = p.Max(m => m.CreateTime),
                    jmje = Convert.ToDecimal(0.00),
                    jmbl = Convert.ToDecimal(0.00),
                    zfbl = Convert.ToDecimal(0.00),
                    zfxz = null,
                    ybbm =null,
                    tdrq =p.Key.tdrq,
                    sl= p.Key.ztsl ?? 0,
                    tsl= p.Key.ztsl ?? 0,
                    je=p.Sum(m=>m.je),
                    ylsl= p.Key.ztylsl ?? 0,
                    ztbh=p.Key.ztbh,
                    dcztbs=p.Key.dcztbs,
                    ztsl=p.Key.ztsl,
                    jfbbhs= string.Join(",", p.Select(s => s.jfbbh).ToArray())

                }).ToList();
            }
            dto.InpatientSettleItemBO.TreatmentItemList = dto.InpatientSettleItemBO.TreatmentItemList
                .Where(p => p.ztbh == null).ToList().Concat(ztlist).ToList();
            return Success("", dto);
        }
        /// <summary>
        ///保存退费
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SaveRefund(string data,string zyh)
        {
            var orgId = this.OrganizeId;
            if (string.IsNullOrEmpty(orgId))
            {
                throw new FailedCodeException("SYS_GET_ORGANIZATIONAL_FAILURE");
            }
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new FailedException("缺少退费明细");
            }
            var list = Tools.Json.ToList<InpatientRefundJsonVO>(data);
            //项目计费表
            List<HospItemBillingEntity> xmjfbEntitylist = new List<HospItemBillingEntity>();
            //药品计费表
            List<HospDrugBillingEntity> ypjfbEntitylist = new List<HospDrugBillingEntity>();
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.isYp))
                {
                    throw new FailedCodeException("HOSP_BILLING ITEMS_BILLING DRUGS_DISTINGUISH_BETWEEN_FAILURE");
                }
                //是否是非治疗项目 0：非 1：是
                if (item.isYp == "1")
                {
                    if (string.IsNullOrEmpty(item.jfbbh))
                    {
                        throw new FailedCodeException("HOSPREFUND_YPJFBBH_IS_EMPTY");
                    }
                    var jfbbh = Convert.ToInt32(item.jfbbh);
                    var oldDrugEntity = _hospDrugBillingRepo.IQueryable().Where(a => a.jfbbh == jfbbh && a.zt=="1" && a.OrganizeId == orgId ).FirstOrDefault();
                    HospDrugBillingEntity entity = new HospDrugBillingEntity();
                    entity = oldDrugEntity.Clone();
                    entity.jfbbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_ypjfb");
                    entity.sl = 0 - item.tsl.Value;
                    entity.cxzyjfbbh = oldDrugEntity.jfbbh;
                    entity.cxry = OperatorProvider.GetCurrent().UserCode;
                    entity.cxrq = DateTime.Now;
                    entity.Create();  
                    ypjfbEntitylist.Add(entity);
                }
                else if (item.isYp == "0")
                {
                    if (string.IsNullOrEmpty(item.jfbbh))
                    {
                        throw new FailedCodeException("HOSPREFUND_XMJFBBH_IS_EMPTY");
                    }
                    string [] jfbbhar ;
                    jfbbhar = string.IsNullOrWhiteSpace(item.jfbbhs) ? item.jfbbh.Split(',') : item.jfbbhs.Split(',');
                    foreach (var zh in jfbbhar)
                    {
                        var jfbbh = Convert.ToInt32(zh);
                        var oldItemEntity = _hospItemBillingRepo.IQueryable().Where(a => a.jfbbh == jfbbh && a.zt == "1" && a.OrganizeId == orgId).FirstOrDefault();

                        HospItemBillingEntity entity = new HospItemBillingEntity();
                        entity = oldItemEntity.Clone();
                        entity.jfbbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb");
                        entity.sl =string.IsNullOrWhiteSpace(entity.ztbh)? 0 - item.tsl.Value: 0 - (item.tsl.Value*(entity.sl/ entity.ztsl.Value));
                        entity.ztsl = string.IsNullOrWhiteSpace(entity.ztbh) ? 0-entity.ztsl : 0-(entity.ztsl - item.tsl);
                        entity.cxzyjfbbh = oldItemEntity.jfbbh;
                        entity.cxry = OperatorProvider.GetCurrent().UserCode;
                        entity.cxtdrq = DateTime.Now;
                        entity.Create();
                        xmjfbEntitylist.Add(entity);
                    }
                    
                }
            }
            //保存
            string result =_inpatientRefundDmnService.SaveRefund(xmjfbEntitylist, ypjfbEntitylist, zyh, orgId);
            if (string.IsNullOrWhiteSpace(result))
            {
                if (_hospdrugbillingRepo.getHckcWith(this.OrganizeId) > 0) {
                    //退还扣减耗材库存
                    var hclist = xmjfbEntitylist.Where(m => m.yzwym == null && m.sfxm.Contains("wz")).ToList();//住院记账所开耗材
                    var yszhclist = xmjfbEntitylist.Where(m => m.yzwym != null && m.sfxm.Contains("wz")).ToList();//医生站开立耗材
                    if (hclist.Count > 0)
                    {
                        string jfbhString = string.Join(",", xmjfbEntitylist.Where(m => m.sfxm.Contains("wz")).Select(p => p.cxzyjfbbh));
                        _hospdrugbillingRepo.Updatezy_wzkcReturn(this.OrganizeId, jfbhString, this.UserIdentity.UserCode);
                    }
                    if (yszhclist.Count > 0)
                        _hospdrugbillingRepo.Updatezyyz_wzkcReturn(yszhclist, this.OrganizeId, this.UserIdentity.UserCode);
                }
                //更新cis住院患者预览右侧费用卡片信息
                _hospdrugbillingRepo.Updatezy_brxxexpand(this.OrganizeId, zyh);
            }else
                throw new FailedException(result);
            return Success();
        }


    }
}