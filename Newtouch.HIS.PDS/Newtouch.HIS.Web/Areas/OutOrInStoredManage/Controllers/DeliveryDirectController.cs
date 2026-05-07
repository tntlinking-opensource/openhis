using Newtouch.HIS.Application.Implementation;
using Newtouch.HIS.Domain.DTO.OutOrInStoredOperate;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.Infrastructure;
using Newtouch.Tools;
using System;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.OutOrInStoredManage.Controllers
{
    /// <summary>
    /// 直接出库
    /// </summary>
    public class DeliveryDirectController : ControllerBase
    {
        private readonly IDeliveryDirectDmnService deliveryDirectDmnService;

        public ActionResult From()
        {
            return View();
        }

        /// <summary>
        /// submit
        /// </summary>
        /// <param name="djInfoDTO"></param>
        /// <returns></returns>
        public ActionResult SubmitDeliveryDirect(DjInfoDTO djInfoDTO)
        {
            djInfoDTO.djlx = (int)EnumDanJuLX.zhijiefayao;
            djInfoDTO.ckbm = Constants.CurrentYfbm.yfbmCode;
            var result = new DeliveryDirectProcess(djInfoDTO).Process();
            return result.IsSucceed ? Success() : Error(result.ResultMsg);
        }
        /// <summary>
        /// get入库发票
        /// </summary>
        /// <param name="djlx"></param>
        /// <param name="fph"></param>
        /// <param name="kssj"></param>
        /// <param name="jssj"></param>
        /// <returns></returns>
        public ActionResult GetRkFphData(int djlx,string fph,DateTime kssj,DateTime jssj)
        {
            var data = deliveryDirectDmnService.GetRkFphData(djlx,fph,kssj,jssj);
            return Content(data.ToJson());
        }
        /// <summary>
        /// 药库入库发票明细
        /// </summary>
        /// <param name="djlx"></param>
        /// <param name="fph"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public ActionResult GetRkFphMxData(int djlx, string fph, string pc)
        {
            var data = deliveryDirectDmnService.GetRkFphMxData(djlx, fph, pc);
            return Content(data.ToJson());
        }

    }
}