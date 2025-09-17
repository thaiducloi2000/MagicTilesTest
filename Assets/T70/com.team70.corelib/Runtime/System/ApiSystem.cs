using System;
using System.Collections.Generic;
using UnityEngine;
using Command = System.Func<object[], object>;

public class ApiSystem
{
    private static readonly Dictionary<string, Command> _map = new Dictionary<string, Command>();

    private static readonly object[] NO_PARAMS = new object[0];

    public static bool isLog = true;

    
    public static void AddCommand(string commandId, Command cmd, bool rewrite = false)
    {
        // Debug.Log("Add Command: " + commandId + " : " + cmd);

        if (_map.ContainsKey(commandId))
        {
            if (rewrite == false) LogWarning($"CommandId {commandId} registered before!");
            if (rewrite == true) _map[commandId] = cmd;
            return;
        }

        if (cmd == null)
        {
            LogWarning($"Can not register a null cmd, commandId = {commandId}");
            return;
        }

        _map.Add(commandId, cmd);
    }

    public static T Execute<T>(string commandId)
    {
        var ret = Execute(commandId, NO_PARAMS);
        if (ret == null) return default(T);
        return (T)ret;
    }

    public static T Execute<T, T1>(string commandId, T1 p1)
    {
        var ret = Execute(commandId, p1);
        if (ret == null) return default(T);
        return (T)ret;
    }

    public static T Execute<T, T1, T2>(string commandId, T1 p1, T2 p2)
    {
        var ret = Execute(commandId, p1, p2);
        if (ret == null) return default(T);
        return (T)ret;
    }

    public static T Execute<T, T1, T2, T3>(string commandId, T1 p1, T2 p2, T3 p3)
    {
        var ret = Execute(commandId, p1, p2, p3);
        if (ret == null) return default(T);
        return (T)ret;
    }

    public static T Execute<T, T1, T2, T3, T4>(string commandId, T1 p1, T2 p2, T3 p3, T4 p4)
    {
        var ret = Execute(commandId, p1, p2, p3, p4);
        if (ret == null) return default(T);
        return (T)ret;
    }

    public static object Execute(string commandId, params object[] args)
    {

        if (_map.TryGetValue(commandId, out Command cmd))
        {
            if (cmd == null)
            {
                LogWarning($"Command is null!!! {commandId}");
                return null;
            }

#if UNITY_EDITOR
            return cmd(args);
#else
			try
            {
                return cmd(args);
            }
            catch (Exception e)
            {
                LogWarning($"Exception when executing command {commandId} \n {e}");
            }
#endif
        }
        else
        {
            LogWarning($"Command {commandId} can not be found!");
        }

        return null;
    }

    private static void LogWarning(string message)
    {
        if (isLog == false) return;
        Debug.LogWarning(message);
    }
}