using System;
using Newtonsoft.Json;

namespace HeroPrism.Data
{
    public abstract class BaseEntity
    {
        
        [JsonProperty("id")]
        public string Id { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    }
}