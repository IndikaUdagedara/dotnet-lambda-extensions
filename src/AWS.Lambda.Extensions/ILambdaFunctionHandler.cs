using System.Threading.Tasks;

namespace AWS.Lambda.Extensions
{
    /// <summary>
    /// Async lambda function handler
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface ILambdaFunctionHandler<TRequest>
    {
        Task HandleAsync(TRequest request);
    }

    /// <summary>
    /// Sync lambda function handler
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ILambdaFunctionHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}