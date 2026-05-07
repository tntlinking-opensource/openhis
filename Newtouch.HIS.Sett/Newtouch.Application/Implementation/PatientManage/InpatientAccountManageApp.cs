using FrameworkBase.MultiOrg.Application;
using FrameworkBase.MultiOrg.Domain.IRepository;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Application.Interface.PatientManage;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.OutputDto;
using Newtouch.HIS.Domain.DTO.OutputDto.HospitalizationManage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.PatientManage;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IDomainServices.PatientManage;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.IRepository.PatientManage;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.HospitalizationManage;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Newtouch.HIS.Application.Implementation.PatientManage
{
    public class InpatientAccountManageApp : AppBase, IInpatientAccountManageApp
    {

        private readonly IInpatientAccountRepo _inpatientAccountRepo;
        private readonly IHospPatientBasicInfoRepo _hospPatientBasicInfoRepo;
        private readonly IInpatientReserveDmnService _InpatientReserveDmnService;
        private readonly IFinanceReceiptRepo _finRecRepos; //收据凭证号记录
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysPatientBasicInfoRepo _sysPatientBasicInfoRepo;
        private readonly IPatientBasicInfoDmnService _PatientBasicInfoDmnService;

        /// <summary>
        /// 查询患者基本信息和账户信息
        /// </summary>
        /// <param name="patid"></param>
        /// <returns></returns>
        public InpatientReserveDto GetHosPatAccInfo(string zyh)
        {
            if (string.IsNullOrEmpty(zyh.ToString()))
            {
                throw new FailedException("病人内码不存在");
            }
            //在基本信息表中查询
            var e = _hospPatientBasicInfoRepo.FindEntity(p =>
                 p.zyh == zyh && p.OrganizeId == OrganizeId && p.zt == "1");
            if (e == null)
            {
                throw new FailedException("病人不存在");
            }
            var patInfo = e.MapperTo<HospPatientBasicInfoEntity, InpatientReservePatientInfoDto>();
            var zhentity = _inpatientAccountRepo.FindEntity(p =>
               p.zyh == zyh && p.OrganizeId == OrganizeId && p.zt == "1");
            if (zhentity == null)
            {
                throw new FailedException("缺少账户");
            }
            var hospatDto = new InpatientReserveDto();
            patInfo.zhCode = zhentity.zhCode;
            hospatDto.patInfo = patInfo;
            hospatDto.accPayInfoList = GetPayList(zhentity.zhCode, null);
            return hospatDto;
        }



        /// <summary>
        /// 查询患者基本信息和账户信息
        /// </summary>
        /// <param name="patid"></param>
        /// <returns></returns>
        public InpatientReserveDto GetHosPatAccInfo(string zyh, string zhxz)
        {
            if (string.IsNullOrEmpty(zyh))
            {
                throw new FailedException("住院号不存在");
            }
            //在基本信息表中查询
            //OutpatAccInfoDto e = _patientBasicInfoDmnService.GetPatInfoByPatid(patid, OrganizeId);

            var e = _hospPatientBasicInfoRepo.FindEntity(p =>
                 p.zyh == zyh && p.OrganizeId == OrganizeId && p.zt == "1");
            if (e == null)
            {
                throw new FailedException("病人不存在");
            }

            int? xz = null;
            if (!string.IsNullOrWhiteSpace(zhxz))
            {
                xz = Convert.ToInt16(zhxz);
            }
            //var patInfo = e.MapperTo<OutpatAccInfoDto, ReservePatientInfoDto>();
            var patInfo = e.MapperTo<HospPatientBasicInfoEntity,InpatientReservePatientInfoDto>();
            var xtbrjbxx=_sysPatientBasicInfoRepo.FindEntity(p =>
                p.patid == patInfo.patid && p.OrganizeId == OrganizeId && p.zt == "1");
            var zhentity = _inpatientAccountRepo.FindEntity(p =>
                p.zyh == zyh && p.zhxz == xz && p.OrganizeId == OrganizeId && p.zt == "1");
            if (zhentity == null)
            {
                throw new FailedException("缺少账户");
            }
            var hospatDto = new InpatientReserveDto();
            patInfo.zhCode = zhentity.zhCode;
            patInfo.zhxz = zhentity.zhxz;
            patInfo.phone=xtbrjbxx.phone;
            hospatDto.patInfo = patInfo;
            hospatDto.accPayInfoList = GetPayList(zhentity.zhCode, zhxz);
            return hospatDto;
        }
        

        /// <summary>
        /// 获取患者账户收支记录
        /// </summary>
        /// <returns></returns>
        public List<InpatientPatAccPayVO> GetPayList(int zh, string zhxz)
        {
            List<InpatientPatAccPayVO> hospayList = _InpatientReserveDmnService.GetAccPayInfo(zh, this.OrganizeId, zhxz);
            foreach (InpatientPatAccPayVO item in hospayList)
            {
                item.szxzmc = ((EnumSZXZ)item.szxz).GetDescription();
            }
            return hospayList;
        }


        /// <summary>
        /// 获取凭证号
        /// </summary>
        //public string GetFinRepSJPZH(out FinanceReceiptEntity outsjhEntity, out string outsjhType)
        //{
        //    FinanceReceiptEntity outEntity = null;
        //    string type = string.Empty;
        //    string sjh = _finRecRepos.getDQSJHNew(UserIdentity.UserCode, this.OrganizeId, out  outEntity, out  type);

        //    outsjhEntity = outEntity;
        //    outsjhType = type;
        //    if (!string.IsNullOrEmpty(sjh))
        //    {
        //        return sjh;
        //    }
        //    return sjh;
        //}

        /// <summary>
        /// 获取凭证号
        /// </summary>
        public string GetFinRepSJPZH()
        {
            FinanceReceiptEntity outEntity = null;
            string type = string.Empty;
            string sjh = _finRecRepos.getDQSJHForpzh(UserIdentity.UserCode, this.OrganizeId);
            
            if (!string.IsNullOrEmpty(sjh))
            {
                return sjh;
            }
            return sjh;
        }



        #region 住院账户操作
        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="zffsbh"></param>
        /// <param name="zffsmc"></param>
        /// <param name="zfje"></param>
        public bool PayDepositPost(DeposDto dto, out string szid)
        {

            if (string.IsNullOrEmpty(dto.patid.ToString())|| string.IsNullOrEmpty(dto.zyh.ToString()))
            {
                throw new FailedException("病人基本信息不可为空!");
            }

            if (string.IsNullOrEmpty(dto.zffsbh.ToString()) || string.IsNullOrEmpty(dto.zffsmc.ToString()))
            {
                throw new FailedException("充值支付方式不能为空");
            }
            if (string.IsNullOrEmpty(dto.zfje.ToString()) || dto.zfje <= 0)
            {
                throw new FailedException("请输入正确的充值金额(不可低于0元)");
            }
            if (string.IsNullOrEmpty(dto.pzh))
            {
                throw new FailedException("凭证号不能为空");
            }

            InpatientAccountRevenueAndExpenseEntity zhszjl = null;
            InpatientAccountEntity oldzh = null;
            InpatientAccountEntity newzh = null;
            FinanceReceiptEntity oldcwsj = null;
            FinanceReceiptEntity newcwsj = null;

            AccPayDeposit(dto, out zhszjl, out oldzh, out newzh); //添加收支记录

            //获取原来的收据凭证Entity  
            oldcwsj = _finRecRepos.getSJKEntity(UserIdentity.UserCode, this.OrganizeId);
            _finRecRepos.DetacheEntity(oldcwsj);

            //新的收据凭证信息
            FinanceReceiptEntity sjEntity = null;
            string type = string.Empty;
            //var pzh =GetFinRepSJPZH(out sjEntity, out type);
            var pzh = GetFinRepSJPZH();
            CheckSJPZH(pzh, out sjEntity, out type);
            //CheckSJPZH(dto.pzh, out sjEntity, out type);
            newcwsj = sjEntity;

            var sqlVO = new PatZyAccDataVO()
            {
                szjl = new InpatientAccountRevenueAndExpenseEntity(),
                oldzh = new InpatientAccountEntity(),
                oldcwsj = new FinanceReceiptEntity()
            };
            sqlVO.szjl = zhszjl;
            sqlVO.oldzh = oldzh;
            sqlVO.oldcwsj = oldcwsj;
            //type默认值
            //type = type == "" ? "update" : type;
            //保存至数据库
            _InpatientReserveDmnService.PatZyAccDBProc(sqlVO, newzh, newcwsj, type);
            szid = zhszjl.Id;
            //实时更新账户余额
            _PatientBasicInfoDmnService.updateZybrxxkExpandZhye(dto.zyh, OrganizeId, newzh.zhye);
            return true;
        }

        /// <summary>
        /// 添加支付、退款记录
        /// </summary>
        /// <param name="dto"></param>
        private void AccPayDeposit(DeposDto dto, out InpatientAccountRevenueAndExpenseEntity payEntity, out InpatientAccountEntity oldAccEntity, out InpatientAccountEntity newAccEntity)
        {

            payEntity = new InpatientAccountRevenueAndExpenseEntity();
            payEntity.szxz = dto.szxz;
            //payEntity.pzh = dto.pzh;
            payEntity.pzh = dto.pzh;
            payEntity.zhCode = dto.zh;
            payEntity.zyh = dto.zyh;
            payEntity.szje =dto.szxz==2? (-1)*System.Math.Abs(dto.zfje) : dto.zfje; // 退款:负数
            payEntity.xjzffs = dto.zffsbh.ToString();
            payEntity.zhye = payEntity.szje + GetzhYe(dto.zyh);
            payEntity.memo = dto.memo;
            payEntity.zt = "1";
            payEntity.OrganizeId = OrganizeId;
            payEntity.outTradeNo = dto.outTradeNo;
            ////获取凭证号s
            ////判断静态参数配置
            payEntity.pzh = GetFinRepSJPZH();
            payEntity.Create(true);
            //更新账户信息
            oldAccEntity = _inpatientAccountRepo.FindEntity(p => p.zyh == dto.zyh && p.OrganizeId == OrganizeId && p.zt == "1");
            _inpatientAccountRepo.DetacheEntity(oldAccEntity);
            newAccEntity = oldAccEntity.Clone();
            newAccEntity.zhye = payEntity.zhye;
        }
        /// <summary>
        /// 获得住院账户余额
        /// </summary>
        public decimal GetzhYe(string zyh)
        {
            var zyzhEntity = _inpatientAccountRepo.FindEntity(p => p.zyh == zyh && p.OrganizeId == OrganizeId && p.zt == "1");
            if (zyzhEntity == null)
            {
                throw new FailedException("病人本次住院尚未开通账户");
            }
            return zyzhEntity.zhye;
        }
        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="zffsbh"></param>
        /// <param name="zffsmc"></param>
        /// <param name="zfje"></param>
        public bool ReturnDepositPost(DeposDto dto)
        {
            if (string.IsNullOrEmpty(dto.patid.ToString()) || string.IsNullOrEmpty(dto.zyh))
            {
                throw new FailedException("患者信息不可为空");
            }
            if (string.IsNullOrEmpty(dto.zffsbh.ToString()) || string.IsNullOrEmpty(dto.zffsmc.ToString()))
            {
                throw new FailedException("支付方式不能为空");
            }
            //if (string.IsNullOrEmpty(dto.zfje.ToString()) || dto.zfje >= 0)
            //{
            //    throw new FailedException("请输入正确的取款金额(金额须为负值)");
            //}
            if (string.IsNullOrEmpty(dto.pzh))
            {
                throw new FailedException("凭证号不能为空");
            }

            InpatientAccountRevenueAndExpenseEntity zhszjl = null;
            InpatientAccountEntity oldzh = null;
            InpatientAccountEntity newzh = null;
            FinanceReceiptEntity oldcwsj = null;
            FinanceReceiptEntity newcwsj = null;

            AccPayDeposit(dto, out zhszjl, out oldzh, out newzh); //添加收支记录 负的

            //新的收据凭证信息
            FinanceReceiptEntity sjEntity = null;
            string type = string.Empty;

            //预交金退款是否使用凭证号
            var isTopzh = _sysConfigRepo.GetBoolValueByCode("Account_Refund_pzh", OrganizeId);

            if (isTopzh is true)
            {
                var pzh = GetFinRepSJPZH();
                CheckSJPZH(pzh, out sjEntity, out type);
                newcwsj = sjEntity;
            }
            else {
                zhszjl.pzh = "";
            }

            var sqlVO = new PatZyAccDataVO()
            {
                szjl = new InpatientAccountRevenueAndExpenseEntity(),
                oldzh = new InpatientAccountEntity(),
                oldcwsj = new FinanceReceiptEntity()
            };
            sqlVO.szjl = zhszjl;
            sqlVO.oldzh = oldzh;
            sqlVO.oldcwsj = oldcwsj;
            //保存至数据库
            _InpatientReserveDmnService.PatZyAccDBProc(sqlVO, newzh, newcwsj, type);
            //实时更新账户余额
            _PatientBasicInfoDmnService.updateZybrxxkExpandZhye(dto.zyh, OrganizeId, newzh.zhye);
            return true;
        }

        /// <summary>
        /// 余额全退
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool RefundAccount(DeposDto dto)
        {

            if (string.IsNullOrEmpty(dto.patid.ToString())||string.IsNullOrWhiteSpace(dto.zyh))
            {
                throw new FailedException("患者信息不可为空");
            }
            if (string.IsNullOrEmpty(dto.zffsbh.ToString()) || string.IsNullOrEmpty(dto.zffsmc.ToString()))
            {
                throw new FailedException("支付方式不能为空");
            }
            //if (string.IsNullOrEmpty(dto.zfje.ToString()) || dto.zfje >= 0)
            //{
            //    throw new FailedException("无可退余额");
            //}

            InpatientAccountRevenueAndExpenseEntity payEntity = null;
            InpatientAccountEntity oldAccEntity = null;
            InpatientAccountEntity newAccEntity = null;

            payEntity = new InpatientAccountRevenueAndExpenseEntity();
            payEntity.szxz = dto.szxz;
            //payEntity.pzh = pzhList;
            payEntity.zhCode = dto.zh;
            payEntity.zyh = dto.zyh;
            payEntity.szje = dto.zfje;
            payEntity.xjzffs = dto.zffsbh.ToString();
            payEntity.zhye = payEntity.szje + GetzhYe(dto.zyh);
            payEntity.zt = "1";
            payEntity.OrganizeId = OrganizeId;
            //payEntity.pzh = GetFinRepSJPZH();//获取凭证号
            payEntity.Create(true);
            //更新账户信息
            oldAccEntity = _inpatientAccountRepo.FindEntity(p => p.zyh == dto.zyh && p.OrganizeId == OrganizeId && p.zt == "1");
            _inpatientAccountRepo.DetacheEntity(oldAccEntity);
            newAccEntity = oldAccEntity.Clone();
            newAccEntity.zhye = payEntity.zhye;
            var sqlVO = new PatZyAccDataVO()
            {
                szjl = new InpatientAccountRevenueAndExpenseEntity(),
                oldzh = new InpatientAccountEntity(),
                oldcwsj = new FinanceReceiptEntity()
            };
            sqlVO.szjl = payEntity;
            sqlVO.oldzh = oldAccEntity;
            sqlVO.oldcwsj = null;
            //保存至数据库
            _InpatientReserveDmnService.PatZyAccDBProc(sqlVO, newAccEntity, null, "");
            //实时更新账户余额
            _PatientBasicInfoDmnService.updateZybrxxkExpandZhye(dto.zyh, OrganizeId, newAccEntity.zhye);
            return true;
        }

        #endregion

        #region 发票相关

        /// <summary>
        /// 后台验证收据凭证号
        /// </summary>
        /// <returns></returns> 
        public bool CheckSJPZH(string pzh, out FinanceReceiptEntity sjEntity, out string type)
        {
            bool flag = false;
            sjEntity = new FinanceReceiptEntity();
            type = string.Empty;

            flag = _finRecRepos.checkSJH(pzh, UserIdentity.UserCode, out sjEntity, out type, this.OrganizeId);
            return flag;
        }
        #endregion
    }
}
