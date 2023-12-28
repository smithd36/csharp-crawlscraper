/*
    File: ImageScraper.cs
    Author: Drey Smith
    Date: 12/28/2023

    Description:
    This C# program is an image scraper that allows the user to input a URL,
    scrape image URLs from the specified webpage, and optionally download and
    save the images. The program utilizes asynchronous programming for HTTP
    requests and the HtmlAgilityPack library for HTML parsing.

    Usage:
    - Run the program and enter a valid URL when prompted.
    - View the found image URLs and decide whether to download and save them.

    Dependencies:
    - HtmlAgilityPack NuGet package (Install using: dotnet add package HtmlAgilityPack)
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class ImageScraper
{
    static async Task Main(string[] args)
    {
        // Prompt the user to enter the URL to scrape images from
        Console.Write("Enter the URL to scrape images from: ");
        string targetUrl = Console.ReadLine();

        // Validate the entered URL
        if (Uri.TryCreate(targetUrl, UriKind.Absolute, out _))
        {
            // Scrape images from the provided URL
            List<string> imageUrls = await ScrapeImagesAsync(targetUrl);

            // Display the found image URLs to the user
            Console.WriteLine("Image URLs:");
            foreach (string imageUrl in imageUrls)
            {
                Console.WriteLine(imageUrl);
            }

            // Ask the user if they want to download and save the images
            Console.WriteLine($"Total Images Found: {imageUrls.Count}");
            Console.Write("Do you want to download and save these images? (Y/N): ");
            string response = Console.ReadLine();
            if (response.Equals("Y", StringComparison.OrdinalIgnoreCase)) {
                // Download and save the images
                await DownloadAndSaveImagesAsync(imageUrls, targetUrl);
                Console.WriteLine("Images downloaded and saved successfully!");
            }
        }
        else
        {
            // Inform the user about an invalid URL
            Console.WriteLine("Invalid URL entered. Please provide a valid URL.");
        }
    }

    static async Task<List<string>> ScrapeImagesAsync(string url)
    {
        // List to store found image URLs
        List<string> imageUrls = new List<string>();

        // Use HttpClient to make an HTTP request to the specified URL
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                // Retrieve HTML content from the URL
                string htmlContent = await httpClient.GetStringAsync(url);

                // Parse HTML content using HtmlAgilityPack
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                // Extract image URLs from the HTML document
                var imgNodes = htmlDocument.DocumentNode.SelectNodes("//img[@src]");
                if (imgNodes != null)
                {
                    foreach (var imgNode in imgNodes)
                    {
                        string imageUrl = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(imageUrl) && Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                        {
                            // Add valid image URLs to the list
                            imageUrls.Add(imageUrl);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle errors related to HTTP requests
                Console.WriteLine($"Error making HTTP request: {ex.Message}");
            }
        }

        return imageUrls;
    }

    static async Task DownloadAndSaveImagesAsync(List<string> imageUrls, string baseUrl)
    {
        // Use HttpClient to download and save images
        using (HttpClient httpClient = new HttpClient())
        {
            // Set the base address for relative URLs
            httpClient.BaseAddress = new Uri(baseUrl);

            // Iterate through each image URL and download/save it
            foreach (string imageUrl in imageUrls)
            {
                try
                {
                    // Ensure the URL is an absolute URI before attempting to download
                    if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri absoluteUri))
                    {
                        // Check if the URI scheme is 'file' and skip if true
                        if (absoluteUri.Scheme != "file")
                        {
                            // Download the image bytes
                            byte[] imageBytes = await httpClient.GetByteArrayAsync(absoluteUri);

                            // Extract the original file name from the URL
                            string originalFileName = Path.GetFileName(absoluteUri.LocalPath);

                            // Create a unique file name using GUID and preserve the original extension
                            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";

                            // Construct the file path to save the image
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                            // Save the image to the specified file path
                            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(imageBytes, 0, imageBytes.Length);
                            }

                            // Provide feedback about the downloaded image
                            Console.WriteLine($"Image '{originalFileName}' downloaded and saved as '{fileName}'");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Handle errors related to downloading images
                    Console.WriteLine($"Error downloading image from {imageUrl}: {ex.Message}");
                }
            }
        }
    }
}