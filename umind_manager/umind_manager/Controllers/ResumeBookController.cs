using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ia_test.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeBookController : ControllerBase
    {
        [HttpPost("gerar")]
        public async Task<IActionResult> GerarResumo([FromBody] ResumoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Texto))
                return BadRequest("Texto inválido");

            var resumo = await ObterResumoViaPython(request.Texto);
            return Ok(new { Resumo = resumo });
        }

        private async Task<string> ObterResumoViaPython(string texto)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "C:\\Users\\Xand\\AppData\\Local\\Programs\\Python\\Python313\\python.exe",
                Arguments = "Scripts\\Resume.py",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi };
            process.Start();

            await process.StandardInput.WriteAsync(texto);
            process.StandardInput.Close();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            // Se houver erro, mas só for o aviso do device, ignore
            if (!string.IsNullOrWhiteSpace(error) && !error.Contains("Device set to use cpu"))
                throw new Exception("Erro ao gerar resumo: " + error);

            return output.Trim();
        }
    }


    public class ResumoRequest
    {
        public string Texto { get; set; }
    }
}
