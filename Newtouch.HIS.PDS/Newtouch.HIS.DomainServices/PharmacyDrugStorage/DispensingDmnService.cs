using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using FrameworkBase.MultiOrg.Domain.DBContext.Infrastructure;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.HIS.Domain.DO;
using Newtouch.HIS.Domain.DTO.Medicine;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.HospitalizationPharmacy;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Repository;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.TSQL;
using Newtouch.PDS.Requset;
using Newtouch.PDS.Requset.Stock;
using Newtouch.PDS.Requset.Zyypyz;
using static Newtouch.Common.Web.APIRequestHelper;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 发药
    /// </summary>
    public class DispensingDmnService : DmnServiceBase, IDispensingDmnService
    {
        private readonly IOutpatientPrescriptionDetailBatchNumberRepo _mzphRepo;
        private readonly ISysMedicineStockInfoRepo _kcxx;
        private readonly IMzCfRepo _mzCfRepo;
        private readonly IMzCfmxRepo _mzCfmxRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        public DispensingDmnService(IDefaultDatabaseFactory databaseFactory, bool needIoc = true) : base(databaseFactory, needIoc)
        {
        }

        #region 门诊

        /// <summary>
        /// 门诊处方预定（冻结库存）
        /// </summary>
        /// <param name="bookItemDo"></param>
        /// <returns></returns>
        public string OutPatientBook(BookItemDo bookItemDo)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@ypdm", bookItemDo.YpCode),
                new SqlParameter("@zxdwsl", bookItemDo.Sl),
                new SqlParameter("@yfbmCode", bookItemDo.Yfbm),
                new SqlParameter("@cfh", bookItemDo.Cfh),
                new SqlParameter("@cfmxId", bookItemDo.CfmxId),
                new SqlParameter("@czh", bookItemDo.czh??""),
                new SqlParameter("@OrganizeId", bookItemDo.OrganizeId),
                new SqlParameter("@CreatorCode", bookItemDo.CreatorCode)
            };
            return FirstOrDefaultNoLog<string>(TSqlDispensing.mz_yp_book, param);
        }

        /// <summary>
        /// 门诊处方预定（冻结库存） V2
        /// </summary>
        /// <param name="bookItemDo"></param>
        /// <returns></returns>
        public string OutPatientBookV2(BookItemDo bookItemDo)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh", bookItemDo.Cfh),
                new SqlParameter("@OrganizeId", bookItemDo.OrganizeId),
                new SqlParameter("@yfbmCode", bookItemDo.Yfbm),
                new SqlParameter("@CreatorCode", bookItemDo.CreatorCode)
            };
            return FirstOrDefault<string>(TSqlDispensing.mz_yp_book_v2, param);
        }

        /// <summary>
        /// 取消预定--支持多批次
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="sl">最小单位数量</param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string OutPatientBookCancelMulti(string cfmxphid, string ypCode, int sl, string yfbmCode, string cfh, string organizeId, string creatorCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@ypdm", ypCode),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@sl", sl),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@CreatorCode", creatorCode),
                new SqlParameter("@oldcfmxphid",cfmxphid)
            };
            return FirstOrDefaultNoLog<string>(TSqlDispensing.mz_yp_book_cancel_v2, param);
        }


        /// <summary>
        /// 取消预定
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="sl">最小单位数量</param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string OutPatientBookCancel(string ypCode, int sl, string yfbmCode, string cfh, string organizeId, string creatorCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@ypdm", ypCode),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@sl", sl),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@CreatorCode", creatorCode)
            };
            return FirstOrDefaultNoLog<string>(TSqlDispensing.mz_yp_book_cancel, param);
        }

        /// <summary>
        /// 取消预定
        /// </summary>
        /// <param name="mxphlist"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string OutPatientBookCancel(List<OutpatientPrescriptionDetailBatchNumberEntity> mxphlist, string organizeId, string creatorCode)
        {
            if (mxphlist == null || mxphlist.Count == 0)
            {
                return "请传入要取消的排班明细";
            }

            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mxphlist)
                {
                    var tempResult = OutPatientBookCancelMulti(p.cfmxphId, p.yp, (int)p.sl, p.fyyf, p.cfh, organizeId, creatorCode);
                    if (!string.IsNullOrWhiteSpace(tempResult)) return tempResult;
                    p.zt = "0";
                    p.gjzt = "1";
                    p.LastModifyTime = DateTime.Now;
                    p.LastModifierCode = creatorCode;
                    _mzphRepo.Update(p);
                };
                db.Commit();

                return "";
            }
        }

        /// <summary>
        /// HIS门诊取消结算冻结返还
        /// </summary>
        /// <param name="mxphlist"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string OutPatientYpdjReturn(List<OutpatientPrescriptionDetailBatchNumberEntity> mxphlist, string organizeId, string creatorCode)
        {
            if (mxphlist == null || mxphlist.Count == 0)
            {
                return "请传入要取消的排班明细";
            }
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var p in mxphlist)
                {
                    var tempResult = OutPatientCancelSettMulti(p.cfmxphId, p.yp, (int)p.sl, p.fyyf, p.cfh, organizeId, creatorCode);
                    if (!string.IsNullOrWhiteSpace(tempResult)) return tempResult;
                    p.zt = "0";
                    p.gjzt = "1";
                    p.LastModifyTime = DateTime.Now;
                    p.LastModifierCode = creatorCode;
                    _mzphRepo.Update(p);
                };
                db.Commit();

                return "";
            }
        }
        /// <summary>
        /// HIS门诊取消结算冻结返还
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="sl">最小单位数量</param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string OutPatientCancelSettMulti(string cfmxphid, string ypCode, int sl, string yfbmCode, string cfh, string organizeId, string creatorCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@ypdm", ypCode),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@sl", sl),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@CreatorCode", creatorCode),
                new SqlParameter("@oldcfmxphid",cfmxphid)
            };
            return FirstOrDefaultNoLog<string>(TSqlDispensing.mz_yp_cancelSett_kcjd, param);
        }

        /// <summary>
        /// outpatient commit
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string OutpatientCommit(string ypCode, string yfbmCode, string cfh, string organizeId, string creatorCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                var mzCf = _mzCfRepo.FindEntity(p =>
                    p.cfh == cfh && p.OrganizeId == organizeId && p.lyyf == yfbmCode && p.zt == "1");
                if (mzCf == null) throw new FailedException("获取处方信息失败");
                if (mzCf.fybz != ((int)EnumFybz.Yp).ToString()) throw new FailedException("只能操作发药状态为已排的处方");

                var mzphs = _mzphRepo.IQueryable(p =>
                    p.yp == ypCode && p.fyyf == yfbmCode && p.cfh == cfh && p.OrganizeId == organizeId &&
                    p.gjzt == "0" && p.zt == "1");
                if (mzphs == null || !mzphs.Any()) throw new FailedException("未找到指定已预订的药品");
                mzphs.ToList().ForEach(mzph =>
                {
                    var kcxx = _kcxx.IQueryable(p =>
                        p.ypdm == mzph.yp && p.OrganizeId == organizeId && p.zt == ((int)EnumKCZT.Enabled).ToString() &&
                        p.pc == mzph.pc && p.ph == mzph.ph && p.yfbmCode == yfbmCode && p.yxq == mzph.yxq);
                    if (!kcxx.Any()) throw new FailedException("在库存中为找到指定批号批次药品");
                    if (kcxx.Select(p => p.djsl).Sum() < mzph.sl) throw new FailedException("已冻结数量不足");
                    mzCf.fybz = ((int)EnumFybz.Yf).ToString();
                    mzCf.LastModiFierCode = creatorCode;
                    mzCf.LastModifyTime = DateTime.Now;
                    _mzCfRepo.Update(mzCf);

                    var sysl = (int)mzph.sl;
                    kcxx.ToList().ForEach(p =>
                    {
                        if (sysl <= 0) return;
                        if (p.djsl >= sysl)
                        {
                            p.djsl -= sysl;
                            p.kcsl -= sysl;
                            sysl = 0;
                        }
                        else
                        {
                            sysl -= p.djsl;
                            p.djsl = 0;
                            p.kcsl -= p.djsl;
                        }
                        _kcxx.Update(p);
                    });
                });
                db.Commit();
            }
            return "";
        }

        /// <summary>
        /// 门诊发药扣库存
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="sl">最小单位数量</param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <param name="cfmxId">mz_cfmx.id</param>
        /// <returns></returns>
        public string OutpatientDispensingDrug(string ypCode, long cfmxId, int sl, string yfbmCode, string cfh, string organizeId, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                var reduceStockResult = OutpatientDispensingReduceStock(ypCode, yfbmCode, cfh, organizeId, userCode);
                if (!string.IsNullOrWhiteSpace(reduceStockResult)) return reduceStockResult;
                db.Commit();
                return "";
            }
        }

        /// <summary>
        /// 门诊发药扣库存
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string OutpatientDispensingReduceStock(string ypCode, string yfbmCode, string cfh, string organizeId, string userCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@ypCode", ypCode),
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@userCode", userCode)
            };
            return FirstOrDefaultNoLog<string>(TSqlOutpatient.mz_reduce_stock, param.ToArray());
        }

        /// <summary>
        /// 门诊退药还库存
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="zxdwsl"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <param name="ph"></param>
        /// <param name="pc"></param>
        /// <param name="returnDrugBillNo">退药单号</param>
        /// <returns></returns>
        public string OutpatientReturnDrugAddStock(string ypCode, string ph,
            string pc, int zxdwsl,
            string yfbmCode, string cfh,
            string organizeId, string userCode, string returnDrugBillNo)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@ypCode",ypCode ),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@ph", ph),
                new SqlParameter("@pc", pc),
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@tysl", zxdwsl),//最小单位数量
                new SqlParameter("@userCode", userCode),
                new SqlParameter("@returnDrugBillNo", returnDrugBillNo)
            };
            return FirstOrDefaultNoLog<string>(TSqlOutpatient.mz_rp_reture_drug, param);
        }

        #endregion

        #region 住院

        /// <summary>
        /// 住院Book冻结库存 并保存zy_ypyzzxph
        /// </summary>
        /// <param name="bookItemDo"></param>
        /// <returns></returns>
        public string HospitalizationBook(BookItemDo bookItemDo)
        {
            try
            {
                using (var db = new Infrastructure.EF.EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    if (bookItemDo.Sl <= 0) return string.Format("药品【{0}】数量不能小于零", bookItemDo.YpCode);
                    var kcxxRepo = new SysMedicineStockInfoRepo(new DefaultDatabaseFactory());
                    var kcxx = kcxxRepo.SelectData(bookItemDo.YpCode, bookItemDo.Yfbm, bookItemDo.OrganizeId);
                    if (kcxx == null || kcxx.Count == 0) return string.Format("药品【{0}】未找到可用库存", bookItemDo.YpCode);
                    var yxkcxx = kcxx.FindAll(p => p.kcsl - p.djsl > 0);
                    if (yxkcxx.Count == 0) return string.Format("药品【{0}】库存不足", bookItemDo.YpCode);
                    if (yxkcxx.Sum(p => p.kcsl - p.djsl) < bookItemDo.Sl) return string.Format("药品【{0}】库存不足", bookItemDo.YpCode);
                    var sykc = bookItemDo.Sl;
                    foreach (var itemKc in yxkcxx.OrderBy(p => p.yxq))
                    {
                        if (sykc <= 0) break;
                        var kykc = itemKc.kcsl - itemKc.djsl;
                        var tagDjsl = 0;
                        var zyphEntity = new ZyYpyzzxphEntity
                        {
                            OrganizeId = bookItemDo.OrganizeId,
                            zxId = bookItemDo.zxId,
                            ph = itemKc.ph,
                            pc = itemKc.pc,
                            ypCode = itemKc.ypdm,
                            yxq = itemKc.yxq,
                            fyyf = itemKc.yfbmCode,
                            zt = "1",
                            gjzt = "0",
                            CreateTime = DateTime.Now,
                            CreatorCode = bookItemDo.CreatorCode
                        };
                        if (kykc >= sykc)
                        {
                            //当前批次库存 足够
                            tagDjsl = sykc;
                            zyphEntity.sl = sykc;
                            sykc = 0;
                        }
                        else
                        {
                            //当前批次库存 不足
                            tagDjsl = kykc;
                            itemKc.djsl = itemKc.djsl + kykc;
                            zyphEntity.sl = kykc;
                            sykc -= kykc;
                        }

                        db.Insert(zyphEntity);

                        #region 修改库存冻结数量

                        const string sqlKcxx = @"
UPDATE dbo.xt_yp_kcxx SET djsl=djsl+@djsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
WHERE ypdm=@ypCode AND yfbmCode=@yfbmCode AND ph=@ph AND pc=@pc AND zt='1' AND tybz='0' ";
                        var paramKcxx = new DbParameter[]
                        {
                        new SqlParameter("@djsl", tagDjsl),
                        new SqlParameter("@userCode", bookItemDo.CreatorCode),
                        new SqlParameter("@ypCode", itemKc.ypdm),
                        new SqlParameter("@yfbmCode",itemKc.yfbmCode ),
                        new SqlParameter("@ph", itemKc.ph),
                        new SqlParameter("@pc", itemKc.pc),
                        };
                        db.ExecuteSqlCommandNoLog(sqlKcxx, paramKcxx);

                        #endregion 修改库存冻结数量
                    }
                    db.Commit();
                    return "";
                }
            }
            catch (Exception e)
            {
                return e.Message + (e.InnerException == null ? "" : e.InnerException.Message);
            }
        }

        public string getYpDl(string ypcode, string organizeId)
        {
            string sql = @"select p.dlCode from NewtouchHIS_Base.dbo.xt_yp p where p.ypCode=@ypCode and p.organizeId=@organizeId";
            var param = new DbParameter[]
            {
                new SqlParameter("@ypCode", ypcode),
                new SqlParameter("@organizeId", organizeId)
            };
            return FirstOrDefaultNoLog<string>(sql, param);
        }

        /// <summary>
        /// 住院Book冻结库存 并保存zy_ypyzzxph
        /// </summary>
        /// <param name="yzId"></param>
        /// <param name="zxId">执行ID</param>
        /// <param name="organizeId">组织机构ID</param>
        /// <param name="yfbmCode">药房部门编码</param>
        /// <param name="creatorCode">创建人</param>
        /// <param name="patientName">医嘱所属患者</param>
        /// <returns></returns>
        public string HospitalizationBook(string yzId, string zxId, string organizeId, string yfbmCode, string creatorCode, string patientName)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@yzId", yzId),
                new SqlParameter("@zxId", zxId),
                new SqlParameter("@OrganizeId",organizeId ),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@CreatorCode", creatorCode),
                new SqlParameter("@patientName", patientName)
            };
            return FirstOrDefault<string>(TSqlDispensing.zy_yp_book, param);
        }

        /// <summary>
        /// 取消冻结，并物理删除医嘱信息 慎用
        /// </summary>
        /// <param name="yzId"></param>
        /// <param name="zxId"></param>
        /// <returns></returns>
        public string CancelForzenAndDeleteYz(string yzId, string zxId)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@yzId", yzId),
                new SqlParameter("@zxId", zxId)
            };
            return FirstOrDefault<string>(TSqlDispensing.zy_yp_cancelForzen_deleteData, param);
        }

        /// <summary>
        /// 住院Book冻结库存 并保存zy_ypyzzxph
        /// </summary>
        /// <param name="zxId"></param>
        /// <param name="yzDetails"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public string HospitalizationBook(string zxId, List<YzDetail> yzDetails, string organizeId)
        {
            try
            {
                if (yzDetails == null || yzDetails.Count == 0) return "医嘱明细不能为空";
                using (var db = new Infrastructure.EF.EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    foreach (var yzxx in yzDetails)
                    {
                        var bookItemDo = new BookItemDo
                        {
                            YpCode = yzxx.ypCode,
                            Sl = (int)yzxx.sl * yzxx.zhyz,
                            Yfbm = string.IsNullOrWhiteSpace(yzxx.fyyf) ? Constants.CurrentYfbm.yfbmCode : yzxx.fyyf,
                            OrganizeId = organizeId ?? "",
                            CreatorCode = yzxx.yzzxsqr ?? "",
                            zxId = zxId,
                            zh = yzxx.zh
                        };

                        if (bookItemDo.Sl <= 0) throw new Exception(string.Format("药品【{0}】数量不能小于零", yzxx.ypmc));
                        var kcxxRepo = new SysMedicineStockInfoRepo(new DefaultDatabaseFactory());
                        var kcxx = kcxxRepo.SelectData(bookItemDo.YpCode, bookItemDo.Yfbm, bookItemDo.OrganizeId);
                        if (kcxx == null || kcxx.Count == 0) throw new Exception(string.Format("药品【{0}】未找到可用库存", yzxx.ypmc));
                        var yxkcxx = kcxx.FindAll(p => p.kcsl - p.djsl > 0);
                        if (yxkcxx.Count == 0) throw new Exception(string.Format("药品【{0}】库存不足", yzxx.ypmc));
                        if (yxkcxx.Sum(p => p.kcsl - p.djsl) < bookItemDo.Sl) throw new Exception(string.Format("药品【{0}】库存不足", yzxx.ypmc));
                        var sykc = bookItemDo.Sl;
                        foreach (var itemKc in yxkcxx.OrderBy(p => p.yxq))
                        {
                            if (sykc <= 0) break;
                            var kykc = itemKc.kcsl - itemKc.djsl;
                            int tagDjsl;
                            var zyphEntity = new ZyYpyzzxphEntity
                            {
                                yzId = yzxx.yzId,
                                OrganizeId = bookItemDo.OrganizeId,
                                zxId = bookItemDo.zxId,
                                ph = itemKc.ph,
                                pc = itemKc.pc,
                                ypCode = itemKc.ypdm,
                                yxq = itemKc.yxq,
                                fyyf = itemKc.yfbmCode,
                                zt = "1",
                                gjzt = "0",
                                zh = yzxx.zh,
                                CreateTime = DateTime.Now,
                                CreatorCode = bookItemDo.CreatorCode
                            };
                            if (kykc >= sykc)
                            {
                                //当前批次库存 足够
                                tagDjsl = sykc;
                                zyphEntity.sl = sykc;
                                sykc = 0;
                            }
                            else
                            {
                                //当前批次库存 不足
                                tagDjsl = kykc;
                                itemKc.djsl = itemKc.djsl + kykc;
                                zyphEntity.sl = kykc;
                                sykc -= kykc;
                            }

                            db.Insert(zyphEntity);

                            #region 修改库存冻结数量

                            const string sqlKcxx = @"
UPDATE dbo.xt_yp_kcxx SET djsl=djsl+@djsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
WHERE ypdm=@ypCode AND yfbmCode=@yfbmCode AND ph=@ph AND pc=@pc AND zt='1' AND tybz='0' ";
                            var paramKcxx = new DbParameter[]
                            {
                                new SqlParameter("@djsl", tagDjsl),
                                new SqlParameter("@userCode", bookItemDo.CreatorCode),
                                new SqlParameter("@ypCode", itemKc.ypdm),
                                new SqlParameter("@yfbmCode",itemKc.yfbmCode ),
                                new SqlParameter("@ph", itemKc.ph),
                                new SqlParameter("@pc", itemKc.pc),
                            };
                            db.ExecuteSqlCommandNoLog(sqlKcxx, paramKcxx);

                            #endregion 修改库存冻结数量
                        }
                    }
                    db.Commit();
                    return "";
                }
            }
            catch (Exception e)
            {
                return e.Message + (e.InnerException == null ? "" : e.InnerException.Message);
            }
        }

        /// <summary>
        /// 取消住院book
        /// </summary>
        /// <param name="zxId"></param>
        /// <param name="bookItemDo"></param>
        /// <returns></returns>
        public string HospitalizationCancelBook(string zxId, BookItemDo bookItemDo)
        {
            if (bookItemDo.Sl <= 0) return "";
            var kcxxRepo = new SysMedicineStockInfoRepo(new DefaultDatabaseFactory());
            var kcxx = kcxxRepo.SelectData(bookItemDo.YpCode, bookItemDo.Yfbm, bookItemDo.OrganizeId);
            if (kcxx == null || kcxx.Count == 0) return "";
            var yxkcxx = kcxx.FindAll(p => p.djsl > 0);
            if (yxkcxx.Count == 0) return "";
            if (yxkcxx.Sum(p => p.djsl) < bookItemDo.Sl) return string.Format("药品【{0}】库存冻结数量不足", bookItemDo.YpCode);
            var zyphRepo = new ZyYpyzzxphRepo(new DefaultDatabaseFactory());
            var zyphList = zyphRepo.SelectZyphList(zxId, bookItemDo.zxId, bookItemDo.YpCode, bookItemDo.OrganizeId);
            if (zyphList == null || zyphList.Count == 0) return "";
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var zyph in zyphList)
                {
                    var sykc = zyph.sl;
                    var pckcxxList = yxkcxx.FindAll(p => p.ph == zyph.ph && p.pc == zyph.pc);
                    if (pckcxxList.Count == 0) continue;
                    foreach (var pckcxx in pckcxxList)
                    {
                        int subtactSl;
                        if (pckcxx.djsl >= sykc)
                        {
                            subtactSl = (int)sykc;
                            sykc = 0;
                        }
                        else
                        {
                            subtactSl = pckcxx.djsl;
                            sykc -= pckcxx.djsl;
                        }
                        kcxxRepo.SubtractForzenKc(subtactSl, pckcxx.pc, pckcxx.ph, pckcxx.ypdm, pckcxx.yfbmCode,
                            pckcxx.OrganizeId, bookItemDo.CreatorCode);
                    }

                    zyph.gjzt = "1";
                    zyph.zt = "0";
                    zyphRepo.Update(zyph);
                }

                db.Commit();
                return "";
            }
        }

        /// <summary>
        /// 取消排药
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string HospitalizationCancelArrangement(CancelArrangement param)
        {
            var kcxxRepo = new SysMedicineStockInfoRepo(new DefaultDatabaseFactory());
            var zyphRepo = new ZyYpyzzxphRepo(new DefaultDatabaseFactory());
            var kcxx = kcxxRepo.SelectData(param.ypCode, param.ph, param.pc, param.yfbmCode, param.OrganizeId);
            if (kcxx == null || kcxx.Count == 0) return UpdateArrangementZtAndGjzt(zyphRepo, param);

            var yxkcxx = kcxx.FindAll(p => p.djsl > 0);
            if (yxkcxx.Count == 0) return UpdateArrangementZtAndGjzt(zyphRepo, param);

            var zyphList = zyphRepo.SelectZyphList(param.yzId, param.zxId, param.ypCode, param.pc, param.ph, param.OrganizeId);
            if (zyphList == null || zyphList.Count == 0) return "";

            if (yxkcxx.Sum(p => p.djsl) < zyphList.Sum(p => p.sl)) return string.Format("药品【{0}】库存冻结数量不足", param.ypCode);

            foreach (var zyph in zyphList)
            {
                var sykc = zyph.sl;
                foreach (var pckcxx in yxkcxx)
                {
                    int subtactSl;
                    if (pckcxx.djsl >= sykc)
                    {
                        subtactSl = (int)sykc;
                        sykc = 0;
                    }
                    else
                    {
                        subtactSl = pckcxx.djsl;
                        sykc -= pckcxx.djsl;
                    }
                    kcxxRepo.SubtractForzenKc(subtactSl, pckcxx.pc, pckcxx.ph, pckcxx.ypdm, pckcxx.yfbmCode,
                        pckcxx.OrganizeId, param.userCode);
                }

                zyph.gjzt = "1";
                zyph.zt = "0";
                zyphRepo.Update(zyph);
            }

            return "";
        }


        /// <summary>
        /// 排药
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string HospitalizationCancelArrangementWithTrans(CancelArrangement param)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                var tmpResult = HospitalizationCancelArrangement(param);
                if (string.IsNullOrWhiteSpace(tmpResult)) db.Commit();
                return tmpResult;
            }
        }

        /// <summary>
        /// 修改排药状态和归架标志
        /// </summary>
        /// <param name="zyphRepo"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private string UpdateArrangementZtAndGjzt(ZyYpyzzxphRepo zyphRepo, CancelArrangement param)
        {
            //直接取消排药
            var zyph = zyphRepo.FindEntity(p =>
                p.zxId == param.zxId && p.yzId == param.yzId && p.OrganizeId == param.OrganizeId && p.ph == param.ph && p.pc == param.pc &&
                p.zt == "1" && p.gjzt == "0" && p.fyyf == param.yfbmCode);
            if (zyph == null) return "";
            zyph.zt = "0";
            zyph.gjzt = "1";
            return zyphRepo.Update(zyph) > 0 ? "" : "修改排药归架标志位失败";
        }

        /// <summary>
        /// 住院发药 支持多条医嘱多个排药明细，但医嘱和排药明细必须对应
        /// </summary>
        /// <param name="zyphLs"></param>
        /// <param name="yzLs"></param>
        /// <param name="userCode"></param>
        /// <param name="fyid"></param>
        /// <returns></returns>
        public string HospitalizationDispensing(List<ZyYpyzzxphEntity> zyphLs, List<ZyYpyzxxEntity> yzLs, string userCode,out string fyid,string organizeId)
        {
            fyid = Guid.NewGuid().ToString();//发药ID
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var zyph in zyphLs)
                {
                    if (zyph.sl <= 0) continue;

                    var drug = new SysMedicineExDmnService(new DefaultDatabaseFactory(), false).GetYpDetails(zyph.OrganizeId, zyph.ypCode);
                    var ypmc = drug != null && !string.IsNullOrWhiteSpace(drug.ypmc) ? drug.ypmc : zyph.ypCode;
                    #region 获取库存信息
                    const string sql1 = @"
SELECT * FROM xt_yp_kcxx
WHERE ypdm = @ypCode AND yfbmCode = @yfbmCode AND tybz = '0' AND kcsl>0 AND djsl>=0 
AND pc=@pc AND ph=@ph AND zt='1' AND OrganizeId = @OrganizeId 
";
                    var param1 = new DbParameter[]
                    {
                        new SqlParameter("@ypCode", zyph.ypCode),
                        new SqlParameter("@yfbmCode", zyph.fyyf),
                        new SqlParameter("@OrganizeId", zyph.OrganizeId),
                        new SqlParameter("@pc", zyph.pc),
                        new SqlParameter("@ph",zyph.ph)
                    };
                    var kcxxLs = db.FindList<SysMedicineStockInfoEntity>(sql1, param1);
                    if (kcxxLs == null || kcxxLs.Count == 0) return string.Format("批次【{1}】的药品【{0}】未找到有效库存", ypmc, zyph.pc);
                    if (kcxxLs.Sum(p => p.kcsl) < zyph.sl) return string.Format("批次【{1}】的药品【{0}】库存数量不足，无法发药", ypmc, zyph.pc);
                    #endregion
                    #region 扣库存

                    var sysl = (int)zyph.sl;
                    if (kcxxLs.Sum(p => p.djsl) < zyph.sl)
                    {
                        //冻结数不足
                        foreach (var kcxx in kcxxLs.TakeWhile(kcxx => sysl > 0))
                        {
                            int subtractKc;
                            if (kcxx.kcsl >= sysl)
                            {
                                subtractKc = sysl;
                                sysl = 0;
                            }
                            else
                            {
                                subtractKc = kcxx.kcsl;
                                sysl -= kcxx.kcsl;
                            }
                            const string sql2 = @"
UPDATE dbo.xt_yp_kcxx SET djsl=0, kcsl=kcsl-@sl, LastModifyTime=GETDATE(), LastModifierCode=@userCode
WHERE pc=@pc AND ph=@ph AND ypdm=@ypCode AND OrganizeId=@OrganizeId AND yfbmCode=@yfbmCode AND zt='1' AND tybz='0'
";
                            var param2 = new DbParameter[]
                            {
                                new SqlParameter("@sl", subtractKc ),
                                new SqlParameter("@userCode", userCode),
                                new SqlParameter("@pc", kcxx.pc),
                                new SqlParameter("@ph",kcxx.ph ),
                                new SqlParameter("@ypCode",kcxx.ypdm ),
                                new SqlParameter("@OrganizeId", kcxx.OrganizeId),
                                new SqlParameter("@yfbmCode", kcxx.yfbmCode)
                            };
                            db.ExecuteSqlCommand(sql2, param2);
                        }
                    }
                    else
                    {
                        //冻结数足够
                        foreach (var kcxx in kcxxLs.TakeWhile(kcxx => sysl > 0))
                        {
                            int subtractKc;
                            if (kcxx.kcsl >= sysl)
                            {
                                subtractKc = sysl;
                                sysl = 0;
                            }
                            else
                            {
                                subtractKc = kcxx.kcsl;
                                sysl -= kcxx.kcsl;
                            }

                            const string sql2 = @"
UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@sl, kcsl=kcsl-@sl, LastModifyTime=GETDATE(), LastModifierCode=@userCode
WHERE pc=@pc AND ph=@ph AND ypdm=@ypCode AND OrganizeId=@OrganizeId AND yfbmCode=@yfbmCode AND zt='1' AND tybz='0'
";
                            var param2 = new DbParameter[]
                            {
                                new SqlParameter("@sl", subtractKc ),
                                new SqlParameter("@userCode", userCode),
                                new SqlParameter("@pc", kcxx.pc),
                                new SqlParameter("@ph",kcxx.ph ),
                                new SqlParameter("@ypCode",kcxx.ypdm ),
                                new SqlParameter("@OrganizeId", kcxx.OrganizeId),
                                new SqlParameter("@yfbmCode", kcxx.yfbmCode)
                            };
                            db.ExecuteSqlCommand(sql2, param2);
                        }
                    }
                    #endregion
                }
                int isdjnr = 0;
                foreach (var yzxx in yzLs)
                {
                    #region 修改发药状态 已排 => 已发

                    yzxx.fybz = "2";
                    yzxx.LastModifierCode = userCode;
                    yzxx.LastModifyTime = DateTime.Now;
                    db.Update(yzxx);
                    if(yzxx.yzlx == ((int)Yzlx.zcy))
                    {
                        if (isdjnr == 0)
                        {
                            var data = GetBindTCMDj(yzxx.OrganizeId);
                            if (data != null)
                            {
                                string sql2 = @"
 select * from [Newtouch_CIS].dbo.zy_lsyz where Id=@yzId  and OrganizeId=@OrganizeId and zt='1' and djbz='1' 
";
                                var param2 = new DbParameter[]
                                {
                        new SqlParameter("@yzId", yzxx.yzId),
                        new SqlParameter("@OrganizeId", yzxx.OrganizeId)
                                };
                                zylsyzDTO lsyz = db.FirstOrDefault<zylsyzDTO>(sql2, param2);
                                if (lsyz != null && lsyz.yzh != "")
                                {
                                    var xmObj = new
                                    {
                                        OrganizeId = yzxx.OrganizeId,
                                        zyh = lsyz.zyh,
                                        sfxm = data.sfxmCode,
                                        dl = data.sfdlCode,
                                        ys = lsyz.ysgh,
                                        ks = lsyz.zxksdm,
                                        ysmc = "",
                                        ksmc = "",
                                        dj = data.dj,
                                        sl = lsyz.sl,
                                        dw = data.dw,
                                        zxks = lsyz.zxksdm,
                                        zfxz = data.zfxz,
                                        zfbl = data.zfbl,
                                        yzh=lsyz.yzh
                                    };
                                    var apiResp = SiteSettAPIHelper.Request<object, DefaultResponse>("/api/Prescription/DjxmInsert", xmObj);
                                    isdjnr++;
                                }
                            }
                        }
                    }
                    #endregion
                    #region 新增操作记录

                    var zyczjlEntity = new ZyYpyzczjlEntity
                    {
                        OrganizeId = yzxx.OrganizeId,
                        ypyzxxId = yzxx.Id,
                        operateType = "1",
                        ypCode = yzxx.ypCode,
                        sl = yzxx.sl * yzxx.zhyz,
                        zxId = yzxx.zxId,
                        yzId = yzxx.yzId,
                        CreatorCode = userCode,
                        CreateTime = DateTime.Now,
                        bz = "",
                        LastModifierCode = "",
                        LastModifyTime = null,
                        zyfyapplyno= fyid,
                        zsm = yzxx.zsm,

                    };
                    db.Insert(zyczjlEntity);
                    #endregion
                }
                db.Commit();
                var zyhArr = yzLs.Select(p => p.zyh).Distinct().ToArray();
                string zyhs = "";
                zyhs = string.Join(",", zyhArr);
                SyncPatFee(organizeId, zyhs, 1);
                Updatezy_brxxexpand(organizeId, zyhs);
                return "";
            }
        }
        #endregion
        #region 病人费用实时计算
        public void Updatezy_brxxexpand(string OrganizeId, string zyh)
        {
            try
            {
                string sql = @" exec Newtouch_CIS..usp_zy_brxxexpand_update @orgId,@zyh";
                SqlParameter[] para ={
                new SqlParameter("@orgId",OrganizeId),
                 new SqlParameter("@zyh",zyh)
                };
                int i = this.ExecuteSqlCommand(sql, para);
            }
            catch (Exception)
            {
            }

        }
        #endregion
        #region 生成项目费用
        /// <summary>
        /// 同步最新CPOE项目费用
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="zyh"></param>
        public void SyncPatFee(string orgId, string zyh, int zxtype)
        {
            //是否启用费用同步最新机制
            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                string sql = "";
                if (zxtype == 0) //同步项目费用
                {
                    sql = @" exec [NewtouchHIS_Sett]..[skd_syncxmjfbcope] @orgId=@orgId,@lqrq=@lqrq,@zyh=@zyh ";
                }
                else //同步药品费用
                {
                    sql = @" exec [NewtouchHIS_Sett]..[skd_syncypjfbcope] @orgId=@orgId,@lqrq=@lqrq,@zyh=@zyh";
                }
                SqlParameter[] para = new SqlParameter[] {
                        new SqlParameter("@orgId",orgId),
                        new SqlParameter("@lqrq",DateTime.Now),
                        new SqlParameter("@zyh",zyh)
                    };
                db.ExecuteSqlCommand(sql, para);
                db.Commit();
            }
        }
        #endregion
        public TCMDjXMVO GetBindTCMDj(string OrganizeId)
        {
            var sfxmCode = _sysConfigRepo.GetValueByCode("TCM_DaiJianCode", OrganizeId);
            var sql = @"
SELECT *
  FROM[NewtouchHIS_Base]..[V_S_xt_sfxm]
WHERE zt = '1' and organizeId = @orgId and sfxmCode = @sfxmCode";
            return this.FirstOrDefault<TCMDjXMVO>(sql, new[] { new SqlParameter("@orgId", OrganizeId), new SqlParameter("@sfxmCode", sfxmCode) });

        }
        public string PrepareMedicine(BYDjInfoDTO yplist, string organizeId, string yhgh)
        {
            try
            {
                using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    var ksbyid = Guid.NewGuid().ToString();
                    var byEntity = new XtksbyEntity
                    {
                        Id = ksbyid,
                        OrganizeId = organizeId,
                        yfbm = yplist.yfbm,
                        bqbm = yplist.bqbm,
                        ksbm = yplist.ksbm,
                        djh = yplist.djh,
                        shzt = "1",
                        zt = "1",
                        CreatorCode = yhgh,
                        CreateTime = DateTime.Now,
                        LastModifierCode = "",
                        LastModifyTime = null,
                    };
                    db.Insert(byEntity);
                    foreach (var byxx in yplist.mx)
                    {
                        var bymxEntity = new XtksbymxEntity
                        {
                            Id = Guid.NewGuid().ToString(),
                            byId = ksbyid,
                            OrganizeId = organizeId,
                            ypdm = byxx.ypdm,
                            sl = byxx.sl.ToString(),
                            dw = byxx.ckdw,
                            zhyz = byxx.zhyz.ToString(),
                            pfj = byxx.pfj.ToString(),
                            lsj = byxx.lsj.ToString(),
                            zje = byxx.zje.ToString(),
                            zt = "1",
                            CreatorCode = yhgh,
                            CreateTime = DateTime.Now,
                            LastModifierCode = "",
                            LastModifyTime = null,
                            gg = byxx.gg,
                            yplb = byxx.yplb,
                            yxq = byxx.yxq,
                            sccj = byxx.sccj,
                            ypmc = byxx.ypmc,
                            pc = byxx.pc,
                            ph = byxx.ph
                        };
                        db.Insert(bymxEntity);
                    };
                    db.Commit();
                }

            }
            catch (Exception ex)
            {

                return ex.Message.ToString();
            }
            return "";
        }
        public string WithdrawalPreparation(string Djh, string organizeId, string yhgh,string shzt)
        {
            try
            {
                using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    var sql = @"
select [Id]
      ,[OrganizeId]
      ,[yfbm]
      ,[bqbm]
      ,[ksbm]
      ,[djh]
      ,[shzt]
      ,[zt]
      ,[CreatorCode]
      ,convert(date,[CreateTime]) CreateTime
      ,[LastModiFierCode]
      ,[LastModifyTime] from [NewtouchHIS_PDS].dbo.xt_bqksby where
 organizeId=@orgId  and zt='1' and djh=@Djh";
                  var ksbyxx=this.FirstOrDefault<XtksbyEntity>(sql, new[] { new SqlParameter("@orgId", organizeId), new SqlParameter("@Djh", Djh.Trim()) });
                    if (ksbyxx.shzt!="1")
                    {
                        return "撤回失败！单据必须是已提交状态才能撤回！";
                    }
                    string sqlByxx = @"
delete from  xt_bqksby where id=@Id and organizeId=@orgId";
                    var paramByxx = new DbParameter[]
                    {
                        new SqlParameter("@Id", ksbyxx.Id),
                        new SqlParameter("@orgId", organizeId),
                    };
                    db.ExecuteSqlCommand(sqlByxx, paramByxx);
                    string sqlBymxxx = @"
delete from  xt_bqksbymx where byid=@byid and organizeId=@orgId";
                    var paramBymxxx = new DbParameter[]
                    {
                        new SqlParameter("@byid", ksbyxx.Id),
                        new SqlParameter("@orgId", organizeId),
                    };
                    db.ExecuteSqlCommand(sqlBymxxx, paramBymxxx);
                    db.Commit();
                }
                
            }
            catch (Exception ex)
            {

                return ex.Message.ToString();
            }
            return "";
        }
        public string PrepareMedicineReturn(BythDjInfoDTO yplist, string organizeId, string yhgh)
        {
            try
            {
                using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    var ksbyid = Guid.NewGuid().ToString();
                    var byEntity = new XtksbythEntity
                    {
                        Id = ksbyid,
                        OrganizeId = organizeId,
                        djh = yplist.djh,
                        thzt = "2",
                        yfbm = yplist.yfbm,
                        ksbm = yplist.ksbm,
                        bqbm = yplist.rkbm,
                        tjsj = DateTime.Now.Date,
                        thyy = yplist.thyy,
                        zt = "1",
                        CreatorCode = yhgh,
                        CreateTime = DateTime.Now,
                        LastModifierCode = "",
                        LastModifyTime = null,
                    };
                    db.Insert(byEntity);
                    foreach (var byxx in yplist.mx)
                    {
                        var bymxEntity = new XtksbythmxEntity
                        {
                            Id = Guid.NewGuid().ToString(),
                            byId = ksbyid,
                            OrganizeId = organizeId,
                            ypdm = byxx.ypdm,
                            ypmc = byxx.ypmc,
                            yplb = byxx.yplb,
                            tsl = byxx.tsl,
                            dw = byxx.dw,
                            gg = byxx.gg,
                            yxq = byxx.yxq,
                            //yfbm=byxx.yfbm,
                            sccj = byxx.sccj,
                            //thyy=byxx.thyy,
                            ph = byxx.ph,
                            pc = byxx.pc,
                            zt = "1",
                            CreatorCode = yhgh,
                            CreateTime = DateTime.Now,
                            LastModifierCode = "",
                            LastModifyTime = null,
                        };
                        db.Insert(bymxEntity);
                    };
                    db.Commit();
                }

            }
            catch (Exception ex)
            {

                return ex.Message.ToString();
            }
            return "";
        }

        public string WithdrawalPreparationReturn(string Djh, string organizeId, string yhgh, string thzt)
        {
            try
            {
                using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    var sql = @" 
SELECT [Id]
      ,[OrganizeId]
      ,[djh]
      ,[thzt]
      ,[yfbm]
      ,[ksbm]
      ,[bqbm]
      ,[tjsj]
      ,[thyy]
      ,[zt]
      ,[CreatorCode]
      ,[CreateTime]
      ,[LastModiFierCode]
      ,[LastModifyTime]
  FROM [NewtouchHIS_PDS].[dbo].[xt_ksbyth] where
 organizeId=@orgId  and zt='1' and djh=@Djh";
                    var ksbyxx = this.FirstOrDefault<XtksbythEntity>(sql, new[] { new SqlParameter("@orgId", organizeId), new SqlParameter("@Djh", Djh.Trim()) });
                    if (ksbyxx.thzt != "2")
                    {
                        return "撤回失败！单据必须是已提交状态才能撤回！";
                    }
                    string sqlByxx = @"
delete from  xt_ksbyth where id=@Id and organizeId=@orgId";
                    var paramByxx = new DbParameter[]
                    {
                        new SqlParameter("@Id", ksbyxx.Id),
                        new SqlParameter("@orgId", organizeId),
                    };
                    db.ExecuteSqlCommand(sqlByxx, paramByxx);
                    string sqlBymxxx = @"
delete from  xt_ksbythmx where byid=@byid and organizeId=@orgId";
                    var paramBymxxx = new DbParameter[]
                    {
                        new SqlParameter("@byid", ksbyxx.Id),
                        new SqlParameter("@orgId", organizeId),
                    };
                    db.ExecuteSqlCommand(sqlBymxxx, paramBymxxx);
                    db.Commit();
                }

            }
            catch (Exception ex)
            {

                return ex.Message.ToString();
            }
            return "";
        }
    }
}