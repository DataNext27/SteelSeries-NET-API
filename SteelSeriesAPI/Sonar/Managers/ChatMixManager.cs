using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Http;

using System.Text.Json;
using System.Globalization;

namespace SteelSeriesAPI.Sonar.Managers;

internal class ChatMixManager : IChatMixManager
{
    public double GetBalance()
    {
        JsonDocument chatMix = new Fetcher().Provide("chatMix");

        return chatMix.RootElement.GetProperty("balance").GetDouble();
    }

    public bool GetState()
    {
        JsonDocument chatMix = new Fetcher().Provide("chatMix");
        string cState = chatMix.RootElement.GetProperty("state").ToString();
        
        if (cState == "enabled")
        {
            return true;
        }

        return false;
    }
    
    public void SetBalance(double balance)
    {
        if (!GetState())
        {
            throw new ChatMixDisabledException();
        }

        if (balance > 1 || balance < -1)
        {
            throw new ChatMixBalanceException();
        }

        new Fetcher().Put("chatMix?balance=" + balance.ToString("0.00", CultureInfo.InvariantCulture));
    }
}