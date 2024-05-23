using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.Domain.DBModels;
using ICWebApp.Application.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Provider
{
    public class CONFProvider : ICONFProvider
    {
        private ISessionWrapper _sessionWrapper;
        private IUnitOfWork _unitOfWork;
        private IFORMDefinitionProvider _formDefinitionProvider;
        private ILANGProvider _langProvider;

        public CONFProvider(ISessionWrapper _sessionWrapper, IUnitOfWork _unitOfWork, IFORMDefinitionProvider _formDefinitionProvider,
                            ILANGProvider _langProvider)
        {
            this._sessionWrapper = _sessionWrapper;
            this._unitOfWork = _unitOfWork;
            this._formDefinitionProvider = _formDefinitionProvider;
            this._langProvider = _langProvider;
        }

        public async Task<CONF_Mailer?> GetMailerConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_Mailer>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_Mailer>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<CONF_SMS?> GetSMSConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_SMS>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_SMS>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<CONF_Sign?> GetSignConfiguration(Guid? ID = null, Guid? AUTH_Municipality_ID = null)
        {
            if (AUTH_Municipality_ID == null)
            {
                AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);

            return await _unitOfWork.Repository<CONF_Sign>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<CONF_Mailer_Type?> GetMailerType(Guid? ID)
        {
            return await _unitOfWork.Repository<CONF_Mailer_Type>().GetByIDAsync(ID);
        }
        public async Task<CONF_Enviroment?> GetEnviromentConfiguration(Guid? ID)
        {
            if(ID == null || ID == Guid.Empty)
            {
                return await _unitOfWork.Repository<CONF_Enviroment>().FirstOrDefaultAsync();
            }

            return await _unitOfWork.Repository<CONF_Enviroment>().GetByIDAsync(ID);
        }
        public async Task<CONF_MainMenu> GetMenuByID(Guid ID)
        {
            if (_sessionWrapper.CurrentUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (UserRoles.Any())
                {
                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().FirstOrDefaultAsync(p => p.Enabled == true && p.ID == ID);

                    var AllowedMenuItem = new CONF_MainMenu();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_RolesID).ToList();
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value)).ToList();

                    if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(AlleMenuItems.ID))
                    {
                        AllowedMenuItem = AlleMenuItems;
                    }

                    return AllowedMenuItem;
                }
            }

            return null;
        }
        public async Task<CONF_MainMenu_Setttings?> GetMainMenuSettings(Guid? ID)
        {
            if(ID == null || ID == Guid.Empty)
            {
                return await _unitOfWork.Repository<CONF_MainMenu_Setttings>().FirstOrDefaultAsync(p => p.ID == ID);
            }

            return await _unitOfWork.Repository<CONF_MainMenu_Setttings>().FirstOrDefaultAsync();
        }
        public async Task<List<CONF_MainMenu>?> GetLoggedInMainMenuElements()
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            if (_sessionWrapper.CurrentUser != null && _sessionWrapper.AUTH_Municipality_ID != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (UserRoles.Any())
                {
                    var list = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
                    var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);
                    var userAuthorities = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentUser.ID);

                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ShowInNavMenu == true).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_RolesID).ToList();
                    //ALL except public ones
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value)).ToList();
                    var AllowedNonPublicMenuItems = MenuItemRoles.Where(p => p.AUTH_Roles_ID != AuthRoles.Public
                                                                          && p.AUTH_Roles_ID != AuthRoles.Citizen).ToList();
                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedNonPublicMenuItems.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                        {
                            if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)
                                || (list.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)))
                            {
                                if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                                {
                                    AllowedMenuItems.Add(menu);
                                }
                            }
                        }
                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetLoggedInSubMenuElements(Guid ParentID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }
            if (_sessionWrapper.CurrentUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (UserRoles.Any())
                {
                    var list = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
                    var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);
                    var userAuthorities = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentUser.ID);

                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ParentID == ParentID).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_RolesID).ToList();
                    //ALL except public ones
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value)).ToList();
                    var AllowedNonPublicMenuItems = MenuItemRoles.Where(p => p.AUTH_Roles_ID != AuthRoles.Public
                                                                          && p.AUTH_Roles_ID != AuthRoles.Citizen).ToList();

                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedNonPublicMenuItems.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                        {
                            if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)
                                || (list.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)))
                            {
                                if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                                {
                                    AllowedMenuItems.Add(menu);
                                }
                            }
                        }
                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetPublicMainMenuElements()
        {
            if(_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ShowInNavMenu == true).ToListAsync();

            var AllowedMenuItems = new List<CONF_MainMenu>();
            var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
            var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID == AuthRoles.Public).ToList(); //Only Public Menu Items
            var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

            if (_sessionWrapper.CurrentUser != null)
            {
                var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (roles != null && (roles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || roles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                {
                    AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                }
            }

            foreach (var menu in AlleMenuItems)
            {
                if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                {
                    if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                    {
                        AllowedMenuItems.Add(menu);
                    }
                }
            }

            return AllowedMenuItems.OrderBy(p => p.GroupUrl).ThenBy(p => p.DynamicName).ToList();
        }
        public async Task<List<CONF_MainMenu>?> GetPublicSubMenuElements(Guid ParentID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ParentID == ParentID).ToListAsync();

            var AllowedMenuItems = new List<CONF_MainMenu>();
            var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
            var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID == AuthRoles.Public).ToList(); //Only Public Menu Items
            var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

            if (_sessionWrapper.CurrentUser != null)
            {
                var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (roles != null && (roles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || roles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                {
                    AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                }
            }

            foreach (var menu in AlleMenuItems)
            {
                if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                {
                    if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                    {
                        AllowedMenuItems.Add(menu);
                    }
                }
            }

            return AllowedMenuItems.OrderBy(p => p.GroupUrl).ThenBy(p => p.DynamicName).ToList();
        }
        public async Task<List<CONF_MainMenu>?> GetCitizenMainMenuElements()
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            if (_sessionWrapper.CurrentUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (UserRoles.Any())
                {
                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ShowInNavMenu == true).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_RolesID).ToList();
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value) && p.AUTH_Roles_ID == AuthRoles.Citizen).ToList();

                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                        {
                            if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                            {
                                AllowedMenuItems.Add(menu);
                            }
                        }

                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetCitizenSubMenuElements(Guid ParentID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            if (_sessionWrapper.CurrentUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                if (UserRoles.Any())
                {
                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ParentID == ParentID).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_RolesID).ToList();
                    //ALL except public ones
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value) && p.AUTH_Roles_ID == AuthRoles.Citizen).ToList();

                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_RolesID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                        {
                            if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                            {
                                AllowedMenuItems.Add(menu);
                            }
                        }
                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetMunicipalLoggedInMainMenuElements()
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            if (_sessionWrapper != null && _sessionWrapper.CurrentMunicipalUser != null && _sessionWrapper.AUTH_Municipality_ID != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                if (UserRoles.Any())
                {
                    var list = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
                    var roles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);
                    var userAuthorities = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ShowInNavMenu == true).ToListAsync();
                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_Roles_ID).ToList();
                    //ALL except public ones
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value)).ToList();
                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentMunicipalUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)
                            || (list.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)))
                        {
                            if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                            {
                                AllowedMenuItems.Add(menu);
                            }
                        }                        
                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetMunicipalLoggedInSubMenuElements(Guid ParentID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }
            if (_sessionWrapper != null && _sessionWrapper.CurrentMunicipalUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                if (UserRoles.Any())
                {
                    var list = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
                    var roles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);
                    var userAuthorities = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ParentID == ParentID).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_Roles_ID).ToList();
                    //ALL except public ones
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value)).ToList();

                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentMunicipalUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)
                            || (list.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID)))
                        {
                            if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                            {
                                AllowedMenuItems.Add(menu);
                            }
                        }                        
                    }

                    AllowedMenuItems = await FilterConditionalSubMenuItems(AllowedMenuItems);
                    return AllowedMenuItems;
                }
            }

            return null;
        }
        private async Task<List<CONF_MainMenu>?> FilterConditionalSubMenuItems(List<CONF_MainMenu>? list)
        {
            if (list == null)
                return list;

            //Requested POS Cards
            var item = list.FirstOrDefault(e => e.ID == Guid.Parse("0B6BF3AF-6356-4E9C-A81B-C2398FE7BC71"));

            if (item != null)
            {
                var conf = await _unitOfWork.Repository<CANTEEN_Configuration>().FirstOrDefaultAsync(e => e.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID);

                if (conf == null || conf.PosMode != true)
                    list.Remove(item);
            }
            return list;
        }
        public async Task<List<CONF_MainMenu>?> GetMunicipalCitizenMainMenuElements()
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            if (_sessionWrapper != null && _sessionWrapper.CurrentMunicipalUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                if (UserRoles.Any())
                {
                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ShowInNavMenu == true).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_Roles_ID).ToList();
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value) && p.AUTH_Roles_ID == AuthRoles.Citizen).ToList();

                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentMunicipalUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                        {
                            if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                            {
                                AllowedMenuItems.Add(menu);
                            }
                        }

                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetMunicipalCitizenSubMenuElements(Guid ParentID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<CONF_MainMenu>();
            }

            if (_sessionWrapper != null && _sessionWrapper.CurrentMunicipalUser != null)
            {
                var UserRoles = await _unitOfWork.Repository<AUTH_Municipal_Users_Roles>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentMunicipalUser.ID);

                if (UserRoles.Any())
                {
                    var AlleMenuItems = await _unitOfWork.Repository<CONF_MainMenu>().Where(p => p.Enabled == true && p.ParentID == ParentID).ToListAsync();

                    var AllowedMenuItems = new List<CONF_MainMenu>();
                    var MenuItemRoles = await _unitOfWork.Repository<AUTH_Roles_MainMenu>().ToListAsync();
                    var UserRolesID = UserRoles.Select(p => p.AUTH_Roles_ID).ToList();
                    //ALL except public ones
                    var AllowedRoles = MenuItemRoles.Where(p => p.AUTH_Roles_ID != null && UserRolesID.Contains(p.AUTH_Roles_ID.Value) && p.AUTH_Roles_ID == AuthRoles.Citizen).ToList();

                    var AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && p.Deaktivated == null);

                    if (_sessionWrapper.CurrentMunicipalUser != null)
                    {
                        if (UserRoles != null && (UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Employee) || UserRoles.Select(p => p.AUTH_Roles_ID).Contains(AuthRoles.Administrator)))
                        {
                            AktiveApps = await _unitOfWork.Repository<AUTH_MunicipalityApps>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value && (p.Deaktivated == null || (p.InPreparation && p.Deaktivated != null)));
                        }
                    }

                    foreach (var menu in AlleMenuItems)
                    {
                        if (AllowedRoles.Select(p => p.CONF_MainMenu_ID).Contains(menu.ID))
                        {
                            if (menu.APP_Application_ID == null || AktiveApps.Select(p => p.APP_Application_ID).ToList().Contains(menu.APP_Application_ID.Value))
                            {
                                AllowedMenuItems.Add(menu);
                            }
                        }
                    }

                    return AllowedMenuItems;
                }
            }

            return null;
        }
        public async Task<List<CONF_MainMenu>?> GetAuthorityList(Guid ParentID)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                Lang = Language.ID;
            }

            var menuAuthoriy = new List<CONF_MainMenu>();

            if (_sessionWrapper != null && _sessionWrapper.AUTH_Municipality_ID != null)
            {
                var list = await _formDefinitionProvider.GetDefinitionListByCategory(_sessionWrapper.AUTH_Municipality_ID.Value, Guid.Parse("93efca6b-c191-473d-b49a-4d6e4d2117e5"));
                int count = 0;

                foreach (var l in list.Where(p => p.Enabled).OrderBy(p => p.SortOrder))
                {
                    var menuItem = new CONF_MainMenu();

                    menuItem.ParentID = ParentID;

                    if (l.AUTH_Authority != null)
                    {
                        menuItem.Group_TEXT_SysemTexts_Code = l.AUTH_Authority.TEXT_SystemText_Code;
                        menuItem.GroupUrl = "/Form/List/" + l.AUTH_Authority_ID;
                    }

                    menuItem.SortOrder = count;

                    if (l.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Lang) != null)
                    {
                        menuItem.DynamicName = l.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Lang).Name;
                    }

                    menuItem.Url = "/Form/Detail/" + l.ID;

                    menuItem.InDevelopment = false;
                    menuItem.ShowInNavMenu = true;
                    menuItem.Enabled = true;

                    menuAuthoriy.Add(menuItem);

                    count++;
                }
            }

            return menuAuthoriy;
        }
        public async Task<List<CONF_MainMenu>?> GetBackendAuthorityList(Guid ParentID)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                Lang = Language.ID;
            }

            var menuAuthoriy = new List<CONF_MainMenu>();

            if (_sessionWrapper != null && _sessionWrapper.AUTH_Municipality_ID != null)
            {
                var list = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

                var userAuthorities = await _unitOfWork.Repository<AUTH_Municipal_Users_Authority>().ToListAsync(p => p.AUTH_Municipal_Users_ID == _sessionWrapper.CurrentUser.ID);

                var roles = await _unitOfWork.Repository<AUTH_UserRoles>().ToListAsync(p => p.AUTH_UsersID == _sessionWrapper.CurrentUser.ID);

                int count = 0;

                foreach (var l in list.OrderBy(p => p.Name))
                {
                    var menuItem = new CONF_MainMenu();

                    menuItem.ParentID = ParentID;

                    menuItem.SortOrder = count;
                    menuItem.TEXT_SystemTextsCode = l.TEXT_SystemText_Code;

                    menuItem.Url = "/Backend/Authority/" + l.ID;

                    menuItem.InDevelopment = false;
                    menuItem.ShowInNavMenu = true;
                    menuItem.Enabled = true;

                    menuAuthoriy.Add(menuItem);

                    count++;
                }
            }

            return menuAuthoriy;
        }
        public async Task<CONF_Push?> GetPushConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }
            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_Push>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_Push>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<CONF_Sign?> SetSignConfiguration(CONF_Sign Data)
        {
            return await _unitOfWork.Repository<CONF_Sign>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<CONF_Sign>> GetSignConfigurationList()
        {
            return await _unitOfWork.Repository<CONF_Sign>().ToListAsync();
        }
        public async Task<CONF_PAY?> GetPayConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_PAY>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_PAY>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<List<CONF_MainMenu>?> GetMantainanceList(Guid ParentID)
        {
            Guid Lang = LanguageSettings.German;

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (Language != null)
            {
                Lang = Language.ID;
            }

            var menuAuthoriy = new List<CONF_MainMenu>();

            if (_sessionWrapper != null && _sessionWrapper.AUTH_Municipality_ID != null)
            {
                var list = await _formDefinitionProvider.GetDefinitionListByCategory(_sessionWrapper.AUTH_Municipality_ID.Value , FORMCategories.Maintenance);
                int count = 0;

                foreach (var l in list.Where(p => p.Enabled).OrderBy(p => p.SortOrder))
                {
                    var menuItem = new CONF_MainMenu();

                    menuItem.ParentID = ParentID;

                    if (l.AUTH_Authority != null)
                    {
                        menuItem.Group_TEXT_SysemTexts_Code = l.AUTH_Authority.TEXT_SystemText_Code;
                        menuItem.GroupUrl = "/Mantainance";
                    }

                    menuItem.SortOrder = count;

                    if (l.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Lang) != null)
                    {
                        menuItem.DynamicName = l.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Lang).Name;
                    }

                    menuItem.Url = "/Mantainance/Detail/" + l.ID;

                    menuItem.InDevelopment = false;
                    menuItem.ShowInNavMenu = true;
                    menuItem.Enabled = true;

                    menuAuthoriy.Add(menuItem);

                    count++;
                }
            }

            return menuAuthoriy;
        }
        public async Task<CONF_PagoPA?> GetPagoPAConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_PagoPA>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_PagoPA>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<CONF_Veriff?> GetVeriffConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_Veriff>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_Veriff>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<CONF_Freshdesk?> GetFreshDeskConfiguration(Guid? ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
            }

            if (ID == null || ID == Guid.Empty)
                return await _unitOfWork.Repository<CONF_Freshdesk>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            return await _unitOfWork.Repository<CONF_Freshdesk>().FirstOrDefaultAsync(p => p.ID == ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);
        }
        public async Task<List<V_CONF_Freshdesk_Priority>?> GetVPriorityList()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_CONF_Freshdesk_Priority>().Where(p => p.LANG_LanguagesID == Language.ID).ToListAsync();
        }
        public async Task<CONF_Spid?> GetSpidConfiguration(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<CONF_Spid>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
        public async Task<CONF_Spid_Maintenance?> GetSpidMaintenance()
        {
            return await _unitOfWork.Repository<CONF_Spid_Maintenance>().FirstOrDefaultAsync(p => p.Enabled == true && DateTime.Now >= p.DisplayFrom && DateTime.Now <= p.DisplayTo);
        }
        public async Task<List<CONF_MyCivis>?> GetMyCivisConfiguration(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<CONF_MyCivis>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
        }
    }
}