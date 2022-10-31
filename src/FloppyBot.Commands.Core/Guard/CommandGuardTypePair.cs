namespace FloppyBot.Commands.Core.Guard;

internal record CommandGuardTypePair(
    Type AttributeType,
    Type ImplementationType);
