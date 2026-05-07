USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[usp_zy_OrderExecutionGetOrderExecutionYZList]    Script Date: 2025/12/1 15:23:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO












/**
exec [usp_zy_OrderExecutionGetOrderExecutionYZList] '03335','2025-11-28 11:00:00','876b94b4-eea4-4e51-b9cb-2552a6c0c1b6',NULL,NULL,NULL,NULL,1,20
,'kssj desc,zyh,yzlx,zh','asc',''

exec [usp_zy_OrderExecutionGetOrderExecutionYZList] '03335','2025-11-28 11:00:00','6d5752a7-234a-403e-aa1c-df8b45d3469f',NULL,NULL,NULL,NULL,1,20
,'kssj desc,zyh,yzlx,zh','asc',''
**/
ALTER  proc [dbo].[usp_zy_OrderExecutionGetOrderExecutionYZList] 
	@patList varchar(max),
	@vzxsj varchar(50),
	@orgId varchar(50),
	@wnes bit,
	@IsRehabAuthtoNurse varchar(2)='0',
	@Iskf  varchar(2)='0',
	@zxks varchar(20)='',
	@page INT ,	--分页数据 页码
    @rows INT ,	--分页数据 每页行数
    @sidx VARCHAR(50) ,	--分页数据 排序
    @sord VARCHAR(50) ,	--分页数据 排序方式 asc desc
    @records INT OUTPUT 	--分页数据 总记录数
as
begin

declare @kfwhere varchar(100)=''
declare @sql varchar(max);
--护士未授权康复医嘱执行权限
if(@IsRehabAuthtoNurse='0')
begin
	if(@Iskf='1')
	begin
		set @kfwhere=' and yzlx=11 and zxksdm='''+@zxks+''' '
	end
	else if(@Iskf='0')
	begin 
		set @kfwhere=' and yzlx<>11 '
	end
end  
set @sql='select * from (SELECT hzxm,zyh,id yzid,2 yzxz,''长期'' yzxzsm,kssj,xmdm,xmmc,ypjl,
yznr as yzmc, CONCAT(CONVERT(float,ypjl),a.dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc, 
isnull(convert(numeric(18,4),isnull(case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end,c.dj)),0) dj,a.tzsj tzsj,tzysgh,tzr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,'''' zh1,a.zxr zxr,isnull(isjf,1) isjf,
    a.yztag,case a.yztag when ''JI'' then ''精I'' when ''JII'' then ''精II'' when ''MZ'' then ''麻醉'' else a.yztag end yztagName--,yply
	,convert(numeric(18,2),isnull(convert(numeric(18,4),case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end)*a.sl,c.dj*a.sl)) je
	,isnull(a.sl,0) sl,(cast(sl as varchar)+isnull(yp.zycldw,'''')) slstr,a.yzh,a.isfsyz,a.px,deptCode
	from [dbo].[zy_cqyz] a with(nolock) 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c  ON c.sfxmCode=a.xmdm  AND c.OrganizeId=a.OrganizeId and c.zt=''1'' 
	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on a.ypyfdm = ypyf.yfCode 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on a.pcCode = yzpc.yzpcCode and a.organizeId=yzpc.organizeId
	LEFT JOIN  [NewtouchHIS_Base].[dbo].[xt_yp] yp on a.xmdm=yp.ypCode and a.OrganizeId=yp.OrganizeId
	where a.OrganizeId='''+@orgId+''' AND a.yzzt in (1,2) '+@kfwhere+'
	AND Convert(DATE,a.kssj)< =Convert(DATE,'''+@vzxsj+''') 
	AND (Convert(DATE,a.tzsj) IS NULL OR Convert(DATE,a.tzsj)>=Convert(DATE,'''+@vzxsj+''')) 
	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)<Convert(DATE,'''+@vzxsj+''')) 
	 and ((case when a.zxzqdw=''1'' and abs(datediff(d,a.zxsj,'''+@Vzxsj+''')%a.zxzq)=0 then ''1'' --天
 --when a.zxzqdw=''2'' then '''' --小时
 --when a.zxzqdw=''3'' then '''' --分钟
 else ''0'' end)=''1'' or (CONVERT(DATE,a.zxsj) IS NULL))
	AND a.zt=1 And a.zyh in(select col from dbo.f_split('''+@patList+''','','') where col>'''')';
 if @wnes='0' --不包含文字医嘱
 begin
 set @sql+=' and a.yzlx<>''3''';
 end

 --print @sql
 --return
 --convert(numeric(18,2),isnull(convert(numeric(18,4),yp.lsj/bzs)*a.sl,c.dj*a.sl)) je
--isnull(convert(numeric(18,4),isnull(yp.lsj/bzs,c.dj)),0) dj
 set @sql+=' union all
select hzxm,zyh, id yzid,1 yzxz,''临时'' yzxzsm,kssj,xmdm,xmmc,ypjl,
yznr as yzmc, CONCAT(CONVERT(float,ypjl),a.dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc,
isnull(convert(numeric(18,4),isnull(case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end,c.dj)),0) dj,a.zfsj tzsj,zfysgh,zfr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,'''' zh1,a.zxr zxr,isnull(isjf,1) isjf,
	a.yztag,case a.yztag when ''JI'' then ''精I'' when ''JII'' then ''精II'' when ''MZ'' then ''麻醉'' else a.yztag end yztagName--,yply
	,case when a.yzlx=''10'' then  isnull(isnull(yp.lsj*a.sl*ypjl,c.dj*a.sl*ypjl),0) else convert(numeric(18,2),isnull(convert(numeric(18,4),case when yp.zycldw=yp.bzdw THEN yp.lsj else yp.lsj/bzs end)*a.sl,c.dj*a.sl)) end je
	,case when a.yzlx=''10'' then  cast(isnull(ypjl,1) as int)*sl else isnull(a.sl,0) end sl
	,cast(case when a.yzlx=''10'' then  cast(isnull(ypjl,1) as int)*sl else isnull(a.sl,0) end  as varchar)+ isnull(yp.zycldw,'''') slstr,a.yzh,a.isfsyz,a.px,deptCode
	from [dbo].[zy_lsyz] a  with(nolock)
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c with(nolock) ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt=''1''
	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId	 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on a.ypyfdm = ypyf.yfCode 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on a.pcCode = yzpc.yzpcCode and a.organizeId=yzpc.organizeId
	LEFT JOIN  [NewtouchHIS_Base].[dbo].[xt_yp] yp on a.xmdm=yp.ypCode and a.OrganizeId=yp.OrganizeId
	where a.OrganizeId='''+@orgId+''' AND a.yzzt =1 '+@kfwhere+'
	AND  Convert(DATE,a.kssj)< =Convert(DATE,'''+@vzxsj+''')
	AND (Convert(DATE,a.zfsj) IS NULL OR Convert(DATE,a.zfsj)>=Convert(DATE,'''+@vzxsj+'''))
	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)< Convert(DATE,'''+@vzxsj+'''))
	AND a.zt=1 AND a.yzlx not in(''6'',''7'') and a.zyh in(select col from dbo.f_split('''+@patList+''','','') where col>'''')';
 if @wnes='0'--不包含文字医嘱
 begin
 set @sql+=' and a.yzlx<>''3''';
 end
  --print @sql
 --return
 set @sql+=' union all
 select hzxm,zyh,max(yzid) yzid,yzxz,yzxzsm,max(kssj) kssj,xmdm,xmmc,ypjl,--case when CHARINDEX((CAST(count(1) as varchar)+''项''),ztmc)>0 
--then ztmc else ztmc+CAST(count(1) as varchar)+''项'' end  
yzmc,yzjl,yfmc,yzpcmc ,sum(dj) dj,--,sum(dj) dj,
tzsj,zfysgh,zfr,shr,yzlx,zxsj,zh,zh1,zxr,isjf,'''' yztag, '''' yztagName--,yply 
,sum(je) je,1 sl,''1'' slstr,yzh,isfsyz,px,deptCode
from(
	select row_number() over(partition by yzh,ztmc order by a.createtime desc) num,
	hzxm,zyh, Id yzid,1 yzxz,''临时'' yzxzsm,kssj,'''' xmdm,ztmc xmmc,
	ypjl,ztmc as yzmc, CONCAT(CONVERT(float,ypjl),a.dw) as yzjl, ypyf.yfmc, yzpc.yzpcmc,
	isnull(ISNULL(yp.lsj,c.dj),0) dj,a.zfsj tzsj,zfysgh,zfr,b.Name shr,yzlx,a.zxsj zxsj,a.zh zh,yzh zh1,a.zxr zxr,isnull(isjf,1) isjf
	,isnull(isnull(yp.lsj*a.sl,c.dj*a.sl),0) je,isnull(a.sl,0) sl,a.yzh,a.isfsyz,''0'' px,a.deptCode
	from [dbo].[zy_lsyz] a  with(nolock)
	LEFT JOIN [NewtouchHIS_Base].[dbo].[V_S_xt_sfxm] c with(nolock) ON c.sfxmmc=a.xmmc AND c.sfxmCode=a.xmdm AND c.OrganizeId=a.OrganizeId and c.zt=''1''
	left join [NewtouchHIS_Base].[dbo].[V_C_Sys_UserStaff] b with(nolock) on a.CreatorCode=b.Account and a.OrganizeId=b.OrganizeId	  
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypyf] ypyf on a.ypyfdm = ypyf.yfCode 
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_yzpc] yzpc on a.pcCode = yzpc.yzpcCode and a.organizeId=yzpc.organizeId
	LEFT JOIN  [NewtouchHIS_Base].[dbo].[xt_yp] yp on a.xmdm=yp.ypCode and a.OrganizeId=yp.OrganizeId
	where a.OrganizeId='''+@orgId+''' AND a.yzzt =1 '+@kfwhere+'
	AND  Convert(DATE,a.kssj)< =Convert(DATE,'''+@vzxsj+''')
	AND (Convert(DATE,a.zfsj) IS NULL OR Convert(DATE,a.zfsj)>=Convert(DATE,'''+@vzxsj+'''))
	AND (Convert(DATE,a.zxsj) IS NULL OR Convert(DATE,a.zxsj)< Convert(DATE,'''+@vzxsj+'''))
	AND a.zt=1 and a.yzlx in(''6'',''7'') and a.zyh in(select col from dbo.f_split('''+@patList+''','','') where col>'''')';
  if @wnes='0'--不包含文字医嘱
   begin
   set @sql+=' and a.yzlx<>''3''';
   end
 set @sql+=' ) a
	group by 
				hzxm,zyh,yzxz,yzxzsm,xmdm,xmmc,ypjl, yzmc,yzjl,yfmc,yzpcmc ,
				tzsj,zfysgh,zfr,shr,yzlx,zxsj,zh,zh1,zxr,isjf, yzh,isfsyz,px,deptCode ';

 set @sql+=' ) as k';
-- print @sql
 SET @records='0'
 
 DECLARE @iiirecords INT;
  EXEC sp_QueryWithPage @sql, @rows, @page, @sidx, @sord,
        @iiirecords OUT;
		 SET @records = @iiirecords;
end









GO


