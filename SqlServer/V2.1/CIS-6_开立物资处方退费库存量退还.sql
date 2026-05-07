USE [Newtouch_CIS]
GO

/****** Object:  StoredProcedure [dbo].[物资处方退费库存量退还]    Script Date: 2025/11/21 15:40:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE proc [dbo].[物资处方退费库存量退还]
@cfh varchar(50),
@orgId varchar(50),
@rygh varchar(50),
@zxrq varchar(20)
as

BEGIN

IF(@zxrq!=NULL)
BEGIN
	--库存表退还库存量 
	update b set b.kcsl=(b.kcsl+a.sl) from Dept_kcxx_djjl a with(nolock)
	left join Dept_kcxx b  with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
	where a.zt='1' and a.isfs='1' and a.cfh =@cfh and a.OrganizeId=@orgId and convert(varchar(18),a.fyrq,121)=@zxrq

	--冻结表 修改zt状态为0已作废
	update a set a.zt='0',a.LastModifierCode=@rygh,a.LastModifyTime=GETDATE()  from Dept_kcxx_djjl a with(nolock)
	left join Dept_kcxx b with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
	where a.zt='1' and a.isfs='1' and a.cfh =@cfh and a.OrganizeId=@orgId and convert(varchar(18),a.fyrq,121)=@zxrq
END
ELSE
BEGIN
	--库存表退还库存量 
	update b set b.kcsl=(b.kcsl+a.sl) from Dept_kcxx_djjl a with(nolock)
	left join Dept_kcxx b  with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
	where a.zt='1' and a.isfs='1' and a.cfh in (select col from f_split(@cfh,',')) and a.OrganizeId=@orgId



	--冻结表 修改zt状态为0已作废
	update a set a.zt='0',a.LastModifierCode=@rygh,a.LastModifyTime=GETDATE()  from Dept_kcxx_djjl a with(nolock)
	left join Dept_kcxx b with(nolock) on a.sfxmcode=b.productCode and a.ks=b.ks and a.pc=b.pc and a.ph=b.ph and b.OrganizeId=a.OrganizeId and b.zt='1' 
	where a.zt='1' and a.isfs='1' and a.cfh in (select col from f_split(@cfh,',')) and a.OrganizeId=@orgId

END


END

GO


