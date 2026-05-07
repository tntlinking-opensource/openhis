USE [NewtouchHIS_herp]
GO

/****** Object:  StoredProcedure [dbo].[SynchDeptProduct_CIS]    Script Date: 2025/11/5 11:26:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/***
物资出库至科室-科室库存信息管理
exec SynchDeptProduct_CIS '','6d5752a7-234a-403e-aa1c-df8b45d3469f','bdadmin'
exec SynchDeptProduct_CIS @Pdh ,@OrganizeId,@UserCode
*/

CREATE proc [dbo].[SynchDeptProduct_CIS]
 @Pdh varchar(50), --物资出库至科室单据号
 @OrganizeId varchar(50), --组织机构
 @UserCode varchar(50)  --工号
 as
--DECLARE @Pdh varchar(50)='CKZKS20251104192453'
--DECLARE @OrganizeId varchar(50)='6d5752a7-234a-403e-aa1c-df8b45d3469f'
--DECLARE @UserCode varchar(50)='bdadmin'
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#ckks') and type='U')
BEGIN
	DROP TABLE #ckks;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#ckks2') and type='U')
BEGIN
	DROP TABLE #ckks2;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#rkkcxx') and type='U')
BEGIN
	DROP TABLE #rkkcxx;
END
select djmx.Id,dj.Pdh,djmx.productId,wz.name,dj.ckbm,ks.Code rkbm,ks.Name rkbmmc,djmx.jj,djmx.sl*djmx.zhyz sl,djmx.zhyz,
	djmx.pc,djmx.ph,djmx.fph,djmx.yxq,dj.OrganizeId ,wz.productCode 
	INTO #ckks
	FROM [kf_crkdj] dj
	JOIN [kf_crkmx] djmx on djmx.crkId=dj.Id 
	JOIN [wz_product] wz on wz.Id=djmx.productId and wz.OrganizeId=@OrganizeId
	JOIN NewtouchHIS_Base.dbo.Sys_Department(NOLOCK) ks ON ks.Id=dj.rkbm AND ks.OrganizeId=dj.OrganizeId AND ks.zt='1' 
	where 1=2
IF(@Pdh='' or @Pdh IS NULL)
BEGIN
print 1
	insert into #ckks
	select djmx.Id,dj.Pdh,djmx.productId,wz.name,dj.ckbm,ks.Code rkbm,ks.Name rkbmmc,djmx.jj,djmx.sl*djmx.zhyz sl,djmx.zhyz,
	djmx.pc,djmx.ph,djmx.fph,djmx.yxq,dj.OrganizeId ,wz.productCode 
	FROM [kf_crkdj] dj
	JOIN [kf_crkmx] djmx on djmx.crkId=dj.Id 
	JOIN [wz_product] wz on wz.Id=djmx.productId and wz.OrganizeId=@OrganizeId
	JOIN NewtouchHIS_Base.dbo.Sys_Department(NOLOCK) ks ON ks.Id=dj.rkbm AND ks.OrganizeId=dj.OrganizeId AND ks.zt='1' 
	where dj.djlx='7' and dj.SyncStatus is null and dj.OrganizeId=@OrganizeId and dj.zt='1'

	select * into #ckks2 from #ckks
END
ELSE
BEGIN
print 2
	insert into #ckks
	select djmx.Id,dj.Pdh,djmx.productId,wz.name,dj.ckbm,ks.Code rkbm,ks.Name rkbmmc,djmx.jj,djmx.sl*djmx.zhyz sl,djmx.zhyz,
	djmx.pc,djmx.ph,djmx.fph,djmx.yxq,dj.OrganizeId ,wz.productCode 
	FROM [kf_crkdj] dj
	JOIN [kf_crkmx] djmx on djmx.crkId=dj.Id 
	JOIN [wz_product] wz on wz.Id=djmx.productId and wz.OrganizeId=@OrganizeId
	JOIN NewtouchHIS_Base.dbo.Sys_Department(NOLOCK) ks ON ks.Id=dj.rkbm AND ks.OrganizeId=dj.OrganizeId AND ks.zt='1' 
	where dj.Pdh=@Pdh and dj.OrganizeId=@OrganizeId and dj.zt='1'
END
--select * from #ckks2
--return
SELECT kskc.Id,kskc.productId,kskc.pc,kskc.ph,kskc.ks,ckdj.rkbm INTO #rkkcxx 
FROM  Newtouch_CIS..Dept_kcxx kskc
join #ckks  ckdj on kskc.productId=ckdj.productId and kskc.ph=ckdj.ph and  kskc.ph=ckdj.ph and kskc.ks=ckdj.rkbm and kskc.OrganizeId=ckdj.OrganizeId

DECLARE @productId VARCHAR(50), @pc VARCHAR(50), @ph VARCHAR(50), @sl int,@kcId varchar(50),@Id varchar(50),@ks varchar(50);

WHILE EXISTS(SELECT 1 FROM #ckks) 
BEGIN
	SELECT TOP 1 @Id=Id,@productId=productId, @pc=pc, @ph=ph,@sl=sl,@ks=rkbm from #ckks
	IF EXISTS(SELECT 1 FROM #rkkcxx WHERE productId=@productId and pc=@pc and ph=@ph and ks=@ks)
	BEGIN
		SELECT TOP 1 @kcId=Id FROM #rkkcxx WHERE productId=@productId and pc=@pc and ph=@ph and  ks=@ks
		UPDATE Newtouch_CIS..Dept_kcxx SET kcsl=kcsl+@sl where Id=@kcId
	END
	ELSE
	BEGIN
		INSERT INTO Newtouch_CIS..Dept_kcxx
		SELECT NEWID(),@OrganizeId,rkbm,productId,ph,pc,yxq,@sl,0,Id,jj,zhyz,NULL,'1',@UserCode,GETDATE(),NULL,NULL,productCode  from #ckks where Id=@Id
	END
	DELETE FROM #ckks WHERE Id=@Id;
END

IF(@Pdh='' or @Pdh IS NULL)
BEGIN
	update [kf_crkdj] set SyncStatus=1 where Pdh IN (SELECT pdh from #ckks2)
END
ELSE
BEGIN
update [kf_crkdj] set SyncStatus=1 where Pdh=@Pdh
END

GO


