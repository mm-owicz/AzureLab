// using OpenAI.Managers;
// using OpenAI.GPT3;
using OpenAI.Chat;
using OpenAI;
using System.Text;
using Docnet.Core;
using Docnet.Core.Models;

class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("No API key found.");
            return;
        }

        ChatClient client = new("gpt-4o-mini", apiKey);

        string pdfFolder = @"C:\Users\magma\Desktop\MGSTR\sem1\ChmuraLab\zad1\PDFSummaryApp\PDFs";
        string summaryFolder = @"C:\Users\magma\Desktop\MGSTR\sem1\ChmuraLab\zad1\PDFSummaryApp\PDFSummaries";

        Directory.CreateDirectory(summaryFolder);
        var pdfFiles = Directory.GetFiles(pdfFolder, "*.pdf");

        foreach (var pdfFile in pdfFiles)
        {
            Console.WriteLine($"Summarizing...: {Path.GetFileName(pdfFile)}");
            string text = ExtractTextFromPdf(pdfFile);

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine($"No text found in {pdfFile}");
                continue;
            }

            string summary = await GenerateSummary(client, text);

            string summaryFilePath = Path.Combine(summaryFolder, Path.GetFileNameWithoutExtension(pdfFile) + "_summary.txt");
            File.WriteAllText(summaryFilePath, summary);

            Console.WriteLine($"\nSummary for {Path.GetFileName(pdfFile)}:\n");
            Console.WriteLine(summary);
        }
    }

    static string ExtractTextFromPdf(string filePath)
    {

        var textContent = string.Empty;
       using (var library = DocLib.Instance)
        {
            using (var docReader = library.GetDocReader(File.ReadAllBytes(filePath), new PageDimensions(1080, 1920)))
            {
                var pageCount = docReader.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        var pageText = pageReader.GetText();
                        textContent += pageText + Environment.NewLine;
                    }
                }
            }
        }
        return textContent;
    }

    static async Task<string> GenerateSummary(ChatClient client, string text)
    {

        ChatCompletion completion = client.CompleteChat($"Write a summary of this text: {text}");

        return completion.Content[0].Text;
    }
}
