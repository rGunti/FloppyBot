﻿using System.Collections.Immutable;
using System.Text.Json;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;

namespace FloppyBot.Commands.Core.Tests.Impl;

[CommandHost]
public class SampleCommands
{
    public enum SampleEnum
    {
        A,
        B,
        C,
    }

    [Command("ping")]
    public ChatMessage? Ping(CommandInstruction instruction)
    {
        return instruction.CreateReply("Pong!");
    }

    [Command("sping")]
    public static ChatMessage? StaticPing(CommandInstruction instruction)
    {
        return instruction.CreateReply("Pong, but static!");
    }

    [Command("simple")]
    public string Simple(CommandInstruction _)
    {
        return "Simple Response";
    }

    [Command("noargs")]
    public static string NoArgsCommand()
    {
        return "No args at all";
    }

    [Command("args")]
    public static string ArgsCommand(
        [ArgumentRange(1, 3)] string arg1,
        [ArgumentIndex(0)] string arg0)
    {
        return JsonSerializer.Serialize(new
        {
            arg0,
            arg1,
        });
    }

    [Command("add")]
    public static string Add(
        [ArgumentIndex(0)] int a,
        [ArgumentIndex(1)] int b)
    {
        return $"{a + b}";
    }

    [Command("list")]
    public static string List(
        [ArgumentRange(0, outputAsArray: true)]
        IImmutableList<string> list)
    {
        return string.Join(",", list);
    }

    [Command("allargs")]
    public static string AllArgs(
        [AllArguments] IImmutableList<string> allArgs)
    {
        return string.Join("/", allArgs);
    }

    [Command("enum")]
    public static string Enum(
        [ArgumentIndex(0)] SampleEnum sampleEnum)
    {
        return $"Enum value was: {sampleEnum}";
    }
}
