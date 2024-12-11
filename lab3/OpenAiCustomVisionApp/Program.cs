using OpenAI.Chat;
using OpenAI;
using System.Text;
using Docnet.Core;
using Docnet.Core.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;


using System.Collections.Generic;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

class Program
{
    static async Task Main(string[] args)
    {
        string openAIApiKey = "";

        ChatClient client = new("gpt-4o-mini", openAIApiKey);

        string pdfFolder = @"PDFs";
        string summaryFolder = @"PDFSummaries";
        string imageFolder = @"images";

        Directory.CreateDirectory(imageFolder);
        Directory.CreateDirectory(summaryFolder);
        var pdfFiles = Directory.GetFiles(pdfFolder, "*.pdf");

        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Summarize PDF and Translate Summary");
        Console.WriteLine("2. Extract Image from PDF, Classify and Describe");
        Console.Write("Enter your choice (1 or 2): ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("Enter the source language code (fromLang, e.g., 'english'): ");
                string fromLang = Console.ReadLine();

                Console.Write("Enter the target language code (toLang, e.g., 'polish'): ");
                string toLang = Console.ReadLine();

                foreach (var pdfFile in pdfFiles)
                {
                    string summary = await GetSummaryConsoleFile(client, pdfFile, summaryFolder);
                    await GetTranslation(client, summary, fromLang, toLang);
                }
                break;
            case "2":
                foreach (var pdfFile in pdfFiles)
                {
                    ProcessPDFImages(client, pdfFile);
                }
                break;

            default:
                Console.WriteLine("Invalid choice. Please select either 1 or 2.");
                break;
        }

    }

    static async Task<String> GetSummaryConsoleFile(ChatClient client, string pdfFile, string summaryFolder){
        Console.WriteLine($"Summarizing...: {Path.GetFileName(pdfFile)}");
        string text = ExtractTextFromPdf(pdfFile);

        if (string.IsNullOrEmpty(text))
        {
            Console.WriteLine($"No text found in {pdfFile}");
        }

        string summary = await GenerateSummary(client, text);

        string summaryFilePath = Path.Combine(summaryFolder, Path.GetFileNameWithoutExtension(pdfFile) + "_summary.txt");
        File.WriteAllText(summaryFilePath, summary);

        Console.WriteLine($"\nSummary for {Path.GetFileName(pdfFile)}:\n");
        Console.WriteLine(summary);

        return summary;
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

    static async Task GetTranslation(ChatClient client, string text, string fromLang, string toLang){
        Console.WriteLine($"Translating summary from {fromLang} to {toLang}...");

        string trans = await GenerateTranslation(client, text, fromLang, toLang);

        Console.WriteLine($"Translation: \n");
        Console.WriteLine(trans);
    }

    static async Task<string> GenerateTranslation(ChatClient client, string text, string fromLang, string toLang)
    {
        ChatCompletion completion = client.CompleteChat($"Translate this text from {fromLang} to {toLang}: {text}");

        return completion.Content[0].Text;
    }

    static async Task ProcessPDFImages(ChatClient client, string filePath){
        ExtractImagesAndSaveToFile(filePath);

        string imageFolder = @"images";
        var images = Directory.GetFiles(imageFolder, "*.png");
        foreach(var image in images){
            GetPredictionAndDescription(client, image);
            File.Delete(image);
        }

    }

    static async Task GetPredictionAndDescription(ChatClient client, string imagePath){
        Console.WriteLine($"--- Image: {imagePath} ---");
        byte[] imageBytes = File.ReadAllBytes(imagePath);

        var prediction = await PredictImageType(imageBytes);
        Console.WriteLine($"Image Type Prediction: {prediction}");

        string description = await GenerateImageDescription(client, imagePath, prediction);
        Console.WriteLine($"Image Description: {description}");
    }

    static async Task<string> PredictImageType(byte[] imageBytes){
        string endpoint = "https://pdfimageclassificationlab3-prediction.cognitiveservices.azure.com/";
        string predictionKey = "";
        string projectId = "";
        string modelName = "GuitarHatBottleModel";


        var client = new CustomVisionPredictionClient(
            new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
        {
            Endpoint = endpoint
        };

        Stream stream = new MemoryStream(imageBytes);
        var result = client.ClassifyImage(Guid.Parse(projectId), modelName, stream);
        if (result.Predictions.Count > 0)
        {
            return result.Predictions[0].TagName;
        }
        else
        {
            return "No prediction made";
        }

    }

    static async Task<string> GenerateImageDescription(ChatClient client, string imageFilePath, string type)
    {
        using Stream imageStream = File.OpenRead(imageFilePath);
        BinaryData imageBytes = BinaryData.FromStream(imageStream);

        List<ChatMessage> messages =
        [
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart($"Please describe the following image. It was labelled with {type}."),
                ChatMessageContentPart.CreateImagePart(imageBytes, "image/png")),
        ];

        ChatCompletion completion = client.CompleteChat(messages);
        return completion.Content[0].Text;
    }

    public static void ExtractImagesAndSaveToFile(string filePath)
    {

        byte[] pdfBytes = File.ReadAllBytes(filePath);
        string outputDirectory = "images";

        using var document = PdfDocument.Open(pdfBytes);
        int imageCount = 0;

        foreach (var page in document.GetPages())
        {
            foreach (var pdfImage in page.GetImages())
            {
                var bytes = TryGetImage(pdfImage);
                using var mem = new MemoryStream(bytes);
                System.Drawing.Image img;
                try
                {
                    img = System.Drawing.Image.FromStream(mem);
                }
                catch (Exception)
                {
                    continue;
                }

                string fileName = Path.Combine(outputDirectory, $"image_{imageCount++}.png");

                img.Save(fileName, ImageFormat.Png);
            }
        }
    }

    private static byte[] TryGetImage(IPdfImage image)
    {
        if (image.TryGetPng(out var bytes))
            return bytes;

        return image.RawBytes.ToArray();
    }



}


