using System;
using UnityEngine;

namespace CoreToolkit.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredFieldAttribute : PropertyAttribute { }
}