using DAL.Models;

namespace DAL.ViewModels;

public class MenuViewModel
{
    public List<Category> categories { get; set; }
    public Category category { get; set; }
    public ItemsViewModel items {get; set;}
    public PaginationViewModel<ItemsViewModel> PaginationForItem{get; set;}
}
