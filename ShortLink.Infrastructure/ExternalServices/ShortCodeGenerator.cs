using ShortLink.Domain.Interfaces;
using ShortLink.Domain.ValueObject;

namespace ShortLink.Infrastructure.ExternalServices;

public sealed class ShortCodeGenerator : IShortCodeGenerator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random;
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

    public ShortCodeGenerator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _random = new Random();
    }

    public async Task<ShortCode> GenerateShortCodeAsync(int length = 6)
    {
        const int maxAttempts = 5;
        var attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;
            var code = GenerateRandomCode(length);
            var shortCode = ShortCode.Create(code);

            var exists = await _unitOfWork.LinkRepository.ShortCodeExistsAsync(shortCode);

            if (!exists)
                return shortCode;
        }

        return await GenerateShortCodeAsync(length + 1);
    }

    private string GenerateRandomCode(int length)
    {
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}