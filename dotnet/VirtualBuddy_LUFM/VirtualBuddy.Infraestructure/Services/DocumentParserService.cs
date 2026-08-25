using System.Text;
using DocumentFormat.OpenXml.Packaging;
using ExcelDataReader;
using UglyToad.PdfPig;
using VirtualBuddy.Application.Common.Interfaces;

namespace VirtualBuddy.Infraestructure.Services
{
    public class DocumentParserService : IDocumentParser
    {
        public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => await ExtractTextFromPdfAsync(fileStream),
                ".docx" => await ExtractTextFromWordAsync(fileStream),
                ".xlsx" or ".xls" => await ExtractTextFromExcelAsync(fileStream),
                ".txt" => await ExtractTextFromTxtAsync(fileStream),
                _ => string.Empty
            };
        }

        private async Task<string> ExtractTextFromPdfAsync(Stream fileStream)
        {
            return await Task.Run(() =>
            {
                using var document = PdfDocument.Open(fileStream);
                var textBuilder = new StringBuilder();
                foreach (var page in document.GetPages())
                {
                    textBuilder.AppendLine(page.Text);
                }
                return textBuilder.ToString();
            });
        }

        private async Task<string> ExtractTextFromWordAsync(Stream fileStream)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var wordDoc = WordprocessingDocument.Open(fileStream, false);
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    return body?.InnerText ?? string.Empty;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            });
        }

        private async Task<string> ExtractTextFromExcelAsync(Stream fileStream)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Required for ExcelDataReader on some platforms/files
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using var reader = ExcelReaderFactory.CreateReader(fileStream);
                    var result = reader.AsDataSet();
                    var textBuilder = new StringBuilder();

                    foreach (System.Data.DataTable table in result.Tables)
                    {
                        foreach (System.Data.DataRow row in table.Rows)
                        {
                            foreach (var item in row.ItemArray)
                            {
                                if (item != null)
                                {
                                    textBuilder.Append(item.ToString() + " ");
                                }
                            }
                            textBuilder.AppendLine();
                        }
                    }
                    return textBuilder.ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            });
        }

        private async Task<string> ExtractTextFromTxtAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
