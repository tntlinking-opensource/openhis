USE [NewtouchHIS_PDS]
GO

/****** Object:  UserDefinedFunction [dbo].[f_getYfYpComplexYpSlandDwV2]    Script Date: 2025/9/4 17:51:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE function [dbo].[f_getYfYpComplexYpSlandDwV2]
(
	@sl int,
	@mzzybz varchar(1),	--门诊住院标志 0药库 1门诊药房 2住院药房 3混合
	@ypCode varchar(50),	--药品Code
	@orgId varchar(50)	--组织机构Id
	)
RETURNS varchar(200)
as   
    begin

	if(@mzzybz <> '0' and @mzzybz <> '1' and @mzzybz <> '2' AND @mzzybz <> '3')
		return ''

	declare @zhxs int
	declare @zhdw varchar(8)
	declare @zxdw varchar(8)	--最小单位

	declare @bzs numeric(9,4)
	declare @bzdw varchar(20)
	declare @mzcls numeric(9,4)
	declare @mzcldw varchar(20)
	declare @zycls numeric(9,4)
	declare @zycldw varchar(20)

	select @bzs = bzs,@bzdw = bzdw,@mzcls = mzcls,@zycls = zycls,@mzcldw = mzcldw,@zycldw = zycldw,@zxdw = zxdw
	from [NewtouchHIS_Base]..V_S_xt_yp where ypCode = @ypCode and OrganizeId = @orgId

	if(@mzzybz = '0')
	begin
		set @zhxs = @bzs
		set @zhdw = @bzdw
	end

	if(@mzzybz = '1')
	begin
		set @zhxs = @mzcls
		set @zhdw = @mzcldw
	end

	if(@mzzybz = '2')
	begin
		set @zhxs = @zycls
		set @zhdw = @zycldw
	end

	if(@mzzybz = '3')
	begin
		set @zhxs = @mzcls
		set @zhdw = @mzcldw
	END
    
	return dbo.f_getComplexYpSlandDwV2(@sl, @zhxs, @zhdw, @zxdw)

    end
GO


