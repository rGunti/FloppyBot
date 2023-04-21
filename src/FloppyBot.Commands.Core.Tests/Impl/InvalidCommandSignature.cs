using FloppyBot.Commands.Core.Attributes;

namespace FloppyBot.Commands.Core.Tests.Impl;

[CommandHost]
public class InvalidCommandSignature
{
    [Command("string")]
    public string StringAsReturnType( /* no args are supported */
    )
    {
        return "this should compute";
    }

    [Command("int")]
    public int IntegerAsReturnType()
    {
        // This signature is not allowed
        return 42;
    }
}
