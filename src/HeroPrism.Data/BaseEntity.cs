using System;

namespace HeroPrism.Data
{
    public abstract class BaseEntity
    {
        public string Id { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    }
}