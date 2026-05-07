USE [NewtouchHIS_PDS]
GO

/****** Object:  UserDefinedFunction [dbo].[f_getYfbmYpComplexYpSlandDwV2]    Script Date: 2025/9/4 17:50:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE function [dbo].[f_getYfbmYpComplexYpSlandDwV2]
(
	@sl int, --最小单位
	@yfbmCode varchar(30),	--药房部门Code
	@ypCode varchar(50),	--药品Code
	@orgId varchar(50),	--组织机构Id
	@mzzybz varchar(1) --门诊住院标志
	)
RETURNS varchar(200)
as   
    BEGIN

		DECLARE @mzzybz1 VARCHAR(1)	--门诊住院标志 0药库 1门诊药房 2住院药房 3混合

		SELECT @mzzybz1 = mzzybz FROM [NewtouchHIS_Base]..V_S_xt_yfbm WHERE yfbmCode = @yfbmCode and OrganizeId = @orgId
		if(@mzzybz1='3') --混合药房按门诊住院实际拆零换算
		begin
			set @mzzybz=@mzzybz
		end
		else
		begin
			set @mzzybz=@mzzybz1
		end
		RETURN dbo.f_getYfYpComplexYpSlandDwV2(@sl, @mzzybz, @ypCode, @orgId);
		--RETURN dbo.f_getYfYpComplexYpSlandDw(@sl, @mzzybz, @ypCode, @orgId);

    END 
GO


