using DMS_Final.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public interface IDocumentRepository
{
    // Document methods
    int InsertDocument(DocumentModel document, SqlConnection conn, SqlTransaction transaction);
    List<DocumentModel> GetDocumentsByUser(string userName);

    List<DocumentDetailsModel> GetLatestFilesByDocumentId(int documentId);








    DocumentModel GetDocumentById(int documentId);   // ✅ New
    //void UpdateDocumentInfo(int documentId, string title, string description, SqlConnection conn, SqlTransaction transaction); // ✅ New

    // DocumentDetails methods
    int InsertDocumentDetails(DocumentDetailsModel details, SqlConnection conn, SqlTransaction transaction);
    void SetArchiveStatus(int documentDetailId, bool isArchive, SqlConnection conn, SqlTransaction transaction);
    void SetApproveStatus(int documentDetailId, string status, SqlConnection conn, SqlTransaction transaction);


    //List<DocumentDetailsModel> GetMyDocumentDetails(string userName);
    List<DocumentModel> GetMyDocumentDetails(string userName);



    //List<DocumentDetailsModel> GetPendingDocumentDetailsWithHeader();
    //List<DocumentDetailsModel> GetUserPendingDocumentDetailsWithHeader(string userName);
    List<DocumentDetailsModel> GetPendingDocumentDetailsWithHeader(string userName = null);

    // DocumentStatusHistory methods
    void InsertStatusHistory(DocumentStatusHistoryModel history, SqlConnection conn, SqlTransaction transaction);
    void SetLastUpdateInfo(int documentDetailId, string approver, DateTime now, SqlConnection conn, SqlTransaction transaction);
    DocumentDetailsModel GetDocumentDetailById(int documentDetailId);


    List<TagModel> GetAllTags();
    void InsertDocumentTag(int documentId, int tagId, SqlConnection conn, SqlTransaction transaction);





    // IDocumentTagsRepository.cs
    List<DocumentTagModel> GetTagsByDocumentDetailsId(int documentDetailsId);



    List<DocumentModel> GetAllDocuments();
    List<DocumentDetailsModel> GetDocumentDetailsByDocumentId(int documentId);


    List<DocumentDetailsModel> Search(int documentId, string tag, string status, string version);
    void DeleteDocumentDetailById(int documentDetailId);



    (DocumentModel, List<DocumentDetailsModel>) GetDocumentWithDetailsForEdit(int documentId);
    void UpdateDocumentInfo(int documentId, string title, string description);
    void UpdateDocumentDetailInfo(int documentDetailId, string description, List<int> tagIds);
    void InsertNewDocumentDetailVersion(int documentDetailId, string description, List<int> tagIds, string fileName, string originalFileName, string createdBy, string approveStatus);

    List<DocumentDetailsModel> GetDocumentVersionHistoryByUser(string userName, int DocumentId);
    List<DocumentDetailsModel> GetDetailsWithNotes(string createdBy);
    List<DocumentDetailsModel> GetRecentApprovedOrPendingDocuments(int count = 5);
    List<DocumentDetailsModel> GetRecentApprovedOrPendingDocumentsUser(string userName, int count = 5);

    List<(int Year, int Month, int Count)> GetMonthlyPendingCount();
    List<(int Year, int Month, int Count)> GetMonthlyApprovedCount();
    List<(int Year, int Month, int Count)> GetMonthlyRejectedCount();

}
