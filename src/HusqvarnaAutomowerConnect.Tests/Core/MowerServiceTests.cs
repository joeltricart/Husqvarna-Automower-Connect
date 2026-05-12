using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Core.Services;

namespace HusqvarnaAutomowerConnect.Tests.Core;

public sealed class MowerServiceTests
{
    [Fact]
    public async Task SendCommandAsync_ShouldStopBeforeApiCommand_WhenCommandIsUnavailable()
    {
        FakeApiClient apiClient = new(new Mower
        {
            Id = "m1",
            Capabilities = MowerCapabilities.None
        });

        MowerService service = new(apiClient, new CommandAvailabilityService());

        OperationResult<CommandResult> result = await service.SendCommandAsync(
            "m1",
            new MowerCommand { Type = MowerCommandType.Pause },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        apiClient.SendCommandCalls.Should().Be(0);
    }

    [Fact]
    public async Task SendCommandAsync_ShouldForwardToApi_WhenCommandIsAvailable()
    {
        FakeApiClient apiClient = new(new Mower
        {
            Id = "m1",
            Capabilities = new MowerCapabilities
            {
                CanPause = true
            }
        });

        MowerService service = new(apiClient, new CommandAvailabilityService());

        OperationResult<CommandResult> result = await service.SendCommandAsync(
            "m1",
            new MowerCommand { Type = MowerCommandType.Pause },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        apiClient.SendCommandCalls.Should().Be(1);
    }

    private sealed class FakeApiClient(Mower mower) : IHusqvarnaApiClient
    {
        public int SendCommandCalls { get; private set; }

        public Task<OperationResult<Mower>> GetMowerAsync(string mowerId, CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<Mower>.Success(mower));

        public Task<OperationResult<IReadOnlyList<Mower>>> GetMowersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<IReadOnlyList<Mower>>.Success([mower]));

        public Task<OperationResult<CommandResult>> SendCommandAsync(
            string mowerId,
            MowerCommand command,
            CancellationToken cancellationToken)
        {
            SendCommandCalls++;
            return Task.FromResult(OperationResult<CommandResult>.Success(new CommandResult
            {
                Success = true,
                Command = command,
                Message = "Commande envoyée.",
                AcceptedAt = DateTimeOffset.UtcNow,
                ShouldRefresh = true
            }));
        }
    }
}

