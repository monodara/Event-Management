import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import 'bootstrap/dist/css/bootstrap.min.css';
import { AuthProvider } from './context/AuthContext';
import Navigation from './components/Navbar';
import PrivateRoute from './components/PrivateRoute';

// Pages
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import EventList from './pages/EventList';
import EventDetails from './pages/EventDetails';
import MyRegistrations from './pages/MyRegistrations';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Navigation />
        <main className="py-3">
          <Routes>
            {/* Public Routes */}
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/events" element={<EventList />} />
            <Route path="/events/:id" element={<EventDetails />} />
            
            {/* User Specific Routes */}
            <Route 
              path="/my-registrations" 
              element={
                <PrivateRoute roles={['User']}>
                  <MyRegistrations />
                </PrivateRoute>
              } 
            />
            
            {/* Manager Specific Routes */}
            <Route 
              path="/dashboard" 
              element={
                <PrivateRoute roles={['EventProvider', 'Admin']}>
                  <Dashboard />
                </PrivateRoute>
              } 
            />
            
            {/* Redirects */}
            <Route path="/" element={<Navigate to="/events" replace />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </Router>
    </AuthProvider>
  );
}

export default App;
