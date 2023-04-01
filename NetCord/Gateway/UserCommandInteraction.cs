﻿using NetCord.JsonModels;
using NetCord.Rest;

namespace NetCord.Gateway;

public class UserCommandInteraction : ApplicationCommandInteraction
{
    public override UserCommandInteractionData Data { get; }

    public UserCommandInteraction(JsonInteraction jsonModel, Guild? guild, TextChannel? channel, RestClient client) : base(jsonModel, guild, channel, client)
    {
        Data = new(jsonModel.Data!, jsonModel.GuildId, client);
    }
}
