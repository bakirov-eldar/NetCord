﻿namespace NetCord
{
    public class Emoji : ClientEntity
    {
        private readonly JsonModels.JsonEmoji _jsonEntity;

        public override DiscordId? Id => _jsonEntity.Id;

        public string? Name => _jsonEntity.Name;

        public IReadOnlyDictionary<DiscordId, Role> AllowedRoles { get; }

        public User? Creator { get; }

        public bool? RequireColons => _jsonEntity.RequireColons;

        public bool? Managed => _jsonEntity.Managed;

        public bool? Animated => _jsonEntity.Animated;

        public bool? Available => _jsonEntity.Available;

        internal Emoji(JsonModels.JsonEmoji jsonEntity, BotClient client) : base(client)
        {
            _jsonEntity = jsonEntity;
            Creator = new(jsonEntity.Creator, client);
            AllowedRoles = jsonEntity.AllowedRoles.ToDictionaryOrEmpty(r => r.Id, r => new Role(r, client));
        }
    }
}
