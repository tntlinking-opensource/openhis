using FrameworkBase.MultiOrg.Application;
using Newtouch.HIS.Application.Interface;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.Infrastructure;

namespace Newtouch.HIS.Application.Implementation
{
    /// <summary>
    /// 首页统计
    /// </summary>
    internal class HomePageStatisticsApp : AppBase, IHomePageStatisticsApp
    {
        private readonly ISysPharmacyDepartmentRepo _sysPharmacyDepartmentRepo;
        private readonly IPyDmnService pyDmnService;
        private string _mzzybz;

        /// <summary>
        /// 获取代办HTML
        /// </summary>
        /// <returns></returns>
        public string AssembleNeedDealHtml()
        {
            var result = "";
            if (string.IsNullOrWhiteSpace(_mzzybz))
            {
                _mzzybz = _sysPharmacyDepartmentRepo.GetMzzybzByCode(Constants.CurrentYfbm.yfbmCode, OrganizeId);
            }
            if (string.IsNullOrWhiteSpace(_mzzybz)) return result;
            switch (_mzzybz.Trim())
            {
                case "0":
                    result = AssembleYkNeedDelaHtml();
                    break;
                case "1":
                    result = AssembleMzyfNeedDelaHtmlV2();
                    break;
                case "2":
                    result = AssembleZyyfNeedDelaHtml();
                    break;
                default:
                    result = AssembleMzyfNeedDelaHtmlV2();
                    break;
            }
            return result;
        }

        /// <summary>
        /// 组装药库代办HTML
        /// </summary>
        /// <returns></returns>
        private string AssembleYkNeedDelaHtml()
        {
            return @"
                    <div class='dashboard-statss' menuName='调价审核' menuUrl='/DrugStorage/PriceAdjustmentApproval'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_tjshCount'></h2>
                            <h5>调价审核</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-cny fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='出库待审批' menuUrl='/PharmacyDrugStorage/ReceiptApproval?djlx=2'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_ckdshCount'></h2>
                            <h5>出库待审批</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-truck fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='入库待审核' menuUrl='/PharmacyDrugStorage/ReceiptApproval'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_rkdshCount'></h2>
                            <h5>入库待审核</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-truck fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='未出库申领单' menuUrl='/DrugStorage/ApplyDelivery'>
                        <div class='dashboard-stats-item' style='background-color: #f3ce85;'>
                            <h2 class='m-top-none' id='h_sldshCount'></h2>
                            <h5>未出库申领单</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-shopping-cart fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' style='display: none'  menuName='1个月过期药品' menuUrl='/StorageManage/DrugStockSearch/ExpiredDrugsView?type=1'>
                          < div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_todayDrugCount'></h2>
                            <h5>1个月过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='过期药品' menuUrl='/Medicine/ExpiredDrugs'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_expriedDrugCount'></h2>
                            <h5>过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div> ";
        }

        /// <summary>
        /// 组装门诊药房代办HTML
        /// </summary>
        /// <returns></returns>
        private string AssembleMzyfNeedDelaHtml()
        {
            return @"
                    <div class='dashboard-stats' menuName='调价药品' menuUrl='/DrugStorage/PriceAdjustmentHistory'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_tjypCount'></h2>
                            <h5>调价药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exchange fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='入库审核' menuUrl='/PharmacyDrugStorage/ReceiptApproval'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_rkdshCount'></h2>
                            <h5>入库审核</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-truck fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='门诊待排处方' menuUrl='/OutPatientPharmacy/DrugArrangement2018'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_mzdpCount'></h2>
                            <h5>门诊待排处方</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-tasks fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='门诊待发处方' menuUrl='/OutPatientPharmacy/DrugDelivery2018'>
                        <div class='dashboard-stats-item' style='background-color: #f3ce85;'>
                            <h2 class='m-top-none' id='h_mzdfCount'></h2>
                            <h5>门诊待发处方</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-send-o fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='过期药品' menuUrl='/Medicine/ExpiredDrugs'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_expriedDrugCount'></h2>
                            <h5>过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div> ";
        }

        /// <summary>
        /// 组装门诊药房代办HTML
        /// </summary>
        /// <returns></returns>
        private string AssembleMzyfNeedDelaHtmlV2()
        {
            return @"
                    <div class='dashboard-stats' menuName='调价药品' menuUrl='/DrugStorage/PriceAdjustmentHistory'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_tjypCount'></h2>
                            <h5>调价药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exchange fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='入库审核' menuUrl='/BillManage/OutOrInStorageBill/Approval'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_rkdshCount'></h2>
                            <h5>入库审核</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-truck fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='门诊待发处方' menuUrl='/OutPatientPharmacy/DrugDelivery2018'>
                        <div class='dashboard-stats-item' style='background-color: #f3ce85;'>
                            <h2 class='m-top-none' id='h_mzdfCount'></h2>
                            <h5>门诊待发处方</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-send-o fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' style='display: none' menuName='1个月过期药品' menuUrl='/StorageManage/DrugStockSearch/ExpiredDrugsView?type=1'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_todayDrugCount'></h2>
                            <h5>1个月过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-stats' menuName='过期药品' menuUrl='/Medicine/ExpiredDrugs'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_expriedDrugCount'></h2>
                            <h5>过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div> ";
        }

        /// <summary>
        /// 组装住院药房代办HTML
        /// </summary>
        /// <returns></returns>
        private string AssembleZyyfNeedDelaHtml()
        {
            return @"
                    <div class='dashboard-statss' menuName='调价药品' menuUrl='/DrugStorage/PriceAdjustmentHistory'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_tjypCount'></h2>
                            <h5>调价药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exchange fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='入库审核' menuUrl='/PharmacyDrugStorage/ReceiptApproval'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_rkdshCount'></h2>
                            <h5>入库审核</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-truck fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='住院待发药数' menuUrl='/HospitalizationPharmacy/DispenseIndex'>
                        <div class='dashboard-stats-item' style='background-color: #65cea7;'>
                            <h2 class='m-top-none' id='h_zydfCount'></h2>
                            <h5>住院待发药数</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-send-o fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='住院待退药数' menuUrl='/HospitalizationPharmacy/RepercussionIndex'>
                        <div class='dashboard-stats-item' style='background-color: #f3ce85;'>
                            <h2 class='m-top-none' id='h_zydtCount'></h2>
                            <h5>住院待退药数</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-rotate-left fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' style='display: none' menuName='1个月过期药品' menuUrl='/StorageManage/DrugStockSearch/ExpiredDrugsView?type=1'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_todayDrugCount'></h2>
                            <h5>1个月过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div>
                    <div class='dashboard-statss' menuName='过期药品' menuUrl='/Medicine/ExpiredDrugs'>
                        <div class='dashboard-stats-item' style='background-color: #fc8675;'>
                            <h2 class='m-top-none' id='h_expriedDrugCount'></h2>
                            <h5>过期药品</h5>
                            <div class='stat-icon'>
                                <i class='fa fa-exclamation-triangle fa-3x'></i>
                            </div>
                        </div>
                    </div> ";
        }

        /// <summary>
        /// 获取代办总数
        /// </summary>
        /// <returns></returns>
        public NeedDealCountVO GetPendingCount()
        {
            var result = new NeedDealCountVO();
            if (string.IsNullOrWhiteSpace(_mzzybz))
            {
                _mzzybz = _sysPharmacyDepartmentRepo.GetMzzybzByCode(Constants.CurrentYfbm.yfbmCode, OrganizeId);
            }
            if (string.IsNullOrWhiteSpace(_mzzybz)) return result;
            switch (_mzzybz.Trim())
            {
                case "0":
                    result = pyDmnService.GetNeedDealCountByYk();
                    break;
                case "1":
                case "2":
                default:
                    result = pyDmnService.GetNeedDealCountByYf();
                    break;
            }
            return result;
        }
    }
}
