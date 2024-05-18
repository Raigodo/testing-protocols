namespace Microservice.Util;

public static class WebSocketMessageExtensions
{
    public static string[] ToChunks(this string msg, int chunkSize)
    {
        var chunkCount = (int)Math.Ceiling(msg.Length / (float)chunkSize);
        var chunks = new string[chunkCount];

        for (int i = 0; i < chunkCount; i++)
        {
            var isChunkInBounds = i * chunkSize + chunkSize > msg.Length;
            if (isChunkInBounds)
                chunks[i] = msg.Substring(i * chunkSize, chunkSize);
            else
                chunks[i] = msg.Substring(i * chunkSize);
        }
        return chunks;
    }
}
