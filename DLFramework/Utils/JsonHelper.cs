using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    public static Dictionary<string, object> FromJson(string json)
    {
        return JsonUtility.FromJson<Dictionary<string, object>>(json);
    }
}