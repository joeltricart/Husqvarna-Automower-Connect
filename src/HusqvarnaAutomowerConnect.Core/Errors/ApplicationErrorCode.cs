namespace HusqvarnaAutomowerConnect.Core.Errors;

public enum ApplicationErrorCode
{
    Unknown = 0,
    Validation = 1,
    InvalidConfiguration = 2,
    Network = 3,
    Unauthorized = 4,
    Forbidden = 5,
    NotFound = 6,
    RateLimited = 7,
    ServiceUnavailable = 8,
    UnsupportedCommand = 9,
    NoMowerFound = 10,
    SecureStorageUnavailable = 11
}

