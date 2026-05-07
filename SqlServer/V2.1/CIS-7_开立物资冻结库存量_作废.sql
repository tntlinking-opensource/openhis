USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[开立物资冻结库存量_作废]    Script Date: 2025/11/21 15:38:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE proc [dbo].[开立物资冻结库存量_作废]
@cfh varchar(50),
@orgId varchar(50),
@rygh varchar(50)
as

BEGIN

--declare
--@cfh varchar(50),
--@orgId varchar(50),@rygh varchar(50)
--select @cfh='R20230904N000068',@orgId='6d5752a7-234a-403e-aa1c-df8b45d3469f',@rygh='000000'

update b set b.djsl=b.djsl-a.sl from Dept_kcxx_djjl a
left join Dept_kcxx b on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

update a set a.zt='0',LastModifierCode=@rygh,LastModifyTime=GETDATE() from Dept_kcxx_djjl a
left join Dept_kcxx b on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
where a.zt='1' and a.isfs='0' and a.cfh=@cfh and a.OrganizeId=@orgId

END
GO


