using System.Threading.Tasks;

namespace LambdaExtensions
{
    public interface IProxyHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}