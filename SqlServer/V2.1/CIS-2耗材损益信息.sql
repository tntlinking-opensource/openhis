USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[dept_syxx]    Script Date: 2025/12/8 17:03:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dept_syxx](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrganizeId] [varchar](50) NOT NULL,
	[ks] [varchar](50) NOT NULL,
	[productId] [varchar](50) NOT NULL,
	[Ph] [varchar](30) NULL,
	[pc] [varchar](30) NOT NULL,
	[Yxq] [datetime] NULL,
	[Bgsj] [datetime] NOT NULL,
	[Sysl] [int] NOT NULL,
	[UnitId] [varchar](50) NULL,
	[Lsj] [decimal](11, 4) NOT NULL,
	[Zhyz] [int] NOT NULL,
	[Syyy] [varchar](50) NULL,
	[Zrr] [varchar](50) NULL,
	[Djh] [varchar](50) NULL,
	[Sykc] [int] NOT NULL,
	[Jj] [decimal](11, 4) NOT NULL,
	[remark] [varchar](500) NULL,
	[zt] [char](1) NOT NULL,
	[px] [int] NULL,
	[CreatorCode] [varchar](50) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
 CONSTRAINT [PK_KC_SYXX] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'组织机构（医院）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'OrganizeId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'科室代码' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'ks'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'物资ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'productId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Ph'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'批次' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'pc'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有效期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Yxq'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'默认值Getdate()' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Bgsj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'正数：报溢，负数：报损  最小单位数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Sysl'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'单位ID  与价格配合使用，和zhyz成套' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'UnitId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'零售价' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Lsj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'转换因子' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Zhyz'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'损益原因' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Syyy'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'责任人工号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Zrr'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'单据号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Djh'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'剩余库存' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Sykc'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'进价' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'Jj'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'备注' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'remark'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'zt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'排序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'px'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'CreatorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'LastModifyTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx', @level2type=N'COLUMN',@level2name=N'LastModifierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存_损益信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syxx'
GO


