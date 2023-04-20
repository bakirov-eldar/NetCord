﻿namespace NetCord.Services.Commands.TypeReaders;

public class PublicGuildThreadTypeReader<TContext> : ChannelTypeReader<TContext> where TContext : ICommandContext
{
    public override Task<object?> ReadAsync(ReadOnlyMemory<char> input, TContext context, CommandParameter<TContext> parameter, CommandServiceConfiguration<TContext> configuration)
    {
        var guild = context.Message.Guild;
        if (guild is not null)
            return Task.FromResult<object?>(GetGuildChannel<PublicGuildThread>(guild, input.Span));

        throw new EntityNotFoundException("The channel was not found.");
    }
}
