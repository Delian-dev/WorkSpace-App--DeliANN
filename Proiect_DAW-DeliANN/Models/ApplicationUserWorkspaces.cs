using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect_DAW_DeliANN.Models
{
    public class ApplicationUserWorkspaces
    {
        // tabelul asociativ care face legatura intre Article si Bookmark
        // un articol are mai multe colectii din care face parte
        // iar o colectie contine mai multe articole in cadrul ei
        public class ApplicationUserWorkspace
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            // cheie primara compusa (Id, ArticleId, BookmarkId)
            public int Id { get; set; }
            public string? UserId { get; set; }
            public int? WorkspaceId { get; set; }

            public virtual ApplicationUser? User { get; set; }
            public virtual Workspace? Workspace { get; set; }

            public bool status { get; set; } //lasam doar true/false: True-face parte din workspace, false-pending. Cand se da un request de join sa va insera automat cu false. Cand se creeaza un workspace, se insereaza automat cu true
        
            public bool? moderator { get; set; } //null cat timp status = false, apoi cand user-ul devine membru, moderator=false daca e user normal, moderator=true daca este user tip moderator pe acel workspace
        }
    }
}
