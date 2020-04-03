using HeroPrism.Data;

namespace HeroPrism.Api.Infrastructure
{
    public class HeroPrismSession
    {
        public string CorrelationId { get; }
        
        public string AuthId { get; }
        
        public string UserId { get; set; }
        
        public User User { get; set; }

        public HeroPrismSession(string correlationId, string authId)
        {
            CorrelationId = correlationId;
            AuthId = authId;
        }
    }
}