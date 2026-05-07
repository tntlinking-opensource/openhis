USE [Newtouch_EMR]
GO

/****** Object:  StoredProcedure [dbo].[usp_Pat_MedRecordTree]    Script Date: 2026/3/28 15:17:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*  
author:chl  
createtime:2018-9-4  
desc:获取患者全部病历树  
bllx --0 zybl,1 bcjl,2 ylws,3 hljl,4 basy  
exec usp_Pat_MedRecordTree '6d5752a7-234a-403e-aa1c-df8b45d3469f','03344','000000'  

----------------------------------------------------
修改原因：按要求不显示病历书写的二级菜单
修改方式：去除插入该菜单的语句
修 改 人：朱骏	
修改日期：2022-09-21
修改标志：20220921A
----------------------------------------------------
*/  
alter proc [dbo].[usp_Pat_MedRecordTree]  
 @OrgId varchar(50),  
 @zyh varchar(50),  
 @rygh varchar(50)  
as 

/*
declare  @OrgId varchar(50),  
 @zyh varchar(50),  
 @rygh varchar(50) 
 
select @zyh=N'01551',@OrgId=N'6d5752a7-234a-403e-aa1c-df8b45d3469f',@rygh=N'000000'
*/

begin  
 declare @config varchar(2000)  
  
 --select a.Id,a.Name,convert(varchar(2),'') Blzt,convert(varchar(20),'')Doccode,convert(varchar(50),'')Docname,  
 --convert(datetime,null) Blrq, convert(varchar(50),null)parentId,a.Code,0 Dateclass,  
 --convert(datetime,null)LastModifierCode,convert(varchar(50),'') BllxId ,  
 --convert(varchar(50),'')zyh,convert(varchar(50),'') BlId ,1 addPermit,  
 --(case a.code when 'zybl' then 1 when 'bcjl' then 2 when 'ylws' then 3 when 'hljl' then 4 when 'basy' then 5 when 'kfpg' then 6 end)bllx,  
 ---1 ctrlLevel  
 ----null bllx  
 --into #tmp  
 --from [NewtouchHIS_Base].[dbo].[Sys_ItemsDetail] a with(nolock)  
 --where exists(select 1 from [NewtouchHIS_Base].[dbo].[Sys_Items] b with(nolock)  
 --   where a.itemid=b.id and b.code='MedRecordDocuments' and b.zt=1)  
 --and a.zt=1  
 --2021-1-6 chl 数据表维护病案大类  
 --取权限范围内病历大类  
 select a.Id,a.bllxmc [name],
		convert(varchar(2),'') Blzt,
		convert(varchar(20),'')Doccode,
		convert(varchar(50),'')Docname,  
		convert(datetime,null) Blrq, 
		parentId,
		a.bllxcode code,
		0 Dateclass,  
		convert(datetime,null)LastModifierTime,
		convert(varchar(50),'') BllxId ,  
		convert(varchar(50),'')zyh,
		convert(varchar(50),'') BlId ,
		1 addPermit,
		a.bllx,
		a.MenuLev,
		MenuLevName,  
		-1 ctrlLevel,
		convert(varchar(50),'') mbId,
		convert(varchar(2),'') PlanStu ,
		convert(varchar(6),'') doctype
   into #tmp  
   from bl_bllx a with(nolock)  
  where a.zt='1' 
    and a.OrganizeId=@OrgId 
	and exists(select 1 
				 from [NewtouchHIS_Base].[dbo].[V_C_Sys_StaffDuty] b  
				where b.organizeid= a.OrganizeId 
				  and b.StaffGh=@rygh 
				  and --isroot='1' and   
					  CHARINDEX(','+b.DutyCode+',',','+a.RelDutys)>0
				)  

 order by a.bllx  
  
 if(@zyh<>'')  
 begin   
  
  select a.Id,a.blmc,a.blzt,a.ysgh,a.ysxm,a.blrq,a.LastModifierCode,max(isnull(a.[LastModifyTime],a.CreateTime))CreateTime,a.zyh,  
  a.blid,a.zt,a.bllx,max(b.ctrlLevel)ctrlLevel, --多岗位权限配置时取最高权限,  
  a.mbId,a.PlanStu ,a.doctype 
    into #list  
    from [dbo].[zy_meddocs_relation] a with(nolock) ,bl_mbqxkz b with(nolock)  
   where a.organizeid=@OrgId 
     and a.zyh=@zyh 
	 and a.zt=1
	 and a.mbid=b.mbid 
	 and b.zt=1 
	 and exists( select 1   
				   from [NewtouchHIS_Base].[dbo].[V_C_Sys_StaffDuty] c with(nolock)  
				  where organizeid=@OrgId 
				    and staffgh=@rygh 
					and b.dutycode=c.dutycode )  
  group by a.Id,a.blmc,a.blzt,a.ysgh,a.ysxm,a.blrq,a.LastModifierCode,a.zyh,a.blid,a.zt,a.bllx,a.mbId,a.PlanStu  ,a.doctype

  --病历管理员只读权限  
  if(not exists(select 1 from #list) 
	 and exists( select 1   
				   from [NewtouchHIS_Base].[dbo].[V_C_Sys_StaffDuty] c with(nolock)  
				  where organizeid=@OrgId 
				    and staffgh=@rygh 
					and c.dutycode='blmanager' ))  
  begin  
   insert  into #list  
   select a.Id,a.blmc,a.blzt,a.ysgh,a.ysxm,a.blrq,a.LastModifierCode,max(isnull(a.[LastModifyTime],a.CreateTime))CreateTime,a.zyh,  
   a.blid,a.zt,a.bllx,1 ctrlLevel, --多岗位权限配置时取最高权限,  
   a.mbId,a.PlanStu,a.doctype   
   from [dbo].[zy_meddocs_relation] a with(nolock) ,bl_mbqxkz b with(nolock)  
   where a.organizeid=@OrgId and a.zyh=@zyh and a.zt=1  
   and a.mbid=b.mbid and b.zt=1   
   group by a.Id,a.blmc,a.blzt,a.ysgh,a.ysxm,a.blrq,a.LastModifierCode,a.zyh,a.blid,a.zt,a.bllx,a.mbId,a.PlanStu  ,a.doctype
  end  
  
  insert into #tmp(id,name,Blzt,Doccode,Docname,Blrq,parentId,Code,Dateclass,LastModifierTime,BllxId,  
  zyh,BlId,addPermit,bllx,ctrlLevel,MenuLev,MenuLevName,mbId,PlanStu,doctype)  
  select a.Id,a.blmc,a.blzt,ysgh,ysxm,a.blrq,b.Id,b.code ,0,CreateTime,b.Id,  
  a.zyh,a.blid,1,b.bllx,a.ctrlLevel,b.MenuLev,MenuLevName,a.mbId,a.PlanStu ,a.doctype 
  from #list a ,#tmp b  
  where zt=1 and a.bllx=b.bllx  
  and b.MenuLev=1 --无需处理子目录   

  /*插入日期菜单					20220921A*/
  /*insert into #tmp(id,name,Blzt,Docname,Blrq,parentId,Code,Dateclass,LastModifierTime,BllxId,addPermit,bllx,ctrlLevel,MenuLev,MenuLevName,mbId,PlanStu)  
  select newid(),convert(varchar(7),a.blrq,120) blmc,'','',null,b.Id,b.code ,1,null,b.id,1,b.bllx,a.ctrlLevel,MenuLev,MenuLevName,'' mbId,'' PlanStu  
  from #list a ,#tmp b  
  where zt=1 
    and a.bllx=b.bllx  
    and b.MenuLev=2  --子目录为yyyy-MM  
  group by b.Id,b.code,convert(varchar(7),a.blrq,120),b.bllx,a.ctrlLevel,MenuLev,MenuLevName--,a.mbId,a.PlanStu  */

  insert into #tmp(id,name,Blzt,Doccode,Docname,Blrq,parentId,Code,Dateclass,LastModifierTime,BllxId,zyh,  
  BlId,addPermit,bllx,ctrlLevel,MenuLev,MenuLevName,mbId,PlanStu,doctype)  
  select a.Id,a.blmc,a.blzt,ysgh,ysxm,a.blrq,b.Id,b.code ,0,CreateTime,b.id,a.zyh,  
  a.blid,1,b.bllx,a.ctrlLevel,MenuLev,MenuLevName,a.mbId,a.PlanStu ,a.doctype 
  from #list a,#tmp b  
  where zt=1 
    and a.bllx=b.bllx 
	/*and b.bllxId=b.parentId*/					/*20220921A*/
	and b.MenuLev=2 --护理与病程需进行二级处理 
	/*and b.Dateclass=1									/*20220921A*/
	and convert(varchar(7),a.blrq,120)=b.name*/ 

  --3  
  insert into #tmp(id,name,Blzt,Docname,Blrq,parentId,Code,Dateclass,LastModifierTime,BllxId,addPermit,bllx,ctrlLevel,MenuLev,MenuLevName,mbId,PlanStu,doctype)  
  select newid(),MenuLevName blmc,'','',null,b.Id,b.code ,1,null,b.id,1,b.bllx,a.ctrlLevel,MenuLev,MenuLevName,a.mbId,a.PlanStu,a.doctype  
  from #list a ,#tmp b  
  where zt=1 and a.bllx=b.bllx  
    and b.MenuLev=3  --子目录为yyyy-MM  
  group by b.Id,b.code,b.bllx,a.ctrlLevel,MenuLev,MenuLevName,a.mbId,a.PlanStu  ,a.doctype  
  
  insert into #tmp(id,name,Blzt,Doccode,Docname,Blrq,parentId,Code,Dateclass,LastModifierTime,BllxId,zyh,  
  BlId,addPermit,bllx,ctrlLevel,MenuLev,MenuLevName,mbId,PlanStu,doctype)  
  select a.Id,a.blmc,a.blzt,a.ysgh,ysxm,a.blrq,b.Id,b.code ,0,a.CreateTime,b.id,a.zyh,  
  a.blid,1,b.bllx,a.ctrlLevel,MenuLev,MenuLevName,a.mbId,a.PlanStu  ,a.doctype
  from #tmp b,#list a  
  where a.zt=1 and a.bllx=b.bllx and b.bllxId=b.parentId  
  and b.MenuLev=3    
  and b.Dateclass=1  
  and MenuLevName=b.[name] and a.bllx=b.bllx      
 
  drop table #list  
 end  
  
  
  
 select a.Id,Name,Blzt,Doccode,Docname,Blrq,a.parentId,a.LastModifierTime,a.BllxId,rtrim(zyh)zyh,rtrim(BlId)BlId,addPermit,a.bllx,ctrlLevel ,b.bllx parentbllx,c.Ybbm,PlanStu,c.LoadWay,c.mblj  ,doctype
 from #tmp  a   
 left join bl_bllx b on a.ParentId=b.Id  
 left join bl_mblb c with(nolock) on a.mbid=c.id   
 order by a.bllx,LastModifierTime desc  
  
 drop table #tmp  
end  
  
GO


