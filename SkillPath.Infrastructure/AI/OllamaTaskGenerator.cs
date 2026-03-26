// Implements task generation using a local Ollama instance.
using System.Text;
using System.Text.Json;
using SkillPath.Application.Abstractions.AI;

namespace SkillPath.Infrastructure.AI;

public sealed class OllamaTaskGenerator : ITaskGenerator
{
    private readonly HttpClient _httpClient;

    public OllamaTaskGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<GeneratedTask>> GenerateAsync(
        string skillName,
        string skillDescription,
        string goalTitle,
        CancellationToken cancellationToken)
    {
        var prompt =
            "You are a learning path expert. Generate concrete actionable tasks for the following skill.\n\n" +
            $"Overall Goal: {goalTitle}\n" +
            $"Skill: {skillName}\n" +
            $"Skill Description: {skillDescription}\n\n" +
            "Respond ONLY with a valid JSON array. No explanation, no markdown, no code blocks.\n" +
            "Each item must have exactly these fields:\n" +
            "- \"title\": short task title (max 200 chars)\n" +
            "- \"description\": what to do in this task (max 1000 chars)\n" +
            "- \"order\": integer starting from 0\n\n" +
            "Generate between 3 and 6 tasks. Return ONLY the JSON array.";

        var payload = new
        {
            model = "mistral",
            prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}: {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var rawText = ollamaResponse?.Response ?? throw new InvalidOperationException("Empty response from Ollama.");

        var cleaned = rawText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        if (cleaned.StartsWith("\\[") || cleaned.Contains("\\\""))
            cleaned = System.Text.RegularExpressions.Regex.Unescape(cleaned);

        return JsonSerializer.Deserialize<List<GeneratedTask>>(cleaned,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialize tasks from Ollama response.");
    }

    private sealed class OllamaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }
}