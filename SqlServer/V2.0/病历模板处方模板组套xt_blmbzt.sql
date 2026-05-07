USE [Newtouch_CIS]
GO
/*
 Navicat Premium Dump SQL

 Source Server         : 合理用药院版
 Source Server Type    : SQL Server
 Source Server Version : 15002000 (15.00.2000)
 Source Host           : 61.172.179.73:41125
 Source Catalog        : Newtouch_CIS
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 15002000 (15.00.2000)
 File Encoding         : 65001

 Date: 15/08/2025 19:01:43
*/


-- ----------------------------
-- Table structure for xt_blmbzt
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[xt_blmbzt]') AND type IN ('U'))
	DROP TABLE [dbo].[xt_blmbzt]
GO

CREATE TABLE [dbo].[xt_blmbzt] (
  [mbztId] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [OrganizeId] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [mbId] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [mbmc] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [ztId] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [ztmc] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [CreateTime] datetime  NOT NULL,
  [CreatorCode] varchar(50) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [LastModifyTime] datetime  NULL,
  [LastModifierCode] varchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [zt] char(1) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [cflx] int DEFAULT NULL NULL
)
GO

ALTER TABLE [dbo].[xt_blmbzt] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Indexes structure for table xt_blmbzt
-- ----------------------------
CREATE NONCLUSTERED INDEX [ix_xt_blmbzt_ztid]
ON [dbo].[xt_blmbzt] (
  [ztId] ASC
)
GO


-- ----------------------------
-- Primary Key structure for table xt_blmbzt
-- ----------------------------
ALTER TABLE [dbo].[xt_blmbzt] ADD CONSTRAINT [PK__xt_blmbz__73C82758738F56B7] PRIMARY KEY CLUSTERED ([mbztId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

