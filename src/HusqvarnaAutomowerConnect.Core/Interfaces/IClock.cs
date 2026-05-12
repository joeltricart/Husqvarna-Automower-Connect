namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

