using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW_DeliANN.Models
{
    public class Channel
    {
        [Key]
        public int ChannelId { get; set; }

        [Required(ErrorMessage = "Chanel Name is required")]
        [StringLength(200, ErrorMessage = "Channel Name must have at most 200 characters")]
        [MinLength(4, ErrorMessage = "Channel Name must have at least 4 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Channel Type is required")]
        [StringLength(100, ErrorMessage = "Channel Type must have at most 200 characters")]
        [MinLength(3, ErrorMessage = "Channel Type must have at least 4 characters")]
        public string Type { get; set; }

        [Required(ErrorMessage = "The workspace is mandatory")]
        public int? WorkspaceId { get; set; }

        public virtual Workspace? Workspace { get; set; }

        public ICollection<Post>? Posts { get; set; }
    }
}
