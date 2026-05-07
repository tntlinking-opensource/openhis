USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[usp_zy_OrderExecution_TyxmWithWz]    Script Date: 2025/12/3 14:12:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





/*
author:chl
createtime:2021-7-4
desc: 医嘱执行(执行当前)-项目
exec [dbo].[usp_zy_OrderExecution_TyxmWithWz] '6d5752a7-234a-403e-aa1c-df8b45d3469f','','20210705114737656','kradmin','2021-07-05 11:40:53.000',475
*/
ALTER proc [dbo].[usp_zy_OrderExecution_TyxmWithWz]
	@orgId  varchar(50),
	@yzxhlist varchar(max),
	@jyjcyz varchar(max),
	@czyh varchar(50),
	@zxrq datetime,
	@lyxh numeric(12,0)
as
begin

BEGIN TRAN
BEGIN TRY
	DECLARE @yzxhTab TABLE (
		yzxh VARCHAR(50)
	)
	print '医嘱Id'
	INSERT INTO @yzxhTab ( yzxh )
	SELECT * FROM dbo.f_split(@yzxhlist,',')

	IF @@error <> 0 
	BEGIN      
		SELECT 'F|获取医嘱序号传值失败'  
		ROLLBACK TRAN
		RETURN    
	END 

	----创建医嘱表，保存医嘱信息
	DECLARE @OrderTab TABLE
	(
		Id VARCHAR(50),
		OrganizeId VARCHAR(50),
		zyh VARCHAR(20),
		zh INT,
		WardCode VARCHAR(30),
		DeptCode VARCHAR(30),
		ysgh VARCHAR(20),
		pcCode VARCHAR(20),
		zxcs INT,
		xmdm VARCHAR(50),
		xmmc NVARCHAR(50),
		dw VARCHAR(50),
		zbbz INT,
		sl INT,
		yzlx INT,
		zxsj DATETIME,
		zxr VARCHAR(50),
		ypjl NUMERIC(14,3),
		ypgg VARCHAR(200),
		zxksdm NVARCHAR(200),
		yzxz INT,
		isjf INT
	)
	---根据医嘱id将医嘱信息插入表中
	print '医嘱Id-将医嘱信息插入表中'
	INSERT INTO @OrderTab
	(Id ,OrganizeId ,zyh ,zh ,WardCode ,DeptCode ,ysgh,pcCode ,zxcs ,xmdm ,xmmc ,dw ,
zbbz,sl ,yzlx ,zxsj ,zxr ,ypjl ,ypgg ,zxksdm,yzxz,isjf)
	SELECT Id,OrganizeId,zyh,zh,WardCode,DeptCode,ysgh,pccode,ISNULL(zxcs,1) ,xmdm,xmmc,dw,zbbz,sl,yzlx,zxsj,zxr,ypjl,ypgg,zxksdm,2 AS yzxz,isjf 
	FROM dbo.zy_cqyz a with(nolock)
	WHERE Id IN (SELECT yzxh FROM @yzxhTab) and a.zt='1' --AND yzlx IN (2,4)
	IF @@error <> 0 
	BEGIN      
		SELECT 'F|获取长期医嘱信息失败'  
		ROLLBACK TRAN
		RETURN    
	END 
	INSERT INTO @OrderTab
	(Id ,OrganizeId ,zyh ,zh ,WardCode ,DeptCode ,ysgh,pcCode ,zxcs ,xmdm ,xmmc ,dw ,
zbbz,sl ,yzlx ,zxsj ,zxr ,ypjl ,ypgg ,zxksdm,yzxz,isjf)
	SELECT Id,OrganizeId,zyh,zh,WardCode,DeptCode,ysgh,pccode,ISNULL(zxcs,1),xmdm,xmmc,dw,zbbz,sl,yzlx,zxsj,zxr,ypjl,ypgg,zxksdm,1 AS yzxz ,isjf
	FROM dbo.zy_lsyz b with(nolock)
	WHERE Id IN (SELECT yzxh FROM @yzxhTab) and b.zt='1' --and yzlx not IN (6,7) AND yzlx IN (2,4)
	IF @@error <> 0 
	BEGIN      
		SELECT 'F|获取临时医嘱信息失败'  
		ROLLBACK TRAN
		RETURN    
	END 
	print '检验检查'
	print (@jyjcyz)
	--检验检查
	if(@jyjcyz<>'')
	begin
		INSERT INTO @OrderTab( Id ,OrganizeId ,zyh ,zh ,WardCode ,DeptCode ,ysgh,pcCode ,zxcs ,xmdm ,xmmc ,dw ,zbbz,sl ,yzlx ,zxsj ,zxr ,ypjl ,ypgg ,zxksdm,yzxz,isjf)
		SELECT Id,OrganizeId,zyh,zh,WardCode,DeptCode,ysgh,pccode,ISNULL(zxcs,1),xmdm,xmmc,dw,zbbz,sl,yzlx,zxsj,zxr,ypjl,ypgg,zxksdm,1 AS yzxz ,isjf
		FROM dbo.zy_lsyz b with(nolock)
		WHERE b.OrganizeId=@orgId and b.zt='1' AND yzlx IN (6,7) and 
		yzh IN (select * from dbo.f_split(@jyjcyz,','))
		and not exists(select 1 from @OrderTab c where b.Id=c.Id)
	end	

	if(not exists(select 1 from @OrderTab))
	begin
		SELECT 'F|无可执行医嘱'  
		ROLLBACK TRAN
		RETURN   
	end 

	print '药品插入费用明细库'
	if(exists(select 1 from @OrderTab where yzlx IN (2,4,10)))
	begin
		INSERT INTO dbo.zy_fymxk
		(Id ,OrganizeId ,zyh ,yzxh ,	zxrq ,qqrq ,xmdm ,xmmc ,gg ,dw ,sl ,dj , zje ,yzxz ,ybdm ,jzks ,gdzxbz ,yzlb ,WardCode ,DeptCode ,cwdm ,czyh ,ysgh ,ysksdm ,zxksdm ,CreateTime ,CreatorCode ,zt,isjf)
		SELECT NEWID(),a.OrganizeId,a.zyh,a.Id,@zxrq,GETDATE(),a.xmdm,a.xmmc,a.ypgg,b.zycldw,a.sl,b.lsj/b.bzs*b.zycls AS dj,a.sl*b.lsj/b.bzs*b.zycls AS zje,a.yzxz,c.ybdm,a.DeptCode,0,0,a.WardCode,a.DeptCode,d.BedCode,@czyh,a.ysgh,a.DeptCode,a.zxksdm,GETDATE(),@czyh,'1',isjf 
		FROM @OrderTab a
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_yp b ON a.xmdm=b.ypCode AND a.OrganizeId=b.OrganizeId AND b.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx c ON b.ypId=c.ypId  AND b.OrganizeId=c.OrganizeId AND c.zt='1'
		LEFT JOIN dbo.zy_brxxk d ON a.zyh =d.zyh AND a.OrganizeId=d.OrganizeId AND d.zt='1'
		WHERE  a.yzlx IN (2,4,10)
		and not exists(select 1 from zy_fymxk f with(nolock) where a.zyh=f.zyh and a.Id=f.yzxh and f.zxrq=@zxrq and f.zt='1')
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|药品插入费用明细库失败'   
			ROLLBACK TRAN
			RETURN    
		END 

		print '药品医嘱插入发药请求库'
		INSERT INTO dbo.zy_fyqqk
		(Id ,OrganizeId ,lyxh ,zyh ,hzxm ,yzxh ,fzxh ,yfdm ,WardCode ,DeptCode ,ysgh ,
		zxrq ,qqrq ,ypdm ,ypmc ,ypsl ,ypgg ,ypdw ,ypdj ,zxcs ,tybz ,yzxz ,zbbz ,mcsl ,
		CreateTime ,CreatorCode ,zt	)
		SELECT NEWID(),a.OrganizeId, @lyxh, a.zyh,d.xm,a.Id,a.zh,a.zxksdm,a.WardCode,a.DeptCode,a.ysgh,@zxrq,GETDATE(),a.xmdm,a.xmmc,a.sl,a.ypgg,b.zycldw AS dw,b.lsj/b.bzs*b.zycls AS ypdj,
		a.zxcs,1 AS tybz,a.yzxz,a.zbbz,a.ypjl ,GETDATE(),@czyh,'1' 
		FROM @OrderTab a
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_yp b with(nolock) ON a.xmdm=b.ypCode AND a.OrganizeId=b.OrganizeId AND b.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx c with(nolock) ON b.ypId=c.ypId AND b.OrganizeId=c.OrganizeId AND c.zt='1'
		LEFT JOIN dbo.zy_brxxk d ON a.zyh =d.zyh AND a.OrganizeId=d.OrganizeId AND d.zt='1'
		WHERE  a.yzlx IN (2,4,10) and a.isjf<>'0'
		and not exists(select 1 from zy_fyqqk f with(nolock) where a.Id=f.yzxh and f.zxrq=@zxrq and f.zt='1')
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|插入发药请求库失败'    
			ROLLBACK TRAN
			RETURN    
		END 
	end

	print '非药品插入费用明细库'
	if(exists(select 1 from @OrderTab where yzlx not IN (2,4,10)))
	begin
		INSERT INTO dbo.zy_fymxk
		( Id ,OrganizeId ,zyh ,yzxh ,	zxrq ,qqrq ,xmdm ,xmmc ,gg ,dw ,sl ,dj , zje ,yzxz ,ybdm ,jzks ,gdzxbz ,yzlb ,WardCode ,DeptCode ,cwdm ,czyh ,ysgh ,ysksdm ,zxksdm ,CreateTime ,CreatorCode ,zt,isjf
		)
		SELECT NEWID(),a.OrganizeId,a.zyh,a.Id,@zxrq,GETDATE(),a.xmdm,a.xmmc,a.ypgg,b.dw,a.sl,b.dj AS dj,a.sl*b.dj AS zje,a.yzxz,b.ybdm,a.DeptCode,0,1,a.WardCode,a.DeptCode,d.BedCode,@czyh,a.ysgh,a.DeptCode,a.zxksdm,GETDATE(),@czyh,'1' ,isjf
		FROM @OrderTab a
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfxm b with(nolock) ON a.xmdm=b.sfxmCode AND a.OrganizeId=b.OrganizeId AND b.zt='1'
		LEFT JOIN dbo.zy_brxxk d with(nolock) ON a.zyh =d.zyh AND a.OrganizeId=d.OrganizeId AND d.zt='1'
		WHERE a.yzlx NOT IN (2,3,4,10,11)
		and not exists(select 1 from zy_fymxk f with(nolock) where a.zyh=f.zyh and a.Id=f.yzxh and f.zxrq=@zxrq and f.zt='1')
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|非药品插入费用明细库失败'   
			ROLLBACK TRAN
			RETURN    
		END 
		
		print '项目医嘱插入通用膳食请求库'
		INSERT INTO dbo.zy_tyssqqk( 
		Id , OrganizeId ,lyxh , zyh ,hzxm,yzxh,fzxh ,yfdm ,   WardCode ,DeptCode ,  ysgh ,zxrq ,qqrq ,     xmdm , xmmc ,  sl , dw ,  dj ,zxcs, zyxz ,  zbbz,  mcsl,  yzlx,CreateTime,CreatorCode ,zt)
		SELECT NEWID(),a.OrganizeId,@lyxh,a.zyh,d.xm,a.Id,a.zh,a.zxksdm,a.WardCode,a.DeptCode,a.ysgh,@zxrq,GETDATE(),a.xmdm,a.xmmc,a.sl,b.dw ,ISNULL(b.dj,0) AS ypdj,a.zxcs,a.yzxz,a.zbbz,a.ypjl,a.yzlx,GETDATE(), @czyh,       '1' 
		FROM @OrderTab a
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfxm b with(nolock) ON a.xmdm=b.sfxmCode AND a.OrganizeId=b.OrganizeId AND b.zt='1'
		LEFT JOIN dbo.zy_brxxk d with(nolock) ON a.zyh =d.zyh AND a.OrganizeId=d.OrganizeId AND d.zt='1'
		WHERE  a.yzlx NOT IN (2,4,10)
		and not exists(select 1 from zy_tyssqqk f with(nolock) where a.Id=f.yzxh and f.zxrq=@zxrq and f.zt='1')
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|插入通用膳食请求库失败'   
			ROLLBACK TRAN
			RETURN    
		END 
	end
	
	-----更新医嘱状态
	UPDATE dbo.zy_cqyz SET yzzt=2, zxsj=@zxrq,zxr=@czyh,zxing=null,LastModifierCode=@czyh,LastModifyTime=GETDATE() WHERE Id IN (SELECT yzxh FROM @yzxhTab)
	IF @@error <> 0 
	BEGIN      
		SELECT 'F|更新长期医嘱信息失败'  
		ROLLBACK TRAN
		RETURN    
	END 
	UPDATE dbo.zy_lsyz SET yzzt=2, zxsj=@zxrq,zxr=@czyh,zxing=null,LastModifierCode=@czyh,LastModifyTime=GETDATE() WHERE Id IN (SELECT yzxh FROM @yzxhTab)
	IF @@error <> 0 
	BEGIN      
		SELECT 'F|更新临时医嘱信息失败'  
		ROLLBACK TRAN
		RETURN    
	END 
	if(@jyjcyz<>'')
	begin
		UPDATE dbo.zy_lsyz 
		SET yzzt=2, zxsj=@zxrq,zxing=null,zxr=@czyh,LastModifierCode=@czyh,LastModifyTime=GETDATE() 
		WHERE OrganizeId=@orgId and zt='1' AND yzlx IN (6,7) and 
		yzh IN (select * from dbo.f_split(@jyjcyz,','))
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|更新检验检查医嘱信息失败'  
			ROLLBACK TRAN
			RETURN    
		END 
	end

	--医嘱是否存在物资耗材 并且配置启用库存扣减开关（开立需扣减库存）
	if(exists(select 1 from @OrderTab WHERE charindex('wz',LOWER(xmdm))>0) and exists (select 1  from Sys_Config where code='openWzhckc' and organizeid=@orgId and zt='1' and value='ON'))
	BEGIN
		DECLARE @ks VARCHAR(50),@zyh VARCHAR(20),@xmdm VARCHAR(50),@yzid VARCHAR(50),@sl NUMERIC(6,2);
		IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#hcTab') and type='U')
		BEGIN
			DROP TABLE #hcTab;
		END
		select * INTO #hcTab from @OrderTab WHERE charindex('wz',LOWER(xmdm))>0
		--循环计费数
		WHILE(EXISTS(SELECT 1 FROM #hcTab))
		BEGIN
			IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#hczxkc') and type='U')
			BEGIN
				DROP TABLE #hczxkc;
			END
			SELECT TOP 1 @yzid=id, @xmdm=xmdm, @sl=sl,@ks=DeptCode,@zyh=zyh FROM #hcTab;
			print('取当前物资库存批次批号信息')
			SELECT Id,kcsl,djsl,yxq,productCode,pc,ph
			INTO #hczxkc 
			FROM Newtouch_CIS..dept_kcxx a 
			WHERE ks=@ks and OrganizeId=@orgId and productCode=@xmdm and zt='1' and (kcsl-djsl)>0
			IF @@error <> 0 
			BEGIN      
				SELECT 'F|获取耗材：'+@xmdm+'的有效库存失败'   
				ROLLBACK TRAN
				RETURN    
			END 
			DECLARE @curKcxxKcId VARCHAR(50),@curKcxxsl int
			DECLARE @sysl INT;
			SET @sysl=@sl;
			print('循环扣减库存并存记录')
			WHILE EXISTS(SELECT 1 FROM #hczxkc ) AND @sysl>0
			BEGIN
				SELECT TOP 1 @curKcxxKcId=Id, @curKcxxsl=kcsl-djsl FROM #hczxkc order by yxq
				IF @curKcxxsl>=@sl
				BEGIN
					UPDATE Newtouch_CIS..dept_kcxx  SET kcsl-=@sysl, LastModifyTime=GETDATE(), LastModifierCode=@czyh WHERE Id=@curKcxxKcId AND zt='1'
					print('当前批次库存足够扣减')--存储库存使用记录
					insert into Newtouch_CIS..dept_kcxx_djjl(OrganizeId,ks,mzzyh,cfh,sfxmcode,pc,ph,fyrq,sl,isfs,zt,CreateTime,CreatorCode)
					select @orgId,@ks,@zyh,@yzid,productCode,pc,ph,@zxrq,@sysl sl,1 isfs,1 zt,GETDATE() createtime, @czyh CreatorCode from #hczxkc where Id=@curKcxxKcId
					SET @sysl=0;
				END
				ELSE
				BEGIN
					UPDATE Newtouch_CIS..dept_kcxx SET kcsl-=@curKcxxsl, LastModifyTime=GETDATE(), LastModifierCode=@czyh WHERE Id=@curKcxxKcId AND zt='1'
					print('当前批次库存不足以扣减')--存储库存使用记录
					insert into Newtouch_CIS..dept_kcxx_djjl(OrganizeId,ks,mzzyh,cfh,sfxmcode,pc,ph,fyrq,sl,isfs,zt,CreateTime,CreatorCode)
					select @orgId,@ks,@zyh,@yzid,productCode,pc,ph,@zxrq,@sysl sl,1 isfs,1 zt,GETDATE() createtime, @czyh CreatorCode from #hczxkc where Id=@curKcxxKcId
					SET @sysl-=@curKcxxsl; 
				END
				DELETE FROM #hczxkc WHERE Id=@curKcxxKcId; 
			END
			DELETE FROM #hcTab WHERE Id=@yzid;  
		END
	END

	SELECT 'T|执行成功'
	COMMIT TRAN
END TRY
BEGIN CATCH
	SELECT 'F|'+ERROR_MESSAGE()
	ROLLBACK TRAN
END CATCH

end
GO


