using System;
using System.IO;
using System.Text;
using System.Web;
using Newtouch.Herp.Application.Interface;
using Newtouch.Herp.Domain.Entity;
using Newtouch.Herp.Domain.Entity.VEntity;
using Newtouch.Herp.Domain.IDomainServices;
using Newtouch.Herp.Domain.IRepository;
using Newtouch.Herp.Domain.ValueObjects;
using Newtouch.Herp.Infrastructure.Common;
using Newtouch.Herp.Infrastructure.Enum;
using Newtouch.Herp.Infrastructure.Log;
using Newtouch.Tools;

namespace Newtouch.Herp.Application.Implementation
{
    /// <summary>
    /// 物资维护
    /// </summary>
    public class ProductApp : AppBase, IProductApp
    {
        private readonly IWzProductRepo wzProductRepo;
        private readonly IRelProductUnitRepo _relProductUnitRepo;
        private readonly IWzUnitRepo _wzUnitRepo;
        private readonly ISupplierApp _supplierApp;
        private readonly IWzProductDmnService _wzProductDmnService;

        /// <summary>
        /// 物资维护表单提交
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public int SubmitForm(VProductInfoEntity source, string keyWord)
        {
            var wzProductEntity = TransformProductEntity(source);
            if (string.IsNullOrWhiteSpace(keyWord))
            {
                wzProductEntity.Create(true);
                if (_wzProductDmnService.InsertProductToSfxm(wzProductEntity) <= 0)
                {
                    throw new Exception("插入物资数据失败");
                }
                if (SubmitRelProductUnit(AssembleRelProductUnit(wzProductEntity), wzProductEntity.OrganizeId) <= 0)
                {
                    throw new Exception("初始化物资单位失败");
                }
				//同步到BASE库中
				_wzProductDmnService.InsertTOSfxm(wzProductEntity.OrganizeId, wzProductEntity.productCode);
				return 1;
            }
            var dbProduct = wzProductRepo.FindEntity(p => p.Id == keyWord);
            if (dbProduct == null) return 0;
            dbProduct.brand = wzProductEntity.brand;
            dbProduct.gg = wzProductEntity.gg;
            dbProduct.imageUrl = string.IsNullOrWhiteSpace(wzProductEntity.imageUrl) ? dbProduct.imageUrl : wzProductEntity.imageUrl;
            dbProduct.jj = wzProductEntity.jj;
            dbProduct.lsj = wzProductEntity.lsj;
            dbProduct.minUnit = wzProductEntity.minUnit;
            dbProduct.name = wzProductEntity.name;
            dbProduct.py = wzProductEntity.py;
            dbProduct.sffy = wzProductEntity.sffy;
            dbProduct.sfgt = wzProductEntity.sfgt;
            dbProduct.sflkc = wzProductEntity.sflkc;
            dbProduct.supplierId = wzProductEntity.supplierId;
            dbProduct.typeId = wzProductEntity.typeId;
            dbProduct.zczh = wzProductEntity.zczh;
            dbProduct.zt = wzProductEntity.zt;
            dbProduct.zxqds = wzProductEntity.zxqds;
            dbProduct.productCode = wzProductEntity.productCode;
			dbProduct.hcgjybdm = wzProductEntity.hcgjybdm;
			dbProduct.iswzsame = wzProductEntity.iswzsame;
			dbProduct.zfxz = wzProductEntity.zfxz;
			dbProduct.zfbl = wzProductEntity.zfbl;
			dbProduct.gjybdm = wzProductEntity.gjybdm;
			dbProduct.ybdm = wzProductEntity.ybdm;
            dbProduct.zblb = wzProductEntity.zblb;
            dbProduct.hslb = wzProductEntity.hslb;
            dbProduct.kcyjz = wzProductEntity.kcyjz;
            dbProduct.Modify();
			int updatecount= wzProductRepo.Update(dbProduct);

			//同步到BASE库中
			_wzProductDmnService.InsertTOSfxm(wzProductEntity.OrganizeId, wzProductEntity.productCode);

			return updatecount;

		}

		private WzProductEntity TransformProductEntity(VProductInfoEntity s)
		{
			return new WzProductEntity
			{
				brand = s.brand,
				CreateTime = s.CreateTime,
				CreatorCode = s.CreatorCode,
				gg = s.gg,
				Id = s.Id,
				imageUrl = s.imageUrl,
				jj = s.jj,
				LastModifierCode = s.LastModifierCode,
				LastModifyTime = s.LastModifyTime,
				lsj = s.lsj,
				minUnit = s.minUnit,
				name = s.name,
				OrganizeId = s.OrganizeId,
				py = s.py,
				sffy = s.sffy,
				sfgt = s.sfgt,
				sflkc = s.sflkc,
				supplierId = GetSupplierId(s.supplierId, s.supplierName),
				typeId = s.typeId,
				zczh = s.zczh,
				zt = s.zt,
				zxqds = s.zxqds,
				productCode = s.productCode,
				hcgjybdm = s.hcgjybdm,
				iswzsame = s.iswzsame,
				zfxz = s.zfxz,
				zfbl = s.zfbl,
				gjybdm = s.gjybdm,
				ybdm = s.ybdm,
                zblb=s.zblb,
                hslb=s.hslb,
                kcyjz=s.kcyjz
			};
		}

		/// <summary>
		/// 获取生产商ID
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="supplierName"></param>
		/// <returns></returns>
		private string GetSupplierId(string supplierId, string supplierName)
        {
            if (string.IsNullOrEmpty(supplierName))
            {
                return null;
            }
            return _supplierApp.CreateProducerQuick(supplierId, supplierName, OrganizeId);
        }

        /// <summary>
        /// 组装物资最小单位信息
        /// </summary>
        private ProductUnitRelVo AssembleRelProductUnit(WzProductEntity wzProductEntity)
        {
            return new ProductUnitRelVo
            {
                keyWord = wzProductEntity.Id,
                unitName = _wzUnitRepo.FindEntity(p => p.Id == wzProductEntity.minUnit).name,
                unitId = wzProductEntity.minUnit,
                zhyz = 1
            };
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="file"></param>
        /// <returns>图片上传路径</returns>
        public string UploadImag(HttpPostedFileBase file)
        {
            try
            {
                var result = "";
                if (file == null || !ValidateImg(file)) return result;
                var urn = Core.Common.Utils.ConfigurationHelper.GetAppConfigValue("imageUrn");
                urn = string.IsNullOrWhiteSpace(urn) ? "/Upload/productImg" : urn;
                var url = HttpContext.Current.Request.MapPath(urn);
                if (!FileHelper.IsExistDirectory(url)) FileHelper.CreateDirectory(url);
                var ext = file.FileName.GetFileNameExt();
                var newFileName = Guid.NewGuid() + ext;
                result = urn + "/" + newFileName;
                var parth = Path.Combine(url, newFileName);
                file.SaveAs(parth);
                return result;
            }
            catch (Exception ex)
            {
                LogCore.Error("UploadImag error", ex);
                return "";
            }
        }

        /// <summary>
        /// 提交物资单位
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public int SubmitRelProductUnit(ProductUnitRelVo rel, string organizeId)
        {
            if (string.IsNullOrWhiteSpace(rel.id))
            {
                var entity = new RelProductUnitEntity
                {
                    OrganizeId = organizeId,
                    productId = rel.keyWord,
                    unit = rel.unitName,
                    unitId = rel.unitId,
                    zhyz = rel.zhyz,
                    zt = ((int)Enumzt.Enable).ToString()
                };
                entity.Create(true);
                return _relProductUnitRepo.Insert(entity);
            }
            else
            {
                var entity = _relProductUnitRepo.FindEntity(p => p.Id == rel.id);
                entity.productId = rel.keyWord;
                entity.unit = rel.unitName;
                entity.unitId = rel.unitId;
                entity.zhyz = rel.zhyz;
                entity.Modify();
                return _relProductUnitRepo.Update(entity);
            }
        }


        #region private function

        /// <summary>
        /// 图片效验
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool ValidateImg(HttpPostedFileBase file)
        {
            if (!ValidateExt(file))
            {
                return false;
            }
            if (!ValidateSize(file))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 类型验证
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool ValidateExt(HttpPostedFileBase file)
        {
            var ext = file.FileName.GetFileNameExt();
            if (string.IsNullOrWhiteSpace(ext)) return false;
            switch (ext)
            {
                case ".jpg":
                case ".bmp":
                case ".png":
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 大小验证
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool ValidateSize(HttpPostedFileBase file)
        {
            var maxSize = Core.Common.Utils.ConfigurationHelper.GetAppConfigValue("imageMaxSize");
            maxSize = string.IsNullOrWhiteSpace(maxSize) ? 5.ToString() : maxSize;
            var size = file.ContentLength;
            return size / 1024 / 1024 <= Convert.ToInt32(maxSize);
        }

        #endregion
    }
}
