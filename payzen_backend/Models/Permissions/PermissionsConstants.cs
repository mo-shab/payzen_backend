namespace payzen_backend.Models.Permissions
{
    /// <summary>
    /// Constantes pour les permissions du système
    /// </summary>
    public static class PermissionsConstants
    {
        // ==================== USERS ====================
        public const string READ_USERS = "READ_USERS";
        public const string VIEW_USERS = "VIEW_USERS";
        public const string CREATE_USERS = "CREATE_USERS";
        public const string EDIT_USERS = "EDIT_USERS";
        public const string DELETE_USERS = "DELETE_USERS";

        // ==================== ROLES ====================
        public const string READ_ROLES = "READ_ROLES";
        public const string VIEW_ROLE = "VIEW_ROLE";
        public const string CREATE_ROLE = "CREATE_ROLE";
        public const string EDIT_ROLE = "EDIT_ROLE";
        public const string DELETE_ROLE = "DELETE_ROLE";
        public const string ASSIGN_ROLES = "ASSIGN_ROLES";
        public const string REVOKE_ROLES = "REVOKE_ROLES";

        // ==================== PERMISSIONS ====================
        public const string READ_PERMISSIONS = "READ_PERMISSIONS";
        public const string MANAGE_PERMISSIONS = "MANAGE_PERMISSIONS";

        // ==================== COMPANIES ====================
        public const string READ_COMPANIES = "READ_COMPANIES";
        public const string VIEW_COMPANY = "VIEW_COMPANY";
        public const string CREATE_COMPANY = "CREATE_COMPANY";
        public const string EDIT_COMPANY = "EDIT_COMPANY";
        public const string DELETE_COMPANY = "DELETE_COMPANY";
        public const string VIEW_MANAGED_COMPANIES = "VIEW_MANAGED_COMPANIES";
        public const string VIEW_CABINET_EXPERTS = "VIEW_CABINET_EXPERTS";
        public const string MANAGE_COMPANY_HIERARCHY = "MANAGE_COMPANY_HIERARCHY";

        // ==================== EMPLOYEES ====================
        public const string READ_EMPLOYEES = "READ_EMPLOYEES";
        public const string VIEW_EMPLOYEE = "VIEW_EMPLOYEE";
        public const string CREATE_EMPLOYEE = "CREATE_EMPLOYEE";
        public const string EDIT_EMPLOYEE = "EDIT_EMPLOYEE";
        public const string DELETE_EMPLOYEE = "DELETE_EMPLOYEE";
        public const string VIEW_COMPANY_EMPLOYEES = "VIEW_COMPANY_EMPLOYEES";
        public const string VIEW_SUBORDINATES = "VIEW_SUBORDINATES";
        public const string MANAGE_EMPLOYEE_MANAGER = "MANAGE_EMPLOYEE_MANAGER";

        // ==================== DEPARTMENTS ====================
        public const string READ_DEPARTMENTS = "READ_DEPARTMENTS";
        public const string VIEW_DEPARTMENT = "VIEW_DEPARTMENT";
        public const string CREATE_DEPARTMENT = "CREATE_DEPARTMENT";
        public const string EDIT_DEPARTMENT = "EDIT_DEPARTMENT";
        public const string DELETE_DEPARTMENT = "DELETE_DEPARTMENT";
    }
}