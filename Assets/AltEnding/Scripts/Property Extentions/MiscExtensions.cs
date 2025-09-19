using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

public static class Vector2Extensions
{
    public static float AspectRatio(this Vector2 vector2)
    {
        return vector2.x / vector2.y;
    }

    public static float InverseAspectRatio(this Vector2 vector2)
    {
        return vector2.y / vector2.x;
    }
}

public static class ListExtensions
{
    /// <summary>
    /// Removes all null entries from a a list. If the list itself is null, creates a new list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void CleanList<T>(this List<T> list)
    {
        if (list == null) list = new List<T>();
        for (int c = list.Count - 1; c >= 0; c--)
        {
            if (list[c] == null) list.RemoveAt(c);
        }
    }

    public static List<T> ToList<T>(this T[] array) 
    {
        List<T> result = new List<T>();
        for(int c = 0; c < array.Length; c++)
        {
            result.Add(array[c]);
        }
        return result;
    }

    public static int CompareScrollRectsBySiblingIndex(this ScrollRect sourceRect, ScrollRect compareRect)
    {
        if (compareRect == null) return 1;
        if (sourceRect.transform.parent != compareRect.transform.parent) return 1;
        return sourceRect.transform.GetSiblingIndex() - compareRect.transform.GetSiblingIndex();
    }
}

public static class UIBehaviourExtenstions
{
    public static Canvas GetCanvas(this UnityEngine.EventSystems.UIBehaviour uIBehaviour)
    {
        if (uIBehaviour == null) return null;
        Canvas result = null;
        Transform transformToTest = uIBehaviour.transform;
        while (transformToTest != null && result == null)
        {
            if (transformToTest.TryGetComponent(out result))
            {
                return result;
            }
            transformToTest = transformToTest.parent;

        }

        return null;
    }

    public static Canvas GetRootCanvas(this UnityEngine.EventSystems.UIBehaviour uIBehaviour)
    {
        if (uIBehaviour == null) return null;
        Canvas result = null;
        Canvas finalResult = null;
        Transform transformToTest = uIBehaviour.transform;
        while (transformToTest != null)
        {
            if (transformToTest.TryGetComponent(out result))
            {
                finalResult = result;
            }
            transformToTest = transformToTest.parent;

        }

        return finalResult;
    }
}

public static class NumberExtensions
{
    public static string AddOrdinal(int num)
    {
        if (num <= 0) return num.ToString();

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
    }
}
