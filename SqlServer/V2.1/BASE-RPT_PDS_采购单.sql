USE [NewtouchHIS_Base]
GO

/****** Object:  StoredProcedure [dbo].[RPT_PDS_采购单]    Script Date: 2025/10/17 15:29:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
exec RPT_PDS_采购单 'cd66562c-8f9f-4363-a1c2-d91432688ca4','6d5752a7-234a-403e-aa1c-df8b45d3469f'
**/
CREATE PROCEDURE [dbo].[RPT_PDS_采购单] 
(
@cgId varchar(50),--发票号
@hospitalCode varchar(50)
)    
AS
SELECT cgmx.cgmxId,cgmx.ypCode,cgmx.ypName,cgmx.sxh,cgmx.splx,case cgmx.cglx when '1' then '常规采购' when '2' then '紧急采购' end cglx ,
	    cgmx.zxspbm,cgmx.cgjldw,cgmx.ggbz,cgmx.cgsl,cgmx.cgdj,
		cgmx.dcpsbs,cgmx.bzsm,cgmx.dw,cgmx.zje,cgmx.yqbm,gys.gysmc yqmc,
		sfdl.dlmc,
	    yp.bzs, yp.bzdw, yp.zxdw, ddbh,yyjhdh,yp.ycmc
		,yp.lsj/yp.bzs zxdwlsj,yp.pfj/yp.bzs zxdwpfj,yp.lsj yklsj,yp.pfj ykpfj
	FROM NewtouchHIS_PDS..xt_yp_cgmx cgmx (nolock)
	JOIN NewtouchHIS_PDS..xt_yp_cg cg (nolock) on cgmx.cgId=cg.cgId and cg.OrganizeId=cgmx.OrganizeId and cg.zt='1'
	LEFT JOIN [NewtouchHIS_Base].dbo.V_S_xt_yp yp (nolock) on cgmx.ypCode =yp.ypCode and cgmx.OrganizeId=yp.OrganizeId and cgmx.zt=yp.zt
	LEFT JOIN [NewtouchHIS_Base].[dbo].[xt_ypgys] gys (nolock) on cgmx.yqbm=gys.gysCode and cgmx.OrganizeId=gys.OrganizeId and cgmx.zt=gys.zt
	LEFT JOIN NewtouchHIS_Base.dbo.V_S_xt_sfdl sfdl ON sfdl.dlCode=yp.dlCode AND sfdl.OrganizeId=cgmx.OrganizeId AND sfdl.zt='1'
	WHERE cgmx.zt = '1'
	  and cgmx.OrganizeId=@hospitalCode
	  and cgmx.cgId=@cgId



GO


