using System.Reflection;

namespace Shared.Composition.Helper
{
    public static class CompositionHelper
    {
        /// <summary>
        /// Uses Reflection to copy properties from the source (user config) to the destination (library default).
        /// This ensures that if the underlying library adds new properties, they are mapped automatically.
        /// </summary>
        public static void CopyProperties<T>(T source, T destination) where T : class
        {
            if (source == null || destination == null) return;

            // Get all readable and writable public properties
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source);

                // Only copy if the value is not null (optional safeguard)
                // or simply copy everything including defaults if that's preferred.
                // Here we copy everything to ensure the user's intent overrides defaults.
                prop.SetValue(destination, value);
            }
        }
    }
}