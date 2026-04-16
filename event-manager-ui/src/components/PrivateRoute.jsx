import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const PrivateRoute = ({ children, roles }) => {
  const { user, loading, isAdmin, isEventProvider, isUser } = useAuth();
  const location = useLocation();

  if (loading) return <div>Loading...</div>;

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles) {
    const hasRole = roles.some(role => {
      if (role === 'Admin') return isAdmin();
      if (role === 'EventProvider') return isEventProvider();
      if (role === 'User') return isUser();
      return false;
    });

    if (!hasRole) {
      return <Navigate to="/" replace />;
    }
  }

  return children;
};

export default PrivateRoute;
