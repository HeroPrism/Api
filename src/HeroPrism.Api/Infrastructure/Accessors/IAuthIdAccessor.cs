using System.Threading;
using System.Threading.Tasks;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public interface IAuthIdAccessor
    {
        Task<string> GetAuthId(CancellationToken cancellationToken);
    }
}