namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record BatteryInfo
{
    public int? LevelPercent { get; init; }

    public bool? IsCharging { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public static int? NormalizePercent(int? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value is >= 0 and <= 100 ? value : null;
    }
}

