using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace PCL_CE.Neo.Core.Utils.Diagnostics;

public static class StackHelper
{
    private const string Unknown = "<unknown>";

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetDirectCallerName(
        bool includeNamespace = true,
        bool includeParameters = false,
        int skipAppFrames = 0)
    {
        var st = new StackTrace(skipFrames: 1, fNeedFileInfo: false);
        var frame = st.GetFrame(skipAppFrames);
        var method = frame?.GetMethod();
        if (method is null) return Unknown;

        method = _TryMapAsyncOrIterator(method);
        return _FormatMethod(method, includeNamespace, includeParameters);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IReadOnlyList<string> GetStack(
        int maxFrames = 10,
        bool includeNamespace = true,
        bool includeParameters = false,
        bool needFileInfo = false)
    {
        if (maxFrames <= 0) return [];

        var st = new StackTrace(skipFrames: 1, fNeedFileInfo: needFileInfo);
        var list = new List<string>(capacity: Math.Min(maxFrames, st.FrameCount));

        for (int i = 0, added = 0; i < st.FrameCount && added < maxFrames; i++)
        {
            var method = st.GetFrame(i)?.GetMethod();
            if (method is null) continue;

            method = _TryMapAsyncOrIterator(method);
            var sig = _FormatMethod(method, includeNamespace, includeParameters);

            if (needFileInfo)
            {
                var f = st.GetFrame(i);
                var file = f?.GetFileName();
                var line = f?.GetFileLineNumber() ?? 0;
                if (!string.IsNullOrEmpty(file) && line > 0)
                {
                    sig = $"{sig} ({System.IO.Path.GetFileName(file)}:{line})";
                }
            }

            list.Add(sig);
            added++;
        }

        return list;
    }

    private static MethodBase _TryMapAsyncOrIterator(MethodBase method)
    {
        if (method.Name != "MoveNext") return method;

        var dt = method.DeclaringType;
        if (dt is null) return method;

        var name = dt.Name;
        var lt = name.IndexOf('<');
        var gt = name.IndexOf('>');
        if (lt >= 0 && gt > lt + 1)
        {
            var originalName = name.Substring(lt + 1, gt - lt - 1);
            var parent = dt.DeclaringType ?? dt;
            foreach (var m in parent.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (string.Equals(m.Name, originalName, StringComparison.Ordinal))
                    return m;
            }
        }

        return method;
    }

    private static string _FormatMethod(MethodBase method, bool includeNamespace, bool includeParameters, bool includeParameterNamespace = false)
    {
        var sb = new StringBuilder();
        var type = method.DeclaringType;
        if (type is not null)
        {
            sb.Append(_FormatTypeName(type, includeNamespace));
            sb.Append('.');
        }

        sb.Append(method.Name);

        if (method is MethodInfo { IsGenericMethod: true } mi)
        {
            sb.Append('[');
            var args = mi.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_FormatTypeName(args[i], false));
            }
            sb.Append(']');
        }

        if (includeParameters)
        {
            var ps = method.GetParameters();
            sb.Append('(');
            for (var i = 0; i < ps.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_FormatTypeName(ps[i].ParameterType, includeParameterNamespace));
                if (!string.IsNullOrEmpty(ps[i].Name))
                {
                    sb.Append(' ').Append(ps[i].Name);
                }
            }
            sb.Append(')');
        }

        return sb.ToString();
    }

    private static string _FormatTypeName(Type t, bool includeNamespace)
    {
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition();
            var name = def.Name;
            var tick = name.IndexOf('`');
            if (tick >= 0) name = name[..tick];

            var ns = includeNamespace ? (def.Namespace is null ? "" : def.Namespace + ".") : "";
            var sb = new StringBuilder(ns).Append(name).Append('[');
            var args = t.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_FormatTypeName(args[i], false));
            }
            sb.Append(']');
            return sb.ToString();
        }

        return includeNamespace && t.Namespace is not null
            ? t.Namespace + "." + t.Name
            : t.Name;
    }
}