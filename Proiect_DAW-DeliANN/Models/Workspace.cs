using Microsoft.AspNetCore.Mvc.Rendering;
using Proiect_DAW_DeliANN.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Proiect_DAW_DeliANN.Models.ApplicationUserWorkspaces;

namespace Proiect_DAW_DeliANN.Models
{
    public class Workspace
    {
        [Key]
        public int WorkspaceId { get; set; }

        [Required(ErrorMessage = "Workspace Name is required")]
        [StringLength(200, ErrorMessage = "Workspace Name must have at most 200 characters")]
        [MinLength(4, ErrorMessage = "Workspace Name must have at least 4 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Workspace Description is required")]
        public string Description { get; set; }

        public DateTime Date { get; set; }

        public int? CategoryId { get; set; }

        public string? UserId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public ICollection<Channel>? Channels { get; set; }

        public virtual ICollection<ApplicationUserWorkspace>? ApplicationUserWorkspaces { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}
