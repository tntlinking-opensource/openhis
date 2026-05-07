USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[dept_syyy]    Script Date: 2025/12/8 17:03:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[dept_syyy](
	[Id] [varchar](50) NOT NULL,
	[syyy] [varchar](100) NOT NULL,
	[sybz] [varchar](2) NOT NULL,
	[zt] [varchar](1) NOT NULL,
	[px] [int] NULL,
	[CreatorCode] [varchar](50) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
 CONSTRAINT [PK_KC_SYYY] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[dept_syyy] ADD  DEFAULT ('0') FOR [sybz]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主键' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'Id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'损溢原因' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'syyy'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 报损，1 报溢' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'sybz'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态 0：无效  1：有效' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'zt'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'排序' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'px'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'CreatorCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'CreateTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'LastModifyTime'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后修改用户ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy', @level2type=N'COLUMN',@level2name=N'LastModifierCode'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'库存_损益原因' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'dept_syyy'
GO


