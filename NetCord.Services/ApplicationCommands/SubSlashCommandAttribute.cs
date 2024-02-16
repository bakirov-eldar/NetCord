﻿using System.Diagnostics.CodeAnalysis;

namespace NetCord.Services.ApplicationCommands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SubSlashCommandAttribute(string name, string description) : Attribute
{
    public string Name { get; } = name;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? NameTranslationsProviderType { get; init; }

    public string Description { get; } = description;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type? DescriptionTranslationsProviderType { get; init; }
}
