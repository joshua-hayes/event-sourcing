# Eventum 🚀

## Overview
Eventum is an open-source event sourcing framework designed to simplify the implementation of event-driven architectures. It provides a set of tools and abstractions to manage event streams, projections, and persistence in a scalable and maintainable way.

## Key Features
- **Event Sourcing**: Capture and store events to build a complete history of changes.
- **Persistence**: Support for multiple persistence providers, including CosmosDB and InMemory.
- **Projections**: Create and manage projections to transform event streams into materialized views.
- **Abstractions**: Provide interfaces and base classes to facilitate the implementation of event sourcing and projections.

## What is Event Sourcing? ▶️

Event sourcing is a design pattern where changes to the application state are stored as a sequence of events. Instead of storing just the current state, the system records every change (event) that has occurred. This allows for a complete history of changes, which can be replayed to reconstruct the state at any point in time.

## What are Materialized Views? 🗃️

Materialized views are precomputed views that store the result of a query. In the context of event sourcing, they are projections of the event stream that provide a read-optimized representation of the data. Materialized views can be used to improve query performance and provide a simplified view of the data for consumers.

## Getting Started
To get started with Eventum, follow these steps:

1. **Clone the Repository**: Clone the Eventum repository to your local machine.
   ```bash
   git clone https://github.com/joshua-hayes/Eventum.git

### Test Projects

You can refer to the test projects in the repository to understand how to use Eventum in different scenarios. The test projects provide examples and can serve as a reference for implementing event sourcing and projections.

## License
Eventum is released under the MIT License. See the [LICENSE](/LICENSE) file for more details.
