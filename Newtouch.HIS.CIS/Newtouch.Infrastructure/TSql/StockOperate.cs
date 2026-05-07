namespace Newtouch.Infrastructure.TSql
{
    /// <summary>
    /// 库存操作
    /// </summary>
    public class StockOperate
    {
        /// <summary>
        /// 减库存 无需解冻
        /// </summary>
        public const string just_subtract_kcsl = @"
BEGIN TRANSACTION
BEGIN TRY 
	IF @sl<=0
	BEGIN
		SELECT '消耗库存必须大于零';
		ROLLBACK TRANSACTION
		RETURN;
	END 

	DECLARE @totalKcsl INT;
	SELECT @totalKcsl=ISNULL(SUM(kcsl),0) FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt='1' 
	IF @totalKcsl<@sl
	BEGIN
		SELECT '库存不足，出库失败';
		ROLLBACK TRANSACTION;
		RETURN
	END 

	IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#allkcxx') and type='U')
	BEGIN
		DROP TABLE #allkcxx;
	END
	SELECT Id, kcsl INTO #allkcxx FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt='1' 

	DECLARE @kcsl INT; --当前可用库存
	DECLARE @kcId VARCHAR(50);
	DECLARE @sykc INT;
	SET @sykc=@sl;

	WHILE(EXISTS(SELECT 1 FROM #allkcxx) AND @sykc>0)
	BEGIN
		SELECT TOP 1 @kcId=Id,@kcsl=kcsl FROM #allkcxx
		IF @kcsl>=@sykc
		BEGIN
			UPDATE dbo.Dept_kcxx SET kcsl=kcsl-@sykc, LastModifyTime=GETDATE(),LastModifierCode=@userCode
			WHERE Id=@kcId 
			AND productId=@proId 
			AND ks=@warehouseId 
			AND OrganizeId=@OrganizeId 
			AND pc=@pc 
			AND ph=@ph 
			AND zt='1'  
			SET @sykc=0;
		END 
		ELSE 
		BEGIN
			UPDATE dbo.Dept_kcxx SET kcsl=0,LastModifyTime=GETDATE(),LastModifierCode=@userCode
			WHERE Id=@kcId 
			AND productId=@proId 
			AND ks=@warehouseId 
			AND OrganizeId=@OrganizeId 
			AND pc=@pc 
			AND ph=@ph 
			AND zt='1'  
			SET @sykc=@sykc-@kcsl;
		END 
		DELETE FROM #allkcxx WHERE Id=@kcId;
	END 
	COMMIT TRANSACTION
	SELECT '';
END TRY
BEGIN CATCH
	SELECT ERROR_MESSAGE();
	ROLLBACK TRANSACTION
	RETURN
END CATCH
";
        /// <summary>
        /// 只冻结库存
        /// </summary>
        public const string just_frozen_kcsl = @"
BEGIN TRANSACTION
BEGIN TRY
	DECLARE @dqkykc INT; --当前可用库存
	SELECT @dqkykc=SUM(kcsl-djsl) FROM dbo.Dept_kcxx WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt='1' AND ks=@warehouseId
	IF @sl > @dqkykc
	BEGIN
		SELECT '库存不足，申请失败';
		ROLLBACK TRANSACTION;
		RETURN
	END
	IF @sl <=0
	BEGIN
		SELECT '需冻结库存必须大于零';
		ROLLBACK TRANSACTION;
		RETURN
	END

	IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#allkcxx') and type='U')
	BEGIN
		DROP TABLE #allkcxx;
	END
	SELECT Id kcId,(kcsl-djsl) kykc INTO #allkcxx FROM dbo.Dept_kcxx WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt='1' AND ks=@warehouseId
	DECLARE @kcId VARCHAR(50),@kykc INT, @sysl INT;
	SET @sysl=@sl
	WHILE EXISTS(SELECT 1 FROM #allkcxx) AND @sysl>0
	BEGIN
		SELECT TOP 1 @kcId=kcId, @kykc=kykc FROM #allkcxx
		IF @kykc>=@sysl
		BEGIN
			UPDATE dbo.Dept_kcxx 
			SET djsl=djsl+@sysl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
			WHERE Id=@kcId AND productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt='1' AND ks=@warehouseId
			SET @sysl=0;
		END 
		ELSE
        BEGIN
			UPDATE dbo.Dept_kcxx
			SET djsl=djsl+@kykc, LastModifyTime=GETDATE(), LastModifierCode=@userCode
			WHERE Id=@kcId AND productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt='1' AND ks=@warehouseId
			SET @sysl=@sysl-@kykc
		END 

		DELETE FROM #allkcxx WHERE kcId=@kcId;
	END 
	
	COMMIT TRANSACTION;
	SELECT '';
END TRY
BEGIN CATCH
	SELECT ERROR_MESSAGE();
	ROLLBACK TRANSACTION;
	RETURN ;
END CATCH
";

        /// <summary>
        /// 减冻结并扣库存  一般用作审核通过
        /// </summary>
        public const string subtract_freeze_and_kcsl = @"
BEGIN TRANSACTION
BEGIN TRY
	DECLARE @kcsl INT; --当前库存
	DECLARE @djsl INT; --当前冻结库存
	SELECT @kcsl=ISNULL(SUM(kcsl),0), @djsl=ISNULL(SUM(djsl),0)
	FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt='1' AND ks=@warehouseId AND djsl>0 AND kcsl>0
	IF @kcsl<=0 OR @djsl<=0
	BEGIN
		SELECT '库存中未有已冻结物资';
		ROLLBACK TRANSACTION;
		RETURN;
	END 
	IF @kcsl<@sl OR @kcsl<@djsl
	BEGIN
		SELECT '已冻数量小于出库数量，禁止出库';
		ROLLBACK TRANSACTION;
		RETURN;
	END 
	
	IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#allkcxx') and type='U')
	BEGIN
		DROP TABLE #allkcxx;
	END

	SELECT Id kcId, kcsl, djsl INTO #allkcxx 
	FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND zt='1' AND ks=@warehouseId AND djsl>0 AND kcsl>0
	
	DECLARE @sysl INT,@curkcId VARCHAR(50), @curkcsl INT, @curdjsl INT, @tmpsl int;
	SET @tmpsl=0;
	SET @sysl=@sl;
	WHILE EXISTS(SELECT 1 FROM #allkcxx) AND @sysl>0
	BEGIN
		SELECT TOP 1 @curkcId=kcId, @curkcsl=kcsl, @curdjsl=djsl FROM #allkcxx
		IF @sysl<=@curdjsl AND @sysl<=@curkcsl
		BEGIN
			UPDATE dbo.Dept_kcxx SET kcsl=kcsl-@sysl, djsl=djsl-@sysl, LastModifyTime=GETDATE(), LastModifierCode=@userCode
			WHERE Id=@curkcId AND productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt='1'
			SET @sysl=0
		END 
		ELSE 
		BEGIN
			--取冻结数和库存数中最小的一个赋值给@tmpsl
			IF @curdjsl<=@curkcsl
			BEGIN 
				SET @tmpsl=@curdjsl;
			END 
			ELSE
            BEGIN
				SET @tmpsl=@curkcsl;
			END 
			UPDATE dbo.Dept_kcxx SET kcsl=kcsl-@tmpsl, djsl=djsl-@tmpsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode
			WHERE Id=@curkcId AND productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt='1'
			SET @sysl=@sysl-@tmpsl;
		END 

		DELETE FROM #allkcxx WHERE kcId=@curkcId;
	END
    	
	COMMIT TRANSACTION;
	SELECT '';
END TRY
BEGIN CATCH
	SELECT ERROR_MESSAGE();
	ROLLBACK TRANSACTION;
	RETURN ;
END CATCH
";

        /// <summary>
        /// 仅解冻
        /// </summary>
        public const string just_unfreeze = @"
BEGIN TRANSACTION
BEGIN TRY 
	IF @sl<=0
	BEGIN
		SELECT '需要解冻的数量必须大于零'
		ROLLBACK TRANSACTION
		RETURN;
	END 
	DECLARE @totalDjsl INT;
	SELECT @totalDjsl=ISNULL(SUM(djsl),0) 
	FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt='1' AND djsl>0
	
	IF @sl>@totalDjsl
	BEGIN
		SELECT '已冻结总量小于需要解冻数，解冻失败'
		ROLLBACK TRANSACTION
		RETURN;
	END 
	IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#allkcxx') and type='U')
	BEGIN
		DROP TABLE #allkcxx;
	END
	SELECT Id, djsl INTO #allkcxx FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt='1' AND djsl>0
		
	DECLARE @djsl INT; --当前冻结数量
	DECLARE @kcId VARCHAR(50); --当前库存ID
	DECLARE @sysl INT; --剩余数量
	SET @sysl=@sl; 
	WHILE(EXISTS(SELECT 1 FROM #allkcxx) AND @sysl>0)
	BEGIN
		SELECT TOP 1 @kcId=Id,@djsl=djsl FROM #allkcxx
		IF @djsl>=@sysl
		BEGIN
			UPDATE dbo.Dept_kcxx SET djsl=djsl-@sysl, LastModifyTime=GETDATE(),LastModifierCode=@userCode
			WHERE Id=@kcId 
			SET @sysl=0;
		END 
		ELSE 
		BEGIN
			UPDATE dbo.Dept_kcxx SET djsl=0,LastModifyTime=GETDATE(),LastModifierCode=@userCode
			WHERE Id=@kcId 
			SET @sysl=@sysl-@djsl;
		END 
		DELETE FROM #allkcxx WHERE Id=@kcId;
	END 
	COMMIT TRANSACTION
	SELECT '';
END TRY
BEGIN CATCH
	SELECT ERROR_MESSAGE();
	ROLLBACK TRANSACTION
	RETURN
END CATCH
";

        /// <summary>
        /// 作废  减库存
        /// </summary>
        public const string cancel_subtract_kcsl = @"
BEGIN TRANSACTION
BEGIN TRY 
	IF @sl<=0
	BEGIN
		SELECT '需作废的库存数量必须大于零';
		ROLLBACK TRANSACTION;
		RETURN;
	END 
	DECLARE @totalDjsl INT;
	DECLARE @totalKcsl INT;
	SELECT @totalKcsl=ISNULL(SUM(kcsl),0), @totalDjsl=ISNULL(SUM(djsl),0) FROM dbo.Dept_kcxx 
	WHERE pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId AND ks=@warehouseId AND productId=@proId AND zt=@zt
	IF @totalDjsl>0  
	BEGIN
		-- 有冻结说明正在被使用，作废可能导致后期扣库存库存不足
		SELECT '该批次存在冻结库存，不能作废';
		ROLLBACK TRANSACTION;
		RETURN;
	END
	IF @totalKcsl<@sl
	BEGIN
		SELECT '库存不足，不能作废';
		ROLLBACK TRANSACTION;
		RETURN;
	END

	IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#allkcxx') and type='U')
	BEGIN
		DROP TABLE #allkcxx;
	END
	SELECT Id, kcsl INTO #allkcxx FROM dbo.Dept_kcxx 
	WHERE productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt=@zt

	DECLARE @kcsl INT; --当前可用库存
	DECLARE @kcId VARCHAR(50); --当前库存ID
	DECLARE @sykc INT; --剩余库存
	SET @sykc=@sl;

	WHILE(EXISTS(SELECT 1 FROM #allkcxx) AND @sykc>0)
	BEGIN
		SELECT TOP 1 @kcId=Id,@kcsl=kcsl FROM #allkcxx
		IF @kcsl>=@sykc
		BEGIN
			UPDATE dbo.Dept_kcxx SET kcsl=kcsl-@sykc, LastModifyTime=GETDATE(),LastModifierCode=@userCode
			WHERE Id=@kcId AND productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt=@zt
			SET @sykc=0;
		END 
		ELSE 
		BEGIN
			UPDATE dbo.Dept_kcxx SET kcsl=0,LastModifyTime=GETDATE(),LastModifierCode=@userCode
			WHERE Id=@kcId AND productId=@proId AND ks=@warehouseId AND OrganizeId=@OrganizeId AND pc=@pc AND ph=@ph AND zt=@zt
			SET @sykc=@sykc-@kcsl;
		END 
		DELETE FROM #allkcxx WHERE Id=@kcId;
	END 
	COMMIT TRANSACTION
	SELECT '';
END TRY
BEGIN CATCH
	SELECT ERROR_MESSAGE();
	ROLLBACK TRANSACTION
	RETURN
END CATCH
";
    }
}
