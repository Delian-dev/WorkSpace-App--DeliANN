using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using static Proiect_DAW_DeliANN.Models.ApplicationUserWorkspaces;
// PASUL 1: useri si roluri

namespace Proiect_DAW_DeliANN.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Reaction>? Reactions { get; set; }

        // un user poate posta mai multe articole
        public virtual ICollection<Workspace>? Workspaces { get; set; }

        // un user poate sa creeze mai multe colectii
        public virtual ICollection<Post>? Posts { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public virtual ICollection<ApplicationUserWorkspace>? ApplicationUserWorkspaces { get; set; }
        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}