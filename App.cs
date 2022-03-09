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

            Gmad gmad = new(path)
            {
                name = Path.GetFileNameWithoutExtension(path.TrimEnd(Path.DirectorySeparatorChar)),
                description = "A description for my GMA"
            };

            gmad.Write();
            gmad.SaveFile(gmad.name + ".gma");

            Console.WriteLine("\nCompleted.");
        }
    }
}