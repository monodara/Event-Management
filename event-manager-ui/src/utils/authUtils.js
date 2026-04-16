export const checkAdmin = (user) => user?.role === 'Admin' || user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'Admin';
export const checkEventProvider = (user) => user?.role === 'EventProvider' || user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'EventProvider';
export const checkUser = (user) => user?.role === 'User' || user?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'User';
