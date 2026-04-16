import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Table, Modal, Form, Alert } from 'react-bootstrap';
import { eventService } from '../services/api';
// import { useAuth } from '../context/AuthContext';

const Dashboard = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [currentEvent, setCurrentEvent] = useState({ name: '', description: '', location: '', date: '', maxReg: 50 });
  const [isEditing, setIsEditing] = useState(false);

  // const { user } = useAuth(); // Removed as it's not used

  const fetchEvents = async () => {
    try {
      const response = await eventService.getAll();
      // Filter events where the user is the organizer (if the API supports this or manually filter)
      // For now, show all for simplicity, but ideally filter by organizerId
      setEvents(response.data);
    } catch (_err) {
      console.error("Failed to fetch events:", _err);
      setError('Failed to fetch events.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEvents();
  }, []);

  const handleShowModal = (event = null) => {
    if (event) {
      setCurrentEvent(event);
      setIsEditing(true);
    } else {
      setCurrentEvent({ name: '', description: '', location: '', date: '', maxReg: 50 });
      setIsEditing(false);
    }
    setShowModal(true);
  };

  const handleCloseModal = () => setShowModal(false);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setCurrentEvent({ ...currentEvent, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (isEditing) {
        await eventService.update(currentEvent.id, currentEvent);
      } else {
        await eventService.create(currentEvent);
      }
      fetchEvents();
      handleCloseModal();
    } catch (_err) {
      console.error("Failed to save event:", _err);
      setError('Failed to save event.');
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('Are you sure you want to delete this event?')) {
      try {
        await eventService.delete(id);
        fetchEvents();
      } catch (_err) {
        console.error("Failed to delete event:", _err);
        setError('Failed to delete event.');
      }
    }
  };

  if (loading) return <Container className="mt-5 text-center">Loading...</Container>;

  return (
    <Container className="mt-4">
      <Row className="mb-4 align-items-center">
        <Col>
          <h2>Event Manager Dashboard</h2>
        </Col>
        <Col className="text-end">
          <Button variant="success" onClick={() => handleShowModal()}>Create New Event</Button>
        </Col>
      </Row>

      {error && <Alert variant="danger">{error}</Alert>}

      <Card>
        <Card.Body>
          <Table responsive striped hover>
            <thead>
              <tr>
                <th>Name</th>
                <th>Date</th>
                <th>Location</th>
                <th>Max Reg</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {events.length > 0 ? events.map(event => (
                <tr key={event.id}>
                  <td>{event.name}</td>
                  <td>{event.date}</td>
                  <td>{event.location}</td>
                  <td>{event.maxReg}</td>
                  <td>
                    <Button variant="info" size="sm" className="me-2" onClick={() => handleShowModal(event)}>Edit</Button>
                    <Button variant="danger" size="sm" onClick={() => handleDelete(event.id)}>Delete</Button>
                  </td>
                </tr>
              )) : (
                <tr>
                  <td colSpan="5" className="text-center">No events found.</td>
                </tr>
              )}
            </tbody>
          </Table>
        </Card.Body>
      </Card>

      <Modal show={showModal} onHide={handleCloseModal}>
        <Modal.Header closeButton>
          <Modal.Title>{isEditing ? 'Edit Event' : 'Create Event'}</Modal.Title>
        </Modal.Header>
        <Form onSubmit={handleSubmit}>
          <Modal.Body>
            <Form.Group className="mb-3">
              <Form.Label>Event Name</Form.Label>
              <Form.Control 
                name="name" 
                value={currentEvent.name} 
                onChange={handleInputChange} 
                required 
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Description</Form.Label>
              <Form.Control 
                as="textarea" 
                rows={3} 
                name="description" 
                value={currentEvent.description} 
                onChange={handleInputChange} 
                required 
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Location</Form.Label>
              <Form.Control 
                name="location" 
                value={currentEvent.location} 
                onChange={handleInputChange} 
                required 
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Date</Form.Label>
              <Form.Control 
                type="date" 
                name="date" 
                value={currentEvent.date} 
                onChange={handleInputChange} 
                required 
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Max Registration</Form.Label>
              <Form.Control 
                type="number" 
                name="maxReg" 
                value={currentEvent.maxReg} 
                onChange={handleInputChange} 
                required 
              />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={handleCloseModal}>Cancel</Button>
            <Button variant="primary" type="submit">{isEditing ? 'Update' : 'Create'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </Container>
  );
};

export default Dashboard;
