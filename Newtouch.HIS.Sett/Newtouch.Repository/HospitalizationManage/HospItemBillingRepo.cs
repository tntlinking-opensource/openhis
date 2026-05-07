using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Newtouch.HIS.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class HospItemBillingRepo : RepositoryBase<HospItemBillingEntity>, IHospItemBillingRepo
    {
        public HospItemBillingRepo(IDefaultDatabaseFactory databaseFactory, IHospSettlementRepo SettOfTheHosRepository)
            : base(databaseFactory)
        {
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="hospItemFeeEntity"></param>
        /// <param name="keyValue"></param>
        public void SubmitForm(HospItemBillingEntity hospItemFeeEntity, int? keyValue)
        {
            if (keyValue > 0)
            {
                var entity = this.FindEntity(hospItemFeeEntity.jfbbh);
                entity.Modify(keyValue);
                this.Update(entity);
                hospItemFeeEntity.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb"));
                this.Insert(hospItemFeeEntity);
            }
            else
            {
                hospItemFeeEntity.Create(true, EFDBBaseFuncHelper.Instance.GetNewPrimaryKeyInt("zy_xmjfb"));
                this.Insert(hospItemFeeEntity);
            }
        }

        /// <summary>
        /// 查询 时间段内的 项目计费EntityList
        /// 已考虑退费
        /// </summary>
        /// <param name="zyh"></param>
        /// <param name="orgId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sourceQuery"></param>
        /// <returns></returns>
        public IList<HospItemBillingEntity> GetItemFeeEntityListByTime(string zyh, string orgId, DateTime startTime, DateTime endTime
            , IQueryable<HospItemBillingEntity> sourceQuery = null)
        {
            if (sourceQuery == null)
            {
                sourceQuery = this.IQueryable();
            }
            //没有加入结束日期的过滤 是为了 退费
            var query1 = sourceQuery.Where(p => p.zyh == zyh
              && p.OrganizeId == orgId && p.zt == "1"
              && p.CreateTime > startTime).ToList();
            //
            var query2 = query1.GroupBy(p => p.cxzyjfbbh != 0 ? p.cxzyjfbbh : p.jfbbh)
                .Select(p => new HospItemBillingEntity()
                {
                    jfbbh = p.Key,
                    sl = p.Sum(i => i.sl)
                }).ToList();
            if (query2.Count == query1.Count)
            {
                //没有退过费
                return query1.Where(p => p.CreateTime <= endTime).ToList();
            }
            query1 = query1.Where(p => p.CreateTime <= endTime).ToList();
            //
            var query = from q2 in query2
                        join q1 in query1
                        on q2.jfbbh equals q1.jfbbh
                        //一定是q2joinq1，然后过滤q1的时间
                        //退完不显示
                        where q1.CreateTime <= endTime && q2.sl > 0
                        select new HospItemBillingEntity
                        {
                            jfbbh = q1.jfbbh,
                            OrganizeId = q1.OrganizeId,
                            zyh = q1.zyh,
                            tdrq = q1.tdrq,
                            sfxm = q1.sfxm,
                            dl = q1.dl,
                            ys = q1.ys,
                            ysmc = q1.ysmc,
                            ks = q1.ks,
                            ksmc = q1.ksmc,
                            cw = q1.cw,
                            dj = q1.dj,
                            sl = q2.sl, //q2.sl
                            jfdw = q1.jfdw,
                            zfbl = q1.zfbl,
                            zfxz = q1.zfxz,
                            ssbz = q1.ssbz,
                            ssry = q1.ssry,
                            ssrq = q1.ssrq,
                            yzxz = q1.yzxz,
                            yzzt = q1.yzzt,
                            CreatorCode = q1.CreatorCode,
                            CreateTime = q1.CreateTime,
                            LastModifyTime = q1.LastModifyTime,
                            LastModifierCode = q1.LastModifierCode,
                            px = q1.px,
                            bq = q1.bq,
                            zt = q1.zt,
                            jzjhmxId = q1.jzjhmxId,
                            kflb = q1.kflb,
                            ttbz = q1.ttbz,
                            duration = q1.duration,
                            zzll = q1.zzll,
                        };

            return query.ToList();
        }
        /// <summary>
        /// 变更耗材库存
        /// </summary>
        /// <param name="OrganizeId"></param>
        /// <param name="sl"></param>
        /// <param name="yfbm"></param>
        /// <param name="ypdm"></param>
        public void Updatezyaddfee(string orgId,string jfbhs, string yfbm, string userCode)
        {
            try
            {
                string sql = @" IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#zyjsfy') and type='U')
BEGIN
	DROP TABLE #zyjsfy;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#zyjzkcxx') and type='U')
BEGIN
	DROP TABLE #zyjzkcxx;
END
--1.取计费物资数据
SELECT jfbbh,sfxm,sl
INTO #zyjsfy
FROM zy_xmjfb with(nolock)
WHERE OrganizeId=@orgId  AND zt='1' and jfbbh in (select col from f_split(@jfbhs,',')) 

IF(EXISTS(SELECT 1 FROM #zyjsfy))
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION
			DECLARE @sfxm VARCHAR(50),@jfbbh VARCHAR(50),@sl NUMERIC(6,2);
			--2.循环计费数
			WHILE(EXISTS(SELECT 1 FROM #zyjsfy))
			BEGIN
				IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#zyjzkcxx') and type='U')
				BEGIN
					DROP TABLE #zyjzkcxx;
				END
				SELECT TOP 1 @jfbbh=jfbbh, @sfxm=sfxm, @sl=sl FROM #zyjsfy;
				--3.取当前物资库存批次批号信息
				SELECT Id,kcsl,djsl,yxq,productCode,pc,ph
				INTO #zyjzkcxx 
				FROM Newtouch_CIS..dept_kcxx a
				WHERE ks=@ks and OrganizeId=@orgId and productCode=@sfxm and zt='1'

				DECLARE @curKcxxKcId VARCHAR(50),@curKcxxsl int
				DECLARE @sysl INT;
				SET @sysl=@sl;
				--4.循环批次扣减库存
				WHILE EXISTS(SELECT 1 FROM #zyjzkcxx ) AND @sysl>0
				BEGIN
					SELECT TOP 1 @curKcxxKcId=Id, @curKcxxsl=kcsl-djsl FROM #zyjzkcxx order by yxq
					IF @curKcxxsl>=@sl
					BEGIN
						UPDATE Newtouch_CIS..dept_kcxx  SET kcsl-=@sysl, LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE Id=@curKcxxKcId AND zt='1'
						--存储库存使用记录
						insert into Newtouch_CIS..dept_kcxx_djjl(OrganizeId,ks,cfh,sfxmcode,pc,ph,sl,isfs,zt,CreateTime,CreatorCode)
						select @orgId,@ks,@jfbbh,productCode,pc,ph,@sysl sl,1 isfs,1 zt,GETDATE() createtime, @userCode CreatorCode from #zyjzkcxx where Id=@curKcxxKcId
						SET @sysl=0;
					END
					ELSE
					BEGIN
						UPDATE Newtouch_CIS..dept_kcxx SET kcsl-=@curKcxxsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE Id=@curKcxxKcId AND zt='1'
						--存储库存使用记录
						insert into Newtouch_CIS..dept_kcxx_djjl(OrganizeId,ks,cfh,sfxmcode,pc,ph,sl,isfs,zt,CreateTime,CreatorCode)
						select @orgId,@ks,@jfbbh,productCode,pc,ph,@sysl sl,1 isfs,1 zt,GETDATE() createtime, @userCode CreatorCode from #zyjzkcxx where Id=@curKcxxKcId
						SET @sysl-=@curKcxxsl; 
					END
					DELETE FROM #zyjzkcxx WHERE Id=@curKcxxKcId; 
				END
				DELETE FROM #zyjsfy WHERE jfbbh=@jfbbh;  
			END
			COMMIT TRANSACTION
	END TRY 
	BEGIN CATCH 
		ROLLBACK TRANSACTION
		SELECT ERROR_MESSAGE();
	END CATCH 
END";
                SqlParameter[] para ={
                new SqlParameter("@orgId",orgId),
                 new SqlParameter("@jfbhs",jfbhs),
                 new SqlParameter("@ks",yfbm),
                 new SqlParameter("@userCode",userCode)
                };
                int i = this.ExecuteSqlCommand(sql, para);
            }
            catch (Exception ex)
            {
            }
        }
    }
}


