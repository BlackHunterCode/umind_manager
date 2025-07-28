using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using umind_manager.Core.Contracts.Repositories;
using umind_manager.Core.Entities;
using umind_manager.Core.Models;

namespace umind_manager.Controllers
{
    public class BookManagerController : Controller
    {
        private readonly ILogger<BookManagerController> _logger;
        private readonly IBookRepository _bookRepository;

        public BookManagerController(ILogger<BookManagerController> logger, IBookRepository bookRepository)
        {
            _logger = logger;
            _bookRepository = bookRepository;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Buscando pelos livros");
            var livros = await _bookRepository.GetAllAsync();
            return View(livros);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateBook([FromBody] Books book)
        {
            if (book == null || book.Id <= 0)
                return BadRequest("Dados inválidos.");

            _logger.LogInformation("Atualizando livro com ID {Id}", book.Id);

            var existingBook = await _bookRepository.GetByIdAsync(book.Id);
            if (existingBook == null)
                return NotFound("Livro não encontrado.");

            var affectedRows = await _bookRepository.UpdateBookAsync(book);
            if (affectedRows == 1)
                return NoContent();

            return BadRequest("Erro ao atualizar o livro.");
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound("Livro não encontrado.");
            }

            var result = await _bookRepository.DeleteBookAsync(id);
            if (result > 0)
            {
                return NoContent();
            }

            return BadRequest("Não foi possível deletar o livro.");
        }

        [HttpGet]
        public async Task<IEnumerable<Books>> SearchBooks()
        {
            _logger.LogInformation("Buscando pelos livros");
            var livros = await _bookRepository.GetAllAsync();
            return livros;
        }

        [HttpPost]
        public async Task<IActionResult> CriarResumo(IFormFile livroPdf, IFormFile capa, string titulo, string sumario)
        {
            _logger.LogInformation("Criando Resumo...");

            if (livroPdf == null || livroPdf.Length == 0)
                return BadRequest("Arquivo PDF do livro não enviado.");

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsPath);

            var pdfFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".pdf";
            var livroPath = Path.Combine(uploadsPath, pdfFileName);
            var resumoPath = Path.Combine(Directory.GetCurrentDirectory(), "saida", "resumo.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(resumoPath)); // garante que pasta exista

            // Salva PDF do livro
            using (var stream = new FileStream(livroPath, FileMode.Create))
                await livroPdf.CopyToAsync(stream);

            string capaFileName = null;
            if (capa != null && capa.Length > 0)
            {
                capaFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(capa.FileName);
                var capaPath = Path.Combine(uploadsPath, capaFileName);

                using (var stream = new FileStream(capaPath, FileMode.Create))
                    await capa.CopyToAsync(stream);
            }

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"Scripts/resumidor_rag.py \"{livroPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logger.LogInformation("Processando...");
            var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0 || !System.IO.File.Exists(resumoPath))
                return Content("Erro ao gerar resumo: " + error + output);

            var livro = new Books
            {
                Title = titulo,
                Sumary = sumario,
                PathPdf = pdfFileName,
                PathCape = capaFileName,
                Active = true,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _bookRepository.InsertAsync(livro);

            _logger.LogInformation("Processado com sucesso.");

            var bytes = await System.IO.File.ReadAllBytesAsync(resumoPath);
            return File(bytes, "application/pdf", $"resumo_{titulo}.pdf");
        }
    }
}
