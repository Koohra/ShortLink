using ShortLink.Domain.ValueObject;

namespace ShortLink.Domain.Interfaces;

public interface IShortCodeGenerator
{
    Task<ShortCode> GenerateShortCodeAsync(int length = 6);
}