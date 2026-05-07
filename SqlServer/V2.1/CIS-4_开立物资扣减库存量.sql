USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[物资扣减库存量]    Script Date: 2025/11/21 15:39:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE proc [dbo].[物资扣减库存量]
@cfh varchar(50),
@orgId varchar(50),
@rygh varchar(50)
as

BEGIN

--declare
--@cfh varchar(50),
--@orgId varchar(50),@rygh varchar(50)
--select @cfh='R20230905N000109',@orgId='6d5752a7-234a-403e-aa1c-df8b45d3469f',@rygh='000000'


--select b.kcsl,(b.kcsl-a.sl)kcslyj,b.djsl,(b.djsl-a.sl) djslyj,a.sl  from kf_kcxx_djjl a
--left join kf_kcxx b on a.productId=b.productId and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
--where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

--库存表 扣除库存量 并且 扣除冻结量
update b set b.kcsl=(b.kcsl-a.sl),
b.djsl=(b.djsl-a.sl)  from dept_kcxx_djjl a with(nolock)
left join dept_kcxx b with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

--冻结表 修改isfs状态为1已扣库存量
update a set a.isfs='1',a.LastModifierCode=@rygh,a.LastModifyTime=GETDATE()  from dept_kcxx_djjl a with(nolock)
left join dept_kcxx b with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId


END

GO


