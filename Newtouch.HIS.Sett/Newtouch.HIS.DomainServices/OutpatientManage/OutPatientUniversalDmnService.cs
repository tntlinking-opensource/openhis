using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.BusinessObjects;
using Newtouch.HIS.Domain.DTO;
using Newtouch.HIS.Domain.DTO.InputDto;
using Newtouch.HIS.Domain.DTO.OutputDto.OutpatientManage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.OutpatientManage;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.IRepository.OutpatientManage;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Proxy.guian.DTO.S25;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using static Newtouch.Infrastructure.Constants;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 门诊的一些通用方法
    /// 一定是不区分版本
    /// </summary>
    public class OutPatientUniversalDmnService : DmnServiceBase, IOutPatientUniversalDmnService
    {
        private readonly ISysStaffRepo _sysStaffRepo;
        private readonly ISysDepartmentRepo _sysDepartmentRepo;
        private readonly IOutpatientRegistRepo _outpatientRegistRepo;
        private readonly ISysChargeItemRepo _sysChargeItemRepo;
        private readonly ISysMedicinRepo _sysMedicinRepo;
        private readonly ISysPharmacyDepartmentRepo _sysPharmacyDepartmentRepo;
        private readonly IOutpatientPrescriptionRepo _outpatientPrescriptionRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly IFinancialInvoiceRepo _financialInvoiceRepo;
        private readonly ICqybSett23Repo _iCqybSett23Repo;
        public OutPatientUniversalDmnService(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
        }

        /// <summary>
        /// 提交明细（重要：如果是向处方追加或作废明细，线下意义这是在变更这张处方，医生一般不这么做）（处方的话更新主表信息）（请提前构造好其请求参数）（根据明细的主键xmnm、cfmxId值来判断这是新增，还是修改记录）（当是新增，根据cfnm字段，又分新增明细及处方，还是向已有处方追加明细）（修改的话是直接更新数据库明细记录/此版本暂不支持，否则请去退费）
        /// </summary>
        /// <param name="orgId">医疗机构Id</param>
        /// <param name="changedCfnmList">返回用的</param>
        /// <param name="changedXmnmList">返回用的（如果项目不关联处方）</param>
        /// <param name="updateSkipSettledCfhList">（这是一个返回）更新已结处方被跳过的cfh List</param>
        /// <param name="mzh">如果需要新增的话，给你准备好</param>
        /// <param name="ys">如果需要新增的话，给你准备好（未指定时去挂号的ys）</param>
        /// <param name="ItemsGroupBOList">计费信息分组（新增处方/新增处方明细/新增项目明细）处方信息/处方明细信息/项目信息</param>
        /// <param name="settledUpdateForbidden">已结时 是否禁止更新</param>
        /// <param name="settledUpdateSkip">（已结时禁止更新）检查是否Skip，true不抛异常跳过，否则抛异常</param>
        /// <param name="cfly">处方来源（0 收费处 1 医生站）</param>
        public void SubmitItems(
            string orgId
            , out IList<int> changedCfnmList, out IList<int> changedXmnmList
            , out IList<string> updateSkipSettledCfhList
            , string mzh = null, string ys = null
            , IList<FeeItemsGroupedBO> ItemsGroupBOList = null
            , bool settledUpdateForbidden = true
            , bool settledUpdateSkip = false
            , string cfly = null)
        {
            changedCfnmList = new List<int>();
            changedXmnmList = new List<int>();
            updateSkipSettledCfhList = new List<string>();

            if (ItemsGroupBOList == null || ItemsGroupBOList.Count == 0)
            {
                return;
            }

            //系统门诊药房
            IList<SysPharmacyDepartmentVEntity> mzyfList = null;

            //药品是否关联药房库存
            var isMedicineSearchRelatedKC = _sysConfigRepo.GetBoolValueByCode("IS_MedicineSearchRelatedKC", orgId);

            //预处理
            foreach (var itemsGroup in ItemsGroupBOList)
            {
                foreach (var item in itemsGroup.ypList)
                {
                    UpdateEntity(orgId, item);
                }
                foreach (var item in itemsGroup.xmList)
                {
                    UpdateEntity(orgId, item);
                }

                if (isMedicineSearchRelatedKC != true || itemsGroup.ypList == null || itemsGroup.ypList.Count <= 0) continue;
                if (!string.IsNullOrWhiteSpace(itemsGroup.lyyf)) continue;
                //如果就1个门诊药房、
                mzyfList = mzyfList ?? _sysPharmacyDepartmentRepo.GetPharmacyDepartmentList(orgId, ((int)Enummzzybz.mz).ToString());
                if (mzyfList.Count == 1)
                {
                    itemsGroup.lyyf = mzyfList[0].yfbmCode;
                }
            }

            //准备好挂号信息 医生 科室信息
            var mzghEntity = _outpatientRegistRepo.FindEntity(p => p.mzh == mzh && p.OrganizeId == orgId && p.zt == "1");
            if (mzghEntity == null)
            {
                throw new FailedException("REGISTINFO_NOT_FOUND", "关联挂号信息异常");
            }
            if (string.IsNullOrWhiteSpace(mzghEntity.ys))
            {
                //没挂到科室，则尝试取收费明细的挂号
                var settPatInfo = GetSettPatientInfo(mzh, orgId);
                if (settPatInfo != null)
                {
                    mzghEntity.ys = settPatInfo.ys;
                }
            }
            ys = ys ?? mzghEntity.ys;
            if (string.IsNullOrWhiteSpace(ys))
            {
                //挂号信息未指定医生 //则尝试取当前操作者
                try
                {
                    var opr = Newtouch.Common.Operator.OperatorProvider.GetCurrent();
                    if (opr != null)
                    {
                        ys = opr.rygh;
                        //check is 医生
                        var checkIsDoctorSql = @"select 1 from [NewtouchHIS_Base]..V_C_Sys_StaffDuty where StaffGh = @rygh and DutyCode = 'Doctor' and zt = '1' and OrganizeId = @orgId";
                        var curOprIsDoctor = this.FirstOrDefault<int?>(checkIsDoctorSql, new[] { new SqlParameter("@rygh", ys), new SqlParameter("@orgId", orgId) });
                        if ((curOprIsDoctor ?? 0) == 0)
                        {
                            ys = null;  //当前操作人不是医生
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            var ysEntity = string.IsNullOrWhiteSpace(ys) ? null : _sysStaffRepo.GetValidStaffByGh(ys, orgId);
            if (ysEntity == null)
            {
                throw new FailedException("REGISTDOCTORINFO_NOT_FOUND", "缺少医生信息");
            }
            var ysmc = ysEntity.Name;
            var ks = ysEntity.DepartmentCode;
            var ksmc = _sysDepartmentRepo.GetNameByCode(ysEntity.DepartmentCode, orgId);

            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                //处方主表的一些字段 统一更新下 比如ys ks ksmc等
                var cfzbEntityList = new List<OutpatientPrescriptionEntity>();
                //项目表的一些字段 统一更新下 比如ys ks ksmc等
                var xmEntityList = new List<OutpatientItemEntity>();

                foreach (var itemsGroup in ItemsGroupBOList)
                {
                    if (itemsGroup.xmList.Count == 0
                        && itemsGroup.ypList.Count == 0
                        && itemsGroup.ztInvalidXmList.Count == 0
                        && itemsGroup.ztInvalidYpList.Count == 0)
                    {
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(itemsGroup.cfh) && (itemsGroup.cflx ?? 0) > 0)
                    {
                        #region 给了我处方号 这一定是在操作一个处方内的数据
                        //给了我处方号 这一定是要操作一个处方内的数据
                        //去找处方主表，没有就新增
                        bool isNewCf = false;
                        var cfzbEntity = db.IQueryable<OutpatientPrescriptionEntity>(p => p.OrganizeId == orgId && p.cfh == itemsGroup.cfh && p.cflx == itemsGroup.cflx.Value && p.zt == "1").FirstOrDefault();
                        if (cfzbEntity == null)
                        {
                            isNewCf = true;
                            //新增一张处方
                            cfzbEntity = new OutpatientPrescriptionEntity()
                            {
                                cfnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_cf"),
                                zje = 0,    //只能后面再根据明细更新
                                cfzt = "0", //0待结
                                cfly = cfly,
                                cfh = itemsGroup.cfh,
                                cflx = itemsGroup.cflx,
                                cflxmc = itemsGroup.cflxmc,
                                zt = "1",
                                sfrq = itemsGroup.cfsfrq,
                                ts = (itemsGroup.cflx.Value == (int)EnumPrescriptionType.Medicine) ? itemsGroup.ts : null,
                                djbz = (itemsGroup.cflx.Value == (int)EnumPrescriptionType.Medicine) ? itemsGroup.djbz : null,
                                lyyf = (itemsGroup.cflx.Value == (int)EnumPrescriptionType.Medicine) ? itemsGroup.lyyf : null,
                            };
                            cfzbEntity.Create();
                            db.Insert(cfzbEntity);
                        }
                        else
                        {
                            if (cfzbEntity.cfzt == "1" && settledUpdateForbidden)
                            {
                                if (settledUpdateSkip)
                                {
                                    updateSkipSettledCfhList.Add(cfzbEntity.cfh);
                                    continue;   //跳过该处方
                                }
                                throw new FailedException("SETTLED_UPDATE_FORBIDDEN", "已结状态不允许修改变更");
                            }
                            if (cfzbEntity.ys != ys)
                            {
                                //历史处方 改医生是不对的
                                throw new FailedException("DOCTOR_CHANGED_FORBIDDEN", "发生错误处方医生发生变更");
                            }
                            cfzbEntity.Modify();
                            db.Update(cfzbEntity);
                        }
                        changedCfnmList.Add(cfzbEntity.cfnm);
                        cfzbEntityList.Add(cfzbEntity);
                        decimal ypzje = 0.00M;
                        if (itemsGroup.cflx.Value == (int)EnumPrescriptionType.Medicine)
                        {
                            //这是一张药品处方
                            if (itemsGroup.ypList.Count == 0 && itemsGroup.ztInvalidYpList.Count == 0)
                            {
                                //没给明细
                                throw new FailedException("PARAMS_REQUIRE_DETAILS", "参数错误缺少明细");
                            }

                            //数据库中的当前明细List
                            IList<OutpatientPrescriptionDetailEntity> existList = isNewCf
                                ? new List<OutpatientPrescriptionDetailEntity>()
                                : db.IQueryable<OutpatientPrescriptionDetailEntity>(p => p.OrganizeId == orgId && p.cfnm == cfzbEntity.cfnm && p.zt == "1").ToList();

                            //变更之前的总金额
                            cfzbEntity.zje = existList.Select(p => p.je).Sum();
                            
                            //作废details
                            if (itemsGroup.ztInvalidYpList.Count > 0)
                            {
                                var ids = itemsGroup.ztInvalidYpList.Select(p => p.cfmxId).Where(p => p > 0).ToList();
                                var codes = itemsGroup.ztInvalidYpList.Where(p => p.cfmxId == 0).Select(p => p.yp).ToList();
                                var inValidItems = existList.Where(p => (ids.Contains(p.cfmxId) || codes.Contains(p.yp)) && p.zt == "1").ToList();
                                if (inValidItems.Count != itemsGroup.ztInvalidYpList.Count)
                                {
                                    //指定的作废明细 参数有问题
                                    throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误作废明细发生异常");
                                }
                                foreach (var item in inValidItems)
                                {
                                    item.zt = "0";
                                    item.Modify();
                                    db.Update(item);

                                    cfzbEntity.zje -= item.je;  //同时更新下处方总金额
                                }
                            }

                            //add/update details
                            if (itemsGroup.ypList.Count > 0)
                            {
                                ;
                                if (itemsGroup.ypList.Select(p => (p.czh ?? "") + p.yp).Count()
                                    != itemsGroup.ypList.Count)
                                {
                                    //#######规则问题########
                                    throw new FailedException("DETAILS_REPEATED_ERROR", "单张处方下明细不能重复");
                                }
                                foreach (var item in itemsGroup.ypList)
                                {
                                    if (existList.Where(p => p.zt == "1").Any(p => p.yp == item.yp
                                        && (p.czh ?? "") == (item.czh ?? "")
                                        && p.cfmxId != item.cfmxId))
                                    {
                                        //#######规则问题########
                                        throw new FailedException("DETAILS_REPEATED_ERROR", "单张处方下明细不能重复");
                                    }
                                    if (item.cfmxId == 0)
                                    {
                                        //这是要向已有处方追加明细
                                        item.cfmxId = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_cfmx");
                                        item.OrganizeId = orgId;
                                        item.cfnm = cfzbEntity.cfnm;
                                        item.Create();
                                        db.Insert(item);
                                        cfzbEntity.zje += item.je;  //同时更新下处方总金额
                                    }
                                    else
                                    {
                                        //这是要直接Update数据库里的明细 暂未支持
                                        throw new NotSupportedException();
                                    }
                                }
                            }
                            ypzje = cfzbEntity.zje;
                            if (!existList.Any(p => p.zt == "1") && itemsGroup.ypList.Count == 0)
                            {
                                cfzbEntity.zt = "0";   //明细若都作废了，主表也跟着作废
                                cfzbEntity.Modify();
                                db.Update(cfzbEntity);
                            }

                            #region 药品处方中有收费项目组套

                            if (itemsGroup.xmList.Count > 0)
                            {


                                //这是一张项目处方
                                if (itemsGroup.xmList.Count == 0 && itemsGroup.ztInvalidXmList.Count == 0)
                                {
                                    //没给明细
                                    throw new FailedException("PARAMS_REQUIRE_DETAILS", "参数错误缺少明细");
                                }

                                //数据库中的当前明细List
                                IList<OutpatientItemEntity> existList2 = isNewCf
                                    ? new List<OutpatientItemEntity>()
                                    : db.IQueryable<OutpatientItemEntity>(p => p.OrganizeId == orgId && p.cfnm == cfzbEntity.cfnm && p.zt == "1").ToList();

                                //变更之前的总金额
                                cfzbEntity.zje = existList2.Select(p => p.je).Sum();
                                
                                //作废details
                                if (itemsGroup.ztInvalidXmList.Count > 0)
                                {
                                    var ids = itemsGroup.ztInvalidXmList.Select(p => p.xmnm).Where(p => p > 0).ToList();
                                    var codes = itemsGroup.ztInvalidXmList.Where(p => p.xmnm == 0).Select(p => p.sfxm).ToList();
                                    var inValidItems = existList2.Where(p => (ids.Contains(p.xmnm) || codes.Contains(p.sfxm)) && p.zt == "1").ToList();
                                    if (inValidItems.Count != itemsGroup.ztInvalidXmList.Count)
                                    {
                                        //指定的作废明细 参数有问题
                                        throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误作废明细发生异常");
                                    }
                                    foreach (var item in inValidItems)
                                    {
                                        item.zt = "0";
                                        item.Modify();
                                        db.Update(item);

                                        cfzbEntity.zje -= item.je;  //同时更新下处方总金额
                                    }
                                }

                                //add/update details
                                if (itemsGroup.xmList.Count > 0)
                                {
                                    if (itemsGroup.xmList.Select(p => p.sfxm).Distinct().Count() !=
                                    itemsGroup.xmList.Count)
                                    {
                                        //#######规则问题########
                                        throw new FailedException("DETAILS_REPEATED_ERROR", "单张处方下明细不能重复");
                                    }
                                    foreach (var item in itemsGroup.xmList)
                                    {
                                        if (existList2.Where(p => p.zt == "1").Any(p => p.sfxm == item.sfxm && p.xmnm != item.xmnm))
                                        {
                                            //#######规则问题########
                                            throw new FailedException("DETAILS_REPEATED_ERROR", "单张处方下明细不能重复");
                                        }
                                        if (item.xmnm == 0)
                                        {
                                            //这是要向已有处方追加明细
                                            item.xmnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_xm");
                                            item.cfnm = cfzbEntity.cfnm;
                                            item.xmzt = cfzbEntity.cfzt;
                                            item.xmly = cfzbEntity.cfly;
                                            //如果是已结算 取当前时间
                                            item.jsrq = cfzbEntity.jsrq.HasValue ? (DateTime?)DateTime.Now : null;
                                            item.Create();
                                            db.Insert(item);
                                            xmEntityList.Add(item);

                                            cfzbEntity.zje += item.je;  //同时更新下处方总金额
                                        }
                                        else
                                        {
                                            //这是要直接Update数据库里的明细 暂未支持
                                            throw new NotSupportedException();
                                        }
                                    }
                                }
                                cfzbEntity.zje += ypzje;
                                if (!existList2.Any(p => p.zt == "1") && itemsGroup.xmList.Count == 0)
                                {
                                    cfzbEntity.zt = "0";   //明细若都作废了，主表也跟着作废
                                    cfzbEntity.Modify();
                                    db.Update(cfzbEntity);
                                }
                            }
                            #endregion

                        }
                        else if (itemsGroup.cflx.Value == (int)EnumPrescriptionType.Treament)
                        {
                            //这是一张项目处方
                            if (itemsGroup.xmList.Count == 0 && itemsGroup.ztInvalidXmList.Count == 0)
                            {
                                //没给明细
                                throw new FailedException("PARAMS_REQUIRE_DETAILS", "参数错误缺少明细");
                            }

                            //数据库中的当前明细List
                            IList<OutpatientItemEntity> existList = isNewCf
                                ? new List<OutpatientItemEntity>()
                                : db.IQueryable<OutpatientItemEntity>(p => p.OrganizeId == orgId && p.cfnm == cfzbEntity.cfnm && p.zt == "1").ToList();

                            //变更之前的总金额
                            cfzbEntity.zje = existList.Select(p => p.je).Sum();

                            //作废details
                            if (itemsGroup.ztInvalidXmList.Count > 0)
                            {
                                var ids = itemsGroup.ztInvalidXmList.Select(p => p.xmnm).Where(p => p > 0).ToList();
                                var codes = itemsGroup.ztInvalidXmList.Where(p => p.xmnm == 0).Select(p => p.sfxm).ToList();
                                var inValidItems = existList.Where(p => (ids.Contains(p.xmnm) || codes.Contains(p.sfxm)) && p.zt == "1").ToList();
                                if (inValidItems.Count != itemsGroup.ztInvalidXmList.Count)
                                {
                                    //指定的作废明细 参数有问题
                                    throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误作废明细发生异常");
                                }
                                foreach (var item in inValidItems)
                                {
                                    item.zt = "0";
                                    item.Modify();
                                    db.Update(item);

                                    cfzbEntity.zje -= item.je;  //同时更新下处方总金额
                                }
                            }

                            //add/update details
                            if (itemsGroup.xmList.Count > 0)
                            {
                                if (itemsGroup.xmList.Select(p => p.sfxm).Distinct().Count() !=
                                itemsGroup.xmList.Count)
                                {
                                    //#######规则问题########
                                    throw new FailedException("DETAILS_REPEATED_ERROR", "单张处方下明细不能重复");
                                }
                                foreach (var item in itemsGroup.xmList)
                                {
                                    if (existList.Where(p => p.zt == "1").Any(p => p.sfxm == item.sfxm && p.xmnm != item.xmnm))
                                    {
                                        //#######规则问题########
                                        throw new FailedException("DETAILS_REPEATED_ERROR", "单张处方下明细不能重复");
                                    }
                                    if (item.xmnm == 0)
                                    {
                                        //这是要向已有处方追加明细
                                        item.xmnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_xm");
                                        item.cfnm = cfzbEntity.cfnm;
                                        item.xmzt = cfzbEntity.cfzt;
                                        item.xmly = cfzbEntity.cfly;
                                        //如果是已结算 取当前时间
                                        item.jsrq = cfzbEntity.jsrq.HasValue ? (DateTime?)DateTime.Now : null;
                                        item.Create();
                                        db.Insert(item);
                                        xmEntityList.Add(item);

                                        cfzbEntity.zje += item.je;  //同时更新下处方总金额
                                    }
                                    else
                                    {
                                        //这是要直接Update数据库里的明细 暂未支持
                                        throw new NotSupportedException();
                                    }
                                }
                            }

                            if (!existList.Any(p => p.zt == "1") && itemsGroup.xmList.Count == 0)
                            {
                                cfzbEntity.zt = "0";   //明细若都作废了，主表也跟着作废
                                cfzbEntity.Modify();
                                db.Update(cfzbEntity);
                            }
                        }
                        else
                        {
                            //处方类型 异常 非范围内
                            throw new FailedException("PARAMS_ERROR_CFLX", "处方类型参数错误");
                        }
                        #endregion 给了我处方号 这一定是在操作一个处方内的数据
                    }
                    else if (string.IsNullOrWhiteSpace(itemsGroup.cfh) && (itemsGroup.cflx ?? 0) == 0)  //不可以指定处方类型？
                    {
                        #region 没有指定处方号（2018.04.18内部不再自动分方）

                        #region 先药品
                        if (itemsGroup.ypList.Count > 0 || itemsGroup.ztInvalidYpList.Count > 0)
                        {
                            //这些是要新增处方的
                            var itemsNew = itemsGroup.ypList.Where(p => p.cfnm == 0).ToList();
                            //这些是向已有处方追加明细的（暂不支持直接Update数据库）
                            var itemsAppendOrUpdate = itemsGroup.ypList.Where(p => p.cfnm > 0).ToList();
                            //下面分两部分 1添加新处方 2历史处方的追加明细或作废明细

                            #region 1新增部分（mz_cfmx + mz_cf）
                            if (itemsNew.Count > 0)
                            {
                                foreach (var item in itemsNew)
                                {
                                    item.cfmxId = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_cfmx");
                                    item.OrganizeId = orgId;
                                    item.Create();
                                    db.Insert(item);
                                }

                                //新增1张处方
                                var cfzbEntity = new OutpatientPrescriptionEntity()
                                {
                                    cfnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_cf"),
                                    zje = 0,    //只能后面再根据明细更新
                                    cfzt = "0", //0待结
                                    cfly = cfly,
                                    cfh = _outpatientPrescriptionRepo.GeneratePresNo(orgId, (int)EnumPrescriptionType.Medicine),
                                    cflx = (int)EnumPrescriptionType.Medicine,
                                    zt = "1",
                                    sfrq = itemsGroup.cfsfrq,
                                    ts = itemsGroup.ts,
                                    djbz = itemsGroup.djbz,
                                    lyyf = itemsGroup.lyyf,
                                };

                                cfzbEntity.zje = itemsNew.Sum(p => p.je);  //同时更新下处方总金额

                                cfzbEntity.Create();
                                db.Insert(cfzbEntity);

                                changedCfnmList.Add(cfzbEntity.cfnm);
                                cfzbEntityList.Add(cfzbEntity);

                                //更新新增明细的cfnm
                                foreach (var item in itemsNew)
                                {
                                    item.cfnm = cfzbEntity.cfnm;
                                }
                            }
                            #endregion
                            #region 2历史处方的追加明细或作废明细

                            #endregion
                        }
                        #endregion

                        #region 后项目
                        if (itemsGroup.xmList.Count > 0 || itemsGroup.ztInvalidXmList.Count > 0)
                        {
                            //这些是要新增处方的 //或未使用处方新增明细的
                            var itemsNew = itemsGroup.xmList.Where(p => p.cfnm == null || p.cfnm == 0).ToList();
                            //这些是向已有处方追加明细的（暂不支持直接Update数据库）
                            var itemsAppendOrUpdate = itemsGroup.xmList.Where(p => p.cfnm > 0).ToList();
                            //下面分两部分 1新门诊项目计费（新处方） 2历史处方的追加明细或作废明细
                            #region 1新增部分（门诊项目/新处方）（也可能仅门诊项目）
                            if (itemsNew.Count > 0)
                            {
                                foreach (var item in itemsNew)
                                {
                                    item.xmnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_xm");
                                    item.xmzt = "0";
                                    item.xmly = cfly;
                                    item.jsrq = null;
                                    item.Create();
                                    db.Insert(item);
                                    xmEntityList.Add(item);

                                    if (!itemsGroup.xmWithCf)
                                    {
                                        changedXmnmList.Add(item.xmnm);
                                    }
                                }

                                if (itemsGroup.xmWithCf)
                                {
                                    //新增1张处方
                                    var cfzbEntity = new OutpatientPrescriptionEntity()
                                    {
                                        cfnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_cf"),
                                        zje = 0,    //只能后面再根据明细更新
                                        cfzt = "0", //0待结
                                        cfly = cfly,
                                        cfh = _outpatientPrescriptionRepo.GeneratePresNo(orgId, (int)EnumPrescriptionType.Treament),
                                        cflx = (int)EnumPrescriptionType.Treament,
                                        zt = "1",
                                        sfrq = itemsGroup.cfsfrq,
                                    };

                                    cfzbEntity.zje = itemsNew.Sum(p => p.je);  //同时更新下处方总金额

                                    cfzbEntity.Create();
                                    db.Insert(cfzbEntity);

                                    changedCfnmList.Add(cfzbEntity.cfnm);
                                    cfzbEntityList.Add(cfzbEntity);

                                    //更新新增明细的cfnm
                                    foreach (var item in itemsNew)
                                    {
                                        item.cfnm = cfzbEntity.cfnm;
                                    }
                                }

                            }
                            #endregion
                            #region 2历史处方的追加明细或作废明细
                            if (itemsGroup.xmWithCf)
                            {
                                var inValidItems = new List<OutpatientItemEntity>();    //后面要将其作废
                                var oldPresCfnmPartList = itemsAppendOrUpdate.Select(p => p.cfnm).ToList();
                                if (itemsGroup.ztInvalidXmList.Count > 0)
                                {
                                    var ids = itemsGroup.ztInvalidXmList.Select(p => p.xmnm).ToList();
                                    inValidItems = db.IQueryable<OutpatientItemEntity>(p => ids.Contains(p.xmnm) && p.zt == "1").ToList();
                                    if (inValidItems.Count != itemsGroup.ztInvalidXmList.Count)
                                    {
                                        //指定的作废明细 参数有问题
                                        throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误作废明细发生异常");
                                    }
                                    oldPresCfnmPartList.AddRange(inValidItems.Select(p => p.cfnm));
                                }
                                var oldPresList = db.IQueryable<OutpatientPrescriptionEntity>(p => oldPresCfnmPartList.Contains(p.cfnm)).ToList();
                                foreach (var cfzbEntity in oldPresList)
                                {
                                    if (cfzbEntity.cfzt == "1" && settledUpdateForbidden)
                                    {
                                        throw new FailedException("SETTLED_UPDATE_FORBIDDEN", "已结状态不允许修改变更");
                                    }
                                    IList<OutpatientItemEntity> existList = db.IQueryable<OutpatientItemEntity>(p => p.OrganizeId == orgId && p.cfnm == cfzbEntity.cfnm && p.zt == "1").ToList();
                                    //变更之前的总金额
                                    cfzbEntity.zje = existList.Select(p => p.je).Sum();
                                    //作废
                                    foreach (var item in existList.Where(p => inValidItems.Any(t => t.xmnm == p.xmnm)))
                                    {
                                        item.zt = "0";
                                        item.Modify();
                                        db.Update(item);

                                        cfzbEntity.zje -= item.je;  //同时更新下处方总金额
                                    }
                                    //追加明细
                                    var thisitemsAppendOrUpdate = itemsAppendOrUpdate.Where(p => p.cfnm == cfzbEntity.cfnm).ToList();
                                    foreach (var item in thisitemsAppendOrUpdate)
                                    {
                                        if (item.xmnm == 0)
                                        {
                                            //这是要向已有处方追加明细
                                            item.xmnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_xm");
                                            item.cfnm = cfzbEntity.cfnm;
                                            item.xmzt = cfzbEntity.cfzt;
                                            item.xmly = cfzbEntity.cfly;
                                            //如果是已结算 取当前时间
                                            item.jsrq = cfzbEntity.jsrq.HasValue ? (DateTime?)DateTime.Now : null;
                                            item.Create();
                                            db.Insert(item);
                                            xmEntityList.Add(item);

                                            cfzbEntity.zje += item.je;  //同时更新下处方总金额
                                        }
                                        else
                                        {
                                            //这是要直接Update数据库里的明细 暂未支持
                                            throw new NotSupportedException();
                                        }
                                    }
                                    if (!existList.Any(p => p.zt == "1") && thisitemsAppendOrUpdate.Count == 0)
                                    {
                                        cfzbEntity.zt = "0";   //明细若都作废了，主表也跟着作废
                                    }
                                    cfzbEntity.Modify();
                                    db.Update(cfzbEntity);

                                    changedCfnmList.Add(cfzbEntity.cfnm);
                                    cfzbEntityList.Add(cfzbEntity);
                                }
                            }
                            else
                            {
                                //项目不关联处方 新增明细 或 作废明细
                                //作废
                                if (itemsGroup.ztInvalidXmList.Count > 0)
                                {
                                    var ids = itemsGroup.ztInvalidXmList.Select(p => p.xmnm).ToList();
                                    var inValidItems = db.IQueryable<OutpatientItemEntity>(p => ids.Contains(p.xmnm) && p.zt == "1").ToList();
                                    if (inValidItems.Count != itemsGroup.ztInvalidXmList.Count)
                                    {
                                        //指定的作废明细 参数有问题
                                        throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误作废明细发生异常");
                                    }
                                    foreach (var item in inValidItems)
                                    {
                                        item.zt = "0";
                                        item.Modify();
                                        db.Update(item);

                                        changedXmnmList.Add(item.xmnm);
                                    }
                                }

                                //update
                                if (itemsAppendOrUpdate.Count > 0)
                                {
                                    //这是要直接Update数据库里的明细 暂未支持
                                    throw new NotSupportedException();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #endregion 没有指定处方号（这里面也可能有几个处方）
                    }
                }
                #region 收尾
                //批量更新下处方的ys、ks等字段
                foreach (var cfzbEntityItem in cfzbEntityList)
                {
                    cfzbEntityItem.OrganizeId = orgId;
                    cfzbEntityItem.ghnm = mzghEntity.ghnm;
                    cfzbEntityItem.patid = mzghEntity.patid;
                    cfzbEntityItem.brxz = mzghEntity.brxz;
                    cfzbEntityItem.mjzbz = mzghEntity.mjzbz;
                    cfzbEntityItem.ys = ys;
                    cfzbEntityItem.ks = ks;
                    cfzbEntityItem.ysmc = ysmc;
                    cfzbEntityItem.ksmc = ksmc;
                }
                //批量更新下项目的ys、ks等字段
                foreach (var xmEntity in xmEntityList)
                {
                    xmEntity.OrganizeId = orgId;
                    xmEntity.ghnm = mzghEntity.ghnm;
                    xmEntity.patid = mzghEntity.patid;
                    xmEntity.brxz = mzghEntity.brxz;
                    xmEntity.mjzbz = mzghEntity.mjzbz;
                    xmEntity.ys = ys;
                    xmEntity.ks = ks;
                    xmEntity.ysmc = ysmc;
                    xmEntity.ksmc = ksmc;
                }
                #endregion
                if (changedCfnmList.Count > 0 || changedXmnmList.Count > 0)
                {
                    db.Commit();
                }
            }
            return;
        }

        /// <summary>
        /// 更新门诊处方/门诊项目的结算信息（更新结算信息）
        /// （这个cfnm可能已作废，目的可能是也作废结算信息）（调用此方法前，请先更新好mz_cfmx、mz_xm，且已持久化到数据库中）
        /// 根据cfnm查出所有的相关表mz_js mz_jsmx mz_cf mz_cfmx mz_xm，根据mz_cfmx、mz_xm的正确数据更新其他表（可能会新增mz_jsmx）
        /// （可能会新增mz_js）（check mz_xm、mz_cf zt 0，对应mz_jsmx 也要更新）（多个mz_cf可以对应一个结算）
        /// （但一个mz_cf不能对应多个结算）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mzh"></param>
        /// <param name="settlementAddBO"></param>
        /// <param name="feeRelated">金额及支付信息</param>
        /// <param name="ybfeeRelated">医保相关费用</param>
        /// <param name="xnhybfeeRelated"></param>
        /// <returns>返回新生成的结算内码List</returns>
        public IList<int> AddSettlement(string orgId, string mzh
            , OutpatientSettlementAddBO settlementAddBO
            , OutpatientSettFeeRelatedDTO feeRelated = null
            , CQMzjs05Dto ybfeeRelated = null
            , S25ResponseDTO xnhybfeeRelated = null
            , string outTradeNo = null)
        {
            var newJsnmList = new List<int>();

            settlementAddBO.cfnmList = settlementAddBO.cfnmList ?? new List<int>();
            settlementAddBO.xmnmList = settlementAddBO.xmnmList ?? new List<int>();

            if (settlementAddBO.cfnmList.Count == 0 && settlementAddBO.xmnmList.Count == 0)
            {
                return null;
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var mzghEntity = db.IQueryable<OutpatientRegistEntity>(p => p.mzh == mzh && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                if (mzghEntity == null)
                {
                    throw new FailedException("REGISTINFO_NOT_FOUND", "关联挂号信息异常");
                }
                mzghEntity.ghzt = "1";  //无差别至已结状态
                db.Update(mzghEntity);

                //重复结的判断
                IList<int> jsnmList = new List<int>();
                if (settlementAddBO.xmnmList.Count > 0)
                {
                    var jT = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.zt == "1" && p.mxnm.HasValue && settlementAddBO.xmnmList.Contains(p.mxnm.Value)).Select(p => p.jsnm).ToList();
                    jsnmList = jsnmList.Union(jT).ToList();
                }
                if (settlementAddBO.cfnmList.Count > 0)
                {
                    var cfmxIdList = db.IQueryable<OutpatientPrescriptionDetailEntity>(p => settlementAddBO.cfnmList.Contains(p.cfnm)).Select(p => p.cfmxId).ToList();
                    if (cfmxIdList.Count > 0)
                    {
                        var jT = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.zt == "1" && p.cf_mxnm.HasValue && cfmxIdList.Contains(p.cf_mxnm.Value)).Select(p => p.jsnm).ToList();
                        jsnmList = jsnmList.Union(jT).ToList();
                    }
                }

                if (jsnmList.Count > 0)
                {
                    if (db.IQueryable<OutpatientSettlementEntity>(p => p.zt == "1" && p.tbz != 1 && jsnmList.Contains(p.jsnm)).Any())
                    {
                        throw new FailedException("REPEATED_SETTLED_ERROR", "重复结算");
                    }
                }

                //结
                var beaddjsCfzbEntityList = db.IQueryable<OutpatientPrescriptionEntity>(p => p.zt == "1" && settlementAddBO.cfnmList.Contains(p.cfnm)).ToList();
                
                var beaddjsMzxmEntityList = db.IQueryable<OutpatientItemEntity>(p => p.zt == "1" && settlementAddBO.xmnmList.Contains(p.xmnm)).ToList();
                if (beaddjsCfzbEntityList.Count > 0 || beaddjsMzxmEntityList.Count > 0)
                {
                    if (settlementAddBO.settSfrq.HasValue || settlementAddBO.isQfyj)
                    {
                        //作为一个结算
                        var jsnm = AddSettlement(db, orgId
                            , settlementAddBO.settSfrq, beaddjsCfzbEntityList
                            , beaddjsMzxmEntityList, mzghEntity, feeRelated, ybfeeRelated
                            , xnhybfeeRelated: xnhybfeeRelated
                            , autoGenePlan: settlementAddBO.autoGenePlan
                            , isQfyj: settlementAddBO.isQfyj
                            , fph: (settlementAddBO.isQfyj ? null : settlementAddBO.fph)
                            , outTradeNo: outTradeNo
                            );
                        newJsnmList.Add(jsnm);
                    }
                    else
                    {
                        //未指定收费日期 一定是非常规途径收费结算
                        //肯定不会关联发票号
                        var sfrqList = beaddjsCfzbEntityList.Select(p => p.sfrq)
                            .Union(beaddjsMzxmEntityList.Select(p => p.sfrq)).Distinct()
                            .ToList();
                        if (!settlementAddBO.sfrqWithSameJs)
                        {
                            foreach (var sfrq in sfrqList)
                            {
                                //null会单独生成一个结算
                                var thiscfzbEntityList = beaddjsCfzbEntityList.Where(p => p.sfrq == sfrq).ToList();
                                var thismzxmEntityList = beaddjsMzxmEntityList.Where(p => p.sfrq == sfrq).ToList();
                                var jsnm = AddSettlement(db, orgId, sfrq, thiscfzbEntityList, thismzxmEntityList, mzghEntity, null, null, null, autoGenePlan: settlementAddBO.autoGenePlan);
                                newJsnmList.Add(jsnm);
                            }
                        }
                        else
                        {
                            DateTime? sfrq = null;
                            if (sfrqList.Count == 1 && sfrqList[0].HasValue)
                            {
                                sfrq = sfrqList[0];
                            }
                            //一个结算
                            var jsnm = AddSettlement(db, orgId, sfrq, beaddjsCfzbEntityList, beaddjsMzxmEntityList, mzghEntity, null, null, null, autoGenePlan: settlementAddBO.autoGenePlan);
                            newJsnmList.Add(jsnm);
                        }
                    }
                }

                db.Commit();    //没有要Commit的会抛异常？
            }

            return newJsnmList;
        }

        /// <summary>
        /// 门诊退费（不用关联门诊处方、项目）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jsnm"></param>
        /// <param name="tjsxmDict">退结算明细List jsmxnm:退数量 退数量 正数</param>
        /// <param name="expectedTmxZje">预期退费项目金额合计（单价*退数量），防止过程中数据发生变更</param>
        /// <param name="newfph">新结算发票号</param>
        /// <param name="newJszbInfo">生成的新结算结算信息</param>
        /// <param name="outTradeNo">原支付交易号</param>
        /// <param name="refundAmount">退现金金额（现金、支付宝、微信等）</param>
        /// <param name="hcybjsh">退费.医保交易.红冲结算号</param>
        /// <param name="newwjybjsh">退费.医保交易.新的结算号</param>
        /// <param name="hcybfeeRelated">红冲结算 的 结算医保金额相关</param>
        /// <param name="newybfeeRelated">退费.医保交易.新的结算 结算医保返回费用信息</param>
        /// <param name="guiannewybfeeRelated">贵安和重庆医保使用，由于医保返回与常熟字段不同</param>
        /// <param name="s25ResponseDto">贵安新农合医保使用，结算返回结果</param>
        /// <param name="outpId">贵安新农合医保使用，门诊补偿序号</param>
        public void RefundSettlement(string orgId, int jsnm
            , Dictionary<int, decimal> tjsxmDict, decimal expectedTmxZje
            , string newfph
            , out object newJszbInfo, out string outTradeNo, out decimal refundAmount, string hcybjsh = null, string newwjybjsh = null
            , CQMzjs05Dto hcybfeeRelated = null
            , OutpatientSettYbFeeRelatedDTO newybfeeRelated = null
            , CQMzjs05Dto guiannewybfeeRelated = null
            , GuiAnMztfProDto guianMztfProDto = null
            , S25ResponseDTO s25ResponseDto = null
            , string outpId = "")
        {
            bool ytw = false;

            outTradeNo = null;
            refundAmount = 0;
            var medicalInsurance = _sysConfigRepo.GetValueByCode("Outpatient_MedicalInsurance", orgId);
            decimal oldxjzf;
            OutpatientSettlementEntity newJszbEntity = null;

            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var oldJszbEntity = db.IQueryable<OutpatientSettlementEntity>(p => p.jsnm == jsnm && p.zt == "1" && (p.tbz == null || p.tbz == 0)).FirstOrDefault();
                var oldJsmxEntityList = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.jsnm == jsnm && p.zt == "1").ToList();
                oldxjzf = oldJszbEntity.xjzf;
                //insert list
                var newJszbEntityList = new List<OutpatientSettlementEntity>();
                var newJsmxEntityList = new List<OutpatientSettlementDetailEntity>();

                //先退流水 //对冲
                var newDcJszbEntity = oldJszbEntity.Clone();
                newDcJszbEntity.LastModifyTime = null;
                newDcJszbEntity.LastModifierCode = null;
                newDcJszbEntity.jsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_js");
                newDcJszbEntity.tbz = 1;  //退或被退 都是1
                newDcJszbEntity.jszt = (int)EnumJieSuanZT.YT;   //已退
                newDcJszbEntity.cxjsnm = oldJszbEntity.jsnm;    //关联oldjsnm
                newJszbEntityList.Add(newDcJszbEntity);
                foreach (var jsmxEntity in oldJsmxEntityList)
                {
                    var newJsmxEntity = jsmxEntity.Clone();
                    newJsmxEntity.LastModifyTime = null;
                    newJsmxEntity.LastModifierCode = null;
                    newJsmxEntity.jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx");
                    newJsmxEntity.jsnm = newDcJszbEntity.jsnm;

                    newJsmxEntityList.Add(newJsmxEntity);
                }

                //后新
                newJszbEntity = oldJszbEntity.Clone();
                newJszbEntity.LastModifyTime = null;
                newJszbEntity.LastModifierCode = null;
                newJszbEntity.jsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_js");
                newJszbEntity.fph = newfph;
                newJszbEntity.ysk = null;   //2018-11-14退费产生的新结算 预收款默认用这一次的现金支付（门诊收据显示）
                newJszbEntity.zl = null;
                newJszbEntity.ybjslsh = newwjybjsh;
                newJszbEntityList.Add(newJszbEntity);

                UpdateCurrentFinancialInvoice(db, orgId, newfph);

                newJszbEntity.zje = 0;  //foreach内累加
                decimal tjehj = Convert.ToDecimal(0.0000);  //预期退费项目金额合计（单价*退数量） //foreach内累加
				foreach (var jsmxEntity in oldJsmxEntityList)
                {
                    var newJsmxEntity = jsmxEntity.Clone();
                    if (newJsmxEntity.cf_mxnm > 0)
                    {
                        //药品 已退过，那可退数量就是0 必须等待药房发起新的退药
                        newJsmxEntity.ktsl = 0;
                    }
                    newJsmxEntity.LastModifyTime = null;
                    newJsmxEntity.LastModifierCode = null;
                    newJsmxEntity.jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx");
                    newJsmxEntity.jsnm = newJszbEntity.jsnm;
                    //
                    decimal tsl = 0;
                    if (tjsxmDict.TryGetValue(jsmxEntity.jsmxnm, out tsl))
                    {
                        //要退
                        if (jsmxEntity.sl > 0)
                        {
                            decimal thisDj = 0;
                            if (jsmxEntity.cf_mxnm > 0)
                            {
                                var thisMxEntity = db.IQueryable<OutpatientPrescriptionDetailEntity>(p => p.cfmxId == jsmxEntity.cf_mxnm).FirstOrDefault();
                                if (thisMxEntity == null)
                                {
                                    throw new FailedException("处方药品明细数据错误，未找到");
                                }
                                thisDj = thisMxEntity.dj;
                            }
                            else if (jsmxEntity.mxnm > 0)
                            {
                                var thisMxEntity = db.IQueryable<OutpatientItemEntity>(p => p.xmnm == jsmxEntity.mxnm).FirstOrDefault();
                                if (thisMxEntity == null)
                                {
                                    throw new FailedException("门诊项目数据错误，未找到");
                                }
                                thisDj = thisMxEntity.dj;
                            }
                            newJsmxEntity.sl = newJsmxEntity.sl - Math.Abs(tsl);
                            newJsmxEntity.jyje = newJsmxEntity.sl * thisDj;

                            tjehj += Math.Abs(tsl) * thisDj;
                        }
                        else
                        {
                            throw new FailedException("数量为0，不可再退！");
                        }
                    }

                    newJszbEntity.zje += (newJsmxEntity.jyje ?? 0);
                    newJsmxEntityList.Add(newJsmxEntity);
                }
				////去除 2019-09-12 因四位小数问题，频繁报错，考虑去除
				if (tjehj != expectedTmxZje)
				{
					throw new FailedException("退款金额异常.");
				}

                if (hcybfeeRelated != null && medicalInsurance == "changshu")
                {
                    //这是一次医保退费
                    newJszbEntity.xjzf = newybfeeRelated == null ? 0 : (newybfeeRelated.XJZF ?? 0);
                    //2018-11-14退费产生的新结算 预收款默认用这一次的现金支付（门诊收据显示）
                    newJszbEntity.ysk = newJszbEntity.xjzf;
                    ytw = newybfeeRelated == null || (newybfeeRelated.ZFY ?? 0) <= 0;   //是否已退完

                    //计算退现金金额   //前提：仅一种支付方式    //仅一种支付方式mz_js.xjzffs会有值
                    if (!string.IsNullOrWhiteSpace(newJszbEntity.xjzffs) && oldJszbEntity.xjzf - newJszbEntity.xjzf > 0)
                    {
                        refundAmount = oldJszbEntity.xjzf - newJszbEntity.xjzf;  //应该取支付的
                        outTradeNo = newJszbEntity.OutTradeNo;
                    }
                }
                else if (!string.IsNullOrEmpty(hcybjsh) && guiannewybfeeRelated != null && medicalInsurance == "guian")
                {
                    //这是一次医保退费
                    newJszbEntity.xjzf = guianMztfProDto == null ? 0 : (guianMztfProDto.XJZF ?? 0);
                    //2018-11-14退费产生的新结算 预收款默认用这一次的现金支付（门诊收据显示）
                    newJszbEntity.ysk = newJszbEntity.xjzf;
                    ytw = guianMztfProDto == null || (guianMztfProDto.ZFY ?? 0) <= 0;   //是否已退完

                    //计算退现金金额   //前提：仅一种支付方式    //仅一种支付方式mz_js.xjzffs会有值
                    if (!string.IsNullOrWhiteSpace(newJszbEntity.xjzffs) && oldJszbEntity.xjzf - newJszbEntity.xjzf > 0)
                    {
                        refundAmount = oldJszbEntity.xjzf - newJszbEntity.xjzf;  //应该取支付的
                        outTradeNo = newJszbEntity.OutTradeNo;
                    }
                }
                else if (guiannewybfeeRelated != null)// (!string.IsNullOrEmpty(hcybjsh) && guiannewybfeeRelated != null && medicalInsurance == "chongqing")
                {
                    //这是一次医保退费
                    newJszbEntity.xjzf = guianMztfProDto == null ? 0 : (guianMztfProDto.XJZF ?? 0);
                    //2018-11-14退费产生的新结算 预收款默认用这一次的现金支付（门诊收据显示）
                    newJszbEntity.ysk = newJszbEntity.xjzf;
                    ytw = guianMztfProDto == null || (guianMztfProDto.ZFY ?? 0) <= 0;   //是否已退完

                    //计算退现金金额   //前提：仅一种支付方式    //仅一种支付方式mz_js.xjzffs会有值
                    if (!string.IsNullOrWhiteSpace(newJszbEntity.xjzffs) && oldJszbEntity.xjzf - newJszbEntity.xjzf > 0)
                    {
                        refundAmount = oldJszbEntity.xjzf - newJszbEntity.xjzf;  //应该取支付的
                        outTradeNo = newJszbEntity.OutTradeNo;
                    }
                }
				else if (s25ResponseDto != null && medicalInsurance == "guian")
                {
                    newJszbEntity.xjzf = newJszbEntity.zje - HIS.Proxy.guian.Common.CalculationXnhZhzf(s25ResponseDto);
                    newJszbEntity.ysk = newJszbEntity.xjzf;
                    ytw = oldJszbEntity.zje == newJszbEntity.zje;//是否已退完

                    //计算退现金金额   //前提：仅一种支付方式    //仅一种支付方式mz_js.xjzffs会有值
                    if (!string.IsNullOrWhiteSpace(newJszbEntity.xjzffs) && oldJszbEntity.xjzf - newJszbEntity.xjzf > 0)
                    {
                        refundAmount = oldJszbEntity.xjzf - newJszbEntity.xjzf;  //应该取支付的
                        outTradeNo = newJszbEntity.OutTradeNo;
                    }
                }
                else
                {
                    if (oldJszbEntity.xjzf != oldJszbEntity.zje)
                    {
                        //非全自费结算记录
                        if (tjehj != oldJszbEntity.zje)
                        {
                            throw new FailedException("非全自费结算记录只支持全退，请重试");
                        }
                        newJszbEntity.xjzf = 0; //退完了
                        ytw = true;   //已退完

                        //计算退现金金额   //前提：仅一种支付方式    //仅一种支付方式mz_js.xjzffs会有值
                        if (!string.IsNullOrWhiteSpace(newJszbEntity.xjzffs) && oldJszbEntity.xjzf > 0)
                        {
                            refundAmount = oldJszbEntity.xjzf;  //应该取支付的
                            outTradeNo = newJszbEntity.OutTradeNo;
                        }
                    }
                    else
                    {
                        newJszbEntity.xjzf = newJszbEntity.zje;
                        //2018-11-14退费产生的新结算 预收款默认用这一次的现金支付（门诊收据显示）
                        newJszbEntity.ysk = newJszbEntity.xjzf;
                        ytw = newJszbEntity.zje <= 0;   //是否已退完

                        //计算退现金金额   //前提：仅一种支付方式    //仅一种支付方式mz_js.xjzffs会有值
                        if (!string.IsNullOrWhiteSpace(newJszbEntity.xjzffs) && oldJszbEntity.xjzf - newJszbEntity.xjzf > 0)
                        {
                            refundAmount = oldJszbEntity.xjzf - newJszbEntity.xjzf;  //应该取支付的
                            outTradeNo = newJszbEntity.OutTradeNo;
                        }
                    }
                }

                //变更数据库
                oldJszbEntity.tbz = 1;
                oldJszbEntity.Modify();
                db.Update(oldJszbEntity);
                //
                for (var iIndex = 0; iIndex < newJszbEntityList.Count; iIndex++)
                {
                    newJszbEntityList[iIndex].Create();
                    newJszbEntityList[iIndex].CreateTime = DateTime.Now.AddSeconds(iIndex);
                    db.Insert(newJszbEntityList[iIndex]);
                }
                //
                foreach (var jsmxEntity in newJsmxEntityList)
                {
                    jsmxEntity.Create();
                    db.Insert(jsmxEntity);
                }

                var isYbjyjz = false;

                switch (medicalInsurance)
                {
                    case "guian":
                        isYbjyjz = !string.IsNullOrWhiteSpace(hcybjsh) && !string.IsNullOrWhiteSpace(newwjybjsh);
                        break;
	                case "chongqing":
		                isYbjyjz = !string.IsNullOrWhiteSpace(hcybjsh) && !string.IsNullOrWhiteSpace(newwjybjsh);
		                break;
					case "changshu":
                        isYbjyjz = !string.IsNullOrWhiteSpace(hcybjsh) && !string.IsNullOrWhiteSpace(newwjybjsh) && hcybfeeRelated != null;
                        break;
                }

                #region 医保结算相关
                if (isYbjyjz)
                {

                    if (!string.IsNullOrWhiteSpace(medicalInsurance) && medicalInsurance == "changshu")
                    {
                        #region 常熟医保逻辑
                        var oldJsYbFeeEntity = db.IQueryable<OutpatientSettlementYBFeeEntity>(p => p.jsnm == jsnm && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                        if (oldJsYbFeeEntity != null)
                        {
                            var hcybFeeEntity = new OutpatientSettlementYBFeeEntity() { };
                            hcybFeeEntity.jsnm = newDcJszbEntity.jsnm;
                            hcybFeeEntity.ybjsh = hcybjsh;
                            hcybFeeEntity.OrganizeId = orgId;
                            hcybFeeEntity.ZFY = hcybfeeRelated.ZFY;
                            hcybFeeEntity.XJZF = hcybfeeRelated.XJZF;
                            hcybFeeEntity.YBFY = hcybfeeRelated.YBFY;
                            hcybFeeEntity.TSFY = hcybfeeRelated.TSFY;
                            hcybFeeEntity.JBZF = hcybfeeRelated.JBZF;
                            hcybFeeEntity.GBZF = hcybfeeRelated.GBZF;
                            hcybFeeEntity.SUMZFDYBFY = hcybfeeRelated.SUMZFDYBFY;
                            hcybFeeEntity.ZFDYBFY = hcybfeeRelated.ZFDYBFY;
                            hcybFeeEntity.JBYE = hcybfeeRelated.JBYE;
                            hcybFeeEntity.GBYE = hcybfeeRelated.GBYE;
                            hcybFeeEntity.KBXYBFY = hcybfeeRelated.KBXYBFY;
                            hcybFeeEntity.KBXTSFY = hcybfeeRelated.KBXTSFY;
                            hcybFeeEntity.TCZF = hcybfeeRelated.TCZF;
                            hcybFeeEntity.JZZF = hcybfeeRelated.JZZF;
                            hcybFeeEntity.DKC023 = hcybfeeRelated.DKC023;
                            hcybFeeEntity.Create();
                            db.Insert(hcybFeeEntity);
                        }

                        var newybFeeEntity = new OutpatientSettlementYBFeeEntity() { };
                        newybFeeEntity.jsnm = newJszbEntity.jsnm;
                        newybFeeEntity.ybjsh = newwjybjsh;
                        newybFeeEntity.OrganizeId = orgId;
                        newybFeeEntity.ZFY = newybfeeRelated == null ? null : newybfeeRelated.ZFY;
                        newybFeeEntity.XJZF = newybfeeRelated == null ? null : newybfeeRelated.XJZF;
                        newybFeeEntity.YBFY = newybfeeRelated == null ? null : newybfeeRelated.YBFY;
                        newybFeeEntity.TSFY = newybfeeRelated == null ? null : newybfeeRelated.TSFY;
                        newybFeeEntity.JBZF = newybfeeRelated == null ? null : newybfeeRelated.JBZF;
                        newybFeeEntity.GBZF = newybfeeRelated == null ? null : newybfeeRelated.GBZF;
                        newybFeeEntity.SUMZFDYBFY = newybfeeRelated == null ? null : newybfeeRelated.SUMZFDYBFY;
                        newybFeeEntity.ZFDYBFY = newybfeeRelated == null ? null : newybfeeRelated.ZFDYBFY;
                        //全退时，不产生新的结算，余额则取红冲那一条结算的余额
                        newybFeeEntity.JBYE = newybfeeRelated == null ? hcybfeeRelated.JBYE : newybfeeRelated.JBYE;
                        newybFeeEntity.GBYE = newybfeeRelated == null ? hcybfeeRelated.GBYE : newybfeeRelated.GBYE;
                        newybFeeEntity.KBXYBFY = newybfeeRelated == null ? null : newybfeeRelated.KBXYBFY;
                        newybFeeEntity.KBXTSFY = newybfeeRelated == null ? null : newybfeeRelated.KBXTSFY;
                        newybFeeEntity.TCZF = newybfeeRelated == null ? null : newybfeeRelated.TCZF;
                        newybFeeEntity.JZZF = newybfeeRelated == null ? null : newybfeeRelated.JZZF;
                        newybFeeEntity.DKC023 = newybfeeRelated == null ? null : newybfeeRelated.DKC023;
                        newybFeeEntity.Create();
                        db.Insert(newybFeeEntity);
                        #endregion
                    }
                    if (!string.IsNullOrWhiteSpace(medicalInsurance) && medicalInsurance == "guian")
                    {
                        #region 贵安医保逻辑
                        var oldJsYbFeeEntity = db.IQueryable<OutpatientSettlementGAYBFeeEntity>(p => p.jsnm == jsnm && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
                        if (oldJsYbFeeEntity != null)
                        {
                            var hcybFeeEntity = new OutpatientSettlementGAYBFeeEntity() { };
                            hcybFeeEntity.jsnm = newDcJszbEntity.jsnm;
                            hcybFeeEntity.OrganizeId = orgId;
                            hcybFeeEntity.prm_yka198 = hcybjsh;
                            hcybFeeEntity.Create();
                            db.Insert(hcybFeeEntity);
                        }
                        var newybFeeEntity = new OutpatientSettlementGAYBFeeEntity()
                        {
                            OrganizeId = orgId,
                            astr_jylsh = guianMztfProDto.astr_jylsh,
                            astr_jyyzm = guianMztfProDto.astr_jyyzm,
                            aint_appcode = guianMztfProDto.aint_appcode,
                            astr_appmsg = guianMztfProDto.astr_appmsg,
                            prm_akc190 = guiannewybfeeRelated.prm_akc190,
                            prm_yab003 = guiannewybfeeRelated.prm_yab003,
                            prm_yka103 = guiannewybfeeRelated.prm_yka103,
                            prm_aac001 = guiannewybfeeRelated.prm_aac001,
                            prm_yka065 = guiannewybfeeRelated.prm_yka065,
                            prm_aae036 = guiannewybfeeRelated.prm_aae036,
                            prm_yka055 = guiannewybfeeRelated.prm_yka055,
                            prm_yka056 = guiannewybfeeRelated.prm_yka056,
                            prm_yka057 = guiannewybfeeRelated.prm_yka057,
                            prm_yka111 = guiannewybfeeRelated.prm_yka111,
                            prm_yka058 = guiannewybfeeRelated.prm_yka058,
                            prm_yka248 = guiannewybfeeRelated.prm_yka248,
                            prm_yka062 = guiannewybfeeRelated.prm_yka062,
                            prm_yke030 = guiannewybfeeRelated.prm_yke030,
                            prm_ake032 = guiannewybfeeRelated.prm_ake032,
                            prm_ake181 = guiannewybfeeRelated.prm_ake181,
                            prm_ake173 = guiannewybfeeRelated.prm_ake173,
                            prm_akc087 = guiannewybfeeRelated.prm_akc087,
                            prm_ykb037 = guiannewybfeeRelated.prm_ykb037,
                            prm_yka316 = guiannewybfeeRelated.prm_yka316,
                            prm_akc021 = guiannewybfeeRelated.prm_akc021,
                            prm_ykc120 = guiannewybfeeRelated.prm_ykc120,
                            prm_yab139 = guiannewybfeeRelated.prm_yab139,
                            prm_aac003 = guiannewybfeeRelated.prm_aac003,
                            prm_aac004 = guiannewybfeeRelated.prm_aac004,
                            prm_aac002 = guiannewybfeeRelated.prm_aac002,
                            prm_aac006 = guiannewybfeeRelated.prm_aac006,
                            prm_akc023 = guiannewybfeeRelated.prm_akc023,
                            prm_aab001 = guiannewybfeeRelated.prm_aab001,
                            prm_aab004 = guiannewybfeeRelated.prm_aab004,
                            prm_aac031 = guiannewybfeeRelated.prm_aac031,
                            prm_ykc280 = guiannewybfeeRelated.prm_ykc280,
                            prm_ykc281 = guiannewybfeeRelated.prm_ykc281,
                            prm_yka054 = guiannewybfeeRelated.prm_yka054,
                            prm_yae366 = guiannewybfeeRelated.prm_yae366,
                            prm_akc090 = guiannewybfeeRelated.prm_akc090,
                            prm_yka120 = guiannewybfeeRelated.prm_yka120,
                            prm_yka122 = guiannewybfeeRelated.prm_yka122,
                            prm_yka368 = guiannewybfeeRelated.prm_yka368,
                            prm_yke025 = guiannewybfeeRelated.prm_yke025,
                            prm_yka900 = guiannewybfeeRelated.prm_yka900,
                            prm_aae001 = guiannewybfeeRelated.prm_aae001,
                            prm_yka089 = guiannewybfeeRelated.prm_yka089,
                            prm_yka027 = guiannewybfeeRelated.prm_yka027,
                            prm_yka028 = guiannewybfeeRelated.prm_yka028,
                            prm_yka345 = guiannewybfeeRelated.prm_yka345,
                            jsnm = newJszbEntity.jsnm
                        };
                        newybFeeEntity.Create();
                        db.Insert(newybFeeEntity);
                        #endregion
                    }
	                if (!string.IsNullOrWhiteSpace(medicalInsurance) && medicalInsurance == "chongqing")
	                {
		                #region 重庆医保逻辑
		                var oldJsYbFeeEntity = db.IQueryable<CqybSett05Entity>(p => p.jsnm == jsnm && p.OrganizeId == orgId && p.zt == "1").FirstOrDefault();
						//红冲交易落地
		                if (oldJsYbFeeEntity != null && hcybfeeRelated !=null)
		                {
                            var hcybFeeEntity = new CqybSett05Entity() {
                                jslb = "2",
                                OrganizeId = orgId,
                                jsnm = jsnm,
                                jylsh = hcybfeeRelated.jylsh,
                                cqtczf = hcybfeeRelated.cqtczf,
                                zhzf = hcybfeeRelated.zhzf,
                                gwybz = hcybfeeRelated.gwybz,
                                cqxjzf = hcybfeeRelated.cqxjzf,
                                delpje = hcybfeeRelated.delpje,
                                lsqfxgwyff = hcybfeeRelated.lsqfxgwyff,
                                zhye = hcybfeeRelated.zhye,
                                dbzddyljgdz = hcybfeeRelated.dbzddyljgdz,
                                mzjzje = hcybfeeRelated.mzjzje,
                                mzjzmzye = hcybfeeRelated.mzjzmzye,
                                ndyxmzfje = hcybfeeRelated.ndyxmzfje,
                                ybzlzfs = hcybfeeRelated.ybzlzfs,
                                shjzjjzfs = hcybfeeRelated.shjzjjzfs,
                                bntczflj = hcybfeeRelated.bntczflj,
                                bndezflj = hcybfeeRelated.bndezflj,
                                tbqfxzflj = hcybfeeRelated.tbqfxzflj,
                                ndyxmlj = hcybfeeRelated.ndyxmlj,
                                bnmzjzzyzflj = hcybfeeRelated.bnmzjzzyzflj,
                                zxjssj = DateTime.Now,//hcybfeeRelated.zxjssj,
				                bcqfxzfje = hcybfeeRelated.bcqfxzfje,
				                bcjrybfwfy = hcybfeeRelated.bcjrybfwfy,
				                ysfwzfs = hcybfeeRelated.ysfwzfs,
				                yycbkkje = hcybfeeRelated.yycbkkje,
				                syjjzf = hcybfeeRelated.syjjzf,
				                syxjzf = hcybfeeRelated.syxjzf,
				                gsjjzf = hcybfeeRelated.gsjjzf,
				                gsxjzf = hcybfeeRelated.gsxjzf,
				                gsdbzjgdz = hcybfeeRelated.gsdbzjgdz,
				                gsqzfyy = hcybfeeRelated.gsqzfyy,
				                qtbz = hcybfeeRelated.qtbz,
				                syzhzf = hcybfeeRelated.syzhzf,
				                gszhzf = hcybfeeRelated.gszhzf,
				                bcjssfjkfpry = hcybfeeRelated.bcjssfjkfpry,
				                jkfpyljj = hcybfeeRelated.jkfpyljj,
				                jztpbxje = hcybfeeRelated.jztpbxje,
				                zt = "1"
			                };
			                hcybFeeEntity.Create();
			                db.Insert(hcybFeeEntity);
		                }
		                if (guiannewybfeeRelated !=null && guiannewybfeeRelated.jylsh != "111111111")
		                {
							//医保再结信息落地
			                var newybFeeEntity = new CqybSett05Entity()
			                {
				                jslb = "1",
				                OrganizeId = orgId,
				                jsnm = newJszbEntity.jsnm,
				                jylsh = guiannewybfeeRelated.jylsh,
				                cqtczf = guiannewybfeeRelated.cqtczf,
				                zhzf = guiannewybfeeRelated.zhzf,
				                gwybz = guiannewybfeeRelated.gwybz,
				                cqxjzf = guiannewybfeeRelated.cqxjzf,
				                delpje = guiannewybfeeRelated.delpje,
				                lsqfxgwyff = guiannewybfeeRelated.lsqfxgwyff,
				                zhye = guiannewybfeeRelated.zhye,
				                dbzddyljgdz = guiannewybfeeRelated.dbzddyljgdz,
				                mzjzje = guiannewybfeeRelated.mzjzje,
				                mzjzmzye = guiannewybfeeRelated.mzjzmzye,
				                ndyxmzfje = guiannewybfeeRelated.ndyxmzfje,
				                ybzlzfs = guiannewybfeeRelated.ybzlzfs,
				                shjzjjzfs = guiannewybfeeRelated.shjzjjzfs,
				                bntczflj = guiannewybfeeRelated.bntczflj,
				                bndezflj = guiannewybfeeRelated.bndezflj,
				                tbqfxzflj = guiannewybfeeRelated.tbqfxzflj,
				                ndyxmlj = guiannewybfeeRelated.ndyxmlj,
				                bnmzjzzyzflj = guiannewybfeeRelated.bnmzjzzyzflj,
				                zxjssj = guiannewybfeeRelated.zxjssj,
				                bcqfxzfje = guiannewybfeeRelated.bcqfxzfje,
				                bcjrybfwfy = guiannewybfeeRelated.bcjrybfwfy,
				                ysfwzfs = guiannewybfeeRelated.ysfwzfs,
				                yycbkkje = guiannewybfeeRelated.yycbkkje,
				                syjjzf = guiannewybfeeRelated.syjjzf,
				                syxjzf = guiannewybfeeRelated.syxjzf,
				                gsjjzf = guiannewybfeeRelated.gsjjzf,
				                gsxjzf = guiannewybfeeRelated.gsxjzf,
				                gsdbzjgdz = guiannewybfeeRelated.gsdbzjgdz,
				                gsqzfyy = guiannewybfeeRelated.gsqzfyy,
				                qtbz = guiannewybfeeRelated.qtbz,
				                syzhzf = guiannewybfeeRelated.syzhzf,
				                gszhzf = guiannewybfeeRelated.gszhzf,
				                bcjssfjkfpry = guiannewybfeeRelated.bcjssfjkfpry,
				                jkfpyljj = guiannewybfeeRelated.jkfpyljj,
				                jztpbxje = guiannewybfeeRelated.jztpbxje,
				                zt = "1"
			                };
			                newybFeeEntity.Create();
			                db.Insert(newybFeeEntity);
						}
						
		                #endregion
	                }
				}
                #endregion 医保结算相关

                if (s25ResponseDto != null)
                {
                    if (!string.IsNullOrWhiteSpace(medicalInsurance) && medicalInsurance == "guian")
                    {
                        #region 贵安新农合医保逻辑
                        var xnhSett = new OutpatientXnhSettlementResultEntity
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrganizeId = orgId,
                            jsnm = newDcJszbEntity.jsnm,
                            outpId = outpId,
                            jszt = 1,
                            accRedeem = s25ResponseDto.accRedeem.ToString(CultureInfo.InvariantCulture),
                            bottomRedeem = s25ResponseDto.bottomRedeem ?? "",
                            bottomSecondRedeem = s25ResponseDto.bottomSecondRedeem ?? "",
                            civilCost = s25ResponseDto.civilCost ?? "",
                            compensateCost = s25ResponseDto.compensateCost ?? "",
                            continentInsuranceCost = s25ResponseDto.continentInsuranceCost ?? "",
                            generalRedeem = s25ResponseDto.generalRedeem ?? "",
                            totalCost = s25ResponseDto.totalCost,
                            insuranceCost = s25ResponseDto.insuranceCost,
                            undulatingLine = s25ResponseDto.undulatingLine,
                            insureCost = s25ResponseDto.insureCost ?? "",
                            salvaCLCost = s25ResponseDto.salvaCLCost ?? "",
                            salvaFPCost = s25ResponseDto.salvaFPCost ?? "",
                            salvaJKCost = s25ResponseDto.salvaJKCost ?? "",
                            salvaJSCost = s25ResponseDto.salvaJSCost ?? "",
                            salvaMZCost = s25ResponseDto.salvaMZCost ?? "",
                            salvaYFCost = s25ResponseDto.salvaYFCost ?? "",
                            medicineCost = s25ResponseDto.medicineCost ?? "",
                            specialPovertyCost = s25ResponseDto.specialPovertyCost ?? "",
                            medicalAssistanceCost = s25ResponseDto.medicalAssistanceCost ?? "",
                            medicalSecondCost = s25ResponseDto.medicalSecondCost ?? "",
                        };
                        xnhSett.Create();
                        db.Insert(xnhSett);
                        if (s25ResponseDto.list != null && s25ResponseDto.list.Any())
                        {
                            var xnhSettResultList = new List<OutpatientXnhSettlementCalcDetailEntity>();
                            s25ResponseDto.list.ForEach(p =>
                            {
                                var item = new OutpatientXnhSettlementCalcDetailEntity
                                {
                                    resultId = xnhSett.Id,
                                    OrganizeId = orgId,
                                    calcAfter = p.calcAfter,
                                    calcBefore = p.calcBefore,
                                    calcMemo = p.calcMemo,
                                    calcName = p.calcName
                                };
                                item.Create();
                                xnhSettResultList.Add(item);
                            });
                            db.Insert(xnhSettResultList);
                        }
                        #endregion
                    }
                }

                bool isGhToYT = newJszbEntity.jslx == ((int)EnumJslx.GH).ToString() && newJszbEntity.zje <= 0;
                //或者仅单一结算，且已退完，则号状态已退
                isGhToYT = isGhToYT || (isYbjyjz && ytw);
                if (!isGhToYT && newJszbEntity.zje <= 0)
                {
                    //有一种情况，挂号时未产生结算
                    //配置：当费用退完时，号是否自动退
                    var configFeeYtwghtoYt = _sysConfigRepo.GetBoolValueByCode("Outpatient_Refund_FeeYTW_GHToYT", orgId, false);
                    if (configFeeYtwghtoYt.Value)
                    {
                        if (!db.IQueryable<OutpatientSettlementEntity>(p => p.ghnm == newJszbEntity.ghnm && p.zt == "1" && p.tbz != 1 && p.zje > 0
                        && p.jsnm != jsnm && p.jsnm != newJszbEntity.jsnm && p.jsnm != newDcJszbEntity.jsnm).Any())
                        {
                            isGhToYT = true;
                        }
                    }
                }
                if (isGhToYT && medicalInsurance != "chongqing") //重庆医保由于挂号收费，不能自动作废挂号
                {
                    //置挂号状态 已退
                    var ghEnitty = db.IQueryable<OutpatientRegistEntity>(p => p.ghnm == newJszbEntity.ghnm && p.zt == "1" && p.ghzt == "1").FirstOrDefault();
                    if (ghEnitty == null)
                    {
                        throw new FailedException("内部错误，his本地挂号无法退号：未找到有效状态的挂号数据，挂号内码："+ newJszbEntity.ghnm.ToString());
                    }
                    ghEnitty.ghzt = "2";    //已退
                    ghEnitty.Modify();
                    db.Update(ghEnitty);
                }

                //如果之前是预交金支付，要还款(单一支付方式情况 多种组合支付暂只能退现金)
                var preYjjPayEntity = db.IQueryable<OutpatientSettlementPaymentModelEntity>(p => p.jsnm == jsnm && p.zfje > 0 && p.zt=="1").ToList();
                if (preYjjPayEntity.Count ==1 && preYjjPayEntity[0].xjzffs== xtzffs.ZYYJZHZF) {
                    int zhcode = (int)preYjjPayEntity.FirstOrDefault().zh;
                    //给预交金账户充值- 部分退情况先全退回账户再insert预交金扣减记录
                    var accountEntity =
                        db.IQueryable<SysAccountEntity>(p => p.patid == oldJszbEntity.patid && p.zhCode == zhcode && p.zt=="1")
                            .FirstOrDefault();
                    if (accountEntity != null)
                    {
                        var zhszEntity = new SysAccountRevenueAndExpenseEntity()
                        {
                            OrganizeId = oldJszbEntity.OrganizeId,
                            patid= oldJszbEntity.patid,
                            zhCode = accountEntity.zhCode,
                            szje = preYjjPayEntity.FirstOrDefault().zfje,
                            zhye = accountEntity.zhye + preYjjPayEntity.FirstOrDefault().zfje,
                            pzh = null,
                            szxz = (int)EnumSZXZ.mzjsth,
                            xjzffs = Constants.xtzffs.ZYYJZHZF,
                            jsnm = newDcJszbEntity.jsnm,
                            zt = "1",
                        };
                        zhszEntity.Create(true);
                        db.Insert(zhszEntity);
                        if (newJszbEntity.xjzf > 0)
                        {
                            var newzhszEntity = new SysAccountRevenueAndExpenseEntity()
                            {
                                OrganizeId = oldJszbEntity.OrganizeId,
                                patid = oldJszbEntity.patid,
                                zhCode = accountEntity.zhCode,
                                szje = 0- newJszbEntity.xjzf,
                                zhye = accountEntity.zhye + preYjjPayEntity.FirstOrDefault().zfje- newJszbEntity.xjzf,
                                pzh = null,
                                szxz = (int)EnumSZXZ.mzjs,
                                xjzffs = Constants.xtzffs.ZYYJZHZF,
                                jsnm = newJszbEntity.jsnm,
                                zt = "1",
                            };
                            newzhszEntity.Create(true);
                            db.Insert(newzhszEntity);
                            //insert mz_jszffs 
                            var newmzzfEntity = new OutpatientSettlementPaymentModelEntity()
                            {
                                mzjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jszffs"),
                                OrganizeId = oldJszbEntity.OrganizeId,
                                jsnm = newJszbEntity.jsnm,
                                xjzffs = xtzffs.ZYYJZHZF,
                                zfje = newJszbEntity.xjzf,
                                zh= accountEntity.zhCode,
                                zt = "1",
                            };
                            newmzzfEntity.Create();
                            db.Insert(newmzzfEntity);
                        }
                        //更新账户余额
                        accountEntity.zhye = accountEntity.zhye + preYjjPayEntity.FirstOrDefault().zfje - newJszbEntity.xjzf;
                        db.Update(accountEntity);
                    }
                }
                db.Commit();
            }
            newJszbInfo = new
            {
                ytw = ytw,
                jsnm = newJszbEntity.jsnm,
                //新的医保交易结算号
                ybjsh = newwjybjsh,
                jszje = newJszbEntity.zje,
                jsxjzf = newJszbEntity.xjzf,
                xjzlje = oldxjzf - newJszbEntity.xjzf,
                YBZHZC = guiannewybfeeRelated != null ? guiannewybfeeRelated.zhzf : null,
                JBYE = guiannewybfeeRelated != null ? guiannewybfeeRelated.zhye : 0,
                GBYE = guiannewybfeeRelated != null ? guiannewybfeeRelated.GBYE : null,
            };
        }

        /// <summary>
        /// 计划退费全停
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jsnm"></param>
        public void AccountPlanRefundAll(string orgId, int jsnm)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var jsyj = (int)EnumJieSuanZT.YJ;
                var jszbEntity = db.IQueryable<OutpatientSettlementEntity>(p => p.jsnm == jsnm && p.OrganizeId == orgId && p.zt == "1" && p.jszt == jsyj && p.tbz != 1).FirstOrDefault();
                if (jszbEntity != null)
                {
                    var jsmxEntityList = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.jsnm == jsnm && p.OrganizeId == orgId).ToList();
                    if (jsmxEntityList.Any(p => p.zt != "1") || jsmxEntityList.Count == 0)
                    {
                        throw new FailedException("非原始费用记录，操作失败");
                    }
                    var xmnmList = jsmxEntityList.Select(p => p.mxnm).Where(p => p > 0).Distinct().ToList();
                    var xmEntityList = db.IQueryable<OutpatientItemEntity>(p => xmnmList.Contains(p.xmnm) && p.zt == "1").ToList();
                    if (xmEntityList.Count != jsmxEntityList.Count)
                    {
                        throw new FailedException("非原始费用记录，操作失败.");
                    }
                    var jzjhmxIdList = xmEntityList.Select(p => p.jzjhmxId).Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
                    var jzjhmxEntityList = db.IQueryable<OutpatientAccountDetailEntity>(p => jzjhmxIdList.Contains(p.jzjhmxId) && p.zt == "1").ToList();
                    if (jzjhmxEntityList.Count != jsmxEntityList.Count)
                    {
                        throw new FailedException("非原始费用记录，操作失败..");
                    }
                    var jzjhId = jzjhmxEntityList[0].jzjhId;
                    var sourceJzjhmxEntityList = db.IQueryable<OutpatientAccountDetailEntity>(p => p.jzjhId == jzjhId).ToList();
                    if (jzjhmxEntityList.Count != sourceJzjhmxEntityList.Count)
                    {
                        throw new FailedException("非原始费用记录，操作失败...");
                    }

                    //结算主表
                    jszbEntity.jszt = (int)EnumJieSuanZT.YT;
                    jszbEntity.tbz = 1;
                    jszbEntity.Modify();
                    db.Update(jszbEntity);
                    //结算明细
                    jsmxEntityList.ForEach(p =>
                    {
                        p.zt = "0";
                        p.Modify();
                        db.Update(p);
                    });
                    //记账计划明细
                    jzjhmxEntityList.ForEach(p =>
                    {
                        p.zt = "0";
                        p.Modify();
                        db.Update(p);
                    });
                    //计划主表

                    db.Commit();
                }
            }
        }

        /// <summary>
        /// 作废处方（已结一定是不能作废）
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="cfnmList">处方内码List</param>
        /// <param name="cfhList">处方号List</param>
        /// <param name="settledUpdateForbidden">已结时 是否禁止更新</param>
        public void CancelPrescription(string orgId
            , IList<int> cfnmList = null
            , IList<string> cfhList = null
            , bool settledUpdateForbidden = true)
        {
            cfnmList = cfnmList ?? new List<int>();
            cfhList = cfhList ?? new List<string>();
            if (cfnmList.Count == 0 && cfhList.Count == 0)
            {
                return;
            }
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var cfzbEntityList = db.IQueryable<OutpatientPrescriptionEntity>(p => p.zt == "1" && p.OrganizeId == orgId && (cfnmList.Contains(p.cfnm) || cfhList.Contains(p.cfh))).ToList();
                if (cfzbEntityList.Count != (cfnmList.Count + cfhList.Count))
                {
                    //指定的处方 参数有问题 对不上
                    throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误定位处方异常");
                }
                foreach (var cfzbEntity in cfzbEntityList)
                {
                    if (cfzbEntity.cfzt == "1" && settledUpdateForbidden)
                    {
                        throw new FailedException("SETTLED_UPDATE_FORBIDDEN", "已结状态不允许修改变更");
                    }
                    //数据库中的当前明细List
                    IList<OutpatientItemEntity> itemList = db.IQueryable<OutpatientItemEntity>(p => p.OrganizeId == orgId && p.cfnm == cfzbEntity.cfnm && p.zt == "1").ToList();
                    foreach (var item in itemList)
                    {
                        item.zt = "0";
                        item.Modify();
                        db.Update(item);
                    }

                    cfzbEntity.zt = "0";
                    cfzbEntity.Modify();
                    db.Update(cfzbEntity);
                }
                db.Commit();
            }
        }

        /// <summary>
        /// 根据门诊号 等 查询处方号
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="mzh"></param>
        /// <param name="cfzt"></param>
        /// <param name="cfly"></param>
        /// <param name="cfhs">多个处方号，逗号分割</param>
        /// <returns></returns>
        public IList<OutpatientPrescriptionEntity> GetValidCfListByMzh(string orgId, string mzh
            , string cfzt = null, string cfly = null, string cfhs = null)
        {
            var sql = @"select * from mz_cf(nolock)
where zt = '1' and OrganizeId = @orgId
and ghnm in
(
select ghnm from mz_gh(nolock) where mzh = @mzh and zt = '1' and OrganizeId = @orgId
)";
            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@orgId", orgId));
            pars.Add(new SqlParameter("@mzh", mzh ?? "-1111111"));
            if (!string.IsNullOrWhiteSpace(cfzt))
            {
                if (cfzt == "0")
                {
                    sql += " and isnull(cfzt, '') <> '1'";  //未收费
                }
                else if (cfzt == "1")
                {
                    sql += " and isnull(cfzt, '') = '1'";   //已收费
                }
            }
            if (!string.IsNullOrWhiteSpace(cfly))
            {
                sql += " and cfly = @cfly";   //处方来源
                pars.Add(new SqlParameter("@cfly", cfly));
            }
            if (!string.IsNullOrWhiteSpace(cfhs))
            {
                sql += " and cfh in (select col from [dbo].[f_split](@cfhs, ','))";   //处方来源
                pars.Add(new SqlParameter("@cfhs", cfhs));
            }
            return this.FindList<OutpatientPrescriptionEntity>(sql, pars.ToArray());
        }

        /// <summary>
        /// 更新欠费预结的记录至已收费
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jsnm"></param>
        /// <param name="feeRelated">金额及支付信息</param>
        /// <param name="sfrq"></param>
        /// <param name="fph"></param>
        public void UpdateArrearageSettlement(string orgId, int jsnm
            , OutpatientSettFeeRelatedDTO feeRelated
            , DateTime sfrq
            , string fph = null)
        {
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var jszbEntity = db.IQueryable<OutpatientSettlementEntity>(p => p.jsnm == jsnm && p.zt == "1" && (p.tbz == null || p.tbz == 0)).FirstOrDefault();
                if (!(jszbEntity.isQfyj == true))
                {
                    throw new FailedException("ERROR_NOTQFYJ", "非欠费记录，操作失败");
                }
                if (jszbEntity.zje != feeRelated.zje)
                {
                    throw new Exception("结算总金额异常" + jszbEntity.zje.ToString() + "不等于" + feeRelated.zje.ToString());
                }
                //总金额 肯定不会变
                jszbEntity.fph = fph;
                jszbEntity.jzsj = sfrq;
                jszbEntity.jmbl = feeRelated.zkbl ?? 0;
                jszbEntity.jmje = feeRelated.zkje ?? 0;
                jszbEntity.xjzf = feeRelated.xjzfys ?? 0;
                //记账部分 等
                jszbEntity.ysk = feeRelated.ssk;
                jszbEntity.zl = feeRelated.zhaoling;
                jszbEntity.jszt = (int)EnumJieSuanZT.YJ;    //已结
                jszbEntity.isQfyj = null;

                UpdateCurrentFinancialInvoice(db, orgId, fph);

                if (feeRelated.orglxjzfys.HasValue && feeRelated.xjzfys.HasValue)
                {
                    //要减去记账部分 等金额
                    if (feeRelated.orglxjzfys.Value != jszbEntity.zje - 0)
                    {
                        throw new FailedException("ERROR_SETT_JE_ERROR", "结算金额异常" + feeRelated.orglxjzfys.Value.ToString() + "不等于" + jszbEntity.zje.ToString());
                    }
                    //orglxjzfys xjzfys zkbl zkje要保持一致性
                    var zkje = feeRelated.orglxjzfys.Value * Math.Abs(feeRelated.zkbl ?? 0) + Math.Abs(feeRelated.zkje ?? 0);
                    if (Math.Abs(zkje + feeRelated.xjzfys.Value - feeRelated.orglxjzfys.Value) > (decimal)0.01)
                    {
                        throw new FailedException("ERROR_ZKJE_ZFJE_ERROR", "折扣金额异常");
                    }
                }
                else
                {
                    //???xjzf有必要赋值么
                    //要减去记账部分 等金额
                    jszbEntity.xjzf = jszbEntity.zje - 0;
                }

                if (feeRelated.ssk.HasValue && feeRelated.zhaoling.HasValue)
                {
                    //现金误差 收到的-应收的
                    jszbEntity.xjwc = jszbEntity.xjzf - (feeRelated.ssk.Value - feeRelated.zhaoling.Value);
                    if (Math.Abs(jszbEntity.xjwc) >= (decimal)0.1)
                    {
                        throw new FailedException("ERROR_SSK_ZHAOLING", "实收找零金额异常");
                    }
                }

                jszbEntity.Modify();
                db.Update(jszbEntity);

                PaymentModelAccountReserve(db, jszbEntity, feeRelated);

                //
                var jsmxEntityList = db.IQueryable<OutpatientSettlementDetailEntity>(p => p.jsnm == jsnm && p.zt == "1").ToList();
                var mzxmnmList = jsmxEntityList.Where(p => (p.mxnm ?? 0) > 0).Select(p => p.mxnm.Value).ToList();
                var cfmxIdList = jsmxEntityList.Where(p => (p.cf_mxnm ?? 0) > 0).Select(p => p.cf_mxnm.Value).ToList();
                var cfnmList = new List<int>();
                if (mzxmnmList.Count > 0)
                {
                    var mzxmEntityList = db.IQueryable<OutpatientItemEntity>(p => mzxmnmList.Contains(p.xmnm)).ToList();
                    foreach (var mzxmEntity in mzxmEntityList)
                    {
                        mzxmEntity.xmzt = "1";  //已结
                        mzxmEntity.sfrq = sfrq;
                        mzxmEntity.Modify();
                        db.Update(mzxmEntity);
                    }
                    cfnmList.AddRange(mzxmEntityList.Select(p => p.cfnm ?? 0));
                    //mz_jzjhmx的jzsj
                    var jzjhmxIdList = mzxmEntityList.Select(p => p.jzjhmxId).ToList();
                    var jzjhmxEntityList = db.IQueryable<OutpatientAccountDetailEntity>(p => jzjhmxIdList.Contains(p.jzjhmxId) && p.zt == "1").ToList();
                    foreach (var jzjhmxEntity in jzjhmxEntityList)
                    {
                        jzjhmxEntity.jzsj = sfrq;
                        jzjhmxEntity.Modify();
                        db.Update(jzjhmxEntity);
                    }
                }
                if (cfmxIdList.Count > 0)
                {
                    var cfmxEntityList = db.IQueryable<OutpatientPrescriptionDetailEntity>(p => cfmxIdList.Contains(p.cfmxId)).ToList();
                    cfnmList.AddRange(cfmxEntityList.Select(p => p.cfnm));
                }
                cfnmList = cfnmList.Where(p => p > 0).Distinct().ToList();
                if (cfnmList.Count > 0)
                {
                    var cfEntityList = db.IQueryable<OutpatientPrescriptionEntity>(p => cfnmList.Contains(p.cfnm)).ToList();
                    foreach (var cfEntity in cfEntityList)
                    {
                        cfEntity.cfzt = "1";  //已结
                        cfEntity.sfrq = sfrq;
                        cfEntity.Modify();
                        db.Update(cfEntity);
                    }
                }

                db.Commit();
            }
        }

        #region private methods

        /// <summary>
        /// 新增结算（一条主结算 多条明细）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgId"></param>
        /// <param name="sfrq"></param>
        /// <param name="cfList"></param>
        /// <param name="xmList"></param>
        /// <param name="mzghEntity"></param>
        /// <param name="feeRelated">金额及支付信息</param>
        /// <param name="isQfyj">是否是欠费预结</param>
        /// <param name="fph"></param>
        /// <param name="autoGenePlan">是否自动生成门诊计划</param>
        /// <param name="ybfeeRelated"></param>
        /// <param name="outTradeNo"></param>
        /// <returns>返回结算内码</returns>
        private int AddSettlement(Infrastructure.EF.EFDbTransaction db, string orgId, DateTime? sfrq
            , IList<OutpatientPrescriptionEntity> cfList
            , IList<OutpatientItemEntity> xmList
            , OutpatientRegistEntity mzghEntity
            //金额相关
            , OutpatientSettFeeRelatedDTO feeRelated
            //医保金额相关
            , CQMzjs05Dto ybfeeRelated
            , S25ResponseDTO xnhybfeeRelated
            , bool isQfyj = false
            , string fph = null
            , bool autoGenePlan = false
            , string outTradeNo = null)
        {
            var fcgjs = false; //标志 非常规结算（常规：选择支付方式、金额 发票号等）
            if (feeRelated == null)
            {
                fcgjs = true;
                feeRelated = new OutpatientSettFeeRelatedDTO();
            }

            var jszbEntity = new OutpatientSettlementEntity()
            {
                jsnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_js"),
                OrganizeId = orgId,
                patid = mzghEntity.patid,
                brxz = mzghEntity.brxz,
                jslx = "2", //门诊结账
                fph = fph,
                jszt = (int)EnumJieSuanZT.YJ,
                ghnm = mzghEntity.ghnm,
                xm = mzghEntity.xm,
                xb = mzghEntity.xb,
                csny = mzghEntity.csny,
                blh = mzghEntity.blh,
                zjh = mzghEntity.zjh,
                zjlx = mzghEntity.zjlx,
                jzsj = sfrq,
                jmbl = feeRelated.zkbl ?? 0,
                jmje = feeRelated.zkje ?? 0,
                xjzf = feeRelated.xjzfys ?? 0,
                //记账部分 等
                ysk = feeRelated.ssk,
                zl = feeRelated.zhaoling,
                isQfyj = isQfyj ? (bool?)true : null,
                OutTradeNo = outTradeNo,
                ybjslsh=feeRelated.ybjslsh
            };
            jszbEntity.Create();
            db.Insert(jszbEntity);

            UpdateCurrentFinancialInvoice(db, orgId, fph);

            //此次新增的jsmx
            var newJsmxEntityList = new List<OutpatientSettlementDetailEntity>();
            //涉及的门诊项目列表
            var mzxmEntityList = new List<OutpatientItemEntity>();

            //处方
            foreach (var cf in cfList)
            {
                switch (cf.cflx)
                {
                    case (int)EnumPrescriptionType.Medicine:
                        {
                            if (!string.IsNullOrWhiteSpace(cf.lyyf))
                            {
                                ////算法 结算时直接生成领药窗口、领药序号
                                //cf.lyck = "2";   //领药窗口
                                //cf.lyxh = 13;   //领药序号
                            }
                            var mxList = db.IQueryable<OutpatientPrescriptionDetailEntity>(p => p.cfnm == cf.cfnm && p.zt == "1").ToList();
                            foreach (var item in mxList)
                            {
                                //新增mz_jsmx
                                var jsmxEntity = new OutpatientSettlementDetailEntity()
                                {
                                    jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx"),
                                    cf_mxnm = item.cfmxId,
                                    sl = item.sl,
                                    jyje = item.je,
                                };
                                jsmxEntity.Create();
                                db.Insert(jsmxEntity);

                                newJsmxEntityList.Add(jsmxEntity);
                            }
                            var mxxmList = db.IQueryable<OutpatientItemEntity>(p => p.cfnm == cf.cfnm && p.zt == "1").ToList();
                            if (mxxmList.Count>0)
                            {
                                mzxmEntityList.AddRange(mxxmList);  //
                                foreach (var item in mxxmList)
                                {
                                    //新增mz_jsmx
                                    var jsmxEntity = new OutpatientSettlementDetailEntity()
                                    {
                                        jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx"),
                                        mxnm = item.xmnm,
                                        sl = item.sl,
                                        jyje = item.je,
                                    };
                                    jsmxEntity.Create();
                                    db.Insert(jsmxEntity);

                                    newJsmxEntityList.Add(jsmxEntity);

                                    //更新mz_xm 已结
                                    item.xmzt = "1";  //已结
                                    item.jsrq = DateTime.Now; //结算时间 当前时间
                                    if (sfrq.HasValue)
                                    {
                                        item.sfrq = sfrq;   //强制更新收费日期
                                    }
                                    item.Modify();
                                    db.Update(item);
                                }
                            }

                            break;
                        }
                    case (int)EnumPrescriptionType.Treament:
                        {
                            var mxList = db.IQueryable<OutpatientItemEntity>(p => p.cfnm == cf.cfnm && p.zt == "1").ToList();
                            mzxmEntityList.AddRange(mxList);  //
                            foreach (var item in mxList)
                            {
                                //新增mz_jsmx
                                var jsmxEntity = new OutpatientSettlementDetailEntity()
                                {
                                    jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx"),
                                    mxnm = item.xmnm,
                                    sl = item.sl,
                                    jyje = item.je,
                                };
                                jsmxEntity.Create();
                                db.Insert(jsmxEntity);

                                newJsmxEntityList.Add(jsmxEntity);

                                //更新mz_xm 已结
                                item.xmzt = "1";  //已结
                                item.jsrq = DateTime.Now; //结算时间 当前时间
                                if (sfrq.HasValue)
                                {
                                    item.sfrq = sfrq;   //强制更新收费日期
                                }
                                item.Modify();
                                db.Update(item);
                            }

                            break;
                        }
                    default:
                        //处方类型 异常 非范围内
                        throw new FailedException("PARAMS_ERROR_CFLX", "处方类型参数错误");
                }

                //更新mz_cf 已结
                cf.cfzt = "1";  //已结
                cf.jsrq = DateTime.Now; //结算时间 当前时间
                if (sfrq.HasValue)
                {
                    cf.sfrq = sfrq;   //强制更新收费日期
                }
                cf.Modify();
                db.Update(cf);
            }

            //没关联处方的部分
            mzxmEntityList.AddRange(xmList);  //
            foreach (var item in xmList)
            {
                if (newJsmxEntityList.Any(p => p.mxnm == item.xmnm))
                {
                    continue;   //这个错误调用 可以避免
                }
                //新增mz_jsmx
                var jsmxEntity = new OutpatientSettlementDetailEntity()
                {
                    jsmxnm = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jsmx"),
                    mxnm = item.xmnm,
                    sl = item.sl,
                    jyje = item.je,
                };
                jsmxEntity.Create();
                db.Insert(jsmxEntity);

                newJsmxEntityList.Add(jsmxEntity);

                //更新mz_xm 已结
                item.xmzt = "1";  //已结
                item.jsrq = DateTime.Now; //结算时间 当前时间
                if (sfrq.HasValue)
                {
                    item.sfrq = sfrq;   //强制更新收费日期
                }
                item.Modify();
                db.Update(item);
            }

            //收尾
            foreach (var item in newJsmxEntityList)
            {
                item.OrganizeId = orgId;
                item.jsnm = jszbEntity.jsnm;
                item.jslx = "2";
            }

            var mxzje = newJsmxEntityList.Sum(p => p.jyje ?? 0);
            if (!fcgjs && mxzje != feeRelated.zje)
            {
                throw new Exception("结算总金额异常" + mxzje.ToString() + "不等于" + feeRelated.zje.ToString());
            }
            jszbEntity.zt = "1";
            jszbEntity.zje = mxzje;

            if (feeRelated.orglxjzfys.HasValue && feeRelated.xjzfys.HasValue)
            {
                //要减去记账部分 等金额
                if ((feeRelated.orglxjzfys.Value != jszbEntity.zje - 0)
                    && (ybfeeRelated == null || ybfeeRelated.XJZF != feeRelated.orglxjzfys.Value)
                    && (ybfeeRelated != null && ybfeeRelated.ybdlx == "changshu"))
                {
                    throw new FailedException("ERROR_SETT_JE_ERROR", "结算金额异常");
                }
                //orglxjzfys xjzfys zkbl zkje要保持一致性
                //var zkje =  Math.Abs(feeRelated.zkje ?? 0);
                //var cs = Math.Abs(zkje + feeRelated.xjzfys.Value - feeRelated.orglxjzfys.Value);
                //if (Math.Abs(zkje + feeRelated.xjzfys.Value - feeRelated.orglxjzfys.Value) > (decimal)0.01)
                //{
                //    throw new FailedException("ERROR_ZKJE_ZFJE_ERROR", "折扣金额异常");
                //}
            }
            else
            {
                //???xjzf有必要赋值么
                //要减去记账部分 等金额
                jszbEntity.xjzf = jszbEntity.zje - 0;
            }

            if (feeRelated.ssk.HasValue && feeRelated.zhaoling.HasValue)
            {
                jszbEntity.xjwc = (decimal)feeRelated.xjwc;
                //现金误差 收到的-应收的
                //jszbEntity.xjwc = jszbEntity.xjzf - (feeRelated.ssk.Value - feeRelated.zhaoling.Value);
                //if (Math.Abs(jszbEntity.xjwc) >= (decimal)0.1)
                //{
                //    throw new FailedException("ERROR_SSK_ZHAOLING", "实收找零金额异常");
                //}
            }

            PaymentModelAccountReserveII(db, jszbEntity, feeRelated);

            //门诊结算支付方式

            if (autoGenePlan)
            {
                GenePlan(db, orgId, mzxmEntityList, mzghEntity);
            }

            return jszbEntity.jsnm;
        }


        public void UpdateYPcfxm(List<OutGridInfoDto2018> deletelist,string orgId,string usercode)
        {
            for (int i = 0; i < deletelist.Count; i++)
            {
                var sql = @"update [NewtouchHIS_Sett].[dbo].[mz_xm]  set zt='0',LastModifyTime=GETDATE(),LastModifierCode=@LastModifierCode  
where cfnm=@cfnm and OrganizeId=@orgId and ztId=@ztid 

update [NewtouchHIS_Sett].[dbo].[mz_cf]   set zje=(zje-convert(int,0)),LastModifyTime=GETDATE(),LastModifierCode=@LastModifierCode 
where cfnm=@cfnm   and OrganizeId=@orgId and cflx=@cflx and zt='1' 

update [Newtouch_CIS].[dbo].[xt_cf]  set zje=(zje-convert(int,0)),LastModifyTime=GETDATE(),LastModifierCode=@LastModifierCode 
where cfh=@cfh   and OrganizeId=@orgId and cflx=@cflx and zt='1'  

update [Newtouch_CIS].[dbo].[xt_cfmx]  set  zt='0',LastModifyTime=GETDATE(),LastModifierCode=@LastModifierCode  
where cfId=(select top 1 cfId from [Newtouch_CIS].[dbo].[xt_cf] with(nolock) 
where cfh=@cfh   and OrganizeId=@orgId and cflx=@cflx and zt='1' ) and OrganizeId=@orgId and ztId=@ztid "; 


                ExecuteSqlCommand(sql,
                    new[] { new SqlParameter("@orgId",orgId),
                new SqlParameter("@LastModifierCode",usercode),
                new SqlParameter("@cflx",deletelist[i].cflx),
                new SqlParameter("@cfh",deletelist[i].cfh),
                new SqlParameter("@cfnm",deletelist[i].cfnm),
                new SqlParameter("@ztid",deletelist[i].sfxmCode) });
            }
        }

        /// <summary>
        /// 为项目List生成计划
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgId"></param>
        /// <param name="xmList"></param>
        /// <param name="mzghEntity"></param>
        private void GenePlan(Infrastructure.EF.EFDbTransaction db, string orgId
            , IList<OutpatientItemEntity> xmList
            , OutpatientRegistEntity mzghEntity)
        {
            var jzjhEntity = new OutpatientAccountEntity()
            {
                jzjhId = Guid.NewGuid().ToString(),
                OrganizeId = orgId,
                patid = mzghEntity.patid.ToString(),
                ghnm = mzghEntity.ghnm,
            };

            bool mxAdded = false;

            foreach (var item in xmList)
            {
                if (!item.dczll.HasValue || !item.zxcs.HasValue || ((int)(item.sl / item.zxcs) == 0))
                {
                    continue;
                }
                var jzjhmxEntity = new OutpatientAccountDetailEntity
                {
                    jzjhmxId = Guid.NewGuid().ToString(),
                    jzjhId = jzjhEntity.jzjhId,
                    OrganizeId = orgId,
                    //zlsc = item.duration ?? 0,
                    sfxmCode = item.sfxm,
                    sl = (int)(item.sl / item.zxcs),  //?
                    jzsj = item.sfrq,
                    bz = item.bz,
                    ys = item.ys,
                    ks = item.ks,
                    ysmc = item.ysmc,
                    ksmc = item.ksmc,
                    //kflb = item.kflb,
                    yzxz = "2", //2长期
                    StartDate = Constants.MinDate,
                    EndDate = Constants.MaxDate,
                    zxzt = (int)EnumJzjhZXZT.None,
                    zll = item.dczll,
                    zcs = item.zxcs
                };
                jzjhmxEntity.Create();
                db.Insert(jzjhmxEntity);

                item.jzjhmxId = jzjhmxEntity.jzjhmxId;
                item.ssbz = "9";    //直接进入 实施过程中 这样 不可退费
                //item.Modify();
                //db.Update(item);    //更新mz_xm   
                //应该多余的 前面肯定会更新器jszt jsrq等

                mxAdded = true;
            }

            if (!mxAdded)
            {
                //return;
            }

            jzjhEntity.Create();
            db.Insert(jzjhEntity);
        }

        /// <summary>
        /// 更新单价 金额 等
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jfypEntity"></param>
        private void UpdateEntity(string orgId, OutpatientPrescriptionDetailEntity jfypEntity)
        {
            SysMedicineVEntity ypEntity = _sysMedicinRepo.GetEntityByCode(orgId, jfypEntity.yp);
            if (jfypEntity.sl <= 0)
            {
                throw new FailedException("SL_IS_REQUIRED", "处方药品异常缺少数量");
            }
            //zfxz zfbl 以这个为准
            jfypEntity.zfxz = ypEntity.zfxz;
            jfypEntity.zfbl = ypEntity.zfbl;
            if (string.IsNullOrWhiteSpace(jfypEntity.dl))
            {
                jfypEntity.dl = ypEntity.dlCode;
            }
            if (string.IsNullOrWhiteSpace(jfypEntity.dw))
            {
                //是门诊拆零单位
                jfypEntity.dw = ypEntity.mzcldw;
            }
            if (jfypEntity.jl > 0 && string.IsNullOrWhiteSpace(jfypEntity.jldw))
            {
                jfypEntity.jldw = ypEntity.jldw;    //剂量单位
            }
            //单价可能 是 自定义 的
            if (jfypEntity.dj < 0)  //20180910由 <=0 变为 <0
            {
                //单位 单价 换算
                if (jfypEntity.dw == ypEntity.bzdw)
                {
                    jfypEntity.dj = ypEntity.lsj;
                }
                else if (jfypEntity.dw == ypEntity.mzcldw)
                {

                    jfypEntity.dj = (ypEntity.lsj / ypEntity.bzs) * ypEntity.mzcls.Value;
                }
                else
                {
                    //那只能默认用门诊的了
                    jfypEntity.dw = ypEntity.mzcldw;
                    jfypEntity.dj = (ypEntity.lsj / ypEntity.bzs) * ypEntity.mzcls.Value;
                }
            }
            if (jfypEntity.je <= 0)
            {
                jfypEntity.je = jfypEntity.dj * jfypEntity.sl;
            }
            jfypEntity.je = Math.Round(jfypEntity.je, 2, MidpointRounding.AwayFromZero);//四舍五入
        }

        /// <summary>
        /// 根据治疗量 更新计价数量 单价 金额 等
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="jfxmEntity"></param>
        private void UpdateEntity(string orgId, OutpatientItemEntity jfxmEntity)
        {
            var sfxmEntity = _sysChargeItemRepo.SelectSysChargeItemBysfxm(jfxmEntity.sfxm, orgId);
            //zfxz zfbl 以这个为准
            jfxmEntity.zfxz = sfxmEntity.zfxz;
            jfxmEntity.zfbl = sfxmEntity.zfbl;
            if (jfxmEntity.sl <= 0)
            {
                if ((jfxmEntity.dczll ?? 0) > 0 && (jfxmEntity.zxcs ?? 0) > 0)
                {
                    jfxmEntity.sl = CommmHelper.CalcSfxmSl(jfxmEntity.dczll, sfxmEntity.dwjls, sfxmEntity.jjcl) * jfxmEntity.zxcs.Value;
                }
                else
                {
                    throw new FailedException("SL_OR_DCZLL_ZXCS_IS_REQUIRED", "项目异常缺少数量");
                }
            }

            //单价可能 是 自定义 的
            if (jfxmEntity.dj < 0)  //20180910由 <=0 变为 <0
            {
                jfxmEntity.dj = sfxmEntity.dj;
            }
            if (string.IsNullOrWhiteSpace(jfxmEntity.dl))
            {
                jfxmEntity.dl = sfxmEntity.sfdlCode;
            }
            if (string.IsNullOrWhiteSpace(jfxmEntity.dw))
            {
                jfxmEntity.dw = sfxmEntity.dw;
            }
            if (jfxmEntity.je <= 0)
            {
                jfxmEntity.je = jfxmEntity.dj * jfxmEntity.sl;
            }
            if ((jfxmEntity.dczll ?? 0) > 0 && (jfxmEntity.zxcs ?? 0) > 0 && (jfxmEntity.zzll ?? 0) <= 0)
            {
                jfxmEntity.zzll = jfxmEntity.dczll.Value * jfxmEntity.zxcs.Value;
            }
        }

        /// <summary>
        /// 结算支付方式、预交金支付 账户收支
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jszbEntity"></param>
        /// <param name="feeRelated"></param>
        private void PaymentModelAccountReserve(Infrastructure.EF.EFDbTransaction db, OutpatientSettlementEntity jszbEntity
            , OutpatientSettFeeRelatedDTO feeRelated)
        {
            if (jszbEntity.isQfyj == true)
            {
                return; //欠费预结return
            }
            OutpatientSettlementPaymentModelEntity zfEntity1 = null;
            OutpatientSettlementPaymentModelEntity zfEntity2 = null;

            if (!string.IsNullOrWhiteSpace(feeRelated.zffs1) && (feeRelated.zfje1 ?? 0) > 0)
            {
                zfEntity1 = new OutpatientSettlementPaymentModelEntity();
                zfEntity1.mzjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jszffs");
                zfEntity1.OrganizeId = jszbEntity.OrganizeId;
                zfEntity1.jsnm = jszbEntity.jsnm;
                zfEntity1.xjzffs = feeRelated.zffs1;
                zfEntity1.zfje = feeRelated.zfje1.Value;
                zfEntity1.zt = "1";
                zfEntity1.Create();
                db.Insert(zfEntity1);
            }
            if (!string.IsNullOrWhiteSpace(feeRelated.zffs2) && (feeRelated.zfje2 ?? 0) > 0)
            {
                zfEntity2 = new OutpatientSettlementPaymentModelEntity();
                zfEntity2.mzjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jszffs");
                zfEntity2.OrganizeId = jszbEntity.OrganizeId;
                zfEntity2.jsnm = jszbEntity.jsnm;
                zfEntity2.xjzffs = feeRelated.zffs2;
                zfEntity2.zfje = feeRelated.zfje2.Value;
                zfEntity2.zt = "1";
                zfEntity2.Create();
                zfEntity2.CreateTime = DateTime.Now.AddSeconds(1);
                db.Insert(zfEntity2);
            }

            //如果只有一种支付方式 存入js.xjzffs
            //2019.06.06 huangshanshan 门诊计划录入时，报错修复
            jszbEntity.xjzffs = zfEntity2 == null ? (zfEntity1 == null ? null : zfEntity1.xjzffs) : null;

            //预交金支付 构建账户收支  //预交金支付 一定作为第一支付方式
            if (zfEntity1 != null && zfEntity1.xjzffs == xtzffs.ZYYJZHZF)
            {
                var accountEntity = db.IQueryable<SysAccountEntity>(p => p.patid == jszbEntity.patid).FirstOrDefault();
                if (accountEntity != null)
                {
                    zfEntity1.zh = accountEntity.zhCode;
                    if (!(zfEntity1.zfje == accountEntity.zhye
                        || (zfEntity1.zfje <= feeRelated.xjzfys.Value && zfEntity1.zfje <= accountEntity.zhye)))
                    {
                        throw new FailedException("", "预交账户支付金额异常");
                    }
                    var zhszEntity = new SysAccountRevenueAndExpenseEntity()
                    {
                        OrganizeId = jszbEntity.OrganizeId,
                        zhCode = accountEntity.zhCode,
                        patid = accountEntity.patid,
                        szje = 0 - Math.Min(zfEntity1.zfje, feeRelated.xjzfys.Value),
                        zhye = accountEntity.zhye - Math.Min(zfEntity1.zfje, feeRelated.xjzfys.Value),
                        pzh = null,
                        szxz = (int)EnumSZXZ.mzjs,
                        xjzffs = Constants.xtzffs.ZYYJZHZF,
                        jsnm = jszbEntity.jsnm,
                        zt = "1",
                    };
                    zhszEntity.Create(true);
                    db.Insert(zhszEntity);
                    //
                    accountEntity.zhye = zhszEntity.zhye;
                    db.Update(accountEntity);
                    //预交金支付 账户收支
                    if (zfEntity1.zfje > feeRelated.xjzfys.Value)
                    {
                        //还有一条收支取款（应该是预交金余额全退）
                        var zhszEntity2 = new SysAccountRevenueAndExpenseEntity()
                        {
                            OrganizeId = jszbEntity.OrganizeId,
                            zhCode = accountEntity.zhCode,
                            patid = accountEntity.patid,
                            szje = 0 - (zfEntity1.zfje - feeRelated.xjzfys.Value),
                            zhye = accountEntity.zhye - (zfEntity1.zfje - feeRelated.xjzfys.Value),
                            pzh = null,
                            szxz = (int)EnumSZXZ.qk,
                            //xjzffs = Constants.xtzffs.ZYYJZHZF,
                            xjzffs = "0",   //现金
                            zt = "1",
                        };
                        zhszEntity2.Create(true);
                        zhszEntity2.CreateTime = DateTime.Now.AddSeconds(1);
                        db.Insert(zhszEntity2);
                        //
                        accountEntity.zhye = zhszEntity2.zhye;
                        //db.Update(accountEntity);
                    }
                }
                else
                {
                    throw new FailedException("ERROR_ACCOUNT_INFO", "获取预交金账户信息失败");
                }
            }
        }
        private void PaymentModelAccountReserveII(Infrastructure.EF.EFDbTransaction db, OutpatientSettlementEntity jszbEntity
           , OutpatientSettFeeRelatedDTO feeRelated)
        {
            if (jszbEntity.isQfyj == true)
            {
                return; //欠费预结return
            }
            OutpatientSettlementPaymentModelEntity mzzfEntity1 = null;
            OutpatientSettlementPaymentModelEntity mzzfEntity2 = null;
            if (!string.IsNullOrWhiteSpace(feeRelated.yjjzfje.ToString()) &&(feeRelated.yjjzfje ?? 0) > 0)
            {
                mzzfEntity1 = new OutpatientSettlementPaymentModelEntity();
                mzzfEntity1.mzjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jszffs");
                mzzfEntity1.OrganizeId = jszbEntity.OrganizeId;
                mzzfEntity1.jsnm = jszbEntity.jsnm;
                mzzfEntity1.xjzffs = xtzffs.ZYYJZHZF;
                mzzfEntity1.zfje = feeRelated.yjjzfje.Value;
                mzzfEntity1.zt = "1";
                mzzfEntity1.Create();
                db.Insert(mzzfEntity1);
                jszbEntity.xjzffs = xtzffs.ZYYJZHZF;
            }
            if (feeRelated.patZflist.Count() > 0)
            {
                foreach (var item in feeRelated.patZflist)
                {
                    mzzfEntity2 = new OutpatientSettlementPaymentModelEntity();
                    mzzfEntity2.mzjszffsbh = EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("mz_jszffs");
                    mzzfEntity2.OrganizeId = jszbEntity.OrganizeId;
                    mzzfEntity2.jsnm = jszbEntity.jsnm;
                    mzzfEntity2.xjzffs = item.zffsmc;
                    mzzfEntity2.zfje = (decimal)item.zfje;
                    mzzfEntity2.zt = "1";
                    mzzfEntity2.Create();
                    mzzfEntity2.CreateTime = DateTime.Now.AddSeconds(1);
                    db.Insert(mzzfEntity2);
                }
                jszbEntity.xjzffs = feeRelated.djjesszffs;
            }


            //预交金支付 构建账户收支  //预交金支付 一定作为第一支付方式
            if (mzzfEntity1 != null && mzzfEntity1.xjzffs == xtzffs.ZYYJZHZF)
            {
                decimal tye = 0;
                if (feeRelated.yjjtye.Value > 0)
                {
                    tye = feeRelated.yjjtye.Value;
                }
                var accountEntity = db.IQueryable<SysAccountEntity>(p => p.patid == jszbEntity.patid).FirstOrDefault();
                if (accountEntity != null)
                {
                    mzzfEntity1.zh = accountEntity.zhCode;
                    if (!((mzzfEntity1.zfje + tye) == accountEntity.zhye
                        || (mzzfEntity1.zfje <= feeRelated.xjzfys.Value && (mzzfEntity1.zfje + tye) <= accountEntity.zhye)))
                    {
                        throw new FailedException("", "预交账户支付金额异常");
                    }
                    var zhszEntity = new SysAccountRevenueAndExpenseEntity()
                    {
                        OrganizeId = jszbEntity.OrganizeId,
                        zhCode = accountEntity.zhCode,
                        patid = accountEntity.patid,
                        //szje = 0 - Math.Min(mzzfEntity1.zfje, feeRelated.xjzfys.Value),
                        //zhye = accountEntity.zhye - Math.Min(mzzfEntity1.zfje, feeRelated.xjzfys.Value),
                        szje = 0 - mzzfEntity1.zfje,
                        zhye = accountEntity.zhye - mzzfEntity1.zfje,
                        pzh = null,
                        szxz = (int)EnumSZXZ.mzjs,
                        xjzffs = Constants.xtzffs.ZYYJZHZF,
                        jsnm = jszbEntity.jsnm,
                        zt = "1",
                    };
                    zhszEntity.Create(true);
                    db.Insert(zhszEntity);
                    //
                    accountEntity.zhye = zhszEntity.zhye;
                    db.Update(accountEntity);
                    //预交金支付 账户收支
                    if (feeRelated.yjjtye.Value > 0 && accountEntity.zhye == feeRelated.yjjtye.Value)
                    {
                        //还有一条收支取款（应该是预交金余额全退）
                        var zhszEntity2 = new SysAccountRevenueAndExpenseEntity()
                        {
                            OrganizeId = jszbEntity.OrganizeId,
                            zhCode = accountEntity.zhCode,
                            patid = accountEntity.patid,
                            szje = 0 - feeRelated.yjjtye.Value,
                            zhye = accountEntity.zhye - feeRelated.yjjtye.Value,
                            pzh = null,
                            szxz = (int)EnumSZXZ.qk,
                            //xjzffs = Constants.xtzffs.ZYYJZHZF,
                            xjzffs = Constants.xtzffs.XJZF,   //现金
                            zt = "1",
                        };
                        zhszEntity2.Create(true);
                        zhszEntity2.CreateTime = DateTime.Now.AddSeconds(1);
                        db.Insert(zhszEntity2);
                        //
                        accountEntity.zhye = zhszEntity2.zhye;
                    }
                    else if (feeRelated.yjjtye.Value > 0)
                    {
                        throw new FailedException("ERROR_ACCOUNT_INFO", "预交账户结算成功，退余额失败");
                    }
                    //if (mzzfEntity1.zfje > feeRelated.xjzfys.Value)
                    //{
                    //    //还有一条收支取款（应该是预交金余额全退）
                    //    var zhszEntity2 = new SysAccountRevenueAndExpenseEntity()
                    //    {
                    //        OrganizeId = jszbEntity.OrganizeId,
                    //        zhCode = accountEntity.zhCode,
                    //        patid = accountEntity.patid,
                    //        szje = 0 - (mzzfEntity1.zfje - feeRelated.xjzfys.Value),
                    //        zhye = accountEntity.zhye - (mzzfEntity1.zfje - feeRelated.xjzfys.Value),
                    //        pzh = null,
                    //        szxz = (int)EnumSZXZ.qk,
                    //        //xjzffs = Constants.xtzffs.ZYYJZHZF,
                    //        xjzffs = "0",   //现金
                    //        zt = "1",
                    //    };
                    //    zhszEntity2.Create(true);
                    //    zhszEntity2.CreateTime = DateTime.Now.AddSeconds(1);
                    //    db.Insert(zhszEntity2);
                    //    //
                    //    accountEntity.zhye = zhszEntity2.zhye;
                    //    //db.Update(accountEntity);
                    //}
                    db.Update(accountEntity);
                }
                else
                {
                    throw new FailedException("ERROR_ACCOUNT_INFO", "获取预交金账户信息失败");
                }
            }
        }

        /// <summary>
        /// 更新当前发票号
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgId"></param>
        /// <param name="fph"></param>
        private void UpdateCurrentFinancialInvoice(Infrastructure.EF.EFDbTransaction db, string orgId, string fph)
        {
            if (!string.IsNullOrWhiteSpace(fph))
            {
                //可用发票更新
                //插入/更新cw_fp
                var opr = Newtouch.Common.Operator.OperatorProvider.GetCurrent();
                if (opr != null)
                {
                    FinancialInvoiceEntity fpUpdateEntity, fpInsertEntity;
                    _financialInvoiceRepo.UpdateCurrentGetEntitys(fph, opr.UserCode, out fpUpdateEntity, out fpInsertEntity, orgId);
                    if (fpUpdateEntity != null)
                    {
                        db.Update(fpUpdateEntity);
                    }
                    if (fpInsertEntity != null)
                    {
                        db.Insert(fpInsertEntity);
                    }
                }
                else
                {
                    throw new FailedException("FINANCIALINVOICE_UPDATE_ERROR", "发票号更新异常");
                }
            }
        }

        #endregion

        /// <summary>
        /// 获取门诊结算/退费 患者出院信息（医保结算号、社保编号、就诊原因、科室、医生、诊断）
        /// </summary>
        /// <param name="mzh"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public OutPatChargePatientInfoVO GetSettPatientInfo(string mzh, string orgId)
        {
            var sql = @"select gh.ybjsh, brjbxx.sbbh, gh.jzyy, gh.zdicd10, gh.zdmc
, isnull(ysks.ys, ghysStaff.gh) ys, isnull(ysks.ks, ghDept.Code) ks, isnull(ysks.ysmc, ghysStaff.Name) ysmc, isnull(ysks.ksmc, ghDept.Name) ksmc
from mz_gh(nolock) gh
inner join xt_brjbxx(nolock) brjbxx
on brjbxx.patid = gh.patid and brjbxx.OrganizeId = gh.OrganizeId
left join [NewtouchHIS_Base]..V_S_Sys_Staff ghysStaff
on ghysStaff.gh = gh.ys and ghysStaff.OrganizeId = @orgId and ghysStaff.zt = '1'
left join [NewtouchHIS_Base]..V_S_Sys_Department ghDept
on ghDept.Code = gh.ks and ghDept.OrganizeId = @orgId and ghDept.zt = '1'
left join 
(
select top 1 *
from
(
select ys, ks, ysmc, ksmc, CreateTime from mz_xm(nolock)
where zt = '1' and isnull(ys,'')<>'' and isnull(ks,'')<>'' and isnull(ysmc,'')<>'' and isnull(ksmc,'')<>'' and ys not LIKE '%,%'
and OrganizeId  = @orgId and patid in(
 select patid from mz_gh(nolock) where OrganizeId = @orgId and zt = '1' and mzh = @mzh
)
union
select ys, ks, ysmc, ksmc, CreateTime from mz_cf(nolock)
where zt = '1' and isnull(ys,'')<>'' and isnull(ks,'')<>'' and isnull(ysmc,'')<>'' and isnull(ksmc,'')<>'' and ys not LIKE '%,%'
and OrganizeId  = @orgId and patid in(
 select patid from mz_gh(nolock) where OrganizeId = @orgId and zt = '1' and mzh = @mzh
)
) as innerjfysks
Order by CreateTime
) ysks
on 1 = 1
where gh.mzh = @mzh and gh.zt = '1' and gh.OrganizeId = @orgId";
            return this.FirstOrDefault<OutPatChargePatientInfoVO>(sql,
                new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@mzh", mzh) });
        }

        /// <summary>
        /// 获取门诊收费结算组套对应的明细
        /// </summary>
        /// <param name="cfnms"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public List<RefundZtDot> RefundZtDetail(string cfnms,string orgId)
        {
            string sql = @"select jsmx.jsmxnm,ztId,dj,jsmx.sl,cfnm from mz_jsmx jsmx 
join mz_xm xm  on jsmx.mxnm=xm.xmnm  and jsmx.organizeid=xm.organizeid and xm.zt=1
where jsmx.zt=1 and xm.cfnm in  (select col from f_split(@cfnms,','))";

            return this.FindList<RefundZtDot>(sql,
                new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@cfnms", cfnms) });
        }

        public CheckApplicationfromDTO pushApplicationform(BasicInfoDto2018 bacDto, int cfnm, string orgid,string typezt)
        {
            CheckApplicationfromDTO checkApplicationfromDTO = new CheckApplicationfromDTO();
            string sql = @"select a.cfh Reqno,a.CreateTime Indate,a.ys DoctorID,a.ysmc DoctorName,gh.mzh RecordID,a.ks WardNo, a.cfh CardID,b.ztmc ApplyName,a.ks ApplyDepartmentCode,a.ksmc ApplyDepartmentName,convert(varchar(50),b.xmnm) Seqno,b.sfxm ItemID,sf.sfxmmc ItemName,convert(varchar(10),b.sl) Qty,convert(varchar(10),b.dj) Price  from [NewtouchHIS_Sett]..mz_cf a
left join [NewtouchHIS_Sett]..mz_gh gh on gh.ghnm=a.ghnm and a.organizeid=gh.organizeid
left join [NewtouchHIS_Sett]..mz_xm b on a.cfnm=b.cfnm and a.organizeid=b.organizeid
left join [NewtouchHIS_Base]..xt_sfxm sf on b.sfxm=sf.sfxmcode and b.organizeid=sf.organizeid
where a.cfnm=@cfnms and a.organizeid=@orgId  and a.cflxmc='检查处方'";

			sql = "exec MZ_pacsxx @cfnms,@orgId";
			var cfmxlist = new List<CheckfromDTO>(); ;
            try
            {
                cfmxlist = this.FindList<CheckfromDTO>(sql,
                new[] { new SqlParameter("@orgId", orgid), new SqlParameter("@cfnms", cfnm.ToString()) });
            }
            catch (Exception e)
            {

                throw;
            }
            
            if (cfmxlist.Count==0|| cfmxlist==null)
            {
                return null;
            }
            RequestHost requestHost = new RequestHost();
            requestHost.MedNo = orgid;//先传入测试
            requestHost.MedName = "上海明德五官科医院";//先传入测试

            Patient patient = new Patient();
            patient.Name = bacDto.xm;
            patient.Gender = bacDto.xb=="1"?"M":"F";
            patient.Birthday = bacDto.csny;
            patient.IdentityID = bacDto.zjh;
            patient.WardNo = cfmxlist[0].WardNo;
            patient.BedNo = "";
            patient.CardID = cfmxlist[0].CardID;

            Request request = new Request();
            request.Reqno = cfmxlist[0].Reqno;
            request.Indate = DateTime.Parse(cfmxlist[0].Indate.ToDateTimeString());
            request.DoctorID = cfmxlist[0].DoctorID;
            request.DoctorName = cfmxlist[0].DoctorName;
            request.DoctorIdCard = "";
            request.RecordID = cfmxlist[0].RecordID;
            request.Source = "MagQ700001";
            request.Pic = "";
            request.PicDetail = cfmxlist[0].PicDetail;//先放空 后面字典对应
            request.ApplyName = cfmxlist[0].ApplyName;
            request.WhichNo = null;
            request.IsVaild = typezt;
            request.IsCharge = "";
            request.ChargeDate = DateTime.Now;
            request.InvoiceID = "";
            request.CancelDate = null;
            request.Comment = "";
            request.ApplyDepartmentCode = cfmxlist[0].ApplyDepartmentCode;
            request.ApplyDepartmentName = cfmxlist[0].ApplyDepartmentName;
            request.DiagnoseName = "";

            List<Order> order = new List<Order>();
            foreach (var item in cfmxlist)
            {
                Order itemorder = new Order();
                itemorder.Seqno = item.Seqno;
                itemorder.Reqno = item.Reqno;
                itemorder.ItemID = item.ItemID;//先传sfcode 后面字典对应
                itemorder.ItemName = item.ItemName;
                itemorder.BodyPart = "";
                itemorder.Qty = item.Qty;
                itemorder.Price = item.Price;
                itemorder.SN = "";
                order.Add(itemorder);
            }
            request.Order = order;
            patient.Request = request;
            checkApplicationfromDTO.Patient = patient;
            checkApplicationfromDTO.RequestHost = requestHost;
            return checkApplicationfromDTO;
        }

        public CheckApplicationfromDTO retApplicationform(string mzh, string cfnmList, string orgid, string typezt)
        {
            CheckApplicationfromDTO checkApplicationfromDTO = new CheckApplicationfromDTO();
            string sql = @"select gh.xm,(case when gh.xb='1'then '男' else '女'end)xb,gh.csny,gh.zjh, cf.cfh Reqno,cf.CreateTime Indate,cf.ys DoctorID,cf.ysmc DoctorName,gh.mzh RecordID,cf.ks WardNo, cf.cfh CardID,xm.ztmc ApplyName,cf.ks ApplyDepartmentCode,cf.ksmc ApplyDepartmentName,convert(varchar(50),xm.xmnm) Seqno,xm.sfxm ItemID,sf.sfxmmc ItemName,convert(varchar(10),xm.sl) Qty,convert(varchar(10),xm.dj) Price from [NewtouchHIS_Sett]..mz_cf cf 
left join [NewtouchHIS_Sett]..mz_xm xm  on cf.cfnm=xm.cfnm  and cf.organizeid=xm.organizeid and xm.zt=1
left join [NewtouchHIS_Base]..xt_sfxm sf on xm.sfxm=sf.sfxmcode and xm.organizeid=sf.organizeid
--left join [NewtouchHIS_Sett]..mz_cf cf on xm.cfnm=cf.cfnm  and cf.organizeid=xm.organizeid and cf.zt=1
left join [NewtouchHIS_Sett]..mz_gh gh on gh.ghnm=cf.ghnm and cf.organizeid=gh.organizeid and gh.zt=1
where cf.zt=1  and cf.cfh =@cfh  and gh.mzh=@mzh and cf.organizeid=@orgId and cf.cflxmc='检查处方' ";
            
            var cfmxlist = new List<CheckfromDTO>(); ;
            try
            {
                cfmxlist = this.FindList<CheckfromDTO>(sql,
                new[] { new SqlParameter("@orgId", orgid), new SqlParameter("@cfh", cfnmList), new SqlParameter("@mzh", mzh) });
            }
            catch (Exception e)
            {

                throw;
            }


            if (cfmxlist.Count == 0 || cfmxlist == null)
            {
                return null;
            }
            RequestHost requestHost = new RequestHost();
            requestHost.MedNo = "testclinic1";//先传入测试
            requestHost.MedName = "正德测试1";//先传入测试

            Patient patient = new Patient();
            patient.Name = cfmxlist[0].xm;
            patient.Gender = cfmxlist[0].xb;
            patient.Birthday = cfmxlist[0].csny;
            patient.IdentityID = cfmxlist[0].zjh;
            patient.WardNo = cfmxlist[0].WardNo;
            patient.BedNo = "";
            patient.CardID = cfmxlist[0].CardID;

            Request request = new Request();
            request.Reqno = cfmxlist[0].Reqno;
            request.Indate = DateTime.Parse(cfmxlist[0].Indate.ToDateTimeString()); ;
            request.DoctorID = cfmxlist[0].DoctorID;
            request.DoctorName = cfmxlist[0].DoctorName;
            request.DoctorIdCard = "";
            request.RecordID = cfmxlist[0].RecordID;
            request.Source = "MagQ700001";
            request.Pic = "";
            request.PicDetail = "";//先放空 后面字典对应
            request.ApplyName = cfmxlist[0].ApplyName;
            request.WhichNo = null;
            request.IsVaild = typezt;
            request.IsCharge = "";
            request.ChargeDate = DateTime.Now;
            request.InvoiceID = "";
            request.CancelDate = null;
            request.Comment = "";
            request.ApplyDepartmentCode = cfmxlist[0].ApplyDepartmentCode;
            request.ApplyDepartmentName = cfmxlist[0].ApplyDepartmentName;
            request.DiagnoseName = "";

            List<Order> order = new List<Order>();
            foreach (var item in cfmxlist)
            {
                Order itemorder = new Order();
                itemorder.Seqno = item.Seqno;
                itemorder.Reqno = item.Reqno;
                itemorder.ItemID = item.ItemID;//先传sfcode 后面字典对应
                itemorder.ItemName = item.ItemName;
                itemorder.BodyPart = "";
                itemorder.Qty = item.Qty;
                itemorder.Price = item.Price;
                itemorder.SN = "";
                order.Add(itemorder);
            }
            request.Order = order;
            patient.Request = request;
            checkApplicationfromDTO.Patient = patient;
            checkApplicationfromDTO.RequestHost = requestHost;
            return checkApplicationfromDTO;
        }

        public int pushApplicationformRef(string cfh, string orgid,int sqdzt)
        {
            int refcount = 0;
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var cfzbEntityList = db.IQueryable<OutpatientPrescriptionEntity>(p => p.zt == "1" && p.OrganizeId == orgid && p.cfh == cfh).ToList();
                if (cfzbEntityList.Count == 0 || cfzbEntityList == null)
                {
                    //指定的处方 参数有问题 对不上
                    //throw new FailedException("PARAMS_ERROR_CFNMLIST_OR_CFHLIST", "参数错误定位处方异常");
                    return refcount;
                }
                foreach (var cfzbEntity in cfzbEntityList)
                {

                    cfzbEntity.sqdzt = sqdzt;
                    //cfzbEntity.Modify();
                    db.Update(cfzbEntity);
                }
                refcount = db.Commit();
            }
            return refcount;
        }

		/// <summary>
		/// 开立物资项目扣减库存数量 扣减冻结数量
		/// </summary>
		/// <param name="cfh"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public int Sumwzdj(string[] cfh, string orgId, string rygh)
		{
			foreach (var item in cfh)
			{
                string sql = "exec Newtouch_CIS..物资扣减库存量 @cfh,@orgId,@rygh";//"exec NewtouchHIS_herp..herp_物资扣减库存量 @cfh,@orgId,@rygh";
				var pars = new List<SqlParameter>();
				pars.Add(new SqlParameter("@cfh", item));
				pars.Add(new SqlParameter("@orgId", orgId));
				pars.Add(new SqlParameter("@rygh", rygh));
				this.ExecuteSqlCommand(sql, pars.ToArray());
			}

			return 1;
		}

		public int TuiHuiwzdj(string[] cfh, string orgId, string rygh)
		{
			foreach (var item in cfh)
			{
				string sql = "exec Newtouch_CIS..物资处方退费库存量退还 @cfh,@orgId,@rygh,NULL";
				var pars = new List<SqlParameter>();
				pars.Add(new SqlParameter("@cfh", item));
				pars.Add(new SqlParameter("@orgId", orgId));
				pars.Add(new SqlParameter("@rygh", rygh));
				this.ExecuteSqlCommand(sql, pars.ToArray());
			}

			return 1;
		}
	}
}
