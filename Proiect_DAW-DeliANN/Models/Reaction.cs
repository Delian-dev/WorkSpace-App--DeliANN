using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW_DeliANN.Models
{
    public class Reaction
    {
        [Key]
        public int ReactionId { get; set; }

        public string ReactionEmoji { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public int? PostId { get; set; }

        public virtual Post? Post { get; set; }
    }
}
