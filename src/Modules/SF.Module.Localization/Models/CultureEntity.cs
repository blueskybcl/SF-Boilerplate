﻿using System.Collections.Generic;
using SF.Core.Entitys.Abstraction;

namespace SF.Module.Localization.Models
{
    public class CultureEntity : BaseEntity
    {
        public string Name { get; set; }

        public IList<ResourceEntity> Resources { get; set; }
    }
}
