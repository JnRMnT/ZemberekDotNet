namespace ZemberekDotNet.Core.Native
{
    public class Objects
    {
        public static bool Equal(object object1, object object2)
        {
            return (object1 == object2) || (object1 != null && object1.Equals(object2));
        }

        public static int HashCode(params object[] objects)
        {
            if (objects == null)
                return 0;

            int result = 1;

            foreach (object element in objects)
            {
                result = 31 * result + (element == null ? 0 : element.GetHashCode());
            }

            return result;
        }
    }
}
