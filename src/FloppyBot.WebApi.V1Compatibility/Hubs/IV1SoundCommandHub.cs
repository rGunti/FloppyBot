using FloppyBot.WebApi.V1Compatibility.Dtos;

namespace FloppyBot.WebApi.V1Compatibility.Hubs;

public interface IV1SoundCommandHub
{
    Task InvokeSoundCommand(InvokeSoundCommandEvent soundCommandEvent);
}
