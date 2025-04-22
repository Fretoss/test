using System;
using System.IO;

public static class Logger
{
    private static readonly StreamWriter logFile = new StreamWriter("log.log");
    private static readonly object lockObj = new object();

    public static void Log(string message)
    {
        string formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}";

        lock (lockObj)
        {
            // Вывод в консоль
            Console.WriteLine(formattedMessage);

            // Запись в файл
            logFile.WriteLine(formattedMessage);
            logFile.Flush();
        }
    }

    public static void Close()
    {
        logFile.Close();
    }
}

abstract class Tester
{
    protected void LogTestResult(string testName, bool passed, string message = "")
    {
        string result = passed ? "PASS" : "FAIL";
        string logMessage = $"[{testName}] {result} {message}";
        Logger.Log(logMessage);
    }
}

// Тестируемый класс TCommand
public class TCommand
{
    public int CommandCode { get; set; }

    public string GetFullCommandName()
    {
        return CommandCode switch
        {
            1 => "ПОЛУЧИТЬ ИЗ ВХОДНОЙ ЯЧЕЙКИ",
            2 => "ОТПРАВИТЬ ИЗ ЯЧЕЙКИ В ВЫХОДНУЮ ЯЧЕЙКУ",
            4 => "ПОЛОЖИТЬ В РЕЗЕРВ",
            6 => "ПРОИЗВЕСТИ ЗАНУЛЕНИЕ",
            20 => "ЗАВЕРШЕНИЕ КОМАНД ВЫДАЧИ",
            _ => "ОШИБКА: Неверный код команды"
        };
    }
}

// Тестер для TCommand
class TCommandTester : Tester
{
    public void RunAllTests()
    {
        TestInvalidCommandCode();
        TestValidCommandCodes();
    }

    private void TestInvalidCommandCode()
    {
        var command = new TCommand { CommandCode = -1 };
        string result = command.GetFullCommandName();
        bool passed = result == "ОШИБКА: Неверный код команды";

        LogTestResult("InvalidCommandTest", passed, $"Код: -1, Результат: {result}");
    }

    private void TestValidCommandCodes()
    {
        var testCases = new[]
        {
            (code: 1, expected: "ПОЛУЧИТЬ ИЗ ВХОДНОЙ ЯЧЕЙКИ"),
            (code: 2, expected: "ОТПРАВИТЬ ИЗ ЯЧЕЙКИ В ВЫХОДНУЮ ЯЧЕЙКУ"),
            (code: 4, expected: "ПОЛОЖИТЬ В РЕЗЕРВ"),
            (code: 6, expected: "ПРОИЗВЕСТИ ЗАНУЛЕНИЕ"),
            (code: 20, expected: "ЗАВЕРШЕНИЕ КОМАНД ВЫДАЧИ")
        };

        foreach (var testCase in testCases)
        {
            var command = new TCommand { CommandCode = testCase.code };
            string result = command.GetFullCommandName();
            bool passed = result == testCase.expected;

            LogTestResult($"CommandCodeTest_{testCase.code}", passed,
                         $"Ожидалось: {testCase.expected}, Получено: {result}");
        }
    }
}

// Главная программа
class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("=== НАЧАЛО ТЕСТИРОВАНИЯ ===");
            Logger.Log("Начало тестирования");

            // Запуск тестов
            var commandTester = new TCommandTester();
            commandTester.RunAllTests();

            Logger.Log("Тестирование завершено");
            Console.WriteLine("=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ===");
        }
        catch (Exception ex)
        {
            Logger.Log($"КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
            Console.WriteLine($"ОШИБКА: {ex.Message}");
        }
        finally
        {
            Logger.Close();
            Console.WriteLine("\nРезультаты сохранены в log.log");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}