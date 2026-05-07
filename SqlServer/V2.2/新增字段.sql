alter table [Newtouch_EMR]..[zy_meddocs_relation] add  doctype varchar(6)


insert into [Newtouch_EMR]..Sys_Config(Id	,OrganizeId	,Code,	Name,	Value,	Memo	,CreateTime,	CreatorCode	,LastModifyTime	,LastModifierCode,	zt,	px)
values('d6fcac5b-6366-46f9-b14f-9cc837fe9c39','6d5752a7-234a-403e-aa1c-df8b45d3469f',	'OpenEditorSwitch'	,'新版编辑器开关',	'ON',	NULL,	'2026-04-03 14:52:21.333',	'bdadmin',	NULL,	NULL,'1',	NULL)

insert into Newtouch_EMR..Sys_Module(Id,OrganizeId,ParentId,Name,EnName,Code,Icon,UrlAddress,Target,px	,Description,CreateTime,CreatorCode,LastModifyTime,LastModifierCode,zt,AppId)
values('90179dff-60d0-46e7-939c-598811570904',	'6d5752a7-234a-403e-aa1c-df8b45d3469f',	'f0224c99-9c5a-481e-ad66-de9381e3694d',	'病历模板制作(新)',	NULL,	NULL,	NULL,	'/MedicalRecordManage/MedRecordTemplate/MedicalOpenEditor',	'iframe',	NULL,	NULL,	'2023-12-22 16:23:03.997',	'000000',	'2026-04-03 14:40:55.437',	'bdadmin',	'1',	NULL)

