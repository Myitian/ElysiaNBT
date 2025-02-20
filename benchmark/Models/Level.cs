namespace Benchmark.Models;

public class Level
{
    public Data? Data { get; set; }
}
public class Data
{
    public object? CustomBossEvents { get; set; }
    public DataPacks? DataPacks { get; set; }
    public DragonFight? DragonFight { get; set; }
    public Dictionary<string, string>? GameRules { get; set; }
    public Player? Player { get; set; }
    public Version? Version { get; set; }
    public WorldGenSettings? WorldGenSettings { get; set; }
    public List<object>? ScheduledEvents { get; set; }
    public List<string>? ServerBrands { get; set; }
    public string? LevelName { get; set; }
    public double BorderCenterX { get; set; }
    public double BorderCenterZ { get; set; }
    public double BorderDamagePerBlock { get; set; }
    public double BorderSafeZone { get; set; }
    public double BorderSize { get; set; }
    public double BorderSizeLerpTarget { get; set; }
    public double BorderWarningBlocks { get; set; }
    public double BorderWarningTime { get; set; }
    public float SpawnAngle { get; set; }
    public long BorderSizeLerpTime { get; set; }
    public long DayTime { get; set; }
    public long LastPlayed { get; set; }
    public long Time { get; set; }
    public int DataVersion { get; set; }
    public int GameType { get; set; }
    public int SpawnX { get; set; }
    public int SpawnY { get; set; }
    public int SpawnZ { get; set; }
    public int WanderingTraderSpawnChance { get; set; }
    public int WanderingTraderSpawnDelay { get; set; }
    public int clearWeatherTime { get; set; }
    public int rainTime { get; set; }
    public int thunderTime { get; set; }
    public long version { get; set; }
    public byte Difficulty { get; set; }
    public byte DifficultyLocked { get; set; }
    public byte WasModded { get; set; }
    public byte allowCommands { get; set; }
    public byte hardcore { get; set; }
    public byte initialized { get; set; }
    public byte raining { get; set; }
    public byte thundering { get; set; }
}

public class DataPacks
{
    public List<string>? Disabled { get; set; }
    public List<string>? Enabled { get; set; }
}
public class DragonFight
{
    public int[]? Gateways { get; set; }
    public byte DragonKilled { get; set; }
    public byte NeedsStateScanning { get; set; }
    public byte PreviouslyKilled { get; set; }
}
public class Player
{
    public List<Item>? EnderItems { get; set; }
    public List<Item>? Inventory { get; set; }
    public List<double>? Motion { get; set; }
    public List<double>? Pos { get; set; }
    public List<float>? Rotation { get; set; }
    public List<Attribute>? attributes { get; set; }
    public List<double>? current_explosion_impact_pos { get; set; }
    public int[]? UUID { get; set; }
    public string? Dimension { get; set; }
    public float AbsorptionAmount { get; set; }
    public float FallDistance { get; set; }
    public float Health { get; set; }
    public float XpP { get; set; }
    public float foodExhaustionLevel { get; set; }
    public float foodSaturationLevel { get; set; }
    public int DataVersion { get; set; }
    public int HurtByTimestamp { get; set; }
    public int PortalCooldown { get; set; }
    public int Score { get; set; }
    public int SelectedItemSlot { get; set; }
    public int XpLevel { get; set; }
    public int XpSeed { get; set; }
    public int XpTotal { get; set; }
    public int current_impulse_context_reset_grace_time { get; set; }
    public int foodLevel { get; set; }
    public int foodTickTimer { get; set; }
    public int playerGameType { get; set; }
    public short Air { get; set; }
    public short DeathTime { get; set; }
    public short Fire { get; set; }
    public short HurtTime { get; set; }
    public short SleepTimer { get; set; }
    public byte FallFlying { get; set; }
    public byte Invulnerable { get; set; }
    public byte OnGround { get; set; }
    public byte ignore_fall_damage_from_current_explosion { get; set; }
    public byte seenCredits { get; set; }
    public byte spawn_extra_particles_on_fall { get; set; }

    public class Attribute
    {
        public string? id { get; set; }
        public double @base { get; set; }
    }
    public class Item
    {
        public string? id { get; set; }
        public int count { get; set; }
        public byte Slot { get; set; }
    }
}
public class Version
{
    public string? Name { get; set; }
    public string? Series { get; set; }
    public int Id { get; set; }
    public byte Snapshot { get; set; }
}
public class WorldGenSettings
{

}