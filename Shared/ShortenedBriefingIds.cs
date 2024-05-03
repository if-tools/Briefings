using System;

namespace IFToolsBriefings.Shared;

public class ShortenedBriefingIds
{
    private const int NumShortenedCharacters = 6;
    
    public static string Shorten(string id)
    {
        if (id == null || id.Length < NumShortenedCharacters)
            return String.Empty;
        return id.Substring(0, NumShortenedCharacters);
    }
}