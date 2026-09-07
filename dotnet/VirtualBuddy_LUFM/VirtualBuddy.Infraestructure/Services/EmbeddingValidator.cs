using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Infraestructure.Services
{
    internal static class EmbeddingValidator
    {
        public static void Validate(float[] embedding, int expectedDimension)
        {
            if (embedding.Length != expectedDimension)
                throw new ValidationException(
                    $"La dimension del embedding debe ser {expectedDimension}, pero se recibio {embedding.Length}.");
        }
    }
}
