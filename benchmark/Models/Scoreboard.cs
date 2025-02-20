namespace Benchmark.Models;

public class Scoreboard
{
    public Data? data { get; set; }
    public int DataVersion { get; set; }

    public class Data
    {
        public Dictionary<string, string>? DisplaySlots { get; set; }
        public List<Objective>? Objectives { get; set; }
        public List<PlayerScore>? PlayerScores { get; set; }
        public List<Team>? Teams { get; set; }
    }
    public struct Objective
    {
        public string? CriteriaName { get; set; }
        public string? DisplayName { get; set; }
        public string? Name { get; set; }
        public string? RenderType { get; set; }
    }
    public struct PlayerScore
    {
        public string? Name { get; set; }
        public string? Objective { get; set; }
        public int Score { get; set; }
        public byte Locked { get; set; }
    }
    public struct Team
    {
        public List<string>? Players { get; set; }
        public string? CollisionRule { get; set; }
        public string? DeathMessageVisibility { get; set; }
        public string? DisplayName { get; set; }
        public string? MemberNamePrefix { get; set; }
        public string? MemberNameSuffix { get; set; }
        public string? Name { get; set; }
        public string? NameTagVisibility { get; set; }
        public string? TeamColor { get; set; }
        public byte AllowFriendlyFire { get; set; }
        public byte SeeFriendlyInvisibles { get; set; }
    }
}