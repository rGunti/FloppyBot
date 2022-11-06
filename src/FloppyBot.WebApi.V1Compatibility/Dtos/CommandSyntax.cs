using System.Collections.Immutable;

namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CommandSyntax(
    string Syntax,
    string Purpose,
    IImmutableList<string> Examples);
