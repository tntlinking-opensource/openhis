USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[usp_open_SelectCfYp]    Script Date: 2025/11/21 15:43:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO












/*
EXEC [usp_open_SelectCfYp] '2343','6d5752a7-234a-403e-aa1c-df8b45d3469f','1','00000019','WM','磷酸奥司他韦颗粒','0','0'
*/
ALTER PROCEDURE [dbo].[usp_open_SelectCfYp]
 @topCount int,
 @orgId varchar(50),
 @mzzybz varchar(50),
 @yfbmCode varchar(50),
 @sfdllx varchar(20),
 @keyword varchar(50),
 @isQyKssKZ bit = 0,
 @qxjb VARCHAR(2) = '0'
AS
BEGIN
WITH
ypkc AS
(
	SELECT ypdm,yfbmCode,sum(kcsl-djsl) AS zxdwsl FROM [NewtouchHIS_PDS].[dbo].[xt_yp_kcxx] with(nolock)
	WHERE OrganizeId=@orgId 
	--AND (isnull(yfbmCode,'') = '' or yfbmCode in (select * from dbo.f_split(isnull(yfbmCode, ''), ','))) 
	AND yfbmCode=@yfbmCode
	AND (kcsl > 0 AND kcsl > djsl)
	GROUP BY ypdm,yfbmCode
),
ypdl AS
(
	SELECT lx.dlCode,dl.dlmc FROM [NewtouchHIS_BASE].[dbo].[xt_sfdl_lx] lx
	INNER JOIN [NewtouchHIS_BASE].[dbo].[xt_sfdl] dl ON lx.OrganizeId=dl.OrganizeId AND lx.dlCode=dl.dlCode
	WHERE lx.OrganizeId=@orgId AND lx.[Type]=@sfdllx AND lx.zt='1' AND dl.zt='1'
),
ypkss AS
(
	SELECT Id,qxjb,jlfwBegin,jlfwEnd,pcfwBegin,pcfwEnd,(CASE WHEN qxjb<=@qxjb THEN '1' ELSE '0' END) AS kssKy
	FROM [NewtouchHIS_Base].[dbo].[xt_ypKss]
	WHERE @isQyKssKZ=1 AND @sfdllx='WM' AND OrganizeId=@orgId AND zt='1'
),
yzpc AS
(
	SELECT yzpcCode,yzpcmc FROM NewtouchHIS_Base..xt_yzpc WHERE OrganizeId=@orgId AND zt='1'
),
yzyf as
(
   SELECT yfCode,yfmc FROM NewtouchHIS_Base..xt_ypyf where zt='1'
)
SELECT TOP (@topCount) yp.ypCode AS sfxmCode,yp.ypmc AS sfxmmc,yp.dlCode AS sfdlCode,ypdl.dlmc AS sfdlmc,yp.jx AS ypjxCode,
	(CASE @mzzybz WHEN '1' THEN yp.mzcldw WHEN '2' THEN yp.zycldw end) AS dw,
	(CASE @mzzybz WHEN '1' THEN yp.mzcls WHEN '2' THEN yp.zycls end) AS cls,
	CONVERT(DECIMAL(19,4), CASE @mzzybz WHEN '1' THEN (CASE WHEN yp.mzcldw=yp.bzdw THEN yp.lsj ELSE (yp.lsj/yp.bzs*yp.mzcls) END) WHEN '2' THEN (CASE WHEN yp.zycldw=yp.bzdw THEN yp.lsj ELSE (yp.lsj/yp.bzs*yp.zycls) END) END) AS dj,
	yp.py,yp.px,yp.jldw,yp.jl AS jldwzhxs,yp.zfxz,yp.zfbl,
	CONVERT(NUMERIC(11,2),ypkc.zxdwsl) AS kcsl,yp.zxdw,'0' AS kzbz,
	ypsx.ypgg AS gg,ypsx.jzlx AS zyqzlx,ypsx.ybdm,
	ypsx.xzyy,ypsx.xzyysm,ypsx.mrjl,ypsx.mrpc,yzpc.yzpcmc AS mrpcmc,
	ypsx.mryf,yzyf.yfmc AS mryfmc,
	yp.isKss,ypkss.jlfwBegin,ypkss.jlfwEnd,ypkss.pcfwBegin,ypkss.pcfwEnd,ypkss.qxjb AS kssqxjb,ypkss.kssKy,
	yp.cxjje,yp.tsypbz,
	--yp.bz
	ybyp.NAT_HI_DRUGLIST_MEMO AS bz
	,'2' AS jybz,
	NULL AS duration,NULL AS dwjls,NULL AS jjcl,'' AS zxks,'' AS zxksmc,
	'1' AS yzlx,
	ypkc.yfbmCode AS yfbmCode,yfbm.yfbmmc AS yfbmmc,yp.ycmc sccj,ypsx.gjybdm
	FROM ypkc
	INNER JOIN [NewtouchHIS_BASE].[dbo].[xt_yp] yp ON yp.OrganizeId=@orgId AND ypkc.ypdm=yp.ypCode
	INNER JOIN [NewtouchHIS_BASE].[dbo].[xt_ypsx] ypsx ON yp.ypId=ypsx.ypId and yp.organizeid=ypsx.organizeid
	INNER JOIN ypdl ON yp.dlCode = ypdl.dlCode
	LEFT JOIN ypkss ON yp.kssId = ypkss.Id
	LEFT JOIN yzpc ON ypsx.mrpc = yzpc.yzpcCode
	left join yzyf on ypsx.mryf=yzyf.yfCode
	left join NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm 
	on yfbm.OrganizeId =@orgId and yfbm.yfbmCode = ypkc.yfbmCode  and yfbm.fybz!='0'
     left join  NewtouchHIS_Base.dbo.[G_yb_wm_tcmpat_info_b] ybyp on ypsx.gjybdm =  ybyp.MED_LIST_CODG
	WHERE
	yp.zt='1'
	AND (LEN(@keyword)=0 OR (LEN(@keyword)>0 AND (yp.py LIKE CONCAT('%',@keyword,'%') OR yp.ypmc LIKE CONCAT('%',@keyword,'%'))))
    AND (@mzzybz = '1' or @mzzybz = '2') --查询药品一定要指定查询门诊或住院    
	AND (yfbm.mzzybz = @mzzybz or yfbm.mzzybz = '3') --门诊/住院药房 或 通用药房    
	ORDER BY yp.ypCode;
END
GO


