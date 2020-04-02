using System.Threading;
using System.Threading.Tasks;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public interface IUserIdAccessor
    {
        Task<string> GetUserId(CancellationToken cancellationToken);
    }
}