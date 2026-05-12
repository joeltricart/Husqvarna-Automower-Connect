using HusqvarnaAutomowerConnect.Core.Interfaces;

namespace HusqvarnaAutomowerConnect.Infrastructure.Configuration;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

