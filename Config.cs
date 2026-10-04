using System.IO;
using System.Reflection;
using System.Text.Json;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigField : Attribute
{
    public object DefaultValue { get; }

    public ConfigField(object defaultValue)
    {
        DefaultValue = defaultValue;
    }
}

public static class Config
{
    private static Dictionary<string, object> _cache = new();

    private static string GetConfigPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "ransomconfig.json");
    }

    public static bool ConfigExists()
    {
        return File.Exists(GetConfigPath());
    }

    public static bool LoadConfig()
    {
        if (ConfigExists())
        {
            try
            {
                string json = File.ReadAllText(GetConfigPath());
                _cache = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
                return true;
            }
            catch { }
        }
        return false;
    }

    private static T GetOrDefault<T>(string propertyName)
    {
        if (_cache.TryGetValue(propertyName, out object? value))
        {
            try
            {
                if (value is JsonElement element)
                    return ConvertJsonElement<T>(element);

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch { }
        }

        PropertyInfo prop = typeof(Config).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        ConfigField attr = prop?.GetCustomAttribute<ConfigField>();

        if (attr != null)
            return (T)Convert.ChangeType(attr.DefaultValue, typeof(T));

        throw new KeyNotFoundException($"Config key '{propertyName}' not found");
    }

    // Dictionary<string,object> values deserialize as JsonElement, not the raw type
    private static T ConvertJsonElement<T>(JsonElement element)
    {
        return element.Deserialize<T>();
    }

    public static void SaveConfig()
    {
        Dictionary<string, object> configToSave = new Dictionary<string, object>();

        PropertyInfo[] properties = typeof(Config).GetProperties(
            BindingFlags.Public | BindingFlags.Static);

        foreach (PropertyInfo prop in properties)
        {
            ConfigField attr = prop.GetCustomAttribute<ConfigField>();
            if (attr != null)
            {
                object value = _cache.ContainsKey(prop.Name) ? _cache[prop.Name] : attr.DefaultValue;
                configToSave[prop.Name] = value;
            }
        }

        string json = JsonSerializer.Serialize(configToSave, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetConfigPath(), json);
        _cache = configToSave;
    }


    // Config properties with automatic setter
    // ------------------- Behavior ----------------------
    [ConfigField(true)]
    public static bool SpawnAutomatically
    {
        get => GetOrDefault<bool>(nameof(SpawnAutomatically));
        set { _cache[nameof(SpawnAutomatically)] = value; }
    }

    [ConfigField(false)]
    public static bool a90Mode
    {
        get => GetOrDefault<bool>(nameof(a90Mode));
        set { _cache[nameof(a90Mode)] = value; }
    }

    [ConfigField(5)]
    public static int MinSpawnDelay
    {
        get => GetOrDefault<int>(nameof(MinSpawnDelay));
        set { _cache[nameof(MinSpawnDelay)] = value; }
    }

    [ConfigField(600)]
    public static int MaxSpawnDelay
    {
        get => GetOrDefault<int>(nameof(MaxSpawnDelay));
        set { _cache[nameof(MaxSpawnDelay)] = value; }
    }

    [ConfigField(90)]
    public static int InfectionDuration
    {
        get => GetOrDefault<int>(nameof(InfectionDuration));
        set { _cache[nameof(InfectionDuration)] = value; }
    }

    // ------------------- Death -------------------

    [ConfigField(false)]
    public static bool CrashOnDeath
    {
        get => GetOrDefault<bool>(nameof(CrashOnDeath));
        set { _cache[nameof(CrashOnDeath)] = value; }
    }

    [ConfigField(false)]
    public static bool ExecCMDOnDeath
    {
        get => GetOrDefault<bool>(nameof(ExecCMDOnDeath));
        set { _cache[nameof(ExecCMDOnDeath)] = value; }
    }

    [ConfigField("shutdown /s /t 0")]
    public static string CMDOnDeath
    {
        get => GetOrDefault<string>(nameof(CMDOnDeath));
        set { _cache[nameof(CMDOnDeath)] = value; }
    }

    // ------------------ Items -------------------

    [ConfigField(false)]
    public static bool UseDrawerMode
    {
        get => GetOrDefault<bool>(nameof(UseDrawerMode));
        set { _cache[nameof(UseDrawerMode)] = value; }
    }

    [ConfigField(500)]
    public static int RansomAmount // Gold needed to pay the ransom
    {
        get => GetOrDefault<int>(nameof(RansomAmount));
        set { _cache[nameof(RansomAmount)] = value; }
    }

    [ConfigField(10)]
    public static int CrucChance
    {
        get => GetOrDefault<int>(nameof(CrucChance));
        set { _cache[nameof(CrucChance)] = value; }
    }

    [ConfigField(25)]
    public static int HoneypotChance
    {
        get => GetOrDefault<int>(nameof(HoneypotChance));
        set { _cache[nameof(HoneypotChance)] = value; }
    }
}