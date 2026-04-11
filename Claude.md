# SkillPath - AI-Powered Learning Path Application

## Project Overview

**Purpose**: Portfolio project showcasing full-stack .NET/Angular development with AI integration for .NET/Angular developer roles.

**Tech Stack**:
- Backend: .NET 8, C#, Clean Architecture, EF Core, SQL Server
- Frontend: Angular 21, TypeScript, Cytoscape.js, RxJS, Signals
- AI: Ollama (Mistral) for skill tree and task generation
- Testing: xUnit, Jasmine/Karma

**What It Does**: Users create learning goals, and AI generates personalized skill trees with tasks. As users complete tasks, they earn XP and unlock new skills in a dependency-based progression system.

## Developer Info

**Primary Language**: C#
**Coding Style**: Clean Architecture, SOLID principles, domain-driven design
**Preferences**: 
- Detailed explanations for complex architectural decisions
- Code examples with inline comments for clarity
- Follow Microsoft C# naming conventions
- Use modern C# features (records, pattern matching, etc.)
- Angular standalone components (not NgModules)
- Signals over NgRx for simpler state (unless complexity demands otherwise)

## Project Structure

```
SkillPath/
├── src/
│   ├── SkillPath.Api/              # Web API project
│   ├── SkillPath.Application/      # Application layer (CQRS, handlers)
│   ├── SkillPath.Domain/           # Domain models, interfaces
│   ├── SkillPath.Infrastructure/   # Data access, external services
│   └── SkillPath.Web/              # Angular frontend
├── tests/
│   ├── SkillPath.Application.Tests/
│   ├── SkillPath.Domain.Tests/
│   └── SkillPath.Infrastructure.Tests/
├── docs/
│   ├── architecture.md
│   ├── ui_spec.md
│   └── api_spec.md
└── .claude/                        # Claude agent configurations
```

## Domain Model Overview

**Core Entities**:
- `Goal` - User's learning objective (e.g., "Learn Full-Stack Development")
- `Skill` - Individual skill in the learning path (e.g., "JavaScript Basics")
- `Task` - Actionable item to complete a skill (e.g., "Build a to-do app")
- `User` - Application user (future: authentication)

**Key Relationships**:
- Goal → Skills (one-to-many)
- Skill → Skills (dependencies, forms a DAG)
- Skill → Tasks (one-to-many)
- Task completion unlocks dependent skills when all tasks in a skill are complete

**Business Rules**:
- A skill can only be unlocked if all prerequisite skills are completed
- Each task awards 10-30 XP
- A skill is "completed" when all its tasks are marked complete
- Total XP for a skill ≈ 100 XP (sum of all task XP values)
- AI generates 5-7 skills per goal, 4-6 tasks per skill

## Common Development Workflows

### Running the Application
```bash
# Backend
cd src/SkillPath.Api
dotnet run

# Frontend
cd src/SkillPath.Web
npm start
```

### Database Migrations
```bash
cd src/SkillPath.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../SkillPath.Api
dotnet ef database update --startup-project ../SkillPath.Api
```

### Running Tests
```bash
# All tests
dotnet test

# Specific project
dotnet test tests/SkillPath.Application.Tests/
```

## AI Integration Details

**Ollama Setup**: 
- Local Mistral model running on http://localhost:11434
- Prompts structured to return JSON with skill tree data
- Retry logic with exponential backoff for timeouts

**Prompt Structure** (simplified):
```
Generate a learning path for: {goalTitle}
Skill level: {beginner|intermediate|advanced}

Return JSON:
{
  "skills": [
    {
      "title": "Skill name",
      "description": "What this skill covers",
      "dependencies": [0], // indices of prerequisite skills
      "tasks": [
        {
          "title": "Task name",
          "description": "Detailed task description",
          "xp": 15
        }
      ]
    }
  ]
}
```

## Known Challenges & Solutions

**Challenge**: Cytoscape layout for sequential dependencies
**Solution**: Use `breadthfirst` or `cola` layout with `directed: true` and proper edge direction

**Challenge**: XP "zeroes out" when skill completes
**Current**: XP resets to 0 (probably a bug)
**Desired**: XP should persist, just mark skill as complete

**Challenge**: Task regeneration while preserving completed tasks
**Solution**: Only regenerate incomplete tasks, keep completed task XP

## Portfolio Goals

This project demonstrates:
1. **Clean Architecture** - Clear separation of concerns, dependency inversion
2. **CQRS Pattern** - Commands and queries separated with MediatR
3. **Domain-Driven Design** - Rich domain models with business logic
4. **Modern .NET** - C# 12, .NET 8 features, async/await patterns
5. **Angular Best Practices** - Standalone components, Signals, reactive patterns
6. **AI Integration** - Practical LLM use case with error handling
7. **Full-Stack Skills** - End-to-end feature implementation

## Documentation Locations

- **UI Specifications**: `/docs/ui_spec.md`
- **API Documentation**: `/docs/api_spec.md` (to be created)
- **Architecture Decisions**: `/docs/architecture.md` (to be created)
- **Agent Configurations**: `/.claude/AGENT_*.md`

## Current Status (as of today)

**Completed**:
- ✅ Basic goal, skill, task entities
- ✅ AI skill tree generation via Ollama
- ✅ Cytoscape visualization (sequential dependencies)
- ✅ Task panel with XP badges
- ✅ Task completion and regeneration
- ✅ UI mockups and design system

**In Progress**:
- Setting up optimal Claude project structure
- Defining feature roadmap for portfolio readiness

**Next Steps**:
1. Fix XP zeroing bug
2. Implement dashboard with multiple goals
3. Add authentication & authorization
4. Build analytics/progress page
5. Add comprehensive testing
6. Docker deployment setup

## How to Work with Claude on This Project

**For Backend Work**: Tag `@AGENT_BACKEND` for C# architecture, domain logic, API endpoints
**For Frontend Work**: Tag `@AGENT_FRONTEND` for Angular components, services, UI logic
**For AI Integration**: Tag `@AGENT_AI` for Ollama prompts, parsing, error handling
**For Code Review**: Tag `@AGENT_REVIEW` to check SOLID principles, security, patterns

**General Guidance**:
- Always consider Clean Architecture boundaries (don't let Infrastructure leak into Domain)
- Follow SOLID principles (especially SRP and DIP)
- Write unit tests for business logic
- Use meaningful variable/method names
- Add XML comments for public APIs
- Handle errors gracefully (never swallow exceptions)

## Questions to Ask When Starting a Feature

1. Which layer does this belong in? (Domain/Application/Infrastructure/API)
2. Is this a Command or Query? (CQRS)
3. What are the business rules? (Domain logic)
4. What validation is needed? (FluentValidation)
5. What could go wrong? (Error handling)
6. How will we test this? (Unit/integration tests)

---

**Last Updated**: April 4, 2026
**Project Timeline**: 3 months to portfolio-ready
**Target Audience**: .NET/Angular hiring managers and technical interviewers
