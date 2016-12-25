﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SF.Core.Abstraction.Events;
using SF.Core.Common;
using SF.Core.Data;
using SF.Core.Entitys;
using SF.Core.Extensions;
using SF.Core.QueryExtensions.Extensions;
using SF.Module.Backend.Services;
using SF.Module.Backend.ViewModels;
using SF.Web.Common.Base.Args;
using SF.Web.Common.Base.Controllers;
using SF.Web.Common.Base.DataContractMapper;
using SF.Web.Control.JqGrid.Core.Json;
using SF.Web.Control.JqGrid.Core.Request;
using SF.Web.Control.JqGrid.Core.Response;
using SF.Web.Common.Models.GridTree;
using SF.Web.Common.Models.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SF.Module.Backend.Domain.DataItem.ViewModel;
using SF.Module.Backend.Domain.DataItem.Service;

namespace SF.Module.Backend.Controllers
{
    /// <summary>
    /// 字典管理API
    /// </summary>
    [Authorize]
    [Route("Api/DataItem/")]
    public class DataItemApiController : CrudControllerBase<DataItemEntity, DataItemViewModel>
    {
        private readonly IMediator _mediator;
        private readonly IDataItemService _dataItemService;
        private readonly IBaseUnitOfWork _baseUnitOfWork;
        public DataItemApiController(IServiceCollection collection, ILogger<DataItemApiController> logger,
             IBaseUnitOfWork baseUnitOfWork,
             IMediator mediator,
             IDataItemService dataItemService)
            : base(baseUnitOfWork, collection, logger)
        {
            this._baseUnitOfWork = baseUnitOfWork;
            this._mediator = mediator;
            this._dataItemService = dataItemService;

        }
        #region 事件
        /// <summary>
        /// 新增后
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnAfterAdd(CrudEventArgs<DataItemEntity, DataItemViewModel> arg)
        {
            this._mediator.Publish(new EntityInserted<DataItemEntity>(arg.Entity));
        }
        /// <summary>
        /// 编辑后
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnAfterEdit(CrudEventArgs<DataItemEntity, DataItemViewModel> arg)
        {
            this._mediator.Publish(new EntityUpdated<DataItemEntity>(arg.Entity));
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 字典树数据源
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rootDataItemId"></param>
        /// <param name="countsType"></param>
        /// <returns></returns>
        [Route("GetChildren/{id}")]
        public IQueryable<TreeViewItem> GetChildren(
        int id,
        int rootDataItemId = 0,
        TreeViewItem.GetCountsType countsType = TreeViewItem.GetCountsType.None)
        {
            var qry = _dataItemService.GetChildren(id, rootDataItemId);

            List<DataItemViewModel> dataItemEntityList = new List<DataItemViewModel>();
            List<TreeViewItem> groupNameList = new List<TreeViewItem>();
            var groups = qry.OrderBy(g => g.ItemName);
            foreach (var group in groups)
            {

                dataItemEntityList.Add(group);
                var treeViewItem = new TreeViewItem();
                treeViewItem.Id = group.Id.ToString();
                treeViewItem.Name = $"{group.ItemName}({group.ItemCode})";
                treeViewItem.IsActive = (group.EnabledMark ?? 0) > 0;

                treeViewItem.IconCssClass = "fa fa-sitemap";

                if (countsType == TreeViewItem.GetCountsType.ChildGroups)
                {
                    treeViewItem.CountInfo = this._baseUnitOfWork.BaseWorkArea.DataItem.Query().Where(a => a.ParentId.HasValue && a.ParentId == group.Id).Count();
                }

                groupNameList.Add(treeViewItem);

            }

            //快速找出哪些项目有子级
            List<long> resultIds = dataItemEntityList.Select(a => a.Id).ToList();
            var qryHasChildrenList = this._baseUnitOfWork.BaseWorkArea.DataItem.Query()
                .Where(g =>
                   g.ParentId.HasValue &&
                   resultIds.Contains(g.ParentId.Value)).Select(g => g.ParentId.Value)
                .Distinct()
                .ToList();

            foreach (var g in groupNameList)
            {
                int groupId = g.Id.AsInteger();
                g.HasChildren = qryHasChildrenList.Any(a => a == groupId);
            }

            return groupNameList.AsQueryable();
        }
        /// <summary>
        /// 分类树列表
        /// </summary>
        /// <param name="keyword">关键字查询</param>
        /// <returns>返回树形列表Json</returns>
        [HttpGet]
        [Route("GetTreeList")]
        public ActionResult GetTreeListJson(string keyword)
        {
            var data = _repository.Query().ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                data = data.TreeWhere(t => t.ItemName.Contains(keyword));
            }
            JqGridResponse response = new JqGridResponse();
            var dtos = CrudDtoMapper.MapEntityToDtos(data);
            var TreeList = new List<TreeGridEntity>();
            foreach (DataItemViewModel item in dtos)
            {
                TreeGridEntity tree = new TreeGridEntity();
                bool hasChildren = data.Count(t => t.ParentId == item.Id && t.ParentId != t.Id) == 0 ? false : true;
                tree.id = item.Id.ToString();
                tree.parentId = item.ParentId.HasValue ? item.ParentId.Value.ToString() : "";
                tree.expanded = true;
                tree.hasChildren = hasChildren;
                tree.entityJson = item.ToJson();
                TreeList.Add(tree);
            }
            return Content(TreeList.TreeJson());
        }

        /// <summary>
        /// 分类普通列表
        /// </summary>
        /// <param name="pagination">分页参数</param>
        /// <param name="queryJson">查询参数</param>
        /// <returns>返回分页列表Json</returns>
        [HttpGet]
        [Route("GetPageList")]
        public ActionResult GetPageListJson(JqGridRequest request, string keyword)
        {
            Expression<Func<DataItemEntity, bool>> pc = d => d.Id > 0;
            if (!string.IsNullOrEmpty(keyword))
            {
                pc = d => d.ItemName.Contains(keyword);
            }
            var query = _repository.QueryPage(pc, page: request.PageIndex, pageSize: request.RecordsCount);
            var dtos = CrudDtoMapper.MapEntityToDtos(query);
            JqGridResponse response = new JqGridResponse()
            {
                TotalPagesCount = query.TotalPages,
                PageIndex = request.PageIndex,
                TotalRecordsCount = query.TotalCount,
            };
            foreach (DataItemViewModel userInput in dtos)
            {
                response.Records.Add(new JqGridRecord(Convert.ToString(userInput.Id), userInput));
            }

            response.Reader.RepeatItems = false;
            return new JqGridJsonResult(response);
        }
        #endregion

        #region 验证数据
        /// <summary>
        /// 分类编号不能重复
        /// </summary>
        /// <param name="ItemCode">编号</param>
        /// <param name="keyValue">主键</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExistItemCode")]
        public ActionResult ExistItemCode(string itemCode, string keyValue)
        {
            var query = _repository.Query();
            Expression<Func<DataItemEntity, bool>> pi = d => d.ItemCode == itemCode;
            if (!string.IsNullOrEmpty(keyValue))
            {
                Expression<Func<DataItemEntity, bool>> pk = d => d.Id != keyValue.AsInteger(0);
                pi.And(pk);
            }
            bool IsOk = query.Where(pi).Count() == 0 ? true : false;
            return Content(IsOk.ToString());
        }
        /// <summary>
        /// 分类名称不能重复
        /// </summary>
        /// <param name="ItemName">名称</param>
        /// <param name="keyValue">主键</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExistItemName")]
        public ActionResult ExistItemName(string itemName, string keyValue)
        {

            var query = _repository.Query();
            Expression<Func<DataItemEntity, bool>> pi = d => d.ItemName == itemName;
            if (!string.IsNullOrEmpty(keyValue))
            {
                Expression<Func<DataItemEntity, bool>> pk = d => d.Id != keyValue.AsInteger(0);
                pi.And(pk);
            }
            bool IsOk = query.Where(pi).Count() == 0 ? true : false;
            return Content(IsOk.ToString());
        }
        #endregion

    }


}
