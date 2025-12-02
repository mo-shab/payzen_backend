# Payzen Backend API Documentation

Complete API reference for Payzen backend services.

## Table of Contents
- [Authentication & Authorization](#authentication--authorization)
- [User Management](#user-management)
- [Roles & Permissions](#roles--permissions)
- [Companies & Organization](#companies--organization)
- [Reference Data](#reference-data)
- [Employees Management](#employees-management)
- [Statistics](#statistics)

---

## Authentication & Authorization

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `POST` | `/api/auth/login` | Public | User login |
| `POST` | `/api/auth/refresh` | Public | Refresh JWT token |
## User Management

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/users` | `READ_USERS` | List all users |
| `GET` | `/api/users/{id}` | `VIEW_USER` | Get user details |
| `POST` | `/api/users` | `CREATE_USER` | Create a user |
| `PUT` | `/api/users/{id}` | `UPDATE_USER` | Update a user |
| `DELETE` | `/api/users/{id}` | `DELETE_USER` | Delete a user |
## Roles & Permissions

### Roles Management

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/roles` | `READ_ROLES` | List all roles |
| `GET` | `/api/roles/{id}` | `VIEW_ROLE` | Get role details |
| `POST` | `/api/roles` | `CREATE_ROLE` | Create a role |
| `PUT` | `/api/roles/{id}` | `UPDATE_ROLE` | Update a role |
| `DELETE` | `/api/roles/{id}` | `DELETE_ROLE` | Delete a role |

### Permissions Management

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/permissions` | `READ_PERMISSIONS` | List all permissions |
| `GET` | `/api/permissions/{id}` | `VIEW_PERMISSION` | Get permission details |
| `POST` | `/api/permissions` | `CREATE_PERMISSION` | Create a permission |
| `PUT` | `/api/permissions/{id}` | `UPDATE_PERMISSION` | Update a permission |
| `DELETE` | `/api/permissions/{id}` | `DELETE_PERMISSION` | Delete a permission |

### Roles ↔ Permissions Association

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/roles-permissions` | `READ_ROLE_PERMISSIONS` | List all associations |
| `GET` | `/api/roles-permissions/role/{roleId}` | `VIEW_ROLE_PERMISSIONS` | Get role permissions |
| `POST` | `/api/roles-permissions/assign` | `ASSIGN_ROLE_PERMISSION` | Assign permission to role |
| `POST` | `/api/roles-permissions/bulk-assign` | `ASSIGN_ROLE_PERMISSION` | Bulk assign permissions |
| `DELETE` | `/api/roles-permissions/{id}` | `REVOKE_ROLE_PERMISSION` | Revoke permission |

### Users ↔ Roles Association

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/users-roles` | `READ_USER_ROLES` | List all associations |
| `GET` | `/api/users-roles/user/{userId}` | `VIEW_USER_ROLES` | Get user roles |
| `POST` | `/api/users-roles/assign` | `ASSIGN_USER_ROLE` | Assign role to user |
| `POST` | `/api/users-roles/bulk-assign` | `ASSIGN_USER_ROLE` | Bulk assign roles |
| `DELETE` | `/api/users-roles/{id}` | `REVOKE_USER_ROLE` | Revoke role |
## Companies & Organization

### Companies Management

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/companies` | `READ_COMPANIES` | List all companies |
| `GET` | `/api/companies/{id}` | `VIEW_COMPANY` | Get company details |
| `POST` | `/api/companies` | `CREATE_COMPANY` | Create a company |
| `PUT` | `/api/companies/{id}` | `UPDATE_COMPANY` | Update a company |
| `DELETE` | `/api/companies/{id}` | `DELETE_COMPANY` | Delete a company |

### Departments Management

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/departements` | `READ_DEPARTEMENTS` | List all departments |
| `GET` | `/api/departements/{id}` | `VIEW_DEPARTEMENTS` | Get department details |
| `GET` | `/api/departements/company/{companyId}` | `READ_DEPARTEMENTS` | List company departments |
| `POST` | `/api/departements` | `CREATE_DEPARTEMENTS` | Create a department |
| `PUT` | `/api/departements/{id}` | `UPDATE_DEPARTEMENTS` | Update a department |
| `DELETE` | `/api/departements/{id}` | `DELETE_DEPARTEMENTS` | Delete a department |

### Contract Types

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/contract-types` | `READ_CONTRACT_TYPES` | List all contract types |
| `GET` | `/api/contract-types/{id}` | `VIEW_CONTRACT_TYPE` | Get contract type details |
| `GET` | `/api/contract-types/company/{companyId}` | `READ_CONTRACT_TYPES` | List company contract types |
| `POST` | `/api/contract-types` | `CREATE_CONTRACT_TYPE` | Create a contract type |
| `PUT` | `/api/contract-types/{id}` | `UPDATE_CONTRACT_TYPE` | Update a contract type |
| `DELETE` | `/api/contract-types/{id}` | `DELETE_CONTRACT_TYPE` | Delete a contract type |

### Job Positions

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/job-positions` | `READ_JOB_POSITIONS` | List all positions |
| `GET` | `/api/job-positions/{id}` | `VIEW_JOB_POSITION` | Get position details |
| `GET` | `/api/job-positions/company/{companyId}` | `READ_JOB_POSITIONS` | List company positions |
| `POST` | `/api/job-positions` | `CREATE_JOB_POSITION` | Create a position |
| `PUT` | `/api/job-positions/{id}` | `UPDATE_JOB_POSITION` | Update a position |
| `DELETE` | `/api/job-positions/{id}` | `DELETE_JOB_POSITION` | Delete a position |
## Reference Data

### Countries

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/countries` | `READ_COUNTRIES` | List all countries |
| `GET` | `/api/countries/{id}` | `VIEW_COUNTRY` | Get country details |
| `POST` | `/api/countries` | `CREATE_COUNTRY` | Create a country |
| `PUT` | `/api/countries/{id}` | `UPDATE_COUNTRY` | Update a country |
| `DELETE` | `/api/countries/{id}` | `DELETE_COUNTRY` | Delete a country |

### Cities

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/cities` | `READ_CITIES` | List all cities |
| `GET` | `/api/cities/{id}` | `VIEW_CITY` | Get city details |
| `GET` | `/api/cities/country/{countryId}` | `READ_CITIES` | List country cities |
| `POST` | `/api/cities` | `CREATE_CITY` | Create a city |
| `PUT` | `/api/cities/{id}` | `UPDATE_CITY` | Update a city |
| `DELETE` | `/api/cities/{id}` | `DELETE_CITY` | Delete a city |

### Holidays Management

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/holidays` | `READ_HOLIDAYS` | List all holidays |
| `GET` | `/api/holidays/{id}` | `VIEW_HOLIDAY` | Get holiday details |
| `GET` | `/api/holidays/company/{companyId}` | `READ_HOLIDAYS` | List company holidays |
| `GET` | `/api/holidays/country/{countryId}` | `READ_HOLIDAYS` | List country holidays |
| `GET` | `/api/holidays/company/{companyId}/year/{year}` | `READ_HOLIDAYS` | List holidays by year |
| `POST` | `/api/holidays` | `CREATE_HOLIDAY` | Create a holiday |
| `PUT` | `/api/holidays/{id}` | `UPDATE_HOLIDAY` | Update a holiday |
| `DELETE` | `/api/holidays/{id}` | `DELETE_HOLIDAY` | Delete a holiday |

### Working Calendar

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/working-calendar` | `READ_WORKING_CALENDAR` | List all calendars |
| `GET` | `/api/working-calendar/{id}` | `VIEW_WORKING_CALENDAR` | Get calendar details |
| `GET` | `/api/working-calendar/company/{companyId}` | `READ_WORKING_CALENDAR` | Get company calendar |
| `POST` | `/api/working-calendar` | `CREATE_WORKING_CALENDAR` | Create a calendar |
| `PUT` | `/api/working-calendar/{id}` | `UPDATE_WORKING_CALENDAR` | Update a calendar |
| `DELETE` | `/api/working-calendar/{id}` | `DELETE_WORKING_CALENDAR` | Delete a calendar |

### Status

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/statuses` | `READ_STATUSES` | List all statuses |
| `GET` | `/api/statuses/{id}` | `VIEW_STATUS` | Get status details |
| `POST` | `/api/statuses` | `CREATE_STATUS` | Create a status |
| `PUT` | `/api/statuses/{id}` | `UPDATE_STATUS` | Update a status |
| `DELETE` | `/api/statuses/{id}` | `DELETE_STATUS` | Delete a status |

### Gender

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/genders` | `READ_GENDERS` | List all genders |
| `GET` | `/api/genders/{id}` | `VIEW_GENDER` | Get gender details |
| `POST` | `/api/genders` | `CREATE_GENDER` | Create a gender |
| `PUT` | `/api/genders/{id}` | `UPDATE_GENDER` | Update a gender |
| `DELETE` | `/api/genders/{id}` | `DELETE_GENDER` | Delete a gender |

### Education Levels

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/education-levels` | `READ_EDUCATION_LEVELS` | List all education levels |
| `GET` | `/api/education-levels/{id}` | `VIEW_EDUCATION_LEVEL` | Get education level details |
| `POST` | `/api/education-levels` | `CREATE_EDUCATION_LEVEL` | Create an education level |
| `PUT` | `/api/education-levels/{id}` | `UPDATE_EDUCATION_LEVEL` | Update an education level |
| `DELETE` | `/api/education-levels/{id}` | `DELETE_EDUCATION_LEVEL` | Delete an education level |

### Marital Status

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/marital-statuses` | `READ_MARITAL_STATUSES` | List all marital statuses |
| `GET` | `/api/marital-statuses/{id}` | `VIEW_MARITAL_STATUS` | Get marital status details |
| `POST` | `/api/marital-statuses` | `CREATE_MARITAL_STATUS` | Create a marital status |
| `PUT` | `/api/marital-statuses/{id}` | `UPDATE_MARITAL_STATUS` | Update a marital status |
| `DELETE` | `/api/marital-statuses/{id}` | `DELETE_MARITAL_STATUS` | Delete a marital status |
## Employees Management

### Employees

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/employees` | `READ_EMPLOYEES` | List all employees |
| `GET` | `/api/employees/{id}` | `VIEW_EMPLOYEE` | Get employee details |
| `GET` | `/api/employees/company/{companyId}` | `VIEW_COMPANY_EMPLOYEES` | List company employees |
| `GET` | `/api/employees/departement/{departementId}` | `VIEW_COMPANY_EMPLOYEES` | List department employees |
| `GET` | `/api/employees/manager/{managerId}/subordinates` | `VIEW_SUBORDINATES` | List manager's subordinates |
| `POST` | `/api/employees` | `CREATE_EMPLOYEE` | Create an employee |
| `PUT` | `/api/employees/{id}` | `EDIT_EMPLOYEE` | Update an employee |
| `DELETE` | `/api/employees/{id}` | `DELETE_EMPLOYEE` | Delete an employee |

### Employee Contracts

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/employee-contracts` | `READ_EMPLOYEE_CONTRACTS` | List all contracts |
| `GET` | `/api/employee-contracts/{id}` | `VIEW_EMPLOYEE_CONTRACT` | Get contract details |
| `GET` | `/api/employee-contracts/employee/{employeeId}` | `VIEW_EMPLOYEE_CONTRACT` | List employee contracts |
| `POST` | `/api/employee-contracts` | `CREATE_EMPLOYEE_CONTRACT` | Create a contract |
| `PUT` | `/api/employee-contracts/{id}` | `UPDATE_EMPLOYEE_CONTRACT` | Update a contract |
| `DELETE` | `/api/employee-contracts/{id}` | `DELETE_EMPLOYEE_CONTRACT` | Delete a contract |

### Employee Salaries

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/employee-salaries` | `READ_EMPLOYEE_SALARIES` | List all salaries |
| `GET` | `/api/employee-salaries/{id}` | `VIEW_EMPLOYEE_SALARY` | Get salary details |
| `GET` | `/api/employee-salaries/employee/{employeeId}` | `VIEW_EMPLOYEE_SALARY` | List employee salaries |
| `GET` | `/api/employee-salaries/contract/{contractId}` | `VIEW_EMPLOYEE_SALARY` | List contract salaries |
| `POST` | `/api/employee-salaries` | `CREATE_EMPLOYEE_SALARY` | Create a salary |
| `PUT` | `/api/employee-salaries/{id}` | `UPDATE_EMPLOYEE_SALARY` | Update a salary |
| `DELETE` | `/api/employee-salaries/{id}` | `DELETE_EMPLOYEE_SALARY` | Delete a salary |

### Employee Salary Components

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/employee-salary-components` | `READ_EMPLOYEE_SALARY_COMPONENTS` | List all components |
| `GET` | `/api/employee-salary-components/{id}` | `VIEW_EMPLOYEE_SALARY_COMPONENT` | Get component details |
| `GET` | `/api/employee-salary-components/salary/{salaryId}` | `VIEW_EMPLOYEE_SALARY_COMPONENT` | List salary components |
| `POST` | `/api/employee-salary-components` | `CREATE_EMPLOYEE_SALARY_COMPONENT` | Create a component |
| `PUT` | `/api/employee-salary-components/{id}` | `UPDATE_EMPLOYEE_SALARY_COMPONENT` | Update a component |
| `DELETE` | `/api/employee-salary-components/{id}` | `DELETE_EMPLOYEE_SALARY_COMPONENT` | Delete a component |

### Employee Addresses

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/employee-addresses` | `READ_EMPLOYEE_ADDRESSES` | List all addresses |
| `GET` | `/api/employee-addresses/{id}` | `VIEW_EMPLOYEE_ADDRESS` | Get address details |
| `GET` | `/api/employee-addresses/employee/{employeeId}` | `VIEW_EMPLOYEE_ADDRESS` | List employee addresses |
| `POST` | `/api/employee-addresses` | `CREATE_EMPLOYEE_ADDRESS` | Create an address |
| `PUT` | `/api/employee-addresses/{id}` | `UPDATE_EMPLOYEE_ADDRESS` | Update an address |
| `DELETE` | `/api/employee-addresses/{id}` | `DELETE_EMPLOYEE_ADDRESS` | Delete an address |

### Employee Documents

| Method | Endpoint | Permission | Description |
|--------|----------|-----------|-------------|
| `GET` | `/api/employee-documents` | `READ_EMPLOYEE_DOCUMENTS` | List all documents |
| `GET` | `/api/employee-documents/{id}` | `VIEW_EMPLOYEE_DOCUMENT` | Get document details |
| `GET` | `/api/employee-documents/employee/{employeeId}` | `VIEW_EMPLOYEE_DOCUMENT` | List employee documents |
| `POST` | `/api/employee-documents` | `CREATE_EMPLOYEE_DOCUMENT` | Create a document |
| `PUT` | `/api/employee-documents/{id}` | `UPDATE_EMPLOYEE_DOCUMENT` | Update a document |
| `DELETE` | `/api/employee-documents/{id}` | `DELETE_EMPLOYEE_DOCUMENT` | Delete a document |
## Statistics

| Category | Endpoint Count | Permission Count |
|----------|---|---|
| Authentication | 2 | 0 (Public) |
| Users & Roles | 15 | 15 |
| Permissions | 15 | 15 |
| Company | 30 | 30 |
| Reference Data | 40 | 40 |
| Employees | 50 | 50 |
| **TOTAL** | **152** | **150** |
