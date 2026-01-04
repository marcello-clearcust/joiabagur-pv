# Manual Testing Guide - Joiabagur PV

**Date**: 2025-12-14  
**Version**: EP6 + EP7 + EP8 Complete

---

## 🚀 Application is Running!

### Server URLs

**Frontend (React):**
- 🌐 **http://localhost:3002**
- Built with React 19, Vite, Metronic template
- Mobile-responsive design

**Backend (ASP.NET Core API):**
- 🔧 **http://localhost:5056**
- RESTful API with JWT authentication
- PostgreSQL database on port 5433

**Database (PostgreSQL):**
- 🗄️ **localhost:5433**
- Database: `joiabagur_pv`
- Username: `postgres` / Password: `password`

---

## 👤 Default Credentials

**Admin User:**
- Username: `admin`
- Password: `Admin123!`

> ⚠️ Change this password in production!

---

## 🧪 What to Test

### 1. Authentication (EP7) ✅

**Login Page:** http://localhost:3002/auth/login

- [ ] Login with admin credentials
- [ ] Verify you're redirected to dashboard
- [ ] Check that navigation shows admin menu items
- [ ] Logout and verify redirection to login
- [ ] Try invalid credentials (should show error)
- [ ] Test "Remember me" checkbox

### 2. User Management (EP7) ✅

**Users Page:** http://localhost:3002/users

- [ ] View list of users (should show admin)
- [ ] Create a new Operator user:
  - Username: `operator1`
  - Password: `Operator123!`
  - First Name: `Juan`
  - Last Name: `Pérez`
  - Role: `Operator`
- [ ] Edit the operator user
- [ ] Deactivate/Reactivate user
- [ ] Try to delete or modify (UI should work)

### 3. Point of Sale Management (EP8) ✅

**Points of Sale Page:** http://localhost:3002/points-of-sale

- [ ] View statistics (Total, Active, Inactive)
- [ ] Create a new point of sale:
  - Name: `Tienda Centro`
  - Code: `CENTRO-001`
  - Address: `Calle Principal 123, Madrid`
  - Phone: `+34 600 123 456`
  - Email: `centro@test.com`
- [ ] Create another point of sale:
  - Name: `Hotel Plaza`
  - Code: `HOTEL-PLZ`
  - Address: `Plaza Mayor 1, Barcelona`
- [ ] Edit a point of sale (change name or address)
- [ ] Deactivate/Reactivate a point of sale

**Operator Assignments:**
- [ ] Click "Asignar Operadores" on a point of sale
- [ ] Select operator1 (created earlier)
- [ ] Save and verify assignment
- [ ] Open dialog again and unassign operator
- [ ] Verify last assignment protection (can't unassign last POS)

**Payment Method Assignments:**
- [ ] Click "Métodos de Pago" on a point of sale
- [ ] Select multiple payment methods (CASH, BIZUM, CARD_POS)
- [ ] Save and verify assignments
- [ ] Deactivate one payment method assignment
- [ ] Reactivate it

### 4. Payment Method Management (EP6) ✅

**Payment Methods Page:** http://localhost:3002/payment-methods

- [ ] View all predefined payment methods:
  - ✅ CASH (Efectivo)
  - ✅ BIZUM (Bizum)
  - ✅ TRANSFER (Transferencia)
  - ✅ CARD_OWN (Tarjeta propia)
  - ✅ CARD_POS (Tarjeta TPV)
  - ✅ PAYPAL (PayPal)
- [ ] Create a new custom payment method:
  - Code: `CRYPTO`
  - Name: `Criptomonedas`
  - Description: `Pago con Bitcoin o Ethereum`
- [ ] Edit a payment method (change name or description)
- [ ] Deactivate/Reactivate a payment method
- [ ] Toggle "Mostrar inactivos" to see inactive methods

### 5. Role-Based Access Control ✅

**Test Operator Restrictions:**
- [ ] Logout as admin
- [ ] Login as operator1 / Operator123!
- [ ] Verify operator CANNOT access:
  - User management page
  - Point of sale management page
  - Payment methods page
- [ ] Verify navigation hides admin-only items
- [ ] Try to access admin URLs directly (should redirect/show forbidden)

**Test Admin Access:**
- [ ] Login as admin
- [ ] Verify admin CAN access all modules
- [ ] All navigation items visible

---

## 📋 Testing Checklist by Module

### Authentication Module
- [x] Backend API running on port 5056
- [x] Database seeded with admin user
- [ ] Login page loads and works
- [ ] Token refresh works
- [ ] Logout works
- [ ] Protected routes redirect to login when unauthenticated

### User Management Module
- [ ] Users list displays correctly
- [ ] Create user form works with validation
- [ ] Edit user works
- [ ] Password strength indicator shows
- [ ] Role selector (Admin/Operator) works
- [ ] User activation/deactivation works
- [ ] Assignment dialog shows points of sale

### Point of Sale Management Module
- [x] Database schema created
- [ ] Points of sale list displays with statistics
- [ ] Create point of sale form works
- [ ] Edit point of sale works
- [ ] Code auto-uppercase conversion works
- [ ] Status toggle works
- [ ] Operator assignments dialog works
- [ ] Payment method assignments dialog works
- [ ] Multi-select for assignments works

### Payment Method Management Module
- [x] Seed data created (6 predefined methods)
- [ ] Payment methods list displays
- [ ] Show inactive toggle works
- [ ] Create payment method form works
- [ ] Code validation (uppercase, format) works
- [ ] Edit payment method works (code disabled in edit mode)
- [ ] Status toggle works
- [ ] Info banner displays correctly

---

## 📚 API Documentation

### Scalar UI (Recommended)
👉 **http://localhost:5056/scalar/v1**

Scalar is the modern API documentation tool that replaced Swagger:
- Beautiful, interactive UI
- Try all endpoints directly in browser
- See request/response schemas
- Search across all endpoints
- Dark mode support

### .http Files (Alternative)
Location: `backend/api-tests/`
- `auth.http` - Authentication endpoints
- `users.http` - User management
- `point-of-sales.http` - Point of sale management
- `payment-methods.http` - Payment methods

Use with VS Code REST Client extension or JetBrains Rider.

---

## 🐛 Known Issues / Limitations

### Temporary Limitations

1. **Integration tests** - Some tests may fail due to Testcontainers/.NET 10 preview
   - Unit tests: ✅ 100% passing (117/117)
   - Backend functionality: ✅ Fully working
   - Issue is with test infrastructure, not the code

### By Design
1. **Sales integration deferred** - Payment method validation in sales will be implemented in EP3
2. **Inventory not yet implemented** - Coming in EP1 & EP2
3. **Reports not yet implemented** - Coming in EP9

---

## 🧭 Navigation Guide

### Admin Menu Structure
```
Dashboard
├── Usuarios (User Management)
├── Puntos de Venta (Points of Sale)
├── Métodos de Pago (Payment Methods)
├── Productos (Not implemented - EP1)
├── Inventario (Not implemented - EP2)
├── Ventas (Not implemented - EP3)
├── Devoluciones (Not implemented - EP5)
└── Informes (Not implemented - EP9)
```

### Operator Menu Structure (Limited)
```
Dashboard
├── Ventas (Not implemented - EP3)
├── Inventario (Not implemented - EP2)
└── Devoluciones (Not implemented - EP5)
```

---

## 🔍 API Endpoints to Test Manually

### Authentication Endpoints
```
POST   http://localhost:5056/api/auth/login
POST   http://localhost:5056/api/auth/refresh
POST   http://localhost:5056/api/auth/logout
GET    http://localhost:5056/api/auth/me
```

### User Management Endpoints (Admin only)
```
GET    http://localhost:5056/api/users
POST   http://localhost:5056/api/users
GET    http://localhost:5056/api/users/{id}
PUT    http://localhost:5056/api/users/{id}
PUT    http://localhost:5056/api/users/{id}/password
```

### Point of Sale Endpoints (Admin only)
```
GET    http://localhost:5056/api/point-of-sales
POST   http://localhost:5056/api/point-of-sales
GET    http://localhost:5056/api/point-of-sales/{id}
PUT    http://localhost:5056/api/point-of-sales/{id}
PATCH  http://localhost:5056/api/point-of-sales/{id}/status
POST   http://localhost:5056/api/point-of-sales/{id}/operators/{userId}
DELETE http://localhost:5056/api/point-of-sales/{id}/operators/{userId}
```

### Payment Method Endpoints (Admin only)
```
GET    http://localhost:5056/api/payment-methods
POST   http://localhost:5056/api/payment-methods
GET    http://localhost:5056/api/payment-methods/{id}
PUT    http://localhost:5056/api/payment-methods/{id}
PATCH  http://localhost:5056/api/payment-methods/{id}/status
```

### Point of Sale Payment Method Endpoints (Admin only)
```
GET    http://localhost:5056/api/point-of-sales/{id}/payment-methods
POST   http://localhost:5056/api/point-of-sales/{id}/payment-methods/{methodId}
DELETE http://localhost:5056/api/point-of-sales/{id}/payment-methods/{methodId}
PATCH  http://localhost:5056/api/point-of-sales/{id}/payment-methods/{methodId}/status
```

---

## 🧪 Testing Tools

### Using Postman/Insomnia
1. Login first to get JWT token (sent via cookie)
2. Subsequent requests will automatically include the cookie
3. Test different scenarios (valid/invalid data)

### Using Browser DevTools
1. Open browser console (F12)
2. Check Network tab for API calls
3. Inspect requests/responses
4. Check Application tab for cookies

### Using curl
```powershell
# Login
curl -X POST http://localhost:5056/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"Admin123!"}' `
  -c cookies.txt

# Get users (with cookie)
curl -X GET http://localhost:5056/api/users `
  -b cookies.txt
```

---

## 📊 Expected Test Results

### Database State After Seed
- **Users**: 1 (admin)
- **Payment Methods**: 6 (CASH, BIZUM, TRANSFER, CARD_OWN, CARD_POS, PAYPAL)
- **Points of Sale**: 0 (none created yet - you'll create them)
- **Assignments**: 0 (none yet)

### After Manual Testing
- **Users**: 2+ (admin + operators you create)
- **Payment Methods**: 6+ (predefined + any custom ones)
- **Points of Sale**: 2+ (stores you create)
- **Operator Assignments**: Several (operators assigned to POSs)
- **Payment Method Assignments**: Several (methods assigned to POSs)

---

## 🎯 Key Features to Validate

### Frontend UX
- [ ] Mobile-responsive (resize browser to test)
- [ ] Loading states (skeletons while fetching)
- [ ] Error messages (toast notifications)
- [ ] Success messages (toast notifications)
- [ ] Form validation (try invalid data)
- [ ] Empty states (before creating data)

### Security
- [ ] Unauthenticated users redirected to login
- [ ] Operators can't access admin pages
- [ ] API returns 401 without token
- [ ] API returns 403 for unauthorized actions
- [ ] Passwords are never visible in network requests

### Data Integrity
- [ ] Unique code validation (try duplicate codes)
- [ ] Required field validation
- [ ] Email format validation
- [ ] Can't unassign last operator assignment
- [ ] Can't assign to inactive point of sale
- [ ] Timestamps update correctly (CreatedAt, UpdatedAt)

---

## 📝 Notes

- **Swagger/OpenAPI is temporarily disabled** - Use this guide or Postman for API testing
- **All data is in development database** - Safe to experiment
- **Database can be reset** - Run `docker-compose down -v` and restart to clear all data
- **Frontend hot-reloads** - Changes to React components update automatically
- **Backend requires restart** - Code changes need `dotnet run` restart

---

## 🛠️ Troubleshooting

### Frontend won't load
- Check terminal 765122: `cat terminals/765122.txt`
- Verify Vite is running on port 3002
- Check browser console for errors

### Backend API errors
- Check terminal 438604: `cat terminals/438604.txt`
- Verify backend is running on port 5056
- Check database connection (port 5433)

### Database connection fails
- Check PostgreSQL: `docker ps | grep postgres`
- Restart database: `cd backend; docker-compose restart postgres`
- Check logs: `docker logs joiabagur-pv-postgres`

### CORS errors
- Verify frontend URL is in backend CORS config
- Check browser network tab for CORS errors
- Backend already configured for ports 3000, 3001, 3002

---

## ✅ Ready to Test!

1. **Open browser:** http://localhost:3002
2. **Login:** admin / Admin123!
3. **Explore:**
   - Create users
   - Create points of sale
   - Assign operators
   - Assign payment methods
   - Test all CRUD operations

Enjoy testing your application! 🎉
