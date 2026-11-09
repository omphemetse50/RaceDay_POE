# RaceDay Web Application Project Document

## 1. Project Title
RaceDay Web Application

## 2. Module / Assessment
POE - Automation of Web Application

## 3. Overview
This project involves the design, development, and deployment of a full web application for the RaceDay platform. The application will consist of an ASP.NET Core MVC web front end that consumes a RESTful API, manages user authentication, supports role-based navigation, and integrates Azure Blob Storage for image uploads.

The system is intended to support two main user groups:
- Organisers: manage events, categories, enrolments, results, and profile information.
- Participants: view upcoming events, enrol in races, track their personal status, and view result information.

The application will also be containerised using Docker to allow it to run consistently in a production-ready environment.

## 4. Project Purpose
The purpose of this project is to demonstrate the ability to build a modern web application using ASP.NET Core, REST APIs, authentication, database access, cloud storage integration, and deployment automation. The system is based on a real-world race/event management scenario and requires a complete development lifecycle from design to deployment.

## 5. Learning Outcomes Addressed
At the end of this project, students should be able to:
- Build an ASP.NET Core MVC web application that consumes a RESTful API.
- Implement session-based authentication and role-specific interfaces for Organiser and Participant users.
- Integrate Azure Blob Storage into the API for cloud-based file storage.
- Containerise an ASP.NET Core application using Docker.

## 6. Problem Statement
RaceDay is an event management platform where organisers create running and cycling events, set categories, manage participant enrolments, and record results. Participants need a simple way to view events, enrol in the appropriate category, and monitor their progress.

The application must provide a secure, role-aware, and user-friendly experience for both user types while incorporating robust system functionality and deployment readiness.

## 7. Scope of the System
The system will contain the following major components:
1. Web front end (ASP.NET Core MVC)
2. REST API backend
3. Database for users, events, categories, enrolments, and results
4. Session-based user authentication
5. Role-based access control
6. Azure Blob Storage for uploaded files
7. Docker configuration for container deployment
8. CI/CD configuration via GitHub Actions

## 8. Functional Requirements

### 8.1 MVC Application Setup
- The MVC application must be created as an ASP.NET Core MVC web app.
- It must consume the REST API for data operations.
- The MVC project must not contain any direct database access or replicated business logic.
- The system must interact with the API through HTTP requests.

### 8.2 Authentication and Role-Based Navigation
- Users must be able to register and log in through the MVC application by communicating with the REST API.
- The application must maintain the user’s authenticated state and role across all pages using session management.
- Each role must have a distinct interface and navigation experience.
- Unauthenticated users must not be able to access protected pages.
- Users must be able to end their session and be redirected appropriately.

### 8.3 Organiser Interface
The organiser interface must allow the organiser to:
- View a dashboard showing a summary of all events, total enrolments per event, and upcoming event dates.
- Create, update, and delete events, including required fields:
  - name
  - description
  - date
  - location
  - distance
  - event type
- Upload an event banner image when creating or editing an event.
- Manage event categories by adding age or distance categories for each event.
- View all enrolments for events and see which participants have enrolled in each category.
- Capture and manage race results for enrolled participants.
- View a list of registered participants and their result information.
- Manage their personal profile.

### 8.4 Participant Interface
The participant interface must allow a participant to:
- View a home page showing upcoming events with filtering options.
- View a page showing event information and available categories.
- Enrol in an event of their choice and select a category.
- View their personal event enrolment status.
- View a personal dashboard showing all events they have enrolled in and the current enrolment status.
- View a personal results page containing completed events and result details.
- Update their personal profile information and upload a profile image.

### 8.5 Azure Blob Storage
- The application must support file uploads through Azure Blob Storage.
- Event banner images and profile images must be stored in Azure Blob Storage.
- Uploaded images must be displayed on the event details page and profile page.
- The API must communicate with Azure Blob Storage securely and not expose storage credentials directly in the application.

### 8.6 Docker
- The MVC application must run successfully inside a Docker container.
- The Dockerfile must be well structured and follow best practices for containerising an ASP.NET Core application.
- The Docker run instructions must be documented clearly.
- The application should be configured to run locally through a single command.

## 9. Non-Functional Requirements
- Security: authentication, authorization, and secure handling of credentials.
- Usability: clean and intuitive interface, navigation, and validation.
- Responsiveness: mobile-friendly and desktop-compatible layout.
- Reliability: correct handling of validation and system errors.
- Maintainability: clear project structure and separation of concerns.
- Scalability: architecture should support future growth and additional features.
- Performance: efficient loading of event and result data.

## 10. Technical Requirements
The project should use the following technology stack:
- ASP.NET Core 8 MVC
- RESTful API backend
- Entity Framework Core
- SQL Server / LocalDB
- Session-based authentication
- Role-based authorization
- Azure Blob Storage SDK
- Docker
- GitHub repository management
- GitHub Actions for CI/CD

## 11. System Architecture
The system will follow a layered architecture:

1. Presentation Layer
   - ASP.NET Core MVC web application
   - Views and controllers for all user-facing pages

2. API Layer
   - RESTful endpoints for authentication, users, events, categories, enrolments, and results

3. Data Access Layer
   - Entity Framework Core with SQL Server
   - Database models for events, categories, enrolments, users, and results

4. Storage Layer
   - Azure Blob Storage for uploaded media files

5. Security Layer
   - Cookie-based session management
   - Role checks for organiser and participant access

## 12. Role-Based Access Model

### Organiser
Organisers can:
- register
- log in
- manage events
- create/update/delete event categories
- view participant enrolments
- record and update results
- manage their own profile

### Participant
Participants can:
- register
- log in
- view events
- select and enrol in event categories
- view and cancel their own enrolments
- view personal results
- manage their own profile

### Access Restrictions
- Unauthenticated users cannot access protected resources.
- Participants cannot create or edit events.
- Organisers cannot enrol in events as participants.
- Users can only modify resources they own unless otherwise authorised.

## 13. Database Design
The application should store the following entities:
- Users
- Events
- Categories
- Enrolments
- Results

### Suggested Relationships
- One organiser can create many events.
- One event can have many categories.
- One event can have many enrolments.
- One participant can have many enrolments.
- One enrolment can have one result.

This design ensures proper data integrity and supports role-based permissions across the platform.

## 14. API Endpoints (Suggested)
### Authentication
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/logout

### Profile
- GET /api/profile
- PUT /api/profile

### Events
- GET /api/events
- GET /api/events/{id}
- POST /api/events
- PUT /api/events/{id}
- DELETE /api/events/{id}

### Categories
- GET /api/events/{eventId}/categories
- POST /api/events/{eventId}/categories
- PUT /api/categories/{id}
- DELETE /api/categories/{id}

### Enrolments
- POST /api/enrolments
- GET /api/enrolments/mine
- GET /api/events/{eventId}/enrolments
- DELETE /api/enrolments/{id}

### Results
- POST /api/results
- PUT /api/results/{id}
- GET /api/events/{eventId}/results
- GET /api/results/mine

## 15. Front-End User Experience Requirements
The interface should be:
- clean
- easy to navigate
- visually structured
- consistent in colour and typography
- mobile responsive
- accessible and understandable

### UI Requirements
- Clear navigation menus for organiser and participant roles
- Distinct dashboard layout for each role
- Forms with validation and user-friendly error messages
- Consistent styling across pages
- Modal or alert messages when required

## 16. Error Handling
The application must handle invalid inputs gracefully and show meaningful messages without crashing. Examples include:
- duplicate email registration
- invalid login attempts
- invalid event parameters
- duplicate enrolments
- invalid finish times
- unauthorised access attempts

## 17. Deployment and Containerisation
The application must be run successfully in Docker. The deployment solution should include:
- Dockerfile for the ASP.NET Core MVC application
- instructions for building and running the container
- environment configuration for production and local environments
- secure handling of secrets and connection strings

## 18. GitHub Repository Requirements
The project must be maintained in a GitHub repository with:
- a clear repository name
- working project structure
- meaningful commit history
- documentation
- setup instructions
- CI/CD workflow

## 19. Testing Strategy
The project should include tests covering:
- user registration and login
- role-based authorization
- event creation and updates
- category validation
- participant enrolment logic
- result recording and validation
- unauthorised access protection

Automated tests should be implemented using xUnit and integrated with GitHub Actions to validate the build on every push.

## 20. Submission Requirements
The final submission should include:
- source code in GitHub
- a working README with project setup instructions
- a Dockerfile and run instructions
- a project demonstration video link
- project documentation explaining architecture, features, and deployment

## 21. Project Timeline
A realistic development plan is as follows:

### Week 1
- Requirements review and system design
- Database and model creation
- API controller planning

### Week 2
- Authentication and role-based security
- Event and category implementation
- Enrolment logic

### Week 3
- Results management and validation
- MVC application setup
- UI design and views

### Week 4
- Azure Blob Storage integration
- Docker setup and testing
- CI/CD configuration

### Week 5
- Final testing and bug fixing
- Documentation and submission preparation

## 22. Risks and Considerations
Potential risks include:
- authentication/session mismatch between API and MVC app
- invalid role-based checks
- file upload failures in cloud storage
- Docker setup issues
- incorrect mapping between MVC forms and API models
- stale or invalid data due to poor validation

Mitigation strategies:
- maintain strict validation on both server and client
- use clear role checks at controller and service level
- use secure environment variables for cloud credentials
- test container locally before submission
- validate API-to-MVC integration thoroughly

## 23. Expected Outcome
The final RaceDay application should provide a complete, secure, and user-friendly platform that allows organisers and participants to interact with events, enrolments, results, profile information, and uploaded media. It should demonstrate professional-level software design, deployment readiness, and a good user experience.

## 24. Conclusion
This project combines web application development, API integration, role-based security, cloud storage, and containerisation into one comprehensive assignment. It requires a practical, structured, and tested implementation that demonstrates the student’s ability to build a modern, full-stack application using industry-standard development practices.

## 25. Checklist for Completion
Before submitting the project, confirm that:
- [ ] The MVC app is built and running
- [ ] The API is consumed correctly
- [ ] Authentication works for both roles
- [ ] Protected pages are restricted properly
- [ ] Event management works for organisers
- [ ] Participant enrolment works correctly
- [ ] Results can be captured and displayed
- [ ] Azure Blob Storage is integrated
- [ ] Docker setup works
- [ ] The README is complete
- [ ] GitHub repository is ready
- [ ] Demo video is submitted

## 26. References
- ASP.NET Core Documentation
- Entity Framework Core Documentation
- Azure Blob Storage Documentation
- Docker Documentation
- GitHub Actions Documentation

---

This document serves as the official project specification for the RaceDay Web Application POE assignment.
