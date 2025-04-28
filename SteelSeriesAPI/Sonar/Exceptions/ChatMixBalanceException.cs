namespace SteelSeriesAPI.Sonar.Exceptions;

public class ChatMixBalanceException : Exception
{
    public ChatMixBalanceException() : base("ChatMix balance out of range.") { }
    public ChatMixBalanceException(string message) : base(message) { }
    public ChatMixBalanceException(string message, Exception innerException) : base(message, innerException) { }
}