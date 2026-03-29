// Implements skill tree generation using a local Ollama instance with dependency mapping.
using System.Text;
using System.Text.Json;
using SkillPath.Application.Abstractions.AI;

namespace SkillPath.Infrastructure.AI;

public sealed class OllamaSkillTreeGenerator : ISkillTreeGenerator
{
    private readonly HttpClient _httpClient;

    public OllamaSkillTreeGenerator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<GeneratedSkill>> GenerateAsync(
        string goalTitle,
        string goalDescription,
        IReadOnlyCollection<string> existingSkillNames,
        CancellationToken cancellationToken)
    {
        var existingSkillsSection = existingSkillNames.Count > 0
            ? $"The user already has these skills: {string.Join(", ", existingSkillNames)}. Do not include them."
            : string.Empty;

        var prompt = $"""
            You are a learning path expert. Generate a structured skill tree for the following goal.

            Goal: {goalTitle}
            Description: {goalDescription}
            {existingSkillsSection}

            Create a logical progression of skills where later skills build upon earlier ones.
            Each skill can depend on one or more previous skills (using their order numbers).

            Respond ONLY with a valid JSON array. No explanation, no markdown, no code blocks.
            Each item must have exactly these fields:
            - "name": short skill name (max 200 chars)
            - "description": what this skill covers (max 1000 chars)
            - "order": integer starting from 0, representing learning sequence
            - "dependsOn": array of order numbers this skill requires (e.g., [0, 1] means it depends on skills 0 and 1). First skill should have empty array [].

            IMPORTANT:
            - Generate between 5 and 8 skills maximum (CPU performance)
            - First skill (order 0) must have "dependsOn": []
            - Skills can only depend on skills with lower order numbers
            - Most skills should depend on 1-2 previous skills to create a clear learning path
            - Skills at the same level can share dependencies

            Return ONLY the JSON array.
            """;

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

        // Strip markdown code blocks if the model wrapped it anyway
        var cleaned = rawText
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        // Unescape if Mistral returned escaped JSON
        if (cleaned.StartsWith("\\[") || cleaned.Contains("\\\""))
            cleaned = System.Text.RegularExpressions.Regex.Unescape(cleaned);

        var skillsWithOrderDeps = JsonSerializer.Deserialize<List<SkillWithOrderDeps>>(cleaned,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialize skill tree from Ollama response.");

        // Convert order-based dependencies to actual skill data
        return skillsWithOrderDeps.Select(s => new GeneratedSkill
        {
            Name = s.Name,
            Description = s.Description,
            Order = s.Order
        }).ToArray();
    }

    // Internal class to parse the initial response with order-based dependencies
    private sealed class SkillWithOrderDeps
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int Order { get; init; }
        public List<int> DependsOn { get; init; } = new();
    }

    private sealed class OllamaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }
}