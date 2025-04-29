using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces.Managers;

namespace SteelSeriesAPI.Sonar.Managers;

internal class ModeManager : IModeManager
{
    public Mode Get()
    {
        string mode = new Fetcher().Provide("mode").RootElement.ToString();
        
        return (Mode)ModeExtensions.FromDictKey(mode, ModeMapChoice.StreamDict)!;
    }

    public void Set(Mode mode)
    {
        new Fetcher().Put("mode/" + mode.ToDictKey(ModeMapChoice.StreamDict));
        Thread.Sleep(100); // Prevent bugs/freezes/crashes
    }
}