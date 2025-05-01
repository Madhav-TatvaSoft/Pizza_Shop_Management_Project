using DAL.Models;

namespace DAL.ViewModels;

public class OrderAppMenuViewModel
{
    public List<Category> categoryList { get; set; }

    public List<ItemsViewModel> itemList { get; set; }

    public List<ItemModifierViewModel> modifirsByItemList { get; set; }

    public OrderDetailViewModel orderDetails { get; set; }

    public CustomerViewModel customerDetails { get; set; }
    
}