using DAL.ViewModels;

namespace BLL.Interface;

public interface IOrderAppTableService
{
    public List<OrderAppSectionVM> GetAllSectionList();
    public List<OrderAppTableVM> GetTablesBySection(long sectionid);

}
