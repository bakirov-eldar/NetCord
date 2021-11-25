﻿using NetCord.Commands;

namespace NetCord.Test;

public class NormalCommands : CommandModule
{
    [Command("say")]
    public Task Say([Remainder] string text)
    {
        return SendAsync(text);
    }

    [Command("reply")]
    public Task Reply([Remainder] string text)
    {
        return ReplyAsync(text);
    }

    [Command("roles")]
    public async Task Roles(params DiscordId[] roles)
    {
        if (Context.User is GuildUser guildUser)
        {
            await guildUser.ModifyAsync(p => p.NewRolesIds = guildUser.RolesIds.Concat(roles).Distinct());
            await ReplyAsync("Added the roles!!!");
        }
        else
        {
            await ReplyAsync("You are not in a guild");
        }
    }

    [Command("roles")]
    public async Task Roles()
    {
        if (Context.User is GuildUser user)
        {
            MessageBuilder message = new()
            {
                Content = "Select roles",
                Components = new()
            };
            var menu = CreateRolesMenu(Context.Guild.Roles.Values, user.RolesIds);
            message.Components.Add(menu);
            await SendAsync(message.Build());
        }
        else
            await ReplyAsync("Required context: Guild");
    }

    public static Menu CreateRolesMenu(IEnumerable<Role> guildRoles, IEnumerable<DiscordId> defaultValues)
    {
        var roles = guildRoles.OrderByDescending(r => r.Position).SkipLast(1);
        Menu menu = new("roles")
        {
            Placeholder = "Select roles",
            MaxValues = roles.Count(),
            MinValues = 0,
            Options = new()
        };
        foreach (var role in roles)
        {
            menu.Options.Add(new(role.Name, role.Id.ToString()) { IsDefault = defaultValues.Contains(role.Id) });
        }

        return menu;
    }

    [Command("ping")]
    public Task Ping() => ReplyAsync("Pong!");

    [Command("pong")]
    public Task Pong() => ReplyAsync("Ping!");

    [Command("timer")]
    public Task Timer(TimeSpan time)
    {
        Timestamp s = new(DateTimeOffset.UtcNow + time);
        return ReplyAsync($"{s.ToString(TimestampStyle.RelativeTime)} ({s.ToString(TimestampStyle.LongTime)})");
    }

    [Command("user")]
    public async Task User([Remainder] GuildUser user = null)
    {
        user ??= (GuildUser)Context.User;
        await ReplyAsync(user.Username);
    }

    [Command("avatar")]
    public Task Avatar([Remainder] GuildUser user = null)
    {
        user ??= (GuildUser)Context.User;
        MessageBuilder message = new()
        {
            Embeds = new(),
            MessageReference = new(Context.Message, false),
            AllowedMentions = new()
            {
                ReplyMention = false
            }
        };
        EmbedBuilder embed = new()
        {
            Title = $"Avatar of {user.Username}#{user.Discriminator}",
            ImageUrl = user.HasAvatar ? user.GetAvatarUrl(4096) : user.DefaultAvatarUrl,
            Color = new(0, 255, 0)
        };
        message.Embeds.Add(embed.Build());
        return SendAsync(message.Build());
    }
}