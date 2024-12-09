﻿using System.ComponentModel.DataAnnotations.Schema;

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

            public bool? status { get; set; }
        }
    }
}
