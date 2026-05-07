USE [Newtouch_EMR]
GO

/****** Object:  StoredProcedure [dbo].[usp_bl_zybrjbxx_bak]    Script Date: 2026/3/24 15:32:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




/*==================================================         
过程内容：查询住院病人信息 
新增时间：2026年3月24日 
使用程序：emr --病历文书   

usp_bl_zybrjbxx_V2 @zyh='00007',@organizeId='9bb029d0-5da0-4118-9d19-06b829eede46',@user='mzys01'       
select *from dbo.bl_ys where DataSource='zy_brjbxx'    
==================================================*/    
create proc [dbo].[usp_bl_zybrjbxxV2]
(
@zyh varchar(50),
@organizeId varchar(50)
)
as

select 
a.zyh as zyh, --住院号
a.blh as blh, --[病历号]
c.bqmc as bqmc, --[病区名称]
a.brxzmc as brxzmc, --[病人性质名称]
a.birth as csny, --[出生年月]
a.cyfs as cyfs, --[出院方式]
a.cqrq as cqrq, --[出院日期]
a.cyzdmc as cyzdmc, --出院诊断名称]
a.BedName as cwmc, --[床位名称]
a.gj as gj,  --[国籍]
a.hljb as hljb, --[护理级别]
b.name as ksmc, --[科室名称]
a.lxrdh as lxrdh, --[联系人电话]
a.lxrgx as lxrgx, --[联系人关系]
a.lxr as lxr,  --[联系人姓名]
mz.mzmc as mz, --[民族]
a.nlshow as nl1,
CONVERT(VARCHAR(10),DATEDIFF(year,a.birth,GETDATE()))   as nl, --[年龄]
a.rqrq as rqrq, --[入区日期]
a.ryrq as ryrq,  --[入院日期]
a.zddm as ryzddm, --[入院诊断代码]
a.zdmc as ryzdmc, --[入院诊断名称]
a.wzjb as wzjb,  --[危重级别]
a.sex as xb, --[性别]
a.xm as xm, --[姓名]
d.name as ysxm, --[医生姓名]
a.zy as zy, --[职业]
'' as zs, --主诉
a.cardno as kh, --[主要卡号]
--convert(varchar(10),dbo.get_zyts(a.rqrq,a.cqrq)) + '天' as zyts,    
a.zyts as zyts, --[住院天数]

-----------新增元素
g.ysmc zyys, --住院医生
h.ysmc zzys, --主治医生
i.ysmc zrys, --责任医生
k.name zrhs, --责任护士
isnull(e.cs_sheng,'') +isnull(e.cs_shi,'') +isnull(e.cs_xian,'')  cs_dz , --出生地
isnull(e.xian_sheng,'') +isnull(e.xian_shi,'') +isnull(e.xian_xian,'')+isnull(e.xian_dz,'') xian_dz ,--现地址
isnull(e.hu_sheng,'') +isnull(e.hu_shi,'') +isnull(e.hu_xian,'')+isnull(e.hu_dz,'') hu_dz , -- 户籍地
isnull(e.dwmc,'') dwmc, --单位每次
case e.hf when '1' then '未婚' when 2 then '已婚' when 3 then '丧偶' when 4 then '离婚' else '其他' end hf, --婚姻
'' as jg, 
e.phone as phone, --电话
l.zycs as zycs, --住院次数
convert (varchar(100),GETDATE(),120) jlrq, --记录日期
f.name yymc , --医院名称
'' as sg, --身高
'' as tz  --体重
from zy_brjbxx a  with(nolock)  
left join [NewtouchHIS_Base].[dbo].[Sys_Department] b on a.DeptCode=b.Code and a.organizeId=b.organizeId and a.zt=b.zt
left join [NewtouchHIS_Base].[dbo].[xt_bq] c on a.WardCode=c.bqCode and a.organizeId=c.organizeId and a.zt=c.zt
left join [NewtouchHIS_Base].[dbo].[Sys_Staff] d on a.ysgh=d.gh and a.organizeId=d.organizeId and a.zt=d.zt
left join [NewtouchHIS_Sett].dbo.xt_brjbxx e on a.blh=e.blh and a.organizeId=e.organizeId and a.zt=e.zt
left join NewtouchHIS_Base.dbo.V_S_xt_mz mz on mz.mzCode = e.mz and mz.zt = '1'  
left join [NewtouchHIS_Base].[dbo].[Sys_Organize] f on a.organizeId=f.id and a.zt=e.zt
left join [Newtouch_CIS].[dbo].zy_PatDocInfo g on a.zyh=g.zyh and  a.organizeId=g.organizeId and a.zt=g.zt and g.type=1 --住院医生
left join [Newtouch_CIS].[dbo].zy_PatDocInfo h on a.zyh=h.zyh and  a.organizeId=h.organizeId and a.zt=h.zt and h.type=2 --主治医生
left join [Newtouch_CIS].[dbo].zy_PatDocInfo i on a.zyh=i.zyh and  a.organizeId=i.organizeId and a.zt=i.zt and i.type=3 --主任医生
left join [Newtouch_CIS].[dbo].zy_bedCard j on a.zyh =j.zyh and a.organizeId=j.organizeId and a.zt=j.zt 
left join [NewtouchHIS_Base].[dbo].[Sys_Staff] k on j.organizeId= k.organizeId and j.zrhs= k.gh and j.zt=k.zt --责任护士
left join (select zyh,count(*) zycs from [Newtouch_CIS].[dbo].[zy_brxxk] 
where organizeId=@organizeId  and zt=1
group by zyh)l on a.zyh=l.zyh
where a.zyh=@zyh and a.organizeId=@organizeId and a.zt=1
 

GO


