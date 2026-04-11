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
        int requiredXP,
        CancellationToken cancellationToken)
    {
        var prompt = $$$"""
            You are a learning path expert. Generate concrete actionable tasks for the following skill.

            Overall Goal: {{{goalTitle}}}
            Skill: {{{skillName}}}
            Skill Description: {{{skillDescription}}}

            CRITICAL RULES - FOLLOW EXACTLY:
            1. Respond with ONLY a JSON array - no text before or after
            2. Do NOT use markdown code blocks (no ```json or ```)
            3. Start order at 0 and increment by 1 (must be: 0, 1, 2, 3, 4, 5)
            4. Generate exactly 5-7 tasks
            5. Each task should take 30-120 minutes

            Required JSON structure (COPY THIS FORMAT EXACTLY):
            [
              {"title": "First task name", "description": "What to do first", "order": 0},
              {"title": "Second task name", "description": "What to do second", "order": 1},
              {"title": "Third task name", "description": "What to do third", "order": 2}
            ]

            MANDATORY: First task must have "order": 0
            MANDATORY: Each subsequent task increments order by 1

            Return ONLY the JSON array starting with [ and ending with ]
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

            // Validate and auto-fix any ordering or data issues
            var validatedTasks = ValidateAndFixTasks(tasks ?? new List<GeneratedTask>(), skillName, requiredXP);

            _logger.LogInformation("Successfully generated {Count} tasks for skill {Skill}",
                validatedTasks.Count, skillName);

            return validatedTasks;
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

    private List<GeneratedTask> ValidateAndFixTasks(List<GeneratedTask> tasks, string skillName, int requiredXP)
    {
        if (tasks == null || tasks.Count == 0)
        {
            _logger.LogError("No tasks generated for skill: {Skill}", skillName);
            throw new InvalidOperationException($"AI generated zero tasks for skill {skillName}");
        }

        _logger.LogInformation("Validating {Count} tasks for skill: {Skill}", tasks.Count, skillName);

        // Validate each task has required fields
        foreach (var task in tasks)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                _logger.LogError("Task with empty title found for skill: {Skill}", skillName);
                throw new InvalidOperationException($"AI generated task with empty title for skill {skillName}");
            }

            if (string.IsNullOrWhiteSpace(task.Description))
            {
                _logger.LogError("Task with empty description found for skill: {Skill}", skillName);
                throw new InvalidOperationException($"AI generated task with empty description for skill {skillName}");
            }
        }

        // Assign random but well-distributed XP values
        var xpValues = GenerateDistributedXP(tasks.Count, requiredXP);

        // CRITICAL: Fix ordering issues
        // Sort by current order, then reassign 0, 1, 2, 3...
        var fixedTasks = tasks
            .OrderBy(t => t.Order)
            .Select((task, index) => new GeneratedTask
            {
                Title = task.Title.Trim(),
                Description = task.Description.Trim(),
                Order = index,  // Force correct sequential ordering starting from 0
                ExperiencePoints = xpValues[index]
            })
            .ToList();
        var totalXP = fixedTasks.Sum(t => t.ExperiencePoints);
        _logger.LogInformation("Fixed task ordering for skill {Skill}: now {Count} tasks with order 0-{Max}, Total XP: {TotalXP}",
            skillName, fixedTasks.Count, fixedTasks.Count - 1, totalXP);

        return fixedTasks;
    }

    private List<int> GenerateDistributedXP(int taskCount, int requiredXP)
    {
        if (taskCount == 0)
            return new List<int>();

        var xpValues = new List<int>();

        // Target total XP pool larger than completion requirement
        var totalXPBudget = requiredXP * 1.6; // allows skipping tasks


        // Descending XP curve (earlier tasks worth more)
        var weightSum = Enumerable.Range(1, taskCount).Sum();

        for (int i = 0; i < taskCount; i++)
        {
            var weight = taskCount - i;
            var xp = (int)Math.Round(
                (double)weight / weightSum * totalXPBudget);

            xpValues.Add(Math.Max(15, xp));
        }

        _logger.LogInformation(
            "Generated weighted XP distribution: [{XP}], Total: {Total}",
            string.Join(", ", xpValues),
            xpValues.Sum());

        return xpValues;
    }

    private sealed class OllamaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }
}