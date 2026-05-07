namespace Newtouch.Infrastructure.TSQL
{
    /// <summary>
    /// 发药
    /// </summary>
    public class TSqlDispensing
    {
        #region 门诊预定 冻结库存

        /// <summary>
        /// 资源预定
        /// </summary>
        public const string mz_yp_book = @"
DECLARE @ypmc VARCHAR(200);--药品名称
DECLARE @yfmc VARCHAR(200);--药房名称
SELECT @ypmc=yp.ypmc 
FROM NewtouchHIS_Base.dbo.V_S_xt_yp yp
INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx ypsx ON ypsx.ypId=yp.ypId AND ypsx.OrganizeId=yp.OrganizeId
WHERE yp.ypCode=@ypdm AND yp.OrganizeId=@OrganizeId

--验证 1：药品是否有效
IF ISNULL(@ypmc, '')=''
BEGIN
	SELECT '药品无效';
	RETURN;
END 

SELECT @yfmc=yfbmmc FROM NewtouchHIS_Base.dbo.V_S_xt_yfbm WHERE yfbmCode=@yfbmCode AND zt='1' AND OrganizeId=@OrganizeId

--验证 2：药房是否有效
IF ISNULL(@yfmc, '')=''
BEGIN
	SELECT '指定的药房暂未开放';
	RETURN;
END 

--验证 3：部门是否拥有该药品
IF NOT EXISTS(SELECT 1 FROM dbo.xt_yp_bmypxx(NOLOCK) WHERE yfbmCode=@yfbmCode AND Ypdm=@ypdm AND OrganizeId=@OrganizeId AND zt='1')
BEGIN
	SELECT CONCAT(@yfmc, '未开放', @ypmc) ;
	RETURN;
END

SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ COMMITTED

DECLARE @kyck INT
DECLARE @curkcId VARCHAR(50); 
DECLARE @curkysl INT;
DECLARE @sykc INT;--剩余库存
DECLARE @yxq DATETIME;

IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#kcData') AND type='U')
BEGIN
	DROP TABLE #kcData;
END

BEGIN TRY

	BEGIN TRANSACTION
	SELECT kcId, kcsl, djsl, (kcsl-djsl) kysl, ph, pc, yxq 
	INTO #kcData FROM dbo.xt_yp_kcxx WITH (XLOCK, ROWLOCK) 
	WHERE (kcsl-djsl)>0 AND ypdm=@ypdm AND yfbmCode=@yfbmCode AND OrganizeId=@OrganizeId AND zt='1'

	SET @kyck=(SELECT ISNULL(SUM(kysl),0) FROM #kcData);
	SET @sykc=@zxdwsl;
	IF @kyck<@zxdwsl
	BEGIN
		SELECT CONCAT(@ypmc, '库存不足');
		COMMIT TRANSACTION ;
		RETURN;
	END
	WHILE @sykc > 0 AND EXISTS(SELECT 1 FROM #kcData)
	BEGIN
		SELECT TOP 1 @curkcId=kcId, @curkysl=kysl FROM #kcData ORDER BY yxq
		DECLARE @curkjsl INT;--扣减数量
		IF @curkysl>@sykc 
		BEGIN
			--当前批号批次库存足够
			SET @curkjsl=@sykc;
			SET @sykc=0;		
		END
		ELSE
		BEGIN
			--当前批号批次库存不足
			SET @curkjsl=@curkysl;
			SET @sykc=@sykc-@curkysl;
		END  
		UPDATE dbo.xt_yp_kcxx SET djsl=djsl+@curkjsl, LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE kcId=@curkcId;
	
		INSERT INTO dbo.mz_cfmxph( cfmxphId ,OrganizeId ,yp ,ph ,pc ,sl, yxq, czh ,zt ,px ,CreatorCode ,CreateTime ,LastModifyTime ,LastModifierCode ,gjzt ,fyyf ,cfh,cfmxId)
		SELECT    NEWID() , -- cfmxphId - varchar(50)
				  @OrganizeId , -- OrganizeId - varchar(50)
				  @ypdm , -- yp - varchar(50)
				  ph , -- ph - varchar(30)
				  pc , -- pc - varchar(30)
				  @curkjsl , -- sl - numeric
				  yxq , --yxq -datetime
				  @czh , --czh -varchar(50)
				  '1' , -- zt - char(1)
				  0 , -- px - int
				  @CreatorCode , -- CreatorCode - varchar(50)
				  GETDATE() , -- CreateTime - datetime
				  NULL , -- LastModifyTime - datetime
				  '' , -- LastModifierCode - varchar(50)
				  '0' , -- gjzt - char(1)
				  @yfbmCode , -- fyyf - varchar(50)
				  @cfh , -- cfh - varchar(50)
                  @cfmxId
		FROM #kcData WHERE kcId=@curkcId ;

		DELETE FROM #kcData WHERE kcId=@curkcId
	END 
	COMMIT TRANSACTION
	SELECT '';
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION
	SELECT ERROR_MESSAGE();
END CATCH   
";


        /// <summary>
        /// 门诊资源预定（冻结库存，生成排药明细）
        /// </summary>
        public const string mz_yp_book_v2 = @"
DECLARE @resultMsg VARCHAR(500);
SET @resultMsg=''
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#rpDetail') AND type='U')
BEGIN
	DROP TABLE #rpDetail;
END

SELECT cfmx.* INTO #rpDetail 
FROM dbo.mz_cfmx(NOLOCK) cfmx
INNER JOIN dbo.mz_cf(NOLOCK) cf ON cf.cfh=cfmx.cfh AND cf.zt='1' AND cf.OrganizeId=cfmx.OrganizeId  
WHERE cf.cfh=@cfh AND cfmx.zt='1' AND cfmx.OrganizeId=@OrganizeId AND cf.jsnm=0

IF NOT EXISTS(SELECT 1 FROM #rpDetail)
BEGIN
	SET @resultMsg=CONCAT('处方【',@cfh,'】未查到有效明细');
END 
ELSE
BEGIN
	DECLARE @ypCode VARCHAR(50), @ypmc VARCHAR(256), @sl INT, @zxdwsl INT, @zhyz INT, @Id BIGINT, @czh VARCHAR(50);
	DECLARE @ypId INT, @bmypId VARCHAR(50), @yfbmId INT, @yfbmmc VARCHAR(256);
	
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	BEGIN TRY
    BEGIN TRANSACTION

	DECLARE @kyck INT
	DECLARE @curkcId VARCHAR(50); 
	DECLARE @curkysl INT;
	DECLARE @sykc INT;--剩余库存
	DECLARE @yxq DATETIME;

	WHILE EXISTS(SELECT 1 FROM #rpDetail)
	BEGIN
		SELECT TOP 1 @ypCode=ypCode, @ypmc=ypmc, @sl=sl, @zxdwsl=sl*zhyz, @zhyz=zhyz, @czh=czh, @Id=Id FROM #rpDetail
			
		SELECT @ypId=yp.ypId, @bmypId=bmypxx.bmypId, @yfbmId=yfbm.yfbmId, @yfbmmc=yfbm.yfbmmc
		FROM NewtouchHIS_Base.dbo.V_C_xt_yp yp
		LEFT JOIN dbo.xt_yp_bmypxx(NOLOCK) bmypxx ON bmypxx.Ypdm=yp.ypCode AND bmypxx.OrganizeId=yp.OrganizeId AND bmypxx.yfbmCode=@yfbmCode AND bmypxx.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm ON yfbm.yfbmCode=@yfbmCode AND yfbm.OrganizeId = yp.OrganizeId AND yfbm.zt='1'
		WHERE yp.ypCode=@ypCode 
		AND yp.zt='1'
		AND yp.OrganizeId=@OrganizeId

		--效验 1:药品是否有效
		IF ISNULL(@ypId,0)=0
		BEGIN
			SET @resultMsg=CONCAT('处方【',@cfh,'】的药品【',@ypmc,'】无效');
			BREAK;
		END 
		
		--验证 2：药房是否有效
		IF ISNULL(@yfbmId,0)=0
		BEGIN
			SET @resultMsg=CONCAT('药房部门代码为【',@yfbmCode, '】的药房未开放') ;
			BREAK;
		END 

		--验证 3：部门是否拥有该药品
		IF ISNULL(@bmypId,'')=''
		BEGIN
			SET @resultMsg=CONCAT('药房部门【',@yfbmmc,'】没有药品【',@ypmc, '】的权限') ;
			BREAK;
		END

		SET @kyck=0;--可用库存
		SET @curkcId=''; --当前库存ID
		SET @curkysl =0;--当前库存数量
		SET @sykc =0;--剩余库存
		SET @yxq='';--有效期

		IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#kcData') AND type='U')
		BEGIN
			DROP TABLE #kcData;
		END
		
		SELECT kcId, kcsl, djsl, (kcsl-djsl) kysl, ph, pc, yxq 
		INTO #kcData FROM dbo.xt_yp_kcxx WITH (XLOCK, ROWLOCK) 
		WHERE ypdm=@ypCode AND yfbmCode=@yfbmCode AND OrganizeId=@OrganizeId AND zt='1'

		IF NOT EXISTS(SELECT 1 FROM #kcData)
		BEGIN
			SET @resultMsg= CONCAT('药品【',@ypmc, '】未找到有效库存，该库存药品可能被控制');
			BREAK;
		END 

		DELETE FROM #kcData WHERE (kcsl-djsl)<=0 

		SET @kyck=(SELECT ISNULL(SUM(kysl),0) FROM #kcData);
		SET @sykc=@zxdwsl;
		IF @kyck<@zxdwsl
		BEGIN
			SET @resultMsg= CONCAT('药品【',@ypmc, '】库存不足');
			BREAK;
		END

		WHILE @sykc > 0 AND EXISTS(SELECT 1 FROM #kcData)
		BEGIN
			SELECT TOP 1 @curkcId=kcId, @curkysl=kysl FROM #kcData ORDER BY yxq
			DECLARE @curkjsl INT;--扣减数量
			IF @curkysl>=@sykc 
			BEGIN
				--当前批号批次库存足够
				SET @curkjsl=@sykc;
				SET @sykc=0;		
			END
			ELSE
			BEGIN
				--当前批号批次库存不足
				SET @curkjsl=@curkysl;
				SET @sykc=@sykc-@curkysl;
			END  
			UPDATE dbo.xt_yp_kcxx SET djsl=djsl+@curkjsl, LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE kcId=@curkcId;
	
			INSERT INTO dbo.mz_cfmxph( cfmxphId ,OrganizeId ,yp ,ph ,pc ,sl, yxq, czh ,zt ,px ,CreatorCode ,CreateTime ,LastModifyTime ,LastModifierCode ,gjzt ,fyyf ,cfh, cfmxId)
			SELECT    NEWID() , -- cfmxphId - varchar(50)
					  @OrganizeId , -- OrganizeId - varchar(50)
					  @ypCode , -- yp - varchar(50)
					  ph , -- ph - varchar(30)
					  pc , -- pc - varchar(30)
					  @curkjsl , -- sl - numeric
					  yxq , --yxq -datetime
					  @czh , --czh -varchar(50)
					  '1' , -- zt - char(1)
					  0 , -- px - int
					  @CreatorCode , -- CreatorCode - varchar(50)
					  GETDATE() , -- CreateTime - datetime
					  NULL , -- LastModifyTime - datetime
					  '' , -- LastModifierCode - varchar(50)
					  '0' , -- gjzt - char(1)
					  @yfbmCode , -- fyyf - varchar(50)
					  @cfh,  -- cfh - varchar(50)
					  @Id --cfmxId BIGINT 处方明细ID
			FROM #kcData WHERE kcId=@curkcId ;

			DELETE FROM #kcData WHERE kcId=@curkcId
		END 

		DELETE FROM #rpDetail WHERE Id=@Id
	END
	 
	IF(@resultMsg='')
	BEGIN
		COMMIT TRANSACTION;
	END
	ELSE
    BEGIN
		ROLLBACK TRANSACTION;
	END 
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SET @resultMsg=ERROR_MESSAGE();
	END CATCH
END

SELECT @resultMsg resultMsg;
";

        #endregion

        #region 门诊取消预定 解冻

        /// <summary>
        /// 门诊取消预定 解冻 （精简版）
        /// </summary>
        public const string mz_yp_book_cancel_simplify = @"
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#mxph') AND type='U')
BEGIN
	DROP TABLE #mxph;
END

SELECT * INTO #mxph FROM dbo.mz_cfmxph(NOLOCK) WHERE cfh=@cfh AND OrganizeId=@OrganizeId AND gjzt='0' AND zt='1' AND fyyf=@yfbmCode

DECLARE @resultMsg VARCHAR(500);
SET @resultMsg='';
	
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ COMMITTED
BEGIN TRY
BEGIN TRANSACTION

IF EXISTS(SELECT 1 FROM #mxph)
BEGIN
	DECLARE @Id VARCHAR(50), @ypCode VARCHAR(50), @pc VARCHAR(50), @ph VARCHAR(50), @zxsl INT;

	WHILE EXISTS(SELECT 1 FROM #mxph)
	BEGIN
		SELECT TOP 1 @Id=cfmxphId, @ypCode=yp, @pc=pc, @ph=ph, @zxsl=CONVERT(INT,sl) FROM #mxph
		IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#kcxx') AND type='U')
		BEGIN
			DROP TABLE #kcxx;
		END
		
		SELECT yp.ypmc, kcxx.* INTO #kcxx 
		FROM NewtouchHIS_Base.dbo.V_C_xt_yp yp
		INNER JOIN xt_yp_kcxx(NOLOCK) kcxx ON yp.ypCode=kcxx.ypdm AND kcxx.yfbmCode = @yfbmCode AND yp.OrganizeId=kcxx.OrganizeId AND kcxx.zt='1' AND kcxx.tybz = '0' AND kcxx.djsl>0 AND kcxx.pc=@pc AND kcxx.ph=@ph
		WHERE yp.ypCode = @ypCode AND yp.OrganizeId = @OrganizeId AND yp.zt='1'
				
		IF EXISTS(SELECT 1 FROM #kcxx) 
		BEGIN
			DECLARE @totalDjsl INT;
			SELECT @totalDjsl=ISNULL(SUM(djsl),0) FROM #kcxx

			IF @totalDjsl>=@zxsl
			BEGIN
				DECLARE @sykc INT, @curKcId VARCHAR(50), @curDjsl INT;
				SET @sykc=@zxsl;
				WHILE EXISTS(SELECT 1 FROM #kcxx) AND @sykc>0
				BEGIN
					SELECT TOP 1 @curKcId=kcId, @curDjsl=djsl FROM #kcxx
					IF @curDjsl>=@sykc
					BEGIN
						UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@sykc, LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE kcId=@curKcId
						SET @sykc=0;
					END 
					ELSE
					BEGIN
						UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@curDjsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE kcId=@curKcId
						SET @sykc=@sykc-@curDjsl;
					END
					DELETE FROM #kcxx WHERE kcId=@curKcId;
				END 
			END 
		END 

		UPDATE dbo.mz_cfmxph SET zt='0', gjzt='1', LastModifyTime=GETDATE(), LastModifierCode=@userCode WHERE cfmxphId=@Id

		DELETE FROM #mxph WHERE cfmxphId=@Id
	END 
END 
UPDATE dbo.mz_cf SET fybz='0', LastModiFierCode=@userCode, LastModifyTime=GETDATE() WHERE cfh=@cfh AND OrganizeId=@OrganizeId AND zt='1'

IF(@resultMsg='')
BEGIN
	COMMIT TRANSACTION;
END
ELSE
BEGIN
	ROLLBACK TRANSACTION;
END 
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
	SET @resultMsg=ERROR_MESSAGE();
END CATCH

SELECT @resultMsg resultMsg;
";
        /// <summary>
        /// 门诊取消预定 解冻--按照cfmxphid解冻
        /// </summary>
        public const string mz_yp_book_cancel_v2 = @"
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#ArrData') AND type='U')
BEGIN
	DROP TABLE #ArrData; --排药数据
END

BEGIN TRY
DECLARE @cfId BIGINT, @ph VARCHAR(50), @pc VARCHAR(50), @kcsl INT, @cfmxphId VARCHAR(50), @curKcId VARCHAR(50), @fybz CHAR(1), @jsnm BIGINT;

SELECT @cfId=Id, @fybz=fybz, @jsnm=jsnm FROM dbo.mz_cf WHERE cfh=@cfh AND zt='1' AND OrganizeId=@OrganizeId AND lyyf=@yfbmCode   

IF ISNULL(@cfId, 0)=0
BEGIN
	SELECT '未找到指定处方';
	RETURN ;
END
IF ISNULL(@jsnm, 0)>0
BEGIN
	SELECT '处方已结算，不能取消预定';
	RETURN ;
END 

SELECT * 
INTO #ArrData FROM dbo.mz_cfmxph	
WHERE yp=@ypdm AND cfmxphid=@oldcfmxphid AND OrganizeId=@OrganizeId AND fyyf=@yfbmCode AND zt='1' AND gjzt='0' AND cfh=@cfh 

IF NOT EXISTS(SELECT 1 FROM #ArrData)
BEGIN
	SELECT '';
	RETURN;
END 
DECLARE @bookCout INT;
SELECT @bookCout=SUM(sl) FROM #ArrData
IF @bookCout<@sl
BEGIN
	SELECT '取消数量不得大于已排数量';
	RETURN;
END 
DECLARE @sysl INT;
SET @sysl=@sl;

DECLARE @errorMsg VARCHAR(500);
SET @errorMsg='';
BEGIN TRANSACTION
	WHILE EXISTS(SELECT 1 FROM #ArrData) AND @sysl>0
	BEGIN
		SELECT TOP 1 @cfmxphId=cfmxphId, @ph=ph, @pc=pc, @kcsl=sl FROM #ArrData ORDER BY yxq asc
		SELECT TOP 1 @curKcId=ISNULL(kcId,'') 
		FROM dbo.xt_yp_kcxx WITH (XLOCK, ROWLOCK) 
		WHERE ypdm=@ypdm AND yfbmCode=@yfbmCode AND ph=@ph AND pc=@pc AND OrganizeId=@OrganizeId 
		IF @curKcId=''
		BEGIN
			DECLARE @ypmc VARCHAR(256);
			SELECT @ypmc=ypmc FROM NewtouchHIS_Base.dbo.V_C_xt_yp WHERE ypCode=@ypdm AND OrganizeId=@OrganizeId AND zt='1'
			SET @errorMsg=CONCAT('处方【',@cfh,'】未找到冻结时所用批号为【',@ph,'】的药品【',@ypmc,'】。');
			BREAK;
		END 
		IF @kcsl>@sysl
		BEGIN
			SET @kcsl=@sysl;
			SET @sysl=0;
		END
		ELSE
		BEGIN
			SET @sysl=@sysl-@kcsl;
			UPDATE dbo.mz_cfmxph SET gjzt='1', zt='0', LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE cfmxphId=@cfmxphId
		END  
		UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@kcsl, LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE kcId=@curKcId ;
		DELETE FROM #ArrData WHERE cfmxphId=@cfmxphId
	END 
	IF(@errorMsg='')
	BEGIN
		COMMIT TRANSACTION;
	END
	ELSE
    BEGIN
		ROLLBACK TRANSACTION;
	END 
	
	SELECT @errorMsg;
END TRY
BEGIN CATCH
	IF @@TRANCOUNT>0
	BEGIN
		ROLLBACK TRANSACTION
	END 
	SELECT ERROR_MESSAGE();
END CATCH
";

        /// <summary>
        /// 门诊取消预定 解冻
        /// </summary>
        public const string mz_yp_book_cancel = @"
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#ArrData') AND type='U')
BEGIN
	DROP TABLE #ArrData; --排药数据
END

BEGIN TRY
DECLARE @cfId BIGINT, @ph VARCHAR(50), @pc VARCHAR(50), @kcsl INT, @cfmxphId VARCHAR(50), @curKcId VARCHAR(50), @fybz CHAR(1), @jsnm BIGINT;

SELECT @cfId=Id, @fybz=fybz, @jsnm=jsnm FROM dbo.mz_cf WHERE cfh=@cfh AND zt='1' AND OrganizeId=@OrganizeId AND lyyf=@yfbmCode   

IF ISNULL(@cfId, 0)=0
BEGIN
	SELECT '未找到指定处方';
	RETURN ;
END
IF ISNULL(@jsnm, 0)>0
BEGIN
	SELECT '处方已结算，不能取消预定';
	RETURN ;
END 

SELECT * 
INTO #ArrData FROM dbo.mz_cfmxph	
WHERE yp=@ypdm AND OrganizeId=@OrganizeId AND fyyf=@yfbmCode AND zt='1' AND gjzt='0' AND cfh=@cfh 

IF NOT EXISTS(SELECT 1 FROM #ArrData)
BEGIN
	SELECT '';
	RETURN;
END 
DECLARE @bookCout INT;
SELECT @bookCout=SUM(sl) FROM #ArrData
IF @bookCout<@sl
BEGIN
	SELECT '取消数量不得大于已排数量';
	RETURN;
END 
DECLARE @sysl INT;
SET @sysl=@sl;

DECLARE @errorMsg VARCHAR(500);
SET @errorMsg='';
BEGIN TRANSACTION
	WHILE EXISTS(SELECT 1 FROM #ArrData) AND @sysl>0
	BEGIN
		SELECT TOP 1 @cfmxphId=cfmxphId, @ph=ph, @pc=pc, @kcsl=sl FROM #ArrData ORDER BY yxq asc
		SELECT TOP 1 @curKcId=ISNULL(kcId,'') 
		FROM dbo.xt_yp_kcxx WITH (XLOCK, ROWLOCK) 
		WHERE ypdm=@ypdm AND yfbmCode=@yfbmCode AND ph=@ph AND pc=@pc AND OrganizeId=@OrganizeId 
		IF @curKcId=''
		BEGIN
			DECLARE @ypmc VARCHAR(256);
			SELECT @ypmc=ypmc FROM NewtouchHIS_Base.dbo.V_C_xt_yp WHERE ypCode=@ypdm AND OrganizeId=@OrganizeId AND zt='1'
			SET @errorMsg=CONCAT('处方【',@cfh,'】未找到冻结时所用批号为【',@ph,'】的药品【',@ypmc,'】。');
			BREAK;
		END 
		IF @kcsl>@sysl
		BEGIN
			SET @kcsl=@sysl;
			SET @sysl=0;
		END
		ELSE
		BEGIN
			SET @sysl=@sysl-@kcsl;
			UPDATE dbo.mz_cfmxph SET gjzt='1', zt='0', LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE cfmxphId=@cfmxphId
		END  
		UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@kcsl, LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE kcId=@curKcId ;
		DELETE FROM #ArrData WHERE cfmxphId=@cfmxphId
	END 
	IF(@errorMsg='')
	BEGIN
		COMMIT TRANSACTION;
	END
	ELSE
    BEGIN
		ROLLBACK TRANSACTION;
	END 
	
	SELECT @errorMsg;
END TRY
BEGIN CATCH
	IF @@TRANCOUNT>0
	BEGIN
		ROLLBACK TRANSACTION
	END 
	SELECT ERROR_MESSAGE();
END CATCH
";

        #endregion

        #region 门诊发药处方明细查询

        /// <summary>
        /// 门诊发药处方明细查询
        /// </summary>
        public const string mz_fy_cfmx = @"
SELECT a.czh, a.Fph, a.cfh,convert(varchar(50),a.cfnm) cfnm,a.ypCode, a.ypmc, a.gg, CONVERT(INT,a.zxdwsl/a.zhyz) sl, a.dw, a.dj, a.ycmc, a.je, ISNULL(a.jl,0) jl, a.jldw, a.yfmc, a.yszt, a.ysmc, a.sfsj,a.gjybdm
FROM (
	SELECT cf.Fph, cf.cfh,cf.cfnm,cfmx.ypCode, cfmx.ypmc, cfmx.gg, SUM(mxph.sl) zxdwsl, cfmx.dw, cfmx.dj, cfmx.ycmc, cfmx.je, cfmx.jl, cfmx.jldw, cfmx.yfmc
	,cfmx.bz yszt, cf.ysmc, cf.sfsj, cfmx.czh, cfmx.zhyz,xtyp.gjybdm
	FROM dbo.mz_cf(NOLOCK) cf 
	INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh = cf.cfh AND cfmx.OrganizeId=cf.OrganizeId AND cfmx.zt='1'
	INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.cfh=cfmx.cfh AND mxph.gjzt='0' AND mxph.yp=cfmx.ypCode AND mxph.OrganizeId=cfmx.OrganizeId AND mxph.fyyf=cf.lyyf AND mxph.zt='1'
	INNER JOIN [NewtouchHIS_Base].dbo.xt_ypsx(NOLOCK) xtyp ON xtyp.ypcode = cfmx.ypcode AND cfmx.OrganizeId=xtyp.OrganizeId AND xtyp.zt='1'
	WHERE cf.OrganizeId=@OrganizeId
	AND cf.zt='1'
	AND cf.jsnm>0
	AND cf.lyyf=@yfbmCode
	AND cf.CardNo=@CardNo
	AND ISNULL(cf.cfh,'')=ISNULL(@cfh,'')
	AND ISNULL(mxph.czh,'')=ISNULL(cfmx.czh,'')
	AND mxph.cfmxId=cfmx.Id
	GROUP BY cf.Fph, cf.cfh,cf.cfnm, cfmx.ypCode, cfmx.ypmc, cfmx.gg, cfmx.dw, cfmx.dj, cfmx.ycmc, cfmx.je, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz, cf.ysmc, cf.sfsj, cfmx.czh, cfmx.zhyz,xtyp.gjybdm
) a 
";
		public const string mz_fy_cfmx_new = @"
SELECT a.czh, a.Fph, a.cfh,convert(varchar(50),a.cfnm) cfnm,a.ypCode, a.ypmc, a.gg, CONVERT(INT,a.zxdwsl/a.zhyz) sl,
dbo.f_getYfbmYpComplexYpSlandDwV2(a.zxdwsl,@yfbmCode,a.ypCode,@OrganizeId,'1' ) slStr,a.dw, a.dj, a.ycmc, a.je, ISNULL(a.jl,0) jl, a.jldw, a.yfmc, a.yszt, a.ysmc, a.sfsj,a.gjybdm,ph, pc
FROM (
	SELECT cf.Fph, cf.cfh,cf.cfnm,cfmx.ypCode, cfmx.ypmc, cfmx.gg, SUM(mxph.sl) zxdwsl, cfmx.dw, cfmx.dj, cfmx.ycmc, cfmx.je, cfmx.jl, cfmx.jldw, cfmx.yfmc
	,cfmx.bz yszt, cf.ysmc, cf.sfsj, cfmx.czh, cfmx.zhyz,xtyp.gjybdm,mxph.ph, mxph.pc
	FROM dbo.mz_cf(NOLOCK) cf 
	INNER JOIN dbo.mz_cfmx(NOLOCK) cfmx ON cfmx.cfh = cf.cfh AND cfmx.OrganizeId=cf.OrganizeId AND cfmx.zt='1'
	INNER JOIN dbo.mz_cfmxph(NOLOCK) mxph ON mxph.cfh=cfmx.cfh AND mxph.gjzt='0' AND mxph.yp=cfmx.ypCode AND mxph.OrganizeId=cfmx.OrganizeId AND mxph.fyyf=cf.lyyf AND mxph.zt='1'
	INNER JOIN [NewtouchHIS_Base].dbo.xt_ypsx(NOLOCK) xtyp ON xtyp.ypcode = cfmx.ypcode AND cfmx.OrganizeId=xtyp.OrganizeId AND xtyp.zt='1'
	WHERE cf.OrganizeId=@OrganizeId
	AND cf.zt='1'
	AND cf.jsnm>0
	AND cf.lyyf=@yfbmCode
	AND cf.cfh in(select col from dbo.f_split(@cfh,',') where col>'')
	AND ISNULL(mxph.czh,'')=ISNULL(cfmx.czh,'')
	AND mxph.cfmxId=cfmx.Id
	GROUP BY cf.Fph, cf.cfh,cf.cfnm, cfmx.ypCode, cfmx.ypmc, cfmx.gg, cfmx.dw, cfmx.dj, cfmx.ycmc, cfmx.je, cfmx.jl, cfmx.jldw, cfmx.yfmc, cfmx.bz, cf.ysmc, cf.sfsj, cfmx.czh, cfmx.zhyz,xtyp.gjybdm,mxph.ph, mxph.pc
) a 
order by czh
";

		#endregion

		#region 门诊发药

		/// <summary>
		/// 门诊发药
		/// </summary>
		public const string mz_yp_delivery = @"
--处理页面输入追溯码
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#zsminput') AND type='U')
		BEGIN
			DROP TABLE #kcData;
		END
select left(col,num1-1) ypdm,
	substring(col,num1+1,num2-num1-1) zsm,
	substring(col,num2+1,num3-num2-1) sfcl,
    substring(col,num3+1,num4-num3-1) pc,
	substring(col,num4+1,len(col)-num4) ph
	INTO #zsminput
from (
	select col,
		CHARINDEX('/', col) num1,
		CHARINDEX('/', col, CHARINDEX('/', col) + 1) num2,
		CHARINDEX('/', col, CHARINDEX('/', col, CHARINDEX('/', col) + 1)+1) num3,
		CHARINDEX('/', col, CHARINDEX('/', col, CHARINDEX('/', col, CHARINDEX('/', col) + 1)+1)+1) num4
		from(
		select col from dbo.f_split(isnull(@zsm, ''), '|')
		) taba2
) taba1	

DECLARE @resultMsg VARCHAR(500);
SET @resultMsg=''
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#mxph') AND type='U')
BEGIN
	DROP TABLE #mxph;
END

SELECT cfmxphId Id, sl, pc, ph, yp, cfmxId INTO #mxph 
FROM dbo.mz_cfmxph(NOLOCK) WHERE cfh=@cfh  AND OrganizeId=@OrganizeId AND zt='1' AND fyyf=@yfbmCode AND gjzt='0'

SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ COMMITTED

BEGIN TRY
BEGIN TRANSACTION

IF EXISTS(SELECT 1 FROM #mxph)
BEGIN
	DECLARE @Id VARCHAR(50), @ypCode VARCHAR(50), @zxsl INT, @pc VARCHAR(50), @ph VARCHAR(50),@ypmc VARCHAR(256), @cfmxId BIGINT;
	DECLARE @sumKcsl INT, @sumDjsl INT, @sysl INT,@curkcId VARCHAR(50), @curdjsl INT, @curkcsl INT;


	WHILE EXISTS(SELECT 1 FROM #mxph)
	BEGIN
		SELECT TOP 1 @Id=Id, @ypCode=yp, @zxsl=sl, @pc=pc, @ph=ph, @cfmxId=cfmxId FROM #mxph
		
		IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#kcData') AND type='U')
		BEGIN
			DROP TABLE #kcData;
		END
		SELECT * INTO #kcData FROM xt_yp_kcxx(NOLOCK) 
		WHERE ypdm=@ypCode AND yfbmCode=@yfbmCode 
		AND tybz = '0' AND kcsl>0 AND djsl>=0 AND zt='1'  
		AND pc=@pc AND ph=@ph AND OrganizeId=@OrganizeId 

		IF NOT EXISTS(SELECT 1 FROM #kcData)
		BEGIN
			SELECT @ypmc=ypmc FROM NewtouchHIS_Base.dbo.V_C_xt_yp WHERE zt='1' AND OrganizeId=@OrganizeId AND ypCode=@ypCode
			SET @resultMsg=CONCAT('批次【',@pc,'】、批号【',@ph,'】的药品【',@ypmc,'】未找到有效库存信息')
			BREAK;
		END

		SET @sumKcsl=0;
		SET @sumDjsl=0;
		SET @sysl=@zxsl;
		SELECT @sumKcsl=SUM(kcsl), @sumDjsl=SUM(djsl) FROM #kcData 

		IF @sumKcsl<@zxsl
		BEGIN
			SELECT @ypmc=ypmc FROM NewtouchHIS_Base.dbo.V_C_xt_yp WHERE zt='1' AND OrganizeId=@OrganizeId AND ypCode=@ypCode
			SET @resultMsg=CONCAT('批次【',@pc,'】、批号【',@ph,'】的药品【',@ypmc,'】的药品库存不足')
			BREAK;
		END 
		IF @sumDjsl<@zxsl
		BEGIN
			--SELECT @ypmc=ypmc FROM NewtouchHIS_Base.dbo.V_C_xt_yp WHERE zt='1' AND OrganizeId=@OrganizeId AND ypCode=@ypCode
			--SET @resultMsg=CONCAT('批次【',@pc,'】、批号【',@ph,'】的药品【',@ypmc,'】已冻结库存不足')
			--BREAK;
			WHILE EXISTS(SELECT 1 FROM #kcData) AND @sysl>0
			BEGIN
				SET @curkcId='';
				SET @curkcsl=0;
				SELECT TOP 1 @curkcId=kcId, @curkcsl=kcsl FROM #kcData
				IF @curkcsl>=@sysl
				BEGIN
					UPDATE dbo.xt_yp_kcxx SET djsl=0, kcsl-=@sysl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
					WHERE kcId=@curkcId AND zt='1'
					SET @sysl=0;
				END 
				ELSE
				BEGIN
					UPDATE dbo.xt_yp_kcxx SET djsl=0, kcsl-=@curkcsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
					WHERE kcId=@curkcId AND zt='1'
					SET @sysl=@sysl-@curdjsl;
				END 
				DELETE FROM #kcData WHERE kcId=@curkcId;
			END 
		END 
		ELSE
        BEGIN
			WHILE EXISTS(SELECT 1 FROM #kcData) AND @sysl>0
			BEGIN
				SET @curkcId='';
				SET @curdjsl=0;
				SELECT TOP 1 @curkcId=kcId, @curdjsl=djsl FROM #kcData
				IF @curdjsl>=@sysl
				BEGIN
					UPDATE dbo.xt_yp_kcxx SET djsl-=@sysl, kcsl-=@sysl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
					WHERE kcId=@curkcId AND zt='1'
					SET @sysl=0;
				END 
				ELSE
				BEGIN
					UPDATE dbo.xt_yp_kcxx SET djsl-=@curdjsl, kcsl-=@curdjsl, LastModifyTime=GETDATE(), LastModifierCode=@userCode 
					WHERE kcId=@curkcId AND zt='1'
					SET @sysl=@sysl-@curdjsl;
				END 
				DELETE FROM #kcData WHERE kcId=@curkcId;
			END 
		END 
        declare @inputzsm varchar(500)='';
        declare @zsmsfcl varchar(1)=1;
		SELECT @inputzsm=zsm,@zsmsfcl=sfcl FROM #zsminput where ypdm=@ypCode and pc=@pc and ph=@ph
		INSERT INTO dbo.mz_cfypczjl(mzcfmxId, operateType ,ypCode ,cfh ,sl ,bz ,CreateTime ,CreatorCode ,LastModifyTime ,LastModifierCode,zsm,sfcl)
		VALUES  ( @cfmxId , -- mzcfmxId - bigint
				  '1' , -- operateType - char(1)
				  @ypCode , -- ypCode - varchar(20)
				  @cfh , -- cfh - varchar(50)
				  @zxsl , -- sl - int
				  '' , -- bz - varchar(200)
				  GETDATE() , -- CreateTime - datetime
				  @userCode , -- CreatorCode - varchar(50)
				  NULL , -- LastModifyTime - datetime
				  '',  -- LastModifierCode - varchar(50)
				  @inputzsm ,--@zsm,
				  @zsmsfcl --@sfcl
				)
		DELETE FROM #mxph WHERE Id=@Id
	END 	
END

UPDATE dbo.mz_cf SET fybz='2' WHERE cfh=@cfh AND OrganizeId=@OrganizeId AND zt='1'

IF(@resultMsg='')
BEGIN
	COMMIT TRANSACTION;
END
ELSE
BEGIN
	ROLLBACK TRANSACTION;
END 
END TRY
BEGIN CATCH
	SET @resultMsg=ERROR_MESSAGE();
	ROLLBACK TRANSACTION;
END CATCH

SELECT @resultMsg resultMsg
";

        #endregion

        #region 住院预定 冻结库存

        /// <summary>
        /// 住院预定 冻结库存
        /// </summary>
        public const string zy_yp_book = @"
DECLARE @resultMsg VARCHAR(500);
SET @resultMsg=''
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#yzDetail') AND type='U')
BEGIN
	DROP TABLE #yzDetail;
END

SELECT * INTO #yzDetail FROM dbo.zy_ypyzxx(NOLOCK) WHERE zxId=@zxId AND yzId=@yzId

IF NOT EXISTS(SELECT 1 FROM #yzDetail)
BEGIN
	SET @resultMsg=CONCAT('未找到【',@patientName,'】的医嘱信息');
END 
ELSE
BEGIN
	DECLARE @ypCode VARCHAR(50), @ypmc VARCHAR(256), @sl INT, @zxdwsl INT, @zhyz INT, @Id BIGINT, @czh VARCHAR(50);
	DECLARE @ypId INT, @bmypId VARCHAR(50), @yfbmId INT, @yfbmmc VARCHAR(256);
	
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	BEGIN TRY
    BEGIN TRANSACTION

	DECLARE @kyck INT
	DECLARE @curkcId VARCHAR(50); 
	DECLARE @curkysl INT;
	DECLARE @sykc INT;--剩余库存
	DECLARE @yxq DATETIME;

	WHILE EXISTS(SELECT 1 FROM #yzDetail)
	BEGIN
		SELECT TOP 1 @ypCode=ypCode, @sl=sl, @zxdwsl=sl*zhyz, @zhyz=zhyz, @czh=zh, @Id=Id FROM #yzDetail
		
		SELECT @ypId=yp.ypId, @bmypId=bmypxx.bmypId, @yfbmId=yfbm.yfbmId, @yfbmmc=yfbm.yfbmmc, @ypmc=yp.ypmc
		FROM NewtouchHIS_Base.dbo.V_C_xt_yp yp
		LEFT JOIN dbo.xt_yp_bmypxx(NOLOCK) bmypxx ON bmypxx.Ypdm=yp.ypCode AND bmypxx.OrganizeId=yp.OrganizeId AND bmypxx.yfbmCode=@yfbmCode AND bmypxx.zt='1'
		LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm ON yfbm.yfbmCode=@yfbmCode AND yfbm.OrganizeId = yp.OrganizeId AND yfbm.zt='1'
		WHERE yp.ypCode=@ypCode 
		AND yp.zt='1'
		AND yp.OrganizeId=@OrganizeId

		--效验 1:药品是否有效
		IF ISNULL(@ypId,0)=0
		BEGIN
			SET @resultMsg=CONCAT('编码为【',@ypCode,'】的药品无效');
			BREAK;
		END 
		
		--验证 2：药房是否有效
		IF ISNULL(@yfbmId,0)=0
		BEGIN
			SET @resultMsg=CONCAT('药房部门代码为【',@yfbmCode, '】的药房未开放') ;
			BREAK;
		END 

		--验证 3：部门是否拥有该药品
		IF ISNULL(@bmypId,'')=''
		BEGIN
			SET @resultMsg=CONCAT('药房部门【',@yfbmmc,'】没有药品【',@ypmc, '】的权限') ;
			BREAK;
		END

		SET @kyck=0;--可用库存
		SET @curkcId=''; --当前库存ID
		SET @curkysl =0;--当前库存数量
		SET @sykc =0;--剩余库存
		SET @yxq='';--有效期

		IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#kcData') AND type='U')
		BEGIN
			DROP TABLE #kcData;
		END

		SELECT kcId, kcsl, djsl, (kcsl-djsl) kysl, ph, pc, yxq 
		INTO #kcData FROM dbo.xt_yp_kcxx WITH (XLOCK, ROWLOCK) 
		WHERE (kcsl-djsl)>0 AND ypdm=@ypCode AND yfbmCode=@yfbmCode AND OrganizeId=@OrganizeId AND zt='1'

		SET @kyck=(SELECT ISNULL(SUM(kysl),0) FROM #kcData);
		SET @sykc=@zxdwsl;
		IF @kyck<@zxdwsl
		BEGIN
			SET @resultMsg= CONCAT('药品【',@ypmc, '】库存不足');
			BREAK;
		END

		WHILE @sykc > 0 AND EXISTS(SELECT 1 FROM #kcData)
		BEGIN
			SELECT TOP 1 @curkcId=kcId, @curkysl=kysl FROM #kcData ORDER BY yxq
			DECLARE @curkjsl INT;--扣减数量
			IF @curkysl>=@sykc 
			BEGIN
				--当前批号批次库存足够
				SET @curkjsl=@sykc;
				SET @sykc=0;		
			END
			ELSE
			BEGIN
				--当前批号批次库存不足
				SET @curkjsl=@curkysl;
				SET @sykc=@sykc-@curkysl;
			END  

			UPDATE dbo.xt_yp_kcxx SET djsl=djsl+@curkjsl, LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE kcId=@curkcId;
			
			INSERT INTO dbo.zy_ypyzzxph( OrganizeId ,yzId ,zxId ,ph ,pc ,sl ,ypCode ,yxq ,fyyf ,gjzt ,px ,zt ,CreateTime ,CreatorCode ,LastModifyTime ,LastModifierCode ,zh ,zyypxxId)
			SELECT    @OrganizeId , -- OrganizeId - varchar(50)
			          @yzId , -- yzId - varchar(50)
			          @zxId , -- zxId - varchar(50)
			          ph , -- ph - varchar(30)
			          pc , -- pc - varchar(30)
			          @curkjsl , -- sl - numeric
			          @ypCode , -- ypCode - varchar(50)
			          @yxq , -- yxq - datetime
			          @yfbmCode , -- fyyf - varchar(50)
			          '0' , -- gjzt - varchar(1)
			          0 , -- px - int
			          '1' , -- zt - varchar(1)
			          GETDATE() , -- CreateTime - datetime
			          @CreatorCode , -- CreatorCode - varchar(50)
			          NULL , -- LastModifyTime - datetime
			          '' , -- LastModifierCode - varchar(50)
			          @czh , -- zh - int
			          @Id  -- zyypxxId - bigint
			FROM #kcData WHERE kcId=@curkcId ;
			IF @@ERROR <> 0
			BEGIN
				SET @resultMsg= CONCAT('药品【',@ypmc,'】插入排班明细表失败');
				BREAK;
			END

			DELETE FROM #kcData WHERE kcId=@curkcId
		END 
		
		DELETE FROM #yzDetail WHERE Id=@Id
	END 

	IF(@resultMsg='')
	BEGIN
		COMMIT TRANSACTION;
	END
	ELSE
    BEGIN
		ROLLBACK TRANSACTION;
	END 
	END TRY
	BEGIN CATCH
		SET @resultMsg=ERROR_MESSAGE();
		ROLLBACK TRANSACTION;
	END CATCH
END 

SELECT @resultMsg resultMsg
";

        /// <summary>
        /// 住院药品取消冻结，并物理删除医嘱信息（慎用）
        /// </summary>
        public const string zy_yp_cancelForzen_deleteData = @"
DECLARE @resultMsg VARCHAR(500);
SET @resultMsg=''
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#phmx') AND type='U')
BEGIN
	DROP TABLE #phmx;
END
SELECT * INTO #phmx FROM dbo.zy_ypyzzxph(NOLOCK) WHERE yzId=@yzId AND zxId=@zxId AND zt='1' AND gjzt='0'

IF EXISTS(SELECT 1 FROM #phmx)
BEGIN
	DECLARE @Id BIGINT, @yfbmCode VARCHAR(50), @ypCode VARCHAR(50), @zxsl INT, @ph VARCHAR(50), @pc VARCHAR(50), @OrganizeId VARCHAR(50);
		
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	BEGIN TRY
	BEGIN TRANSACTION

	WHILE EXISTS(SELECT 1 FROM #phmx)
	BEGIN
		SELECT TOP 1 @Id=Id, @yfbmCode=fyyf, @OrganizeId=OrganizeId, @pc=pc, @ph=ph, @ypCode=ypCode, @zxsl=sl FROM #phmx

		UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@zxsl WHERE ypdm=@ypCode AND pc=@pc AND ph=@ph AND yfbmCode=@yfbmCode AND OrganizeId=@OrganizeId AND zt='1'
		
		DELETE FROM dbo.zy_ypyzzxph WHERE Id=@Id
		DELETE FROM #phmx WHERE Id=@Id
	END 

	IF(@resultMsg='')
	BEGIN
		COMMIT TRANSACTION;
	END
	ELSE
	BEGIN
		ROLLBACK TRANSACTION;
	END 
	END TRY
	BEGIN CATCH
		SET @resultMsg=ERROR_MESSAGE();
		ROLLBACK TRANSACTION;
	END CATCH
END

SELECT @resultMsg resultMsg
";

        #endregion

        #region 门诊取消结算 库存解冻
        public const string mz_yp_cancelSett_kcjd = @"
IF EXISTS(SELECT 1 FROM tempdb..sysobjects WHERE id=OBJECT_ID(N'tempdb..#CancalSettData') AND type='U')
BEGIN
	DROP TABLE #CancalSettData; --排药数据
END

BEGIN TRY
DECLARE @cfId BIGINT, @ph VARCHAR(50), @pc VARCHAR(50), @kcsl INT, @cfmxphId VARCHAR(50), @curKcId VARCHAR(50), @fybz CHAR(1), @jsnm BIGINT;

SELECT @cfId=Id, @fybz=fybz, @jsnm=jsnm FROM dbo.mz_cf WHERE cfh=@cfh  AND OrganizeId=@OrganizeId AND lyyf=@yfbmCode   --AND zt='1'

IF ISNULL(@cfId, 0)=0
BEGIN
	SELECT '未找到指定处方';
	RETURN ;
END

SELECT * 
	INTO #CancalSettData
FROM dbo.mz_cfmxph	
WHERE yp=@ypdm AND OrganizeId=@OrganizeId AND fyyf=@yfbmCode AND zt='1' AND gjzt='0' AND cfh=@cfh 

IF NOT EXISTS(SELECT 1 FROM #CancalSettData)
BEGIN
	SELECT '未找到排药数或已取消';
	RETURN;
END 

if(exists(select 1 from [mz_cfypczjl] a with(nolock),#CancalSettData b where a.mzcfmxid=b.cfmxid))
begin
	SELECT '处方已发药，不能取消预定';
	RETURN ;
end

DECLARE @bookCout INT;
DECLARE @sysl INT;

SELECT @sysl=SUM(sl) FROM #CancalSettData

DECLARE @errorMsg VARCHAR(500);
SET @errorMsg='';

BEGIN TRANSACTION
	WHILE EXISTS(SELECT 1 FROM #CancalSettData) AND @sysl>0
	BEGIN
		SELECT TOP 1 @cfmxphId=cfmxphId, @ph=ph, @pc=pc, @kcsl=sl FROM #CancalSettData ORDER BY yxq asc
		SELECT TOP 1 @curKcId=ISNULL(kcId,'') 
			FROM dbo.xt_yp_kcxx WITH (XLOCK, ROWLOCK) 
			WHERE ypdm=@ypdm AND yfbmCode=@yfbmCode AND ph=@ph AND pc=@pc AND OrganizeId=@OrganizeId 
		IF @curKcId=''
			BEGIN
				DECLARE @ypmc VARCHAR(256);
				SELECT @ypmc=ypmc FROM NewtouchHIS_Base.dbo.V_C_xt_yp WHERE ypCode=@ypdm AND OrganizeId=@OrganizeId AND zt='1'
				SET @errorMsg+=CONCAT('处方【',@cfh,'】未找到冻结时所用批号为【',@ph,'】的药品【',@ypmc,'】。');
				BREAK;
			END 
				print @kcsl;
				print @sysl;
			IF @kcsl>@sysl
			BEGIN
				SET @kcsl=@sysl;
				SET @sysl=0;
			END
			ELSE
			BEGIN
				
				SET @sysl=@sysl-@kcsl; 
				UPDATE dbo.mz_cfmxph SET gjzt='1', zt='0', LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE cfmxphId=@cfmxphId
			END  
			UPDATE dbo.xt_yp_kcxx SET djsl=djsl-@kcsl, LastModifyTime=GETDATE(), LastModifierCode=@CreatorCode WHERE kcId=@curKcId ;
			DELETE FROM #CancalSettData WHERE cfmxphId=@cfmxphId

			update dbo.mz_cfmx set zt='0',LastModifyTime=getdate(),LastModiFierCode=@CreatorCode where cfh=@cfh and OrganizeId=@OrganizeId and ypCode=@ypdm and zt='1' 

			if(not exists(select 1 from mz_cfmx a with(nolock),mz_cf b with(nolock) 
					where a.cfh=b.cfh and a.OrganizeId=b.OrganizeId and a.zt='1' and b.zt='1' and b.cfh=@cfh))
			begin
					update dbo.mz_cf 
					set zt='0',LastModifyTime=getdate(),LastModiFierCode=@CreatorCode 
					where cfh=@cfh and OrganizeId=@OrganizeId and zt='1'
			end
	END
IF(@errorMsg='')
	BEGIN
		COMMIT TRANSACTION;
	END
	ELSE
	BEGIN
		ROLLBACK TRANSACTION;
	END 
	SELECT @errorMsg;
END TRY
BEGIN CATCH
IF @@TRANCOUNT>0
BEGIN
	ROLLBACK TRANSACTION
END 
SELECT ERROR_MESSAGE();
END CATCH
";
        #endregion
    }
}
