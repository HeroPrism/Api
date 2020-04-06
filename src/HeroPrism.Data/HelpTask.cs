using Cosmonaut;
using Cosmonaut.Attributes;
using Microsoft.Azure.Documents.Spatial;

namespace HeroPrism.Data
{
    [SharedCosmosCollection("shared")]
    public class HelpTask : BaseEntity, ISharedCosmosEntity
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string ZipCode { get; set; }
        
        public Point ZipLocation { get; set; }
        
        public string UserId { get; set; }
        
        public TaskStatuses Status { get; set; }
        
        public TaskCategory Category { get; set; }
        
        public string CosmosEntityName { get; set; }
        
        public string HelperId { get; set; }

        public bool IsOpen()
        {
            return Status == TaskStatuses.Active || Status == TaskStatuses.New;
        }
    }
    
    public enum TaskStatuses
    {
        New,
        Active,
        Completed,
        Deleted
    }
    
    public enum TaskCategory
    {
        Item,
        Labor,
        Friendship,
        Help
    }
}