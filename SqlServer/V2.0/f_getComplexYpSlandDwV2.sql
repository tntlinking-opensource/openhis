USE [NewtouchHIS_PDS]
GO

/****** Object:  UserDefinedFunction [dbo].[f_getComplexYpSlandDwV2]    Script Date: 2025/9/4 17:50:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE function [dbo].[f_getComplexYpSlandDwV2]
(
	@sl int, --最小单位数量
	@zhxs int,	--部门转换系数
	@zhdw varchar(8),	--部门单位
	@zxdw varchar(8)	--最小单位
	)
RETURNS varchar(200)
as   
    begin     
      return case when @sl <> 0
   then
   (
		(case when Floor(@sl / @zhxs) <> 0 then Convert(varchar(20), Floor(@sl / @zhxs)) + isnull(@zhdw,'') else '' end)
	+ (case when @sl % @zhxs <> 0 then (Convert(varchar(20),(@sl % @zhxs)) + isnull(@zxdw,'')) else '' end)
	)
	else '0' end	--可领数量and单位 5盒1支，仅显示用
	end

GO


