using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Proyector_Web_3.Models;

public class Usuario : IdentityUser
{
    [Required, StringLength(80)]
    public string Nombre { get; set; } = "";
}
