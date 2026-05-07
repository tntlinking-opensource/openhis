using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.DTO.DrugStorage;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.Entity.V;

namespace Newtouch.HIS.DomainServices
{
	/// <summary>
	/// 药品库存信息
	/// </summary>
	public partial class DrugStorageDmnService
	{
		/// <summary>
		/// 获取出入库部门共同拥有的药品信息 库存数量为出库部门数量
		/// </summary>
		/// <param name="ckbm"></param>
		/// <param name="rkbm"></param>
		/// <param name="keyWord"></param>
		/// <param name="organizeid"></param>
		/// <returns></returns>
		public List<DrugStockInfoVEntity> GetDrugAndStock(string ckbm, string rkbm, string keyWord, string organizeid)
		{
			const string sql = @"
SELECT s.dlmc,s.ypmc,s.ypCode ypdm,s.ypgg gg,SUM(s.kykc) kykc, dbo.f_getComplexYpSlandDw(SUM(s.kykc),s.zhyz,s.bmdw,s.zxdw) slStr,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs
,s.bzdw,s.zxdw,s.pzwh,CONVERT(NUMERIC(11,4),s.zxdwlsj) zxdwlsj,CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz) lsj,CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz) pfj,s.ycmc sccj,s.yklsj,s.ykpfj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw, gjybdm
FROM 
(
	SELECT sfdl.dlmc, yp.ypmc, bmypxx.Ypdm ypCode, ypsx.ypgg, (kcxx.kcsl-kcxx.djsl) kykc, dbo.f_getyfbmDw(@YfbmCode, bmypxx.Ypdm, @Organizeid) bmdw
	, dbo.f_getyfbmZhyz(@YfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(ypsx.pzwh,'') pzwh,yp.ycmc
	,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj,ypsx.gjybdm
	FROM dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN dbo.xt_yp_bmypxx(NOLOCK) rkbmypxx ON rkbmypxx.Ypdm=bmypxx.Ypdm AND rkbmypxx.OrganizeId=bmypxx.OrganizeId AND rkbmypxx.zt='1'
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=bmypxx.OrganizeId
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId AND kcxx.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE bmypxx.yfbmCode=@yfbmCode
	AND bmypxx.zt='1'
	AND bmypxx.OrganizeId=@Organizeid
	AND rkbmypxx.yfbmCode=@rkbm
	AND (yp.ypCode LIKE '%'+@keyWord+'%' OR yp.ypmc LIKE '%'+@keyWord+'%' OR yp.py LIKE '%'+@keyWord+'%')
) s
GROUP BY s.dlmc,s.ypmc,s.ypCode,s.ypgg,s.bmdw,s.zhyz,s.bzs,s.bzdw,s.zxdw,s.pzwh,s.zxdwlsj,s.zxdwpfj,s.ycmc,s.yklsj,s.ykpfj,s.gjybdm
";
			var param = new DbParameter[] {
				new SqlParameter("@yfbmCode", ckbm??""),
				new SqlParameter("@rkbm", rkbm??""),
				new SqlParameter("@Organizeid", organizeid??""),
				new SqlParameter("@keyWord", keyWord??"")
			};
			return FindList<DrugStockInfoVEntity>(sql, param);
		}

		/// <summary>
		/// 获取当前部门拥有的药品和库存信息  （外部入库）
		/// </summary>
		/// <param name="currentYfbmCode"></param>
		/// <param name="keyWord"></param>
		/// <param name="organizeid"></param>
		/// <returns></returns>
		public List<DrugStockInfoVEntity> GetDrugAndStock(string currentYfbmCode, string keyWord, string organizeid)
		{
			const string sql = @"
SELECT s.dlmc,s.ypmc,s.ypCode ypdm,s.ypgg gg,SUM(s.kykc) kykc, dbo.f_getComplexYpSlandDw(SUM(s.kykc),s.zhyz,s.bmdw,s.zxdw) slStr,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs
,s.bzdw,s.zxdw,s.pzwh,CONVERT(NUMERIC(11,4),s.zxdwlsj) zxdwlsj,CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz) lsj,CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz) pfj,s.ycmc sccj,s.yklsj,s.ykpfj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwpfj*s.zhyz))+'元/'+s.bmdw) pfjdjdw,gjybdm
FROM 
(
	SELECT sfdl.dlmc, yp.ypmc, bmypxx.Ypdm ypCode, yp.ypgg, (kcxx.kcsl-kcxx.djsl) kykc, dbo.f_getyfbmDw(@yfbmCode, bmypxx.Ypdm, @Organizeid) bmdw
	, dbo.f_getyfbmZhyz(@YfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(yp.pzwh,'') pzwh,yp.ycmc
	,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj,yp.gjybdm
	FROM dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId AND kcxx.zt='1'
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE bmypxx.yfbmCode=@yfbmCode
	AND bmypxx.zt='1'
	AND bmypxx.OrganizeId=@Organizeid
	AND (yp.ypCode LIKE '%'+@keyWord+'%' OR yp.ypmc LIKE '%'+@keyWord+'%' OR yp.py LIKE '%'+@keyWord+'%')
) s
GROUP BY s.dlmc,s.ypmc,s.ypCode,s.ypgg,s.bmdw,s.zhyz,s.bzs,s.bzdw,s.zxdw,s.pzwh,s.zxdwlsj,s.zxdwpfj,s.ycmc,s.yklsj,s.ykpfj,s.gjybdm
";
			var param = new DbParameter[] {
				new SqlParameter("@yfbmCode", currentYfbmCode??""),
				new SqlParameter("@Organizeid", organizeid??""),
				new SqlParameter("@keyWord", keyWord??"")
			};
			return FindList<DrugStockInfoVEntity>(sql, param);
		}

		/// <summary>
		/// 获取当前部门拥有的药品和库存信息  （外部出库）
		/// </summary>
		/// <param name="currentYfbmCode"></param>
		/// <param name="keyWord"></param>
		/// <param name="organizeid"></param>
		/// <param name="fph">发票号</param>
		/// <param name="gysCode">供应商代码</param>
		/// <returns></returns>
		public List<DrugStockInfoVEntity> GetDrugAndStockByFph(string currentYfbmCode, string keyWord, string organizeid, string fph, string gysCode)
		{
			const string sql = @"
SELECT s.dlmc,s.ypmc,s.ypCode ypdm,s.ypgg gg,SUM(s.kykc) kykc, dbo.f_getComplexYpSlandDw(SUM(s.kykc),s.zhyz,s.bmdw,s.zxdw) slStr,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs
,s.bzdw,s.zxdw,s.pzwh,CONVERT(NUMERIC(11,4),s.zxdwlsj) zxdwlsj,CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz) lsj,CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz) pfj,s.ycmc sccj,s.yklsj,s.ykpfj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw
FROM 
(
	SELECT sfdl.dlmc, yp.ypmc, bmypxx.Ypdm ypCode, ypsx.ypgg, (kcxx.kcsl-kcxx.djsl) kykc, dbo.f_getyfbmDw(@yfbmCode, bmypxx.Ypdm, @Organizeid) bmdw
	, dbo.f_getyfbmZhyz(@YfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(ypsx.pzwh,'') pzwh,yp.ycmc
	,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj
	FROM dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=bmypxx.OrganizeId
	INNER JOIN dbo.xt_yp_crkmx(NOLOCK) crkmx ON crkmx.Ypdm=bmypxx.Ypdm AND crkmx.zt='1' 
	INNER JOIN dbo.xt_yp_crkdj(NOLOCK) crkdj ON crkdj.crkId=crkmx.crkId AND crkdj.OrganizeId=bmypxx.OrganizeId AND crkdj.zt='1'
	LEFT JOIN newtouchhis_base.dbo.V_S_xt_ypcrkfs fs on fs.crkfsCode = crkdj.crkfsdm
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId AND kcxx.pc=crkmx.pc AND kcxx.ph=crkmx.Ph 
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE bmypxx.yfbmCode=@yfbmCode
	AND bmypxx.zt='1'
	AND (REPLACE(LTRIM(RTRIM(crkmx.fph)),'	','') = @fph OR ''=@fph)
	AND bmypxx.OrganizeId=@Organizeid
	AND fs.crkbz = '0' --0：入库 1：出库
    AND crkdj.Ckbm=@gysCode
	AND (yp.ypCode LIKE '%'+@keyword+'%' OR yp.ypmc LIKE '%'+@keyword+'%' OR yp.py LIKE '%'+@keyword+'%')
) s
GROUP BY s.dlmc,s.ypmc,s.ypCode,s.ypgg,s.bmdw,s.zhyz,s.bzs,s.bzdw,s.zxdw,s.pzwh,s.zxdwlsj,s.zxdwpfj,s.ycmc,s.yklsj,s.ykpfj
";
			var param = new DbParameter[] {
				new SqlParameter("@yfbmCode", currentYfbmCode??""),
				new SqlParameter("@Organizeid", organizeid??""),
				new SqlParameter("@fph", (fph??"").Trim()),
				new SqlParameter("@gysCode", gysCode),
				new SqlParameter("@keyword", (keyWord??"").Trim())
			};
			return FindList<DrugStockInfoVEntity>(sql, param);
		}

		/// <summary>
		/// 批次分组
		/// </summary>
		/// <param name="ypdm"></param>
		/// <param name="yfbmCode"></param>
		/// <param name="organizeid"></param>
		/// <returns></returns>
		public List<DrugStockInfoVEntity> GetStockGroupByBatch(string ypdm, string yfbmCode, string organizeid)
		{
			const string sql = @"
SELECT s.ypdm,dbo.f_getComplexYpSlandDw(s.kykc,s.zhyz,s.bmdw,s.zxdw) slStr, CONVERT(INT,s.kykc) kykc,s.zxdw
,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs,s.bzdw,LTRIM(RTRIM(s.pc)) pc,LTRIM(RTRIM(s.ph)) ph,s.yxq,fph
,s.zxdwjj,s.bzdwjj,CONVERT(NUMERIC(11,4),s.zxdwjj*s.zhyz) bmdwjj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwjj*s.zhyz))+'元/'+s.bmdw) jjdjdw
FROM (
	SELECT yp.ypCode ypdm,SUM(kcxx.kcsl-kcxx.djsl) kykc,yp.zxdw,dbo.f_getyfbmDw(@yfbmCode, yp.ypCode, @Organizeid) bmdw
	,dbo.f_getyfbmZhyz(@yfbmCode, yp.ypCode, @Organizeid) zhyz, yp.bzs, yp.bzdw,kcxx.pc,kcxx.ph,crkmx.Fph
	,CONVERT(NUMERIC(11,4),kcxx.jj/yp.bzs) zxdwjj,kcxx.jj bzdwjj,kcxx.yxq
	FROM NewtouchHIS_Base.dbo.V_S_xt_yp yp  
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=yp.ypCode AND kcxx.OrganizeId=yp.OrganizeId AND kcxx.zt='1'
    LEFT JOIN dbo.xt_yp_crkmx (NOLOCK) crkmx ON crkmx.crkmxId=kcxx.crkmxId 
	WHERE kcxx.yfbmCode=@yfbmCode 
	AND yp.OrganizeId=@Organizeid
	AND yp.ypCode=@ypdm
	GROUP BY yp.zxdw,yp.ypCode,yp.bzs,yp.bzdw,kcxx.ph,kcxx.pc,kcxx.jj,kcxx.yxq,crkmx.Fph
) s
ORDER BY s.kykc DESC
";
			var param = new DbParameter[] {
				new SqlParameter("@yfbmCode", yfbmCode??""),
				new SqlParameter("@ypdm", ypdm??""),
				new SqlParameter("@Organizeid", organizeid??"")
			};
			return FindList<DrugStockInfoVEntity>(sql, param);
		}

		/// <summary>
		/// 根据发票号筛选 获取批次库存
		/// </summary>
		/// <param name="ypdm"></param>
		/// <param name="yfbmCode"></param>
		/// <param name="organizeid"></param>
		/// <param name="fph"></param>
		/// <param name="gysCode"></param>
		/// <returns></returns>
		public List<DrugStockInfoVEntity> GetStockGroupByBatchByFph(string ypdm, string yfbmCode, string organizeid, string fph, string gysCode, string pc = null)
		{
			var sql = new StringBuilder(@"
SELECT s.ypdm,dbo.f_getComplexYpSlandDw(s.kykc,s.zhyz,s.bmdw,s.zxdw) slStr, CONVERT(INT,s.kykc) kykc,s.zxdw
,s.bmdw dw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs,s.bzdw,LTRIM(RTRIM(s.pc)) pc,LTRIM(RTRIM(s.ph)) ph,s.yxq
,s.zxdwjj,s.bzdwjj,CONVERT(NUMERIC(11,4),s.zxdwjj*s.zhyz) bmdwjj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,2),s.zxdwjj*s.zhyz))+'元/'+s.bmdw) jjdjdw
FROM (
	SELECT yp.ypCode ypdm,SUM(kcxx.kcsl-kcxx.djsl) kykc,yp.zxdw,dbo.f_getyfbmDw(@yfbmCode, yp.ypCode, @Organizeid) bmdw
	,dbo.f_getyfbmZhyz(@yfbmCode, yp.ypCode, @Organizeid) zhyz, yp.bzs, yp.bzdw,kcxx.pc,kcxx.ph
	,CONVERT(NUMERIC(11,4),kcxx.jj/yp.bzs) zxdwjj,kcxx.jj bzdwjj,kcxx.yxq
	FROM NewtouchHIS_Base.dbo.V_S_xt_yp yp  
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
	LEFT JOIN dbo.xt_yp_crkmx(NOLOCK) crkmx ON crkmx.Ypdm=yp.ypCode AND crkmx.zt='1' AND (REPLACE(LTRIM(RTRIM(crkmx.fph)),'	','') = @fph OR ''=@fph)
	LEFT JOIN dbo.xt_yp_crkdj(NOLOCK) crkdj ON crkdj.crkId=crkmx.crkId AND crkdj.OrganizeId=yp.OrganizeId AND crkdj.zt='1' AND crkdj.Ckbm=@gysCode
	LEFT JOIN newtouchhis_base.dbo.V_S_xt_ypcrkfs fs on fs.crkfsCode = crkdj.crkfsdm AND fs.crkbz = '0'
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=yp.ypCode AND kcxx.OrganizeId=yp.OrganizeId AND kcxx.pc=crkmx.pc AND kcxx.ph=crkmx.Ph  and kcxx.crkmxId=crkmx.crkmxId
	WHERE kcxx.yfbmCode=@yfbmCode 
	AND yp.OrganizeId=@Organizeid
	AND yp.ypCode=@ypdm
");
			if (pc != null)
			{
				sql.AppendFormat("and  kcxx.pc= @pc");
			}
			sql.AppendFormat(" GROUP BY yp.zxdw,yp.ypCode,yp.bzs,yp.bzdw,kcxx.ph,kcxx.pc,kcxx.jj,kcxx.yxq)s");
			var param = new DbParameter[] {
				new SqlParameter("@yfbmCode", yfbmCode??""),
				new SqlParameter("@ypdm", ypdm??""),
				new SqlParameter("@fph", (fph??"").Trim()),
				new SqlParameter("@gysCode", gysCode),
				new SqlParameter("@Organizeid", organizeid??""),
				new SqlParameter("@pc", pc??"")
			};
			return FindList<DrugStockInfoVEntity>(sql.ToString(), param);
		}

		/// <summary>
		/// 批次分组
		/// </summary>
		/// <param name="ypdm"></param>
		/// <param name="yfbmCode"></param>
		/// <param name="kczt">库存状态 1-展示有效库存  0-展示无效库存  空-全部展示</param>
		/// <param name="organizeid"></param>
		/// <returns></returns>
		public IList<DrugStockInfoVEntity> GetStockGroupByBatch(Pagination pagination, string ypdm, string kczt, string yfbmCode, string organizeid)
		{
			const string sql = @"
SELECT s.ypdm,s.zxdw,s.bmdw dw,s.bzdw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs,LTRIM(RTRIM(s.pc)) pc,LTRIM(RTRIM(s.ph)) ph,s.yxq,s.zxdwjj,s.bzdwjj,s.zt kczt
,dbo.f_getComplexYpSlandDw(s.kykc,s.zhyz,s.bmdw,s.zxdw) slStr
,dbo.f_getComplexYpSlandDw(s.kcsl,s.zhyz,s.bmdw,s.zxdw) kcslStr
,dbo.f_getComplexYpSlandDw(s.djsl,s.zhyz,s.bmdw,s.zxdw) djslStr
,CONVERT(INT,s.kykc) kykc
,CONVERT(NUMERIC(11,4),s.zxdwjj*s.zhyz) bmdwjj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,4),s.zxdwjj*s.zhyz))+'元/'+s.bmdw) jjdjdw
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz))+'元/'+s.bmdw) pfjdjdw
,CONVERT(NUMERIC(11,2),s.zxdwpfj*s.kcsl) pjze
,CONVERT(NUMERIC(11,2),s.zxdwlsj*s.kcsl) lsze
,CONVERT(NUMERIC(11,2),s.zxdwjj*s.kcsl) jjze
FROM (
	SELECT yp.ypCode ypdm,yp.zxdw,dbo.f_getyfbmDw(@yfbmCode, yp.ypCode, @Organizeid) bmdw,kcxx.zt
	,SUM(kcxx.kcsl-kcxx.djsl) kykc
	,SUM(kcxx.kcsl) kcsl,SUM(kcxx.djsl) djsl
	,dbo.f_getyfbmZhyz(@yfbmCode, yp.ypCode, @Organizeid) zhyz, yp.bzs, yp.bzdw,kcxx.pc,kcxx.ph
	,CONVERT(NUMERIC(11,4),kcxx.jj/yp.bzs) zxdwjj,kcxx.jj bzdwjj,kcxx.yxq
	,CONVERT(NUMERIC(11,4),yp.lsj/yp.bzs) zxdwlsj,CONVERT(NUMERIC(11,4),yp.pfj/yp.bzs) zxdwpfj
	FROM NewtouchHIS_Base.dbo.V_S_xt_yp yp  
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=yp.ypCode AND kcxx.OrganizeId=yp.OrganizeId
	WHERE kcxx.yfbmCode=@yfbmCode 
	AND yp.OrganizeId=@Organizeid
	AND yp.ypCode=@ypdm
    AND (kcxx.zt=@kczt OR ''=@kczt)
	GROUP BY yp.zxdw,yp.ypCode,yp.bzs,yp.bzdw,kcxx.ph,kcxx.pc,kcxx.jj,kcxx.yxq,yp.pfj,yp.lsj,kcxx.zt
) s
";
			var param = new DbParameter[] {
				new SqlParameter("@yfbmCode", yfbmCode??""),
				new SqlParameter("@ypdm", ypdm??""),
				new SqlParameter("@kczt", kczt??""),
				new SqlParameter("@Organizeid", organizeid??"")
			};
			return QueryWithPage<DrugStockInfoVEntity>(sql, pagination, param);
		}

		/// <summary>
		/// 获取库存信息  库存量查询用
		/// </summary>
		/// <param name="yfbmCode"></param>
		/// <param name="keyWord"></param>
		/// <param name="tybz">停用标志 1-展示有效的本部门信息 0-无效的本部门信息 0-全部</param>
		/// <param name="kczt">库存状态 1-展示有效库存  0-展示无效库存  空-全部展示</param>
		/// <param name="show0kc">是否展示零库存  0-不展示 1-展示</param>
		/// <param name="organizeid"></param>
		/// <returns></returns>
		public IList<DrugStockInfoVEntity> GetDrugAndStock(Pagination pagination, string yfbmCode, string keyWord, string tybz, string kczt, string show0kc, string organizeid, string kcyjcode,string iskcyjz=null)
		{
			var sql = new System.Text.StringBuilder(@"
SELECT s.dlmc,s.ypmc,s.ypCode ypdm,s.py,s.ypgg gg,s.bmdw dw,s.bzdw,s.zxdw,s.pzwh,s.ycmc sccj,s.yklsj,s.ykpfj,s.kykc,kcsl,kcyjz
,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs
,dbo.f_getComplexYpSlandDw(s.kykc,s.zhyz,s.bmdw,s.zxdw) slStr
,CONVERT(NUMERIC(11,4),s.zxdwlsj) zxdwlsj,CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz) lsj,CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz) pfj
,CONVERT(NUMERIC(11,2),s.zxdwlsj*s.kcsl) lsze,CONVERT(NUMERIC(11,2),s.zxdwpfj*s.kcsl) pjze
,CONCAT(CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz),'元/',s.bmdw) lsjdjdw
,CONCAT(CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz),'元/',s.bmdw) pfjdjdw,gjybdm
FROM (
	SELECT sfdl.dlmc, yp.ypmc, yp.py, bmypxx.Ypdm ypCode,yp.ycmc, yp.ypgg
	,SUM(ISNULL((kcxx.kcsl-kcxx.djsl),0)) kykc,SUM(ISNULL(kcxx.kcsl,0)) kcsl,kcyjz
	,dbo.f_getyfbmDw(@yfbmCode, bmypxx.Ypdm, @Organizeid) bmdw
	,dbo.f_getyfbmZhyz(@YfbmCode, bmypxx.Ypdm, bmypxx.OrganizeId) zhyz, yp.bzs, yp.bzdw, yp.zxdw, ISNULL(yp.pzwh,'') pzwh
	,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj,yp.gjybdm
	FROM dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId 
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=bmypxx.OrganizeId AND sfdl.zt='1'
	WHERE bmypxx.yfbmCode=@yfbmCode
	AND bmypxx.OrganizeId=@Organizeid
	AND yp.ypmc LIKE '%'+@keyWord+'%'
");
			var param = new List<SqlParameter>();
			param.Add(new SqlParameter("@yfbmCode", yfbmCode ?? ""));
			param.Add(new SqlParameter("@Organizeid", organizeid ?? ""));
			param.Add(new SqlParameter("@kczt", kczt ?? ""));
			param.Add(new SqlParameter("@tybz", tybz ?? ""));
			param.Add(new SqlParameter("@keyWord", keyWord ?? ""));
			if (!string.IsNullOrWhiteSpace(kczt))
			{
				sql.AppendLine("	AND kcxx.zt=@kczt");
			}
			if (!string.IsNullOrWhiteSpace(tybz))
			{
				sql.AppendLine("	AND bmypxx.zt=@tybz");
			}
			if (!string.IsNullOrWhiteSpace(show0kc) && "0".Equals(show0kc))
			{
				sql.AppendLine("	AND kcxx.kcsl>0");
			}
			if (!string.IsNullOrWhiteSpace(kcyjcode))
			{
				sql.AppendLine("	and yp.ypcode in(select col from dbo.f_split(@kcyjcode,',') where col>'')");
				param.Add(new SqlParameter("@kcyjcode", kcyjcode));
			}
			sql.AppendLine(@"	GROUP BY sfdl.dlmc, yp.ypmc, yp.py, bmypxx.Ypdm,yp.ycmc, yp.ypgg,yp.lsj,yp.bzs,yp.pfj,bmypxx.OrganizeId,yp.bzdw
	, yp.zxdw, yp.pzwh,yp.gjybdm,yp.kcyjz
) s
");
            if (!string.IsNullOrWhiteSpace(iskcyjz))//库存预警生成采购计划使用
            {
                sql.AppendLine("	where   kcsl<=isnull(kcyjz,0)");
            }
            return QueryWithPage<DrugStockInfoVEntity>(sql.ToString(), pagination, param.ToArray());
		}

		/// <summary>
		/// 库存查询
		/// </summary>
		/// <param name="ypdms"></param>
		/// <param name="yfbmCode"></param>
		/// <param name="organizeid"></param>
		/// <returns></returns>
		public List<DrugStockInfo> SelectStock(string ypdm, string yfbmCode, string organizeid)
		{
			const string sql = @"
SELECT kcxx.ypdm ypCode, yp.ypmc, kcxx.yfbmCode lyyf, yfbm.yfbmmc lyyfmc
,SUM(kcxx.kcsl) kcsl, SUM(kcxx.djsl) djsl, SUM(kcxx.kcsl-kcxx.djsl) kysl
FROM dbo.xt_yp_kcxx(NOLOCK) kcxx
INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=kcxx.ypdm AND yp.OrganizeId=kcxx.OrganizeId
INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=kcxx.OrganizeId
LEFT JOIN NewtouchHIS_Base.dbo.xt_yfbm(NOLOCK) yfbm ON yfbm.yfbmCode=kcxx.yfbmCode AND yfbm.zt='1' AND yfbm.OrganizeId=kcxx.OrganizeId
WHERE kcxx.OrganizeId=@OrganizeId
AND kcxx.yfbmCode=@yfbmCode
AND kcxx.zt='1'
AND kcxx.ypdm=@ypdm
GROUP BY kcxx.ypdm, yp.ypmc, kcxx.yfbmCode, yfbm.yfbmmc ";
			var param = new DbParameter[] {
				new SqlParameter("@OrganizeId", organizeid),
				new SqlParameter("@yfbmCode", yfbmCode),
				new SqlParameter("@ypdm", ypdm)
			};
			return FindList<DrugStockInfo>(sql, param);
		}

		/// <summary>
		/// 查询过期药品
		/// </summary>
		/// <param name="pagination"></param>
		/// <param name="keyword"></param>
		/// <param name="gpyf">距离过期还剩x个月</param>
		/// <param name="yfbmCode"></param>
		/// <param name="organizeId"></param>
		/// <returns></returns>
		public IList<DrugStockInfoVEntity> SelectExpiredDrugs(Pagination pagination, string keyword, int gpyf, string yfbmCode, string organizeId, string gqyjcode,int? gqyyjts, string noShow0Kc = null)
		{
			var sql = new StringBuilder(@"select * from (
SELECT yp.ypmc, yp.ypgg gg, yp.py, yp.ycmc sccj, kcxx.ph, kcxx.pc, kcxx.yxq
,dbo.f_getYfbmYpComplexYpSlandDw(kcxx.kcsl, @yfbmCode, kcxx.ypdm, @OrganizeId) kcslStr
,dbo.f_getYfbmYpComplexYpSlandDw(kcxx.djsl, @yfbmCode, kcxx.ypdm, @OrganizeId) djslStr
,CONCAT(kcxx.jj, '元/', yp.bzdw) jjdjdw, CONVERT(NUMERIC(11,2),kcxx.jj*kcxx.kcsl/yp.bzs) jjze
,CONCAT(yp.lsj, '元/', yp.bzdw) lsjdjdw, CONVERT(NUMERIC(11,2),yp.lsj*kcxx.kcsl/yp.bzs) lsze 
,yp.ypCode ypdm,d.gysmc,kcxx.kcsl
FROM dbo.xt_yp_kcxx(NOLOCK) kcxx 
INNER JOIN NewtouchHIS_Base.dbo.V_C_xt_yp yp ON yp.ypCode=kcxx.ypdm AND yp.OrganizeId=kcxx.OrganizeId AND yp.zt='1' 
left join xt_yp_crkmx bb on bb.crkmxId=kcxx.crkmxId -- on bb.pc=kcxx.pc and bb.Ph=kcxx.ph and bb.Fph is not null 
left join xt_yp_crkdj(nolock) crkdj on bb.crkId=crkdj.crkId
left join  NewtouchHIS_Base.dbo.V_S_xt_ypgys d on d.gysCode=crkdj.Ckbm and d.OrganizeId =kcxx.OrganizeId 
WHERE kcxx.OrganizeId=@OrganizeId AND kcxx.yfbmCode=@yfbmCode
AND kcxx.zt='1'
");
			var param = new List<SqlParameter>();
			param.Add(new SqlParameter("@yfbmCode", yfbmCode));
			param.Add(new SqlParameter("@OrganizeId", organizeId));
			param.Add(new SqlParameter("@gpyf", gpyf)); 
		    param.Add(new SqlParameter("@keyword", keyword));
			string gltj = "0";
            if (noShow0Kc=="1") {
                sql.AppendLine(" AND kcxx.kcsl>0");
            }
			if (!string.IsNullOrWhiteSpace(gqyjcode))
			{
                sql.AppendLine(" and yp.ypcode in(select col from dbo.f_split(@gqyjcode,',') where col>'')");
				param.Add(new SqlParameter("@gqyjcode", gqyjcode));
				gltj = "1";
			}
            //if (!string.IsNullOrWhiteSpace(gqyyjts.ToString()))
            //{
            //    sql.AppendLine(" and DATEADD(day,@gqyyjts ,SUBSTRING(CONVERT(VARCHAR(15), kcxx.yxq, 120), 0, 11))<GETDATE()");
            //    param.Add(new SqlParameter("@gqyyjts", gqyyjts));
            //}
			if (gltj=="0")
			{
				switch (gpyf)
				{
					case 0:
						sql.AppendLine("AND DATEDIFF(day, GETDATE(),kcxx.yxq)<@gpyf ");
						break;
					case 1:
					case 2:
					case 3:
						sql.AppendLine("AND DATEDIFF(MONTH, GETDATE(),kcxx.yxq)<=@gpyf ");
						break;
					case 4:
						sql.AppendLine("AND DATEDIFF(MONTH, GETDATE(),kcxx.yxq)>@gpyf");
						break;
					case 6:
						sql.AppendLine("AND DATEDIFF(MONTH, GETDATE(),kcxx.yxq)>@gpyf");
						break;
                    case 7:
                        sql.AppendLine("AND DATEDIFF(day, GETDATE(),kcxx.yxq)=0");
                        break;
				}

			}
			if (!string.IsNullOrWhiteSpace(keyword)) sql.AppendLine("AND (yp.ypCode LIKE '%'+@keyword+'%' OR yp.ypmc LIKE '%'+@keyword+'%')");
			sql.AppendLine(@"  )bb
group by bb.ypmc, bb.gg, bb.py, bb.sccj, ph, pc, yxq, kcslStr, djslStr, jjdjdw, jjze, lsjdjdw, lsze, ypdm, gysmc,kcsl");
			//var param = new DbParameter[]
			//{
			//	new SqlParameter("@yfbmCode",yfbmCode ),
			//	new SqlParameter("@OrganizeId", organizeId),
			//	new SqlParameter("@gpyf", gpyf),
			//	new SqlParameter("@keyword", keyword),
			//};
			return QueryWithPage<DrugStockInfoVEntity>(sql.ToString(), pagination, param.ToArray());
		}

		public IList<CrkMxAll> GetCrkMxAll(string crkId, string orgId)
		{

			var sql = new StringBuilder(@"
select crkmxId,crkId,sldmxId,ypdm,fph,Kprq,Dprq,ph,yxq,a.pfj,a.lsj,ykpfj,yklsj,Zje,sl,Rkzhyz,Rkbmkc,rkdw,Ckzhyz,Ckbmkc,ckdw,Wg,zbbz,jkzcz,hgzm,ysjg,thyy,Cljg,scrq
,kl,jj,pc,cd,a.zt,px,CreatorCode,CreateTime,LastModifyTime,LastModifierCode,pfjze,ypmc,dlmc,ypgg,ycmc from  NewtouchHIS_PDS..xt_yp_crkmx a 
left join NewtouchHIS_Base..V_C_xt_yp b on a.Ypdm =b.ypCode and b.organizeId=@orgId
left join NewtouchHIS_Base..V_S_xt_sfdl c on b.dlCode = c.dlCode and c.organizeId=@orgId
where crkId = @crkId and a.zt='1'
");
			var param = new DbParameter[]
			{
				new SqlParameter("@crkId",crkId ),
                new SqlParameter("@orgId",orgId ),
            };
			return FindList<CrkMxAll>(sql.ToString(), param);
		}

		public int DeleteCrkDj(string crkId)
		{
			var index = 0;
			using (var db = new EFDbTransaction(_databaseFactory).BeginTrans())
			{
				//删除出入库表
				var inventoryEntity = db.FindEntity<SysMedicineStorageIOReceiptEntity>(crkId);
				db.Delete(inventoryEntity);
				//删除出入库明细表
				var inventoryDetailEntityList = db.IQueryable<SysMedicineStorageIOReceiptDetailEntity>().Where(a => a.crkId == crkId).ToList();
				foreach (var item in inventoryDetailEntityList)
				{
					db.Delete(item);
				}
				db.Commit();
				index++;
			}
			return index;
		}

		public int ReviseCrkDj(string crkId)
		{
			const string sql = @"
update dbo.xt_yp_crkdj set shzt=0 where crkId=@crkId
";
			var param = new DbParameter[]
			{
					new SqlParameter("@crkId", crkId ),
			};
			return ExecuteSqlCommand(sql, param);
		}

		public DjGys GetCrkDjh(string crkId, int? djlx)
		{
			var sql = new StringBuilder();
			if (djlx == null)
			{
				sql = new StringBuilder(@"select a.Pdh djh,a.Rkbm gyscode,c.gysmc from xt_yp_crkdj a
left join  NewtouchHIS_Base..xt_ypgys c on a.rkbm = c.gysCode
where crkId = @crkId");
			}
			else
			{
				sql = new StringBuilder(@"select a.Pdh djh,a.Ckbm gyscode,c.gysmc from xt_yp_crkdj a
left join  NewtouchHIS_Base..xt_ypgys c on a.ckbm = c.gysCode
where crkId = @crkId");
			}
			var param = new DbParameter[]
			{
				new SqlParameter("@crkId",crkId ),
			};
			return FirstOrDefault<DjGys>(sql.ToString(), param);

		}

        #region 药品有效期管理
        public IList<DrugExpiredInfoVEntity> GetStockExpiredSearch(Pagination pagination, string keyword,  string show0kc,string yfbmCode, string organizeid)
        {
            const string sql = @"
SELECT s.ypdm,s.zxdw,s.bmdw dw,s.bzdw,CONVERT(INT,s.zhyz) zhyz,CONVERT(INT,s.bzs) bzs,LTRIM(RTRIM(s.pc)) pc,LTRIM(RTRIM(s.ph)) ph,s.yxq,s.zxdwjj,s.bzdwjj,s.zt kczt
,dbo.f_getComplexYpSlandDw(s.kykc,s.zhyz,s.bmdw,s.zxdw) slStr
,dbo.f_getComplexYpSlandDw(s.kcsl,s.zhyz,s.bmdw,s.zxdw) kcslStr
,dbo.f_getComplexYpSlandDw(s.djsl,s.zhyz,s.bmdw,s.zxdw) djslStr
,CONVERT(INT,s.kykc) kykc
,CONVERT(NUMERIC(11,4),s.zxdwjj*s.zhyz) bmdwjj
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,4),s.zxdwjj*s.zhyz))+'元/'+s.bmdw) jjdjdw
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,4),s.zxdwlsj*s.zhyz))+'元/'+s.bmdw) lsjdjdw
,(CONVERT(VARCHAR(11),CONVERT(NUMERIC(11,4),s.zxdwpfj*s.zhyz))+'元/'+s.bmdw) pfjdjdw
,CONVERT(NUMERIC(11,2),s.zxdwpfj*s.kcsl) pjze
,CONVERT(NUMERIC(11,2),s.zxdwlsj*s.kcsl) lsze
,CONVERT(NUMERIC(11,2),s.zxdwjj*s.kcsl) jjze
,ypmc,gg,ycmc,kcId,createtime
FROM (
	SELECT yp.ypCode ypdm,yp.zxdw,dbo.f_getyfbmDw(@yfbmCode, yp.ypCode, @Organizeid) bmdw,kcxx.zt
	,SUM(kcxx.kcsl-kcxx.djsl) kykc
	,SUM(kcxx.kcsl) kcsl,SUM(kcxx.djsl) djsl
	,dbo.f_getyfbmZhyz(@yfbmCode, yp.ypCode, @Organizeid) zhyz, yp.bzs, yp.bzdw,kcxx.pc,kcxx.ph
	,CONVERT(NUMERIC(11,4),kcxx.jj/yp.bzs) zxdwjj,kcxx.jj bzdwjj,kcxx.yxq
	,CONVERT(NUMERIC(11,4),yp.lsj/yp.bzs) zxdwlsj,CONVERT(NUMERIC(11,4),yp.pfj/yp.bzs) zxdwpfj
	,yp.ypmc,yp.ypgg gg,yp.ycmc,kcxx.kcId,kcxx.createtime
	FROM NewtouchHIS_Base.dbo.V_S_xt_yp yp  
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=yp.ypCode AND kcxx.OrganizeId=yp.OrganizeId
	WHERE kcxx.yfbmCode=@yfbmCode 
	AND yp.OrganizeId=@Organizeid
    AND ( @keyword='' or  yp.ypCode like @keyword or yp.ypmc like @keyword )
    AND ( @show0kc=1 or (@show0kc=0 and kcxx.kcsl>0))
	--AND yp.ypCode=@ypdm
    --AND (kcxx.zt=@kczt OR ''=@kczt)
	GROUP BY yp.zxdw,yp.ypCode,yp.bzs,yp.bzdw,kcxx.ph,kcxx.pc,kcxx.jj,kcxx.yxq,yp.pfj,yp.lsj,kcxx.zt
	,yp.ypmc,yp.ypgg,yp.ycmc,kcxx.kcId,kcxx.CreateTime
) s
";
            var param = new DbParameter[] {
                new SqlParameter("@yfbmCode", yfbmCode??""),
                new SqlParameter("@keyword", keyword==""? "":'%'+keyword + '%'),
                new SqlParameter("@Organizeid", organizeid??""),
                new SqlParameter("@show0kc", show0kc??""),
            };
            return QueryWithPage<DrugExpiredInfoVEntity>(sql, pagination, param);
        }
        #endregion
    }
}
