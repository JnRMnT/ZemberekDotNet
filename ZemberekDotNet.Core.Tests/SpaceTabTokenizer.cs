using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Core;

namespace ZemberekDotNet.Core.Tests
{
    [TestClass]
    public class SpaceTabTokenizer
    {
        [TestMethod]
        public void BasicTokenizationTest()
        {
            string sentence = "__label__dünya Yabancılar dokunmasın diye kızının boğulmasına göz yumdu";
            Core.SpaceTabTokenizer tokenizer = new Core.SpaceTabTokenizer();
            string[] words = tokenizer.Split(sentence);
        }
    }
}
