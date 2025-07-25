using System;

namespace StudentManager.Utils
{
    public static class RandomCommentGenerator
    {
        private static readonly string[] Comments = {
            "Great job!", "Keep it up!", "You can do even better!",
            "Good progress!", "Bravo!", "Don't give up!",
            "You are on the right track!", "Super result!", "Impressive!",
            "Keep up this form!", "Very good!", "Wonderful achievement!",
            "Proud of you!", "Excellent work!", "Keep going!"
        };

        private static readonly Random _random = new();

        public static string GetRandomComment() => Comments[_random.Next(Comments.Length)];

        public static string GetCommentForAverage(double average)
        {
            if (average >= 4.5) return "Great job! You are a role model!";
            if (average >= 3.5) return "Good progress! Keep it up!";
            return "Don't give up! You can do even better!";
        }
    }
}