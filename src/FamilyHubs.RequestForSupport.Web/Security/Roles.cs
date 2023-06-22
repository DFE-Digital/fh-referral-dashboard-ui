using FamilyHubs.SharedKernel.Identity;

namespace FamilyHubs.RequestForSupport.Web.Security;

public static class Roles
{
    public const string LaProfessionalOrDualRole = RoleTypes.LaProfessional + "," + RoleTypes.LaDualRole;
    public const string VcsProfessionalOrDualRole = RoleTypes.VcsProfessional + "," + RoleTypes.VcsDualRole;
}