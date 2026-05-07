USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[usp_open_SelectCfXm]    Script Date: 2025/12/4 10:49:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO











/*
remark:处方医嘱开立项目耗材浮层
exec [usp_open_SelectCfXm] '21332','6d5752a7-234a-403e-aa1c-df8b45d3469f','1','Rehab','',''
exec [usp_open_SelectCfXm] '212132','6d5752a7-234a-403e-aa1c-df8b45d3469f','1','RegularItem','镊子',''
exec [usp_open_SelectCfXm] '22322','6d5752a7-234a-403e-aa1c-df8b45d3469f','1','cl','','KS01'
*/

ALTER PROCEDURE [dbo].[usp_open_SelectCfXm] 
 @topCount int,
 @orgId varchar(50),
 @mzzybz varchar(50),
 @sfdllx varchar(20),
 @keyword varchar(50),
 @ypyfbmCode varchar(50) --物资科室
AS
BEGIN
--set @topCount=2;
--declare  @topCount int,
-- @orgId varchar(50),
-- @mzzybz varchar(50),
-- @sfdllx varchar(20),
-- @keyword varchar(50)
-- select @topCount='12',@orgId='6d5752a7-234a-403e-aa1c-df8b45d3469f',@mzzybz='1',@sfdllx='RegularItem',@keyword='cswz'

IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#XmQuery') and type='U')
BEGIN
	DROP TABLE #XmQuery;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#zttab') and type='U')
BEGIN
	DROP TABLE #zttab;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#tempcl') and type='U')
BEGIN
	DROP TABLE #tempcl;
END
declare @xzyysm varchar(200);
WITH
xmdl AS
(
	SELECT DISTINCT lx.dlCode,dl.dlmc,lx.Organizeid FROM [NewtouchHIS_BASE].[dbo].[xt_sfdl_lx] lx
	INNER JOIN [NewtouchHIS_BASE].[dbo].[xt_sfdl] dl ON lx.OrganizeId=dl.OrganizeId AND lx.dlCode=dl.dlCode
	WHERE lx.OrganizeId=@orgId AND lx.[Type]=@sfdllx AND lx.zt='1' AND dl.zt='1'
)
SELECT --TOP (@topCount) 
	xm.sfxmCode,xm.sfxmmc,xm.sfdlCode,xmdl.dlmc AS sfdlmc,'' AS ypjxCode,
	xm.dw,1.00 AS cls,CAST(ROUND(xm.dj,2) AS NUMERIC(12,2)) AS dj,
	xm.py,xm.px,'' AS jldw,NULL AS jldwzhxs,xm.zfxz,xm.zfbl,
	NULL AS kcsl,'' AS kzbz,
	xm.gg,'' AS zyqzlx,xm.ybdm,
	NULL AS xzyy,@xzyysm xzyysm,NULL AS mrjl,NULL AS mrpc,NULL AS mrpcmc,
	'0' AS isKss,NULL AS jlfwBegin,NULL AS jlfwEnd,NULL AS pcfwBegin,NULL AS pcfwEnd,'' AS kssqxjb,NULL AS kssKy,
	xm.cxjje,'' AS tsypbz,xm.bz,'2' AS jybz,
	xm.duration,ISNULL(xm.dwjls,0) AS dwjls,ISNULL(xm.jjcl,2) AS jjcl,xm.zxks,dept.[Name] AS zxksmc,
	'2' AS yzlx,
	@xzyysm AS yfbmCode,@xzyysm AS yfbmmc
	INTO #XmQuery
	FROM [NewtouchHIS_BASE].[dbo].[xt_sfxm] xm with(nolock)
	INNER JOIN xmdl ON xm.sfdlCode = xmdl.dlCode and xmdl.Organizeid=xm.Organizeid
	LEFT JOIN [NewtouchHIS_BASE].[dbo].[Sys_Department] dept ON dept.OrganizeId=@orgId AND xm.zxks=dept.Code
	WHERE
	xm.zt='1'
	AND (LEN(@keyword)=0 OR (LEN(@keyword)>0 AND (xm.py LIKE CONCAT('%',@keyword,'%') OR xm.sfxmmc LIKE CONCAT('%',@keyword,'%'))))
	--ORDER BY xm.sfxmCode;
	
	--常规项目将维护的收费项目组套合并进来可开立
	IF(@sfdllx='RegularItem') 
	BEGIN
		select  b.sfxm,sfmb sfxmcode,sfmbmc sfxmmc,sfmb sfdlcode,'收费项目组合' sfdlmc,'套' dw, dj,sl ,a.mzzybz,a.py,NULL px,0 duratoin
			,'' bz , 1 dwjls,2 jjcl,'9' zfxz,0 zfbl,a.ks,c.Name zxksmc,'' gg,'000000' ybdm,0.00 cxjje 
		into #zttab 
		from NewtouchHIS_Sett..[xt_sfmb] a with(nolock)
		inner join NewtouchHIS_Sett..[xt_sfmbxm] b with(nolock) on a.sfmbbh=b.sfmbbh and a.OrganizeId=b.OrganizeId
		left join NewtouchHIS_Base..Sys_Department c with(nolock) on a.ks=c.code and a.organizeid=c.organizeid and c.zt='1'
		where a.organizeid=@orgId and a.zt='1'  and (a.mzzybz = @mzzybz or a.mzzybz=0)
	
		insert into #XmQuery
		select 
			a.sfxmcode,
			a.sfxmmc,
			a.sfdlcode,
			a.sfdlmc,
			'' ypjxCode,
			a.dw,
			1.00 cls,
			isnull(a.dj,0.00) dj,
			a.py,
			a.px,
			'' jldw,
			NULL jldwzhxs,
			a.zfxz,
			a.zfbl,
			NULL kcsl,
			'' kzbz,
			NULL gg,
			'' zyqzlx,
			a.ybdm,
			NULL xzyy,
			case zfxz when '9' then b.sfxm else '' end xzyysm,
			NULL mrjl,
			NULL mrpc,
			NULL mrpcmc,
			'0' isKss,
			NULL jlfwBegin,
			NULL jlfwEnd,
			NULL pcfwBegin,
			NULL pcfwEnd,
			'' kssqxjb,
			NULL kksKy,
			a.cxjje,
			'' tsypbz,
			a.bz,
			'2' jybz,
			NULL duratoin,
			a.dwjls,
			a.jjcl,
			NULL zxks,
			NULL zxksmc,
			'2' yzlx,
			'' yfbmCode,
			'' yfbmmc
			from (
		select  sfxmcode, sfxmmc, sfdlcode, sfdlmc, dw, sum(convert(decimal(18,2),dj*sl)) dj ,mzzybz,py, px, duratoin
		, bz ,  dwjls, jjcl, zfxz, zfbl,ks, zxksmc, gg,ybdm,cxjje  from #zttab
		group by  sfxmcode, sfxmmc, sfdlcode, sfdlmc, dw,mzzybz,py, px, duratoin, bz ,  dwjls, jjcl, zfxz, zfbl,ks, zxksmc, gg,ybdm,cxjje
		) a 
		join (select  sfxmcode ,
		sfxm=stuff((select ','+sfxm from #zttab where sfxmcode=t.sfxmcode for xml path('')),1,1,'')  
		from #zttab t
		group by sfxmcode) b on  a.sfxmcode=b.sfxmcode

	END
	--材料开立根据参数取herp物资耗材所出库至科室的耗材(或耗材并入药房开立)
	IF(@sfdllx='cl' and exists (select 1  from Sys_Config where code='openWzhckc' and organizeid=@orgId and zt='1' and value='ON'))
	BEGIN
		select  a.sfxmCode,convert(decimal(10,2),sum(kskc.kcsl-kskc.djsl)) kcsl,c.name lbName,kskc.ks,d.Name  ksmc
		into #tempcl
		from #XmQuery a
		join Newtouch_CIS..Dept_kcxx kskc on kskc.productCode=a.sfxmCode and kskc.ks=@ypyfbmCode and kskc.OrganizeId=@orgId 
	    join NewtouchHIS_herp..wz_product b on a.sfxmCode=b.productCode and b.OrganizeId=@orgId and b.zt='1'
		join NewtouchHIS_Base..wz_type c  on c.Id=b.typeId
		join NewtouchHIS_Base..Sys_Department d on d.Code=kskc.ks and d.OrganizeId=kskc.OrganizeId
		--left join NewtouchHIS_herp..kf_kcxx c on b.Id=c.productId  and c.zt='1' and c.OrganizeId=@orgId and (c.kcsl-c.djsl)>0
		--where c.productId is not null
		group by a.sfxmCode,c.name,kskc.ks,d.Name

		update a set a.kcsl=convert(decimal(10,2),b.kcsl),a.yzlx='1',sfdlmc=b.lbName,a.yfbmCode=b.ks,a.yfbmmc=b.ksmc
		from #XmQuery a
		inner join #tempcl b on a.sfxmCode=b.sfxmCode
		--where b.sfxmCode is not null
		--材料费 走物资系统 库存为null的不显示 
        delete from #XmQuery where isnull(kcsl,0)=0 --AND CHARINDEX('wz',sfxmCode)>0  
	END
	ELSE IF(@sfdllx!='cl')
	BEGIN 
		delete from #XmQuery where CHARINDEX('wz',sfxmCode)>0 
	END
	
SELECT TOP (@topCount) 
sfxmcode,
sfxmmc,
sfdlcode,
sfdlmc,
ypjxCode,
dw,
convert(decimal(10,2),'1.00') cls,
dj,
py,
px,
jldw,
jldwzhxs,
zfxz,
zfbl,
convert(decimal(10,2),kcsl) kcsl,
kzbz,
gg,
zyqzlx,
ybdm,
xzyy,
xzyysm,
mrjl,
mrpc,
mrpcmc,
isKss,
jlfwBegin,
jlfwEnd,
pcfwBegin,
pcfwEnd,
kssqxjb,
cxjje,
tsypbz,
bz,
jybz,
dwjls,
jjcl,
zxks,
zxksmc,
yzlx,
yfbmCode,
yfbmmc
from #XmQuery 
where  (LEN(@keyword)=0 OR (LEN(@keyword)>0 AND (py LIKE CONCAT('%',@keyword,'%') OR sfxmmc LIKE CONCAT('%',@keyword,'%'))))

END
GO


