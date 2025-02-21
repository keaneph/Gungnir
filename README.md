# Gungnir - Student Information System

A comprehensive Student Information Management System developed using Windows Presentation Foundation (WPF), designed to efficiently manage college records, program information, and student data.

## Features

- **User Authentication**
  - Secure login system
  - User session management

- **College Management**
  - Add/View/Edit/Delete colleges
  - Real-time updates
  - Sorting capabilities

- **Program Management**
  - Add/View/Edit/Delete programs
  - Link programs to colleges
  - Automated college code association

- **Student Management**
  - Add/View/Edit/Delete students
  - ID number generation
  - Program and college association

- **History Tracking**
  - Track all system activities
  - Real-time updates display
  - Sortable history view

## Color Reference

| Color             | Hex                                                                |
| ----------------- | ------------------------------------------------------------------ |
| Primary Purple | ![#7160E8](https://via.placeholder.com/10/7160E8?text=+) #7160E8 |
| Dark Gray | ![#1F1F1F](https://via.placeholder.com/10/1F1F1F?text=+) #1F1F1F |
| Medium Light Gray | ![#2E2E2E](https://via.placeholder.com/10/2E2E2E?text=+) #2E2E2E |
| Light Gray | ![#383838](https://via.placeholder.com/10/383838?text=+) #383838 |
| White | ![#D6D6D6](https://via.placeholder.com/10/D6D6D6?text=+) #D6D6D6 |

## Installation

1. Clone the repository
```bash
git clone https://github.com/keaneph/Gungnir.git
```

2. Open the project (sis-app.sln) in Visual Studio
3. Build the project
4. Run the project

## Project Structure

```bash
sis-app/
├── Controls/          
│   ├── Add/          # Add operation controls
│   └── View/         # View operation controls
├── Models/           # Data models
├── Services/         # Data services
├── Views/            # Main views
├── Resources/        # Resource files
└── App.xaml         # Application entry point
```

## Technologies Used
C# (.NET Framework)
Windows Presentation Foundation (WPF)
XAML
CSV Data Storage

## Running the Application
Start the application (F5 or Run button in Visual Studio)
Login with your credentials
Navigate using the sidebar menu
Manage colleges, programs, and students

## Author
[@keaneph](https://github.com/keaneph)

## Acknowledgements

