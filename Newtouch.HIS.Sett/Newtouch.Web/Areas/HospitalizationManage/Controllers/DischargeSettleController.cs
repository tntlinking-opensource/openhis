using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Application.Interface.HospitalizationManage;
using Newtouch.HIS.Domain.DTO;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.OutpatientManage;
using Newtouch.HIS.Domain.Entity.PatientManage;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IDomainServices.API;
using Newtouch.HIS.Domain.IDomainServices.HospitalizationManage;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.IRepository.OutpatientManage;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Proxy.guian;
using Newtouch.HIS.Proxy.guian.DTO;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.Model;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.HospitalizationManage.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class DischargeSettleController : ControllerBase
    {
        // GET: HospitalizationManage/DischargeSettle

        private readonly IDischargeSettleApp _dischargeSettleApp;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IPayApp _payApp;
        private readonly IDischargeSettleDmnService _dischargeSettleDmnService;
        private readonly IHospSimulateSettlementGAYBFeeRepo _hospSimulateSettlementGAYBFeeRepo;
        private readonly IHospFeeDmnService _hospFeeDmnService;
        private readonly IHospSettlementGAYBZYMXXRFeeRepo _hospSettlementGAYBZYMXXRFeeRepo;
        private readonly IInpatientApp _inpatientApp;
        private readonly IPatientBasicInfoDmnService _patientBasicInfoDmnService;
        private readonly ICqybSett05Repo _cqybSett05Repo;
        private readonly IBookingDmnService _bookingDmnService;
        private readonly IHospMultiDiagnosisRepo _hospMultiDiagnosisRepo;
        private readonly IHospSettlementRepo _hospSettlementRepo;
        private readonly IHospDrugBillingRepo _hospdrugbillingRepo;

        public override ActionResult Index()
        {
            return View();
        }

        public ActionResult CancelIndex()
        {
            return View();
        }

        public ActionResult MedicalInsuranceApproval()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmForm()
        {
            return View();
        }

        public ActionResult SettConfirmForm()
        {
            return View();
        }

        public ActionResult HosAccountDeposit()
        {
            return View();
        }
        public ActionResult SimulateFeeItems()
        {
            return View();
        }
        public ActionResult HospFeeItems()
        {
            return View();
        }
        public ActionResult DetailedQuery()
        {
            return View();
        }
        public ActionResult HospFeeDetailedQuery()
        {
            return View();
        }
        public ActionResult HospZyFeeDetailedQuery()
        {
            return View();
        }
        public ActionResult SimulateIndex2021()
        {
            return View();
        }

        public ActionResult Index2021()
        {
            return View();
        }

        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetUploadFeeDetails(Pagination pagination, string zyh)
        {
            decimal ybzje = Convert.ToDecimal(0.0000);
            var data = new
            {
                updata = _dischargeSettleDmnService.GetUploadFeeDetails(pagination, zyh, this.OrganizeId, this.UserIdentity.UserName, out ybzje),
                ybzje = ybzje,
                total = pagination.total,
                page = pagination.page
            };
            return Content(data.ToJson());
        }

        #region 重庆医保
        public ActionResult GetCQUploadFeeDetails(Pagination pagination, string zyh)
        {
            decimal ybzje = Convert.ToDecimal(0.0000);
            decimal zfzje = Convert.ToDecimal(0.0000);
            var data = new
            {
                updata = _dischargeSettleDmnService.GetCQUploadFeeDetails(pagination, zyh, this.OrganizeId, this.UserIdentity.UserCode, out ybzje, out zfzje),
                ybzje = ybzje,
                //zfzje = zfzje,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(data.ToJson());
        }
        public ActionResult GetCQAlreadyUploadFeeDetails(string mzzyh)
        {
            var ybzje = _dischargeSettleDmnService.GetCQAlreadyUploadFeeDetails(mzzyh, this.OrganizeId);
            var data = new
            {
                ybzje = ybzje
            };
            return Content(data.ToJson());
        }
        public ActionResult GetNogjybdm(string mzzyh, string cfnms, string cflx)
        {
            var xmmc = _dischargeSettleDmnService.Getgjybdm(mzzyh, cfnms, cflx, this.OrganizeId);
            var data = new
            {
                xmmc = xmmc
            };
            return Content(data.ToJson());

        }
        /// <summary>
        /// 上传至医保总费用
        /// </summary>
        /// <param name="mzzyh"></param>
        /// <returns></returns>
        public ActionResult GetCQAlreadyUploadFeeDetailsV2(string mzzyh, DateTime jssj)
        {
            var ybzje = _dischargeSettleDmnService.GetCQAlreadyUploadFeeDetailsV2(mzzyh, this.OrganizeId, jssj);
            var data = new
            {
                ybzje = ybzje.zje,
                csrq = ybzje.cyrq.ToString("yyyy-MM-dd")
            };
            return Content(data.ToJson());
        }
        public ActionResult ValialUploadData(string zyh, DateTime jssj, string jsbz)
        {
            var zjjfbh = "";
            var jfbh = _dischargeSettleDmnService.ValialUploadData(zyh, this.OrganizeId, jssj);
            if (jsbz == "2")//中途结算
            {
                zjjfbh = _dischargeSettleDmnService.ValialPartialUploadData(zyh, this.OrganizeId, jssj);
            }
            var data = new
            {
                jfbh = jfbh,
                zjjfbh = zjjfbh
            };
            return Content(data.ToJson());

        }
        /// <summary>
        /// 获取上海医保未上传的信息
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="jssj"></param>
        /// <param name="jsbz"></param>
        /// <returns></returns>
        public ActionResult ValialUploadDataShyb(string zyh, DateTime jssj, string jsbz)
        {
            var zjjfbh = "";
            var jfbh = _dischargeSettleDmnService.ValialUploadDataShyb(zyh, this.OrganizeId, jssj);
            var data = new
            {
                jfbh = jfbh,
                zjjfbh = zjjfbh
            };
            return Content(data.ToJson());

        }
        /// <summary>
        /// 获取结算回退信息
        /// </summary>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public ActionResult GetCQJshtInfo(int jsnm)
        {
            CqybSett05Entity entity =
                _cqybSett05Repo.FindEntity(p => p.jsnm == jsnm && p.zt == "1" && p.OrganizeId == this.OrganizeId);
            return Content(entity.ToJson());
        }

        /// <summary>
        /// 获取结算信息
        /// </summary>
        /// <param name="jsnm"></param>
        /// <returns></returns>
        public ActionResult GetCQJsPatInfo(string zyh)
        {
            var data = _dischargeSettleDmnService.GetCQInpatientSettYbPatInfo(zyh, this.OrganizeId);
            if (data == null)
            {
                return Error("入院办理信息未找到");
            }
            return Content(data.ToJson());
        }
        #endregion

        #region 贵安新农合

        public ActionResult GuiAnXnhMnjs(string zyh)
        {
            try
            {
                bool isSuccess = true;
                string message = string.Empty;
                string state = "1";
                object retdata = null;
                if (string.IsNullOrEmpty(zyh))
                {
                    isSuccess = false;
                    state = "0";
                    message = "住院号不能为空，请重试！";
                }

                InpatientSettXnhPatInfoVO patinfo = null;
                if (isSuccess)
                {
                    patinfo = _dischargeSettleDmnService.GetInpatientSettXnhPatInfo(zyh, this.OrganizeId);
                    if (patinfo == null || string.IsNullOrEmpty(patinfo.inpId))
                    {
                        isSuccess = false;
                        state = "0";
                        message = "获取新农合患者住院补偿序号inpId为空，请联系his处理";
                    }
                }


                if (isSuccess)
                {
                    decimal nhfyzje;
                    S10RequestDTO S10ReqDto = _dischargeSettleDmnService.GetUploadXnhFeeDetails(zyh, this.OrganizeId, out nhfyzje);
                    if (S10ReqDto.list.Count == 0)
                    {
                        isSuccess = false;
                        state = "0";
                        message = "该患者所产生的费用中，没有可报销的项目或药品，无法进行农合结算！请核对项目或药品配置是否有误，或将该患者转为自费结算！";
                    }

                    if (isSuccess)
                    {
                        string delMsg = "";
                        //每次上传之前，先删除历史上传的明细
                        if (GuiAnXnhDelFeeDetails(S10ReqDto.inpId, out delMsg))
                        {

                            //住院费用明细上传
                            Response<S10ResponseDTO> S10ResDto = HospitalizationProxy.GetInstance(OrganizeId).S10(S10ReqDto);
                            if (S10ResDto.state)
                            {
                                S13RequestDTO S13ReqDTO = new S13RequestDTO()
                                {
                                    inpId = patinfo.inpId
                                };
                                Response<S13ResponseDTO> S13ResDto = HospitalizationProxy.GetInstance(OrganizeId).S13(S13ReqDTO);
                                if (S13ResDto.state)
                                {
                                    S13OutResponseDTO S13Out = new S13OutResponseDTO();
                                    S13ResDto.data.MapperTo(S13Out);
                                    S13Out.nhzje = nhfyzje;
                                    S13Out.salvaQTCost = Convert.ToDecimal(S13Out.accidentRedeem) +
                                                         Convert.ToDecimal(S13Out.bottomSecondRedeem ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.medicineCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.trafficCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.liveCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.medicalCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.salvaYFCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.salvaCLCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.salvaJKCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.continentInsuranceCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.nurseCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.cwAccountCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.specialPovertyCost ?? "0.00") +
                                                         Convert.ToDecimal(S13Out.medicalAssistanceCost ?? "0.00");
                                    S13Out.nhxjzf = Convert.ToDecimal(S13Out.totalCost) -
                                                    Convert.ToDecimal(S13Out.compensateCost ?? "0.00") -
                                                    Convert.ToDecimal(S13Out.insureCost ?? "0.00") -
                                                    Convert.ToDecimal(S13Out.civilCost ?? "0.00") -
                                                    Convert.ToDecimal(S13Out.salvaJSCost ?? "0.00") -
                                                    Convert.ToDecimal(S13Out.bottomRedeem ?? "0.00") -
                                                    S13Out.salvaQTCost;

                                    retdata = S13Out;
                                }
                                else
                                {
                                    isSuccess = false;
                                    state = "0";
                                    message = "模拟结算失败，农保返回信息为：【" + S13ResDto.message + "】请重试！";
                                }
                            }
                            if (!S10ResDto.state)
                            {
                                isSuccess = false;
                                state = "0";
                                message = "新农合患者住院费用明细上传失败：" + S10ResDto.message;
                            }
                        }
                        else
                        {
                            isSuccess = false;
                            state = "0";
                            message = "新农合住院患者，删除已经上传的明细失败【" + delMsg + "】";
                        }
                    }

                }
                var data = new
                {
                    state = state,
                    message = message,
                    Data = retdata
                };
                return Content(data.ToJson());

            }
            catch (Exception ex)
            {
                var data = new
                {
                    state = "0",
                    message = ex.Message,
                    Data = ""
                };
                return Content(data.ToJson());
            }
        }

        /// <summary>
        /// 贵安新农合结算
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GuiAnXnhSettlement(string zyh)
        {
            try
            {
                bool isSuccess = true;
                string message = string.Empty;
                string state = "1";
                object retdata = null;
                if (string.IsNullOrEmpty(zyh))
                {
                    isSuccess = false;
                    state = "0";
                    message = "住院号不能为空，请重试！";
                }

                InpatientSettXnhPatInfoVO patinfo = null;
                if (isSuccess)
                {
                    patinfo = _dischargeSettleDmnService.GetInpatientSettXnhPatInfo(zyh, this.OrganizeId);
                    if (patinfo == null || string.IsNullOrEmpty(patinfo.inpId))
                    {
                        isSuccess = false;
                        state = "0";
                        message = "获取新农合患者住院补偿序号inpId为空，请联系his处理";
                    }
                }


                if (isSuccess)
                {
                    decimal nhfyzje;
                    S10RequestDTO S10ReqDto = _dischargeSettleDmnService.GetUploadXnhFeeDetails(zyh, this.OrganizeId, out nhfyzje);
                    if (S10ReqDto.list.Count == 0)
                    {
                        isSuccess = false;
                        state = "0";
                        message = "该患者所产生的费用中，没有可报销的项目或药品，无法进行农合结算！请核对项目或药品配置是否有误，或将该患者转为自费结算！";
                    }
                    string delMsg = "";
                    if (isSuccess)
                    {
                        //每次上传之前，先删除历史上传的明细
                        if (GuiAnXnhDelFeeDetails(S10ReqDto.inpId, out delMsg))
                        {
                            //住院费用明细上传
                            Response<S10ResponseDTO> S10ResDto = HospitalizationProxy.GetInstance(OrganizeId).S10(S10ReqDto);
                            if (S10ResDto.state)
                            {
                                S07RequestDTO S07ReqDto =
                                    _dischargeSettleDmnService.GetXnhS07RequestDTO(zyh, this.OrganizeId);
                                if (S07ReqDto != null && !string.IsNullOrEmpty(S07ReqDto.inpId) && !string.IsNullOrEmpty(S07ReqDto.dischargeDate) && !string.IsNullOrEmpty(S07ReqDto.dischargeDepartments) && !string.IsNullOrEmpty(S07ReqDto.dischargeStatus))
                                {
                                    //患者出院办理
                                    Response<S07ResponseDTO> S07ResDto = HospitalizationProxy.GetInstance(OrganizeId).S07(S07ReqDto);
                                    if (S07ResDto.state)
                                    {
                                        S13RequestDTO S13ReqDTO = new S13RequestDTO()
                                        {
                                            inpId = patinfo.inpId
                                        };
                                        Response<S13ResponseDTO> S13ResDto = HospitalizationProxy.GetInstance(OrganizeId).S13(S13ReqDTO);
                                        if (S13ResDto.state)
                                        {
                                            S13OutResponseDTO S13Out = new S13OutResponseDTO();
                                            S13ResDto.data.MapperTo(S13Out);
                                            S13Out.nhzje = nhfyzje;
                                            S13Out.salvaQTCost = Convert.ToDecimal(S13Out.accidentRedeem) +
                                                                 Convert.ToDecimal(S13Out.bottomSecondRedeem ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.medicineCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.trafficCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.liveCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.medicalCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.salvaYFCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.salvaCLCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.salvaJKCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.continentInsuranceCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.nurseCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.cwAccountCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.specialPovertyCost ?? "0.00") +
                                                                 Convert.ToDecimal(S13Out.medicalAssistanceCost ?? "0.00");
                                            S13Out.nhxjzf = Convert.ToDecimal(S13Out.totalCost) -
                                                            Convert.ToDecimal(S13Out.compensateCost ?? "0.00") -
                                                            Convert.ToDecimal(S13Out.insureCost ?? "0.00") -
                                                            Convert.ToDecimal(S13Out.civilCost ?? "0.00") -
                                                            Convert.ToDecimal(S13Out.salvaJSCost ?? "0.00") -
                                                            Convert.ToDecimal(S13Out.bottomRedeem ?? "0.00") -
                                                            S13Out.salvaQTCost;

                                            retdata = S13Out;
                                        }
                                        else
                                        {
                                            S08RequestDTO S08ReqDTO = new S08RequestDTO()
                                            {
                                                inpId = patinfo.inpId,
                                                areaCode = "",
                                                isTransProvincial = "0",
                                                reason = ""
                                            };
                                            Response<string> S08ResDto = HospitalizationProxy.GetInstance(OrganizeId).S08(S08ReqDTO);
                                            if (S08ResDto.state)
                                            {
                                                isSuccess = false;
                                                state = "0";
                                                message = "新农合患者结算失败，农保返回信息为：【" + S13ResDto.message + "】请重试！";
                                            }
                                            else
                                            {
                                                isSuccess = false;
                                                state = "0";
                                                message = "新农合患者结算失败，农保返回信息为：【" + S07ResDto.message + "】" + "新农合患者出院办理回退失败，农保返回信息为：【" + S08ResDto.message + "】";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        isSuccess = false;
                                        state = "0";
                                        message = "新农合患者出院办理失败：" + S07ResDto.message;
                                    }
                                }
                                else
                                {
                                    isSuccess = false;
                                    state = "0";
                                    message = "新农合患者出院办理失败：获取出院办理S07必填信息为空或失败";
                                }
                            }
                            if (!S10ResDto.state)
                            {
                                isSuccess = false;
                                state = "0";
                                message = "新农合患者住院费用明细上传失败：" + S10ResDto.message;
                            }
                        }
                        else
                        {
                            isSuccess = false;
                            state = "0";
                            message = "新农合住院患者，删除已经上传的明细失败【" + delMsg + "】";
                        }
                    }


                }
                var data = new
                {
                    state = state,
                    message = message,
                    Data = retdata
                };
                return Content(data.ToJson());

            }
            catch (Exception ex)
            {
                var data = new
                {
                    state = "0",
                    message = ex.Message,
                    Data = ""
                };
                return Content(data.ToJson());
            }

        }

        /// <summary>
        /// 取消出院结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GuiAnXnhSettCancel(string zyh)
        {
            try
            {
                bool isSuccess = true;
                string message = string.Empty;
                string state = "1";
                object retdata = null;
                if (string.IsNullOrEmpty(zyh))
                {
                    isSuccess = false;
                    state = "0";
                    message = "住院号不能为空，请重试！";
                }

                InpatientSettXnhPatInfoVO patinfo = null;
                if (isSuccess)
                {
                    patinfo = _dischargeSettleDmnService.GetInpatientSettXnhPatInfo(zyh, this.OrganizeId);
                    if (patinfo == null || string.IsNullOrEmpty(patinfo.inpId))
                    {
                        isSuccess = false;
                        state = "0";
                        message = "获取新农合患者住院补偿序号inpId为空，请联系his处理";
                    }

                    if (isSuccess)
                    {
                        S15RequestDTO S15ReqDTO = new S15RequestDTO()
                        {
                            inpId = patinfo.inpId,
                            areaCode = "",
                            isTransProvincial = "0"
                        };
                        Response<string> S15ResDto = HospitalizationProxy.GetInstance(OrganizeId).S15(S15ReqDTO);
                        if (S15ResDto.state)
                        {
                            S04RequestDTO S04ReqDTO = _inpatientApp.GetZfToXnhPatInfo(zyh);
                            if (S04ReqDTO == null || string.IsNullOrEmpty(S04ReqDTO.memberId) || string.IsNullOrEmpty(S04ReqDTO.inpatientNo) || string.IsNullOrEmpty(S04ReqDTO.admissionDate) || string.IsNullOrEmpty(S04ReqDTO.admissionDepartments))
                            {
                                isSuccess = false;
                                state = "0";
                                message = "入院办理缺少必须的值！";
                            }
                            Response<S04ResponseDTO> S04ResDto = HospitalizationProxy.GetInstance(OrganizeId).S04(S04ReqDTO);
                            if (!S04ResDto.state)
                            {
                                isSuccess = false;
                                state = "0";
                                message = "入院办理失败，农保返回错误信息为：【" + S04ResDto.message + "】，请联系HIS运维解决";
                            }
                            else
                            {
                                GuianXnhS04InfoEntity S04InfoEntity = new GuianXnhS04InfoEntity()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    inpId = S04ResDto.data.inpId,
                                    zyh = zyh,
                                    xnhgrbm = patinfo.memberNo,
                                    xnhylzh = patinfo.bookNo,
                                    OrganizeId = this.OrganizeId,
                                    zt = "1"
                                };
                                string msg = string.Empty;
                                if (!_patientBasicInfoDmnService.InpatXnhInsertS04data(S04InfoEntity, out msg))
                                {
                                    isSuccess = false;
                                    state = "0";
                                    message = "新农合患者取消结算失败:【" + msg + "】";
                                }
                            }
                        }
                        else
                        {
                            isSuccess = false;
                            state = "0";
                            message = "新农合患者取消结算失败，农保返回信息为：【" + S15ResDto.message + "】";
                        }
                    }
                }
                var data = new
                {
                    state = state,
                    message = message
                };
                return Content(data.ToJson());
            }
            catch (Exception e)
            {
                var data = new
                {
                    state = "0",
                    message = "取消出院结算失败：【" + e.Message + "】"
                };
                return Content(data.ToJson());
            }
        }
        /// <summary>
        /// 出院回退
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GuiAnXnhCyblCancel(string zyh)
        {
            try
            {
                bool isSuccess = true;
                string message = string.Empty;
                string state = "1";
                object retdata = null;
                if (string.IsNullOrEmpty(zyh))
                {
                    isSuccess = false;
                    state = "0";
                    message = "住院号不能为空，请重试！";
                }

                InpatientSettXnhPatInfoVO patinfo = null;
                if (isSuccess)
                {
                    patinfo = _dischargeSettleDmnService.GetInpatientSettXnhPatInfo(zyh, this.OrganizeId);
                    if (patinfo == null || string.IsNullOrEmpty(patinfo.inpId))
                    {
                        isSuccess = false;
                        state = "0";
                        message = "获取新农合患者住院补偿序号inpId为空，请联系his处理";
                    }

                    if (isSuccess)
                    {
                        S08RequestDTO S08ReqDTO = new S08RequestDTO()
                        {
                            inpId = patinfo.inpId,
                            areaCode = "",
                            isTransProvincial = "0",
                            reason = ""
                        };
                        Response<string> S08ResDto = HospitalizationProxy.GetInstance(OrganizeId).S08(S08ReqDTO);
                        if (!S08ResDto.state)
                        {
                            isSuccess = false;
                            state = "0";
                            message = "取消出院办理失败，农保返回错误信息为：【" + S08ResDto.message + "】";
                        }
                    }
                }
                var data = new
                {
                    state = state,
                    message = message
                };
                return Content(data.ToJson());
            }
            catch (Exception e)
            {
                var data = new
                {
                    state = "0",
                    message = "取消出院结算失败：【" + e.Message + "】"
                };
                return Content(data.ToJson());
            }
        }



        /// <summary>
        /// 删除住院患者已上传的费用明细信息
        /// </summary>
        /// <param name="inpId"></param>
        /// <returns></returns>
        public bool GuiAnXnhDelFeeDetails(string inpId, out string delMsg)
        {
            S11RequestDTO S11ReqDTO = new S11RequestDTO()
            {
                inpId = inpId,
                startDate = "",
                endDate = "",
                areaCode = "",
                isTransProvincial = "0"
            };
            Response<List<detail>> S11ResDTO = HospitalizationProxy.GetInstance(OrganizeId).S11(S11ReqDTO);
            if (S11ResDTO.state)
            {
                if (S11ResDTO.data.Count == 0)
                {
                    delMsg = "";
                    return true;
                }
                List<string> listStr = new List<string>();
                foreach (var item in S11ResDTO.data)
                {
                    listStr.Add(item.detailId);
                }
                S12RequestDTO S12ReqDTO = new S12RequestDTO()
                {
                    inpId = inpId,
                    isTransProvincial = "0",
                    areaCode = "",
                    list = listStr
                };
                Response<string> S12ResDTO = HospitalizationProxy.GetInstance(OrganizeId).S12(S12ReqDTO);
                if (!S12ResDTO.state)
                {
                    delMsg = S12ResDTO.message;
                    return false;
                }
            }
            else
            {
                delMsg = S11ResDTO.message;
                return false;
            }
            delMsg = "";
            return true;
        }
        /// <summary>
        /// 根据个人编码获取住院号（新农合）
        /// </summary>
        /// <param name="xnhgrbm">个人编码</param>
        /// <param name="sfjs">是否结算1已结 0 未结</param>
        /// <returns></returns>
        public ActionResult GetZyhByGrbm(string xnhgrbm, string sfjs)
        {
            var zyh = _dischargeSettleDmnService.GetZyhByGrbm(xnhgrbm, sfjs, this.OrganizeId);
            return Success(null, zyh);
        }

        #endregion

        #region 出院结算//（无医保算法）//20190408改为有医保算法

        /// <summary>
        /// 住院号查询数据（包括病人信息和计费明细） zyh 或 kh +cardType 或 sfz +cardType
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="sfz"></param>
        /// <param name="kh"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetInpatientSettleStatusDetail(string zyh, string sfz, string kh, string cardType, string jslx, string ver)
        {
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 0);//同步项目费用
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 1);//同步药品费用
            }
            var dto = _dischargeSettleApp.GetInpatientSettleStatusDetail(zyh, sfz, kh, cardType, jslx, ver);
            return Success("", dto);
        }
        [HandlerAjaxOnly]
        public ActionResult GetInpatientSettleStatusDetailbySfdl(Pagination pagination, string zyh, string dlCode)
        {
            var dto = _dischargeSettleDmnService.GetHospGroupFeeVOList(pagination, zyh, OrganizeId, dlCode);
            var data = new
            {
                rows = dto,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 带模糊查询
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="dlCode"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetInpatientSettleStatusDetailbySfdlData(Pagination pagination, string zyh, string keyword)
        {
            var dto = _dischargeSettleDmnService.GetHospGroupFeeVOData(pagination, zyh, OrganizeId, keyword);
            var data = new
            {
                rows = dto,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 查询项目明细
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="dlCode"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetDetailedQuery(Pagination pagination, string zyh, string sfxmCode, string zzfbz, decimal? dj)
        {
            var dto = _dischargeSettleDmnService.GetDetailedQuery(pagination, zyh, OrganizeId, sfxmCode, zzfbz, dj);
            var data = new
            {
                rows = dto,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
        /// <summary>
        /// 查询住院费用账单
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="zyh"></param>
        /// <param name="dlCode"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetZyDetailedQuery(Pagination pagination, string zyh, string sfxmCode, string zzfbz)
        {
            var dto = _dischargeSettleDmnService.GetFyzdDetailedQuery(pagination, zyh, OrganizeId, sfxmCode);
            var data = new
            {
                rows = dto,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }
        #region 医保数据传输
        [HandlerAjaxOnly]
        public ActionResult GetZybrInfo(string zyh, string jslx)
        {
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 0);//同步项目费用
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 1);//同步药品费用
            }
            var dto = _dischargeSettleApp.GetZyUploadDetail(zyh, jslx);
            return Success("", dto);
        }

        public ActionResult GetZyUploadDetailJe(string zyh, string sczt, DateTime jssj, string xmmc)
        {
            var dto = _dischargeSettleApp.GetInpatientSettleJe(zyh, sczt, null, jssj, xmmc);
            return Success("", dto);
        }

        [HandlerAjaxOnly]
        public ActionResult GetZyUploadDetail(Pagination pagination, string zyh, string sczt, DateTime jssj, string xmmc, string isnewyb)
        {
            var dto = _dischargeSettleDmnService.GetHospXmYpFeeVOList(pagination, zyh, OrganizeId, sczt, xmmc, null, jssj, isnewyb);
            var data = new
            {
                rows = dto,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        public ActionResult GetHospPreSettInfo(string zyh, DateTime jzsj)
        {
            var InpatObj = _dischargeSettleDmnService.GetHospPreInfo(zyh, this.OrganizeId, jzsj);
            //var ybzje = _dischargeSettleDmnService.GetCQAlreadyUploadFeeDetailsV2(zyh, this.OrganizeId);
            var data = new
            {
                zje = InpatObj.zje,
                cyrq = InpatObj.cyrq.ToString("yyyy-MM-dd"),
                ybzje = InpatObj.ybzje
            };
            return Content(data.ToJson());
        }
        #endregion
        /// <summary>
        /// 选择出院日期
        /// </summary>
        /// <returns></returns>
        public ActionResult SettSelectCyrq()
        {
            return View();
        }

        /// <summary>
        /// 保存结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="expectedcyrq"></param>
        /// <param name="fph"></param>
        /// <param name="feeRelated">金额及支付信息</param>
        /// <param name="ybfeeRelated">医保相关费用信息</param>
        /// <param name="outTradeNo">支付交易号</param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult SaveSettle(string zyh, DateTime expectedcyrq, string fph, InpatientSettFeeRelatedDTO feeRelated
            , CQZyjs05Dto ybfeeRelated, S13OtherResponseDTO xnhfeeRelated, string jslx
            , string outTradeNo)
        {
            if (string.IsNullOrEmpty(zyh))
            {
                throw new FailedCodeException("HOSP_ZYH_IS_EMPTY");
            }

            if (string.IsNullOrEmpty(jslx))
            {
                return Error("结算类型不能为空，请联系his运维处理！");
            }
            S14OutResponseDTO S14OutRes = null;
            if (jslx == "8" && xnhfeeRelated != null)
            {
                S14OutRes = new S14OutResponseDTO();
                InpatientSettXnhPatInfoVO patinfo = _dischargeSettleDmnService.GetInpatientSettXnhPatInfo(zyh, this.OrganizeId);
                if (patinfo == null || string.IsNullOrEmpty(patinfo.inpId))
                {
                    return Error("获取新农合患者住院补偿序号inpId为空，请联系his运维处理");
                }
                S14RequestDTO S14ReqDTO = new S14RequestDTO()
                {
                    inpId = patinfo.inpId
                };
                Response<S14ResponseDTO> S14ResDto = HospitalizationProxy.GetInstance(OrganizeId).S14(S14ReqDTO);
                if (!S14ResDto.state)
                {
                    return Error("住院结算失败，调用农合接口S04返回错误：【" + S14ResDto.message + "】");
                }

                S14ResDto.data.MapperTo(S14OutRes);
                S14OutRes.salvaQTCost = xnhfeeRelated.salvaQTCost;
                S14OutRes.nhzje = xnhfeeRelated.nhzje;
                S14OutRes.nhxjzf = xnhfeeRelated.nhxjzf;
            }


            int jsnm;
            _dischargeSettleApp.SaveSett(zyh, expectedcyrq, fph, feeRelated, ybfeeRelated, S14OutRes, outTradeNo, jslx, out jsnm);
            _hospdrugbillingRepo.Updatezy_brxxexpand(this.OrganizeId, zyh);
            if (_sysConfigRepo.GetBoolValueByCode("HOSP_INTERFACE_WITH_CPOE", this.OrganizeId) == true)
            {
                //同步zybz 已出院给CIS
                SiteCISAPIHelper.UpdateInpatientBasicInfo(new InpatientPatientInfoDTO()
                {
                    zyh = zyh,
                    zybz = ((int)EnumZYBZ.Ycy).ToString(),
                });
            }
             
            var res = new
            {
                jsnm = jsnm
            };  
            return Success(null, res); 
        }

        /// <summary>
        /// 获取医保结算 构造入参
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult GetInpatientSettYbPatInfo(string zyh)
        {
            var data = _dischargeSettleDmnService.GetInpatientSettYbPatInfo(zyh, this.OrganizeId);
            if (data == null)
            {
                return Error("入院办理信息未找到");
            }
            return Success(null, data);
        }

        public ActionResult upLoadReturnData(IList<HospSettlementGAYBZYMXXRFeeEntity> upReturnData, string patzyh)
        {
            if (upReturnData != null && upReturnData.Count > 0)
            {
                foreach (var feeEntity in upReturnData)
                {
                    feeEntity.Create(true);
                    feeEntity.OrganizeId = this.OrganizeId;
                    feeEntity.zt = "1";
                    feeEntity.zyh = patzyh;
                    _hospSettlementGAYBZYMXXRFeeRepo.Insert(feeEntity);
                }
            }

            return Success();
        }

        /// <summary>
        /// 模拟结算 保存结果
        /// </summary>
        /// <param name="feeEntity"></param>
        /// <returns></returns>
        public ActionResult SubmitSimulateSettlementResult(HospSimulateSettlementGAYBFeeEntity feeEntity)
        {
            feeEntity.Create(true);
            feeEntity.OrganizeId = this.OrganizeId;
            feeEntity.zt = "1";
            _hospSimulateSettlementGAYBFeeRepo.Insert(feeEntity);
            return Success();
        }

        #endregion

        #region 取消出院结算//（无医保算法）//20190408改为有医保算法

        /// <summary>
        /// 取消结算查询
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetCancelSettleStatusDetail(string zyh, string sfz, string kh, string cardType)
        {
            var dto = _dischargeSettleApp.GetCancelSettleStatusDetail(zyh, sfz, kh, cardType);
            return Success(null, dto);
        }

        /// <summary>
        /// 取消结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="expectedjsnm"></param>
        /// <param name="cancelReason"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult DoCancel(string zyh, int expectedjsnm, string cancelReason, string cancelyblsh)
        {
            string outTradeNo;
            decimal refundAmount;
            _dischargeSettleApp.DoCancel(zyh, expectedjsnm, cancelReason, cancelyblsh, out outTradeNo, out refundAmount);

            if (_sysConfigRepo.GetBoolValueByCode("HOSP_INTERFACE_WITH_CPOE", this.OrganizeId) == true)
            {
                //同步zybz 待结账给CIS
                SiteCISAPIHelper.UpdateInpatientBasicInfo(new InpatientPatientInfoDTO()
                {
                    zyh = zyh,
                    zybz = ((int)EnumZYBZ.Djz).ToString(),
                });
            }

            bool? isTradeRefundError = null;
            if (!string.IsNullOrWhiteSpace(outTradeNo) && refundAmount > 0)   //需要原路退回
            {
                string errorMsg;
                var refundReuslt = _payApp.TradeRefund(outTradeNo, refundAmount, "门诊退费", "", out errorMsg);
                isTradeRefundError = refundReuslt == (int)EnumRefundStatus.Failed || refundReuslt == (int)EnumRefundStatus.UnKnown;    //失败 或 未知
            }
            _hospdrugbillingRepo.Updatezy_brxxexpand(this.OrganizeId, zyh);
            var msg = "取消结算成功";  
            if (isTradeRefundError.HasValue)
            {
                if (!isTradeRefundError.Value)
                {
                    msg = "取消结算成功，应退金额已原路退回";
                }
                else
                {
                    msg = "HIS取消结算成功，但应退金额退回失败，请人工核查";
                }
            } 
            return Success(msg);
        }
        #endregion

        #region 模拟结算（GA）

        /// <summary>
        /// 模拟结算
        /// </summary>
        /// <returns></returns>
        public ActionResult SimulateIndex()
        {
            return View();
        }

        /// <summary>
        /// 模拟结算 显示医保返回结果
        /// </summary>
        /// <returns></returns>
        public ActionResult SimulateForm()
        {
            return View();
        }
        public ActionResult SimulateForm2021()
        {
            return View();
        }
        public ActionResult SimulateFormShyb2023()
        {
            return View();
        }
        /// <summary>
        /// 模拟结算 提交后台 落地结果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SimulateSettSubmit()
        {
            return Success();
        }

        #endregion

        #region private methods

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

        #endregion


        #region 中途结算
        public ActionResult PartialSettleIndex()
        {
            return View();
        }
        public ActionResult PartialSettleIndex2021()
        {
            return View();
        }

        /// <summary>
        /// 中途结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="fph"></param>
        /// <param name="feeRelated"></param>
        /// <param name="ybfeeRelated"></param>
        /// <param name="xnhfeeRelated"></param>
        /// <param name="jslx"></param>
        /// <param name="outTradeNo"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult PartialSettleSave(string zyh, DateTime startTime, DateTime endTime, string fph, InpatientSettFeeRelatedDTO feeRelated
            , CQZyjs05Dto ybfeeRelated, string jslx
            , string outTradeNo)
        {
            if (string.IsNullOrEmpty(zyh))
            {
                throw new FailedCodeException("HOSP_ZYH_IS_EMPTY");
            }

            if (string.IsNullOrEmpty(jslx))
            {
                return Error("结算类型不能为空，请联系his运维处理！");
            }


            int jsnm;
            _dischargeSettleApp.SavePartialSettle(zyh, startTime, endTime, fph, feeRelated, ybfeeRelated, outTradeNo, jslx, out jsnm);
            _hospdrugbillingRepo.Updatezy_brxxexpand(this.OrganizeId, zyh);
            var res = new
            {
                jsnm = jsnm
            };
            return Success(null, res);
        }
        /// <summary>
        /// 中途结算后需处理掉上传明细的记录 避免出院时医保端取值不准
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult ExecPartialSettleDeatil(string zyh, string jsnm, string czlx)
        {
            _hospdrugbillingRepo.ExecPartialSettleFeeDetail(zyh, jsnm, czlx);
            return Success();
        }
        /// <summary>
        /// 住院号查询数据（包括病人信息和计费明细） zyh 或 kh +cardType 或 sfz +cardType
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="sfz"></param>
        /// <param name="kh"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        [HandlerAjaxOnly]
        public ActionResult GetPartialInpatientSettleStatusDetail(string zyh, string sfz, string kh, string cardType, string jslx, string ver)
        {
            if (!string.IsNullOrWhiteSpace(zyh))
            {
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 0);//同步项目费用
                _hospFeeDmnService.SyncPatFee(OrganizeId, zyh, 1);//同步药品费用
            }
            var dto = _dischargeSettleApp.GetPartialInpatientSettleStatusDetail(zyh, sfz, kh, cardType, jslx, ver);
            return Success("", dto);
        }

        /// <summary>
        /// 转出院结算
        /// </summary>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public ActionResult TransfertoCYsettle(string zyh)
        {
            var data = _dischargeSettleApp.PreCYsettle(zyh);

            SiteCISAPIHelper.UpdateInpatientBasicInfo(new InpatientPatientInfoDTO()
            {
                zyh = zyh,
                zybz = ((int)EnumZYBZ.Ycy).ToString(),
            });
            return Success("", data);
        }


        public ActionResult CancelPartialConfirmForm()
        {
            return View();
        }
        /// <summary>
        /// 获取退费金额
        /// </summary>
        /// <returns></returns>
        public ActionResult Getrefunds_available(int jsnm)
        {
            if (jsnm == 0)
            {
                throw new FailedException("缺少结算内码");
            }
            var jsentity = _hospSettlementRepo.IQueryable().FirstOrDefault(p => p.jsnm == jsnm && p.OrganizeId == OrganizeId && p.zt == "1" && p.jszt == "1");
            return Success("", jsentity.xjzf);
            //db.IQueryable<HospSettlementEntity>(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.jszt == "2").Select(p => p.cxjsnm);
        }
        #endregion

        #region 取消出院结算
        public ActionResult GetCQLastjsnm(string zyh)
        {
            var data = _dischargeSettleDmnService.GetCQLastJsnm(zyh, this.OrganizeId);
            return Content(data.ToString());
        }

        public ActionResult GetCQLastLsh(int jsnm)
        {
            var data = _dischargeSettleDmnService.GetCQLastLsh(jsnm, this.OrganizeId);
            return Success("", data);
        }
        public ActionResult GetCancelSettInfo(int jsnm)
        {
            var data = _dischargeSettleDmnService.GetCancelSettInfo(jsnm, this.OrganizeId);
            data.operatorId = this.UserIdentity.rygh;
            data.operatorName = this.UserIdentity.UserName;
            return Content(data.ToJson());
        }
        #endregion

        #region 医保未审批
        public ActionResult GetCYZD(string zyh)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("缺少病人信息");
            }

            var DiseaseInfo = _hospMultiDiagnosisRepo.SelectData(zyh, this.OrganizeId);
            var zdmc = DiseaseInfo.Select(p => p.zdmc + ",");
            return Success("", zdmc);
        }


        public ActionResult GetUnapproveddetails(string cfmxlshstr, string zyh)
        {
            var data = _dischargeSettleDmnService.sweep_expired_approvals(cfmxlshstr, this.OrganizeId, zyh);
            return Content(data.ToJson());
        }

        public ActionResult updatespbz(string cfhstr)
        {
            _dischargeSettleDmnService.updatespbz(cfhstr, OrganizeId, this.UserIdentity.UserCode);
            return Success();
        }

        public ActionResult GetCurrentpatientcharge(string zyh)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("缺少住院号");
            }
            IList<HospItemFeeDetailVO> sfxm = _hospFeeDmnService.GetItemFeeDetailVOList(zyh, this.OrganizeId).ToList();
            IList<HospItemFeeDetailVO> yp = _hospFeeDmnService.GetMedicinFeeDetailVOList(zyh, this.OrganizeId).ToList();
            return Content(sfxm.Union(yp).ToList().ToJson());
        }

        public ActionResult MedicalInsuranceApprovalIndex()
        {
            return View();
        }
        #endregion

        #region 医保预结算落地
        public ActionResult subybyjsld(CqybSett2303Entity YuJieSuan)
        {
            DateTime dateTime = DateTime.Now;
            YuJieSuan.prejs_id = Guid.NewGuid().ToString();
            YuJieSuan.czrq = dateTime;
            YuJieSuan.czydm = this.UserIdentity.UserCode;
            YuJieSuan.zt = 1;
            YuJieSuan.zt_czy = this.UserIdentity.UserCode;
            YuJieSuan.zt_rq = dateTime;
            _dischargeSettleDmnService.SaveSett2303(YuJieSuan);
            return Success();
        }

        #endregion

        public ActionResult QualityControlIndex()
        {
            return View();
        }

        public ActionResult CountLisIncompletezy(string zyh)
        {
            var num = _dischargeSettleDmnService.CountLisIncompletezy(OrganizeId, zyh);
            var data = new
            {
                num = num
            };
            return Content(data.ToJson());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class InpatientDischargeStatus
    {
        /// <summary>
        /// 住院号
        /// </summary>
        public string admsNum { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string dischargeStatus { get; set; }

    }
}

