using FrameworkBase.MultiOrg.Domain.DBContext.Infrastructure;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Application.Implementation;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.OutPatientPharmacy;
using Newtouch.HIS.Domain.VO;
using Newtouch.HIS.DomainServices;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.Enum;
using Newtouch.Infrastructure.Log;
using Newtouch.PDS.Requset;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Newtouch.Common.Web.APIRequestHelper;

namespace Newtouch.HIS.Web.Controllers
{
    /// <summary>
    /// 门诊药房Controller
    /// </summary>
    public class OutPatientPharmacyController : ControllerBase
    {
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IPyDmnService _pyDmnService;
        private readonly IfyDmnService _fyDmnService;
        private readonly IPharmacyDrugStorageDmnService _pharmacyDrugStorageDmnService;
        private readonly IMzCfRepo _mzCfRepo;
        private readonly IMzCfmxRepo _mzCfmxRepo;
        private readonly ItyDmnService _tyDmnService;
        private readonly IResourcesOperateApp _resourcesOperateApp;
        private readonly IOutPatientDispensingApp _outPatientDispensingApp;
        private readonly ISysYpksfypzEntityRepo _ISysYpksfypzEntityRepo;
        private readonly string _OrganizeId = OperatorProvider.GetCurrent().OrganizeId;//组织id

        #region 后台指向页面
        /// <summary>
        /// 门诊排药
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugArrangement()
        {
            return View();
        }

        /// <summary>
        /// 门诊发药查询
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugSearch()
        {
            return View();
        }
        public ActionResult MzPrescriptionQuery()
        {
            ViewBag.OrganizeId = _OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }
        #endregion

        #region 公共调用
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public ActionResult Loadconfig()
        {
            var config = new pyConfig();
            var pycxjgsj = _sysConfigRepo.GetValueByCode("pycxjgsj", OrganizeId);
            if (pycxjgsj == "" || string.IsNullOrWhiteSpace(pycxjgsj))
            {
                throw new FailedException("请先配置排药查询间隔时间");
            }

            var isAutoPy = _sysConfigRepo.GetValueByCode("IsAutoPy", OrganizeId);
            if (isAutoPy == "" || string.IsNullOrWhiteSpace(isAutoPy))
            {
                throw new FailedException("请先配置自动排药");
            }

            var isAutoFy = _sysConfigRepo.GetValueByCode("IsAutoFy", OrganizeId);
            if (isAutoFy == "" || string.IsNullOrWhiteSpace(isAutoFy))
            {
                throw new FailedException("请先配置自动发药");
            }

            config.pycxjgsj = int.Parse(pycxjgsj);
            config.IsAutoFy = isAutoFy;
            config.IsAutoPy = isAutoPy;
            return Content(config.ToJson());
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="configInfo"></param>
        /// <returns></returns>
        public ActionResult SubmitConfig(pyConfig configInfo)
        {
            var isAutoFy = configInfo.IsAutoFy == "true" ? "Y" : "N";
            var isAutoPy = configInfo.IsAutoPy == "true" ? "Y" : "N";
            //_sysConfigRepo.SetValueByCode("IsAutoFy", isAutoFy, OrganizeId);
            //_sysConfigRepo.SetValueByCode("IsAutoPy", isAutoPy, OrganizeId);
            //_sysConfigRepo.SetValueByCode("pycxjgsj", configInfo.pycxjgsj.ToString(), OrganizeId);
            return Success();
        }
        #endregion

        #region 补打发药信息


        /// <summary>
        /// 补打发药单
        /// </summary>
        /// <returns></returns>
        public ActionResult RePrintMedicineDoc()
        {
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            ViewBag.OrganizeId = OrganizeId;
            return View();
        }

        /// <summary>
        /// 补打发药信息 加载处方信息
        /// </summary>
        /// <param name="fph"></param>
        /// <param name="keyword"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public ActionResult fyInfoSearch(string fph, string keyword, DateTime kssj, DateTime jssj)
        {
            if (kssj == null || jssj == null)
            {
                throw new FailedCodeException("THERE_ARE_UNRECOGNIZED_RECORDS_OF_THE_DRUG_AND_CAN_NOT_BE_REAPPLIED");
            }

            var result = _fyDmnService.SelectRpList(keyword, fph, kssj, jssj, Constants.CurrentYfbm.yfbmCode, OrganizeId);
            return Content(result.ToJson());
        }
        #endregion

        #region 门诊排药

        #region old code

        /// <summary>
        /// 初始化排药员
        /// </summary>
        /// <returns></returns>
        public ActionResult initPYY()
        {
            var data = OperatorProvider.GetCurrent();
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加载获取排药统计信息
        /// </summary>
        /// <returns></returns>
        public ActionResult loadpyhj()
        {
            var yyqconfig = _sysConfigRepo.GetValueByCode("fyyxq", OrganizeId);

            if (yyqconfig == "" || string.IsNullOrWhiteSpace(yyqconfig))
            {
                throw new FailedException("请先配置发药有效期");
            }

            var yxq = int.Parse(yyqconfig);
            var reqObj = new
            {
                yfbmCode = Constants.CurrentYfbm.yfbmCode,
                yxq = yxq,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/WaitingDispenseMedicineQuery", reqObj);
            return Content(apiResp.data.ToJson());
        }

        /// <summary>
        /// 加载获取排药主表信息
        /// </summary>
        /// <returns></returns>
        public ActionResult PyInfoSearch()
        {
            var yyqconfig = _sysConfigRepo.GetValueByCode("fyyxq", OrganizeId);

            if (yyqconfig == "" || string.IsNullOrWhiteSpace(yyqconfig))
            {
                throw new FailedException("请先配置发药有效期");
            }
            var yxq = int.Parse(yyqconfig);
            var reqObj = new
            {
                yfbmCode = Constants.CurrentYfbm.yfbmCode,
                yxq = yxq,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/WaitingDispenseMedicineQuery", reqObj);
            if (apiResp.code == ResponseResultCode.SUCCESS)
            {
                return Content(apiResp.data.ToJson());
            }
            throw new FailedException("排药接口信息获取失败");
        }

        /// <summary>
        /// 加载获取排药子表信息
        /// </summary>
        /// <returns></returns>
        public ActionResult pyDeailInfoSearch(string pCfh)
        {
            if (string.IsNullOrWhiteSpace(pCfh) || pCfh == "undefined")
            {
                return Content(null);
            }

            var result = _fyDmnService.SelectRpDetail(pCfh, Constants.CurrentYfbm.yfbmCode, OrganizeId);
            return Content(result.ToJson());
        }

        /// <summary>
        /// 排药
        /// </summary>
        /// <param name="cfnm">处方内码</param>
        /// <param name="sfck">收费窗口</param>
        /// <returns></returns>
        public ActionResult SetPyInfo(string cfnm, string sfck)
        {
            //1. 获取处方药品信息
            var reqObj = new
            {
                cfnm = cfnm,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GetpyParListBycfnm", reqObj);
            if (apiResp.code != ResponseResultCode.SUCCESS)
            {
                throw new FailedException("处方信息接口获取失败");
            }
            //2. 获取处方总金额和领药药房
            reqObj = new
            {
                cfnm = cfnm,
                TimeStamp = DateTime.Now
            };
            var apicfRep = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GetcfInfoBycfnm", reqObj);
            if (apicfRep.code != ResponseResultCode.SUCCESS)
            {
                throw new FailedException("处方总金额和领药药房接口获取失败");
            }
            var vo = apicfRep.data.ToJson().ToList<cdInfoVO>().FirstOrDefault();

            //3. 循环配药
            var res = _pharmacyDrugStorageDmnService.OutPatientBookItem(apiResp.data, cfnm, vo);
            if (!string.IsNullOrWhiteSpace(res))
            {
                return Error(res);
            }

            //5. 更新发放状态
            var reqzt = new
            {
                cfnm = cfnm,
                lyck = sfck ?? "",
                user_code = this.UserIdentity.UserCode,
                TimeStamp = DateTime.Now
            };
            var apiztRep = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/Updatefyzt", reqzt);
            if (apiztRep.code != ResponseResultCode.SUCCESS)
            {
                throw new FailedException("配药接口处理失败");
            }
            return Success("配药成功！");
        }
        #endregion

        #region 排药  new


        /// <summary>
        /// 门诊排药查询
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugArrangementQuery()
        {
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }

        /// <summary>
        /// 门诊排药查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientDrugArrangementForzenQuery()
        {
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }

        /// <summary>
        /// 加载获取排药主表信息
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryCfs()
        {
            var queryResult = new QueryCfsApp("").Process();
            if (queryResult != null && queryResult.IsSucceed)
            {
                return Content(((List<MzCfEntity>)queryResult.Data).ToJson());
            }
            throw new FailedException(queryResult != null ? queryResult.ResultMsg : "获取处方失败");
        }

        /// <summary>
        /// 异步拉取新结算处方
        /// </summary>
        /// <returns></returns>
        public ActionResult WaitingDispenseMedicineQuery()
        {
            var reqObj = new
            {
                yfbmCode = Constants.CurrentYfbm.yfbmCode,
                yxq = 100,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/WaitingDispenseMedicineQuery", reqObj);
            var tags = new Dictionary<string, string>()
            {
                { Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId},
                { Constants.Yfbm, Constants.CurrentYfbm.yfbmCode},
                { Constants.CreatorCode, OperatorProvider.GetCurrent().UserCode}
            };
            if (apiResp.code == ResponseResultCode.SUCCESS || apiResp.data != null)
            {
                var cfs = apiResp.data.ToString().ToObject<List<Domain.DTO.OutPatientPharmacy.OutPatientpyCfListDTO>>();
                if (cfs == null || cfs.Count <= 0)
                {
                    return Success();
                }

                var downloadResult = new DownloadCfsApp(cfs).Process();
                if (downloadResult != null && downloadResult.IsSucceed)
                {
                    return Content(((List<MzCfEntity>)downloadResult.Data).ToJson()); ;
                }
                LogCore.Fatal("QueryCfsApp WaitingDispenseMedicineQuery fail", message: downloadResult != null ? downloadResult.ResultMsg : "下载处方失败", addInfo: tags);
            }
            LogCore.Fatal("QueryCfsApp WaitingDispenseMedicineQuery fail", message: apiResp.ToJson(), addInfo: tags);
            return Error("异步拉取新结算处方失败");
        }

        /// <summary>
        /// 获取处方信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetCfInfo(string cardNo, string xm)
        {

            var rpList = _mzCfRepo.GetRpInfo(Constants.CurrentYfbm.yfbmCode, cardNo, xm, EnumFybz.Wp, OrganizeId);
            if (rpList == null)
            {
                return Content(null);
            }

            decimal zje = 0;
            var completeCfhs = new StringBuilder();
            var completeFph = new StringBuilder();
            rpList.ForEach(p =>
            {
                zje += p.je;
                completeCfhs.Append(p.cfh + ",");
                if (p.Fph != null && completeFph.ToString().IndexOf(p.Fph, StringComparison.Ordinal) == -1)
                {
                    completeFph.Append(p.Fph + ",");
                }
            });


            var partCfhs = completeCfhs.Length > 70 ? completeCfhs.ToString().Substring(0, 70) + "..." : completeCfhs.ToString();
            var partFph = completeFph.Length > 70 ? completeFph.ToString().Substring(0, 30) + "..." : completeFph.ToString();

            var result = new cfInfoVo
            {
                xm = rpList[0].xm,
                nl = rpList[0].nl,
                brxzmc = rpList[0].brxzmc,
                CardNo = rpList[0].CardNo,
                cfh = partCfhs.Trim(','),
                cfhComplete = completeCfhs.ToString().Trim(','),
                cfnm = rpList[0].cfnm,
                Fph = partFph.Trim(','),
                FphComplete = completeFph.ToString().Trim(','),
                fybz = rpList[0].fybz,
                ysmc = rpList[0].ysmc,
                ksmc = rpList[0].ksmc,
                Zje = zje,
            };
            return Content(result.ToJson());
        }

        /// <summary>
        /// 获取处方信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCfInfo(string cfh)
        {
            var entity = _mzCfRepo.FindEntity(p => p.cfh == cfh);
            return Content(entity != null ? entity.ToJson() : null);
        }

        /// <summary>
        /// 加载获取排药子表信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryCfmx(string cardNo, string xm)
        {
            var result = new List<CfmxVo>();
            var cfs = _mzCfRepo.GetCfs(cardNo, xm);
            var failMsg = "";
            if (cfs != null && cfs.Count > 0)
            {
                ActResult appResult;
                cfs.ForEach(p =>
                {
                    appResult = new DownloadCfmxApp(p.cfh).Process();
                    if (appResult.IsSucceed && appResult.Data != null)
                    {
                        var ret = appResult.Data as List<MzCfmxEntity>;
                        if (ret != null)
                        {
                            ret.ForEach(q =>
                            {
                                var cfmx = q.MapperTo<MzCfmxEntity, CfmxVo>();
                                cfmx.sfsj = p.sfsj;
                                cfmx.fph = p.Fph;
                                result.Add(cfmx);
                            });
                        }
                    }
                    failMsg = appResult.ResultMsg;
                });
            }

            if (result.Count > 0)
            {
                result = result.OrderBy(p => p.fph).ThenBy(p => p.cfh).ThenBy(p => p.czh).ToList();
                return Content(result.ToJson());
            }
            if (!string.IsNullOrWhiteSpace(failMsg))
            {
                throw new FailedException(failMsg);
            }

            return Content(null);
        }

        /// <summary>
        /// 加载获取排药子表信息
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryCfmx(string pCfh)
        {
            var result = new DownloadCfmxApp(pCfh).Process();
            if (result.IsSucceed && result.Data != null)
            {
                var ret = result.Data as List<MzCfmxEntity>;
                if (ret != null)
                {
                    return Content(ret.ToJson());
                }
            }
            throw new FailedException(result.ResultMsg);
        }

        /// <summary>
        /// 获取患者姓名和收费时间
        /// </summary>
        /// <returns></returns>
        public ActionResult GetXmAndSfsj(Pagination pagination)
        {
            var result = _mzCfRepo.GetXmAndCardNo(pagination, Constants.CurrentYfbm.yfbmCode, OrganizeId);
            if (result == null)
            {
                throw new FailedException("所有待排完成，请同步新处方！");
            }

            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 加载获取排药子表信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RpDetailQuery(string cardNo, string xm)
        {
            var result = new List<CfmxVo>();
            var yfbmCode = Constants.CurrentYfbm.yfbmCode;
            var cfs = _mzCfRepo.IQueryable(p => p.xm == xm && p.CardNo == cardNo && p.OrganizeId == OrganizeId &&
                                               p.lyyf == yfbmCode && p.zt == "1" &&
                                               p.fybz == ((int)EnumFybz.Wp).ToString() &&
                                               p.jsnm > 0);
            if (!cfs.Any())
            {
                return Content(null);
            }

            cfs.ToList().ForEach(p =>
            {
                var rpDetails = _mzCfmxRepo.IQueryable(q => q.cfh == p.cfh && q.zt == "1" && q.OrganizeId == OrganizeId);
                if (!rpDetails.Any())
                {
                    return;
                }

                var result1 = result;
                Parallel.ForEach(rpDetails.ToList(), t =>
                {
                    var tmp = t.MapperTo<MzCfmxEntity, CfmxVo>();
                    tmp.sfsj = p.sfsj;
                    tmp.fph = p.Fph;
                    result1.Add(tmp);
                });
            });
            result = result.OrderBy(p => p.fph).ThenBy(p => p.cfh).ThenBy(p => p.czh).ToList();
            return Content(result.ToJson());
        }

        /// <summary>
        /// 获取排药处方
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="xm"></param>
        /// <param name="cardNo"></param>
        /// <param name="invoiceNo"></param>
        /// <param name="ksmc"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="jszt">结算状态 0-未结算 1-已结算 2/其他-全部</param>
        /// <param name="settExpired">结算已过期 1-是 0-否</param>
        /// <returns></returns>
        public ActionResult QueryDrugArrangementRpByPage(Pagination pagination, string xm, string cardNo, string invoiceNo, string ksmc, DateTime kssj, DateTime jssj, int jszt, string settExpired)
        {
            var result = _pyDmnService.SelectRpList(pagination, xm, cardNo, invoiceNo, ksmc, kssj, jssj, OrganizeId, Constants.CurrentYfbm.yfbmCode, jszt, settExpired);
            if (result.Count > 0)
            {
                result = result.Where(p => new[] { ((int)EnumFybz.Yp).ToString(), ((int)EnumFybz.Wp).ToString() }.Contains(p.fybz)).ToList();
            }
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取排药处方
        /// </summary>
        /// <param name="xm"></param>
        /// <param name="cardNo"></param>
        /// <param name="invoiceNo"></param>
        /// <param name="ksmc"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="jszt">结算状态 0-未结算 1-已结算 其他-全部</param>
        /// <returns></returns>
        public ActionResult QueryDrugArrangementRp(string xm, string cardNo, string invoiceNo, string ksmc, DateTime kssj, DateTime jssj, int jszt)
        {
            var result = _pyDmnService.SelectRpList(xm, cardNo, invoiceNo, ksmc, kssj, jssj, OrganizeId,
                Constants.CurrentYfbm.yfbmCode, jszt);
            if (result.Count > 0)
            {
                result = result.Where(p =>
                    new[] { ((int)EnumFybz.Yp).ToString(), ((int)EnumFybz.Wp).ToString() }.Contains(p.fybz)).ToList();
            }

            return Content(result.ToJson());
        }

        /// <summary>
        /// 获取排药处方明细
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="cardNo"></param>
        /// <returns></returns>
        public ActionResult QueryDrugArrangementRpDetail(string cfh, string cardNo)
        {
            if (string.IsNullOrWhiteSpace(cfh))
            {
                return Content(null);
            }

            var result = _pyDmnService.SelectRpArrangementDetail(cardNo, cfh, OrganizeId, Constants.CurrentYfbm.yfbmCode);
            return Content(result.ToJson());
        }

        /// <summary>
        /// 取消门诊排药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="cardNo"></param>
        /// <returns></returns>
        public ActionResult DrugArrangementReturn(string cfh, string cardNo)
        {
            if (string.IsNullOrWhiteSpace(cfh))
            {
                throw new FailedException("请选择操作的患者！");
            }

            string result;
            try
            {
                var userCode = OperatorProvider.GetCurrent().UserCode;
                result = _pyDmnService.CancelArrangedDrug(cardNo, cfh, Constants.CurrentYfbm.yfbmCode, OrganizeId, userCode);
            }
            catch (FailedException e)
            {
                result = e.Msg;
            }
            return string.IsNullOrWhiteSpace(result) ? Success("操作成功") : Error(result);
        }
        /// <summary>
        /// 门诊排药
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugArrangement2018()
        {
            return View();
        }

        /// <summary>
        /// 门诊排药
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugArrangement2019()
        {
            return View();
        }

        /// <summary>
        /// 排药
        /// </summary>
        /// <param name="patients"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DrugArrangementV2(patientInfoVO[] patients)
        {
            if (patients == null || patients.Length <= 0)
            {
                throw new FailedException("请选择需要排药的患者！");
            }

            var errorMsg = new StringBuilder();
            var yfbmCode = Constants.CurrentYfbm.yfbmCode;
            foreach (var item in patients)
            {
                errorMsg.Append(_pyDmnService.DrugArrangement(item, yfbmCode, OrganizeId, OperatorProvider.GetCurrent().UserCode));

            }
            if (string.IsNullOrWhiteSpace(errorMsg.ToString()))
            {
                return Success("排药成功");
            }

            throw new FailedException(errorMsg.ToString());
        }

        #endregion

        #endregion

        #region 门诊发药

        #region old code

        /// <summary>
        /// 查询门诊发药主表信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult SearchFyInfo(string keyword)
        {
            var reqObj = new
            {
                keyword = keyword ?? "",
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GetfyMainInfoList", reqObj);
            return Content(apiResp.data.ToJson());
        }

        /// <summary>
        /// 查询门诊发药子表信息
        /// </summary>
        /// <param name="cfh"></param>
        /// <returns></returns>
        public ActionResult SearchFyDetail(string cfh)
        {
            if (string.IsNullOrEmpty(cfh))
            {
                return null;
            }
            var reqObj = new
            {
                keyword = cfh,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GetfyDetailInfoList", reqObj);
            if (apiResp.code != ResponseResultCode.SUCCESS)
            {
                throw new FailedException("门诊发药处方明细查询失败");
            }
            var result = _fyDmnService.GetDetailInfo(apiResp.data.ToJson().ToList<fyDetailListRequest>()).ToJson();
            return Content(result);
        }

        ///// <summary>
        ///// 门诊发药
        ///// </summary>
        ///// <param name="pCfh"></param>
        ///// <returns></returns>
        //public ActionResult ExecMzHandOutMedicine(string pCfh)
        //{
        //    if (string.IsNullOrEmpty(pCfh))
        //    {
        //        return Error(string.Format("处方{0} 没有要发药的信息！", pCfh));
        //    }
        //    var reqObj = new
        //    {
        //        cfh = pCfh,
        //        usercode = Constants.CurrentYfbm.yfbmCode,
        //        TimeStamp = DateTime.Now,
        //        Token = SiteSettAPIHelper.GetToken()
        //    };
        //    var apiResp = SiteSettAPIHelper.Request<DefaultResponse>("api/OutpatientPharmacy/GetfyDetailCFInfo", reqObj);
        //    if (apiResp.data == null || apiResp.data.ToJson() == "")
        //    {
        //        return Error("该处方不存在发药信息");
        //    }
        //    var reqdata = apiResp.data.ToJson().ToList<GetfyDetailCFList>();
        //    var data = _fyDmnService.MzDistributingMedicines(reqdata);
        //    if (data == null || data.Count <= 0) return Error(string.Format("处方{0} 发药失败", pCfh));
        //    var ztObj = new
        //    {
        //        cfnm = string.Join(",", data),
        //        user_code = UserIdentity.UserCode,
        //        TimeStamp = DateTime.Now,
        //        Token = SiteSettAPIHelper.GetToken()
        //    };
        //    SiteSettAPIHelper.Request<DefaultResponse>("api/OutpatientPharmacy/UpdatefyztByFY", ztObj);
        //    return Success();
        //}

        ///// <summary>
        ///// 全部发药
        ///// </summary>
        ///// <param name="cfh"></param>
        ///// <returns></returns>
        //public ActionResult ExecAllMzHandOutMedicine(List<string> cfh)
        //{
        //    foreach (var item in cfh)
        //    {
        //        if (string.IsNullOrEmpty(item))
        //        {
        //            return Error(string.Format("处方{0}没有要发药的信息！", item));
        //        }
        //        var reqObj = new
        //        {
        //            cfh = item,
        //            usercode = OperatorProvider.GetCurrent().DepartmentCode,
        //            TimeStamp = DateTime.Now,
        //            Token = SiteSettAPIHelper.GetToken()
        //        };
        //        var apiResp = SiteSettAPIHelper.Request<DefaultResponse>("api/OutpatientPharmacy/GetfyDetailCFInfo", reqObj);
        //        if (apiResp.data.ToJson() == null || (apiResp.data.ToJson() == ""))
        //        {
        //            return Error(string.Format("处方{0}不存在发药信息", item));
        //        }
        //        var reqdata = apiResp.data.ToJson().ToList<GetfyDetailCFList>();
        //        var data = _fyDmnService.MzDistributingMedicines(reqdata);
        //        if (data == null || data.Count <= 0)
        //        {
        //            return Error(string.Format("处方{0} 发药失败", item));
        //        }

        //        var ztObj = new
        //        {
        //            cfnm = string.Join(",", data),
        //            user_code = UserIdentity.UserCode,// OperatorProvider.GetCurrent().DepartmentCode,
        //            TimeStamp = DateTime.Now,
        //            Token = SiteSettAPIHelper.GetToken()
        //        };
        //        SiteSettAPIHelper.Request<DefaultResponse>("api/OutpatientPharmacy/UpdatefyztByFY", ztObj);
        //    }
        //    return Success();
        //}
        #endregion

        #region new 门诊发药

        /// <summary>
        /// 门诊发发药
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugDelivery2018()
        {
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            return View();
        }
        public ActionResult ChooseCf()
        {

            return View();
        }

        /// <summary>
        /// 获取患者姓名和收费时间
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFyXmAndSfsj(Pagination pagination, string keyword = "")
        {
            var result = _mzCfRepo.GetXmAndCardNo(pagination, Constants.CurrentYfbm.yfbmCode, OrganizeId, keyword, EnumFybz.Yp);
            if (result == null) throw new FailedException("所有待排完成，请同步新处方！");
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        /// <summary>
        /// 获取发药处方信息
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="xm"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetCfXX(string cardNo, string xm)
        {
            var rpList = _fyDmnService.GetRpInfo(Constants.CurrentYfbm.yfbmCode, cardNo, xm, EnumFybz.Yp, OrganizeId);
            if (rpList == null)
            {
                return Content(null);
            }
            return Content(rpList.ToJson());
        }

        [HttpGet]
        public ActionResult GetFyCfInfo(string cardNo, string xm)
        {
            var rpList = _fyDmnService.GetRpInfo(Constants.CurrentYfbm.yfbmCode, cardNo, xm, EnumFybz.Yp, OrganizeId);
            if (rpList == null)
            {
                return Content(null);
            }

            decimal zje = 0;
            var completeCfhs = new StringBuilder();
            var completeFph = new StringBuilder();
            rpList.ForEach(p =>
            {
                zje += p.je;
                completeCfhs.Append(p.cfh + ",");
                if (p.Fph != null && completeFph.ToString().IndexOf(p.Fph, StringComparison.Ordinal) == -1)
                {
                    completeFph.Append(p.Fph + ",");
                }
            });


            var partCfhs = completeCfhs.Length > 70 ? completeCfhs.ToString().Substring(0, 70) + "..." : completeCfhs.ToString();
            var partFph = completeFph.Length > 70 ? completeFph.ToString().Substring(0, 30) + "..." : completeFph.ToString();

            if (rpList.Count > 0)
            {
                var result = new cfInfoVo
                {
                    xm = rpList[0].xm,
                    nl = rpList[0].nl,
                    brxzmc = rpList[0].brxzmc,
                    CardNo = rpList[0].CardNo,
                    cfh = partCfhs.Trim(','),
                    cfhComplete = completeCfhs.ToString().Trim(','),
                    cfnm = rpList[0].cfnm,
                    Fph = partFph.Trim(','),
                    FphComplete = completeFph.ToString().Trim(','),
                    fybz = rpList[0].fybz,
                    ysmc = rpList[0].ysmc,
                    ksmc = rpList[0].ksmc,
                    Zje = zje,
                };
                return Content(result.ToJson());
            }
            else
            {
                return Content(null);
            }
        }

        /// <summary>
        /// 加载获取排药子表信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryFyCfmx(string cardNo, string xm)
        {
            var result = new List<CfmxVo>();
            var cfs = _mzCfRepo.GetCfs(cardNo, xm, (int)EnumFybz.Yp);
            var failMsg = "";
            if (cfs == null || cfs.Count <= 0)
            {
                return Content(result.ToJson());
            }

            ActResult appResult;
            cfs.ForEach(p =>
            {
                appResult = new DownloadCfmxApp(p.cfh).Process();
                if (appResult.IsSucceed && appResult.Data != null)
                {
                    var ret = appResult.Data as List<MzCfmxEntity>;
                    if (ret != null)
                    {
                        ret.ForEach(q =>
                        {
                            var cfmx = q.MapperTo<MzCfmxEntity, CfmxVo>();
                            cfmx.sfsj = p.sfsj;
                            cfmx.fph = p.Fph;
                            result.Add(cfmx);
                        });
                    }
                }
                failMsg = appResult.ResultMsg;
            });

            return Content(result.ToJson());
        }

        private static object threadLocker = new object();

        /// <summary>
        /// 加载获取排药子表信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryFyCfmxV2(string cardNo, string xm)
        {
            var result = new List<CfmxVO>();
            var cfs = _mzCfRepo.GetCf(Constants.CurrentYfbm.yfbmCode, cardNo, xm, EnumFybz.Yp, true, OrganizeId);
            if (cfs.Count <= 0)
            {
                return Content(result.ToJson());
            }

            try
            {
                Parallel.ForEach(cfs, p =>
                {
                    var tmpli = new fyDmnService(new DefaultDatabaseFactory(), false).SelectCfmx(p.xm, p.CardNo, p.cfh, p.OrganizeId, p.lyyf);
                    lock (threadLocker)
                    {
                        result.AddRange(tmpli);
                    }
                });
            }
            catch (Exception e)
            {
                LogCore.Error("QueryFyCfmxV2 error", e, "门诊发药获取处方明细异常");
            }

            return Content(result.ToJson());
        }
        /// <summary>
        /// 按照处方进行查询明细
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="xm"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryFyCfmxV2_new(string cfh)
        {
            var result = new List<CfmxVO>();
            try
            {
                var tmpli = new fyDmnService(new DefaultDatabaseFactory(), false).SelectCfmx_new(cfh, OrganizeId, Constants.CurrentYfbm.yfbmCode);
                lock (threadLocker)
                {
                    result.AddRange(tmpli);
                }
            }
            catch (Exception e)
            {
                LogCore.Error("QueryFyCfmxV2_new error", e, "门诊发药获取处方明细异常");
            }
            return Content(result.ToJson());
        }
        /// <summary>
        /// 查询门诊发药主表信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult SearchCfList(string keyword)
        {
            var cfList = _mzCfRepo.GetCfsByKeyword(keyword);
            if (cfList != null && cfList.Count > 0)
            {
                return Content(cfList.ToJson());
            }
            return Content(null);
        }

        /// <summary>
        /// 预退药 处方明细查询
        /// </summary>
        /// <param name="cfh"></param>
        /// <returns></returns>
        public ActionResult SearchRefundeRpDetail(string cfh)
        {
            List<RefundedDrugVo> cfmxList;
            var partReturnSwitchStr = _sysConfigRepo.GetValueByCode("Outpatient_PreRefund_Partreturn", OrganizeId);
            if (!string.IsNullOrWhiteSpace(partReturnSwitchStr) && !("t".Equals(partReturnSwitchStr.ToLower()) || "true".Equals(partReturnSwitchStr.ToLower())))
            {
                cfmxList = _tyDmnService.SelectCompleteRefundedDrugs(cfh, Constants.CurrentYfbm.yfbmCode, OrganizeId);
            }
            else
            {
                cfmxList = _tyDmnService.SelectRefundedDrugs(cfh, Constants.CurrentYfbm.yfbmCode, OrganizeId);
            }
            return Content(cfmxList.ToJson());
        }

        /// <summary>
        /// 查询门诊发药子表信息
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SearchCfmx(string cfh, string operateType)
        {
            var cfmxList = _fyDmnService.GetDeliveryDrugs(cfh, (EnumOperateType)Convert.ToInt32(operateType));
            if (cfmxList != null && cfmxList.Count > 0)
            {
                return Content(cfmxList.ToJson());
            }
            return Content(null);
        }

        /// <summary>
        /// 全部发药
        /// </summary>
        /// <param name="patients"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExecAllDeliveryDrug(patientInfoVO[] patients)
        {
            var cfhls = new List<string>();
            var eq = new OutpatienDrugDeliveryInfo
            {
                PatientInfo = patients.ToList().GroupBy(p => new { p.cfh, p.cfnm, p.CardNo, p.xm }).Select(v => new patientInfoVO { cfh = v.Key.cfh, cfnm = v.Key.cfnm, CardNo = v.Key.CardNo, xm = v.Key.xm, zsm = string.Join("|", v.Select(f => f.zsm.Trim())) }).ToList(),
                yfbmCode = Constants.CurrentYfbm.yfbmCode,
                userCode = OperatorProvider.GetCurrent().UserCode,
                organizeId = OrganizeId
            };
            var result = _outPatientDispensingApp.ExecAllDeliveryDrugV2(eq, out cfhls);
            var cfhStr = new StringBuilder();
            cfhls.ForEach(p => cfhStr.Append(p).Append(","));
            return string.IsNullOrWhiteSpace(result) ? Success("", cfhStr.ToString().Trim(',')) : Error(result);
        }

        /// <summary>
        /// 获取处方号
        /// </summary>
        /// <param name="patients"></param>
        /// <returns></returns>
        public string GetAllNeedPrintRpByXm(patientInfoVO[] patients)
        {
            var result = new StringBuilder();
            try
            {
                if (patients == null || patients.Length <= 0)
                {
                    throw new FailedException("请选择需要打印的处方！");
                }

                foreach (var item in patients)
                {
                    var cfhs = _mzCfRepo.GetCf(Constants.CurrentYfbm.yfbmCode, item.CardNo, item.xm, EnumFybz.Yf, true, OrganizeId);
                    if (cfhs.Count > 0)
                    {
                        cfhs.ForEach(p => { result.Append(p.cfh + ","); });
                    }
                }
            }
            catch (Exception e)
            {
                LogCore.Error("GetAllNeedPrintRpByXm error", e, "门诊发药获取处方信息失败");
            }

            return result.ToString();
        }

        /// <summary>
        /// 通过处方号获取组号
        /// </summary>
        /// <param name="cfh"></param>
        /// <returns></returns>
        public string GetRecipeGroupNum(string cfh, string type)
        {
            try
            {
                string zh = "";
                var output = _mzCfRepo.GetZhInOutpatient(cfh, type);
                List<string> result = new List<string>();
                foreach (var item in output)
                {
                    result.Add(item.zh);
                }
                HashSet<string> tmp = new HashSet<string>(result);
                var returnStr = string.Join(",", tmp);
                return returnStr;
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion 

        #endregion

        #region 门诊发药查询

        /// <summary>
        /// 门诊发药查询显示详细信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="cfnm"></param>
        /// <returns></returns>
        public ActionResult SearchFyDetailInDrug(Pagination pagination, string cfnm)
        {
            var reqObj = new
            {
                cfnm = cfnm,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GetfyQueryDetailInfoList", reqObj);
            if (apiResp == null || apiResp.data == null)
            {
                return Content(null);
            }
            //var data = _fyDmnService.MZHandOutDetailQuery(apiResp.data.ToJson().ToList<fyDetailListRequest>(), pc);
            var data = _fyDmnService.GetMzHandOutDetail(apiResp.data.ToJson().ToList<fyDetailListRequest>());
            return Content(data.ToJson());
        }

        #region 门诊发药查询 new

        /// <summary>
        /// 获取发药标志
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetFybz()
        {
            var dic = EnumCommon.Instance().GetEnumDesAndVal<EnumFybz>();
            if (dic != null && dic.Count > 0)
            {
                var arr = new List<object>();
                foreach (var item in dic)
                {
                    if (item.Value == (int)EnumFybz.Wp)
                    {
                        continue;
                    }

                    arr.Add(new { key = item.Key, value = item.Value });
                }
                return Content(arr.ToJson());
            }
            return Content(null);
        }

        /// <summary>
        /// 门诊发药查询界面，主表信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ActionResult SearchCfs(Pagination pagination, searchFyInfoReqVO req)
        {
            EnumOperateType operateType;
            switch (req.type)
            {
                case "1":
                    operateType = EnumOperateType.Fy;
                    break;
                case "2":
                    operateType = EnumOperateType.Ty;
                    break;
                default:
                    operateType = EnumOperateType.None;
                    break;
            }
            var cfs = _fyDmnService.SelectDispensedRpInfo(pagination, req.keyWord, req.fph, req.cfh,
                operateType, (DateTime)req.begindate, (DateTime)req.enddate, Constants.CurrentYfbm.yfbmCode, OrganizeId);
            if (cfs == null || cfs.Count <= 0)
            {
                return Content(null);
            }

            var data = new
            {
                rows = cfs,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }

        #endregion

        #endregion

        #region 门诊退药

        /// <summary>
        /// 门诊退药
        /// </summary>
        /// <returns></returns>
        public ActionResult DrugRepercussion()
        {
            var switchValue = _sysConfigRepo.GetValueByCode("outpaitentReturnDrugAuotPrint", OrganizeId);
            ViewBag.autoPrintSwitch = string.IsNullOrWhiteSpace(switchValue) || "true".Equals(switchValue.ToLower()) ? ViewBag.autoPrintSwitch = "checked=\"checked\"" : "";
            ViewBag.OrganizeId = OrganizeId;
            ViewBag.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            ViewBag.partReturnSwitch = true;
            var partReturnSwitchStr = _sysConfigRepo.GetValueByCode("Outpatient_PreRefund_Partreturn", OrganizeId);
            if (!string.IsNullOrWhiteSpace(partReturnSwitchStr))
            {
                ViewBag.partReturnSwitch = "t".Equals(partReturnSwitchStr.ToLower()) || "true".Equals(partReturnSwitchStr.ToLower());
            }
            return View();
        }

        /// <summary>
        /// 查询门诊退药主表信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult SearchTyInfo(string keyword)
        {
            var reqObj = new
            {
                keyword = keyword ?? "",
                lyyf = Constants.CurrentYfbm.yfbmCode,//OperatorProvider.GetCurrent().DepartmentCode,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GettyMainInfoList", reqObj);
            if (apiResp == null || apiResp.data == null)
            {
                return Content("");
            }
            var reqdata = apiResp.data.ToJson().ToList<tyCFMainInfo>();
            if (reqdata.Count > 0)
            {
                reqdata = _fyDmnService.ExecMZExitMedicine(reqdata).Distinct().ToList();
            }
            return Content(reqdata.ToJson());
        }

        /// <summary>
        /// 查询门诊退药子表信息
        /// </summary>
        /// <param name="pCfh"></param>
        /// <returns></returns>
        public ActionResult SearchTyDetail(string pCfh)
        {
            if (string.IsNullOrEmpty(pCfh))
            {
                return null;
            }
            var reqObj = new
            {
                cfh = pCfh,
                TimeStamp = DateTime.Now
            };
            var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/GettyDetailInfoList", reqObj);
            var result = _fyDmnService.GetAllDetailInfo(apiResp.data.ToJson().ToList<fyDetailListRequest>()).ToJson();
            return Content(result);
        }

        #region 退药  new

        /// <summary>
        /// 查询门诊发药主表信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ActionResult SearchFyCfList(searchFyInfoReqVO req)
        {
            req = req ?? new searchFyInfoReqVO();
            req.OrganizeId = OrganizeId;
            req.yfbmCode = Constants.CurrentYfbm.yfbmCode;
            req.begindate = req.begindate ?? DateTime.Now.AddDays(-5);
            req.enddate = req.enddate ?? DateTime.Now.AddDays(1);
            var cfList = _mzCfRepo.SelectDispensedRpList(req);
            return Content(cfList.ToJson());
        }

        /// <summary>
        /// 门诊退药操作
        /// </summary>
        /// <param name="tyParam"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExecReturnStorage(tyParam[] tyParam)
        {
            var tyInfo = new tyInfo
            {
                yfbmCode = Constants.CurrentYfbm.yfbmCode,
                userCode = OperatorProvider.GetCurrent().UserCode,
                organizeId = OrganizeId,
                tyDrugDetail = tyParam.ToList()
            };
            List<string> billNoList;
            var result = _resourcesOperateApp.OutpatientReturnDrugs(tyInfo, out billNoList);
            return string.IsNullOrWhiteSpace(result) ? Success("", billNoList.ToJson()) : Error(result);
        }
        #endregion

        #endregion

        #region 门诊处方查询

        public ActionResult PrescriptionDetailFrom()
        {
            ViewBag.OrganizeId = _OrganizeId;
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMzcfGridJson(Pagination pagination, string ks, string cflx, DateTime kssj, DateTime jssj, string keyword)
        {
            var reqObj = new MzcfcxVo
            {
                ks = ks,
                kssj = kssj,
                cflx = cflx,
                jssj = jssj,
                keyword = keyword,
                organizeId = OrganizeId
            };
            var data = new
            {
                rows = _fyDmnService.GetMzcfList(pagination, reqObj).ToList(),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMzcfGridDetailJson(Pagination pagination, string cfh)
        {
            var reqObj = new MzcfcxVo
            {
                cfh = cfh,
                organizeId = OrganizeId
            };
            var data = new
            {
                rows = _fyDmnService.GetMzcfDetailList(pagination, reqObj).ToList(),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        public ActionResult GetTheLowerKsCodeList(string keyword)
        {
            var data = _ISysYpksfypzEntityRepo.GetCodeName(OrganizeId, 0, keyword);
            return Content(data.ToJson());
        }

        #endregion

        #region  电子处方审核
        public ActionResult ElectronicPrescription()
        {
            return View();
        }
        /// <summary>
        /// 获取电子处方病人信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetElectronicPrescription(Pagination pagination, string isysh, string keyword = "")
        {
            var result = _mzCfRepo.GetElectronicPrescription(pagination, OrganizeId, isysh, keyword);
            var data = new
            {
                rows = result,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取发药处方信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetElectronicPrescriptionCfInfo(string cfh, string xm)
        {
            var rpList = _fyDmnService.GetElectronicPrescriptionCfInfo(cfh, xm, OrganizeId);
            return Content(rpList.ToJson());
        }
        /// <summary>
        /// 获取处方明细信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult QueryElectronicPrescriptionCfmx(string cfh, string xm)
        {
            var result = _mzCfRepo.QueryElectronicPrescriptionCfmx(cfh, xm, OrganizeId);
            return Content(result.ToJson());
        }
        /// <summary>
		/// 门诊就诊登记修改获取
		/// </summary>
		/// <param name="mzh"></param>
		/// <returns></returns>
		public ActionResult GetCQjzdjInfo(string mzh)
        {
            Input_2203A model = _mzCfRepo.GetCQjzdjInfo(mzh, this.OrganizeId);
            return Content(model.ToJson());
        }
        /// <summary>
        /// 修改审核不通过意见
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="ysshyj"></param>
        /// <returns></returns>
		public ActionResult UpdateCfYsshyj(string cfh, string ysshyj)
        {
            _mzCfRepo.UpdateCfYsshyj(cfh, ysshyj, this.OrganizeId);
            return Content("");
        }

        #endregion
    }
}