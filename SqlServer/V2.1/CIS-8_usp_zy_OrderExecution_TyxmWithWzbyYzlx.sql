USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[usp_zy_OrderExecution_TyxmWithWzbyYzlx]    Script Date: 2025/12/3 14:14:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




/*
author:chl
createtime:2021-7-4
desc: 医嘱执行(执行长期、临时、全部)-项目
exec [dbo].[usp_zy_OrderExecution_TyxmWithWzbyYzlx] '6d5752a7-234a-403e-aa1c-df8b45d3469f','00109,00145,00113,00166,00107,00203,00157,00199,00129,00177,00120,00142,00097,00076,00191,00098,00105,00195,00110,00117,00134,00112,00188,00135,00108,00192,00189,00170,00158,00119,00082,00186,00106,00197,00165,00141,00148',0,'kradmin','2021-07-12 17:22:09.000',3441
*/
ALTER proc [dbo].[usp_zy_OrderExecution_TyxmWithWzbyYzlx]
	@orgId  varchar(50),
	@zyhs varchar(max),
	@yzxz int,
	@czyh varchar(50),
	@zxrq datetime,
	@lyxh numeric(12,0)
as
begin

BEGIN TRAN
BEGIN TRY
	declare @msg varchar(100)=''
	DECLARE @zyhTab TABLE (
		zyh VARCHAR(50)
	)

	INSERT INTO @zyhTab ( zyh )
	SELECT * FROM dbo.f_split(@zyhs,',')

	IF @@error <> 0 
	BEGIN      
		SELECT 'F|获取住院号传值失败'  
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
		WardCode VARCHAR(20),
		DeptCode VARCHAR(20),
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
		isjf INT,
		zxing varchar(2)
	)
	
	IF(isnull(@yzxz,0)=0 or @yzxz=2)
	BEGIN
		print '长期医嘱采集'
		INSERT INTO @OrderTab
		(Id ,OrganizeId ,zyh ,zh ,WardCode ,DeptCode ,ysgh,pcCode ,zxcs ,xmdm ,xmmc ,dw ,
			zbbz,sl ,yzlx ,zxsj ,zxr ,ypjl ,ypgg ,zxksdm,yzxz,isjf,zxing)
		SELECT Id,OrganizeId,zyh,zh,WardCode,DeptCode,ysgh,pccode,ISNULL(zxcs,1) ,xmdm,xmmc,dw,zbbz,sl,yzlx,zxsj,zxr,ypjl,ypgg,zxksdm,2 AS yzxz,isjf ,zxing
		FROM dbo.zy_cqyz a with(nolock)
		WHERE a.yzzt in (1,2)  and a.OrganizeId=@orgId
		AND Convert(DATE,kssj) <= Convert(DATE,@zxrq) 
		AND (tzsj IS NULL OR Convert(DATE,tzsj) >= Convert(DATE,@zxrq)) AND (zxsj IS NULL OR Convert(DATE,zxsj) < Convert(DATE,@zxrq))  
		and (zxsj is null or(a.zxzqdw='1' and abs(datediff(d,a.zxsj,@zxrq)%a.zxzq)=0))
		and a.zt=1 	and yzlx not IN (2,4,10)
		and a.zyh in(select zyh from @zyhTab) 

		IF @@error <> 0 
		BEGIN      
			SELECT 'F|获取长期医嘱信息失败'  
			ROLLBACK TRAN
			RETURN    
		END 
		
	END
	IF(isnull(@yzxz,0)=0 or @yzxz=1)
	BEGIN
		print '临时医嘱采集'
		INSERT INTO @OrderTab
		(Id ,OrganizeId ,zyh ,zh ,WardCode ,DeptCode ,ysgh,pcCode ,zxcs ,xmdm ,xmmc ,dw ,
			zbbz,sl ,yzlx ,zxsj ,zxr ,ypjl ,ypgg ,zxksdm,yzxz,isjf,zxing)
		SELECT Id,OrganizeId,zyh,zh,WardCode,DeptCode,ysgh,pccode,ISNULL(zxcs,1),xmdm,xmmc,dw,zbbz,sl,yzlx,zxsj,zxr,ypjl,ypgg,zxksdm,1 AS yzxz ,isjf,zxing
		FROM dbo.zy_lsyz a with(nolock)
		WHERE  a.yzzt =1  and a.OrganizeId=@orgId
		AND  Convert(DATE,kssj)< =Convert(DATE,@zxrq)  
		AND (zfsj IS NULL OR Convert(DATE,zfsj)>=Convert(DATE,@zxrq))  
		AND (zxsj IS NULL OR Convert(DATE,zxsj)<Convert(DATE,@zxrq)) 
		and a.zt=1 	and yzlx not IN (2,4,10)
		and a.zyh in (select zyh from @zyhTab) 
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|获取临时医嘱信息失败'  
			ROLLBACK TRAN
			RETURN    
		END 
	END

	IF(not exists(select 1 from @OrderTab))
	BEGIN
		SELECT 'F|无可执行医嘱'   
		ROLLBACK TRAN
		RETURN    
	END
	ELSE IF( exists(select 1 from @OrderTab where zxing='1'))
	BEGIN
		SELECT 'F|已有医嘱同步执行中，请刷新重试。'   
		ROLLBACK TRAN
		RETURN    
	END

	--锁定医嘱
	IF(exists(select 1 from @OrderTab where yzxz=2))
	BEGIN
		UPDATE dbo.zy_cqyz 
		SET zxing='1',LastModifierCode=@czyh,LastModifyTime=GETDATE() 
		WHERE Id IN (SELECT Id FROM @OrderTab where yzxz=2)
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|锁定执行长期医嘱失败。'   
			ROLLBACK TRAN
			RETURN    
		END 
	END
	IF(exists(select 1 from @OrderTab where yzxz=1))
	BEGIN
		UPDATE dbo.zy_lsyz 
		SET zxing='1',LastModifierCode=@czyh,LastModifyTime=GETDATE() 
		WHERE Id IN (SELECT Id FROM @OrderTab where yzxz=1)
		IF @@error <> 0 
		BEGIN      
			SELECT 'F|锁定执行临时医嘱失败。'   
			ROLLBACK TRAN
			RETURN    
		END 
	END

	IF(exists(select 1 from @OrderTab where yzlx not IN (2,4,10)))
	begin
		INSERT INTO dbo.zy_fymxk
		( Id ,OrganizeId ,zyh ,yzxh ,	zxrq ,qqrq ,xmdm ,xmmc ,gg ,dw ,sl ,dj , zje ,yzxz ,ybdm ,jzks ,gdzxbz ,yzlb ,WardCode ,DeptCode ,cwdm ,czyh ,ysgh ,ysksdm ,zxksdm ,CreateTime ,CreatorCode ,zt,isjf
		)
		SELECT NEWID(),a.OrganizeId,a.zyh,a.Id,@zxrq,GETDATE(),a.xmdm,a.xmmc,a.ypgg,b.dw,a.sl,b.dj AS dj,a.sl*b.dj AS zje,a.yzxz,b.ybdm,a.DeptCode,0,1,a.WardCode,a.DeptCode,d.BedCode,@czyh,a.ysgh,a.DeptCode,a.zxksdm,GETDATE(),@czyh,'1' ,isjf
		FROM @OrderTab a
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfxm b with(nolock) ON a.xmdm=b.sfxmCode AND a.OrganizeId=b.OrganizeId AND b.zt='1'
		LEFT JOIN dbo.zy_brxxk d with(nolock) ON a.zyh =d.zyh AND a.OrganizeId=d.OrganizeId AND d.zt='1'
		WHERE a.yzlx NOT IN (2,3,4,10,11) --2:药品 3:文字 4: 出院带药 10:中草药 11:康复
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
	END

	-----更新医嘱状态
	UPDATE dbo.zy_cqyz SET yzzt=2, zxsj=@zxrq,zxr=@czyh,zxing=null,LastModifierCode=@czyh,LastModifyTime=GETDATE() WHERE Id IN (SELECT Id FROM @OrderTab)
	IF @@error <> 0 
	BEGIN      
		SELECT 'F|更新长期医嘱信息失败'  
		ROLLBACK TRAN
		RETURN    
	END 
	UPDATE dbo.zy_lsyz SET yzzt=2, zxsj=@zxrq,zxr=@czyh,zxing=null,LastModifierCode=@czyh,LastModifyTime=GETDATE() WHERE Id IN (SELECT Id FROM @OrderTab)
	IF @@error <> 0 
	BEGIN      
		SELECT 'F|更新临时医嘱信息失败'  
		ROLLBACK TRAN
		RETURN    
	END 

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


