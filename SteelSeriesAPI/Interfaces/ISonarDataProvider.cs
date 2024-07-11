using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Interfaces;

public interface ISonarDataProvider
{
    Mode GetMode();
}