using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect_DAW_DeliANN.Models
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public string? UserId { get; set; } //cheia straina
        public virtual ApplicationUser? User { get; set; } //fiecare profil apartine de un user

        public string? ProfileImage {  get; set; } //imaginea de profil

        public string? Bio {  get; set; }
        public string? DisplayName { get; set; } //numele ce va fi afisat pentru user
        public bool? IsActive { get; set; } //statusul activ/inactiv/away
    }
}
