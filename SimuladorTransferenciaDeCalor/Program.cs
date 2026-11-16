namespace SimuladorTransferenciaDeCalor
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Modo benchmark: com o argumento --benchmark, mede o desempenho (N1 x N2) em vez de abrir a tela.
            // Não cria o Form1, então não precisa de banco de dados nem da variável SIMULADOR_DB.
            if (args.Length > 0 && args[0] == "--benchmark")
            {
                Benchmark.Executar();
                return;
            }

            Application.Run(new Form1());
        }
    }
}
