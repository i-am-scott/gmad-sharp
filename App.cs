namespace umad
{
    public static class App
    {
        public static void Main(string[] args)
        {
            if (args is null || args[0].Length == 0)
                return;

            Gmad gmad = new(args[0])
            {
                name = Path.GetFileNameWithoutExtension(args[0]),
                description = "This is a pack of dicks"
            };

            gmad.Write();
            gmad.SaveFile(gmad.name + ".gma");            
        }
    }
}