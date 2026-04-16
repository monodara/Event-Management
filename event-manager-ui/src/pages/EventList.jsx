import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Alert } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { eventService } from '../services/api';

const EventList = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const response = await eventService.getAll();
        setEvents(response.data);
      } catch (_err) {
        console.error("Failed to fetch events:", _err);
        setError('Failed to fetch events.');
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  if (loading) return <Container className="mt-5 text-center">Loading events...</Container>;

  return (
    <Container className="mt-4">
      <h2 className="mb-4">Browse Events</h2>
      {error && <Alert variant="danger">{error}</Alert>}
      <Row>
        {events.length > 0 ? events.map(event => (
          <Col md={4} key={event.id} className="mb-4">
            <Card className="h-100 shadow-sm">
              <Card.Body className="d-flex flex-column">
                <Card.Title>{event.name}</Card.Title>
                <Card.Subtitle className="mb-2 text-muted">{event.date} | {event.location}</Card.Subtitle>
                <Card.Text className="text-truncate">
                  {event.description}
                </Card.Text>
                <div className="mt-auto">
                  <Button as={Link} to={`/events/${event.id}`} variant="primary" className="w-100">View Details</Button>
                </div>
              </Card.Body>
            </Card>
          </Col>
        )) : (
          <Col>
            <Alert variant="info">No events available at the moment.</Alert>
          </Col>
        )}
      </Row>
    </Container>
  );
};

export default EventList;
