using DMS_Final.Attribute;
using DMS_Final.Models;
using DMS_Final.Services.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

namespace DMS_Final.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }


        [RoleAuthorize("Admin","Developer")]
        [HttpGet]
        public IActionResult UploadDocument()
        {
            var tags = _documentService.GetAllTags();
            ViewBag.Tags = tags;
            return View(new DocumentModel());
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult UploadDocument(DocumentModel model, List<IFormFile> Files, List<string> FileDescriptions, List<List<int>> TagIds)
        {
            if (Files == null || Files.Count == 0 || FileDescriptions == null || FileDescriptions.Count != Files.Count || TagIds == null || TagIds.Count != Files.Count)
            {
                ViewBag.Error = "Please select at least one file, provide a description, and select tags for each.";
                ViewBag.Tags = _documentService.GetAllTags();
                return View(model);
            }
            var createdBy = HttpContext.Session.GetString("UserName");
            var createdFrom = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userRole = HttpContext.Session.GetString("UserRole");
            _documentService.UploadMultipleFiles(model, Files, FileDescriptions, createdBy, createdFrom, userRole, TagIds);
            ViewBag.Message = "Documents uploaded successfully.";
            ViewBag.Tags = _documentService.GetAllTags();
            return RedirectToAction("MyDocuments");
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult MyDocuments()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var myDetails = _documentService.GetMyDocumentDetails(userName);
            return View(myDetails);
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult EditDocument(int documentId)
        {
            // Get the document for the title (optional, for display)
            var document = _documentService.GetDocumentById(documentId);

            // Get the latest files for this document
            var latestFiles = _documentService.GetLatestFilesByDocumentId(documentId);

            ViewBag.DocumentId = documentId;
            ViewBag.DocumentTitle = document?.Title ?? "Document";
            ViewBag.DocumentDescription = document?.Description ?? "";

            return View(latestFiles); // Passes List<DocumentDetailsModel> to the view
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult PendingDocuments()
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = role;
            var userName = HttpContext.Session.GetString("UserName");

            List<DocumentDetailsModel> pendingDocs;
            if (role == "Admin" || role == "Manager")
                pendingDocs = _documentService.GetPendingDocumentDetailsWithHeader();
            else
                pendingDocs = _documentService.GetPendingDocumentDetailsWithHeader(userName);

            return View(pendingDocs);
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult ApproveDocument(int documentId, int documentDetailId, string notes = null)
        {
            var approver = HttpContext.Session.GetString("UserName");
            var approverIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            _documentService.ApproveDocument(documentDetailId, documentId, approver, approverIp, notes);

            return RedirectToAction("PendingDocuments");
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult RejectDocument(int documentId, int documentDetailId, string notes = null)
        {
            var rejector = HttpContext.Session.GetString("UserName");
            var rejectorIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            _documentService.RejectDocument(documentDetailId, documentId, rejector, rejectorIp, notes);

            return RedirectToAction("PendingDocuments");
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult UploadNewVersion(int documentDetailId, int documentId)
        {
            var document = _documentService.GetDocumentById(documentId);
            var documentDetail = _documentService.GetDocumentDetailById(documentDetailId);

            if (document == null || documentDetail == null)
                return NotFound();

            // All tags
            var allTags = _documentService.GetAllTags();
            var selectedTags = _documentService.GetTagsByDocumentDetailsId(documentDetailId);

            ViewBag.AllTags = allTags;
            ViewBag.SelectedTags = selectedTags;

            ViewBag.DocumentDetailId = documentDetailId;
            ViewBag.DocumentId = documentId;
            ViewBag.FileDescription = documentDetail.Description;

            return View(document);
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult UploadNewVersion(int documentDetailId, int documentId, string Title, DocumentModel model, List<IFormFile> Files, List<string> FileDescriptions, List<List<int>> TagIds)
        {
            if (Files == null || Files.Count == 0 || FileDescriptions == null || FileDescriptions.Count != Files.Count)
            {
                ViewBag.Error = "Please select at least one file and provide a description for each.";
                ViewBag.DocumentDetailId = documentDetailId;
                ViewBag.DocumentId = documentId;
                return View(model);
            }

            var createdBy = HttpContext.Session.GetString("UserName");
            var createdFrom = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userRole = HttpContext.Session.GetString("UserRole");

            // Update the document title
            model.Title = Title;

            // Pass TagIds to the service
            _documentService.UploadNewVersionMultiple(documentDetailId, documentId, model, Files, FileDescriptions, createdBy, createdFrom, userRole, TagIds);

            TempData["Message"] = "New version(s) uploaded successfully.";
            return RedirectToAction("MyDocuments");
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult ViewDocument(int documentDetailId)
        {
            var documentDetail = _documentService.GetDocumentDetailById(documentDetailId);
            if (documentDetail == null)
                return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolder, documentDetail.FileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileExtension = Path.GetExtension(documentDetail.FileName).ToLowerInvariant();
            var contentType = fileExtension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, enableRangeProcessing: true);
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult DownloadDocument(int documentDetailId)
        {
            var documentDetail = _documentService.GetDocumentDetailById(documentDetailId);
            if (documentDetail == null)
                return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolder, documentDetail.FileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", documentDetail.OriginalFileName);
        }



        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult AllDocuments()
        {
            var documents = _documentService.GetAllDocuments();
            return View(documents);
        }


        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult ShowDocumentDetails(int id, string tag, string status, string version)
        {
            var document = _documentService.GetDocumentById(id);

            List<DocumentDetailsModel> details;
            // If any search parameter is provided, use search
            if (!string.IsNullOrWhiteSpace(tag) || !string.IsNullOrWhiteSpace(status) || !string.IsNullOrWhiteSpace(version))
            {
                details = _documentService.Search(id, tag, status, version);
            }
            else
            {
                details = _documentService.GetDocumentDetailsByDocumentId(id);
            }

            // Get all tags from the database and pass to the view
            var allTags = _documentService.GetAllTags();
            ViewBag.AllTags = allTags;

            ViewBag.Document = document;
            return View(details);
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult DeleteDocumentDetail(int documentDetailId)
        {
            _documentService.DeleteDocumentDetailById(documentDetailId);
            return RedirectToAction("MyDocuments");
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult EditDocumentFull(int documentId)
        {
            var document = _documentService.GetDocumentById(documentId);
            var details = _documentService.GetDocumentDetailsByDocumentId(documentId);
            var allTags = _documentService.GetAllTags();

            ViewBag.Document = document;
            ViewBag.AllTags = allTags;
            return View(details); 
        }

        // FIXED Save edits to document info and file info (ADO.NET only)
        [HttpPost]
        public IActionResult SaveDocumentEdits(
    int DocumentId,
    string Title,
    string Description,
    List<int> FileIds,
    List<string> FileDescriptions,
    List<List<int>> TagIds,
    List<List<IFormFile>> NewFiles)
        {
            // Update document info
            _documentService.UpdateDocumentInfo(DocumentId, Title, Description);

            var createdBy = HttpContext.Session.GetString("UserName");
            var createdFrom = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userRole = HttpContext.Session.GetString("UserRole");
            var approveStatus = (userRole == "Admin" || userRole == "Manager") ? "approved" : "pending";

            for (int i = 0; i < FileIds.Count; i++)
            {
                var prevDetail = _documentService.GetDocumentDetailById(FileIds[i]);

                // --- Check for new files ---
                bool hasNewFiles = false;
                List<IFormFile> validFiles = new List<IFormFile>();

                if (NewFiles != null && NewFiles.Count > i && NewFiles[i] != null)
                {
                    foreach (var file in NewFiles[i])
                    {
                        if (file != null && file.Length > 0)
                        {
                            validFiles.Add(file);
                        }
                    }
                    hasNewFiles = validFiles.Count > 0;
                }

                // --- Description changed? ---
                string prevDesc = prevDetail.Description == null ? "" : prevDetail.Description.Trim();
                string newDesc = FileDescriptions[i] == null ? "" : FileDescriptions[i].Trim();
                bool descriptionChanged = !prevDesc.Equals(newDesc, StringComparison.OrdinalIgnoreCase);

                // --- Tags changed? ---
                List<string> prevTags = new List<string>();
                if (prevDetail.Tags != null)
                {
                    foreach (var t in prevDetail.Tags)
                        prevTags.Add(t);
                }
                prevTags.Sort();

                List<string> newTagNames = new List<string>();
                if (TagIds != null && TagIds.Count > i && TagIds[i] != null)
                {
                    List<int> tagIdList = TagIds[i];
                    var allTags = _documentService.GetAllTags();
                    foreach (var tagId in tagIdList)
                    {
                        string tagName = null;
                        foreach (var tag in allTags)
                        {
                            if (tag.Id == tagId)
                            {
                                tagName = tag.Name;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(tagName))
                            newTagNames.Add(tagName);
                    }
                }
                newTagNames.Sort();

                bool tagsChanged = false;
                if (prevTags.Count != newTagNames.Count)
                {
                    tagsChanged = true;
                }
                else
                {
                    for (int j = 0; j < prevTags.Count; j++)
                    {
                        if (!prevTags[j].Equals(newTagNames[j], StringComparison.OrdinalIgnoreCase))
                        {
                            tagsChanged = true;
                            break;
                        }
                    }
                }

                // --- Create new version if something changed ---
                if (hasNewFiles || descriptionChanged || tagsChanged)
                {
                    if (hasNewFiles)
                    {
                        foreach (var file in validFiles)
                        {
                            string originalFileName = Path.GetFileName(file.FileName);
                            string fileName = Path.GetFileNameWithoutExtension(originalFileName) +
                                              "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") +
                                              Path.GetExtension(originalFileName);

                            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);

                            string filePath = Path.Combine(uploadsFolder, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                                file.CopyTo(stream);

                            _documentService.InsertNewDocumentDetailVersion(
                                FileIds[i],
                                FileDescriptions[i],
                                TagIds[i] ?? new List<int>(),
                                fileName,
                                originalFileName,
                                createdBy,
                                approveStatus,
                                "Document edited by user"
                            );
                        }
                    }
                    else
                    {
                        // No new files, but description or tags changed - create new version with same file
                        _documentService.InsertNewDocumentDetailVersion(
                            FileIds[i],
                            FileDescriptions[i],
                            TagIds[i] ?? new List<int>(),
                            prevDetail.FileName,
                            prevDetail.OriginalFileName,
                            createdBy,
                            approveStatus,
                            "Document edited by user"
                        );
                    }
                }
            }

            TempData["Message"] = "Document and files updated successfully.";
            return RedirectToAction("MyDocuments");
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult DocumentVersionHistory(int documentId)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var versionHistory = _documentService.GetDocumentVersionHistoryByUser(userName, documentId);

            // Get the title from the first item if available
            string title = versionHistory.Any() ? versionHistory.First().Title : string.Empty;
            ViewBag.Title = title;

            return View(versionHistory);
        }


        [RoleAuthorize("Developer")]
        [HttpGet]
        public IActionResult DetailsWithNotes()
        {
            var createdBy = HttpContext.Session.GetString("UserName");
            var details = _documentService.GetDetailsWithNotes(createdBy);
            return View(details);
        }


    }
}