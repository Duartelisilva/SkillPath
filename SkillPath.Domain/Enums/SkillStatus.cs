// Defines the possible lifecycle states of a skill.
namespace SkillPath.Domain.Enums;

public enum SkillStatus
{
    Locked = 1,
    Available = 2,
    InProgress = 3,
    Completed = 4
}