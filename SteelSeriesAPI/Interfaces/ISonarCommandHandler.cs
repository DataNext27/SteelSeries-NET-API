using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Interfaces;

public interface ISonarCommandHandler
{
    void SetMode(Mode mode);
}