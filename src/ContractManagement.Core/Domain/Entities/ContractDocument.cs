using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Domain.Entities;

public class ContractDocument
{
    public int Id { get; private set; }
    public int ContractId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public string? FilePath { get; private set; }
    public int UploadedById { get; private set; }
    public DateTime UploadedDate { get; private set; }

    private ContractDocument() { }
    
    internal ContractDocument(string fileName, DocumentType documentType, string? filePath, int uploadedById)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name is required.");

        FileName = fileName;
        DocumentType = documentType;
        FilePath = filePath;
        UploadedById = uploadedById;
        UploadedDate = DateTime.UtcNow;
    }
}