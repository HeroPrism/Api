namespace HeroPrism.Api.Infrastructure
{
    public class HeroPrismSession
    {
        public string CorrelationId { get; }
        public string UserId { get; }

        public HeroPrismSession(string correlationId, string userId)
        {
            CorrelationId = correlationId;
            UserId = userId;
        }
    }
}