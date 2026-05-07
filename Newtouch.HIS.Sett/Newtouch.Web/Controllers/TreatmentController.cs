using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtouch.Common;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IRepository.SystemManage;
using FrameworkBase.MultiOrg.Web;
using Newtouch.Tools;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtouch.Core.Common;

namespace Newtouch.HIS.Web.Controllers
{


    public class TreatmentController : OrgControllerBase
    {

        private readonly ITreatmentportfolioRepo _treatmentportfolio;


        // GET: Treatment
        public ActionResult Index()
        {

            return View();
        }


        public ActionResult AForm()
        {


            return View();
        }

        public ActionResult tianjia(TreatmentportfolioEntity entity, string keyValue)
        {

            //_treatmentportfolio.Insert(collection["OrganizeId"], collection["zhmc"], collection["zhcode"], collection["ord"], collection["zlxm"], collection["zlxmmc"], collection["price"], collection["zlxmpy"], collection["zhje"]);
            _treatmentportfolio.ADDInsert(entity, keyValue);

            return Success("操作成功");
        }




        /// <summary>
        /// 页面加载时的数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Login(string keyword)
        {


            //var data = keyword == "" ? _treatmentportfolio.Listselect().ToJson() : _treatmentportfolio.Keyword(keyword).ToJson();
            //var data = keyword == "" ? _treatmentportfolio.Loginview().ToJson() : _treatmentportfolio.Keyword(keyword).ToJson();
            //return Content(data);
            return View();

        }

        /// <summary>
        /// 查询收费项目组合
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="keyword"></param>
        /// <param name="OrganizeId"></param>
        /// <returns></returns>
        public ActionResult loginfy(Pagination pagination, string keyword, string OrganizeId)
        {
            var data = new
            {
                rows = _treatmentportfolio.Keyword(pagination, keyword, OrganizeId).ToJson(),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records,
            };
            return Content(data.rows);
        }

        /// <summary>
        /// 删除收费项目组合
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult deleteid(string keyValue, string orgId)
        {
            _treatmentportfolio.deleteid(keyValue, orgId);
            return Success("删除成功");
        }

        /// <summary>
        /// 删除收费组合中明细
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="zhcodemc"></param>
        /// <returns></returns>
        public ActionResult deletemc(string keyValue, string zhcodemc, string orgId = null)
        {
            _treatmentportfolio.deletemc(keyValue, zhcodemc, orgId ?? OrganizeId);
            return Success("删除成功");
        }

        /// <summary>
        /// 保存收费项目组合
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult rowdata(string dataset, string orgId = null)
        {
            JArray jar = JArray.Parse(dataset);
            for (int i = 0; i < jar.Count; i++)
            {
                JObject jobjar = JObject.Parse(jar[i].ToString());

                string zhcodetj = jobjar["zhcode"].ToString();
                string sfxmmc = jobjar["zlxmmc"].ToString();
                if (_treatmentportfolio.TJchaxun(zhcodetj, sfxmmc, orgId ?? OrganizeId).Count > 0)
                {
                    continue;
                }
                else
                {
                    TreatmentportfolioEntity addrow = new TreatmentportfolioEntity
                    {
                        zhmc = jobjar["zhmc"].ToString(),
                        zhcode = jobjar["zhcode"].ToString(),
                        ord = string.IsNullOrWhiteSpace(jobjar["ord"].ToString()) ? (int?)null: int.Parse(jobjar["ord"].ToString()),
                        zlxm = jobjar["zlxm"].ToString(),
                        zlxmmc = jobjar["zlxmmc"].ToString(),
                        price = decimal.Parse(jobjar["price"].ToString()),
                        zlxmpy = jobjar["zlxmpy"].ToString(),
                        OrganizeId = orgId ?? OrganizeId,
                        sfdl = jobjar["sfdlcode"].ToString()
                    };
                    _treatmentportfolio.ADDrowdata(addrow);
                }

            }

            return Success("添加成功");
        }
        /// <summary>
        /// 是否已存在收费项目组合
        /// </summary>
        /// <param name="zhcodetj"></param>
        /// <param name="sfxmmc123"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult Tjchaxun(string zhcodetj, string sfxmmc, string orgId = null)
        {

            var data = _treatmentportfolio.TJchaxun(zhcodetj, sfxmmc, orgId ?? OrganizeId);
            if (data.Count > 0)
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        /// <summary>
        /// 获取修改收费项目组合
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="zhcode"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        public ActionResult Codecx(TreatmentportfolioEntity entity, string zhcode, string orgId)
        {
            var data = JsonConvert.SerializeObject(_treatmentportfolio.Codecx(orgId ?? OrganizeId, zhcode));
            return Content(data);
        }


    }
}