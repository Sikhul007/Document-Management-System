using DMS_Final.Attribute;
using DMS_Final.Models;
using DMS_Final.Services;
using DMS_Final.Services.Pagination.DocumentStatusHistory;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DMS_Final.Controllers
{
    public class DocumentStatusHistoryController : Controller
    {
        private readonly IDocumentStatusHistoryService _documentStatusHistoryService;
        private readonly IDocumentStatusHistoryPaginationService _documentStatusHistoryPaginationService;

        public DocumentStatusHistoryController(IDocumentStatusHistoryService documentStatusHistoryService, IDocumentStatusHistoryPaginationService documentStatusHistoryPaginationService)
        {
            _documentStatusHistoryService = documentStatusHistoryService;
            _documentStatusHistoryPaginationService = documentStatusHistoryPaginationService;

        }


        [RoleAuthorize("Admin")]
        public IActionResult ShowHistory(int page = 1, int pageSize = 7)
        {
            var result = _documentStatusHistoryPaginationService.GetPaged(page, pageSize);

            ViewBag.CurrentPage = result.PageNumber;
            ViewBag.TotalPages = result.TotalPages;

            return View("~/Views/DocumentHistory/DocumentStatusHistory.cshtml", result);
        }
    }
}