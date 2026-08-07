using System.Text.Json;

namespace SteelSeriesAPI.Core;

/// <summary>Tolerant navigation helpers for <see cref="JsonElement"/>.</summary>
internal static class JsonExtensions
{
    /// <summary>
    /// Walks down a chain of JSON object properties, tolerating missing or null nodes.
    /// </summary>
    /// <exception cref="SonarResponseException">A node in the path is missing, null, or not an object.</exception>
    internal static JsonElement Dig(this JsonElement element, params string[] path)
    {
        JsonElement current = element;
        foreach (string key in path)
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(key, out current))
            {
                throw new SonarResponseException(
                    $"Expected JSON path '{string.Join('/', path)}' not found in Sonar response (missing at '{key}').");
            }
        }
        return current;
    }
}