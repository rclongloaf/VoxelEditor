using System.Collections.Generic;

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
}
}