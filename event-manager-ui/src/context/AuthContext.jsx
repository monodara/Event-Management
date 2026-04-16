/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useState, useContext, useCallback } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authService } from '../services/api';
import { checkAdmin, checkEventProvider, checkUser } from '../utils/authUtils';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded = jwtDecode(token);
        if (decoded.exp * 1000 < Date.now()) {
          localStorage.removeItem('token');
          return null;
        }
        return decoded;
      } catch (_e) {
        console.error("Error decoding token:", _e);
        localStorage.removeItem('token');
        return null;
      }
    }
    return null;
  });
  const loading = user === null; // loading state is derived directly from user presence

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    setUser(null);
  }, []);

  const login = async (email, password) => {
    const response = await authService.login(email, password);
    const { token } = response.data;
    localStorage.setItem('token', token);
    const decoded = jwtDecode(token);
    setUser(decoded);
    return decoded;
  };

  const value = {
    user,
    loading,
    login,
    logout,
    isAdmin: () => checkAdmin(user),
    isEventProvider: () => checkEventProvider(user),
    isUser: () => checkUser(user),
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
