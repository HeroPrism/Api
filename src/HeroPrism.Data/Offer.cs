using System;
using Cosmonaut;
using Cosmonaut.Attributes;

namespace HeroPrism.Data
{
    [SharedCosmosCollection("shared")]
    public class Offer : BaseEntity, ISharedCosmosEntity
    {
        public string CosmosEntityName { get; set; }
        
        public string TaskId { get; set; }
        
        public string RequesterId { get; set; }
        public bool RequesterCompleted { get; set; }
        
        public string HelperId { get; set; }
        public bool HelperCompleted { get; set; }
        
    }
}