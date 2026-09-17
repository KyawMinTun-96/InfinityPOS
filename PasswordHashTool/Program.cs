using BCrypt.Net;

var hash = BCrypt.Net.BCrypt.HashPassword("admin");

Console.WriteLine(hash);
Console.WriteLine("Length: " + hash.Length);
