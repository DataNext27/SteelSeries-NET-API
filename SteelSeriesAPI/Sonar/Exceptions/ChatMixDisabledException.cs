namespace SteelSeriesAPI.Sonar.Exceptions;

public class ChatMixDisabledException : Exception
{
    public ChatMixDisabledException() : base("ChatMix is not enabled.") { }
    public ChatMixDisabledException(string message) : base(message) { }
    public ChatMixDisabledException(string message, Exception innerException) : base(message, innerException) { }
}