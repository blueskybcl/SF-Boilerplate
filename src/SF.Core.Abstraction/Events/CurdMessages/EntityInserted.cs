﻿
using MediatR;
using SF.Core.Entitys.Abstraction;

namespace SF.Core.Abstraction.Events
{
    /// <summary>
    /// A container for entities that have been inserted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityInserted<T> : INotification where T : BaseEntity
    {

        public EntityInserted(T entity)
        {
            this.Entity = entity;
        }


        public T Entity { get; private set; }
    }
}
