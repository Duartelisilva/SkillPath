// Implements task generation using a local Ollama instance with robust error handling.
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SkillPath.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;

namespace SkillPath.Infrastructure.AI;

public sealed class OllamaTaskGenerator : ITaskGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaTaskGenerator> _logger;

    public OllamaTaskGenerator(HttpClient httpClient, ILogger<OllamaTaskGenerator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<GeneratedTask>> GenerateAsync(
        string skillName,
        string skillDescription,
        string goalTitle,
        CancellationToken cancellationToken)
    {
        var prompt = $$$"""
            You are a learning path expert. Generate concrete actionable tasks for the following skill.

            Overall Goal: {{{goalTitle}}}
            Skill: {{{skillName}}}
            Skill Description: {{{skillDescription}}}

            CRITICAL: Respond ONLY with a valid JSON array. No explanation, no markdown, no code blocks, no extra text.

            Each item must have exactly these fields:
            - "title": short task title (max 200 chars)
            - "description": what to do in this task (max 1000 chars)
            - "order": integer starting from 0

            IMPORTANT RULES:
            - Generate exactly 4-6 tasks
            - Tasks should be specific and actionable
            - Each task should take 30 minutes to 2 hours
            - Tasks should build on each other
            - Use simple, direct language

            Example format:
            [
              {"title": "Setup Environment", "description": "Install required tools", "order": 0},
              {"title": "First Exercise", "description": "Complete tutorial", "order": 1}
            ]

            RESPOND WITH ONLY THE JSON ARRAY NOW:
            """;

        try
        {
            var payload = new
            {
                model = "mistral",
                prompt,
                stream = false,
                options = new
                {
                    temperature = 0.7,
                    top_p = 0.9
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Generating tasks for skill: {Skill}", skillName);

            var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ollama returned error {StatusCode}: {Error}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var rawText = ollamaResponse?.Response ?? throw new InvalidOperationException("Empty response from Ollama.");

            _logger.LogInformation("Received task generation response, length: {Length}", rawText.Length);

            // Clean and validate the response
            var cleanedJson = CleanJsonResponse(rawText);

            if (!IsValidJson(cleanedJson))
            {
                _logger.LogError("Invalid JSON structure for tasks: {Json}", cleanedJson);
                throw new InvalidOperationException($"AI generated invalid task JSON for skill {skillName}");
            }

            var tasks = JsonSerializer.Deserialize<List<GeneratedTask>>(cleanedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tasks == null || tasks.Count == 0)
            {
                _logger.LogWarning("No tasks generated for skill: {Skill}", skillName);
                throw new InvalidOperationException($"AI failed to generate tasks for skill {skillName}");
            }

            // Validate task data
            foreach (var task in tasks)
            {
                if (string.IsNullOrWhiteSpace(task.Title))
                    throw new InvalidOperationException($"AI generated task with empty title for skill {skillName}");

                if (string.IsNullOrWhiteSpace(task.Description))
                    throw new InvalidOperationException($"AI generated task with empty description for skill {skillName}");
            }

            // Ensure correct ordering
            var orderedTasks = tasks.OrderBy(t => t.Order).ToList();
            for (int i = 0; i < orderedTasks.Count; i++)
            {
                orderedTasks[i] = orderedTasks[i] with { Order = i };
            }

            _logger.LogInformation("Successfully generated {Count} tasks for skill {Skill}", orderedTasks.Count, skillName);
            return orderedTasks;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed for tasks of skill: {Skill}", skillName);
            throw new InvalidOperationException($"AI generated invalid task format for skill {skillName}. Skipping tasks.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during task generation for skill: {Skill}", skillName);
            throw;
        }
    }

    private string CleanJsonResponse(string rawText)
    {
        // Remove markdown code blocks
        var cleaned = Regex.Replace(rawText, @"```(?:json)?\s*", "", RegexOptions.IgnoreCase);
        cleaned = cleaned.Replace("```", "");

        // Find the JSON array
        var arrayMatch = Regex.Match(cleaned, @"\[\s*\{.*\}\s*\]", RegexOptions.Singleline);
        if (arrayMatch.Success)
        {
            cleaned = arrayMatch.Value;
        }

        cleaned = cleaned.Trim();

        // Unescape if needed
        if (cleaned.StartsWith("\\[") || cleaned.Contains("\\\""))
        {
            cleaned = Regex.Unescape(cleaned);
        }

        // Remove leading/trailing non-JSON text
        if (!cleaned.StartsWith("["))
        {
            var firstBracket = cleaned.IndexOf('[');
            if (firstBracket >= 0)
                cleaned = cleaned.Substring(firstBracket);
        }

        if (!cleaned.EndsWith("]"))
        {
            var lastBracket = cleaned.LastIndexOf(']');
            if (lastBracket >= 0)
                cleaned = cleaned.Substring(0, lastBracket + 1);
        }

        return cleaned;
    }

    private bool IsValidJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }

    private sealed class OllamaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }
}