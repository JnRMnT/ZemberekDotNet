using System;
using System.Runtime.CompilerServices;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    /// <summary>
    /// A cache for set of morphemic attributes to surface forms.
    /// 
    /// For thread safety, writes to map are synchronized, when map needs expanding
    /// writer thread creates the expanded version and replaces the map. The map
    /// reference is volatile so readers always see a consistent albeit possibly
    /// stale version of the map.
    /// 
    /// This is also knows as cheap read-write lock trick (see item #5 in the link)
    /// https://www.ibm.com/developerworks/java/library/j-jtp06197/index.html
    /// 
    /// </summary>
    public class AttributeToSurfaceCache
    {
        // volatile guarantees atomic reference copy.
        private volatile IntMap<string> attributeMap;

        internal AttributeToSurfaceCache()
        {
            attributeMap = IntMap<string>.CreateManaged();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void AddSurface(int attributes, String surface)
        {
            while (!attributeMap.Put(attributes, surface))
            {
                attributeMap = attributeMap.Expand();
            }
        }

        internal string GetSurface(int attributes)
        {
            IntMap<string> map = attributeMap;
            return map.Get(attributes);
        }
    }
}
