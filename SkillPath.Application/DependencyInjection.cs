// Registers application handlers and use case services.
using Microsoft.Extensions.DependencyInjection;
using SkillPath.Application.Goals.Commands.CreateGoal;
using SkillPath.Application.Goals.Commands.DeleteGoal;
using SkillPath.Application.Goals.Commands.UpdateGoal;
using SkillPath.Application.Goals.Queries.GetGoalById;
using SkillPath.Application.Goals.Queries.ListGoals;

namespace SkillPath.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateGoalHandler>();
        services.AddScoped<UpdateGoalHandler>();
        services.AddScoped<DeleteGoalHandler>();
        services.AddScoped<GetGoalByIdHandler>();
        services.AddScoped<ListGoalsHandler>();

        return services;
    }
}
