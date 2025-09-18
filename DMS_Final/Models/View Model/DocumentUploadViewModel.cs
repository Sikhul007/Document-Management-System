//using Microsoft.AspNetCore.Http;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

//namespace DMS_Final.Models
//{
//    public class DocumentUploadFile
//    {
//        [Required]
//        public IFormFile File { get; set; }
//        [Required]
//        public string Description { get; set; }
//    }

//    public class DocumentUploadViewModel
//    {
//        [Required]
//        public string Title { get; set; }
//        public string MainDescription { get; set; } // Optional, for Documents table

//        [Required]
//        public List<DocumentUploadFile> Files { get; set; } = new();
//    }
//}