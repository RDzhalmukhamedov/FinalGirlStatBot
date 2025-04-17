using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot;

public static class Shared
{
    public static class Text
    {
        public const string SelectGirl = "Выбрать 👩";
        public const string SelectGirlCallback = "sGirl";

        public const string SelectKiller = "Выбрать 🔪";
        public const string SelectKillerCallback = "sKiller";

        public const string SelectLocation = "Выбрать 🏠";
        public const string SelectLocationCallback = "sLocation";

        public const string RandomGirl = "Случ. 👩";
        public const string RandomGirlCallback = "rGirl";

        public const string RandomKiller = "Случ. 🔪";
        public const string RandomKillerCallback = "rKiller";

        public const string RandomLocation = "Случ. 🏠";
        public const string RandomLocationCallback = "rLocation";

        public const string Win = "🏆\nПобеда";
        public const string WinCallback = "win";

        public const string Lose = "❌\nПоражение";
        public const string LoseCallback = "lose";

        public const string Reset = "🔄\nСброс";
        public const string ResetCallback = "reset";

        public const string StartGame = "🎥 Начинаем съёмку!";
        public const string StartGameCallback = "start";

        public const string WriteResults = "✏ Отметить результат";
        public const string WriteResultsCallback = "write";

        public const string KillerStats = "По 🔪";
        public const string KillerStatsCallback = "killerStat";

        public const string LocationStats = "По 🏠";
        public const string LocationStatsCallback = "locationStat";

        public const string InitPrivateCallback = "init";

        public const string SelectGirlMessage = "👩 Выберите девушку:";
        public const string SelectKillerMessage = "🔪 Выберите убийцу:";
        public const string SelectLocationMessage = "🏠 Выберите локацию:";
        public const string WriteResultsMessage = "Отметьте результат партии:";
        public const string UHaveUnfinishedGameMessage = "У вас есть другой незавершенный фильм";
        public const string SelectionQuestionMessage = "Какой фильм будем снимать?";
        public const string GameResetedMessage = "Съёмки прерваны, статистика записана не будет!";
        public const string TotalGamesMessage = "Общее количество игр:";
        public const string WinPercentageMessage = "Процент побед:";
        public const string ShootEndedMessage = "Снято!";
        public const string WinCongratsMessage = "Поздравляем с победой!🎉";
        public const string LoseCongratsMessage = "Повезёт в следующий раз!";
        public const string KillerWinsMessage = "В этот раз победил убийца!";
        public const string SomethingWrongMessage = "Что-то пошло не так, попробуйте ещё раз";
    }

    public static class Buttons
    {
        public static readonly InlineKeyboardButton SelectGirl          = (Text.SelectGirl, Text.SelectGirlCallback);
        public static readonly InlineKeyboardButton SelectKiller        = (Text.SelectKiller, Text.SelectKillerCallback);
        public static readonly InlineKeyboardButton SelectLocation      = (Text.SelectLocation, Text.SelectLocationCallback);
        public static readonly InlineKeyboardButton RandomGirl          = (Text.RandomGirl, Text.RandomGirlCallback);
        public static readonly InlineKeyboardButton RandomKiller        = (Text.RandomKiller, Text.RandomKillerCallback);
        public static readonly InlineKeyboardButton RandomLocation      = (Text.RandomLocation, Text.RandomLocationCallback);

        public static readonly InlineKeyboardButton StartGame           = (Text.StartGame, Text.StartGameCallback);
        public static readonly InlineKeyboardButton WriteResults        = (Text.WriteResults, Text.WriteResultsCallback);

        public static readonly InlineKeyboardButton Win                 = (Text.Win, Text.WinCallback);
        public static readonly InlineKeyboardButton Lose                = (Text.Lose, Text.LoseCallback);
        public static readonly InlineKeyboardButton Reset               = (Text.Reset, Text.ResetCallback);

        public static readonly InlineKeyboardButton KillerStats         = (Text.KillerStats, Text.KillerStatsCallback);
        public static readonly InlineKeyboardButton KillerStatsMarked   = ($"•{Text.KillerStats}•", Text.KillerStatsCallback);
        public static readonly InlineKeyboardButton LocationStats       = (Text.LocationStats, Text.LocationStatsCallback);
        public static readonly InlineKeyboardButton LocationStatsMarked = ($"•{Text.LocationStats}•", Text.LocationStatsCallback);

        public static readonly InlineKeyboardButton[][] InitKeyboard =
        [
            [SelectGirl, SelectKiller, SelectLocation],
            [RandomGirl, RandomKiller, RandomLocation],
            [Reset]
        ];

        public static readonly InlineKeyboardButton[][] InitKeyboardReadyToStart =
        [
            [SelectGirl, SelectKiller, SelectLocation],
            [RandomGirl, RandomKiller, RandomLocation],
            [Reset, StartGame],
        ];

        public static readonly InlineKeyboardButton[][] ResultKeyboard =
        [
            [Lose, Reset, Win]
        ];

        public static readonly InlineKeyboardButton[][] StatsKeyboard =
        [
            [KillerStats, LocationStats]
        ];

        public static readonly InlineKeyboardButton[][] StatsKeyboardKiller =
        [
            [KillerStatsMarked, LocationStats]
        ];

        public static readonly InlineKeyboardButton[][] StatsKeyboardLocation =
        [
            [KillerStats, LocationStatsMarked]
        ];
    }
}
