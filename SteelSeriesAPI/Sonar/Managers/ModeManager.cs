using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

namespace SteelSeriesAPI.Sonar.Managers;

public class ModeManager : IModeManager
{
    public Mode Get()
    {
        string mode = new HttpFetcher().Provide("mode").RootElement.ToString();
        
        return (Mode)ModeExtensions.FromDictKey(mode, ModeMapChoice.StreamDict);
    }

    public void Set(Mode mode)
    {
        new HttpFetcher().Put("mode/" + mode.ToDictKey(ModeMapChoice.StreamDict));
        Thread.Sleep(100); // Prevent bugs/freezes/crashes
    }
}