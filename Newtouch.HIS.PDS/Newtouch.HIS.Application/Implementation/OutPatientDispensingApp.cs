using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FrameworkBase.MultiOrg.Application;
using FrameworkBase.MultiOrg.Domain.DBContext.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.DomainServices;
using Newtouch.HIS.Repository;
using Newtouch.HIS.Sett.Request.OutPatientPharmacy;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.Log;
using Newtouch.Tools;
using static Newtouch.Common.Web.APIRequestHelper;
using Exception = System.Exception;

namespace Newtouch.HIS.Application.Implementation
{
    /// <summary>
    /// 门诊发药
    /// </summary>
    public class OutPatientDispensingApp : AppBase, IOutPatientDispensingApp
    {
        private readonly IfyDmnService _fydeDmnService;

        /// <summary>
        /// 执行发药
        /// </summary>
        /// <param name="deliveryInfo"></param>
        /// <param name="cfhList">发药处方号</param>
        /// <returns></returns>
        public string ExecAllDeliveryDrugV2(OutpatienDrugDeliveryInfo deliveryInfo, out List<string> cfhList)
        {
            cfhList = new List<string>();
            if (deliveryInfo == null || deliveryInfo.PatientInfo == null || deliveryInfo.PatientInfo.Count == 0) return "发药患者信息不能为空。";

            var errorMsg = new StringBuilder();
            try
            {
                var successLs = new List<object>();
                var tmpCfhList = new List<string>();
                foreach (var p in deliveryInfo.PatientInfo)
                {
                    //var cfxx = _fydeDmnService.GetRpInfo(deliveryInfo.yfbmCode, p.CardNo, p.xm, organizeId: deliveryInfo.organizeId);
                    //if (cfxx == null || cfxx.Count == 0) continue;
                    //foreach (var cf in cfxx)
                    {
                        var request = new
                        {
                            cfh = p.cfh,
                            lyyf = deliveryInfo.yfbmCode,
                            organizeId = deliveryInfo.organizeId,
                            TimeStamp = DateTime.Now
                        };
                        var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/DeliveryRpDetailQuery", request);
                        if (apiResp == null || apiResp.data == null)
                        {
                            errorMsg.Append(string.Format("处方【{0}】门诊结算接口异常，请联系管理员；", p.cfh));
                            break;
                        }

                        var rpd = apiResp.data.ToJson().ToObject<List<fyCfYpInfo>>();
                        if (rpd == null || rpd.Count == 0)
                        {
                            errorMsg.Append(string.Format(@"处方【{0}】非已结算状态，无法发药；", p.cfh));
                            break;
                        }

                        var deliveryResult = _fydeDmnService.ExecOutpatientDispensingDrugV2(p.cfh, deliveryInfo.yfbmCode, deliveryInfo.userCode, deliveryInfo.organizeId,p.zsm);
                        if (!string.IsNullOrWhiteSpace(deliveryResult))
                        {
                            errorMsg.Append(deliveryResult + "；");
                            break;
                        }

                        successLs.Add(new
                        {
                            cfnm = p.cfnm,
                            user_code = deliveryInfo.userCode,
                            TimeStamp = DateTime.Now
                        });
                        tmpCfhList.Add(p.cfh);
                    }
                }

                if (successLs.Count <= 0) return errorMsg.ToString();
                foreach (var item in successLs)
                {
                    var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("api/OutpatientPharmacy/UpdatefyztByFY", item);
                    LogCore.Info("UpdatefyztByFY", string.Format("门诊发药同步发药状态; \n request:{0} \n response:{1} ; ", item.ToJson(), apiResp.ToJson()));
                }

                if (tmpCfhList.Count > 0) cfhList = tmpCfhList;
                return errorMsg.ToString();
            }
            catch (FailedException e)
            {
                LogCore.Error("ExecAllDeliveryDrugV2 error", e);
                return e.Msg;
            }
        }

        /// <summary>
        /// 执行发药
        /// </summary>
        /// <param name="patients"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <param name="cfhList">发药处方号</param>
        /// <returns></returns>
        public string ExecAllDeliveryDrug(patientInfoVO[] patients, string yfbmCode, string userCode, string organizeId, out List<string> cfhList)
        {
            cfhList = new List<string>();
            if (patients == null || patients.Length <= 0)
            {
                return "请选择需要排药的患者！";
            }

            var errorMsg = "";
            var locker = new object();
            try
            {

                var tmpCfhList = new List<string>();
                var successLs = new List<object>();
                var cts = new CancellationTokenSource();
                var parent = new Task(() =>
                {
                    var taskFactory = new TaskFactory<bool>(cts.Token, TaskCreationOptions.AttachedToParent,
                        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                    foreach (var p in patients.ToList())
                    {
                        var cfhs = new MzCfRepo(new DefaultDatabaseFactory()).GetCf(yfbmCode, p.CardNo, p.xm, EnumFybz.Yp, true,
                            organizeId);
                        if (cfhs == null || cfhs.Count == 0) continue;

                        foreach (var cfxx in cfhs)
                        {
                            if (string.IsNullOrEmpty(cfxx.cfh))
                            {
                                continue;
                            }

                            taskFactory.StartNew(s =>
                            {
                                if (cts.IsCancellationRequested)
                                {
                                    return false;
                                }

                                var request = new
                                {
                                    cfh = cfxx.cfh,
                                    lyyf = yfbmCode,
                                    organizeId = organizeId,
                                    TimeStamp = DateTime.Now
                                };
                                var apiResp = SiteSettAPIHelper.Request<object,DefaultResponse>("api/OutpatientPharmacy/DeliveryRpDetailQuery", request);
                                if (apiResp == null || apiResp.data == null)
                                {
                                    errorMsg = "门诊结算接口异常，请联系管理员";
                                    cts.Cancel();
                                    return false;
                                }

                                var rpd = apiResp.data.ToJson().ToObject<List<fyCfYpInfo>>();
                                if (rpd == null || rpd.Count == 0)
                                {
                                    errorMsg = string.Format(@"处方【{0}】非已结算状态，无法发药", cfxx.cfh);
                                    cts.Cancel();
                                    return false;
                                }

                                var appResult = new fyDmnService(new DefaultDatabaseFactory()).ExecOutpatientDispensingDrug(cfxx.cfh, yfbmCode, userCode, organizeId);
                                if (string.IsNullOrWhiteSpace(appResult))
                                {
                                    successLs.Add(new
                                    {
                                        cfnm = cfxx.cfnm,
                                        user_code = userCode,
                                        TimeStamp = DateTime.Now
                                    });
                                    lock (locker)
                                    {
                                        tmpCfhList.Add(cfxx.cfh);
                                    }
                                    return true;
                                }

                                errorMsg = appResult;
                                cts.Cancel();
                                return false;

                            }, cts.Token);

                        }
                    }
                });
                parent.Start();
                parent.Wait();
                if (cts.IsCancellationRequested || !string.IsNullOrWhiteSpace(errorMsg))
                {
                    throw new Exception(errorMsg);
                }

                if (successLs.Count <= 0) return "";
                foreach (var item in successLs)
                {
                    var apiResp = SiteSettAPIHelper.Request<object,DefaultResponse>("api/OutpatientPharmacy/UpdatefyztByFY", item);
                    LogCore.Info("UpdatefyztByFY", string.Format("门诊发药同步发药状态; \n request:{0} \n response:{1} ; ", item.ToJson(), apiResp.ToJson()));
                }

                if (tmpCfhList.Count > 0) cfhList = tmpCfhList;
                return "";
            }
            catch (Exception e)
            {
                return e.Message + (e.InnerException == null ? "" : e.InnerException.Message);
            }
        }
    }
}