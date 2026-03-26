// Registers infrastructure services and persistence dependencies.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillPath.Application.Abstractions.AI;
using SkillPath.Application.Abstractions.Persistence;
using SkillPath.Infrastructure.AI;
using SkillPath.Infrastructure.Persistence;
using SkillPath.Infrastructure.Persistence.Repositories;

namespace SkillPath.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<ILearningTaskRepository, LearningTaskRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddHttpClient<ISkillTreeGenerator, OllamaSkillTreeGenerator>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434");
            client.Timeout = TimeSpan.FromMinutes(2); // local models can be slow
        });
        return services;
    }
}
