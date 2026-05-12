using FluentAssertions;
using HusqvarnaAutomowerConnect.App.ViewModels;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Tests.App;

public sealed class ViewModelTests
{
    [Fact]
    public async Task SettingsViewModel_LoadAsync_populates_local_configuration()
    {
        FakeAppSettingsStore store = new(new AppSettings
        {
            ApplicationKey = "demo-key",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 90,
            MinimumLogLevel = "Warning"
        });

        SettingsViewModel viewModel = new(store, new FakeApplicationSecretStore());

        await viewModel.LoadAsync(CancellationToken.None);

        viewModel.ApplicationKey.Should().Be("demo-key");
        viewModel.RedirectUri.Should().Be("http://localhost");
        viewModel.RefreshIntervalSeconds.Should().Be(90);
        viewModel.MinimumLogLevel.Should().Be("Warning");
        viewModel.StatusMessage.Should().Contain("chargé");
        viewModel.SecretStatusMessage.Should().Contain("Aucun");
    }

    [Fact]
    public async Task SettingsViewModel_SaveAsync_rejects_invalid_configuration()
    {
        FakeAppSettingsStore store = new(new AppSettings());

        SettingsViewModel viewModel = new(store, new FakeApplicationSecretStore())
        {
            ApplicationKey = "",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 20,
            MinimumLogLevel = "Information"
        };

        await viewModel.SaveAsync(CancellationToken.None);

        store.SaveCallCount.Should().Be(0);
        viewModel.ValidationMessage.Should().Contain("Husqvarna");
        viewModel.StatusMessage.Should().Contain("Corrigez");
    }

    [Fact]
    public async Task SettingsViewModel_SaveAsync_stores_the_application_secret()
    {
        FakeApplicationSecretStore secretStore = new();
        FakeAppSettingsStore store = new(new AppSettings
        {
            ApplicationKey = "demo-key",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 60
        });

        SettingsViewModel viewModel = new(store, secretStore)
        {
            ApplicationKey = "demo-key",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 60,
            MinimumLogLevel = "Information",
            ApplicationSecret = "new-secret"
        };

        await viewModel.SaveAsync(CancellationToken.None);

        secretStore.SavedSecret.Should().Be("new-secret");
        store.SavedSettings.Should().NotBeNull();
        store.SavedSettings!.ApplicationKey.Should().Be("demo-key");
        store.SavedSettings.RedirectUri.Should().Be("http://localhost");
        store.SavedSettings.RefreshIntervalSeconds.Should().Be(60);
        viewModel.HasApplicationSecret.Should().BeTrue();
        viewModel.ApplicationSecret.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginViewModel_RefreshAsync_detects_local_session()
    {
        FakeSecureTokenStore store = new(new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        LoginViewModel viewModel = CreateLoginViewModel(
            store,
            new FakeApplicationSecretStore("secret"),
            new FakeAuthClient(),
            new FakeOAuthCallbackListener(),
            new FakeBrowserLauncher());

        await viewModel.RefreshAsync(CancellationToken.None);

        viewModel.HasLocalSession.Should().BeTrue();
        viewModel.ConnectionState.Should().Contain("disponible");
        viewModel.LocalSessionDetail.Should().Contain("renouvellement");
        viewModel.SecretState.Should().Contain("enregistré");
    }

    [Fact]
    public async Task LoginViewModel_DisconnectAsync_deletes_the_local_session()
    {
        FakeSecureTokenStore store = new(new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        FakeAuthClient authClient = new();
        LoginViewModel viewModel = CreateLoginViewModel(
            store,
            new FakeApplicationSecretStore("secret"),
            authClient,
            new FakeOAuthCallbackListener(),
            new FakeBrowserLauncher());

        await viewModel.DisconnectAsync(CancellationToken.None);

        store.DeleteCallCount.Should().Be(1);
        authClient.RevokeCallCount.Should().Be(1);
        viewModel.HasLocalSession.Should().BeFalse();
        viewModel.ConnectionState.Should().Contain("supprimée");
    }

    [Fact]
    public async Task LoginViewModel_ConnectAsync_opens_the_browser_and_saves_the_session()
    {
        FakeAppSettingsStore settingsStore = new(new AppSettings
        {
            ApplicationKey = "demo-key",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 60
        });
        FakeApplicationSecretStore secretStore = new("secret");
        FakeSecureTokenStore tokenStore = new(new AuthSession());
        FakeAuthClient authClient = new()
        {
            AuthorizationUri = new Uri("https://api.authentication.husqvarnagroup.dev/v1/oauth2/authorize?client_id=demo-key"),
            ExchangeResult = OperationResult<AuthSession>.Success(new AuthSession
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            })
        };
        FakeOAuthCallbackListener callbackListener = new(
            OperationResult<OAuthCallbackResult>.Success(new OAuthCallbackResult
            {
                Code = "auth-code",
                State = "expected-state"
            }));
        FakeBrowserLauncher browserLauncher = new();

        LoginViewModel viewModel = new(
            settingsStore,
            secretStore,
            tokenStore,
            authClient,
            callbackListener,
            browserLauncher);

        await viewModel.ConnectAsync(CancellationToken.None);

        browserLauncher.OpenedUris.Should().ContainSingle();
        tokenStore.SaveCallCount.Should().Be(1);
        viewModel.ConnectionState.Should().Contain("réussie");
    }

    [Fact]
    public async Task DashboardViewModel_LoadAsync_populates_cards_and_french_labels()
    {
        FakeMowerService mowerService = new([
            new Mower
            {
                Id = "mower-1",
                Name = "Pelouse",
                Model = "450X",
                SerialNumber = "701009001",
                Battery = new BatteryInfo
                {
                    LevelPercent = 77,
                    IsCharging = true,
                    UpdatedAt = DateTimeOffset.Parse("2026-05-12T08:00:00+00:00")
                },
                Location = new MowerLocation
                {
                    Latitude = 57.79051,
                    Longitude = 14.28367,
                    UpdatedAt = DateTimeOffset.Parse("2026-05-12T08:05:00+00:00")
                },
                Status = new MowerStatus
                {
                    State = MowerState.InOperation,
                    Activity = MowerActivity.Mowing,
                    Mode = MowerMode.MainArea,
                    Connected = true,
                    UpdatedAt = DateTimeOffset.Parse("2026-05-12T08:06:00+00:00")
                },
                LastUpdatedAt = DateTimeOffset.Parse("2026-05-12T08:06:00+00:00")
            }
        ]);

        DashboardViewModel viewModel = new(
            mowerService,
            new FakeClock(DateTimeOffset.Parse("2026-05-12T10:15:00+02:00")));

        await viewModel.LoadAsync(CancellationToken.None);

        viewModel.Cards.Should().ContainSingle();
        viewModel.StatusMessage.Should().Contain("1 robot");
        viewModel.RefreshInfo.Should().Contain("12/05/2026");
        viewModel.Cards[0].Id.Should().Be("mower-1");
        viewModel.Cards[0].Name.Should().Be("Pelouse");
        viewModel.Cards[0].StatusLine.Should().Contain("En tonte");
        viewModel.Cards[0].BatteryLine.Should().Contain("77");
        viewModel.Cards[0].ConnectivityLine.Should().Contain("en ligne");
    }

    [Fact]
    public async Task DashboardViewModel_LoadAsync_handles_empty_lists()
    {
        DashboardViewModel viewModel = new(
            new FakeMowerService([]),
            new FakeClock(DateTimeOffset.Parse("2026-05-12T10:15:00+02:00")));

        await viewModel.LoadAsync(CancellationToken.None);

        viewModel.Cards.Should().BeEmpty();
        viewModel.StatusMessage.Should().Contain("Aucun robot");
        viewModel.EmptyStateMessage.Should().Contain("Aucun robot");
    }

    [Fact]
    public async Task DashboardViewModel_LoadAsync_handles_api_errors()
    {
        DashboardViewModel viewModel = new(
            new FakeMowerService(OperationResult<IReadOnlyList<Mower>>.Failure(new ApplicationError(
                ApplicationErrorCode.ServiceUnavailable,
                "Service indisponible"))),
            new FakeClock(DateTimeOffset.Parse("2026-05-12T10:15:00+02:00")));

        await viewModel.LoadAsync(CancellationToken.None);

        viewModel.Cards.Should().BeEmpty();
        viewModel.ErrorMessage.Should().Contain("indisponible");
        viewModel.StatusMessage.Should().Contain("n'a pas pu");
    }

    [Fact]
    public async Task MowerDetailsViewModel_LoadAsync_populates_state_and_command_flags()
    {
        FakeDetailMowerService service = new(
            mower: new Mower
            {
                Id = "mower-1",
                Name = "Pelouse",
                Battery = new BatteryInfo { LevelPercent = 54 },
                Status = new MowerStatus
                {
                    State = MowerState.InOperation,
                    Activity = MowerActivity.Mowing,
                    Mode = MowerMode.MainArea,
                    Connected = true
                },
                Capabilities = new MowerCapabilities
                {
                    CanPause = true,
                    CanParkUntilNextSchedule = true,
                    CanParkUntilFurtherNotice = true,
                    CanResumeSchedule = false,
                    CanStartForDuration = true
                }
            });

        MowerDetailsViewModel viewModel = new(service);

        await viewModel.LoadAsync(CancellationToken.None);

        viewModel.HasMower.Should().BeTrue();
        viewModel.MowerName.Should().Be("Pelouse");
        viewModel.StateSummary.Should().Contain("En tonte");
        viewModel.CanPause.Should().BeTrue();
        viewModel.CanResumeSchedule.Should().BeFalse();
        viewModel.CanStartForDuration.Should().BeTrue();
        viewModel.CommandHint.Should().Contain("Pause");
    }

    [Fact]
    public async Task MowerDetailsViewModel_PauseAsync_sends_command_and_updates_feedback()
    {
        FakeDetailMowerService service = new(
            mower: new Mower
            {
                Id = "mower-1",
                Name = "Pelouse",
                Capabilities = new MowerCapabilities
                {
                    CanPause = true
                },
                Status = new MowerStatus
                {
                    State = MowerState.InOperation,
                    Activity = MowerActivity.Mowing,
                    Mode = MowerMode.MainArea,
                    Connected = true
                }
            });

        MowerDetailsViewModel viewModel = new(service)
        {
            SelectedDurationMinutes = 30
        };

        await viewModel.LoadAsync(CancellationToken.None);
        await viewModel.PauseAsync(CancellationToken.None);

        service.SentCommands.Should().ContainSingle();
        service.SentCommands[0].Type.Should().Be(MowerCommandType.Pause);
        viewModel.StatusMessage.Should().Contain("Commande envoyée");
    }

    [Fact]
    public async Task MowerDetailsViewModel_LoadAsync_with_a_specific_id_uses_that_robot()
    {
        FakeDetailMowerService service = new(
            mower: new Mower
            {
                Id = "mower-42",
                Name = "Haie",
                Capabilities = new MowerCapabilities
                {
                    CanPause = true
                }
            });

        MowerDetailsViewModel viewModel = new(service);

        await viewModel.LoadAsync(CancellationToken.None, "mower-42");

        service.RequestedIds.Should().ContainSingle().Which.Should().Be("mower-42");
        viewModel.MowerName.Should().Be("Haie");
    }

    private static LoginViewModel CreateLoginViewModel(
        ISecureTokenStore secureTokenStore,
        IApplicationSecretStore secretStore,
        IHusqvarnaAuthClient authClient,
        IOAuthCallbackListener callbackListener,
        IBrowserLauncher browserLauncher) =>
        new(
            new FakeAppSettingsStore(new AppSettings
            {
                ApplicationKey = "demo-key",
                RedirectUri = "http://localhost",
                RefreshIntervalSeconds = 60
            }),
            secretStore,
            secureTokenStore,
            authClient,
            callbackListener,
            browserLauncher);

    private sealed class FakeAppSettingsStore(AppSettings settings) : IAppSettingsStore
    {
        public int SaveCallCount { get; private set; }

        public AppSettings? SavedSettings { get; private set; }

        public Task<OperationResult<AppSettings>> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<AppSettings>.Success(settings));

        public Task<OperationResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            SaveCallCount++;
            SavedSettings = settings;
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class FakeApplicationSecretStore : IApplicationSecretStore
    {
        public FakeApplicationSecretStore(string? secret = null)
        {
            CurrentSecret = secret;
        }

        public int SaveCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public string? SavedSecret { get; private set; }

        public string? CurrentSecret { get; private set; }

        public Task<OperationResult<string?>> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<string?>.Success(CurrentSecret));

        public Task<OperationResult> SaveAsync(string applicationSecret, CancellationToken cancellationToken)
        {
            SaveCallCount++;
            SavedSecret = applicationSecret;
            CurrentSecret = applicationSecret;
            return Task.FromResult(OperationResult.Success());
        }

        public Task<OperationResult> DeleteAsync(CancellationToken cancellationToken)
        {
            DeleteCallCount++;
            CurrentSecret = null;
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class FakeSecureTokenStore(AuthSession session) : ISecureTokenStore
    {
        public int DeleteCallCount { get; private set; }

        public int SaveCallCount { get; private set; }

        public Task<OperationResult> SaveAsync(AuthSession session, CancellationToken cancellationToken)
        {
            SaveCallCount++;
            return Task.FromResult(OperationResult.Success());
        }

        public Task<OperationResult<AuthSession?>> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<AuthSession?>.Success(session));

        public Task<OperationResult> DeleteAsync(CancellationToken cancellationToken)
        {
            DeleteCallCount++;
            session = new AuthSession();
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class FakeAuthClient : IHusqvarnaAuthClient
    {
        public Uri AuthorizationUri { get; set; } = new("https://example.invalid");

        public OperationResult<AuthSession> ExchangeResult { get; set; } = OperationResult<AuthSession>.Failure(
            new ApplicationError(ApplicationErrorCode.Unknown, "Non utilisé"));

        public int RevokeCallCount { get; private set; }

        public Uri BuildAuthorizationUri(AppSettings settings, string state, string? codeChallenge) => AuthorizationUri;

        public Task<OperationResult<AuthSession>> ExchangeAuthorizationCodeAsync(
            string code,
            AppSettings settings,
            string clientSecret,
            CancellationToken cancellationToken) =>
            Task.FromResult(ExchangeResult);

        public Task<OperationResult<AuthSession>> RefreshAccessTokenAsync(
            AuthSession session,
            AppSettings settings,
            CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<AuthSession>.Success(session));

        public Task<OperationResult> RevokeAsync(string accessToken, CancellationToken cancellationToken)
        {
            RevokeCallCount++;
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class FakeOAuthCallbackListener : IOAuthCallbackListener
    {
        private readonly OperationResult<OAuthCallbackResult> result;

        public FakeOAuthCallbackListener()
        {
            result = OperationResult<OAuthCallbackResult>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Non utilisé"));
        }

        public FakeOAuthCallbackListener(OperationResult<OAuthCallbackResult> result)
        {
            this.result = result;
        }

        public Task<OperationResult<OAuthCallbackResult>> WaitForCallbackAsync(
            Uri redirectUri,
            string expectedState,
            CancellationToken cancellationToken)
        {
            if (result.IsSuccess && result.Value is not null)
            {
                return Task.FromResult(OperationResult<OAuthCallbackResult>.Success(result.Value with { State = expectedState }));
            }

            return Task.FromResult(result);
        }
    }

    private sealed class FakeBrowserLauncher : IBrowserLauncher
    {
        public List<Uri> OpenedUris { get; } = [];

        public Task<OperationResult> OpenAsync(Uri uri, CancellationToken cancellationToken)
        {
            OpenedUris.Add(uri);
            return Task.FromResult(OperationResult.Success());
        }
    }

    private sealed class FakeMowerService : IMowerService
    {
        private readonly OperationResult<IReadOnlyList<Mower>> listResult;

        public FakeMowerService(IReadOnlyList<Mower> mowers)
        {
            listResult = OperationResult<IReadOnlyList<Mower>>.Success(mowers);
        }

        public FakeMowerService(OperationResult<IReadOnlyList<Mower>> listResult)
        {
            this.listResult = listResult;
        }

        public Task<OperationResult<IReadOnlyList<Mower>>> GetMowersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(listResult);

        public Task<OperationResult<Mower>> GetMowerAsync(string mowerId, CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<Mower>.Failure(new ApplicationError(
                ApplicationErrorCode.NotFound,
                "Non utilisé dans ce test.")));

        public Task<OperationResult<CommandResult>> SendCommandAsync(
            string mowerId,
            MowerCommand command,
            CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<CommandResult>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Non utilisé dans ce test.")));
    }

    private sealed class FakeDetailMowerService : IMowerService
    {
        private readonly Mower mower;

        public FakeDetailMowerService(Mower mower)
        {
            this.mower = mower;
        }

        public List<string> RequestedIds { get; } = [];

        public List<MowerCommand> SentCommands { get; } = [];

        public Task<OperationResult<IReadOnlyList<Mower>>> GetMowersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<IReadOnlyList<Mower>>.Success([mower]));

        public Task<OperationResult<Mower>> GetMowerAsync(string mowerId, CancellationToken cancellationToken)
        {
            RequestedIds.Add(mowerId);
            return Task.FromResult(OperationResult<Mower>.Success(mower));
        }

        public Task<OperationResult<CommandResult>> SendCommandAsync(
            string mowerId,
            MowerCommand command,
            CancellationToken cancellationToken)
        {
            SentCommands.Add(command);
            return Task.FromResult(OperationResult<CommandResult>.Success(new CommandResult
            {
                Success = true,
                Command = command,
                Message = "Commande envoyée.",
                ShouldRefresh = false
            }));
        }
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
