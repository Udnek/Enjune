namespace UiAddon;

public static class Extensions
{
    extension(IReadOnlyList<char> str)
    {
        /// <summary>
        /// returns index of first target before index (-1 if not found)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FirstBefore(int index, char target)
        {
            for (int i = index-1; i >= 0; i--)
            {
                if (str[i] == target)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// returns index of first target after index (length if not found)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FirstAfter(int index, char target)
        {
            for (int i = index+1; i < str.Count; i++)
            {
                if (str[i] == target)
                    return i;
            }
            return str.Count;
        }
    }
    
    extension(string str)
    {
        /// <summary>
        /// returns index of first target before index (-1 if not found)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FirstBefore(int index, char target)
        {
            for (int i = index-1; i >= 0; i--)
            {
                if (str[i] == target)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// returns index of first target after index (length if not found)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int FirstAfter(int index, char target)
        {
            for (int i = index+1; i < str.Length; i++)
            {
                if (str[i] == target)
                    return i;
            }
            return str.Length;
        }
    }
}