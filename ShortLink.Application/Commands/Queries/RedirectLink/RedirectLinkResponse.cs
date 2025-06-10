namespace ShortLink.Application.Commands.Queries.RedirectLink;

public sealed class RedirectLinkResponse
{
    public bool Success { get; private set; }
    public string OriginalUrl { get; private set; }
    public string ErrorMessage { get; private set; }
    
    private RedirectLinkResponse() { }
    
    public static RedirectLinkResponse Successful(string originalUrl) =>
        new RedirectLinkResponse { Success = true, OriginalUrl = originalUrl };
        
    public static RedirectLinkResponse Failed(string errorMessage) =>
        new RedirectLinkResponse { Success = false, ErrorMessage = errorMessage };
}