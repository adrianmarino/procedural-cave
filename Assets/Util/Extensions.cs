﻿using System;
using System.Collections.Generic;

namespace Util
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source) 
                action(element);
        }
    }
}