using System.Text;

namespace umad
{
    public static class UtilExtentions
    {
        public static void WriteTerminated(this BinaryWriter writer, string value)
        {
            writer.Write(Encoding.ASCII.GetBytes(value));
            writer.Write((byte)0);
        }
    }

    public static class Util
    {
        public static uint Crc32(byte[] input, uint seed = 0xFFFFFF)
            => umad.Crc32.Compute(input, seed);

        public static uint Crc32(string input, uint seed = 0xFFFFFF)
         => Crc32(Encoding.UTF8.GetBytes(input), seed);

        public static void Log(string str)
        {
            try
            {
                string dateTime = DateTime.Now.ToString("G");
                using (StreamWriter output = File.AppendText($"debug.log"))
                {
                    output.AutoFlush = true;
                    output.WriteLine($"[{dateTime}] {str}");
                }
            }
            catch
            {
                Console.WriteLine();
            }
        }
    }
}