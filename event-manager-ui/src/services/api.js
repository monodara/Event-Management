import axios from 'axios';

const API_BASE_URL = 'http://localhost:5001/api/v1';

const api = axios.create({
    baseURL: API_BASE_URL,
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export const authService = {
    login: (email, password) => api.post('/accounts/login', { email, password }),
    register: (email, password, role) => api.post('/accounts/register', { email, password, role }),
    getProfile: () => api.get('/accounts/profile'),
};

export const eventService = {
    getAll: () => api.get('/events'),
    getById: (id) => api.get(`/events/${id}`),
    create: (data) => api.post('/events', data),
    update: (id, data) => api.put(`/events/${id}`, data),
    delete: (id) => api.delete(`/events/${id}`),
    uploadMaterial: (id, file) => {
        const formData = new FormData();
        formData.append('file', file);
        return api.post(`/events/${id}/upload`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    },
    register: (id) => api.post(`/events/${id}/register`),
    unregister: (id) => api.delete(`/events/${id}/unregister`),
    getMyRegistrations: () => api.get('/events/my-registrations'),
};

export default api;
