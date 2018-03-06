﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator.Step
{
    static class GenerationStepMethods
    {
        public static List<GenerationStep> AllLessThanOrEqualThis(this GenerationStep stepEnum)
        {
            return Enum.GetValues(typeof(GenerationStep)).Cast<GenerationStep>().Where(it => it <= stepEnum).ToList();
        }
    }
    
    public enum GenerationStep
    {
        Cell = 0,
        Square = 1,
        Mesh = 2
    }
}