using Newtouch.Herp.Application.Implementation.BillApprovalProcess;
using Newtouch.Herp.Application.Interface;
using Newtouch.Herp.Domain.DTO.InputDto;
using Newtouch.Herp.Domain.DTO.OutputDto;
using Newtouch.Herp.Infrastructure.Enum;

namespace Newtouch.Herp.Application.Implementation
{
    /// <summary>
    /// 出入库单审核
    /// </summary>
    public class OutOrInStorageBillApprovalApp : AppBase, IOutOrInStorageBillApprovalApp
    {

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="billApprovalDTO"></param>
        /// <returns></returns>
        public string Approval(BillApprovalDTO billApprovalDTO)
        {
            if (billApprovalDTO == null) return "单据ID不能为空";
            IProcess process;
            billApprovalDTO.OrganizeId = OrganizeId;
            var result = new ActResult();
            switch (billApprovalDTO.djlx)
            {
                case (int)EnumOutOrInStorageBillType.Wbrk:
                    process = new WbrkProcess(billApprovalDTO);
                    result = process.Process();
                    break;
                case (int)EnumOutOrInStorageBillType.Wbck:
                    process = new WbckProcess(billApprovalDTO);
                    result = process.Process();
                    break;
                case (int)EnumOutOrInStorageBillType.Zjck:
                    process = new DirectDeliveryProcess(billApprovalDTO);
                    result = process.Process();
                    break;
                case (int)EnumOutOrInStorageBillType.Nbth:
                    process = new DeliveryOfReturnProcess(billApprovalDTO);
                    result = process.Process();
                    break;
                default:
                    result.IsSucceed = false;
                    result.ResultMsg = "未找到符合要求的审核过程";
                    break;
            }
            return result.ResultMsg;
        }
    }
}
