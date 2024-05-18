namespace Middleman.Util;

public static class Payload
{
    public static string CurrentPayload = string.Empty;

    public static readonly string[] SupportedFileNames = new[] { "0kb", "1kb", "10kb", "50kb", "100kb", "500kb", "1mb" };

    async public static Task<string> PreparePayload(string fileName)
    {
        string filePath = Path.Combine(".\\payloads\\", $"{fileName}.txt");

        string fileContent = await File.ReadAllTextAsync(filePath);

        CurrentPayload = fileContent;

        return fileContent;
    }
}
