public static class SecurityUtils
{
    public static byte[] GetMaskedBytes()
    {
        var password = new List<byte>();
        Console.Write("Enter Master Password: ");

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            
            if (key.Key == ConsoleKey.Backspace && password.Count > 0)
            {
                password.RemoveAt(password.Count - 1);
                Console.Write("\b \b"); 
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Add((byte)key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return password.ToArray();
    }
}
