// Registers application handlers and use case services.
using Microsoft.Extensions.DependencyInjection;
using SkillPath.Application.Goals.Commands.CreateGoal;
using SkillPath.Application.Goals.Commands.DeleteGoal;
using SkillPath.Application.Goals.Commands.GenerateSkillTree;
using SkillPath.Application.Goals.Commands.UpdateGoal;
using SkillPath.Application.Goals.Queries.GetGoalById;
using SkillPath.Application.Goals.Queries.ListGoals;
using SkillPath.Application.Skills.Commands.CreateSkill;
using SkillPath.Application.Skills.Commands.DeleteSkill;
using SkillPath.Application.Skills.Commands.UpdateSkill;
using SkillPath.Application.Skills.Queries.GetSkillById;
using SkillPath.Application.Skills.Queries.ListSkillsByGoal;
using SkillPath.Application.Tasks.Commands.CreateTask;
using SkillPath.Application.Tasks.Commands.DeleteTask;
using SkillPath.Application.Tasks.Commands.UpdateTask;
using SkillPath.Application.Tasks.Commands.UpdateTaskStatus;
using SkillPath.Application.Tasks.Queries.GetTaskById;
using SkillPath.Application.Tasks.Queries.ListTasksBySkill;

namespace SkillPath.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Goals
        services.AddScoped<CreateGoalHandler>();
        services.AddScoped<UpdateGoalHandler>();
        services.AddScoped<DeleteGoalHandler>();
        services.AddScoped<GetGoalByIdHandler>();
        services.AddScoped<ListGoalsHandler>();
        services.AddScoped<GenerateSkillTreeHandler>();

        // Skills
        services.AddScoped<CreateSkillHandler>();
        services.AddScoped<UpdateSkillHandler>();
        services.AddScoped<DeleteSkillHandler>();
        services.AddScoped<GetSkillByIdHandler>();
        services.AddScoped<ListSkillsByGoalHandler>();

        // Tasks
        services.AddScoped<CreateTaskHandler>();
        services.AddScoped<UpdateTaskHandler>();
        services.AddScoped<DeleteTaskHandler>();
        services.AddScoped<GetTaskByIdHandler>();
        services.AddScoped<ListTasksBySkillHandler>();
        services.AddScoped<UpdateTaskStatusHandler>();

        return services;
    }
}