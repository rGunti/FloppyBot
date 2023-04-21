namespace FloppyBot.Commands.Aux.Translation.Exceptions;

public class TranslationException : Exception
{
    public TranslationException(string message)
        : base(message) { }
}
