using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.OutputDto.DRGManage;
using Newtouch.HIS.Domain.IDomainServices.DRGManage;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Newtouch.HIS.Web.Areas.DRGManage.Controllers
{
    public class DRGTradUploadController : ControllerBase
    {
        private readonly IDRGTradUploadDmnService _DRGTradUploadDmnService;
		// GET: DRGManage/DRGTradUpload

		public ActionResult From()
		{
			return View();
		}

		[HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(string kssj, string jssj, string scqk, string zyh,string tradiNumber)
        {
            var data = new
            {
                rows = _DRGTradUploadDmnService.GetList(this.OrganizeId, kssj, jssj, scqk, zyh, tradiNumber)
            };
            return Content(data.ToJson());
        }


        //DRG上传
        public ActionResult DRGUpload(List<DRGUploadDto> list,string tradiNumber)
        {
            var data = _DRGTradUploadDmnService.DRGUpload(this.OrganizeId, list, tradiNumber);
            return Content(data.ToJson());
        }

        public ActionResult GetGridJsonYp(string kssj, string jssj, string scqk, string tradiNumber)
        {
            var data = new
            {
                rows = _DRGTradUploadDmnService.GetListYp(this.OrganizeId, kssj, jssj, scqk, tradiNumber)
            };
            return Content(data.ToJson());
        }
        public ActionResult GetGridJsonsqsz(string kssj, string jssj, string scqk, string zyh, string tradiNumber)
        {
            var data = new
            {
                rows = _DRGTradUploadDmnService.GetListsqsz(this.OrganizeId, kssj, jssj, scqk, zyh, tradiNumber)
            };
            return Content(data.ToJson());
        }

		public ActionResult GetGridyzList(string zyh,string types)
		{
			var data = new
			{
				rows = _DRGTradUploadDmnService.Getmxlist(this.OrganizeId, zyh, types)
			};
			return Content(data.ToJson());
		}
	}
}