﻿using Microsoft.Extensions.Options;

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
}
