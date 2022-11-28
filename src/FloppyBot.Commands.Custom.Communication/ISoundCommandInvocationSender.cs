using FloppyBot.Commands.Custom.Communication.Entities;

namespace FloppyBot.Commands.Custom.Communication;

public interface ISoundCommandInvocationSender
{
    void InvokeSoundCommand(SoundCommandInvocation invocation);
}

public interface ISoundCommandInvocationReceiver
{
    event SoundCommandInvokedDelegate SoundCommandInvoked;
}

public delegate void SoundCommandInvokedDelegate(object sender, SoundCommandInvocation args);
