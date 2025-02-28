namespace DAL.ViewModels;

public class RolesPermissionViewModel
{
    public long RolepermissionmappingId { get; set; }

     public bool Canview { get; set; }

    public bool Canaddedit { get; set; }

    public bool Candelete { get; set; }

    public bool Permissioncheck { get; set; }
}
