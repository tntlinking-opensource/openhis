using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IDomainServices;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Model;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO.InputDto.Inpatient;
using Newtouch.Domain.DTO.OutputDto;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Domain.IRepository.Inpatient;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ValueObjects.Inpatient;
using Newtouch.Domain.ValueObjects.Operation;
using Newtouch.Domain.ViewModels;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System.Net.NetworkInformation;
using System.Xml;
using Newtouch.CIS.Proxy.ServiceReferenceSqsq;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Net;
using Newtouch.Domain.ValueObjects.Apply;
using Newtouch.Domain.DTO;

namespace Newtouch.DomainServices.Inpatient
{
    public class DoctorserviceDmnService : DmnServiceBase, IDoctorserviceDmnService
    {
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IInpatientPatientInfoRepo _inpatientPatientInfoRepo;
        private readonly IInpatientLongTermOrderRepo _inpatientLongTermOrderRepo;
        private readonly IInpatientSTATOrderRepo _inpatientSTATOrderRepo;
        private readonly IInpatientMedicinejxCodeComparedRepo _inpatientMedicinejxCodeComparedRepo;
        private readonly IInpatientDietDetailSplitRepo _InpatientDietDetailSplitRepo;
        private readonly IInpatientDietBaseRepo _InpatientDietBaseRepo;
        private readonly IMedicalRecordDmnService _medicalRecordDmnService;
        private readonly IInpatientPatientDoctorRepo _inpatientpatientdoctorRepo;
        private readonly ITTCataloguesComparisonDmnService _tTCataloguesComparisonDmnService;
        private readonly IInpatientLongTermOrderRepo _InpatientLongTermOrderRepo;
        private readonly IQhdZnshSqtxRepo _qhdznshsqtxRepo;
        public DoctorserviceDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        { }

        #region 医嘱录入
        /// <summary>
        /// 医嘱转换成数据表的状态
        /// </summary>
        /// <param name="orgId"></param>3
        /// <param name="reqdoctorservices"></param>
        public void SubmitdoctorService(string orgId, List<DoctorServiceRequestDto> reqdoctorservices, List<string> deldata)
        {
            var dto = new SaveDoctorServiceDto()
            {
                cqyzList = new List<InpatientLongTermOrderEntity>(),
                lsyzList = new List<InpatientSTATOrderEntity>(),
                EditcqyzList = new List<InpatientLongTermOrderEntity>(),
                EditlsyzList = new List<InpatientSTATOrderEntity>(),
                ssyzList = new List<InpatientDietDetailSplitEntity>(),
                DelIds = new List<string>(),
                DelssyzIds = new List<string>()
            };

            try
            {
                if (deldata != null && deldata.Count() > 0)
                {
                    dto.DelIds.AddRange(deldata);
                }
                //代表临时的批次集合
                var FrequencyStr = _sysConfigRepo.GetValueByCode("FrequencyStr", orgId) ?? "";
                var FequencyList = FrequencyStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //组号生成集合 key操作界面带过来的组号，value后台生成的组号，最终保存到数据库
                var diczh = new Dictionary<int, int>();
                var zhreqdoctorservices = reqdoctorservices.ToJson().ToObject<List<DoctorServiceRequestDto>>();
                if (reqdoctorservices != null && reqdoctorservices.Count > 0)
                {
                    var zyh = reqdoctorservices[0].zyh;
                    var patientobj = _inpatientPatientInfoRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                    if (patientobj == null)
                    {
                        throw new FailedException("病人不存在");
                    }

                    foreach (var item in reqdoctorservices)
                    {
                        //病人住院信息表的基本信息带到医嘱表
                        item.hzxm = patientobj.xm;
                        item.WardCode = patientobj.WardCode;
                        item.DeptCode = patientobj.DeptCode;
                        //不存在频次，没办法继续。
                        if (string.IsNullOrWhiteSpace(item.pcCode))
                        {
                            continue;
                        }
                        if (FequencyList != null && FequencyList.Length > 0)
                        {
                            foreach (var fl in FequencyList)
                            {

                                var zhcnt = zhreqdoctorservices.Where(p => p.zh == item.zh).Count();
                                if (zhcnt > 1)
                                {
                                    //组号重新赋值
                                    if (!string.IsNullOrWhiteSpace(item.zh.ToString()))
                                    {
                                        if (diczh.ContainsKey(item.zh.ToInt()))
                                        {
                                            foreach (KeyValuePair<int, int> zhitem in diczh)
                                            {
                                                if (item.zh == zhitem.Key)
                                                {
                                                    item.zh = zhitem.Value;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //生成组号
                                            var No = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_cqlsyz.zh", orgId, "", false);
                                            diczh.Add(item.zh.ToInt(), No.ToInt());
                                            item.zh = No.ToInt();
                                        }
                                    }
                                }
                                else
                                {//同组cnt<1的话，成组失败
                                    item.zh = null;
                                }


                                //医嘱内容 格式：药品名称 药品规格  药品剂量  剂量单位 用法编码 频次名称 执行时间 部位

                                //item.yznr = "【" + item.xmmc + " "
                                //    + (item.ypgg ?? "")
                                //    + "】 " + (item.ypjl.ToString() ?? "")
                                //    + (item.dwwwwwww ?? "")
                                //    + " " + (item.yfmc ?? "")
                                //    + " " + item.pcmc + " "
                                //    + item.zxsj + (item.bw ?? "");
                                item.yznr = item.xmmc + " "
                                    + (item.ypjl.ToString() ?? "")
                                    + (item.dwwwwwww ?? "")
                                    + " " + (item.yfmc ?? "")
                                    + " " + item.pcmc + " "
                                    + item.zxsj + " " + (item.bw ?? "")
                                    + " " + item.ztnr;
                                if (item.yzlx == (int)EnumYzlx.Cydy)
                                {
                                    item.yznr = item.yznr + " " + item.sl + item.zydw;
                                }
                                if (item.yzlx == (int)EnumYzlx.ssyz)
                                {
                                    if (item.xmmc == "禁食")
                                    {
                                        item.yznr = item.xmmc;
                                    }
                                    else if (item.yslb == "膳食自备")
                                    {
                                        item.yznr = item.yslb;
                                    }
                                    else
                                    {
                                        item.yznr = (item.yslb ?? item.xmmc) + "[" + item.yszs + "]" + "  " + (item.ypjl.ToString() ?? "") + " " + (item.nlmd ?? "") + "  " + item.yfmc + "  " + item.pcmc;//普通膳食
                                    }
                                }
                                var ssyzcfId = "";

                                //医嘱对象赋值  
                                if (item.pcCode == fl || item.yzlx == ((int)EnumYzlx.Cydy))//临时频次
                                {
                                    ssyzcfId = item.Id;
                                    //数据库找不到这个医嘱记录的话，就新增
                                    if (!EditLSdoctorService(item, orgId, ref dto))
                                    {
                                        var addentity = AddLSdoctorService(item, orgId);
                                        if (addentity != null)
                                        {
                                            ssyzcfId = addentity.Id;
                                            dto.lsyzList.Add(addentity);
                                        }
                                    }
                                }
                                else //长期频次
                                {
                                    //根据天数算出停止时间
                                    var tzsj = new DateTime();
                                    if (!string.IsNullOrWhiteSpace(item.ts.ToString()))
                                    {
                                        tzsj = DateTime.Parse(item.kssj).AddDays(item.ts.ToInt());
                                    }
                                    ssyzcfId = item.Id;
                                    if (!EditCQdoctorService(item, orgId, ref dto, tzsj))
                                    {
                                        var addentity = AddCQdoctorService(item, orgId, tzsj);
                                        if (addentity != null)
                                        {
                                            ssyzcfId = addentity.Id;
                                            dto.cqyzList.Add(addentity);
                                        }
                                    }
                                }

                                if (item.yzlx == (int)EnumYzlx.ssyz)
                                {
                                    item.ssyzcfId = ssyzcfId;
                                    EditSSdoctorService(item, orgId, ref dto);
                                    var addentityList = AddSSdoctorService(item, orgId);
                                    if (addentityList != null)
                                    {
                                        dto.ssyzList.AddRange(addentityList);
                                    }
                                }

                            }
                        }
                        else
                        {
                            throw new FailedException("缺少临时频次配置");
                        }
                    }
                }
                SavedoctorService(dto, orgId);
            }
            catch (Exception e)
            {

                throw new FailedException("医嘱保存失败，" + e.InnerException);
            }
        }

        /// <summary>
        /// 医嘱转换成数据表的状态 长期、临时不走配置，有医生主观控制
        /// </summary>
        /// <param name="orgId"></param>3
        /// <param name="reqdoctorservices"></param>
        /// <param name="deldata"></param>
        /// <param name="yzh"></param>
        public void SubmitdoctorServiceV2(string orgId, List<DoctorServiceRequestDto> reqdoctorservices, List<string> deldata, out string yzh)
        {
            var dto = new SaveDoctorServiceDto
            {
                cqyzList = new List<InpatientLongTermOrderEntity>(),
                lsyzList = new List<InpatientSTATOrderEntity>(),
                EditcqyzList = new List<InpatientLongTermOrderEntity>(),
                EditlsyzList = new List<InpatientSTATOrderEntity>(),
                ssyzList = new List<InpatientDietDetailSplitEntity>(),
                zyyzzdList = new List<InpatientDiagnosisEntity>(),
                DelIds = new List<string>(),
                DelssyzIds = new List<string>()
            };

            try
            {
                foreach (var mx in reqdoctorservices)
                {
                    if (mx.yfztbm != null)
                    {
                        mx.yfztbs = Guid.NewGuid().ToString();
                    }

                }
                #region 收费项目组合
                var sfxmztList = reqdoctorservices.Where(p => p.sfxmzt == "Y" || p.yfztbm != null).ToList();
                reqdoctorservices = reqdoctorservices.Where(p => p.sfxmzt != "Y").ToList();
                if (sfxmztList != null && sfxmztList.Count > 0)
                {

                    foreach (var item in sfxmztList)
                    {
                        var sfmbdm = string.IsNullOrWhiteSpace(item.yfztbm) ? item.xmdm : item.yfztbm;
                        var sfxmguid = Guid.NewGuid().ToString();
                        var zhlist = GetSfxmItem(sfmbdm, orgId);
                        if (zhlist.Count == 0) continue;

                        foreach (var sfxmitem in zhlist)
                        {
                            DoctorServiceRequestDto dto1 = new DoctorServiceRequestDto();
                            dto1 = item.Clone();
                            dto1.Id = "999999999";
                            dto1.ztId = sfxmitem.ztId;
                            dto1.ztmc = sfxmitem.ztmc;
                            dto1.zxksdm = sfxmitem.zxks;
                            dto1.xmdm = sfxmitem.sfxm;
                            dto1.xmmc = sfxmitem.sfxmmc;
                            dto1.ztsl = string.IsNullOrWhiteSpace(dto1.yfztbm) ? dto1.sl : 1;
                            dto1.sl = string.IsNullOrWhiteSpace(dto1.yfztbm) ? sfxmitem.sl * dto1.sl : sfxmitem.sl;
                            dto1.dwwwwwww = sfxmitem.dw;
                            dto1.yzlx = (int)EnumYzlx.sfxm;
                            dto1.ypyfdm = string.Empty;
                            dto1.ypjl = string.IsNullOrWhiteSpace(dto1.yfztbm) ? dto1.ypjl : 1;
                            dto1.ypgg = string.IsNullOrWhiteSpace(dto1.yfztbm) ? dto1.ypgg : string.Empty;
                            dto1.yfmc = string.IsNullOrWhiteSpace(dto1.yfztbm) ? dto1.yfmc : string.Empty;
                            dto1.dcztbs = string.IsNullOrWhiteSpace(dto1.yfztbm) ? sfxmguid : string.Empty;
                            reqdoctorservices.Add(dto1);
                        }
                    }

                }
                #endregion

                #region 生成医嘱号
                var yzhPart1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                var yzhPart2 = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_lsyz.yzh", orgId);
                yzh = string.Format("{0}{1}", yzhPart1, yzhPart2);
                int px = 1;
                #endregion
                if (deldata != null && deldata.Count > 0) dto.DelIds.AddRange(deldata);
                //组号生成集合 key操作界面带过来的组号，value后台生成的组号，最终保存到数据库
                var diczh = new Dictionary<int, int>();
                var zhreqdoctorservices = reqdoctorservices.ToJson().ToObject<List<DoctorServiceRequestDto>>();
                if (reqdoctorservices != null && reqdoctorservices.Count > 0)
                {
                    var zyh = reqdoctorservices[0].zyh;
                    var patientobj = _inpatientPatientInfoRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                    if (patientobj == null) throw new FailedException("病人不存在");

                    //IList<DoctorServiceRequestDto> jyList = reqdoctorservices.Where(a => (a.yzlx == Convert.ToInt32(EnumYzlx.jy))).ToList();
                    //IList<DoctorServiceRequestDto> jcList = reqdoctorservices.Where(a => (a.yzlx == Convert.ToInt32(EnumYzlx.jc))).ToList();


                    foreach (var item in reqdoctorservices)
                    {
                        //病人住院信息表的基本信息带到医嘱表
                        item.hzxm = patientobj.xm;
                        item.WardCode = patientobj.WardCode;
                        item.DeptCode = patientobj.DeptCode;

                        //if (item.yzlx == ((int)EnumYzlx.jy) || item.yzlx == ((int)EnumYzlx.jc))
                        //{
                        //    var jyjcyzh1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //    var jyjcyzh2 = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_lsyz.yzh", orgId);
                        //    var jyjcyzh = string.Format("{0}{1}", jyjcyzh1, jyjcyzh2);
                        //    item.yzh = jyjcyzh;
                        //}
                        //else
                        //{
                        item.yzh = yzh;
                        //}

                        //不存在频次，没办法继续。
                        if (string.IsNullOrWhiteSpace(item.pcCode)) continue;

                        #region 组号
                        var zhcnt = zhreqdoctorservices.Count(p => p.zh == item.zh);
                        if (zhcnt > 1)
                        {
                            //组号重新赋值
                            if (!string.IsNullOrWhiteSpace(item.zh.ToString()))
                            {
                                if (diczh.ContainsKey(item.zh.ToInt()))
                                {
                                    foreach (KeyValuePair<int, int> zhitem in diczh)
                                    {
                                        if (item.zh == zhitem.Key)
                                        {
                                            item.zh = zhitem.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    //生成组号
                                    var No = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_cqlsyz.zh", orgId, "");
                                    diczh.Add(item.zh.ToInt(), No.ToInt());
                                    item.zh = No.ToInt();
                                }
                            }
                        }
                        else
                        {
                            //同组cnt<1的话，成组失败
                            item.zh = null;
                        }
                        #endregion

                        #region 医嘱内容 格式：药品名称 药品规格  药品剂量  剂量单位 用法编码 频次名称 执行时间 部位
                        item.yznr = item.xmmc + " "
                            + (item.ypjl.ToString() ?? "")
                            + (item.dwwwwwww ?? "")
                            + " " + (item.yfmc ?? "")
                            + " " + item.pcmc + " "
                            + item.zxsj + " " + (item.bw ?? "")
                            + " " + item.ztnr;
                        if (item.yzlx == (int)EnumYzlx.Cydy)
                        {
                            item.yznr = item.yznr + " " + item.sl + item.zydw;
                        }
                        if ((item.yzlx == (int)EnumYzlx.rehab || item.yzlx == (int)EnumYzlx.sfxm || item.yzlx == (int)EnumYzlx.yyhc) && (item.dwwwwwww ?? "").Length > 2)
                        {
                            item.yznr = item.xmmc + " "
                            + (item.ypjl.ToString() ?? " ") + "  "
                            + (item.dwwwwwww ?? "")
                            + " " + (item.yfmc ?? "")
                            + " " + item.pcmc + " "
                            + item.zxsj + " " + (item.bw ?? "")
                            + " " + item.ztnr;
                        }
                        if (item.yzlx == (int)EnumYzlx.ssyz)
                        {
                            if (item.xmmc == "禁食")
                            {
                                item.yznr = item.xmmc;
                            }
                            else if (item.yslb == "膳食自备")
                            {
                                item.yznr = item.yslb;
                            }
                            else
                            {
                                item.yznr = (item.yslb ?? item.xmmc) + "[" + item.yszs + "]" + "  " + (item.ypjl.ToString() ?? "") + " " + (item.nlmd ?? "") + "  " + item.yfmc + "  " + item.pcmc;//普通膳食
                            }
                        }
                        if (item.yzlx == (int)EnumYzlx.zcy)
                        {
                            //医嘱格式：药品名称 规格 每次剂量 每次剂量单位 用法 频次 贴数
                            item.yznr = item.xmmc + "  " + item.ypgg + " " + item.ypjl + item.dwwwwwww + " " + item.yfmc + " " + item.pcmc + " " + item.sl + "剂";
                        }
                        if (item.yzlx == (int)EnumYzlx.jy)
                        {
                            item.yznr = item.xmmc + " "
                            + (item.sl.ToString() ?? "")
                            + (item.dwwwwwww ?? "")
                            + " " + item.pcmc + " "
                            + item.zxsj
                            + " " + item.ztnr;
                        }
                        #endregion
                        var uuid = Guid.NewGuid().ToString();
                        //代表临时的批次集合
                        var frequencyStr = _sysConfigRepo.GetValueByCode("FrequencyStr", orgId) ?? "";
                        var frequencyLs = (frequencyStr.Split(',') ?? new string[0]).ToList();
                        string ssyzcfId;
                        //医嘱对象赋值  
                        if ("临".Equals(item.yzlb) || item.yzlx == (int)EnumYzlx.Cydy || item.yzlx == (int)EnumYzlx.zcy || frequencyLs.Exists(p => p == item.pcCode))//临时频次
                        {
                            ssyzcfId = item.Id;
                            //数据库找不到这个医嘱记录的话，就新增
                            if (!EditLSdoctorService(item, orgId, ref dto))
                            {
                                var addentity = AddLSdoctorService(item, orgId, uuid);
                                if (addentity != null)
                                {
                                    ssyzcfId = addentity.Id;
                                    dto.lsyzList.Add(addentity);
                                }
                            }
                        }
                        else//长期频次
                        {
                            //根据天数算出停止时间
                            var tzsj = new DateTime();
                            if (!string.IsNullOrWhiteSpace(item.ts.ToString()))
                            {
                                tzsj = DateTime.Parse(item.kssj).AddDays(item.ts.ToInt());
                            }
                            ssyzcfId = item.Id;
                            if (!EditCQdoctorService(item, orgId, ref dto, tzsj))
                            {
                                var addentity = AddCQdoctorService(item, orgId, uuid, tzsj);
                                if (addentity != null)
                                {
                                    ssyzcfId = addentity.Id;
                                    dto.cqyzList.Add(addentity);
                                }
                            }
                        }
                        var addentityListzyyzzd = AddZyZdService(item, orgId, px, uuid);
                        if (addentityListzyyzzd != null)
                        {
                            dto.zyyzzdList.AddRange(addentityListzyyzzd);
                        }
                        px++;
                        if (item.yzlx != (int)EnumYzlx.ssyz) continue;
                        item.ssyzcfId = ssyzcfId;
                        EditSSdoctorService(item, orgId, ref dto);
                        var addentityList = AddSSdoctorService(item, orgId);
                        if (addentityList != null)
                        {
                            dto.ssyzList.AddRange(addentityList);
                        }

                    }
                }
                SavedoctorService(dto, orgId);

            }
            catch (Exception e)
            {

                throw new FailedException("医嘱保存失败，" + e.InnerException);
            }
        }
        #region 医嘱事前提醒
        /// <summary>
        /// 获取第三方对照值
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="TTcode"></param>
        /// <returns></returns>
        public string GetTTItem(string orgId, string TTtype, string TTcode, string txtype)
        {
            FirstSecondThird TTobj = new FirstSecondThird();
            try
            {
                TTobj = _tTCataloguesComparisonDmnService.GetTTItem(orgId, TTtype, TTcode, txtype);
                return TTobj.First;
            }
            catch (Exception er)
            {
            }
            switch (TTtype)
            {
                case "fylb":
                    return "91";//其他费用
                case "sypc":
                    return "99";
                case "yf":
                    return "9";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 获取医嘱事前提醒数据
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="reqdoctorservices"></param>
        /// <param name="rygh"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public string GetqhdyzSqtxData(string orgId, List<DoctorServiceRequestDto> reqdoctorservices, InpatientInfo brxx, string rygh, string username, string yzcfh, out string jlId)
        {
            var zyh = reqdoctorservices[0].zyh;
            var patientobj = _inpatientPatientInfoRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
            var zlysobj = _inpatientpatientdoctorRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
            QhdPrescriptionDTO pre = new QhdPrescriptionDTO();
            REQUESTDATA request = new REQUESTDATA();
            KC21XML kc21 = new KC21XML();
            List<KC22ROW> kc22list = new List<KC22ROW>();
            List<KC33ROW> kc33list = new List<KC33ROW>();
            OrganizationData orgInfo = _medicalRecordDmnService.GetOrgInfo(orgId);

            #region 根节点组装
            request.AKB020 = orgInfo.orgCode;
            request.AKB021 = orgInfo.orgName;
            request.MSGNO = "5100";
            request.MSGID = orgInfo.orgCode + DateTime.Now.ToString("yyyyMMddHHmmss");
            request.GRANTID = string.Empty;
            request.OPERID = rygh;
            request.OPERNAME = username;
            #endregion

            #region kc21节点数据组装
            kc21.AKC190 = zyh;
            kc21.AKA130 = "21";
            kc21.AKC192 = patientobj.ryrq.ToString("yyyyMMddHHmmss");
            kc21.AKC193 = patientobj.zddm != null ? patientobj.zddm : " ";
            kc21.ZKC274 = patientobj.zdmc != null ? patientobj.zdmc : " ";
            kc21.AKC194 = string.Empty;
            kc21.AKC195 = string.Empty;
            kc21.AKC196 = string.Empty;
            kc21.ZKC275 = string.Empty;
            kc21.ZKC285 = string.Empty;
            kc21.ZKC286 = string.Empty;
            kc21.BKF040 = patientobj.DeptCode;
            kc21.ZKC272 = string.Empty;
            kc21.BKF050 = zlysobj.ysgh;
            kc21.ZKC271 = zlysobj.ysmc;
            kc21.AAC001 = patientobj.grbh;
            kc21.CKC502 = patientobj.cardno;
            kc21.AAC003 = patientobj.xm;
            kc21.AAE135 = patientobj.sfzh;
            kc21.AAC004 = patientobj.sex;
            kc21.BAE450 = Convert.ToDecimal(brxx.age);
            kc21.AAE011 = patientobj.CreatorCode;
            kc21.AAE036 = DateTime.Now.ToString("yyyyMMddHHmmss");
            request.KC21XML = kc21;

            #endregion

            #region kc22节点数据
            var sfdlcode = "";
            FirstSecondThird fylbobj = new FirstSecondThird();
            FirstSecondThird dwobj = new FirstSecondThird();
            foreach (var item in reqdoctorservices)
            {
                KC22ROW row = new KC22ROW();
                row.AKC190 = zyh;
                row.AKC220 = yzcfh;
                row.AKC221 = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (item.yzlx == (int)EnumYzlx.Yp || item.yzlx == (int)EnumYzlx.Cydy || item.yzlx == (int)EnumYzlx.zcy)
                {
                    SqtxXmYpInfoVO ypInfo = _medicalRecordDmnService.GetXmYpInfo(item.xmdm, "2", orgId);
                    sfdlcode = ypInfo.sfdlCode;
                    row.AKE001 = ypInfo.ybdm;
                    row.AKC224 = "1";
                    row.AKC225 = ypInfo.dj;
                    row.AKC227 = System.Decimal.Round((ypInfo.dj * item.sl), 4);
                }
                else
                {
                    SqtxXmYpInfoVO xmInfo = _medicalRecordDmnService.GetXmYpInfo(item.xmdm, "1", orgId);
                    sfdlcode = xmInfo.sfdlCode;
                    row.AKE001 = xmInfo.ybdm;
                    row.AKC224 = "2";
                    row.AKC225 = xmInfo.dj;
                    row.AKC227 = System.Decimal.Round((xmInfo.dj * item.sl), 4);
                }
                row.AKE002 = item.xmmc;
                row.AKC515 = item.xmdm;
                row.AKC223 = item.xmmc;
                row.AKA063 = GetTTItem(orgId, "fylb", sfdlcode, "sqtx");//其他费用 
                row.AKC226 = Convert.ToDecimal(item.sl);
                row.AKA070 = string.Empty;
                row.CKC132 = GetTTItem(orgId, "jldw", item.dwwwwwww, "sqtx");
                row.AKA074 = item.ypgg;
                row.AKA071 = item.ypjl;
                row.AKA075 = item.ypjl;
                row.AKA072 = GetTTItem(orgId, "sypc", item.pcCode, "sqtx");
                row.AKA073 = GetTTItem(orgId, "yf", item.ypyfdm, "sqtx");
                row.ZKA101 = GetTTItem(orgId, "xsdw", item.zydw, "sqtx");
                row.AKA067 = "1";
                row.AKC229 = 1;
                row.BKC127 = string.Empty;
                row.BKF050 = rygh;
                row.ZKC271 = username;
                row.AAE011 = patientobj.CreatorCode;
                row.AAE036 = DateTime.Now.ToString("yyyyMMddHHmmss");

                kc22list.Add(row);
            }
            request.KC22XML = kc22list;
            #endregion

            #region kc33节点数据
            KC33ROW xyrow = new KC33ROW();
            xyrow.AKC190 = zyh;
            xyrow.BKE150 = 1;
            xyrow.CKC305 = "1";
            xyrow.CKC304 = DateTime.Now.ToString("yyyyMMddHHmmss");
            xyrow.CKC302 = patientobj.zddm;
            xyrow.CKC303 = patientobj.zdmc;
            xyrow.BKF050 = rygh;
            xyrow.ZKC271 = username;
            xyrow.AAE013 = string.Empty;
            kc33list.Add(xyrow);
            request.KC33XML = kc33list;
            #endregion

            pre.REQUESTDATA = request;
            string responsexml = pre.XmlSerialize().Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            var qhdznshentity = new QhdZnshSqtxEntity
            {
                OrganizeId = orgId,
                jydm = "5100",
                mzzyh = zyh,
                Type = "2",
                XmlRequest = responsexml,
                zt = "1",
            };
            var rzid = "";
            _qhdznshsqtxRepo.SubmitForm(qhdznshentity, out rzid);
            jlId = rzid;
            return responsexml;
        }
        #endregion
        /// <summary>
        /// 增加医嘱诊断
        /// </summary>
        /// <param name="item"></param>
        /// <param name="orgId"></param>
        /// <param name="px"></param>
        /// <returns></returns>
        List<InpatientDiagnosisEntity> AddZyZdService(DoctorServiceRequestDto item, string orgId, int px, string guid)
        {
            var list = new List<InpatientDiagnosisEntity>();
            if (item != null && item.yzzdid != null)
            {
                var entity = new InpatientDiagnosisEntity();
                entity.Create(true);
                entity.OrganizeId = orgId;
                entity.yzId = guid;
                entity.yzh = item.yzh;
                entity.zyh = item.zyh;
                entity.zdsj = DateTime.Parse(item.yzzdsj);
                entity.icd10 = item.yzzdid;
                entity.zdmc = item.yzzdmc;
                entity.yztag = item.yztag;
                entity.yzpx = px.ToString();
                entity.yzlx = item.yzlx;
                if ("临".Equals(item.yzlb))
                {
                    entity.yzxz = "临";
                }
                else
                {
                    entity.yzxz = "长";
                }
                entity.zt = "1";
                list.Add(entity);
            }
            return list;
        }

        /// <summary>
        /// 修改临时医嘱
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool EditLSdoctorService(DoctorServiceRequestDto item, string orgId, ref SaveDoctorServiceDto dto)
        {
            var edititem = _inpatientSTATOrderRepo.FindEntity(p => p.Id == item.Id && p.zt == "1");
            if (edititem != null)//修改临时医嘱
            {
                //药品修改 用法对应组套先作废在生成
                if (edititem.yfztbs != null)
                {
                    var lsyzzt = _inpatientSTATOrderRepo.IQueryable(p => p.yfztbs == edititem.yfztbs && p.zyh == edititem.zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();
                    if (lsyzzt.Count > 0)
                    {
                        foreach (var delztId in lsyzzt)
                        {
                            dto.DelIds.Add(delztId.Id);
                        }
                    }
                }
                if (edititem.xmdm != item.xmdm)
                {
                    throw new FailedException("不能修改药品");
                }

                var editlsyz = edititem.Clone();
                editlsyz.zh = item.zh;
                editlsyz.pcCode = item.pcCode;
                editlsyz.zxcs = item.zxcs;
                editlsyz.zxzq = item.zxzq;
                editlsyz.zxzqdw = item.zxzqdw;
                editlsyz.zdm = item.zdm;
                editlsyz.zbbz = item.zbbzzzzzzz;
                editlsyz.dw = item.dwwwwwww;
                editlsyz.dwlb = item.dwlb.ToInt();
                editlsyz.zbbz = item.zbbzzzzzzz;
                editlsyz.ypjl = item.ypjl;
                editlsyz.ztnr = item.ztnr;
                editlsyz.yznr = item.yznr;
                editlsyz.ypyfdm = item.ypyfdm;
                editlsyz.kssj = DateTime.Parse(item.kssj);
                editlsyz.sl = item.sl;
                editlsyz.ztId = item.ztId;
                editlsyz.ztmc = item.ztmc;
                editlsyz.zxksdm = item.zxksdm;
                editlsyz.nlmddm = item.nlmddm;
                editlsyz.djbz = item.djbzzzzzzz;
                editlsyz.cydybz = item.cydybzzzzzzz;
                editlsyz.zzfbz = item.iszzffffff;
                editlsyz.isjf = item.isjffffff;
                editlsyz.yztag = item.yztag;
                editlsyz.ispscs = item.ispscsfffff.ToString();
                editlsyz.yfztbs = item.yfztbs;
                editlsyz.yzh = item.yzh;
                editlsyz.Modify(item.Id);
                _inpatientSTATOrderRepo.DetacheEntity(edititem);
                dto.EditlsyzList.Add(editlsyz);
                return true;

            }

            //从长期转到临时医嘱 把之前长期医嘱设置成无效，再新增临时医嘱
            var edititemtrans = _inpatientLongTermOrderRepo.FindEntity(p => p.Id == item.Id && p.zt == "1");
            if (edititemtrans == null) return false;
            if (edititemtrans.xmdm != item.xmdm)
            {
                throw new FailedException("不能修改药品");
            }
            dto.DelIds.Add(item.Id);
            dto.lsyzList.Add(AddLSdoctorService(item, orgId));
            return true;
        }
        public InpatientSTATOrderEntity AddLSdoctorService(DoctorServiceRequestDto item, string orgId)
        {
            return AddLSdoctorService(item, orgId, null);
        }
        /// <summary>
        /// 增加临时医嘱
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        InpatientSTATOrderEntity AddLSdoctorService(DoctorServiceRequestDto item, string orgId, string guid)
        {
            var lsyz = item.MapperTo<DoctorServiceRequestDto, InpatientSTATOrderEntity>();
            lsyz.dw = item.dwwwwwww;
            lsyz.zbbz = item.zbbzzzzzzz;
            lsyz.OrganizeId = orgId;
            lsyz.kssj = DateTime.Parse(item.kssj);
            lsyz.nlmddm = item.nlmddm;
            lsyz.djbz = item.djbzzzzzzz;
            lsyz.cydybz = item.cydybzzzzzzz;
            lsyz.zzfbz = item.iszzffffff;
            lsyz.isjf = item.isjffffff;
            lsyz.yztag = item.yztag;
            lsyz.ispscs = item.ispscsfffff.ToString();
            if (guid == null || guid == "")
            {
                lsyz.Create(true);
            }
            else
            {
                lsyz.Create(true, guid);
            }
            return lsyz;
        }

        /// <summary>
        /// 修改长期医嘱
        /// </summary>
        /// <param name="item"></param>
        /// <param name="orgId"></param>
        /// <param name="tzsj"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        bool EditCQdoctorService(DoctorServiceRequestDto item, string orgId, ref SaveDoctorServiceDto dto, DateTime? tzsj = null)
        {
            var Edititem = _inpatientLongTermOrderRepo.FindEntity(p => p.Id == item.Id && p.zt == "1");
            if (Edititem != null)//修改长期医嘱
            {
                if (item.yzlx != (int)EnumYzlx.ssyz && Edititem.xmdm != item.xmdm)
                {
                    throw new FailedException("不能修改药品");
                }
                //药品修改 用法对应组套先作废在生成
                if (Edititem.yfztbs != null)
                {
                    var cqyzzt = _inpatientLongTermOrderRepo.IQueryable(p => p.yfztbs == Edititem.yfztbs && p.zyh == Edititem.zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();
                    if (cqyzzt.Count > 0)
                    {
                        foreach (var delztId in cqyzzt)
                        {
                            dto.DelIds.Add(delztId.Id);
                        }
                    }
                }

                var Editcqyz = Edititem.Clone();
                Editcqyz.zh = item.zh;
                Editcqyz.pcCode = item.pcCode;
                Editcqyz.zxcs = item.zxcs;
                Editcqyz.zxzq = item.zxzq;
                Editcqyz.zxzqdw = item.zxzqdw;
                Editcqyz.zdm = item.zdm;
                Editcqyz.zbbz = item.zbbzzzzzzz;
                Editcqyz.dw = item.dwwwwwww;
                Editcqyz.dwlb = item.dwlb.ToInt();
                Editcqyz.zbbz = item.zbbzzzzzzz;
                Editcqyz.ypjl = item.ypjl;
                Editcqyz.ztnr = item.ztnr;
                Editcqyz.yznr = item.yznr;
                Editcqyz.kssj = DateTime.Parse(item.kssj);
                if (tzsj != DateTime.MinValue)
                {
                    Editcqyz.tzsj = tzsj;
                    Editcqyz.tzr = Editcqyz.ysgh;
                }
                Editcqyz.ypyfdm = item.ypyfdm;
                Editcqyz.sl = item.sl;
                Editcqyz.zxksdm = item.zxksdm;
                Editcqyz.nlmddm = item.nlmddm;
                Editcqyz.zzfbz = item.iszzffffff;
                Editcqyz.ispscs = item.ispscsfffff.ToString();
                Editcqyz.isjf = item.isjffffff;
                Editcqyz.yztag = item.yztag;
                Editcqyz.ztId = item.ztId;
                Editcqyz.ztmc = item.ztmc;
                Editcqyz.yfztbs = item.yfztbs;
                Editcqyz.yzh = item.yzh;
                Editcqyz.Modify(item.Id);
                _inpatientLongTermOrderRepo.DetacheEntity(Edititem);
                dto.EditcqyzList.Add(Editcqyz);
                return true;
            }
            else
            {//从临时医嘱转到长期医嘱 把之前临时医嘱设置成无效，再新增长期医嘱
                var Edititemtrans = _inpatientSTATOrderRepo.FindEntity(p => p.Id == item.Id && p.zt == "1");
                if (Edititemtrans != null)
                {
                    if (Edititemtrans.xmdm != item.xmdm)
                    {
                        throw new FailedException("不能修改药品");
                    }
                    dto.DelIds.Add(item.Id);
                    dto.cqyzList.Add(AddCQdoctorService(item, orgId, tzsj));
                    return true;
                }
                return false;
            }
        }
        public InpatientLongTermOrderEntity AddCQdoctorService(DoctorServiceRequestDto item, string orgId, DateTime? tzsj = null)
        {
            return AddCQdoctorService(item, orgId, null, tzsj);
        }
        /// <summary>
        /// 增加长期医嘱
        /// </summary>
        /// <param name="item"></param>
        /// <param name="orgId"></param>
        /// <param name="tzsj"></param>
        /// <returns></returns>
        InpatientLongTermOrderEntity AddCQdoctorService(DoctorServiceRequestDto item, string orgId, string uuid, DateTime? tzsj = null)
        {
            var cqyz = item.MapperTo<DoctorServiceRequestDto, InpatientLongTermOrderEntity>();
            cqyz.dw = item.dwwwwwww;
            cqyz.zbbz = item.zbbzzzzzzz;
            cqyz.zzfbz = item.iszzffffff;
            cqyz.ispscs = item.ispscsfffff.ToString();
            cqyz.isjf = item.isjffffff;
            cqyz.yztag = item.yztag;
            cqyz.OrganizeId = orgId;
            cqyz.kssj = DateTime.Parse(item.kssj);
            if (tzsj != DateTime.MinValue)
            {
                cqyz.tzsj = tzsj;
                cqyz.tzr = cqyz.ysgh;
            }
            cqyz.nlmddm = item.nlmddm;
            if (uuid == null || uuid == "")
            {
                cqyz.Create(true);
            }
            else
            {
                cqyz.Create(true, uuid);
            }
            return cqyz;
        }

        /// <summary>
        /// 修改膳食医嘱
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool EditSSdoctorService(DoctorServiceRequestDto item, string orgId, ref SaveDoctorServiceDto dto)
        {

            var Edititem = _InpatientDietDetailSplitRepo.IQueryable().Where(p => p.Id == item.Id && p.zt == "1" && p.OrganizeId == orgId);
            if (Edititem != null)//修改膳食医嘱
            {
                dto.DelssyzIds.Add(item.Id);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 增加膳食医嘱
        /// </summary>
        /// <param name="item"></param>
        /// <param name="orgId"></param>
        /// <param name="tzsj"></param>
        /// <returns></returns>
        List<InpatientDietDetailSplitEntity> AddSSdoctorService(DoctorServiceRequestDto item, string orgId)
        {
            var list = new List<InpatientDietDetailSplitEntity>();
            if (item != null && !string.IsNullOrWhiteSpace(item.yszsval))
            {
                var ssIds = item.yszsval.Split(',');
                if (ssIds != null)
                {
                    for (int i = 0; i < ssIds.Length - 1; i++)
                    {
                        var v = ssIds[i];
                        var nameentity = _InpatientDietBaseRepo.FindEntity(p => p.Id == v && p.zt == "1" && p.OrganizeId == orgId);
                        if (nameentity == null)
                        {
                            throw new FailedException("缺少膳食基础");
                        }
                        var entity = new InpatientDietDetailSplitEntity();
                        entity.OrganizeId = orgId;
                        entity.MainId = item.ssyzcfId;
                        entity.BaseId = ssIds[i];
                        entity.Name = nameentity.Name;
                        entity.sslb = item.yslbdm;
                        entity.zt = "1";
                        entity.Create(true);
                        list.Add(entity);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 提交医嘱到数据库
        /// </summary>
        /// <param name="dto"></param>
        void SavedoctorService(SaveDoctorServiceDto dto, string orgId)
        {
            try
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (dto != null)
                    {
                        if (dto.lsyzList != null && dto.lsyzList.Count > 0)
                        {
                            foreach (var item in dto.lsyzList)
                            {
                                db.Insert(item);
                            }
                        }
                        if (dto.cqyzList != null && dto.cqyzList.Count > 0)
                        {
                            foreach (var item in dto.cqyzList)
                            {
                                db.Insert(item);
                            }
                        }
                        if (dto.EditlsyzList != null && dto.EditlsyzList.Count > 0)
                        {
                            foreach (var item in dto.EditlsyzList)
                            {
                                db.Update(item);
                            }
                        }
                        if (dto.EditcqyzList != null && dto.EditcqyzList.Count > 0)
                        {
                            foreach (var item in dto.EditcqyzList)
                            {
                                db.Update(item);
                            }
                        }
                        if (dto.EditlsyzList != null && dto.EditlsyzList.Count > 0)
                        {
                            foreach (var item in dto.EditlsyzList)
                            {
                                db.Update(item);
                            }
                        }
                        if (dto.EditcqyzList != null && dto.EditcqyzList.Count > 0)
                        {
                            foreach (var item in dto.EditcqyzList)
                            {
                                db.Update(item);
                            }
                        }
                        //物理删除
                        if (dto.DelIds != null && dto.DelIds.Count() > 0)
                        {
                            var sql = @"UPDATE  dbo.zy_cqyz
                                SET     zt = 0
                                WHERE   Id IN ( SELECT  *
                                                FROM    dbo.f_split(@str, ',') )
                                        AND OrganizeId = @orgId;
                                UPDATE  dbo.zy_lsyz
                                SET     zt = 0
                                WHERE   Id IN ( SELECT  *
                                                FROM    dbo.f_split(@str, ',') )
                                        AND OrganizeId = @orgId;";

                            db.ExecuteSqlCommand(sql, new[] { new SqlParameter("@orgId", orgId),
                            new SqlParameter("@str", string.Join(",", dto.DelIds))});
                        }
                        if (dto.DelssyzIds != null && dto.DelssyzIds.Count() > 0)
                        {
                            var sql = @"UPDATE  zy_DietServicecf
                                SET     zt = 0
                                WHERE   MainId IN ( SELECT  *
                                                    FROM    dbo.f_split(@str, ',') )
                                        AND OrganizeId = @orgId;";
                            db.ExecuteSqlCommand(sql, new[] { new SqlParameter("@orgId", orgId),
                            new SqlParameter("@str", string.Join(",", dto.DelssyzIds))});
                        }
                    }
                    //更新数据库的组号
                    db.ExecuteSqlCommand("exec dbo.refreshZH @orgId", new[] { new SqlParameter("@orgId", orgId) });

                    if (dto.ssyzList != null && dto.ssyzList.Count > 0)
                    {
                        foreach (var item in dto.ssyzList)
                        {
                            db.Insert(item);
                        }
                    }
                    if (dto.zyyzzdList != null && dto.zyyzzdList.Count > 0)
                    {
                        foreach (var item in dto.zyyzzdList)
                        {
                            db.Insert(item);
                        }
                    }
                    db.Commit();
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        /// <summary>
        /// 医嘱复制新建
        /// </summary>
        /// <param name="yzId"></param>
        /// <param name="OrganizeId"></param>
        /// <param name="yzlb"></param>
        /// <param name="zyh"></param>
        public void RegistTzyz(string yzId, string OrganizeId, string yzlb, string zyh)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var yzzt = Convert.ToInt32(EnumYzzt.TZ);
                if (yzlb == "临")
                {
                    var lsyz = db.FindEntity<InpatientSTATOrderEntity>(p => p.zyh == zyh && p.Id == yzId && p.yzzt == yzzt && p.OrganizeId == OrganizeId && p.zt == "1");
                    if (lsyz == null)
                        throw new FailedException("未找到该医嘱或该医嘱非停止状态");
                    List<InpatientSTATOrderEntity> lsyzlist = new List<InpatientSTATOrderEntity>();
                    if (!string.IsNullOrWhiteSpace(lsyz.dcztbs))
                    {
                        lsyzlist = _inpatientSTATOrderRepo.IQueryable(p => p.zyh == zyh && p.dcztbs == lsyz.dcztbs && p.yzzt == yzzt && p.OrganizeId == OrganizeId && p.zt == "1").ToList();
                    }
                    else
                    {
                        lsyzlist.Add(lsyz);
                    }
                    foreach (var item in lsyzlist)
                    {
                        var lsyznew = item.Clone();
                        lsyznew.Id = null;
                        lsyznew.yzzt = Convert.ToInt32(EnumYzzt.Ds);
                        lsyznew.zh = null;
                        lsyznew.zfr = null;
                        lsyznew.zfsj = null;
                        lsyznew.zfysgh = null;
                        lsyznew.shr = null;
                        lsyznew.shsj = null;
                        lsyznew.kssj = DateTime.Now;
                        lsyznew.zxsj = null;
                        lsyznew.zxr = null;
                        var yzhPart1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                        var yzhPart2 = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_lsyz.yzh", OrganizeId);
                        lsyznew.yzh = string.Format("{0}{1}", yzhPart1, yzhPart2);
                        lsyznew.Create(true);
                        db.Insert(lsyznew);
                    }

                }
                else
                {
                    var cqyz = db.FindEntity<InpatientLongTermOrderEntity>(p => p.zyh == zyh && p.Id == yzId && p.yzzt == yzzt && p.OrganizeId == OrganizeId && p.zt == "1");
                    if (cqyz == null)
                        throw new FailedException("未找到该医嘱或该医嘱非停止状态");
                    List<InpatientLongTermOrderEntity> cqyzlist = new List<InpatientLongTermOrderEntity>();
                    if (!string.IsNullOrWhiteSpace(cqyz.dcztbs))
                    {
                        cqyzlist = _inpatientLongTermOrderRepo.IQueryable(p => p.zyh == zyh && p.dcztbs == cqyz.dcztbs && p.yzzt == yzzt && p.OrganizeId == OrganizeId && p.zt == "1").ToList();
                    }
                    else
                    {
                        cqyzlist.Add(cqyz);
                    }
                    foreach (var item in cqyzlist)
                    {
                        var cqyznew = item.Clone();
                        cqyznew.Id = null;
                        cqyznew.yzzt = Convert.ToInt32(EnumYzzt.Ds);
                        cqyznew.zh = null;
                        cqyznew.tzysgh = null;
                        cqyznew.tzsj = null;
                        cqyznew.tzr = null;
                        cqyznew.tzyy = null;
                        cqyznew.shr = null;
                        cqyznew.shsj = null;
                        cqyznew.kssj = DateTime.Now;
                        cqyznew.zxsj = null;
                        cqyznew.zxr = null;
                        var yzhPart1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                        var yzhPart2 = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_cqyz.yzh", OrganizeId);
                        cqyznew.yzh = string.Format("{0}{1}", yzhPart1, yzhPart2);
                        cqyznew.Create(true);
                        db.Insert(cqyznew);
                    }

                }
                db.Commit();
            }
        }
        #endregion

        #region 医嘱开立

        /// <summary>
        /// 获取当天有效医嘱
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        /// <returns></returns>
        public IList<CurrentViewVO> GetTodayValidService(Pagination pagination, string orgId, string zyh)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("住院号为空");
            }
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"SELECT  aaaaaa.yzId,
                                    aaaaaa.clbz ,
                                    aaaaaa.kssj ,
                                    aaaaaa.yznr ,
                                    aaaaaa.tzsj ,
                                    aaaaaa.yzzt ,
                                    aaaaaa.zh ,
                                    u.Name lrz
                            FROM    ( SELECT   Id yzId,
                                                '长' clbz ,
                                                kssj ,
                                                CreatorCode ,
                                                ( ISNULL(yznr,'') + ' ' + ISNULL(ztnr, '') ) yznr ,
                                                tzsj ,
                                                yzzt ,
                                                zh
                                      FROM      dbo.zy_cqyz
                                      WHERE     ( yzzt != 3
                                                  AND yzzt != 4
                                                )
                                                AND zyh = @zyh
                                                AND OrganizeId = @orgId
                                                AND zt = '1'
                                      UNION ALL
                                      SELECT     Id yzId,
                                                '临' clbz ,
                                                kssj ,
                                                CreatorCode ,
                                                ( ISNULL(yznr,'') + ' ' + ISNULL(ztnr, '') ) yznr,
                                                null tzsj ,
                                                yzzt ,
                                                zh
                                      FROM      dbo.zy_lsyz
                                      WHERE     ( yzzt != 3
                                                  AND yzzt != 4
                                                )
                                                AND zyh = @zyh
                                                AND CONVERT(VARCHAR(100), kssj, 111) = CONVERT(VARCHAR(100), GETDATE(), 111)
                                                AND OrganizeId = @orgId
                                                AND zt = '1'
                                    ) aaaaaa
                                    LEFT JOIN NewtouchHIS_Base..V_C_Sys_UserStaff u ON u.Account = aaaaaa.CreatorCode
                                                                                       AND u.OrganizeId = @orgId");
            par.Add(new SqlParameter("@zyh", zyh));
            par.Add(new SqlParameter("@orgId", orgId));
            return this.QueryWithPage<CurrentViewVO>(sqlstr.ToString(), pagination, par.ToArray());
        }
        /// <summary>
        /// 验证医嘱有效性
        /// </summary>
        /// <param name="req"></param>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<DSrepeatResponseDto> DoctorserviceValidate(IList<DSrepeatRequestDto> req, string zyh, string orgId)
        {
            var response = new List<DSrepeatResponseDto>();
            var inParameters = new List<SqlParameter>();
            inParameters.Add(new SqlParameter("@orgId", orgId));
            inParameters.Add(new SqlParameter("@zyh", zyh));
            inParameters.Add(new SqlParameter("@yzpartable", SqlDbType.Structured) { Value = req.ToDataTable(), TypeName = "yzpartable" });
            return FindList<DSrepeatResponseDto>("exec doctorservicevalidate @zyh,@orgId,@yzpartable", inParameters.ToArray());
        }
        /// <summary>
        /// 频次编码转换成长临标志
        /// </summary>
        /// <param name="req"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<DSrepeatRequestDto> DSTransferCL(IList<DSrepeatRequestVO> req, string orgId)
        {
            var response = new List<DSrepeatRequestDto>();
            if (req != null && req.Count() > 0)
            {
                //代表临时的批次集合
                var FrequencyStr = _sysConfigRepo.GetValueByCode("FrequencyStr", orgId) ?? "";
                var FequencyList = FrequencyStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in req)
                {
                    var entity = new DSrepeatRequestDto() { clbz = 1 };
                    entity = item.MapperTo<DSrepeatRequestVO, DSrepeatRequestDto>();
                    if (FequencyList.Contains(item.pccode))
                    {
                        entity.clbz = 0;//临时
                    }
                    else
                    {
                        entity.clbz = 1;//长期
                    }
                    response.Add(entity);
                }
            }

            return response;
        }

        /// <summary>
        /// 根据频次用法代码 获取频次用法名称 获取药品的计量单位和住院单位,剂型
        /// </summary>
        /// <param name="pcdm"></param>
        /// <param name="yfdm"></param>
        /// <param name="pcmc"></param>
        /// <param name="yfmc"></param>
        public void GetypPredata(string pcdm, string yfdm, string ypdm, string dwlb, string orgId, ref string pcmc, ref string yfmc, ref string redundant_jldw, ref string jx, ref decimal? jlzhxs, ref decimal? zyzhxs, ref string qzfs)
        {
            pcmc = "";
            yfmc = "";
            redundant_jldw = "";
            jx = "";
            jlzhxs = null;
            zyzhxs = null;
            if (!string.IsNullOrWhiteSpace(yfdm))
            {
                //获取用法
                var par = new List<SqlParameter>();
                var yfsql = new StringBuilder();
                yfsql.Append(@"SELECT  yf.yfmc
                            FROM    NewtouchHIS_Base..V_S_xt_ypyf yf
                            WHERE   yf.yfCode = @yfcode
                                    AND zt = '1'");
                par.Add(new SqlParameter("@yfcode", yfdm));

                yfmc = FirstOrDefault<string>(yfsql.ToString(), par.ToArray());
            }

            if (!string.IsNullOrWhiteSpace(pcdm))
            {
                //获取频次
                var parpc = new List<SqlParameter>();
                var pcsql = new StringBuilder();
                pcsql.Append(@"SELECT  pc.yzpcmc
                            FROM    NewtouchHIS_Base..V_S_xt_yzpc pc
                            WHERE   pc.yzpccode = @pccode
                                    AND zt = '1'");
                parpc.Add(new SqlParameter("@pccode", pcdm));
                pcmc = this.FirstOrDefault<string>(pcsql.ToString(), parpc.ToArray());
            }

            if (!string.IsNullOrWhiteSpace(ypdm) && !string.IsNullOrWhiteSpace(dwlb))
            {
                //获取药品单位下拉框的值
                var paryp = new List<SqlParameter>();
                var ypsql = new StringBuilder();
                //单位类别是剂量单位时，取住院单位，反之取剂量单位
                ypsql.Append(@"SELECT  ( CASE @dwlb
                            WHEN '1' THEN zycldw
                            WHEN 4 THEN jldw
                          END ) dw,jx,
                            jl jlzhxs ,
                            zycls zyzhxs,
                            (CASE jzlx
                              WHEN 1 THEN 'day'
                              WHEN 2 THEN 'times'
                              ELSE 'times'
                            END) qzfs
                FROM    NewtouchHIS_Base..V_C_xt_yp
                WHERE   OrganizeId = @orgId
                        AND ypCode = @ypdm
                        AND zt = '1';");
                paryp.Add(new SqlParameter("@ypdm", ypdm));
                paryp.Add(new SqlParameter("@orgId", orgId));
                paryp.Add(new SqlParameter("@dwlb", dwlb));
                var data = this.FirstOrDefault<MedicineInfoDto>(ypsql.ToString(), paryp.ToArray());
                if (data != null)
                {
                    redundant_jldw = data.dw;
                    jx = data.jx;
                    jlzhxs = data.jlzhxs;
                    zyzhxs = data.zyzhxs;
                    qzfs = data.qzfs;

                }
            }


        }

        /// <summary>
        /// 获取单位计量数
        /// </summary>
        /// <param name="sfxmCode"></param>
        /// <param name="orgId"></param>
        /// <param name="dwjls"></param>
        public void GetdwjlsBysfxmCode(string sfxmCode, string orgId, ref int? dwjls,ref decimal? dj)
        {
            if (!string.IsNullOrWhiteSpace(sfxmCode))
            {
                //获取单位计量数
                var sqlpar = new List<SqlParameter>();
                var par = new StringBuilder();
                par.Append(@"select dwjls,dj from NewtouchHIS_Base..V_S_xt_sfxm where OrganizeId = @orgId
                        AND sfxmCode=@sfxmCode and zt='1'");
                sqlpar.Add(new SqlParameter("@sfxmCode", sfxmCode));
                sqlpar.Add(new SqlParameter("@orgId", orgId));
                var data = this.FirstOrDefault<base_SfxmDto>(par.ToString(), sqlpar.ToArray());
                dwjls = data.dwjls;dj = data.dj;
            }
        }
        /// <summary>
        /// 获取执行科室名称
        /// </summary>
        /// <param name="zxksCode"></param>
        /// <param name="orgId"></param>
        /// <param name="zxksmc"></param>
        public void Getzxksmc(string zxksCode, string orgId, ref string zxksmc)
        {
            if (!string.IsNullOrWhiteSpace(zxksCode))
            {
                //获取单位计量数
                var sqlpar = new List<SqlParameter>();
                var par = new StringBuilder();
                par.Append(@"select top 1 Name from NewtouchHIS_Base..[Sys_Department] where OrganizeId = @orgId
                        AND Code=@zxksCode and zt='1'");
                sqlpar.Add(new SqlParameter("@zxksCode", zxksCode));
                sqlpar.Add(new SqlParameter("@orgId", orgId));
                zxksmc = this.FirstOrDefault<string>(par.ToString(), sqlpar.ToArray());
            }
        }

        /// <summary>
        /// 计算每天频次
        /// </summary>
        /// <param name="zxdw"></param>
        /// <param name="zydw"></param>
        /// <param name="zxcs"></param>
        /// <param name="zxzq"></param>
        /// <param name="zxzqdw"></param>
        /// <returns></returns>
        public float Getpcsl(int zxcs, int zxzq, EnumZxzqdw zxzqdw)
        {
            if (string.IsNullOrWhiteSpace(zxzqdw.ToString()))
            {
                throw new FailedException("执行周期单位不能为空");
            }
            float rtnsl;
            switch (zxzqdw)
            {
                case EnumZxzqdw.Day:
                    rtnsl = (zxcs / zxzq);
                    break;
                case EnumZxzqdw.Hour:
                    rtnsl = (zxcs / zxzq) * 24;
                    break;
                case EnumZxzqdw.Minute:
                    rtnsl = (zxcs / zxzq) * 24 * 60;
                    break;
                default:
                    rtnsl = 0;
                    break;
            }
            return rtnsl;
        }

        /// <summary>
        /// 计算药品数量
        /// </summary>
        /// <param name="jlzhxs"></param>
        /// <param name="zyzhxs"></param>
        /// <param name="ypjl"></param>
        /// <param name="ts"></param>
        /// <param name="dwlb"></param>
        /// <param name="orgId"></param>
        /// <param name="zxcs"></param>
        /// <param name="zxzq"></param>
        /// <param name="zxzqdw"></param>
        /// <returns></returns>
        public int Getypsl(decimal? jlzhxs, decimal? zyzhxs, float ypjl, EnumYPdwlb dwlb, string orgId, int zxcs, int zxzq, EnumZxzqdw zxzqdw, string qzfs, int ts = 1)
        {
            //hss 修改与2019.8.8 数量应该按照执行周期计算，因为医嘱频次QOD 代表隔天一次，和之前执行周期有区别，导致有问题。
            if (zxzqdw == EnumZxzqdw.Day)
            {
                ts = zxzq;
            }
            //取整方式
            //var qzfs = _sysConfigRepo.GetValueByCode("QZFS", orgId) ?? "day";
            var zysl = 0;
            if (string.IsNullOrWhiteSpace(dwlb.ToString()))
            {
                throw new FailedException("单位类别不能为空");
            }
            var qzint = ypjl * ts;//默认住院单位 ，不需转换
            if (dwlb == EnumYPdwlb.Jldw)
            {//剂量单位 转换成剂量单位

                if (jlzhxs == 0 || zyzhxs == 0)
                {
                    throw new FailedException("剂量转换系数和住院转换系数不能为空");
                }
                qzint = (ypjl * ts / float.Parse(jlzhxs.ToString()) / float.Parse(zyzhxs.ToString()));
            }

            if (qzfs == "day")
            {
                zysl = (Math.Ceiling(qzint * Getpcsl(zxcs, zxzq, zxzqdw))).ToInt();
            }
            else if (qzfs == "times")
            {
                zysl = (Math.Ceiling(qzint) * Getpcsl(zxcs, zxzq, zxzqdw)).ToInt();
            }
            return zysl;
        }

        ///// <summary>
        ///// 获取执行科室代码
        ///// </summary>
        ///// <param name="orgId"></param>
        ///// <param name="xmdm"></param>
        ///// <returns></returns>
        //string Getzxksdm(string orgId,string xmdm) {
        //    //获取用法
        //    var par = new List<SqlParameter>();
        //    var sql = new StringBuilder();
        //    sql.Append(@"SELECT  yfbmcode
        //        FROM    [dbo].[zy_ypjxyfdz] dz
        //                INNER JOIN NewtouchHIS_Base..V_C_xt_yp yp ON dz.jxcode = yp.jx
        //                                                             AND dz.[OrganizeId] = yp.OrganizeId
        //        WHERE   dz.zt = '1'
        //                AND yp.ypCode = @xmdm
        //                AND dz.[OrganizeId] = @orgId");
        //    par.Add(new SqlParameter("@orgId", orgId));
        //    par.Add(new SqlParameter("@xmdm", xmdm));

        //    return FirstOrDefault<string>(sql.ToString(), par.ToArray());
        //}

        #endregion

        #region 医嘱查询
        public List<AdviceListGridVO> AdviceGridView(Pagination pagination, AdviceListRequestVO req)
        {
            if (req == null || req.zyh == null)
            {
                throw new FailedException("缺少查询条件");
            }

            string yzztExpr = "";
            if (req.dryz) { }
            else if (req.wsh)
            {
                yzztExpr = " and yzzt=@yzzt";
            }
            else if (!string.IsNullOrWhiteSpace(req.yzlb))
            {
                yzztExpr = " and yzzt<>@yzzt";
            }
            if (req.yx == "yx")
            {
                if (req.wsh)
                {
                    yzztExpr = " and (yzzt=@ds or yzzt=@sh or yzzt=@zx or yzzt=@dc)";
                }
                else
                {
                    yzztExpr = " and (yzzt=@sh or yzzt=@zx or yzzt=@dc)";
                }
            }

            var rtn = new List<AdviceListGridVO>();
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"SELECT  *
                            FROM    ( SELECT    '长' yzlb ,iszt,yzh,
                                        case when yzlx in ('6','7') then yzh else yz.Id end Id,
                                        yzlx ,
                                        kssj ,
                                        s.Name ysmc ,
                                        case when yzlx in ('6','7') then ztmc else xmmc end as yzmc,
                                        CONCAT(CONVERT(float,ypjl),dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc,
                                        yz.zh ,
                                        yz.tzsj ,
                                        tzr.Name tzr ,
                                        zxr.Name zxr ,
                                        yz.yzzt ,
                                        yz.CreateTime,
						                yz.shsj,
						                yz.zxsj,
										dept.Name deptName ,
                                        yz.yztag,
                                        case yz.yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else yz.yztag end yztagName,
										isnull(yz.isjf,1) isjf,ispscs,isnull(zzfbz,0) zzfbz , yz.xmdm,case when yzlx='10' then yz.sl* (cast(yz.ypjl as int)) else yz.sl end sl,  yz.Px,
                                        case when yzlx in (2,4,10) then (cast(case when yzlx='10' then convert(numeric(18,2),yz.sl*yz.ypjl) else yz.sl end as varchar)+ypxx.zycldw) end zycldw,
                                        yfztbs,yply,isfsyz,zyh,hzxm xm
                              FROM      (
                                              select 'Y' iszt,row_number() over(partition by ztid,yzh order by createtime desc) num ,*
                                                from zy_cqyz yz with(nolock) where ztid is not null
                                                and yz.zt = '1'
                                                and yz.OrganizeId =@orgId
                                                and yz.zyh=@zyh and yfztbs is null" + yzztExpr + @"
                                                union all
										      select 'N' iszt,1 num ,*
                                                from zy_cqyz yz with(nolock) where ztid is  null 
                                                and yz.zt = '1'
                                                and yz.OrganizeId =@orgId
                                                and yz.zyh=@zyh" + yzztExpr + @"
                                        ) yz
                                        LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Staff] s ON ( ( s.gh = yz.ysgh
                                                             AND s.zt = '1'
                                                             AND s.OrganizeId =@orgId
                                                           )
                                                         ) 
                                      LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Staff] tzr ON ( ( tzr.gh = yz.tzr
                                                             AND tzr.zt = '1'
                                                             AND tzr.OrganizeId =@orgId
                                                           )
                                                         )
                                     LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Staff] zxr ON ( ( zxr.gh = yz.zxr
                                                             AND zxr.zt = '1'
                                                             AND zxr.OrganizeId =@orgId
                                                           )
                                                         )
                                      LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Department] dept ON ( yz.zxksdm=dept.Code 
                                                             AND dept.zt = '1'
                                                             AND dept.OrganizeId =@orgId
                                                           )
                                     LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on yz.ypyfdm = ypyf.yfCode 
									 LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on yz.pcCode = yzpc.yzpcCode and yzpc.OrganizeId = @orgId
                                     LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yp] ypxx  ON yz.xmdm = ypxx.ypCode AND ypxx.OrganizeId =@orgId
                                             WHERE num=1
                                             --AND yz.zt = '1' 
                                             --AND yz.OrganizeId =@orgId
                                             --and yz.zyh=@zyh
                               UNION ALL
                               select '临' yzlb ,iszt,yzh,
                                        case when yzlx in ('6','7') then yzh else yz.Id end Id ,
                                        yzlx ,
                                        kssj ,
                                        s.Name ysmc ,
                                        case when yzlx in ('6','7') then ztmc else xmmc end as yzmc,
                                        CONCAT(CONVERT(float,ypjl),dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc,
                                        yz.zh ,
                                        NULL tzsj ,
                                        NULL tzr ,
                                        zxr.Name zxr ,
                                        yz.yzzt ,
                                        yz.CreateTime,
						                yz.shsj,
						                yz.zxsj,
										dept.Name deptName,yztag,
                                        case yztag when 'JI' then '精I' when 'JII' then '精II' when 'MZ' then '麻醉' else yztag end yztagName,
                                        isnull(yz.isjf,1) isjf,
                                        ispscs ,isnull(zzfbz,0) zzfbz,
                                        xmdm,
										case when yzlx='10' then sl* (cast(yz.ypjl as int)) else sl end sl, yz.Px,
										case when yzlx in (2,4,10) then (cast(case when yzlx='10' then convert(numeric(18,2),sl*yz.ypjl) else  sl end as varchar)+ypxx.zycldw) end zycldw,
                                        yfztbs,yply,isfsyz,zyh,hzxm xm
                                        from (
                                            select 'Y' iszt,row_number() over(partition by ztid,yzh order by createtime desc) num ,*
                                            from zy_lsyz jyjcyz with(nolock) where ztid is not null--yzlx in ('6','7') 
                                            and jyjcyz.zt = '1'
                                            and jyjcyz.OrganizeId =@orgId
                                            and jyjcyz.zyh=@zyh and yfztbs is null" + yzztExpr + @"
                                            union all
										     select 'N' iszt,1 num ,*
                                            from zy_lsyz jyjcyz with(nolock) where ztid is  null --yzlx in ('6','7') 
                                            and jyjcyz.zt = '1'
                                            and jyjcyz.OrganizeId =@orgId 
                                            and jyjcyz.zyh=@zyh" + yzztExpr + @"
                                        ) yz
                                        LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Staff] s ON ( ( s.gh = yz.ysgh
                                                             AND s.zt = '1'
                                                             AND s.OrganizeId =@orgId
                                                           )
                                                         ) 
                                        LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Staff] zxr ON ( ( zxr.gh = yz.zxr
                                                             AND zxr.zt = '1'
                                                             AND zxr.OrganizeId =@orgId
                                                           )
                                                         )
                                        LEFT JOIN [NewtouchHIS_Base].[dbo].[Sys_Department] dept ON (  yz.zxksdm=dept.Code 
                                                             AND dept.zt = '1'
                                                             AND dept.OrganizeId =@orgId
                                                           )
                                      LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yp] ypxx
		                                 ON yz.xmdm = ypxx.ypCode
			                            AND ypxx.OrganizeId = @orgId
                                    LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on yz.ypyfdm = ypyf.yfCode 
									LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on yz.pcCode = yzpc.yzpcCode and yzpc.OrganizeId = @orgId
                                    WHERE  num=1
                                        --yz.zt = '1'
                                       --  AND yz.OrganizeId =@orgId
                                      --   and yz.zyh=@zyh
                            ) tttt WHERE   1 = 1");
            if (req != null)
            {
                if (req.dryz)//当日医嘱
                {
                    sqlstr.Append(" AND CONVERT(VARCHAR(10), tttt.CreateTime, 111) = CONVERT(VARCHAR(10), GETDATE(), 111)");
                }
                else if (req.wsh)
                {//未审核
                    //sqlstr.Append(" AND tttt.yzzt=@yzzt");
                    par.Add(new SqlParameter("@yzzt", EnumYzzt.Ds.ToInt()));
                }
                else if (!string.IsNullOrWhiteSpace(req.yzlb))
                {//医嘱类别
                    if (req.yzlb == "长" || req.yzlb == "临")
                    {
                        sqlstr.Append(" AND tttt.yzlb = @yzlb");
                        par.Add(new SqlParameter("@yzlb", req.yzlb));
                    }
                    //sqlstr.Append(" AND tttt.yzzt <> @yzzt");
                    par.Add(new SqlParameter("@yzzt", Convert.ToInt32(EnumYzzt.Ds)));
                }
                //if ((!req.wsh) && req.yx == "yx")//未审核前提下的有效医嘱
                //{
                //    sqlstr.Append("  AND (tttt.yzzt=@sh or tttt.yzzt=@zx)");
                //    par.Add(new SqlParameter("@sh", Convert.ToInt32(EnumYzzt.Sh)));
                //    par.Add(new SqlParameter("@zx", Convert.ToInt32(EnumYzzt.Zx)));
                //}
                if (req.yx == "yx") //有效医嘱
                {
                    if (req.wsh)
                    {//待审核
                        //sqlstr.Append("  AND (tttt.yzzt=@ds or tttt.yzzt=@sh or tttt.yzzt=@zx or tttt.yzzt=@dc)");
                        par.Add(new SqlParameter("@ds", Convert.ToInt32(EnumYzzt.Ds)));
                        par.Add(new SqlParameter("@sh", Convert.ToInt32(EnumYzzt.Sh)));
                        par.Add(new SqlParameter("@zx", Convert.ToInt32(EnumYzzt.Zx)));
                        par.Add(new SqlParameter("@dc", Convert.ToInt32(EnumYzzt.DC)));
                    }
                    else
                    {
                        //sqlstr.Append("  AND (tttt.yzzt=@sh or tttt.yzzt=@zx or tttt.yzzt=@dc)");
                        par.Add(new SqlParameter("@sh", Convert.ToInt32(EnumYzzt.Sh)));
                        par.Add(new SqlParameter("@zx", Convert.ToInt32(EnumYzzt.Zx)));
                        par.Add(new SqlParameter("@dc", Convert.ToInt32(EnumYzzt.DC)));
                    }
                }
                if (!string.IsNullOrWhiteSpace(req.yzlx.ToString()) && req.yzlx != 0)//医嘱类型
                {
                    sqlstr.Append("  AND tttt.yzlx=@yzlx");
                    par.Add(new SqlParameter("@yzlx", req.yzlx));
                }
                if (req.kssj != DateTime.MaxValue && req.kssj != DateTime.MinValue &&
                    req.jssj != DateTime.MaxValue && req.jssj != DateTime.MinValue)
                {
                    sqlstr.Append(" AND tttt.kssj > @kssj AND tttt.kssj < DATEADD(dd, 1, @jssj)");
                    par.Add(new SqlParameter("@kssj", req.kssj));
                    par.Add(new SqlParameter("@jssj", req.jssj));
                }
            }
            par.Add(new SqlParameter("@orgId", req.orgId));
            par.Add(new SqlParameter("@zyh", req.zyh));
            return this.QueryWithPage<AdviceListGridVO>(sqlstr.ToString(), pagination, par.ToArray(), false).ToList();
        }

        /// <summary>
        /// 停止长期医嘱或者作废临时医嘱 同组号
        /// </summary>
        /// <param name="yzId"></param>
        /// <param name="tzsj"></param>
        /// <param name="czr"></param>
        public void AdviceStop(string yzId, DateTime? tzsj, string czr, string orgId, string yzlx, string zyh, string iszt)
        {
            if (string.IsNullOrWhiteSpace(yzId))
            {
                throw new FailedException("医嘱不存在");
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {

                var cqyz = db.FindEntity<InpatientLongTermOrderEntity>(p => p.Id == yzId && p.OrganizeId == orgId && p.zt == "1");

                if (cqyz != null)
                {
                    if (tzsj == null)
                    {
                        throw new FailedException("缺少停止时间");
                    }
                    if (cqyz.zxsj != null)
                    {
                        if (DateTimeManger.DateDiff(DateInterval.Second, DateTime.Parse(cqyz.zxsj.ToString()), DateTime.Parse(tzsj.ToString())) < 1)
                        {
                            throw new FailedException("停止时间不能小于最后执行时间；最后执行时间：" + cqyz.zxsj);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(cqyz.zh.ToString()))
                    {
                        var zhlist = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.zh == cqyz.zh && p.OrganizeId == orgId && p.zt == "1");
                        if (zhlist != null && zhlist.Count() > 0)
                        {
                            foreach (var item in zhlist)
                            {
                                item.tzsj = tzsj;
                                item.tzr = czr;
                                item.yzzt = (int)EnumYzzt.DC;
                                db.DetacheEntity<InpatientLongTermOrderEntity>(item);
                                db.Update(item);
                            }
                        }
                    }
                    else
                    {
                        List<InpatientLongTermOrderEntity> cqyzzt = new List<InpatientLongTermOrderEntity>();
                        if (!string.IsNullOrWhiteSpace(cqyz.dcztbs))
                        {
                            cqyzzt = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.dcztbs == cqyz.dcztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(cqyz.yfztbs))
                                cqyzzt = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.yfztbs == cqyz.yfztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();

                            cqyzzt.Add(cqyz);

                        }
                        foreach (var item in cqyzzt)
                        {
                            item.tzsj = tzsj;
                            item.tzr = czr;
                            item.yzzt = (int)EnumYzzt.DC;
                            db.DetacheEntity<InpatientLongTermOrderEntity>(item);
                            db.Update(item);
                        }
                    }
                }
                else
                {
                    if (yzlx == ((int)EnumYzlx.jy).ToString() || yzlx == ((int)EnumYzlx.jc).ToString())
                    {
                        var jyjcyz = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.yzh == yzId && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                        if (jyjcyz != null && jyjcyz.Count() > 0)
                        {
                            foreach (var item in jyjcyz)
                            {
                                item.zfsj = DateTime.Now;
                                item.zfr = czr;
                                item.yzzt = (int)EnumYzzt.DC;
                                db.DetacheEntity<InpatientSTATOrderEntity>(item);
                                db.Update(item);
                            }
                        }
                    }
                    else
                    {
                        var lsyz = db.FindEntity<InpatientSTATOrderEntity>(p => p.Id == yzId && p.OrganizeId == orgId && p.zt == "1");
                        if (lsyz != null)
                        {
                            if (!string.IsNullOrWhiteSpace(lsyz.zh.ToString()))
                            {
                                var zhlist = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.zh == lsyz.zh && p.OrganizeId == orgId && p.zt == "1");
                                if (zhlist != null && zhlist.Count() > 0)
                                {
                                    foreach (var item in zhlist)
                                    {
                                        item.zfsj = DateTime.Now;
                                        item.zfr = czr;
                                        item.yzzt = (int)EnumYzzt.DC;
                                        db.DetacheEntity<InpatientSTATOrderEntity>(item);
                                        db.Update(item);
                                    }
                                }
                            }
                            else
                            {
                                if (lsyz.sssj != null && lsyz.sqdh != "")
                                {
                                    var sqlpar = new List<SqlParameter>();
                                    var cxsql = @"select 1 as jg from [Newtouch_OR].[dbo].[OR_ApplyInfo] a 
                                        left join [Newtouch_OR].[dbo].[OR_ApplyInfo_Expand] b 
                                        on a.applyno=b.applyno and b.px=1
                                        where a.zyh=@zyh and a.zt=1  and a.OrganizeId=@orgId and a.sqzt<>2  and a.Applyno=@sqdh";
                                    sqlpar.Add(new SqlParameter("@zyh", lsyz.zyh));
                                    sqlpar.Add(new SqlParameter("@orgId", orgId));
                                    sqlpar.Add(new SqlParameter("@sqdh", lsyz.sqdh));
                                    var listjg = FindList<TovoidOperationVO>(cxsql, sqlpar.ToArray());
                                    if (listjg.Count > 0)
                                    {
                                        var sqlupdate = @"update a set a.sqzt=3
                                                        from [Newtouch_OR].[dbo].[OR_ApplyInfo] a 
                                                        left join [Newtouch_OR].[dbo].[OR_ApplyInfo_Expand] b 
                                                        on a.applyno=b.applyno and b.px=1
                                                        where a.zyh=@zyh and  a.zt=1 and a.OrganizeId=@orgId and  a.sqzt<>2 and  a.Applyno=@sqdh";

                                        ExecuteSqlCommand(sqlupdate, new SqlParameter[] { new SqlParameter("@zyh", lsyz.zyh),
                                        new SqlParameter("@orgId", orgId),
                                        new SqlParameter("@sqdh", lsyz.sqdh) });
                                        lsyz.zfsj = DateTime.Now;
                                        lsyz.zfr = czr;
                                        lsyz.yzzt = (int)EnumYzzt.DC;
                                        db.DetacheEntity<InpatientSTATOrderEntity>(lsyz);
                                        db.Update(lsyz);
                                    }
                                    else
                                    {
                                        throw new FailedException("审核通过的医嘱不能删除！");
                                    }
                                }
                                else
                                {
                                    List<InpatientSTATOrderEntity> lsyzzt = new List<InpatientSTATOrderEntity>();
                                    if (!string.IsNullOrWhiteSpace(lsyz.dcztbs))
                                    {
                                        lsyzzt = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.dcztbs == lsyz.dcztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(lsyz.yfztbs))
                                            lsyzzt = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.yfztbs == lsyz.yfztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();

                                        lsyzzt.Add(lsyz);

                                    }
                                    foreach (var lsitem in lsyzzt)
                                    {
                                        lsitem.zfsj = DateTime.Now;
                                        lsitem.zfr = czr;
                                        lsitem.yzzt = (int)EnumYzzt.DC;
                                        db.DetacheEntity<InpatientSTATOrderEntity>(lsitem);
                                        db.Update(lsitem);
                                    }
                                }
                            }

                        }
                    }
                }
                db.Commit();
            }
        }

        /// <summary>
        /// 物理删除,删除同组号的数据
        /// </summary>
        /// <param name="yzId"></param>
        public void AdviceDel(string yzId, string orgId, string yzlx, string zyh, string iszt, string conflinktoOR)
        {
            if (string.IsNullOrWhiteSpace(yzId))
            {
                throw new FailedException("医嘱不存在");
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var cqyz = db.FindEntity<InpatientLongTermOrderEntity>(p => p.Id == yzId && p.OrganizeId == orgId && p.zt == "1");
                if (cqyz != null)
                {
                    if (!string.IsNullOrWhiteSpace(cqyz.zh.ToString()))
                    {
                        var zhlist = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.zh == cqyz.zh && p.OrganizeId == orgId && p.zt == "1");
                        if (zhlist != null && zhlist.Count() > 0)
                        {
                            foreach (var item in zhlist)
                            {
                                item.zt = "0";
                                db.DetacheEntity<InpatientLongTermOrderEntity>(item);
                                db.Update(item);
                            }
                        }
                    }
                    else
                    {
                        List<InpatientLongTermOrderEntity> cqyzzt = new List<InpatientLongTermOrderEntity>();
                        if (!string.IsNullOrWhiteSpace(cqyz.dcztbs))
                        {
                            cqyzzt = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.dcztbs == cqyz.dcztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(cqyz.yfztbs))
                                cqyzzt = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.yfztbs == cqyz.yfztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();

                            cqyzzt.Add(cqyz);

                        }
                        foreach (var item in cqyzzt)
                        {
                            item.zt = "0";
                            db.DetacheEntity<InpatientLongTermOrderEntity>(item);
                            db.Update(item);
                        }
                    }
                }
                else
                {
                    //检验检查以组套删除
                    if (yzlx == ((int)EnumYzlx.jy).ToString() || yzlx == ((int)EnumYzlx.jc).ToString())
                    {
                        var jyjcyz = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.yzh == yzId && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                        if (jyjcyz != null && jyjcyz.Count() > 0)
                        {
                            foreach (var item in jyjcyz)
                            {
                                item.zt = "0";
                                db.DetacheEntity<InpatientSTATOrderEntity>(item);
                                db.Update(item);
                            }

                        }
                    }
                    else
                    {
                        var lsyz = db.FindEntity<InpatientSTATOrderEntity>(p => p.Id == yzId && p.OrganizeId == orgId && p.zt == "1");
                        if (lsyz != null)
                        {
                            if (!string.IsNullOrWhiteSpace(lsyz.zh.ToString()))
                            {
                                var zhlist = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.zh == lsyz.zh && p.OrganizeId == orgId && p.zt == "1");
                                if (zhlist != null && zhlist.Count() > 0)
                                {
                                    foreach (var item in zhlist)
                                    {
                                        item.zt = "0";
                                        db.DetacheEntity<InpatientSTATOrderEntity>(item);
                                        db.Update(item);

                                    }

                                }
                            }
                            else
                            {
                                if (lsyz.sssj != null && lsyz.sqdh != "")
                                {

                                    if (!string.IsNullOrWhiteSpace(conflinktoOR) && conflinktoOR == "true")
                                    {
                                        var sqlpar = new List<SqlParameter>();

                                        var cxsql = @"select 1 as jg from [Newtouch_OR].[dbo].[OR_ApplyInfo] a 
                                        left join [Newtouch_OR].[dbo].[OR_ApplyInfo_Expand] b 
                                        on a.applyno=b.applyno and b.px=1
                                        where a.zyh=@zyh and a.zt=1  and a.OrganizeId=@orgId and a.sqzt<>2  and a.Applyno=@sqdh";
                                        sqlpar.Add(new SqlParameter("@zyh", lsyz.zyh));
                                        sqlpar.Add(new SqlParameter("@orgId", orgId));
                                        sqlpar.Add(new SqlParameter("@sqdh", lsyz.sqdh));
                                        var listjg = FindList<SelectoperationjgVO>(cxsql.ToString(), sqlpar.ToArray());
                                        if (listjg.Count > 0)
                                        {
                                            var sqlupdate = @"update a set a.sqzt=3
                                                        from [Newtouch_OR].[dbo].[OR_ApplyInfo] a 
                                                        left join [Newtouch_OR].[dbo].[OR_ApplyInfo_Expand] b 
                                                        on a.applyno=b.applyno and b.px=1
                                                        where a.zyh=@zyh and  a.zt=1 and a.OrganizeId=@orgId and  a.sqzt<>2 and  a.Applyno=@sqdh";

                                            ExecuteSqlCommand(sqlupdate.ToString(), new SqlParameter[] { new SqlParameter("@zyh", lsyz.zyh),
                                        new SqlParameter("@orgId", orgId),
                                        new SqlParameter("@sqdh", lsyz.sqdh)});
                                            lsyz.zt = "0";
                                            db.DetacheEntity<InpatientSTATOrderEntity>(lsyz);
                                            db.Update(lsyz);
                                        }
                                        else
                                        {
                                            throw new FailedException("审核通过的医嘱不能删除！");
                                        }
                                    }
                                    else
                                    {
                                        lsyz.zt = "0";
                                        db.DetacheEntity<InpatientSTATOrderEntity>(lsyz);
                                        db.Update(lsyz);
                                    }

                                }
                                else
                                {
                                    List<InpatientSTATOrderEntity> lsyzzt = new List<InpatientSTATOrderEntity>();
                                    if (!string.IsNullOrWhiteSpace(lsyz.dcztbs))
                                    {
                                        lsyzzt = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.dcztbs == lsyz.dcztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(lsyz.yfztbs))
                                            lsyzzt = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.yfztbs == lsyz.yfztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();

                                        lsyzzt.Add(lsyz);

                                    }
                                    foreach (var lsitem in lsyzzt)
                                    {
                                        lsitem.zt = "0";
                                        db.DetacheEntity<InpatientSTATOrderEntity>(lsitem);
                                        db.Update(lsitem);
                                    }
                                }

                            }
                        }
                    }
                }
                //膳食医嘱
                var ssyzlist = db.IQueryable<InpatientDietDetailSplitEntity>().Where(p => p.MainId == yzId && p.OrganizeId == orgId && p.zt == "1");
                if (ssyzlist != null && ssyzlist.Count() > 0)
                {
                    foreach (var item in ssyzlist)
                    {
                        item.zt = "0";
                        db.DetacheEntity<InpatientDietDetailSplitEntity>(item);
                        db.Update(item);
                    }

                }
                db.Commit();
            }
        }

        /// <summary>
        /// 撤DC 撤dc同组号的医嘱
        /// </summary>
        /// <param name="yzId"></param>
        public void Advicedc(string yzId, string orgId, string yzlx, string zyh, string isFeeGroup)
        {
            if (string.IsNullOrWhiteSpace(yzId))
            {
                throw new FailedException("医嘱不存在");
            }
            try
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    var cqyz = db.FindEntity<InpatientLongTermOrderEntity>(p => p.Id == yzId && p.OrganizeId == orgId && p.zt == "1");
                    if (cqyz != null)
                    {
                        if (cqyz.yzzt != (int)EnumYzzt.DC)
                        {
                            throw new FailedException("医嘱状态不正确");
                        }
                        if (!string.IsNullOrWhiteSpace(cqyz.zh.ToString()))
                        {
                            var zhlist = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.zh == cqyz.zh && p.OrganizeId == orgId && p.zt == "1");
                            if (zhlist != null && zhlist.Count() > 0)
                            {
                                foreach (var item in zhlist)
                                {
                                    item.tzr = null;
                                    item.tzsj = null;
                                    item.yzzt = item.zxsj != null ? (int)EnumYzzt.Zx : (int)EnumYzzt.Sh;
                                    //有执行时间，取已执行状态，有审核时间，取已审核状态
                                    db.DetacheEntity<InpatientLongTermOrderEntity>(item);
                                    db.Update(item);
                                }
                            }
                        }
                        else
                        {
                            List<InpatientLongTermOrderEntity> cqyzzt = new List<InpatientLongTermOrderEntity>();
                            if (!string.IsNullOrWhiteSpace(cqyz.dcztbs))//收费项目组套
                            {
                                cqyzzt = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.dcztbs == cqyz.dcztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(cqyz.yfztbs))//药品开立 用法对应组套
                                    cqyzzt = db.IQueryable<InpatientLongTermOrderEntity>().Where(p => p.yfztbs == cqyz.yfztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();

                                cqyzzt.Add(cqyz);

                            }
                            foreach (var item in cqyzzt)
                            {
                                item.tzr = null;
                                item.tzsj = null;
                                item.yzzt = item.zxsj != null ? (int)EnumYzzt.Zx : (int)EnumYzzt.Sh;
                                //有执行时间，取已执行状态，有审核时间，取已审核状态
                                db.DetacheEntity<InpatientLongTermOrderEntity>(item);
                                db.Update(item);
                            }
                        }
                    }
                    else
                    {
                        if (yzlx == ((int)EnumYzlx.jy).ToString() || yzlx == ((int)EnumYzlx.jc).ToString())//检验检查按医嘱号
                        {
                            var jyjcyz = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.yzh == yzId && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                            if (jyjcyz != null && jyjcyz.Count() > 0)
                            {
                                foreach (var item in jyjcyz)
                                {
                                    //有执行时间，取已执行状态，有审核时间，取已审核状态
                                    item.yzzt = item.zxsj != null ? (int)EnumYzzt.Zx : (int)EnumYzzt.Sh;
                                    db.DetacheEntity<InpatientSTATOrderEntity>(item);
                                    db.Update(item);
                                }

                            }
                        }
                        else
                        {
                            var lsyz = db.FindEntity<InpatientSTATOrderEntity>(p => p.Id == yzId && p.OrganizeId == orgId && p.zt == "1");
                            if (lsyz != null)
                            {
                                if (lsyz.yzzt != (int)EnumYzzt.DC)
                                {
                                    throw new FailedException("医嘱状态不正确");
                                }
                                if (!string.IsNullOrWhiteSpace(lsyz.zh.ToString()))
                                {
                                    var zhlist = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.zh == lsyz.zh && p.OrganizeId == orgId && p.zt == "1");
                                    if (zhlist != null && zhlist.Count() > 0)
                                    {
                                        foreach (var item in zhlist)
                                        {
                                            //有执行时间，取已执行状态，有审核时间，取已审核状态
                                            item.yzzt = item.zxsj != null ? (int)EnumYzzt.Zx : (int)EnumYzzt.Sh;

                                            db.DetacheEntity<InpatientSTATOrderEntity>(item);
                                            db.Update(item);
                                        }

                                    }
                                }
                                else
                                {
                                    List<InpatientSTATOrderEntity> lsyzzt = new List<InpatientSTATOrderEntity>();
                                    if (!string.IsNullOrWhiteSpace(lsyz.dcztbs))
                                    {
                                        lsyzzt = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.dcztbs == lsyz.dcztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1").ToList();
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(lsyz.yfztbs))
                                            lsyzzt = db.IQueryable<InpatientSTATOrderEntity>().Where(p => p.yfztbs == lsyz.yfztbs && p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1" && p.ztId != null).ToList();

                                        lsyzzt.Add(lsyz);

                                    }
                                    foreach (var lsitem in lsyzzt)
                                    {
                                        //有执行时间，取已执行状态，有审核时间，取已审核状态
                                        lsitem.yzzt = lsitem.zxsj != null ? (int)EnumYzzt.Zx : (int)EnumYzzt.Sh;
                                        db.DetacheEntity<InpatientSTATOrderEntity>(lsitem);
                                        db.Update(lsitem);
                                    }
                                }

                            }
                        }
                    }
                    db.Commit();
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }

        /// <summary>
        /// 出院全停
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="tzsj"></param>
        /// <param name="orgId"></param>
        /// <param name="czr"></param>
        public void AdviceLeaveHospitalStopSubmit(string zyh, DateTime tzsj, string orgId, string czr)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("缺少住院号");
            }
            var patientobj = _inpatientPatientInfoRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
            try
            {
                //停止所有长期医嘱
                var cqyzList = _inpatientLongTermOrderRepo.IQueryable().Where(p => p.zyh == zyh && p.yzzt != 3 && p.OrganizeId == orgId && p.zt == "1" && p.yzzt != (int)EnumYzzt.TZ);
                if (cqyzList != null && cqyzList.Count() > 0)
                {
                    var lastzxsj = cqyzList.OrderByDescending(p => p.zxsj).FirstOrDefault().zxsj;
                    if (lastzxsj != null)
                    {
                        if (DateTimeManger.DateDiff(DateInterval.Second, DateTime.Parse(lastzxsj.ToString()), tzsj) < 1)
                        {
                            throw new FailedException("停止时间不能小于最后执行时间；最后执行时间：" + lastzxsj);
                        }
                    }
                }
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (cqyzList != null && cqyzList.Count() > 0)
                    {
                        foreach (var cqyz in cqyzList)
                        {
                            cqyz.tzsj = tzsj;
                            cqyz.tzr = czr;
                            cqyz.yzzt = (int)EnumYzzt.DC;
                            _inpatientLongTermOrderRepo.DetacheEntity(cqyz);
                            db.Update(cqyz);
                        }
                    }
                    //是否生成文字医嘱
                    bool AutoGenerateWordAdvice = _sysConfigRepo.GetBoolValueByCode("AutoGenerateWordAdvice", orgId, true).ToBool();
                    if (AutoGenerateWordAdvice)
                    {
                        //生成一条临时文字医嘱
                        var lsyz = new InpatientSTATOrderEntity()
                        {
                            xmdm = "999999999999999999",
                            xmmc = DateTime.Now + "出院及康复咨询",
                            zyh = zyh,
                            WardCode = patientobj.WardCode,
                            DeptCode = patientobj.DeptCode,
                            ysgh = czr,
                            pcCode = "00",
                            yzzt = (int)EnumYzzt.Ds,
                            yzlx = (int)EnumYzlx.Wz,
                            yznr = DateTime.Now + "出院及康复咨询     ST",
                            OrganizeId = orgId,
                            kssj = tzsj,
                            hzxm = patientobj.xm

                        };
                        lsyz.Create(true);
                        db.Insert(lsyz);
                    }
                    db.Commit();
                }

            }
            catch (Exception e)
            {

                throw;
            }

        }

        /// <summary>
        /// 转区全停
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="czr"></param>
        public void AdviceTransferWardStopSubmit(TransferWardRequestVO req)
        {
            if (string.IsNullOrWhiteSpace(req.zyh))
            {
                throw new FailedException("缺少住院号");
            }

            var patientobj = _inpatientPatientInfoRepo.IQueryable().FirstOrDefault(p => p.zyh == req.zyh && p.OrganizeId == req.orgId && p.zt == "1");
            try
            {
                //停止所有长期医嘱
                var cqyzList = _inpatientLongTermOrderRepo.IQueryable().Where(p => p.zyh == req.zyh && p.yzzt != 3 && p.OrganizeId == req.orgId && p.zt == "1");
                if (cqyzList != null && cqyzList.Count() > 0)
                {
                    var lastzxsj = cqyzList.OrderByDescending(p => p.zxsj).FirstOrDefault().zxsj;
                    if (lastzxsj != null)
                    {
                        if (DateTimeManger.DateDiff(DateInterval.Second, DateTime.Parse(lastzxsj.ToString()), req.kssj) < 1)
                        {
                            throw new FailedException("开始时间不能小于最后执行时间；最后执行时间：" + lastzxsj);
                        }
                    }
                }

                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    if (cqyzList != null && cqyzList.Count() > 0)
                    {

                        foreach (var cqyz in cqyzList)
                        {
                            cqyz.tzsj = req.kssj;
                            cqyz.tzr = req.czr;
                            cqyz.yzzt = (int)EnumYzzt.DC;
                            _inpatientLongTermOrderRepo.DetacheEntity(cqyz);
                            db.Update(cqyz);
                        }
                    }
                    //是否正常文字医嘱
                    bool AutoGenerateWordAdvice = _sysConfigRepo.GetBoolValueByCode("AutoGenerateWordAdvice", req.orgId, true).ToBool();

                    if (AutoGenerateWordAdvice)
                    {
                        //生成一条临时文字医嘱
                        var lsyz = new InpatientSTATOrderEntity()
                        {
                            xmdm = "999999999999999999",
                            xmmc = DateTime.Now + "出院及康复咨询",
                            zyh = req.zyh,
                            WardCode = patientobj.WardCode,
                            DeptCode = patientobj.DeptCode,
                            ysgh = req.czr,
                            pcCode = "00",
                            yzzt = (int)EnumYzzt.Ds,
                            yzlx = (int)EnumYzlx.Wz,
                            yznr = req.kssj.ToString() + "转至" + req.bq,
                            OrganizeId = req.orgId,
                            kssj = req.kssj,
                            hzxm = patientobj.xm
                        };
                        lsyz.Create(true);
                        db.Insert(lsyz);
                    }

                    db.Commit();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 医嘱频次code获取频次entity
        /// </summary>
        /// <param name="code"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public SysMedicalOrderFrequencyVEntity getpcInfoByCode(string code, string orgId)
        {
            //获取用法
            var par = new List<SqlParameter>();
            var yfsql = new StringBuilder();
            yfsql.Append(@"SELECT  yzpcId ,
                            yzpcmc,
                            zxsj ,
                            yzpcCode ,
                            zxcs ,
                            zxzq ,
                            zxzqdw
                    FROM    NewtouchHIS_Base..xt_yzpc
                    WHERE   OrganizeId = @orgId
                            AND yzpcCode = @code
                            AND zt = '1';");
            par.Add(new SqlParameter("@code", code));
            par.Add(new SqlParameter("@orgId", orgId));
            return FirstOrDefault<SysMedicalOrderFrequencyVEntity>(yfsql.ToString(), par.ToArray());
        }


        public List<RehabVO> GetSfxmZxksSelectJson(string orgId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"select Code,Name,py from [NewtouchHIS_Base]..Sys_Department 
            where zt=1 and organizeid=@orgId
            and zxks=1
            ");
            par.Add(new SqlParameter("@orgId", orgId));
            return this.FindList<RehabVO>(sqlstr.ToString(), par.ToArray());
        }
        public List<RehabVO> GetSfxmZxksSelectJson(string orgId, string keyword)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"select Code,Name,py from [NewtouchHIS_Base]..Sys_Department 
            where zt=1 and organizeid=@orgId
            and (code like '%" + keyword + @"%' or name like '%" + keyword + @"%' or py like '%" + keyword + @"%')
            and zxks='1'
            ");
            par.Add(new SqlParameter("@orgId", orgId));
            return this.FindList<RehabVO>(sqlstr.ToString(), par.ToArray());
        }


        #region 收费组合查询
        public List<SfxmztDto> GetSfxmItem(string sfmbbh, string orgId)
        {
            string sql = @" select  b.sfxm ,isnull(sfxm.sfxmmc,yp.ypmc) sfxmmc,a.sfmbbh ztId,sfmbmc ztmc,
sfxm.dw dw, b.dj dj,sl,a.ks zxks,c.Name zxksmc from NewtouchHIS_Sett..[xt_sfmb] a
join NewtouchHIS_Sett..[xt_sfmbxm] b on a.sfmbbh=b.sfmbbh and a.organizeid=b.organizeid and b.zt=1
LEFT JOIN NewtouchHIS_Base..V_S_xt_sfxm sfxm  ON sfxm.sfxmCode=b.sfxm and sfxm.OrganizeId=b.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_xt_yp yp  ON yp.ypCode=b.sfxm and yp.OrganizeId=b.OrganizeId
LEFT JOIN NewtouchHIS_Base..V_S_Sys_Department c  ON c.Code=a.ks  AND c.OrganizeId= a.OrganizeId
where a.zt=1 and a.sfmb=@sfmb and a.OrganizeId=@orgId ";
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            sqlpar.Add(new SqlParameter("@sfmb", sfmbbh));
            var listjg = FindList<SfxmztDto>(sql, sqlpar.ToArray());
            return listjg;
        }

        /// <summary>
        /// 用法对应收费项目组套
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="yfcode"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<yfsfxmdyDto> GetSfxmYfList(string orgId, string yfcode, string keyword)
        {
            var sqlpar = new List<SqlParameter>();
            string sql = @" select sfmb,sfmbmc
 from NewtouchHIS_Sett..[xt_sfmb] a
join NewtouchHIS_Sett..[xt_sfmbxm] b on a.sfmbbh=b.sfmbbh and a.organizeid=b.organizeid and b.zt=1
                            where a.zt=1  and a.OrganizeId=@orgId ";
            if (yfcode != null)
            {
                sql += " and yfdm=@yfcode";
                sqlpar.Add(new SqlParameter("@yfcode", yfcode));
            }
            else
            {
                sql += " and 1=2";
            }
            if (keyword != null)
            {
                sql += " and (a.sfmb like @keyword or a.sfmbmc like @keyword)";
                sqlpar.Add(new SqlParameter("@keyword", "%" + keyword.Trim() + "%"));
            }
            sql += " group by sfmb,sfmbmc,a.py,a.mzzybz,a.ks";
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            var listjg = FindList<yfsfxmdyDto>(sql, sqlpar.ToArray());
            return listjg;
        }
        #endregion

        #region 手术修改按钮
        public List<SelectUpdataOpertionVO> Ssupdate(string yzId, string zyh, string OrganizeId)
        {
            var sqlpar = new List<SqlParameter>();
            var sql = @"select 1 as jg from [Newtouch_CIS].[dbo].[zy_lsyz] 
                       where Id=@yzId and OrganizeId=@OrganizeId and yzlx=3 and sssj is not null and zyh=@zyh and zt=1";
            sqlpar.Add(new SqlParameter("@yzId", yzId));
            sqlpar.Add(new SqlParameter("@OrganizeId", OrganizeId));
            sqlpar.Add(new SqlParameter("@zyh", zyh));
            return FindList<SelectUpdataOpertionVO>(sql, sqlpar.ToArray());
        }
        #endregion

        public PatientInformationDTO patinfo(string zyh, string orgId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"
select xzlx,
case rytj when null then '2101' when '24' then '1301' when '52' then '52' else '2101' end yllb,
case zyxx.brxz when '9' then '1' else '0' end ydjy,
zd.zddm ryzd,
zd.zdmc ryzdmc,
isnull(zd2.zddm,zd.zddm) cyzd,
isnull(zd2.zdmc,zd.zdmc) cyzdmc,
case zd2.cyqk when null then '无' when '1' then '治愈' when '2' then '好转' when '3' then '未愈' when '4' then '死亡' when '5' then '其他' when '6' then '转院'  else  '无' end cyqk,
ks.name ksmc,
ks.Code kscode,
convert(varchar(50),brxx.rqrq,120) rqrq,
convert(varchar(50),isnull(brxx.cqrq,brxx.rqrq),120) cqrq
from zy_brxxk brxx
left join [NewtouchHIS_Sett]..zy_brjbxx zyxx on brxx.zyh=zyxx.zyh and zyxx.OrganizeId=brxx.OrganizeId
left join [NewtouchHIS_Sett].[dbo].[xt_card] kh on kh.patid=zyxx.patid and  kh.OrganizeId=zyxx.OrganizeId
left join [dbo].[zy_PatDxInfo] zd on zd.zyh=brxx.zyh and zd.OrganizeId= zyxx.OrganizeId and zd.zdlb='1' and zd.zt='1' and zd.zddm!='999999999' 
left join [dbo].[zy_PatDxInfo] zd2 on zd2.zyh=brxx.zyh and zd2.OrganizeId= zyxx.OrganizeId and zd2.zdlb='2' and zd2.zt='1'  and zd2.zddm!='999999999' 
left join [NewtouchHIS_Base].[dbo].[Sys_Department] ks on ks.code=zyxx.ks and ks.OrganizeId=zyxx.OrganizeId
where brxx.zyh=@zyh  and brxx.OrganizeId=@orgId
            ");
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@zyh", zyh));
            return this.FirstOrDefault<PatientInformationDTO>(sqlstr.ToString(), par.ToArray());
        }
        public DrugProjectDTO queryypandxm(string xmdm, int yzlx, string orgId)
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            if (yzlx == 2)
            {
                sqlstr = sqlstr.Append(@"
               select Convert(decimal(18,3),(xtyp.lsj/xtyp.bzs),3) dj,gjybdm from [NewtouchHIS_Base].[dbo].[V_C_xt_yp] xtyp
where xtyp.ypCode=@xmdm and xtyp.OrganizeId=@orgId and xtyp.zt='1'
            ");
            }
            else
            {
                sqlstr = sqlstr.Append(@"
               select Convert(decimal(18,3),dj,3) dj,gjybdm from [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] sfxm
where sfxm.sfxmcode=@xmdm and sfxm.OrganizeId=@orgId  and sfxm.zt='1'
            ");

            }
            par.Add(new SqlParameter("@orgId", orgId));
            par.Add(new SqlParameter("@xmdm", xmdm));
            return this.FirstOrDefault<DrugProjectDTO>(sqlstr.ToString(), par.ToArray());
        }
        public string pczd(string pcmc)//频次字典
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"
            select 
case pc.yzpcmc when 'qd' then '11'
when 'bid' then '12'
when 'tid' then '13'
when 'st' then '61'
when 'Q6H' then '35'
when 'Q8h' then '36'
when 'QID' then '14'
when 'QN' then '41'
when 'Q2h' then '32'
when 'PRN' then '62'
when 'QOD' then '42'
when 'QW' then '21'
when 'BIW' then '22'
when 'TIW' then '23'
when 'QOW' then '-1'
when '2W' then '215'
when '3W' then '216'
when '4W' then '217'
when 'Q1/2H' then '30'
when 'QH' then '31'
when 'Q3H' then '-1'
when 'Q4H' then '33'
when 'Q12H' then '37'
when '摆药' then '-1'
else '-1'
 end ybpc
 from [NewtouchHIS_Base].[dbo].V_S_xt_yzpc pc
where pc.yzpcmc=@pcmc
            ");
            par.Add(new SqlParameter("@pcmc", pcmc));
            return this.FirstOrDefault<string>(sqlstr.ToString(), par.ToArray());
        }
        public string jytj(string ypyfmc)//药品用法字典
        {
            var sqlstr = new StringBuilder();
            var par = new List<SqlParameter>();
            sqlstr = sqlstr.Append(@"
select idl.code from [NewtouchHIS_Base].[dbo].[Sys_Items] items
left join[NewtouchHIS_Base].[dbo].sys_itemsdetail idl
on items.Id= idl.ItemId
where items.Code= 'jytj'
and idl.name=@ypyfmc
            ");
            par.Add(new SqlParameter("@ypyfmc", ypyfmc));
            return this.FirstOrDefault<string>(sqlstr.ToString(), par.ToArray());
        }
        /// <summary>
        /// 事前审核接口查询数据
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="reqdoctorservices"></param>
        /// <param name="brxx"></param>
        /// <param name="rygh"></param>
        /// <param name="username"></param>
        /// <param name="HospitalCode"></param>
        /// <param name="HospitalName"></param>
        /// <returns></returns>
        public string GetPriorReviewData(string orgId, List<DoctorServiceRequestDto> reqdoctorservices, InpatientInfo brxx, string rygh, string username, string HospitalCode, string HospitalName, string yzcfh, string GetMAC)
        {
            try
            {

                var yzh = "";
                yzh = yzcfh;
                var zyh = reqdoctorservices[0].zyh;
                var patientobj = _inpatientPatientInfoRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                var zlysobj = _inpatientpatientdoctorRepo.IQueryable().FirstOrDefault(p => p.zyh == zyh && p.OrganizeId == orgId && p.zt == "1");
                PriorReviewDTO pre = new PriorReviewDTO();
                decimal djzje = 0;
                decimal sbzje = 0;
                PatientInformationDTO brxxdata = patinfo(reqdoctorservices[0].zyh, orgId);
                List<ROW> rOWs = new List<ROW>();
                foreach (var item in reqdoctorservices)
                {
                    DrugProjectDTO drugProjectDTO = queryypandxm(item.xmdm, item.yzlx, orgId);
                    djzje += drugProjectDTO.dj * item.sl;
                    if (item.iszzffffff != null && item.iszzffffff == '1')
                    {
                        sbzje += 0;
                    }
                    else
                    {
                        sbzje += drugProjectDTO.dj * item.sl;
                    }
                    var ITEM_TYPE = "";
                    if (item.yzlx == 2)
                    {
                        ITEM_TYPE = "1";
                    }
                    else
                    {
                        ITEM_TYPE = "2";
                    }
                    ROW rOW = new ROW();
                    rOW.HIS_BILL_ID = yzh;
                    rOW.RECIPE_ID = yzh;
                    rOW.HIS_BILL_DETAIL_ID = yzh;
                    rOW.ITEM_TYPE = ITEM_TYPE;
                    rOW.ITEM_CODE = drugProjectDTO.gjybdm;
                    rOW.ITEM_NAME = item.xmmc;
                    rOW.NUMBER = item.sl;
                    rOW.PRICE = drugProjectDTO.dj;
                    rOW.TOTAL_MONEY = djzje;
                    rOW.HOSPITAL_CODE = HospitalCode;
                    rOW.HOSPITAL_NAME = HospitalName;
                    rOW.OFFICE_CODE = brxxdata.kscode;
                    rOW.OFFICE_NAME = brxxdata.ksmc;
                    rOW.DOCTOR_CODE = zlysobj.ysgh;
                    rOW.DOCTOR_NAME = zlysobj.ysmc;
                    rOW.USE_AMOUNT = "";
                    rOW.PACKAGE_UNIT = "";
                    rOW.STD = item.ypgg;
                    rOW.FREQUENCY = pczd(item.pcmc);
                    rOW.USAGE = "";
                    rOW.USE_MEDI_DAYS = item.ts ?? 1;
                    var GIVE_MEDI_WAY = "";
                    if (item.ypyfdm == null || item.yfmc == null || item.yfmc == "" || item.ypyfdm == "")
                    {
                        GIVE_MEDI_WAY = "";
                    }
                    else
                    {
                        GIVE_MEDI_WAY = jytj(item.yfmc);
                        if (GIVE_MEDI_WAY != null && GIVE_MEDI_WAY != "")
                        {
                            GIVE_MEDI_WAY = jytj(item.yfmc);
                        }
                        else
                        { GIVE_MEDI_WAY = "9"; }
                    }
                    rOW.GIVE_MEDI_WAY = GIVE_MEDI_WAY;
                    rOW.IM_MONEY = sbzje;
                    rOW.REAL_NUMBER = item.sl.ToString("F3");
                    rOW.REAL_MONEY = sbzje.ToString("F3");
                    rOW.DOCTOR_LEVEL_CODE = "1";
                    rOW.DOCTOR_DUTY_CODE = "234";
                    rOW.COL1 = "";
                    rOW.COL2 = "";
                    rOW.REMARK = "";
                    rOWs.Add(rOW);
                }
                Domain.DTO.OutputDto.BEAN bEAN = new Domain.DTO.OutputDto.BEAN();
                Domain.DTO.OutputDto.DATA dATA = new Domain.DTO.OutputDto.DATA();
                bEAN.CURRENT_STATUS = "1";//出入院状态:1=在院，2=出院，3=门诊
                bEAN.IS_ADD = "0"; //是否增量。住院单据 第一次是非增量，以后是增量。门诊单据都是非增量。 0表示非增量，1表示增量
                bEAN.BILL_TYPE = "2"; //单据类型。1表示门诊，2表示住院
                bEAN.HIS_BILL_ID = yzh; //医嘱（处方）单ID
                bEAN.END_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");                  //结算时间，住院期间为调用接口服务的系统当前时间。
                bEAN.HOSPITAL_CODE = HospitalCode;   //医院编码（使用医保中心端给医院分配的医院编码）
                bEAN.HOSPITAL_NAME = HospitalName;             //医院名称
                bEAN.PATIENT_CODE = patientobj.sfzh;              //参保人编码。填写参保人身份标识号（身份证号、医保卡号、健康卡号等）。
                bEAN.PATIENT_NAME = patientobj.xm;              //参保人姓名
                bEAN.PATIENT_SEX_CODE = patientobj.sex;          //性别编码
                string csrq = DateTime.Now.ToString("yyyy-MM-dd");
                if (patientobj.birth != null)
                {
                    csrq = DateTime.Parse(Convert.ToString(patientobj.birth)).ToString("yyyy-MM-dd");
                }
                bEAN.PATIENT_BIRTH = csrq;             //出生日期
                bEAN.PERSON_TYPE_CODE = "-1";          //人员类别编码
                bEAN.BENEFIT_TYPE_CODE = brxxdata.xzlx ?? "310";         //险种类型编码
                bEAN.SEE_DOCTOR_TYPE_CODE = brxxdata.yllb;      //医疗类别编码
                bEAN.IS_WITHOUT_PLACE = brxxdata.ydjy;          //是否异地就医，0：否；1：是
                bEAN.WITHOUT_PLACE_PERSON_TYPE = ""; //异地人员类别编码
                bEAN.WITHOUT_PLACE_UNIT_CODE = "";   //异地参保地行政区域代码
                bEAN.IN_HOSPITAL_ICD_CODE = brxxdata.ryzd;      //入院诊断编码，门诊：主要诊断；住院：入院诊断
                bEAN.IN_HOSPITAL_ICD_NAME = brxxdata.ryzdmc;      //入院诊断名称，门诊：主要诊断；住院：入院诊断
                bEAN.OUT_HOSPITAL_ICD_CODE = brxxdata.cyzd;     //出院诊断编码，门诊：次要诊断；住院：出院诊断
                bEAN.OUT_HOSPITAL_ICD_NAME = brxxdata.cyzdmc;     //出院诊断名称，门诊：次要诊断；住院：出院诊断
                bEAN.OUT_HOSPITAL_REASON = brxxdata.cyqk;       //出院原因
                bEAN.ICD1_CODE = "";                 //
                bEAN.ICD1_NAME = "";                 //
                bEAN.ICD2_CODE = "";                 //
                bEAN.ICD2_NAME = "";                 //
                bEAN.ICD3_CODE = "";                 //
                bEAN.ICD3_NAME = "";                 //
                bEAN.ICD4_CODE = "";                 //
                bEAN.ICD4_NAME = "";                 //
                bEAN.ICD5_CODE = "";                 //
                bEAN.ICD5_NAME = "";                 //
                bEAN.ICD6_CODE = "";                 //
                bEAN.ICD6_NAME = "";                 //
                bEAN.ICD7_CODE = "";                 //
                bEAN.ICD7_NAME = "";                 //
                bEAN.ICD8_CODE = "";                 //
                bEAN.ICD8_NAME = "";                 //
                bEAN.ICD9_CODE = "";                 //
                bEAN.ICD9_NAME = "";                 //
                bEAN.ICD10_CODE = "";                //
                bEAN.ICD10_NAME = "";                //
                bEAN.ICD11_CODE = "";                //
                bEAN.ICD11_NAME = "";                //
                bEAN.ICD12_CODE = "";                //
                bEAN.ICD12_NAME = "";                //
                bEAN.ICD13_CODE = "";                //
                bEAN.ICD13_NAME = "";                //
                bEAN.ICD14_CODE = "";                //
                bEAN.ICD14_NAME = "";                //
                bEAN.ICD15_CODE = "";                //
                bEAN.ICD15_NAME = "";                //
                bEAN.ICD16_CODE = "";                //
                bEAN.ICD16_NAME = "";                //
                bEAN.ICD17_CODE = "";                //
                bEAN.ICD17_NAME = "";                //
                bEAN.ICD18_CODE = "";                //
                bEAN.ICD18_NAME = "";                //
                bEAN.ICD19_CODE = "";                //
                bEAN.ICD19_NAME = "";                //
                bEAN.ICD20_CODE = "";                //
                bEAN.ICD20_NAME = "";                //
                bEAN.INPATIENT_NO = zyh;              //住院号（门诊号）
                bEAN.OUT_HOSPITAL_OFFICE = brxxdata.ksmc;       //出院科室（病区）。门诊填写就诊科室名称，住院填写出院科室（病区）名称。填写医院（HIS）系统内部的科室名称
                bEAN.IN_HOSPITAL_TIME = DateTime.Parse(brxxdata.rqrq).ToString("yyyy-MM-dd HH:mm:ss");          //入院时间。门诊为就诊时间，住院为入院时间
                bEAN.OUT_HOSPITAL_TIME = DateTime.Parse(brxxdata.cqrq).ToString("yyyy-MM-dd HH:mm:ss");         //出院时间。门诊为就诊时间，住院为出院时间
                bEAN.SEE_DOCTOR_TIME = DateTime.Parse(brxxdata.rqrq).ToString("yyyy-MM-dd HH:mm:ss");      //就诊时间。门诊为就诊时间，住院为入院时间
                bEAN.HEIGHT = 0;                    //身高(cm)
                bEAN.WEIGHT = 0;                    //体重(kg)
                bEAN.IS_TRAN_IN_HOSPITAL = "0";       //是否转入院0：否；1：是
                bEAN.IS_PREGNANCY = "0";              //是否怀孕0：否；1：是
                bEAN.IS_SUCKLING = "0";               //是否哺乳期0：否；1：是
                bEAN.HIS_BILL_NO = yzh;               //收费单据号
                bEAN.HARD_ILL_FLAG = "0";             //是否特病慢病单据标志0：否；1：是
                bEAN.HARD_ILL_CODE = "";             //慢病特病代码。如果单据为慢病特病单据，需提供该代码或能够从诊断编码找到对应的慢病特病
                bEAN.BILL_TOTAL_MONEY = djzje;          //单据总金额
                bEAN.IM_TOTAL_MONEY = sbzje;            //医保内总金额（申报值，医保需要付给医院的钱，不进行医保报销的主单该金额为0）
                bEAN.COL1 = "";                      //
                bEAN.COL2 = "";                      //
                bEAN.REMARK = "";
                bEAN.ROWS = rOWs;
                Domain.DTO.OutputDto.MESSAGE mESSAGE = new Domain.DTO.OutputDto.MESSAGE();
                mESSAGE.VERSION = "1";
                pre.MESSAGE = mESSAGE;
                dATA.BEAN = bEAN;
                pre.DATA = dATA;
                string responsexml = pre.XmlSerialize().Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                var param = "serviceId{=}AUDIT00001{,}userName{=}admin{,}password{=}adm@123{,}clientMAC{=}";
                var logstr = AuditInterface(responsexml, orgId, zyh, zlysobj.ysgh, zlysobj.ysmc, param, yzh, "医嘱审核", GetMAC);//接口
                return logstr;
            }
            catch (Exception e)
            {

                return "" + e.Message;
            }

        }
        /// <summary>
        /// 病案审核
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="reqdoctorservices"></param>
        /// <param name="brxx"></param>
        /// <param name="rygh"></param>
        /// <param name="username"></param>
        /// <param name="HospitalCode"></param>
        /// <param name="HospitalName"></param>
        /// <returns></returns>
        public string GetBashData(string orgId, string zyh, string rygh, string username, string GetMAC)
        {
            try
            {
                string HospitalCode = ConfigurationManager.AppSettings["OrganizeCodeSd"];
                string HospitalName = ConfigurationManager.AppSettings["HospitalName"];
                var sqlpar = new List<SqlParameter>();
                var sql = @"exec usp_zy_QueryBash @orgId,@zyh,@cryzt,@yljgdm,@yljgmc";
                sqlpar.Add(new SqlParameter("@orgId", orgId));
                sqlpar.Add(new SqlParameter("@zyh", zyh));
                sqlpar.Add(new SqlParameter("@cryzt", "1"));
                sqlpar.Add(new SqlParameter("@yljgdm", HospitalCode));
                sqlpar.Add(new SqlParameter("@yljgmc", HospitalName));
                MedicalBEAN me = FirstOrDefault<MedicalBEAN>(sql.ToString(), sqlpar.ToArray());
                if (me == null)
                {
                    return "";
                }
                MedicalDTO medicalDTO = new MedicalDTO();
                MedicalMESSAGE mESSAGE = new MedicalMESSAGE();
                mESSAGE.VERSION = "1";
                medicalDTO.MESSAGE = mESSAGE;
                MedicalDATA dATA = new MedicalDATA();
                dATA.BEAN = me;
                medicalDTO.DATA = dATA;
                string responsexml = medicalDTO.XmlSerialize().Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                var ysgh = "000000";
                var ysmc = "管理员";
                var param = "serviceId{=}MR00001{,}userName{=}admin{,}password{=}adm@123{,}clientMAC{=}";
                var logstr = AuditInterface(responsexml, orgId, zyh, ysgh, ysmc, param, "", "病案审核", GetMAC);//接口
                return logstr;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        /// <summary>
        /// DIP接口
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="reqdoctorservices"></param>
        /// <param name="brxx"></param>
        /// <param name="rygh"></param>
        /// <param name="username"></param>
        /// <param name="HospitalCode"></param>
        /// <param name="HospitalName"></param>
        /// <returns></returns>
        public string GetDrgData(string orgId, string zyh, string rygh, string username, string GetMAC)
        {
            try
            {
                //DIP接口住院基本信息
                string HospitalCode = ConfigurationManager.AppSettings["OrganizeCodeSd"];
                string HospitalName = ConfigurationManager.AppSettings["HospitalName"];
                var jbxxsqlpar = new List<SqlParameter>();
                var JBXXsql = @"exec usp_zy_QueryDrg_JBXX @orgId,@zyh,@yljgdm,@yljgmc";
                jbxxsqlpar.Add(new SqlParameter("@orgId", orgId));
                jbxxsqlpar.Add(new SqlParameter("@zyh", zyh));
                jbxxsqlpar.Add(new SqlParameter("@yljgdm", HospitalCode));
                jbxxsqlpar.Add(new SqlParameter("@yljgmc", HospitalName));
                DRGDIPJBXX dRGDIPJBXX = FirstOrDefault<DRGDIPJBXX>(JBXXsql.ToString(), jbxxsqlpar.ToArray());
                //DIP接口住院信息
                var zyxxsqlpar = new List<SqlParameter>();
                var zyxxsql = @"exec usp_zy_QueryDrg_ZYXX @orgId,@zyh";
                zyxxsqlpar.Add(new SqlParameter("@orgId", orgId));
                zyxxsqlpar.Add(new SqlParameter("@zyh", zyh));
                DRGDIPZYXX dRGDIPZyxx = FirstOrDefault<DRGDIPZYXX>(zyxxsql.ToString(), zyxxsqlpar.ToArray());
                //DIP接口收费信息
                var sfxxsqlpar = new List<SqlParameter>();
                var sfxxsql = @"exec usp_zy_QueryDrg_SFXX @orgId,@zyh";
                sfxxsqlpar.Add(new SqlParameter("@orgId", orgId));
                sfxxsqlpar.Add(new SqlParameter("@zyh", zyh));
                sfxxsqlpar.Add(new SqlParameter("@yljgdm", HospitalCode));
                sfxxsqlpar.Add(new SqlParameter("@yljgmc", HospitalName));
                DRGDIPSFXX dRGDIPSFXX = FirstOrDefault<DRGDIPSFXX>(sfxxsql.ToString(), sfxxsqlpar.ToArray());
                //DIP接口其他信息
                DRGDIPQTXX dRGDIPQTXX = new DRGDIPQTXX();
                dRGDIPQTXX.DDYLJG_TBBM_BM = dRGDIPZyxx.CYKB_BM;
                dRGDIPQTXX.DDYLJG_TBBM_MC = dRGDIPZyxx.CYKB_MC;
                dRGDIPQTXX.DDYLJG_TBR_BM = rygh;
                dRGDIPQTXX.DDYLJG_TBR_MC = username;
                dRGDIPQTXX.YBJBJG_BM = HospitalCode;
                dRGDIPQTXX.YBJBJG_MC = HospitalName;
                dRGDIPQTXX.YBJBJG_JBR_BM = "-";
                dRGDIPQTXX.YBJBJG_JBR_MC = "-";
                DRGDIPBEAN me = new DRGDIPBEAN();
                me.JBXX = dRGDIPJBXX;
                me.ZYXX = dRGDIPZyxx;
                me.SFXX = dRGDIPSFXX;
                me.QTXX = dRGDIPQTXX;
                DRGDIPDTO medicalDTO = new DRGDIPDTO();
                DRGDIPMESSAGE mESSAGE = new DRGDIPMESSAGE();
                mESSAGE.VERSION = "1";
                medicalDTO.MESSAGE = mESSAGE;
                DRGDIPDATA dATA = new DRGDIPDATA();
                dATA.BEAN = me;
                medicalDTO.DATA = dATA;
                string responsexml = medicalDTO.XmlSerialize().Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                var ysgh = "000000";
                var ysmc = "管理员";
                var param = "serviceId{=}AUDIT00101{,}userName{=}admin{,}password{=}adm@123{,}clientMAC{=}";
                var logstr = AuditInterface(responsexml, orgId, zyh, ysgh, ysmc, param, "", "DIP接口", GetMAC);//接口
                return logstr;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        /// <summary>
        /// 审核单据删除
        /// </summary>
        /// <returns></returns>
        public string DeletePriorReview(string zyh, string yzid, string yzlx, string OrganizeId, string GetMAC)
        {
            try
            {
                var sqlstr = new StringBuilder();
                var par = new List<SqlParameter>();
                sqlstr = sqlstr.Append(@"select * from (select shlog.outlsh,shlog.yzh,cqyz.Id from [dbo].[shjk_log] shlog
inner join zy_cqyz cqyz on shlog.yzh=cqyz.yzh
 where cqyz.OrganizeId=@orgId
union all
select shlog.outlsh,shlog.yzh,lsyz.Id from [dbo].[shjk_log] shlog
inner join zy_lsyz lsyz on shlog.yzh=lsyz.yzh where lsyz.OrganizeId=@orgId) a
where id=@yzid");
                par.Add(new SqlParameter("@yzid", yzid));
                par.Add(new SqlParameter("@orgId", OrganizeId));
                var delrc = this.FirstOrDefault<DelReviewDTO>(sqlstr.ToString(), par.ToArray());
                if (delrc == null)
                {
                    return "";
                }
                DeletePriorReviewDTO medicalDTO = new DeletePriorReviewDTO();
                DELPRMESSAGE mESSAGE = new DELPRMESSAGE();
                mESSAGE.VERSION = "1";
                medicalDTO.MESSAGE = mESSAGE;
                DELPRDATA dATA = new DELPRDATA();
                DELPRBEAN me = new DELPRBEAN();
                me.BILL_ID = delrc.outlsh;
                me.HIS_BILL_ID = delrc.yzh;
                dATA.BEAN = me;
                medicalDTO.DATA = dATA;
                string responsexml = medicalDTO.XmlSerialize().Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                var ysgh = "000000";
                var ysmc = "管理员";
                var param = "serviceId{=}AUDIT00002{,}userName{=}admin{,}password{=}adm@123{,}clientMAC{=}";
                var logstr = AuditInterface(responsexml, OrganizeId, zyh, ysgh, ysmc, param, "", "单据删除", GetMAC);//接口
                return logstr;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string GetDiagnoseData()
        {
            try
            {
                DiagnoseDTO medicalDTO = new DiagnoseDTO();
                DiagnoseSSAGE mESSAGE = new DiagnoseSSAGE();
                mESSAGE.VERSION = "1";
                medicalDTO.MESSAGE = mESSAGE;
                DiagnoseDATA dATA = new DiagnoseDATA();
                DiagnoseBEAN me = new DiagnoseBEAN();
                me.ICD_CODE = "";
                me.ICD_NAME = "流行性感冒";
                dATA.BEAN = me;
                medicalDTO.DATA = dATA;
                string responsexml = medicalDTO.XmlSerialize().Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                var orgId = "";
                var zyh = "00301";
                var ysgh = "000000";
                var ysmc = "管理员";
                var param = "serviceId{=}ICD00001{,}userName{=}admin{,}password{=}adm@123{,}clientMAC{=}";
                var logstr = AuditInterface(responsexml, orgId, zyh, ysgh, ysmc, param, "", "诊断查询", "");//接口
                return logstr;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string AuditInterface(string responsexml, string OrganizeId, string zyh, string yhgh, string ysmc, string param, string yzh, string jkType, string GetMAC)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                try
                {
                    string macAddresses = null;
                    try
                    {
                        if (GetMAC == null || GetMAC == "")
                        {
                            return "MAC地址获取失败！请启用医保小程序！";
                        }
                        else
                        {
                            macAddresses = GetMAC;
                        }
                    }
                    catch (Exception ex)
                    {
                        return "MAC地址获取失败！请启用医保小程序！" + ex.Message + "";
                    }
                    param += macAddresses;
                    var responseXML = new WebServiceClient().service(param, responsexml);
                    var jsonout = responseXML.XmlDeSerialize<OutXMLDTO>();
                    var outlsh = "";
                    if (yzh != null && yzh != "")
                    {
                        outlsh = jsonout.DATA.BEAN.BILL_ID;
                    }

                    var shjkldrzEntity = new shjkldrzEntity
                    {
                        OrganizeId = OrganizeId,
                        jydm = "5100",
                        mzzyh = zyh,
                        Type = "2",
                        jkType = jkType,
                        yzh = yzh,
                        outlsh = outlsh,
                        param = param,
                        XmlRequest = responsexml,
                        XmlResponse = responseXML,
                        zt = "1",
                    };
                    shjkldrzEntity.Create(true);
                    db.Insert(shjkldrzEntity);
                    db.Commit();
                }
                catch (Exception ex)
                {

                    return ex.Message;
                }
                return "";
            }
        }


        #region 医技科室执行
        /// <summary>
        /// 医技执行
        /// </summary>
        /// <param name="jyjclist"></param>
        /// <param name="orgId"></param>
        /// <param name="zxr"></param>
        public void jyjcExec(List<jyjcExecReq> jyjclist, string orgId, string zxr)
        {
            List<XtjyjcExecEntity> entityList = new List<XtjyjcExecEntity>();
            try
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    DateTime date = DateTime.Now;
                    foreach (var item in jyjclist)
                    {
                        XtjyjcExecEntity entity = new XtjyjcExecEntity();
                        entity.OrganizeId = orgId;
                        entity.mzzyh = item.mzzyh;
                        entity.hzlx = item.hzlx;
                        entity.fylx = item.fylx;
                        entity.xm = item.xm;
                        entity.sqdlx = item.sqdlx;
                        entity.sqdh = item.sqdh;
                        entity.sl = item.sl;
                        entity.dj = item.dj;
                        entity.zxr = zxr;
                        entity.zxrq = date;
                        entity.zxks = item.zxks;
                        entity.kdks = item.kdks;
                        entity.je = item.je;
                        entity.ztId = item.ztId;
                        entity.ztmc = item.ztmc;
                        entity.kdrq = item.kdrq;
                        entity.kdys = item.kdys;
                        entity.dw = item.dw;
                        entity.gg = item.gg;
                        entity.shr = item.shr;
                        entity.Create(true);
                        entityList.Add(entity);
                    }
                    if (entityList != null && entityList.Count > 0)
                    {
                        foreach (var item in entityList)
                        {
                            db.Insert(item);
                        }
                    }

                    db.Commit();
                }
            }
            catch (Exception e)
            {
                throw new FailedException("执行失败，" + e.InnerException);
            }
        }
        /// <summary>
        /// 医技执行修改项目是否计费状态(isjf)
        /// </summary>
        /// <param name="jyjclist"></param>
        /// <param name="orgId"></param>
        /// <param name="czr"></param>
        public void UpdatejyjcExecIsjf(List<string> jyjclist, string orgId, string czr,string status)
        {
            string sql = @"  update zy_fymxk set isjf=@status ,LastModifierCode=@czr,LastModifyTime=getdate()
                                 where yzxh in (select Id from zy_lsyz  with(nolock)  where zt=1 and OrganizeId=@orgId 
                                     and sqdh IN ( SELECT  col FROM dbo.f_split(@str, ',') )) ";
             var d=ExecuteSqlCommand(sql, new[] { new SqlParameter("@orgId", orgId),
                     new SqlParameter("@czr",czr),
                     new SqlParameter("@status",status),
                     new SqlParameter("@str", string.Join(",", jyjclist))});
        }
        /// <summary>
        /// 撤销执行
        /// </summary>
        /// <param name="jyjclist"></param>
        /// <param name="orgId"></param>
        /// <param name="czr"></param>
        public void CancaljyjcExec(List<string> jyjclist, string orgId, string czr)
        {
            string sql = @"  DELETE FROM  xt_jyjcexec 
                                 WHERE zt=1 and OrganizeId=@orgId 
                                       and sqdh IN ( SELECT  col FROM dbo.f_split(@str, ',') ) 

                            DELETE FROM  NewtouchHIS_Sett..zy_xmjfb
                                WHERE yzwym in (select Id from zy_lsyz with(nolock) where zt=1 and OrganizeId=@orgId 
                                 and sqdh IN ( SELECT  col FROM dbo.f_split(@str, ',') ))
";
            var d=ExecuteSqlCommand(sql, new[] { new SqlParameter("@orgId", orgId),
                    new SqlParameter("@str", string.Join(",", jyjclist))});
        }
        #endregion
        /// <summary>
        /// 判断是否是慢性病
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zdcode"></param>
        /// <returns></returns>
        public string GetmxbzdList(string orgId, string zdcode)
        {
            string sql = @" select zdmc from [NewtouchHIS_Base]..xt_zd where mxb='1' and zdcode in (select * from f_split(@zdcode,','))  and zt='1' and OrganizeId=@orgId";
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@zdcode", zdcode));
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            string listjg = this.FirstOrDefault<string>(sql, sqlpar.ToArray());
            return listjg;
        }
        /// <summary>
        /// 开立数量限制
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zdcode"></param>
        /// <returns></returns>
        public string GetxzklsList(string orgId, YpxzkldataDTO Ypxzkldata)
        {
            string message = "";
            foreach (var item in Ypxzkldata.cfdata)
            {
                string sql = @" select xs.dcxl,xs.mbxl,yp.ypmc from  [NewtouchHIS_Base]..xt_ypsx xs
 left join [NewtouchHIS_Base]..xt_yp yp on xs.ypId=yp.ypId and xs.OrganizeId=yp.OrganizeId and xs.zt='1'
 where  xs.ypCode=@ypCode and xs.OrganizeId=@orgId";
                var sqlpar = new List<SqlParameter>();
                sqlpar.Add(new SqlParameter("@ypCode", item.ypCode));
                sqlpar.Add(new SqlParameter("@orgId", orgId));
                var listjg = this.FirstOrDefault<YpxzklDTO>(sql, sqlpar.ToArray());
                if (listjg != null)
                {
                    if (Ypxzkldata.mbzd == "1")
                    {
                        if (listjg.mbxl != null)
                        {
                            if (item.sl > listjg.mbxl)
                            {
                                message += listjg.ypmc + "的慢性病最大开立数为【" + listjg.mbxl + "】";
                            }
                            continue;
                        }
                    }
                    if (listjg.dcxl != null && item.sl > listjg.dcxl)
                    {
                        message += listjg.ypmc + "的最大开立数为【" + listjg.dcxl + "】";
                    }
                }
            }
            return message;
        }
        public string addcqyz(string zyh, string yzh, string zh, List<YzbindingfeeVo> ItemFeeVO, string OrganizeId, string usercode)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                InpatientPatientInfoEntity patInfo = db.IQueryable<InpatientPatientInfoEntity>().Where(a => a.zyh == zyh && a.zt == "1" && a.OrganizeId == OrganizeId).FirstOrDefault();

                int cqpx = 1;
                int lspx = 1;
                foreach (var item in ItemFeeVO)
                {
                    if (item.yzxz == "2")
                    {
                        InpatientLongTermOrderEntity newcqyz = new InpatientLongTermOrderEntity();
                        newcqyz.Id = Guid.NewGuid().ToString();
                        newcqyz.OrganizeId = OrganizeId;
                        newcqyz.zyh = zyh;
                        if (item.yzlx == "1"&& zh!=null& zh!="")
                        {
                            newcqyz.zh = int.Parse(zh);
                        }
                        newcqyz.WardCode = patInfo.WardCode;
                        newcqyz.DeptCode = patInfo.DeptCode;
                        newcqyz.ysgh = patInfo.ysgh;
                        newcqyz.pcCode = item.pcCode;
                        newcqyz.zxcs = item.zxcs;
                        newcqyz.zxzq = item.zxzq;
                        newcqyz.zxzqdw = item.zxzqdw;
                        newcqyz.zdm = null;
                        newcqyz.xmdm = item.sfxm;
                        newcqyz.xmmc = item.sfxmmc;
                        newcqyz.yzzt = 1;
                        newcqyz.dw = item.dw;
                        newcqyz.zbbz = 0;
                        newcqyz.sl = int.Parse(item.sl.ToString());
                        newcqyz.dwlb = 4;
                        newcqyz.yzlx = (int)EnumYzlx.yyhc;
                        //int yzlx = 1;//通过浮层传过来的是1,2 1是药品 2是项目
                        //if (item.yzlx == "1")
                        //{
                        //    yzlx = 2;
                        //}
                        //else
                        //{
                        //    yzlx = 5;
                        //}
                        //newcqyz.yzlx = yzlx;
                        newcqyz.tzysgh = null;
                        newcqyz.tzsj = null;
                        newcqyz.tzr = null;
                        newcqyz.tzyy = null;
                        newcqyz.shsj = DateTime.Now;
                        newcqyz.shr = usercode;
                        newcqyz.kssj = DateTime.Now;
                        newcqyz.zxsj = null;
                        newcqyz.zxr = null;
                        newcqyz.dcr = null;
                        newcqyz.ypjl = item.sl;
                        newcqyz.ypgg = item.gg;
                        newcqyz.ztnr = null;
                        newcqyz.yznr = item.sfxmmc + " " + item.sl.ToString() + item.dw + " " + item.dlmc + " " + item.pcmc;
                        newcqyz.ypyfdm = null;
                        newcqyz.zxksdm = item.yfdm;
                        newcqyz.printyznr = null;
                        newcqyz.CreateTime = DateTime.Now;
                        newcqyz.CreatorCode = usercode;
                        newcqyz.LastModifyTime = null;
                        newcqyz.LastModifierCode = null;
                        newcqyz.zt = "1";
                        newcqyz.hzxm = patInfo.xm;
                        newcqyz.bw = null;
                        newcqyz.zxsjd = null;
                        newcqyz.nlmddm = null;
                        newcqyz.kssReason = null;
                        newcqyz.ds = null;
                        if (yzh != null && yzh != "")
                        {
                            newcqyz.Px = (getmaxpx(yzh, item.yzxz, OrganizeId) + 1);
                            newcqyz.yzh = yzh;
                        }
                        else
                        {
                            var yzhPart1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                            var yzhPart2 = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_cqyz.yzh", OrganizeId);
                            newcqyz.yzh = string.Format("{0}{1}", yzhPart1, yzhPart2);
                        }
                        newcqyz.zzfbz = 0;
                        newcqyz.yztag = null;
                        newcqyz.isjf = 1;
                        newcqyz.zxing = null;
                        newcqyz.ispscs = "0";
                        newcqyz.ztmc = null;
                        newcqyz.yfztbs = null;
                        newcqyz.ztsl = null;
                        newcqyz.dcztbs = null;
                        newcqyz.isfsyz = "1";
                        if (item.yzlx == "1"&& (yzh == null && yzh == ""))
                        {
                            newcqyz.Px = cqpx;
                            cqpx += 1;
                        }
                         _InpatientLongTermOrderRepo.SubmitForm(newcqyz, null);
                    }
                    else
                    {
                        InpatientSTATOrderEntity newlsyz = new InpatientSTATOrderEntity();
                        newlsyz.Id = Guid.NewGuid().ToString();
                        newlsyz.OrganizeId = OrganizeId;
                        newlsyz.zyh = zyh;
                        if (item.yzlx == "1" && zh != null & zh != "")
                        {
                            newlsyz.zh = int.Parse(zh);
                        }
                        newlsyz.WardCode = patInfo.WardCode;
                        newlsyz.DeptCode = patInfo.DeptCode;
                        newlsyz.ysgh = patInfo.ysgh;
                        newlsyz.pcCode = item.pcCode;
                        newlsyz.zxcs = item.zxcs;
                        newlsyz.zxzq = item.zxzq;
                        newlsyz.zxzqdw = item.zxzqdw;
                        newlsyz.zdm = null;
                        newlsyz.xmdm = item.sfxm;
                        newlsyz.xmmc = item.sfxmmc;
                        newlsyz.yzzt = 1;
                        newlsyz.dw = item.dw;
                        newlsyz.zbbz = 0;
                        newlsyz.sl = int.Parse(item.sl.ToString());
                        newlsyz.dwlb = 4;
                        //int yzlx = 1;//通过浮层传过来的是1,2 1是药品 2是项目
                        newlsyz.yzlx = (int)EnumYzlx.yyhc;
                        //if (item.yzlx == "1")
                        //{
                        //    yzlx = 2;
                        //}
                        //else
                        //{
                        //    yzlx = 5;
                        //}
                        //newlsyz.yzlx = yzlx;
                        newlsyz.shsj = DateTime.Now;
                        newlsyz.shr = usercode;
                        newlsyz.kssj = DateTime.Now;
                        newlsyz.zxsj = null;
                        newlsyz.zxr = null;
                        newlsyz.ypjl = item.sl;
                        newlsyz.ypgg = item.gg;
                        newlsyz.ztnr = null;
                        newlsyz.yznr = item.sfxmmc + " " + item.sl.ToString() + item.dw + " " + item.dlmc + " " + item.pcmc;
                        newlsyz.ypyfdm = null;
                        newlsyz.zxksdm = item.yfdm;
                        newlsyz.printyznr = null;
                        newlsyz.CreateTime = DateTime.Now;
                        newlsyz.CreatorCode = usercode;
                        newlsyz.LastModifyTime = null;
                        newlsyz.LastModifierCode = null;
                        newlsyz.zt = "1";
                        newlsyz.hzxm = patInfo.xm;
                        newlsyz.bw = null;
                        newlsyz.zxsjd = null;
                        newlsyz.nlmddm = null;
                        newlsyz.kssReason = null;
                        newlsyz.ds = null;
                        if (yzh != null && yzh != "")
                        {
                            newlsyz.Px = (getmaxpx(yzh, item.yzxz, OrganizeId) + 1);
                            newlsyz.yzh = yzh;
                        }
                        else
                        {

                            var yzhPart1 = DateTime.Now.ToString("yyyyMMddHHmmss");
                            var yzhPart2 = EFDBBaseFuncHelper.Instance.GetNewFieldUniqueValue("zy_lsyz.yzh", OrganizeId);
                            newlsyz.yzh = string.Format("{0}{1}", yzhPart1, yzhPart2);
                        }
                        newlsyz.zzfbz = 0;
                        newlsyz.yztag = null;
                        newlsyz.isjf = 1;
                        newlsyz.zxing = null;
                        newlsyz.ispscs = "0";
                        newlsyz.ztmc = null;
                        newlsyz.yfztbs = null;
                        newlsyz.ztsl = null;
                        newlsyz.dcztbs = null;
                        newlsyz.isfsyz = "1";
                        if (item.yzlx == "1"&&( yzh == null || yzh == ""))
                        {
                            newlsyz.Px = lspx;
                            lspx += 1;
                        }
                        _inpatientSTATOrderRepo.SubmitForm(newlsyz, null);
                    }
                }
                db.Commit();
            }
            return "";
        }
        public int getmaxpx(string yzh,string yzxz,string orgId) {
            string sql = @" select isnull(max(px),0) px from zy_lsyz where yzh=@yzh and zt='1' and OrganizeId=@orgId ";
            if (yzxz=="2")
            {
                sql = @" select isnull(max(px),0) px from zy_cqyz where yzh=@yzh and zt='1' and OrganizeId=@orgId ";
            }
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@yzh", yzh));
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            return this.FirstOrDefault<int>(sql, sqlpar.ToArray());
        }
        public PatientMedicalDTO GetlsorcqyzData(string zyh, string yzid, string orgId)
        {
            string sql = @" 		select * from (select xmmc,'2' yzxz,zh,yzh,brxx.xm hzxm,brxx.BedCode bedCode,brxx.DeptCode,yz.px from 
		[Newtouch_CIS].[dbo].zy_brxxk brxx
	left join [Newtouch_CIS].[dbo].[zy_cqyz] yz on brxx.zyh=yz.zyh and yz.OrganizeId=brxx.OrganizeId
	 where (yz.id=@yzid or @yzid='')  and yz.OrganizeId=@orgId and brxx.zyh=@zyh
	union all
  select xmmc,'1' yzxz,zh,yzh,brxx.xm hzxm,brxx.BedCode bedCode,brxx.DeptCode,yz.px from 
		[Newtouch_CIS].[dbo].zy_brxxk brxx
	left join [Newtouch_CIS].[dbo].[zy_lsyz] yz on brxx.zyh=yz.zyh and yz.OrganizeId=brxx.OrganizeId
	 where (yz.id=@yzid or @yzid='')  and yz.OrganizeId=@orgId and brxx.zyh=@zyh
	 ) a  order by a.px desc";
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@zyh", zyh));
            sqlpar.Add(new SqlParameter("@yzid", yzid));
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            return this.FirstOrDefault<PatientMedicalDTO>(sql, sqlpar.ToArray());
        }
        public string DeleteBind(string zyh, string yzid, string yzxz, string orgId)
        {
            try
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {

                    if (yzxz == "1")//临时医嘱
                    {
                        InpatientSTATOrderEntity lsyzdata = db.IQueryable<InpatientSTATOrderEntity>().Where(a => a.zyh == zyh && a.zt == "1" && a.Id == yzid && a.OrganizeId == orgId&&a.isfsyz != null).FirstOrDefault();
                        lsyzdata.zt = "0";
                        _inpatientSTATOrderRepo.SubmitForm(lsyzdata, yzid);
                    }
                    else
                    {
                        InpatientLongTermOrderEntity cqyzdata = db.IQueryable<InpatientLongTermOrderEntity>().Where(a => a.zyh == zyh && a.zt == "1" && a.Id == yzid && a.OrganizeId == orgId && a.isfsyz != null).FirstOrDefault();
                        cqyzdata.yzzt = Convert.ToInt32(EnumYzzt.TZ);
                        _InpatientLongTermOrderRepo.SubmitForm(cqyzdata, yzid);
                    }
                    db.Commit();
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
            return "";
        }



        /// <summary>
        /// 根据住院号获取LIS/PACS报告完成数量
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="yzh"></param>
        /// <returns></returns>
        public int CountLISztzy(string orgId, string zyh)
        {
            string sql = @" 
				select count(*)  from [Newtouch_CIS].[dbo].[zy_lsyz] where zyh =@zyh and syncStatus=2  and organizeId=@orgId and zt='1'";
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@zyh", zyh));
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            return this.FirstOrDefault<int>(sql, sqlpar.ToArray());
        }

        public List<ypyfdataDto> GetYfData(string orgId)
        {
            string sql = @" 
				select yfbmCode,yfbmmc,mzzybz from NewtouchHIS_Base..xt_yfbm where organizeId=@orgId and zt='1' and fybz='1' ";
            var sqlpar = new List<SqlParameter>();
            sqlpar.Add(new SqlParameter("@orgId", orgId));
            return FindList<ypyfdataDto>(sql, sqlpar.ToArray());
        }
    }
}

