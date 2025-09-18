using DMS_Final.Attribute;
using DMS_Final.Models;
using DMS_Final.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DMS_Final.Controllers
{
    public class DocumentStatusHistoryController : Controller
    {
        private readonly IDocumentStatusHistoryService _documentStatusHistoryService;

        public DocumentStatusHistoryController(IDocumentStatusHistoryService documentStatusHistoryService)
        {
            _documentStatusHistoryService = documentStatusHistoryService;
        }


        [RoleAuthorize("Admin")]
        public IActionResult ShowHistory()
        {
            var history = _documentStatusHistoryService.GetAll();
            // Specify the full relative path to the view since it's under Views/DocumentHistory/
            return View("~/Views/DocumentHistory/DocumentStatusHistory.cshtml", history);
        }



    }
}