﻿using Microsoft.EntityFrameworkCore;
using SF.Core.Abstraction.Domain;
using SF.Core.Data;
using SF.Core.Entitys;
using SF.Core.QueryExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SF.Module.Backend.Domain.DataItem.Rule
{
    public class DataItemRules : IDataItemRules
    {
        private readonly IBaseUnitOfWork _baseUnitOfWork;

        public DataItemRules(IBaseUnitOfWork baseUnitOfWork)
        {
            _baseUnitOfWork = baseUnitOfWork;
        }

        public bool DoesDataItemExist(long id)
        {
            var app = _baseUnitOfWork.BaseWorkArea.DataItem.GetById(id);
            return app != null && app.DeleteMark != (int)DeleteStatus.Deleted;
        }

        public bool IsDataItemIdUnique(long id)
        {
            return _baseUnitOfWork.BaseWorkArea.DataItem.Exists(id);
          
        }

        public bool IsDataItemCodeUnique(string code, long dataItemId = 0)
        {
            Expression<Func<DataItemEntity, bool>> expression = d => d.ItemCode == code;
            if (dataItemId!=0)
            {
                expression.And(d => d.Id != dataItemId);  
            }
            return _baseUnitOfWork.BaseWorkArea.DataItem.Query().AsNoTracking().Any(expression);
           
        }

        public bool IsDataItemNameUnique(string name, long dataItemId = 0)
        {
            Expression<Func<DataItemEntity, bool>> expression = d => d.ItemName == name;
            if (dataItemId != 0)
            {
                expression.And(d => d.Id != dataItemId);
            }
            return _baseUnitOfWork.BaseWorkArea.DataItem.Query().AsNoTracking().Any(expression);

        }

        private bool IsDataItemUnique(DataItemEntity dataItem, long dataItemId)
        {
            return dataItem == null
                || dataItem.DeleteMark == (int)DeleteStatus.Deleted
                || (dataItemId != 0 && dataItem.Id == dataItemId);
        }
    }
}
