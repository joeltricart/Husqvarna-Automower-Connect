using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Infrastructure.Api;

internal static class MowerMapper
{
    public static Mower ToMower(string? mowerId, HusqvarnaApiClient.JsonApiMowerAttributesDto attributes)
    {
        MowerStatus? status = MapStatus(attributes);
        MowerLocation? location = MapLocation(attributes);
        MowerSchedule? schedule = null;

        return new Mower
        {
            Id = string.IsNullOrWhiteSpace(mowerId) ? string.Empty : mowerId,
            Name = attributes.System?.Name ?? "Robot sans nom",
            Model = attributes.System?.Model,
            SerialNumber = attributes.System?.SerialNumber?.ToString(),
            Status = status,
            Battery = MapBattery(attributes),
            Location = location,
            Schedule = schedule,
            Capabilities = MapCapabilities(attributes, status),
            LastUpdatedAt = MapUpdatedAt(attributes)
        };
    }

    private static BatteryInfo? MapBattery(HusqvarnaApiClient.JsonApiMowerAttributesDto attributes)
    {
        if (attributes.Battery is null)
        {
            return null;
        }

        return new BatteryInfo
        {
            LevelPercent = BatteryInfo.NormalizePercent(attributes.Battery.BatteryPercent),
            IsCharging = attributes.Mower?.Activity is "CHARGING",
            UpdatedAt = MapUpdatedAt(attributes)
        };
    }

    private static MowerLocation? MapLocation(HusqvarnaApiClient.JsonApiMowerAttributesDto attributes)
    {
        if (attributes.Capabilities?.Position != true || attributes.Positions is null)
        {
            return null;
        }

        HusqvarnaApiClient.JsonApiPositionDto? position = attributes.Positions.FirstOrDefault();
        if (position?.Latitude is null || position.Longitude is null)
        {
            return null;
        }

        if (!MowerLocation.IsValid(position.Latitude.Value, position.Longitude.Value))
        {
            return null;
        }

        return new MowerLocation
        {
            Latitude = position.Latitude.Value,
            Longitude = position.Longitude.Value,
            UpdatedAt = MapUpdatedAt(attributes)
        };
    }

    private static MowerStatus MapStatus(HusqvarnaApiClient.JsonApiMowerAttributesDto attributes)
    {
        MowerState state = attributes.Metadata?.Connected == false
            ? MowerState.Offline
            : MapState(attributes.Mower?.State);

        return new MowerStatus
        {
            State = state,
            Activity = MapActivity(attributes.Mower?.Activity),
            Mode = MapMode(attributes.Mower?.Mode),
            Connected = attributes.Metadata?.Connected,
            RestrictedReason = attributes.Mower?.InactiveReason,
            Error = MapError(attributes),
            UpdatedAt = MapUpdatedAt(attributes)
        };
    }

    private static MowerError? MapError(HusqvarnaApiClient.JsonApiMowerAttributesDto attributes)
    {
        if (attributes.Mower?.ErrorCode is null || attributes.Mower.ErrorCode <= 0)
        {
            return null;
        }

        MowerErrorSeverity severity = attributes.Mower.State is "ERROR" or "FATAL_ERROR" or "ERROR_AT_POWER_UP"
            ? MowerErrorSeverity.Critical
            : MowerErrorSeverity.Warning;

        return new MowerError
        {
            Code = attributes.Mower.ErrorCode.Value.ToString(),
            Message = "Erreur signalée par le robot.",
            Severity = severity,
            OccurredAt = attributes.Mower.ErrorCodeTimestamp is null
                ? null
                : DateTimeOffset.FromUnixTimeMilliseconds(attributes.Mower.ErrorCodeTimestamp.Value),
            IsConfirmable = attributes.Mower.IsErrorConfirmable
        };
    }

    private static MowerCapabilities MapCapabilities(
        HusqvarnaApiClient.JsonApiMowerAttributesDto attributes,
        MowerStatus status)
    {
        bool connected = attributes.Metadata?.Connected == true;
        bool isOperationPossible = connected && status.State is not MowerState.Error and not MowerState.Offline;

        return new MowerCapabilities
        {
            CanPause = status.State == MowerState.InOperation,
            CanParkUntilNextSchedule = connected,
            CanParkUntilFurtherNotice = connected,
            CanResumeSchedule = status.State is MowerState.Paused or MowerState.Parked,
            CanStartForDuration = isOperationPossible,
            CanShowSchedule = false,
            CanEditSchedule = false
        };
    }

    private static DateTimeOffset? MapUpdatedAt(HusqvarnaApiClient.JsonApiMowerAttributesDto attributes) =>
        attributes.Metadata?.StatusTimestamp is null
            ? null
            : DateTimeOffset.FromUnixTimeMilliseconds(attributes.Metadata.StatusTimestamp.Value);

    private static MowerState MapState(string? state) =>
        state?.ToUpperInvariant() switch
        {
            "PAUSED" => MowerState.Paused,
            "IN_OPERATION" => MowerState.InOperation,
            "STOPPED" => MowerState.Parked,
            "OFF" => MowerState.Parked,
            "ERROR" or "FATAL_ERROR" or "ERROR_AT_POWER_UP" => MowerState.Error,
            "RESTRICTED" or "WAIT_UPDATING" or "WAIT_POWER_UP" => MowerState.Ready,
            "UNKNOWN" => MowerState.Unknown,
            _ => MowerState.Unknown
        };

    private static MowerActivity MapActivity(string? activity) =>
        activity?.ToUpperInvariant() switch
        {
            "MOWING" => MowerActivity.Mowing,
            "GOING_HOME" => MowerActivity.GoingHome,
            "CHARGING" => MowerActivity.Charging,
            "LEAVING" => MowerActivity.Leaving,
            "PARKED_IN_CS" => MowerActivity.ParkedInChargingStation,
            "STOPPED_IN_GARDEN" => MowerActivity.Stopped,
            _ => MowerActivity.Unknown
        };

    private static MowerMode MapMode(string? mode) =>
        mode?.ToUpperInvariant() switch
        {
            "MAIN_AREA" => MowerMode.MainArea,
            "SECONDARY_AREA" => MowerMode.SecondaryArea,
            "HOME" => MowerMode.Home,
            "DEMO" => MowerMode.Demo,
            _ => MowerMode.Unknown
        };
}
