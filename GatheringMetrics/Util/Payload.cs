﻿namespace Middleman.Util;

public static class Payload
{
    public static string CurrentPayload = string.Empty;

    public static readonly string[] SupportedFileNames = new[] { "1kb", "10kb", "50kb", "100kb", "500kb", "1mb" };

    async public static Task<string> PreparePayload(string fileName)
    {
        string filePath = Path.Combine("G:\\VS\\repos\\protocol-testing\\GatheringMetrics\\payloads\\", $"{fileName}.txt");

        string fileContent = await File.ReadAllTextAsync(filePath);

        CurrentPayload = fileContent;

        return fileContent;
    }
}