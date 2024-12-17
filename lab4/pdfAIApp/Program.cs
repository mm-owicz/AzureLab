using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

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

using Azure;
using Azure.AI.DocumentIntelligence;

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
        string endpoint = "https://documentintelligencelab42024.cognitiveservices.azure.com/";
        string apiKey = "";

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
        Console.WriteLine("3. Analyze Invoice and Validate Total");
        Console.Write("Enter your choice (1, 2 or 3): ");
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
                    string summary = await GetSummaryConsoleFile(client, pdfFile, summaryFolder, endpoint, apiKey);
                    await GetTranslation(client, summary, fromLang, toLang);
                }
                break;
            case "2":
                foreach (var pdfFile in pdfFiles)
                {
                    ProcessPDFImages(client, pdfFile);
                }
                break;

            case "3":
                foreach (var pdfFile in pdfFiles)
                {
                    await AnalyzeInvoiceAndValidateTotal(endpoint, apiKey, pdfFile);
                }
                break;

            default:
                Console.WriteLine("Invalid choice. Please select either 1, 2 or 3.");
                break;
        }



    }

    public static async Task AnalyzeInvoiceAndValidateTotal(string endpoint, string apiKey, string filePath)
    {
        var credential = new AzureKeyCredential(apiKey);
        var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

        byte[] fileBytes = File.ReadAllBytes(filePath);

        BinaryData binaryData = new BinaryData(fileBytes);
        var content = new AnalyzeDocumentContent() { Base64Source  = binaryData };

        Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-invoice", content);

        AnalyzeResult result = operation.Value;

        double extractedTotal = 0;
        double calculatedTotal = 0;
        double totalTax = 0;

        Console.WriteLine("\n--- Extracted Invoice Fields ---");

        foreach (var document in result.Documents)
        {
            foreach (var field in document.Fields)
            {
                string fieldName = field.Key;
                string fieldValue = field.Value?.Content ?? "N/A";
                Console.WriteLine($"{fieldName}: {fieldValue}");

                if (fieldName.Equals("Items", StringComparison.OrdinalIgnoreCase))
                {

                    IReadOnlyList<DocumentField> items = field.Value.ValueList as IReadOnlyList<DocumentField>;
                    foreach (var item in items)
                    {
                        IReadOnlyDictionary<string, DocumentField> dictionary = item.ValueDictionary as IReadOnlyDictionary<string, DocumentField>;
                        DocumentField amountField = null;

                        if (dictionary.TryGetValue("Amount", out amountField))
                        {
                            double itemAmount = 0;

                            if (amountField?.ValueCurrency != null)
                            {
                                itemAmount = amountField.ValueCurrency?.Amount ?? 0.0;
                            }
                            calculatedTotal += itemAmount;
                        }

                    }
                }
            }

            if (document.Fields.TryGetValue("InvoiceTotal", out DocumentField totalField))
            {
                extractedTotal = totalField.ValueCurrency?.Amount ?? 0.0;
            }
            if (document.Fields.TryGetValue("TotalTax", out DocumentField taxField))
            {
                totalTax = taxField.ValueCurrency?.Amount ?? 0.0;
            }

        }

        calculatedTotal += totalTax;
        if (calculatedTotal == extractedTotal)
        {
            Console.WriteLine($"Validation Success: Item sum ({calculatedTotal}) matches the total ({extractedTotal}).");
        }
        else
        {
            Console.WriteLine($"Validation Error: Item sum ({calculatedTotal}) does NOT match the total ({extractedTotal}).");
        }
    }

    static async Task<string> AnalyzeWithLayoutModel(string endpoint, string apiKey, string pdfPath)
    {
        var credential = new AzureKeyCredential(apiKey);
        var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

        byte[] fileBytes = File.ReadAllBytes(pdfPath);

        BinaryData binaryData = new BinaryData(fileBytes);
        var content = new AnalyzeDocumentContent() { Base64Source  = binaryData };

        Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", content);
        AnalyzeResult result = operation.Value;
        string text = "";

        foreach (var page in result.Pages)
        {

            if (page.Lines != null && page.Lines.Count > 0)
            {
                foreach (var line in page.Lines)
                {
                    text = text + line.Content + '\n';
                }
            }
        }
        return text;
    }







    static async Task<String> GetSummaryConsoleFile(ChatClient client, string pdfFile, string summaryFolder, string endpoint, string apiKey){
        Console.WriteLine($"Summarizing...: {Path.GetFileName(pdfFile)}");
        string text = await AnalyzeWithLayoutModel(endpoint, apiKey, pdfFile);

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
