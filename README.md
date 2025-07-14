# 平台介绍

## 🏠【关于我们】

![天天开源](https://open.tntlinking.com/assets/logo-b-BzFUYaRU.png) 

天天开源致⼒于打造中国应⽤管理软件开源⽣态，⾯向医疗、企业、教育三⼤⾏业信息化需求，提供优质的开源软件产品与解决⽅案。平台现已发布OpenHIS、OpenCOM、OpenEDU系列开源产品，并持续招募⽣态合作伙伴，期待共同构建开源创新的⾏业协作模式，加速⾏业的数字化进程。

天天开源的前⾝是新致开源，最早于2022年6⽉发布开源医疗软件平台OpenHIS.org.cn，于2023年6⽉发布开源企业软件平台OpenCOM.com.cn。2025年7⽉，新致开源品牌更新为天天开源，我们始终秉持开源、专业、协作的理念，致⼒于为医疗、教育、中⼩企业等⾏业提供优质的开源解决⽅案。

了解我们：https://open.tntlinking.com/about?site=gitee

## 💾【部署包下载】

请访问官网产品中心下载部署包：https://open.tntlinking.com/resource/productCenter?site=gitee

## 📚【支持文档】

技术支持资源：https://open.tntlinking.com/resource/technicalSupport?site=gitee
（含演示环境、操作手册、部署手册、开发手册、常见问题等）

产品介绍：https://open.tntlinking.com/resource/industryKnowledge?site=gitee

操作教程：https://open.tntlinking.com/resource/operationTutorial?site=gitee

沙龙回顾：https://open.tntlinking.com/resource/openSourceSalon#23?site=gitee

## 🤝【合作方式】

产品服务价格：https://open.tntlinking.com/cost?site=gitee

加入生态伙伴：https://open.tntlinking.com/ecology/becomePartner?site=gitee

## 🤗【技术社区】

请访问官网扫码加入技术社区交流：https://open.tntlinking.com/ecology/joinCommunity?site=gitee

请关注公众号【天天开源软件】以便获得最新产品更新信息。

# 项目介绍

OpenHIS医院系统（通用版）是一款适用于公立二级以下医院、乡镇卫生院、社区卫生服务中心的综合性医院信息管理系统，包含体检、后台管理、收费结算、医护协同、药房、电子病历等10大功能模块，支持门诊、住院、医技、后勤各项核心业务。

源码支持本地化/私有化部署，支持第三方电子病历，支持二次开发及医保/卫健对接。如有项目需求，可联系官方平台合作。

如需要其他产品源码，可访问天天开源官网。


## 项目目录

```
## 目录结构介绍
his-dll-common                      公共ddl包
JSViewer_MVC_Core                   报表模块
Newtouch.HIS.Base                   后台管理模块
Newtouch.HIS.CIS                    医护协同模块
Newtouch.HIS.EMR                    电子病例模块
Newtouch.HIS.HangFire               定时任务模块
Newtouch.HIS.Herp                   医疗资源管理模块
Newtouch.HIS.MRMS                   病案管理模块
Newtouch.HIS.MRQC                   病例质控模块
Newtouch.HIS.OR                     手术管理模块
Newtouch.HIS.PDS                    药房药库管理模块
Newtouch.HIS.Sett                   结算管理模块
Newtouch.HIS.SSO                    联合工作站以及基础API
Newtouch.HIS.Static                 静态资源服务
Newtouch.HIS.WebAPI.Manage          API管理
WinServiceAPI                       医保插件
```

## 安装

使用Visual Studio 2017 或 Visual Studio 2022 打开对应模块的 .sln文件

### 前提条件

- [Visual Studio](https://visualstudio.microsoft.com/)

### 克隆仓库

```bash
git clone https://github.com/newtouch-cloud/openhis.git
cd openhis
```

