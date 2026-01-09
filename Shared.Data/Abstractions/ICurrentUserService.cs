namespace Shared.Data.Abstractions
{
    public interface ICurrentUserService
    {
        string? EntraId { get; }
        string? UserName { get; }
    }
}