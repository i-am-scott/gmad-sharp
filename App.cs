namespace umad
{
    public static class App
    {
        public static void Main(string[] args)
        {
            if (args is null || args[0].Length == 0)
                return;

            string path = Path.GetFullPath(args[0]);
            Console.WriteLine("Creating " + path + "\n\n");

            Gmad gmad = new(path);
            gmad.Write();

            foreach(GMAFile file in gmad.files)
            {
                Console.WriteLine("Writing: " + file.path);
            }

            string fileName = Path.GetFileNameWithoutExtension(path.TrimEnd(Path.DirectorySeparatorChar));
            gmad.SaveFile(fileName + ".gma");

            Console.WriteLine("\nCompleted.");
        }
    }
}