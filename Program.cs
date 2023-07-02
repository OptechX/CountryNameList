using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

internal class Program
{
    private static string outputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.txt");
    private static string apiUrl = "http://localhost:5179/api/CountryNameIndex";
    private static async Task Main(string[] args)
    {
        bool runPost = true; // Default value

        // Check if "--no-post" switch argument is provided
        if (args.Length > 0 && args[0] == "--no-post")
        {
            runPost = false;
        }

        if (File.Exists(outputFilePath))
            File.Delete(outputFilePath);

        List<string> countryList = new();
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        foreach (CultureInfo culture in cultures)
        {
            RegionInfo region = new RegionInfo(culture.Name);
            if (!countryList.Contains(region.EnglishName))
            {
                countryList.Add(region.EnglishName);
            }
        }

        countryList.Sort();
        countryList.Remove("world");

        using (StreamWriter writer = new StreamWriter(outputFilePath, true))
        {
            int lineCount = countryList.Count;
            for (int i = 0; i < lineCount; i++)
            {
                string country = countryList[i];
                Console.WriteLine(country);
                if (i < lineCount - 1)
                {
                    writer.WriteLine(country);
                }
            }
        }

        if (runPost)
        {
            await PostCountriesFromFileAsync(filePath: outputFilePath);
        }
    }

    private static async Task PostCountriesFromFileAsync(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                string countryName = line.Trim();
                await PostCountryAsync(countryName);
            }
        }
    }

    private static async Task PostCountryAsync(string countryName)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string json = $"{{ \"countryName\": \"{countryName}\" }}";
                HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to post country '{countryName}'. Error: {errorResponse}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while posting country '{countryName}': {ex.Message}");
        }
    }
}
