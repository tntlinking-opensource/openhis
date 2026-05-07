--科室物资管理菜单脚本
alter table NewtouchHIS_Sett..mz_js alter column xjwc numeric(9,2)
delete from Newtouch_CIS..[Sys_Module] where Id='ed210bea-eb87-4c84-b069-332e7fa283d8'

insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'ed210bea-eb87-4c84-b069-332e7fa283d8',	NULL,	'科室物资管理',	NULL,	NULL,	'fa fa-cubes',	'','expand','0',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'9abf371e-89d7-4ce3-b098-96283be7cfb6',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'过期物资查询',	NULL,	NULL,	NULL,	'/NurseManage/DepartmentSupplies/ExpiredStorageQuery','iframe','1',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'44aa70a5-2fb7-4cd1-bad4-19cb51898855',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'库存查询',	NULL,	NULL,	NULL,	'/NurseManage/DepartmentSupplies/StorageQuery','iframe','2',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'c0ff97bf-cc7d-48c0-aa83-ec5e39d6af22',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'物资申领单',	NULL,	NULL,	NULL,	'/NurseManage/DepartmentSupplies/WzSldIndex','iframe','3',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','0',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'd0c5360b-530e-47c0-9693-f42dc2720801',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'科室申领',	NULL,	NULL,	NULL,	'/StorageManage/Storage/DepartmentApply','iframe','3',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,'Herp'
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'0f841e2e-efec-489c-8be4-cd0bdced77cd',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'报损报溢',	NULL,	NULL,	NULL,	'/NurseManage/ProfitAndLoss/index','iframe','4',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'0f841e2e-efec-489c-8be4-cd0bdced7sd',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'报损报溢查询',	NULL,	NULL,	NULL,	'/NurseManage/ProfitAndLoss/QueryProfitAndLoss','iframe','5',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'4ac621f0-fa4d-4a5b-9043-f8c443af85ed',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'损益原因维护',	NULL,	NULL,	NULL,	'/NurseManage/ProfitAndLoss/Syyy','iframe','4',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'6676bfe0-f360-4f75-99f4-ba0317b52f6d',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'库存盘点',	NULL,	NULL,	NULL,	'/NurseManage/StockInventory/index','iframe','6',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Module] (Id,ParentId,Name,EnName,	Code,Icon,UrlAddress,Target,px,	Description,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt,OrganizeId,AppId)
values(
'460a1102-dbaa-4a5e-a198-d904dc32de71',	'ed210bea-eb87-4c84-b069-332e7fa283d8',	'库存结转',	NULL,	NULL,	NULL,	'/NurseManage/StockCarryOver/index','iframe','7',NULL,'2023-09-08 17:10:52.860','root','2025-09-29 10:53:07.160','admin','1',NULL,NULL
)
insert into Newtouch_CIS..[Sys_Config] (Id,OrganizeId,Code,Name,Value,Memo,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt)
values(
'02c3d1f2-e158-46f5-959c-2e214abc51f2',	'6d5752a7-234a-403e-aa1c-df8b45d3469f',	'openWzhccf',	'开关：门诊住院是否开放医用耗材处方',	'ON',	'ON 开启 ：OFF:关闭',	'2025-11-11 15:55:48.650','bdadmin','2025-12-03 16:45:00.647','bdadmin','1'
)
insert into Newtouch_CIS..[Sys_Config] (Id,OrganizeId,Code,Name,Value,Memo,CreateTime,	CreatorCode,LastModifyTime,LastModifierCode,zt)
values(
'cf265b92-84ee-4ac3-8dcb-8ec5a74c3f96',	'6d5752a7-234a-403e-aa1c-df8b45d3469f',	'openWzhckc',	'开关：门诊住院是否开放医用耗材处方',	'ON',	'ON 开启 ：OFF:关闭',	'2025-11-11 15:55:48.650','bdadmin','2025-12-03 16:45:00.647','bdadmin','1'
)
