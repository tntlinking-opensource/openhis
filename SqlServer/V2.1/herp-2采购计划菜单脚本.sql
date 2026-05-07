--herp采购计划及相关菜单脚本
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'c4c1d632-3ac7-48ef-a9f4-37104704143d',	NULL,	'物资采购管理',	NULL,	NULL,	'fa fa-cubes',	'','expand','0','此采购流系统走完后程涉及到第三方对接 采购订单处理流程 -1：拒处理； 0：待处理； 1：备货； 2：配送； 3：签收； 4：完成； 5：拒签','2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'173719ff-a2ad-44b8-aa87-a3a9ecf8da6f',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'采购单审核',	NULL,	NULL,	NULL,	'/BillManage/PurchasingOrder/AuditPurchaseOrder','iframe','6',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'b4ee9067-bac4-4213-8e5f-f498101d68c8',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'生成采购单',	NULL,	NULL,	NULL,	'/BillManage/PurchasingOrder/GeneratingPurchaseOrder','iframe','5',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'fea1a2b7-fe34-450b-8672-0dcdcdfa3f2f',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'采购单查询',	NULL,	NULL,	NULL,	'/BillManage/PurchasingOrder/PurchaseOrderQuery','iframe','7',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'ea2aef90-5502-4ff0-96f9-bed5dfe1ba7e',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'审核采购计划',	NULL,	NULL,	NULL,	'/BillManage/PurchasingPlan/AuditPurchasingPlan','iframe','4',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)

insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'2314f309-85b9-4b65-9151-a167bf1c387f',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'填写采购计划',	NULL,	NULL,	NULL,	'/BillManage/PurchasingPlan/FillPurchasingPlan','iframe','2',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)

insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'80caf0b0-1fd4-49e7-b755-e33f7ad1c546',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'我的采购计划',	NULL,	NULL,	NULL,	'/BillManage/PurchasingPlan/PurchasingPlanQuery','iframe','1',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)

insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'56da2c77-88ce-4023-aedc-522785def9ba',	'c4c1d632-3ac7-48ef-a9f4-37104704143d',	'采购计划(库存预警)',	NULL,	NULL,	NULL,	'/BillManage/PurchasingPlan/PurchasingPlan','iframe','3',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)

insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'e7e4f899-55ae-431e-9d88-eafef3555926',	'73726891-adf2-4647-9d4a-7e1783819d79',	'科室申领',	NULL,	NULL,	NULL,	'/StorageManage/Storage/DepartmentApply','iframe','8',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'a1537aa3-25e5-4efa-9aaf-259802706298',	'73726891-adf2-4647-9d4a-7e1783819d79',	'申领出库',	NULL,	NULL,	NULL,	'/StorageManage/Storage/ApplyOutStock','iframe','9',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)

insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'ba678a3c-e253-41ed-b46f-507ba95245f4',	'f3dc1ea3-2028-4ed0-bdfa-0c293bd6fdda',	'申领单查询',	NULL,	NULL,	NULL,	'/BillManage/ApplyBill/Query','iframe','3',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into NewtouchHIS_herp..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'eb90c70a-ae49-45d4-ab26-151a8753fccf',	'c65b1351-b2df-465d-be60-42aff5b86f24',	'物资收费项目对照',	NULL,	NULL,	NULL,	'/ProductManage/ProductSfxm/Index','iframe','5',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)

