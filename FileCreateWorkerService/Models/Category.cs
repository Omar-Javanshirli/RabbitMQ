using System;
using System.Collections.Generic;

#nullable disable

namespace FileCreateWorkerService.Models
{
    public partial class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
