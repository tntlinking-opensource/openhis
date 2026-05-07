USE [Newtouch_CIS]
GO

/****** Object:  Table [dbo].[xt_blmbzt]    Script Date: 2026/3/24 19:32:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[xt_blmbzt](
	[mbztId] [varchar](50) NOT NULL,
	[OrganizeId] [varchar](50) NOT NULL,
	[mbId] [varchar](50) NOT NULL,
	[mbmc] [varchar](50) NOT NULL,
	[ztId] [varchar](50) NOT NULL,
	[ztmc] [varchar](50) NOT NULL,
	[CreateTime] [datetime] NOT NULL,
	[CreatorCode] [varchar](50) NOT NULL,
	[LastModifyTime] [datetime] NULL,
	[LastModifierCode] [varchar](50) NULL,
	[zt] [char](1) NOT NULL,
	[cflx] [int] NULL,
	[zxks] [varchar](15) NULL,
PRIMARY KEY CLUSTERED 
(
	[mbztId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[xt_blmbzt] ADD  DEFAULT (NULL) FOR [cflx]
GO


