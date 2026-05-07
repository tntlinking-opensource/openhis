USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[开立物资冻结库存量]    Script Date: 2025/11/21 15:38:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
remark:开立科室库存库存冻结
EXEC [开立物资冻结库存量] 'R20251112N000048','6d5752a7-234a-403e-aa1c-df8b45d3469f','bdamin'
*/
CREATE proc [dbo].[开立物资冻结库存量]
@cfh varchar(50),
@orgId varchar(50),
@rygh varchar(50)
as
--declare
--@cfh varchar(50),
--@orgId varchar(50),@rygh varchar(50)
--select @cfh='R20251112N000048',@orgId='6d5752a7-234a-403e-aa1c-df8b45d3469f',@rygh='bdadmin'

--select * from kf_kcxx where productId='56b4c94d-e661-4613-b21a-e022fa4a68f8'
--update kf_kcxx set djsl=0 where productId='56b4c94d-e661-4613-b21a-e022fa4a68f8'
--select * from Newtouch_CIS..xt_cfmx

IF(NOT EXISTS (select 1  from Sys_Config where code='openWzhckc' and organizeid=@orgId and zt='1' and value='ON'))
BEGIN
	RETURN;
END


create table #v(
Id int IDENTITY(1,1) not null,
OrganizeId varchar(50) not null,
ks varchar(20) not null,
cfh varchar(50) not null,
sfxmcode varchar(50) not null,
pc varchar(50) null,
ph varchar(50) null,
sl numeric(10,2) not null,
isfs char(1) not null,
zt char(1) not null,
CreateTime datetime not null,
CreatorCode varchar(50)  null,
LastModifyTime datetime null,
LastModifierCode varchar(50) null,
)

BEGIN
--select * from kf_kcxx_djjl
--先判断是否是修改 如果是 作废冻结记录并退还冻结数  重新计算扣减库存数量
update b set b.djsl=b.djsl-a.sl from dept_kcxx_djjl a with(nolock)
left join dept_kcxx b with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

update a set a.zt='0' from dept_kcxx_djjl a with(nolock)
left join dept_kcxx b with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

--处方下耗材明细
select a.cfh,b.xmCode sfxmcode,b.sl,c.productCode,a.ks,ROW_NUMBER() over(order by a.cfh,b.xmCode,b.sl)num
into #temp
from Newtouch_CIS..xt_cf a with(nolock)
join Newtouch_CIS..xt_cfmx b with(nolock) on a.cfId=b.cfId and b.zt='1' and a.OrganizeId=b.OrganizeId
join NewtouchHIS_herp..wz_product c with(nolock) on b.xmCode=c.productCode and b.OrganizeId=c.OrganizeId and c.zt='1'
where a.cflx='8' and a.zt='1' and a.cfh=@cfh and a.OrganizeId=@orgId  


declare @a int ,@maxnum int 
set @a=1
select @maxnum=COUNT(*) from #temp
while @a<=@maxnum
begin

/*处理数据 耗材循环开始*/
--print(@a)
IF EXISTS(SELECT 1 FROM tempdb..sysobjects where id=object_id(N'tempdb..#kcxx') and type='U')
BEGIN
	DROP TABLE #kcxx;
END

select b.*,a.cfh,a.sfxmcode,a.sl,ROW_NUMBER() over(order by b.yxq )num 
into #kcxx 
from #temp a
left join dept_kcxx b with(nolock) on a.productCode=b.productCode and a.ks=b.ks and b.zt='1'
where (b.kcsl-b.djsl)>0 and a.num=@a
order by yxq 


--select * from #kcxx
/*循环库存信息 判断获取批次 批号及所需要扣减的库存量 根据有效期排序*/
declare @b int ,@kcxxnum int ,@sykcl numeric(10,2)
declare @kcxxsl numeric(10,2) ,@kjsl numeric(10,2) --记录需要扣减的库存
set @b=1
set @sykcl=1
select @kcxxnum=COUNT(*),@kjsl=max(sl) from #kcxx--获取需要扣减的库存数量
while @b<=@kcxxnum and @sykcl>0--如果已经扣减够了提前退出循环
begin /*库存信息循环 开始*/

select @kcxxsl=(kcsl-djsl) from #kcxx where num=@b
if(@kcxxsl>=@kjsl)--库存量够了直接扣减
begin
set @sykcl=@kjsl
--print('库存量够了直接扣减')

insert into #v(OrganizeId,ks,cfh,sfxmcode,pc,ph,sl,isfs,zt,CreateTime,CreatorCode)
select OrganizeId,ks,cfh,sfxmcode,pc,ph,@sykcl sl,0 isfs,1 zt,GETDATE() createtime, @rygh CreatorCode from #kcxx where num=@b

set @sykcl=0

end
else--库存量不够 先扣减当前批次批号所有库存量 再次循环扣减剩余库存
begin

select  @kjsl=@kjsl-@kcxxsl
set @sykcl=@kjsl
--print('库存量不够'+convert(varchar(20), @sykcl))
insert into #v(OrganizeId,ks,cfh,sfxmcode,pc,ph,sl,isfs,zt,CreateTime,CreatorCode)
select OrganizeId,ks,cfh,sfxmcode,pc,ph,@kcxxsl sl,0 isfs,1 zt,GETDATE() createtime, @rygh CreatorCode from #kcxx where num=@b
end
set @b=@b+1
end /*库存信息循环 结束*/

/*处理数据 结束*/
set @a = @a +1

end

--增加冻结数
insert into dept_kcxx_djjl(OrganizeId,ks,cfh,sfxmcode,pc,ph,sl,isfs,zt,CreateTime,CreatorCode)
select OrganizeId,ks,cfh,sfxmcode,pc,ph,sl,isfs,zt,CreateTime,CreatorCode from #v

update b set b.djsl= b.djsl+a.sl from #v a
left join dept_kcxx b on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

--select * from #v

drop table #temp
drop table #v

END
GO


