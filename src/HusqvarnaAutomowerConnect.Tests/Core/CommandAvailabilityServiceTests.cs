using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Core.Services;

namespace HusqvarnaAutomowerConnect.Tests.Core;

public sealed class CommandAvailabilityServiceTests
{
    private readonly CommandAvailabilityService service = new();

    [Fact]
    public void EnsureAvailable_ShouldFail_WhenRobotIsMissing()
    {
        MowerCommand command = new() { Type = MowerCommandType.Pause };

        var result = service.EnsureAvailable(null, command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void EnsureAvailable_ShouldFail_WhenCapabilityIsMissing()
    {
        Mower mower = new()
        {
            Id = "m1",
            Capabilities = MowerCapabilities.None
        };

        MowerCommand command = new() { Type = MowerCommandType.ResumeSchedule };

        var result = service.EnsureAvailable(mower, command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.UserMessage.Should().Contain("commande");
    }

    [Fact]
    public void EnsureAvailable_ShouldFail_WhenDurationIsInvalid()
    {
        Mower mower = new()
        {
            Id = "m1",
            Capabilities = new MowerCapabilities
            {
                CanStartForDuration = true
            }
        };

        MowerCommand command = new()
        {
            Type = MowerCommandType.StartForDuration,
            Duration = TimeSpan.Zero
        };

        var result = service.EnsureAvailable(mower, command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.UserMessage.Should().Contain("durée");
    }
}

