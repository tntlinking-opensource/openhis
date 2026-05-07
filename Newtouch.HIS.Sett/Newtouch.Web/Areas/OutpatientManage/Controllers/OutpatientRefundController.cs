using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Common;
using Newtouch.Common.Web;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.Core.NLogger;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Application.Interface.BusinessManage;
using Newtouch.HIS.Domain.DTO;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;
using Newtouch.HIS.Domain.Entity.OutpatientManage;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Proxy.guian.DTO.S21;
using Newtouch.HIS.Proxy.guian.DTO.S25;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using Newtouch.HIS.Domain.IRepository.OutpatientManage;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.Infrastructure.Model;
using System.Configuration;

namespace Newtouch.HIS.Web.Areas.OutpatientManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class OutpatientRefundController : ControllerBase
    {
        private readonly IRefundDmnService _refundService;
        private readonly IOutPatientDmnService _outPatientDmnService;
        private readonly IRefundApp _IRefundApp;
        private readonly IOutPatientUniversalDmnService _outPatientUniversalDmnService;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IPayApp _payApp;
        private readonly IOutpatientSettlementGAYBFeeRepo _outpatientSettlementGAYBFeeRepo;
        private readonly IGuiAnOutpatientXnhApp _guiAnOutpatientXnhApp;
        private readonly IOutpatientRegistRepo _outpatientRegistRepo;
        private readonly ICqybSett05Repo _cqybSett05Repo;
        private readonly IOutpatientItemRepo _outpatientItemRepo;
        #region 页面初始化

        public ActionResult QueJsList()
        {
            return View();
        }

        /// <summary>
        /// 退费查询时预览病人信息
        /// </summary>
        /// <returns></returns>
        public ActionResult SysPatEntitiesReView(string from = "")
        {
            ViewBag.from = from;
            return View();
        }
        #endregion

        /// <summary>
        /// 分页数据加载
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="blh"></param>
        /// <param name="xm"></param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult PatSearchInfo(Pagination pagination, string blh, string mzh, string xm, string kssj, string jssj
            , bool curUserCreate = false, string jiuzhenbiaozhi = null)
        {
            var data = new
            {
                rows = _refundService.GetBasicInfoSearchList(pagination, blh, mzh, xm, kssj, jssj, OrganizeId
                , curUserCreate ? UserIdentity.UserCode : "", jiuzhenbiaozhi),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 查询结算信息
        /// </summary>
        /// <param name="blh"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public ActionResult btnQuery(string blh, string startTime, string endTime)
        {
            var data = _refundService.GetjsInfoByblh(blh, startTime, endTime, OrganizeId);
            return Success("", data);
        }

        /// <summary>
        /// 按结算内码查门诊项目
        /// </summary>
        /// <param name="jsnm">结算内码</param>
        /// <returns></returns>
        public ActionResult GetGridViewMxByJsnm(int jsnm)
        {
            var data = _refundService.GetGridViewMx(jsnm, OrganizeId);
            return Success("", data);
        }

        /// <summary>
        /// 退费
        /// </summary>
        public ActionResult btnReturn(string blh, int jsnm, string refundData)
        {
            bool isReturnAll;
            var GridViewMx = refundData.ToList<GridViewMx>();
            var result = _IRefundApp.btnReturnInAcc(blh, GridViewMx, jsnm, out isReturnAll);
            if (result)
            {
                return Success("", isReturnAll);
            }
            throw new FailedCodeException("HOSP_REFUNG_FAIL");
        }

        #region 退费2018版

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index2018()
        {
            return View();
        }

        /// <summary>
        /// 门诊退费 主记录 Query
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult RefundableJsQuery(DateTime? kssj, DateTime? jssj, string mzh)
        {
            if (string.IsNullOrEmpty(mzh) || !kssj.HasValue || !jssj.HasValue)
            {
                return null;
            }
            var list = _refundService.RefundableJsQuery(this.OrganizeId, kssj, jssj, mzh);
            return Content(list.ToJson());
        }

        public ActionResult RefundableGuiAnJsQuery(DateTime? kssj, DateTime? jssj, string mzh)
        {
            if (string.IsNullOrEmpty(mzh) || !kssj.HasValue || !jssj.HasValue)
            {
                return null;
            }
            var list = _refundService.RefundableGuiAnJsQuery(this.OrganizeId, kssj, jssj, mzh);
            return Content(list.ToJson());
        }

        /// <summary>
        /// 门诊退费 Query
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult RefundableDetailQuery(int jsnm = 0)
        {
            if (jsnm <= 0)
            {
                return null;
            }
            var list = _refundService.RefundableQuery(this.OrganizeId, jsnm);
            var ztlist = list.Where(p => p.ztId != null && p.ztId != "").ToList();
            var flag = true;
            string flagstr = _sysConfigRepo.GetValueByCode("sfxmztbs", this.OrganizeId);
            if (flagstr == "false")
            {
                flag = false;
            }
            if (flag)
            {
                //收费项目组套合并
                if (ztlist.Count() > 0)
                {
                    ztlist = list.Where(p => p.ztId != null && p.ztId != "").ToList()
                    .GroupBy(m => new { m.ghnm, m.jslx, m.feeType, m.cfh, m.ztmc, m.ztId, m.cfnm, m.ks, m.ys, m.ysmc, m.sfmb, m.sqdzt, m.cflxmc }).Select(p =>
                             new OutPatientRefundableVO
                             {
                                 ghnm = p.Key.ghnm,
                                 jsmxnm = p.Max(m => m.jsmxnm),
                                 jslx = p.Key.jslx,
                                 sl = 1,
                                 ktsl = 1,
                                 tsl = 1,
                                 jsmxje = p.Sum(m => m.jsmxje),
                                 dj =Math.Round(Convert.ToDecimal(p.Sum(m => m.jsmxje)), 2, MidpointRounding.AwayFromZero),
                                 feeType = p.Key.feeType,
                                 dw = "套",
                                 cfh = p.Key.cfh,
                                 mc = p.Key.ztmc,
                                 ztmc = p.Key.ztmc,
                                 ztId = p.Key.ztId,
                                 //ztsl = p.Key.ztsl,
                                 sfmb = p.Key.sfmb,
                                 dlmc = p.Key.ztmc,
                                 czh = "",
                                 cfnm = p.Key.cfnm,
                                 sfxmCode = p.Key.sfmb,
                                 sfdlCode = "",
                                 zfbl = null,
                                 zfxz = null,
                                 xmnm = null,
                                 cfmxId = null,
                                 ks = p.Key.ks,
                                 ys = p.Key.ys,
                                 ysmc = p.Key.ysmc,
                                 klsj = p.Max(m => m.klsj),
                                 zljhwzx = null,
                                 cflxmc = p.Key.cflxmc,
                                 sqdzt = p.Key.sqdzt
                             }
                    ).ToList();
                }
            }


            list = list.Where(p => p.ztId == null || p.ztId == "").ToList().Concat(ztlist).ToList();

            return Content(list.ToJson());
        }

        /// <summary>
        /// 门诊退费确认页
        /// </summary>
        /// <returns></returns>
        public ActionResult DetailConfirmForm()
        {
            ViewBag.ReportServerHOST = ConfigurationHelper.GetAppConfigValue("ReportServer.HOST");
            ViewBag.OrgId = this.OrganizeId;

            return View();
        }

        /// <summary>
        /// 退费提交 持久化操作
        /// </summary>
        /// <param name="jsnm"></param>
        /// <param name="tjsxmDict"></param>
        /// <param name="expectedTmxZje"></param>
        /// <param name="newfph">新结算发票号</param>
        /// <returns></returns>
        public ActionResult RefundSettlement(string mzh, int jsnm
            , Dictionary<string, decimal> tjsxmDict, decimal expectedTmxZje
            , string newfph, Dictionary<string, decimal> tjsxmDictZt, decimal expectedTmxZjeZt, string cfnmArray)
        {
            return handleRefund(mzh, jsnm
            , tjsxmDict, expectedTmxZje, null, null
            , null
            , null
            , newfph, tjsxmDictZt, expectedTmxZjeZt, cfnmArray);
        }

        /// <summary>
        /// 20181120 计划退费全停
        /// </summary>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public ActionResult AccountPlanRefundAll(int jsnm)
        {
            _outPatientUniversalDmnService.AccountPlanRefundAll(this.OrganizeId, jsnm);
            return Success();
        }

        /// <summary>
        /// 视图-选择结算
        /// </summary>
        /// <returns></returns>
        public ActionResult QueJsList2018()
        {
            return View();
        }

        #endregion

        #region

        /// <summary>
        /// 退费流程.新的未结明细上传
        /// </summary>
        /// <param name="oldjsnm"></param>
        /// <param name="tjsxmDict"></param>
        /// <param name="newwjybjsh">新的未结医保结算号</param>
        /// <returns></returns>
        public ActionResult DetailsUploadYb(int oldjsnm
            , Dictionary<string, decimal> tjsxmDict, string newwjybjsh)
        {
            var newDict = new Dictionary<int, decimal>();
            foreach (var item in tjsxmDict)
            {
                newDict.Add(Convert.ToInt32(item.Key), item.Value);
            }
            decimal tmxzje;
            decimal yxzje;
            _IRefundApp.DetailsUploadYb(this.OrganizeId, oldjsnm, newDict, newwjybjsh, out tmxzje, out yxzje);

            return Success(null, new { tmxzje = tmxzje, yxzje = yxzje });
        }

        /// <summary>
        /// 医保 退费提交 持久化操作
        /// </summary>
        /// <param name="jsnm"></param>
        /// <param name="tjsxmDict"></param>
        /// <param name="expectedTmxZje"></param>
        /// <param name="hcybjsh"></param>
        /// <param name="newwjybjsh"></param>
        /// <param name="hcybfeeRelated">红冲结算 的 结算医保金额相关</param>
        /// <param name="newybfeeRelated">新的结算（非全退） 的 结算医保金额相关</param>
        /// <returns></returns>
        public ActionResult YbRefundSettlement(string mzh, int jsnm
            , Dictionary<string, decimal> tjsxmDict, decimal expectedTmxZje, string hcybjsh, string newwjybjsh
            , CQMzjs05Dto hcybfeeRelated
            , OutpatientSettYbFeeRelatedDTO newybfeeRelated)
        {
            string newfph = "";
            decimal ztje = Convert.ToDecimal(0.00);
            return handleRefund(mzh, jsnm
            , tjsxmDict, expectedTmxZje, hcybjsh, newwjybjsh
            , hcybfeeRelated
            , newybfeeRelated
            , newfph, null, ztje, null);
        }

        #endregion

        #region 新农合退费

        /// <summary>
        /// 新农合退费
        /// </summary>
        /// <param name="jsnm"></param>
        /// <param name="tjsxmDict">退结算明细</param>
        /// <param name="mzh"></param>
        /// <param name="outpId">门诊补偿序号</param>
        /// <returns></returns>
        public ActionResult XnhTuiFei(string jsnm, Dictionary<string, decimal> tjsxmDict, string mzh, string outpId)
        {
            TFS21RequestDTO tfs21Request;
            var tuifeiResult = _guiAnOutpatientXnhApp.XnhTuiFeiProcess(jsnm, tjsxmDict, mzh, outpId, OrganizeId, UserIdentity.UserCode, out tfs21Request);
            return string.IsNullOrWhiteSpace(tuifeiResult) ? Success(tuifeiResult, tfs21Request) : Error(tuifeiResult);
        }

        #endregion

        #region private methods

        /// <summary>
        /// （已收费未发药）已收费未发药状态 处方变更告知事件
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="refundDict">jsmx tsl</param>
        private void refundFeeNotify(string mzh, Dictionary<int, decimal> refundDict)
        {
            try
            {
                var ypCfhList = _outPatientDmnService.GetRelatedYpDetailListByMzh(mzh, this.OrganizeId, refundDict);
                if (ypCfhList.Count <= 0) return;
                var orgId = this.OrganizeId;
                var operatorCode = this.UserIdentity.UserCode;
                //获取token
                var token = SitePDSAPIHelper.GetToken();
                var obj = ypCfhList.Select(p => new
                {
                    cfh = p.cfh,
                    ypdm = p.ypdm,
                    Yfbm = p.Yfbm,
                    sl = (int)refundDict.First(a => a.Key == p.jsmxnm).Value,
                    zhyz = 0,
                }).ToList();
                var thread = new Thread(new ThreadStart(delegate { ypCfRefundFeeNotifyThread(token, obj, orgId, operatorCode); }));
                thread.Start();
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// （已收费未发药）药品处方变更通知PDS
        /// </summary>
        /// <param name="tmxList"></param>
        /// <param name="orgId"></param>
        private void ypCfRefundFeeNotifyThread(string token, object tmxList, string orgId, string operatorCode)
        {
            var reqObj = new
            {
                Items = tmxList,
                OrganizeId = orgId,
                CreatorCode = operatorCode,
                TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Token = token

            };
            var apiResp = SitePDSAPIHelper.Request<APIRequestHelper.DefaultResponse>(
                "api/ResourcesOperate/OutpatientPartReturnBeforeDispensingMedicine", reqObj, autoAppendToken: false);

            if (apiResp != null && apiResp.code != APIRequestHelper.ResponseResultCode.SUCCESS)
            {
                new NLogger().Info(string.Format("（已收费未发药）药品处方变更通知PDS，失败：{0}、{1}", reqObj.ToJson(), apiResp.ToJson()));
            }
        }

        /// <summary>
        /// HIS退费（医保、非医保公用）
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="jsnm"></param>
        /// <param name="tjsxmDict"></param>
        /// <param name="expectedTmxZje"></param>
        /// <param name="hcybjsh"></param>
        /// <param name="newwjybjsh"></param>
        /// <param name="hcybfeeRelated"></param>
        /// <param name="newybfeeRelated"></param>
        /// <param name="newfph"></param>
        /// <returns></returns>
        private ActionResult handleRefund(string mzh, int jsnm
            , Dictionary<string, decimal> tjsxmDict, decimal expectedTmxZje, string hcybjsh, string newwjybjsh
            , CQMzjs05Dto hcybfeeRelated
            , OutpatientSettYbFeeRelatedDTO newybfeeRelated
            , string newfph, Dictionary<string, decimal> tjsxmDictZt, decimal expectedTmxZjeZt, string cfnmArray)
        {
            var newDict = new Dictionary<int, decimal>();
            if (expectedTmxZje <= 0) tjsxmDict = null;
            if (expectedTmxZjeZt <= 0) tjsxmDictZt = null;
            if (tjsxmDict != null)
            {
                foreach (var item in tjsxmDict)
                {
                    newDict.Add(Convert.ToInt32(item.Key), item.Value);
                }
            }

            if (tjsxmDictZt != null)
            {
                var tmzxmlist = _outPatientUniversalDmnService.RefundZtDetail(cfnmArray, OrganizeId);
                foreach (var items in tjsxmDictZt)
                {
                    foreach (var m in tmzxmlist)
                    {
                        if (items.Key == m.ztId) //还需加cfnm 多个处方可能有相同组套
                        {
                            newDict.Add(m.jsmxnm, Math.Round(items.Value * m.sl, 2, MidpointRounding.AwayFromZero));
                        }
                    }
                }

            }
            expectedTmxZje += expectedTmxZjeZt;
            object newJszbInfo;
            string outTradeNo;
            decimal refundAmount;
            _outPatientUniversalDmnService.RefundSettlement(this.OrganizeId, jsnm, newDict, expectedTmxZje, newfph, out newJszbInfo, out outTradeNo, out refundAmount, hcybjsh, newwjybjsh, hcybfeeRelated, newybfeeRelated, null, null);
             
            if (!string.IsNullOrWhiteSpace(mzh))
            {
                refundFeeNotify(mzh, newDict);
            }

            bool? isTradeRefundError = null;
            if (!string.IsNullOrWhiteSpace(outTradeNo) && refundAmount > 0)   //需要原路退回
            {
                string errorMsg;
                var refundReuslt = _payApp.TradeRefund(outTradeNo, refundAmount, "门诊退费", "", out errorMsg);
                isTradeRefundError = refundReuslt == (int)EnumRefundStatus.Failed || refundReuslt == (int)EnumRefundStatus.UnKnown;    //失败 或 未知
            }

            var msg = "退费成功"; 
            if (!isTradeRefundError.HasValue) return Success(msg, newJszbInfo);
            msg = !isTradeRefundError.Value ? "退费成功，应退金额已原路退回" : "HIS退费成功，但应退金额退回失败，请人工核查";
            
              return Success(msg , newJszbInfo);
        }


        /// <summary>
        /// 更新CIS退标志和退还药房冻结库存
        /// </summary>
        /// <param name="tcfh"></param>
        public void UpdateChargeTbz(List<string> tcfh, List<string> typcfh)
        {
            if (typcfh!=null)
            {
                foreach (var cfh in typcfh)
                {
                    //接口内容
                    var cancalreqObj = new
                    {
                        cfh = cfh,
                        OrganizeId = OrganizeId,
                        CreatorCode = this.UserIdentity.UserCode,
                    };
                    var apiRespPush = SitePDSAPIHelper.Request<APIRequestHelper.DefaultResponse>(
                  "/api/ResourcesOperate/OutpatientCancelDjYpReturn", cancalreqObj);
                    if (apiRespPush.code != APIRequestHelper.ResponseResultCode.SUCCESS)
                    {
                        AppLogger.Info(string.Format("处方退费返还冻结库存更新API同步至PDS，处方号：{0}，结果：{1}、{2}", tcfh, apiRespPush.code, apiRespPush.sub_code));
                    }
                }
            }
            var reqObj = new
            {
                cfList = tcfh,
                tbz = true,
            };
            var apiResp = SiteCISAPIHelper.Request<APIRequestHelper.DefaultResponse>(
                           "api/Prescription/UpdateChargeTbz", reqObj);
            if (apiResp != null)
            {
                AppLogger.Info(string.Format("处方退标志状态更新API同步至CIS，处方号：{0}，结果：{1}、{2}", string.Join(",", tcfh), apiResp.code, apiResp.sub_code));
            }
            else
            {
                AppLogger.Info(string.Format("处方退标志更新API同步至CIS，处方号：{0}，结果：未获取到响应，同步失败", string.Join(",", tcfh)));
            }
        }

        public ActionResult MzhtResult(OutpatientSettlementGAYBFeeEntity dto)
        {
            dto.OrganizeId = this.OrganizeId;
            _outpatientSettlementGAYBFeeRepo.InsertEntity(dto);
            return Success("写入成功");
        }

        /// <summary>
        /// 退费（贵安）
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="jsnm"></param>
        /// <param name="tjsxmDict"></param>
        /// <param name="expectedTmxZje"></param>
        /// <param name="hcybjsh"></param>
        /// <param name="newwjybjsh"></param>
        /// <param name="hcybfeeRelated"></param>
        /// <param name="newybfeeRelated"></param>
        /// <param name="guianMztfProDto"></param>
        /// <param name="outpId"></param>
        /// <returns></returns>
        public ActionResult guianhandleRefund(string mzh, int jsnm
            , Dictionary<string, decimal> tjsxmDict, decimal expectedTmxZje, string hcybjsh, string newwjybjsh
            , CQMzjs05Dto hcybfeeRelated
            , CQMzjs05Dto newybfeeRelated
            , GuiAnMztfProDto guianMztfProDto
            , string outpId)
        {
            #region 新农合结算

            S25ResponseDTO xnhybfeeRelated = null;
            if (!string.IsNullOrWhiteSpace(outpId))
            {
                var xnhSettResult = _guiAnOutpatientXnhApp.Sett(mzh, outpId, OrganizeId, UserIdentity.UserCode, out xnhybfeeRelated);
                if (!string.IsNullOrWhiteSpace(xnhSettResult) && xnhSettResult.IndexOf("该结算已红冲", StringComparison.Ordinal) < 0)
                {
                    return Error(xnhSettResult);
                }
            }
            #endregion

            string newfph = "";
            var newDict = new Dictionary<int, decimal>();
            foreach (var item in tjsxmDict)
            {
                newDict.Add(Convert.ToInt32(item.Key), item.Value);
            }

            object newJszbInfo;
            string outTradeNo;
            decimal refundAmount;
            _outPatientUniversalDmnService.RefundSettlement(this.OrganizeId, jsnm, newDict, expectedTmxZje, newfph, out newJszbInfo, out outTradeNo, out refundAmount, hcybjsh, newwjybjsh, hcybfeeRelated, null, newybfeeRelated, guianMztfProDto, s25ResponseDto: xnhybfeeRelated, outpId: outpId);

            if (!string.IsNullOrWhiteSpace(mzh))
            {
                refundFeeNotify(mzh, newDict);
            }

            bool? isTradeRefundError = null;
            if (!string.IsNullOrWhiteSpace(outTradeNo) && refundAmount > 0)   //需要原路退回
            {
                string errorMsg;
                var refundReuslt = _payApp.TradeRefund(outTradeNo, refundAmount, "门诊退费", "", out errorMsg);
                isTradeRefundError = refundReuslt == (int)EnumRefundStatus.Failed || refundReuslt == (int)EnumRefundStatus.UnKnown;    //失败 或 未知
            }

            var msg = "退费成功";
            if (isTradeRefundError.HasValue)
            {
                msg = !isTradeRefundError.Value ? "退费成功" : "HIS退费成功，但应退金额退回失败，请人工核查";
            }

            _outpatientRegistRepo.RecordOutpId(mzh, "", UserIdentity.UserCode, OrganizeId);
            return Success(msg, newJszbInfo);
        }
        #endregion

        #region 重庆医保退费
        public ActionResult RefundableChongQingQuery(DateTime? kssj, DateTime? jssj, string mzh)
        {
            if (string.IsNullOrEmpty(mzh) || !kssj.HasValue || !jssj.HasValue)
            {
                return null;
            }
            var list = _refundService.RefundableChongQingQuery(this.OrganizeId, kssj, jssj, mzh);
            return Content(list.ToJson());
        }

        #endregion

        public ActionResult retApplicationform(string mzh, List<string> cfhList)
        {

            List<CheckApplicationfromDTO> datalist = new List<CheckApplicationfromDTO>();
            foreach (var item in cfhList)
            {
                if (item != "")
                {
                    var data = _outPatientUniversalDmnService.retApplicationform(mzh, item, this.OrganizeId, "N");
                    if (data != null)
                    {
                        datalist.Add(data);
                    }

                }
            }
            //var data = _outPatientUniversalDmnService.retApplicationform(mzh, cfnmList, this.OrganizeId, "N");
            var url = "http://192.168.3.17:8080/URISService/services/interface/requestorder";
            if (datalist == null)
            {
                return Success("失败");
            }
            foreach (var data in datalist)
            {
                string datajson = Tools.Json.ToJson(data);
                try
                {
                    System.Net.HttpWebRequest request = null;
                    System.Net.WebResponse response = null;

                    request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                    request.ProtocolVersion = System.Net.HttpVersion.Version10;
                    request.Method = "POST";

                    request.ContentType = "application/json";
                    request.CookieContainer = null;//获取验证码时候获取到的cookie会附加在这个容器里面
                    request.AllowAutoRedirect = true;
                    request.KeepAlive = true;//建立持久性连接
                    request.Host = "192.168.0.101";
                    request.UserAgent = "PostmanRuntime/7.29.2";
                    request.Accept = "*/*";
                    byte[] datas = System.Text.Encoding.UTF8.GetBytes(datajson);
                    using (System.IO.Stream stream = request.GetRequestStream())
                    {
                        stream.Write(datas, 0, datas.Length);
                    }

                    response = (System.Net.HttpWebResponse)request.GetResponse();
                    string outputText = string.Empty;
                    using (System.IO.Stream responseStm = response.GetResponseStream())
                    {
                        System.IO.StreamReader redStm = new System.IO.StreamReader(responseStm, System.Text.Encoding.UTF8);
                        outputText = redStm.ReadToEnd();
                    }
                    var apiresp = JavaScriptJsonSerializerHelper.Deserialize<RefJson>(outputText);
                    if (apiresp != null && apiresp.status != "Success")
                    {
                        AppLogger.Info(string.Format("Pacs检查申请单入参：{0}，出参结果：{1}", datajson, outputText));
                    }
                    else if (apiresp != null && apiresp.status == "Success")
                    {
                        //pacs申请单退成功后改变mz_cf表sqdzt为4 取消
                        _outPatientUniversalDmnService.pushApplicationformRef(data.Patient.CardID, this.OrganizeId, 4);
                    }


                }
                catch (Exception e)
                {

                    throw;
                }
            }

            return Success("");
        }
    }
}