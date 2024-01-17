using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Main.Scripts.Utils
{
public static class Extensions
{
    public static HashSet<T> Plus<T>(this HashSet<T> set, T value)
    {
        set.Add(value);
        return set;
    }
    
    public static HashSet<T> Minus<T>(this HashSet<T> set, T value)
    {
        set.Remove(value);
        return set;
    }
    
    public static void SetVisibility(this VisualElement view, bool isVisible)
    {
        view.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
}