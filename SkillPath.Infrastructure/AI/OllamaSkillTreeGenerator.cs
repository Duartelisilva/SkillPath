// Implements skill tree generation using a local Ollama instance with robust error handling.
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SkillPath.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;
using SkillPath.Application.Goals.Commands.GenerateSkillTree;

namespace SkillPath.Infrastructure.AI;

public sealed class OllamaSkillTreeGenerator : ISkillTreeGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaSkillTreeGenerator> _logger;

    public OllamaSkillTreeGenerator(HttpClient httpClient, ILogger<OllamaSkillTreeGenerator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<GeneratedSkill>> GenerateAsync(
        string goalTitle,
        string goalDescription,
        GenerateSkillTreeCommand parameters,
        CancellationToken cancellationToken)
    {
        var systemPrompt = BuildSystemPrompt(parameters);
        var userPrompt = BuildUserPrompt(goalTitle, goalDescription, parameters);

        var payload = new
        {
            model = "mistral",
            prompt = $"{systemPrompt}\n\n{userPrompt}",
            stream = false,
            options = new
            {
                temperature = parameters.Difficulty == DifficultyLevel.Advanced ? 0.8 : 0.7,
                top_p = 0.9
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending request to Ollama for goal: {Goal}", goalTitle);

            var response = await _httpClient.PostAsync("/api/generate", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ollama returned error {StatusCode}: {Error}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}: {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Raw Ollama response: {Response}", responseJson);

            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var rawText = ollamaResponse?.Response ?? throw new InvalidOperationException("Empty response from Ollama.");

            _logger.LogInformation("Received AI response, length: {Length}", rawText.Length);

            // Clean and validate the response
            var cleanedJson = CleanJsonResponse(rawText);

            _logger.LogDebug("Cleaned JSON: {Json}", cleanedJson);

            // Validate JSON structure before deserializing
            if (!IsValidJson(cleanedJson))
            {
                _logger.LogError("Invalid JSON structure after cleaning: {Json}", cleanedJson);
                throw new InvalidOperationException("AI generated invalid JSON. Please try regenerating.");
            }

            var generatedSkills = JsonSerializer.Deserialize<List<GeneratedSkill>>(cleanedJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? throw new InvalidOperationException("Failed to deserialize skill tree from Ollama response.");

            // Convert to GeneratedSkill
            generatedSkills = generatedSkills.Select(s => new GeneratedSkill
            {
                Name = s.Name,
                Description = s.Description,
                Order = s.Order
            }).ToList();

            // Validate and fix
            var validatedSkills = ValidateAndFixSkills(generatedSkills, goalTitle);

            _logger.LogInformation("Successfully generated {Count} skills", validatedSkills.Count);
            return validatedSkills;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed. Raw response might be malformed.");
            throw new InvalidOperationException("AI generated invalid response format. Please try regenerating the skill tree.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during skill generation");
            throw;
        }
    }

    private string CleanJsonResponse(string rawText)
    {
        // Remove markdown code blocks
        var cleaned = Regex.Replace(rawText, @"```(?:json)?\s*", "", RegexOptions.IgnoreCase);
        cleaned = cleaned.Replace("```", "");

        // Find the JSON array in the text (sometimes AI adds preamble)
        var arrayMatch = Regex.Match(cleaned, @"\[\s*\{.*\}\s*\]", RegexOptions.Singleline);
        if (arrayMatch.Success)
        {
            cleaned = arrayMatch.Value;
        }

        // Trim whitespace
        cleaned = cleaned.Trim();

        // Unescape if needed
        if (cleaned.StartsWith("\\[") || cleaned.Contains("\\\""))
        {
            cleaned = Regex.Unescape(cleaned);
        }

        // Remove any leading/trailing text that's not part of JSON
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

    private List<GeneratedSkill> ValidateAndFixSkills(List<GeneratedSkill> skills, string goalTitle)
    {
        if (skills == null || skills.Count == 0)
        {
            _logger.LogError("No skills generated for goal: {Goal}", goalTitle);
            throw new InvalidOperationException($"AI generated zero skills for goal {goalTitle}");
        }

        _logger.LogInformation("Validating {Count} skills for goal: {Goal}", skills.Count, goalTitle);

        // Validate each skill
        foreach (var skill in skills)
        {
            if (string.IsNullOrWhiteSpace(skill.Name))
            {
                _logger.LogError("Skill with empty name found for goal: {Goal}", goalTitle);
                throw new InvalidOperationException($"AI generated skill with empty name for goal {goalTitle}");
            }

            if (string.IsNullOrWhiteSpace(skill.Description))
            {
                _logger.LogError("Skill with empty description found for goal: {Goal}", goalTitle);
                throw new InvalidOperationException($"AI generated skill with empty description for goal {goalTitle}");
            }
        }

        // Fix ordering - ensure 0, 1, 2, 3...
        var fixedSkills = skills
            .OrderBy(s => s.Order)
            .Select((skill, index) => new GeneratedSkill
            {
                Name = skill.Name.Trim(),
                Description = skill.Description.Trim(),
                Order = index  // Force correct sequential ordering
            })
            .ToList();

        _logger.LogInformation("Fixed skill ordering for goal {Goal}: now {Count} skills with order 0-{Max}",
            goalTitle, fixedSkills.Count, fixedSkills.Count - 1);

        return fixedSkills;
    }

    private sealed class OllamaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }

    private string BuildSystemPrompt(GenerateSkillTreeCommand parameters)
    {
        var basePrompt = @"You are a learning path expert. Generate a structured skill tree for the following goal.
 
            Rules:
            1. Return ONLY valid JSON, no markdown, no explanations
            2. Each skill must have: name, description, order
            3. First skill (order 0) should be foundational";

        var difficultyGuidance = parameters.Difficulty switch
        {
            DifficultyLevel.Beginner => "\n- Use simple, foundational concepts\n- Break complex topics into small steps",
            DifficultyLevel.Advanced => "\n- Focus on advanced concepts and real-world applications\n- Include challenging projects",
            _ => "\n- Balance theory with practical application\n- Mix foundational and intermediate concepts"
        };

        var focusGuidance = parameters.Focus switch
        {
            TreeFocus.Breadth => $"\n- Create {parameters.MaxSkills} diverse parallel skills\n- Minimize dependencies",
            TreeFocus.Depth => $"\n- Create {parameters.MinSkills}-{parameters.MinSkills + 3} skills with deep chains\n- Sequential mastery",
            _ => $"\n- Create {parameters.MinSkills}-{parameters.MaxSkills} skills\n- Mix independent and dependent skills"
        };

        return $"{basePrompt}{difficultyGuidance}{focusGuidance}";
    }

    private string BuildUserPrompt(string goalTitle, string goalDescription, GenerateSkillTreeCommand parameters)
    {
        var prompt = $@"Goal: {goalTitle}
            Description: {goalDescription}
 
            Requirements:
            - Generate {parameters.MinSkills}-{parameters.MaxSkills} skills
            - Difficulty: {parameters.Difficulty}
            - Focus: {parameters.Focus}";

        if (!string.IsNullOrWhiteSpace(parameters.AdditionalContext))
        {
            prompt += $"\n\nAdditional Context: {parameters.AdditionalContext}";
        }

        return prompt;
    }
}