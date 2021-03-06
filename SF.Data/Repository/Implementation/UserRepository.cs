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
using SF.Entitys.Abstraction.Pages;
using SF.Core.EFCore.UoW;
using SF.Entitys;
using System;
using System.Linq;
using System.Threading;

namespace SF.Data.Repository
{
    public class UserRepository : EFCoreQueryableRepository<UserEntity, long>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context)
        {
        }
        public override IQueryable<UserEntity> QueryById(long id)
        {
            return Query().Where(e => e.Id == id);
        }

        public IPagedList<UserEntity> QueryFilterByLevelWithPagination(Guid userGuid, int page = 0, int pageSize = 50, CancellationToken ct = new CancellationToken())
        {
            Func<IQueryable<UserEntity>, IOrderedQueryable<UserEntity>> dataItemFunc = source => source.OrderBy(x => x.CreatedOn);
            return QueryPage(e => e.UserGuid == userGuid, dataItemFunc, null, page, pageSize);
        }
    }

}
