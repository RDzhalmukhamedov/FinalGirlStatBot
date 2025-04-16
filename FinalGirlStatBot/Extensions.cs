using System.Linq;
using FinalGirlStatBot.DB.Domain;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinalGirlStatBot;

public static class Extensions
{
    public static T GetConfiguration<T>(this IServiceProvider serviceProvider)
        where T : class
    {
        var options = serviceProvider.GetService<IOptions<T>>();
        if (options is null)
            throw new ArgumentNullException(nameof(T));

        return options.Value;
    }

    public static InlineKeyboardButton[] ToButtons(this IBaseDomain[] gameObject)
    {
        return gameObject.Select(go => new InlineKeyboardButton(go.Name, go.Id.ToString())).ToArray();
    }
}
