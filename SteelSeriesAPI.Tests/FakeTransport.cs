using System.Text.Json;
using SteelSeriesAPI.Core;

namespace SteelSeriesAPI.Tests;

/// <summary>Fake transport serving canned JSON responses and recording PUT routes.</summary>
internal sealed class FakeTransport : ISonarTransport
{
    private readonly Dictionary<string, string> _responses = new();

    /// <summary>Routes received by <see cref="PutAsync"/>, in call order.</summary>
    public List<string> PutRoutes { get; } = [];

    public FakeTransport With(string route, string json)
    {
        _responses[route] = json;
        return this;
    }

    public Task<JsonDocument> GetAsync(string route, CancellationToken ct = default) =>
        Task.FromResult(JsonDocument.Parse(_responses[route]));

    public Task PutAsync(string route, CancellationToken ct = default)
    {
        PutRoutes.Add(route);
        return Task.CompletedTask;
    }
}