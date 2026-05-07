using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Newtouch.Common;
using Newtouch.Core.Common;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.HIS.Domain.ValueObjects;
using Newtouch.Infrastructure;
using Newtouch.Tools;


namespace Newtouch.HIS.DomainServices
{
	/// <summary>
	/// 药品信息
	/// </summary>
	public class SysMedicineDmnService : DmnServiceBase, ISysMedicineDmnService
	{
		private readonly ISysMedicineRepo _sysMedicineRepository;
		private readonly ISysMedicinePropertyRepo _sysMedicinePropertyRepo;
		
		private readonly ISysMedicineBaseRepo _sysMedicineBaseRepository;
		private readonly ISysMedicinePropertyBaseRepo _sysMedicinePropertyBaseRepo;

		public SysMedicineDmnService(IBaseDatabaseFactory databaseFactory, ISysMedicineRepo sysMedicineRepository
			, ISysMedicinePropertyRepo sysMedicinePropertyRepo, ISysMedicineBaseRepo sysMedicineBaseRepository
			, ISysMedicinePropertyBaseRepo sysMedicinePropertyBaseRepo)
			: base(databaseFactory)
		{
			this._sysMedicineRepository = sysMedicineRepository;
			this._sysMedicinePropertyRepo = sysMedicinePropertyRepo;
			this._sysMedicineBaseRepository = sysMedicineBaseRepository;
			this._sysMedicinePropertyBaseRepo = sysMedicinePropertyBaseRepo;
		}

		/// <summary>
		/// 获取当前组织下的系统药品信息
		/// </summary>
		/// <returns></returns>
		public IList<SysMedicineVO> GetPaginationList(string organizeId, Pagination pagination, string zt, string ypflCode,string keyword = null)
		{
			if (string.IsNullOrWhiteSpace(organizeId))
			{
				return null;
			}
			var sql = @"
            SELECT x.ypId, x.zxdw,x.ypCode,x.ypmc,x.spm,x.py,x.bzdw,x.mzcls,x.mzcldw,x.zycls,x.zycldw,
            x.djdw,x.lsj,x.pfj,x.zfbl,x.zfxz,x.dlCode,x.ycmc,x.zt,0 bgbz,x.ypbzdm,x.cxjje
			,x.nbdl,x.mzzybz,x.CreateTime,d.pzwh,
            d.yptssx,d.ypflCode,d.zlff,d.jzlx,d.mrbzq,d.sjap,d.zbbz,d.yl,d.yldw,d.zjtzsj
			,d.ghdw,d.ypcd,d.ypgg,d.ybdm,d.xnhybdm, d.gjybdm,c.dlmc sfdlMc, e.jxmc jxmc, f.ypflmc ypflMc, case when d.ybdm IS NULL then '2' when d.ybdm IS NOT NULL AND d.LastYBUploadTime IS NOT NULL AND d.LastYBUploadTime >= d.LastModifyTime then '1' else '0' end isSynch,d.gjybmc
            FROM xt_yp as x  
			left join xt_ypsx as d on x.ypId=d.ypId
			left join xt_sfdl c	on x.dlCode = c.dlCode and c.OrganizeId = @organizeId
			left join xt_ypjx e	on x.jx = e.jxCode
			left join xt_ypfl f	on d.ypflCode = f.ypflCode
			where x.OrganizeId = @organizeId and d.OrganizeId = @organizeId ";
			DbParameter[] par;
            if (!string.IsNullOrEmpty(zt)) {
                sql += " and x.zt=@zt";
            }
            if (!string.IsNullOrEmpty(ypflCode))
            {
                sql += " and d.ypflCode=@ypflCode";
            }
            if (!string.IsNullOrEmpty(keyword))
			{
				sql += @"
            and (x.ypCode like @keyword or x.ypmc like @keyword or d.gjybdm like @keyword or d.ybdm like @keyword or x.py like @keyword) ";
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId),
					new SqlParameter("@keyword",  "%" + (keyword??"") + "%"),
                    new SqlParameter("@zt", zt),
                    new SqlParameter("@ypflCode", ypflCode)
                };
			}
			else
			{
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId),
                    new SqlParameter("@zt", zt),
                    new SqlParameter("@ypflCode", ypflCode)
                };
			}

			return this.QueryWithPage<SysMedicineVO>(sql, pagination, par);
		}



		/// <summary>
		/// 获取当前组织下的系统药品信息
		/// </summary>
		/// <returns></returns>
		public IList<SysMedicineVO> GetPaginationListdm(string organizeId, Pagination pagination, string type, string keyword = null)
		{
			if (string.IsNullOrWhiteSpace(organizeId))
			{
				return null;
			}
			var sql = @"
            SELECT x.ypId, x.zxdw,x.ypCode,x.ypmc,x.spm,x.py,x.bzdw,x.mzcls,x.mzcldw,x.zycls,x.zycldw,
            x.djdw,x.lsj,x.pfj,x.zfbl,x.zfxz,x.dlCode,x.ycmc,x.zt,0 bgbz,x.ypbzdm,x.cxjje
			,x.nbdl,x.mzzybz,x.CreateTime,d.pzwh,
            d.yptssx,d.ypflCode,d.zlff,d.jzlx,d.mrbzq,d.sjap,d.zbbz,d.yl,d.yldw,d.zjtzsj
			,d.ghdw,d.ypcd,d.ypgg,d.ybdm,d.xnhybdm, d.gjybdm,c.dlmc sfdlMc, e.jxmc jxmc, f.ypflmc ypflMc, case when d.ybdm IS NULL then '2' when d.ybdm IS NOT NULL AND d.LastYBUploadTime IS NOT NULL AND d.LastYBUploadTime >= d.LastModifyTime then '1' else '0' end isSynch,d.gjybmc
            FROM xt_yp as x  
			left join xt_ypsx as d on x.ypId=d.ypId
			left join xt_sfdl c	on x.dlCode = c.dlCode and c.OrganizeId = @organizeId
			left join xt_ypjx e	on x.jx = e.jxCode
			left join xt_ypfl f	on d.ypflCode = f.ypflCode
			where x.OrganizeId = @organizeId and d.OrganizeId = @organizeId ";
			DbParameter[] par;
			if (!string.IsNullOrEmpty(keyword))
			{
				sql += @"
            and (x.ypCode like @keyword or x.ypmc like @keyword or d.gjybdm like @keyword or d.ybdm like @keyword or x.py like @keyword) ";
				if (type != "qb" && type == "yd")
				{
					sql += "  and d.gjybdm<>'0'";
				}
				else if (type != "qb" && type == "wd")
				{
					sql += "and d.gjybdm='0'";
				}
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId),
					new SqlParameter("@keyword",  "%" + (keyword??"") + "%")
				};
			}
			else
			{
				if (type != "qb" && type == "yd")
				{
					sql += "  and d.gjybdm<>'0'";
				}
				else if (type != "qb" && type == "wd")
				{
					sql += "and d.gjybdm='0'";
				}
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId)
				};
			}

			return this.QueryWithPage<SysMedicineVO>(sql, pagination, par);
		}
        /// 目录对照查询
		/// </summary>
		/// <returns></returns>
		public IList<SysMedicineMldzxxVO> GetPaginationListMldzxx(string organizeId, Pagination pagination, string type, string keyword = null)
        {
            DbParameter[] par;
            if (string.IsNullOrWhiteSpace(organizeId))
            {
                return null;
            }
            var sql = @" ";
            if (type== "101")
            {
                sql = @"select 
mldz.id,
yp.ypcode code,
yp.ypmc name,
yp.ypgg gg,
yp.gjybdm gjybdm,
case isnull(mldz.isdz,'未对照') when '1' then '已对照' when '0' then '未对照' else '未对照' end isdz
 from V_S_xt_yp yp 
left join  xt_mldz mldz on yp.ypCode=mldz.code
 and yp.gjybdm=mldz.gjybdm and yp.OrganizeId =mldz.OrganizeId
 and  mldz.zt='1'
 where (yp.dlCode='01' or yp.dlCode='02')
 and yp.OrganizeId=@organizeId
 and yp.zt='1'";
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += @"
            and (yp.ypCode like @keyword or yp.ypmc like @keyword or yp.gjybdm like @keyword or yp.ybdm like @keyword or yp.py like @keyword) ";
                }
               
            }
            if (type == "102")
            {
                sql = @"select 
mldz.id,
yp.ypcode code,
yp.ypmc name,
yp.ypgg gg,
yp.gjybdm gjybdm,
case isnull(mldz.isdz,'未对照') when '1' then '已对照' when '0' then '未对照' else '未对照' end isdz
 from V_S_xt_yp yp 
left join  xt_mldz mldz on yp.ypCode=mldz.code
 and yp.gjybdm=mldz.gjybdm and yp.OrganizeId =mldz.OrganizeId
 and  mldz.zt='1'
 where  1=1
 and yp.dlCode='03'
 and yp.OrganizeId=@organizeId
 and yp.zt='1'";
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += @"
            and (yp.ypCode like @keyword or yp.ypmc like @keyword or yp.gjybdm like @keyword or yp.ybdm like @keyword or yp.py like @keyword) ";
                }

            }
            if (type == "301")
            {
                sql = @"  select 
mldz.id,
xm.sfxmCode code,
xm.sfxmmc name,
xm.gg gg,
xm.gjybdm gjybdm,
case isnull(mldz.isdz,'未对照') when '1' then '已对照' when '0' then '未对照' else '未对照' end isdz
 from V_S_xt_sfxm xm 
left join  xt_mldz mldz on xm.sfxmCode=mldz.code
 and xm.gjybdm=mldz.gjybdm and xm.OrganizeId =mldz.OrganizeId
 and  mldz.zt='1'
 where 1=1 
 and xm.sfdlCode='126'
 and xm.OrganizeId=@organizeId
 and xm.zt='1'";
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += @"
            and (xm.sfxmCode like @keyword or xm.sfxmmc like @keyword or xm.gjybdm like @keyword  or xm.py like @keyword) ";
                }

            }
            if (type == "201")
            {
                sql = @"  select 
mldz.id,
xm.sfxmCode code,
xm.sfxmmc name,
xm.gg gg,
xm.gjybdm gjybdm,
case isnull(mldz.isdz,'未对照') when '1' then '已对照' when '0' then '未对照' else '未对照' end isdz
 from V_S_xt_sfxm xm 
left join  xt_mldz mldz on xm.sfxmCode=mldz.code
 and xm.gjybdm=mldz.gjybdm and xm.OrganizeId =mldz.OrganizeId
 and  mldz.zt='1'
 where 1=1 
 and xm.sfdlCode!='126'
 and xm.OrganizeId=@organizeId
 and xm.zt='1'";
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += @"
            and (xm.sfxmCode like @keyword or xm.sfxmmc like @keyword or xm.gjybdm like @keyword  or xm.py like @keyword) ";
                }

            }
            par = new DbParameter[]
              {
                    new SqlParameter("@organizeId", organizeId),
                    new SqlParameter("@keyword",  "%" + (keyword??"") + "%")
              };
            return this.QueryWithPage<SysMedicineMldzxxVO>(sql, pagination, par);
        }
        /// <summary>
        /// 查询医保药品信息
        /// </summary>
        /// <param name="organizeId"></param>
        /// <param name="ypmc"></param>
        /// <param name="py"></param>
        /// <param name="gjybdm"></param>
        /// <param name="pzwh"></param>
        /// <returns></returns>
        public IList<G_yb_ypxxVO> GetYpypxxlist(string organizeId, string ypmc, string py, string gjybdm, string pzwh,string dataSource)
        {
	        string sql = "";
	        var pars = new List<SqlParameter>();
	        switch (dataSource)
	        {
		        case "1":
			    sql =  @"select top 200 * from (
select 
药品代码 ypdm
,数据来源 sjly
,注册名称 zcmc
,商品名称 spmc
,注册剂型 zcjx
,实际剂型 sjjx
,注册规格 zcgg
,实际规格 sjgg
,包装材质 bzcz
,最小包装数量 zxbzsl
,最小制剂单位 zxzjdw
,最小包装单位 zxbzdw
,药品企业 ypqy
,上市药品持有人 shypcyr
,批准文号 pzwh
,原批准文号 ypzwh
,药品本位码 ypbwm
,分包装企业名称 fbzqymc
,生产企业 scqy
,市场状态 sczt
,医保药品名称 ybypmc
,[2021版甲乙类] ybjyl
,医保剂型 ybjx
,编号 bh
,备注 bz
,甲乙类 jyl
,备注1 bz1
,[备注（特殊属性标识）] bztssxbs
,医保支付标准 ybzfbz
,pym
from dbo.gb_ypxx 
)a where 1=1 
";
			    if (!string.IsNullOrEmpty(gjybdm))
			    {
				    sql += "  and (ypdm like  '%'+@gjybdm+'%' or zcmc like '%'+@gjybdm+'%' or pzwh like '%'+@gjybdm+'%' or  ybypmc like '%'+@gjybdm+'%' or ypqy like '%'+@gjybdm+'%' or pym like '%'+@gjybdm+'%')";
				    pars.Add(new SqlParameter("@gjybdm", gjybdm));
			    }
			    if (!string.IsNullOrEmpty(pzwh))
			    {
				    sql += "  and pzwh like '%'+@pzwh+'%'";
				    pars.Add(new SqlParameter("@pzwh", pzwh));
			    }
			    if (!string.IsNullOrEmpty(ypmc))
			    {
				    sql += "  and scqy like '%'+@ypmc+'%'";
				    pars.Add(new SqlParameter("@ypmc", ypmc));
			    }   
		        break;
		        case "2":
			        sql =  @"select top 200 * from (
select 
MED_LIST_CODG ypdm
,null sjly
,REG_NAME zcmc
,DRUG_PRODNAME spmc
,REG_DOSFORM zcjx
,DRUG_DOSFORM sjjx
,REG_SPEC zcgg
,DRUG_SPEC sjgg
,PACMATL bzcz
, CONVERT(VARCHAR(255), MIN_PAC_CNT) AS zxbzsl
,MIN_PREPUNT zxzjdw
,MIN_PACUNT zxbzdw
,PRODENTP_NAME ypqy
,'' shypcyr
,APRVNO pzwh
,'' ypzwh
,DRUGSTDCODE ypbwm
,SUBPCK_FCTY fbzqymc
,PRODENTP_NAME scqy
,MKT_STAS sczt
,REG_NAME ybypmc
,'' ybjyl
,REG_DOSFORM ybjx
,'' bh
,MEMO bz
,'' jyl
,null bz1
,null bztssxbs
,null  ybzfbz
,null pym
from G_yb_wm_tcmpat_info_b
)a where 1=1 
";
			        if (!string.IsNullOrEmpty(gjybdm))
			        {
				        sql += "  and (ypdm like  '%'+@gjybdm+'%' or zcmc like '%'+@gjybdm+'%' or pzwh like '%'+@gjybdm+'%' or  ybypmc like '%'+@gjybdm+'%' or ypqy like '%'+@gjybdm+'%' or pym like '%'+@gjybdm+'%')";
				        pars.Add(new SqlParameter("@gjybdm", gjybdm));
			        }
			        if (!string.IsNullOrEmpty(pzwh))
			        {
				        sql += "  and pzwh like '%'+@pzwh+'%'";
				        pars.Add(new SqlParameter("@pzwh", pzwh));
			        }
			        if (!string.IsNullOrEmpty(ypmc))
			        {
				        sql += "  and scqy like '%'+@ypmc+'%'";
				        pars.Add(new SqlParameter("@ypmc", ypmc));
			        }   
			        break;
	        }
	        
				
				
			//if (!string.IsNullOrEmpty(py))
			//{
			//	sql += "  and 注册名称 like @ypmc";
			//	pars.Add(new SqlParameter("@ypmc", "%" + ypmc + "%"));
			//}
			return this.FindList<G_yb_ypxxVO>(sql, pars.ToArray());
		}

		/// <summary>
		/// 保存药品医保对码
		/// </summary>
		/// <param name="ybxx"></param>
		/// <param name="ypid"></param>
		/// <param name="organizeId"></param>
		/// <returns></returns>
		public int SaveYpYb(G_yb_ypxxVO ybxx, int? ypid, string organizeId)
		{
			int updatecount = 0;
			if (ypid.HasValue && ypid.Value > 0)
			{
				using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
				{
					var ypEntity = db.IQueryable<SysMedicineEntity>().FirstOrDefault(p => p.ypId == ypid && p.OrganizeId== organizeId);
					var ypsxEntity = db.IQueryable<SysMedicinePropertyEntity>().FirstOrDefault(p => p.ypId == ypid && p.OrganizeId == organizeId);
					if (!string.IsNullOrEmpty(ybxx.ypdm) && !string.IsNullOrEmpty(ybxx.zcmc))
					{
						ypsxEntity.gjybdm = ybxx.ypdm;
						ypsxEntity.gjybmc = ybxx.zcmc;
					}
					if (!string.IsNullOrEmpty(ybxx.jyl))
					{
						ypsxEntity.ybbz = "1";//存在国家医保代码 不查看药品本身属性 直接默认医保
						ypEntity.zfxz = EnumZFXZv2.J.GetDescription().Contains(ybxx.jyl) ? "4" : EnumZFXZv2.Y.GetDescription().Contains(ybxx.jyl) ? "5" : EnumZFXZv2.B.GetDescription().Contains(ybxx.jyl) ? "6" : "1";
					}

					db.Update(ypEntity);
					db.Update(ypsxEntity);

					updatecount = db.Commit();
				}

			}

			return updatecount;
		}

		/// <summary>
		/// 查询系统材料信息
		/// </summary>
		/// <param name="organizeId"></param>
		/// <param name="pagination"></param>
		/// <param name="type"></param>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public IList<SysMedicineVO> GetclxxList(string organizeId, Pagination pagination, string type, string keyword = null)
		{
			if (string.IsNullOrWhiteSpace(organizeId))
			{
				return null;
			}
			var sql = @" select x.*,d.gjybdm,d.gjybmc, d.ypgg,d.ypzsm from dbo.xt_yp x
          left join xt_ypsx as d on x.ypId=d.ypId
          where x.dlcode='18' and x.zt='1'  and x.organizeid=@organizeId  ";
			DbParameter[] par;
			if (!string.IsNullOrEmpty(keyword))
			{
				sql += " and x.ypmc like @keyword ";
				if (type != "qb" && type == "yd")
				{
					sql += "  and d.gjybdm is not null";
				}
				else if (type != "qb" && type == "wd")
				{
					sql += " and d.gjybdm is  null";
				}
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId),
					new SqlParameter("@keyword",  "%" + (keyword??"") + "%")
				};
			}
			else
			{
				if (type != "qb" && type == "yd")
				{
					sql += "  and gjybdm is not null";
				}
				else if (type != "qb" && type == "wd")
				{
					sql += " and gjybdm is  null";
				}
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId)
				};
			}

			return this.QueryWithPage<SysMedicineVO>(sql, pagination, par);
		}

		/// <summary>
		/// 查询医保材料信息 根据系统材料信息
		/// </summary>
		/// <param name="organizeId"></param>
		/// <param name="ypmc"></param>
		/// <param name="py"></param>
		/// <param name="gjybdm"></param>
		/// <param name="pzwh"></param>
		/// <returns></returns>
		public IList<G_yb_clxxVO> GetYbclxxlist(string organizeId, string ypmc, string py, string gjybdm, string pzwh,string dataSource)
		{
			var sql = "";
			var pars = new List<SqlParameter>();
			switch (dataSource)
			{
				case "1":
					sql = @"select top 200 * from(
select 
耗材代码 hcdm
,一级分类 yjfl
,二级分类 ejfl
,三级分类 sjfl
,单件产品名称 ybtym
,材质 cz
,特征 tz
,注册备案号 zcbah
,单件产品名称 djcpmc
,耗材企业 hcqy
,规格型号数 ggxhs
,注册备案人 zcbar
,ybxz
,pym
from gb_clxx )a where 1=1 ";
					if (!string.IsNullOrEmpty(gjybdm))
					{
						sql +=
							"  and (hcdm like  '%'+@gjybdm+'%' or djcpmc like '%'+@gjybdm+'%' or zcbah like '%'+@gjybdm+'%' or  hcqy like '%'+@gjybdm+'%' or pym like '%'+@gjybdm+'%')";
						pars.Add(new SqlParameter("@gjybdm", gjybdm));
					}

					if (!string.IsNullOrEmpty(pzwh))
					{
						sql += "  and zcbah like '%'+@pzwh+'%'";
						pars.Add(new SqlParameter("@pzwh", pzwh));
					}

					if (!string.IsNullOrEmpty(ypmc))
					{
						sql += "  and hcqy like '%'+@ypmc+'%'";
						pars.Add(new SqlParameter("@ypmc", ypmc));
					}
					
					break;
				case "2":
					//医保下载目录
					sql = @"select top 200 * from(
select 
MED_LIST_CODG hcdm
,'1' yjfl
,'1' ejfl
,'1'sjfl
,MCS_NAME ybtym
,MCS_MATL cz
,'' tz
,REG_FIL_NO zcbah
,MCS_NAME djcpmc
,PRODENTP_NAME hcqy
, CONVERT(int, PAC_CNT) AS zxbzsl
,REGER_NAME zcbar
,null ybxz
,'' pym
from G_yb_mcs_info_b )a where 1=1 ";
					if (!string.IsNullOrEmpty(gjybdm))
					{
						sql +=
							"  and (hcdm like  '%'+@gjybdm+'%' or djcpmc like '%'+@gjybdm+'%' or zcbah like '%'+@gjybdm+'%' or  hcqy like '%'+@gjybdm+'%' or pym like '%'+@gjybdm+'%')";
						pars.Add(new SqlParameter("@gjybdm", gjybdm));
					}

					if (!string.IsNullOrEmpty(pzwh))
					{
						sql += "  and zcbah like '%'+@pzwh+'%'";
						pars.Add(new SqlParameter("@pzwh", pzwh));
					}

					if (!string.IsNullOrEmpty(ypmc))
					{
						sql += "  and hcqy like '%'+@ypmc+'%'";
						pars.Add(new SqlParameter("@ypmc", ypmc));
					}
					
					break;
			}
			
			return this.FindList<G_yb_clxxVO>(sql, pars.ToArray());
		}
		/// <summary>
		/// 保存材料对码
		/// </summary>
		/// <param name="ybxx"></param>
		/// <param name="ypid"></param>
		/// <param name="organizeId"></param>
		/// <returns></returns>
		public int SaveYpcl(G_yb_clxxVO ybxx, int? ypid, string organizeId)
		{
			int updatecount = 0;
			if (ypid.HasValue && ypid.Value > 0)
			{
				using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
				{
					var sfxmEntity = db.IQueryable<SysChargeItemEntity>().FirstOrDefault(p => p.sfxmId == ypid && p.OrganizeId == organizeId);
					
					if (!string.IsNullOrEmpty(ybxx.hcdm) && !string.IsNullOrEmpty(ybxx.djcpmc))
					{
						sfxmEntity.gjybdm = ybxx.hcdm;
						sfxmEntity.gjybmc = ybxx.djcpmc;
					}
					if (!string.IsNullOrEmpty(ybxx.ybxz))
					{
						sfxmEntity.ybbz = "1";
						sfxmEntity.zfxz= EnumZFXZv2.J.GetDescription().Contains(ybxx.ybxz) ? "4" : EnumZFXZv2.Y.GetDescription().Contains(ybxx.ybxz) ? "5" : EnumZFXZv2.B.GetDescription().Contains(ybxx.ybxz) ? "6" : "1";
					}
					if(!string.IsNullOrEmpty(ybxx.zcbah))
					{
						sfxmEntity.pzwh = ybxx.zcbah;
					}
					

					db.Update(sfxmEntity);

					updatecount = db.Commit();
				}

			}

			return updatecount;
		}


		/// <summary>
		/// 查询系统项目信息
		/// </summary>
		/// <param name="organizeId"></param>
		/// <param name="pagination"></param>
		/// <param name="type"></param>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public IList<SysChargeItemEntity> GetxmxxList(string organizeId, Pagination pagination, string type, string keyword = null)
		{
			if (string.IsNullOrWhiteSpace(organizeId))
			{
				return null;
			}
			var sql = @" select * from dbo.xt_sfxm 
             where sfdlcode!='126' and zt='1'  and organizeid=@organizeId  ";
			DbParameter[] par;
			if (!string.IsNullOrEmpty(keyword))
			{
				sql += " and sfxmmc like @keyword ";
				if (type != "qb" && type == "yd")
				{
					sql += "  and gjybdm is not null";
				}
				else if (type != "qb" && type == "wd")
				{
					sql += " and gjybdm is  null";
				}
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId),
					new SqlParameter("@keyword",  "%" + (keyword??"") + "%")
				};
			}
			else
			{
				if (type != "qb" && type == "yd")
				{
					sql += "  and gjybdm is not null";
				}
				else if (type != "qb" && type == "wd")
				{
					sql += " and gjybdm is  null";
				}
				par = new DbParameter[]
				{
					new SqlParameter("@organizeId", organizeId)
				};
			}

			return this.QueryWithPage<SysChargeItemEntity>(sql, pagination, par);
		}
		/// <summary>
		/// 查询医保项目信息 根据系统项目信息
		/// </summary>
		/// <param name="organizeId"></param>
		/// <param name="ypmc"></param>
		/// <param name="py"></param>
		/// <param name="gjybdm"></param>
		/// <param name="pzwh"></param>
		/// <returns></returns>
		public IList<G_yb_xmxxVO> GetYbxmxxlist(string organizeId, string ypmc, string py, string gjybdm, string pzwh,string dataSource)
		{
			string sql = "";
			var pars = new List<SqlParameter>();
			switch (dataSource)
			{
				case "1":
					sql = @"select top 200 * from(
select
结算传输编码 jscsbm
,国家项目代码 gjxmdm
,国家项目名称 gjxmmc
,编码 bm
,项目名称 xmmc
,项目内涵 xmnh
,除外内容 cwnr
,计价单位 jjdw
,说明 sm
,ybxz
,pym
from gb_sfxm)a where 1=1 ";
					// 原视图
					if (!string.IsNullOrEmpty(gjybdm))
					{
						sql +=
							" and(jscsbm like '%'+@gjybdm+'%' or gjxmdm like '%'+@gjybdm+'%' or bm like '%'+@gjybdm+'%' or gjxmmc like '%'+@gjybdm+'%' or xmmc like '%'+@gjybdm+'%' or pym like '%'+@gjybdm+'%')";
						pars.Add(new SqlParameter("@gjybdm", gjybdm));
					}

					break;

				case "2":
					// 医保目录
					sql = @"select top 200 * from(
select
YLML_CODE jscsbm
,'' gjxmdm
,YLFWXM_NAME gjxmmc
,YLML_CODE bm
,YLFWXM_NAME xmmc
,'' xmnh
,'' cwnr
,JJDW jjdw
,JJSM sm
,null ybxz
,'' pym
from G_yb_ylfuxm_new_list_b)a where 1=1 ";
					if (!string.IsNullOrEmpty(gjybdm))
					{
						sql +=
							" and(jscsbm like '%'+@gjybdm+'%' or gjxmdm like '%'+@gjybdm+'%' or bm like '%'+@gjybdm+'%' or gjxmmc like '%'+@gjybdm+'%' or xmmc like '%'+@gjybdm+'%' or pym like '%'+@gjybdm+'%')";
						pars.Add(new SqlParameter("@gjybdm", gjybdm));
					}
					break;
			}

			return this.FindList<G_yb_xmxxVO>(sql, pars.ToArray());
		}

		/// <summary>
		/// 保存项目对码
		/// </summary>
		/// <param name="ybxx"></param>
		/// <param name="ypid"></param>
		/// <param name="organizeId"></param>
		/// <returns></returns>
		public int SaveYpxm(G_yb_xmxxVO ybxx, int? ypid, string organizeId)
		{
			int updatecount = 0;
			if (ypid.HasValue && ypid.Value > 0)
			{
				using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
				{
					var sfxmEntity = db.IQueryable<SysChargeItemEntity>().FirstOrDefault(p => p.sfxmId == ypid && p.OrganizeId == organizeId);

					if (!string.IsNullOrEmpty(ybxx.jscsbm) && !string.IsNullOrEmpty(ybxx.gjxmmc))
					{
						sfxmEntity.gjybdm = ybxx.jscsbm;
						sfxmEntity.gjybmc = ybxx.gjxmmc;
					}
					if (!string.IsNullOrEmpty(ybxx.ybxz))
					{
						sfxmEntity.ybbz = "1";
						sfxmEntity.zfxz = EnumZFXZv2.J.GetDescription().Contains(ybxx.ybxz) ? "4" : EnumZFXZv2.Y.GetDescription().Contains(ybxx.ybxz) ? "5" : EnumZFXZv2.B.GetDescription().Contains(ybxx.ybxz) ? "6" : "1";
					}

					db.Update(sfxmEntity);

					updatecount = db.Commit();
				}

			}

			return updatecount;
		}

		/// <summary>
		/// 获取系统人员信息
		/// </summary>
		/// <param name="keyValue"></param>
		/// <returns></returns>
		public SysMedicineVO GetFormJson(int keyValue)
        {
            const string sql = @"
SELECT TOP 1000 x.ypId, x.zxdw,x.ypCode,x.ypmc,x.spm,x.py,x.bzdw,x.mzcls,x.mzcldw,x.zycls,x.zycldw,x.cxjje,
x.djdw,Convert(decimal(18,4),x.lsj) lsj,Convert(decimal(18,4),x.pfj) pfj,x.zfbl,x.zfxz,x.dlCode,x.ycmc,x.zt,0 bgbz,x.ypbzdm,x.nbdl,x.mzzybz,x.CreateTime,d.pzwh,
d.yptssx,d.ypflCode,d.zlff,d.jzlx,d.mrbzq,d.sjap,d.zbbz,d.yl,d.yldw,d.zjtzsj,d.ghdw,d.ypcd,d.ypgg,d.ybdm,d.xnhybdm,
x.jx,x.jl,x.jldw,x.OrganizeId,x.bzs,x.CreatorCode, c.dlmc sfdlMc, e.jxmc jxmc, f.ypflmc ypflMc ,d.mrjl,d.mrpc,
x.isKss,x.kssId,x.jybz,d.ybbz,x.bz,d.gjybdm,x.tsypbz,d.gjybmc,d.dcxl,d.mbxl,d.mryf,g.yfmc mryfmc,d.ybgg ybgg,kcyjz
FROM xt_yp(NOLOCK) x  
LEFT JOIN xt_ypsx(NOLOCK) d on x.ypId=d.ypId 
LEFT JOIN xt_sfdl(NOLOCK) c ON x.dlCode = c.dlCode and c.OrganizeId = x.OrganizeId
LEFT JOIN xt_ypjx(NOLOCK) e ON x.jx = e.jxCode
LEFT JOIN xt_ypfl(NOLOCK) f ON d.ypflCode = f.ypflCode
LEFT JOIN xt_ypyf(NOLOCK) g ON g.yfCode=d.mryf
WHERE x.ypId =@ypId
";
            DbParameter[] par = {
                new SqlParameter("@ypId", keyValue)
            };
            return FirstOrDefault<SysMedicineVO>(sql, par);
        }

        /// <summary>
        /// 获取系统人员信息
        /// </summary>
        /// <param name="ypCode"></param>
        /// <param name="organizeId"></param>
        /// <returns></returns>
        public SysMedicineVO SelectMedicineInfo(string ypCode, string organizeId)
        {
            const string sql = @"
SELECT TOP 1000 x.ypId, x.zxdw,x.ypCode,x.ypmc,x.spm,x.py,x.bzdw,x.mzcls,x.mzcldw,x.zycls,x.zycldw,
x.djdw,x.lsj,x.pfj,x.zfbl,x.zfxz,x.dlCode,x.ycmc,x.zt,0 bgbz,x.ypbzdm,x.nbdl,x.mzzybz,x.CreateTime,d.pzwh,
d.yptssx,d.ypflCode,d.zlff,d.jzlx,d.mrbzq,d.sjap,d.zbbz,d.yl,d.yldw,d.zjtzsj,d.ghdw,d.ypcd,d.ypgg,d.ybdm,d.xnhybdm,
x.jx,x.jl,x.jldw,x.OrganizeId,x.bzs,x.CreatorCode, c.dlmc sfdlMc, e.jxmc jxmc, f.ypflmc ypflMc ,d.mrjl,d.mrpc,
x.isKss,x.kssId,x.jybz,d.ybbz 
FROM xt_yp(NOLOCK) x  
LEFT JOIN xt_ypsx(NOLOCK) d on x.ypId=d.ypId 
LEFT JOIN xt_sfdl(NOLOCK) c ON x.dlCode = c.dlCode and c.OrganizeId = x.OrganizeId AND c.zt='1'
LEFT JOIN xt_ypjx(NOLOCK) e ON x.jx = e.jxCode AND e.zt='1' 
LEFT JOIN xt_ypfl(NOLOCK) f ON d.ypflCode = f.ypflCode AND f.zt='1' 
WHERE x.ypCode =@ypCode
AND x.OrganizeId=@OrganizeId
";
            DbParameter[] par = {
                new SqlParameter("@ypCode", ypCode),
                new SqlParameter("@OrganizeId", organizeId)
            };
            return FirstOrDefault<SysMedicineVO>(sql, par);
        }

        /// <summary>
        /// 新增药品信息以及药品熟悉信息表
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ypId"></param>
        public void SubmitMedicine(SysMedicineVO model, int? ypId)
        {
            if (ypId.HasValue && ypId.Value > 0)
            {
	            if (!model.OrganizeId.Equals("*"))
	            { model.ypId = ypId.Value;
		            //更新
		            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
		            {
			            var ypEntity = db.IQueryable<SysMedicineEntity>().FirstOrDefault(p => p.ypId == model.ypId);
			            var ypsxEntity = db.IQueryable<SysMedicinePropertyEntity>().FirstOrDefault(p => p.ypId == model.ypId);
			            ypEntity.pfj = model.pfj;
			            var oldypEntity = ypEntity.Clone();
			            var oldypsxEntity = ypsxEntity.Clone();

			            model.OrganizeId = ypEntity.OrganizeId;//组织 机构 不能变
			            UpdateEnityProperties(ypEntity, ypsxEntity, model);

			            ypEntity.Modify();
			            ypsxEntity.Modify();

			            db.Update(ypEntity);
			            db.Update(ypsxEntity);

			            db.Commit();

			            AppLogger.WriteEntityChangeRecordLog(oldypEntity, ypEntity, SysMedicineEntity.GetTableName(), oldypEntity.ypId.ToString());
			            AppLogger.WriteEntityChangeRecordLog(oldypsxEntity, ypsxEntity, SysMedicinePropertyEntity.GetTableName(), oldypsxEntity.ypsxId.ToString());
		            }
		            
	            }
	            else
	            {
		            model.ypId = ypId.Value;
		            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
		            {
			            //系统基础库更新
			            var ypEntity = db.IQueryable<SysMedicineBaseEntity>().FirstOrDefault(p => p.ypId == ypId);
			            var ypsxEntity = db.IQueryable<SysMedicinePropertyBaseEntity>().FirstOrDefault(p => p.ypId == ypId);
			            ypEntity.pfj = model.pfj;
			            var oldypEntity = ypEntity.Clone();
			            var oldypsxEntity = ypsxEntity.Clone();
			            model.OrganizeId = ypEntity.OrganizeId;//组织 机构 不能变
			            UpdateEnityProperties(ypEntity,ypsxEntity, model);
			            ypEntity.Modify();
			            ypsxEntity.Modify();
			            _sysMedicineBaseRepository.Update(ypEntity);
			            _sysMedicinePropertyBaseRepo.Update(ypsxEntity);
			            db.Commit();
			            AppLogger.WriteEntityChangeRecordLog(oldypEntity, ypEntity, SysMedicineBaseEntity.GetTableName(), oldypEntity.ypId.ToString());
			            AppLogger.WriteEntityChangeRecordLog(oldypsxEntity, ypsxEntity, SysMedicinePropertyBaseEntity.GetTableName(), oldypsxEntity.ypsxId.ToString());
		            }
		            
	            }

            }
            else
            {
                //新增
                if (model.OrganizeId.Equals("*"))
                {
	                model.ypId = _sysMedicineBaseRepository.GetBaseLastYpId() + 1;
	                var ypEntity = new SysMedicineBaseEntity();
	                var ypsxEntity = new SysMedicinePropertyBaseEntity();
	                //在系统基础库插入插入药品
	                UpdateEnityBaseProperties(ypEntity, ypsxEntity, model);
	                ypEntity.Create(); ;
	                 _sysMedicineBaseRepository.Insert(ypEntity);
	                //插入药品属性
	                ypsxEntity.ypId = ypEntity.ypId;
	                ypsxEntity.Create();
	                _sysMedicinePropertyBaseRepo.Insert(ypsxEntity);
                }
                else
                {
	                model.ypId = 0;
	                var ypEntity = new SysMedicineEntity();
	                var ypsxEntity = new SysMedicinePropertyEntity();
	                //插入药品
	                UpdateEnityProperties(ypEntity, ypsxEntity, model);
	                ypEntity.Create();
	                _sysMedicineRepository.Insert(ypEntity);
	                //插入药品属性
	                ypsxEntity.ypId = ypEntity.ypId;
	                ypsxEntity.Create();
	                _sysMedicinePropertyRepo.Insert(ypsxEntity);
                }

               
            }
        }
        /// <summary>
        /// 医保同步修改最后上传时间
        /// </summary>
        /// <param name="ypId"></param>
        /// <returns></returns>
        public bool YibaoUpload(int ypId, out string error)
        {
            try
            {
                using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
                {
                    var ypsxEntity = db.IQueryable<SysMedicinePropertyEntity>().Where(p => p.ypId == ypId).FirstOrDefault();
                    if (ypsxEntity == null)
                    {
                        error = "HIS中查无此药";
                        return false;
                    }
                    ypsxEntity.LastYBUploadTime = System.DateTime.Now;
                    ypsxEntity.Modify();
                    db.Update(ypsxEntity);
                    db.Commit();
                }
                error = "";
                return true;
            }
            catch
            {
                error = "HIS更新药品同步医保时间失败";
                return false;
            }
        }

        #region private methods

        /// <summary>
        /// 拼装值
        /// </summary>
        /// <param name="ypEntity"></param>
        /// <param name="ypsxEntity"></param>
        /// <param name="model"></param>
        public void UpdateEnityProperties(SysMedicineEntity modelyp, SysMedicinePropertyEntity modelypsx, SysMedicineVO model)
        {
            #region 药品信息表值
            modelyp.bzdw = model.bzdw;
            modelyp.bzs = model.bzs;
            modelyp.cfdw = "";//默认
            modelyp.cfl = decimal.Parse("0.0000");
            //modelyp.cldw = model.cldw;
            //modelyp.cls = model.cls;
            modelyp.djdw = model.djdw;
            modelyp.dlCode = model.dlCode;
            modelyp.jl = model.jl;
            modelyp.jldw = model.jldw; ;
            modelyp.jx = model.jx;//( or liek ) and bgbz='0' and zt='1' 根据代码/姓名/首拼获取剂型信息获得
            modelyp.lsj = model.lsj;
            modelyp.medextid = 0;//默认
            modelyp.medid = 0;//默认
            modelyp.mzcldw = model.mzcldw;
            modelyp.mzcls = model.mzcls;
            modelyp.mzzybz = "1";//门诊住院标志默认1
            modelyp.nbdl = model.nbdl;
            modelyp.pfj = model.pfj;
            modelyp.px = model.px;
            modelyp.py = model.py;
            modelyp.spm = model.spm;
            modelyp.OrganizeId = model.OrganizeId;    //组织机构Id
            modelyp.ycmc = model.ycmc;
            modelyp.ypbzdm = model.ypbzdm;//药品包装代码默认3
            modelyp.bz = model.bz;
            modelyp.cxjje = model.cxjje;//超限价金额
            modelyp.ypId = model.ypId;
            modelyp.ypCode = model.ypCode;
            modelyp.ypmc = model.ypmc;
            modelyp.yphz = "";//默认
            modelyp.ypqz = "";//默认
            modelyp.zfbl = model.zfbl;
            modelyp.zfxz = model.zfxz;
            modelyp.zt = model.zt;
            modelyp.zycldw = model.zycldw;
            modelyp.zycls = model.zycls;
            modelyp.zxdw = model.zxdw;
            modelyp.isKss = model.isKss;
            modelyp.kssId = model.kssId;
            modelyp.jybz = model.jybz;
			modelyp.tsypbz = model.tsypbz;
			#endregion

			#region 药品SX值
			modelypsx.OrganizeId = model.OrganizeId;    //组织机构Id
            modelypsx.dczdjl = model.dczdjl;
            modelypsx.dczdsl = model.dczdsl;
            modelypsx.ghdw = model.ghdw;
            modelypsx.gzy = model.gzy;
            modelypsx.jsbz = model.jsbz;
            modelypsx.jzlx = model.jzlx;
            modelypsx.ljzdjl = model.ljzdjl;
            modelypsx.ljzdsl = model.ljzdsl;
            modelypsx.mrbzq = model.mrbzq;
            modelypsx.mzy = model.mzy;
            //modelypsx.px = model.px;
            modelypsx.pzwh = model.pzwh;
            modelypsx.shbz = model.shbz;
            modelypsx.sjap = model.sjap;
            modelypsx.syts = model.syts;
            modelypsx.tsbz = model.tsbz;
            modelypsx.xglx = model.xglx;
            modelypsx.ybdm = model.ybdm;
            modelypsx.xnhybdm = model.xnhybdm;
            modelypsx.yl = model.yl;
            modelypsx.yldw = model.yldw;
            modelypsx.yljsy = model.yljsy;
            modelypsx.ypcd = model.ypcd;
            modelypsx.ypCode = model.ypCode;
            modelypsx.ypflCode = model.ypflCode;
            modelypsx.ypgg = model.ypgg;
            modelypsx.ypId = model.ypId;
            modelypsx.yptssx = model.yptssx;
            modelypsx.zbbz = model.zbbz;
            modelypsx.zjtzsj = model.zjtzsj;
            modelypsx.zlff = model.zlff;
            modelypsx.mrjl = model.mrjl;
            modelypsx.mrpc = model.mrpc;
            modelypsx.ybbz = model.ybbz;
            modelypsx.gjybdm = model.gjybdm;
            modelypsx.gjybmc = model.gjybmc;
            modelypsx.dcxl = model.dcxl;
            modelypsx.mbxl = model.mbxl;
            modelypsx.mryf = model.mryf;
            modelypsx.ybgg = model.ybgg;
            modelypsx.kcyjz = model.kcyjz;
            //modelypsx.zt = model.zt;

            #endregion
        }
        
         public void UpdateEnityBaseProperties(SysMedicineBaseEntity modelyp, SysMedicinePropertyBaseEntity modelypsx, SysMedicineVO model)
        {
            #region 药品信息表值
            modelyp.bzdw = model.bzdw;
            modelyp.bzs = model.bzs;
            modelyp.cfdw = "";//默认
            modelyp.cfl = decimal.Parse("0.0000");
            //modelyp.cldw = model.cldw;
            //modelyp.cls = model.cls;
            modelyp.djdw = model.djdw;
            modelyp.dlCode = model.dlCode;
            modelyp.jl = model.jl;
            modelyp.jldw = model.jldw; ;
            modelyp.jx = model.jx;//( or liek ) and bgbz='0' and zt='1' 根据代码/姓名/首拼获取剂型信息获得
            modelyp.lsj = model.lsj;
            modelyp.medextid = 0;//默认
            modelyp.medid = 0;//默认
            modelyp.mzcldw = model.mzcldw;
            modelyp.mzcls = model.mzcls;
            modelyp.mzzybz = "1";//门诊住院标志默认1
            modelyp.nbdl = model.nbdl;
            modelyp.pfj = model.pfj;
            modelyp.px = model.px;
            modelyp.py = model.py;
            modelyp.spm = model.spm;
            modelyp.OrganizeId = model.OrganizeId;    //组织机构Id
            modelyp.ycmc = model.ycmc;
            modelyp.ypbzdm = model.ypbzdm;//药品包装代码默认3
            modelyp.bz = model.bz;
            modelyp.cxjje = model.cxjje;//超限价金额
            modelyp.ypId = model.ypId;
            modelyp.ypCode = model.ypCode;
            modelyp.ypmc = model.ypmc;
            modelyp.yphz = "";//默认
            modelyp.ypqz = "";//默认
            modelyp.zfbl = model.zfbl;
            modelyp.zfxz = model.zfxz;
            modelyp.zt = model.zt;
            modelyp.zycldw = model.zycldw;
            modelyp.zycls = model.zycls;
            modelyp.zxdw = model.zxdw;
            modelyp.isKss = model.isKss;
            modelyp.kssId = model.kssId;
            modelyp.jybz = model.jybz;
			modelyp.tsypbz = model.tsypbz;
			#endregion

			#region 药品SX值
			modelypsx.OrganizeId = model.OrganizeId;    //组织机构Id
            modelypsx.dczdjl = model.dczdjl;
            modelypsx.dczdsl = model.dczdsl;
            modelypsx.ghdw = model.ghdw;
            modelypsx.gzy = model.gzy;
            modelypsx.jsbz = model.jsbz;
            modelypsx.jzlx = model.jzlx;
            modelypsx.ljzdjl = model.ljzdjl;
            modelypsx.ljzdsl = model.ljzdsl;
            modelypsx.mrbzq = model.mrbzq;
            modelypsx.mzy = model.mzy;
            //modelypsx.px = model.px;
            modelypsx.pzwh = model.pzwh;
            modelypsx.shbz = model.shbz;
            modelypsx.sjap = model.sjap;
            modelypsx.syts = model.syts;
            modelypsx.tsbz = model.tsbz;
            modelypsx.xglx = model.xglx;
            modelypsx.ybdm = model.ybdm;
            modelypsx.xnhybdm = model.xnhybdm;
            modelypsx.yl = model.yl;
            modelypsx.yldw = model.yldw;
            modelypsx.yljsy = model.yljsy;
            modelypsx.ypcd = model.ypcd;
            modelypsx.ypCode = model.ypCode;
            modelypsx.ypflCode = model.ypflCode;
            modelypsx.ypgg = model.ypgg;
            modelypsx.ypId = model.ypId;
            modelypsx.yptssx = model.yptssx;
            modelypsx.zbbz = model.zbbz;
            modelypsx.zjtzsj = model.zjtzsj;
            modelypsx.zlff = model.zlff;
            modelypsx.mrjl = model.mrjl;
            modelypsx.mrpc = model.mrpc;
            modelypsx.ybbz = model.ybbz;
            modelypsx.gjybdm = model.gjybdm;
            modelypsx.gjybmc = model.gjybmc;
            modelypsx.dcxl = model.dcxl;
            modelypsx.mbxl = model.mbxl;
            modelypsx.mryf = model.mryf;
            modelypsx.ybgg = model.ybgg;
            //modelypsx.zt = model.zt;

            #endregion
        }
        
        

		#endregion

		/// <summary>
		/// 获取药品系数
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string GetTradePricePlus(string name)
		{
			string sql = @"select top 1 code from [dbo].[Sys_ItemsDetail] where name=@name ";
			DbParameter[] par = {
				new SqlParameter("@name", name)
			};
			return FirstOrDefault<string>(sql, par);
		}


        /// <summary>
        /// 根据查询药品用法
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public IList<SysMedicineFormulationEntity> GetValidList(string keyword = null)
        {
            var sql = @"select * from xt_ypjx(nolock) where zt = '1' and (jxCode like @searchKeyword or jxmc like @searchKeyword or py like @searchKeyword) order by py asc";
            return this.FindList<SysMedicineFormulationEntity>(sql, new SqlParameter[] {
                new SqlParameter("@searchKeyword", "%" + (keyword ?? "") + "%")
            });
        }

        //药品用法
        public IList<SysMedicineUsageEntity> GetUsageFloat(string keyword)
        {
            var sql = @"select * from xt_ypyf(nolock) where zt = '1' and (yfCode like @searchKeyword or yfmc like @searchKeyword or py like @searchKeyword) order by py asc";
            return this.FindList<SysMedicineUsageEntity>(sql, new SqlParameter[] {
                new SqlParameter("@searchKeyword", "%" + (keyword ?? "") + "%")
            });
        }
         public void UpdateEnityProperties(SysMedicineBaseEntity modelyp, SysMedicinePropertyBaseEntity modelypsx, SysMedicineVO model)
        {
            #region 药品信息表值
            modelyp.bzdw = model.bzdw;
            modelyp.bzs = model.bzs;
            modelyp.cfdw = "";//默认
            modelyp.cfl = decimal.Parse("0.0000");
            //modelyp.cldw = model.cldw;
            //modelyp.cls = model.cls;
            modelyp.djdw = model.djdw;
            modelyp.dlCode = model.dlCode;
            modelyp.jl = model.jl;
            modelyp.jldw = model.jldw; ;
            modelyp.jx = model.jx;//( or liek ) and bgbz='0' and zt='1' 根据代码/姓名/首拼获取剂型信息获得
            modelyp.lsj = model.lsj;
            modelyp.medextid = 0;//默认
            modelyp.medid = 0;//默认
            modelyp.mzcldw = model.mzcldw;
            modelyp.mzcls = model.mzcls;
            modelyp.mzzybz = "1";//门诊住院标志默认1
            modelyp.nbdl = model.nbdl;
            modelyp.pfj = model.pfj;
            modelyp.px = model.px;
            modelyp.py = model.py;
            modelyp.spm = model.spm;
            modelyp.OrganizeId = model.OrganizeId;    //组织机构Id
            modelyp.ycmc = model.ycmc;
            modelyp.ypbzdm = model.ypbzdm;//药品包装代码默认3
            modelyp.bz = model.bz;
            modelyp.cxjje = model.cxjje;//超限价金额
            modelyp.ypId = model.ypId;
            modelyp.ypCode = model.ypCode;
            modelyp.ypmc = model.ypmc;
            modelyp.yphz = "";//默认
            modelyp.ypqz = "";//默认
            modelyp.zfbl = model.zfbl;
            modelyp.zfxz = model.zfxz;
            modelyp.zt = model.zt;
            modelyp.zycldw = model.zycldw;
            modelyp.zycls = model.zycls;
            modelyp.zxdw = model.zxdw;
            modelyp.isKss = model.isKss;
            modelyp.kssId = model.kssId;
            modelyp.jybz = model.jybz;
			modelyp.tsypbz = model.tsypbz;
			#endregion

			#region 药品SX值
			modelypsx.OrganizeId = model.OrganizeId;    //组织机构Id
            modelypsx.dczdjl = model.dczdjl;
            modelypsx.dczdsl = model.dczdsl;
            modelypsx.ghdw = model.ghdw;
            modelypsx.gzy = model.gzy;
            modelypsx.jsbz = model.jsbz;
            modelypsx.jzlx = model.jzlx;
            modelypsx.ljzdjl = model.ljzdjl;
            modelypsx.ljzdsl = model.ljzdsl;
            modelypsx.mrbzq = model.mrbzq;
            modelypsx.mzy = model.mzy;
            //modelypsx.px = model.px;
            modelypsx.pzwh = model.pzwh;
            modelypsx.shbz = model.shbz;
            modelypsx.sjap = model.sjap;
            modelypsx.syts = model.syts;
            modelypsx.tsbz = model.tsbz;
            modelypsx.xglx = model.xglx;
            modelypsx.ybdm = model.ybdm;
            modelypsx.xnhybdm = model.xnhybdm;
            modelypsx.yl = model.yl;
            modelypsx.yldw = model.yldw;
            modelypsx.yljsy = model.yljsy;
            modelypsx.ypcd = model.ypcd;
            modelypsx.ypCode = model.ypCode;
            modelypsx.ypflCode = model.ypflCode;
            modelypsx.ypgg = model.ypgg;
            modelypsx.ypId = model.ypId;
            modelypsx.yptssx = model.yptssx;
            modelypsx.zbbz = model.zbbz;
            modelypsx.zjtzsj = model.zjtzsj;
            modelypsx.zlff = model.zlff;
            modelypsx.mrjl = model.mrjl;
            modelypsx.mrpc = model.mrpc;
            modelypsx.ybbz = model.ybbz;
            modelypsx.gjybdm = model.gjybdm;
            modelypsx.gjybmc = model.gjybmc;
            modelypsx.dcxl = model.dcxl;
            modelypsx.mbxl = model.mbxl;
            modelypsx.mryf = model.mryf;
            modelypsx.ybgg = model.ybgg;
            modelypsx.kcyjz = model.kcyjz;
            //modelypsx.zt = model.zt;

            #endregion
        }


    }
}
