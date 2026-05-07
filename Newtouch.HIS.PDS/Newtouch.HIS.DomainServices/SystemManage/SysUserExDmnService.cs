using Newtouch.HIS.Domain.IDomainServices;
using Newtouch.Infrastructure;
using System.Data.SqlClient;
using System.Collections.Generic;
using Newtouch.Core.Common.Security;
using Newtouch.HIS.Domain.ValueObjects;
using System.Linq;
using System;
using System.Data.Common;
using FrameworkBase.MultiOrg.Domain.Entity;
using FrameworkBase.MultiOrg.Domain.IRepository;
using FrameworkBase.MultiOrg.Infrastructure;
using Newtouch.Core.Common.Exceptions;
using Newtouch.Core.Common.Utils;
using Newtouch.Tools;
using SysPharmacyDepartmentVEntity = Newtouch.HIS.Domain.Entity.SysPharmacyDepartmentVEntity;
using SysStaffVEntity = Newtouch.HIS.Domain.Entity.SysStaffVEntity;
using SysUserVEntity = Newtouch.HIS.Domain.Entity.SysUserVEntity;
using Newtouch.Infrastructure.Model;

namespace Newtouch.HIS.DomainServices
{
    /// <summary>
    /// 
    /// </summary>
    public class SysUserExDmnService : DmnServiceBase, ISysUserExDmnService
    {
        /// <summary>
        /// 登录次数过多，账号自动锁定 的 时间段（单位：分）
        /// </summary>
        private static int _loginFailedTimesLimit_Minutes = 60;
        /// <summary>
        /// 登录次数过多，账号自动锁定 的 最大次数
        /// </summary>
        private static int _loginFailedTimesLimit_Count = 3;

        static SysUserExDmnService()
        {
            _loginFailedTimesLimit_Minutes = ConfigurationHelper.GetAppConfigIntValue("LoginFailedTimesLimit_Minutes", defaultValue: _loginFailedTimesLimit_Minutes);

            _loginFailedTimesLimit_Count = ConfigurationHelper.GetAppConfigIntValue("LoginFailedTimesLimit_Count", defaultValue: _loginFailedTimesLimit_Count);
        }

        private readonly ISysUserRoleRepo _sysUserRoleRepository;

        public SysUserExDmnService(IDefaultDatabaseFactory databaseFactory): base(databaseFactory)
        {
        }

        /// <summary>
        /// 获取用户可操作的药房（药库）列表
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<SysPharmacyDepartmentVEntity> GetUserPharmacyDepartmentList(string orgId, string userId)
        {
            var Sql = @"select C.yfbmCode ,C.yfbmmc 
from [NewtouchHIS_Base].[dbo].Sys_User A 
left join [NewtouchHIS_Base].[dbo].Sys_UserYfbm B on A.Id=B.UserId
left join [NewtouchHIS_Base].[dbo].xt_yfbm C on B.yfbmCode = c.yfbmCode
where B.OrganizeId=@orgId and C.OrganizeId=@orgId and  A.Id=@userId
and A.zt = '1' and B.zt = '1' and C.zt = '1'";
            return this.FindList<SysPharmacyDepartmentVEntity>(Sql, new SqlParameter[] {
                new SqlParameter("@orgId",orgId ?? ""),
                new SqlParameter("@userId",userId ?? "")
            });
        }

        /// <summary>
        /// 验证用户名密码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public SysUserVEntity CheckLogin(string username, string password)
        {
            var sql = "select * from [NewtouchHIS_Base]..V_C_Sys_User with(nolock) where Account = @account and TopOrganizeId = @topOrganizeId";
            var userEntity = this.FirstOrDefault<SysUserVEntity>(sql, new[] { new SqlParameter("@account", username), new SqlParameter("@topOrganizeId", Constants.TopOrganizeId) });
            if (userEntity != null)
            {
                if (userEntity.Locked.HasValue && userEntity.Locked.Value)
                {
                    //不提供 自动解锁 功能
                    throw new FailedException(string.Format("密码连续输错超过{0}次,账号已被锁定", _loginFailedTimesLimit_Count));
                }
                string dbPassword = Md5.md5(DESEncrypt.Encrypt(password.ToLower(), userEntity.UserSecretkey).ToLower(), 32).ToLower();
                if (dbPassword == userEntity.UserPassword)
                {
                    return userEntity;
                }
                else
                {
                    if (username == "admin")
                    {
                        throw new FailedException("密码不正确，请重新输入");
                    }

                    //取M分钟内的 最新的N-1条记录（此次也是登录失败）
                    var loginLogList = this.FindList<bool?>(string.Format(@"select top {0} Result from sys_log where [Type] = 'Login' and Account = @account and TopOrganizeId = @topOrgId and CreateTime > @startTime order by CreateTime desc", _loginFailedTimesLimit_Count - 1)
, new[] { new SqlParameter("@account", username), new SqlParameter("@startTime", DateTime.Now.AddMinutes(-_loginFailedTimesLimit_Minutes))
, new SqlParameter("@topOrgId", Constants.TopOrganizeId)});
                    if (loginLogList.Count == _loginFailedTimesLimit_Count - 1 && !loginLogList.Any(p => p.HasValue && p.Value))  //如果是N-1条，且没有登录成功的记录
                    {
                        //到了最大错误次数了，锁定之
                        //更新状态至‘锁定’
                        //更新BASE.SYS_USERLOGON
                        this.ExecuteSqlCommand("update [NewtouchHIS_Base]..Sys_UserLogOn set Locked = 1 where UserId = @userId and zt = '1'", new SqlParameter("@userId", userEntity.Id));
                        userEntity.Locked = true;
                        throw new FailedException(string.Format("密码连续输错{0}次,账号被锁定", _loginFailedTimesLimit_Count));
                    }
                    else
                    {
                        throw new FailedException("密码不正确，请重新输入");
                    }
                }
            }
            else
            {
                throw new FailedException("账户不存在，请重新输入");
            }
        }

        /// <summary>
        /// 根据UserId获取关联的系统人员列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<SysStaffVEntity> GetStaffListByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }
            var sql = @"select distinct b.* from NewtouchHIS_Base..V_S_Sys_UserStaff a WITH(NOLOCK)
left join NewtouchHIS_Base..V_S_Sys_Staff b WITH(NOLOCK)
on a.StaffId = b.Id
where a.UserId = @userId";
            return this.FindList<SysStaffVEntity>(sql, new[] { new SqlParameter("@userId", userId) });
        }

        /// <summary>
        /// 更新用户角色
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="roleList"></param>
        public void UpdateUserRole(string keyValue, string[] roleList)
        {
            //角色list
            var roleLists = new List<SysUserRoleEntity>();
            foreach (var item in roleList.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
            {
                var entity = new SysUserRoleEntity();
                entity.Create(true);
                entity.RoleId = item;
                entity.UserId = keyValue;
                entity.zt = "1";
                roleLists.Add(entity);
            }

            using (var db = new EFDbTransaction(this._databaseFactory).BeginTrans())
            {
                var oldRoleList = _sysUserRoleRepository.IQueryable().Where(p => p.UserId == keyValue).ToList();
                for (int i = 0; i < roleLists.Count; i++)
                {
                    if (oldRoleList.Any(p => p.RoleId == roleLists[i].RoleId))
                    {
                        oldRoleList.Remove(oldRoleList.Where(p => p.RoleId == roleLists[i].RoleId).First());
                        continue;
                    }
                    db.Insert(roleLists[i]);
                }
                foreach (var item in oldRoleList)
                {
                    db.Delete(item);
                }
                db.Commit();
            }
        }

        /// <summary>
        /// 根据OrganizeId获取 系统人员VO（且有对应的SysUser）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<SysUserStaffVO> GetSatffVOListByOrg(string orgId)
        {
            if (string.IsNullOrWhiteSpace(orgId))
            {
                return null;
            }
            var sql = @"select c.Id UserId, c.Account UserCode, a.Id StaffId, a.gh gh, a.DepartmentCode, a.Name  
from [NewtouchHIS_Base]..V_S_Sys_Staff a
left join [NewtouchHIS_Base]..V_S_Sys_UserStaff b
on b.StaffId = a.Id
left join [NewtouchHIS_Base]..V_C_Sys_User c
on b.UserId = c.Id
where 
c.TopOrganizeId = @topOrgId
and c.Id is not null
and a.OrganizeId = @orgId";
            return this.FindList<SysUserStaffVO>(sql, new[] { new SqlParameter("@orgId", orgId)
                ,new SqlParameter("@topOrgId", Constants.TopOrganizeId) });
        }

        /// <summary>
        /// 获取组织机构的人员列表
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<SysStaffVEntity> GetStaffListByOrg(string orgId)
        {
            if (string.IsNullOrWhiteSpace(orgId))
            {
                return null;
            }
            var sql = @"select * from [NewtouchHIS_Base]..V_S_Sys_Staff(nolock)
where OrganizeId = @orgId";
            return this.FindList<SysStaffVEntity>(sql, new[] { new SqlParameter("@orgId", orgId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<string> GetYfbmCodeListByUserId(string userId)
        {
            var sql = "select distinct yfbmCode from [NewtouchHIS_Base]..V_S_Sys_UserYfbm where UserId = @userId";
            return this.FindList<string>(sql, new[] { new SqlParameter("@userId", userId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetLanguageTypeByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }
            var sql = "select LanguageType from [NewtouchHIS_Base]..V_C_Sys_User(nolock) where Id = @userId";
            return this.FirstOrDefault<string>(sql, new[] { new SqlParameter("@userId", userId) });
        }

        /// <summary>
        /// 根据父级OrgId（递归树）获取 所有 指定 职位 的 StaffId
        /// </summary>
        /// <param name="parentOrgId"></param>
        /// <param name="dutyCode"></param>
        /// <returns></returns>
        public List<string> GetStaffIdListByParentOrg(string parentOrgId, string dutyCode)
        {
            var sql = @"

WITH cteTree
        AS (SELECT *
              FROM [NewtouchHIS_Base]..V_S_Sys_Organize
              WHERE Id = @parentOrgId --第一个查询作为递归的基点(锚点)
            UNION ALL
            SELECT [NewtouchHIS_Base]..V_S_Sys_Organize.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
              FROM
                   cteTree INNER JOIN [NewtouchHIS_Base]..V_S_Sys_Organize ON cteTree.Id= [NewtouchHIS_Base]..V_S_Sys_Organize.ParentId) 

select aa.Id from [NewtouchHIS_Base]..V_S_Sys_Staff aa
left join [NewtouchHIS_Base]..V_S_Sys_StaffDuty bb on aa.Id = bb.StaffId
left join [NewtouchHIS_Base]..V_S_Sys_Duty cc on bb.DutyId = cc.Id
where aa.OrganizeId in
(
	select Id from cteTree
)
and cc.Code = @dutyCode";
            return this.FindList<string>(sql, new[] { new SqlParameter("@parentOrgId", parentOrgId), new SqlParameter("@dutyCode", dutyCode) });
        }

        /// <summary>
        /// 根据员工Id获取所有岗位
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="StaffId"></param>
        /// <returns></returns>
        public IList<string> GetDutyListByStaffId(string orgId, string StaffId)
        {
            var sql = @"SELECT  DutyCode
                FROM    NewtouchHIS_Base..V_C_Sys_StaffDuty V_staff
                        RIGHT JOIN NewtouchHIS_Base..V_S_Sys_Staff staff ON V_staff.StaffGh = staff.gh
                                                                            AND staff.OrganizeId = V_staff.OrganizeId
                WHERE   staff.Id = @StaffId
                        AND staff.OrganizeId = @orgId";
            return this.FindList<string>(sql, new[] { new SqlParameter("@orgId", orgId), new SqlParameter("@StaffId", StaffId) });
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userPassword"></param>
        /// <param name="keyValue"></param>
        public void RevisePassword(string userPassword, string userId)
        {
            var secretkey = Md5.md5(Comm.CreateNo(), 16).ToLower();
            var password = Md5.md5(DESEncrypt.Encrypt(Md5.md5(userPassword, 32).ToLower(), secretkey).ToLower(), 32).ToLower();

            //这里不直接操作表 就没法弄了，不能通过视图
            this.ExecuteSqlCommand("update [NewtouchHIS_Base]..sys_userlogon set UserSecretkey = @secretkey, UserPassword = @password where UserId = @userId and zt = '1'", new[] {
                new SqlParameter("@userId", userId)
                ,new SqlParameter("@secretkey", secretkey)
                ,new SqlParameter("@password", password)
            });
        }

        /// <summary>
        /// 根据UserId获取系统用户可操作的药房部门Code List
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<string> GetYfbmCodeListByUserId(string userId, string orgId)
        {
            const string sql = "select distinct yfbmCode from [NewtouchHIS_Base]..V_S_Sys_UserYfbm where UserId = @userId and zt = '1' and OrganizeId = @orgId";
            return FindList<string>(sql, new DbParameter[] { new SqlParameter("@userId", userId), new SqlParameter("@orgId", orgId) });
        }

        /// <summary>
        /// 根据UserId获取系统用户可操作的药房部门List
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public IList<LoginUserCurrentYfbmModel> GetYfbmListByUserId(string userId, string orgId)
        {
            const string sql = "select distinct a.yfbmCode,b.yfbmmc,CONVERT(VARCHAR(1), b.mzzybz) as mzzybz,CONVERT(VARCHAR(4), b.yjbmjb) as yfbmjb from [NewtouchHIS_Base]..V_S_Sys_UserYfbm a inner join [NewtouchHIS_Base]..V_S_xt_yfbm b on a.OrganizeId = b.OrganizeId and b.zt = '1' and a.yfbmCode = b.yfbmCode where a.UserId = @userId and a.zt = '1' and a.OrganizeId = @orgId";
            return FindList<LoginUserCurrentYfbmModel>(sql, new DbParameter[] { new SqlParameter("@userId", userId), new SqlParameter("@orgId", orgId) });
        }


        /// <summary>
        /// 获取过期预警和库存预警总条数
        /// </summary>
        /// <param name="yfbmcode"></param>
        /// <param name="orgid"></param>
        /// <param name="gqyj"></param>
        /// <param name="kcyj"></param>
        /// <returns></returns>
        public IList<SysMSGQueryVO> MSGQuery(string yfbmcode, string orgid, int gqyj, string kcyj)
		{
            string sql = @"select ypCode,pc,ph,kykc,kcsl,yxq,typeas from (
SELECT  bmypxx.Ypdm ypCode,''pc,''ph,kcyjz
	,SUM(ISNULL((kcxx.kcsl-kcxx.djsl),0)) kykc,SUM(ISNULL(kcxx.kcsl,0)) kcsl,''yxq,'1'typeas
	FROM dbo.xt_yp_bmypxx(NOLOCK) bmypxx
	INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_yp yp ON yp.ypCode=bmypxx.Ypdm AND yp.OrganizeId=bmypxx.OrganizeId 
	LEFT JOIN dbo.xt_yp_kcxx(NOLOCK) kcxx ON kcxx.ypdm=bmypxx.Ypdm AND kcxx.yfbmCode=bmypxx.yfbmCode AND kcxx.OrganizeId=bmypxx.OrganizeId 
	WHERE bmypxx.yfbmCode=@yfbmcode
	AND bmypxx.OrganizeId=@orgId
	AND kcxx.zt='1'
	AND bmypxx.zt='1'
	GROUP BY  bmypxx.Ypdm,kcyjz
	)a where a.kykc<kcyjz--@kcyj
union all
select * from (
SELECT yp.ypCode ypCode,kcxx.pc,kcxx.ph,0 kykc,0 kcsl, SUBSTRING(CONVERT(VARCHAR(15),kcxx.yxq, 120),0, 11)yxq,
case when convert(varchar(10),kcxx.yxq,121)>convert(varchar(10),GETDATE(),121) then '3' else '2' end typeas
FROM dbo.xt_yp_kcxx(NOLOCK) kcxx 
INNER JOIN NewtouchHIS_Base.dbo.V_C_xt_yp yp ON yp.ypCode=kcxx.ypdm AND yp.OrganizeId=kcxx.OrganizeId AND yp.zt='1' 
left join xt_yp_crkmx bb on bb.pc=kcxx.pc and bb.Ph=kcxx.ph and bb.Fph is not null 
left join xt_yp_crkdj(nolock) crkdj on bb.crkId=crkdj.crkId
left join  NewtouchHIS_Base.dbo.V_S_xt_ypgys d on d.gysCode=crkdj.Ckbm and d.OrganizeId =kcxx.OrganizeId 
WHERE kcxx.OrganizeId=@orgId AND kcxx.yfbmCode=@yfbmcode
AND kcxx.zt='1'
AND kcxx.kcsl>0
group by yp.ypCode, kcxx.yxq,kcxx.pc,kcxx.ph
)b where  DATEADD(day,@gqyj ,yxq)<GETDATE()  or  DATEDIFF(MONTH, GETDATE(),yxq) <='1'  ";

            var partm = new DbParameter[] {
                new SqlParameter("@yfbmcode", yfbmcode),
                new SqlParameter("@orgId", orgid),
                new SqlParameter("@gqyj", gqyj),
                new SqlParameter("@kcyj", kcyj)
            };
            return FindList<SysMSGQueryVO>(sql,partm);


        }
    }
}
