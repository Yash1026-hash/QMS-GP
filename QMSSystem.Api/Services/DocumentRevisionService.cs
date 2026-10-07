using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Models;
using QMSSystem.Api.Data;

namespace QMSSystem.Api.Services
{
    public class DocumentRevisionService
    {
        private readonly ApplicationDbContext _context;

        public DocumentRevisionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UploadRevisionAsync(
            DocumentRevision revision,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Create upload folder
            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "revisions");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Create unique file name
            var fileName =
                $"{Guid.NewGuid()}_{file.FileName}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName);

            // Save the file
            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Store file path
            revision.FilePath =
                $"/uploads/revisions/{fileName}";

            // System-generated values
            revision.FileName = file.FileName;
            revision.UploadedTime = DateTime.UtcNow;
            revision.ApprovalStatus = "Pending";

            // Save revision record
            _context.DocumentRevisions.Add(revision);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<DocumentRevision>>
            GetRevisionsByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentRevisions
                .Where(x => x.DocumentId == documentId)
                .ToListAsync();
        }
    }
}