#nullable enable
using System;
using System.IO;
using J2N.Collections.Generic;

namespace Lucene.Net.Analysis.OpenNlp.Upstream.Support;

internal class Properties : Dictionary<object, object>
{
    public string? GetProperty(string key)
    {
        if (TryGetValue(key, out object? value) && value is string s)
        {
            return s;
        }
        return null;
    }

    public string? GetProperty(string key, string? defaultValue)
    {
        if (TryGetValue(key, out object? value) && value is string s)
        {
            return s;
        }
        return defaultValue;
    }

    public void Load(Stream s)
    {
        using var reader = new StreamReader(s);
        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            if (line.Length == 0 || // LUCENENET: string.StartsWith(char) is net5.0+.
                line.StartsWith("#", StringComparison.Ordinal))
                continue;

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex > 0)
            {
                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();
                this[key] = value;
            }
        }
    }

    public object? SetProperty(string key, string value)
    {
        var oldValue = ContainsKey(key) ? this[key] : null;
        this[key] = value;
        return oldValue;
    }

    public void Store(Stream s, string? comments)
    {
        using var writer = new StreamWriter(s);
        if (!string.IsNullOrEmpty(comments))
        {
            writer.WriteLine("# " + comments);
        }
        foreach (var kvp in this)
        {
            writer.WriteLine($"{kvp.Key}={kvp.Value}");
        }
        writer.Flush();
    }
}
