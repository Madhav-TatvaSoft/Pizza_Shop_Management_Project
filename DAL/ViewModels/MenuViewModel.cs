using DAL.Models;

namespace DAL.ViewModels;

public class MenuViewModel
{
    public List<Category> categoryList { get; set; }
    public Category category { get; set; }
    public ItemsViewModel items { get; set; }
    public PaginationViewModel<ItemsViewModel> PaginationForItemByCategory { get; set; }
    public AddItemViewModel addItems { get; set; }
}