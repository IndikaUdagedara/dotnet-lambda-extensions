using System.Threading.Tasks;

namespace LambdaExtensions
{
    public interface ILambdaFunctionHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}