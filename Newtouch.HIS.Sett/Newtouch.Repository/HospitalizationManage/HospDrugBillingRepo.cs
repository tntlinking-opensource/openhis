using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Newtouch.HIS.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class HospDrugBillingRepo : RepositoryBase<HospDrugBillingEntity>, IHospDrugBillingRepo
    {
        public HospDrugBillingRepo(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {

        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="hospItemFeeEntity"></param>
        /// <param name="keyValue"></param>
        public void SubmitForm(HospDrugBillingEntity hospItemFeeEntity, int? keyValue)
        {
            if (keyValue > 0)
            {
                var entity = this.FindEntity(hospItemFeeEntity.jfbbh);
                entity.Modify(keyValue);
                this.Update(entity);
                hospItemFeeEntity.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_ypjfb"));
                this.Insert(hospItemFeeEntity);
            }
            else
            {
                hospItemFeeEntity.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_ypjfb"));
                this.Insert(hospItemFeeEntity);
            }
        }
        public void ExecPartialSettleFeeDetail(string zyh,string jsnm,string czlx)
        {
            if (czlx == "delete")
            {
                string sql = @"update  Drjk_zyfymxsc_input set zyh=@zyh,jsnm =NULL  where left(zyh,5)=@zyh and jsnm=@jsnm and jsnm is not null" +
                    "  update  Drjk_zyfymxsc_output set zyh=@zyh,jsnm =NULL  where left(zyh,5)=@zyh and jsnm=@jsnm and jsnm is not null";
                SqlParameter[] para ={
                new SqlParameter("@zyh",zyh ?? ""),
                 new SqlParameter("@jsnm",jsnm)
                };
                int i = this.ExecuteSqlCommand(sql, para);
            }
            else { 
            var zyh_Rd= zyh+"_t";
            string sql = @"update Drjk_zyfymxsc_input set zyh=@zyh_Rd,jsnm=@jsnm where zyh=@zyh and jsnm is  null"+
                    "  update Drjk_zyfymxsc_output set zyh=@zyh_Rd,jsnm=@jsnm  where zyh=@zyh and jsnm is  null";
            SqlParameter[] para ={
                new SqlParameter("@zyh",zyh ?? ""),
                 new SqlParameter("@zyh_Rd",zyh_Rd),
                 new SqlParameter("@jsnm",jsnm)
                };
            int i= this.ExecuteSqlCommand(sql, para);
            }
        }
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
        //住院补计费扣掉相应库存
        public void Updatezyaddfee(string OrganizeId, decimal sl,string yfbm,string ypdm)
        {
            try
            {
                string sql = @" update [NewtouchHIS_PDS].[dbo].[xt_yp_kcxx] set kcsl-=@sl   where ypdm=@ypdm and yfbmcode=@yfbmcode'
and organizeid=@orgId and zt='1'";
                SqlParameter[] para ={
                new SqlParameter("@orgId",OrganizeId),
                 new SqlParameter("@sl",sl),
                 new SqlParameter("@ypdm",ypdm),
                 new SqlParameter("@yfbmcode",yfbm)
                };
                int i = this.ExecuteSqlCommand(sql, para);
            }
            catch (Exception ex)
            {
            }

        }
        /// <summary>
        /// 退费退回耗材库存
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="jfbbhs"></param>
        /// <param name="userCode"></param>
        public void Updatezy_wzkcReturn(string OrganizeId, string jfbbhs, string userCode)
        {
            this.ExecuteSqlCommand("exec Newtouch_CIS..物资处方退费库存量退还 @jfbbhs,@OrganizeId,@userCode,NULL ", new SqlParameter("@jfbbhs", jfbbhs)
                ,new SqlParameter("@OrganizeId", OrganizeId),new SqlParameter("@userCode", userCode));
        }

        /// <summary>
        /// 住院退费退回医生站开立耗材库存
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="jfbbhs"></param>
        /// <param name="userCode"></param>
        public void Updatezyyz_wzkcReturn(List<HospItemBillingEntity> entityList, string OrganizeId, string userCode)
        {
            foreach (var item in entityList)
            {
                var ss = item.tdrq.ToString("yyyy-MM-dd HH:mm:ss");
                this.ExecuteSqlCommand("exec Newtouch_CIS..物资处方退费库存量退还 @jfbbhs,@OrganizeId,@userCode,@zxrq ", new SqlParameter("@jfbbhs", item.yzwym)
                ,new SqlParameter("@zxrq", item.tdrq.ToString("yyyy-MM-dd HH:mm:ss")), new SqlParameter("@OrganizeId", OrganizeId), new SqlParameter("@userCode", userCode));
            }
        }
        public int getHckcWith(string orgId)
        {
            string sql = @" select COUNT(1)  from Newtouch_CIS..Sys_Config where code='openWzhckc' and organizeid=@orgId  and value='ON' and zt='1'";
            SqlParameter[] para ={
                new SqlParameter("@orgId",orgId) };
            return this.FirstOrDefault<int>(sql, para);
        }
    }
}


