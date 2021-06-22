using System;
using System.Collections.Generic;
using ZemberekDotNet.Classification;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Examples.Classification
{
    public class SimpleClassification
    {
        public static void Main()
        {
            // assumes models are generated with NewsTitleCategoryFinder
            string path = @"C:\Users\ozank\Desktop\ZemberekDotNet\data\classification\news-title-category-set.tokenized.model";
            FastTextClassifier classifier = FastTextClassifier.Load(path);

            String s = "Beşiktaş berabere kaldı.";

            // process the input exactly the way trainin set is processed
            String processed = String.Join(" ", TurkishTokenizer.Default.TokenizeToStrings(s));
            processed = processed.ToLower(Turkish.Locale);

            // results, only top three.
            List<ScoredItem<String>> res = classifier.Predict(processed, 3);

            foreach (ScoredItem<String> re in res)
            {
                Console.WriteLine(re);
            }
            Console.ReadLine();
        }
    }
}