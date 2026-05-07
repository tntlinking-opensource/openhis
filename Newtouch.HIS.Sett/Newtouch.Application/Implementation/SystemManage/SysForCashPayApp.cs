using FrameworkBase.MultiOrg.Application;
using Newtouch.HIS.Domain.Entity;
using Newtouch.HIS.Domain.IRepository;
using Newtouch.Tools;
using Newtouch.Tools.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Newtouch.HIS.Application
{
    /// <summary>
    /// 
    /// </summary>
    public class SysForCashPayApp : AppBase, ISysForCashPayApp
    {

        private readonly ISysCashPaymentModelRepo _sysForCashPayRepository;

        /// <summary>
        /// 收费大类所有数据
        /// </summary>
        /// <returns></returns>
        public List<SysCashPaymentModelEntity> GetList(string keyword=null)
        {
            if(string.IsNullOrWhiteSpace(keyword))
                return _sysForCashPayRepository.IQueryable().OrderBy(p => p.xjzffsbh).ToList();
            else
                return _sysForCashPayRepository.IQueryable(p=>p.xjzffsmc.Contains(keyword)).OrderBy(p => p.xjzffsbh).ToList();

        }

        /// <summary>
        /// A页面带一个对象到B页面
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public SysCashPaymentModelEntity GetForm(int keyValue)
        {
            return _sysForCashPayRepository.FindEntity(p=>p.xjzffsbh== keyValue);
        }
        public void DeleteForm(int keyValue)
        {
            _sysForCashPayRepository.Delete(t => t.xjzffsbh == keyValue);
        }
        public void SubmitForm(SysCashPaymentModelEntity xt_xjzffsEntity, int? keyValue)
        {
            if (!string.IsNullOrEmpty(keyValue.ToString()))
            {
                //xt_xjzffsEntity.Id = new Guid(keyValue);
                xt_xjzffsEntity.Modify(keyValue);
                _sysForCashPayRepository.Update(xt_xjzffsEntity);
            }
            else
            {
                //xt_xjzffsEntity.Id = System.Guid.NewGuid();
                xt_xjzffsEntity.Create();
                _sysForCashPayRepository.Insert(xt_xjzffsEntity);
            }
        }
        
        /// <summary>
        /// Grid筛选查询显示
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public DataTable GetListBySearch(string keyword)
        {
            string sql = @"select *,(select a.dlmc from xt_xjzffs a where b.mzprintreportcode=a.dl) dlmc2,(select c.dlmc from xt_xjzffs c where c.dl=b.mzprintbillcode) dlmc3 from xt_xjzffs b";
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += "  where b.dlmc like '%" + keyword + "%'";
            }
            return DbHelper.ExecuteSqlCommand2(sql);
        }
         
        /// <summary>
        /// 获取系统现金支付方式
        /// </summary>
        /// <returns></returns>
        public object GetCashPay()
        { 
            var list = _sysForCashPayRepository.IQueryable()
                .Where (p => p.zt =="1")
                .Select(p => new
                {
                    xjzffs = p.xjzffs,
                    xjzffsbh = p.xjzffsbh,
                    xjzffsmc = p.xjzffsmc,
                    zhxz = p.zhxz,
                    zt = p.zt
                }).ToList(); 
            return list;
        }
    }
}
