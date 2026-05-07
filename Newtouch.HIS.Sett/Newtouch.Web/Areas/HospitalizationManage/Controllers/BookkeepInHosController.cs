using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Common;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Security;
using Newtouch.Core.Common.Utils;
using Newtouch.HIS.Application;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Application.Interface.OutpatientManage;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.InputDto.OutpatientAccounting;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using Newtouch.Tools.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.HospitalizationManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class BookkeepInHosController : ControllerBase
    {
        private readonly IPatientBasicInfoDmnService _PatientBasicInfoDmnService;
        private readonly IBookkeepInHosDmnService _BookkeepInHosDmnService;
        private readonly ISysPatiChargeAddApp _SysPatiChargeAddApp;
        private readonly ISysPatiChargeWaiverApp _SysPatiChargeWaiverApp;
        private readonly IHospItemFeeApp _HospItemFeeApp;
        private readonly IHospMedicinFeeApp _HospMedicinFeeApp;
        private readonly IHospSettApp _OutHospBillApp;
        private readonly IOutPatChargeApp _outPatChargeApp;
        private readonly IBookkeepInpatientApp _bookkeepInpatientApp;
        private readonly ISysUserDmnService _sysUserDmnService;
        private readonly IMoneyUpperLimitReminderDmnService _moneyUpperLimitReminderDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISyncTreatmentServiceRecordDmnService _syncTreatmentServiceRecordDmnService;
        private readonly IOutPatientDmnService _outPatientDmnService;
        private readonly IRefundDmnService _refundDmnService;
        private readonly IHosPatAccDmnService _hosPatAccDmnService;
        private readonly IHospSettlementRepo _hospSettlementRepo;
        private readonly IHospItemBillingRepo _hospItemBillingRepo;
        private readonly IHospAccountingPlanDetailRepo _hospAccountingPlanDetailRepo;
        private readonly IItemDmnService _itemDmnService;
        private readonly ISysStaffRepo _sysStaffRepo;
        private readonly IOutPatientApp _outpatientApp;
        private readonly IHospPatientBasicInfoRepo _hospPatientBasicInfoRepo;
        private readonly IAccountManageApp _accountmanageApp;
        private readonly ISysChargeTemplateRepo _sysChargeTemplateRepo;
        private readonly ISysChargeTemplateItemMappRepo _sysChargeTemplateItemMappRepo;
        private readonly IHospDrugBillingRepo _hospdrugbillingRepo;
        
        #region HIS住院记账 选择计费项目（药品），填写数量等，直接往zy_xmjfb zy_ypjfb中写计费数据


        public override ActionResult Index()
        {
            var mayMedicine = _sysConfigRepo.GetBoolValueByCode("HOSP_AccountingPage_May_Medicine", OrganizeId);
            if (!(mayMedicine == false))
            {
                ViewBag.ISMedicineSearchRelatedKC = _sysConfigRepo.GetBoolValueByCode("IS_MedicineSearchRelatedKC", OrganizeId);
            }
            ViewBag.ISAccountingPageMayMedicine = mayMedicine;
            var ss = _sysConfigRepo.GetValueByCode("OperationDeptConfig", OrganizeId);
            ViewBag.staffDept = this.UserIdentity.DepartmentCode;
            ViewBag.staffDeptName = this.UserIdentity.DepartmentName;
            ViewBag.OperationDeptConfig = ss == null ? "" : ss;
            return base.Index();
        }
        
        /// <summary>
        /// 根据卡号或者住院号查询住院病人基本信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetGridJson(string kh, string zyh)
        {
            if (!string.IsNullOrWhiteSpace(kh) || !string.IsNullOrWhiteSpace(zyh))
            {
                var data = _PatientBasicInfoDmnService.GetSysBasicByZHY(kh, zyh, this.OrganizeId);//[0];
                if (data.Count > 0)
                {
                    #region 计算未结金额
                    if (!string.IsNullOrWhiteSpace(data[0].zyh))
                    {
                        var dlentitys = _OutHospBillApp.GetHospFeeClassifyWithDLBOList(data[0].zyh);
                        decimal wjje = 0;
                        if (dlentitys.Count != 0 && dlentitys != null)
                        {
                            for (int i = 0; i < dlentitys.Count; i++)
                            {
                                wjje += dlentitys[i].jsje;
                            }
                        }
                        data[0].wjje = wjje;
                    }
                    #endregion

                    return Content(data[0].ToJson());
                }
                else
                {
                    return Error("该病人不存在！");
                }
            }
            else
            {
                return Error("卡号或者住院号为空！");
            }

        }

        /// <summary>
        /// 获取药品和收费项目
        /// </summary>
        /// <param name="keyword">关键字查询条件</param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetYPItemInfo(string keyword)
        {
            List<ChargeItemDetailVO> ypItemList = _outPatChargeApp.GetYpItemInfo(keyword);
            // return Content(ypItemList.ToJson());
            return Content(ypItemList.ToJson());
        }

        /// <summary>
        /// 保存数据时的操作
        /// </summary>
        /// <param name="xmjf"></param>
        /// <returns></returns>
        public ActionResult AddData(string zyh,List<ItemFeeVO> ItemFeeVO)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("缺少住院号");
            }
            if (ItemFeeVO != null && ItemFeeVO.Count > 0)
            {
                var zyxx=  _hospPatientBasicInfoRepo.FindEntity(p=>p.zyh==zyh&&p.OrganizeId==this.OrganizeId);
                if (zyxx.zybz==((int)EnumZYBZ.Wry).ToString()||zyxx.zybz==((int)EnumZYBZ.Ycy).ToString())
                {
                    throw new FailedException("病人不在院");
                }
                DateTime? cqrq = _PatientBasicInfoDmnService.IFCQRQISJZSJ(zyh,this.OrganizeId);
                //项目计费表
                List<HospItemBillingEntity> xmjfbEntitylist = new List<HospItemBillingEntity>();
                //药品计费表
                List<HospDrugBillingEntity> ypjfbEntitylist = new List<HospDrugBillingEntity>();
                for (int i = 0; i < ItemFeeVO.Count; i++)
                {
                    if (ItemFeeVO[i].tdrq> cqrq)
                    {
                        return Error("记账日期不能晚于病人出区日期!");
                    }
                    if (ItemFeeVO[i].ryrq > ItemFeeVO[i].tdrq && (ItemFeeVO[i].tdrq != new DateTime(0001, 01, 01)))
                    {
                        return Error("记账日期不能早于病人入院日期!");
                    }
                    if ((ItemFeeVO[i].cyrq != new DateTime(0001, 01, 01)) && ItemFeeVO[i].cyrq < ItemFeeVO[i].tdrq)
                    {
                        return Error("记账日期不能晚于病人出院日期!");
                    }
                    if (string.IsNullOrWhiteSpace(ItemFeeVO[i].yzlx))
                    {
                        return Error("缺少项目类型!");
                    }
                    if (ItemFeeVO[i].yzlx == ((int)EnumxmType.XM).ToString())
                    {
                        #region 项目计费表
                        if (ItemFeeVO[i].zfxz=="9")
                        {
                            var mbmx = _sysChargeTemplateItemMappRepo.getmbmx(ItemFeeVO[i].sfxm, this.OrganizeId);
                            if (mbmx == null|| mbmx.Count==0)
                            {
                                throw new FailedException("未找到对应收费组套明细");
                            }
                            var newid= Guid.NewGuid().ToString();
                            for (int k = 0; k < mbmx.Count; k++)
                            {
                                HospItemBillingEntity zyXmjfb = new HospItemBillingEntity();
                                zyXmjfb.zyh = zyh;
                                zyXmjfb.tdrq = ItemFeeVO[i].tdrq;
                                zyXmjfb.sfxm = mbmx[k].sfxmcode;
                                zyXmjfb.dl = mbmx[k].sfdlcode;
                                zyXmjfb.ys = ItemFeeVO[i].ys;
                                zyXmjfb.ks = ItemFeeVO[i].ks;
                                zyXmjfb.ysmc = ItemFeeVO[i].ysmc;
                                zyXmjfb.ksmc = ItemFeeVO[i].ksmc;
                                zyXmjfb.bq = zyxx.bq;
                                zyXmjfb.cw = zyxx.cw;
                                zyXmjfb.dj = mbmx[k].dj;
                                zyXmjfb.sl = mbmx[k].sl.ToDecimal()* ItemFeeVO[i].sl;
                                zyXmjfb.jfdw = mbmx[k].dw;
                                zyXmjfb.zxks = this.UserIdentity.DepartmentCode;

                                zyXmjfb.yzxz = ((int)EnumYZXZ.LSYZ).ToString();
                                zyXmjfb.yzzt = ((int)EnumYZZT.WCX).ToString();
                                //zyXmjfb.zfxz = ItemFeeVO[i].zfxz;
                                zyXmjfb.zfxz = _BookkeepInHosDmnService.getzfxzById(mbmx[k].sfxmcode,this.OrganizeId);
                                zyXmjfb.zfbl = ItemFeeVO[i].zfbl;
                                zyXmjfb.dcztbs = newid;
                                zyXmjfb.cxzyjfbbh = 0;
                                zyXmjfb.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                                zyXmjfb.ztbh = mbmx[k].sfmbbh;
                                zyXmjfb.ztsl= ItemFeeVO[i].ztsl;
                                zyXmjfb.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb"));
                                xmjfbEntitylist.Add(zyXmjfb);
                                //_HospItemFeeApp.SubmitForm(zyXmjfb, null);
                            }
                        }
                        else
                        {
                            HospItemBillingEntity zyXmjfb = new HospItemBillingEntity();
                            zyXmjfb.zyh = zyh;
                            zyXmjfb.tdrq = ItemFeeVO[i].tdrq;
                            zyXmjfb.sfxm = ItemFeeVO[i].sfxm;
                            zyXmjfb.dl = ItemFeeVO[i].dl;
                            zyXmjfb.ys = ItemFeeVO[i].ys;
                            zyXmjfb.ks = ItemFeeVO[i].ks;
                            zyXmjfb.ysmc = ItemFeeVO[i].ysmc;
                            zyXmjfb.ksmc = ItemFeeVO[i].ksmc;
                            zyXmjfb.bq = zyxx.bq;
                            zyXmjfb.cw = zyxx.cw;
                            zyXmjfb.dj = ItemFeeVO[i].dj;
                            zyXmjfb.sl = ItemFeeVO[i].sl;
                            zyXmjfb.jfdw = ItemFeeVO[i].dw;
                            zyXmjfb.zxks = this.UserIdentity.DepartmentCode;

                            zyXmjfb.yzxz = ((int)EnumYZXZ.LSYZ).ToString();
                            zyXmjfb.yzzt = ((int)EnumYZZT.WCX).ToString();
                            zyXmjfb.zfxz = ItemFeeVO[i].zfxz;
                            zyXmjfb.zfbl = ItemFeeVO[i].zfbl;
                            zyXmjfb.cxzyjfbbh = 0;
                            zyXmjfb.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                            //_HospItemFeeApp.SubmitForm(zyXmjfb, null);
                            zyXmjfb.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb"));
                            xmjfbEntitylist.Add(zyXmjfb);
                        }
                        #endregion
                    }
                    else
                    {
                        #region 药品计费表
                        HospDrugBillingEntity zyYpjfb = new HospDrugBillingEntity();
                        zyYpjfb.zyh = zyh;
                        zyYpjfb.tdrq = ItemFeeVO[i].tdrq;
                        zyYpjfb.yp = ItemFeeVO[i].sfxm;
                        zyYpjfb.dl = ItemFeeVO[i].dl;
                        zyYpjfb.ys = ItemFeeVO[i].ys;
                        zyYpjfb.ysmc = ItemFeeVO[i].ysmc;
                        zyYpjfb.ks = ItemFeeVO[i].ks;
                        zyYpjfb.ksmc = ItemFeeVO[i].ksmc;
                        zyYpjfb.bq = zyxx.bq;
                        zyYpjfb.cw = zyxx.cw;
                        zyYpjfb.dj = ItemFeeVO[i].dj;
                        zyYpjfb.sl = ItemFeeVO[i].sl;
                        zyYpjfb.jfdw = ItemFeeVO[i].dw;

                        zyYpjfb.lyyf = ItemFeeVO[i].yfdm;
                        zyYpjfb.yzxz = ((int)EnumYZXZ.LSYZ).ToString();
                        zyYpjfb.yzzt = ((int)EnumYZZT.WCX).ToString();
                        zyYpjfb.zfxz = ItemFeeVO[i].zfxz;
                        zyYpjfb.zfbl = ItemFeeVO[i].zfbl;
                        zyYpjfb.cxzyjfbbh = 0;
                        zyYpjfb.ybbm = ItemFeeVO[i].ybbm;
                        zyYpjfb.yfdm = ItemFeeVO[i].yfdm;
                        zyYpjfb.OrganizeId = OperatorProvider.GetCurrent().OrganizeId;
                        zyYpjfb.cls = (short?)ItemFeeVO[i].cls;
                        zyYpjfb.zxks = ItemFeeVO[i].yfdm; //药品补记账  默认执行科室为
                        zyYpjfb.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_ypjfb"));
                        //_HospMedicinFeeApp.SubmitForm(zyYpjfb, null);
                        ypjfbEntitylist.Add(zyYpjfb);
                        //_hospdrugbillingRepo.Updatezyaddfee(OrganizeId, ItemFeeVO[i].sl, ItemFeeVO[i].yfdm, ItemFeeVO[i].sfxm);
                        #endregion
                    }
                }
                //存在物资耗材做库存验证
                var wzList = xmjfbEntitylist.Where(p => p.sfxm.Contains("wz")).ToList();
                if (wzList.Count>0)
                {
                    var kclist= _BookkeepInHosDmnService.Verification(string.Join(",", wzList.Where(m => m.sfxm.Contains("wz")).Select(p => p.sfxm)),this.OrganizeId, ItemFeeVO[0].yfdm);
                    foreach (var item in wzList) {
                        var resultNum = kclist.Where(p => p.productCode == item.sfxm && p.kykc >= item.sl).Count();
                        if(resultNum == 0)
                            throw new FailedException("编码为："+item.sfxm +" 库存不足！");
                    }
                }
                //保存
                string result=_BookkeepInHosDmnService.SaveFee(xmjfbEntitylist, ypjfbEntitylist);
                if (string.IsNullOrWhiteSpace(result))
                {
                    string jfbhs = string.Join(",", wzList.Where(m => m.sfxm.Contains("wz")).Select(p => p.jfbbh));
                    _hospItemBillingRepo.Updatezyaddfee(OrganizeId, jfbhs,ItemFeeVO[0].yfdm,this.UserIdentity.UserCode);
                    //更新cis住院患者预览右侧费用卡片信息
                    _hospdrugbillingRepo.Updatezy_brxxexpand(OrganizeId, zyh);
                }
                else
                    throw new FailedException(result);
            }
            else
            {
                throw new FailedException("不存在记账信息！");
            }
            return Success("保存成功！");
        }

        public ActionResult ChargeItemTemplate()
        {
            return View();
        }

        /// <summary>
        /// 根据科室 获取收费项目模板
        /// </summary>
        /// <returns></returns>
        public ActionResult GetChargeItemData(string ks)
        {
            var data = _BookkeepInHosDmnService.GetChargeTemplate(ks, OrganizeId);
            if (data != null)
            {
                return Content(data.ToJson());
            }
            return View();
        }

        /// <summary>
        /// 根据收费模板编号 获取模板内容 （项目列表）
        /// </summary>
        /// <param name="sfxmbh"></param>
        /// <returns></returns>
        public ActionResult loadSfxmDataBySfmbbh(string sfmbbh)
        {
            var data = _BookkeepInHosDmnService.GetChargeItemContent(sfmbbh, this.OrganizeId);
            if (data != null)
            {
                return Content(data.ToJson());
            }
            return Content("[]");
        }

        #endregion HIS 住院记账

        #region 住院记账 计划记账，写jzjh，通过执行，往zy_xmjfb zy_ypjfb中写计费数据 2018.03部分门诊计划的逻辑也在这

        #region

        /// <summary>
        /// 住院记账
        /// 记账方式：长期 or 临时
        /// </summary>
        /// <returns></returns>
        public ActionResult Accounting()
        {
            //治疗师记账金额上限开关
            var IsUpperLimitReminder = _sysConfigRepo.GetValueByCode("xt_UpperLimitReminder", this.OrganizeId);
            ViewBag.IsUpperLimitReminder = IsUpperLimitReminder;
            ViewBag.IsBlh = _sysConfigRepo.GetByCode("IsBlh", OrganizeId) == null ? "OFF" : _sysConfigRepo.GetByCode("IsBlh", OrganizeId).Value;
            var isRehabDoctor = _sysUserDmnService.CheckStaffIsBelongDuty(this.UserIdentity.StaffId, "RehabDoctor");
            if (isRehabDoctor)
            {
                ViewBag.CurYsStaffId = this.UserIdentity.StaffId;
                ViewBag.CurYs = this.UserIdentity.rygh;
                ViewBag.CurYsmc = this.UserIdentity.UserName;
                ViewBag.CurKs = this.UserIdentity.DepartmentCode;
                ViewBag.CurKsmc = this.UserIdentity.DepartmentName;
            }

            //计划记账 单价 可编辑 开关配置
            ViewBag.jhjz_dj_editable = _sysConfigRepo.GetBoolValueByCode("jhjz.dj.editable", OrganizeId);

            return View();
        }

        /// <summary>
        /// 执行记账 计划 执行页
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountingExecution()
        {
            var isRehabDoctor = _sysUserDmnService.CheckStaffIsBelongDuty(this.UserIdentity.StaffId, "RehabDoctor");
            if (isRehabDoctor)
            {
                //ViewBag.CurYsStaffId = this.UserIdentity.StaffId;
                ViewBag.CurYs = this.UserIdentity.rygh;
            }


            ViewBag.OrgId = this.OrganizeId;
            return View();
        }

        public ActionResult ExecutionDateComfirm()
        {
            ViewBag.DefaultPrevDate = _sysConfigRepo.GetIntValueByCode("AccountingExecution_DefaultPrevDate", this.OrganizeId, 3);

            return View();
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountingExecutionQuery()
        {
            return View();
        }

        /// <summary>
        /// 记账计划 查询
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountingPlan()
        {
            return View();
        }

        /// <summary>
        /// Pop层 停止记账计划 选择 预（停止）日期
        /// </summary>
        /// <returns></returns>
        public ActionResult StopPlan()
        {
            return View();
        }

        /// <summary>
        /// 根据住院号 获取 病人 记账 信息
        /// </summary>
        /// <param name="zyh">住院号</param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetAccountingStatusDetail(string zyh)
        {
            var data = _bookkeepInpatientApp.GetAccountingStatusDetail(zyh);
            data.patInfo.cqrq= _PatientBasicInfoDmnService.IFCQRQISJZSJ(zyh, this.OrganizeId);
            return Success(null, data);
        }

        /// <summary>
        /// 提交记账计划
        /// </summary>
        /// <param name="zyh">住院号</param>
        /// <param name="xmList">记账项目列表</param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SubmitAccountingPlan(string zyh, IList<InpatientAccountingPlanItemDto> xmList)
        {
            _bookkeepInpatientApp.SubmitAccountingPlan(zyh, xmList);
            return Success("提交成功");
        }

        /// <summary>
        /// 取消预停止
        /// </summary>
        /// <param name="jzjhmxIdStr"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult CancelPreStopAccountingPlan(string jzjhmxIdStr)
        {
            _bookkeepInpatientApp.CancelPreStopAccountingPlan(jzjhmxIdStr);
            return Success("操作成功");
        }

        /// <summary>
        /// 查询 待执行 记账计划 列表
        /// </summary>
        /// <param name="zyhList"></param>
        /// <param name="zxrq"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult WaitingAccountingPlanQuery(string zyhStr, string from = null)
        {

            var list = _bookkeepInpatientApp.WaitingAccountingPlanQuery((zyhStr ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray(),from);
            return Content(list.ToJson());
        }

        /// <summary>
        /// 执行记账计划
        /// </summary>
        /// <param name="jzjhmxIdStr"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult ExecuteAccountingPlan(DateTime? zxrq, IList<zxGirdDto> zxItem, string from = null)
        {
            _bookkeepInpatientApp.ExecuteAccountingPlan(zxItem, zxrq, from);
            return Success("成功执行，已计费");
        }

        /// <summary>
        /// 终止 记账计划
        /// </summary>
        /// <param name="jzjhmxIdStr"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult StopAccountingPlan(string jzjhmxIdStr, DateTime? stopDate
            , string from)
        {
            var mxIdList = (jzjhmxIdStr ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (mxIdList == null || mxIdList.Length == 0)
            {
                throw new FailedException("缺少记账计划参数");
            }
            if (from == "mz")
            {
                foreach (var mxId in mxIdList)
                {
                    _outPatientDmnService.overAccountingPlan(mxId, this.OrganizeId, this.UserIdentity.rygh);
                }
            }
            else
            {
                foreach (var mxId in mxIdList)
                {
                    _BookkeepInHosDmnService.StopAccountingPlan(mxIdList.ToList(), DateTime.Now.Date, this.UserIdentity.DepartmentCode, this.UserIdentity.rygh);
                }
            }
            return Success("操作成功");
        }


        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zls"></param>
        /// <param name="zxzt"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetAccountingExecutionQuery(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj, string zls
            , int? zxzt, string from, string cxzt, string jzjhmxId
            , string sfzt)
        {
            var orgId = this.OrganizeId;
            if (string.IsNullOrEmpty(orgId))
            {
                throw new FailedCodeException("SYS_GET_ORGANIZATIONAL_FAILURE");
            }
            var list = new
            {
                rows = from == "mz"
                ? _BookkeepInHosDmnService.SelectOutAccountingExecuteList(pagination, keyword, kssj, jssj, zxzt, orgId, zls, cxzt, jzjhmxId, sfzt)
                : _BookkeepInHosDmnService.SelectAccountingExecuteList(pagination, keyword, kssj, jssj, zxzt, orgId, zls, cxzt, jzjhmxId, sfzt),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(list.ToJson());
        }

        /// <summary>
        /// 记账计划执行 导出Excel
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zls"></param>
        /// <param name="zxzt"></param>
        /// <param name="from"></param>
        /// <param name="cxzt"></param>
        /// <param name="jzjhmxId"></param>
        /// <param name="cols"></param>
        /// <param name="colStanWidth"></param>
        /// <returns></returns>
        public ActionResult AccountingExecutionExportExcel(string keyword, DateTime? kssj, DateTime? jssj, string zls
            , int? zxzt, string from, string cxzt, string jzjhmxId
            , string cols, int colStanWidth
            , bool? isContainFilter
            , string sfzt)
        {
            var orgId = this.OrganizeId;
            if (string.IsNullOrEmpty(orgId))
            {
                throw new FailedCodeException("SYS_GET_ORGANIZATIONAL_FAILURE");
            }
            if (string.IsNullOrWhiteSpace(cols))
            {
                cols = WebHelper.GetCookie("ExportExcelCols");
                if (!string.IsNullOrWhiteSpace(cols))
                {
                    cols = System.Web.HttpUtility.UrlDecode(cols);
                    WebHelper.RemoveCookie("ExportExcelCols");
                }
            }
            if (string.IsNullOrWhiteSpace(cols))
            {
                throw new FailedException("未指定导出列");
            }
            var pagination = new Pagination();
            pagination.sidx = "CreateTime"; //时间升序
            pagination.rows = 65536 - 1;    //Excel最大行数
            pagination.page = 1;    //第一页把所有都查出来
            var list =
                   from == "mz"
                ? _BookkeepInHosDmnService.SelectOutAccountingExecuteList(pagination, keyword, kssj, jssj, zxzt, orgId, zls, cxzt, jzjhmxId, sfzt)
                : _BookkeepInHosDmnService.SelectAccountingExecuteList(pagination, keyword, kssj, jssj, zxzt, orgId, zls, cxzt, jzjhmxId, sfzt)
                   ;
            var colList = cols.ToObject<IList<ExcelColumn>>();
            var kflbItemDetails = _itemDmnService.GetItemsDetailListByOrgIdAndItemCode(orgId, "RehabTreatmentMethod");
            var sheet = new ExcelSheet()
            {
                Title = (from == "mz" ? "门诊" : "住院") + "记账执行一览表",
                columns = colList,
            };
            sheet.columns.Where(p => p.Name == "zlrq").ToList().ForEach(p =>
            {
                p.DateTimeFormat = "yyyy-MM-dd";
            });
            sheet.columns.Where(p => p.Name == "kflb").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                return kflbItemDetails.Where(p => p.Code == obj.ToString()).Select(p => p.Name).FirstOrDefault();
            });
            sheet.columns.Where(p => p.Name == "zt").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                return obj.ToString() == "1" ? "正常" : "已撤销";
            });
            sheet.columns.Where(p => p.Name == "sfzt").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                return obj.ToString() == "0" ? "未收费" : (obj.ToString() == "1" ? "已撤销" : "");
            });
            sheet.columns.Where(p => p.Name == "dj" || p.Name == "je").ToList().ForEach(p =>
            {
                p.NumberDigits = 2;
            });
            sheet.columns.Where(p => p.Name == "zfxz").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                return ((EnumZiFuXingZhi)(obj.ToInt())).GetDescription();
            });
            sheet.columns.ToList().ForEach(p =>
            {
                p.WidthTimes = (double)p.Width / colStanWidth;
                p.Width = 0;    //Width都置为0
            });

            var path = DateTime.Now.ToString("\\\\yyyyMMdd\\\\HHmmssfff") + ".xls";

            var filePath = CommmHelper.GetLocalFilePath("\\Excel导出\\记账执行" + path);

            if (isContainFilter == true)
            {
                //筛选条件
                var filterDict = new Dictionary<string, string>();
                if (kssj.HasValue)
                {
                    filterDict.Add("开始日期", kssj.Value.ToString("yyyy-MM-dd"));
                }
                if (jssj.HasValue)
                {
                    filterDict.Add("结束日期", jssj.Value.ToString("yyyy-MM-dd"));
                }
                if (!string.IsNullOrWhiteSpace(zls))
                {
                    filterDict.Add("治疗师", _sysStaffRepo.GetNameByGh(zls, orgId));
                }
                if (!string.IsNullOrWhiteSpace(cxzt))
                {
                    filterDict.Add("撤消状态", cxzt == "0" ? "已撤销" : "正常");
                }
                if (!string.IsNullOrWhiteSpace(sfzt))
                {
                    filterDict.Add("收费状态", sfzt == "0" ? "未收费" : (sfzt == "1" ? "已收费" : ""));
                }
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    filterDict.Add("关键字", keyword);
                }
                if (filterDict.Count > 0)
                {
                    sheet.filters = filterDict;
                }
            }

            var rest = list.ToExcel(filePath, sheet);

            if (rest)
            {
                return File(filePath, "application/x-xls", path.Replace("\\", ""));
            }
            else
            {
                return Content("文件导出失败，请返回列表页重试");
            }
        }

        /// <summary>
        /// 记账计划查询
        /// </summary>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult AccountingPlanQuery(Pagination pagination, string keyword, DateTime? kssj, DateTime? jssj
            , int? zxzt, string from,int? zsftx,int? sycstx
            , string sfzt
            , int? yzxz)
        {
            var orgId = this.OrganizeId;
            if (string.IsNullOrEmpty(orgId))
            {
                throw new FailedCodeException("SYS_GET_ORGANIZATIONAL_FAILURE");
            }
            var list = new
            {
                rows =
                   from == "mz"
                   ? _outPatientDmnService.SelectAccountingPlanList(pagination, keyword, kssj, jssj, zxzt, orgId, zsftx, sycstx, sfzt)
                   : _BookkeepInHosDmnService.SelectAccountingPlanList(pagination, keyword, kssj, jssj, zxzt, orgId, yzxz),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }

        /// <summary>
        /// 导出Excel 选择列视图
        /// </summary>
        /// <returns></returns>
        public ActionResult ExcelExportChooseColumns()
        {
            return View();
        }

        /// <summary>
        /// 记账计划导出Excel
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="zxzt"></param>
        /// <param name="from"></param>
        /// <param name="cols"></param>
        /// <param name="colStanWidth"></param>
        /// <returns></returns>
        public ActionResult AccountingPlanExportExcel(string keyword, DateTime? kssj, DateTime? jssj
            , int? zxzt, string from
            , string cols, int colStanWidth
            , bool? isContainFilter,int? zsftx,int? sycstx
            , string sfzt
            , int? yzxz)
        {
            var orgId = this.OrganizeId;
            if (string.IsNullOrEmpty(orgId))
            {
                throw new FailedCodeException("SYS_GET_ORGANIZATIONAL_FAILURE");
            }
            if (string.IsNullOrWhiteSpace(cols))
            {
                cols = WebHelper.GetCookie("ExportExcelCols");
                if (!string.IsNullOrWhiteSpace(cols))
                {
                    cols = System.Web.HttpUtility.UrlDecode(cols);
                    WebHelper.RemoveCookie("ExportExcelCols");
                }
            }
            if (string.IsNullOrWhiteSpace(cols))
            {
                throw new FailedException("未指定导出列");
            }
            var pagination = new Pagination();
            pagination.sidx = "CreateTime"; //时间升序
            pagination.rows = 65536 - 1;    //Excel最大行数
            pagination.page = 1;    //第一页把所有都查出来
            var list =
                   from == "mz"
                   ? _outPatientDmnService.SelectAccountingPlanList(pagination, keyword, kssj, jssj, zxzt, orgId, zsftx, sycstx, sfzt)
                   : _BookkeepInHosDmnService.SelectAccountingPlanList(pagination, keyword, kssj, jssj, zxzt, orgId, yzxz)
                   ;
            var colList = cols.ToObject<IList<ExcelColumn>>();
            var sheet = new ExcelSheet()
            {
                Title = (from == "mz" ? "门诊" : "住院") + "记账计划一览表",
                columns = colList,
            };
            sheet.columns.Where(p => p.Name == "LastEexcutionTime" || p.Name == "CreateTime" || p.Name == "sfrq").ToList().ForEach(p =>
            {
                p.DateTimeFormat = "yyyy-MM-dd";
            });
            sheet.columns.Where(p => p.Name == "startDate").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                if (obj != null)
                {
                    DateTime thisDate;
                    if (DateTime.TryParse(obj.ToString(), out thisDate))
                    {
                        if (thisDate >= new DateTime(2000, 01, 01))
                        {
                            return thisDate.ToString("yyyy-MM-dd");
                        }
                    }
                }
                return "";
            });
            sheet.columns.Where(p => p.Name == "zxzt").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                return ((EnumJzjhZXZT)(obj.ToInt())).GetDescription();
            });
            sheet.columns.Where(p => p.Name == "yzxz").ToList().ForEach(t => t.FuncGetCellValue = (obj) =>
            {
                return obj.ToString() == "2" ? "长" : (obj.ToString() == "1" ? "临" : "");
            });
            sheet.columns.ToList().ForEach(p =>
            {
                p.WidthTimes = (double)p.Width / colStanWidth;
                p.Width = 0;    //Width都置为0
            });

            var path = DateTime.Now.ToString("\\\\yyyyMMdd\\\\HHmmssfff") + ".xls";

            var filePath = CommmHelper.GetLocalFilePath("\\Excel导出\\记账计划" + path);

            if (isContainFilter == true)
            {
                //筛选条件
                var filterDict = new Dictionary<string, string>();
                if (kssj.HasValue)
                {
                    filterDict.Add("开始日期", kssj.Value.ToString("yyyy-MM-dd"));
                }
                if (jssj.HasValue)
                {
                    filterDict.Add("结束日期", jssj.Value.ToString("yyyy-MM-dd"));
                }
                if (zxzt.HasValue)
                {
                    filterDict.Add("执行状态", ((EnumJzjhZXZT)zxzt.Value).GetDescription());
                }
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    filterDict.Add("关键字", keyword);
                }
                if (filterDict.Count > 0)
                {
                    sheet.filters = filterDict;
                }
            }

            var rest = list.ToExcel(filePath, sheet);

            if (rest)
            {
                return File(filePath, "application/x-xls", path.Replace("\\", ""));
            }
            else
            {
                return Content("文件导出失败，请返回列表页重试");
            }
        }

        /// <summary>
        /// 查询 治疗师 本月 已 记账金额（jfb、mzxm、mzcfmx）、记账上限金额
        /// </summary>
        /// <param name="gh"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetStaffjfjeInfo(string gh, string ks)
        {
            decimal jfjesx;
            decimal yjfzje;
            _moneyUpperLimitReminderDmnService.GetStaffjfjeInfo(this.OrganizeId, gh, ks, out jfjesx, out yjfzje);
            return Success(null, new { jfjesx = jfjesx, yjfzje = yjfzje });
        }

        #endregion

        #region 门诊记账第四版，单次治疗量
        /// <summary>
        /// 保存门诊记账内容
        /// </summary>
        /// <param name="bacDto"></param>
        /// <param name="xmList"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SavepatientAcountInfo(OutpatAccBasicInfoDto bacDto, List<OutpatAccGridInfoDto> xmList)
        {
            _outpatientApp.SaveoutpatientAccountInfo(bacDto, xmList, OrganizeId);
            //_outPatientDmnService.SaveoutpatientAccountInfo(bacDto, xmList, OrganizeId, UserIdentity.UserCode);
            return Success("提交成功");
        }


        /// <summary>
        /// 搜索病人基本信息
        /// </summary>
        /// <returns></returns>
        public ActionResult PatSearchInfo(string keyword)
        {
            var OrganizeId = this.OrganizeId;
            var list = _refundDmnService.GetBasicInfoSearchListInRegister(keyword, OrganizeId);
            //return Json(list, JsonRequestBehavior.AllowGet);
            return Content(list.ToJson());
        }
        #endregion

        #region 记账计划查询
        public ActionResult ViewPlanDetail()
        {
            return View();
        }

        public ActionResult UpdateExecForm()
        {
            return View();
        }

        public ActionResult GetFormJson(string keyValue, string from)
        {
            var data = _BookkeepInHosDmnService.GetFormPlan(keyValue, OrganizeId, from);
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取记账计划详情
        /// </summary>
        /// <param name="jzjhmxId"></param>
        /// <returns></returns>
        public ActionResult AccountingPlanDetailQuery(Pagination pagination, string jzjhmxId, string from)
        {

            var data = _BookkeepInHosDmnService.GetFormPlan(pagination, jzjhmxId, this.OrganizeId, from);
            var list = new
            {
                rows = data,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(list.ToJson());
        }

        /// <summary>
        ///  撤销执行
        /// </summary>
        /// <param name="zxbh"></param>
        /// <param name="jzjhmxId"></param>
        /// <returns></returns>
        public ActionResult cancelExce(string zxbh, string jzjhmxId, string from)
        {
            _BookkeepInHosDmnService.RevokeExec(jzjhmxId, zxbh, OrganizeId, from);
            return Success("取消成功");
        }

        public ActionResult updateExc(cxzxGirdDto zxItem, string from)
        {
            if (zxItem != null)
            {
                _BookkeepInHosDmnService.UpdateExec(zxItem, from, OrganizeId);
            }
            return Success("更改成功");
        }
        #endregion

        #endregion 住院记账 记账计划

        #region 住院记账 与Optima同步数据，Optima给我们推送治疗记录，确认之，往zy_xmjfb zy_ypjfb中写计费数据

        /// <summary>
        /// 住院记账 Index
        /// </summary>
        /// <returns></returns>
        public ActionResult InpatientAccountingIndex()
        {
            var isRehabDoctor = _sysUserDmnService.CheckStaffIsBelongDuty(this.UserIdentity.StaffId, "RehabDoctor");
            if (isRehabDoctor)
            {
                ViewBag.CurYsStaffId = this.UserIdentity.StaffId;
                ViewBag.CurYs = this.UserIdentity.rygh;
                ViewBag.CurYsmc = this.UserIdentity.UserName;
                ViewBag.CurKs = this.UserIdentity.DepartmentCode;
                ViewBag.CurKsmc = this.UserIdentity.DepartmentName;
            }
            //住院记账 单价 可编辑 开关配置
            ViewBag.zyjz_dj_editable = _sysConfigRepo.GetBoolValueByCode("zyjz.dj.editable", OrganizeId);

            return View();
        }

        /// <summary>
        /// 根据住院号 获取 病人 记账 信息
        /// </summary>
        /// <param name="zyh">住院号</param>
        /// <returns></returns>
        public ActionResult GetInpatientAccountingStatusDetail(string zyh)
        {
            var data = _bookkeepInpatientApp.GetInpatientAccountingStatusDetail(zyh);
            return Success(null, data);
        }

        /// <summary>
        /// 住院记账 提交计费
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="xmList"></param>
        /// <returns></returns>
        public ActionResult InpatientAccountingSubmitAccounting(string zyh
            , IList<InpatientAccountingItemDto> xmList)
        {
            _BookkeepInHosDmnService.SaveInpatientAccounting(zyh, xmList, this.OrganizeId, this.UserIdentity.UserCode);
            return Success("提交成功");
        }

        /// <summary>
        /// 查询未确认的同步治疗记录
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUnConfirmedSyncedTreatmentRecord(string zyh, string mzh)
        {
            var data = _syncTreatmentServiceRecordDmnService.GetList(OrganizeId, clzt: 1, mzh: mzh, zyh: zyh, zlsgh: this.UserIdentity.rygh);

            return Success(null, data);
        }

        #endregion 住院记账 与Optima同步数据

        #region 费用结算 出院结算（其实是配合住院计划记账加上的，但感觉比较通用）

        /// <summary>
        /// 未结费用结算Index
        /// </summary>
        /// <returns></returns>
        public ActionResult UnSettedFeeSettIndex()
        {
            return View();
        }

        /// <summary>
        /// 获取最后一次中途结算 结束结算日期
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetLastValidMidwaySettTimeJson(string zyh)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                return Error("zyh is required");
            }
            var dt = _BookkeepInHosDmnService.GetLastValidMidwaySettTime(zyh, this.OrganizeId);
            return Success(null, dt);
        }

        /// <summary>
        /// 未结费用结算 Items Query
        /// </summary>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetUnSettedFeeItemListJson(string zyh, DateTime? endTime,string ver)
        {
            if (!endTime.HasValue || string.IsNullOrWhiteSpace(zyh))
            {
                return null;
            }
            //开始日期
            var startTime = _BookkeepInHosDmnService.GetLastValidMidwaySettTime(zyh, this.OrganizeId);
            if (!startTime.HasValue)
            {
                //取入院日期
                startTime = new DateTime(1970, 01, 01);
            }
            //让其查不到
            endTime = endTime ?? new DateTime(1970, 01, 01);

            var list = _BookkeepInHosDmnService.GetChargeItemPaginationList(new Pagination()
            {
                page = 1,
                rows = 100000,
                sidx = "CreateTime"
            }, zyh, this.OrganizeId, startTime.Value, endTime.Value);
            if (ver=="2")
            {
                //结果
                var dataV2 = list.GroupBy(p => p.sfxmmc).Select(p => new
                {
                    sfxmmc = p.First().sfxmmc,
                    sl = p.Sum(i => i.sl),
                    zje = p.Sum(i => Math.Round(i.je, 2, MidpointRounding.AwayFromZero))
                });
                return Content(dataV2.ToJson());
            }
            //结果
            var data = list.GroupBy(p => p.sfxmmc).Select(p => new
            {
                sfxmmc = p.First().sfxmmc,
                sl = p.Sum(i => i.sl),
                zje = p.Sum(i =>Math.Round(i.dj * i.sl,2,MidpointRounding.AwayFromZero))
            });

            return Content(data.ToJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="lastValidMidwaySettTime"></param>
        /// <param name="endTime"></param>
        /// <param name="expectedCount"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult UnSettedItemFeeConfirm(string zyh, DateTime startTime, DateTime endTime, int? expectedCount)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                return Error("zyh is required");
            }
            var dbDt = _BookkeepInHosDmnService.GetLastValidMidwaySettTime(zyh, this.OrganizeId);
            if (dbDt.HasValue && dbDt.Value != startTime)
            {
                return Error("过程中结算状态发生变更，请重试!");
            }

            _BookkeepInHosDmnService.MidwaySettlement(zyh, this.OrganizeId, startTime, endTime, expectedCount);

            return Success("操作成功");
        }

        /// <summary>
        /// 出院结算
        /// </summary>
        /// <returns></returns>
        public ActionResult DischargeSettlementIndex()
        {
            return View();
        }

        /// <summary>
        /// 获取已结 中途结算ListJson
        /// </summary>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetMidwaySettedListJson(string zyh)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                return null;
            }

            var list = _hospSettlementRepo.GetValidList(zyh, this.OrganizeId);

            return Content(list.ToJson());
        }

        /// <summary>
        /// 是否存在未结记录
        /// 通过中途结算的时间
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult CheckIsExistUnSetted(string zyh)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                return Error("zyh is required");
            }
            DateTime? startTime = null;
            startTime = _BookkeepInHosDmnService.GetLastValidMidwaySettTime(zyh, this.OrganizeId);
            if (!startTime.HasValue)
            {
                startTime = new DateTime(1970, 01, 01);  //未结算过的话 从入院开始算所有项目
            }
            var jsItemList = _hospItemBillingRepo.GetItemFeeEntityListByTime(zyh, this.OrganizeId, startTime.Value, DateTime.Now);

            return Success(null, jsItemList.Count > 0);
        }

        /// <summary>
        /// 出院结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="cyrq"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult DischargeSettlement(string zyh, DateTime cyrq)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                return Error("zyh is required");
            }

            if (_sysConfigRepo.GetBoolValueByCode("CYJS_Optima_ZT_Check", this.OrganizeId) == true)
            {
                checkOptimaZyhSettleable(zyh, cyrq);
            }

            DateTime? startTime = null;
            startTime = _BookkeepInHosDmnService.GetLastValidMidwaySettTime(zyh, this.OrganizeId);
            if (!startTime.HasValue)
            {
                return Error("未产生费用，不能出院");
            }
            if (cyrq.Date < startTime.Value.Date)
            {
                return Error("出院日期错误，不能在费用结算日期之前");
            }
            var jsItemList = _hospItemBillingRepo.GetItemFeeEntityListByTime(zyh, this.OrganizeId, startTime.Value, DateTime.Now);
            if (jsItemList.Count > 0)
            {
                return Error("存在未结费用，不能出院");
            }
            if (_hospAccountingPlanDetailRepo.IQueryable(p => p.zyh == zyh && p.OrganizeId == this.OrganizeId && p.zt == "1").ToList().Any(p => p.zxzt == (int)EnumJzjhZXZT.None
             || p.zxzt == (int)EnumJzjhZXZT.Part))
            {
                return Error("存在未执行完记账，不能出院");
            }

            _BookkeepInHosDmnService.DischargeSettlement(zyh, this.OrganizeId, cyrq);

            return Success("成功出院");
        }

        /// <summary>
        /// 撤销最后一次中途结算
        /// </summary>
        /// <param name="zyh"></param>
        public ActionResult CancelTheLastMidwaySett(string zyh,string zffs1,string zffsmc1)
        {
            string szId = "";
            decimal refoundmoney;
            _BookkeepInHosDmnService.CancelTheLastMidwaySett(zyh,zffs1, this.OrganizeId,out refoundmoney);
            if (zffs1=="3"&& refoundmoney>0)
            {
                var zhxx = _accountmanageApp.GetHosPatInfo(zyh);//账户信息
                string sjpzh = _accountmanageApp.GetFinRepSJPZH();
                //充值
                if (!_accountmanageApp.PayDepositPost(new DeposDto
                {
                    patid = zhxx.patid,
                    pzh = sjpzh,
                    szxz = (int)EnumSZXZ.zyjsth,
                    zffsbh = zffs1.ToInt(),
                    zffsmc = zffsmc1,
                    zfje = refoundmoney,
                    zh = zhxx.zh,
                    zhxz = zhxx.zhxz.ToInt()

                }, out szId))
                {
                    throw new FailedException("充值失败");
                }
            }

            return Success("操作成功");
        }

        #endregion

        #region private methods

        /// <summary>
        /// 验证是否可以出院结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        private bool checkOptimaZyhSettleable(string zyh, DateTime cyrq)
        {
            try
            {
                AppLogger.Info(string.Format("出院结算告知Optima，zyh：{0},{1}", zyh, this.OrganizeId));

                RijndaelEnhanced rigndae = null;
                var isEnhanced = ConfigurationHelper.GetAppConfigBoolValue("Optima_API_Enhanced") ?? false;
                if (isEnhanced)
                {
                    string passPhrase = ConfigurationHelper.GetAppConfigValue("Optima_API_passPhrase");
                    string initVector = ConfigurationHelper.GetAppConfigValue("Optima_API_initVector");
                    rigndae = new RijndaelEnhanced(passPhrase, initVector);

                    AppLogger.Info("启用了加密");
                }

                var url = ConfigurationHelper.GetAppConfigValue("Optima_API_PatientInfo_GetPatientDischargeInfo_Url");

                AppLogger.Info(string.Format("接口地址：{0}", url));

                var orgId = this.OrganizeId;

                var reqParams = string.Format(@"<DischargePatient>
  <root>
    <body>
      <authHeader>
        <apiKey>{0}</apiKey>
      </authHeader>
      <params>
        <admsNum>{1}</admsNum>
        <hospitalDischargeDate>{2}</hospitalDischargeDate>
      </params>
    </body>
  </root>
</DischargePatient>", orgId, rigndae != null ? rigndae.Encrypt(zyh) : zyh, cyrq.ToString("yyyy-MM-dd HH:mm:ss"));

                AppLogger.Info(string.Format("请求参数：{0}", reqParams));

                //发请求 获取返回内容
                var respStr = HttpClientHelper.HttpPostString(url, reqParams
                    , contentType: HttpClientHelper.EnumContentType.xml);

                AppLogger.Info(string.Format("响应：{0}", respStr));

                //提取
                var flag = resp_GetResultAttr(respStr, "flag");

                if (string.IsNullOrWhiteSpace(flag))
                {
                    throw new FailedException("出院结算告知Optima接口调用失败");
                }
                else if (flag == "0")
                {
                    throw new FailedException("出院结算告知Optima接口调用失败.");
                }
                else if (flag == "1")
                {
                    //提起具体内容
                    var list = XmlToDtUntility.XmlToIList<InpatientDischargeStatus>(respStr, "row");
                    if (list == null || list.Count != 1)
                    {
                        throw new FailedException("出院结算告知Optima接口调用失败..");
                    }
                    if (!(!string.IsNullOrWhiteSpace(list[0].dischargeStatus) && list[0].dischargeStatus.ToLower() == "0"))
                    {
                        throw new FailedException("该病人在Optima系统中状态异常（非可出院）...");
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error(ex.Message, ex);
            }
            return true;
        }

        /// <summary>
        /// 获取返回内容中的flag值
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private string resp_GetResultAttr(string xml, string attrName)
        {
            var dt = getDataTableFromXml(xml, "result");
            if (dt != null && dt.Rows.Count == 1)
            {
                var flagRow = dt.Rows[0][attrName];
                if (flagRow != null)
                {
                    return flagRow.ToString().Trim();
                }
            }
            return null;
        }

        /// <summary>
        /// 从xml中提取tableName的DataTable
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private DataTable getDataTableFromXml(string xml, string tableName)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return null;
            }
            var ds = XmlToDtUntility.XmlToDataSet(xml);
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (ds.Tables[i].TableName.Equals(tableName))
                {
                    return ds.Tables[i];
                }
            }
            return null;
        }

        #endregion private methods


        /// <summary>
        /// 门诊/住院计划录入 检索患者浮层
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult PatAccountingSearchInfo(string keyword,string from, string brzybzType = null, bool curUserCreate = false)
         {
            var data=  _refundDmnService.GetBasicInfoSearchList(keyword, this.OrganizeId, curUserCreate ? UserIdentity.UserCode : "", "", from, brzybzType);
            return Content(data.ToJson());
        }
    }
}