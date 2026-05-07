USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[dept_kcjz]    Script Date: 2025/12/8 17:00:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dept_kcjz](
	[Id] [varchar](50) NOT NULL,
	[OrganizeId] [varchar](50) NULL,
	[warehouseId] [varchar](50) NOT NULL,
	[productId] [varchar](50) NOT NULL,
	[ph] [varchar](30) NULL,
	[pc] [varchar](30) NOT NULL,
	[yxq] [datetime] NOT NULL,
	[kcsl] [int] NULL,
	[bmlsj] [decimal](11, 4) NOT NULL,
	[jj] [decimal](11, 4) NOT NULL,
	[zhyz] [int] NOT NULL,
	[jzsj] [datetime] NOT NULL,
	[zt] [varchar](1) NOT NULL,
	[px] [int] NULL,
	[CreatorCode] [varchar](50) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
 CONSTRAINT [PK_KC_KCJZ] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'Id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'组织机构' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'OrganizeId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库房代码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'warehouseId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'物资ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'productId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'ph'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批次' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'pc'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有效时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'yxq'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存数量，最小单位数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'kcsl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部门零售价，转化英子对应单位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'bmlsj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'进价，转化英子对应单位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'jj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'转化因子' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'zhyz'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'结转时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'jzsj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'zt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'排序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'px'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'CreatorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'LastModifyTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz', @level2type=N'COLUMN',@level2name=N'LastModifierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存_库存结转' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_kcjz'
GO


