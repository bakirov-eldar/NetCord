﻿namespace NetCord.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string[] Aliases { get; }
        public int Priority { get; init; }
        public Context RequiredContext { get; init; }


        public CommandAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }
    }
}