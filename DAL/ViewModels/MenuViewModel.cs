using DAL.Models;

namespace DAL.ViewModels;

public class MenuViewModel
{

    // Items Area
    public List<Category> categoryList { get; set; }
    public Category category { get; set; }
    public ItemsViewModel items { get; set; }
    public PaginationViewModel<ItemsViewModel> PaginationForItemByCategory { get; set; }
    public AddItemViewModel addItems { get; set; }

    // Modifiers Starts
    public List<Modifiergroup> modifierGroupList { get; set; }

    public Modifiergroup modifiergroup { get; set; }

    public ModifiersViewModel modifiers { get; set; }

    public PaginationViewModel<ModifiersViewModel> PaginationForModifiersByModGroups { get; set; }

    // Adding the modifier group

    public AddModifierGroupViewModel addModifierGroupVM { get; set; }


    // Adding the modifier item

    public AddModifierViewModel addModifier { get; set; }

}