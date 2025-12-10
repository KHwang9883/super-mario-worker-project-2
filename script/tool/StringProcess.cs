using Godot;
using System;

public static class StringProcess {
    public static string ConvertHashAndNewline(string input) {
        if (string.IsNullOrEmpty(input)) return input;

        const string tempPlaceholder = "☃";
        var step1 = input.Replace(@"\#", tempPlaceholder);
        var step2 = step1.Replace("#", "\n");
        var result = step2.Replace(tempPlaceholder, "#");
        return result;
    }
}
