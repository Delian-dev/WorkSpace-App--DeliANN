using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW_DeliANN.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "The Category must have a name!")]
        public string CategoryName { get; set; }

        public virtual ICollection<Workspace>? Workspaces { get; set; }
    }
}
