using DAL.Models;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace DAL.ViewModels;

public class ItemModifierViewModel
{
    public long ModifierGrpId { get; set; }

    public string ModifierGrpName { get; set; } = null!;

    public int Minmodifier { get; set; }

    public int Maxmodifier { get; set; }

    public List<Modifier> modifiersList { get; set; }

    // public long ModifierType { get; set; }

}
