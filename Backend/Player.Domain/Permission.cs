using System.Collections.Generic;
using Player.Domain.Base;

namespace Player.Domain
{
    public class Permission : Entity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
        
        
        public const string DownloadMusic = nameof(DownloadMusic);
        public const string CreateAdvert = nameof(CreateAdvert);
        public const string CreateGenre = nameof(CreateGenre);
        public const string CreateClient = nameof(CreateClient);
        public const string CreateMusic = nameof(CreateMusic);
        public const string CreateObject = nameof(CreateObject);
        public const string CreateSelection = nameof(CreateSelection);
        public const string EditClient = nameof(EditClient);
        public const string EditObject = nameof(EditObject);
        public const string EditSelection = nameof(EditSelection);
        public const string ReadAdvertById = nameof(ReadAdvertById);
        public const string ReadAllActivityTypes = nameof(ReadAllActivityTypes);
        public const string ReadAllAdvertTypes = nameof(ReadAllAdvertTypes);
        public const string ReadAllAdverts = nameof(ReadAllAdverts);
        public const string ReadAllCities = nameof(ReadAllCities);
        public const string ReadAllClients = nameof(ReadAllClients);
        public const string ReadAllGenres = nameof(ReadAllGenres);
        public const string ReadAllMusic = nameof(ReadAllMusic);
        /// <summary>
        /// Переименовать потом
        /// update "Permissions" set "Name" = 'ReadObjectById' where "Name" = 'ReadAllObjectById';
        /// </summary>
        public const string ReadObjectById = nameof(ReadObjectById);
        public const string ReadAllObjects = nameof(ReadAllObjects);
        public const string ReadAllObjectsForDropDown = nameof(ReadAllObjectsForDropDown);
        public const string ReadAllSelections = nameof(ReadAllSelections);
        public const string ReadAllServiceCompanies = nameof(ReadAllServiceCompanies);
        public const string ReadAllTrackTypes = nameof(ReadAllTrackTypes);
        public const string ReadClientById = nameof(ReadClientById);
        public const string ReadMediaPlanReport = nameof(ReadMediaPlanReport);
        public const string ReadSelectionById = nameof(ReadSelectionById);
        public const string AddObjectToManager = nameof(AddObjectToManager);

        public const string ReadManagerById = nameof(ReadManagerById);
        public const string ReadAllManagers = nameof(ReadAllManagers);
        public const string CreateManager = nameof(CreateManager);
        public const string EditManager = nameof(EditManager);
        public const string PartnerAccessToObject = nameof(PartnerAccessToObject);
        
        public const string Login = nameof(Login);
        public const string AdminAccessObject = nameof(AdminAccessObject);
    }
}