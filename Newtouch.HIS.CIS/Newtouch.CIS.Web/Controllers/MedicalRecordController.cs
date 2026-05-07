using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Application.Interface;
using Newtouch.Common;
using Newtouch.Common.Model;
using Newtouch.Common.Web;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.BusinessObjects.API;
using Newtouch.Domain.DTO.InputDto.Outpatient;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.DTO.OutputDto.Outpatient.API;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.IRepository.Inpatient;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ViewModels.Outpatient;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.Log;
using Newtouch.Tools;
using Newtouch.Tools.Excel;
using newtouchyibao.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtouch.CIS.Proxy.CMMPlatform;
using Newtouch.CIS.Proxy.CMMPlatform.DTO.HLYYRequest;
using Newtouch.Infrastructure.Log;
using Newtouch.Domain.ViewModels;
using Newtouch.Domain.ViewModels.Outpatient;
using Newtouch.Domain.IRepository.Inpatient;
using Newtouch.Domain.IRepository;
using Newtouch.Core.Common.Utils;
using Newtouch.Domain.IRepository.Clinic;
using Newtouch.Domain.Entity.Clinic;
using Newtouch.Domain.IDomainServices.Clinic;
using Newtouch.Domain.ValueObjects.Clinic;

namespace Newtouch.CIS.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class MedicalRecordController : OrgControllerBase
    {
        private readonly IMedicalRecordDmnService _medicalRecordDmnService;
        private readonly ITreatmentRepo _treatmentRepo;
        private readonly IAuxiliaryDictionaryRepo _auxiliaryDictionaryRepo;
        private readonly IMRTemplateRepo _mRTemplateRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IIDBDmnService _idbDmnService;
        private readonly ITTCataloguesComparisonDmnService _tTCataloguesComparisonDmnService;
        private readonly ISysStaffRepo _sysStaffRepo;
        private readonly IReportDmnService _reportDmnService;
        private readonly IOutpatientCmmManagerApp _outpatientCmmManagerApp;
        private readonly IUsedDiagnosisRepo _usedDiagnosisRepo;
        private readonly IMedicalRecordRepo _medicalRecordRepo;
        private readonly IQhdZnshSqtxRepo _qhdznshsqtxRepo;
        private readonly IMzZyBarCodeRepo _mzzybarcodeRepo;
        private readonly IBlctglRepo _blctglRepo;
        private readonly IComDiagnosisRepo _ComDiagnosisRepo;
		private readonly IVisitDeptSetRepo visitDeptSetRepo;
        private readonly IReservationRepo _reservationRepo;
        private readonly IAdmissionNoticeRepo _admissionNoticeRepo;
        private readonly IClinicApplyRepo _clinicApplyRepo;
        private readonly IClinicDmnService _clinicDmnService;
        /// <summary>
        /// 病历
        /// </summary>
        /// <returns></returns>
        public ActionResult MedicalRecord()
        {
            
            return View();
        }

        public ActionResult ViewXindianList()
        {
            return View();
        }
        public ActionResult AdvanceRemind()
        {
            return View();
        }
        public ActionResult LisSqdhQueryForm()
        {
            return View();
        }
        public ActionResult PacsSqdhQueryForm()
        {
            return View();
        }

        public ActionResult GetlisPeportForm()
        {
            return View();
        }
        public ActionResult ReservationForm()
        {
            return View();
        }
        public ActionResult AdmissionNoticeForm()
        {
            return View();
        }
        public ActionResult BlContinuePrint()
        {
            return View();
        }
        /// <summary>
        /// 树结构 （历史病历：仅查询该病历号下的所有历史）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTreeList(string blh, int queryDate)
        {
            if (string.IsNullOrWhiteSpace(blh))
            {
                throw new FailedException("数据异常，缺少病历号");
            }

            var treeList = new List<TreeViewModel>();

            //历史病历
            treeList.Add(new TreeViewModel()
            {
                id = "1",
                value = "1",
                text = "历史病历",
                parentId = null,
                hasChildren = true,
                isexpand = true,
            });
            var historyList = _medicalRecordDmnService.GetHistoryMedicalRecordTree(blh, queryDate, OrganizeId);
            if (historyList.Count > 0)
            {
                foreach (var item in historyList)
                {
                    TreeViewModel mode = new TreeViewModel();
                    mode.id = item.jzId;   //就诊Id
                    mode.value = item.jzId;
                    mode.text = item.blmc;
                    mode.parentId = "1";
                    mode.hasChildren = false;
                    mode.isexpand = false;
                    mode.Ex1 = item.jzzt.ToString();
                    mode.Ex2 = "medicalRecord";
                    mode.Ex3 = item.lsblly.ToString();   //历史病历来源
                    mode.title = item.ghksmc + (!string.IsNullOrEmpty(item.jzysmc) ? (" " + item.jzysmc) : "");

                    treeList.Add(mode);
                }
            }

            //辅助词典
            treeList.Add(new TreeViewModel()
            {
                id = "2",
                value = "2",
                text = "辅助词典",
                parentId = null,
                hasChildren = true,
                isexpand = true,
            });
            var dicData = _auxiliaryDictionaryRepo.GetValidListByOrg(this.OrganizeId, true).ToList();
            foreach (SysAuxiliaryDictionaryEntity item in dicData)
            {
                TreeViewModel mode = new TreeViewModel();
                bool hasChildren = dicData.Count(t => t.ParentId == item.Id) == 0 ? false : true;
                mode.id = item.Id;
                mode.value = item.Code;
                mode.text = item.Name;
                mode.parentId = item.ParentId == null ? "2" : item.ParentId;
                mode.hasChildren = hasChildren;
                mode.isexpand = false;
                mode.complete = true;
                mode.Ex2 = "dictionary";

                treeList.Add(mode);
            }

            //病历模板
            treeList.Add(new TreeViewModel()
            {
                id = "3",
                value = "3",
                text = "病历模板",
                parentId = null,
                hasChildren = true,
                isexpand = true,
            });

            treeList.AddRange(GetMRTemplateTreeList((int)EnumCfMbLx.personal));
            treeList.AddRange(GetMRTemplateTreeList((int)EnumCfMbLx.department));
            treeList.AddRange(GetMRTemplateTreeList((int)EnumCfMbLx.hospital));

            return Content(treeList.TreeViewJson(null));
        }
        public ActionResult SelectNodeContentV2(string jzId, string Id, string pageType, string jzzt)
        {
            var data = _medicalRecordDmnService.SelectNodeContent(jzId);
            if (pageType == "yczl" && (jzzt == "1" || jzzt == null))
            {//远程诊疗  待就诊
                //获取云诊所患者病历
                var applyInfo = _clinicApplyRepo.GetYczl(Id, this.OrganizeId);
                var blDataObj = new
                {
                    ApplyId = applyInfo.sqlsh,
                    Sqlsh = "",
                };
                var blReqObj = new
                {
                    Data = blDataObj,
                    AppId = AuthenManageHelper.appId,
                    OrganizeId = OrganizeId,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var outstrbl = AuthenManageHelper.HttpPost<PatMedicalRecordVO>(blReqObj.ToJson(), AuthenManageHelper.SiteConmonAPIHost + "api/Clinic/GetClinicPatMedicalRecord", this.UserIdentity);
                var clinicBl = outstrbl.data;
                if (clinicBl != null)
                {
                    data.zs = data.zs == "" ? clinicBl.zs : data.zs;
                    data.fbsj = data.fbsj == null ? clinicBl.fbsj : data.fbsj;
                    data.xbs = data.xbs == "" ? clinicBl.xbs : data.xbs;
                    data.jws = data.jws == "" ? clinicBl.jws : data.jws;
                    data.ct = data.ct == "" ? clinicBl.ct : data.ct;
                    data.clfa = data.clfa == "" ? clinicBl.clfa : data.clfa;
                    data.fzjc = data.fzjc == "" ? clinicBl.fzjc : data.fzjc;
                    data.yjs = data.yjs == "" ? clinicBl.yjs : data.yjs;
                    data.gms = data.gms == "" ? clinicBl.gms : data.gms;
                }
            }
            return Content(data.ToJson());
        }
        /// <summary>
        /// 获取病历模板树
        /// </summary>
        /// <param name="mblx"></param>
        /// <returns></returns>
        private List<TreeViewModel> GetMRTemplateTreeList(int mblx)
        {
            var mblxmc = ((EnumCfMbLx)mblx).GetDescription();
            var treeList = new List<TreeViewModel>();

            treeList.Add(new TreeViewModel()
            {
                id = "mblx_" + mblx.ToString(),
                value = mblx.ToString(),
                text = mblxmc,
                parentId = "3",
                hasChildren = true,
                isexpand = false,
                complete = true,
            });

            var data = _mRTemplateRepo.IQueryable(a => a.mblx == mblx && a.OrganizeId == this.OrganizeId && a.zt == "1").Select(a => new { a.mbmc, a.mbId, a.mblx, a.ks, a.CreatorCode, a.CreateTime, a.LastModifyTime }).ToList();
            if (mblx == (int)EnumCfMbLx.department)
            {
                var curDepartmentCode = this.UserIdentity.DepartmentCode;
                data = data.Where(p => p.ks == curDepartmentCode).ToList();
            }
            else if (mblx == (int)EnumCfMbLx.personal)
            {
                var curUserCode = this.UserIdentity.UserCode;
                data = data.Where(p => p.CreatorCode == curUserCode).ToList();
            }
            data = data.OrderByDescending(a => a.CreateTime).OrderByDescending(a => a.LastModifyTime).ToList();
            foreach (var item in data)
            {
                treeList.Add(new TreeViewModel()
                {
                    id = item.mbId,  //模板Id
                    value = item.mbmc,
                    text = item.mbmc,
                    parentId = "mblx_" + item.mblx.ToString(),
                    hasChildren = false,
                    isexpand = false,
                    complete = true,
                    //Ex1 = ((int)EnumJzzt.Treating).ToString(),    //符合逻辑，也为了保持结构一致，前台共用填充的方法
                    Ex2 = "mrtemplate"
                });
            }
            return treeList;
        }

        /// <summary>
        /// 详情树节点   （固定）
        /// </summary>
        /// <returns></returns>
        public ActionResult GetDetailTreeNode()
        {
            var treeList = new List<TreeViewModel>();

            treeList.Add(new TreeViewModel()
            {
                id = "1",
                value = "1",
                text = "主诉",
                Code = "nc_zs",
                complete = true,
                showcheck = true,
            });

            treeList.Add(new TreeViewModel()
            {
                id = "2",
                value = "2",
                text = "现病史",
                Code = "nc_xbs",
                complete = true,
                showcheck = true,
            });

            treeList.Add(new TreeViewModel()
            {
                id = "3",
                value = "3",
                text = "既往史",
                Code = "nc_jws",
                complete = true,
                showcheck = true,
            });

            treeList.Add(new TreeViewModel()
            {
                id = "4",
                value = "4",
                text = "查体",
                Code = "nc_ct",
                complete = true,
                showcheck = true,
            });

            treeList.Add(new TreeViewModel()
            {
                id = "5",
                value = "5",
                text = "西医诊断",
                Code = "nc_xyzd",
                complete = true,
                showcheck = true,
            });

            treeList.Add(new TreeViewModel()
            {
                id = "6",
                value = "6",
                text = "中医诊断",
                Code = "nc_zyzd",
                complete = true,
                showcheck = true,
            });

            if (_sysConfigRepo.GetBoolValueByCode("openKfcf", this.OrganizeId) == true)
            {
                treeList.Add(new TreeViewModel()
                {
                    id = "7",
                    value = "7",
                    text = "康复处方",
                    Code = "nc_kfcf",
                    complete = true,
                    showcheck = true,
                });
            }

            if (_sysConfigRepo.GetBoolValueByCode("openCgxmcf", this.OrganizeId) == true)
            {
                treeList.Add(new TreeViewModel()
                {
                    id = "8",
                    value = "8",
                    text = "常规项目处方",
                    Code = "nc_cgxmcf",
                    complete = true,
                    showcheck = true,
                });
            }

            treeList.Add(new TreeViewModel()
            {
                id = "9",
                value = "9",
                text = "西药处方",
                Code = "nc_xycf",
                complete = true,
                showcheck = true,
            });

            treeList.Add(new TreeViewModel()
            {
                id = "10",
                value = "10",
                text = "中药处方",
                Code = "nc_zycf",
                complete = true,
                showcheck = true,
            });

            if (_sysConfigRepo.GetBoolValueByCode("openJyJcSwitch", this.OrganizeId) == true)
            {
                treeList.Add(new TreeViewModel()
                {
                    id = "11",
                    value = "11",
                    text = "检验处方",
                    Code = "nc_jycf",
                    complete = true,
                    showcheck = true,
                });

                treeList.Add(new TreeViewModel()
                {
                    id = "12",
                    value = "12",
                    text = "检查处方",
                    Code = "nc_jccf",
                    complete = true,
                    showcheck = true,
                });
            }
            treeList.Add(new TreeViewModel()
            {
                id = "13",
                value = "13",
                text = "发病日期",
                Code = "nc_fbsj",
                complete = true,
                showcheck = true,
            });
            treeList.Add(new TreeViewModel()
            {
                id = "14",
                value = "14",
                text = "处理",
                Code = "nc_clfa",
                complete = true,
                showcheck = true,
            });
            treeList.Add(new TreeViewModel()
            {
                id = "15",
                value = "15",
                text = "辅助检查",
                Code = "nc_fzjc",
                complete = true,
                showcheck = true,
            });
            return Content(treeList.TreeViewJson(null));
        }

        /// <summary>
        /// 详情树节点内容 （根据jzId查询病历和处方内容）
        /// </summary>
        /// <param name="jzId"></param>
        /// <returns></returns>
        public ActionResult SelectNodeContent(string jzId,string ex2)
        {
            if ("mrtemplate".Equals(ex2))
            {
                var data = _medicalRecordDmnService.SelectNodeContentByBlmb(jzId,this.OrganizeId);
                //病历模板
                return Content(data.ToJson());
            }
            else
            {
                var data = _medicalRecordDmnService.SelectNodeContent(jzId);
                return Content(data.ToJson());
            }
            
        }

        /// <summary>
        /// 保存就诊记录、诊断、病历、处方、处方明细
        /// </summary>
        /// <param name="jzObject"></param>
        /// <param name="xyzdList"></param>
        /// <param name="zyzdList"></param>
        /// <param name="blObject"></param>
        /// <param name="cfDto"></param>
        /// <param name="jzFinish"></param>
        /// <returns></returns>
        public ActionResult SaveMedicalRecord(TreatmentEntity jzObject, List<WMDiagnosisHtmlVO> xyzdList, List<TCMDiagnosisHtmlVO> zyzdList, MedicalRecordEntity blObject, string cfDto, bool jzFinish, List<CFZDDiagnosisHtmlVO> cfzdlist)
        {
            jzObject.jzks = this.UserIdentity.DepartmentCode;   //就诊科室 取操作人员当前科室
            jzObject.jzys = this.UserIdentity.rygh;    //就诊医生 取操作人员当前工号 //最后看诊医生工号
            jzObject.jzysmc = this.UserIdentity.UserName;   //最后看诊医生名称
            jzObject.zt = "1";
            jzObject.OrganizeId = this.OrganizeId;
            if (jzFinish)
            {
                //jzFinish:true 表示结束就诊
                jzObject.jzzt = (int)EnumJzzt.Treated;
                jzObject.zljssj = DateTime.Now;
            }
            else
            {
                jzObject.jzzt = (int)EnumJzzt.Treating;

                //更新排队叫号状态：4应答
                var reqObj = new
                {
                    mzh = jzObject.mzh,
                    calledstu = (int)EmunQueueCalledStu.reply,
                    yhcode = this.UserIdentity.rygh,
                    orgId = this.OrganizeId
                };
                var apiResp = SiteSettAPIHelper.Request<APIRequestHelper.DefaultResponse<SignInStateRequest>>(
                     "api/SignInAppointment/SignInState", reqObj);
                if (apiResp.msg != null || apiResp.sub_msg != null)
                {
                    LogCore.Error("签到状态修改接口调用失败【" + apiResp.msg + apiResp.sub_msg + "】");
                    throw new FailedException("签到状态修改接口调用失败【" + apiResp.msg + apiResp.sub_msg + "】");
                }
            }

            //页面上提交过来的处方明细列表    //重构处方数据
            var cfDtoList = cfDto.ToList<PrescriptionHtmlDTO>();
            var cfList = new List<PrescriptionDTO>();
            foreach (var item in cfDtoList)
            {
                foreach (var cfitem in item.cfHtml)
                {
                    cfitem.cflx = item.cflx == "kfcf" ? (int)EnumCflx.RehabPres : item.cflx=="wzhccf" ? (int)EnumCflx.Yyhc: item.cflx == "cgxmcf" ? (int)EnumCflx.RegularItemPres : item.cflx == "xycf" ? (int)EnumCflx.WMPres : item.cflx == "zycf" ? cfitem.cflx = (int)EnumCflx.TCMPres : item.cflx == "jycf" ? (int)EnumCflx.InspectionPres : item.cflx == "jccf" ? (int)EnumCflx.ExaminationPres : item.cflx == "dzcf"? (int)EnumCflx.Dzcf:0;
                    var ztsl2 = cfitem.ztsl != null ? cfitem.ztsl : cfitem.sl;
                    switch (cfitem.cflx)
                    {
                        case (int)EnumCflx.InspectionPres:
                            //检验 检查 sl 1
                            cfitem.sl = cfitem.sl == 0 ? 1 : cfitem.sl;  //sl 1
                            //cfitem.zl = 1;
                            //cfitem.mczll = 1;
                            cfitem.je = cfitem.dj* cfitem.sl;
                            break;
                        case (int)EnumCflx.ExaminationPres:
                            cfitem.je = cfitem.dj * cfitem.sl;
                            break;
                    }

                    if (cfitem.yfztbm != null && cfitem.cflx==1)
                    {
                        cfitem.ztId = cfitem.yfztbm;
                        cfitem.ztmc = cfitem.yfztmc;
                        cfitem.ztsl = ztsl2;
                    }
                    var matchCf = cfList.FirstOrDefault(a => a.cflx == cfitem.cflx && a.cfh == cfitem.cfh);
                    if (matchCf != null)
                    {
                        matchCf.zje += cfitem.je;    //引用类型  能够直接改变list中的值
                        var cfmx = new PrescriptionDetailVO();
                        cfitem.MapperTo(cfmx);

                        if (!string.IsNullOrWhiteSpace(cfitem.yfztbm) && !string.IsNullOrWhiteSpace(cfitem.yfztmc))
                        {
                            var results = _medicalRecordDmnService.Getsfmbxm(this.OrganizeId, cfitem.yfztbm, cfitem.yfztmc);
                            if (cfitem.cflx == (int)EnumCflx.RegularItemPres)
                            {
                                if (results.Count > 0)
                                {
                                    string guid = Guid.NewGuid().ToString();
                                    for (int i = 0; i < results.Count; i++)
                                    {
                                        results[i].px = i + 1;
                                        results[i].ztsl = ztsl2;
                                        results[i].sl = results[i].sl * cfitem.sl;
                                        results[i].syncfbz = guid;
                                        matchCf.cfmxList.Add(results[i]);
                                    }
                                }
                            }
                            else
                            {

                                var ztcf = matchCf.Clone();
                                ztcf.cfh = EFDBBaseFuncHelper.Instance.GetRequisitionNo(this.OrganizeId);
                                ztcf.cflx = (int)EnumCflx.RegularItemPres;
                                ztcf.cfmxList = new List<PrescriptionDetailVO>();
                                if (results.Count > 0) {
                                    string guid= Guid.NewGuid().ToString();
                                    for (int i = 0; i < results.Count; i++)
                                    {
                                        results[i].px = i + 1;
                                        results[i].ztsl = ztsl2;
                                        results[i].sl = results[i].sl * cfitem.sl;
                                        results[i].syncfbz = guid;
                                        ztcf.cfmxList.Add(results[i]);
                                    }
                                    ztcf.zje = ztcf.cfmxList.Sum(p => p.je);
                                    cfmx.syncfbz = guid;
                                    cfList.Add(ztcf);
                                }
                            }
                        }
                        else
                        {
                            matchCf.cfmxList.Add(cfmx);
                        }
                    }
                    else
                    {
                        var cf = new PrescriptionDTO
                        {
                            cfmxList = new List<PrescriptionDetailVO>(),
                            cfId = cfitem.cfId,
                            cfh = cfitem.cfh,
                            cflx = cfitem.cflx,
                            sfbz = cfitem.sfbz ?? false,
                            zje = cfitem.je,
                            ys = this.UserIdentity.rygh,//当前登录人的人员工号
                            ks = this.UserIdentity.DepartmentCode,//当前登录人的人员科室
                            tieshu = cfitem.tieshu,
                            cfyf = cfitem.cfyf,
                            djbz = cfitem.djbz,
                            lcyx = cfitem.lcyx,
                            sqbz = cfitem.sqbz,
                            cftag = cfitem.cftag,
                            djfs = cfitem.djfs,
                            djts = cfitem.djts,
                            cfzt = cfitem.cfzt,
							valiDays=3,
                            isdzcf = cfitem.isdzcf,
                            gjybdm = cfitem.gjybdm
                            //ztId = cfitem.yfztbm,
                            //ztmc=cfitem.yfztmc,

                        };
                        var cfmx = new PrescriptionDetailVO();
                        cfitem.MapperTo(cfmx);
                        cf.cfmxList.Add(cfmx);
                        if (string.IsNullOrWhiteSpace(cfitem.yfztbm) && !string.IsNullOrWhiteSpace(cfitem.cfh))
                        {
                            cfList.Add(cf);
                        }
                        else if ((cfitem.cflx == (int)EnumCflx.WMPres || cfitem.cflx == (int)EnumCflx.TCMPres))
                        {
                            cfList.Add(cf);
                        }
                        if (!string.IsNullOrWhiteSpace(cfitem.yfztbm) && !string.IsNullOrWhiteSpace(cfitem.yfztmc))
                        {
                            string guid = Guid.NewGuid().ToString();
                            var ztcf = cf.Clone();
                            if (cfitem.cflx != (int)EnumCflx.RegularItemPres)
                            {
                                ztcf.cfh = EFDBBaseFuncHelper.Instance.GetRequisitionNo(this.OrganizeId);
                            }
                            else
                            {
                                ztcf.cfh = cfitem.cfh;
                            }
                            ztcf.cfmxList = new List<PrescriptionDetailVO>();
                            var results = _medicalRecordDmnService.Getsfmbxm(this.OrganizeId, cfitem.yfztbm, cfitem.yfztmc);
                            if (results.Count > 0)
                            {
                                for (int i = 0; i < results.Count; i++)
                                {
                                    results[i].px = i + 1;
                                    results[i].sl = results[i].sl * cfitem.sl;
                                    results[i].ztsl = ztsl2;
                                    results[i].syncfbz = guid;
                                    ztcf.cfmxList.Add(results[i]);
                                }
                                ztcf.cflx= (int)EnumCflx.RegularItemPres;
                                ztcf.zje = ztcf.cfmxList.Sum(p=>p.je);
                                cfmx.syncfbz = guid;
                                cfList.Add(ztcf);
                            }
                        }
                        if (cfitem.djbz == true&&!string.IsNullOrWhiteSpace(cfitem.djts))
                        {
                            var data = _medicalRecordDmnService.GetBindTCMDj(this.OrganizeId);
                            if (data != null)
                            {
                                string guid = Guid.NewGuid().ToString();
                                var zydjcf = cf.Clone();
                                zydjcf.cfh = EFDBBaseFuncHelper.Instance.GetRequisitionNo(this.OrganizeId);
                                zydjcf.cflx = (int)EnumCflx.RegularItemPres;
                                zydjcf.cfmxList = new List<PrescriptionDetailVO>();
                                decimal dj = data.dj;
                                int sl = Convert.ToInt32(cfitem.djts);
                                var cfmx2 = new PrescriptionDetailVO
                                {
                                    xmCode = data.sfxmCode,
                                    xmmc = data.sfxmmc,
                                    ypCode = null,
                                    ypmc = null,
                                    mczll = 1,
                                    mcjl = null,
                                    mcjldw = null,
                                    yfCode = null,
                                    pcCode = null,
                                    zl = sl,
                                    sl = sl,
                                    dw = data.dw,
                                    dj = dj,
                                    je = (sl * dj),
                                    zh = null,
                                    bw = null,
                                    urgent = null,
                                    purpose = null,
                                    Remark = null,
                                    zxks = this.UserIdentity.DepartmentCode,
                                    zxsj = null,
                                    ybwym =null,
                                    xzsybz = null,
                                    ts = null,
                                    ztId = null,
                                    ztmc = null,
                                    kssReason = null,
                                    bwff = null,
                                    sqlx = null,
                                    px = 1,
                                    ds = null,
                                    zzfbz = null,
                                    syncfbz = guid
                                };
                                zydjcf.zje = cfmx2.je;
                                zydjcf.cfmxList.Add(cfmx2);
                                cfmx.syncfbz = guid;
                                
                                cfList.Add(zydjcf);
                            }
                        }
                    }
                }
            }

            //检查药品的领药药房 一张处方不能开多个药房的药品
            foreach (var cf in cfList.Where(p => p.sfbz == false && (p.cflx == (int)EnumCflx.WMPres || p.cflx == (int)EnumCflx.TCMPres)))
            {
                if (cf.cfmxList.Select(p => p.zxks!=null&&p.sfzt!=1).Distinct().Count() > 1)
                {
                    throw new FailedException("错误：处方" + cf.cfh + "不能开立多个药房的药品");
                }
                //西药处方的组号   
                //处方没改，因为系统机制， 组号也一直在变 不合理？
                if (cf.cflx != (int)EnumCflx.WMPres) continue;
                var zhList = cf.cfmxList.Select(p => p.zh).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
                if (zhList.Count <= 0) continue;
                var zhDict = new Dictionary<string, int>();
                foreach (var zh in zhList)
                {
                    //关联组号
                    var newZh = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueIntValue("xt_cfmx.zh", this.OrganizeId);
                    zhDict.Add(zh, newZh);
                }
                foreach (var cfmx in cf.cfmxList.Where(cfmx => !string.IsNullOrWhiteSpace(cfmx.zh)))
                {
                    cfmx.zh = zhDict[cfmx.zh].ToString();
                }
            }

            //康复处方录入错误
            if (cfList.Where(p => p.sfbz == false && (p.cflx == (int)EnumCflx.RehabPres)).Any(cf => cf.cfmxList.Any(p => p.zl == 0)))
            {
                throw new FailedException("错误：处方录入错误");
            }

            List<string> addedYpCfList;
            List<string> updatedYpCfList;
            _medicalRecordDmnService.SaveMedicalRecord(jzObject, xyzdList, zyzdList, blObject, cfList, cfzdlist, UserIdentity.UserCode, out addedYpCfList, out updatedYpCfList);
            try
            {
                var syncMethod = _sysConfigRepo.GetValueByCode("HISSyncMethod", this.OrganizeId);
                if (syncMethod == "IDB")
                {
                    _idbDmnService.SyncTo(this.OrganizeId, jzObject.mzh);//将mzh的未收费处方+就诊标志 等信息 同步至中间库
                }
                else
                {
                    PerformanceMonitoring.Running(() => _medicalRecordDmnService.sendPresToHis(jzObject.mzh, OrganizeId, UserIdentity.UserCode), "SaveMedicalRecord-sendPresToHis", "记录用接口同步至HIS耗时");//用接口同步至HIS
                }
            }
            catch (Exception ex)
            {
                LogCore.Error("SaveMedicalRecord error", ex);
                return Success("开立成功，但同步失败，请手动同步处方信息！", jzObject.jzId);
            }
            //返回jzId
            return Success(null, jzObject.jzId);
        }

        /// <summary>
        /// 作废病历 包括病历诊断 处方 处方明细
        /// </summary>
        /// <returns></returns>
        public ActionResult ObsoleteMedicalRecord(string jzId)
        {
            if (string.IsNullOrWhiteSpace(jzId))
            {
                throw new FailedException("数据异常，缺少就诊Id");
            }
            _medicalRecordDmnService.ObsoleteMedicalRecord(jzId, this.OrganizeId);

            var syncMethod = _sysConfigRepo.GetValueByCode("HISSyncMethod", this.OrganizeId);
            if (syncMethod == "IDB")
            {
                //中间库
                _idbDmnService.ObsoleteAllPresToHIS(jzId, this.OrganizeId);
            }
            else
            {
                _medicalRecordDmnService.ObsoleteAllPresToHIS(jzId, this.OrganizeId);
            }

            return Success();
        }

        /// <summary>
        /// 发送处方
        /// </summary>
        /// <returns></returns>
        public ActionResult sendSinglePresToHis(string mzh, string cfId)
        {
            var pushResult = true;
            var syncMethod = _sysConfigRepo.GetValueByCode("HISSyncMethod", this.OrganizeId);
            if (syncMethod == "IDB")
            {
                _idbDmnService.SyncTo(this.OrganizeId, mzh, cfId);
            }
            else
            {
                pushResult = _medicalRecordDmnService.sendPresToHis(mzh, OrganizeId, UserIdentity.UserCode, cfId);
            }
            return Success(null, pushResult);
        }

        /// <summary>
        /// 作废处方
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="cfId"></param>
        /// <returns></returns>
        public ActionResult cancelSinglePres(string mzh, string cfId)
        {
            int apicflx;   //1药品 2项目
            string cfh;
            bool isAlertinherit = false;
            _medicalRecordDmnService.cancelSinglePres(this.OrganizeId, mzh, cfId, out apicflx, out cfh, out isAlertinherit);

            var syncMethod = _sysConfigRepo.GetValueByCode("HISSyncMethod", this.OrganizeId);
            if (syncMethod == "IDB")
            {
                //中间库
                _idbDmnService.cancelSinglePresToHIS(this.OrganizeId, mzh, cfId, apicflx, cfh);
            }
            else
            {
                _medicalRecordDmnService.cancelSinglePresToHIS(this.OrganizeId, mzh, cfId, apicflx, cfh);
            }

            _medicalRecordDmnService.cancelSinglePresToPDS(this.OrganizeId, this.UserIdentity.UserCode, mzh, cfId, apicflx, cfh);

            return Success(isAlertinherit ? "作废成功，是否继承处方" : "");
        }


		public ActionResult delDzcf(string mzh, string cfId)
		{
			int apicflx;   //1药品 2项目
			string cfh;
			bool isAlertinherit = false;
			_medicalRecordDmnService.delDzcf(this.OrganizeId, mzh, cfId, out apicflx, out cfh, out isAlertinherit);
			return Success(isAlertinherit ? "作废成功，是否继承处方" : "");
		}
		/// <summary>
		/// 根据ghsj查询历史病历（API）
		/// </summary>
		/// <param name="blh"></param>
		/// <param name="ghsj"></param>
		/// <returns></returns>
		public ActionResult pullHistoryMRApi(string blh, string ghsj)
        {
            if (string.IsNullOrWhiteSpace(ghsj))
            {
                return null;
            }

            var reqObj = new
            {
                blh = blh
            };
            var apiResp = SiteSettAPIHelper.Request<APIRequestHelper.DefaultResponse<List<MedicalRecordInfoResponse>>>(
                "/api/Patient/PatientMedicalRecordQuery", reqObj);

            ghsj = ghsj.Substring(0, 8);
            var list = apiResp.data.ToList();

            return Content(list.ToJson());
        }

        public ActionResult GetJcDtxdmc(string mzh)
        {
            string XdHisCode = "";
            YCYL_DtxdscDTO sqdmc = _medicalRecordDmnService.GetJcDtxdmc(mzh, this.OrganizeId);
            if (sqdmc != null && !string.IsNullOrEmpty(sqdmc.CheckNumber))
            {
                XdHisCode = sqdmc.Sjscjgmc + sqdmc.CheckNumber + ".7z";
            }
            else
            {
                XdHisCode = "";
            }
            return Success("", XdHisCode);
        }

        public ActionResult GetMbqxSelectJson()
        {
            //SysStaffVEntity sysStaffEntity = _sysStaffRepo.GetValidStaffByGh(this.UserIdentity.rygh, this.OrganizeId);
            //int mbqx = Convert.ToInt32((sysStaffEntity.mbqx == null || sysStaffEntity.mbqx == "") ? "1" : sysStaffEntity.mbqx);
            //if (mbqx == 3)
            //{
            //    mbqx = 2;  //由于人员模板权限维护中3为病区，门诊3为全院，故此3降档为2科室
            //}
            int mbqx = (int)EnumCfMbLx.hospital;
            var data = new List<object>();
            foreach (EnumCfMbLx item in Enum.GetValues(typeof(EnumCfMbLx)))
            {
                if (mbqx < (int)item)
                {
                    continue;
                }
                var obj = new
                {
                    id = (int)item,
                    text = ((EnumCfMbLx)item).GetDescription()
                };
                data.Add(obj);
            }
            return Content(data.ToJson());
        }

        #region 获取事前提醒json
        public ActionResult GetPatJzJson(string mjzbz)
        {
            //获取就诊类型对应
            FirstSecondThird jzlxobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "jzlx", mjzbz, "sqtx");
            var obj = new
            {
                jzlx = jzlxobj.First
            };
            return Content(obj.ToJson());
        }
        /// <summary>
        /// 秦皇岛处方事前提醒
        /// </summary>
        /// <param name="strCfList"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult GetqhdCfSqtxData(List<WMDiagnosisHtmlVO> xyzdList, List<TCMDiagnosisHtmlVO> zyzdList, string strCfList,string mzh)
        {
            var jlId = "";
            var responsexml = _medicalRecordDmnService.GetqhdCfSqtxData(xyzdList, zyzdList, strCfList,mzh, out jlId, OrganizeId,this.UserIdentity.rygh,this.UserIdentity.UserName);
            var data = new
            {
                jlId = jlId,
                jydm = "5100",
                xmldata = responsexml
            };
            return Content(data.ToJson());
        }

        [ValidateInput(false)]
        public ActionResult Getqhdxmltojson(string xmldata)
        {
            List<OUTROW> rowlist = new List<OUTROW>();
            xmldata = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmldata;
            var resultobj = xmldata.XmlDeSerialize<QhdPrescriptoinVo>();
            if (resultobj!=null && resultobj.RESPONSEDATA.OUTPUT.Count>0)
            {
                rowlist = resultobj.RESPONSEDATA.OUTPUT;
                return Content(rowlist.ToJson());
            }
            return null;
        }

        /// <summary>
        /// 获取事前提醒json
        /// </summary>
        /// <param name="strCfList">事前提醒List</param>
        /// <param name="mzh">门诊号</param>
        /// <param name="czlx">操作类型 1保存2删除</param>
        /// <returns></returns>
        public ActionResult GetCfSqtxJson(string strCfList, string mzh, int czlx)
        {
            //获取当前患者就诊信息
            TreatmentEntity patjzInfo = _treatmentRepo.IQueryable(p => p.mzh == mzh && p.OrganizeId == this.OrganizeId && p.zt == "1").FirstOrDefault();
            var prescriptionSqtxDTO = new List<PrescriptionSqtxDTO>();

            //页面上提交过来的处方明细列表    //重构处方数据
            var cfDtoList = Tools.Json.ToList<PrescriptionHtmlDTO>(strCfList);
            //var cfList = new List<PrescriptionDTO>();
            foreach (var item in cfDtoList)
            {
                int zyOrder = 0; //一个处方循环重置标识 0为循环开始初始值
                string cfh = "";
                foreach (var cfitem in item.cfHtml)
                {
                    cfitem.cflx = item.cflx == "kfcf" ? (int)EnumCflx.RehabPres : item.cflx == "cgxmcf" ? (int)EnumCflx.RegularItemPres : item.cflx == "xycf" ? (int)EnumCflx.WMPres : item.cflx == "zycf" ? cfitem.cflx = (int)EnumCflx.TCMPres : item.cflx == "jycf" ? (int)EnumCflx.InspectionPres : item.cflx == "jccf" ? (int)EnumCflx.ExaminationPres : 0;

                    if (cfitem.cflx == (int)EnumCflx.InspectionPres || cfitem.cflx == (int)EnumCflx.ExaminationPres)
                    {
                        //检验 检查 sl 1
                        cfitem.sl = 1;  //sl 1
                        cfitem.je = cfitem.dj;
                    }

                    var matchCf = prescriptionSqtxDTO.Where(a => a.CFLX == cfitem.cflx && a.CFH == cfitem.cfh).FirstOrDefault();
                    if (matchCf != null)
                    {
                        if (cfitem.cflx == (int)EnumCflx.InspectionPres || cfitem.cflx == (int)EnumCflx.ExaminationPres || cfitem.cflx == (int)EnumCflx.RegularItemPres || cfitem.cflx == (int)EnumCflx.RehabPres)
                        {
                            var xmcfmx = getXmCfJson(cfitem, czlx);
                            matchCf.xmcfmxList.Add(xmcfmx);
                        }
                        if (cfitem.cflx == (int)EnumCflx.TCMPres)
                        {
                            if (zyOrder == 0)
                            {
                                cfh = cfitem.cfh;
                            }

                            if (cfh == cfitem.cfh)
                            {
                                zyOrder += 1;
                            }
                            else
                            {
                                zyOrder = 1;
                                cfh = cfitem.cfh;
                            }

                            var ypcfmx = getYpCfJson(cfitem, czlx, zyOrder);
                            matchCf.ypcfmxList.Add(ypcfmx);
                        }

                        if (cfitem.cflx == (int)EnumCflx.WMPres)
                        {
                            var ypcfmx = getYpCfJson(cfitem, czlx);
                            matchCf.ypcfmxList.Add(ypcfmx);
                        }
                    }
                    else
                    {
                        var cf = new PrescriptionSqtxDTO() { ypcfmxList = new List<SqtxCFYP>(), xmcfmxList = new List<SqtxCFXM>() };
                        //获取科室对应
                        FirstSecondThird ksobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "ks", this.UserIdentity.DepartmentCode, "sqtx");
                        //获取就诊类型对应
                        FirstSecondThird jzlxobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "jzlx", patjzInfo.mjzbz.ToString(), "sqtx");


                        cf.CFLX = cfitem.cflx;
                        cf.JZLX = jzlxobj.First;
                        cf.MZH = mzh;
                        cf.CFH = cfitem.cfh;
                        cf.YYKSBM = ksobj.First;   //当前登录人的人员科室
                        cf.YYKSMC = ksobj.Second;
                        cf.YYYSGH = this.UserIdentity.rygh;   //当前登录人的人员工号
                        cf.YSXM = this.UserIdentity.UserName;
                        if (cfitem.cflx == (int)EnumCflx.InspectionPres || cfitem.cflx == (int)EnumCflx.ExaminationPres || cfitem.cflx == (int)EnumCflx.RegularItemPres || cfitem.cflx == (int)EnumCflx.RehabPres)
                        {
                            var xmcfmx = getXmCfJson(cfitem, czlx);
                            cf.xmcfmxList.Add(xmcfmx);
                        }
                        if (cfitem.cflx == (int)EnumCflx.TCMPres || cfitem.cflx == (int)EnumCflx.WMPres)
                        {
                            if (zyOrder == 0)
                            {
                                cfh = cfitem.cfh;
                            }

                            if (cfh == cfitem.cfh)
                            {
                                zyOrder += 1;
                            }
                            else
                            {
                                zyOrder = 1;
                                cfh = cfitem.cfh;
                            }
                            var ypcfmx = getYpCfJson(cfitem, czlx, zyOrder);
                            cf.ypcfmxList.Add(ypcfmx);

                        }
                        if (cfitem.cflx == (int)EnumCflx.WMPres)
                        {
                            var ypcfmx = getYpCfJson(cfitem, czlx);
                            cf.ypcfmxList.Add(ypcfmx);
                        }
                        prescriptionSqtxDTO.Add(cf);
                    }


                }
            }

            var obj = new
            {
                PrescriptionSqtxDTO = prescriptionSqtxDTO
            };
            return Content(obj.ToJson());
        }

        private SqtxCFXM getXmCfJson(PrescriptionHtmlVO cfmxVo, int czlx)
        {
            FirstSecondThird jjdwobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "jjdw", cfmxVo.dw, "sqtx");
            SqtxXmYpInfoVO xmInfo = _medicalRecordDmnService.GetXmYpInfo(cfmxVo.xmCode, "1", this.OrganizeId);
            SqtxCFXM xmcf = new SqtxCFXM();
            xmcf.YZID = cfmxVo.ybwym;
            xmcf.CZLX = czlx;
            xmcf.FSSJ = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            xmcf.KSSYSJ = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            xmcf.YYXMDM = cfmxVo.xmCode;
            xmcf.YYXMMC = cfmxVo.xmmc;
            xmcf.YBXMDM = xmInfo.ybdm;
            xmcf.DJ = System.Decimal.Round(cfmxVo.dj, 4).ToString();
            xmcf.JJJG = "1";
            xmcf.JJDW = jjdwobj.First;
            xmcf.JJSL = ((cfmxVo.mczll == null ? 1 : cfmxVo.mczll) * cfmxVo.sl).ToString();
            xmcf.JCBWBM = ""; //
            xmcf.BZ_XY = (cfmxVo.xzsybz == null ? "0" : cfmxVo.xzsybz);
            xmcf.SYSM = "";
            xmcf.BZ_ZT = ""; //
            xmcf.BZ_ZF = xmInfo.zfbl.ToString();
            return xmcf;
        }
        private SqtxCFYP getYpCfJson(PrescriptionHtmlVO cfmxVo, int czlx, int zyOrder = 0)
        {
            //获取用药途径对应
            string cfyf = cfmxVo.yfCode == null ? cfmxVo.cfyf : cfmxVo.yfCode;
            FirstSecondThird yytjobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "yytj", cfyf, "sqtx");
            FirstSecondThird pcobj = new FirstSecondThird();
            if (!string.IsNullOrWhiteSpace(cfmxVo.pcCode))
            {
                pcobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "yzpc", cfmxVo.pcCode, "sqtx");
            }

            FirstSecondThird jjdwobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "jjdw", cfmxVo.dw, "sqtx");
            FirstSecondThird jldwobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "jjdw", cfmxVo.mcjldw, "sqtx");
            SqtxXmYpInfoVO ypInfo = _medicalRecordDmnService.GetXmYpInfo(cfmxVo.ypCode, "2", this.OrganizeId);
            SqtxCFYP ypcf = new SqtxCFYP();
            ypcf.YZID = cfmxVo.ybwym;
            ypcf.CZLX = czlx;
            ypcf.FSSJ = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ypcf.KSSYSJ = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ypcf.YYXMDM = cfmxVo.ypCode;
            ypcf.YYXMMC = cfmxVo.ypmc;
            ypcf.YBXMDM = ypInfo.ybdm; //
            ypcf.YYTJ = yytjobj.First;
            ypcf.FYYQ = "";
            ypcf.FYYQSM = "";
            ypcf.JL = cfmxVo.mcjl.ToString();
            ypcf.DW = jldwobj.First;
            ypcf.GYPC = pcobj.First;
            ypcf.GYMD = "3";
            ypcf.YYTS = (cfmxVo.ts == null ? "" : cfmxVo.ts.ToString());
            ypcf.DJ = System.Decimal.Round(cfmxVo.dj, 4).ToString();
            ypcf.JJDW = jjdwobj.First;
            ypcf.JJSL = cfmxVo.sl.ToString();
            if (ypInfo.jldw == cfmxVo.mcjldw)
            {
                ypcf.ZHB = (ypInfo.jl * ypInfo.mzcls).ToString();
            }
            else
            {
                ypcf.ZHB = "1";
            }
            ypcf.BZ_XY = ((cfmxVo.xzsybz == null || cfmxVo.xzsybz == "") ? "0" : cfmxVo.xzsybz);
            //ypcf.BZ_FF = ((cfmxVo.zh == null || cfmxVo.zh == "") ? "00" : cfmxVo.zh);
            if (cfmxVo.cflx == (int)EnumCflx.TCMPres)
            {
                if (zyOrder > 0 && zyOrder < 10)
                {
                    ypcf.BZ_FF = "0" + zyOrder.ToString();
                }
                else if (zyOrder >= 10)
                {
                    ypcf.BZ_FF = zyOrder.ToString();
                }
                else
                {
                    ypcf.BZ_FF = "01";
                }
            }
            else
            {
                ypcf.BZ_FF = ((cfmxVo.zh == null || cfmxVo.zh == "") ? "00" : cfmxVo.zh);
            }
            ypcf.BZ_MJ = "";
            ypcf.BZ_PS = "";
            ypcf.BZ_ZF = ypInfo.zfbl.ToString();

            return ypcf;
        }
        /// <summary>
        /// 获取作废处方JSON
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="cfid"></param>
        /// <returns></returns>
        public ActionResult GetzfcfJson(string mzh, string cfid)
        {
            //获取当前患者就诊信息
            TreatmentEntity patjzInfo = _treatmentRepo.IQueryable(p => p.mzh == mzh && p.OrganizeId == this.OrganizeId && p.zt == "1").FirstOrDefault();
            int czlx = 2;
            List<PrescriptionHtmlVO> cfListHtml = _medicalRecordDmnService.GetzfcfJson(cfid);
            PrescriptionSqtxDTO cfDto = new PrescriptionSqtxDTO() { xmcfmxList = new List<SqtxCFXM>(), ypcfmxList = new List<SqtxCFYP>() };
            foreach (var item in cfListHtml)
            {
                //获取科室对应
                FirstSecondThird ksobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "ks", this.UserIdentity.DepartmentCode, "sqtx");
                //获取就诊类型对应
                FirstSecondThird jzlxobj = _tTCataloguesComparisonDmnService.GetTTItem(this.OrganizeId, "jzlx", patjzInfo.mjzbz.ToString(), "sqtx");
                cfDto.CFLX = item.cflx;
                cfDto.JZLX = jzlxobj.First;
                cfDto.MZH = mzh;
                cfDto.CFH = item.cfh;
                cfDto.YYKSBM = ksobj.First;   //当前登录人的人员科室
                cfDto.YYKSMC = ksobj.Second;
                cfDto.YYYSGH = this.UserIdentity.rygh;   //当前登录人的人员工号
                cfDto.YSXM = this.UserIdentity.UserName;

                if (Convert.ToInt32(item.cflx) == (int)EnumCflx.InspectionPres || Convert.ToInt32(item.cflx) == (int)EnumCflx.ExaminationPres || Convert.ToInt32(item.cflx) == (int)EnumCflx.RegularItemPres || Convert.ToInt32(item.cflx) == (int)EnumCflx.RehabPres)
                {
                    var xmcfmx = getXmCfJson(item, czlx);
                    cfDto.xmcfmxList.Add(xmcfmx);
                }
                if (item.cflx == (int)EnumCflx.TCMPres || item.cflx == (int)EnumCflx.WMPres)
                {
                    var ypcfmx = getYpCfJson(item, czlx);
                    cfDto.ypcfmxList.Add(ypcfmx);
                }
            }
            var obj = new
            {
                PrescriptionSqtxDTO = cfDto
            };
            return Content(obj.ToJson());
        }
        #endregion


        #region

        public ActionResult LogQuery()
        {
            return View();
        }
        /// <summary>
        /// 门诊病人日志查询
        /// </summary>
        /// <returns></returns>
        public ActionResult OutpatientPatientQuery(Pagination pagination, string keyword, string kssj, string jssj, string jzysgh, string hznl, string xtxy, string qbdc)
        {
            var data = new
            {
                rows = _medicalRecordDmnService.GetLogGridList(pagination, keyword, kssj, jssj, jzysgh, this.OrganizeId, hznl, xtxy, qbdc),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.ToJson());
        }

        public ActionResult Export(string keyword, string kssj, string jssj, string jzysgh, string hznl, string xtxy, string qbdc)
        {
            var data = _medicalRecordDmnService.GetLogGridList(keyword, kssj, jssj, this.UserIdentity.rygh, this.OrganizeId, hznl, xtxy, qbdc);
            var path = DateTime.Now.ToString("\\\\门诊日志查询yyyyMMdd\\\\HHmmssfff") + ".xls";
            var filePath = CommmHelper.GetLocalFilePath("\\Excel导出\\门诊日志查询" + path);
            var cols = new List<ExcelColumn>()
            {
                   new ExcelColumn("jzsj","就诊日期"),
                    new ExcelColumn("mzh","门诊号"),
                    new ExcelColumn("xm","姓名"),
                    new ExcelColumn("jzxm","家长姓名"),
                    new ExcelColumn("dhhm","电话号码"),
                    new ExcelColumn("xb","性别"),
                     new ExcelColumn("nlshow","年龄"),
                     new ExcelColumn("zy","职业"),
                      new ExcelColumn("dz","地址"),
                    new ExcelColumn("fbsj","发病日期"),
                     new ExcelColumn("tiwen","体温"),
                      new ExcelColumn("xueya","血压"),
                       new ExcelColumn("cfz","初复诊"),
                        new ExcelColumn("zs","主要症状"),
                        new ExcelColumn("zdmc","诊断名称"),
                        new ExcelColumn("clfa","处理"),
                         new ExcelColumn("fzjc","辅助检查"),
                         new ExcelColumn("jzysmc","医生签字"),
                          new ExcelColumn("sfzh","有效证件号"),
                          new ExcelColumn("ContactNum","电话"),
                    new ExcelColumn("ghksmc","科别"),
                    new ExcelColumn("xuetang","血糖"),
                    new ExcelColumn("xbs","现病史"),
                    new ExcelColumn("bz","备注"),
            };
            var sheet = new ExcelSheet()
            {
                Title = "门诊日志列表",
                columns = cols
            };

            var result = data.ToExcel(filePath, sheet);
            if (result)
            {
                return File(filePath, "application/x-xls", path.Replace("\\", ""));
            }
            else
            {
                return Content("文件导出失败，请返回列表页重试");
            }
        }
        #endregion

        #region 远程调阅

        /// <summary>
        /// 院内心电报告调用
        /// </summary>
        /// <param name="kh"></param>
        /// <returns></returns>
        public ActionResult chakanmingxidiaoyong(string kh)
        {
            var data = _medicalRecordDmnService.chakanmingxidiaoyong(kh);
            return Success("", data.ToJson());
        }

        public ActionResult OutpatientTelemedicine()
        {
            return View();
        }

        public ActionResult TelxdReport()
        {
            return View();
        }
        public ActionResult BlctForm()
        {
            return View();
        }
        
        /// <summary>
        /// 远程心电申请
        /// </summary>
        /// <param name="kh"></param>
        /// <param name="sqId"></param>
        /// <returns></returns>
        public ActionResult xindianshenqing(string patid, string orderno)
        {
            var data = _medicalRecordDmnService.xindianshenqing(patid, orderno);
            return Success("", data.ToJson());
        }

        /// <summary>
        /// 检验申请
        /// </summary>
        /// <param name="kh"></param>
        /// <param name="sqId"></param>
        /// <returns></returns>
        public ActionResult jianyanshenqing(string patid, string jzlsh)
        {
            var data = _medicalRecordDmnService.jianyanshenqing(patid, jzlsh, this.OrganizeId);
            return Success("", data.ToJson());
        }
        /// <summary>
        /// 影像申请
        /// </summary>
        /// <param name="kh"></param>
        /// <param name="sqId"></param>
        /// <returns></returns>
        public ActionResult yingxiangshenqing(string patid, string num)
        {
            var data = _medicalRecordDmnService.yingxiangshenqing(patid, num);
            return Success("", data.ToJson());
        }

        public ActionResult xindiandiaoyue(string kh, string orderno)
        {
            var data = _medicalRecordDmnService.xindiandiaoyue(kh, orderno);
            return Success("", data.ToJson());
        }
        #endregion

        public ActionResult TelemedicineRequest()
        {
            return View();
        }

        public ActionResult submitTelRequest(string patid, string mzh, bool jy, bool yx, bool xd, bool dtxd)
        {
            if (string.IsNullOrWhiteSpace(patid) || string.IsNullOrWhiteSpace(mzh))
            {
                throw new FailedException("缺少病人唯一编码：patid或mzh为空！");
            }
            if (Request.Files.Count > 0)
            {

                string dataHash = string.Empty;
                string sqdh = string.Empty;
                if (dtxd)//如果是动态心电，则进行数据转化，调用ESB接口上传数据
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        byte[] data = new byte[Request.Files[i].ContentLength];
                        dataHash = string.Empty;
                        YCYL_DtxdscDTO dtxdscDto = new YCYL_DtxdscDTO();
                        dtxdscDto = _medicalRecordDmnService.GetJcDtxdmc(mzh, this.OrganizeId);
                        if (dtxdscDto == null)
                        {
                            throw new FailedException("缺少配置远程医疗文件：CIS.ycyl_sysConfig");
                        }
                        string fileName = Request.Files[i].FileName;
                        sqdh = fileName.Replace(dtxdscDto.Sjscjgmc, "").Replace(".7z", "");
                        using (Stream inputStream = Request.Files[i].InputStream)
                        {
                            //MemoryStream memoryStream = inputStream as MemoryStream;
                            //if (memoryStream == null)
                            //{
                            //    memoryStream = new MemoryStream();
                            //    inputStream.CopyTo(memoryStream);
                            //}

                            //StreamReader sreader = new StreamReader(memoryStream);
                            //string ssss = sreader.ReadToEnd();


                            inputStream.Read(data, 0, data.Length);
                            inputStream.Seek(0, SeekOrigin.Begin);
                            inputStream.Close();
                            //var strStream = Encoding.Default.GetString(data);
                            //strStream = Server.UrlDecode(strStream);

                            //var strStreamss = Server.UrlDecode(Encoding.UTF8.GetString(data)); 
                            //data = inputStream.ToArray();
                            dataHash = Request.Files[i].GetHashCode().ToString();
                            //string sssss = Convert.ToString(data);
                            //string sss = Convert.ToBase64String(data);
                        }
                        //dataHash = ComputeHash(data);

                        //dtxdscDto.FileType = "ECG";
                        dtxdscDto.FileNo = "1";
                        dtxdscDto.FileTotalNum = "1";
                        dtxdscDto.FileStreams = Convert.ToBase64String(data);
                        dtxdscDto.FileHash = dataHash;
                        dtxdscDto.FileDesc = Request.Files[i].FileName.Replace("&", "&amp;");

                        YCYL_DtxdSqdDTO dtxdsqd = _medicalRecordDmnService.GetJcDtxdSqd(mzh, this.OrganizeId, sqdh);

                        _medicalRecordDmnService.DTXD_SqdUploadData(dtxdsqd);

                        _medicalRecordDmnService.DTXD_UploadData(dtxdscDto);
                    }
                }

            }
            else
            {
                if (dtxd)
                {
                    throw new FailedException("动态远程医疗心电，必须上传该患者心电波形数据！");
                }
            }
            if (jy)
            {
                var cfhList = _medicalRecordDmnService.GetCfhBycflx(OrganizeId, (int)EnumCflx.InspectionPres, mzh);
                if (cfhList.Count > 0)
                {
                    string jzlsh = string.Join(",", cfhList.ToArray());
                    _medicalRecordDmnService.jianyanshenqing(mzh, jzlsh, this.OrganizeId);
                }
                else
                {
                    throw new FailedException("未找到可上传的检验申请单");
                }

            }

            //if (xd)
            //{
            //    var cfhList = _medicalRecordDmnService.GetCfhBysqlx(OrganizeId, "XDCode", mzh);
            //    string jzlsh = string.Join(",", cfhList.ToArray());
            //    _medicalRecordDmnService.xindianshenqing(patid, jzlsh);
            //}
            //if (yx)
            //{
            //    var cfhList = _medicalRecordDmnService.GetCfhBysqlx(OrganizeId, "PACSCode", mzh);
            //    string jzlsh = string.Join(",", cfhList.ToArray());
            //    _medicalRecordDmnService.yingxiangshenqing(patid, jzlsh);
            //}
            return Success("上传成功");
        }
        /// <summary>
        /// 计算哈希值字符串
        /// </summary>
        public static string ComputeHash(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 1)
                return "";

            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();

            foreach (var b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
        public ActionResult GetcfByJzId(string jzId)
        {
            var data = _medicalRecordDmnService.GetcfByJzId(OrganizeId, jzId);
            return Content(data.ToJson());
        }

        public ActionResult GetBarCodeBycfh(string cfh)
        {
            return Content(CommmHelper.GenerateBarCode(cfh, 25, 25));
        }

        public ActionResult SaveBarCodeByYzh(string yzlist)
        {
            var yzh = yzlist.Substring(1).Split(',');
            foreach (var item in yzh)
            {
                var entity = _mzzybarcodeRepo.IQueryable(p => p.yzh == item && p.OrganizeId == this.OrganizeId && p.zt == "1").FirstOrDefault();
                if (entity == null)
                {
                    string barcode = CommmHelper.GenerateBarCode(item, 25, 25);
                    var yzhentity = new MzZyBarCodeEntity
                    {
                        OrganizeId = this.OrganizeId,
                        yzh=item,
                        barcode=barcode,
                        zt = "1",
                    };
                    _mzzybarcodeRepo.SubmitForm(yzhentity);
                }

            }

            return Content("");
        }

        public ActionResult fastPrint(string jsonstr)
        {
            var item = Newtouch.Tools.Json.ToObject<Outpatientfastreportreq>(jsonstr);
            var j = "";
            switch (item.title)
            {
                case "打印中草药":
                    j = _reportDmnService.GetZCYCFjson(item.cfId, item.mzh, OrganizeId);
                    break;
                case "打印处方":
                case "打印常规处方":
                    j = _reportDmnService.GetCFDjson(item.cfId, item.mzh, OrganizeId);
                    break;
                case "打印检验单":
                    j = _reportDmnService.GetJYDjson(item.cfId, item.mzh, OrganizeId);
                    break;
                case "打印检查单":
                    j = _reportDmnService.GetJCDjson(item.cfId, item.mzh, OrganizeId);
                    break;
                case "打印雾化单":
                    j = _reportDmnService.GetWHDjson(item.cfId, item.mzh, OrganizeId);
                    break;
                case "打印注射单":
                    j = _reportDmnService.GetZSDjson(item.cfId, item.mzh, OrganizeId);
                    break;
            }
            return Success("", j);
        }

        public ActionResult GetLastzcf(string blh)
        {
            var data = _medicalRecordDmnService.GetLastzcf(blh, this.OrganizeId);
            return Success("", data.ToJson());
        }

        /// <summary>
        /// 获取常用诊断
        /// </summary>
        /// <returns></returns>
        public JsonResult Getcyzd(int type)
        {
            var kscode = UserIdentity.DepartmentCode;
            //var data = new List<object>();
            var udlist = _usedDiagnosisRepo.IQueryable().Where(p => p.ksCode == kscode && p.OrganizeId == OrganizeId && p.type == type).ToList();
            //if (udlist!=null&& udlist.Count()>0)
            //{
            //    foreach (var item in udlist)
            //    {
            //        var obj = new
            //        {
            //            id = item.zdCode,
            //            text = item.zdmc
            //        };
            //        data.Add(obj);
            //    }
            //}
            //return Content(data.ToJson());
            return Json(udlist, JsonRequestBehavior.AllowGet);
        }

        #region 修改病历相关信息
        public ActionResult UpdateMedicalRecord()
        {
            return View();
        }

        public ActionResult SaveUpdateMed(string jzid, string fbrq, string xuetang, string xuetangclfs, string tiwen, string shousuoya, string shuzhangya)
        {
            TreatmentEntity entity = _treatmentRepo
                .IQueryable(p => p.jzId == jzid && p.OrganizeId == this.OrganizeId && p.zt == "1")
                .FirstOrDefault();
            MedicalRecordEntity blEntity = _medicalRecordRepo
                .IQueryable(p => p.jzId == jzid && p.OrganizeId == this.OrganizeId && p.zt == "1")
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(fbrq))
            {
                blEntity.fbsj = Convert.ToDateTime(fbrq);
            }
            if (entity == null || blEntity == null)
            {
                return Error("获取患者就诊信息失败，请重新选择患者后重试");
            }
            decimal? decimalNull = null;
            entity.xuetang = (string.IsNullOrEmpty(xuetang) ? decimalNull : Convert.ToDecimal(xuetang));
            entity.xuetangclfs = xuetangclfs;
            entity.tiwen = string.IsNullOrEmpty(tiwen) ? decimalNull : Convert.ToDecimal(tiwen);
            entity.shousuoya = string.IsNullOrEmpty(shousuoya) ? decimalNull : Convert.ToDecimal(shousuoya);
            entity.shuzhangya = string.IsNullOrEmpty(shuzhangya) ? decimalNull : Convert.ToDecimal(shuzhangya);
            _treatmentRepo.Update(entity);
            _medicalRecordRepo.Update(blEntity);
            return Success();
        }
        #endregion

        public ActionResult GetLisSqdhGridJson(string mzzyh, string type, string ztmc, string kssj, string jssj)
        {
            var data = _medicalRecordDmnService.GetLisSqdhData(this.OrganizeId, mzzyh, type, ztmc, kssj, jssj);
            return Content(data.ToJson());
        }

        public ActionResult GetLisSqdyczGridJson(Pagination pagination, string mzzyh, string type, string ztmc,string ycbz,string sqdht)
        {
            var data = new
            {
                rows = _medicalRecordDmnService.GetLisSqdyczData(pagination, this.OrganizeId, mzzyh, type, ztmc, ycbz, sqdht),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            //var data = _medicalRecordDmnService.GetLisSqdyczData(pagination,this.OrganizeId, mzzyh, type, ztmc,ycbz,sqdht);
            return Content(data.ToJson());
        }
        public ActionResult GetPacsSqdhGridJson(string mzzyh, string type,string ztmc,string kssj,string jssj)
        {
            var data = _medicalRecordDmnService.GetPacsSqdhData(this.OrganizeId, mzzyh, type,ztmc,kssj,jssj);
            return Content(data.ToJson());
        }
        public ActionResult OpenExe(string sqdh)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "C:\\Users\\len\\Desktop\\金马扬名电子病历\\GoldHISManager.exe";//需要启动的程序名 
            p.StartInfo.Arguments = "G_HisApply.ReqNO='"+sqdh+"'" + " ";
            p.Start();//启动
            return Content("");
        }
        public ActionResult SyncToPDSRequest(string rq)
        {
            /**
             * 
             *     function synctoPDS() {
                        var rq = $("#syncPdsbc").val();
                        $.ajax({
                            type: "POST",
                            url: "/MedicalRecord/SyncToPDSRequest",
                            data: { rq: rq },

                        });
                    }
             * ***/
            _medicalRecordDmnService.SyncToPdsQuery(this.UserIdentity.rygh, rq);
            return Content("");
        }

        #region  病历词条

        private char[] trimstr = { ' ', '-', '\'', '\"', '\\', '\n', '\r' };

        public ActionResult GetctTreeList(int qx)
        {

            var treeList = new List<TreeViewModel>();
            var dataList = _blctglRepo.GetTreeList(OrganizeId, qx, this.UserIdentity.UserCode, "0");

            foreach (var data in dataList)
            {

                treeList.Add(new TreeViewModel()
                {
                    id = data.ID,
                    value = "parent",
                    text = data.mc,
                    parentId = null,
                    hasChildren = true,
                    isexpand = true,
                });
                var dataChildren = _blctglRepo.GetTreeList(OrganizeId, qx, this.UserIdentity.UserCode, data.ID);

                foreach (var child in dataChildren)
                {
                    if (!string.IsNullOrWhiteSpace(child.mc) && !string.IsNullOrWhiteSpace(child.ctnr))
                    {
                        treeList.Add(new TreeViewModel()
                        {
                            id = child.ID,
                            value = child.ctnr.Replace("\n", "").Trim(trimstr),
                            text = child.mc,
                            title = child.ctnr.Replace("\n", "").Trim(trimstr),
                            parentId = child.parentId,
                            hasChildren = false,
                            isexpand = false,
                        });
                    }

                }

            }
            var show = treeList.TreeViewJson(null);
            return Content(treeList.TreeViewJson(null));

        }

        /// <summary>
        /// 保存表单（新增、修改）
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="keyValue">主键值</param>
        /// <returns></returns>
        public ActionResult blctSubmitForm(BlctglEntity entity, string keyValue)
        {

            if (!string.IsNullOrEmpty(entity.ID))
            {
                entity.LastModifierCode = this.UserIdentity.UserCode;
            }
            else
            {
                if (Request["type"].ToString() == "mulu")
                {
                    entity.parentId = "0";
                    entity.qx = Convert.ToInt32(Request["mlqx"]);
                    entity.mc = Request["mlmc"].ToString().Trim(trimstr);
                }
                else
                {
                    string PartCTID = Request["parentId"].ToString();
                    entity.parentId = PartCTID;
                }
                entity.CreatorCode = this.UserIdentity.UserCode;
                entity.ksbm = this.UserIdentity.DepartmentCode;
                entity.OrganizeId = OrganizeId;
            }

            if (!string.IsNullOrWhiteSpace(entity.mc))
            {
                entity.mc = entity.mc.ToString().Trim(trimstr);
            }
            if (!string.IsNullOrWhiteSpace(entity.ctnr))
            {
                entity.ctnr = entity.ctnr.ToString().Trim(trimstr);
            }
            entity.qx = Convert.ToInt32(Request["mlqx"]);
            _blctglRepo.SubmitForm(entity, entity.ID);
            return Success("操作成功。");
        }
        public ActionResult DeleteForm(string delCTID)
        {
            _blctglRepo.DeleteForm(delCTID);
            return Success("操作成功。");
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns></returns>
        public ActionResult GetFormJson(string keyValue)
        {
            var entity = _blctglRepo.FindEntity(keyValue);
            string mlmc = "";
            if (entity != null)
            {
                mlmc = _blctglRepo.FindEntity(entity.parentId).mc;
            }
            var data = new
            {
                ct = entity,
                parentmc = mlmc
            };
            return Content(data.ToJson());
        }
        #endregion


        #region 常用诊断
        /// <summary>
        /// 诊断记录弹框
        /// </summary>
        /// <returns></returns>
        public ActionResult CommonDiag()
        {
            return View();
        }
        public ActionResult ToothBitmap()
        {
            return View();
        }

        /// <summary>
        /// 保存常用诊断
        /// </summary>
        /// <param name="zdmc"></param>
        /// <param name="zdbm"></param>
        /// <param name="type"></param>
        /// <param name="icd"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        public ActionResult SubmitDiag(string zdmc, string zdbm, string type, string icd, string py, string qxkz = null)
        {

            if (string.IsNullOrEmpty(py))
            {
                var Dpy = _ComDiagnosisRepo.Getzdpy(zdmc, zdbm);
                py = Dpy[0].py;
            }

            ComDiagnosisEntity entity = new ComDiagnosisEntity();
            entity.OrganizeId = this.OrganizeId;
            entity.cyzdmc = zdmc.Trim();
            entity.cyzdbm = zdbm.Trim();
            entity.icd10 = icd.Trim();
            entity.cyzdtype = type.Trim();
            entity.py = py.Trim();
            entity.ksCode = UserIdentity.DepartmentCode;
            entity.ys = this.UserIdentity.UserCode;
            if (qxkz == null || qxkz == "" || qxkz == "1")
            {
                entity.qxkz = "1";
            }
            else
            {
                entity.qxkz = "2";
            }
            entity.zt = "1";
            _ComDiagnosisRepo.SubmitDiag(entity);
            return Success("添加成功");
        }
        /// <summary>
        /// 获取常用诊断信息-管理界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetDiagsAdmin(string type, string keyword, string lx)
        {
            var entity = _ComDiagnosisRepo.GetComDiagnosisAdmin(this.UserIdentity.UserCode, this.UserIdentity.DepartmentCode, this.OrganizeId, type, keyword.Trim(), lx);

            return Content(entity.ToJson());
        }
        /// <summary>
        /// 获取常用诊断信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetDiags(string type, string keyword,string lx)
        {
            var entity = _ComDiagnosisRepo.GetComDiagnosis(this.UserIdentity.UserCode, this.UserIdentity.DepartmentCode, this.OrganizeId, type, keyword.Trim(), lx);
            
            return Content(entity.ToJson());
        }
        /// <summary>
        /// 获取诊断记录信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mzh"></param>
        /// <returns></returns>
        public ActionResult GetDiagsLists(string type, string mzh) {
            var data = _ComDiagnosisRepo.GetDiagsLists(this.OrganizeId,type,mzh);
            return Content(data.ToJson());
        }


        #endregion

        #region 历史病历
        public ActionResult HistoryMedicalRecordForm()
        {
            return View();
        }


        #endregion


		public ActionResult Getdjzlist(int mjzbz)
		{
			#region 获取出诊科室

			var kcCode = UserIdentity.DepartmentCode;
			var visitDept = visitDeptSetRepo.SelectData(this.UserIdentity.rygh, OrganizeId);
			if (visitDept != null && visitDept.Count > 0)
			{
				var tkcCode = new StringBuilder();
				foreach (var deptSetEntity in visitDept)
				{
					if (!string.IsNullOrWhiteSpace(deptSetEntity.visitksCode))
					{
						tkcCode.Append("," + deptSetEntity.visitksCode);
					}
				}

				if (tkcCode.Length > 0)
				{
					kcCode = tkcCode.Append(",").ToString();
					if (kcCode.IndexOf("," + UserIdentity.DepartmentCode + ',', StringComparison.Ordinal) < 0) kcCode += UserIdentity.DepartmentCode;
					kcCode = kcCode.Trim(',');
				}
			}
			#endregion
			Pagination pagination = new Pagination();
			pagination.page = 1;
			pagination.rows = 1000;
			pagination.sidx = "operatingTime";

            var cxsx = _sysConfigRepo.GetValueByCode("YXZ_CIS_1101", this.OrganizeId);
            var ysgh = "";
            if (cxsx == "1")
            {
                ysgh = this.UserIdentity.rygh;
            }
            else
            {
                ysgh = mjzbz == 3 ? this.UserIdentity.rygh : "";
            }
            var reqObj = new
			{
				ksCode = kcCode,
                ysgh = ysgh,
                mjzbz = mjzbz.ToString(),
                jiuzhenbz = ((int)EnumJzzt.NotYetTreate).ToString(),   //就诊标志
				pagination = pagination
			};

			var data = _medicalRecordDmnService.Getdjzlist(pagination, OrganizeId, reqObj.ksCode, reqObj.ysgh, reqObj.mjzbz, reqObj.jiuzhenbz);


			var RespList = new List<TreatEntityObj>();
			foreach (var item in data)
			{
                var ghrq = Convert.ToDateTime(item.ghsj).Date;
				var newghrq = ghrq.AddDays(1);
				if (ghrq.AddDays(1) < DateTime.Now)
				{
					continue;
				}
				RespList.Add(new TreatEntityObj
				{
					mzh = item.mzh,
					mjzbz = Convert.ToInt32(item.mjzbz),//1普通门诊 2急诊 3专家门诊
					blh = item.blh,
					xm = item.brxm,
					xb = item.sexValue,
					csny = item.birth.ToDate(),
					brxzmc = item.brxzmc,
					brxzCode = item.brxz,
					zjlx = item.zjlx.ToInt(),
					zjh = item.zjh,
					ghksmc = item.ksmc,
					ghys = item.ysxm,//挂号医生名称（专家号时肯定有）
					ghsj = item.ghsj.ToDate(),
					ghczsj = item.operatingTime.ToDate(),
					jzzt = (int)EnumJzzt.NotYetTreate,
					jzks = item.ks,
					jzys = item.ys,
					ybjsh = item.ybjsh,
					cfzbz = item.fzbz,
					sbbh = item.sbbh,
					cbdbm = item.cbdbm,
					py = item.py,
					kh = item.kh,
					ContactNum = item.contactNum,
					province = item.province,
					city = item.city,
					county = item.county,
					address = item.address,
					nlshow = item.nlshow,
					ghlybz = item.ghlybz,
					grbh = item.grbh,
                    queno = item.queno
                });
			}
			return Content(RespList.ToJson());
		}

		public ActionResult dzcfcxForm()
		{
			return View();
		}

        public ActionResult BaseMedicineQuery() 
        {
            return View();
        }


        public ActionResult GetBaseMedicinetList(Pagination pagination , string zxdlb, string xmbm, string xmmc, string ck_kc) 
        {
            var reqcwNewObj = new  
            {
                Timestamp=DateTime.Now.ToString(),
                pagination = pagination, 
                orgId = this.OrganizeId,
                zxdlb = zxdlb,
                xmbm = xmbm,
                xmmc = xmmc,
                ck_kc = ck_kc,
            };
            if (!string.IsNullOrWhiteSpace(zxdlb))
            {
                if (zxdlb == "ynyp")
                {
                    var response = SiteBaseAPIHelper.Request<APIRequestHelper.DefaultResponse<MedicineQueryResponse>>("/api/MedicineStorageIOMode/GETYNMidecine", reqcwNewObj);
                    if(response!=null && response.data!=null)
                    {
                        var data = new
                        {
                            rows = response.data.list,
                            total = response.data.pagination.total,
                            page = response.data.pagination.page,
                            records = response.data.pagination.records
                        };
                        return Content(data.ToJson());
                    }
                    return Content(response.ToJson());
                    
                }
                else if (zxdlb == "zlxm")
                { 
                    var response = SiteBaseAPIHelper.Request<APIRequestHelper.DefaultResponse<MedicineQueryResponse>>("/api/MedicineStorageIOMode/GETYNzlxm", reqcwNewObj);
                    if (response != null && response.data != null)
                    {
                        var data = new
                        {
                            rows = response.data.list,
                            total = response.data.pagination.total,
                            page = response.data.pagination.page,
                            records = response.data.pagination.records
                        }; 
                        return Content(data.ToJson());
                    }
                    return Content(response.ToJson());
                } else if (zxdlb== "ynhc") 
                {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                    var data = new
                    {
                        rows = _medicalRecordDmnService.GetMedicineInfoList(pagination,xmbm,xmmc,ck_kc, this.OrganizeId),
                        total = pagination.total,
                        page = pagination.page,
                        records = pagination.records
                    };
                    return Content(data.ToJson());
                }
            }
            return Content(new
            {
                rows =new List<MedicineInfoVO2>(),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            }.ToJson());
        }
        public ActionResult SaveReservationData(string yyrq, string yysj, string yyks, string mzh, string yylxfs)
        {
            ReservationEntity entity = _reservationRepo
                .IQueryable(p => p.mzh == mzh && p.OrganizeId == this.OrganizeId && p.zt == "1")
                .FirstOrDefault();
            if (entity == null)
            {
                var yyentity = new ReservationEntity
                {
                    OrganizeId = this.OrganizeId,
                    mzh = mzh,
                    yyrq = yyrq,
                    yysj = yysj,
                    yyks = yyks,
                    yyys = this.UserIdentity.UserCode,
                    yylxfs = yylxfs,
                    zt = "1",
                };
                _reservationRepo.SubmitForm(yyentity);
            }
            else
            {
                entity.yyrq = yyrq;
                entity.yysj = yysj;
                entity.yylxfs = yylxfs;
                _reservationRepo.Update(entity);
            }
            return Content("");
        }
        public ActionResult GetReservationData(string mzh)
        {
            ReservationEntity entity = _reservationRepo
                .IQueryable(p => p.mzh == mzh && p.OrganizeId == this.OrganizeId && p.zt == "1")
                .FirstOrDefault();
            return Content(entity.ToJson());
        }
        public ActionResult DelReservationData(string mzh)
        {
            ReservationEntity entity = _reservationRepo
                .IQueryable(p => p.mzh == mzh && p.OrganizeId == this.OrganizeId && p.zt == "1")
                .FirstOrDefault();
            if (entity != null)
            {
                _reservationRepo.Delete(entity);
            }
            return Content("");
        }
        public ActionResult SaveAdmissionNoticeData(AdmissionNoticeEntity postData)
        {
            AdmissionNoticeEntity entity = _admissionNoticeRepo
               .IQueryable(p => p.mzh == postData.mzh && p.OrganizeId == this.OrganizeId && p.zt == "1")
               .FirstOrDefault();
            if (entity == null)
            {
                postData.OrganizeId = this.OrganizeId;
                _admissionNoticeRepo.SubmitForm(postData);
            }
            else
            {
                entity.xm = postData.xm;
                entity.xb = postData.xb;
                entity.nl = postData.nl;
                entity.rq = postData.rq;
                entity.mzh = postData.mzh;
                entity.cbzd = postData.cbzd;
                entity.ks = postData.ks;
                entity.cw = postData.cw;
                entity.jtzz = postData.jtzz;
                entity.gzdw = postData.gzdw;
                entity.yjj = postData.yjj;
                entity.qzys = postData.qzys;
                entity.LastModifierCode = this.UserIdentity.UserCode;
                entity.LastModifyTime = DateTime.Now;
                entity.qzys = postData.qzys;
                _admissionNoticeRepo.Update(entity);
            }
            return Content("");
        }
        public ActionResult GetAdmissionNoticeData(string mzh)
        {
            AdmissionNoticeEntity entity = _admissionNoticeRepo
                .IQueryable(p => p.mzh == mzh && p.OrganizeId == this.OrganizeId && p.zt == "1")
                .FirstOrDefault();
            return Content(entity.ToJson());
        }

        public ActionResult GetyczlPat(string Id, string jzzt)
        {
            var patInfo = _clinicDmnService.getClinicPatInfo(this.OrganizeId, Id);
            //待就诊,调诊所获取病历接口
            //if (jzzt=="1") {//待就诊
            //获取云诊所患者病历
            var applyInfo = _clinicApplyRepo.GetYczl(Id, this.OrganizeId);
            var blDataObj = new
            {
                ApplyId = applyInfo.sqlsh,
                Sqlsh = "",
            };
            var blReqObj = new
            {
                Data = blDataObj,
                AppId = AuthenManageHelper.appId,
                OrganizeId = OrganizeId,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            var outstrbl = AuthenManageHelper.HttpPost<PatMedicalRecordVO>(blReqObj.ToJson(), AuthenManageHelper.SiteConmonAPIHost + "api/Clinic/GetClinicPatMedicalRecord", this.UserIdentity);
            var clinicBl = outstrbl.data;
            if (clinicBl != null)
            {
                patInfo.blh = patInfo.blh == null ? clinicBl.blh : patInfo.blh;
                patInfo.xm = patInfo.xm == null ? clinicBl.xm : patInfo.xm;
                patInfo.patid = patInfo.patid == null ? clinicBl.patid : patInfo.patid;
                patInfo.mzh = patInfo.mzh == null ? clinicBl.mzh : patInfo.mzh;
            }
            //}
            return Content(patInfo.ToJson());

            //#region 获取出诊科室

            //var kcCode = "00000080";
            ////var visitDept = visitDeptSetRepo.SelectData(this.UserIdentity.rygh, OrganizeId);
            ////if (visitDept != null && visitDept.Count > 0)
            ////{
            ////    var tkcCode = new StringBuilder();
            ////    foreach (var deptSetEntity in visitDept)
            ////    {
            ////        if (!string.IsNullOrWhiteSpace(deptSetEntity.visitksCode))
            ////        {
            ////            tkcCode.Append("," + deptSetEntity.visitksCode);
            ////        }
            ////    }

            ////    if (tkcCode.Length > 0)
            ////    {
            ////        kcCode = tkcCode.Append(",").ToString();
            ////        if (kcCode.IndexOf("," + UserIdentity.DepartmentCode + ',', StringComparison.Ordinal) < 0) kcCode += UserIdentity.DepartmentCode;
            ////        kcCode = kcCode.Trim(',');
            ////    }
            ////}
            //#endregion
            //Pagination pagination = new Pagination();
            //pagination.page = 1;
            //pagination.rows = 1000;
            //pagination.sidx = "blh";
            //var ysgh = this.UserIdentity.rygh;
            ////var cxsx = _sysConfigRepo.GetValueByCode("YXZ_CIS_1101", this.OrganizeId);
            ////var ysgh = "";
            ////if (cxsx == "1")
            ////{
            ////    ysgh = this.UserIdentity.rygh;
            ////}
            ////else
            ////{
            ////    ysgh = mjzbz == 3 ? this.UserIdentity.rygh : "";
            ////}
            //var reqObj = new
            //{
            //    ksCode = kcCode,
            //    ysgh = ysgh,
            //    mjzbz = "1",
            //    jiuzhenbz = ((int)EnumJzzt.NotYetTreate).ToString(),   //就诊标志
            //    pagination = pagination
            //};

            //var data = _medicalRecordDmnService.Getdjzlist(pagination, OrganizeId, reqObj.ksCode, reqObj.ysgh, reqObj.mjzbz, reqObj.jiuzhenbz, "");

            ////var showUnTreateDays = _sysConfigRepo.GetValueByCode("ShowUnTreateDays", this.OrganizeId);
            ////var unTreateDays = 3;
            ////if (!string.IsNullOrEmpty(showUnTreateDays))
            ////{
            ////    unTreateDays = Convert.ToInt32(showUnTreateDays);
            ////}
            //var RespList = new List<TreatEntityObj>();
            //foreach (var item in data)
            //{
            //    //var ghrq = Convert.ToDateTime(item.ghsj);
            //    //if (ghrq.Date.AddDays(unTreateDays) < DateTime.Now)
            //    //{
            //    //	continue;
            //    //}
            //    RespList.Add(new TreatEntityObj
            //    {
            //        mzh = item.mzh,
            //        mjzbz = Convert.ToInt32(item.mjzbz),//1普通门诊 2急诊 3专家门诊
            //        blh = item.blh,
            //        xm = item.brxm,
            //        xb = item.sexValue,
            //        csny = item.birth.ToDate(),
            //        brxzmc = item.brxzmc,
            //        brxzCode = item.brxz,
            //        zjlx = item.zjlx.ToInt(),
            //        zjh = item.zjh,
            //        ghksmc = item.ksmc,
            //        ghys = item.ysxm,//挂号医生名称（专家号时肯定有）
            //        ghsj = item.ghsj.ToDate(),
            //        ghczsj = item.operatingTime.ToDate(),
            //        jzzt = (int)EnumJzzt.NotYetTreate,
            //        jzks = item.ks,
            //        jzys = item.ys,
            //        ybjsh = item.ybjsh,
            //        cfzbz = item.fzbz,
            //        sbbh = item.sbbh,
            //        cbdbm = item.cbdbm,
            //        py = item.py,
            //        kh = item.kh,
            //        ContactNum = item.contactNum,
            //        province = item.province,
            //        city = item.city,
            //        county = item.county,
            //        address = item.address,
            //        nlshow = item.nlshow,
            //        ghlybz = item.ghlybz,
            //        grbh = item.grbh,
            //        queno = item.queno
            //    });
            //}
            //return Content(RespList.ToJson());
        }


        public ActionResult CountLISztmz(string jzId)
        {
            var num = _medicalRecordDmnService.CountLISztmz(OrganizeId, jzId);
            var data = new
            {
                num = num
            };
            return Content(data.ToJson());
        }


        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetMzHlyy(string jzId,string cfId)
        {
            //获取当前用户病历信息
            var data = _medicalRecordDmnService.SelectNodeContent(jzId);
            var orgId = XMLSerializer.GenerateShortUUIDFromString(UserIdentity.OrganizeId);
            //构造合理用药参数
            var request = new EngineReq
            {
                Type = "prescription",
                Patient = new PatientRecord
                {  
                    DepartID =  data.jzks, 
                    Department = data.ghksmc,
                    BedNo = "",
                    PresType = "处方",
                    PresSource = data.mjzbz == 1 ? "门诊" : data.mjzbz == 2 ? "急诊" : "门诊",
                    PresDatetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    PayType = "",
                    PatientNo = data.mzh,
                    PresNo = data.cfBoList.Count>0? data.cfBoList[0].presEntity.cfId:null,
                    Name = data.xm,
                    DiagnoseID = "",
                    //默认西医
                    Diagnose =string.Join("|", data.xyzdList.Select(item => item.zdmc)),
                    IDCard = "",
                    Address = "",
                    PhoneNo = "",
                    Age =data.nlshow+"0月" ,
                    Sex = data.xb,
                    Height = Regex.Match(data.ct == null ? "身高：170cm" : data.ct, @"身高：(\d+cm)").Groups[1].Value,
                    Weight = Regex.Match(data.ct == null ? "体重：60kg" : data.ct, @"体重：(\d+kg)").Groups[1].Value,
                    BirthWeight = "",
                    PreviousHistory = "",
                    NowMedicalHistory = "",
                    Ccr = "",
                    Anaphylactogen = data.gms == null? "":data.gms,
                    AllergicHistory = "",
                    Pregnancy = "",
                    TimeOfPreg = "",
                    Disease = "2",
                    BreastFeeding = "",
                    Dialysis = "",
                    ProxName = "",
                    ProxIDCard = "",
                    DocID =data.jzys,
                    DocName = data.jzysmc,
                    TotalAmount = "",
                },
                Operation = new Operation
                {
                    OperationCode = "",
                    OperationName = "",
                    OperationStartTime = "" ,
                    OperationEndTime = "",
                    IncisionType = "",
                    IncisionStatus = "",
                    Inplant = "false"
                },
            };
            var prescriptions = new List<Prescription>();
            var dataList = new List<EngineRes>();
            data.cfBoList.ForEach(item =>
            {
                if (item.presEntity.cflx != 1 )
                {
                    return;
                }
               
                item.presDetailList.ForEach(val =>
                {
                    var ypjx = _medicalRecordDmnService.GetYpjx(val.ypCode, UserIdentity.OrganizeId);
                    var pres = new Prescription
                    {
                        Drug = val.ypCode+'_'+orgId,
                        DrugName = val.ypmc,
                        RegName = val.ypmc,
                        Specification = val.mcjl+val.mcjldw,
                        Package = val.ypgg,
                        Quantity = val.sl,
                        PackUnit = val.dw,
                        UnitPrice = "",
                        Amount = "",
                        GroupNo = val.zh,
                        FirstUse = "",
                        PrepForm = ypjx == null?"":ypjx,
                        AdminRoute = val.yfmc,
                        AdminArea = "无",
                        AdminFrequency = val.pcmc,
                        AdminDose = val.mcjl+val.mcjldw,
                        AdminMethod = "",
                        Type = "",
                        AdminGoal = "",
                        DocID = "",
                        DocName = data.jzysmc,
                        DocTitle = "",
                        DepartID = "",
                        Department = data.jzks,
                        NurseName = "",
                        StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = DateTime.Now.AddDays(val.ts.ToDouble()).ToString("yyyy-MM-dd HH:mm:ss"),
                        SpecialPromote = "",
                        ContinueDays = val.ts.ToString()
                    
                    };
                    prescriptions.Add(pres);
                    
                });
                request.Prescriptions = prescriptions;
                var hlyyProxy = new HlyyProxy();
                var engineRes = hlyyProxy.engine(request);
                prescriptions.Clear();
                engineRes.cfh = item.presEntity.cfh;
                dataList.Add(engineRes);
            });
            
            return Success("查询成功", dataList);
        }
        
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult GetypSms(string drugId)
        {
            var hlyyProxy = new HlyyProxy();
            var hisSmsJson = hlyyProxy.GetHisSmsJson(drugId);
            
            return Success("查询成功", hisSmsJson);;
        }
        
        
    }
    
    
    
}