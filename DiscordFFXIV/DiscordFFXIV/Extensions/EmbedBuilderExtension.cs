﻿using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordFFXIV.Extensions
{
    class EmbedBuilderExtension
    {
        public static EmbedBuilder EmbedBuilder()
        {
            return null;
        }

        public static EmbedBuilder CustomEmbed(string Title, string description, string contentTitle, string content)
        {
            var builder = new EmbedBuilder();
            builder.WithTitle(Title);
            builder.WithDescription(description);
            if (contentTitle != null || content != null)
            {
                builder.AddInlineField(contentTitle, content);
            }
            builder.Timestamp = DateTimeOffset.Now;

            builder.WithColor(Color.DarkRed);
            return builder;
        }
    }
}
