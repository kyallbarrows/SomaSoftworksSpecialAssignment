using UnityEngine;

public class ArrayElementNameAttribute : PropertyAttribute
{
    public string Varname;
    public ArrayElementNameAttribute(string ElementTitleVar)
    {
        Varname = ElementTitleVar;
    }
}
