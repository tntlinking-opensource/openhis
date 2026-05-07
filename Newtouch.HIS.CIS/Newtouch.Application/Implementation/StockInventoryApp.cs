using System;
using System.Collections.Generic;
using System.Linq;
using FrameworkBase.MultiOrg.Application;
using Newtouch.Application.Interface;
using Newtouch.Common.Operator;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Domain.DTO.InputDto;
using Newtouch.Domain.Entity.DeptStorageManage;
using Newtouch.Domain.IDomainServices.DeptStorageManage;
using Newtouch.Domain.IRepository.DeptStorageManage;
using Newtouch.Domain.ValueObjects.Storage;
using Newtouch.Infrastructure;

namespace Newtouch.Application.Implementation
{
    /// <summary>
    /// 盘点
    /// </summary>
    public class StockInventoryApp : AppBase, IStockInventoryApp
    {
        private readonly IStockInventoryDmnService _stockInventoryDmnService;
        private readonly IKcPdxxRepo _kcPdxxRepo;
        private readonly IKcPdxxmxRepo _kcPdxxmxRepo;

        /// <summary>
        /// 开始盘点
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public VInventoryDateDropDownEntity StartInventory(string warehouseId, string organizeId)
        {
            var pdDate = _stockInventoryDmnService.GetHangUpPdDates(warehouseId, organizeId);
            if (pdDate != null && pdDate.Count > 0)
            {
                return null;
            }
            var inventoryResult = _stockInventoryDmnService.GenerateInventoryInfo(warehouseId, organizeId);
            if (!string.IsNullOrEmpty(inventoryResult))
            {
                throw new FailedException(inventoryResult);
            }
            var result = _stockInventoryDmnService.GetPdSj(warehouseId, organizeId).FirstOrDefault();
            if (result == null)
            {
                throw new FailedException("重新获取盘点时间失败");
            }
            return result;
        }

        /// <summary>
        /// 盘点保存
        /// </summary>
        /// <param name="inventoryInfoList"></param>
        /// <param name="pdId">盘点单ID</param>
        /// <param name="noPc">  0：按批次盘点 1：不按批次盘点</param>
        public void SaveInventoryInfo(List<SaveInventoryDTO> inventoryInfoList, long pdId, string noPc = "0")
        {
            var pdInfo = _kcPdxxRepo.SelectPdInfByPdId(pdId, OperatorProvider.GetCurrent().OrganizeId);
            if (pdInfo == null)
            {
                throw new FailedException("未找到指定盘点信息");
            }
            if (pdInfo.jssj != null || pdInfo.jssj >= Constants.MinDateTime)
            {
                throw new FailedException("该盘点已结束");
            }
            SaveInventoryInfo(inventoryInfoList, pdId, noPc, UserIdentity.DepartmentCode, OrganizeId);
        }

        /// <summary>
        /// 盘点保存
        /// </summary>
        /// <param name="inventoryInfoList"></param>
        /// <param name="pdId"></param>
        /// <param name="noPc">0：按批次盘点 1：不按批次盘点</param>
        /// <param name="warehouseId"></param>
        /// <param name="organizeId"></param>
        private void SaveInventoryInfo(List<SaveInventoryDTO> inventoryInfoList, long pdId, string noPc, string warehouseId, string organizeId)
        {
            switch (noPc)
            {
                case "0":
                    _kcPdxxmxRepo.UpdateSlBySaveInventoryInfo(inventoryInfoList);
                    break;
                case "1":
                    var result = _stockInventoryDmnService.SaveInventoryDetailNoPc(inventoryInfoList, pdId, warehouseId, organizeId);
                    if (!string.IsNullOrWhiteSpace(result)) throw new FailedException(result);
                    break;
            }
        }

        /// <summary>
        /// 取消盘点
        /// </summary>
        /// <param name="pdId"></param>
        public void CancelInventory(long pdId)
        {
            //判断是否有未完成的盘点
            var count = _kcPdxxRepo.SelectUnfinishedInventoryByPdId(pdId, OrganizeId);
            if (count != 1)
            {
                throw new FailedException("该盘点已结束,不能取消");
            }
            _stockInventoryDmnService.CancelInventory(pdId);
        }

        /// <summary>
        /// 结束盘点
        /// </summary>
        /// <returns></returns>
        public void EndInventory(long pdId)
        {
            var pdInfo = _kcPdxxRepo.SelectPdInfByPdId(pdId, OrganizeId);
            if (pdInfo == null)
            {
                throw new FailedException("未找到指定盘点信息");
            }
            if (pdInfo.jssj != null || pdInfo.jssj >= Constants.MinDateTime)
            {
                throw new FailedException("该盘点已结束");
            }
            //生成盘点信息
            var resultMessage = _stockInventoryDmnService.EndInventory(pdId, OrganizeId, OperatorProvider.GetCurrent().UserCode);
            if (!string.IsNullOrEmpty(resultMessage) && !resultMessage.Contains("成功"))
            {
                throw new FailedException(resultMessage);
            }
        }

        /// <summary>
        /// 提交损益
        /// </summary>
        /// <param name="plist"></param>
        /// <param name="warehouseId"></param>
        /// <returns>error msg</returns>
        public string LossAndProfitSubmit(List<LossAndProditSubmitDTO> plist, string warehouseId)
        {
            var syxxlist = AssembleSyxxEntities(plist, UserIdentity.DepartmentCode);
            if (syxxlist.Count <= 0) return "缺少损益信息";
            _stockInventoryDmnService.InsertSyxxBatch(syxxlist, UserIdentity.DepartmentCode, OrganizeId);
            return "";
        }

        /// <summary>
        /// 组装损益信息
        /// </summary>
        /// <param name="plist"></param>
        /// <param name="warehouseId"></param>
        /// <returns></returns>
        private List<KcSyxxEntity> AssembleSyxxEntities(List<LossAndProditSubmitDTO> plist, string warehouseId)
        {
            if (plist == null || plist.Count <= 0) return new List<KcSyxxEntity>();
            var result = new List<KcSyxxEntity>();
            plist.ForEach(p =>
            {
                result.Add(new KcSyxxEntity
                {
                    Bgsj = DateTime.Now,
                    CreateTime = DateTime.Now,
                    CreatorCode = OperatorProvider.GetCurrent().UserCode,
                    Djh = p.Djh,
                    Jj = p.Jj,
                    LastModifierCode = null,
                    LastModifyTime = null,
                    Lsj = p.Lsj,
                    OrganizeId = OrganizeId,
                    pc = p.pc,
                    Ph = p.Ph,
                    productId = p.productId,
                    px = 0,
                    remark = p.remark,
                    Sykc = p.Sykc,
                    Sysl = p.Sysl = p.sybz == ((int)EnumSybz.Loss).ToString() ? -1 * p.Sysl : p.Sysl,
                    Syyy = p.syyyId,
                    UnitId = p.UnitId,
                    ks = warehouseId,
                    Yxq = p.Yxq,
                    Zhyz = p.zhyz,
                    Zrr = p.Zrr,
                    zt = "1"
                });
            });
            return result;
        }
    }
}
