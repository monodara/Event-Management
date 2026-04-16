import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Card, Button, Row, Col, Alert, Badge, ListGroup } from 'react-bootstrap';
import { eventService } from '../services/api';
import { useAuth } from '../context/AuthContext';

const EventDetails = () => {
  const { id } = useParams();
  const [event, setEvent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [registering, setRegistering] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const { user, isUser } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchEvent = async () => {
      try {
        const response = await eventService.getById(id);
        setEvent(response.data);
      } catch (_err) {
        console.error("Failed to fetch event details:", _err);
        setError('Failed to fetch event details.');
      } finally {
        setLoading(false);
      }
    };
    fetchEvent();
  }, [id]);

  const handleRegister = async () => {
    if (!user) {
      navigate('/login', { state: { from: { pathname: `/events/${id}` } } });
      return;
    }

    setRegistering(true);
    setError('');
    setSuccess('');

    try {
      await eventService.register(id);
      setSuccess('Registration request sent! You will receive an email confirmation shortly.');
    } catch (_err) {
      console.error("Failed to register for event:", _err);
      setError(_err.response?.data?.Message || 'Failed to register for the event.');
    } finally {
      setRegistering(false);
    }
  };

  if (loading) return <Container className="mt-5 text-center">Loading event details...</Container>;
  if (!event) return <Container className="mt-5"><Alert variant="danger">Event not found.</Alert></Container>;

  return (
    <Container className="mt-4">
      <Button variant="link" onClick={() => navigate(-1)} className="mb-3 ps-0 text-decoration-none text-muted">
        &larr; Back to events
      </Button>
      
      <Row>
        <Col md={8}>
          <Card className="shadow-sm">
            <Card.Body>
              <div className="d-flex justify-content-between align-items-start mb-3">
                <h2 className="mb-0">{event.name}</h2>
                <Badge bg="info" className="p-2">{event.isOpenForReg ? 'Open for Registration' : 'Closed'}</Badge>
              </div>
              
              <p className="text-muted mb-4">
                <i className="bi bi-calendar-event me-2"></i> {event.date} | 
                <i className="bi bi-geo-alt me-2 ms-2"></i> {event.location}
              </p>
              
              <h5 className="mb-3">About this event</h5>
              <p className="lead">{event.description}</p>
              
              <hr className="my-4" />
              
              <h5>Materials & Resources</h5>
              {event.documentUris && event.documentUris.length > 0 ? (
                <ListGroup variant="flush">
                  {event.documentUris.map((uri, index) => (
                    <ListGroup.Item key={index} className="px-0">
                      <a href={uri} target="_blank" rel="noopener noreferrer" className="text-primary text-decoration-none">
                        Material {index + 1}
                      </a>
                    </ListGroup.Item>
                  ))}
                </ListGroup>
              ) : (
                <p className="text-muted">No materials uploaded yet.</p>
              )}
            </Card.Body>
          </Card>
        </Col>
        
        <Col md={4}>
          <Card className="shadow-sm mb-4">
            <Card.Body>
              <h5>Registration</h5>
              <p className="text-muted mb-4">Max Capacity: {event.maxReg}</p>
              
              {error && <Alert variant="danger" className="py-2 small">{error}</Alert>}
              {success && <Alert variant="success" className="py-2 small">{success}</Alert>}
              
              <Button 
                variant="primary" 
                className="w-100 py-2 fw-bold" 
                onClick={handleRegister}
                disabled={registering || !event.isOpenForReg || !isUser()}
              >
                {registering ? 'Processing...' : 'Register Now'}
              </Button>
              
              {!isUser() && user && (
                <p className="mt-2 text-center small text-warning">Only standard users can register for events.</p>
              )}
            </Card.Body>
          </Card>
          
          <Card className="shadow-sm">
            <Card.Body>
              <h5>Organizer</h5>
              <p className="mb-0 fw-bold">{event.organizer?.fullName || 'Event Management Team'}</p>
              <p className="text-muted small">{event.organizer?.email}</p>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default EventDetails;
