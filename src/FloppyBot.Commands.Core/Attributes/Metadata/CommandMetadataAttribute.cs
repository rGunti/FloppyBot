﻿namespace FloppyBot.Commands.Core.Attributes.Metadata;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class CommandMetadataAttribute : Attribute
{
    public CommandMetadataAttribute(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }
    public string Value { get; }
}