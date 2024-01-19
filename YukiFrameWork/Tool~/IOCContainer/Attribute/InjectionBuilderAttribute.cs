using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    [AttributeUsage(AttributeTargets.Field
        | AttributeTargets.Property
        | AttributeTargets.Struct)]
    public class InjectionBuilderAttribute : Attribute
    {
        
    }
}
