using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using FrameworkBase.MultiOrg.DmnService;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Interface;
using Newtouch.Domain.Entity;
using Newtouch.Domain.IDomainServices;
using Newtouch.Domain.ValueObjects;
using Newtouch.Domain.ViewModels;
using Newtouch.Infrastructure;

namespace Newtouch.DomainServices
{
	public class BaseDataDmnService : DmnServiceBase, IBaseDataDmnService
	{
		private readonly ICache _cache;
		private readonly ISysConfigRepo _sysConfigRepo;

		public BaseDataDmnService(IDefaultDatabaseFactory databaseFactory, ICache cache) : base(databaseFactory)
		{
			this._cache = cache;
		}

		/// <summary>
		/// 药品用法 检索
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public IList<SysMedicineUsageVEntity> GetMedicineUsageList()
		{
			//_cache.Remove(string.Format(Infrastructure.CacheKey.ValidSysMedicineUsageListSetKey));
			return _cache.Get(Infrastructure.CacheKey.ValidSysMedicineUsageListSetKey, () =>
			{
				var sql = @"
SELECT yfId,yfCode,yfmc,yplx,zt
  FROM [NewtouchHIS_Base]..[V_S_xt_ypyf]
WHERE zt='1'
                        ";
				return this.FindList<SysMedicineUsageVEntity>(sql);
			});
		}
        /// <summary>
		/// 药品用法 检索
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public IList<SysAuxiliaryDictionaryEntity> GetDjfsUsageList(string OrganizeId)
        {
          var sql = @"select * from xt_fzcd where parentId is not null and OrganizeId='" + OrganizeId + @"'";
         return this.FindList<SysAuxiliaryDictionaryEntity>(sql);
        }
        /// <summary>
        /// 获取中药代煎费
        /// </summary>
        /// <returns></returns>
        public decimal? GetDjFee(string orgId)
		{
            var sfxmCode = _sysConfigRepo.GetValueByCode("TCM_DaiJianCode", orgId);
            if (string.IsNullOrWhiteSpace(sfxmCode))
            {
                return 0;
            }
            var sql = @"
            SELECT dj
              FROM[NewtouchHIS_Base]..[V_S_xt_sfxm]
            WHERE zt = '1' and organizeId = @orgId and sfxmCode = @sfxmCode";
            decimal? dj=this.FirstOrDefault<decimal?>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@sfxmCode", sfxmCode) });
            return dj;
            //            return _cache.Get(string.Format(CacheKey.ValidSysDaiJianFeeSetKey, orgId), () =>
            //			{

            //				var sql = @"
            //SELECT dj
            //  FROM[NewtouchHIS_Base]..[V_S_xt_sfxm]
            //WHERE zt = '1' and organizeId = @orgId and sfxmCode = @sfxmCode";
            //				return this.FirstOrDefault<decimal?>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@sfxmCode", sfxmCode) });

            //			});
        }

        public IList<GetSysBodyPartsVO> GetSysBodyBwFl(string orgId)
        {
            var sql = @"
                select distinct bwflmc from xt_bw(nolock) where OrganizeId=@orgId and bwflmc is not null";
            return this.FindList<GetSysBodyPartsVO>(sql, new[] { new SqlParameter("@orgId", orgId) });
        }

        public IList<GetSysBodyPartsVO> GetSysBodyJcBwFl(string orgId)
        {
            var sql = @"
                select distinct bwflmc from pacs_jcbw(nolock) where OrganizeId=@orgId and bwflmc is not null";
            return this.FindList<GetSysBodyPartsVO>(sql, new[] { new SqlParameter("@orgId", orgId) });
        }

        /// <summary>
        /// 获取系统部位
        /// </summary>
        /// <returns></returns>
        public IList<GetSysBodyPartsVO> GetSysBodyParts(string orgId)
		{
			//_cache.Remove(string.Format(Infrastructure.CacheKey.ValidSysBodyPartsSetKey_));
			//return _cache.Get(string.Format(Infrastructure.CacheKey.ValidSysBodyPartsSetKey_, orgId), () =>
			//{
				
			//});
            var sql = @"
select  isnull(bwCode,'') bwCode,isnull(bwmc,'') bwmc,isnull(bwflCode,'') bwflCode,isnull(bwflmc,'') bwflmc from xt_bw where OrganizeId=@orgId";
            return this.FindList<GetSysBodyPartsVO>(sql, new[] { new SqlParameter("@orgId", orgId) });
        }

		/// <summary>
		/// 获取影像部位
		/// </summary>
		/// <returns></returns>
		public IList<GetYxBodyPartsVO> GetYxBodyParts(string orgId)
		{
			var sql = @"
select DISTINCT jcbw,isnull(bwflCode,'') bwflCode,isnull(bwflmc,'') bwflmc from pacs_jcbw where OrganizeId=@orgId";
			return this.FindList<GetYxBodyPartsVO>(sql, new[] { new SqlParameter("@orgId", orgId) });
		}

		/// <summary>
		/// 获取影像方法
		/// </summary>
		/// <returns></returns>
		public IList<string> GetYxBodyMethod(string orgId, string jcbw)
		{
			var sql = @"SELECT DISTINCT
                       jcff
                FROM pacs_jcbw
                WHERE OrganizeId = @orgId
                      AND jcbw IN (
                      SELECT * FROM dbo.f_split(@jcbw, ','))";
			return this.FindList<string>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@jcbw", jcbw) });
		}

        public IList<GetSysJcBodyPartsVo> GetJcListByOrg(string orgId)
        {
            var sql = @"
                        select Id,OrganizeId,isnull(bwflCode,'') bwflCode,isnull(bwflmc,'') bwflmc,isnull(jcbwCode,'') bwCode,isnull(jcbw,'') bwmc,jcff,Createtime,CreatorCode,LastModifierCode,
                LastModifyTime,zt from pacs_jcbw where OrganizeId=@orgId";
            return this.FindList<GetSysJcBodyPartsVo>(sql, new[] { new SqlParameter("@orgId", orgId) });
        }

        /// <summary>
        /// 获取系统嘱托
        /// </summary>
        /// <returns></returns>
        public IList<SysDoctorRemarkVO2> GetSysDoctorRemark(string orgId, string ksCode)
		{
			var sql = @"
select ztCode,ztmc from xt_zt where OrganizeId=@orgId and ksCode=@ksCode and zt = '1'";
			return this.FindList<SysDoctorRemarkVO2>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@ksCode", ksCode) });
		}

		/// <summary>
		/// 获取
		/// </summary>
		/// <returns></returns>
		public string GetDoctorDepartCode(string orgId, string ysgh)
		{
			string sql = @"SELECT DepartmentCode FROM NewtouchHIS_Base..V_S_Sys_Staff WITH(NOLOCK) WHERE zt='1' AND OrganizeId=@orgId AND gh=@ysgh ";
			return this.FirstOrDefault<string>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@ysgh", ysgh) });
		}

		/// <summary>
		/// 通过字典代码获取子项数据
		/// </summary>
		/// <param name="code"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public IList<SysItemsDetailVEntity> GetItemsDetailInfo(string code, string orgId = null)
		{
			var sql = @"select Id ,OrganizeId ,TopOrganizeId ,ItemId , Name ,Code ,px ,zt 
                        from NewtouchHIS_Base..Sys_ItemsDetail
                        where zt = '1'
                            and (isnull(@orgId,'') = '' or OrganizeId = '*' or OrganizeId = @orgId)
                            and ItemId in (
                            select Id from NewtouchHIS_Base..Sys_Items where zt = '1' and Code = @code )";
			return this.FindList<SysItemsDetailVEntity>(sql, new SqlParameter[] {
				new SqlParameter("@code", code),
				new SqlParameter("@orgId", orgId ?? "")
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="mzh"></param>
		/// <param name="zyh"></param>
		/// <param name="brxzCode"></param>
		/// <param name="brxzmc"></param>
		public void UpdatebrxzInfo(string orgId, string mzh, string zyh, string brxzCode, string brxzmc)
		{
			if (string.IsNullOrWhiteSpace(orgId) || (string.IsNullOrWhiteSpace(mzh) && string.IsNullOrWhiteSpace(zyh)))
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(brxzCode) || string.IsNullOrWhiteSpace(brxzmc))
			{
				return;
			}

			if (!string.IsNullOrWhiteSpace(mzh))
			{
				var sql = @"update xt_jz set brxzCode=@brxzCode,brxzmc=@brxzmc where OrganizeId=@orgId and mzh=@mzh";
				ExecuteSqlCommand(sql, new SqlParameter("@brxzCode", brxzCode), new SqlParameter("@brxzmc", brxzmc), new SqlParameter("@orgId", orgId), new SqlParameter("@mzh", mzh));
			}

			if (string.IsNullOrWhiteSpace(zyh)) return;
			var zysql = @"update zy_brxxk set brxzdm=@brxzCode,brxzmc=@brxzmc where OrganizeId=@orgId and zyh=@zyh";
			ExecuteSqlCommand(zysql, new SqlParameter("@brxzCode", brxzCode), new SqlParameter("@brxzmc", brxzmc), new SqlParameter("@orgId", orgId), new SqlParameter("@zyh", zyh));
		}

		/// <summary>
		/// 获取病人基本信息
		/// </summary>
		/// <param name="blh">病历号</param>
		/// <param name="mzh">门诊号</param>
		/// <param name="organizeId"></param>
		/// <returns></returns>
		public List<SysPatientBasicInfoVO> SelectXtBrjbxx(string blh, string mzh, string organizeId)
		{
			const string sql = @"
SELECT brxx.*, brxz.brxzmc, k.CardNo, k.CardType, k.CardTypeName
FROM NewtouchHIS_Sett.dbo.xt_brjbxx(NOLOCK) brxx 
LEFT JOIN NewtouchHIS_Sett.dbo.mz_gh(NOLOCK) gh ON gh.mzh=@mzh AND gh.OrganizeId=brxx.OrganizeId AND gh.zt='1' AND gh.blh=brxx.blh
LEFT JOIN NewtouchHIS_Sett.dbo.xt_brxz(NOLOCK) brxz ON brxz.brxz=gh.brxz AND brxz.zt='1' AND brxz.OrganizeId=brxx.OrganizeId
LEFT JOIN NewtouchHIS_Sett.dbo.xt_card(NOLOCK) k ON k.patid=gh.patid AND k.OrganizeId=brxx.OrganizeId AND k.zt='1'
WHERE brxx.blh=@blh AND brxx.zt='1' AND brxx.OrganizeId=@OrganizeId
";
			var param = new DbParameter[]
			{
				new SqlParameter("@blh",blh ),
				new SqlParameter("@mzh",mzh ),
				new SqlParameter("@OrganizeId", organizeId)
			};
			return FindList<SysPatientBasicInfoVO>(sql, param);
		}

		/// <summary>
		/// 获取病人卡信息
		/// </summary>
		/// <param name="blh"></param>
		/// <param name="organizeId"></param>
		/// <returns></returns>
		public List<SysPatientCardDetail> SelectCardDetail(string blh, string organizeId)
		{
			const string sql = @"
SELECT k.* FROM NewtouchHIS_Sett.dbo.xt_card(NOLOCK) k 
INNER JOIN NewtouchHIS_Sett.dbo.xt_brjbxx(NOLOCK) brxx ON brxx.patid=k.patid AND brxx.OrganizeId=k.OrganizeId AND brxx.zt='1'
WHERE k.OrganizeId=@OrganizeId
AND k.zt='1'
AND brxx.blh=@blh
";
			var param = new DbParameter[]
			{
				new SqlParameter("@OrganizeId", organizeId),
				new SqlParameter("@blh", blh)
			};
			return FindList<SysPatientCardDetail>(sql, param);
		}

		public List<SysDeptWardRelVO> GetDeptWardRel(string orgId, string ks, string bq)
		{
			string sql = @"select a.DepartmentCode,b.[Name] DepartmentName,a.bqCode,c.bqmc
from [NewtouchHIS_Base].[dbo].[Sys_DepartmentWardRelation] a with(nolock)
left join NewtouchHIS_Base.dbo.sys_department b with(nolock) on a.DepartmentId=b.id and a.OrganizeId=b.OrganizeId and b.zt='1'
left join NewtouchHIS_Base.dbo.xt_bq c with(nolock) on a.bqCode=c.bqCode and a.OrganizeId=c.OrganizeId and c.zt='1'
where a.OrganizeId=@orgId and a.zt='1'";
			if (!string.IsNullOrWhiteSpace(ks))
			{
				sql += " and a.DepartmentCode=@ks ";
			}
			if (!string.IsNullOrWhiteSpace(bq))
			{
				sql += " and a.bqCode=@bq ";
			}

			return FindList<SysDeptWardRelVO>(sql, new SqlParameter[] {
				new SqlParameter("@orgId",orgId),
				new SqlParameter("@ks",ks??""),
				new SqlParameter("@bq",bq??"")
			});
		}

		/// <summary>
		/// 获取医生开药权限
		/// </summary>
		/// <param name="orgid"></param>
		/// <param name="gh">工号</param>
		/// <param name="tsypzl">药品种类</param>
		/// <param name="qxMc">权限名称</param>
		/// <returns></returns>
		public int GetPermissions(string orgid, string gh, string tsypzl, string dlcode, string kssqxjb)
		{
			var code = 1;
			string sqls = @"select count(*) from [Newtouch_CIS]..[Sys_Config] where OrganizeId=@orgId and code = 'DocPresAuthCtrl' and Value = '1'";//查看是否具有开药的权限,此表的value 0为未开启此权限，1为开启
			var tt = this.FirstOrDefault<int>(sqls, new SqlParameter[] { new SqlParameter("@orgId", orgid) });
			if (tt == 0) //如未在医生站开启开药权限，则所有药品均可开立
			{
				return code;
			}
			if (!string.IsNullOrEmpty(tsypzl))
			{
				string sql = @"select COUNT(a.qxId) from  NewtouchHIS_Base.dbo.xt_qxkz a 
left join  NewtouchHIS_Base.dbo.xt_qxkz_rel b on a.OrganizeId=b.OrganizeId and a.qxId=b.qxId
where a.OrganizeId=@orgid and b.gh=@gh and a.rel_value=@ypzl and a.rel_lxcode=1 and a.zt=1 and b.zt=1
";
				var parm = new SqlParameter[] {
				new SqlParameter("@orgId",orgid),
				new SqlParameter("@gh",gh),
				new SqlParameter("@ypzl",tsypzl),
			};
				var tsbz = this.FirstOrDefault<int>(sql, parm);//获取是否具有此类药物的特殊药品标志的开立权限,如大于0则有权限，等于0 则无权限
				if (tsbz == 0)
				{
					code = -1;
				}
			}
			if (!string.IsNullOrEmpty(kssqxjb))
			{
				string sqlqxjb = @"select COUNT(a.qxId) from  NewtouchHIS_Base.dbo.xt_qxkz a 
left join  NewtouchHIS_Base.dbo.xt_qxkz_rel b on a.OrganizeId=b.OrganizeId and a.qxId=b.qxId
where a.OrganizeId=@orgid and b.gh=@gh and a.rel_value=2 and a.rel_lxcode=3 and a.zt=1 and b.zt=1
";
				var parm1 = new SqlParameter[] {
				new SqlParameter("@orgId",orgid),
				new SqlParameter("@gh",gh),
				new SqlParameter("@ksscode",kssqxjb),
			};
				var qxjb = this.FirstOrDefault<int>(sqlqxjb, parm1);//获取是否具有此类药物的抗生素开立权限,如大于0则有权限，等于0 则无权限
				if (qxjb == 0)//该医生不具备抗生素非限制使用药物权限，则进行下一步判断
				{
					string sql = @"select COUNT(a.qxId) from  NewtouchHIS_Base.dbo.xt_qxkz a 
left join  NewtouchHIS_Base.dbo.xt_qxkz_rel b on a.OrganizeId=b.OrganizeId and a.qxId=b.qxId
where a.OrganizeId=@orgid and b.gh=@gh and a.rel_value=@ksscode and a.rel_lxcode=3 and a.zt=1 and b.zt=1
";
				var parm2 = new SqlParameter[] {
				new SqlParameter("@orgId",orgid),
				new SqlParameter("@gh",gh),
				new SqlParameter("@ksscode",kssqxjb),
			};
					var kss = this.FirstOrDefault<int>(sql, parm2);//获取是否具有此类药物的抗生素开立权限,如大于0则有权限，等于0 则无权限
					if (kss == 0)
					{
						code = -2;
					}
				}
			}
			if (!string.IsNullOrEmpty(dlcode))
			{
				string sql = @"select COUNT(a.qxId) from  NewtouchHIS_Base.dbo.xt_qxkz a 
left join  NewtouchHIS_Base.dbo.xt_qxkz_rel b on a.OrganizeId=b.OrganizeId and a.qxId=b.qxId
where a.OrganizeId=@orgid and b.gh=@gh and a.rel_value=@dlcode and a.rel_lxcode=2 and a.zt=1 and b.zt=1
";
				var parm = new SqlParameter[] {
				new SqlParameter("@orgId",orgid),
				new SqlParameter("@gh",gh),
				new SqlParameter("@dlcode",dlcode),
			};
				var ypdl= this.FirstOrDefault<int>(sql, parm);//获取是否具有此类药物的大类名称开立权限,如大于0则有权限，等于0 则无权限
				if (ypdl == 0)
				{
					code = -3;
				}
			}
			return code;
		}

	}
}
