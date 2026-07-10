using Web_Learning_Vocabulary.Models;

namespace Web_Learning_Vocabulary.Services;

public interface IPassageExerciseService
{
    Task<List<PassageExercise>> GetAllExercisesAsync();
    Task<PassageExercise?> GetExerciseByIdAsync(int id);
    Task<PassageExercise> CreateExerciseAsync(PassageExercise exercise);
    Task<bool> DeleteExerciseAsync(int id);
    Task<PassageSubmission> SubmitAnswersAsync(int exerciseId, List<BlankAnswer> answers);
}

public class PassageExerciseService : IPassageExerciseService
{
    private static List<PassageExercise> _exercises = new()
    {
        new PassageExercise
        {
            Id = 1,
            Title = "A Day at the Office",
            OriginalPassage = "Every morning, I arrive at my office early to prepare for the day. My colleagues are reliable and always help me with difficult tasks. The company has an innovative approach to solving problems. I persevere through challenges and always try to maintain a serene atmosphere.",
            PassageWithBlanks = "Every morning, I arrive at my office early to prepare for the day. My colleagues are reliable and always help me with difficult tasks. The company has an _____ approach to solving problems. I persevere through challenges and always try to maintain a serene atmosphere.",
            BlankWords = new()
            {
                new BlankWord { Id = 1, EnglishWord = "innovative", VietnameseMeaning = "sáng tạo", Position = 1 }
            }
        }
    };

    private static int _nextExerciseId = 2;

    public Task<List<PassageExercise>> GetAllExercisesAsync()
    {
        return Task.FromResult(_exercises);
    }

    public Task<PassageExercise?> GetExerciseByIdAsync(int id)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(exercise);
    }

    public Task<PassageExercise> CreateExerciseAsync(PassageExercise exercise)
    {
        exercise.Id = _nextExerciseId++;
        exercise.CreatedAt = DateTime.Now;
        _exercises.Add(exercise);
        return Task.FromResult(exercise);
    }

    public Task<bool> DeleteExerciseAsync(int id)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == id);
        if (exercise != null)
        {
            _exercises.Remove(exercise);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<PassageSubmission> SubmitAnswersAsync(int exerciseId, List<BlankAnswer> answers)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new KeyNotFoundException($"Exercise with ID {exerciseId} not found");

        var submission = new PassageSubmission
        {
            Id = Guid.NewGuid().GetHashCode(),
            ExerciseId = exerciseId,
            Answers = answers,
            TotalCount = exercise.BlankWords.Count,
            SubmittedAt = DateTime.Now
        };

        // Check answers
        submission.CorrectCount = 0;
        foreach (var answer in answers)
        {
            var blankWord = exercise.BlankWords.FirstOrDefault(b => b.Id == answer.BlankId);
            if (blankWord != null)
            {
                answer.CorrectAnswer = blankWord.EnglishWord;
                answer.IsCorrect = answer.UserAnswer.Equals(blankWord.EnglishWord, StringComparison.OrdinalIgnoreCase);
                if (answer.IsCorrect)
                    submission.CorrectCount++;
            }
        }

        return Task.FromResult(submission);
    }
}
