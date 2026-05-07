using Newtouch.EMR.Domain.Entity;
using Newtouch.EMR.Domain.IRepository;
using Newtouch.Infrastructure;
using System.Collections.Generic;
using Newtouch.Core.Common;
using System.Data.SqlClient;
using System.Text;
using FrameworkBase.MultiOrg.Infrastructure;
using FrameworkBase.MultiOrg.Repository;
using System.Linq;
using System;
namespace Newtouch.EMR.Repository
{
    /// <summary>
    /// 创 建：hyj
    /// 日 期：2018-10-12 17:43
    /// 描 述：元素表
    /// </summary>
    public class BlysRepo : RepositoryBase<BlysEntity>, IBlysRepo
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="databaseFactory">EF上下文工厂</param>
        public BlysRepo(IDefaultDatabaseFactory databaseFactory)
            : base(databaseFactory)
        {
            
        }
        public List<BlysEntity> GetYsTree(string orgId,string yssjid)
        {
            return this.IQueryable().Where(p => p.zt == "1"&&p.yssjid== yssjid).OrderBy(p=>p.ysmc).ToList();

        }
        public List<BlysEntity> GetYsTreeV2(string orgId,string keyword)
        {
            string sql = @"select * from [dbo].[bl_ys] a 
where  a.zt='1' and (a.OrganizeId=@orgId or a.OrganizeId='*')
 and a.ysmc like @keyword and a.yssjid!='-1'
 union all
 select * from [dbo].[bl_ys] b
where  b.zt='1' and (b.OrganizeId=@orgId or b.OrganizeId='*')
  and b.yssjid='-1'  order by yssjid ,px";
            var para = new List<SqlParameter>();
            para.Add(new SqlParameter("@orgId", orgId));
            para.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
            return this.FindList<BlysEntity>(sql, para.ToArray()).OrderBy(p=>p.px).ToList();
        }
        public List<BlysMXEntity> GetYsMX(string orgId, string YsId)
        {
            var para = new List<SqlParameter>();
            StringBuilder sqlstr = new StringBuilder();
            sqlstr.Append(" select * from bl_ysmx where zt='1'");
            if (!string.IsNullOrWhiteSpace(YsId))
            {
                sqlstr.Append(" and YsId =@YsId");
                para.Add(new SqlParameter("@YsId",YsId));
            }
            sqlstr.Append(" and (OrganizeId=@orgId or OrganizeId='*')");
            para.Add(new SqlParameter("@orgId", orgId));
           
            return this.FindList<BlysMXEntity>(sqlstr.ToString(), para.ToArray());
        }
        /// <summary>
        /// 保存表单（新增、修改）
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="keyValue">主键值</param>
        public void SubmitForm(BlysEntity entity, string keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue))
            {
                var dbEntity = this.FindEntity(keyValue);            

        
                dbEntity.ysmc = entity.ysmc;
                dbEntity.BackgroundText = entity.BackgroundText;
                dbEntity.DataSource = entity.DataSource;
                dbEntity.BindingPath = entity.BindingPath;
                dbEntity.yscode = entity.yscode;
                dbEntity.AutoUpdate = entity.AutoUpdate;
                dbEntity.Readonly = entity.Readonly;
                dbEntity.TypeName = entity.TypeName;
                dbEntity.Description = entity.Description;
                dbEntity.ListSource = entity.ListSource;
                dbEntity.UserEditable = entity.UserEditable;
                dbEntity.EditStyle = entity.EditStyle;
                dbEntity.Value = entity.Value;
                dbEntity.Style = entity.Style;
                dbEntity.Format = entity.Format;

                dbEntity.LastModifierCode = entity.LastModifierCode;
                dbEntity.LastModifyTime = DateTime.Now;
                dbEntity.Modify(keyValue);
                this.Update(dbEntity);
            }
            else
            {
                entity.Create(true);
                this.Insert(entity);
            }
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="keyValue">主键值</param>
        public void DeleteForm(string keyValue)
        {
            //  this.Delete(p => p.Id == keyValue);
            if (!string.IsNullOrEmpty(keyValue))
            {
                var dbEntity = this.FindEntity(keyValue);
                //properties
                dbEntity.zt = "0";
                dbEntity.Modify(keyValue);
                this.Update(dbEntity);
            }
        }

    }
}