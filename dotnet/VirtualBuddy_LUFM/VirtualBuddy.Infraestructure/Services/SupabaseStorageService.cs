using Microsoft.Extensions.Options;
using Supabase;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure.Services
{
    public class SupabaseStorageService : IFileStorageService, IProjectImageStorageService
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
                AutoConnectRealtime = false,
                AutoRefreshToken = false
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

        public async Task<string> UploadAsync(Guid projectId, Stream imageStream, string contentType)
        {
            await EnsureInitializedAsync();

            var storagePath = GetProjectImagePath(projectId, Guid.NewGuid());
            var bucket = _supabaseClient.Storage.From(_settings.ProjectImagesBucketName);
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);

            await bucket.Upload(memoryStream.ToArray(), storagePath, new Supabase.Storage.FileOptions
            {
                ContentType = contentType,
                Upsert = false
            });

            return bucket.GetPublicUrl(storagePath);
        }

        public async Task DeleteAsync(Guid projectId, string url)
        {
            await EnsureInitializedAsync();
            if (!TryGetStoragePath(projectId, url, out var storagePath))
                return;

            var bucket = _supabaseClient.Storage.From(_settings.ProjectImagesBucketName);
            await bucket.Remove([storagePath]);
        }

        public bool IsManagedUrl(Guid projectId, string url)
        {
            return TryGetStoragePath(projectId, url, out _);
        }

        private bool TryGetStoragePath(Guid projectId, string url, out string storagePath)
        {
            storagePath = string.Empty;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var imageUri))
                return false;

            var projectPath = $"projects/{projectId:D}/";
            var projectBaseUrl = _supabaseClient.Storage
                .From(_settings.ProjectImagesBucketName)
                .GetPublicUrl(projectPath);
            if (!Uri.TryCreate(projectBaseUrl, UriKind.Absolute, out var projectBaseUri) ||
                imageUri.Scheme != projectBaseUri.Scheme ||
                imageUri.Host != projectBaseUri.Host ||
                !imageUri.AbsolutePath.StartsWith(projectBaseUri.AbsolutePath, StringComparison.Ordinal))
            {
                return false;
            }

            var fileName = Uri.UnescapeDataString(imageUri.AbsolutePath[projectBaseUri.AbsolutePath.Length..]);
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/'))
                return false;

            storagePath = projectPath + fileName;
            return true;
        }

        private static string GetProjectImagePath(Guid projectId, Guid imageId) =>
            $"projects/{projectId:D}/{imageId:D}";
    }
}
