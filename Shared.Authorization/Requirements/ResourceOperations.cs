namespace Shared.Authorization.Requirements
{
    public static class ResourceOperations
    {
        public static readonly OperationAuthorizationRequirement Create = new() { Name = nameof(Create) };
        public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
        public static readonly OperationAuthorizationRequirement Update = new() { Name = nameof(Update) };
        public static readonly OperationAuthorizationRequirement Delete = new() { Name = nameof(Delete) };
        public static readonly OperationAuthorizationRequirement Approve = new() { Name = nameof(Approve) };
        public static readonly OperationAuthorizationRequirement Export = new() { Name = nameof(Export) };
    }
}