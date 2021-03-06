﻿using System.Collections.Generic;
using System.Linq;
using Procedural.Generator.Output;
using Procedural.Generator.Step;

namespace Procedural.Generator.Pipeline
{
    public class GenerationPipeline
    {
        public object Perform(StepContext ctx)
        {
            return Perform(ctx, null);
        }

        public object Perform(StepContext ctx, object initialInput)
        {
            var input = initialInput;
            object output = null;

            foreach (var step in steps)
            {
                output = step.Perform(ctx, input);
                input = output;
            }
            return output;
        }
        
        private readonly List<IGenerationStep> steps;

        public GenerationPipeline(List<IGenerationStep> steps)
        {
            this.steps = steps;
        }
    }
}