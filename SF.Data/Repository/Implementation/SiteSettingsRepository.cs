﻿/*******************************************************************************
* 命名空间: SF.Data.Repository
*
* 功 能： N/A
* 类 名： IRoleRepository
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2016/11/11 15:18:03 疯狂蚂蚁 初版
*
* Copyright (c) 2016 SF 版权所有
* Description: SF快速开发平台
* Website：http://www.mayisite.com
*********************************************************************************/
using Microsoft.EntityFrameworkCore;
using SF.Core.EFCore.UoW;
using SF.Entitys;
using System.Linq;

namespace SF.Data.Repository
{
    public class SiteSettingsRepository : EFCoreQueryableRepository<SiteSettings, long>, ISiteSettingsRepository
    {
        public SiteSettingsRepository(DbContext context) : base(context)
        {
        }
        public override IQueryable<SiteSettings> QueryById(long id)
        {
            return Query().Where(e => e.Id == id);
        }


    }

}
