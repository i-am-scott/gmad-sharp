using SevenZip;
using System.Text.Json;

namespace umad
{
    public class GMAFile
    {
        public string? path = null;
        public string? fullpath = null;
        public long size;
        public uint crc;
    }

    public class AddonJson
    {
        public string? title { get; set; }
        public string? type { get; set; }
        public string? description { get; set; }
        public string[]? tags { get; set; }
    }

    public class AddonJsonOutput
    {
        public string? description { get; set; }
        public string? type { get; set; }
        public string[]? tags { get; set; }
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

        public string title = "Unknown";
        public ulong steamid = 2UL;
        public ulong timestamp = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
        public bool ignoreTimestamp = false;
        public uint crc = 0;

        public string AddonJsonStr;
        public AddonJson AddonConfig;

        public JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        public List<GMAFile> files { get; protected set; } = new List<GMAFile>();
        public byte[]? data { get; protected set; }

        public Gmad(string dir, AddonJson config = null)
        {
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(dir);

            if (!dir.EndsWith(Path.DirectorySeparatorChar))
            {
                dir += Path.DirectorySeparatorChar;
            }

            folder = dir;

            GetFiles(folder);
            LoadConfig(config);
        }

        private void LoadConfig(AddonJson config = null)
        {
            if (config == null)
            {
                AddonJsonStr = File.ReadAllText(folder + "addon.json");
                AddonConfig = JsonSerializer.Deserialize<AddonJson>(AddonJsonStr);
                title = AddonConfig.title;
            }
            else
            {
                AddonConfig = config;
                title = AddonConfig.title;
            }
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
                        path = Path.GetRelativePath(folder, fp).Replace(Path.DirectorySeparatorChar, '/'),
                        size = data.LongLength
                    };

                    files.Add(gmaFile);
                    data = null;
                }

                GetFiles(dp);
            }
        }

        public bool Write(out uint createdCrc)
        {
            if (files == null || files.Count == 0)
            {
                createdCrc = 0;
                return false;
            }

            AddonJsonStr = JsonSerializer.Serialize(new AddonJsonOutput
            {
                type = AddonConfig.type,
                description = AddonConfig.description,
                tags = AddonConfig.tags,
            }, jsonOptions);

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("GMAD"));
                    writer.Write((char)version);
                    writer.Write(steamid);

                    if (ignoreTimestamp)
                    {
                        writer.Write((ulong)0);
                    }
                    else
                    {
                        writer.Write((ulong)DateTimeOffset.Now.ToUnixTimeSeconds());
                    }

                    writer.Write((char)0);
                    writer.WriteTerminated(AddonConfig.title);
                    writer.WriteTerminated(AddonJsonStr);
                    writer.WriteTerminated("Author Name");
                    writer.Write((int)1);

                    uint c = 0;
                    for(int i = 0; i < files.Count; i++)
                    {
                        GMAFile file = files[i];

                        if (file.size == 0) continue;

                        c++;
                        writer.Write(c);
                        writer.WriteTerminated(file.path.ToLower());
                        writer.Write(file.size);
                        writer.Write(Util.Crc32(file.fullpath));
                    }

                    writer.Write(0u);

                    for (int i = 0; i < files.Count; i++)
                    {
                        GMAFile file = files[i];

                        byte[] data = File.ReadAllBytes(files[i].fullpath);
                        writer.Write(data);
                    }

                    crc = Util.Crc32(stream.ToArray(), (uint)stream.Length);
                    writer.Write(crc);

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

                createdCrc = crc;
                return data.Length > 0 && crc > 0;
            }
        }

        public void SaveFile(string outFile)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outFile));
            File.WriteAllBytes(outFile, data);
        }

        public void Dispose()
        {
            data = null;
        }
    }
}
