using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.Medicine;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.V;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.OutPatientPharmacy;
using Newtouch.HIS.Domain.ValueObjects.SystemManage;
using Newtouch.HIS.Domain.VO;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;

namespace Newtouch.HIS.Domain.IDomainServices
{
    /// <summary>
    /// 发药
    /// </summary>
    public interface IfyDmnService
    {
        /// <summary>
        /// 通过处方信息，获取处方明细
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        IList<fydisplayDetailInfo> GetDetailInfo(List<fyDetailListRequest> reqdata);

        /// <summary>
        /// 获取发药明细
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        IList<fydisplayDetailInfo> GetAllDetailInfo(List<fyDetailListRequest> reqdata);

        ///// <summary>
        ///// 门诊发药
        ///// </summary>
        ///// <param name="reqdata"></param>
        ///// <returns></returns>
        //List<string> ExecMzHandoutMedicine(List<GetfyDetailCFList> reqdata);

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        List<string> MzDistributingMedicines(List<GetfyDetailCFList> reqdata);

        /// <summary>
        /// 门诊退药（mz_cfmxph验证）
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        List<tyCFMainInfo> ExecMZExitMedicine(List<tyCFMainInfo> reqdata);

        /// <summary>
        /// 获取收货部门
        /// </summary>
        /// <returns></returns>
        List<DrugSpecialPropertiesVO> GetMeidicineSHBMList();

        /// <summary>
        /// 归还冻结库存
        /// </summary>
        /// <param name="list"></param>
        void ReleaseFrozenStock(List<OutpatientPrescriptionDetailBatchNumberEntity> list);

        /// <summary>
        /// 门诊发药查询显示详细信息(mz_cfmxph验证)
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        List<fydisplayDetailInfo> GetMzHandOutDetail(List<fyDetailListRequest> reqdata);

        /// <summary>
        /// 获取发药药品详细
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        List<fyMeidicneInfo> GetDeliveryDrugs(string cfh, EnumOperateType operateType = EnumOperateType.Fy);

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        string UpdateStock(string cfh, string yfbmCode);

        /// <summary>
        /// 门诊发药V2.0
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        string ExecOutpatientDispensingDrugV2(string cfh, string yfbmCode, string userCode, string organizeId,string zsm);

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        string ExecMzHandoutMedicine(string cfh, string yfbmCode, string userCode, string organizeId);

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="ypCode"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        string UpdateStock(string cfh, string ypCode, string yfbmCode, string userCode, string organizeId);


        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <returns></returns>
        string ReturnDrug(string cfh);

        /// <summary>
        /// 获取已发处方
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="fph"></param>
        /// <param name="cfh"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        IList<cfInfoVo> GetDeliveredCf(string keyWord, string fph, string cfh, Pagination pagination = null);

        /// <summary>
        /// 获取发药信息
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="keyWork"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<DrugDeliveryEntity> GetDrugDeliveryList(DateTime startTime, DateTime endTime, string keyWork, string yfbmCode, string organizeId);


        /// <summary>
        /// 门诊发药处方明细查询
        /// </summary>
        /// <param name="xm"></param>
        /// <param name="cardNo"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        List<CfmxVO> SelectCfmx(string xm, string cardNo, string cfh, string organizeId, string yfbmCode);

        /// <summary>
        /// 已发药查询 处方信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword">cardNo or xm</param>
        /// <param name="invoiceNo"></param>
        /// <param name="cfh"></param>
        /// <param name="operateType"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<CffyjlVO> SelectDispensedRpInfo(Pagination pagination, string keyword, string invoiceNo, string cfh, EnumOperateType operateType, DateTime kssj, DateTime jssj, string yfbmCode, string organizeId);

        /// <summary>
        /// 查询处方信息
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="fph"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        IList<CfxxVO> SelectRpList(string keyword, string fph, DateTime kssj, DateTime jssj, string yfbmCode, string organizeId);

        /// <summary>
        /// 查询处方明细
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<CfmxVO> SelectRpDetail(string cfh, string yfbmCode, string organizeId);

        /// <summary>
        /// 判断是该处方是否存在
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns>返回有效的处方数</returns>
        int ExistEffectiveRp(string cfh, string yfbmCode, string organizeId);

        /// <summary>
        /// 根据卡号和姓名获取处方信息
        /// </summary>
        /// <param name="yfbmCode"></param>
        /// <param name="cardNo"></param>
        /// <param name="xm"></param>
        /// <param name="fybz"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        List<MzCfEntity> GetRpInfo(string yfbmCode, string cardNo, string xm, EnumFybz fybz = EnumFybz.Yp,
           string organizeId = "");

        /// <summary>
        /// 查询药品大类
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        IList<YpdlDTO> getYpdl(string orgId);


        #region 药品 、耗材使用情况
        List<YfMaterialTjVo> GetMaterialList(Pagination pagination, string orgId, string ks, string ry, string slly, DateTime kssj, DateTime jssj, string keyword);

        #endregion

        #region 门诊处方
        IList<MzcfcxList> GetMzcfList(Pagination pagination, MzcfcxVo req);
        IList<MzcfcxDetailList> GetMzcfDetailList(Pagination pagination, MzcfcxVo req);

        #endregion

        #region 医保电子处方
        /// <summary>
        /// 获取电子处方病人信息
        /// </summary>
        /// <returns></returns>
        List<DzcfBrxxDTO> GetElectronicPrescriptionCfInfo(string cfh, string xm, string organizeId = "");

        #endregion
    }
}
