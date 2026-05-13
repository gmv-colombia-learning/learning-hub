namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="filePath"></param>
        /// <param name="fileStream"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<string> UploadFileAsync(string filePath, Stream fileStream, string contentType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="filePath"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        Task<string> UploadFileFromPathAsync(string bucketName, string filePath,
                                                           string destinationPath);

        /// <summary>
        /// Obtiene la URL pública de un archivo.
        /// </summary>
        /// <param name="filePath">Ruta relativa del archivo en el bucket.</param>
        /// <returns>La URL absoluta para acceder al archivo.</returns>
        Task<string> GetSignedUrlAsync(string filePath, int expiresInSeconds = 3600);

        Task<bool> DeleteFileAsync(string filePath);


    }
}
