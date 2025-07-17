using Crawler.Api.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace Crawler.Api.Infrastructure.Services
{
    public class PythonCrawlerService : ICrawlerService
    {
        private readonly string _pythonExecutablePath;
        private readonly string _workerScriptPath;

        public PythonCrawlerService(IConfiguration configuration)
        {
            _pythonExecutablePath = configuration["CrawlerSettings:PythonExecutablePath"]
                ?? throw new ArgumentNullException("PythonExecutablePath não configurado.");

            _workerScriptPath = configuration["CrawlerSettings:WorkerScriptPath"]
                ?? throw new ArgumentNullException("WorkerScriptPath não configurado.");
        }

        public async Task<string> RunCrawlAsync(string targetUsername, int? maxItems)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutablePath,
                    // Montamos os argumentos para o script main.py
                    Arguments = $"{_workerScriptPath} instagram {targetUsername} --max_items {maxItems ?? 10}",
                    RedirectStandardOutput = true, // Para capturar o 'print' do Python
                    RedirectStandardError = true,  // Para capturar qualquer erro
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8, // Garante a leitura correta de caracteres especiais

                    Environment = { ["PYTHONUTF8"] = "1" }
                }
            };

            process.Start();

            // Lemos a saída do script de forma assíncrona
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Se o script Python retornou um erro, nós lançamos uma exceção
                throw new InvalidOperationException($"Erro ao executar o worker Python: {error}");
            }

            // Se tudo deu certo, retornamos o JSON capturado do 'print'
            return output;
        }
    }
}
