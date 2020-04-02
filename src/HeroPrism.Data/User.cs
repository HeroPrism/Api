using Cosmonaut;
using Cosmonaut.Attributes;

namespace HeroPrism.Data
{
    [SharedCosmosCollection("shared")]
    public class User : BaseEntity, ISharedCosmosEntity
    {
        public string CosmosEntityName { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public int Score { get; set; }
        
        public UserTypes UserType { get; set; }
        
        public int PictureId { get; set; }
    }

    public enum UserTypes
    {
        Individual,
        Organization
    }
}