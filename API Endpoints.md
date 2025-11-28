## Liste des Endpoints API
---
## Authentication
•	POST /api/auth/login<br>
•	POST /api/auth/logout<br>
## Users
•	GET /api/users<br>
•	GET /api/users/{id}<br>
•	POST /api/users<br>
•	PUT /api/users/{id}<br>
•	DELETE /api/users/{id}<br>
## Roles
•	GET /api/roles<br>
•	GET /api/roles/{id}<br>
•	POST /api/roles<br>
•	PUT /api/roles/{id}<br>
•	DELETE /api/roles/{id}<br>
## Permissions
•	GET /api/permissions<br>
•	GET /api/permissions/{id}<br>
•	POST /api/permissions<br>
•	PUT /api/permissions/{id}<br>
•	DELETE /api/permissions/{id}<br>
## Roles-Permissions
•	GET /api/roles-permissions/role/{roleId}<br>
•	POST /api/roles-permissions<br>
•	DELETE /api/roles-permissions<br>
## Users-Roles
•	GET /api/users-roles/user/{userId}<br>
•	GET /api/users-roles/role/{roleId}<br>
•	POST /api/users-roles<br>
•	POST /api/users-roles/bulk-assign<br>
•	PUT /api/users-roles/replace<br>
•	DELETE /api/users-roles<br>
## Companies
•	GET /api/companies<br>
•	GET /api/companies/{id}<br>
•	GET /api/companies/managed-by/{managedByCompanyId}<br>
•	GET /api/companies/cabinets-experts<br>
•	POST /api/companies<br>
•	PUT /api/companies/{id}<br>
•	DELETE /api/companies/{id}<br>
## Employees
•	GET /api/employees<br>
•	GET /api/employees/{id}<br>
•	GET /api/employees/company/{companyId}<br>
•	GET /api/employees/manager/{managerId}/subordinates<br>
•	POST /api/employees<br>
•	PUT /api/employees/{id}<br>
•	DELETE /api/employees/{id}<br>