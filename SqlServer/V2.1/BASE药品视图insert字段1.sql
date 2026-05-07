USE [NewtouchHIS_Base]
GO

/****** Object:  View [dbo].[V_S_xt_yp]    Script Date: 2025/10/9 16:59:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*修改日期2022年3月18日10:37:57
修改人 陈杨洋
添加内容 新增cxjje字段*/
ALTER VIEW [dbo].[V_S_xt_yp]
AS
SELECT  a.ypId, a.ypCode, a.ypmc, a.OrganizeId, a.spm, a.py, a.cfl, a.cfdw, a.jl, a.jldw, a.bzs, a.bzdw, a.mzcls, a.mzcldw, a.zycls, 
                   a.zycldw, a.zxdw, a.djdw, a.lsj, a.pfj, a.zfbl, a.zfxz, a.dlCode, a.jx, a.ycmc, a.ypbzdm, a.nbdl, a.mzzybz, a.CreatorCode, 
                   a.CreateTime, a.LastModifyTime, a.LastModifierCode, a.zt, a.px, a.lsbz, a.mjzbz, a.yfCode, a.isKss, a.kssId, b.ybdm, a.bz, 
                   b.gjybdm, b.ypgg, a.cxjje, a.tsypbz,b.kcyjz,pzwh
FROM      dbo.xt_yp AS a WITH (NOLOCK) INNER JOIN
                   dbo.xt_ypsx AS b WITH (NOLOCK) ON a.ypCode = b.ypCode AND a.ypId = b.ypId AND a.OrganizeId = b.OrganizeId
WHERE   (a.zt = '1')
GO


