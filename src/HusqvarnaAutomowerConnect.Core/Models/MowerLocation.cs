namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record MowerLocation
{
    public required double Latitude { get; init; }

    public required double Longitude { get; init; }

    public double? AccuracyMeters { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public static bool IsValid(double latitude, double longitude) =>
        latitude is >= -90 and <= 90 &&
        longitude is >= -180 and <= 180;
}

