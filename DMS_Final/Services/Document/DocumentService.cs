using DMS_Final.Models;
using DMS_Final.Repository.Document;
using DMS_Final.Repository.DocumentTags;
using DMS_Final.Repository.User;
using DMS_Final.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly INotificationService _notificationService;
    private readonly string _connectionString;
    private readonly IUserRepository _userRepository;

    public DocumentService(IDocumentRepository documentRepository, IConfiguration configuration, INotificationService notificationService, IUserRepository userRepository)
    {
        _documentRepository = documentRepository;
        _notificationService = notificationService;
        _userRepository = userRepository;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public List<DocumentModel> GetDocumentsByUser(string userName)
    {
        return _documentRepository.GetDocumentsByUser(userName);
    }

    public List<DocumentModel> GetMyDocumentDetails(string userName)
    {
        return _documentRepository.GetMyDocumentDetails(userName);
    }

    public List<DocumentDetailsModel> GetPendingDocumentDetailsWithHeader(string userName = null)
    {
        return _documentRepository.GetPendingDocumentDetailsWithHeader(userName);
    }


    public void UploadMultipleFiles(DocumentModel model, List<IFormFile> files, List<string> fileDescriptions, string createdBy, string createdFrom, string userRole, List<List<int>> tagIds)
    {
        if (files == null || fileDescriptions == null || tagIds == null ||
            files.Count == 0 ||
            fileDescriptions.Count != files.Count ||
            tagIds.Count != files.Count)
        {
            throw new ArgumentException("Files, descriptions, and tag lists must all be present and have the same count.");
        }

        bool isAdminOrManager = userRole == "Admin" || userRole == "Manager";
        bool isArchive = !isAdminOrManager;
        string status = isAdminOrManager ? "approved" : "pending";

        // Store document detail info for notifications
        var uploadedFiles = new List<(int documentDetailId, string originalFileName)>();
        int documentId = 0;

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    model.CreatedBy = createdBy;
                    model.CreatedOn = DateTime.Now;
                    model.CreatedFrom = createdFrom;
                    documentId = _documentRepository.InsertDocument(model, conn, transaction);

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var fileDescription = fileDescriptions[i];
                        var fileTagIds = tagIds[i];

                        string originalFileName = Path.GetFileName(file.FileName);
                        string fileName = Path.GetFileNameWithoutExtension(originalFileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(originalFileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        var details = new DocumentDetailsModel
                        {
                            DocumentId = documentId,
                            OriginalFileName = originalFileName,
                            FileName = fileName,
                            Description = fileDescription,
                            VersionNumber = 1,
                            CreatedBy = createdBy,
                            CreatedOn = DateTime.Now,
                            ApproveStatus = status,
                            IsArchive = isArchive,
                            ParentDocumentId = 0
                        };

                        int documentDetailId = _documentRepository.InsertDocumentDetails(details, conn, transaction);
                        uploadedFiles.Add((documentDetailId, originalFileName));

                        foreach (var tagId in fileTagIds)
                        {
                            _documentRepository.InsertDocumentTag(documentDetailId, tagId, conn, transaction);
                        }

                        var history = new DocumentStatusHistoryModel
                        {
                            DocumentId = documentId,
                            DocumentDetailId = documentDetailId,
                            ApproveStatus = status,
                            CreatedBy = createdBy,
                            CreatedOn = DateTime.Now,
                            CreatedFrom = createdFrom,
                            Notes = isAdminOrManager ? "Document auto-approved" : "Document upload pending approval"
                        };
                        _documentRepository.InsertStatusHistory(history, conn, transaction);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Create notifications AFTER the main transaction is committed
        try
        {
            if (status == "pending" || status == "approved")
            {
                var adminUserIds = _userRepository.GetUsersByRole("Admin").Select(u => u.Id).ToList();
                var uploaderUserId = _userRepository.GetUserIdByUsername(createdBy);

                if (uploaderUserId > 0)
                {
                    // Filter out the uploader from admin notification list
                    // Only notify other admins, not the one who uploaded
                    var adminsToNotify = adminUserIds.Where(adminId => adminId != uploaderUserId).ToList();

                    foreach (var fileInfo in uploadedFiles)
                    {
                        // Different message based on status
                        string notificationMessage;
                        string eventType;

                        if (status == "pending")
                        {
                            notificationMessage = $"A new file '{fileInfo.originalFileName}' from document '{model.Title}' has been uploaded by {createdBy} and is pending your review.";
                            eventType = "document_pending";
                        }
                        else // approved
                        {
                            notificationMessage = $"A new file '{fileInfo.originalFileName}' from document '{model.Title}' has been uploaded by {createdBy}.";
                            eventType = "document_approved";
                        }

                        // Only notify other admins, not the uploading admin
                        foreach (var adminId in adminsToNotify)
                        {
                            try
                            {
                                _notificationService.CreateNotification(
                                    targetUserId: adminId,
                                    actorUserId: uploaderUserId,
                                    eventType: eventType,
                                    message: notificationMessage,
                                    documentId: documentId,
                                    documentDetailId: fileInfo.documentDetailId
                                );
                            }
                            catch (Exception ex)
                            {
                                // Log the error but don't fail the entire upload
                                Console.WriteLine($"Failed to create notification: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the entire upload
            Console.WriteLine($"Failed to create notifications: {ex.Message}");
        }
    }




    //public void ApproveDocument(int documentDetailId, int documentId, string approver, string approverIp, string notes = null)
    //{
    //    using (SqlConnection conn = new SqlConnection(_connectionString))
    //    {
    //        conn.Open();
    //        using (SqlTransaction transaction = conn.BeginTransaction())
    //        {
    //            try
    //            {
    //                _documentRepository.SetApproveStatus(documentDetailId, "approved", conn, transaction);
    //                _documentRepository.SetArchiveStatus(documentDetailId, false, conn, transaction);

    //                // Set last update info
    //                _documentRepository.SetLastUpdateInfo(documentDetailId, approver, DateTime.Now, conn, transaction);

    //                var history = new DocumentStatusHistoryModel
    //                {
    //                    DocumentId = documentId,
    //                    DocumentDetailId = documentDetailId,
    //                    ApproveStatus = "approved",
    //                    CreatedBy = approver,
    //                    CreatedOn = DateTime.Now,
    //                    CreatedFrom = approverIp,
    //                    Notes = notes ?? "Document approved"
    //                };
    //                _documentRepository.InsertStatusHistory(history, conn, transaction);

    //                transaction.Commit();
    //            }
    //            catch
    //            {
    //                transaction.Rollback();
    //                throw;
    //            }
    //        }
    //    }
    //}

    //public void RejectDocument(int documentDetailId, int documentId, string rejector, string rejectorIp, string notes = null)
    //{
    //    using (SqlConnection conn = new SqlConnection(_connectionString))
    //    {
    //        conn.Open();
    //        using (SqlTransaction transaction = conn.BeginTransaction())
    //        {
    //            try
    //            {
    //                _documentRepository.SetApproveStatus(documentDetailId, "rejected", conn, transaction);
    //                _documentRepository.SetArchiveStatus(documentDetailId, true, conn, transaction);

    //                // Set last update info
    //                _documentRepository.SetLastUpdateInfo(documentDetailId, rejector, DateTime.Now, conn, transaction);


    //                var history = new DocumentStatusHistoryModel
    //                {
    //                    DocumentId = documentId,
    //                    DocumentDetailId = documentDetailId,
    //                    ApproveStatus = "rejected",
    //                    CreatedBy = rejector,
    //                    CreatedOn = DateTime.Now,
    //                    CreatedFrom = rejectorIp,
    //                    Notes = notes ?? "Document rejected"
    //                };
    //                _documentRepository.InsertStatusHistory(history, conn, transaction);

    //                transaction.Commit();
    //            }
    //            catch
    //            {
    //                transaction.Rollback();
    //                throw;
    //            }
    //        }
    //    }
    //}


    public void ApproveDocument(int documentDetailId, int documentId, string approver, string approverIp, string notes = null)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    _documentRepository.SetApproveStatus(documentDetailId, "approved", conn, transaction);
                    _documentRepository.SetArchiveStatus(documentDetailId, false, conn, transaction);
                    // Set last update info
                    _documentRepository.SetLastUpdateInfo(documentDetailId, approver, DateTime.Now, conn, transaction);
                    var history = new DocumentStatusHistoryModel
                    {
                        DocumentId = documentId,
                        DocumentDetailId = documentDetailId,
                        ApproveStatus = "approved",
                        CreatedBy = approver,
                        CreatedOn = DateTime.Now,
                        CreatedFrom = approverIp,
                        Notes = notes ?? "Document approved"
                    };
                    _documentRepository.InsertStatusHistory(history, conn, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Send notification to the document uploader after transaction commit
        try
        {
            // Get document details to find the uploader
            var documentDetail = _documentRepository.GetDocumentDetailById(documentDetailId);
            var document = _documentRepository.GetDocumentById(documentId);

            if (documentDetail != null && document != null)
            {
                var uploaderUserId = _userRepository.GetUserIdByUsername(documentDetail.CreatedBy);
                var approverUserId = _userRepository.GetUserIdByUsername(approver);

                if (uploaderUserId > 0 && approverUserId > 0)
                {
                    string notificationMessage = $"Your document '{document.Title}' - file '{documentDetail.OriginalFileName}' has been approved by {approver}.";

                    _notificationService.CreateNotification(
                        targetUserId: uploaderUserId,
                        actorUserId: approverUserId,
                        eventType: "document_approved",
                        message: notificationMessage,
                        documentId: documentId,
                        documentDetailId: documentDetailId
                    );
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the approval
            Console.WriteLine($"Failed to create approval notification: {ex.Message}");
        }
    }

    public void RejectDocument(int documentDetailId, int documentId, string rejector, string rejectorIp, string notes = null)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    _documentRepository.SetApproveStatus(documentDetailId, "rejected", conn, transaction);
                    _documentRepository.SetArchiveStatus(documentDetailId, true, conn, transaction);
                    // Set last update info
                    _documentRepository.SetLastUpdateInfo(documentDetailId, rejector, DateTime.Now, conn, transaction);
                    var history = new DocumentStatusHistoryModel
                    {
                        DocumentId = documentId,
                        DocumentDetailId = documentDetailId,
                        ApproveStatus = "rejected",
                        CreatedBy = rejector,
                        CreatedOn = DateTime.Now,
                        CreatedFrom = rejectorIp,
                        Notes = notes ?? "Document rejected"
                    };
                    _documentRepository.InsertStatusHistory(history, conn, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Send notification to the document uploader after transaction commit
        try
        {
            // Get document details to find the uploader
            var documentDetail = _documentRepository.GetDocumentDetailById(documentDetailId);
            var document = _documentRepository.GetDocumentById(documentId);

            if (documentDetail != null && document != null)
            {
                var uploaderUserId = _userRepository.GetUserIdByUsername(documentDetail.CreatedBy);
                var rejectorUserId = _userRepository.GetUserIdByUsername(rejector);

                if (uploaderUserId > 0 && rejectorUserId > 0)
                {
                    string notificationMessage = $"Your document '{document.Title}' - file '{documentDetail.OriginalFileName}' has been rejected by {rejector}.";

                    // Include rejection reason if provided
                    if (!string.IsNullOrEmpty(notes) && notes != "Document rejected")
                    {
                        notificationMessage += $" Reason: {notes}";
                    }

                    _notificationService.CreateNotification(
                        targetUserId: uploaderUserId,
                        actorUserId: rejectorUserId,
                        eventType: "document_rejected",
                        message: notificationMessage,
                        documentId: documentId,
                        documentDetailId: documentDetailId
                    );
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the rejection
            Console.WriteLine($"Failed to create rejection notification: {ex.Message}");
        }
    }

    //public void UploadNewVersionMultiple(
    //    int parentDetailId,
    //    int documentId,
    //    DocumentModel model,
    //    List<IFormFile> files,
    //    List<string> fileDescriptions,
    //    string createdBy,
    //    string createdFrom,
    //    string userRole,
    //    List<List<int>> TagIds)
    //{
    //    bool isAdminOrManager = userRole == "Admin" || userRole == "Manager";
    //    bool isArchive = !isAdminOrManager;
    //    string status = isAdminOrManager ? "approved" : "pending";

    //    using (SqlConnection conn = new SqlConnection(_connectionString))
    //    {
    //        conn.Open();
    //        using (SqlTransaction transaction = conn.BeginTransaction())
    //        {
    //            try
    //            {
    //                // Mark previous detail as superseded
    //                _documentRepository.SetApproveStatus(parentDetailId, "superseded", conn, transaction);

    //                // Get previous version number
    //                int previousVersion = 1;
    //                string getVersionSql = "SELECT VersionNumber FROM DocumentDetails WHERE Id = @Id";
    //                using (var cmd = new SqlCommand(getVersionSql, conn, transaction))
    //                {
    //                    cmd.Parameters.AddWithValue("@Id", parentDetailId);
    //                    var result = cmd.ExecuteScalar();
    //                    if (result != null)
    //                        previousVersion = Convert.ToInt32(result);
    //                }

    //                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    //                if (!Directory.Exists(uploadsFolder))
    //                    Directory.CreateDirectory(uploadsFolder);

    //                for (int i = 0; i < files.Count; i++)
    //                {
    //                    var file = files[i];
    //                    var fileDescription = fileDescriptions[i];
    //                    var fileTagIds = TagIds[i];

    //                    string originalFileName = Path.GetFileName(file.FileName);
    //                    string fileName = Path.GetFileNameWithoutExtension(originalFileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(originalFileName);
    //                    string filePath = Path.Combine(uploadsFolder, fileName);

    //                    using (var stream = new FileStream(filePath, FileMode.Create))
    //                    {
    //                        file.CopyTo(stream);
    //                    }

    //                    var details = new DocumentDetailsModel
    //                    {
    //                        DocumentId = documentId,
    //                        OriginalFileName = originalFileName,
    //                        FileName = fileName,
    //                        Description = fileDescription,
    //                        VersionNumber = previousVersion + 1,
    //                        CreatedBy = createdBy,
    //                        CreatedOn = DateTime.Now,
    //                        ApproveStatus = status,
    //                        IsArchive = isArchive,
    //                        ParentDocumentId = parentDetailId,
    //                        LastUpdateBy = createdBy,
    //                        LastUpdateOn = DateTime.Now
    //                    };
    //                    int documentDetailId = _documentRepository.InsertDocumentDetails(details, conn, transaction);

    //                    // Insert tags for this new version
    //                    foreach (var tagId in fileTagIds)
    //                    {
    //                        _documentRepository.InsertDocumentTag(documentDetailId, tagId, conn, transaction);
    //                    }

    //                    var history = new DocumentStatusHistoryModel
    //                    {
    //                        DocumentId = documentId,
    //                        DocumentDetailId = documentDetailId,
    //                        ApproveStatus = status,
    //                        CreatedBy = createdBy,
    //                        CreatedOn = DateTime.Now,
    //                        CreatedFrom = createdFrom,
    //                        Notes = isAdminOrManager ? "New version auto-approved" : "New version pending approval"
    //                    };
    //                    _documentRepository.InsertStatusHistory(history, conn, transaction);
    //                }

    //                transaction.Commit();
    //            }
    //            catch
    //            {
    //                transaction.Rollback();
    //                throw;
    //            }
    //        }
    //    }
    //}


    public void UploadNewVersionMultiple(
        int parentDetailId,
        int documentId,
        DocumentModel model,
        List<IFormFile> files,
        List<string> fileDescriptions,
        string createdBy,
        string createdFrom,
        string userRole,
        List<List<int>> TagIds)
    {
        bool isAdminOrManager = userRole == "Admin" || userRole == "Manager";
        bool isArchive = !isAdminOrManager;
        string status = isAdminOrManager ? "approved" : "pending";

        // Store uploaded file info for notifications
        var uploadedNewVersions = new List<(int documentDetailId, string originalFileName, int versionNumber)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    // Mark previous detail as superseded
                    _documentRepository.SetApproveStatus(parentDetailId, "superseded", conn, transaction);

                    // Get previous version number
                    int previousVersion = 1;
                    string getVersionSql = "SELECT VersionNumber FROM DocumentDetails WHERE Id = @Id";
                    using (var cmd = new SqlCommand(getVersionSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Id", parentDetailId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                            previousVersion = Convert.ToInt32(result);
                    }

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    int newVersionNumber = previousVersion + 1;

                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var fileDescription = fileDescriptions[i];
                        var fileTagIds = TagIds[i];

                        string originalFileName = Path.GetFileName(file.FileName);
                        string fileName = Path.GetFileNameWithoutExtension(originalFileName) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(originalFileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        var details = new DocumentDetailsModel
                        {
                            DocumentId = documentId,
                            OriginalFileName = originalFileName,
                            FileName = fileName,
                            Description = fileDescription,
                            VersionNumber = newVersionNumber,
                            CreatedBy = createdBy,
                            CreatedOn = DateTime.Now,
                            ApproveStatus = status,
                            IsArchive = isArchive,
                            ParentDocumentId = parentDetailId,
                            LastUpdateBy = createdBy,
                            LastUpdateOn = DateTime.Now
                        };
                        int documentDetailId = _documentRepository.InsertDocumentDetails(details, conn, transaction);

                        // Store for notifications
                        uploadedNewVersions.Add((documentDetailId, originalFileName, newVersionNumber));

                        // Insert tags for this new version
                        foreach (var tagId in fileTagIds)
                        {
                            _documentRepository.InsertDocumentTag(documentDetailId, tagId, conn, transaction);
                        }

                        var history = new DocumentStatusHistoryModel
                        {
                            DocumentId = documentId,
                            DocumentDetailId = documentDetailId,
                            ApproveStatus = status,
                            CreatedBy = createdBy,
                            CreatedOn = DateTime.Now,
                            CreatedFrom = createdFrom,
                            Notes = isAdminOrManager ? "New version auto-approved" : "New version pending approval"
                        };
                        _documentRepository.InsertStatusHistory(history, conn, transaction);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Create notifications AFTER the main transaction is committed
        try
        {
            if (status == "pending" || status == "approved")
            {
                var adminUserIds = _userRepository.GetUsersByRole("Admin").Select(u => u.Id).ToList();
                var uploaderUserId = _userRepository.GetUserIdByUsername(createdBy);

                if (uploaderUserId > 0)
                {
                    // Get document details from database
                    var document = _documentRepository.GetDocumentById(documentId);
                    string documentTitle = document?.Title ?? "Unknown Document";
                    string documentDescription = document?.Description ?? "";

                    // Filter out the uploader from admin notification list
                    // Only notify other admins, not the one who uploaded
                    var adminsToNotify = adminUserIds.Where(adminId => adminId != uploaderUserId).ToList();

                    foreach (var versionInfo in uploadedNewVersions)
                    {
                        // Different message based on status
                        string notificationMessage;
                        string eventType;

                        if (status == "pending")
                        {
                            notificationMessage = $"A new version (v{versionInfo.versionNumber}) of file '{versionInfo.originalFileName}' from document '{documentTitle}' has been uploaded by {createdBy} and is pending your review.";
                            eventType = "document_version_pending";
                        }
                        else // approved
                        {
                            notificationMessage = $"A new version (v{versionInfo.versionNumber}) of file '{versionInfo.originalFileName}' from document '{documentTitle}' has been uploaded and approved by {createdBy}.";
                            eventType = "document_version_approved";
                        }

                        // Only notify other admins, not the uploading admin
                        foreach (var adminId in adminsToNotify)
                        {
                            try
                            {
                                _notificationService.CreateNotification(
                                    targetUserId: adminId,
                                    actorUserId: uploaderUserId,
                                    eventType: eventType,
                                    message: notificationMessage,
                                    documentId: documentId,
                                    documentDetailId: versionInfo.documentDetailId
                                );
                            }
                            catch (Exception ex)
                            {
                                // Log the error but don't fail the entire upload
                                Console.WriteLine($"Failed to create new version notification: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the entire upload
            Console.WriteLine($"Failed to create new version notifications: {ex.Message}");
        }
    }

    public DocumentModel GetDocumentById(int documentId)
    {
        return _documentRepository.GetDocumentById(documentId);
    }

    public DocumentDetailsModel GetDocumentDetailById(int documentDetailId)
    {
        return _documentRepository.GetDocumentDetailById(documentDetailId);
    }

    public List<DocumentDetailsModel> GetLatestFilesByDocumentId(int documentId)
    {
        return _documentRepository.GetLatestFilesByDocumentId(documentId);
    }

    public List<TagModel> GetAllTags()
    {
        return _documentRepository.GetAllTags();
    }

    public List<DocumentTagModel> GetTagsByDocumentDetailsId(int documentDetailsId)
    {
        return _documentRepository.GetTagsByDocumentDetailsId(documentDetailsId);
    }

    public List<DocumentModel> GetAllDocuments()
    {
        return _documentRepository.GetAllDocuments();
    }

    public List<DocumentDetailsModel> GetDocumentDetailsByDocumentId(int documentId)
    {
        return _documentRepository.GetDocumentDetailsByDocumentId(documentId);
    }

    public List<DocumentDetailsModel> Search(int documentId, string tag, string status, string version)
    {
        return _documentRepository.Search(documentId, tag, status, version);
    }

    public void DeleteDocumentDetailById(int documentDetailId)
    {
        _documentRepository.DeleteDocumentDetailById(documentDetailId);
    }

    public (DocumentModel, List<DocumentDetailsModel>) GetDocumentWithDetailsForEdit(int documentId)
    {
        return _documentRepository.GetDocumentWithDetailsForEdit(documentId);
    }

    public void UpdateDocumentInfo(int documentId, string title, string description)
    {
        _documentRepository.UpdateDocumentInfo(documentId, title, description);
    }

    public void UpdateDocumentDetailInfo(int documentDetailId, string description, List<int> tagIds)
    {
        _documentRepository.UpdateDocumentDetailInfo(documentDetailId, description, tagIds);
    }

    public void InsertNewDocumentDetailVersion(
    int parentDetailId,
    string description,
    List<int> tagIds,
    string fileName,
    string originalFileName,
    string createdBy,
    string approveStatus,
    string notes = null)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Mark previous detail as superseded
                    _documentRepository.SetApproveStatus(parentDetailId, "superseded", conn, transaction);
                    // Mark IsArchive as 1
                    _documentRepository.SetArchiveStatus(parentDetailId, true, conn, transaction);
                    // Get previous version number and documentId
                    int previousVersion = 1;
                    int documentId = 0;
                    string getVersionSql = "SELECT VersionNumber, DocumentId FROM DocumentDetails WHERE Id = @Id";
                    using (var cmd = new SqlCommand(getVersionSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Id", parentDetailId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                previousVersion = Convert.ToInt32(reader["VersionNumber"]);
                                documentId = Convert.ToInt32(reader["DocumentId"]);
                            }
                        }
                    }

                    // Insert new version
                    var details = new DocumentDetailsModel
                    {
                        DocumentId = documentId,
                        OriginalFileName = originalFileName,
                        FileName = fileName,
                        Description = description,
                        VersionNumber = previousVersion + 1,
                        CreatedBy = createdBy,
                        CreatedOn = DateTime.Now,
                        ApproveStatus = approveStatus,
                        IsArchive = false,
                        ParentDocumentId = parentDetailId,
                        LastUpdateBy = createdBy,
                        LastUpdateOn = DateTime.Now
                    };
                    int documentDetailId = _documentRepository.InsertDocumentDetails(details, conn, transaction);

                    // Insert tags
                    foreach (var tagId in tagIds)
                    {
                        _documentRepository.InsertDocumentTag(documentDetailId, tagId, conn, transaction);
                    }

                    // Insert status history
                    var history = new DocumentStatusHistoryModel
                    {
                        DocumentId = documentId,
                        DocumentDetailId = documentDetailId,
                        ApproveStatus = approveStatus,
                        CreatedBy = createdBy,
                        CreatedOn = DateTime.Now,
                        CreatedFrom = "::1",
                        Notes = notes ?? "Document version updated"
                    };
                    _documentRepository.InsertStatusHistory(history, conn, transaction);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public List<DocumentDetailsModel> GetDocumentVersionHistoryByUser(string userName, int DocumentId)
    {
        return _documentRepository.GetDocumentVersionHistoryByUser(userName, DocumentId);
    }

    public List<DocumentDetailsModel> GetDetailsWithNotes(string createdBy)
    {
        return _documentRepository.GetDetailsWithNotes(createdBy);
    }


    public List<DocumentDetailsModel> GetRecentApprovedOrPendingDocuments(int count = 5)
    {
        return _documentRepository.GetRecentApprovedOrPendingDocuments(count);
    }


    public List<DocumentDetailsModel> GetRecentApprovedOrPendingDocumentsUser(string userName, int count = 5)
    {
        return _documentRepository.GetRecentApprovedOrPendingDocumentsUser(userName,count);
    }

    public List<(int Year, int Month, int Count)> GetMonthlyPendingCount()
        => _documentRepository.GetMonthlyPendingCount();

    public List<(int Year, int Month, int Count)> GetMonthlyApprovedCount()
        => _documentRepository.GetMonthlyApprovedCount();

    public List<(int Year, int Month, int Count)> GetMonthlyRejectedCount()
        => _documentRepository.GetMonthlyRejectedCount();
}