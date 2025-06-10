namespace ShortLink.Application.Common;

public interface IQueryHandler<in TQuery , TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken = default);
}