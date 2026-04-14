namespace Arkanoid.FNA
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (FNA game = new FNA())
            {
                game.Run();
            }
        }
    }
}
