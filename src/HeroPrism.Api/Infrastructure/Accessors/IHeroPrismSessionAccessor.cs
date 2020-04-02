using System.Threading;
using System.Threading.Tasks;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public interface IHeroPrismSessionAccessor
    {
        Task<HeroPrismSession> Get(CancellationToken cancellationToken);
    }
}