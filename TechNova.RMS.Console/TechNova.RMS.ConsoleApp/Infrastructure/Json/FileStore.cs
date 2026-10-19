using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TechNova.RMS.ConsoleApp.Infrastructure.Json;

internal static class FileStore
{
    private static readonly object _lock = new();

    public static string BasePath
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("TECHNOVA_RMS_DATA_DIR");
            if (!string.IsNullOrWhiteSpace(env)) return env!;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TechNova.RMS");
        }
    }

    private static readonly Dictionary<string,int> _guidToIntMap = new(StringComparer.OrdinalIgnoreCase);

    public static async Task<List<T>> LoadAsync<T>(string fileName)
    {
        Directory.CreateDirectory(BasePath);
        var path = Path.Combine(BasePath, fileName);
        if (!File.Exists(path)) return new List<T>();
        try
        {
            using var fs = File.OpenRead(path);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
            var data = await JsonSerializer.DeserializeAsync<List<T>>(fs, opts).ConfigureAwait(false);
            return data ?? new List<T>();
        }
        catch (Exception)
        {
            // Attempt to migrate older JSON that used GUID string ids to integer ids
            try
            {
                var text = await File.ReadAllTextAsync(path).ConfigureAwait(false);
                var root = JsonNode.Parse(text);
                if (root is null) throw new InvalidOperationException($"Failed to parse storage file for migration: {path}");

                JsonArray? arr = root as JsonArray;
                if (arr is null)
                {
                    // if root is single object, wrap it
                    if (root is JsonObject obj) arr = new JsonArray(obj);
                    else throw new InvalidOperationException($"Unexpected JSON structure in storage file: {path}");
                }

                // find current max numeric id in this array to continue numbering
                var maxId = 0;
                foreach (var item in arr)
                {
                    if (item is JsonObject jo && jo.TryGetPropertyValue("Id", out var idNode) && idNode is JsonValue idVal && idVal.TryGetValue<int>(out var numeric))
                    {
                        if (numeric > maxId) maxId = numeric;
                    }
                }

                foreach (var item in arr)
                {
                    if (item is not JsonObject obj) continue;
                    var props = obj.ToList(); // snapshot to avoid modifying during enumeration
                    foreach (var kv in props)
                    {
                        var prop = kv.Key;
                        var node = kv.Value;
                        if (!prop.Equals("Id", StringComparison.OrdinalIgnoreCase) && !prop.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) continue;
                        if (node is null) continue;
                        // if already numeric, skip
                        if (node is JsonValue v && v.TryGetValue<int>(out _)) continue;
                        // if string and GUID, map to int
                        if (node is JsonValue sv && sv.TryGetValue<string>(out var s) && Guid.TryParse(s, out _))
                        {
                            if (!_guidToIntMap.TryGetValue(s, out var mapped))
                            {
                                mapped = ++maxId;
                                _guidToIntMap[s] = mapped;
                            }
                            obj[prop] = mapped;
                        }
                    }
                }

                // write migrated content back atomically
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var tmp = path + ".migr";
                await File.WriteAllTextAsync(tmp, arr.ToJsonString(opts)).ConfigureAwait(false);
                File.Delete(path);
                File.Move(tmp, path);

                // deserialize migrated content
                var data = JsonSerializer.Deserialize<List<T>>(arr.ToJsonString(opts), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return data ?? new List<T>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read storage file: {path}", ex);
            }
        }
    }

    public static Task SaveAsync<T>(string fileName, List<T> items)
    {
        Directory.CreateDirectory(BasePath);
        var path = Path.Combine(BasePath, fileName);
        var temp = path + ".tmp";
        lock (_lock)
        {
            using var fs = File.Create(temp);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
            JsonSerializer.Serialize(fs, items, opts);
        }
        if (File.Exists(path)) File.Delete(path);
        File.Move(temp, path);
        return Task.CompletedTask;
    }
}
