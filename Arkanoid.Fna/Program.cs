namespace Arkanoid.FNA
{
    /// <summary>
    /// Точка входа в приложение. Запускает игровой цикл Arkanoid.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            using (FNA game = new FNA())
            {
                game.Run();
            }
        }
    }
}
