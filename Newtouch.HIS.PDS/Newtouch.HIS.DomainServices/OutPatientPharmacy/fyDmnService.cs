using FrameworkBase.MultiOrg.Domain.DBContext.Infrastructure;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.Medicine;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.V;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.ValueObjects.OutPatientPharmacy;
using Newtouch.HIS.Domain.ValueObjects.SystemManage;
using Newtouch.HIS.Domain.VO;
using Newtouch.HIS.Repository;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.TSQL;
using Newtouch.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 发药
    /// </summary>
    public class fyDmnService : DmnServiceBase, IfyDmnService
    {
        private readonly IMzCfRepo _mzCfRepo;
        private readonly IMzCfmxRepo _mxCfmxRepo;
        private readonly IMzCfypczjlRepo _mzCfypczjlRepo;
        private readonly IDispensingDmnService _dispensingDmnService;

        public fyDmnService(IDefaultDatabaseFactory databaseFactory, bool needIoc = true) : base(databaseFactory, needIoc)
        {
        }

        /// <summary>
        /// 获取发药明细
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        public IList<fydisplayDetailInfo> GetDetailInfo(List<fyDetailListRequest> reqdata)
        {
            var yfbmcode = Constants.CurrentYfbm.yfbmCode;
            var param = new DbParameter[]
            {
                new SqlParameter("@fyDetailListRequest", reqdata.ToDataTable()){
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.fyDetailListRequest"
                },
                new SqlParameter("@yfbmcode", yfbmcode),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            return FindList<fydisplayDetailInfo>("EXEC dbo.sp_fy_getcfdetial @YfbmCode, @OrganizeId, @fyDetailListRequest", param);
        }

        /// <summary>
        /// 获取发药明细
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        public IList<fydisplayDetailInfo> GetAllDetailInfo(List<fyDetailListRequest> reqdata)
        {
            var yfbmcode = Constants.CurrentYfbm.yfbmCode;
            var param = new DbParameter[]
            {
                new SqlParameter("@fyDetailListRequest", reqdata.ToDataTable()){
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.fyDetailListRequest"
                },
                new SqlParameter("@yfbmcode", yfbmcode),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            return FindList<fydisplayDetailInfo>("EXEC dbo.[sp_fy_getAllCFDetial] @YfbmCode, @OrganizeId, @fyDetailListRequest", param);
        }

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="detaildata"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        private List<string> UpdateStock(List<fyMeidicneInfo> detaildata, string yfbmCode)
        {
            var cfnmList = new List<string>();
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var item in detaildata)
                {
                    var strSql = new StringBuilder();
                    strSql.Append(@"   UPDATE    kc
                                  SET       kc.Kcsl = kc.Kcsl - @sl ,
                                            kc.Djsl = kc.Djsl - @sl
                                  FROM      XT_YP_KCXX kc
                                  WHERE     1 = 1
                                            AND kc.yfbmCode = @deptCode
                                            AND kc.Ypdm = @yp
                                            AND LTRIM(RTRIM(kc.pc)) = @pc;

                                    UPDATE  dbo.mz_cfmxph
                                    SET     LastModifyTime = GETDATE()
                                    WHERE   cfmxphId = @cfmxphId;");
                    var paraList = new DbParameter[]
                    {
                        new SqlParameter("@sl", item.sl),
                        new SqlParameter("@deptCode", yfbmCode),
                        new SqlParameter("@yp", item.ypCode),
                        new SqlParameter("@pc", item.pc.Trim()),
                        new SqlParameter("@cfmxphId", item.cfmxphId)
                    };
                    db.ExecuteSqlCommand(strSql.ToString(), paraList);
                    if (!cfnmList.Contains(item.cfnm))
                    {
                        cfnmList.Add(item.cfnm);
                    }
                }
                db.Commit();
            }
            return cfnmList;
        }

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public string UpdateStock(string cfh, string yfbmCode)
        {
            var param = new List<DbParameter>
            {
                new SqlParameter("@Yfbm", yfbmCode),
                new SqlParameter("@Cfh", cfh),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId),
                new SqlParameter("@CreatorCode", OperatorProvider.GetCurrent().UserCode)
            };
            return FindList<string>(TSqlOutpatient.sp_yp_commit_mz, param.ToArray()).FirstOrDefault();
        }

        /// <summary>
        /// 门诊发药V2.0
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public string ExecOutpatientDispensingDrugV2(string cfh, string yfbmCode, string userCode, string organizeId,string zsm)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh",cfh ),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@userCode",userCode ),
                new SqlParameter("@zsm",zsm??""),
            };
            return FirstOrDefault<string>(TSqlDispensing.mz_yp_delivery, param);
        }

        /// <summary>
        /// 执行门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public string ExecOutpatientDispensingDrug(string cfh, string yfbmCode, string userCode, string organizeId)
        {
            try
            {
                using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
                {

                    var cfmx = new MzCfmxRepo(new DefaultDatabaseFactory()).SelectData(cfh, organizeId);
                    if (cfmx == null || cfmx.Count == 0) return "未找到处方明细";
                    var mxphRepo = new OutpatientPrescriptionDetailBatchNumberRepo(new DefaultDatabaseFactory());
                    var kcxxRepo = new SysMedicineStockInfoRepo(new DefaultDatabaseFactory());

                    const string sql1 = @"UPDATE dbo.xt_yp_kcxx SET djsl=0, kcsl-=@sl, LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE kcId=@kcId AND zt='1'";
                    const string sql2 = @"UPDATE dbo.xt_yp_kcxx SET djsl-=@sl, kcsl-=@sl, LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE kcId=@kcId AND zt='1'";
                    var effectiveCfmx = new List<MzCfmxEntity>();
                    foreach (var ypCode in cfmx.Select(o => o.ypCode).Distinct().ToList())
                    {
                        var mxphs = mxphRepo.GetListByGroup(cfh, ypCode, yfbmCode, organizeId, gjzt: "0");
                        if (mxphs == null || mxphs.Count == 0) continue;

                        effectiveCfmx.AddRange(cfmx.FindAll(p => p.ypCode == ypCode));
                        foreach (var mxph in mxphs)
                        {
                            if (mxph.sl <= 0) continue;
                            var kcxxs = kcxxRepo.SelectData(ypCode, mxph.ph, mxph.pc, yfbmCode, organizeId);
                            if (kcxxs == null || kcxxs.Count == 0) return string.Format("药品代码为{0}、批次{1}、批号{2}的药品未找到库存信息", ypCode, mxph.pc, mxph.ph);
                            if (kcxxs.Sum(p => p.djsl) < mxph.sl) return string.Format("药品代码为{0}、批次{1}、批号{2}的药品已冻结库存不足", ypCode, mxph.pc, mxph.ph);
                            if (kcxxs.Sum(p => p.kcsl) < mxph.sl) return string.Format("药品代码为{0}、批次{1}、批号{2}的药品库存不足", ypCode, mxph.pc, mxph.ph);
                            var sysl = mxph.sl;
                            foreach (var kcxx in kcxxs)
                            {
                                if (sysl <= 0) break;
                                var param = new List<DbParameter>
                                {
                                    new SqlParameter("@userCode", userCode),
                                    new SqlParameter("@kcId", kcxx.kcId),
                                };
                                if (kcxx.djsl >= sysl)
                                {
                                    sysl = 0;
                                    param.Add(new SqlParameter("@sl", mxph.sl));
                                    db.ExecuteSqlCommand(sql2, param.ToArray());
                                }
                                else
                                {
                                    sysl -= kcxx.djsl;
                                    param.Add(new SqlParameter("@sl", kcxx.djsl));
                                    db.ExecuteSqlCommand(sql1, param.ToArray());
                                }
                            }
                        }
                    }

                    foreach (var mx in effectiveCfmx)
                    {
                        var mzczjl = new MzCfypczjlEntity
                        {
                            bz = "",
                            cfh = cfh,
                            CreatorCode = userCode,
                            CreateTime = DateTime.Now,
                            LastModifierCode = "",
                            LastModifyTime = null,
                            mzcfmxId = mx.Id,
                            operateType = ((int)EnumOperateType.Fy).ToString(),
                            sl = mx.sl * mx.zhyz,
                            ypCode = mx.ypCode
                        };
                        db.Insert(mzczjl);
                    }
                    new MzCfRepo(new DefaultDatabaseFactory()).UpdateFybzByCfh(cfh, EnumFybz.Yf, organizeId);

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
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public string ExecMzHandoutMedicine(string cfh, string yfbmCode, string userCode, string organizeId)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                var cfmx = _mxCfmxRepo.IQueryable(p => p.cfh == cfh && p.zt == "1" && p.OrganizeId == organizeId);
                if (!cfmx.Any()) return "未找到处方明细";
                var errorMsg = new StringBuilder();
                var cts = new CancellationTokenSource();
                try
                {
                    var parent = new Task(() =>
                    {
                        var taskFactory = new TaskFactory<bool>(cts.Token, TaskCreationOptions.AttachedToParent, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                        cfmx.ToList().ForEach(p =>
                        {
                            var childTask = taskFactory.StartNew(() =>
                            {
                                var curItemResult = new DispensingDmnService(new DefaultDatabaseFactory()).OutpatientDispensingReduceStock(p.ypCode, yfbmCode, cfh,
                                        organizeId, userCode);
                                if (string.IsNullOrWhiteSpace(curItemResult))
                                {
                                    var entity = new MzCfypczjlEntity
                                    {
                                        mzcfmxId = p.Id,
                                        operateType = ((int)EnumOperateType.Fy).ToString(),
                                        ypCode = p.ypCode,
                                        sl = p.sl * p.zhyz,
                                        bz = "",
                                        cfh = cfh,
                                        CreatorCode = userCode,
                                        CreateTime = DateTime.Now
                                    };
                                    new MzCfypczjlRepo(new DefaultDatabaseFactory()).Insert(entity);
                                    return true;
                                }

                                errorMsg.Append(string.IsNullOrWhiteSpace(curItemResult) ? "" : curItemResult + ";");
                                return false;
                            }, cts.Token);
                        });
                    });
                    parent.Start();
                    parent.Wait(cts.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                if (!string.IsNullOrWhiteSpace(errorMsg.ToString())) return errorMsg.ToString();

                _mzCfRepo.UpdateFybzByCfh(cfh, EnumFybz.Yf, organizeId);

                db.Commit();
                return errorMsg.ToString();
            }
        }

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="ypCode"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public string UpdateStock(string cfh, string ypCode, string yfbmCode, string userCode, string organizeId)
        {
            var param = new List<DbParameter>
            {
                new SqlParameter("@Yfbm", yfbmCode),
                new SqlParameter("@ypCode", ypCode),
                new SqlParameter("@Cfh", cfh),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@userCode", userCode)
            };
            return FirstOrDefaultNoLog<string>(TSqlOutpatient.mz_fy, param.ToArray());
        }

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        public List<string> MzDistributingMedicines(List<GetfyDetailCFList> reqdata)
        {
            var cfnmList = new List<string>();
            if (reqdata == null || reqdata.Count <= 0)
            {
                return cfnmList;
            }
            var fydisplayDetails = GetDetailInfo(AssemblefyDetailListRequest(reqdata));
            var fyMeidicneInfos = AssemblefyMeidicneInfo(fydisplayDetails);
            return UpdateStock(fyMeidicneInfos, Constants.CurrentYfbm.yfbmCode);
        }

        /// <summary>
        /// GetfyDetailCFList转换成fyMeidicneInfo
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        private List<fyDetailListRequest> AssemblefyDetailListRequest(List<GetfyDetailCFList> reqdata)
        {
            var result = new List<fyDetailListRequest>();
            reqdata.ForEach(p =>
            {
                result.Add(new fyDetailListRequest
                {
                    cfh = p.cfh,
                    cfmxId = p.cfmxId,
                    cfnm = Convert.ToInt32(p.cfnm),
                    ypCode = p.yp,
                    dj = 0,
                    dw = "",
                    gg = "",
                    je = 0,
                    jl = 0,
                    jldw = "",
                    plh = 0,
                    sjap = "",
                    ycmc = "",
                    yfmc = "",
                    yl = 0,
                    yldw = "",
                    ypmc = "",
                    yszt = ""
                });
            });
            return result;
        }

        /// <summary>
        /// fydisplayDetailInfo 转化成 fyMeidicneInfo
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        private List<fyMeidicneInfo> AssemblefyMeidicneInfo(IList<fydisplayDetailInfo> dataList)
        {
            var result = new List<fyMeidicneInfo>();
            if (dataList != null) dataList.ToList().ForEach(p =>
            {
                result.Add(new fyMeidicneInfo
                {
                    cfmxphId = p.cfmxphId,
                    cfnm = p.cfnm.ToString(),
                    pc = p.pc,
                    sl = p.sl,
                    ypCode = p.ypCode
                });
            });
            return result;
        }

        /// <summary>
        /// 门诊退药（mz_cfmxph验证）
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        public List<tyCFMainInfo> ExecMZExitMedicine(List<tyCFMainInfo> reqdata)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@reqdata", reqdata.ToDataTable()){
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.tyCFMainInfo"
                }
            };
            return FindList<tyCFMainInfo>("EXEC dbo.ExecMZExitMedicine @reqdata", param);
        }

        /// <summary>
        /// 门诊发药查询页面 验证mz_cfmxph
        /// </summary>
        /// <param name="reqdata"></param>
        /// <param name="gjzt"></param>
        /// <returns></returns>
        public List<fyQuerydisplayMainInfo> GetMzCfList(List<fyQueryMainInfo> reqdata, string gjzt)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@tyCFMainInfo", reqdata.ToDataTable()){
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.tyCFMainInfo"
                },
                new SqlParameter("@gjzt", gjzt),
                new SqlParameter("@YfbmCode", Constants.CurrentYfbm.yfbmCode),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            return FindList<fyQuerydisplayMainInfo>("EXEC dbo.[sp_fy_getCfInfo] @gjzt, @YfbmCode, @OrganizeId, @tyCFMainInfo", param);
        }

        #region new MZHandOutDetailQuery code
        /// <summary>
        /// 门诊发药查询显示详细信息(mz_cfmxph验证)
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        public List<fydisplayDetailInfo> GetMzHandOutDetail(List<fyDetailListRequest> reqdata)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@fyDetailListRequest", reqdata.ToDataTable()){
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.fyDetailListRequest"
                },
                new SqlParameter("@yfbmcode", Constants.CurrentYfbm.yfbmCode),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
            };
            return FindList<fydisplayDetailInfo>("EXEC dbo.[sp_fycx_MZHandOutDetailQuery] @YfbmCode, @OrganizeId, @fyDetailListRequest", param);
        }
        #endregion

        /// <summary>
        /// 获取收货部门
        /// </summary>
        /// <returns></returns>
        public List<DrugSpecialPropertiesVO> GetMeidicineSHBMList()
        {
            var strSql = new StringBuilder(@"
                        SELECT  yfbmCode Code ,
                        yfbmmc Name
                FROM    NewtouchHIS_Base..V_S_xt_yfbm
                WHERE OrganizeId=@OrganizeId
                                            ");
            DbParameter[] param =
            {
                new SqlParameter("@OrganizeId",OperatorProvider.GetCurrent().OrganizeId)
            };
            return FindList<DrugSpecialPropertiesVO>(strSql.ToString(), param).ToList();
        }

        #region 修改处方

        /// <summary>
        /// 归还冻结库存
        /// </summary>
        /// <param name="list"></param>
        public void ReleaseFrozenStock(List<OutpatientPrescriptionDetailBatchNumberEntity> list)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                foreach (var item in list)
                {
                    var strSql = new StringBuilder();
                    strSql.Append(@"UPDATE    kc
                                  SET   kc.Djsl = kc.Djsl - @sl
                                  FROM  XT_YP_KCXX kc
                                  WHERE kc.yfbmCode = @deptCode
                                        AND kc.Ypdm = @yp
                                        AND kc.pc = @pc;");
                    var paraList = new DbParameter[]
                    {
                        new SqlParameter("@sl", item.sl),
                        new SqlParameter("@deptCode", item.fyyf),
                        new SqlParameter("@yp", item.yp),
                        new SqlParameter("@pc", item.pc),
                        new SqlParameter("@cfmxphId", item.cfmxphId)
                    };
                    db.ExecuteSqlCommand(strSql.ToString(), paraList);
                }
                db.Commit();
            }
        }

        #endregion

        /// <summary>
        /// 获取发药药品详细
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public List<fyMeidicneInfo> GetDeliveryDrugs(string cfh, EnumOperateType operateType = EnumOperateType.Fy)
        {
            var sql = new StringBuilder(@"
select 
cfmx.ypCode, cfmx.ypmc, cfmx.gg, RTRIM(LTRIM(mxph.ph)) ph, RTRIM(LTRIM(mxph.pc)) pc
,ISNULL(CONVERT(NUMERIC(6,2),mxph.sl/(CASE yp.mzzybz WHEN '0' THEN yp.bzs WHEN '1' THEN yp.mzcls WHEN '2' THEN yp.zycls WHEN '3' THEN yp.mzcls END)),0) sl
,dbo.f_getYfbmYpComplexYpSlandDwV2(mxph.sl,@YfbmCode,cfmx.ypCode,@OrganizeId,'1' ) slstr
,cfmx.dw
,dbo.f_getYfbmYpComplexYpSlandDw(mxph.sl, mxph.yp, cfmx.ypCode, cfmx.OrganizeId) slstr
,cfmx.dj, cfmx.je, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz, cfmx.ycmc, cfmx.czh, cfmx.operateType,cfmx.CreateTime
 from mz_cfmxph mxph inner join(
select distinct  czjl.operateType,cfmx.CreateTime,cfmx.dj, cfmx.je, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz, cfmx.ycmc,cfmx.dw,cfmx.gg,cfmx.ypmc,cfmx.Id,cfmx.zt ,cfmx.cfh,cfmx.ypCode,OrganizeId,cfmx.czh from dbo.mz_cfmx(NOLOCK) cfmx
INNER JOIN dbo.mz_cfypczjl(NOLOCK) czjl ON czjl.mzcfmxId=cfmx.Id 
WHERE cfmx.cfh=@Cfh
AND cfmx.OrganizeId=@OrganizeId) cfmx
on cfmx.cfh=mxph.cfh and cfmx.ypCode=mxph.yp AND cfmx.OrganizeId=mxph.OrganizeId AND cfmx.zt='1'
INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=cfmx.ypCode AND yp.OrganizeId=cfmx.OrganizeId
--INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
--INNER JOIN dbo.xt_yp_bmypxx(NOLOCK) bmypxx ON bmypxx.Ypdm=mxph.yp AND bmypxx.OrganizeId=cfmx.OrganizeId 
--INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm ON yfbm.yfbmCode=bmypxx.yfbmCode AND yfbm.OrganizeId=cfmx.OrganizeId
AND ISNULL(mxph.czh,'')=ISNULL(cfmx.czh,'')
AND mxph.sl>0
AND mxph.zt='1' 
AND mxph.gjzt='0'
AND mxph.cfmxId=cfmx.Id
AND mxph.fyyf=@YfbmCode
");
            switch (operateType)
            {
                case EnumOperateType.Fy:
                case EnumOperateType.Ty:
                    sql.AppendLine("AND cfmx.operateType=@operateType ");
                    break;
            }

            sql.Append("ORDER BY cfmx.CreateTime ");
            var param = new DbParameter[]
            {
                new SqlParameter("@Cfh", cfh),
                new SqlParameter("@operateType", ((int)operateType).ToString()),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId),
                new SqlParameter("@YfbmCode", Constants.CurrentYfbm.yfbmCode)
            };
            return FindList<fyMeidicneInfo>(sql.ToString(), param);
        }

        /// <summary>
        /// 门诊发药
        /// </summary>
        /// <param name="cfh"></param>
        /// <returns></returns>
        public string ReturnDrug(string cfh)
        {
            var param = new List<DbParameter>
            {
                new SqlParameter("@Yfbm", Constants.CurrentYfbm.yfbmCode),
                new SqlParameter("@Cfh", cfh),
                new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId),
                new SqlParameter("@CreatorCode", OperatorProvider.GetCurrent().UserCode)
            };
            var outpar = new SqlParameter("@Res", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            param.Add(outpar);
            FindList<object>(@" EXEC [dbo].[sp_yp_returnDrug_mz] @Yfbm, @Cfh, @OrganizeId, @CreatorCode, @Res out", param.ToArray());
            return outpar.Value.ToString();
        }

        /// <summary>
        /// 获取已发处方
        /// </summary>
        /// <param name="keyWord"></param>
        /// <param name="fph"></param>
        /// <param name="cfh"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        public IList<cfInfoVo> GetDeliveredCf(string keyWord, string fph, string cfh, Pagination pagination = null)
        {
            var sql = new StringBuilder();
            sql.AppendLine("SELECT distinct cf.[cfnm] ");
            sql.AppendLine("	  ,cf.[cfh] ");
            sql.AppendLine("      ,cf.[Fph] ");
            sql.AppendLine("      ,cf.[CardNo] ");
            sql.AppendLine("      ,cf.[xm] ");
            sql.AppendLine("      ,cf.[nl] ");
            sql.AppendLine("      ,cf.[brxzmc] ");
            sql.AppendLine("      ,cf.[je] ");
            sql.AppendLine("      ,czjl.CreateTime fysj ");
            sql.AppendLine("FROM dbo.mz_cf(NOLOCK) cf  ");
            sql.AppendLine("INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh=cf.cfh ");
            sql.AppendLine("INNER JOIN dbo.mz_cfypczjl(NOLOCK) czjl ON czjl.mzcfmxId=cfmx.Id AND czjl.operateType= '" + (int)EnumOperateType.Fy + "' ");
            sql.AppendLine("WHERE cf.fybz= '" + (int)EnumFybz.Yf + "'  ");
            sql.AppendLine("AND cf.OrganizeId=@Organizeid");
            var param = new List<DbParameter>
            {
                new SqlParameter("@Organizeid", OperatorProvider.GetCurrent().OrganizeId)
            };
            if (!string.IsNullOrWhiteSpace(keyWord))
            {
                sql.AppendLine("AND (xm LIKE @xm OR CardNo LIKE @CardNo) ");
                param.Add(new SqlParameter("@xm", "%" + keyWord.Trim() + "%"));
                param.Add(new SqlParameter("@CardNo", "%" + keyWord.Trim() + "%"));
            }
            if (!string.IsNullOrWhiteSpace(fph))
            {
                sql.AppendLine("AND cf.fph=@fph ");
                param.Add(new SqlParameter("@fph", fph.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(cfh))
            {
                sql.AppendLine("AND cf.cfh=@cfh ");
                param.Add(new SqlParameter("@cfh", cfh.Trim()));
            }
            return QueryWithPage<cfInfoVo>(sql.ToString(), pagination, param.ToArray());
        }

        /// <summary>
        /// 获取发药信息
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="keyWork"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<DrugDeliveryEntity> GetDrugDeliveryList(DateTime startTime, DateTime endTime, string keyWork, string yfbmCode, string organizeId)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@startTime", startTime),
                new SqlParameter("@endTime", endTime),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@srm", keyWork.Trim()??""),
                new SqlParameter("@organizeId", organizeId??"")
            };
            return FindList<DrugDeliveryEntity>(TSqlOutpatient.drug_deliver_statistics, param);
        }

        /// <summary>
        /// 门诊发药处方明细查询
        /// </summary>
        /// <param name="xm"></param>
        /// <param name="cardNo"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public List<CfmxVO> SelectCfmx(string xm, string cardNo, string cfh, string organizeId, string yfbmCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@xm", xm),
                new SqlParameter("@cardNo",cardNo ),
                new SqlParameter("@cfh",cfh ),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode),
            };
            return FindListNoLog<CfmxVO>(TSqlDispensing.mz_fy_cfmx, param);
        }
        public List<CfmxVO> SelectCfmx_new(string cfh, string organizeId, string yfbmCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh",cfh ),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode),
            };
            return FindListNoLog<CfmxVO>(TSqlDispensing.mz_fy_cfmx_new, param);
        }
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
        public IList<CffyjlVO> SelectDispensedRpInfo(Pagination pagination, string keyword, string invoiceNo, string cfh, EnumOperateType operateType, DateTime kssj, DateTime jssj, string yfbmCode, string organizeId)
        {
            var sql = new StringBuilder(@"
SELECT DISTINCT cf.cfnm, cf.cfh, fybz, cf.Fph, cf.xm, cf.CardNo, cf.nl, cf.brxzmc, czjl.CreatorCode fyry 
FROM dbo.mz_cf(NOLOCK) cf
INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh=cf.cfh AND cfmx.OrganizeId=cf.OrganizeId AND cfmx.zt='1' 
INNER JOIN dbo.mz_cfypczjl(NOLOCK) czjl ON czjl.mzcfmxId=cfmx.Id 
WHERE cf.OrganizeId=@OrganizeId
AND cf.lyyf=@yfbmCode
AND cf.zt='1'
AND czjl.CreateTime BETWEEN @kssj AND @jssj
AND ISNULL(cf.cfh, '') like '%'+@cfh+'%'
AND ISNULL(cf.Fph, '') like '%'+@fph+'%'
AND (ISNULL(cf.xm, '') like '%'+@keyword+'%' or ISNULL(cf.CardNo, '') like '%'+@keyword+'%') 
AND czjl.operateType=1
");
            switch (operateType)
            {
                case EnumOperateType.Fy:
                case EnumOperateType.Ty:
                    sql.AppendLine("AND czjl.operateType=@operateType ");
                    break;
            }
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@operateType", ((int)operateType).ToString()),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@cfh", cfh??""),
                new SqlParameter("@fph", invoiceNo??""),
                new SqlParameter("@keyword", keyword??"")
            };
            return QueryWithPage<CffyjlVO>(sql.ToString(), pagination, param);
        }

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
        public IList<CfxxVO> SelectRpList(string keyword, string fph, DateTime kssj, DateTime jssj, string yfbmCode, string organizeId)
        {
            const string sql = @"
SELECT DISTINCT cf.cfnm, cf.CardNo, cf.xm,vout.mzh,cf.Fph, cf.sfsj, cf.cfh, cf.brxzmc, cf.ysmc, cf.ksmc, t.fysj
FROM dbo.mz_cf(NOLOCK) cf
INNER JOIN (
	SELECT czjl.cfh, MAX(cf.CreateTime) fysj, cf.OrganizeId
	FROM dbo.mz_cfypczjl(NOLOCK) czjl
	INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.Id=czjl.mzcfmxId AND cfmx.ypCode=czjl.ypCode AND cfmx.OrganizeId=@OrganizeId AND cfmx.zt='1' 
	INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=cfmx.OrganizeId AND cf.lyyf=@yfbmCode AND cf.zt='1'
	WHERE (ISNULL(cf.CardNo,'') LIKE '%'+@keyword+'%' OR ISNULL(cf.xm, '') LIKE '%'+@keyword+'%')
	AND czjl.CreateTime BETWEEN @kssj AND @jssj AND czjl.operateType='1' 
	GROUP BY czjl.cfh, cf.OrganizeId
) t ON t.cfh=cf.cfh AND t.OrganizeId=cf.OrganizeId
INNER JOIN [NewtouchHIS_Sett].[dbo].[V_invoiceEBillOutpatient] vout on vout.busNo=cf.jsnm
WHERE (ISNULL(cf.CardNo, '') LIKE '%'+@keyword+'%' OR ISNULL(cf.xm, '') LIKE '%'+@keyword+'%')
AND ISNULL(cf.Fph, '') LIKE '%'+@fph+'%'
AND t.fysj BETWEEN @kssj AND @jssj
AND cf.zt='1'
AND cf.OrganizeId=@OrganizeId
AND cf.lyyf=@yfbmCode
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@keyword", keyword),
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@fph", fph)
            };
            return FindList<CfxxVO>(sql, param);
        }

        /// <summary>
        /// 查询处方明细
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<CfmxVO> SelectRpDetail(string cfh, string yfbmCode, string organizeId)
        {
            const string sql = @"
SELECT b.ypmc, b.gg, CONVERT(INT,b.zxdwsl/b.zhyz) sl, dbo.f_getComplexYpSlandDw(b.zxdwsl, b.zhyz, b.dw, yp.zxdw) slStr,dbo.f_getyfbmDw(@yfbmCode, b.ypCode,yp.OrganizeId) dw,
CONVERT(NUMERIC(11,4),b.dj*b.zhyz) dj, CONVERT(NUMERIC(11,2),b.dj*b.zxdwsl) je, b.jl, b.jldw, b.yfmc, b.bz yszt, b.ycmc, b.czh, b.ysmc, b.sfsj
FROM (
	SELECT a.ypCode, a.ypmc, a.gg, SUM(a.zxdwsl) zxdwsl, a.dj, a.jl, a.jldw, a.yfmc, a.bz, a.ycmc, a.czh, dbo.f_getyfbmDw(@yfbmCode, a.ypCode, @OrganizeId) dw, 
	dbo.f_getyfbmZhyz(@yfbmCode, a.ypCode, @OrganizeId) zhyz, a.ysmc, a.sfsj
	FROM (
		SELECT cfmx.ypCode, cfmx.ypmc, cfmx.gg, czjl.sl zxdwsl, cfmx.dj, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz,cfmx.ycmc, cfmx.czh, cf.ysmc, cf.sfsj
		FROM dbo.mz_cfmx(NOLOCK) cfmx
		INNER JOIN dbo.mz_cfypczjl(NOLOCK) czjl ON czjl.mzcfmxId=cfmx.Id AND czjl.cfh=cfmx.cfh AND czjl.operateType='1'
		INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=cfmx.OrganizeId AND cf.zt='1'
		WHERE cfmx.OrganizeId=@OrganizeId
		AND cfmx.cfh=@cfh
		AND cfmx.zt='1'
		AND cf.lyyf=@yfbmCode
		UNION ALL
		SELECT cfmx.ypCode, cfmx.ypmc, cfmx.gg, -1*czjl.sl zxdwsl, cfmx.dj, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz,cfmx.ycmc, cfmx.czh, cf.ysmc, cf.sfsj
		FROM dbo.mz_cfmx(NOLOCK) cfmx
		INNER JOIN dbo.mz_cfypczjl(NOLOCK) czjl ON czjl.mzcfmxId=cfmx.Id AND czjl.cfh=cfmx.cfh AND czjl.operateType='2'
		INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=cfmx.OrganizeId AND cf.zt='1'
		WHERE cfmx.OrganizeId=@OrganizeId
		AND cfmx.cfh=@cfh
		AND cfmx.zt='1'
		AND cf.lyyf=@yfbmCode
	) a
	GROUP BY a.ypCode, a.ypmc, a.gg, a.dj, a.jl, a.jldw, a.yfmc, a.bz, a.ycmc, a.czh, a.ysmc, a.sfsj
) b
INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=b.ypCode AND yp.OrganizeId=@OrganizeId
INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
";
            var param = new DbParameter[]
            {
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@cfh", cfh)
            };
            return FindList<CfmxVO>(sql, param);
        }

        /// <summary>
        /// 判断是该处方是否存在
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns>返回有效的处方数</returns>
        public int ExistEffectiveRp(string cfh, string yfbmCode, string organizeId)
        {
            const string sql =
                @"SELECT COUNT(0) rpCount FROM dbo.mz_cf(NOLOCK) WHERE cfh=@cfh AND lyyf=@yfbmCode AND OrganizeId=@OrganizeId AND zt='1'";
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@OrganizeId",organizeId )
            };
            return FirstOrDefaultNoLog<int>(sql, param);
        }

        /// <summary>
        /// 根据卡号和姓名获取处方信息
        /// </summary>
        /// <param name="yfbmCode"></param>
        /// <param name="cardNo"></param>
        /// <param name="xm"></param>
        /// <param name="fybz"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<MzCfEntity> GetRpInfo(string yfbmCode, string cardNo, string xm, EnumFybz fybz = EnumFybz.Yp, string organizeId = "")
        {
            const string sql = @"
SELECT DISTINCT cf.* 
FROM dbo.mz_cf(NOLOCK) cf
INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh=cf.cfh AND cfmx.OrganizeId=cf.OrganizeId AND cfmx.zt='1' 
INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.fyyf=cf.lyyf AND mxph.cfh=cf.cfh AND mxph.yp=cfmx.ypCode AND mxph.zt='1' AND mxph.gjzt='0' AND mxph.OrganizeId=cf.OrganizeId
WHERE cf.CardNo=@CardNo
AND cf.OrganizeId=@OrganizeId
AND cf.xm=@xm
AND cf.fybz=@fybz
AND cf.zt='1'
AND cf.lyyf=@yfbmCode
AND cf.jsnm>0
";

            var param = new DbParameter[]
            {
                new SqlParameter("@CardNo", cardNo),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@xm", xm),
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@fybz", ((int)fybz).ToString()),
            };
            return FindList<MzCfEntity>(sql, param);
        }

        public IList<YpdlDTO> getYpdl(string orgId)
        {
            return FindList<YpdlDTO>(@"select distinct d.dlcode,d.dlmc from NewtouchHIS_Base..V_C_xt_yp p
            inner join NewtouchHIS_Base..xt_sfdl d on p.dlCode = d.dlCode
            where p.zt='1' and p.organizeid=@orgId", new DbParameter[] { new SqlParameter("@orgId", orgId) });
        }

        #region 药品、耗材使用情况
        public List<YfMaterialTjVo> GetMaterialList(Pagination pagination, string orgId, string ks, string ry, string slly, DateTime kssj,
            DateTime jssj, string keyword)
        {
            //string sql = @" exec [Rpt_CfMaterialTj] @beginDate ,@endDate,@OrganizeId,@ks,@ys,@ly,@xmmc  ";
            string sql = @"";
            if (slly == "1")
            {
                sql += @" select top 100 cf.createtime tdrq,dl.dlcode,dl.dlmc,sfxm.sfxmcode,sfxm.sfxmmc,'' jx,sfxm.gg,sfxm.bz gys,sfxm.dw,cfmx.sl,cfmx.dj,CONVERT(decimal(12,2),cfmx.dj*sl) zje, cf.ks,kdks.name kdksmc,cfmx.zxks,zxks.name zxksmc
from Newtouch_CIS..xt_cf cf
inner
join Newtouch_CIS..xt_cfmx cfmx on cfmx.cfid = cf.cfid and cfmx.OrganizeId = cf.OrganizeId and cfmx.zt = 1
left join NewtouchHIS_Base..xt_sfxm sfxm on sfxm.sfxmcode = cfmx.xmCode and cfmx.organizeid = sfxm.organizeid
left join NewtouchHIS_Base..V_S_Sys_Department kdks on kdks.code = cf.ks and kdks.organizeid = cf.organizeid
left join NewtouchHIS_Base..V_S_Sys_Department zxks on zxks.code = cfmx.zxks and zxks.organizeid = cfmx.organizeid
left join NewtouchHIS_Base..xt_sfdl dl on sfxm.sfdlCode = dl.dlcode and sfxm.organizeid = dl.organizeid and dl.zt = 1
where cf.OrganizeId = @OrganizeId and cf.zt = 1 and cf.createtime >= @beginDate and cf.createtime <= @endDate
and cf.cflx = 6--  and cf.sfbz = 1
and(cf.ks = @ks or @ks = '') and(cf.ys = @ys or @ys = '')
and cfmx.xmmc like '%' + @xmmc + '%'
and(dl.dlcode = '126' or dl.dlcode = '00000032') ";
            }
            else
            {
                sql += @"
select xm.tdrq , dl.dlcode,dl.dlmc,sfxm.sfxmcode,sfxm.sfxmmc,'' jx,sfxm.gg,sfxm.bz gys, sfxm.dw,xm.sl,xm.dj,CONVERT(decimal(12, 2), xm.dj * sl) zje,xm.ks,kdks.name kdksmc, xm.zxks,zxks.name zxksmc
from NewtouchHIS_Sett..zy_xmjfb xm
left
join NewtouchHIS_Base..V_S_Sys_Department kdks on kdks.code = xm.ks and kdks.organizeid = xm.organizeid
left join NewtouchHIS_Base..V_S_Sys_Department zxks on zxks.code = xm.ks and zxks.organizeid = xm.organizeid
left join NewtouchHIS_Base..xt_sfdl dl on xm.dl = dl.dlcode and xm.organizeid = dl.organizeid and dl.zt = 1
left join NewtouchHIS_Base..xt_sfxm sfxm on sfxm.sfxmcode = xm.sfxm and xm.organizeid = sfxm.organizeid
where xm.OrganizeId = @OrganizeId and(dl = '126' or dl = '00000032')
and xm.tdrq >= @beginDate and xm.tdrq <= @endDate
and(xm.ks = @ks or @ks = '') and(xm.ys = @ys or @ys = '')
and sfxm.sfxmmc like '%' + @xmmc + '%'  ";
            }

            var param = new DbParameter[] {
                 new SqlParameter("@beginDate", kssj),
                    new SqlParameter("@endDate", jssj),
                    new SqlParameter("@OrganizeId", orgId),
                    new SqlParameter("@ks", ks??""),
                    new SqlParameter("@ys", ry??""),
                    new SqlParameter("@ly", slly??""),
                    new SqlParameter("@xmmc", keyword??""),
            };
            var list = this.QueryWithPage<YfMaterialTjVo>(sql, pagination, param).ToList();
            return list;
        }

        #endregion

        #region 门诊处方
        public IList<MzcfcxList> GetMzcfList(Pagination pagination, MzcfcxVo req)
        {
            var parms = new List<SqlParameter> { };
            var strSql = new StringBuilder(@" select  c.mzh,c.xm,b.CreateTime kssj,a.zje,a.cftag
    ,case  a.cftag when 'JI' then '精神I类处方' 
			   when 'JII' then '精神II类处方' 
			   when 'MZ' then '麻醉处方' 
			   when 'LXGB' then '离休干部'
			   when 'TBCF' then '特病处方' end cftagName
    ,a.ks,ks.name ksmc,c.xb,b.cfh,a.cfId,a.ys yscode,ys.name ysmc,a.cflx,b.fybz,tf.returnDrugBillNo AS tydh
from Newtouch_CIS..xt_cf(nolock) a
join NewtouchHIS_PDS..mz_cf (nolock) b on b.cfh=a.cfh and b.OrganizeId=a.OrganizeId and b.zt=1
join Newtouch_CIS..xt_jz(nolock) c on c.jzId = a.jzId and a.OrganizeId=c.OrganizeId
left join NewtouchHIS_Base..Sys_Staff (nolock) ys on ys.gh=a.ys  and a.OrganizeId=ys.OrganizeId
left join NewtouchHIS_Base..V_S_Sys_Department (nolock) ks on ks.code=a.ks and ks.OrganizeId=a.OrganizeId
LEFT JOIN NewtouchHIS_PDS..mz_tfmx (nolock) tf ON tf.cfh = b.cfh 
where  a.zt='1' and a.OrganizeId=@orgId
    and a.CreateTime>=@kssj and a.CreateTime<=@jssj 
");
            if (!string.IsNullOrWhiteSpace(req.cflx))
            {
                strSql.AppendLine(" and a.cflx=@cflx");
                parms.Add(new SqlParameter("@cflx", req.cflx));
            }
            if (!string.IsNullOrWhiteSpace(req.ks))
            {
                strSql.AppendLine(" and a.ks=@ks");
                parms.Add(new SqlParameter("@ks", req.ks));
            }
            if (!string.IsNullOrWhiteSpace(req.keyword))
            {
                strSql.AppendLine("  and (c.mzh like @keyword or c.xm like @keyword)");
                parms.Add(new SqlParameter("@keyword", "%" + req.keyword + "%"));
            }
            parms.Add(new SqlParameter("@orgId", req.organizeId));
            parms.Add(new SqlParameter("@kssj", req.kssj));
            parms.Add(new SqlParameter("@jssj", req.jssj));
            return QueryWithPage<MzcfcxList>(strSql.ToString(), pagination, parms.ToArray());
        }
        public IList<MzcfcxDetailList> GetMzcfDetailList(Pagination pagination, MzcfcxVo req)
        {
            var sql = new StringBuilder();
            var parms = new List<SqlParameter> { };
            const string strsql = @" select c.mzh,c.blh,c.xm,a.zh,a.CreateTime kssj,isnull(a.ypCode,a.xmcode) ypCode
     ,a.ypmc , d.ypgg gg , a.sl sl,a.dj,a.dw ,mcjl ,pcCode,a.je,e.yzpcmcsm pcmc
     ,c.xb,b.cfh,b.cfId,f.yfmc ypyfmc,a.yfcode,b.ys yscode,ys.name ysmc,b.cflx,yfcf.fybz
 from Newtouch_CIS..xt_cfmx(nolock) a
 left join Newtouch_CIS..xt_cf(nolock) b on a.cfId = b.cfId and a.OrganizeId=b.OrganizeId
 join NewtouchHIS_PDS..mz_cf (nolock) yfcf on yfcf.cfh=b.cfh and yfcf.OrganizeId=b.OrganizeId and yfcf.zt=1
 left join Newtouch_CIS..xt_jz(nolock) c on c.jzId = b.jzId and a.OrganizeId=c.OrganizeId
 left join[NewtouchHIS_Base]..xt_ypsx d on a.ypcode = d.ypcode  and a.OrganizeId=d.OrganizeId
 left join[NewtouchHIS_Base]..xt_yzpc e on e.yzpcCode = a.pcCode  and a.OrganizeId=e.OrganizeId
 left join[NewtouchHIS_Base]..xt_ypyf f on f.yfcode = a.yfcode
 left join NewtouchHIS_Base..Sys_Staff (nolock) ys on ys.gh=b.ys  and a.OrganizeId=ys.OrganizeId
 where  a.zt='1' and a.OrganizeId=@orgId and b.cfh=@cfh ";
            parms.Add(new SqlParameter("@cfh", req.cfh));
            parms.Add(new SqlParameter("@orgId", req.organizeId));
            return QueryWithPage<MzcfcxDetailList>(strsql, pagination, parms.ToArray());
        }
        #endregion

        #region 电子处方信息获取病人信息
        /// <summary>
        /// 根据卡号和姓名获取处方信息
        /// </summary>
        /// <param name="yfbmCode"></param>
        /// <param name="cardNo"></param>
        /// <param name="xm"></param>
        /// <param name="fybz"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<DzcfBrxxDTO> GetElectronicPrescriptionCfInfo(string cfh, string xm, string organizeId = "")
        {
            const string sql = @"
select 
jz.xm xm
,jz.nlshow nl
,jz.mzh mzh
,jz.brxzmc brxzmc 
,jz.kh  CardNo 
,cf.cfh  cfh 
,cf.cfh  cfhComplete 
,''  cfnm 
,''  Fph 
,''  FphComplete 
,''  fybz 
,jz.jzysmc  ysmc 
,jz.jzys  yscode 
,jz.ghksmc  ksmc 
,cf.zje  Zje ,
jz.xb,
stuff(( select ','+ xyzd.zdmc from [Newtouch_CIS]..xt_xyzd xyzd where (xyzd.jzid=jz.jzId ) for xml path('')),1,1,'') xyzd,
stuff(( select ','+ zyzd.zdmc from [Newtouch_CIS]..xt_zyzd zyzd where (zyzd.jzid=jz.jzId ) for xml path('')),1,1,'') zyzd
from 
[Newtouch_CIS]..xt_cf cf
inner join [Newtouch_CIS]..xt_jz jz on jz.jzId=cf.jzId and jz.OrganizeId=cf.OrganizeId and jz.zt='1'
 where cf.isdzcf='1' 
 and cf.zt='1'
and cf.cfh=@cfh
and  cf.OrganizeId=@OrganizeId
and jz.xm=@xm
";
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@xm", xm),
            };
            return FindList<DzcfBrxxDTO>(sql, param);
        }
        #endregion
    }
}
