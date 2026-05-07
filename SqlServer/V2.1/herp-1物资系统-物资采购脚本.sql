alter table NewtouchHIS_herp..[cg_purchaseOrderDetail] add supplierId varchar(50)--供应商id
alter table NewtouchHIS_herp..[cg_purchaseOrderDetail] add supplierName varchar(50) --供应商名称
alter table NewtouchHIS_herp..[wz_product] add kcyjz int  --库存预警值
alter table NewtouchHIS_herp..[cg_orderDetail] add  fph varchar(30) --发票号

alter table NewtouchHIS_herp..[kf_crkdj] add SyncStatus char(1)  --出库科室物资同步状态
--删掉多余菜单
delete from NewtouchHIS_herp..[Sys_Module] where Id in(
select Id from NewtouchHIS_herp..[Sys_Module] where Id='15d30a51-38dd-48d8-ae96-26bf3a22ba38' or ParentId='15d30a51-38dd-48d8-ae96-26bf3a22ba38')
update NewtouchHIS_herp..[Sys_Module] set zt=0 where Id='a02d3517-1f09-4725-99dc-53fb78f3989f'
