using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CraneAPI.Models
{
    public class DetectBindingModel
    {
        [Required]
        [Display(Name = "url")]
        public string url { get; set; }

        [Required]
        [Display(Name = "faceListId")]
        public string faceListId { get; set; }

    }

    public class AddFaceBindingModel
    {
        [Required]
        [Display(Name = "url")]
        public string url { get; set; }
        [Required]
        [Display(Name = "faceListId")]
        public string faceListId { get; set; }
        [Display(Name = "userData")]
        public string userData { get; set; }


    }

    public class FindSimilarBindingModel
    {
        [Required]
        [Display(Name = "faceId")]
        public string faceId { get; set; }
        [Required]
        [Display(Name = "faceListId")]
        public string faceListId { get; set; }
        [Display(Name = "maxNumOfCandidatesReturned")]
        public int maxNumOfCandidatesReturned { get; set; }


    }
}