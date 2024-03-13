namespace Player.AuthorizationLogic
{
}

/*
https://stackoverflow.com/questions/36445780/how-to-implement-permission-based-access-control-with-asp-net-core
public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(PermissionEnum permission)
    {
        Permission = permission;
    }

    public PermissionEnum Permission { get; }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserPermissionsRepository permissionRepository;

    public PermissionHandler(IUserPermissionsRepository permissionRepository)
    {
        if(permissionRepository == null)
            throw new ArgumentNullException(nameof(permissionRepository));

        this.permissionRepository = permissionRepository;
    }

    protected override void Handle(AuthorizationContext context, PermissionRequirement requirement)
    {
        if(context.User == null)
        {
            // no user authorizedd. Alternatively call context.Fail() to ensure a failure 
            // as another handler for this requirement may succeed
            return null;
        }

        bool hasPermission = permissionRepository.CheckPermissionForUser(context.User, requirement.Permission);
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
*/