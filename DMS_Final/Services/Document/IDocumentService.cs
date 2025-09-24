using DMS_Final.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public interface IDocumentService
{
    List<DocumentModel> GetDocumentsByUser(string userName);
    List<DocumentModel> GetMyDocumentDetails(string userName);
    List<DocumentDetailsModel> GetLatestFilesByDocumentId(int documentId);
    List<DocumentDetailsModel> GetPendingDocumentDetailsWithHeader(string userName = null);
    void ApproveDocument(int documentDetailId, int documentId, string approver, string approverIp, string notes = null);
    void RejectDocument(int documentDetailId, int documentId, string rejector, string rejectorIp, string notes = null);
    void UploadNewVersionMultiple(int parentDetailId, int documentId, DocumentModel model, List<IFormFile> files, List<string> fileDescriptions, string createdBy, string createdFrom, string userRole, List<List<int>> TagIds);
    DocumentModel GetDocumentById(int documentId);
    DocumentDetailsModel GetDocumentDetailById(int documentDetailId);
    List<TagModel> GetAllTags();
    void UploadMultipleFiles(DocumentModel model, List<IFormFile> files, List<string> fileDescriptions, string createdBy, string createdFrom, string userRole, List<List<int>> tagIds);
    List<DocumentTagModel> GetTagsByDocumentDetailsId(int documentDetailsId);
    List<DocumentModel> GetAllDocuments();
    List<DocumentDetailsModel> GetDocumentDetailsByDocumentId(int documentId);
    List<DocumentDetailsModel> Search(int documentId, string tag, string status, string version);
    void DeleteDocumentDetailById(int documentDetailId);
    (DocumentModel, List<DocumentDetailsModel>) GetDocumentWithDetailsForEdit(int documentId);
    void UpdateDocumentInfo(int documentId, string title, string description);
    void UpdateDocumentDetailInfo(int documentDetailId, string description, List<int> tagIds);
    public void InsertNewDocumentDetailVersion(int parentDetailId, string description, List<int> tagIds, string fileName, string originalFileName, string createdBy, string approveStatus, string notes = null);
    List<DocumentDetailsModel> GetDocumentVersionHistoryByUser(string userName, int DocumentId);
    List<DocumentDetailsModel> GetDetailsWithNotes(string createdBy);
    List<DocumentDetailsModel> GetRecentApprovedOrPendingDocuments(int count = 5);
    List<DocumentDetailsModel> GetRecentApprovedOrPendingDocumentsUser(string userName, int count = 5);
    List<(int Year, int Month, int Count)> GetMonthlyPendingCount();
    List<(int Year, int Month, int Count)> GetMonthlyApprovedCount();
    List<(int Year, int Month, int Count)> GetMonthlyRejectedCount();


    TagModel CreateTag(string tagName);
}
