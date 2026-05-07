USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[Dept_kcxx]    Script Date: 2025/11/4 11:27:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Dept_kcxx](
	[Id] [varchar](50) NOT NULL,
	[OrganizeId] [varchar](50) NOT NULL,
	[ks] [varchar](50) NOT NULL,
	[productId] [varchar](50) NOT NULL,
	[ph] [varchar](50) NOT NULL,
	[pc] [varchar](50) NULL,
	[yxq] [datetime] NULL,
	[kcsl] [int] NOT NULL,
	[djsl] [int] NOT NULL,
	[crkmxId] [bigint] NULL,
	[jj] [numeric](11, 4) NULL,
	[zhyz] [int] NULL,
	[locked] [int] NULL,
	[zt] [varchar](1) NOT NULL,
	[CreatorCode] [varchar](50) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
	[productCode] [varchar](30) NULL,
 CONSTRAINT [PK_DEPT_KCXX] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'Id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'组织机构ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'OrganizeId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'科室代码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'ks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'物资ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'productId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'ph'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批次' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'pc'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有效期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'yxq'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存数量，最小单位数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'kcsl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'冻结数量，最小单位数量' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'djsl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'出入库明细ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'crkmxId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'进价，转化因子对应单位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'jj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'转化因子，转化成当前库房单位' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'zhyz'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存锁 0/null：未上锁      >0：已锁' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'locked'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0:作废；1.有效' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'zt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'CreatorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'LastModifyTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx', @level2type=N'COLUMN',@level2name=N'LastModifierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库房_库存' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dept_kcxx'
GO


