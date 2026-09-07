namespace VirtualBuddy.Infraestructure.Util
{
    public sealed class EmbeddingSettings
    {
        public const string SectionName = "Ollama";
        public const int MaximumAzureSqlDimensions = 1998;

        public int EmbeddingDimension { get; set; }
    }
}
