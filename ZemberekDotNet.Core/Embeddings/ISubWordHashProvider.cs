
namespace ZemberekDotNet.Core.Embeddings
{
    public interface ISubWordHashProvider
    {
        int[] GetHashes(string word, int wordId);

        int GetMinN();

        int GetMaxN();
    }
}
