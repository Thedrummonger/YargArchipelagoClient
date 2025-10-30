using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoCommon;

namespace YargArchipelagoCLI
{
    internal class CLIHelpers
    {
        public static T NextEnumValue<T>(T value) where T : struct, Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            int index = Array.IndexOf(values, value);
            return values[(index + 1) % values.Length];
        }

        public static IEnumerable<T[]> ChunkFromStart<T>(T[] source, int chunkSize)
        {
            if (source.Length == 0)
                yield break;

            int remainder = source.Length % chunkSize;
            int firstChunkSize = remainder == 0 ? chunkSize : remainder;
            int index = 0;

            if (firstChunkSize != chunkSize)
            {
                yield return source[..firstChunkSize];
                index += firstChunkSize;
            }

            while (index < source.Length)
            {
                int next = Math.Min(chunkSize, source.Length - index);
                yield return source[index..(index + next)];
                index += next;
            }
        }
    }
}
