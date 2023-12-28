using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;

class ImageScraper
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        // Replace this URL with the target website you want to scrape images from
        string targetUrl = "https://www.google.com/search?q=images&oq=images&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIHCAEQABiPAjIHCAIQABiPAjIGCAMQRRg9MgYIBBBFGDzSAQkxMDA3OGowajGoAgCwAgA&sourceid=chrome&ie=UTF-8";

        List<string> imageUrls = await ScrapeImagesAsync(targetUrl);

        Console.WriteLine("Image URLs:");
        foreach (string imageUrl in imageUrls)
        {
            Console.WriteLine(imageUrl);
        }

        Console.WriteLine($"Total Images Found: {imageUrls.Count}");
    }

    static async System.Threading.Tasks.Task<List<string>> ScrapeImagesAsync(string url)
    {
        List<string> imageUrls = new List<string>();

        // Make an HTTP request to the target URL
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                string htmlContent = await httpClient.GetStringAsync(url);

                // Parse HTML content using HtmlAgilityPack
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                // Extract image URLs (you may need to adjust the XPath expression based on the website structure)
                var imgNodes = htmlDocument.DocumentNode.SelectNodes("//img[@src]");
                if (imgNodes != null)
                {
                    foreach (var imgNode in imgNodes)
                    {
                        string imageUrl = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // Handle relative URLs by converting them to absolute URLs
                            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                            {
                                Uri baseUri = new Uri(url);
                                Uri absoluteUri = new Uri(baseUri, imageUrl);
                                imageUrl = absoluteUri.ToString();
                            }

                            imageUrls.Add(imageUrl);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error making HTTP request: {ex.Message}");
            }
        }

        return imageUrls;
    }
}