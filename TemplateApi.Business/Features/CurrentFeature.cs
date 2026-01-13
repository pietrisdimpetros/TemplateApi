namespace TemplateApi.Business.Features
{
    public static class CurrentFeature
    {
        public const string NewGraphCheck = nameof(NewGraphCheck);
        public const string BetaDashboard = nameof(BetaDashboard);

        // 3. Vectorized validation for hot paths (e.g. middleware)
        public static readonly System.Buffers.SearchValues<string> AllFeatures =
            System.Buffers.SearchValues.Create(
[
                    NewGraphCheck, 
                    BetaDashboard
                ], 
                StringComparison.Ordinal);
    }
}
