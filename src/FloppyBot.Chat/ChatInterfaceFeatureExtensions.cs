namespace FloppyBot.Chat;

public static class ChatInterfaceFeatureExtensions
{
    public static bool Supports(
        this ChatInterfaceFeatures features,
        ChatInterfaceFeatures feature)
    {
        return features.HasFlag(feature);
    }

    public static bool Supports(
        this IChatInterface chatInterface,
        ChatInterfaceFeatures feature)
    {
        return chatInterface.SupportedFeatures.Supports(feature);
    }
}
