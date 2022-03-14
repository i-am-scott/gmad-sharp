using SevenZip;
using System.Text;

namespace umad
{
    public class GMAFile
    {
        public string? path = null;
        public string? fullpath = null;
        public long size;
        public uint crc;
    }

    public class Gmad : IDisposable
    {
        string folder;
        public bool compress = false;

        static int dictionary = 256000000; // 32MB
        static CoderPropID[] propIDs =
        {
            CoderPropID.DictionarySize,
            CoderPropID.PosStateBits,
            CoderPropID.LitContextBits,
            CoderPropID.LitPosBits,
            CoderPropID.Algorithm,
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder,
            CoderPropID.EndMarker
        };

        static object[] properties =
        {
            dictionary,
            2,
            3,
            0,
            2,
            256,
            "bt4",
            false
        };

        public int version = 3;
        public ulong steamid = 0UL;
        public ulong timestamp = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();

        public string extension = "gma";
        public string name = "Uknown";
        public string description = "Its a GMA";
        public string author = "Superman";

        public List<GMAFile> files { get; protected set; } = new List<GMAFile>();
        public byte[]? data { get; protected set; }

        public Gmad(string dir)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(dir);

            if (!dir.EndsWith(Path.DirectorySeparatorChar))
            {
                dir += Path.DirectorySeparatorChar;
            }

            folder = dir;
            GetFiles(folder);
        }

        private void GetFiles(string dir)
        {
            foreach (string dp in Directory.GetDirectories(dir))
            {
                foreach (string fp in Directory.GetFiles(dp))
                {
                    byte[] data = File.ReadAllBytes(fp);

                    GMAFile gmaFile = new()
                    {
                        fullpath = fp,
                        path = fp.RelativeFromFolder(folder),
                        size = data.LongLength
                    };

                    files.Add(gmaFile);
                    data = null;
                }

                GetFiles(dp);

            }
        }

        public void Write()
        {
            if (files == null || files.Count == 0)
                return;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write('G');
                    writer.Write('M');
                    writer.Write('A');
                    writer.Write('D');
                    writer.Write((char)version);
                    writer.Write(steamid);
                    writer.Write((ulong)DateTimeOffset.Now.ToUnixTimeSeconds());
                    writer.Write((char)0);

                    writer.WriteTerminated(name);
                    writer.WriteTerminated(description);
                    writer.WriteTerminated(author);
                    writer.Write((int)1);

                    uint c = 0;
                    foreach (GMAFile file in files)
                    {
                        if (file.size == 0) continue;

                        c++;
                        writer.Write(c);
                        writer.WriteTerminated(file.path.ToLower());
                        writer.Write(file.size);
                        writer.Write(0u);
                    }

                    writer.Write(0u);

                    foreach (GMAFile file in files)
                    {
                        byte[] data = File.ReadAllBytes(file.fullpath);
                        writer.Write(data);
                    }

                    writer.Write(Util.Crc32(stream.ToArray()));

                    if (!compress)
                    {
                        data = stream.ToArray();
                    }
                    else
                    {
                        var encoder = new SevenZip.Compression.LZMA.Encoder();
                        encoder.SetCoderProperties(propIDs, properties);

                        using (MemoryStream compressed = new MemoryStream())
                        {
                            encoder.WriteCoderProperties(compressed);
                            long size = stream.ToArray().Length;

                            for (int i = 0; i < 8; i++)
                                compressed.WriteByte((byte)(size >> (8 * i)));

                            stream.Position = 0;
                            encoder.Code(stream, compressed, -1, -1, null);

                            data = compressed.ToArray();
                        }
                    }
                }
            }
        }

        public void SaveFile(string outFile)
        {
            File.WriteAllBytes(outFile, data);
        }

        public void Dispose()
        {
            data = null;
        }
    }
}
