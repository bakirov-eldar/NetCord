﻿using System.Text.Json.Serialization;

namespace NetCord.JsonModels;
internal record JsonUser : JsonEntity
{
    [JsonPropertyName("username")]
    public virtual string Username { get; init; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    [JsonPropertyName("discriminator")]
    public virtual ushort Discriminator { get; init; }

    [JsonPropertyName("avatar")]
    public virtual string? AvatarHash { get; init; }

    [JsonPropertyName("bot")]
    public virtual bool IsBot { get; init; }

    [JsonPropertyName("system")]
    public virtual bool? IsOfficialDiscordUser { get; init; }

    [JsonPropertyName("mfa_enabled")]
    public virtual bool? MFAEnabled { get; init; }

    [JsonPropertyName("locale")]
    public virtual string? Locale { get; init; }

    [JsonPropertyName("verified")]
    public virtual bool? Verified { get; init; }

    [JsonPropertyName("email")]
    public virtual string? Email { get; init; }

    [JsonPropertyName("flags")]
    public virtual UserFlags? Flags { get; init; }

    [JsonPropertyName("premium_type")]
    public virtual PremiumType? PremiumType { get; init; }

    [JsonPropertyName("public_flags")]
    public virtual UserFlags? PublicFlags { get; init; }
}
