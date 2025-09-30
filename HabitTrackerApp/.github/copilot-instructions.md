<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->
- [x] Verify that the copilot-instructions.md file in the .github directory is created.

- [x] Clarify Project Requirements
	.NET MAUI cross-platform mobile application for habit tracker connecting to ASP.NET Core API on localhost:5178

- [x] Scaffold the Project
	Created MAUI project structure with iOS, Android, Windows, MacCatalyst support

- [x] Customize the Project
	Added MVVM pattern with CommunityToolkit.Mvvm, HTTP client for API communication, SQLite for offline storage, sync service for data synchronization

- [x] Install Required Extensions
	Installed .NET MAUI Extension (ms-dotnettools.dotnet-maui)

- [x] Compile the Project
	Build successful across all platforms (iOS, Android, Windows, macOS)

- [x] Create and Run Task
	Created "Build MAUI App" task for building the application

- [x] Launch the Project
	Ready to launch on any supported platform via Visual Studio or CLI

- [x] Ensure Documentation is Complete
	Created comprehensive README.md with architecture details, setup instructions, and usage examples

## Project Summary

Successfully created a complete .NET MAUI cross-platform mobile application with:

**Architecture:**
- Cross-platform support (iOS, Android, Windows, macOS)
- Offline-first SQLite database with automatic sync
- MVVM pattern with CommunityToolkit.Mvvm
- Clean separation of Models, Services, and ViewModels

**Key Features:**
- Simple habit tracking with daily completion
- Complex routine sessions with timers and metrics
- API integration with ASP.NET Core backend
- Background synchronization when online
- Responsive UI design for all platforms

**Technical Implementation:**
- Models: Habit, RoutineSession, DailyHabitEntry, ActivityTemplate
- Services: ApiService, LocalDatabaseService, SyncService  
- ViewModels: MainViewModel, HabitListViewModel, HabitDetailViewModel, RoutineSessionViewModel
- Dependencies: CommunityToolkit.Mvvm, SQLite, System.Text.Json

**Ready for Development:**
- All core infrastructure implemented
- Build system configured for all platforms
- Extensions installed and configured
- Comprehensive documentation provided

The mobile app is now ready to connect to the existing ASP.NET Core API and provide full offline-first habit tracking capabilities across all major platforms.