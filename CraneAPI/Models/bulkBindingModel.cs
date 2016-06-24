using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CraneAPI.Models
{
    public class BulkBindingModel
    {
        [Required]
        [Display(Name = "file")]
        public string file { get; set; }
    }
}
