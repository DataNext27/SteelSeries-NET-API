namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

internal interface IChatMixManager
{
    /// <summary>
    /// Get the actual ChatMix balance value
    /// </summary>
    /// <returns>A double between -1 and 1</returns>
    double GetBalance();

    /// <summary>
    /// Get the actual state of the ChatMix
    /// </summary>
    /// <returns>True if ChatMix is enabled <br/> False if ChatMix is disabled</returns>
    bool GetState();
    
    /// <summary>
    /// Set the balance of the ChatMix
    /// </summary>
    /// <remarks>-1 to balance to Game channel<br/>1 to balance to Chat channel</remarks>
    /// <param name="balance">A <see cref="double"/> between -1 and 1</param>
    void SetBalance(double balance);
}