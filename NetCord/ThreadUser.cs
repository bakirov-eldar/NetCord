﻿namespace NetCord
{
    public class ThreadUser : Entity
    {
        private readonly JsonModels.JsonThreadUser _jsonEntity;

        public override DiscordId Id => _jsonEntity.UserId;
        public DiscordId ThreadId => _jsonEntity.ThreadId;
        public DateTimeOffset JoinTimestamp => _jsonEntity.JoinTimestamp;
        public int Flags => _jsonEntity.Flags;

        internal ThreadUser(JsonModels.JsonThreadUser jsonEntity)
        {
            _jsonEntity = jsonEntity;
        }

        public override string ToString() => $"<@{Id}>";
    }
}