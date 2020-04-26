using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Embeddings
{
    public class SupervisedModel : Model
    {
        public SupervisedModel(Model model, int seed) : base(model, seed)
        {

        }
    }
}
