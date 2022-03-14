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
        private static Crc32 Crc = new Crc32();

        public static uint Crc32(byte[] input)
            => Crc.ComputeChecksum(input);

        public static uint Crc32(string input)
         => Crc32(Encoding.UTF8.GetBytes(input));

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