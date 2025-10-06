using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerApp.Data
{
    /// <summary>
    /// Seed data for Exercise system - ATG exercises and gym workouts
    /// </summary>
    public static class ExerciseSeedData
    {
        public static async Task SeedExercisesAsync(AppDbContext context)
        {
            // Check if exercises already exist
            if (await context.Exercises.AnyAsync())
            {
                Console.WriteLine("Exercises already seeded. Skipping...");
                return;
            }

            Console.WriteLine("Seeding exercise data...");

            // Find or create habits
            var atgHabit = await GetOrCreateHabit(context, "ATG Exercise List", 
                "Athletic Truth Group daily routine for knee and lower leg health", 
                HabitType.Routine);

            var gymAHabit = await GetOrCreateHabit(context, "Gym Workout A", 
                "Full body workout focusing on deadlifts and vertical pulling", 
                HabitType.Gym);

            var gymBHabit = await GetOrCreateHabit(context, "Gym Workout B", 
                "Full body workout focusing on squats and overhead pressing", 
                HabitType.Gym);

            var gymCHabit = await GetOrCreateHabit(context, "Gym Workout C", 
                "Full body workout focusing on loaded carries and conditioning", 
                HabitType.Gym);

            // Seed ATG Exercises (13 exercises)
            await SeedATGExercises(context, atgHabit.Id);

            // Seed Gym A Exercises (7 exercises)
            await SeedGymAExercises(context, gymAHabit.Id);

            // Seed Gym B Exercises (8 exercises)
            await SeedGymBExercises(context, gymBHabit.Id);

            // Seed Gym C Exercises (7 exercises)
            await SeedGymCExercises(context, gymCHabit.Id);

            await context.SaveChangesAsync();

            Console.WriteLine("Exercise seeding complete!");
            Console.WriteLine($"- ATG: {await context.Exercises.CountAsync(e => e.HabitId == atgHabit.Id)} exercises");
            Console.WriteLine($"- Gym A: {await context.Exercises.CountAsync(e => e.HabitId == gymAHabit.Id)} exercises");
            Console.WriteLine($"- Gym B: {await context.Exercises.CountAsync(e => e.HabitId == gymBHabit.Id)} exercises");
            Console.WriteLine($"- Gym C: {await context.Exercises.CountAsync(e => e.HabitId == gymCHabit.Id)} exercises");
        }

        private static async Task<Habit> GetOrCreateHabit(AppDbContext context, string name, string description, HabitType habitType)
        {
            var habit = await context.Habits.FirstOrDefaultAsync(h => h.Name == name);
            
            if (habit == null)
            {
                // Ensure we have at least one category
                var category = await context.Categories.FirstOrDefaultAsync();
                if (category == null)
                {
                    category = new Category
                    {
                        Name = "Fitness",
                        Description = "Health and fitness activities",
                        ImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=400"
                    };
                    context.Categories.Add(category);
                    await context.SaveChangesAsync();
                }

                habit = new Habit
                {
                    Name = name,
                    Description = description,
                    ShortDescription = description.Length > 50 ? description.Substring(0, 50) + "..." : description,
                    HabitType = habitType,
                    RecurrenceType = 0, // Daily
                    ImageUrl = habitType == HabitType.Routine ? 
                        "https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=400" : // Stretching
                        "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",  // Gym
                    Tags = habitType == HabitType.Routine ? "ATG,Mobility,Flexibility" : "Strength,Gym,Weights",
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    CategoryId = category.Id
                };
                
                context.Habits.Add(habit);
                await context.SaveChangesAsync();
            }

            return habit;
        }

        private static async Task SeedATGExercises(AppDbContext context, int habitId)
        {
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Tibialis Raises",
                    OrderIndex = 1,
                    TargetSets = 3,
                    TargetReps = 25,
                    TargetRPE = 7,
                    RestSeconds = 60,
                    ExerciseType = "Strength",
                    MuscleGroups = "Tibialis,Lower Leg",
                    Equipment = "Bodyweight,Slant Board (optional)",
                    Notes = "Raise toes toward shin, control on the way down. Can add weight by holding a dumbbell.",
                    ImageUrl = "https://images.unsplash.com/photo-1434682772747-f16d3ea162c3?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=kpdq7KJkNdA",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Straight-Leg Calf Raises",
                    OrderIndex = 2,
                    TargetSets = 3,
                    TargetReps = 25,
                    TargetRPE = 7,
                    RestSeconds = 60,
                    ExerciseType = "Strength",
                    MuscleGroups = "Calves,Gastrocnemius",
                    Equipment = "Bodyweight,Elevated Surface",
                    Notes = "Keep legs straight, full range of motion from deep stretch to full contraction.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "KOT Calf Raises (Knees Over Toes)",
                    OrderIndex = 3,
                    TargetSets = 3,
                    TargetReps = 25,
                    TargetRPE = 7,
                    RestSeconds = 60,
                    ExerciseType = "Strength",
                    MuscleGroups = "Calves,Soleus,Achilles",
                    Equipment = "Bodyweight,Elevated Surface",
                    Notes = "Bend knees and push them forward over toes while raising heels. Targets soleus muscle.",
                    ImageUrl = "https://images.unsplash.com/photo-1434682881908-b43d0467b798?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "FHL Calf Raises (Big Toe)",
                    OrderIndex = 4,
                    TargetSets = 3,
                    TargetReps = 25,
                    TargetRPE = 7,
                    RestSeconds = 60,
                    ExerciseType = "Strength",
                    MuscleGroups = "Flexor Hallucis Longus,Foot,Ankle",
                    Equipment = "Bodyweight",
                    Notes = "Raise only the big toe while keeping other toes down. Strengthens foot and ankle stability.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Elephant Walk",
                    OrderIndex = 5,
                    TargetDuration = 60,
                    TargetRPE = 6,
                    ExerciseType = "Mobility",
                    MuscleGroups = "Hamstrings,Calves,Lower Back",
                    Equipment = "Bodyweight",
                    Notes = "Walk forward with straight legs, reaching hands to toes with each step. Improves hamstring flexibility.",
                    ImageUrl = "https://images.unsplash.com/photo-1599058917212-d750089bc07e?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "ATG Split Squat",
                    OrderIndex = 6,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetRPE = 8,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Quads,Glutes,Knees",
                    Equipment = "Bodyweight,Bench,Dumbbells (optional)",
                    Notes = "Back foot elevated, front knee tracks over toes into deep lunge. Gold standard for knee health.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=1SmMx1pDN94",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Patrick Step",
                    OrderIndex = 7,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetRPE = 7,
                    RestSeconds = 60,
                    ExerciseType = "Strength",
                    MuscleGroups = "Hip Flexors,Quads,Balance",
                    Equipment = "Bodyweight,Box/Bench",
                    Notes = "Step up leading with hip flexor. Pause at top, control descent. Builds hip flexor strength.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Nordic Hamstring Curl",
                    OrderIndex = 8,
                    TargetSets = 3,
                    TargetReps = 5,
                    TargetRPE = 9,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Hamstrings,Lower Back,Glutes",
                    Equipment = "Anchor Point,Resistance Band (optional)",
                    Notes = "Kneel with ankles secured, slowly lower torso forward. Most effective hamstring exercise. Use assistance as needed.",
                    ImageUrl = "https://images.unsplash.com/photo-1434682881908-b43d0467b798?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=2_VfV7vZiRM",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "ATG Squat Hold",
                    OrderIndex = 9,
                    TargetDuration = 30,
                    TargetRPE = 7,
                    ExerciseType = "Mobility",
                    MuscleGroups = "Quads,Ankles,Hips,Lower Back",
                    Equipment = "Bodyweight",
                    Notes = "Deep squat position, knees forward past toes, heels down. Hold position. Improves ankle and hip mobility.",
                    ImageUrl = "https://images.unsplash.com/photo-1599058917212-d750089bc07e?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Cobra Stretch",
                    OrderIndex = 10,
                    TargetDuration = 60,
                    TargetRPE = 5,
                    ExerciseType = "Flexibility",
                    MuscleGroups = "Spine,Abs,Hip Flexors",
                    Equipment = "Mat",
                    Notes = "Lie face down, press upper body up while keeping hips on ground. Extends and decompresses spine.",
                    ImageUrl = "https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Jefferson Curl",
                    OrderIndex = 11,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetRPE = 6,
                    RestSeconds = 60,
                    ExerciseType = "Flexibility",
                    MuscleGroups = "Spine,Hamstrings,Lower Back",
                    Equipment = "Light Dumbbell (5-10 lbs)",
                    Notes = "Stand on elevated surface, slowly curl spine forward vertebra by vertebra. Improves spinal flexion.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=nNPxi_8r-po",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Pancake Good Morning",
                    OrderIndex = 12,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetRPE = 6,
                    RestSeconds = 60,
                    ExerciseType = "Flexibility",
                    MuscleGroups = "Hamstrings,Adductors,Lower Back",
                    Equipment = "Bodyweight",
                    Notes = "Wide stance, hinge forward trying to get torso parallel to ground. Improves hip hinge and hamstring flexibility.",
                    ImageUrl = "https://images.unsplash.com/photo-1599058917212-d750089bc07e?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Seated Good Morning",
                    OrderIndex = 13,
                    TargetSets = 3,
                    TargetReps = 15,
                    TargetRPE = 6,
                    RestSeconds = 60,
                    ExerciseType = "Flexibility",
                    MuscleGroups = "Lower Back,Glutes,Hamstrings",
                    Equipment = "Bench",
                    Notes = "Sit on bench, hinge forward from hips keeping back straight. Strengthens lower back in safe position.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Exercises.AddRange(exercises);
        }

        private static async Task SeedGymAExercises(AppDbContext context, int habitId)
        {
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Trap Bar Deadlift",
                    OrderIndex = 1,
                    TargetSets = 4,
                    TargetReps = 6,
                    TargetWeight = 225,
                    TargetRPE = 8,
                    RestSeconds = 180,
                    ExerciseType = "Strength",
                    MuscleGroups = "Glutes,Hamstrings,Quads,Lower Back,Traps",
                    Equipment = "Trap Bar,Plates",
                    Notes = "Main lift. Focus on explosiveness. More quad-dominant than conventional deadlift.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Pull-Ups",
                    OrderIndex = 2,
                    TargetSets = 4,
                    TargetReps = 8,
                    TargetRPE = 8,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Lats,Biceps,Upper Back,Core",
                    Equipment = "Pull-Up Bar",
                    Notes = "Full range of motion. Chin over bar. Use assistance or add weight as needed.",
                    ImageUrl = "https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=eGo4IYlbE5g",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Overhead Press",
                    OrderIndex = 3,
                    TargetSets = 4,
                    TargetReps = 8,
                    TargetWeight = 95,
                    TargetRPE = 8,
                    RestSeconds = 150,
                    ExerciseType = "Strength",
                    MuscleGroups = "Shoulders,Triceps,Upper Chest,Core",
                    Equipment = "Barbell,Plates",
                    Notes = "Press from shoulders to overhead. Keep core tight. Bar path straight up.",
                    ImageUrl = "https://images.unsplash.com/photo-1532384816664-01b8b7238c8d?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Barbell Row",
                    OrderIndex = 4,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetWeight = 135,
                    TargetRPE = 7,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Upper Back,Lats,Biceps,Lower Back",
                    Equipment = "Barbell,Plates",
                    Notes = "Bent over row. Pull to lower chest/upper abs. Squeeze shoulder blades together.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Dips",
                    OrderIndex = 5,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetRPE = 7,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Chest,Triceps,Shoulders",
                    Equipment = "Dip Bars,Belt (optional)",
                    Notes = "Lean forward for more chest activation. Keep shoulders down. Add weight when ready.",
                    ImageUrl = "https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Ab Wheel Rollouts",
                    OrderIndex = 6,
                    TargetSets = 3,
                    TargetReps = 12,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Abs,Core,Lower Back,Shoulders",
                    Equipment = "Ab Wheel",
                    Notes = "Roll out slowly, maintain tension. Start from knees if needed. Advanced: standing rollouts.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Farmer's Carry",
                    OrderIndex = 7,
                    TargetDuration = 60,
                    TargetWeight = 70,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Grip,Forearms,Traps,Core,Legs",
                    Equipment = "Dumbbells or Farmer's Walk Handles",
                    Notes = "Heavy weight per hand. Walk with good posture. Great finisher exercise.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Exercises.AddRange(exercises);
        }

        private static async Task SeedGymBExercises(AppDbContext context, int habitId)
        {
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Back Squat",
                    OrderIndex = 1,
                    TargetSets = 4,
                    TargetReps = 6,
                    TargetWeight = 225,
                    TargetRPE = 8,
                    RestSeconds = 180,
                    ExerciseType = "Strength",
                    MuscleGroups = "Quads,Glutes,Hamstrings,Lower Back,Core",
                    Equipment = "Barbell,Rack,Plates",
                    Notes = "Main lift. High bar or low bar. Depth to parallel or below. Brace core hard.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=ultWZbUMPL8",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Bench Press",
                    OrderIndex = 2,
                    TargetSets = 4,
                    TargetReps = 8,
                    TargetWeight = 185,
                    TargetRPE = 8,
                    RestSeconds = 150,
                    ExerciseType = "Strength",
                    MuscleGroups = "Chest,Triceps,Shoulders",
                    Equipment = "Barbell,Bench,Rack,Plates",
                    Notes = "Touch chest, full range of motion. Keep shoulder blades retracted. Feet planted.",
                    ImageUrl = "https://images.unsplash.com/photo-1532384816664-01b8b7238c8d?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Romanian Deadlift",
                    OrderIndex = 3,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetWeight = 185,
                    TargetRPE = 7,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Hamstrings,Glutes,Lower Back",
                    Equipment = "Barbell,Plates",
                    Notes = "Hinge at hips, slight knee bend. Bar stays close to legs. Feel stretch in hamstrings.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Face Pulls",
                    OrderIndex = 4,
                    TargetSets = 4,
                    TargetReps = 15,
                    TargetRPE = 6,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Rear Delts,Upper Back,Rotator Cuff",
                    Equipment = "Cable Machine,Rope Attachment",
                    Notes = "Pull rope to face, external rotation at end. High reps for shoulder health.",
                    ImageUrl = "https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Incline Dumbbell Press",
                    OrderIndex = 5,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetWeight = 65,
                    TargetRPE = 7,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Upper Chest,Shoulders,Triceps",
                    Equipment = "Dumbbells,Incline Bench",
                    Notes = "30-45 degree incline. Full range of motion. Control the descent.",
                    ImageUrl = "https://images.unsplash.com/photo-1532384816664-01b8b7238c8d?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Leg Curl",
                    OrderIndex = 6,
                    TargetSets = 3,
                    TargetReps = 12,
                    TargetWeight = 90,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Hamstrings",
                    Equipment = "Leg Curl Machine",
                    Notes = "Full range of motion. Squeeze at top. Control the negative. Hamstring isolation.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Cable Crunches",
                    OrderIndex = 7,
                    TargetSets = 3,
                    TargetReps = 15,
                    TargetWeight = 100,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Abs,Core",
                    Equipment = "Cable Machine,Rope Attachment",
                    Notes = "Kneel facing away from cable. Crunch down, rounding spine. Squeeze abs hard at bottom.",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Hanging Leg Raises",
                    OrderIndex = 8,
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Abs,Hip Flexors,Core,Grip",
                    Equipment = "Pull-Up Bar",
                    Notes = "Hang from bar, raise legs to 90 degrees or higher. Control the swing. Bend knees if needed.",
                    ImageUrl = "https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Exercises.AddRange(exercises);
        }

        private static async Task SeedGymCExercises(AppDbContext context, int habitId)
        {
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Front Squat",
                    OrderIndex = 1,
                    TargetSets = 4,
                    TargetReps = 6,
                    TargetWeight = 185,
                    TargetRPE = 8,
                    RestSeconds = 180,
                    ExerciseType = "Strength",
                    MuscleGroups = "Quads,Glutes,Core,Upper Back",
                    Equipment = "Barbell,Rack,Plates",
                    Notes = "Bar rests on front delts. More quad dominant than back squat. Keeps torso upright.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Weighted Nordic Curls",
                    OrderIndex = 2,
                    TargetSets = 3,
                    TargetReps = 6,
                    TargetWeight = 10,
                    TargetRPE = 9,
                    RestSeconds = 150,
                    ExerciseType = "Strength",
                    MuscleGroups = "Hamstrings,Glutes,Lower Back",
                    Equipment = "Anchor Point,Weight Plate",
                    Notes = "Eccentric-focused hamstring destroyer. Hold weight at chest. Control the descent.",
                    ImageUrl = "https://images.unsplash.com/photo-1434682881908-b43d0467b798?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Sled Push",
                    OrderIndex = 3,
                    TargetDuration = 30,
                    TargetWeight = 180,
                    TargetRPE = 8,
                    RestSeconds = 120,
                    ExerciseType = "Cardio",
                    MuscleGroups = "Quads,Glutes,Calves,Core,Conditioning",
                    Equipment = "Prowler Sled,Plates",
                    Notes = "Low and powerful. Drive through legs. Great for conditioning without joint impact.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Sled Pull",
                    OrderIndex = 4,
                    TargetDuration = 30,
                    TargetWeight = 90,
                    TargetRPE = 7,
                    RestSeconds = 120,
                    ExerciseType = "Cardio",
                    MuscleGroups = "Upper Back,Biceps,Core,Conditioning",
                    Equipment = "Prowler Sled,Rope,Plates",
                    Notes = "Walk backwards, hand-over-hand pulling. Great for upper back volume.",
                    ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Turkish Get-Up",
                    OrderIndex = 5,
                    TargetSets = 3,
                    TargetReps = 5,
                    TargetWeight = 35,
                    TargetRPE = 7,
                    RestSeconds = 120,
                    ExerciseType = "Strength",
                    MuscleGroups = "Full Body,Shoulders,Core,Stability",
                    Equipment = "Kettlebell or Dumbbell",
                    Notes = "Complex movement. Master form first. Excellent for shoulder stability and full-body coordination.",
                    ImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=400",
                    VideoUrl = "https://www.youtube.com/watch?v=0bWRPC49-KI",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Overhead Carry",
                    OrderIndex = 6,
                    TargetDuration = 45,
                    TargetWeight = 50,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Shoulders,Core,Stability",
                    Equipment = "Kettlebell or Dumbbell",
                    Notes = "Walk with weight overhead. Arm locked out. Great for shoulder stability and core strength.",
                    ImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new Exercise
                {
                    HabitId = habitId,
                    Name = "Suitcase Carry",
                    OrderIndex = 7,
                    TargetDuration = 60,
                    TargetWeight = 70,
                    TargetRPE = 7,
                    RestSeconds = 90,
                    ExerciseType = "Strength",
                    MuscleGroups = "Core,Obliques,Grip,Full Body",
                    Equipment = "Heavy Dumbbell or Kettlebell",
                    Notes = "Carry heavy weight in one hand. Fight to stay upright. Anti-lateral flexion core work.",
                    ImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=400",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.Exercises.AddRange(exercises);
        }
    }
}
