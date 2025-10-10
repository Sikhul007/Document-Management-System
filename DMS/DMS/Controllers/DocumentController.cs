using DMS.Attribute;
using DMS.Models;
using DMS.Services.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;

namespace DMS.Controllers
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
        public IActionResult MyDocuments(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var userName = HttpContext.Session.GetString("UserName");

            // For AJAX requests, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var result = _documentService.GetMyDocumentDetails(userName, page, pageSize, searchTerm, sortColumn, sortDirection);
                return Json(result);
            }

            // For initial page load, return View with empty model
            return View(new List<DocumentModel>());
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult EditDocument(int documentId)
        {
            var document = _documentService.GetDocumentById(documentId);

            var latestFiles = _documentService.GetLatestFilesByDocumentId(documentId);

            ViewBag.DocumentId = documentId;
            ViewBag.DocumentTitle = document?.Title ?? "Document";
            ViewBag.DocumentDescription = document?.Description ?? "";

            return View(latestFiles);
        }



        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult PendingDocuments(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var role = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = role;
            var userName = HttpContext.Session.GetString("UserName");

            // For AJAX requests, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                object result;
                if (role == "Admin" || role == "Manager")
                    result = _documentService.GetPendingDocumentDetailsWithHeader(page, pageSize, searchTerm, sortColumn, sortDirection, null);
                else
                    result = _documentService.GetPendingDocumentDetailsWithHeader(page, pageSize, searchTerm, sortColumn, sortDirection, userName);

                return Json(result);
            }

            // For initial page load, return View with empty model
            return View(new List<DocumentDetailsModel>());
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

            model.Title = Title;

            _documentService.UploadNewVersionMultiple(documentDetailId, documentId, model, Files, FileDescriptions, createdBy, createdFrom, userRole, TagIds);

            TempData["Message"] = "New version(s) uploaded successfully.";
            return RedirectToAction("EditDocument", "Document", new { documentId = documentId });
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
        public IActionResult AllDocuments(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            // For AJAX requests, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var result = _documentService.GetAllDocuments(page, pageSize, searchTerm, sortColumn, sortDirection);
                return Json(result);
            }

            // For initial page load, return View with empty model
            return View(new List<DocumentModel>());
        }



        [RoleAuthorize("Admin")]
        [HttpGet]
        public IActionResult ShowDocumentDetails(int id, string tag, string status, string version)
        {
            var document = _documentService.GetDocumentById(id);

            List<DocumentDetailsModel> details;
            if (!string.IsNullOrWhiteSpace(tag) || !string.IsNullOrWhiteSpace(status) || !string.IsNullOrWhiteSpace(version))
            {
               details = _documentService.Search(id, tag, status, version);
            }
            else
            {
                details = _documentService.GetDocumentDetailsByDocumentId(id);
            }

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



        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult SaveDocumentEdits(int DocumentId,string Title,string Description,List<int> FileIds,List<string> FileDescriptions,List<List<int>> TagIds,List<List<IFormFile>> NewFiles)
        {
            _documentService.UpdateDocumentInfo(DocumentId, Title, Description);

            var createdBy = HttpContext.Session.GetString("UserName");
            var createdFrom = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userRole = HttpContext.Session.GetString("UserRole");
            var approveStatus = (userRole == "Admin" || userRole == "Manager") ? "approved" : "pending";

            for (int i = 0; i < FileIds.Count; i++)
            {
                var prevDetail = _documentService.GetDocumentDetailById(FileIds[i]);

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

                string prevDesc = prevDetail.Description == null ? "" : prevDetail.Description.Trim();
                string newDesc = FileDescriptions[i] == null ? "" : FileDescriptions[i].Trim();
                bool descriptionChanged = !prevDesc.Equals(newDesc, StringComparison.OrdinalIgnoreCase);

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
        public IActionResult DetailsWithNotes(int page = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "", string sortDirection = "asc")
        {
            var createdBy = HttpContext.Session.GetString("UserName");

            // For AJAX requests, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var result = _documentService.GetDetailsWithNotes(createdBy, page, pageSize, searchTerm, sortColumn, sortDirection);
                return Json(result);
            }

            // For initial page load, return View with empty model
            return View(new List<DocumentDetailsModel>());
        }








        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult CreateTag([FromBody] CreateTagRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TagName))
                {
                    return BadRequest(new { success = false, message = "Tag name cannot be empty." });
                }

                var newTag = _documentService.CreateTag(request.TagName);
                return Json(new { success = true, tag = new { id = newTag.Id, text = newTag.Name } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error creating tag: " + ex.Message });
            }
        }

        public class CreateTagRequest
        {
            public string TagName { get; set; }
        }


        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult SearchTags(string term)
        {
            try
            {
                var allTags = _documentService.GetAllTags();

                var filteredTags = string.IsNullOrEmpty(term)
                    ? allTags
                    : allTags.Where(t => t.Name.ToLower().Contains(term.ToLower())).ToList();

                var results = filteredTags.Select(t => new {
                    id = t.Id,
                    text = t.Name
                }).ToList();

                return Json(new { results = results });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to search tags: " + ex.Message });
            }
        }









        [RoleAuthorize("Admin", "Developer")]
        [HttpGet]
        public IActionResult AddNewDocumentVersion(int documentId)
        {
            var document = _documentService.GetDocumentById(documentId);
            if (document == null)
                return NotFound();

            // Get the highest version number for this document
            var latestFiles = _documentService.GetLatestFilesByDocumentId(documentId);
            int nextVersion = 1;
            if (latestFiles.Any())
            {
                nextVersion = latestFiles.Max(f => f.VersionNumber) + 1;
            }

            var allTags = _documentService.GetAllTags();

            ViewBag.AllTags = allTags;
            ViewBag.DocumentId = documentId;
            ViewBag.NextVersion = nextVersion;
            ViewBag.DocumentTitle = document.Title;
            ViewBag.DocumentDescription = document.Description;

            return View(document);
        }

        [RoleAuthorize("Admin", "Developer")]
        [HttpPost]
        public IActionResult AddNewDocumentVersion(int documentId, string Title, DocumentModel model, List<IFormFile> Files, List<string> FileDescriptions, List<List<int>> TagIds)
        {
            if (Files == null || Files.Count == 0 || FileDescriptions == null || FileDescriptions.Count != Files.Count)
            {
                ViewBag.Error = "Please select at least one file and provide a description for each.";
                ViewBag.DocumentId = documentId;
                return View(model);
            }

            var createdBy = HttpContext.Session.GetString("UserName");
            var createdFrom = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userRole = HttpContext.Session.GetString("UserRole");

            model.Title = Title;

            _documentService.AddNewDocumentVersionToDocument(documentId, model, Files, FileDescriptions, createdBy, createdFrom, userRole, TagIds);

            TempData["Message"] = "New document version(s) uploaded successfully.";
            return RedirectToAction("EditDocument", "Document", new { documentId = documentId });
        }
    }
}