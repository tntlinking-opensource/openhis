--采购计划菜单脚本

delete from NewtouchHIS_PDS..[Sys_Module] where Id='0a819308-78a3-4dba-9cf2-4e39f611bff3'
insert into NewtouchHIS_PDS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'0a819308-78a3-4dba-9cf2-4e39f611bff3',	'4155ba38-4e47-43a7-9791-6cd79dfbfc6b',	'采购计划',	NULL,	NULL,	NULL,	'/OutOrInStoredManage/Purchase/PurchasePlan','iframe','1',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_PDS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values('116f18e7-d310-4d26-b39a-49e99ed1edb0',	'4155ba38-4e47-43a7-9791-6cd79dfbfc6b',	'单据审核',	NULL,	NULL,	NULL,	'/OutOrInStoredManage/Purchase/PurchaseApproval',	'iframe',	'2',	NULL,	'2025-09-30 14:13:47.373',	'admin',	NULL,	NULL,	'1',	NULL,	NULL)

