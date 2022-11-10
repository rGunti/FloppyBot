using System.Collections.Immutable;
using AutoMapper;

namespace FloppyBot.Commands.Custom.Storage.Entities.Internal;

public class StringSetAndArrayConverter :
    ITypeConverter<IImmutableSet<string>, string[]>,
    ITypeConverter<string[], IImmutableSet<string>>
{
    public string[] Convert(IImmutableSet<string> source, string[] destination, ResolutionContext context)
    {
        return source.AsEnumerable()
            .OrderBy(i => i)
            .ToArray();
    }

    public IImmutableSet<string> Convert(string[] source, IImmutableSet<string> destination, ResolutionContext context)
    {
        return source
            .ToImmutableHashSet();
    }
}
