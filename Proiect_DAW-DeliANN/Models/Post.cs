using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW_DeliANN.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public DateTime Date { get; set; }

        /// 0 - mesaj default 1 - mesaj important
        public bool Type { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        public string? Media { get; set; }

        public int? ChannelId { get; set; }

        public virtual Channel? Channel { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public ICollection<Reaction>? Reactions { get; set; }

        ///nullable - poate sa fie reply sau nu - se autorferentiaza
        public int? ParentPostId { get; set; }
    }
}
