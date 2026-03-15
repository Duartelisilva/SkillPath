// Registers infrastructure services and persistence dependencies.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillPath.Application.Abstractions.Persistence;
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

        return services;
    }
}
