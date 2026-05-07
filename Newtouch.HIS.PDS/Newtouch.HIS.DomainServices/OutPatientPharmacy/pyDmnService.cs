using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkBase.MultiOrg.Domain.DBContext.Infrastructure;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Common.Operator;
using Newtouch.Core.Common;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.HIS.Domain.DO;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.HIS.Domain.VO;
using Newtouch.Infrastructure;
using Newtouch.Infrastructure.Log;
using Newtouch.Infrastructure.TSQL;
using Newtouch.Tools;

namespace Newtouch.HIS.DomainServices.OutPatientPharmacy
{
    /// <summary>
    /// 药品处理
    /// </summary>
    public class pyDmnService : DmnServiceBase, IPyDmnService
    {
        private readonly IMzCfRepo _mzCfRepo;
        private readonly IMzCfmxRepo _mzCfmxRepo;
        private readonly ISysConfigRepo _sysConfigRepo;
        private readonly ISysMedicineStockInfoRepo _kcxx;
        private readonly IOutpatientPrescriptionDetailBatchNumberRepo _mzCfmxphRepo;
        private readonly IDispensingDmnService _dispensingDmnService;

        public pyDmnService(IDefaultDatabaseFactory databaseFactory) : base(databaseFactory)
        {

        }

        /// <summary>
        /// 排药
        /// </summary>
        /// <param name="apiResp"></param>
        /// <param name="cfnm"></param>
        /// <param name="vo"></param>
        public string Py(object apiResp, string cfnm, cdInfoVO vo)
        {
            var res = "0";
            var organizeId = OperatorProvider.GetCurrent().OrganizeId;
            var userCode = OperatorProvider.GetCurrent().UserCode;
            if (vo.zje <= 0)
            {
                return res;
            }
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                //4. 配药
                var pyparlist = apiResp.ToJson().ToList<PyparListVo>();
                foreach (var t in pyparlist)
                {
                    var sqlParList = new List<SqlParameter>
                    {
                        new SqlParameter("@cfh", t.cfh),
                        new SqlParameter("@ai_nm", int.Parse(cfnm)),
                        new SqlParameter("@ai_mxnm", t.cfmxId),
                        new SqlParameter("@as_yp", t.yp),
                        new SqlParameter("@as_yjbm", vo.lyyf),
                        new SqlParameter("@adc_sl", t.sl),
                        new SqlParameter("@ai_cls", t.cls),
                        new SqlParameter("@OrganizeId", organizeId),
                        new SqlParameter("@user_code", userCode),
                        new SqlParameter("@adt_tdrq", System.DBNull.Value),
                        new SqlParameter("@as_fkcbz", '0')
                    };
                    var outpar = new SqlParameter("@as_err", SqlDbType.VarChar, 255)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    sqlParList.Add(outpar);
                    db.ExecuteSqlCommand(@"EXEC dbo.YP_XT_OP_PY @cfh, @ai_nm, @ai_mxnm, @as_yp, @as_yjbm, @adc_sl, @ai_cls,@OrganizeId, @user_code,@adt_tdrq,'', @as_err OUT,@as_fkcbz", sqlParList.ToArray());
                    if (outpar.Value == null || string.IsNullOrWhiteSpace(outpar.Value.ToString())) continue;
                    res = outpar.Value.ToString();
                    if (res != "1")
                    {
                        return res;
                    }
                }
                db.Commit();
            }
            return res;
        }

        /// <summary>
        /// 取消排药
        /// </summary>
        /// <param name="mzCfmxEntity"></param>
        /// <returns></returns>
        public string CancelArrangedDrug(MzCfmxEntity mzCfmxEntity)
        {
            var param = new List<DbParameter>
            {
                new SqlParameter("@YpCode", mzCfmxEntity.ypCode),
                new SqlParameter("@ypmc", mzCfmxEntity.ypmc),
                new SqlParameter("@Yfbm", Constants.CurrentYfbm.yfbmCode),
                new SqlParameter("@Cfh", mzCfmxEntity.cfh),
                new SqlParameter("@OrganizeId", mzCfmxEntity.OrganizeId),
                new SqlParameter("@CreatorCode", OperatorProvider.GetCurrent().UserCode)
            };
            var outpar = new SqlParameter("@Res", SqlDbType.VarChar, 100)
            {
                Direction = ParameterDirection.Output
            };
            param.Add(outpar);
            FindList<object>(
                @" EXEC [dbo].[sp_yp_book_mz_CancelArrangedDrug] @YpCode, @ypmc, @Yfbm, @Cfh, @OrganizeId, @CreatorCode, @Res out",
                param.ToArray());
            return outpar.Value.ToString();
        }

        /// <summary>
        /// 获取待处理事件总数
        /// </summary>
        /// <returns></returns>
        public NeedDealCountVO GetNeedDealCount()
        {
            try
            {
                var sql = new StringBuilder(@"
DECLARE @mzdfCount BIGINT;
DECLARE @zydfCount BIGINT;
DECLARE @sldshCount BIGINT;
DECLARE @ckdshCount BIGINT;
DECLARE @rkdshCount BIGINT;
");
                sql.AppendLine("SELECT @mzdfCount =COUNT(1) FROM dbo.mz_cf(NOLOCK) WHERE fybz='" + (int)EnumFybz.Yp +
                               "' AND lyyf=@yfbmCode AND OrganizeId=@OrganizeId --门诊待发 ");
                sql.AppendLine("SELECT @zydfCount=COUNT(1) FROM dbo.zy_ypyzxx(NOLOCK) WHERE fybz='" +
                               (int)EnumFybz.Yp + "' AND fyyf=@yfbmCode AND OrganizeId=@OrganizeId --住院待发 ");
                sql.AppendLine("SELECT @sldshCount=COUNT(1) FROM dbo.xt_yp_sld(NOLOCK) WHERE ffzt='" +
                               (int)EnumSLDDeliveryStatus.None +
                               "' AND Ckbm=@yfbmCode AND OrganizeId=@OrganizeId --申领待审核 ");
                sql.AppendLine("SELECT @ckdshCount=COUNT(1) FROM dbo.xt_yp_crkdj(NOLOCK) WHERE shzt='" +
                               (int)EnumDjShzt.WaitingApprove +
                               "' AND Ckbm=@yfbmCode AND OrganizeId=@OrganizeId --出库待审核 ");
                sql.AppendLine("SELECT @rkdshCount=COUNT(1) FROM dbo.xt_yp_crkdj(NOLOCK) WHERE shzt='" +
                               (int)EnumDjShzt.WaitingApprove +
                               "' AND Rkbm=@yfbmCode AND OrganizeId=@OrganizeId --入库待审核 ");
                sql.AppendLine(
                    "SELECT @mzdfCount mzdfCount, @zydfCount zydfCount, @sldshCount sldshCount, @ckdshCount ckdshCount, @rkdshCount rkdshCount ;");
                var param = new DbParameter[]
                {
                    new SqlParameter("@yfbmCode", Constants.CurrentYfbm.yfbmCode),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
                };
                return FindList<NeedDealCountVO>(sql.ToString(), param).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取待处理事件总数
        /// </summary>
        /// <returns></returns>
        public NeedDealCountVO GetNeedDealCountByYk()
        {
            try
            {
                var sql = new StringBuilder(
                    "DECLARE @tjshCount BIGINT, @rkdshCount BIGINT, @ckdshCount BIGINT, @sldshCount BIGINT, @expiryDrugCount BIGINT,@todayDrugCount BIGINT;");
                sql.AppendLine("--调价审核 ");
                sql.AppendLine("SELECT @tjshCount=COUNT(yptjId) FROM dbo.xt_yptj(NOLOCK) WHERE shzt='" +
                               (int)EnumSLDDeliveryStatus.None +
                               "' AND yfbmCode=@yfbmCode AND OrganizeId=@OrganizeId ");
                sql.AppendLine("--入库待审核 ");
                sql.AppendLine("SELECT @rkdshCount=COUNT(crkId) FROM ( ");
                sql.AppendLine("SELECT DISTINCT dj.crkId FROM dbo.xt_yp_crkdj(NOLOCK) dj ");
                sql.AppendLine("INNER JOIN dbo.xt_yp_crkmx(NOLOCK) mx ON mx.crkId=dj.crkId  ");
                sql.AppendLine(
                    "INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=mx.Ypdm AND yp.OrganizeId=dj.OrganizeId ");
                sql.AppendLine(
                    "INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId ");
                sql.AppendLine("WHERE dj.Rkbm=@yfbmCode AND dj.OrganizeId=@OrganizeId AND dj.zt='1' AND dj.shzt='" +
                               (int)EnumDjShzt.WaitingApprove + "' AND dj.Czsj BETWEEN CONVERT(varchar(7), getdate() , 120) + '-1' AND DATEADD(HOUR,1, GETDATE()) ");
                sql.AppendLine(") a ");
                sql.AppendLine("--出库待审核 ");
                sql.AppendLine("SELECT @ckdshCount=COUNT(crkId) FROM ( ");
                sql.AppendLine("SELECT DISTINCT dj.crkId  ");
                sql.AppendLine("FROM dbo.xt_yp_crkdj(NOLOCK) dj ");
                sql.AppendLine("INNER JOIN dbo.xt_yp_crkmx(NOLOCK) mx ON mx.crkId=dj.crkId ");
                sql.AppendLine(
                    "INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=mx.Ypdm AND yp.OrganizeId=dj.OrganizeId ");
                sql.AppendLine(
                    "INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId ");
                sql.AppendLine("WHERE dj.Ckbm=@yfbmCode AND dj.OrganizeId=@OrganizeId AND dj.shzt='" +
                               (int)EnumDjShzt.WaitingApprove +
                               "' and dj.djlx in (2) AND dj.Czsj BETWEEN CONVERT(varchar(7), getdate() , 120) + '-1' AND DATEADD(HOUR,1, GETDATE()) ");
                sql.AppendLine(") a  ");
                sql.AppendLine("--未出库申领单 ");
                sql.AppendLine("SELECT @sldshCount=COUNT(sldId) FROM dbo.xt_yp_sld(NOLOCK) WHERE ffzt='" +
                               (int)EnumSLDDeliveryStatus.None +
                               "' AND Ckbm=@yfbmCode AND OrganizeId=@OrganizeId AND CreateTime >=DATEADD(DAY, -31, GETDATE()) ");
                sql.AppendLine("--过期药品数量 ");
                sql.AppendLine(
                    "SELECT @expiryDrugCount=COUNT(a.kcId) FROM NewtouchHIS_PDS.dbo.xt_yp_kcxx(NOLOCK) a WHERE a.kcsl > 0 AND a.yxq < GETDATE() AND a.yfbmCode=@yfbmCode AND a.OrganizeId=@OrganizeId ");
                sql.AppendLine("--1个月过期药品数量 ");
                sql.AppendLine(
                    "SELECT @todayDrugCount=COUNT(a.kcId) FROM NewtouchHIS_PDS.dbo.xt_yp_kcxx(NOLOCK) a WHERE a.kcsl > 0 and a.zt='1' AND DATEDIFF(MONTH, GETDATE(),a.yxq) <='1'  AND a.yfbmCode=@yfbmCode AND a.OrganizeId=@OrganizeId ");
                sql.AppendLine(
                    "SELECT ISNULL(@tjshCount, 0) tjshCount, ISNULL(@rkdshCount, 0) rkdshCount, ISNULL(@ckdshCount, 0) ckdshCount, ISNULL(@sldshCount, 0) sldshCount, ISNULL(@expiryDrugCount, 0) expiryDrugCount,ISNULL(@todayDrugCount, 0) todayDrugCount;");
                var param = new DbParameter[]
                {
                    new SqlParameter("@yfbmCode", Constants.CurrentYfbm.yfbmCode),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
                };
                return FindList<NeedDealCountVO>(sql.ToString(), param).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    {Constants.Yfbm, Constants.CurrentYfbm.yfbmCode},
                    {Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        /// <summary>
        /// 获取待处理事件总数
        /// </summary>
        /// <returns></returns>
        public NeedDealCountVO GetNeedDealCountByYf()
        {
            try
            {
                int ago;
                var td = ConfigurationHelper.GetAppConfigValue("tjypDaysAgo");
                int.TryParse(td, out ago);
                ago = ago == 0 ? -31 : ago;
                var sql = new StringBuilder();
                sql.AppendLine(
                    "DECLARE @tjshCount BIGINT, @tjypCount BIGINT, @rkdshCount BIGINT, @ckdshCount BIGINT, @sldshCount BIGINT, @mzdpCount BIGINT, @mzdfCount BIGINT,@zydpCount BIGINT,@zydfCount BIGINT, @zydtCount BIGINT,@expiryDrugCount BIGINT,@todayDrugCount BIGINT;");
                sql.AppendLine("--调价审核 ");
                sql.AppendLine("SELECT @tjshCount=COUNT(tj.yptjId)  ");
                sql.AppendLine("FROM dbo.xt_yptj(NOLOCK) tj ");
                sql.AppendLine(
                    "INNER JOIN dbo.xt_yp_bmypxx(NOLOCK) bmypxx ON bmypxx.Ypdm=tj.ypCode AND bmypxx.OrganizeId=tj.OrganizeId AND bmypxx.zt='" +
                    (int)BenBuMenZT.Normal + "'  ");
                sql.AppendLine("WHERE tj.shzt='" + (int)EnumPriceAdjustOperationType.None + "'   ");
                sql.AppendLine("AND tj.OrganizeId=@OrganizeId ");
                sql.AppendLine("--调价药品 ");
                sql.AppendLine("SELECT @tjypCount=COUNT(yptjId) FROM ( ");
                sql.AppendLine("SELECT DISTINCT tj.yptjId ");
                sql.AppendLine("FROM dbo.xt_yptj(NOLOCK) tj  ");
                sql.AppendLine(
                    "INNER JOIN dbo.xt_yp_bmypxx(NOLOCK) bmypxx ON bmypxx.Ypdm=tj.ypCode AND bmypxx.OrganizeId=tj.OrganizeId AND bmypxx.zt='" +
                    (int)BenBuMenZT.Normal + "'   ");
                sql.AppendLine("WHERE tj.zxbz='" + (int)EnumPriceAdjustZXStatus.Executed +
                               "' and tj.yfbmCode=@yfbmCode ");
                sql.AppendLine("AND tj.OrganizeId=@OrganizeId  ");
                sql.AppendLine("AND tj.zxsj>=DATEADD(DAY, " + ago + ", GETDATE()) ");
                sql.AppendLine(") a ");
                sql.AppendLine("--入库审核 ");
                sql.AppendLine("SELECT @rkdshCount=COUNT(crkId) FROM ( ");
                sql.AppendLine("	SELECT DISTINCT dj.crkId FROM dbo.xt_yp_crkdj(NOLOCK) dj ");
                sql.AppendLine("	INNER JOIN dbo.xt_yp_crkmx(NOLOCK) mx ON mx.crkId=dj.crkId ");
                sql.AppendLine(
                    "	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=mx.Ypdm AND yp.OrganizeId=dj.OrganizeId");
                sql.AppendLine(
                    "	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId ");
                sql.AppendLine("    WHERE Rkbm=@yfbmCode AND dj.OrganizeId=@OrganizeId AND dj.shzt='" +
                               (int)EnumDjShzt.WaitingApprove + "' AND dj.Czsj BETWEEN CONVERT(varchar(7), getdate() , 120) + '-1' AND DATEADD(HOUR,1, GETDATE()) ");
                sql.AppendLine(") a ");
                sql.AppendLine(
                    "SELECT @mzdpCount=COUNT(0) FROM (SELECT DISTINCT CardNo FROM dbo.mz_cf(NOLOCK) WHERE fybz='" +
                    (int)EnumFybz.Wp + "' AND OrganizeId=@OrganizeId AND lyyf=@yfbmCode) T --门诊待排药 ");
                //sql.AppendLine(
                //    "SELECT @mzdfCount=COUNT(0) FROM (SELECT DISTINCT CardNo FROM dbo.mz_cf(NOLOCK) WHERE fybz='" +
                //    (int)EnumFybz.Yp + "' AND OrganizeId=@OrganizeId AND lyyf=@yfbmCode) T --门诊待发药 ");
                sql.AppendLine(string.Format(@"
SELECT @mzdfCount=COUNT(0) 
FROM (
	SELECT DISTINCT CardNo 
	FROM dbo.mz_cf(NOLOCK) cf
	INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh=cf.cfh AND cfmx.OrganizeId=cf.OrganizeId AND cfmx.zt='1' 
	INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.fyyf=cf.lyyf AND mxph.cfh=cf.cfh AND mxph.yp=cfmx.ypCode AND mxph.zt='1' AND mxph.gjzt='0' AND mxph.OrganizeId=cf.OrganizeId
	WHERE fybz='{0}' AND cf.OrganizeId=@OrganizeId AND cf.lyyf=@yfbmCode AND cf.zt='1' AND jsnm>0
) T --门诊待发药 ", (int)EnumFybz.Yp));
                sql.AppendLine("SELECT @zydpCount=COUNT(Id) FROM dbo.zy_ypyzxx(NOLOCK) WHERE fybz='" +
                               (int)EnumFybz.Wp + "' AND OrganizeId=@OrganizeId AND fyyf=@yfbmCode --住院待排药 ");
                sql.AppendLine("SELECT @zydfCount=COUNT(Id) FROM dbo.zy_ypyzxx(NOLOCK) WHERE fybz='" +
                               (int)EnumFybz.Yp + "' AND OrganizeId=@OrganizeId AND fyyf=@yfbmCode --住院待发要 ");
                sql.AppendLine("SELECT @zydtCount=COUNT(Id) from dbo.zy_ypyzxx where fybz = '" + (int)EnumFybz.Yf +
                               "' and sqtybz = '1' AND OrganizeId=@OrganizeId AND fyyf=@yfbmCode --住院待退药 ");
                sql.AppendLine("--过期药品数量 ");
                sql.AppendLine(
                    "SELECT @expiryDrugCount=COUNT(a.kcId) FROM NewtouchHIS_PDS.dbo.xt_yp_kcxx(NOLOCK) a WHERE a.kcsl > 0 AND a.yxq < GETDATE() AND a.yfbmCode=@yfbmCode AND a.OrganizeId=@OrganizeId ");
                sql.AppendLine("--1个月过期药品数量 ");
                sql.AppendLine(
                    "SELECT @todayDrugCount=COUNT(a.kcId) FROM NewtouchHIS_PDS.dbo.xt_yp_kcxx(NOLOCK) a WHERE a.kcsl > 0 and a.zt='1' AND DATEDIFF(MONTH, GETDATE(),a.yxq) <='1'  AND a.yfbmCode=@yfbmCode AND a.OrganizeId=@OrganizeId ");
                sql.AppendLine(
                    "SELECT ISNULL(@tjshCount, 0) tjshCount, ISNULL(@tjypCount, 0) tjypCount, ISNULL(@rkdshCount, 0) rkdshCount, ISNULL(@ckdshCount, 0) ckdshCount, ISNULL(@sldshCount, 0) sldshCount ");
                sql.AppendLine(
                    ", ISNULL(@mzdpCount, 0) mzdpCount, ISNULL(@mzdfCount, 0) mzdfCount, ISNULL(@zydpCount, 0) zydpCount, ISNULL(@zydfCount, 0) zydfCount, ISNULL(@zydtCount, 0) zydtCount, ISNULL(@expiryDrugCount, 0) expiryDrugCount, ISNULL(@todayDrugCount, 0) todayDrugCount; ");
                var param = new DbParameter[]
                {
                    new SqlParameter("@yfbmCode", Constants.CurrentYfbm.yfbmCode),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId)
                };
                return FindList<NeedDealCountVO>(sql.ToString(), param).FirstOrDefault();
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    {Constants.Yfbm, Constants.CurrentYfbm.yfbmCode},
                    {Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        /// <summary>
        /// 获取门诊月处方发药次 处方次/月
        /// </summary>
        /// <returns></returns>
        public List<FyCountVoByYfbm> GetMzFyCountVoByYfbm()
        {
            try
            {
                var sql = new StringBuilder("SELECT CONVERT(BIGINT, COUNT(Id)) fyCount, fysj FROM ");
                sql.AppendLine("(");
                sql.AppendLine("    SELECT fyjl.Id, CONVERT(VARCHAR(7), fyjl.CreateTime, 120) fysj");
                sql.AppendLine("    FROM dbo.mz_cf(NOLOCK) cf ");
                sql.AppendLine(
                    "    INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh = cf.cfh AND cfmx.OrganizeId=cf.OrganizeId");
                sql.AppendLine(
                    "    INNER JOIN dbo.mz_cfypczjl(NOLOCK) fyjl ON fyjl.mzcfmxId=cfmx.Id AND fyjl.operateType='" +
                    (int)EnumOperateType.Fy + "'");
                sql.AppendLine("    WHERE cf.fybz='" + (int)EnumFybz.Yf +
                               "' AND cf.OrganizeId=@OrganizeId AND fyjl.CreateTime>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01')");
                sql.AppendLine(") t");
                sql.AppendLine("GROUP BY t.fysj");
                return FindList<FyCountVoByYfbm>(sql.ToString(),
                    new DbParameter[] { new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId) });
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    {Constants.Yfbm, Constants.CurrentYfbm.yfbmCode},
                    {Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        /// <summary>
        /// 获取住院月医嘱发药次 医嘱次/月
        /// </summary>
        /// <returns></returns>
        public List<FyCountVoByYfbm> GetZyFyCountVoByYfbm()
        {
            try
            {
                var sql = new StringBuilder();
                sql.AppendLine("SELECT CONVERT(BIGINT, COUNT(Id)) fyCount, fysj FROM ");
                sql.AppendLine("( ");
                sql.AppendLine("    SELECT fyjl.Id, CONVERT(VARCHAR(7), fyjl.CreateTime, 120) fysj ");
                sql.AppendLine("    FROM dbo.zy_ypyzxx(NOLOCK) yz ");
                sql.AppendLine(
                    "    INNER JOIN dbo.zy_ypyzczjl(NOLOCK) fyjl ON fyjl.ypyzxxId = yz.Id AND fyjl.operateType = '" +
                    (int)EnumOperateType.Fy + "' ");
                sql.AppendLine("    WHERE yz.fybz = '" + (int)EnumFybz.Yf +
                               "' AND yz.OrganizeId = @OrganizeId AND fyjl.CreateTime >= CONVERT(DATETIME, CONVERT(VARCHAR(4), YEAR(GETDATE())) + '-01-01') ");
                sql.AppendLine(") t ");
                sql.AppendLine("GROUP BY t.fysj ");
                return FindList<FyCountVoByYfbm>(sql.ToString(),
                    new DbParameter[] { new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId) });
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    {Constants.Yfbm, Constants.CurrentYfbm.yfbmCode},
                    {Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        /// <summary> 
        /// 获取医嘱次|处方药品次
        /// </summary>
        /// <returns></returns>
        public List<FyCountBydlVO> GetFyCountBydl()
        {
            try
            {
                var sql = new StringBuilder(@"
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#ypInfo') and type='U')
BEGIN
	DELETE FROM #ypInfo;
END
ELSE
BEGIN
	CREATE TABLE #ypInfo(
		Id BIGINT IDENTITY(1,1) NOT NULL,
		ypCode VARCHAR(50) NOT NULL,
		mzzybz VARCHAR(1) NOT NULL
		PRIMARY KEY (Id)
	);
END 
");
                sql.AppendLine("INSERT INTO #ypInfo ");
                sql.AppendLine("SELECT t.ypCode, '3' FROM (");
                sql.AppendLine("    SELECT DISTINCT czjl.Id, czjl.ypCode ");
                sql.AppendLine("    FROM dbo.mz_cfypczjl(NOLOCK) czjl");
                sql.AppendLine(
                    "    INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.Id=czjl.mzcfmxId AND cfmx.OrganizeId=@OrganizeId ");
                sql.AppendLine(
                    "    WHERE czjl.CreateTime>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01') AND czjl.operateType='" +
                    (int)EnumOperateType.Fy + "' ");
                sql.AppendLine(") t ");
                sql.AppendLine("INSERT INTO #ypInfo");
                sql.AppendLine("SELECT t.ypCode, '3' FROM (");
                sql.AppendLine("    SELECT DISTINCT czjl.Id, czjl.ypCode FROM dbo.zy_ypyzczjl(NOLOCK) czjl");
                sql.AppendLine(
                    "    INNER JOIN dbo.zy_ypyzxx(NOLOCK) yz ON yz.Id=czjl.ypyzxxId AND yz.OrganizeId=@OrganizeId");
                sql.AppendLine(
                    "    WHERE czjl.CreateTime>=CONVERT(DATETIME, CONVERT(VARCHAR(4),YEAR(GETDATE()))+'-01-01') AND czjl.operateType='" +
                    (int)EnumOperateType.Fy + "'");
                sql.AppendLine(") t ");
                sql.AppendLine("SELECT CONVERT(BIGINT, COUNT(t.ypCode)) ypCount, t.dlCode, t.dlmc, t.mzzybz ");
                sql.AppendLine("FROM( ");
                sql.AppendLine("    SELECT yp.ypCode, sfdl.dlCode, sfdl.dlmc, ypinfo.mzzybz ");
                sql.AppendLine("    FROM NewtouchHIS_Base.dbo.xt_yp(NOLOCK) yp ");
                sql.AppendLine(
                    "    INNER JOIN NewtouchHIS_Base.dbo.xt_ypsx(NOLOCK) ypsx ON ypsx.ypId= yp.ypId AND ypsx.OrganizeId= yp.OrganizeId AND ypsx.zt= '1' ");
                sql.AppendLine(
                    "    INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yfbm_yp yfbmyp ON yfbmyp.dlCode= yp.dlCode AND yfbmyp.OrganizeId = yp.OrganizeId ");
                sql.AppendLine(
                    "    INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm ON yfbm.yfbmCode= yfbmyp.yfbmCode AND yfbm.OrganizeId= yp.OrganizeId AND yfbm.yfbmCode= @yfbmCode AND yfbm.zt= '1' ");
                sql.AppendLine(
                    "    INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode = yp.dlCode AND sfdl.OrganizeId = yp.OrganizeId ");
                sql.AppendLine("    INNER JOIN #ypInfo ypinfo ON ypinfo.ypCode = yp.ypCode ");
                sql.AppendLine("    WHERE yp.OrganizeId= @OrganizeId ");
                sql.AppendLine("    AND yp.zt= '1' ");
                sql.AppendLine(") t ");
                sql.AppendLine("GROUP BY t.dlCode, t.dlmc, t.mzzybz ");
                var param = new DbParameter[]
                {
                    new SqlParameter("@yfbmCode", Constants.CurrentYfbm.yfbmCode),
                    new SqlParameter("@OrganizeId", OperatorProvider.GetCurrent().OrganizeId),
                };
                return FindList<FyCountBydlVO>(sql.ToString(), param);
            }
            catch (Exception ex)
            {
                var tags = new Dictionary<string, string>
                {
                    {Constants.Yfbm, Constants.CurrentYfbm.yfbmCode},
                    {Constants.OrganizeId, OperatorProvider.GetCurrent().OrganizeId}
                };
                LogCore.Error("GetNeedDealCountByYk error", ex, addInfo: tags);
                return null;
            }
        }

        /// <summary>
        /// 插入门诊处方信息
        /// </summary>
        /// <param name="mzCfEntities"></param>
        /// <param name="mzCfmxEntities"></param>
        /// <returns></returns>
        public string InsertOutpatientRpInfo(List<MzCfEntity> mzCfEntities, List<MzCfmxEntity> mzCfmxEntities)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                _mzCfRepo.Insert(mzCfEntities);
                _mzCfmxRepo.Insert(mzCfmxEntities);
                db.Commit();
            }

            return "";
        }

        /// <summary>
        /// 物理删除处方主表和明细表   慎用
        /// </summary>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public int DeleteRpInfo(string cfh, string organizeId, string yfbmCode)
        {
            const string sql = @"
DELETE FROM dbo.mz_cf WHERE cfh=@cfh AND OrganizeId=@OrganizeId AND lyyf=@yfbmCode;
DELETE FROM dbo.mz_cfmx WHERE cfh=@cfh AND OrganizeId=@OrganizeId 
";
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh", cfh),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode)
            };
            return ExecuteSqlCommand(sql, param);
        }

        /// <summary>
        /// 查询处方信息
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="xm"></param>
        /// <param name="cardNo"></param>
        /// <param name="invoiceNo"></param>
        /// <param name="ksmc">科室名称</param>
        /// <param name="kssj">排药时间</param>
        /// <param name="jssj">排药时间</param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="jszt">结算状态 0-未结算 1-已结算</param>
        /// <param name="settExpired">结算已过期 1-是 0-否</param>
        /// <returns></returns>
        public IList<CfxxVO> SelectRpList(Pagination pagination, string xm, string cardNo, string invoiceNo, string ksmc
            , DateTime kssj, DateTime jssj, string organizeId, string yfbmCode, int jszt, string settExpired)
        {
            var sql = new StringBuilder(@"
SELECT DISTINCT cf.fybz, cf.CardNo, cf.xm, cf.cfh, cf.Fph, cf.sfsj, cf.CreateTime, cf.ysmc
FROM dbo.mz_cf(NOLOCK) cf
INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.cfh = cf.cfh AND mxph.OrganizeId=cf.OrganizeId AND mxph.gjzt='0' AND mxph.zt='1' AND mxph.fyyf=cf.lyyf
WHERE cf.OrganizeId=@OrganizeId
AND cf.zt='1'
AND cf.lyyf=@yfbmCode
AND mxph.CreateTime BETWEEN @kssj AND @jssj ");
            string settExpiredTime;
            int tmpHour;
            switch (jszt)
            {
                case 1:
                    //已结算
                    sql.AppendLine(@"
AND cf.jsnm>0 
AND cf.sfsj<=GETDATE() ");
                    break;
                case 0:
                    //未结算
                    sql.AppendLine(@"AND cf.jsnm<=0 ");
                    settExpiredTime = _sysConfigRepo.GetValueByCode("settExpiredTime", organizeId);
                    int.TryParse(settExpiredTime, out tmpHour);
                    tmpHour = tmpHour <= 0 ? 24 : tmpHour;
                    if ("0".Equals(settExpired))
                    {
                        sql.AppendLine(@"AND mxph.CreateTime>= DATEADD(HOUR, -" + tmpHour + ", GETDATE()) ");
                    }
                    else
                    {
                        sql.AppendLine(@"AND mxph.CreateTime< DATEADD(HOUR, -" + tmpHour + ", GETDATE()) ");
                    }
                    break;
                default:
                    settExpiredTime = _sysConfigRepo.GetValueByCode("settExpiredTime", organizeId);
                    int.TryParse(settExpiredTime, out tmpHour);
                    tmpHour = tmpHour <= 0 ? 24 : tmpHour;
                    if ("0".Equals(settExpired))
                    {
                        sql.AppendLine(@"AND mxph.CreateTime>= DATEADD(HOUR, -" + tmpHour + ", GETDATE()) ");
                    }
                    else
                    {
                        sql.AppendLine(@"AND mxph.CreateTime< DATEADD(HOUR, -" + tmpHour + ", GETDATE()) ");
                    }
                    break;
            }
            var param = new List<DbParameter>
            {
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode)
            };
            if (!string.IsNullOrWhiteSpace(xm))
            {
                sql.AppendLine("AND xm=@xm ");
                param.Add(new SqlParameter("@xm", xm));
            }
            if (!string.IsNullOrWhiteSpace(cardNo))
            {
                sql.AppendLine("AND CardNo=@CardNo ");
                param.Add(new SqlParameter("@CardNo", cardNo));
            }
            if (!string.IsNullOrWhiteSpace(invoiceNo))
            {
                sql.AppendLine("AND Fph=@Fph ");
                param.Add(new SqlParameter("@Fph", invoiceNo));
            }
            if (!string.IsNullOrWhiteSpace(ksmc))
            {
                sql.AppendLine("AND ksmc=@ksmc ");
                param.Add(new SqlParameter("@ksmc", ksmc));
            }

            return QueryWithPage<CfxxVO>(sql.ToString(), pagination, param.ToArray());
        }

        /// <summary>
        /// 查询处方信息
        /// </summary>
        /// <param name="xm"></param>
        /// <param name="cardNo"></param>
        /// <param name="invoiceNo"></param>
        /// <param name="ksmc">科室名称</param>
        /// <param name="kssj">排药时间</param>
        /// <param name="jssj">排药时间</param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="jszt">结算状态 0-未结算 1-已结算</param>
        /// <returns></returns>
        public List<CfxxVO> SelectRpList(string xm, string cardNo, string invoiceNo, string ksmc, DateTime kssj,
            DateTime jssj, string organizeId, string yfbmCode, int jszt)
        {
            var sql = new StringBuilder(@"
SELECT DISTINCT cf.fybz, cf.CardNo, cf.xm, cf.cfh, cf.Fph, cf.sfsj, cf.CreateTime, cf.ysmc
FROM dbo.mz_cf(NOLOCK) cf
INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.cfh = cf.cfh AND mxph.OrganizeId=cf.OrganizeId AND mxph.gjzt='0' AND mxph.zt='1' AND mxph.fyyf=cf.lyyf
WHERE cf.OrganizeId=@OrganizeId
AND cf.zt='1'
AND cf.lyyf=@yfbmCode
AND mxph.CreateTime BETWEEN @kssj AND @jssj ");
            switch (jszt)
            {
                case 1:
                    //已结算
                    sql.AppendLine(@"
AND cf.jsnm>0 
AND cf.sfsj<=GETDATE() ");
                    break;
                case 0:
                    //未结算
                    sql.AppendLine(@"
AND cf.jsnm<=0 ");
                    break;
            }

            var param = new List<DbParameter>
            {
                new SqlParameter("@kssj", kssj),
                new SqlParameter("@jssj", jssj),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode)
            };
            if (!string.IsNullOrWhiteSpace(xm))
            {
                sql.AppendLine("AND xm=@xm ");
                param.Add(new SqlParameter("@xm", xm));
            }

            if (!string.IsNullOrWhiteSpace(cardNo))
            {
                sql.AppendLine("AND CardNo=@CardNo ");
                param.Add(new SqlParameter("@CardNo", cardNo));
            }

            if (!string.IsNullOrWhiteSpace(invoiceNo))
            {
                sql.AppendLine("AND Fph=@Fph ");
                param.Add(new SqlParameter("@Fph", invoiceNo));
            }

            if (!string.IsNullOrWhiteSpace(ksmc))
            {
                sql.AppendLine("AND ksmc=@ksmc ");
                param.Add(new SqlParameter("@ksmc", ksmc));
            }

            return FindList<CfxxVO>(sql.ToString(), param.ToArray());
        }

        /// <summary>
        /// 查询处方明细
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public List<CfmxVO> SelectRpDetail(string cardNo, string cfh, string organizeId, string yfbmCode)
        {
            var sql = new StringBuilder(@"
SELECT cfmx.ypmc, cfmx.gg, cfmx.sl, cfmx.dw, cfmx.ycmc, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz yszt
FROM dbo.mz_cfmx(NOLOCK) cfmx
INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=cfmx.OrganizeId AND cf.zt='1'
WHERE cfmx.cfh=@cfh
AND cfmx.OrganizeId=@OrganizeId
AND cfmx.zt='1'
AND cf.lyyf=@yfbmCode
AND cf.CardNo=@CardNo
");
            var param = new DbParameter[]
            {
                new SqlParameter("@CardNo", cardNo ?? ""),
                new SqlParameter("@cfh", cfh ?? ""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode)
            };

            return FindList<CfmxVO>(sql.ToString(), param);
        }

        /// <summary>
        /// 查询处方排药明细
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="cfh"></param>
        /// <param name="organizeId"></param>
        /// <param name="yfbmCode"></param>
        /// <returns></returns>
        public List<CfmxVO> SelectRpArrangementDetail(string cardNo, string cfh, string organizeId, string yfbmCode)
        {
            var sql = new StringBuilder(@"
SELECT CONVERT(INT,a.zxdwsl/a.zhyz) sl,a.ypmc, a.gg, a.dw, a.ycmc, a.jl, a.jldw, a.yfmc, a.yszt FROM (
	SELECT cfmx.ypmc, cfmx.gg, cfmx.dw, cfmx.ycmc, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz yszt, SUM(mxph.sl) zxdwsl, cfmx.zhyz
	FROM dbo.mz_cfmx(NOLOCK) cfmx
	INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.cfh=cfmx.cfh AND mxph.gjzt='0' AND mxph.yp=cfmx.ypCode AND mxph.OrganizeId=cfmx.OrganizeId AND mxph.fyyf=@yfbmCode AND mxph.zt='1'
	INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=cfmx.OrganizeId AND cf.zt='1'
	WHERE cfmx.cfh=@cfh
	AND cfmx.OrganizeId=@OrganizeId
	AND cfmx.zt='1'
	AND cf.lyyf=@yfbmCode
	AND cf.CardNo=@CardNo
	GROUP BY cfmx.ypmc, cfmx.gg, cfmx.dw, cfmx.ycmc, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz, cfmx.zhyz
) a
");
            var param = new DbParameter[]
            {
                new SqlParameter("@CardNo", cardNo ?? ""),
                new SqlParameter("@cfh", cfh ?? ""),
                new SqlParameter("@OrganizeId", organizeId),
                new SqlParameter("@yfbmCode", yfbmCode)
            };

            return FindList<CfmxVO>(sql.ToString(), param);
        }

        /// <summary>
        /// 取消排药
        /// </summary>
        /// <param name="cardNo"></param>
        /// <param name="cfh"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string CancelArrangedDrug(string cardNo, string cfh, string yfbmCode, string organizeId, string userCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                var cflist = _mzCfRepo.IQueryable(p =>
                    p.cfh == cfh && p.zt == "1" && p.OrganizeId == organizeId && p.lyyf == yfbmCode);
                if (!string.IsNullOrWhiteSpace(cardNo)) cflist = cflist.Where(p => p.CardNo == cardNo);
                if (!cflist.Any()) return "未找到要取消的处方信息";
                var cfxx = cflist.FirstOrDefault();
                if (cfxx.fybz != ((int)EnumFybz.Yp).ToString()) return "取消排班只针对已排处方";
                var cfmxph = _mzCfmxphRepo.IQueryable(p =>
                    p.cfh == cfh && p.fyyf == yfbmCode && p.gjzt == "0" && p.OrganizeId == organizeId && p.zt == "1");
                if (!cfmxph.Any()) return "该处方未找到排药记录";
                cfmxph.ToList().ForEach(p =>
                {
                    var kcxx = _kcxx.IQueryable(k => k.ypdm == p.yp && k.OrganizeId == p.OrganizeId &&
                                                     k.zt == ((int)EnumKCZT.Enabled).ToString() &&
                                                     k.yfbmCode == yfbmCode && k.ph == p.ph && k.pc == p.pc);
                    if (!kcxx.Any()) throw new FailedException("未找到库存信息");
                    var sykc = (int)p.sl;
                    if (kcxx.Select(t => t.djsl).Sum() < p.sl) throw new FailedException("冻结库存数与取消排药数不等");
                    kcxx.ToList().ForEach(k =>
                    {
                        if (sykc <= 0) return;
                        if (k.djsl >= sykc)
                        {
                            k.djsl -= sykc;
                            sykc = 0;
                        }
                        else
                        {
                            sykc -= k.djsl;
                            k.djsl = 0;
                        }

                        k.LastModifyTime = DateTime.Now;
                        _kcxx.Update(k);
                    });
                    p.gjzt = "1";
                    p.zt = "0";
                    p.LastModifyTime = DateTime.Now;
                    p.LastModifierCode = userCode;
                    _mzCfmxphRepo.Update(p);
                });
                cfxx.fybz = ((int)EnumFybz.Wp).ToString();
                cfxx.LastModifyTime = DateTime.Now;
                _mzCfRepo.Update(cfxx);
                db.Commit();
                return "";
            }
        }

        /// <summary>
        /// 门诊排药
        /// </summary>
        /// <param name="patientInfo"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        /// <returns></returns>
        public string DrugArrangement(patientInfoVO patientInfo, string yfbmCode, string organizeId, string creatorCode)
        {
            using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
            {
                var cfhs = _mzCfRepo.IQueryable(p =>
                    p.xm == patientInfo.xm && p.CardNo == patientInfo.CardNo && p.OrganizeId == organizeId &&
                    p.lyyf == yfbmCode && p.zt == "1" &&
                    p.fybz == ((int)EnumFybz.Wp).ToString() &&
                    p.jsnm > 0);
                if (!cfhs.Any()) return "";
                foreach (var cfxx in cfhs.ToList())
                {
                    var cfmx = _mzCfmxRepo.IQueryable(p =>
                        p.cfh == cfxx.cfh && p.zt == "1" && p.OrganizeId == organizeId);
                    if (!cfmx.Any()) continue;
                    Parallel.ForEach(cfmx, p =>
                    {
                        var bookItem = new BookItemDo
                        {
                            YpCode = p.ypCode,
                            Sl = p.sl * p.zhyz,
                            Yfbm = yfbmCode ?? "",
                            OrganizeId = organizeId ?? "",
                            Cfh = cfxx.cfh ?? "",
                            CfmxId = p.Id.ToString(),
                            CreatorCode = p.CreatorCode ?? ""
                        };
                        var result =
                            new DispensingDmnService(new DefaultDatabaseFactory(), false).OutPatientBook(bookItem);
                        if (!string.IsNullOrWhiteSpace(result)) throw new FailedException("", result);
                    });

                    cfxx.fybz = ((int)EnumFybz.Yp).ToString();
                    cfxx.LastModifyTime = DateTime.Now;
                    cfxx.LastModiFierCode = creatorCode;
                    _mzCfRepo.Update(cfxx);
                }

                db.Commit();
            }

            return "";
        }

        /// <summary>
        /// 取消门诊预定
        /// </summary>
        /// <param name="successList"></param>
        /// <param name="organizeId"></param>
        /// <param name="creatorCode"></param>
        private void OutPatientBookCancel(List<BookItemDo> successList, string organizeId, string creatorCode)
        {
            Parallel.ForEach(successList,
                p =>
                {
                    new DispensingDmnService(new DefaultDatabaseFactory()).OutPatientBookCancel(p.YpCode, p.Sl, p.Yfbm,
                        p.Cfh, organizeId, creatorCode);
                });
        }

        /// <summary>
        /// 获取已排未发未结算的排药信息
        /// </summary>
        /// <param name="expirationDate"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<OutpatientPrescriptionDetailBatchNumberEntity> SelectMxph(DateTime expirationDate, string yfbmCode, string organizeId)
        {
            const string sql = @"
SELECT mxph.* 
FROM dbo.mz_cfmxph(NOLOCK) mxph
INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh=mxph.cfh AND cfmx.ypCode=mxph.yp AND cfmx.OrganizeId=mxph.OrganizeId AND cfmx.zt='1'
INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=mxph.OrganizeId AND cf.zt='1' AND cf.lyyf=mxph.fyyf
WHERE mxph.fyyf=@yfbmCode
AND mxph.OrganizeId=@OrganizeId
AND (cf.jsnm IS NULL OR cf.jsnm=0)
AND cf.fybz='1'
AND mxph.gjzt='0'
AND mxph.zt='1'
AND mxph.CreateTime<@expirationDate
";
            var param = new DbParameter[]
            {
                new SqlParameter("@yfbmCode", yfbmCode),
                new SqlParameter("@expirationDate", expirationDate),
                new SqlParameter("@OrganizeId", organizeId)
            };
            return _dataContext.Database.SqlQuery<OutpatientPrescriptionDetailBatchNumberEntity>(sql, param).ToList();
        }

        /// <summary>
        /// 获取要取消的处方
        /// </summary>
        /// <param name="cfh">目标处方号</param>
        /// <param name="processesMaxNum"></param>
        /// <param name="expirationDate"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public List<RpCancelVo> SelectCancelRps(string cfh, int processesMaxNum, DateTime expirationDate, string yfbmCode, string organizeId)
        {
            var sql = string.Format(@"
SELECT DISTINCT {0} cf.cfh, @OrganizeId OrganizeId, @yfbmCode yfbmCode
FROM dbo.mz_cfmxph(NOLOCK) mxph
INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh=mxph.cfh AND cfmx.ypCode=mxph.yp AND cfmx.OrganizeId=mxph.OrganizeId AND cfmx.zt='1'
INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.OrganizeId=mxph.OrganizeId AND cf.zt='1' AND cf.lyyf=mxph.fyyf
WHERE mxph.fyyf=@yfbmCode
{1}
AND mxph.OrganizeId=@OrganizeId
AND ISNULL(cf.jsnm,0)=0
AND cf.fybz='1'
AND mxph.gjzt='0'
AND mxph.zt='1'
AND mxph.CreateTime<@expirationDate
ORDER BY cf.cfh ASC
", processesMaxNum > 0 ? ("TOP " + processesMaxNum) : "", string.IsNullOrWhiteSpace(cfh) ? "" : "AND cf.cfh=@cfh ");
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh",cfh??"" ),
                new SqlParameter("@yfbmCode",yfbmCode ),
                new SqlParameter("@OrganizeId",organizeId ),
                new SqlParameter("@expirationDate",expirationDate ),
            };
            return FindList<RpCancelVo>(sql, param);
        }

        /// <summary>
        /// 取消排药
        /// </summary>
        /// <param name="mxphs"></param>
        /// <param name="yfbmCode"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public string OutPatientCancelArrangementDrug(List<OutpatientPrescriptionDetailBatchNumberEntity> mxphs, string yfbmCode, string userCode, string organizeId)
        {
            try
            {
                using (var db = new Infrastructure.EF.EFDbTransaction(_databaseFactory).BeginTrans())
                {
                    var part1Result = OutPatientSubtractForzenKcAndGj(mxphs, userCode, db);
                    if (!string.IsNullOrWhiteSpace(part1Result)) return part1Result;

                    foreach (var cfh in mxphs.Select(p => p.cfh).Distinct())
                    {
                        const string sql = @"
UPDATE dbo.mz_cf SET fybz=@fybz 
WHERE cfh=@cfh and OrganizeId=@OrganizeId and zt='1'
";
                        var param = new DbParameter[]
                        {
                            new SqlParameter("@cfh", cfh),
                            new SqlParameter("@fybz", ((int)EnumFybz.Wp).ToString()),
                            new SqlParameter("@OrganizeId", organizeId)
                        };
                        if (db.ExecuteSqlCommand(sql, param) <= 0) return "修改处方发药标志失败";
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
        /// 取消排药 取消冻结，药品归架
        /// </summary>
        /// <param name="mxphs"></param>
        /// <param name="userCode"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public string OutPatientSubtractForzenKcAndGj(List<OutpatientPrescriptionDetailBatchNumberEntity> mxphs, string userCode, Infrastructure.EF.EFDbTransaction db = null)
        {
            if (mxphs == null || mxphs.Count == 0) return "未找到需要解冻的药品";

            foreach (var mxph in mxphs)
            {
                var kcxxs = _kcxx.SelectData(mxph.yp, mxph.ph, mxph.pc, mxph.fyyf, mxph.OrganizeId);
                if (kcxxs == null || kcxxs.Count == 0) return string.Format("药品【{0}】未找到库存信息", mxph.yp);
                if (kcxxs.Sum(p => p.djsl) < mxph.sl) return string.Format("药品【{0}】已冻结数小于本次解冻数", mxph.yp);
                var sykc = (int)mxph.sl;
                foreach (var kcxx in kcxxs)
                {
                    if (sykc == 0) break;
                    if (kcxx.djsl >= sykc)
                    {
                        //库存足
                        var part1Result = SubtractForzenKc(kcxx.ypdm, sykc, kcxx.kcId, userCode, kcxx.OrganizeId);
                        if (!string.IsNullOrWhiteSpace(part1Result)) return part1Result;
                        sykc = 0;
                    }
                    else
                    {
                        //库存不足
                        var part1Result = SubtractForzenKc(kcxx.ypdm, kcxx.djsl, kcxx.kcId, userCode, kcxx.OrganizeId);
                        if (!string.IsNullOrWhiteSpace(part1Result)) return part1Result;
                        sykc -= kcxx.djsl;
                    }
                }

                var part2Result = UpdateFybz(mxph, userCode, db);
                if (!string.IsNullOrWhiteSpace(part2Result)) return part2Result;
            }

            return "";
        }

        /// <summary>
        /// 解冻库存
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="sl"></param>
        /// <param name="kcId"></param>
        /// <param name="userCode"></param>
        /// <param name="organizeId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private string SubtractForzenKc(string ypCode, int sl, string kcId, string userCode, string organizeId, Infrastructure.EF.EFDbTransaction db = null)
        {
            if (db == null)
            {
                if (_kcxx.SubtractForzenKc(sl, kcId, organizeId, userCode) <= 0)
                {
                    return string.Format("解冻药品代码为【{0}】的药品失败", ypCode);
                }
            }
            else
            {
                const string sql = @"
UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@sl, LastModifyTime=GETDATE(), LastModifierCode=@userCode
WHERE kcId=@kcId AND OrganizeId=@OrganizeId AND zt='1' AND tybz='0'
";
                var param = new DbParameter[]
                {
                    new SqlParameter("@sl", sl ),
                    new SqlParameter("@kcId", kcId ),
                    new SqlParameter("@userCode", userCode),
                    new SqlParameter("@OrganizeId", organizeId),
                };
                if (db.ExecuteSqlCommand(sql, param) <= 0) return string.Format("解冻药品代码为【{0}】的药品失败", ypCode);
            }

            return "";
        }

        /// <summary>
        /// 修改发药标志
        /// </summary>
        /// <param name="mxph"></param>
        /// <param name="userCode"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private string UpdateFybz(OutpatientPrescriptionDetailBatchNumberEntity mxph, string userCode, Infrastructure.EF.EFDbTransaction db = null)
        {
            if (db != null)
            {
                const string sql2 = @"
UPDATE dbo.mz_cfmxph SET zt='0', gjzt='1', LastModifyTime=GETDATE(), LastModifierCode=@userCode 
WHERE cfh=@cfh AND yp=@ypCode AND fyyf=@yfbmCode AND OrganizeId=@OrganizeId AND zt='1' AND gjzt='0' AND pc=@pc AND ph=@ph
";
                var param2 = new DbParameter[]
                {
                    new SqlParameter("@userCode", userCode),
                    new SqlParameter("@cfh", mxph.cfh),
                    new SqlParameter("@ypCode",mxph.yp ),
                    new SqlParameter("@yfbmCode", mxph.fyyf),
                    new SqlParameter("@pc", mxph.pc),
                    new SqlParameter("@ph",mxph.ph ),
                    new SqlParameter("@OrganizeId", mxph.OrganizeId)
                };
                if (db.ExecuteSqlCommand(sql2, param2) <= 0) return string.Format("药品代码为【{0}】的药品归架失败", mxph.yp);
            }
            else
            {

                mxph.zt = "0";
                mxph.gjzt = "1";
                mxph.LastModifierCode = userCode;
                mxph.LastModifyTime = DateTime.Now;
                if (_mzCfmxphRepo.GJDrug(mxph.yp, mxph.pc, mxph.ph, mxph.cfh, mxph.fyyf, mxph.OrganizeId, userCode) <= 0) return string.Format("药品代码为【{0}】的药品归架失败", mxph.yp);
            }

            return "";
        }

        /// <summary>
        /// 门诊取消排药
        /// </summary>
        /// <param name="rpInfo"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string OutPatientCancelArrangement(RpCancelVo rpInfo, string userCode)
        {
            var param = new DbParameter[]
            {
                new SqlParameter("@cfh", rpInfo.cfh),
                new SqlParameter("@OrganizeId", rpInfo.OrganizeId),
                new SqlParameter("@yfbmCode", rpInfo.yfbmCode),
                new SqlParameter("@userCode", userCode)
            };
            return FirstOrDefault<string>(TSqlDispensing.mz_yp_book_cancel_simplify, param);
        }

    }
}