using System;
using System.Collections.Generic;
using System.Linq;
using Newtouch.Application.Interface.Inpatient;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO.InputDto.Inpatient;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.IRepository;
using Newtouch.Infrastructure;

namespace Newtouch.Application.Implementation.Inpatient
{
    public class DoctorserviceApp : IDoctorserviceApp
    {
        private readonly IInpatientPatientDmnService _inpatientPatientDmnService;
        private readonly IDoctorserviceDmnService _iDoctorserviceDmnService;
        private readonly IInpatientPatientInfoRepo _inpatientPatientInfoRepo;
        private readonly IInpatientLongTermOrderRepo _inpatientLongTermOrderRepo;
        private readonly IInpatientSTATOrderRepo _inpatientSTATOrderRepo;
        private readonly IInpatientDietDetailSplitRepo _InpatientDietDetailSplitRepo;
        private readonly IInpatientDietBaseRepo _inpatientDietBaseRepo;

        /// <summary>
        /// 
        /// </summary>
        public DoctorserviceApp(IInpatientPatientDmnService inpatientPatientDmnService
            , IDoctorserviceDmnService iDoctorserviceDmnService,
            IInpatientPatientInfoRepo inpatientPatientInfoRepo,
            IInpatientLongTermOrderRepo inpatientLongTermOrderRepo,
            IInpatientSTATOrderRepo _inpatientSTATOrderRepo,
            IInpatientDietDetailSplitRepo inpatientDietDetailSplitRepo,
            IInpatientDietBaseRepo inpatientDietBaseRepo)
        {
            this._inpatientPatientDmnService = inpatientPatientDmnService;
            this._iDoctorserviceDmnService = iDoctorserviceDmnService;
            this._inpatientPatientInfoRepo = inpatientPatientInfoRepo;
            this._inpatientLongTermOrderRepo = inpatientLongTermOrderRepo;
            this._inpatientSTATOrderRepo = _inpatientSTATOrderRepo;
            this._InpatientDietDetailSplitRepo = inpatientDietDetailSplitRepo;
            this._inpatientDietBaseRepo = inpatientDietBaseRepo;
        }

        /// <summary>
        /// 修改药品医嘱时，根据医嘱Id获取详情并且展示到医嘱录入界面
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="yzId"></param>
        /// <param name="yzlx"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public DoctorServiceparentRequestDto GetYZDetail(string zyh, string yzId, string yzlx, string orgId)
        {
            if (string.IsNullOrWhiteSpace(zyh))
            {
                throw new FailedException("住院号不存在");
            }
            var patientInfo = _inpatientPatientDmnService.GetInfoByZyh(zyh, orgId);
            if (patientInfo == null)
            {
                throw new FailedException("病人不存在");
            }
            var response = new DoctorServiceparentRequestDto
            {
                patientInfo = patientInfo
                // patientInfo = new patientInfoDto
                // {
                //     age = patientInfo.age,
                //     brxzmc = patientInfo.brxzmc,
                //     sex = patientInfo.sex,
                //     zyh = patientInfo.zyh
                // }
                ,
                DoctorServiceUIRequestDto = new List<DoctorServiceUIRequestDto>()
            };
            if (yzlx == "长")
            {
                var cqList = new List<InpatientLongTermOrderEntity>();
                var entity = _inpatientLongTermOrderRepo.FindEntity(p =>
                    p.Id == yzId && p.zt == "1" && p.yzzt == (int) EnumYzzt.Ds && p.OrganizeId == orgId);
                if (entity == null)
                {
                    throw new FailedException("不存在该条医嘱，请核实");
                }

                if (!string.IsNullOrWhiteSpace(entity.zh.ToString()))
                {
                    var zhEntity = _inpatientLongTermOrderRepo.IQueryable().Where(p =>
                        p.zh == entity.zh && p.zt == "1" && p.yzzt == (int) EnumYzzt.Ds && p.OrganizeId == orgId);
                    if (zhEntity.Count() > 1)
                    {
                        cqList.AddRange(zhEntity);
                    }
                    else
                    {
                        entity.zh = null;
                        cqList.Add(entity);
                    }
                }
                else
                {
                    cqList.Add(entity);
                }

                if (cqList.Count > 0)
                {
                    foreach (var item in cqList)
                    {
                        string pcmc = "", yfmc = "", redundantJldw = "", jx = "", qzfs = "";
                        decimal? jlzhxs = null, zyzhxs = null;
                        int? ts = null;
                        _iDoctorserviceDmnService.GetypPredata(item.pcCode, item.ypyfdm, item.xmdm,
                            item.dwlb.ToString(), orgId, ref pcmc, ref yfmc, ref redundantJldw, ref jx, ref jlzhxs,
                            ref zyzhxs, ref qzfs);
                        int? dwjls = null; decimal? dj = null;
                        string zxksmc = string.Empty;
                        if (item.yzlx == (int) EnumYzlx.sfxm|| item.yzlx == (int)EnumYzlx.yyhc|| item.yzlx == (int)EnumYzlx.rehab)
                        {
                            _iDoctorserviceDmnService.GetdwjlsBysfxmCode(item.xmdm, orgId, ref dwjls,ref dj);
                        }
                        if (item.yzlx==(int) EnumYzlx.rehab|| item.yzlx == (int)EnumYzlx.sfxm|| item.yzlx == (int)EnumYzlx.yyhc)
                        {
                            _iDoctorserviceDmnService.Getzxksmc(item.zxksdm, orgId, ref zxksmc);
                        }

                        //获取单位计量数
                        //根据停止时间计算天数
                        if (item.tzsj != DateTime.MinValue &&
                            item.tzsj != DateTime.MaxValue &&
                            item.tzsj != null &&
                            item.kssj != DateTime.MinValue
                            && item.tzsj != DateTime.MaxValue &&
                            item.kssj != null)
                        {
                            ts = DateTime.Parse(item.tzsj.ToString()).Subtract(item.kssj)
                                .Days; //(item.tzsj - item.kssj).day;
                        }

                        var repentity = new DoctorServiceUIRequestDto
                        {
                            Id = item.Id,
                            yzh = item.yzh,
                            DeptCode = item.DeptCode,
                            dwlb = item.dwlb,
                            dwwwwwww = item.dw,
                            hzxm = item.hzxm,
                            kssj = item.kssj.ToString("yyyy-MM-dd HH:mm:ss"),
                            WardCode = item.WardCode,
                            xmdm = item.xmdm,
                            xmmc = item.xmmc,
                            yfmcval = yfmc,
                            ypgg = item.ypgg,
                            ypjl = item.ypjl,
                            ypyfdm = item.ypyfdm,
                            zbbzzzzzzz = item.zbbz,
                            zdm = item.zdm,
                            zh = item.zh,
                            ztnr = item.ztnr,
                            pcmc = pcmc,
                            pcCode = item.pcCode,
                            zxcs = item.zxcs,
                            zxzq = item.zxzq,
                            zxzqdw = item.zxzqdw,
                            redundant_jldw = redundantJldw,
                            zyh = zyh,
                            ts = ts,
                            jxCode = jx,
                            sl = item.sl,
                            jlzhxs = Convert.ToDecimal(jlzhxs),
                            zyzhxs = Convert.ToDecimal(zyzhxs),
                            zydw = item.dwlb == (int) EnumYPdwlb.Zydw ? item.dw : redundantJldw,
                            yzlx = item.yzlx,
                            qzfs = qzfs,
                            zxksdm = item.zxksdm,
                            nlmddm = item.nlmddm,
                            dwjls = dwjls,
                            yzlb = yzlx,
                            ds = item.ds,
                            zxksmc=zxksmc,
                            isjf = item.isjf,
                            isjffffff = item.isjf,
                            iszzf = item.zzfbz,
                            iszzffffff = item.zzfbz,
                            ispscs = item.ispscs,
                            ispscsfffff = string.IsNullOrWhiteSpace(item.ispscs) == true ? 0 : Convert.ToInt32(item.ispscs),
                            yztag = item.yztag,
                            dj=dj
                        };

                        //膳食医嘱逻辑添加
                        string yslb;
                        string yslbdm;
                        string yszs;
                        string yszsval;
                        GetssyzDetail(item.Id, orgId, out yslb, out yslbdm, out yszs, out yszsval);
                        repentity.yslbdm = yslbdm;
                        repentity.yslb = yslb;
                        repentity.yszs = yszs;
                        repentity.yszsval = yszsval;
                        repentity.ssxhdm = repentity.xmdm;
                        repentity.ssxhval = repentity.xmmc;
                        repentity.ssxh = repentity.xmmc;
                        response.DoctorServiceUIRequestDto.Add(repentity);
                    }
                }
            }
            else if (yzlx == "临")
            {
                var lsList = new List<InpatientSTATOrderEntity>();
                var entity =
                    _inpatientSTATOrderRepo.FindEntity(p => p.Id == yzId && p.zt == "1" && p.yzzt == (int) EnumYzzt.Ds);
                if (entity == null)
                {
                    throw new FailedException("不存在该条医嘱，请核实");
                }

                if (!string.IsNullOrWhiteSpace(entity.zh.ToString()))
                {
                    var zhEntity = _inpatientSTATOrderRepo.IQueryable().Where(p =>
                        p.zh == entity.zh && p.zt == "1" && p.yzzt == (int) EnumYzzt.Ds);
                    if (zhEntity.Count() > 1)
                    {
                        lsList.AddRange(zhEntity);
                    }
                }
                else
                {
                    lsList.Add(entity);
                }

                if (!lsList.Any()) return response;
                foreach (var item in lsList)
                {
                    string pcmc = "", yfmc = "", redundantJldw = "", jx = "", qzfs = "";
                    decimal? jlzhxs = null, zyzhxs = null;
                    _iDoctorserviceDmnService.GetypPredata(item.pcCode, item.ypyfdm, item.xmdm,
                        item.dwlb.ToString(), orgId, ref pcmc, ref yfmc, ref redundantJldw, ref jx, ref jlzhxs,
                        ref zyzhxs, ref qzfs);
                    int? dwjls = null; decimal? dj = null;
                    string zxksmc = string.Empty;
                    if (item.yzlx == (int) EnumYzlx.sfxm)
                    {
                        _iDoctorserviceDmnService.GetdwjlsBysfxmCode(item.xmdm, orgId, ref dwjls,ref dj);
                    }
                    if (item.yzlx == (int)EnumYzlx.rehab)
                    {
                        _iDoctorserviceDmnService.Getzxksmc(item.zxksdm, orgId, ref zxksmc);
                    }
                    var repentity = new DoctorServiceUIRequestDto
                    {
                        Id = item.Id,
                        yzh = item.yzh,
                        DeptCode = item.DeptCode,
                        dwlb = item.dwlb,
                        dwwwwwww = item.dw,
                        hzxm = item.hzxm,
                        kssj = item.kssj.ToString("yyyy-MM-dd HH:mm:ss"),
                        pcCode = item.pcCode,
                        WardCode = item.WardCode,
                        xmdm = item.xmdm,
                        xmmc = item.xmmc,
                        yfmcval = yfmc,
                        ypgg = item.ypgg,
                        ypjl = item.ypjl,
                        ypyfdm = item.ypyfdm,
                        zbbzzzzzzz = item.zbbz,
                        zdm = item.zdm,
                        zh = item.zh,
                        ztnr = item.ztnr,
                        pcmc = pcmc,
                        redundant_jldw = redundantJldw,
                        zyh = zyh,
                        jxCode = jx,
                        sl = item.sl,
                        jlzhxs = Convert.ToDecimal(jlzhxs),
                        zyzhxs = Convert.ToDecimal(zyzhxs),
                        zxcs = item.zxcs,
                        zxzq = item.zxzq,
                        zxzqdw = item.zxzqdw,
                        zydw = item.dwlb == (int)EnumYPdwlb.Zydw ? item.dw : redundantJldw,
                        yzlx = item.yzlx,
                        qzfs = qzfs,
                        zxksdm = item.zxksdm,
                        nlmddm = item.nlmddm,
                        kssReason = item.kssReason,
                        dwjls = dwjls,
                        yzlb = yzlx,
                        ds = item.ds,
                        zxksmc = zxksmc,
                        isjf = item.isjf,
                        isjffffff = item.isjf,
                        iszzf= item.zzfbz,
                        iszzffffff = item.zzfbz,
                        ispscs = item.ispscs,
                        ispscsfffff=string.IsNullOrWhiteSpace(item.ispscs)==true?0:Convert.ToInt32(item.ispscs),
                        yztag = item.yztag,
                        dj=dj
                    };
                    //膳食医嘱逻辑添加
                    string yslb;
                    string yslbdm;
                    string yszs;
                    string yszsval;
                    GetssyzDetail(item.Id, orgId, out yslb, out yslbdm, out yszs, out yszsval);
                    repentity.yslbdm = yslbdm;
                    repentity.yslb = yslb;
                    repentity.yszs = yszs;
                    repentity.yszsval = yszsval;
                    repentity.ssxhdm = repentity.xmdm;
                    repentity.ssxhval = repentity.xmmc;
                    repentity.ssxh = repentity.xmmc;
                    repentity.sqdh = item.sqdh;
                    response.DoctorServiceUIRequestDto.Add(repentity);
                }
            }

            return response;
        }

        public void GetssyzDetail(string yzId, string orgId, out string yslb, out string yslbdm, out string yszs, out string yszsval)
        {
            yslb = "";
            yslbdm = "";
            yszs = "";
            yszsval = "";
            var entity = _InpatientDietDetailSplitRepo.IQueryable().FirstOrDefault(p => p.MainId == yzId && p.zt == "1" && p.OrganizeId == orgId);
            if (entity == null)
            {
                return;
            }
            var entityList = _InpatientDietDetailSplitRepo.IQueryable().Where(p => p.MainId == yzId && p.zt == "1" && p.OrganizeId == orgId);
            if (entityList != null && entityList.Count() > 0)
            {
                foreach (var item in entityList)
                {
                    yszsval += item.BaseId + ",";
                    yszs += item.Name + ",";
                }
                yszsval = yszsval.Substring(0, yszsval.Length - 1);
                yszs = yszs.Substring(0, yszs.Length - 1);
                var sslbdm = entityList.FirstOrDefault().sslb;
                yslbdm = sslbdm;
                var yslbentity = _inpatientDietBaseRepo.IQueryable().FirstOrDefault(p => p.Id == sslbdm && p.zt == "1" && p.OrganizeId == orgId);

                if (yslbentity != null)
                {
                    yslb = yslbentity.Name;
                }
            }
        }
    }
}
