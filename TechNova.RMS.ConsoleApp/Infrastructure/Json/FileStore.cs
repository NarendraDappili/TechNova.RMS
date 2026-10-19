using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TechNova.RMS.ConsoleApp.Infrastructure.Json;

internal static class FileStore
{
    private static readonly object _lock = new();

    public static string BasePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TechNova.RMS");

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
        catch (System.Text.Json.JsonException)
        {
            // Malformed or incompatible JSON: treat as empty store so app can continue.
            return new List<T>();
        }
        catch (IOException)
        {
            // IO errors (locked file, etc): treat as empty store to allow the app to continue.
            return new List<T>();
        }
        catch (Exception)
        {
            // If any unexpected error occurs when reading/parsing the file, treat the store as empty
            // to allow the application to continue running. This prevents startup failure when the
            // storage file is corrupt, inaccessible, or otherwise unreadable.
            return new List<T>();
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
