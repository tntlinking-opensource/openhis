using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Tools;
using System.Web.Mvc;
using System.Linq;
using Newtouch.Common;
using System.Collections.Generic;
using Newtouch.HIS.Repository;

namespace Newtouch.HIS.Base.HOSP.Controllers
{
    /// <summary>
    /// 系统收费大类
    /// </summary>
    public class ChargeCategoryController : ControllerBase
    {
        private readonly ISysChargeCategoryRepo _SysChargeCategoryRepo;
        private readonly ISysOrganizeDmnService _SysOrganizeDmnService;
        private readonly ISysChargeCategoryBaseRepo _SysChargeCategoryBaseRepo;

        public ChargeCategoryController(ISysChargeCategoryRepo SysChargeCategoryRepo
            , ISysOrganizeDmnService sysOrganizeDmnService , ISysChargeCategoryBaseRepo SysChargeCategoryBaseRepo)
        {
            this._SysChargeCategoryRepo = SysChargeCategoryRepo;
            this._SysOrganizeDmnService = sysOrganizeDmnService;
            this._SysChargeCategoryBaseRepo = SysChargeCategoryBaseRepo;
        }

        //收费大类树形（医院） 下拉 数据源
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTreeSelectJson(string organizeId, string treeidFieldName = "Code")
        {
            var data = _SysChargeCategoryRepo.GetValidList(organizeId);
            var data1 = _SysChargeCategoryBaseRepo.GetValidList(organizeId);
            var treeList = new List<TreeSelectModel>();
           // 基础数据
            if (organizeId .Equals("*") )
            {
                // 遍历 data1
                foreach (var item in data1)
                {
                    TreeSelectModel treeModel = new TreeSelectModel();
                    if (treeidFieldName == "Code")
                    {
                        treeModel.id = item.dlCode;
                        treeModel.text = item.dlmc;
                        treeModel.parentId = (item.ParentId.HasValue && item.ParentId.Value != 0)
                            ? data1.Where(p => p.dlId == item.ParentId).Select(p => p.dlCode).FirstOrDefault()
                            : null;
                    }
                    else
                    {
                        treeModel.id = item.dlId.ToString();
                        treeModel.text = item.dlmc;
                        treeModel.parentId = (item.ParentId.HasValue && item.ParentId.Value != 0) ? item.ParentId.ToString() : null;
                    }
                    treeList.Add(treeModel);
                }
            }
            else
            {
                // 遍历 data
                foreach (var item in data)
                {
                    TreeSelectModel treeModel = new TreeSelectModel();
                    if (treeidFieldName == "Code")
                    {
                        treeModel.id = item.dlCode;
                        treeModel.text = item.dlmc;
                        treeModel.parentId = (item.ParentId.HasValue && item.ParentId.Value != 0)
                            ? data.Where(p => p.dlId == item.ParentId).Select(p => p.dlCode).FirstOrDefault()
                            : null;
                    }
                    else
                    {
                        treeModel.id = item.dlId.ToString();
                        treeModel.text = item.dlmc;
                        treeModel.parentId = (item.ParentId.HasValue && item.ParentId.Value != 0) ? item.ParentId.ToString() : null;
                    }
                    treeList.Add(treeModel);
                }
            }

            return Content(treeList.TreeSelectJson(null));

        }

        /// <summary>
        /// 获取树形列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult GetTreeGridJson(string organizeId, string zt,string keyword)
        {
            if (!organizeId.Equals("*"))
            {
                //非基础数据
                var data = _SysChargeCategoryRepo.GetList(organizeId,zt);
                if (!string.IsNullOrEmpty(keyword))
                {
                    data = data.ToList().TreeWhereForKeyInt(t => t.dlmc.Contains(keyword), keyValue: "dlId",
                        parentId: "ParentId");
                }

                var treeList = new List<TreeGridModel>();
                foreach (var item in data)
                {
                    TreeGridModel treeModel = new TreeGridModel();
                    treeModel.id = item.dlId.ToString();
                    bool hasChildren = data.Count(t => t.ParentId == item.dlId) == 0 ? false : true;
                    treeModel.id = item.dlId.ToString();
                    treeModel.isLeaf = hasChildren;
                    treeModel.parentId = (item.ParentId.HasValue && item.ParentId.Value != 0)
                        ? item.ParentId.ToString()
                        : null;
                    treeModel.expanded = hasChildren;
                    treeModel.entityJson = item.ToJson();
                    treeList.Add(treeModel);
                }

                return Content(treeList.TreeGridJson(null));
            }
            else
            {
                var data = _SysChargeCategoryBaseRepo.GetList(organizeId);
                if (!string.IsNullOrEmpty(keyword))
                {
                    data = data.ToList().TreeWhereForKeyInt(t => t.dlmc.Contains(keyword), keyValue: "dlId",
                        parentId: "ParentId");
                }

                var treeList = new List<TreeGridModel>();
                foreach (var item in data)
                {
                    TreeGridModel treeModel = new TreeGridModel();
                    treeModel.id = item.dlId.ToString();
                    bool hasChildren = data.Count(t => t.ParentId == item.dlId) == 0 ? false : true;
                    treeModel.id = item.dlId.ToString();
                    treeModel.isLeaf = hasChildren;
                    treeModel.parentId = (item.ParentId.HasValue && item.ParentId.Value != 0)
                        ? item.ParentId.ToString()
                        : null;
                    treeModel.expanded = hasChildren;
                    treeModel.entityJson = item.ToJson();
                    treeList.Add(treeModel);
                }

                return Content(treeList.TreeGridJson(null));
            }
        }

        /// <summary>
        /// 新增或修改Form
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult GetFormJson(int keyValue, string orgId = null)
        {

            if ("*".Equals(orgId))
            {
                var entity = _SysChargeCategoryBaseRepo.GetForm(keyValue);
                return Content(entity.ToJson());
            }
            else
            {
                var entity = _SysChargeCategoryRepo.GetForm(keyValue);
                return Content(entity.ToJson());

            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="SysPatiChargeAddEntity"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult SubmitForm(SysChargeCategoryEntity entity, int? keyValue)
        {
            if (string.IsNullOrWhiteSpace(entity.mzprintreportcode))
            {
                entity.mzprintreportcode = entity.dlCode;
            }
            if (string.IsNullOrWhiteSpace(entity.mzprintbillcode))
            {
                entity.mzprintbillcode = entity.dlCode;
            }
            if (keyValue.HasValue && keyValue.Value > 0 && keyValue.Value == entity.ParentId)
            {
                throw new FailedException("上级大类选择错误");
            }
            if (!string.IsNullOrWhiteSpace(entity.fjCode))
            {
                entity.fjmc = ((Newtouch.Infrastructure.EnumSffjdlmc)int.Parse(entity.fjCode)).ToString();
            }
            entity.zt = entity.zt == "true" ? "1" : "0";
            if (string.IsNullOrWhiteSpace(entity.OrganizeId))
            {
                throw new FailedException("请选择组织机构");
            }
            else if (!_SysOrganizeDmnService.IsMedicalOrganize(entity.OrganizeId) && !"*".Equals(entity.OrganizeId) )
            {
                throw new FailedException("请选择医疗机构（医院或诊所）");
            }

            if ("*".Equals(entity.OrganizeId))
            {
                _SysChargeCategoryBaseRepo.SubmitForm(entity.ToJson().ToObject<SysChargeCategoryBaseEntity>(), keyValue);
            }
            else
            {
                _SysChargeCategoryRepo.SubmitForm(entity, keyValue);
            }
            return Success("操作成功。");
        }
        /// <summary>
        /// 修改状态
        /// </summary>
        /// <param name="SysPatiChargeAddEntity"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public ActionResult UpdateZt(int dlId,string zt,string organizeId, int? keyValue)
        {
            if ("*".Equals(organizeId))
            {
                var entity = _SysChargeCategoryBaseRepo.FindEntity(val => val.dlId == dlId);
                entity.zt = zt;
                _SysChargeCategoryBaseRepo.Update(entity);
            }
            else
            {
                var entity = _SysChargeCategoryRepo.FindEntity(val => val.dlId == dlId);
                entity.zt = zt;
                _SysChargeCategoryRepo.Update(entity);
            }
            return Success("操作成功。");
        }

    }
}