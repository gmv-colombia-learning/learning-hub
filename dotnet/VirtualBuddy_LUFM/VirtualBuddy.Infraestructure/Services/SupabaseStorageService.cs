using Microsoft.Extensions.Options;
using Supabase;
using VirtualBuddy.Application.Common.Interfaces;

namespace VirtualBuddy.Infraestructure.Services
{
    public class SupabaseStorageService : IFileStorageService
    {
        private readonly Client _supabaseClient;
        private readonly SupabaseSettings _settings;

        public SupabaseStorageService(IOptions<SupabaseSettings> settings)
        {
            _settings = settings.Value;
            var url = _settings.Url;
            var key = _settings.Key;

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true,
                AutoRefreshToken = true
            };

            _supabaseClient = new Client(url, key, options);
        }

        // Método para asegurar que el cliente esté inicializado
        private async Task EnsureInitializedAsync()
        {
            if (_supabaseClient is null)
                throw new InvalidOperationException("El cliente no está inicializado.");

            await _supabaseClient.InitializeAsync();
        }

        /// <summary>
        /// Sube un archivo a Supabase Storage.
        /// </summary>
        /// <param name="bucketName">Nombre del bucket de destino</param>
        /// <param name="filePath">Ruta completa del archivo en el bucket (ej: "carpeta/mi-archivo.pdf")</param>
        /// <param name="fileStream">Stream del contenido del archivo</param>
        /// <param name="contentType">Tipo MIME del archivo (ej: "application/pdf")</param>
        /// <returns>URL pública del archivo subido</returns>
        public async Task<string> UploadFileAsync(string filePath, Stream fileStream, string contentType)
        {
            await EnsureInitializedAsync();

            var storageKey = Guid.NewGuid().ToString();

            var extension = Path.GetExtension(filePath);
            var storagePath = $"{storageKey}{extension}";

            // Obtener referencia al bucket
            var bucket = _supabaseClient.Storage.From(_settings.BucketName);

            // Configurar opciones de subida
            var fileOptions = new Supabase.Storage.FileOptions
            {
                ContentType = contentType,
                Upsert = true // Sobrescribe si el archivo ya existe
            };

            // Convertir Stream a byte array
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            var x = await bucket.Upload(fileBytes, storagePath, fileOptions);

            return storagePath;
        }

        /// <summary>
        /// Sube un archivo desde una ruta física del servidor.
        /// </summary>
        public async Task<string> UploadFileFromPathAsync(string bucketName, string filePath,
                                                           string destinationPath)
        {
            await EnsureInitializedAsync();

            var bucket = _supabaseClient.Storage.From(bucketName);

            // Método directo si tienes la ruta del archivo local
            await bucket.Upload(filePath, destinationPath);

            return bucket.GetPublicUrl(destinationPath);
        }

        /// <summary>
        /// Genera una URL firmada para buckets privados (validez temporal).
        /// </summary>
        public async Task<string> GetSignedUrlAsync(string filePath, int expiresInSeconds = 3600)
        {
            await EnsureInitializedAsync();

            var bucket = _supabaseClient.Storage.From(_settings.BucketName);
            var signedUrl = await bucket.CreateSignedUrl(filePath, expiresInSeconds);
            return signedUrl;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            await EnsureInitializedAsync();

            var bucket = _supabaseClient.Storage.From(_settings.BucketName);

            // El método Remove espera una lista de rutas
            var result = await bucket.Remove(new List<string> { filePath });

            // El resultado contiene las rutas que fueron eliminadas exitosamente
            return result != null && result.Count > 0;
        }
    }
}
