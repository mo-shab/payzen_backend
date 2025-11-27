Liste des Endpoints API
Authentication
•	POST /api/auth/login
•	POST /api/auth/logout
Users
•	GET /api/users
•	GET /api/users/{id}
•	POST /api/users
•	PUT /api/users/{id}
•	DELETE /api/users/{id}
Roles
•	GET /api/roles
•	GET /api/roles/{id}
•	POST /api/roles
•	PUT /api/roles/{id}
•	DELETE /api/roles/{id}
Permissions
•	GET /api/permissions
•	GET /api/permissions/{id}
•	POST /api/permissions
•	PUT /api/permissions/{id}
•	DELETE /api/permissions/{id}
Roles-Permissions
•	GET /api/roles-permissions/role/{roleId}
•	POST /api/roles-permissions
•	DELETE /api/roles-permissions
Users-Roles
•	GET /api/users-roles/user/{userId}
•	GET /api/users-roles/role/{roleId}
•	POST /api/users-roles
•	POST /api/users-roles/bulk-assign
•	PUT /api/users-roles/replace
•	DELETE /api/users-roles
Companies
•	GET /api/companies
•	GET /api/companies/{id}
•	GET /api/companies/managed-by/{managedByCompanyId}
•	GET /api/companies/cabinets-experts
•	POST /api/companies
•	PUT /api/companies/{id}
•	DELETE /api/companies/{id}
Employees
•	GET /api/employees
•	GET /api/employees/{id}
•	GET /api/employees/company/{companyId}
•	GET /api/employees/manager/{managerId}/subordinates
•	POST /api/employees
•	PUT /api/employees/{id}
•	DELETE /api/employees/{id}