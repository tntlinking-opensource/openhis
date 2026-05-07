USE [NewtouchHIS_Sett]
GO

/****** Object:  StoredProcedure [dbo].[usp_SelectSfxmYp]    Script Date: 2025/12/4 15:03:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO









  
/*  
修改人：邓烽  
修改时间：2022年3月18日11:28:20  
修改内容：添加cxjje字段  
----------------------------------------
修 改 人：朱骏
修改时间：2022年8月5日15:13
修改内容：医院要求控制不显示库存为0的药，故在此控制。后续若修改直接注释此处！
修改标志：20220805A
exec  [usp_SelectSfxmYp] '312','6d5752a7-234a-403e-aa1c-df8b45d3469f','2','2','cl','','','','','','','','',''
exec  [usp_SelectSfxmYp] '12','6d5752a7-234a-403e-aa1c-df8b45d3469f','2','1','','','','',NULL,'','','','',''
exec  [usp_SelectSfxmYp] '1232','6d5752a7-234a-403e-aa1c-df8b45d3469f','2','2',NULL,'云南白药创可贴（轻巧透气）',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
exec  [usp_SelectSfxmYp] '123','3a8d2459-91f5-4131-abcf-41b075a52581','2','1',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'',NULL
exec  [usp_SelectSfxmYp] '122','6d5752a7-234a-403e-aa1c-df8b45d3469f','2','2','cl',NULL,NULL,NULL,NULL,'KS01',NULL,'',NULL,NULL
*/    
    
ALTER PROCEDURE [dbo].[usp_SelectSfxmYp]  
(  
 @topCount int, --前多少条    
 @orgId varchar(50),    
 @mzzybz varchar(50) = '', --1门诊 或 2住院 必须指定是查门诊 还是住院    
    
 @dllb varchar(50) = '', --1药品 2项目 3非治疗项目 多个 逗号分割    
 @sfdllx varchar(20) = '', --关联字典ChargeCateType，如WM 西药    
    
 @keyword varchar(50) = '',    
    
 @dlCode varchar(20) = '', --具体的收费大类Code 多个 逗号分割    
 @isContansChildDl bit = 1, --配合@dlCode使用    
    
 @useypckflag bit = 0, --是否使用药品库存逻辑    
 @ypyfbmCode varchar(128) = '', --药品的药房筛选 多个 逗号分割，不传什么都查不到    
 @containyp0ck bit = 0, --是否包含药品0库存，0不包含 1包含    
 @onlyybflag bit = 0, --是否仅医保项目/药品    
 @isQyKssKZ bit = 0, --是否启用抗生素控制    
 @qxjb VARCHAR(2) = '' --抗生素的权限级别    
)  
AS 
declare @xzyysm varchar(200); -- #tbdlCodeTemp  ,#XmQuery,#YpQuery  
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#tempcl') and type='U')
BEGIN
	DROP TABLE #tempcl;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#tbdlCodeTemp') and type='U')
BEGIN
	DROP TABLE #tbdlCodeTemp;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#XmQuery') and type='U')
BEGIN
	DROP TABLE #XmQuery;
END
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#YpQuery') and type='U')
BEGIN
	DROP TABLE #YpQuery;
END


--1.指定查询门诊或住院项目/药品/耗材
if(@mzzybz <> '0'and @mzzybz <> '1' and @mzzybz <> '2' and @mzzybz <> '')    
 return; -- '0' 仅通用的 Empty不管门诊还是住院或通用    
;    
--2.是否走无库存药品查询    
if(@useypckflag = 0)    
begin   
	--print '131'
	--return;
 exec [NewtouchHIS_Base].dbo.usp_SelectSfxmYp_WithNoKc @topCount, @orgId, @mzzybz, @dllb, @sfdllx, @keyword, @dlCode    
 , @isContansChildDl, @onlyybflag,@isQyKssKZ,@qxjb    
 return;    
end    
;    
  
--if exists(select 1 from Sys_Config where OrganizeId = @orgId and Code = 'HISSyncMethod' and Value = 'IDB')    
--begin    
-- --库存在中间库    
-- exec usp_SelectSfxmYp_WithIDB @topCount, @orgId, @mzzybz, @dllb, @sfdllx, @keyword, @dlCode    
-- , @isContansChildDl, @useypckflag, @ypyfbmCode, @containyp0ck, @onlyybflag    
-- return;    
--end    
--;    
   
--3.dlCode dlmc集合 创建临时表#tbdlCodeTemp    
WITH cteTree AS    
    (SELECT *    
    FROM [NewtouchHIS_Base]..V_S_xt_sfdl    
 where dlId in (    
  select dlId from [NewtouchHIS_Base]..V_S_xt_sfdl    
  where Organizeid  = @orgId and zt = '1' and (isnull(@dlCode, '') = '' or dlCode in (select * from dbo.f_split(isnull(@dlCode, ''), ',')))    
 ) and zt = '1'    
    UNION ALL    
 SELECT a.*    
    FROM [NewtouchHIS_Base]..V_S_xt_sfdl a    
    INNER JOIN cteTree ON cteTree.dlId=a.ParentId and a.zt = '1' and @isContansChildDl = 1 
	where a.Organizeid  = @orgId
	)    
    
select distinct dlCode, dlmc, dllb    
into #tbdlCodeTemp    
from cteTree where (isnull(@dllb, '') = '' or dllb in (select Convert(int, col) from dbo.f_split(isnull(@dllb, ''), ',')))    
and (    
isnull(@sfdllx, '') = ''    
or    
dlCode in     
 (    
  select dlCode from [NewtouchHIS_Base]..V_S_xt_sfdl_lx where Organizeid  = @orgId and zt = '1'     
  and ((left(@sfdllx,1) <> '-' and Type in (select col from [dbo].[f_split](@sfdllx,','))) or (left(@sfdllx,1) = '-' and Type not in (select col from [dbo].[f_split]((case when isnull(@sfdllx, '') <> '' then SUBSTRING(@sfdllx,2,len(@sfdllx) - 1) else '' end),','))))    
 )    
)    
and (    
 mzzybz = @mzzybz or mzzybz = '0' or @mzzybz = ''    
)    
--select * from [NewtouchHIS_Base]..V_S_xt_sfxm(nolock)    

--4.1 查询大类下收费项目
 select     
 sfxmCode,    
 sfxmmc,    
 a.sfdlCode sfdlCode,    
 b.dlmc sfdlmc,    
 dw,     
 CAST(ROUND(dj,2) AS NUMERIC(12, 2)) dj,    
 a.mzzybz,    
 a.py,    
    a.px,    
 a.duration    
 ,isnull(a.bz,'') bz    
 ,isnull(a.dwjls,0) dwjls    
 ,isnull(a.jjcl,2) jjcl --默认按数量    
 ,a.zfxz    
 ,a.zfbl    
 ,a.zxks    
 ,sysdept.Name zxksmc    
 ,a.gg    
 ,a.ybdm    
 ,a.cxjje ,NULL AS kcsl,NULL AS cls,@xzyysm yfbmCode,@xzyysm yfbmmc,'2' yzlx
 INTO #XmQuery    
 from[NewtouchHIS_Base]..V_S_xt_sfxm(nolock) a    
 INNER join #tbdlCodeTemp(nolock) b    
 on a.sfdlCode = b.dlCode    
 left join [NewtouchHIS_Base]..V_S_Sys_Department(nolock) sysdept    
 on sysdept.Code = a.zxks and sysdept.OrganizeId = a.OrganizeId and sysdept.zt = '1'    
 where a.OrganizeId = @orgId and a.zt = '1'    
 and (@mzzybz = '' or @mzzybz = '1' or @mzzybz = '2' or (@mzzybz = '0' and dllb = 3)) --查询项目一定要指定门诊或住院、或通用仅非治疗项目、或不管门诊还是住院或通用    
 and (    
  a.mzzybz = @mzzybz or a.mzzybz = '0' or @mzzybz = ''    
 )
 --4.2 住院记账,查询科室耗材库存 并排除非耗材数据
IF(@sfdllx='cl' and EXISTS (select 1  from Newtouch_CIS..Sys_Config where code='openWzhckc' and organizeid=@orgId and zt='1' and value='ON'))
BEGIN
	select  a.sfxmCode,convert(decimal(10,2),sum(kskc.kcsl-kskc.djsl)) kcsl,MIN(kskc.zhyz) cls,c.name lbName,kskc.ks,d.Name  ksmc
	into #tempcl
	from #XmQuery a
	join Newtouch_CIS..Dept_kcxx kskc on kskc.productCode=a.sfxmCode and kskc.ks=@ypyfbmCode and kskc.OrganizeId=@orgId 
	join NewtouchHIS_herp..wz_product b on a.sfxmCode=b.productCode and b.OrganizeId=@orgId and b.zt='1'
	join NewtouchHIS_Base..wz_type c  on c.Id=b.typeId
	join NewtouchHIS_Base..Sys_Department d on d.Code=kskc.ks and d.OrganizeId=kskc.OrganizeId
	group by a.sfxmCode,c.name,kskc.ks,d.Name

	update a set a.kcsl=convert(decimal(10,2),b.kcsl),a.yzlx=3,a.cls=b.cls,sfdlmc=b.lbName,a.yfbmCode=b.ks,a.yfbmmc=b.ksmc
	from #XmQuery a
	inner join #tempcl b on a.sfxmCode=b.sfxmCode
	--where b.sfxmCode is not null
	--材料费 走物资系统 库存为null的不显示 
    delete from #XmQuery where isnull(kcsl,0)=0 --and CHARINDEX('wz',sfxmCode)>0 
END
ELSE IF(@sfdllx!='cl')
BEGIN
	delete from #XmQuery where CHARINDEX('wz',sfxmCode)>0 
END
--5.药品临时表 并判断是否抽取药品数据
 CREATE TABLE #YpQuery(    
 ypCode varchar(50) collate Chinese_PRC_CI_AS,    
 ypmc varchar(256) collate Chinese_PRC_CI_AS,    
 sfdlCode varchar(20) collate Chinese_PRC_CI_AS,    
 sfdlmc varchar(50) collate Chinese_PRC_CI_AS,    
 dw  varchar(20) collate Chinese_PRC_CI_AS,    
 cls  numeric(9,4),    
 dj  numeric(9,4),    
 py  VARCHAR(70) collate Chinese_PRC_CI_AS,    
 px  INT ,    
 jldw varchar(10)  collate Chinese_PRC_CI_AS,    
 jldwzhxs numeric(9,4) ,    
 zfxz char(1) collate Chinese_PRC_CI_AS,    
 zfbl numeric(9,4),    
 yfbmCode varchar(20) collate Chinese_PRC_CI_AS,    
 kcsl  numeric(11,2),    
 yfbmmc varchar(200) collate Chinese_PRC_CI_AS,    
 ypgg varchar(300) collate Chinese_PRC_CI_AS,    
 ypjxCode varchar(20) collate Chinese_PRC_CI_AS,    
 kzbz varchar(2) collate Chinese_PRC_CI_AS,    
 zyqzlx char(1) collate Chinese_PRC_CI_AS,    
 ybdm varchar(100) collate Chinese_PRC_CI_AS,    
 xzyy BIT,    
 xzyysm varchar(256) collate Chinese_PRC_CI_AS,    
 mrjl decimal,    
 mrpc varchar(20) collate Chinese_PRC_CI_AS,    
 mrpcmc varchar(50) collate Chinese_PRC_CI_AS,    
 isKss char(1) collate Chinese_PRC_CI_AS,    
 jlfwBegin NUMERIC(9,4),    
 jlfwEnd  NUMERIC(9,4),    
 pcfwBegin NUMERIC(9,4),    
 pcfwEnd  NUMERIC(9,4),    
 kssKy CHAR(1) collate Chinese_PRC_CI_AS,    
 bz varchar(500) collate Chinese_PRC_CI_AS ,  
 cxjje numeric(9,4)  
 )    
IF(CHARINDEX('1',@dllb)>0)  --dllb：1药品 2项目 3非治疗项目 多个 逗号分割    
BEGIN
	IF @isQyKssKZ=0    
	BEGIN    
	INSERT INTO #YpQuery    
	SELECT aaa.ypCode, aaa.ypmc, aaa.dlCode sfdlCode, bbb.dlmc sfdlmc    
		, case @mzzybz when '1' then aaa.mzcldw when '2' then aaa.zycldw end dw    
	 , case @mzzybz when '1' then aaa.mzcls when '2' then aaa.zycls end cls    
		, CONVERT(DECIMAL(19,4), case @mzzybz when '1' then (case when aaa.mzcldw = aaa.bzdw then aaa.lsj else (aaa.lsj / aaa.bzs * aaa.mzcls) end) when '2' then (case when aaa.zycldw = aaa.bzdw then aaa.lsj else (aaa.lsj / aaa.bzs * aaa.zycls) end)end) dj   
 
		, aaa.py    
		, aaa.px    
		, aaa.jldw    
	 , aaa.jl jldwzhxs    
	 , aaa.zfxz    
	 , aaa.zfbl    
	 , ypkc.yfbmCode, convert(numeric(11,2),isnull(ypkc.zxdwsl,0)) kcsl, yfbm.yfbmmc    
	 , ypsx.ypgg    
	 , aaa.jx ypjxCode    
	 , ypkc.kzbz    
	 , ypsx.jzlx zyqzlx    
	 , ypsx.ybdm    
	 , ypsx.xzyy,ypsx.xzyysm,ypsx.mrjl,ypsx.mrpc,yzpc.yzpcmc mrpcmc,NULL isKss,NULL jlfwBegin,NULL jlfwEnd,NULL pcfwBegin,NULL pcfwEnd,NULL kssKy  
	 , aaa.bz   
	 , aaa. cxjje  
		from [NewtouchHIS_Base]..V_S_xt_yp(nolock) aaa    
		INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx(NOLOCK) ypsx ON ypsx.ypId=aaa.ypId AND ypsx.zt='1' AND ypsx.OrganizeId=aaa.OrganizeId    
		INNER join #tbdlCodeTemp(nolock) bbb on aaa.dlCode = bbb.dlCode    
	 INNER join [NewtouchHIS_PDS].dbo.V_S_P_Kc ypkc    
	 on ypkc.OrganizeId = aaa.OrganizeId and ypkc.ypCode = aaa.ypCode    
	 left join NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm    
	 on yfbm.OrganizeId = aaa.OrganizeId and yfbm.yfbmCode = ypkc.yfbmCode    
	 LEFT JOIN NewtouchHIS_Base..V_S_xt_yzpc yzpc    
	 ON ypsx.mrpc=yzpc.yzpcCode AND yzpc.zt = '1' AND yzpc.OrganizeId = ypsx.OrganizeId    
		where aaa.OrganizeId = @orgId    
	 and (isnull(@ypyfbmCode,'') = '' or ypkc.yfbmCode in (select * from dbo.f_split(isnull(@ypyfbmCode, ''), ',')))    
		and aaa.zt = '1'    
	 and (@mzzybz = '1' or @mzzybz = '2') --查询药品一定要指定查询门诊或住院    
	 and (yfbm.mzzybz = @mzzybz or yfbm.mzzybz = '3') --门诊/住院药房 或 通用药房    
	 and (@containyp0ck = 1 or ypkc.zxdwsl > 0)  
	END    
	ELSE    
	BEGIN    
	INSERT INTO #YpQuery    
	SELECT aaa.ypCode, aaa.ypmc, aaa.dlCode sfdlCode, bbb.dlmc sfdlmc    
		, case @mzzybz when '1' then aaa.mzcldw when '2' then aaa.zycldw end dw    
	 , case @mzzybz when '1' then aaa.mzcls when '2' then aaa.zycls end cls    
		, CONVERT(DECIMAL(19,4), case @mzzybz when '1' then (case when aaa.mzcldw = aaa.bzdw then aaa.lsj else (aaa.lsj / aaa.bzs * aaa.mzcls) end) when '2' then (case when aaa.zycldw = aaa.bzdw then aaa.lsj else (aaa.lsj / aaa.bzs * aaa.zycls) end)end) dj   
 
		, aaa.py    
		, aaa.px    
		, aaa.jldw    
	 , aaa.jl jldwzhxs    
	 , aaa.zfxz    
	 , aaa.zfbl    
	 , ypkc.yfbmCode, convert(numeric(11,2),isnull(ypkc.zxdwsl,0)) kcsl, yfbm.yfbmmc    
	 , ypsx.ypgg    
	 , aaa.jx ypjxCode    
	 , ypkc.kzbz    
	 , ypsx.jzlx zyqzlx    
	 , ypsx.ybdm    
	 , ypsx.xzyy,ypsx.xzyysm,ypsx.mrjl,ypsx.mrpc,yzpc.yzpcmc mrpcmc,aaa.isKss,kss.jlfwBegin,kss.jlfwEnd,    
	 kss.pcfwBegin,kss.pcfwEnd,CASE WHEN ISNULL(kss.qxjb,'0') <=@qxjb THEN '1' ELSE '0' END kssKy, --抗生素可用 1 可用,0 不可用(qxjb:0 非限制使用药物,1 限制使用药物,2 特殊使用药物)    
		 aaa.bz , aaa. cxjje   
	 from [NewtouchHIS_Base]..V_S_xt_yp(nolock) aaa    
		INNER JOIN NewtouchHIS_Base.dbo.V_S_xt_ypsx(NOLOCK) ypsx ON ypsx.ypId=aaa.ypId AND ypsx.zt='1' AND ypsx.OrganizeId=aaa.OrganizeId    
		INNER join #tbdlCodeTemp(nolock) bbb on aaa.dlCode = bbb.dlCode    
	 INNER join [NewtouchHIS_PDS].dbo.V_S_P_Kc ypkc    
	 on ypkc.OrganizeId = aaa.OrganizeId and ypkc.ypCode = aaa.ypCode    
	 left join NewtouchHIS_Base.dbo.V_S_xt_yfbm yfbm    
	 on yfbm.OrganizeId = aaa.OrganizeId and yfbm.yfbmCode = ypkc.yfbmCode    
	 LEFT JOIN NewtouchHIS_Base..V_S_xt_yzpc yzpc    
	 ON ypsx.mrpc=yzpc.yzpcCode AND yzpc.zt = '1' AND yzpc.OrganizeId = ypsx.OrganizeId    
	 LEFT JOIN NewtouchHIS_Base..xt_ypKss(NOLOCK) kss ON aaa.isKss='1' AND aaa.kssId = kss.Id AND kss.zt='1' AND aaa.OrganizeId = kss.OrganizeId    
		where aaa.OrganizeId = @orgId    
	 and (isnull(@ypyfbmCode,'') = '' or ypkc.yfbmCode in (select * from dbo.f_split(isnull(@ypyfbmCode, ''), ',')))    
		and aaa.zt = '1'    
	 and (@mzzybz = '1' or @mzzybz = '2') --查询药品一定要指定查询门诊或住院    
	 and (yfbm.mzzybz = @mzzybz or yfbm.mzzybz = '3') --门诊/住院药房 或 通用药房    
	 and (@containyp0ck = 1 or ypkc.zxdwsl > 0)   
	END  
	
/*医院要求控制不显示库存为0的药，故在此控制。后续若修改直接注释此处！20220805A*/
delete from #YpQuery where kcsl <= 0

END
--PRINT 2 RETURN
--6.显示 top @topCount 
select top (@topCount) * from    
(    
select sfxmCode sfxmCode , sfxmmc sfxmmc, py py    
, sfdlCode sfdlCode, sfdlmc sfdlmc    
, dw dw, dj dj,cxjje cxjje    
, yzlx    
, bz bz    
, duration duration    
, dwjls dwjls    
, jjcl jjcl    
, '' jldw    
, null jldwzhxs    
, zfxz zfxz    
, zfbl zfbl    
,cls    
,yfbmCode    
,yfbmmc    
, kcsl kcsl    
, gg gg    
, '' ypjxCode    
, '' kzbz    
, '' zyqzlx    
, zxks zxks    
, zxksmc zxksmc    
, ybdm ybdm    
, null xzyy,null xzyysm,NULL mrjl, NULL mrpc,NULL mrpcmc    
, NULL isKss    
, NULL jlfwBegin    
, NULL jlfwEnd    
, NULL pcfwBegin    
, NULL pcfwEnd    
, NULL kssKy    
from #XmQuery    
    
union all    
    
select ypCode sfxmCode , ypmc sfxmmc, py py    
, sfdlCode sfdlCode, sfdlmc sfdlmc    
, dw dw, dj dj ,cxjje cxjje    
, '1' yzlx    
, bz    
, null duration    
, null dwjls    
, null jjcl    
, jldw jldw    
, jldwzhxs jldwzhxs    
, zfxz zfxz    
, zfbl zfbl    
, cls cls    
, yfbmCode yfbmCode    
, yfbmmc yfbmmc    
, kcsl kcsl    
, ypgg gg    
, ypjxCode ypjxCode    
, kzbz kzbz    
, zyqzlx zyqzlx    
, '' zxks    
, '' zxksmc    
, ybdm ybdm    
, xzyy xzyy    
, xzyysm xzyysm    
,mrjl mrjl    
,mrpc mrpc    
,mrpcmc mrpcmc    
,isKss    
,jlfwBegin    
,jlfwEnd    
,pcfwBegin    
,pcfwEnd    
,kssKy    
from #YpQuery    
) as reslt    
where (isnull(@keyword, '') = '' or sfxmCode like '%' + @keyword + '%' or sfxmmc like '%' + @keyword + '%' or py like '%' + @keyword + '%')    
and (@onlyybflag = 0 or isnull(ybdm,'')<>'')    
    
order by sfxmCode    
    
--删除临时表    
drop table #tbdlCodeTemp  ,#XmQuery,#YpQuery  
return    
--exec [dbo].[usp_SelectSfxmYp] 100,'6d5752a7-234a-403e-aa1c-df8b45d3469f','1'    
--select * from [NewtouchHIS_Base]..V_S_xt_yp(nolock)
    
    
    
    
    
    
    
GO


