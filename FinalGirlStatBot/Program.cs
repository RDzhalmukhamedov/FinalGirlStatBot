using FinalGirlStatBot;
using FinalGirlStatBot.Abstract;
using FinalGirlStatBot.DB;
using FinalGirlStatBot.DB.Abstract;
using FinalGirlStatBot.Services;
using FinalGirlStatBot.Services.UserActionHandlers;
using Telegram.Bot;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        //logging.ClearProviders();
        logging.SetMinimumLevel(LogLevel.Trace);
        //LogManager.Setup().LoadConfigurationFromAppSettings();
    })
    //.UseNLog()
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection(BotConfiguration.Configuration));
        services.Configure<DbConfiguration>(context.Configuration.GetSection(DbConfiguration.Configuration));
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                BotConfiguration botConfig = sp.GetConfiguration<BotConfiguration>();
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddScoped<TelegramUpdateHandler>();
        services.AddScoped<TelegramReceiverService>();
        services.AddScoped<IGameService, GameService>();

        services.AddSingleton<GameManager>();
        services.AddTransient<GameStateActionFactory>();

        services.AddHostedService<TelegramPollingService>();

        services.AddDbContext<FGStatsContext>();

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IGameRepository, GameRepository>();
        services.AddTransient<IGirlRepository, GirlRepository>();
        services.AddTransient<IKillerRepository, KillerRepository>();
        services.AddTransient<ILocationRepository, LocationRepository>();
        services.AddTransient<IFGStatsUnitOfWork, FGStatsUnitOfWork>();
    })
    .Build();

await host.RunAsync();
