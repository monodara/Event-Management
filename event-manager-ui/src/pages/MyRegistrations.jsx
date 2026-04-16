import React, { useState, useEffect } from 'react';
import { Container, Table, Alert, Button } from 'react-bootstrap';
import { eventService } from '../services/api';
import { Link } from 'react-router-dom';

const MyRegistrations = () => {
  const [registrations, setRegistrations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchMyRegistrations = async () => {
    try {
      const response = await eventService.getMyRegistrations();
      setRegistrations(response.data);
    } catch (_err) {
      console.error("Failed to fetch your registrations:", _err);
      setError('Failed to fetch your registrations.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMyRegistrations();
  }, []);

  const handleUnregister = async (eventId) => {
    if (window.confirm('Are you sure you want to unregister?')) {
      try {
        await eventService.unregister(eventId);
        fetchMyRegistrations(); // Refresh list after unregistering
      } catch (_err) {
        console.error("Failed to unregister:", _err);
        setError('Failed to unregister.');
      }
    }
  };

  if (loading) return <Container className="mt-5 text-center">Loading your registrations...</Container>;

  return (
    <Container className="mt-4">
      <h2 className="mb-4">My Registrations</h2>
      {error && <Alert variant="danger">{error}</Alert>}
      
      <Alert variant="info">
        Registration status is processed asynchronously. You will receive an email confirmation once your registration is confirmed.
      </Alert>

      <Table responsive striped hover>
        <thead>
          <tr>
            <th>Event Name</th>
            <th>Event Date</th>
            <th>Event Location</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {registrations.length > 0 ? registrations.map(reg => (
            <tr key={reg.id}>
              <td>{reg.event.name}</td>
              <td>{reg.event.date}</td>
              <td>{reg.event.location}</td>
              <td>Confirmed</td> {/* Assuming 'Confirmed' once it appears here */}
              <td>
                <Button variant="danger" size="sm" onClick={() => handleUnregister(reg.eventId)}>Unregister</Button>
              </td>
            </tr>
          )) : (
            <tr>
              <td colSpan="5" className="text-center py-4 text-muted">
                You haven't registered for any events yet. <Link to="/events">Browse events</Link>
              </td>
            </tr>
          )}
        </tbody>
      </Table>
    </Container>
  );
};

export default MyRegistrations;
