using HeroPrism.Data;

namespace HeroPrism.Api.Infrastructure
{
    public class HeroPrismSession
    {
        public string CorrelationId { get; }
        public string UserId { get; }
        
        public User User { get; set; }

        public HeroPrismSession(string correlationId, string userId)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}