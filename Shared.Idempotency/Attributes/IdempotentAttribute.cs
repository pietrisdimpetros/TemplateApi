namespace Shared.Idempotency.Attributes
{
    /// <summary>
    /// Marks an action as Idempotent. 
    /// Requires the client to send an Idempotency-Key header.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class IdempotentAttribute : Attribute
    {
    }
}