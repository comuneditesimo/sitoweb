using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class AuthorizationRoleHelper : IAuthorizationRequirement
    {
        public List<Guid> AUTH_RoleIDs{ get; }

        public AuthorizationRoleHelper(List<Guid> AUTH_RoleIDs)
        {
            this.AUTH_RoleIDs = AUTH_RoleIDs;
        }
    }

    public class AuthorizationRoleHelperHandler : AuthorizationHandler<AuthorizationRoleHelper>
    {
        private ISessionWrapper SessionWrapper;
        private IAUTHProvider AuthProvider;

        public AuthorizationRoleHelperHandler(ISessionWrapper SessionWrapper, IAUTHProvider AuthProvider)
        {
            this.SessionWrapper = SessionWrapper;
            this.AuthProvider = AuthProvider;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRoleHelper requirement)
        {
            if (SessionWrapper != null)
            {
                var UserRights = AuthProvider.GetUserRoles();
                var UserMunicipalRights = AuthProvider.GetMunicipalUserRoles();
                var Auth_RoleIDs = requirement.AUTH_RoleIDs;

                foreach (var RoleID in Auth_RoleIDs) 
                { 
                    if (UserRights.Select(p => p.AUTH_RolesID).Contains(RoleID))
                    {
                        context.Succeed(requirement);
                        continue;
                    }
                }

                foreach (var RoleID in Auth_RoleIDs)
                {
                    if (UserMunicipalRights.Select(p => p.AUTH_Roles_ID).Contains(RoleID))
                    {
                        context.Succeed(requirement);
                        continue;
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
