using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

using OpenAI.Chat;
using OpenAI;
using OpenAI.Images;
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

using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;

using Azure;
using Azure.AI.DocumentIntelligence;

using Azure.AI.TextAnalytics;

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
        string audioFolder = @"audio";
        string transcriptionFolder= @"transcriptions";
        string generatedImagesFolder= @"generatedImages";

        Directory.CreateDirectory(imageFolder);
        Directory.CreateDirectory(summaryFolder);
        Directory.CreateDirectory(transcriptionFolder);
        Directory.CreateDirectory(generatedImagesFolder);
        var pdfFiles = Directory.GetFiles(pdfFolder, "*.pdf");
        var audioFiles = Directory.GetFiles(audioFolder, "*.mp3");

        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Summarize PDF and Translate Summary");
        Console.WriteLine("2. Extract Image from PDF, Classify and Describe");
        Console.WriteLine("3. Analyze Invoice and Validate Total");
        Console.WriteLine("4. Speech to Text");
        Console.WriteLine("5. Custom Speech");
        Console.WriteLine("6. Generate Image with DALL-E");
        Console.WriteLine("7. Analyze Text Sentiment");
        Console.Write("Enter your choice (1, 2, 3, 4, 5, 6 or 7): ");
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
            case "4":
                foreach (var audioFile in audioFiles){
                    Console.Write("Transcribing audio file: " + audioFile);
                    Console.Write("Enter the language code (e.g., 'en-US'): ");
                    string language = Console.ReadLine();
                    var sSpeechConfig = standardSpeechToText(language);
                    performSpeechToText(audioFile, sSpeechConfig, transcriptionFolder);
                }
                break;
            case "5":
                var cSpeechConfig = customSpeechToText();
                foreach (var audioFile in audioFiles){
                    Console.Write("Transcribing audio file: " + audioFile);
                    performSpeechToText(audioFile, cSpeechConfig, transcriptionFolder);
                }
                break;
            case "6":
                Console.Write("Enter a prompt for the image: ");
                string prompt = Console.ReadLine();
                GenerateImageWithDescription(prompt, openAIApiKey, generatedImagesFolder);
                break;
            case "7":
                Console.Write("Enter text for sentiment analysis: ");
                string text = Console.ReadLine();
                GetTextSentiment(text);
                break;

            default:
                Console.WriteLine("Invalid choice. Please select either 1, 2, 3, 4, 5, 6 or 7.");
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




    static string ConvertMp3ToWav(string mp3FilePath)
    {
        string wavFilePath = Path.ChangeExtension(mp3FilePath, ".wav");

        try
        {
            using var reader = new Mp3FileReader(mp3FilePath);
            using var writer = new WaveFileWriter(wavFilePath, reader.WaveFormat);
            reader.CopyTo(writer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during MP3-to-WAV conversion: {ex.Message}");
            throw;
        }

        Console.WriteLine($"Converted MP3 to WAV: {wavFilePath}");
        return wavFilePath;
    }

    private static void performSpeechToText(string mp3FilePath, SpeechConfig config, string transcriptionFolder)
    {
        string audioFilePath = ConvertMp3ToWav(mp3FilePath);

        var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);

        using var recognizer = new SpeechRecognizer(config, audioConfig);
        var result = recognizer.RecognizeOnceAsync().Result;

        switch (result.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                string outputFilePath = Path.Combine(
                    transcriptionFolder,
                    Path.GetFileNameWithoutExtension(mp3FilePath) + ".txt");
                File.WriteAllTextAsync(outputFilePath, result.Text);
                Console.WriteLine($"\nTranscription saved to {outputFilePath}");
                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                break;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(result);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }
    }

    private static SpeechConfig standardSpeechToText(string language)
    {
        string subscriptionKey = "";
        string region = "northeurope";

        var config = SpeechConfig.FromSubscription(subscriptionKey, region);
        config.SpeechRecognitionLanguage = language;

        return config;
    }

    private static SpeechConfig customSpeechToText(){
        string subscriptionKey = "";
        string region = "northeurope";
        string modelId = "";

        var config = SpeechConfig.FromSubscription(subscriptionKey, region);
        config.EndpointId = modelId;

        return config;
    }



    static async Task GenerateImageWithDescription(string prompt, string key, string generatedImagesFolder){
        ImageClient client = new("dall-e-3", key);

        ImageGenerationOptions options = new()
        {
            Quality = GeneratedImageQuality.Standard,
            Size = GeneratedImageSize.W1792xH1024,
            ResponseFormat = GeneratedImageFormat.Bytes
        };

        GeneratedImage image = client.GenerateImage(prompt, options);
        BinaryData bytes = image.ImageBytes;

        using FileStream stream = File.OpenWrite($"{generatedImagesFolder}/{Guid.NewGuid()}.png");
        bytes.ToStream().CopyTo(stream);
    }


    static async Task GetTextSentiment(string inputText){

        string endpoint = "";
        string apiKey = "";

        var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        Console.WriteLine("\nDocument Sentiment:");
        try
        {
            Response<DocumentSentiment> response = client.AnalyzeSentiment(inputText);
            DocumentSentiment documentSentiment = response.Value;

            Console.WriteLine($"Overall Sentiment: {documentSentiment.Sentiment}");
            Console.WriteLine("Sentiment Scores:");
            Console.WriteLine($"  Positive: {documentSentiment.ConfidenceScores.Positive:0.00}");
            Console.WriteLine($"  Neutral: {documentSentiment.ConfidenceScores.Neutral:0.00}");
            Console.WriteLine($"  Negative: {documentSentiment.ConfidenceScores.Negative:0.00}");

            foreach (var sentence in documentSentiment.Sentences)
            {
                Console.WriteLine($"\nSentence: \"{sentence.Text}\"");
                Console.WriteLine($"  Sentiment: {sentence.Sentiment}");
                Console.WriteLine($"  Scores -> Positive: {sentence.ConfidenceScores.Positive:0.00}, Neutral: {sentence.ConfidenceScores.Neutral:0.00}, Negative: {sentence.ConfidenceScores.Negative:0.00}");
            }
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }


}
