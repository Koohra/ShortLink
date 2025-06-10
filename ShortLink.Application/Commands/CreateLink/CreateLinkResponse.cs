using ShortLink.Application.DTOs;

namespace ShortLink.Application.Commands.CreateLink;

public sealed class CreateLinkResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public LinkDto Link { get; set; }
}