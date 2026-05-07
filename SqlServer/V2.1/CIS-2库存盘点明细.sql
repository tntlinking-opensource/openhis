USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[dept_pdxxmx]    Script Date: 2025/12/8 17:02:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dept_pdxxmx](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[pdId] [bigint] NOT NULL,
	[productId] [varchar](50) NOT NULL,
	[ph] [varchar](30) NOT NULL,
	[pc] [varchar](30) NOT NULL,
	[yxq] [datetime] NULL,
	[llsl] [int] NOT NULL,
	[sjsl] [int] NOT NULL,
	[zhyz] [int] NOT NULL,
	[lsj] [decimal](11, 4) NOT NULL,
	[px] [int] NULL,
	[zt] [char](1) NOT NULL,
	[CreatorCode] [varchar](50) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
 CONSTRAINT [PK_KC_PDXXMX] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键 明细ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'Id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'盘点主表ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'pdId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'物资ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'productId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'ph'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批次' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'pc'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有效期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'yxq'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'理论数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'llsl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'实际数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'sjsl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'转换因子' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'zhyz'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'零售价' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'lsj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'排序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'px'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'zt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'CreatorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'LastModifyTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx', @level2type=N'COLUMN',@level2name=N'LastModifierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存_盘点信息明细' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_pdxxmx'
GO


