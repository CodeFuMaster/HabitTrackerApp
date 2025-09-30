using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;

namespace HabitTrackerApp.MAUI.Views;

public partial class AddHabitPage : ContentPage
{
    private readonly IHabitService _habitService;

    public AddHabitPage(IHabitService habitService)
    {
        InitializeComponent();
        _habitService = habitService;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a habit name", "OK");
            return;
        }

        try
        {
            var habit = new Habit
            {
                Name = NameEntry.Text.Trim(),
                Description = DescriptionEditor.Text?.Trim() ?? string.Empty
            };

            await _habitService.CreateHabitAsync(habit);
            await DisplayAlert("Success", "Habit created successfully!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create habit: {ex.Message}", "OK");
        }
    }

    private void OnTuesdayGymClicked(object sender, EventArgs e)
    {
        NameEntry.Text = "Tuesday Gym";
        DescriptionEditor.Text = "Complete gym workout with Trap-bar deadlifts, pull-ups, push-ups, and farmers walk";
    }

    private void OnMorningRoutineClicked(object sender, EventArgs e)
    {
        NameEntry.Text = "Morning Routine";
        DescriptionEditor.Text = "Morning wellness routine with Wim Hof breathing, meditation, and cold shower";
    }

    private void OnBJJClicked(object sender, EventArgs e)
    {
        NameEntry.Text = "BJJ Training";
        DescriptionEditor.Text = "Brazilian Jiu-Jitsu training session with techniques and sparring";
    }
}