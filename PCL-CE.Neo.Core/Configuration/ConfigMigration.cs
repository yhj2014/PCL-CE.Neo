using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Configuration;

public delegate void ConfigMigrationHandler(string from, string to);

public class ConfigMigration
{
    public required string From { get; init; }
    public required string To { get; init; }
    public int Priority { get; init; } = 0;
    public required ConfigMigrationHandler OnMigration { get; init; }

    public static bool Migrate(string target, IEnumerable<ConfigMigration> migrations)
    {
        migrations = migrations as ConfigMigration[] ?? migrations.ToArray();
        IEnumerable<ConfigMigration>? solution = null;
        var found = (
            from migration in migrations.Reverse()
            let path = migration.From
            where File.Exists(path) && _TryFindShortestPath(path, target, migrations, out solution)
            select path
        ).Any();
        if (!found) return false;
        foreach (var migration in solution!) migration.OnMigration(migration.From, migration.To);
        return true;
    }

    private static bool _TryFindShortestPath(string start, string end,
        IEnumerable<ConfigMigration> paths, [NotNullWhen(true)] out IEnumerable<ConfigMigration>? result)
    {
        if (start == end)
        {
            result = [];
            return true;
        }

        var adj = new Dictionary<string, List<ConfigMigration>>(StringComparer.Ordinal);
        foreach (var p in paths)
        {
            if (!adj.TryGetValue(p.From, out var list))
            {
                list = [];
                adj[p.From] = list;
            }
            list.Add(p);
        }

        var dist = new Dictionary<string, int>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        dist[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var u = queue.Dequeue();
            if (!adj.TryGetValue(u, out var outgoing))
                continue;

            var du = dist[u];
            foreach (var e in outgoing)
            {
                var v = e.To;
                if (!dist.ContainsKey(v))
                {
                    dist[v] = du + 1;
                    queue.Enqueue(v);
                }
            }
        }

        if (!dist.ContainsKey(end))
        {
            result = null;
            return false;
        }

        var nodesByDepth = new Dictionary<int, List<string>>();
        foreach (var kv in dist)
        {
            if (!nodesByDepth.TryGetValue(kv.Value, out var list))
            {
                list = [];
                nodesByDepth[kv.Value] = list;
            }
            list.Add(kv.Key);
        }

        var bestPriority = new Dictionary<string, long>(StringComparer.Ordinal);
        var prevNode = new Dictionary<string, string>(StringComparer.Ordinal);
        var prevEdge = new Dictionary<string, ConfigMigration>(StringComparer.Ordinal);

        bestPriority[start] = 0;
        var maxDepth = dist[end];

        for (var d = 0; d < maxDepth; d++)
        {
            if (!nodesByDepth.TryGetValue(d, out var layer))
                continue;

            foreach (var u in layer)
            {
                if (!bestPriority.ContainsKey(u)) continue;
                if (!adj.TryGetValue(u, out var outgoing)) continue;

                foreach (var e in outgoing)
                {
                    if (!dist.TryGetValue(e.To, out var dv) || dv != d + 1)
                        continue;

                    var candidate = bestPriority[u] + e.Priority;
                    if (!bestPriority.TryGetValue(e.To, out var cur) || candidate > cur)
                    {
                        bestPriority[e.To] = candidate;
                        prevNode[e.To] = u;
                        prevEdge[e.To] = e;
                    }
                }
            }
        }

        if (!prevEdge.ContainsKey(end))
        {
            result = null;
            return false;
        }
        var path = new List<ConfigMigration>();
        var curr = end;
        while (curr != start)
        {
            var edge = prevEdge[curr];
            path.Add(edge);
            curr = prevNode[curr];
        }
        path.Reverse();
        result = path;
        return true;
    }
}