USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[Dept_kcxx_djjl]    Script Date: 2025/11/13 16:57:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Dept_kcxx_djjl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrganizeId] [varchar](50) NOT NULL,
	[ks] [varchar](50) NOT NULL,
	[mzzyh] [varchar](20) NULL,
	[cfh] [varchar](50) NOT NULL,
	[sfxmcode] [varchar](50) NOT NULL,
	[pc] [varchar](50) NULL,
	[ph] [varchar](50) NULL,
	[fyrq] [datetime] NULL,
	[sl] [numeric](10, 2) NOT NULL,
	[isfs] [char](1) NOT NULL,
	[zt] [char](1) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[CreatorCode] [varchar](50) NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
 CONSTRAINT [PK_DEPTKCXX_DJJL] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键自增ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'Id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门诊填入处方号 住院填入医嘱ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'cfh'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'wz_product.productCode也是base库中xt_sfxm.sfxmcode' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'sfxmcode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批次' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'pc'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'ph'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当前批次批号扣减的数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'sl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'默认为0 作废处方为0 收费后为1代表冻结数清空库存扣减' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl', @level2type=N'COLUMN',@level2name=N'isfs'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'物资库存冻结记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx_djjl'
GO


